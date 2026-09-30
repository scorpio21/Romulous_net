using FluentAssertions;
using Microsoft.Data.Sqlite;
using ROMulus.Core.Models;
using ROMulus.Core.Scanner;
using ROMulus.Infrastructure.Database;
using ROMulus.Infrastructure.SystemRegistry;

namespace ROMulus.Infrastructure.Tests;

/// <summary>
/// Integration tests for <see cref="LibraryScanner"/> and <see cref="SystemRegistryLoader"/>.
/// Mirrors Python's <c>test_scanner.py</c>.
/// Uses a real in-memory SQLite DB and a temporary directory on disk.
/// </summary>
public sealed class ScannerIntegrationTests : IDisposable
{
    private readonly ConnectionFactory _factory;
    private readonly SqliteConnection _conn;
    private readonly RomRepository _repo;
    private readonly LibraryScanner _scanner;
    private readonly string _tmpDir;

    public ScannerIntegrationTests()
    {
        _factory = new ConnectionFactory(":memory:");
        _conn = _factory.GetConnection();
        ConfigRepository configRepo = new(_conn);
        configRepo.SeedDefaults();

        _repo = new RomRepository(_conn);
        _scanner = new LibraryScanner(_repo);

        // Seed SNES + NES systems so the scanner can resolve folder aliases.
        _repo.UpsertSystems(
        [
            new SystemInfo
            {
                Id = "snes",
                DisplayName = "Super Nintendo Entertainment System",
                ShortName = "SNES",
                Extensions = [".sfc", ".smc"],
                FolderAliases = ["snes", "sfc", "supernintendo"],
            },
            new SystemInfo
            {
                Id = "nes",
                DisplayName = "Nintendo Entertainment System",
                ShortName = "NES",
                Extensions = [".nes"],
                FolderAliases = ["nes", "famicom"],
            },
        ]);

        _tmpDir = Path.Combine(Path.GetTempPath(), $"romulus_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tmpDir);
    }

    // ── YAML loader tests ─────────────────────────────────────────────────────

    [Fact]
    public void SystemRegistryLoader_ParsesMinimalYaml()
    {
        const string yaml = """
            systems:
              - id: snes
                display_name: Super Nintendo Entertainment System
                short_name: SNES
                extensions:
                  - .sfc
                  - .smc
                folder_aliases:
                  - snes
                  - supernintendo
            """;

        var systems = SystemRegistryLoader.LoadFromYaml(yaml);

        systems.Should().HaveCount(1);
        systems[0].Id.Should().Be("snes");
        systems[0].Extensions.Should().Contain(".sfc");
        systems[0].FolderAliases.Should().Contain("snes");
    }

    // ── LibraryScanner tests ──────────────────────────────────────────────────

    [Fact]
    public void Scan_EmptyLibrary_Returns_ZeroFiles()
    {
        var result = _scanner.Scan(_tmpDir);
        result.FilesFound.Should().Be(0);
        result.ScanId.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Scan_SnesFolder_DetectsSystem_And_EnrollsRom()
    {
        // Arrange: create snes/Game.sfc
        var snesDir = Path.Combine(_tmpDir, "snes");
        Directory.CreateDirectory(snesDir);
        File.WriteAllBytes(Path.Combine(snesDir, "Super Mario World (USA).sfc"), new byte[4]);

        // Act
        var result = _scanner.Scan(_tmpDir);

        // Assert
        result.FilesFound.Should().Be(1);
        result.SystemsSeen.Should().Contain("snes");

        var row = _conn.QuerySingleRom(
            "SELECT system_id, filename, title FROM roms LIMIT 1");
        row.SystemId.Should().Be("snes");
        row.Filename.Should().Be("Super Mario World (USA).sfc");
        row.Title.Should().Be("Super Mario World");
    }

    [Fact]
    public void Scan_SideFiles_AreSkipped()
    {
        var snesDir = Path.Combine(_tmpDir, "snes");
        Directory.CreateDirectory(snesDir);
        File.WriteAllBytes(Path.Combine(snesDir, "game.sfc"), new byte[4]);
        File.WriteAllBytes(Path.Combine(snesDir, "game.cue"), new byte[4]);
        File.WriteAllBytes(Path.Combine(snesDir, "readme.txt"), []);

        var result = _scanner.Scan(_tmpDir);

        result.FilesFound.Should().Be(1);       // only .sfc
        result.FilesSkipped.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Scan_ZipArchive_EnrolledRegardlessOfSystemExtensions()
    {
        var snesDir = Path.Combine(_tmpDir, "snes");
        Directory.CreateDirectory(snesDir);
        File.WriteAllBytes(Path.Combine(snesDir, "Zelda (USA).zip"), new byte[4]);

        var result = _scanner.Scan(_tmpDir);

        result.FilesFound.Should().Be(1);
    }

    [Fact]
    public void Scan_Tombstone_MissingFileMarked()
    {
        var snesDir = Path.Combine(_tmpDir, "snes");
        Directory.CreateDirectory(snesDir);
        var romPath = Path.Combine(snesDir, "TestGame (USA).sfc");
        File.WriteAllBytes(romPath, new byte[4]);

        // First scan — rom enrolled
        _scanner.Scan(_tmpDir);
        var missing1 = _conn.ExecuteScalar<int>("SELECT missing FROM roms WHERE filename = 'TestGame (USA).sfc'");
        missing1.Should().Be(0);

        // Delete the file, re-scan
        File.Delete(romPath);
        var result2 = _scanner.Scan(_tmpDir);

        var missing2 = _conn.ExecuteScalar<int>("SELECT missing FROM roms WHERE filename = 'TestGame (USA).sfc'");
        missing2.Should().Be(1, "tombstoned row keeps enrichment data");
        result2.FilesNewlyMissing.Should().Be(1);
    }

    [Fact]
    public void Scan_Tombstone_UnTombstonedOnReconnect()
    {
        var snesDir = Path.Combine(_tmpDir, "snes");
        Directory.CreateDirectory(snesDir);
        var romPath = Path.Combine(snesDir, "TestGame (USA).sfc");
        File.WriteAllBytes(romPath, new byte[4]);

        _scanner.Scan(_tmpDir);
        File.Delete(romPath);
        _scanner.Scan(_tmpDir); // tombstones row

        // "Reconnect" — file reappears
        File.WriteAllBytes(romPath, new byte[4]);
        _scanner.Scan(_tmpDir);

        var missing = _conn.ExecuteScalar<int>("SELECT missing FROM roms WHERE filename = 'TestGame (USA).sfc'");
        missing.Should().Be(0, "re-scanning un-tombstones via path-keyed upsert");
    }

    [Fact]
    public void Scan_ScopedBySystem_OnlyTombstonesTargetSystem()
    {
        var snesDir = Path.Combine(_tmpDir, "snes");
        var nesDir = Path.Combine(_tmpDir, "nes");
        Directory.CreateDirectory(snesDir);
        Directory.CreateDirectory(nesDir);
        var snesRom = Path.Combine(snesDir, "Game (USA).sfc");
        var nesRom = Path.Combine(nesDir, "Game (USA).nes");
        File.WriteAllBytes(snesRom, new byte[4]);
        File.WriteAllBytes(nesRom, new byte[4]);

        // Full scan to enroll both
        _scanner.Scan(_tmpDir);

        // Delete the SNES rom, rescan scoped to SNES only
        File.Delete(snesRom);
        _scanner.Scan(_tmpDir, scopeSystemId: "snes");

        var snesMissing = _conn.ExecuteScalar<int>("SELECT missing FROM roms WHERE system_id='snes'");
        var nesMissing = _conn.ExecuteScalar<int>("SELECT missing FROM roms WHERE system_id='nes'");
        snesMissing.Should().Be(1, "SNES rom was deleted");
        nesMissing.Should().Be(0, "NES rom was not in scope — must not be tombstoned");
    }

    public void Dispose()
    {
        _factory.Dispose();
        try { Directory.Delete(_tmpDir, recursive: true); }
        catch (IOException) { /* best-effort: temp dir may be locked on Windows */ }
    }
}

// Dapper helpers used in tests — uses raw SqliteCommand to avoid T? nullable issues.
file static class DapperEx
{
    /// <summary>Returns the first row as a typed ValueTuple, or default if no rows.</summary>
    public static (string SystemId, string Filename, string Title) QuerySingleRom(
        this SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return default;
        return (reader.GetString(0), reader.GetString(1), reader.GetString(2));
    }

    /// <summary>Returns the scalar result of a query as <typeparamref name="T"/>.</summary>
    public static T ExecuteScalar<T>(this SqliteConnection conn, string sql, string? param = null)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        if (param is not null)
            cmd.Parameters.AddWithValue("@p", param);
        var raw = cmd.ExecuteScalar();
        return (T)Convert.ChangeType(raw!, typeof(T));
    }
}
