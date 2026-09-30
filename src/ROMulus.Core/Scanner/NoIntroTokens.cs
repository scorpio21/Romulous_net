using System.Text.RegularExpressions;

namespace ROMulus.Core.Scanner;

/// <summary>
/// Shared No-Intro / Redump / TOSEC parenthesized-token tables and regexes.
/// Mirrors <c>romulus.core._no_intro_tokens</c>.
///
/// Both the scanner (<see cref="FilenameParser"/>) and the DAT parser inspect
/// the (tag, group) segments of canonical ROM names.
/// </summary>
public static partial class NoIntroTokens
{
    /// <summary>Country / super-region tokens used by scanner and DAT parser (lowercase).</summary>
    public static readonly FrozenSet<string> RegionCountryTokens = FrozenSet.ToFrozenSet(
    [
        "usa", "europe", "japan", "world", "asia", "australia",
        "brazil", "canada", "china", "france", "germany", "italy",
        "korea", "netherlands", "spain", "sweden", "taiwan", "uk",
        "unknown", "latin america", "scandinavia", "russia", "hong kong",
    ], StringComparer.OrdinalIgnoreCase);

    /// <summary>ISO 639-1 language codes that appear in filenames only (not in DAT canonical names).</summary>
    public static readonly FrozenSet<string> RegionLanguageTokens = FrozenSet.ToFrozenSet(
    [
        "en", "ja", "jp", "fr", "de", "es", "it",
        "nl", "pt", "ru", "ko", "zh", "sv", "fi", "no", "da", "pl",
    ], StringComparer.OrdinalIgnoreCase);

    /// <summary>Union of country + language tokens — everything the filename parser must recognise.</summary>
    public static readonly FrozenSet<string> FilenameRegionTokens =
        FrozenSet.ToFrozenSet(
            RegionCountryTokens.Concat(RegionLanguageTokens),
            StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Revision-tag regex: matches <c>(Rev 1)</c>, <c>(v1.0)</c>, <c>(1.2a)</c>, etc.
    /// Used identically by scanner and DAT parser.
    /// </summary>
    public static readonly Regex RevisionRe = RevisionRegex();

    [GeneratedRegex(@"^(rev\s+\S+|v\d+(\.\d+[a-z]?)?|\d+\.\d+[a-z]?)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RevisionRegex();
}
