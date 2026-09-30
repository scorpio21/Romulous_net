using Dapper;
using Microsoft.Data.Sqlite;
using ROMulus.Core.Models;
using ROMulus.Core.Scanner;

namespace ROMulus.Infrastructure.Database;

/// <summary>
/// All SQL queries for the <c>roms</c>, <c>scan_history</c>, and
/// <c>systems</c> tables.
/// Mirrors the relevant sections of <c>romulus.db.queries</c>.
/// </summary>
public sealed class RomRepository : IRomRepository
{
    private readonly SqliteConnection _conn;

    public RomRepository(SqliteConnection connection) => _conn = connection;

    // ── System registry ──────────────────────────────────────────────────────

    /// <summary>Seeds the <c>systems</c> table from a list of system definitions.</summary>
    public void UpsertSystems(IEnumerable<SystemInfo> systems)
    {
        using var tx = _conn.BeginTransaction();
        foreach (var s in systems)
        {
            _conn.Execute("""
                INSERT OR REPLACE INTO systems
                    (id, display_name, short_name, manufacturer, generation,
                     extensions, header_rule, libretro_name, folder_aliases, dat_name)
                VALUES
                    (@id, @displayName, @shortName, @manufacturer, @generation,
                     @extensions, @headerRule, @libretroName, @folderAliases, @datName)
                """,
                new
                {
                    id = s.Id,
                    displayName = s.DisplayName,
                    shortName = s.ShortName,
                    manufacturer = s.Manufacturer,
                    generation = s.Generation,
                    extensions = string.Join(",", s.Extensions),
                    headerRule = s.HeaderRule,
                    libretroName = s.LibretroName,
                    folderAliases = string.Join(",", s.FolderAliases),
                    datName = s.DatName,
                },
                transaction: tx);
        }
        tx.Commit();
    }

