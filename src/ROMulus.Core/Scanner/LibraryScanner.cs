using ROMulus.Core.Models;

namespace ROMulus.Core.Scanner;

/// <summary>
/// Quick Scan result summary.
/// Mirrors <c>romulus.core.scanner.ScanResult</c>.
/// </summary>
public sealed record ScanResult
{
    public required long ScanId { get; init; }
    public int FilesFound { get; init; }
    public int FilesWithSystem { get; init; }
    public int FilesSkipped { get; init; }
    public int Errors { get; init; }
    public IReadOnlySet<string> SystemsSeen { get; init; } = new HashSet<string>();
    public int FilesNewlyMissing { get; init; }
}

/// <summary>
/// Filesystem walker for Quick Scan (L1 + L2).
/// Mirrors <c>romulus.core.scanner.scan_library</c>.
///
/// Walk strategy:
/// 1. <c>Directory.EnumerateFiles</c> recursive (no symlink follow).
/// 2. System detection from the folder-name alias map (first matching ancestor).
/// 3. Filename parsing (L1): region, revision, is_hack, is_homebrew.
/// 4. Upsert into the <c>roms</c> table (COALESCE preserves DAT-derived values).
/// 5. Tombstone sweep: mark missing=1 for any row under this root not visited.
/// </summary>
public sealed class LibraryScanner
{
    private readonly IRomRepository _repo;

    public LibraryScanner(IRomRepository repository) => _repo = repository;

    /// <summary>
    /// Walk <paramref name="libraryPath"/>, enroll ROMs, and sweep for missing files.
    ///
    /// <paramref name="progress"/> receives <c>(filesEnrolledSoFar, currentFilename)</c>
    /// during the walk, and a phase-label string during post-walk DB phases.
    /// Pass <see langword="null"/> to suppress progress reporting (e.g. in tests).
    ///
    /// <paramref name="scopeSystemId"/> restricts both the walk and the tombstone
    /// sweep to one platform.
    /// </summary>
    public ScanResult Scan(
        string libraryPath,
        IProgress<(int Count, string Label)>? progress = null,
        string? scopeSystemId = null,
        CancellationToken cancellationToken = default)
    {
        var libraryRoot = Path.GetFullPath(libraryPath);

        var aliasMap = _repo.GetAliasBySystem();
        var extsBySystem = _repo.GetExtensionsBySystem();

        var startedAt = DateTimeOffset.UtcNow;
        var scanId = _repo.InsertScanHistory("quick", startedAt, libraryRoot);

        var filesFound = 0;
        var filesWithSystem = 0;
        var filesSkipped = 0;
        var errors = 0;
        var systemsSeen = new HashSet<string>();
        var visitedIds = new HashSet<long>();

        // Enumerate all files recursively; EnumerateFiles does NOT follow
        // symlinks on Windows (same behaviour as Python's os.walk followlinks=False).
        foreach (var filePath in Directory.EnumerateFiles(libraryRoot, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var filename = Path.GetFileName(filePath);

            if (FilenameParser.IsSideFile(filename))
            {
                filesSkipped++;
                continue;
            }

            var fileDir = Path.GetDirectoryName(filePath)!;
            var systemId = ResolveSystemForDirectory(fileDir, libraryRoot, aliasMap);

            if (systemId is null ||
                !FilenameParser.IsRomFile(filename, extsBySystem.GetValueOrDefault(systemId, [])))
            {
                filesSkipped++;
                continue;
            }

            if (scopeSystemId is not null && systemId != scopeSystemId)
            {
                filesSkipped++;
                continue;
            }

            long sizeBytes;
            double mtime;
            try
            {
                var info = new FileInfo(filePath);
                sizeBytes = info.Length;
                mtime = (double)info.LastWriteTimeUtc.Ticks / TimeSpan.TicksPerSecond;
            }
            catch (IOException)
            {
                errors++;
                continue;
            }

            var parsed = FilenameParser.Parse(filename);
            var fuzzy = FilenameParser.GenerateFuzzyKey(parsed.CleanName, parsed.ReleaseType);
            var title = parsed.DisplayTitle.Length > 0 ? parsed.DisplayTitle : filename;

            var rom = new Rom
            {
                Path = filePath,
                Filename = filename,
                Extension = parsed.Extension,
                SizeBytes = sizeBytes,
                Mtime = mtime,
                SystemId = systemId,
                ScanId = scanId,
                FuzzyKey = fuzzy,
                MatchConfidence = string.IsNullOrEmpty(fuzzy)
                    ? MatchConfidence.Unmatched
                    : MatchConfidence.Fuzzy,
                LibraryRoot = libraryRoot,
                Title = title,
                Region = parsed.Region,
                Revision = parsed.Revision,
                IsHack = parsed.IsHack,
                IsHomebrew = parsed.IsHomebrew,
            };

            var romId = _repo.UpsertRom(rom);
            visitedIds.Add(romId);
            filesFound++;
            filesWithSystem++;
            systemsSeen.Add(systemId);
            progress?.Report((filesFound, filename));
        }

        // Post-walk: tombstone sweep
        progress?.Report((filesFound, "Marking missing entries…"));
        var filesNewlyMissing = _repo.MarkMissingUnderRoot(libraryRoot, visitedIds, scopeSystemId);

        // Finalise scan history
        progress?.Report((filesFound, "Finalising scan history…"));
        var finishedAt = DateTimeOffset.UtcNow;
        _repo.UpdateScanHistory(scanId, finishedAt, filesFound, filesWithSystem, filesWithSystem, errors);

        return new ScanResult
        {
            ScanId = scanId,
            FilesFound = filesFound,
            FilesWithSystem = filesWithSystem,
            FilesSkipped = filesSkipped,
            Errors = errors,
            SystemsSeen = systemsSeen,
            FilesNewlyMissing = filesNewlyMissing,
        };
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Walk up from <paramref name="directory"/> toward <paramref name="libraryRoot"/>,
    /// returning the system_id of the first directory whose basename matches an alias.
    /// </summary>
    private static string? ResolveSystemForDirectory(
        string directory,
        string libraryRoot,
        IReadOnlyDictionary<string, string> aliasMap)
    {
        var current = directory;
        while (true)
        {
            var basename = Path.GetFileName(current);
            if (!string.IsNullOrEmpty(basename) &&
                aliasMap.TryGetValue(basename.ToLowerInvariant(), out var systemId))
                return systemId;

            if (string.Equals(current, libraryRoot, StringComparison.OrdinalIgnoreCase) ||
                Path.GetDirectoryName(current) is not { } parent ||
                string.Equals(parent, current, StringComparison.Ordinal))
                return null;

            current = parent;
        }
    }
}
