namespace ROMulus.App;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null!;

    // ── Controls ──────────────────────────────────────────────────────────────
    private MenuStrip menuStrip;
    private ToolStrip toolStrip;
    private StatusStrip statusStrip;
    private SplitContainer splitMain;
    private TreeView tvSystems;
    private DataGridView dgvRoms;
    private Panel pnlDetail;
    private ToolStripMenuItem miFile;
    private ToolStripMenuItem miFileOpenLibrary;
    private ToolStripMenuItem miFileSeparator1;
    private ToolStripMenuItem miFileExit;
    private ToolStripMenuItem miLibrary;
    private ToolStripMenuItem miLibraryScan;
    private ToolStripMenuItem miLibraryHeavyScan;
    private ToolStripMenuItem miHelp;
    private ToolStripMenuItem miHelpAbout;
    private ToolStripButton tsbScan;
    private ToolStripButton tsbHeavyScan;
    private ToolStripSeparator tsSep1;
    private ToolStripButton tsbSettings;
    private ToolStripStatusLabel tslStatus;
    private ToolStripProgressBar tspProgress;
    private ToolStripStatusLabel tslCount;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support — do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        // ── Menu ──────────────────────────────────────────────────────────────
        menuStrip = new MenuStrip();
        miFile = new ToolStripMenuItem("&File");
        miFileOpenLibrary = new ToolStripMenuItem("&Open Library…", null, MiFileOpenLibrary_Click, Keys.Control | Keys.O);
        miFileSeparator1 = new ToolStripMenuItem("-");
        miFileExit = new ToolStripMenuItem("E&xit", null, (_, _) => Close(), Keys.Alt | Keys.F4);
        miFile.DropDownItems.AddRange([miFileOpenLibrary, new ToolStripSeparator(), miFileExit]);

        miLibrary = new ToolStripMenuItem("&Library");
        miLibraryScan = new ToolStripMenuItem("&Quick Scan", null, MiLibraryScan_Click, Keys.F5);
        miLibraryHeavyScan = new ToolStripMenuItem("&Heavy Scan (Hash + DAT)…", null, MiLibraryHeavyScan_Click, Keys.F6);
        miLibrary.DropDownItems.AddRange([miLibraryScan, miLibraryHeavyScan]);

        miHelp = new ToolStripMenuItem("&Help");
        miHelpAbout = new ToolStripMenuItem("&About ROMulus…", null, MiHelpAbout_Click);
        miHelp.DropDownItems.Add(miHelpAbout);

        menuStrip.Items.AddRange([miFile, miLibrary, miHelp]);

        // ── Toolbar ───────────────────────────────────────────────────────────
        toolStrip = new ToolStrip();
        tsbScan = new ToolStripButton("Quick Scan") { ToolTipText = "Quick Scan (F5)", Image = null };
        tsbScan.Click += MiLibraryScan_Click;
        tsbHeavyScan = new ToolStripButton("Heavy Scan") { ToolTipText = "Heavy Scan (F6)", Image = null };
        tsbHeavyScan.Click += MiLibraryHeavyScan_Click;
        tsSep1 = new ToolStripSeparator();
        tsbSettings = new ToolStripButton("Settings") { ToolTipText = "Settings" };
        tsbSettings.Click += TsbSettings_Click;
        toolStrip.Items.AddRange([tsbScan, tsbHeavyScan, tsSep1, tsbSettings]);

        // ── Status bar ────────────────────────────────────────────────────────
        statusStrip = new StatusStrip();
        tslStatus = new ToolStripStatusLabel("Ready") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
        tspProgress = new ToolStripProgressBar { Visible = false, Width = 200 };
        tslCount = new ToolStripStatusLabel("0 ROMs");
        statusStrip.Items.AddRange([tslStatus, tspProgress, tslCount]);

        // ── System tree (left panel) ──────────────────────────────────────────
        tvSystems = new TreeView
        {
            Dock = DockStyle.Fill,
            HideSelection = false,
            ShowLines = true,
        };
        tvSystems.AfterSelect += TvSystems_AfterSelect;

        // ── ROM grid (right panel, top) ───────────────────────────────────────
        dgvRoms = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
        };
        dgvRoms.SelectionChanged += DgvRoms_SelectionChanged;

        // ── Detail panel (right, bottom) ──────────────────────────────────────
        pnlDetail = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(4),
            MinimumSize = new Size(0, 120),
        };

        // ── Inner split: grid/detail ──────────────────────────────────────────
        var splitRight = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 400,
            Panel1MinSize = 120,
            Panel2MinSize = 100,
        };
        splitRight.Panel1.Controls.Add(dgvRoms);
        splitRight.Panel2.Controls.Add(pnlDetail);

        // ── Main split: tree/content ──────────────────────────────────────────
        splitMain = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 200,
            Panel1MinSize = 140,
        };
        splitMain.Panel1.Controls.Add(tvSystems);
        splitMain.Panel2.Controls.Add(splitRight);

        // ── Form setup ────────────────────────────────────────────────────────
        SuspendLayout();
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 720);
        MinimumSize = new Size(800, 500);
        Text = "ROMulus";
        MainMenuStrip = menuStrip;
        Controls.AddRange([menuStrip, toolStrip, statusStrip, splitMain]);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
}
