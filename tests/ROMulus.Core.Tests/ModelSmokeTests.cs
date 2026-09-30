using FluentAssertions;
using ROMulus.Core.Models;

namespace ROMulus.Core.Tests;

/// <summary>
/// Smoke tests for Core model records — verify construction, default values,
/// and equality semantics of the C# record types.
/// </summary>
public sealed class ModelSmokeTests
{
    [Fact]
    public void Rom_DefaultMatchConfidence_IsUnmatched()
    {
        var rom = new Rom
        {
            Path = @"C:\roms\snes\Zelda (USA).sfc",
            Filename = "Zelda (USA).sfc",
            Extension = ".sfc",
            SizeBytes = 1_048_576,
            Mtime = 1_700_000_000.0,
        };

        rom.MatchConfidence.Should().Be(MatchConfidence.Unmatched);
        rom.Missing.Should().BeFalse();
        rom.IsHack.Should().BeFalse();
        rom.IsHomebrew.Should().BeFalse();
        rom.IsBios.Should().BeFalse();
        rom.Id.Should().BeNull();
    }

    [Fact]
    public void Rom_WithExpression_ProducesNewRecord()
    {
        var original = new Rom
        {
            Path = @"C:\roms\snes\Zelda (USA).sfc",
            Filename = "Zelda (USA).sfc",
            Extension = ".sfc",
            SizeBytes = 1_048_576,
            Mtime = 1_700_000_000.0,
        };

        var identified = original with
        {
            SystemId = "snes",
            MatchConfidence = MatchConfidence.DatVerified,
            CanonicalName = "The Legend of Zelda - A Link to the Past (USA)",
        };

        identified.SystemId.Should().Be("snes");
        identified.MatchConfidence.Should().Be(MatchConfidence.DatVerified);
        // Original is untouched (records are value types)
        original.SystemId.Should().BeNull();
        original.MatchConfidence.Should().Be(MatchConfidence.Unmatched);
    }

    [Fact]
    public void SystemMapping_IsSupported_FalseWhenFolderEmpty()
    {
        var mapping = new SystemMapping { Supported = true, Folder = "" };
        mapping.IsSupported.Should().BeFalse();
    }

    [Fact]
    public void SystemMapping_IsSupported_FalseWhenSupportedFalse()
    {
        var mapping = new SystemMapping { Supported = false, Folder = "snes" };
        mapping.IsSupported.Should().BeFalse();
    }

    [Fact]
    public void SystemMapping_IsSupported_TrueWhenBothSet()
    {
        var mapping = new SystemMapping { Supported = true, Folder = "snes" };
        mapping.IsSupported.Should().BeTrue();
    }

    [Fact]
    public void MatchConfidence_ParseFromString_Works()
    {
        // Verify the enum values used by the DB mapper
        Enum.Parse<MatchConfidence>("Unmatched", ignoreCase: true).Should().Be(MatchConfidence.Unmatched);
        Enum.Parse<MatchConfidence>("DatVerified", ignoreCase: true).Should().Be(MatchConfidence.DatVerified);
    }
}
