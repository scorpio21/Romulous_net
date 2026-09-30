using FluentAssertions;
using ROMulus.Core.Scanner;

namespace ROMulus.Core.Tests;

/// <summary>
/// Tests for <see cref="FilenameParser"/> and <see cref="NoIntroTokens"/>.
/// Mirrors the intent of Python's <c>test_scanner.py</c> filename-parsing tests.
/// </summary>
public sealed class FilenameParserTests
{
    // ── parse_filename ────────────────────────────────────────────────────────

    [Fact]
    public void Parse_NoIntroUsaRev_ExtractsAllFields()
    {
        var result = FilenameParser.Parse("Super Mario World (USA) (Rev 1).sfc");

        result.CleanName.Should().Be("Super Mario World");
        result.DisplayTitle.Should().Be("Super Mario World");
        result.Extension.Should().Be(".sfc");
        result.Region.Should().Be("USA");
        result.Revision.Should().Be("Rev 1");
        result.IsHack.Should().BeFalse();
        result.IsHomebrew.Should().BeFalse();
    }

    [Fact]
    public void Parse_TrailingArticle_MovedToFront()
    {
        var result = FilenameParser.Parse("Legend of Zelda, The (USA).sfc");

        result.DisplayTitle.Should().Be("The Legend of Zelda");
        result.Region.Should().Be("USA");
    }

    [Fact]
    public void Parse_GoodToolsHack_SetsIsHack()
    {
        var result = FilenameParser.Parse("Super Mario World [h1].sfc");
        result.IsHack.Should().BeTrue();
    }

    [Fact]
    public void Parse_GoodToolsVerified_SetsIsVerified()
    {
        var result = FilenameParser.Parse("Donkey Kong Country [!].sfc");
        result.IsVerified.Should().BeTrue();
    }

    [Fact]
    public void Parse_Homebrew_SetsIsHomebrew()
    {
        var result = FilenameParser.Parse("My Game (Homebrew).sfc");
        result.IsHomebrew.Should().BeTrue();
    }

    [Fact]
    public void Parse_DiscNumber_Parsed()
    {
        var result = FilenameParser.Parse("Final Fantasy VII (USA) (Disc 2).bin");
        result.DiscNumber.Should().Be(2);
    }

    [Fact]
    public void Parse_SideFile_DetectedBySideFileCheck()
    {
        FilenameParser.IsSideFile("game.cue").Should().BeTrue();
        FilenameParser.IsSideFile("game.sfc").Should().BeFalse();
    }

    [Fact]
    public void Parse_ArchiveExtension_AlwaysAccepted()
    {
        var exts = new HashSet<string> { ".sfc" };
        FilenameParser.IsRomFile("game.zip", exts).Should().BeTrue();
        FilenameParser.IsRomFile("game.7z", exts).Should().BeTrue();
    }

    // ── generate_fuzzy_key ────────────────────────────────────────────────────

    [Fact]
    public void FuzzyKey_StripsTags_AndNormalizesCase()
    {
        var parsed = FilenameParser.Parse("Super Mario World (USA) (Rev 1).sfc");
        var key = FilenameParser.GenerateFuzzyKey(parsed.CleanName, parsed.ReleaseType);
        key.Should().Be("supermarioworld");
    }

    [Fact]
    public void FuzzyKey_RomanNumerals_Converted()
    {
        var parsed = FilenameParser.Parse("Final Fantasy III (USA).snes");
        var key = FilenameParser.GenerateFuzzyKey(parsed.CleanName);
        key.Should().Be("finalfantasy3");
    }

    [Fact]
    public void FuzzyKey_LeadingArticle_Stripped()
    {
        var key = FilenameParser.GenerateFuzzyKey("The Legend of Zelda");
        key.Should().Be("legendofzelda");
    }

    [Fact]
    public void FuzzyKey_ReleaseType_AppendsDoubleSuffix()
    {
        var key = FilenameParser.GenerateFuzzyKey("Sonic the Hedgehog", "Virtual Console");
        key.Should().Contain("__virtualconsole");
    }

    [Fact]
    public void FuzzyKey_SameTitle_DifferentRegion_SameKey()
    {
        // Region is NOT part of the fuzzy key — two regional variants share a key
        var keyUsa = FilenameParser.GenerateFuzzyKey(
            FilenameParser.Parse("Pac-Man (USA).nes").CleanName);
        var keyEur = FilenameParser.GenerateFuzzyKey(
            FilenameParser.Parse("Pac-Man (Europe).nes").CleanName);
        keyUsa.Should().Be(keyEur);
    }
}
