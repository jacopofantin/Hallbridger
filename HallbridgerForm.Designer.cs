using Hallbridger.Controls;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Xbim.Presentation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Hallbridger
{
    partial class HallbridgerForm
    {
        /* VARIABLE DECLARATION
         */

        // moving element data and acoustic data tabs
        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage movingElementsTab;
        private System.Windows.Forms.TabPage acousticsTab;

        // real hall data group title
        private System.Windows.Forms.Label realHallDataGroupLabel;

        // real hall DataGridView titles
        private System.Windows.Forms.Label realHallStagecraftDataGridViewLabel;
        private System.Windows.Forms.Label realHallLeftPanelsDataGridViewLabel;
        private System.Windows.Forms.Label realHallRightPanelsDataGridViewLabel;

        // real hall DataGridView for viewing stagecraft equipment positions and left/right panel array apertures coming from the hall
        private System.Windows.Forms.DataGridView realHallStagecraftDataGridView;
        private System.Windows.Forms.DataGridView realHallLeftPanelsDataGridView;
        private System.Windows.Forms.DataGridView realHallRightPanelsDataGridView;

        // Horizontal separator between real and 3D hall data groups
        private System.Windows.Forms.Label groupSeparator;

        // 3D hall data group title
        private System.Windows.Forms.Label hall3DModelDataGroupLabel;

        // 3D hall DataGridView titles
        private System.Windows.Forms.Label hall3DModelStagecraftDataGridViewLabel;
        private System.Windows.Forms.Label hall3DModelLeftPanelsDataGridViewLabel;
        private System.Windows.Forms.Label hall3DModelRightPanelsDataGridViewLabel;

        // 3D hall DataGridView for viewing stagecraft equipment positions and left/right panel array apertures coming from 3D hall
        private System.Windows.Forms.DataGridView hall3DModelStagecraftDataGridView;
        private System.Windows.Forms.DataGridView hall3DModelLeftPanelsDataGridView;
        private System.Windows.Forms.DataGridView hall3DModelRightPanelsDataGridView;

        // "Load real hall data" button
        private System.Windows.Forms.Button loadRealHallDataButton;

        // "Load 3D hall" button
        private System.Windows.Forms.Button loadHall3DModelButton;

        // "Update 3D hall" button
        private System.Windows.Forms.Button updateHall3DModelButton;

        // "Export 3D hall data" button
        private System.Windows.Forms.Button exportHall3DModelDataButton;

        // "Highlight data discrepancies" checkbox
        private System.Windows.Forms.CheckBox highlightDataDiscrepanciesCheckBox;

        // "Highlight data discrepancies" tooltip
        private System.Windows.Forms.ToolTip highlightDataDiscrepanciesToolTip;

        // global RT values DataGridView title
        private System.Windows.Forms.Label globalRtDataGridViewLabel;

        // global RT values DataGridView
        private System.Windows.Forms.DataGridView globalRtDataGridView;

        // WPF host and 3D viewer
        private ElementHost hall3DModelViewerHost;
        private DrawingControl3D hall3DModelViewer;

        // "Reposition" button
        private System.Windows.Forms.Button repositionButton;

        // "High performance mode" checkbox
        private System.Windows.Forms.CheckBox highSpeedCheckBox;

        // menu bar and items
        private System.Windows.Forms.MenuStrip mainMenuBar;
        private System.Windows.Forms.ToolStripMenuItem optionsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem automaticDiscrepancyHighlightingMenuEntry;
        private System.Windows.Forms.ToolStripMenuItem configurationsMenuEntry;


        // method for initializing and creating form elements
        private void InitializeComponent()
        {
            /* ELEMENT CREATION
             */

            // main TabControl and TabPages creation
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.movingElementsTab = new System.Windows.Forms.TabPage();
            this.acousticsTab = new System.Windows.Forms.TabPage();

            // real hall data group title creation
            this.realHallDataGroupLabel = new System.Windows.Forms.Label();

            // real hall DataGridView titles creation
            this.realHallStagecraftDataGridViewLabel = new System.Windows.Forms.Label();
            this.realHallLeftPanelsDataGridViewLabel = new System.Windows.Forms.Label();
            this.realHallRightPanelsDataGridViewLabel = new System.Windows.Forms.Label();

            // real hall DataGridView creation
            this.realHallStagecraftDataGridView = new System.Windows.Forms.DataGridView();
            this.realHallLeftPanelsDataGridView = new System.Windows.Forms.DataGridView();
            this.realHallRightPanelsDataGridView = new System.Windows.Forms.DataGridView();

            // Horizontal separator creation
            this.groupSeparator = new System.Windows.Forms.Label();

            // 3D hall data group title creation
            this.hall3DModelDataGroupLabel = new System.Windows.Forms.Label();

            // 3D hall DataGridView titles creation
            this.hall3DModelStagecraftDataGridViewLabel = new System.Windows.Forms.Label();
            this.hall3DModelLeftPanelsDataGridViewLabel = new System.Windows.Forms.Label();
            this.hall3DModelRightPanelsDataGridViewLabel = new System.Windows.Forms.Label();

            // 3D hall DataGridView creation
            this.hall3DModelStagecraftDataGridView = new System.Windows.Forms.DataGridView();
            this.hall3DModelLeftPanelsDataGridView = new System.Windows.Forms.DataGridView();
            this.hall3DModelRightPanelsDataGridView = new System.Windows.Forms.DataGridView();

            // "Load real hall data" button creation
            this.loadRealHallDataButton = new System.Windows.Forms.Button();

            // "Load 3D hall" button creation
            this.loadHall3DModelButton = new System.Windows.Forms.Button();

            // "Update 3D hall" button creation
            this.updateHall3DModelButton = new System.Windows.Forms.Button();

            // "Export 3D hall data" button creation
            this.exportHall3DModelDataButton = new System.Windows.Forms.Button();

            // "Highlight data discrepancies" checkbox creation
            this.highlightDataDiscrepanciesCheckBox = new System.Windows.Forms.CheckBox();

            // "Highlight data discrepancies" tooltip creation
            this.highlightDataDiscrepanciesToolTip = new System.Windows.Forms.ToolTip();

            // global RT values DataGridView title creation
            this.globalRtDataGridViewLabel = new System.Windows.Forms.Label();

            // global RT values DataGridView creation
            this.globalRtDataGridView = new System.Windows.Forms.DataGridView();

            // WPF 3D viewer creation
            hall3DModelViewer = new DrawingControl3D();
            hall3DModelViewerHost = new ElementHost
            {
                Name = "3DViewerHost",
                Dock = DockStyle.None,
            };

            // "Reposition" button creation
            this.repositionButton = new System.Windows.Forms.Button();

            // "High performance mode" checkbox creation
            this.highSpeedCheckBox = new System.Windows.Forms.CheckBox();

            // menu bar and items creation
            this.mainMenuBar = new System.Windows.Forms.MenuStrip();
            this.optionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.automaticDiscrepancyHighlightingMenuEntry = new System.Windows.Forms.ToolStripMenuItem();
            this.configurationsMenuEntry = new System.Windows.Forms.ToolStripMenuItem();

            this.SuspendLayout();
            this.mainMenuBar.SuspendLayout();


            /* ELEMENT INITIALIZATION
             */

            // real hall data group title initialization
            this.realHallDataGroupLabel.Size = new System.Drawing.Size(300, 30);
            this.realHallDataGroupLabel.Location = new System.Drawing.Point((this.ClientSize.Width - 300) / 2, 16);
            this.realHallDataGroupLabel.Text = "Real hall data";
            this.realHallDataGroupLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.realHallDataGroupLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.realHallDataGroupLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;

            // title for real hall stagecraft equipment DataGridView initialization
            this.realHallStagecraftDataGridViewLabel.Location = new System.Drawing.Point(12, 56);
            this.realHallStagecraftDataGridViewLabel.Size = new System.Drawing.Size(240, 20);
            this.realHallStagecraftDataGridViewLabel.Text = "Stagecraft equipment positions";
            this.realHallStagecraftDataGridViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // title for real hall left panel array DataGridView initialization
            this.realHallLeftPanelsDataGridViewLabel.Location = new System.Drawing.Point(270, 56);
            this.realHallLeftPanelsDataGridViewLabel.Size = new System.Drawing.Size(240, 20);
            this.realHallLeftPanelsDataGridViewLabel.Text = "Left panel array apertures";
            this.realHallLeftPanelsDataGridViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // title for real hall right panel array DataGridView initialization
            this.realHallRightPanelsDataGridViewLabel.Location = new System.Drawing.Point(528, 56);
            this.realHallRightPanelsDataGridViewLabel.Size = new System.Drawing.Size(240, 20);
            this.realHallRightPanelsDataGridViewLabel.Text = "Right panel array apertures";
            this.realHallRightPanelsDataGridViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // real hall DataGridView for stagecraft equipment initialization
            this.realHallStagecraftDataGridView.Location = new System.Drawing.Point(12, 76);
            this.realHallStagecraftDataGridView.Size = new System.Drawing.Size(240, 120);
            this.realHallStagecraftDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.realHallStagecraftDataGridView.AllowUserToAddRows = false;
            this.realHallStagecraftDataGridView.ColumnCount = 2;
            this.realHallStagecraftDataGridView.Columns[0].Name = "Name";
            this.realHallStagecraftDataGridView.Columns[0].Width = this.realHallStagecraftDataGridView.Width / 2;
            this.realHallStagecraftDataGridView.Columns[1].Name = "Value";
            this.realHallStagecraftDataGridView.Columns[1].Width = this.realHallStagecraftDataGridView.Width / 2;
            this.realHallStagecraftDataGridView.Name = "realHallStagecraftDataGridView";
            this.realHallStagecraftDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_ColumnHeaderMouseDown);
            this.realHallStagecraftDataGridView.Sorted += new System.EventHandler(this.DataGridView_Sorted);

            // real hall DataGridView for left panel arrays initialization
            this.realHallLeftPanelsDataGridView.Location = new System.Drawing.Point(270, 76);
            this.realHallLeftPanelsDataGridView.Size = new System.Drawing.Size(240, 120);
            this.realHallLeftPanelsDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.realHallLeftPanelsDataGridView.AllowUserToAddRows = false;
            this.realHallLeftPanelsDataGridView.ColumnCount = 2;
            this.realHallLeftPanelsDataGridView.Columns[0].Name = "Name";
            this.realHallLeftPanelsDataGridView.Columns[0].Width = this.realHallLeftPanelsDataGridView.Width / 2;
            this.realHallLeftPanelsDataGridView.Columns[1].Name = "Value";
            this.realHallLeftPanelsDataGridView.Columns[1].Width = this.realHallLeftPanelsDataGridView.Width / 2;
            this.realHallLeftPanelsDataGridView.Name = "realHallLeftPanelsDataGridView";
            this.realHallLeftPanelsDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_ColumnHeaderMouseDown);
            this.realHallLeftPanelsDataGridView.Sorted += new System.EventHandler(this.DataGridView_Sorted);

            // real hall DataGridView for right panel arrays initialization
            this.realHallRightPanelsDataGridView.Location = new System.Drawing.Point(528, 76);
            this.realHallRightPanelsDataGridView.Size = new System.Drawing.Size(240, 120);
            this.realHallRightPanelsDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.realHallRightPanelsDataGridView.AllowUserToAddRows = false;
            this.realHallRightPanelsDataGridView.ColumnCount = 2;
            this.realHallRightPanelsDataGridView.Columns[0].Name = "Name";
            this.realHallRightPanelsDataGridView.Columns[0].Width = this.realHallRightPanelsDataGridView.Width / 2;
            this.realHallRightPanelsDataGridView.Columns[1].Name = "Value";
            this.realHallRightPanelsDataGridView.Columns[1].Width = this.realHallRightPanelsDataGridView.Width / 2;
            this.realHallRightPanelsDataGridView.Name = "realHallRightPanelsDataGridView";
            this.realHallRightPanelsDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_ColumnHeaderMouseDown);
            this.realHallRightPanelsDataGridView.Sorted += new System.EventHandler(this.DataGridView_Sorted);

            // Horizontal separator initialization
            this.groupSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.groupSeparator.Location = new System.Drawing.Point(12, 206);
            this.groupSeparator.Size = new System.Drawing.Size(776, 2);
            this.groupSeparator.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // 3D hall data group title initialization
            this.hall3DModelDataGroupLabel.Size = new System.Drawing.Size(300, 30);
            this.hall3DModelDataGroupLabel.Location = new System.Drawing.Point((this.ClientSize.Width - 300) / 2, 216);
            this.hall3DModelDataGroupLabel.Text = "3D hall data";
            this.hall3DModelDataGroupLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.hall3DModelDataGroupLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.hall3DModelDataGroupLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;

            // title for 3D hall stagecraft equipment DataGridView initialization
            this.hall3DModelStagecraftDataGridViewLabel.Location = new System.Drawing.Point(12, 256);
            this.hall3DModelStagecraftDataGridViewLabel.Size = new System.Drawing.Size(240, 20);
            this.hall3DModelStagecraftDataGridViewLabel.Text = "Stagecraft equipment positions";
            this.hall3DModelStagecraftDataGridViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // title for 3D hall left panel array DataGridView initialization
            this.hall3DModelLeftPanelsDataGridViewLabel.Location = new System.Drawing.Point(270, 256);
            this.hall3DModelLeftPanelsDataGridViewLabel.Size = new System.Drawing.Size(240, 20);
            this.hall3DModelLeftPanelsDataGridViewLabel.Text = "Left panel array apertures";
            this.hall3DModelLeftPanelsDataGridViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // title for 3D hall right panel array DataGridView initialization
            this.hall3DModelRightPanelsDataGridViewLabel.Location = new System.Drawing.Point(528, 256);
            this.hall3DModelRightPanelsDataGridViewLabel.Size = new System.Drawing.Size(240, 20);
            this.hall3DModelRightPanelsDataGridViewLabel.Text = "Right panel array apertures";
            this.hall3DModelRightPanelsDataGridViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // 3D hall DataGridView for stagecraft equipment initialization
            this.hall3DModelStagecraftDataGridView.Location = new System.Drawing.Point(12, 276);
            this.hall3DModelStagecraftDataGridView.Size = new System.Drawing.Size(240, 120);
            this.hall3DModelStagecraftDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.hall3DModelStagecraftDataGridView.AllowUserToAddRows = false;
            this.hall3DModelStagecraftDataGridView.ColumnCount = 2;
            this.hall3DModelStagecraftDataGridView.Columns[0].Name = "Name";
            this.hall3DModelStagecraftDataGridView.Columns[0].Width = this.hall3DModelStagecraftDataGridView.Width / 2;
            this.hall3DModelStagecraftDataGridView.Columns[1].Name = "Value";
            this.hall3DModelStagecraftDataGridView.Columns[1].Width = this.hall3DModelStagecraftDataGridView.Width / 2;
            this.hall3DModelStagecraftDataGridView.Name = "hall3DModelStagecraftDataGridView";
            this.hall3DModelStagecraftDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_ColumnHeaderMouseDown);
            this.hall3DModelStagecraftDataGridView.Sorted += new System.EventHandler(this.DataGridView_Sorted);

            // 3D hall DataGridView for left panel arrays initialization
            this.hall3DModelLeftPanelsDataGridView.Location = new System.Drawing.Point(270, 276);
            this.hall3DModelLeftPanelsDataGridView.Size = new System.Drawing.Size(240, 120);
            this.hall3DModelLeftPanelsDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.hall3DModelLeftPanelsDataGridView.AllowUserToAddRows = false;
            this.hall3DModelLeftPanelsDataGridView.ColumnCount = 2;
            this.hall3DModelLeftPanelsDataGridView.Columns[0].Name = "Name";
            this.hall3DModelLeftPanelsDataGridView.Columns[0].Width = this.hall3DModelLeftPanelsDataGridView.Width / 2;
            this.hall3DModelLeftPanelsDataGridView.Columns[1].Name = "Value";
            this.hall3DModelLeftPanelsDataGridView.Columns[1].Width = this.hall3DModelLeftPanelsDataGridView.Width / 2;
            this.hall3DModelLeftPanelsDataGridView.Name = "hall3DModelLeftPanelDataGridView";
            this.hall3DModelLeftPanelsDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_ColumnHeaderMouseDown);
            this.hall3DModelLeftPanelsDataGridView.Sorted += new System.EventHandler(this.DataGridView_Sorted);

            // 3D hall DataGridView for right panel arrays initialization
            this.hall3DModelRightPanelsDataGridView.Location = new System.Drawing.Point(528, 276);
            this.hall3DModelRightPanelsDataGridView.Size = new System.Drawing.Size(240, 120);
            this.hall3DModelRightPanelsDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.hall3DModelRightPanelsDataGridView.AllowUserToAddRows = false;
            this.hall3DModelRightPanelsDataGridView.ColumnCount = 2;
            this.hall3DModelRightPanelsDataGridView.Columns[0].Name = "Name";
            this.hall3DModelRightPanelsDataGridView.Columns[0].Width = this.hall3DModelRightPanelsDataGridView.Width / 2;
            this.hall3DModelRightPanelsDataGridView.Columns[1].Name = "Value";
            this.hall3DModelRightPanelsDataGridView.Columns[1].Width = this.hall3DModelRightPanelsDataGridView.Width / 2;
            this.hall3DModelRightPanelsDataGridView.Name = "hall3DModelRightPanelDataGridView";
            this.hall3DModelRightPanelsDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_ColumnHeaderMouseDown);
            this.hall3DModelRightPanelsDataGridView.Sorted += new System.EventHandler(this.DataGridView_Sorted);

            // "Load real hall data" button initialization
            this.loadRealHallDataButton.Location = new System.Drawing.Point(172, 410);
            this.loadRealHallDataButton.Anchor = AnchorStyles.Bottom;
            this.loadRealHallDataButton.Name = "loadRealHallDataButton";
            this.loadRealHallDataButton.Size = new System.Drawing.Size(160, 30);
            this.loadRealHallDataButton.TabIndex = 1;
            this.loadRealHallDataButton.Text = "Load real hall data";
            this.loadRealHallDataButton.UseVisualStyleBackColor = true;
            this.loadRealHallDataButton.Click += new System.EventHandler(this.LoadRealHallDataButton_OnClick);

            // "Load 3D hall" button initialization
            this.loadHall3DModelButton.Location = new System.Drawing.Point(340, 410);
            this.loadHall3DModelButton.Anchor = AnchorStyles.Bottom;
            this.loadHall3DModelButton.Name = "loadHall3DModelButton";
            this.loadHall3DModelButton.Size = new System.Drawing.Size(120, 30);
            this.loadHall3DModelButton.TabIndex = 2;
            this.loadHall3DModelButton.Text = "Load 3D hall";
            this.loadHall3DModelButton.UseVisualStyleBackColor = true;
            this.loadHall3DModelButton.Click += new System.EventHandler(this.LoadHall3DModelButton_OnClick);

            // "Update 3D hall" button initialization
            this.updateHall3DModelButton.Location = new System.Drawing.Point(508, 410);
            this.updateHall3DModelButton.Anchor = AnchorStyles.Bottom;
            this.updateHall3DModelButton.Name = "updateHall3DModelButton";
            this.updateHall3DModelButton.Size = new System.Drawing.Size(120, 30);
            this.updateHall3DModelButton.TabIndex = 3;
            this.updateHall3DModelButton.Text = "Update 3D hall";
            this.updateHall3DModelButton.UseVisualStyleBackColor = true;
            this.updateHall3DModelButton.Click += new System.EventHandler(this.UpdateHall3DModelButton_OnClick);

            // "Export 3D hall data" button initialization
            this.exportHall3DModelDataButton.Location = new System.Drawing.Point(676, 410);
            this.exportHall3DModelDataButton.Anchor = AnchorStyles.Bottom;
            this.exportHall3DModelDataButton.Name = "exportHall3DModelDataButton";
            this.exportHall3DModelDataButton.Size = new System.Drawing.Size(160, 30);
            this.exportHall3DModelDataButton.TabIndex = 4;
            this.exportHall3DModelDataButton.Text = "Export 3D hall data";
            this.exportHall3DModelDataButton.UseVisualStyleBackColor = true;
            this.exportHall3DModelDataButton.Click += new System.EventHandler(this.ExportHall3DModelDataButton_OnClick);

            // "Highlight data discrepancies" checkbox initialization
            this.highlightDataDiscrepanciesCheckBox.Location = new System.Drawing.Point(836, 410);
            this.highlightDataDiscrepanciesCheckBox.Anchor = AnchorStyles.Bottom;
            this.highlightDataDiscrepanciesCheckBox.Name = "highlightDataDiscrepanciesCheckBox";
            this.highlightDataDiscrepanciesCheckBox.AutoSize = true;
            this.highlightDataDiscrepanciesCheckBox.TabIndex = 5;
            this.highlightDataDiscrepanciesCheckBox.Text = "Highlight data discrepancies";
            this.highlightDataDiscrepanciesCheckBox.Checked = true; // default: active
            this.highlightDataDiscrepanciesCheckBox.UseVisualStyleBackColor = true;
            this.highlightDataDiscrepanciesCheckBox.Click += new System.EventHandler(this.HighlightDataDiscrepanciesCheckBox_CheckChanged);

            // "Highlight data discrepancies" tooltip initialization
            UpdateHighlightDataDiscrepanciesCheckBoxState();

            // title for global RT values DataGridView initialization
            this.globalRtDataGridViewLabel.Text = "T30 [s] global value trend";
            this.globalRtDataGridViewLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.globalRtDataGridViewLabel.AutoSize = true;
            this.globalRtDataGridViewLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // global RT values DataGridView initialization and population with hard-coded data for each panel configuration and frequency band
            this.globalRtDataGridView.AllowUserToAddRows = false;
            this.globalRtDataGridView.AllowUserToDeleteRows = false;
            this.globalRtDataGridView.RowHeadersVisible = true;
            this.globalRtDataGridView.ReadOnly = true;
            this.globalRtDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            var globalRtDataTable = new DataTable();
            globalRtDataTable.Columns.Add("Configuration");
            globalRtDataTable.Columns.Add("125 Hz");
            globalRtDataTable.Columns.Add("250 Hz");
            globalRtDataTable.Columns.Add("500 Hz");
            globalRtDataTable.Columns.Add("1000 Hz");
            globalRtDataTable.Columns.Add("2000 Hz");
            globalRtDataTable.Columns.Add("4000 Hz");
            globalRtValues.Add("CLOSED", new[] {2.09, 2.02, 1.90, 1.95, 1.92, 1.69});
            globalRtValues.Add("0C", new[] {2.07, 2.00, 1.89, 1.94, 1.91, 1.69});
            globalRtValues.Add("1F", new[] {2.07, 2.00, 1.89, 1.92, 1.90, 1.68});
            globalRtValues.Add("1C", new[] {2.06, 1.99, 1.88, 1.91, 1.89, 1.67});
            globalRtValues.Add("1FC", new[] {2.06, 1.99, 1.88, 1.88, 1.88, 1.66});
            globalRtValues.Add("1P", new[] {2.06, 1.99, 1.88, 1.92, 1.90, 1.68 });
            globalRtValues.Add("1FCP", new[] {2.03, 1.96, 1.85, 1.86, 1.85, 1.65 });
            globalRtValues.Add("2F", new[] {2.06, 1.99, 1.88, 1.93, 1.91, 1.69});
            globalRtValues.Add("2C", new[] {2.05, 1.98, 1.87, 1.91, 1.89, 1.68});
            globalRtValues.Add("2FCP", new[] {2.02, 1.95, 1.84, 1.88, 1.86, 1.67});
            globalRtValues.Add("OPEN", new[] {1.96, 1.89, 1.78, 1.81, 1.81, 1.62});
            foreach (var pair in globalRtValues)
            {
                var row = globalRtDataTable.NewRow();
                row["Configuration"] = pair.Key;
                row["125 Hz"] = pair.Value[0];
                row["250 Hz"] = pair.Value[1];
                row["500 Hz"] = pair.Value[2];
                row["1000 Hz"] = pair.Value[3];
                row["2000 Hz"] = pair.Value[4];
                row["4000 Hz"] = pair.Value[5];
                globalRtDataTable.Rows.Add(row);
            }
            globalRtDataGridView.DataBindingComplete += (s, e) => AdaptDataGridViewSizes(globalRtDataGridView);
            globalRtDataGridView.DataSource = globalRtDataTable;
            this.globalRtDataGridView.Name = "globalRtDataGridView";
            this.globalRtDataGridView.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.GlobalRtDataGridView_RowPostPaint);

            // WPF 3D viewer initialization
            hall3DModelViewer.SelectedEntityChanged += Hall3DModelViewer_SelectedEntityChanged;

            // "Reposition" button initialization
            this.repositionButton.Name = "repositionButton";
            this.repositionButton.Size = new System.Drawing.Size(120, 40);
            this.repositionButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular);
            this.repositionButton.AutoSize = false;
            this.repositionButton.UseVisualStyleBackColor = true;
            this.repositionButton.Text = "Reposition";
            this.repositionButton.Click += (s, e) => hall3DModelViewer.ReloadModel();

            // "High performance mode" checkbox initialization
            this.highSpeedCheckBox.Name = "highSpeedCheckBox";
            this.highSpeedCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular);
            this.highSpeedCheckBox.AutoSize = true;
            this.highSpeedCheckBox.Checked = false; // default unchecked, assumed high-capability system
            this.highSpeedCheckBox.Text = "High performance mode";
            this.highSpeedCheckBox.CheckedChanged += new System.EventHandler(this.HighSpeedCheckBox_CheckedChanged);

            // "Options" menu item initialization
            this.optionsMenuItem.Name = "optionsMenuItem";
            this.optionsMenuItem.Text = "Options";

            // "Automatic discrepancy highlighting" menu entry initialization
            this.automaticDiscrepancyHighlightingMenuEntry.Name = "automaticDiscrepancyHighlightingMenuEntry";
            this.automaticDiscrepancyHighlightingMenuEntry.Text = "Automatic discrepancy highlighting";
            this.automaticDiscrepancyHighlightingMenuEntry.CheckOnClick = true;
            this.automaticDiscrepancyHighlightingMenuEntry.Checked = true; // default: active
            this.automaticDiscrepancyHighlightingMenuEntry.ToolTipText = "Automatically use the same color to highlight row couples referred to corresponding elements between real and 3D hall and containing different values between them when both have been loaded";
            this.automaticDiscrepancyHighlightingMenuEntry.CheckedChanged += new System.EventHandler(this.AutomaticDiscrepancyHighlightingMenuEntry_CheckedChanged);

            // "Configurations" menu entry initialization
            this.configurationsMenuEntry.Name = "configurationsMenuItem";
            this.configurationsMenuEntry.Text = "Configurations";
            this.configurationsMenuEntry.Click += new System.EventHandler(this.ConfigurationsMenuEntry_Click);

            // Form initialization
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.mainTabControl);
            this.Name = "Hallbridger";
            this.Text = "Hallbridger";
            this.ResumeLayout(false);
            this.PerformLayout();

            // menu bar initialization
            this.optionsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.automaticDiscrepancyHighlightingMenuEntry,
                this.configurationsMenuEntry
            });
            this.mainMenuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.optionsMenuItem
            });
            this.MainMenuStrip = this.mainMenuBar;

            // main TabControl and TabPages initialization
            this.mainTabControl.Controls.Add(this.movingElementsTab);
            this.mainTabControl.Controls.Add(this.acousticsTab);
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.SelectedIndexChanged += new System.EventHandler(this.TabControl_SelectedIndexChanged);
            this.movingElementsTab.Name = "movingElementsTab";
            this.movingElementsTab.Text = "Moving element data";
            this.movingElementsTab.MouseMove += MovingElementsTab_MouseMove;
            this.acousticsTab.Name = "acousticsTab";
            this.acousticsTab.Text = "Acoustic data";

            // add form elements to respective TabPages
            this.movingElementsTab.Controls.Add(this.realHallDataGroupLabel);
            this.movingElementsTab.Controls.Add(this.realHallStagecraftDataGridViewLabel);
            this.movingElementsTab.Controls.Add(this.realHallStagecraftDataGridView);
            this.movingElementsTab.Controls.Add(this.realHallLeftPanelsDataGridViewLabel);
            this.movingElementsTab.Controls.Add(this.realHallLeftPanelsDataGridView);
            this.movingElementsTab.Controls.Add(this.realHallRightPanelsDataGridViewLabel);
            this.movingElementsTab.Controls.Add(this.realHallRightPanelsDataGridView);
            this.movingElementsTab.Controls.Add(this.groupSeparator);
            this.movingElementsTab.Controls.Add(this.hall3DModelDataGroupLabel);
            this.movingElementsTab.Controls.Add(this.hall3DModelStagecraftDataGridViewLabel);
            this.movingElementsTab.Controls.Add(this.hall3DModelStagecraftDataGridView);
            this.movingElementsTab.Controls.Add(this.hall3DModelLeftPanelsDataGridViewLabel);
            this.movingElementsTab.Controls.Add(this.hall3DModelLeftPanelsDataGridView);
            this.movingElementsTab.Controls.Add(this.hall3DModelRightPanelsDataGridViewLabel);
            this.movingElementsTab.Controls.Add(this.hall3DModelRightPanelsDataGridView);
            this.movingElementsTab.Controls.Add(this.loadRealHallDataButton);
            this.movingElementsTab.Controls.Add(this.loadHall3DModelButton);
            this.movingElementsTab.Controls.Add(this.updateHall3DModelButton);
            this.movingElementsTab.Controls.Add(this.exportHall3DModelDataButton);
            this.movingElementsTab.Controls.Add(this.highlightDataDiscrepanciesCheckBox);
            this.acousticsTab.Controls.Add(this.globalRtDataGridViewLabel);
            this.acousticsTab.Controls.Add(this.globalRtDataGridView);
            this.acousticsTab.Controls.Add(this.hall3DModelViewerHost);
            this.acousticsTab.Controls.Add(this.repositionButton);
            this.acousticsTab.Controls.Add(this.highSpeedCheckBox);
            this.Controls.Add(this.mainMenuBar);
            this.mainMenuBar.ResumeLayout(false);
            this.mainMenuBar.PerformLayout();
        }
    }
}

