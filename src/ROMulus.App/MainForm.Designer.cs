namespace ROMulus.App;

partial class MainForm
{
    /// <summary>Required designer variable.</summary>
    private System.ComponentModel.IContainer components = null!;

    // ── Controles ─────────────────────────────────────────────────────────────
    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.TreeView tvSystems;
    private System.Windows.Forms.DataGridView dgvRoms;
    private System.Windows.Forms.Panel pnlDetail;
    // Menú
    private System.Windows.Forms.ToolStripMenuItem miFile;
    private System.Windows.Forms.ToolStripMenuItem miFileOpenLibrary;
    private System.Windows.Forms.ToolStripSeparator miFileSep1;
    private System.Windows.Forms.ToolStripMenuItem miFileExit;
    private System.Windows.Forms.ToolStripMenuItem miLibrary;
    private System.Windows.Forms.ToolStripMenuItem miLibraryScan;
    private System.Windows.Forms.ToolStripMenuItem miLibraryHeavyScan;
    private System.Windows.Forms.ToolStripMenuItem miHelp;
    private System.Windows.Forms.ToolStripMenuItem miHelpAbout;
    // Toolbar
    private System.Windows.Forms.ToolStripButton tsbScan;
    private System.Windows.Forms.ToolStripButton tsbHeavyScan;
    private System.Windows.Forms.ToolStripSeparator tsSep1;
    private System.Windows.Forms.ToolStripButton tsbSettings;
    // Status bar
    private System.Windows.Forms.ToolStripStatusLabel tslStatus;
    private System.Windows.Forms.ToolStripProgressBar tspProgress;
    private System.Windows.Forms.ToolStripStatusLabel tslCount;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        // ── Instanciar controles ──────────────────────────────────────────────
        this.menuStrip1 = new System.Windows.Forms.MenuStrip();
        this.toolStrip1 = new System.Windows.Forms.ToolStrip();
        this.statusStrip1 = new System.Windows.Forms.StatusStrip();
        this.splitContainer1 = new System.Windows.Forms.SplitContainer();
        this.splitContainer2 = new System.Windows.Forms.SplitContainer();
        this.tvSystems = new System.Windows.Forms.TreeView();
        this.dgvRoms = new System.Windows.Forms.DataGridView();
        this.pnlDetail = new System.Windows.Forms.Panel();
        // menú
        this.miFile = new System.Windows.Forms.ToolStripMenuItem();
        this.miFileOpenLibrary = new System.Windows.Forms.ToolStripMenuItem();
        this.miFileSep1 = new System.Windows.Forms.ToolStripSeparator();
        this.miFileExit = new System.Windows.Forms.ToolStripMenuItem();
        this.miLibrary = new System.Windows.Forms.ToolStripMenuItem();
        this.miLibraryScan = new System.Windows.Forms.ToolStripMenuItem();
        this.miLibraryHeavyScan = new System.Windows.Forms.ToolStripMenuItem();
        this.miHelp = new System.Windows.Forms.ToolStripMenuItem();
        this.miHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
        // toolbar
        this.tsbScan = new System.Windows.Forms.ToolStripButton();
        this.tsbHeavyScan = new System.Windows.Forms.ToolStripButton();
        this.tsSep1 = new System.Windows.Forms.ToolStripSeparator();
        this.tsbSettings = new System.Windows.Forms.ToolStripButton();
        // status
        this.tslStatus = new System.Windows.Forms.ToolStripStatusLabel();
        this.tspProgress = new System.Windows.Forms.ToolStripProgressBar();
        this.tslCount = new System.Windows.Forms.ToolStripStatusLabel();

