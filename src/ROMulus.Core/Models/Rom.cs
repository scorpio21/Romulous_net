namespace ROMulus.Core.Models;

/// <summary>
/// A single ROM file on disk, as discovered by the scanner.
/// Mirrors the <c>roms</c> SQLite table (strict 1:1 model, v0.4.0+).
/// The <see cref="Id"/> is assigned by the database after insert/upsert;
/// every other field is populated by the scanner (L1+L2) or Heavy Scan (L3).
/// </summary>
public sealed record Rom
{
    /// <summary>Database primary key. Null before first persist.</summary>
    public long? Id { get; init; }

    /// <summary>Absolute path as stored in the DB (may be forward- or back-slash on Windows).</summary>
    public required string Path { get; init; }

    /// <summary>Filename including extension, e.g. <c>Zelda (USA).sfc</c>.</summary>
    public required string Filename { get; init; }

    /// <summary>Lowercase extension including leading dot, e.g. <c>.sfc</c>.</summary>
    public required string Extension { get; init; }

    /// <summary>File size in bytes at time of scan.</summary>
    public required long SizeBytes { get; init; }

    /// <summary>Modification time as a Unix timestamp (float seconds).</summary>
    public required double Mtime { get; init; }

    /// <summary>System id from the system registry, e.g. <c>snes</c>. Null if unknown.</summary>
    public string? SystemId { get; init; }

    /// <summary>FK to <c>scan_history.id</c>.</summary>
    public long? ScanId { get; init; }

    /// <summary>Normalised filename key used for fuzzy matching.</summary>
    public string? FuzzyKey { get; init; }

    /// <summary>Title extracted from the ROM header (L2), if supported.</summary>
    public string? HeaderTitle { get; init; }

    /// <summary>Canonical name from the matching DAT entry (L3).</summary>
    public string? DatMatch { get; init; }

    /// <summary>How confidently this ROM was identified.</summary>
    public MatchConfidence MatchConfidence { get; init; } = MatchConfidence.Unmatched;

    /// <summary>Root of the library this ROM belongs to.</summary>
    public string? LibraryRoot { get; init; }

    /// <summary>
    /// True when the file was not found during the last scan sweep.
    /// The row is kept (tombstoned) so metadata / hashes survive a temporarily
    /// unmounted share; re-scanning un-tombstones via path-keyed upsert.
    /// </summary>
    public bool Missing { get; init; }

    // ── Identity fields (populated from filename parse or DAT match) ──────

    public string? Title { get; init; }
    public string? CanonicalName { get; init; }
    public string? Region { get; init; }
    public string? Revision { get; init; }
    public bool IsHack { get; init; }
    public bool IsHomebrew { get; init; }
    public bool IsBios { get; init; }
}
