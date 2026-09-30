namespace ROMulus.Core.Models;

/// <summary>
/// A scan run record. Mirrors the <c>scan_history</c> table.
/// </summary>
public sealed record ScanHistory
{
    public long? Id { get; init; }
    /// <summary><c>quick</c> or <c>heavy</c>.</summary>
    public required string ScanType { get; init; }
    public required DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? FinishedAt { get; init; }
    public required string RootPath { get; init; }
    public int FilesFound { get; init; }
    public int FilesMatched { get; init; }
    public int FilesNew { get; init; }
    public int Errors { get; init; }
}

/// <summary>
/// A DAT entry parsed from a No-Intro / Redump XML file.
/// Mirrors the <c>dat_entries</c> table.
/// </summary>
public sealed record DatEntry
{
    public long? Id { get; init; }
    public required string DatFile { get; init; }
    public string? SystemId { get; init; }
    public required string GameName { get; init; }
    public required string RomName { get; init; }
    public long? SizeBytes { get; init; }
    public string? Crc32 { get; init; }
    public string? Md5 { get; init; }
    public string? Sha1 { get; init; }
    public string? Region { get; init; }
    public string? Revision { get; init; }
    public bool IsBios { get; init; }
}

/// <summary>
/// A user ROM collection group. Mirrors the <c>collections</c> table.
/// (Named RomCollection to avoid CA1711 — type names must not end in 'Collection'.)
/// </summary>
public sealed record RomCollection
{
    public long? Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    /// <summary>True for built-in system collections like Favorites.</summary>
    public bool IsSystem { get; init; }
}