    /// <summary>
    /// Returns alias → system_id map for the quick-scan system resolver.
    /// Every folder alias (lowercase) maps to the system it belongs to.
    /// </summary>
    public Dictionary<string, string> GetAliasBySystem()
    {
        var rows = _conn.Query<(string FolderAliases, string Id)>(
            "SELECT folder_aliases, id FROM systems");

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (folderAliases, id) in rows)
        {
            foreach (var alias in folderAliases.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var key = alias.Trim().ToLowerInvariant();
                if (!string.IsNullOrEmpty(key))
                    map[key] = id;
            }
        }
        return map;
    }

    /// <summary>
    /// Returns system_id → accepted extensions (lowercase, with dot).
    /// </summary>
    public Dictionary<string, HashSet<string>> GetExtensionsBySystem()
    {
        var rows = _conn.Query<(string Id, string Extensions)>(
            "SELECT id, extensions FROM systems");

        return rows.ToDictionary(
            r => r.Id,
            r => r.Extensions
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim().ToLowerInvariant())
                .ToHashSet());
    }

    // ── Scan history ─────────────────────────────────────────────────────────

    /// <summary>Inserts a new scan_history row and returns its id.</summary>
    public long InsertScanHistory(string scanType, DateTimeOffset startedAt, string rootPath)
    {
        _conn.Execute("""
            INSERT INTO scan_history (scan_type, started_at, root_path)
            VALUES (@scanType, @startedAt, @rootPath)
            """,
            new { scanType, startedAt = startedAt.ToString("O"), rootPath });

        return _conn.ExecuteScalar<long>("SELECT last_insert_rowid()");
    }

    /// <summary>Updates a scan_history row with the final statistics.</summary>
    public void UpdateScanHistory(long scanId, DateTimeOffset finishedAt,
        int filesFound, int filesMatched, int filesNew, int errors)
    {
        _conn.Execute("""
            UPDATE scan_history
            SET finished_at   = @finishedAt,
                files_found   = @filesFound,
                files_matched = @filesMatched,
                files_new     = @filesNew,
                errors        = @errors
            WHERE id = @scanId
            """,
            new
            {
                scanId,
                finishedAt = finishedAt.ToString("O"),
                filesFound,
                filesMatched,
                filesNew,
                errors,
            });
    }

    // ── ROM upsert ───────────────────────────────────────────────────────────

    /// <summary>
    /// Insert-or-update a ROM row keyed on <c>path</c>.
    ///
    /// COALESCE semantics preserve stronger DAT-derived values: if a Heavy Scan
    /// has already written <c>canonical_name</c> / <c>dat_match</c> /
    /// <c>match_confidence='dat_verified'</c>, a Quick Scan rescan must NOT
    /// overwrite those with fuzzy values. The COALESCE in the UPDATE clause
    /// keeps the existing non-null value when the incoming value is null.
    ///
    /// Returns the rom_id (existing or newly inserted).
    /// </summary>
    public long UpsertRom(Rom rom)
    {
        _conn.Execute("""
            INSERT INTO roms
                (path, filename, extension, size_bytes, mtime,
                 system_id, scan_id, fuzzy_key, match_confidence,
                 library_root, title, region, revision,
                 is_hack, is_homebrew, is_bios, missing)
            VALUES
                (@path, @filename, @extension, @sizeBytes, @mtime,
                 @systemId, @scanId, @fuzzyKey, @matchConfidence,
                 @libraryRoot, @title, @region, @revision,
                 @isHack, @isHomebrew, @isBios, 0)
            ON CONFLICT(path) DO UPDATE SET
                filename         = excluded.filename,
                extension        = excluded.extension,
                size_bytes       = excluded.size_bytes,
                mtime            = excluded.mtime,
                system_id        = excluded.system_id,
                scan_id          = excluded.scan_id,
                fuzzy_key        = COALESCE(excluded.fuzzy_key, roms.fuzzy_key),
                match_confidence = CASE
                    WHEN roms.match_confidence = 'dat_verified' THEN 'dat_verified'
                    ELSE excluded.match_confidence
                END,
                library_root     = excluded.library_root,
                title            = COALESCE(roms.title, excluded.title),
                region           = COALESCE(roms.region, excluded.region),
                revision         = COALESCE(roms.revision, excluded.revision),
                is_hack          = COALESCE(roms.is_hack, excluded.is_hack),
                is_homebrew      = COALESCE(roms.is_homebrew, excluded.is_homebrew),
                missing          = 0
            """,
            new
            {
                path = rom.Path,
                filename = rom.Filename,
                extension = rom.Extension,
                sizeBytes = rom.SizeBytes,
                mtime = rom.Mtime,
                systemId = rom.SystemId,
                scanId = rom.ScanId,
                fuzzyKey = rom.FuzzyKey,
                matchConfidence = rom.MatchConfidence.ToString().ToLowerInvariant()
                    .Replace("datverified", "dat_verified", StringComparison.Ordinal),
                libraryRoot = rom.LibraryRoot,
                title = rom.Title,
                region = rom.Region,
                revision = rom.Revision,
                isHack = rom.IsHack ? 1 : 0,
                isHomebrew = rom.IsHomebrew ? 1 : 0,
                isBios = rom.IsBios ? 1 : 0,
            });

        return _conn.ExecuteScalar<long>("SELECT id FROM roms WHERE path = @path",
            new { path = rom.Path });
    }

    // ── Tombstone sweep ───────────────────────────────────────────────────────

    /// <summary>
    /// Marks as <c>missing=1</c> every rom under <paramref name="libraryRoot"/>
    /// whose id is NOT in <paramref name="visitedIds"/>.
    ///
    /// Optionally restricted to a single <paramref name="scopeSystemId"/>.
    /// Returns the count of newly-tombstoned rows.
    /// </summary>
    public int MarkMissingUnderRoot(
        string libraryRoot,
        IReadOnlySet<long> visitedIds,
        string? scopeSystemId = null)
    {
        // SQLite has a 32 k parameter limit; chunk if needed.
        // For a library of typical size (< 100 K), one pass is fine.
        // We build a temp table to avoid massive IN(...) literals.
        using var tx = _conn.BeginTransaction();

        _conn.Execute("CREATE TEMP TABLE IF NOT EXISTS _visited_ids (id INTEGER PRIMARY KEY)",
            transaction: tx);
        _conn.Execute("DELETE FROM _visited_ids", transaction: tx);

        foreach (var id in visitedIds)
            _conn.Execute("INSERT INTO _visited_ids VALUES (@id)", new { id }, transaction: tx);

        var scopeClause = scopeSystemId is null ? "" : " AND system_id = @scopeSystemId";
        var affected = _conn.Execute($"""
            UPDATE roms
            SET missing = 1
            WHERE library_root = @libraryRoot
              AND missing = 0
              AND id NOT IN (SELECT id FROM _visited_ids)
              {scopeClause}
            """,
            new { libraryRoot, scopeSystemId },
            transaction: tx);

        _conn.Execute("DROP TABLE IF EXISTS _visited_ids", transaction: tx);
        tx.Commit();
        return affected;
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    /// <summary>Returns rom count per system_id for the sidebar.</summary>
    public IReadOnlyDictionary<string, int> GetCountBySystem()
    {
        return _conn.Query<(string SystemId, int Count)>(
            "SELECT system_id, COUNT(*) as cnt FROM roms WHERE missing = 0 GROUP BY system_id")
            .ToDictionary(r => r.SystemId, r => r.Count);
    }
}
