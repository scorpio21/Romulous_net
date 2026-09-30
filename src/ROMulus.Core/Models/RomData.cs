namespace ROMulus.Core.Models;

/// <summary>
/// ROM metadata row (1:1 with <see cref="Rom"/>).
/// Populated by the enrichment chain; stored in the <c>metadata</c> table.
/// </summary>
public sealed record RomMetadata
{
    public required long RomId { get; init; }
    public string? Description { get; init; }
    public string? Genre { get; init; }
    public string? Developer { get; init; }
    public string? Publisher { get; init; }
    public string? ReleaseDate { get; init; }
    public int? ReleaseYear { get; init; }
    public string? Players { get; init; }
    public string? Rating { get; init; }
    /// <summary>Which enrichment source produced this row, e.g. <c>libretro</c>, <c>screenscraper</c>.</summary>
    public required string Source { get; init; }
}

/// <summary>
/// Cover art reference. Multiple covers per ROM are allowed;
/// <see cref="IsPreferred"/> marks the one shown in the Detail Panel.
/// </summary>
public sealed record RomCover
{
    public long? Id { get; init; }
    public required long RomId { get; init; }
    /// <summary>e.g. <c>boxart</c>, <c>screenshot</c>, <c>title</c>.</summary>
    public required string CoverType { get; init; }
    public string? SourceUrl { get; init; }
    public string? LocalPath { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
    public bool IsPreferred { get; init; }
}

/// <summary>Hash cache record (expensive — reused when mtime/size unchanged).</summary>
public sealed record RomHash
{
    public required long RomId { get; init; }
    public string? Crc32 { get; init; }
    public string? Sha1 { get; init; }
    public string? Md5 { get; init; }
    /// <summary>Unix timestamp when hashing completed.</summary>
    public required double HashedAt { get; init; }
}
