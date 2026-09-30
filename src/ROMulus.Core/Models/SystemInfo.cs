namespace ROMulus.Core.Models;

/// <summary>
/// A retro console / platform definition loaded from <c>systems/builtin.yaml</c>.
/// Mirrors <c>romulus.models.system.SystemInfo</c> from the Python codebase.
/// </summary>
public sealed record SystemInfo
{
    /// <summary>Unique identifier used as FK across the schema, e.g. <c>snes</c>.</summary>
    public required string Id { get; init; }

    public required string DisplayName { get; init; }
    public required string ShortName { get; init; }
    public string? Manufacturer { get; init; }
    public int? Generation { get; init; }

    /// <summary>Recognised file extensions for this system (lowercase, with dot), e.g. <c>[".sfc", ".smc"]</c>.</summary>
    public IReadOnlyList<string> Extensions { get; init; } = [];

    /// <summary>
    /// Optional header-detection rule used by L2 identification.
    /// Format: <c>"nes_ines"</c>, <c>"snes_copier"</c>, etc.
    /// </summary>
    public string? HeaderRule { get; init; }

    /// <summary>Name used to query libretro-thumbnails, e.g. <c>"Nintendo - Super Nintendo Entertainment System"</c>.</summary>
    public string? LibretroName { get; init; }

    /// <summary>Folder name aliases the scanner accepts for this system (case-insensitive).</summary>
    public IReadOnlyList<string> FolderAliases { get; init; } = [];

    /// <summary>DAT file name prefix for No-Intro matching, e.g. <c>"Nintendo - Super Nintendo Entertainment System"</c>.</summary>
    public string? DatName { get; init; }
}
