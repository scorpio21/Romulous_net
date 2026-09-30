namespace ROMulus.Core.Models;

/// <summary>
/// How one system is laid out inside a destination profile.
/// A mapping with <see cref="Supported"/>=false (or empty <see cref="Folder"/>)
/// means the target device cannot run this system — the exporter skips it
/// explicitly rather than silently omitting it.
/// </summary>
public sealed record SystemMapping
{
    public string Folder { get; init; } = string.Empty;
    public IReadOnlyList<string> Extensions { get; init; } = [];
    public bool Supported { get; init; } = true;

    /// <summary>True only when the target supports the system AND a folder is specified.</summary>
    public bool IsSupported => Supported && !string.IsNullOrEmpty(Folder);
}

/// <summary>
/// An export destination profile (Batocera, MiSTer, Anbernic, etc.).
/// Loaded from YAML files in <c>profiles/</c> at startup.
/// Mirrors <c>romulus.models.profile.DestinationProfile</c>.
/// </summary>
public sealed record DestinationProfile
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool CaseSensitive { get; init; } = true;
    public required string BasePath { get; init; }
    public string? GamelistFormat { get; init; }
    public string? ArtworkSubdir { get; init; }
    /// <summary>
    /// Filename template for copied artwork. <c>{stem}</c> = ROM stem, <c>{ext}</c> = image extension.
    /// Default: <c>{stem}{ext}</c>. EmulationStation targets use <c>{stem}-image{ext}</c>.
    /// </summary>
    public string ArtworkFilenameTemplate { get; init; } = "{stem}{ext}";
    public string? MultiDisc { get; init; }
    public IReadOnlyDictionary<string, SystemMapping> Systems { get; init; } =
        new Dictionary<string, SystemMapping>();
}
