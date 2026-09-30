using ROMulus.Infrastructure.Database;
using Serilog;

namespace ROMulus.App;

/// <summary>
/// Main application window.
/// Layout: left panel = system tree | right panel top = ROM grid | right panel bottom = detail.
/// </summary>
public partial class MainForm : Form
{
    private readonly ConfigRepository _config;
    private readonly RomRepository _romRepo;

    public MainForm(ConfigRepository config, RomRepository romRepo)
    {
        _config = config;
        _romRepo = romRepo;

        InitializeComponent();

        Text = "ROMulus — ROM Collection Manager";
        Load += MainForm_Load;
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void MainForm_Load(object? sender, EventArgs e)
    {
        // Seed defaults on first run
        _config.SeedDefaults();

        // Restore window size / position from config
        if (int.TryParse(_config.Get("window_width"), out var w) &&
            int.TryParse(_config.Get("window_height"), out var h))
        {
            Width = Math.Max(w, MinimumSize.Width);
            Height = Math.Max(h, MinimumSize.Height);
        }

        LoadSystemTree();
        UpdateStatusBar();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Persist window size
        _config.SetMany(new Dictionary<string, string>
        {
            ["window_width"] = Width.ToString(),
            ["window_height"] = Height.ToString(),
        });
        base.OnFormClosing(e);
    }

    // ── System tree ───────────────────────────────────────────────────────────

    private void LoadSystemTree()
    {
        tvSystems.Nodes.Clear();

        var root = tvSystems.Nodes.Add("library", "📁 Library");
        var counts = _romRepo.GetCountBySystem();

        foreach (var (systemId, count) in counts.OrderBy(kv => kv.Key))
        {
            root.Nodes.Add(systemId, $"{systemId.ToUpperInvariant()}  ({count:N0})");
        }

        if (root.Nodes.Count == 0)
            root.Nodes.Add("empty", "(No ROMs scanned yet)");

        root.Expand();
    }

    private void TvSystems_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        var systemId = e.Node?.Name;
        if (systemId is null or "library" or "empty") return;
        LoadRomGrid(systemId);
    }

    // ── ROM grid ──────────────────────────────────────────────────────────────

    private void LoadRomGrid(string systemId)
    {
        // TODO (Session 8): load ROM rows from RomRepository filtered by systemId
        dgvRoms.Rows.Clear();
        dgvRoms.Columns.Clear();
        dgvRoms.Columns.Add("Title", "Title");
        dgvRoms.Columns.Add("Region", "Region");
        dgvRoms.Columns.Add("Revision", "Rev");
        dgvRoms.Columns.Add("Confidence", "Identified");

        tslStatus.Text = $"System: {systemId}";
    }

    private void DgvRoms_SelectionChanged(object? sender, EventArgs e)
    {
        // TODO (Session 9): populate detail panel
    }

    // ── Menu / toolbar handlers ───────────────────────────────────────────────

    private void MiFileOpenLibrary_Click(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Select your ROM library root folder",
            UseDescriptionForTitle = true,
        };

        var current = _config.Get(ConfigRepository.KeyLibraryPath);
        if (!string.IsNullOrEmpty(current) && Directory.Exists(current))
            dlg.InitialDirectory = current;

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        _config.Set(ConfigRepository.KeyLibraryPath, dlg.SelectedPath);
        LoadSystemTree();
        tslStatus.Text = $"Library: {dlg.SelectedPath}";
        Log.Information("Library path changed to {Path}", dlg.SelectedPath);
    }

    private void MiLibraryScan_Click(object? sender, EventArgs e)
    {
        var libraryPath = _config.Get(ConfigRepository.KeyLibraryPath);
        if (string.IsNullOrEmpty(libraryPath))
        {
            MessageBox.Show(
                "Please open a library folder first (File → Open Library…).",
                "No library selected",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        // TODO (Session 10): run LibraryScanner on a BackgroundWorker and show progress
        tslStatus.Text = $"Quick Scan: {libraryPath}  (coming in Session 10)";
    }

    private void MiLibraryHeavyScan_Click(object? sender, EventArgs e)
    {
        // TODO (Session 10)
        tslStatus.Text = "Heavy Scan coming in Session 10…";
    }

    private void TsbSettings_Click(object? sender, EventArgs e)
    {
        // TODO (Session 11)
        MessageBox.Show("Settings dialog coming in Session 11.", "ROMulus",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void MiFileExit_Click(object? sender, EventArgs e) => Close();

    private void MiHelpAbout_Click(object? sender, EventArgs e)
    {
        MessageBox.Show(
            "ROMulus — ROM Collection Manager\n" +
            "Version 0.5.0  •  .NET 8.0 / Windows Forms\n\n" +
            "Apache License 2.0\n" +
            "github.com/scorpio21/Romulous_net",
            "About ROMulus",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void UpdateStatusBar()
    {
        var path = _config.Get(ConfigRepository.KeyLibraryPath);
        tslStatus.Text = string.IsNullOrEmpty(path) ? "No library selected" : $"Library: {path}";
        tslCount.Text = "0 ROMs";
    }
}
