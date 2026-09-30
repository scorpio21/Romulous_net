using Dapper;
using Microsoft.Data.Sqlite;

namespace ROMulus.Infrastructure.Database;

/// <summary>
/// Persistent key-value configuration backed by the <c>config</c> SQLite table.
/// Mirrors <c>romulus.db.config</c> from the Python codebase.
///
/// Keys and their default values are defined as string constants below.
/// The Settings dialog writes through this repository; there are no config files.
/// </summary>
public sealed class ConfigRepository
{
    // ── Config key constants ─────────────────────────────────────────────────

    public const string KeyLibraryPath = "library_path";
    public const string KeyTheme = "theme";
    public const string KeyDefaultView = "default_view";
    public const string KeyLogLevel = "log_level";
    public const string KeyDatFolders = "dat_folders";
    public const string KeyScreenScraperUsername = "screenscraper_username";
    public const string KeyScreenScraperPassword = "screenscraper_password";
    public const string KeyTheGamesDbApiKey = "thegamesdb_api_key";
    public const string KeyTheGamesDbRemainingAllowance = "thegamesdb_remaining_allowance";
    public const string KeyScanWorkers = "scan_workers";
    public const string KeyLaunchBoxXmlPath = "launchbox_xml_path";

    // ── Default values ───────────────────────────────────────────────────────

    private static readonly Dictionary<string, string> Defaults = new()
    {
        [KeyTheme] = "dark",
        [KeyDefaultView] = "table",
        [KeyLogLevel] = "INFO",
        [KeyDatFolders] = "",
        [KeyScanWorkers] = "4",
        [KeyScreenScraperUsername] = "",
        [KeyScreenScraperPassword] = "",
        [KeyTheGamesDbApiKey] = "",
        [KeyTheGamesDbRemainingAllowance] = "-1",
        [KeyLaunchBoxXmlPath] = "",
    };

    private readonly SqliteConnection _conn;

    public ConfigRepository(SqliteConnection connection)
    {
        _conn = connection;
    }

    // ── Read ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the stored value for <paramref name="key"/>, or the registered
    /// default (or null if there is no default).
    /// </summary>
    public string? Get(string key)
    {
        var value = _conn.QuerySingleOrDefault<string?>(
            "SELECT value FROM config WHERE key = @key",
            new { key });

        if (value is not null)
            return value;

        return Defaults.GetValueOrDefault(key);
    }

    /// <summary>Returns all config rows as a flat dictionary.</summary>
    public IReadOnlyDictionary<string, string> GetAll()
    {
        var stored = _conn.Query<(string Key, string Value)>(
            "SELECT key, value FROM config")
            .ToDictionary(r => r.Key, r => r.Value);

        // Merge defaults for keys not yet persisted
        var result = new Dictionary<string, string>(Defaults);
        foreach (var (k, v) in stored)
            result[k] = v;

        return result;
    }

    // ── Write ────────────────────────────────────────────────────────────────

    /// <summary>Persists a single key-value pair (INSERT OR REPLACE).</summary>
    public void Set(string key, string value)
    {
        _conn.Execute(
            "INSERT OR REPLACE INTO config (key, value) VALUES (@key, @value)",
            new { key, value });
    }

    /// <summary>Persists several key-value pairs in a single transaction.</summary>
    public void SetMany(IReadOnlyDictionary<string, string> values)
    {
        using var tx = _conn.BeginTransaction();
        foreach (var (key, value) in values)
        {
            _conn.Execute(
                "INSERT OR REPLACE INTO config (key, value) VALUES (@key, @value)",
                new { key, value },
                transaction: tx);
        }
        tx.Commit();
    }

    // ── Typed helpers ─────────────────────────────────────────────────────────

    public int GetInt(string key, int fallback = 0)
        => int.TryParse(Get(key), out var v) ? v : fallback;

    public bool GetBool(string key, bool fallback = false)
    {
        var raw = Get(key);
        if (raw is null) return fallback;
        return raw is "1" or "true" or "True" or "yes";
    }

    public void SetInt(string key, int value) => Set(key, value.ToString());
    public void SetBool(string key, bool value) => Set(key, value ? "1" : "0");

    /// <summary>
    /// Seeds missing config keys with their default values.
    /// Idempotent — safe to call on every startup after schema creation.
    /// </summary>
    public void SeedDefaults()
    {
        using var tx = _conn.BeginTransaction();
        foreach (var (key, value) in Defaults)
        {
            _conn.Execute(
                "INSERT OR IGNORE INTO config (key, value) VALUES (@key, @value)",
                new { key, value },
                transaction: tx);
        }
        tx.Commit();
    }
}