        // ── BeginInit ────────────────────────────────────────────────────────
        ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
        this.splitContainer1.Panel1.SuspendLayout();
        this.splitContainer1.Panel2.SuspendLayout();
        this.splitContainer1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
        this.splitContainer2.Panel1.SuspendLayout();
        this.splitContainer2.Panel2.SuspendLayout();
        this.splitContainer2.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvRoms)).BeginInit();
        this.menuStrip1.SuspendLayout();
        this.toolStrip1.SuspendLayout();
        this.statusStrip1.SuspendLayout();
        this.SuspendLayout();

        // ── menuStrip1 ───────────────────────────────────────────────────────
        this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFile,
            this.miLibrary,
            this.miHelp});
        this.menuStrip1.Location = new System.Drawing.Point(0, 0);
        this.menuStrip1.Name = "menuStrip1";
        this.menuStrip1.Size = new System.Drawing.Size(1200, 24);
        this.menuStrip1.TabIndex = 0;
        this.menuStrip1.Text = "menuStrip1";

        // ── miFile ───────────────────────────────────────────────────────────
        this.miFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFileOpenLibrary,
            this.miFileSep1,
            this.miFileExit});
        this.miFile.Name = "miFile";
        this.miFile.Size = new System.Drawing.Size(37, 20);
        this.miFile.Text = "&File";

        // ── miFileOpenLibrary ─────────────────────────────────────────────────
        this.miFileOpenLibrary.Name = "miFileOpenLibrary";
        this.miFileOpenLibrary.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
        this.miFileOpenLibrary.Size = new System.Drawing.Size(191, 22);
        this.miFileOpenLibrary.Text = "&Open Library\u2026";
        this.miFileOpenLibrary.Click += new System.EventHandler(this.MiFileOpenLibrary_Click);

        // ── miFileSep1 ────────────────────────────────────────────────────────
        this.miFileSep1.Name = "miFileSep1";
        this.miFileSep1.Size = new System.Drawing.Size(188, 6);

        // ── miFileExit ────────────────────────────────────────────────────────
        this.miFileExit.Name = "miFileExit";
        this.miFileExit.Size = new System.Drawing.Size(191, 22);
        this.miFileExit.Text = "E&xit";
        this.miFileExit.Click += new System.EventHandler(this.MiFileExit_Click);

        // ── miLibrary ────────────────────────────────────────────────────────
        this.miLibrary.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miLibraryScan,
            this.miLibraryHeavyScan});
        this.miLibrary.Name = "miLibrary";
        this.miLibrary.Size = new System.Drawing.Size(54, 20);
        this.miLibrary.Text = "&Library";

        // ── miLibraryScan ─────────────────────────────────────────────────────
        this.miLibraryScan.Name = "miLibraryScan";
        this.miLibraryScan.ShortcutKeys = System.Windows.Forms.Keys.F5;
        this.miLibraryScan.Size = new System.Drawing.Size(191, 22);
        this.miLibraryScan.Text = "&Quick Scan";
        this.miLibraryScan.Click += new System.EventHandler(this.MiLibraryScan_Click);

        // ── miLibraryHeavyScan ────────────────────────────────────────────────
        this.miLibraryHeavyScan.Name = "miLibraryHeavyScan";
        this.miLibraryHeavyScan.ShortcutKeys = System.Windows.Forms.Keys.F6;
        this.miLibraryHeavyScan.Size = new System.Drawing.Size(191, 22);
        this.miLibraryHeavyScan.Text = "&Heavy Scan\u2026";
        this.miLibraryHeavyScan.Click += new System.EventHandler(this.MiLibraryHeavyScan_Click);

        // ── miHelp ────────────────────────────────────────────────────────────
        this.miHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miHelpAbout});
        this.miHelp.Name = "miHelp";
        this.miHelp.Size = new System.Drawing.Size(44, 20);
        this.miHelp.Text = "&Help";

        // ── miHelpAbout ───────────────────────────────────────────────────────
        this.miHelpAbout.Name = "miHelpAbout";
        this.miHelpAbout.Size = new System.Drawing.Size(191, 22);
        this.miHelpAbout.Text = "&About ROMulus\u2026";
        this.miHelpAbout.Click += new System.EventHandler(this.MiHelpAbout_Click);

        // ── toolStrip1 ────────────────────────────────────────────────────────
        this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbScan,
            this.tsbHeavyScan,
            this.tsSep1,
            this.tsbSettings});
        this.toolStrip1.Location = new System.Drawing.Point(0, 24);
        this.toolStrip1.Name = "toolStrip1";
        this.toolStrip1.Size = new System.Drawing.Size(1200, 25);
        this.toolStrip1.TabIndex = 1;

        // ── tsbScan ───────────────────────────────────────────────────────────
        this.tsbScan.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
        this.tsbScan.Name = "tsbScan";
        this.tsbScan.Size = new System.Drawing.Size(65, 22);
        this.tsbScan.Text = "Quick Scan";
        this.tsbScan.ToolTipText = "Quick Scan (F5)";
        this.tsbScan.Click += new System.EventHandler(this.MiLibraryScan_Click);

        // ── tsbHeavyScan ──────────────────────────────────────────────────────
        this.tsbHeavyScan.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
        this.tsbHeavyScan.Name = "tsbHeavyScan";
        this.tsbHeavyScan.Size = new System.Drawing.Size(70, 22);
        this.tsbHeavyScan.Text = "Heavy Scan";
        this.tsbHeavyScan.ToolTipText = "Heavy Scan (F6)";
        this.tsbHeavyScan.Click += new System.EventHandler(this.MiLibraryHeavyScan_Click);

        // ── tsSep1 ────────────────────────────────────────────────────────────
        this.tsSep1.Name = "tsSep1";
        this.tsSep1.Size = new System.Drawing.Size(6, 25);

        // ── tsbSettings ───────────────────────────────────────────────────────
        this.tsbSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
        this.tsbSettings.Name = "tsbSettings";
        this.tsbSettings.Size = new System.Drawing.Size(52, 22);
        this.tsbSettings.Text = "Settings";
        this.tsbSettings.Click += new System.EventHandler(this.TsbSettings_Click);

        // ── statusStrip1 ──────────────────────────────────────────────────────
        this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslStatus,
            this.tspProgress,
            this.tslCount});
        this.statusStrip1.Location = new System.Drawing.Point(0, 698);
        this.statusStrip1.Name = "statusStrip1";
        this.statusStrip1.Size = new System.Drawing.Size(1200, 22);
        this.statusStrip1.TabIndex = 2;

        // ── tslStatus ─────────────────────────────────────────────────────────
        this.tslStatus.Name = "tslStatus";
        this.tslStatus.Size = new System.Drawing.Size(39, 17);
        this.tslStatus.Spring = true;
        this.tslStatus.Text = "Ready";
        this.tslStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        // ── tspProgress ───────────────────────────────────────────────────────
        this.tspProgress.Name = "tspProgress";
        this.tspProgress.Size = new System.Drawing.Size(200, 16);
        this.tspProgress.Visible = false;

        // ── tslCount ──────────────────────────────────────────────────────────
        this.tslCount.Name = "tslCount";
        this.tslCount.Size = new System.Drawing.Size(42, 17);
        this.tslCount.Text = "0 ROMs";

        // ── tvSystems ─────────────────────────────────────────────────────────
        this.tvSystems.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tvSystems.HideSelection = false;
        this.tvSystems.Location = new System.Drawing.Point(0, 0);
        this.tvSystems.Name = "tvSystems";
        this.tvSystems.Size = new System.Drawing.Size(200, 649);
        this.tvSystems.TabIndex = 0;
        this.tvSystems.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TvSystems_AfterSelect);

        // ── dgvRoms ───────────────────────────────────────────────────────────
        this.dgvRoms.AllowUserToAddRows = false;
        this.dgvRoms.AllowUserToDeleteRows = false;
        this.dgvRoms.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvRoms.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvRoms.Location = new System.Drawing.Point(0, 0);
        this.dgvRoms.MultiSelect = false;
        this.dgvRoms.Name = "dgvRoms";
        this.dgvRoms.ReadOnly = true;
        this.dgvRoms.RowHeadersVisible = false;
        this.dgvRoms.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvRoms.Size = new System.Drawing.Size(992, 430);
        this.dgvRoms.TabIndex = 0;
        this.dgvRoms.SelectionChanged += new System.EventHandler(this.DgvRoms_SelectionChanged);

        // ── pnlDetail ─────────────────────────────────────────────────────────
        this.pnlDetail.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlDetail.Location = new System.Drawing.Point(0, 0);
        this.pnlDetail.MinimumSize = new System.Drawing.Size(0, 100);
        this.pnlDetail.Name = "pnlDetail";
        this.pnlDetail.Padding = new System.Windows.Forms.Padding(4);
        this.pnlDetail.Size = new System.Drawing.Size(992, 215);
        this.pnlDetail.TabIndex = 0;

        // ── splitContainer2 (grid / detail) ───────────────────────────────────
        this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitContainer2.Location = new System.Drawing.Point(0, 0);
        this.splitContainer2.Name = "splitContainer2";
        this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
        this.splitContainer2.Panel1.Controls.Add(this.dgvRoms);
        this.splitContainer2.Panel1MinSize = 120;
        this.splitContainer2.Panel2.Controls.Add(this.pnlDetail);
        this.splitContainer2.Panel2MinSize = 100;
        this.splitContainer2.Size = new System.Drawing.Size(992, 649);
        this.splitContainer2.SplitterDistance = 430;
        this.splitContainer2.TabIndex = 0;

        // ── splitContainer1 (tree / content) ─────────────────────────────────
        this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitContainer1.Location = new System.Drawing.Point(0, 49);
        this.splitContainer1.Name = "splitContainer1";
        this.splitContainer1.Panel1.Controls.Add(this.tvSystems);
        this.splitContainer1.Panel1MinSize = 140;
        this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
        this.splitContainer1.Size = new System.Drawing.Size(1200, 649);
        this.splitContainer1.SplitterDistance = 204;
        this.splitContainer1.TabIndex = 3;

        // ── MainForm ──────────────────────────────────────────────────────────
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1200, 720);
        this.Controls.Add(this.splitContainer1);
        this.Controls.Add(this.toolStrip1);
        this.Controls.Add(this.menuStrip1);
        this.Controls.Add(this.statusStrip1);
        this.MainMenuStrip = this.menuStrip1;
        this.MinimumSize = new System.Drawing.Size(800, 500);
        this.Name = "MainForm";
        this.Text = "ROMulus";

        // ── EndInit ───────────────────────────────────────────────────────────
        this.splitContainer1.Panel1.ResumeLayout(false);
        this.splitContainer1.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
        this.splitContainer1.ResumeLayout(false);
        this.splitContainer2.Panel1.ResumeLayout(false);
        this.splitContainer2.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
        this.splitContainer2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvRoms)).EndInit();
        this.menuStrip1.ResumeLayout(false);
        this.menuStrip1.PerformLayout();
        this.toolStrip1.ResumeLayout(false);
        this.toolStrip1.PerformLayout();
        this.statusStrip1.ResumeLayout(false);
        this.statusStrip1.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion
}
