using System.Text.RegularExpressions;

namespace ROMulus.Core.Scanner;

/// <summary>
/// Structured result of parsing a ROM filename.
/// Mirrors <c>romulus.core.scanner.ParsedFilename</c>.
/// </summary>
public sealed record ParsedFilename
{
    /// <summary>Title with all (tag) / [tag] groups stripped and whitespace collapsed.</summary>
    public required string CleanName { get; init; }

    /// <summary>
    /// Display title: trailing article moved to front ("Legend of Zelda, The" →
    /// "The Legend of Zelda"), plus optional "(release_type)" suffix.
    /// </summary>
    public required string DisplayTitle { get; init; }

    /// <summary>Lowercase extension including leading dot, e.g. <c>.sfc</c>.</summary>
    public required string Extension { get; init; }

    public string? Region { get; init; }
    public string? Revision { get; init; }
    public int? DiscNumber { get; init; }
    public string? ReleaseType { get; init; }
    public IReadOnlyList<string> Status { get; init; } = [];
    public bool IsHack { get; init; }
    public bool IsHomebrew { get; init; }
    public bool IsUnlicensed { get; init; }
    public bool IsPrototype { get; init; }
    public bool IsBeta { get; init; }
    public bool IsDemo { get; init; }
    public bool IsBadDump { get; init; }
    public bool IsVerified { get; init; }
    public bool IsTranslation { get; init; }
}

/// <summary>
/// ROM filename parser — extracts structured identity fields from No-Intro,
/// GoodTools, and TOSEC filename conventions.
/// Mirrors <c>romulus.core.scanner.parse_filename</c> and
/// <c>romulus.core.scanner.generate_fuzzy_key</c>.
/// </summary>
public static partial class FilenameParser
{
    // ── Companion / side-file extensions to skip ─────────────────────────────
    private static readonly FrozenSet<string> SideFileExtensions = FrozenSet.ToFrozenSet(
    [
        ".cue", ".m3u", ".sub", ".txt", ".nfo",
        ".jpg", ".jpeg", ".png", ".gif",
        ".xml", ".dat", ".sav", ".srm", ".state", ".oops",
    ], StringComparer.OrdinalIgnoreCase);

    /// <summary>Archive containers always accepted regardless of system extensions.</summary>
    private static readonly FrozenSet<string> ArchiveExtensions =
        FrozenSet.ToFrozenSet([".zip", ".7z"], StringComparer.OrdinalIgnoreCase);

    // ── Token sets ────────────────────────────────────────────────────────────
    private static readonly FrozenSet<string> StatusPrototype =
        FrozenSet.ToFrozenSet(["proto", "prototype"], StringComparer.OrdinalIgnoreCase);
    private static readonly FrozenSet<string> StatusBeta =
        FrozenSet.ToFrozenSet(["beta"], StringComparer.OrdinalIgnoreCase);
    private static readonly FrozenSet<string> StatusDemo =
        FrozenSet.ToFrozenSet(["demo"], StringComparer.OrdinalIgnoreCase);
    private static readonly FrozenSet<string> StatusSample =
        FrozenSet.ToFrozenSet(["sample"], StringComparer.OrdinalIgnoreCase);
    private static readonly FrozenSet<string> StatusUnlicensed =
        FrozenSet.ToFrozenSet(["unl", "unlicensed"], StringComparer.OrdinalIgnoreCase);
    private static readonly FrozenSet<string> StatusHomebrew =
        FrozenSet.ToFrozenSet(["homebrew", "aftermarket"], StringComparer.OrdinalIgnoreCase);
    private static readonly FrozenSet<string> ReleaseTypeTokens = FrozenSet.ToFrozenSet(
    [
        "virtual console", "wii virtual console", "wii u virtual console",
        "3ds virtual console", "switch online", "nso",
        "genesis mini", "mega drive mini", "snes mini", "snes classic",
        "nes classic", "nes mini", "playstation classic", "ps classic",
        "sega channel", "gametap", "eshop", "psn", "playchoice-10",
        "vs.", "broadband adapter", "satellaview", "sufami turbo", "32x",
    ], StringComparer.OrdinalIgnoreCase);

    // Articles to fold for fuzzy comparison (multi-language).
    private static readonly FrozenSet<string> Articles = FrozenSet.ToFrozenSet(
    [
        "the", "a", "an", "le", "la", "les", "el", "los", "las",
        "der", "die", "das", "il", "lo", "gli",
    ], StringComparer.OrdinalIgnoreCase);

