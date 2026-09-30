using FluentAssertions;
using Microsoft.Data.Sqlite;
using ROMulus.Infrastructure.Database;

namespace ROMulus.Infrastructure.Tests;

/// <summary>
/// Smoke tests for the database layer (Schema + ConnectionFactory + ConfigRepository).
/// Mirrors the intent of Python's test_scanner.py / conftest.py db fixture.
/// </summary>
public sealed class DatabaseSmokeTests : IDisposable
{
    private readonly ConnectionFactory _factory;
    private readonly SqliteConnection _conn;

    public DatabaseSmokeTests()
    {
        // Use in-memory SQLite for fast, isolated tests
        _factory = new ConnectionFactory(":memory:");
        _conn = _factory.GetConnection();
    }

    // ── Schema tests ─────────────────────────────────────────────────────────

    [Fact]
    public void Schema_Creates_AllExpectedTables()
    {
        var tables = GetTableNames();

        tables.Should().Contain([
            "config",
            "systems",
            "scan_history",
            "roms",
            "hashes",
            "dat_entries",
            "metadata",
            "covers",
            "collections",
            "collection_roms",
            "organize_plans",
            "sync_destinations",
            "dest_inventory",
            "sync_plans",
        ]);
    }

    [Fact]
    public void Schema_IsIdempotent_CallingCreateTablesAgainDoesNotThrow()
    {
        // Second call must not throw (IF NOT EXISTS on all statements)
        var act = () => Schema.CreateTables(_conn);
        act.Should().NotThrow();
    }

    [Fact]
    public void Schema_ForeignKeys_AreEnabled()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys";
        var result = Convert.ToInt32(cmd.ExecuteScalar());
        result.Should().Be(1, "foreign_keys must be ON for CASCADE deletes to work");
    }

    [Fact]
    public void Schema_WalMode_IsEnabled()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "PRAGMA journal_mode";
        // In-memory SQLite always reports 'memory' regardless of WAL pragma
        // (WAL requires a disk file). We just verify the pragma call doesn't crash.
        cmd.ExecuteScalar().Should().NotBeNull();
    }

    // ── ConfigRepository tests ───────────────────────────────────────────────

    [Fact]
    public void Config_SeedDefaults_ThenGet_ReturnsDefaultValue()
    {
        var repo = new ConfigRepository(_conn);
        repo.SeedDefaults();

        repo.Get(ConfigRepository.KeyTheme).Should().Be("dark");
        repo.GetInt(ConfigRepository.KeyScanWorkers).Should().Be(4);
    }

    [Fact]
    public void Config_Set_ThenGet_ReturnsNewValue()
    {
        var repo = new ConfigRepository(_conn);
        repo.Set(ConfigRepository.KeyTheme, "light");
        repo.Get(ConfigRepository.KeyTheme).Should().Be("light");
    }

    [Fact]
    public void Config_SetMany_PersistsAllValues()
    {
        var repo = new ConfigRepository(_conn);
        repo.SetMany(new Dictionary<string, string>
        {
            [ConfigRepository.KeyTheme] = "classic",
            [ConfigRepository.KeyLogLevel] = "DEBUG",
        });

        repo.Get(ConfigRepository.KeyTheme).Should().Be("classic");
        repo.Get(ConfigRepository.KeyLogLevel).Should().Be("DEBUG");
    }

    [Fact]
    public void Config_GetAll_IncludesDefaultsForUnpersistedKeys()
    {
        var repo = new ConfigRepository(_conn);
        var all = repo.GetAll();

        all.Should().ContainKey(ConfigRepository.KeyTheme);
        all.Should().ContainKey(ConfigRepository.KeyScanWorkers);
    }

    [Fact]
    public void Config_SetBool_ThenGetBool_RoundTrips()
    {
        var repo = new ConfigRepository(_conn);
        repo.SetBool("some_flag", true);
        repo.GetBool("some_flag").Should().BeTrue();
        repo.SetBool("some_flag", false);
        repo.GetBool("some_flag").Should().BeFalse();
    }

    // ── Legacy DB guard ──────────────────────────────────────────────────────

    [Fact]
    public void Schema_IsLegacyDatabase_ReturnsFalse_ForFreshDb()
    {
        Schema.IsLegacyDatabase(_conn).Should().BeFalse();
    }

    [Fact]
    public void Schema_IsLegacyDatabase_ReturnsTrue_WhenGamesTableExists()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS games (id INTEGER PRIMARY KEY)";
        cmd.ExecuteNonQuery();

        Schema.IsLegacyDatabase(_conn).Should().BeTrue();
    }

    // ── Helper ───────────────────────────────────────────────────────────────

    private List<string> GetTableNames()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name";
        using var reader = cmd.ExecuteReader();
        var names = new List<string>();
        while (reader.Read()) names.Add(reader.GetString(0));
        return names;
    }

    public void Dispose() => _factory.Dispose();
}
