using Microsoft.Data.Sqlite;

namespace ROMulus.Infrastructure.Database;

/// <summary>
/// Creates and manages SQLite connections for ROMulus.
/// Mirrors <c>romulus.db.connection</c> from the Python codebase.
///
/// Design notes:
/// - WAL mode: improves read concurrency and crash recovery.
/// - Foreign keys: must be enabled per-connection in SQLite.
/// - A single shared connection is used for the lifetime of the application
///   (single-process, single-user desktop app — no pool required).
/// </summary>
public sealed class ConnectionFactory : IDisposable
{
    private SqliteConnection? _connection;
    private readonly string _dbPath;
    private bool _disposed;

    /// <param name="dbPath">Absolute path to the SQLite database file.</param>
    public ConnectionFactory(string dbPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dbPath);
        _dbPath = dbPath;
    }

    /// <summary>
    /// Opens (or returns the already-open) shared connection.
    /// On first open, enables WAL mode, foreign keys, and creates all tables.
    /// </summary>
    public SqliteConnection GetConnection()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_connection is not null)
            return _connection;

        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = _dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
        };

        _connection = new SqliteConnection(builder.ToString());
        _connection.Open();

        // Enable WAL for better concurrency and crash safety
        Execute("PRAGMA journal_mode=WAL");
        // Enforce FK constraints (SQLite disables them by default)
        Execute("PRAGMA foreign_keys=ON");
        // Busy timeout: wait up to 5 s instead of immediately throwing on lock
        Execute("PRAGMA busy_timeout=5000");

        // Guard against legacy databases from the Python v0.3.x era
        if (Schema.IsLegacyDatabase(_connection))
            throw new InvalidOperationException(Schema.RequiresFreshDbMessage);

        // Ensure schema is up to date (idempotent)
        Schema.CreateTables(_connection);

        return _connection;
    }

    private void Execute(string sql)
    {
        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _connection?.Dispose();
        _connection = null;
    }
}