    // Roman numerals to convert (single-letter omitted — collide with words).
    private static readonly Dictionary<string, int> RomanNumerals = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ii"] = 2, ["iii"] = 3, ["iv"] = 4, ["vi"] = 6, ["vii"] = 7,
        ["viii"] = 8, ["ix"] = 9, ["xi"] = 11, ["xii"] = 12, ["xiii"] = 13,
        ["xiv"] = 14, ["xv"] = 15,
    };

    // ── Compiled regexes ──────────────────────────────────────────────────────
    private static readonly Regex TagGroupRe = TagGroupRegex();
    private static readonly Regex DiscRe = DiscRegex();
    private static readonly Regex TranslationRe = TranslationRegex();
    private static readonly Regex BadDumpRe = BadDumpRegex();
    private static readonly Regex HackRe = HackRegex();
    private static readonly Regex OverdumpRe = OverdumpRegex();
    private static readonly Regex AlternateRe = AlternateRegex();
    private static readonly Regex VersionSuffixRe = VersionSuffixRegex();
    private static readonly Regex WhitespaceRe = WhitespaceRegex();
    private static readonly Regex TrailingSepRe = TrailingSepRegex();
    private static readonly Regex NonAlphanumRe = NonAlphanumRegex();

    [GeneratedRegex(@"\(([^()]*)\)|\[([^\[\]]*)\]", RegexOptions.CultureInvariant)]
    private static partial Regex TagGroupRegex();

    [GeneratedRegex(@"^(disc|disk|side)\s+([0-9A-Za-z]+)(\s+of\s+\d+)?$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DiscRegex();

    [GeneratedRegex(@"^t[+\-]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TranslationRegex();

    [GeneratedRegex(@"^b\d*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BadDumpRegex();

    [GeneratedRegex(@"^h\d*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex HackRegex();

    [GeneratedRegex(@"^o\d*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex OverdumpRegex();

    [GeneratedRegex(@"^a\d*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AlternateRegex();

    [GeneratedRegex(@"\s*(v\d+(\.\d+[a-z]?)?|\d+\.\d+[a-z]?|rev\s*\d+)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex VersionSuffixRegex();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[\s_\-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex TrailingSepRegex();

    [GeneratedRegex(@"[^a-z0-9]", RegexOptions.CultureInvariant)]
    private static partial Regex NonAlphanumRegex();

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Returns true when <paramref name="filename"/> is a side-file to skip.</summary>
    public static bool IsSideFile(string filename)
        => SideFileExtensions.Contains(Path.GetExtension(filename));

    /// <summary>
    /// Returns true when <paramref name="filename"/> has an extension accepted by the system,
    /// or is a ZIP/7z archive (always accepted: hasher cracks it open during Heavy Scan).
    /// </summary>
    public static bool IsRomFile(string filename, IReadOnlySet<string> acceptedExtensions)
    {
        var ext = Path.GetExtension(filename).ToLowerInvariant();
        return ArchiveExtensions.Contains(ext) || acceptedExtensions.Contains(ext);
    }

    /// <summary>
    /// Parses a ROM filename into structured identity fields.
    /// Handles No-Intro (USA)(Rev 1), GoodTools [!][h1], and TOSEC status tags.
    /// </summary>
    public static ParsedFilename Parse(string filename)
    {
        var stem = Path.GetFileNameWithoutExtension(filename);
        var extension = Path.GetExtension(filename).ToLowerInvariant();

        string? region = null, revision = null, releaseType = null;
        int? discNumber = null;
        var status = new List<string>();
        bool isHack = false, isHomebrew = false, isUnlicensed = false;
        bool isPrototype = false, isBeta = false, isDemo = false;
        bool isBadDump = false, isVerified = false, isTranslation = false;

        foreach (Match m in TagGroupRe.Matches(stem))
        {
            var parenContent = m.Groups[1].Value;
            var bracketContent = m.Groups[2].Value;

            if (m.Groups[2].Success) // bracket [...]
            {
                var lower = bracketContent.Trim().ToLowerInvariant();
                if (lower == "!") { isVerified = true; status.Add("verified"); }
                else if (BadDumpRe.IsMatch(lower)) { isBadDump = true; status.Add("bad_dump"); }
                else if (HackRe.IsMatch(lower)) { isHack = true; status.Add("hack"); }
                else if (TranslationRe.IsMatch(lower)) { isTranslation = true; status.Add("translation"); }
                else if (OverdumpRe.IsMatch(lower)) status.Add("overdump");
                else if (AlternateRe.IsMatch(lower)) status.Add("alternate");
            }
            else // paren (...)
            {
                var (kind, value) = ClassifyParenTag(parenContent);
                switch (kind)
                {
                    case "region" when region is null: region = value; break;
                    case "revision" when revision is null: revision = value; break;
                    case "disc" when discNumber is null: discNumber = ParseDiscNumber(value); break;
                    case "prototype": isPrototype = true; status.Add("prototype"); break;
                    case "beta": isBeta = true; status.Add("beta"); break;
                    case "demo": isDemo = true; status.Add("demo"); break;
                    case "sample": status.Add("sample"); break;
                    case "unlicensed": isUnlicensed = true; status.Add("unlicensed"); break;
                    case "homebrew": isHomebrew = true; status.Add("homebrew"); break;
                    case "release" when releaseType is null: releaseType = value; break;
                }
            }
        }

        var clean = TagGroupRe.Replace(stem, "");
        clean = WhitespaceRe.Replace(clean, " ").Trim();
        clean = TrailingSepRe.Replace(clean, "");

        var display = MoveTrailingArticleToFront(clean);
        if (releaseType is not null)
            display = $"{display} ({releaseType})";

        return new ParsedFilename
        {
            CleanName = clean,
            DisplayTitle = display,
            Extension = extension,
            Region = region,
            Revision = revision,
            DiscNumber = discNumber,
            ReleaseType = releaseType,
            Status = status,
            IsHack = isHack,
            IsHomebrew = isHomebrew,
            IsUnlicensed = isUnlicensed,
            IsPrototype = isPrototype,
            IsBeta = isBeta,
            IsDemo = isDemo,
            IsBadDump = isBadDump,
            IsVerified = isVerified,
            IsTranslation = isTranslation,
        };
    }

    /// <summary>
    /// Reduces a parsed title to a stable alphanumeric comparison key (Layer 1 normalization).
    /// Implements the seven normalization steps from ROM-DEDUP-METHODOLOGY.md §3.2.
    /// </summary>
    public static string GenerateFuzzyKey(string cleanName, string? releaseType = null)
    {
        var name = MoveTrailingArticleToFront(cleanName);

        // Strip leading article
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0 && Articles.Contains(parts[0]))
            parts = parts[1..];
        name = string.Join(" ", parts);

        // Convert multi-letter Roman numerals to Arabic
        var converted = name.Split(' ').Select(token =>
        {
            var bare = NonAlphanumRe.Replace(token.ToLowerInvariant(), "");
            return RomanNumerals.TryGetValue(bare, out var arabic)
                ? arabic.ToString()
                : token;
        });
        name = string.Join(" ", converted);

        // Strip trailing version suffixes
        name = VersionSuffixRe.Replace(name, "").Trim();

        // Lowercase + strip non-alphanumeric
        name = NonAlphanumRe.Replace(name.ToLowerInvariant(), "");

        if (releaseType is not null)
        {
            var suffix = NonAlphanumRe.Replace(releaseType.ToLowerInvariant(), "");
            if (!string.IsNullOrEmpty(suffix))
                name = $"{name}__{suffix}";
        }

        return name;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static (string Kind, string Value) ClassifyParenTag(string content)
    {
        var stripped = content.Trim();
        var lower = stripped.ToLowerInvariant();

        if (NoIntroTokens.RevisionRe.IsMatch(stripped)) return ("revision", stripped);
        if (DiscRe.IsMatch(stripped)) return ("disc", stripped);
        if (StatusPrototype.Contains(lower)) return ("prototype", stripped);
        if (StatusBeta.Contains(lower)) return ("beta", stripped);
        if (StatusDemo.Contains(lower)) return ("demo", stripped);
        if (StatusSample.Contains(lower)) return ("sample", stripped);
        if (StatusUnlicensed.Contains(lower)) return ("unlicensed", stripped);
        if (StatusHomebrew.Contains(lower)) return ("homebrew", stripped);
        if (ReleaseTypeTokens.Contains(lower)) return ("release", stripped);

        // Region: every comma-separated token must be a known region/language code.
        var regionParts = stripped.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .ToArray();
        if (regionParts.Length > 0 &&
            regionParts.All(p => NoIntroTokens.FilenameRegionTokens.Contains(p)))
            return ("region", stripped);

        return ("unknown", stripped);
    }

    private static int? ParseDiscNumber(string value)
    {
        var m = DiscRe.Match(value.Trim());
        if (!m.Success) return null;
        var raw = m.Groups[2].Value;
        if (int.TryParse(raw, out var n)) return n;
        if (raw.Length == 1 && char.IsLetter(raw[0]))
            return char.ToUpperInvariant(raw[0]) - 'A' + 1;
        return null;
    }

    private static string MoveTrailingArticleToFront(string name)
    {
        foreach (var article in Articles)
        {
            var pattern = new Regex(
                $@",\s*({Regex.Escape(article)})\s*$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            var m = pattern.Match(name);
            if (m.Success)
            {
                var @base = name[..m.Index].TrimEnd();
                var art = m.Groups[1].Value;
                // Title-case the article (The, A, La...)
                return $"{char.ToUpperInvariant(art[0])}{art[1..].ToLowerInvariant()} {@base}";
            }
        }
        return name;
    }
}
