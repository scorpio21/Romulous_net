using Microsoft.Data.Sqlite;

namespace ROMulus.Infrastructure.Database;

/// <summary>
/// SQLite schema for ROMulus v0.5.0+ (C# port).
/// Mirrors <c>romulus.db.schema</c> from the Python codebase.
///
/// All CREATE TABLE / CREATE INDEX statements use IF NOT EXISTS so this is safe
/// to call on every startup (idempotent). No migration framework — per design
/// rule #14 pre-v0.4.0 Python databases must be wiped and rebuilt.
/// </summary>
public static class Schema
{
    /// <summary>
    /// Human-readable message shown when an incompatible database is detected.
    /// </summary>
    public const string RequiresFreshDbMessage =
        "Your library database predates v0.5.0. " +
        "Delete data/romulus.db and restart to use the new schema.";

    private static readonly string[] Statements =
    [
        // ── App configuration (key-value) ─────────────────────────────────
        """
        CREATE TABLE IF NOT EXISTS config (
            key   TEXT PRIMARY KEY,
            value TEXT NOT NULL
        )
        """,

        // ── System/platform definitions ───────────────────────────────────
        """
        CREATE TABLE IF NOT EXISTS systems (
            id              TEXT PRIMARY KEY,
            display_name    TEXT NOT NULL,
            short_name      TEXT NOT NULL,
            manufacturer    TEXT,
            generation      INTEGER,
            extensions      TEXT NOT NULL,
            header_rule     TEXT,
            libretro_name   TEXT,
            folder_aliases  TEXT NOT NULL,
            dat_name        TEXT
        )
        """,

        // ── Scan history ──────────────────────────────────────────────────
        """
        CREATE TABLE IF NOT EXISTS scan_history (
            id            INTEGER PRIMARY KEY AUTOINCREMENT,
            scan_type     TEXT NOT NULL,
            started_at    TEXT NOT NULL,
            finished_at   TEXT,
            root_path     TEXT NOT NULL,
            files_found   INTEGER DEFAULT 0,
            files_matched INTEGER DEFAULT 0,
            files_new     INTEGER DEFAULT 0,
            errors        INTEGER DEFAULT 0
        )
        """,

        // ── ROM files — identity unit (strict 1:1 model) ──────────────────
        """
        CREATE TABLE IF NOT EXISTS roms (
            id               INTEGER PRIMARY KEY AUTOINCREMENT,
            path             TEXT NOT NULL UNIQUE,
            filename         TEXT NOT NULL,
            extension        TEXT NOT NULL,
            size_bytes       INTEGER NOT NULL,
            mtime            REAL NOT NULL,
            system_id        TEXT REFERENCES systems(id),
            scan_id          INTEGER REFERENCES scan_history(id),
            fuzzy_key        TEXT,
            header_title     TEXT,
            dat_match        TEXT,
            match_confidence TEXT DEFAULT 'unmatched',
            library_root     TEXT,
            missing          INTEGER NOT NULL DEFAULT 0,
            title            TEXT,
            canonical_name   TEXT,
            region           TEXT,
            revision         TEXT,
            is_hack          INTEGER NOT NULL DEFAULT 0,
            is_homebrew      INTEGER NOT NULL DEFAULT 0,
            is_bios          INTEGER NOT NULL DEFAULT 0
        )
        """,
        "CREATE INDEX IF NOT EXISTS idx_roms_system ON roms(system_id)",
        "CREATE INDEX IF NOT EXISTS idx_roms_fuzzy ON roms(system_id, fuzzy_key)",
        "CREATE INDEX IF NOT EXISTS idx_roms_title ON roms(system_id, title)",
        "CREATE INDEX IF NOT EXISTS idx_roms_library_root ON roms(library_root)",
        "CREATE INDEX IF NOT EXISTS idx_roms_missing ON roms(missing) WHERE missing = 1",

        // ── Hash cache ────────────────────────────────────────────────────
        """
        CREATE TABLE IF NOT EXISTS hashes (
            rom_id    INTEGER PRIMARY KEY REFERENCES roms(id),
            crc32     TEXT,
            sha1      TEXT,
            md5       TEXT,
            hashed_at REAL NOT NULL
        )
        """,
        "CREATE INDEX IF NOT EXISTS idx_hashes_sha1 ON hashes(sha1)",
        "CREATE INDEX IF NOT EXISTS idx_hashes_crc32 ON hashes(crc32)",

        // ── DAT entries ───────────────────────────────────────────────────
        """
        CREATE TABLE IF NOT EXISTS dat_entries (
            id         INTEGER PRIMARY KEY AUTOINCREMENT,
            dat_file   TEXT NOT NULL,
            system_id  TEXT REFERENCES systems(id),
            game_name  TEXT NOT NULL,
            rom_name   TEXT NOT NULL,
            size_bytes INTEGER,
            crc32      TEXT,
            md5        TEXT,
            sha1       TEXT,
            region     TEXT,
            revision   TEXT,
            is_bios    INTEGER DEFAULT 0
        )
        """,
        "CREATE INDEX IF NOT EXISTS idx_dat_sha1 ON dat_entries(sha1)",
        "CREATE INDEX IF NOT EXISTS idx_dat_crc32_size ON dat_entries(crc32, size_bytes)",

        // ── Per-ROM metadata (CASCADE: deleting a rom drops its row) ──────
        """
        CREATE TABLE IF NOT EXISTS metadata (
            rom_id       INTEGER PRIMARY KEY REFERENCES roms(id) ON DELETE CASCADE,
            description  TEXT,
            genre        TEXT,
            developer    TEXT,
            publisher    TEXT,
            release_date TEXT,
            release_year INTEGER,
            players      TEXT,
            rating       TEXT,
            source       TEXT NOT NULL
        )
        """,

        // ── Cover art (CASCADE) ───────────────────────────────────────────
        """
        CREATE TABLE IF NOT EXISTS covers (
            id           INTEGER PRIMARY KEY AUTOINCREMENT,
            rom_id       INTEGER REFERENCES roms(id) ON DELETE CASCADE,
            cover_type   TEXT NOT NULL,
            source_url   TEXT,
            local_path   TEXT,
            width        INTEGER,
            height       INTEGER,
            is_preferred INTEGER NOT NULL DEFAULT 0
        )
        """,

        // ── User collections ──────────────────────────────────────────────
        """
        CREATE TABLE IF NOT EXISTS collections (
            id          INTEGER PRIMARY KEY AUTOINCREMENT,
            name        TEXT NOT NULL UNIQUE,
            description TEXT,
            is_system   INTEGER DEFAULT 0
        )
        """,

        // ── Collection membership (CASCADE) ───────────────────────────────
        """
        CREATE TABLE IF NOT EXISTS collection_roms (
            collection_id INTEGER REFERENCES collections(id),
            rom_id        INTEGER REFERENCES roms(id) ON DELETE CASCADE,
            PRIMARY KEY (collection_id, rom_id)
        )
        """,

        // ── Organize plans ────────────────────────────────────────────────
        """
        CREATE TABLE IF NOT EXISTS organize_plans (
            id         INTEGER PRIMARY KEY AUTOINCREMENT,
            created_at TEXT NOT NULL,
            status     TEXT DEFAULT 'pending',
            plan_json  TEXT NOT NULL
        )
        """,

        // ── Sync destinations ─────────────────────────────────────────────
        """
        CREATE TABLE IF NOT EXISTS sync_destinations (
            id                       INTEGER PRIMARY KEY AUTOINCREMENT,
            name                     TEXT NOT NULL UNIQUE,
            target_path              TEXT NOT NULL,
            profile_id               TEXT NOT NULL,
            last_synced_at           TEXT,
            created_at               TEXT NOT NULL,
            last_inventory_signature TEXT
        )
        """,

        // ── Destination inventory cache ───────────────────────────────────
        """
        CREATE TABLE IF NOT EXISTS dest_inventory (
            dest_id      INTEGER NOT NULL REFERENCES sync_destinations(id) ON DELETE CASCADE,
            rel_path     TEXT NOT NULL,
            size_bytes   INTEGER NOT NULL,
            mtime        REAL NOT NULL,
            sha1         TEXT,
            rom_id       INTEGER REFERENCES roms(id),
            last_seen_at TEXT NOT NULL,
            PRIMARY KEY (dest_id, rel_path)
        )
        """,
        "CREATE INDEX IF NOT EXISTS idx_dest_inventory_sha1 ON dest_inventory(sha1)",
        "CREATE INDEX IF NOT EXISTS idx_dest_inventory_rom ON dest_inventory(rom_id)",

        // ── Sync plans ────────────────────────────────────────────────────
        """
        CREATE TABLE IF NOT EXISTS sync_plans (
            id         INTEGER PRIMARY KEY AUTOINCREMENT,
            dest_id    INTEGER NOT NULL REFERENCES sync_destinations(id),
            mode       TEXT NOT NULL,
            created_at TEXT NOT NULL,
            status     TEXT DEFAULT 'pending',
            summary    TEXT NOT NULL,
            plan_json  TEXT NOT NULL
        )
        """,
    ];

    /// <summary>
    /// Execute every CREATE TABLE / CREATE INDEX statement.
    /// Idempotent — safe to call on every startup.
    /// </summary>
    public static void CreateTables(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        foreach (var sql in Statements)
        {
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Returns true when the connection points to a legacy database that still
    /// has the old <c>games</c> table (pre-v0.4.0 Python schema).
    /// </summary>
    public static bool IsLegacyDatabase(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='games'";
        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
    }
}
