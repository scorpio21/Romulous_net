using Microsoft.Extensions.DependencyInjection;
using ROMulus.Infrastructure.Database;
using Serilog;

namespace ROMulus.App;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // ── Logging ───────────────────────────────────────────────────────────
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(AppContext.BaseDirectory, "logs", "romulus.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        Log.Information("ROMulus {Version} starting", typeof(Program).Assembly.GetName().Version);

        // ── DI Container ──────────────────────────────────────────────────────
        var services = new ServiceCollection();
        ConfigureServices(services);
        using var provider = services.BuildServiceProvider();

        // ── WinForms bootstrap ────────────────────────────────────────────────
        ApplicationConfiguration.Initialize();
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.Run(provider.GetRequiredService<MainForm>());

        Log.CloseAndFlush();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Database
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ROMulus", "romulus.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        services.AddSingleton(_ => new ConnectionFactory(dbPath));
        services.AddSingleton(sp => sp.GetRequiredService<ConnectionFactory>().GetConnection());
        services.AddSingleton<ConfigRepository>();
        services.AddSingleton<RomRepository>();

        // Forms
        services.AddTransient<MainForm>();
    }
}
