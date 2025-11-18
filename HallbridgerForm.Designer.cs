using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Hallbridger
{
    partial class HallbridgerForm
    {
        // variable declaration

        // moving element data and acoustic data tabs
        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage movingElementsTab;
        private System.Windows.Forms.TabPage acousticsTab;

        // hall data group title
        private System.Windows.Forms.Label hallDataGroupLabel;

        // hall DataGridView titles
        private System.Windows.Forms.Label hallStagecraftDataGridViewLabel;
        private System.Windows.Forms.Label hallLeftPanelsDataGridViewLabel;
        private System.Windows.Forms.Label hallRightPanelsDataGridViewLabel;

        // hall DataGridView for viewing stagecraft equipment positions and left/right panel array apertures coming from the hall
        private System.Windows.Forms.DataGridView hallStagecraftDataGridView;
        private System.Windows.Forms.DataGridView hallLeftPanelsDataGridView;
        private System.Windows.Forms.DataGridView hallRightPanelsDataGridView;

        // Horizontal separator between hall and IFC model data groups
        private System.Windows.Forms.Label groupSeparator;

        // IFC model data group title
        private System.Windows.Forms.Label ifcDataGroupLabel;

        // IFC model DataGridView titles
        private System.Windows.Forms.Label ifcStagecraftDataGridViewLabel;
        private System.Windows.Forms.Label ifcLeftPanelsDataGridViewLabel;
        private System.Windows.Forms.Label ifcRightPanelsDataGridViewLabel;

        // IFC model DataGridView for viewing stagecraft equipment positions and left/right panel array apertures coming from IFC model
        private System.Windows.Forms.DataGridView ifcStagecraftDataGridView;
        private System.Windows.Forms.DataGridView ifcLeftPanelsDataGridView;
        private System.Windows.Forms.DataGridView ifcRightPanelsDataGridView;

        // "Load hall file" button
        private System.Windows.Forms.Button loadHallButton;

        // "Load IFC file" button
        private System.Windows.Forms.Button loadIfcButton;

        // "Update IFC file" button
        private System.Windows.Forms.Button updateIfcButton;

        // global RT values DataGridView title
        private System.Windows.Forms.Label globalRtDataGridViewLabel;

        // global RT values DataGridView
        private System.Windows.Forms.DataGridView globalRtDataGridView;

        private void InitializeComponent()
        {
            // element creation

            // main TabControl and TabPages creation
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.movingElementsTab = new System.Windows.Forms.TabPage();
            this.acousticsTab = new System.Windows.Forms.TabPage();

            // hall data group title creation
            this.hallDataGroupLabel = new System.Windows.Forms.Label();

            // hall DataGridView titles creation
            this.hallStagecraftDataGridViewLabel = new System.Windows.Forms.Label();
            this.hallLeftPanelsDataGridViewLabel = new System.Windows.Forms.Label();
            this.hallRightPanelsDataGridViewLabel = new System.Windows.Forms.Label();

            // hall DataGridView creation
            this.hallStagecraftDataGridView = new System.Windows.Forms.DataGridView();
            this.hallLeftPanelsDataGridView = new System.Windows.Forms.DataGridView();
            this.hallRightPanelsDataGridView = new System.Windows.Forms.DataGridView();

            // Horizontal separator creation
            this.groupSeparator = new System.Windows.Forms.Label();

            // IFC model data group title creation
            this.ifcDataGroupLabel = new System.Windows.Forms.Label();

            // IFC model DataGridView titles creation
            this.ifcStagecraftDataGridViewLabel = new System.Windows.Forms.Label();
            this.ifcLeftPanelsDataGridViewLabel = new System.Windows.Forms.Label();
            this.ifcRightPanelsDataGridViewLabel = new System.Windows.Forms.Label();

            // IFC model DataGridView creation
            this.ifcStagecraftDataGridView = new System.Windows.Forms.DataGridView();
            this.ifcLeftPanelsDataGridView = new System.Windows.Forms.DataGridView();
            this.ifcRightPanelsDataGridView = new System.Windows.Forms.DataGridView();

            // "Load hall file" button creation
            this.loadHallButton = new System.Windows.Forms.Button();

            // "Load IFC file" button creation
            this.loadIfcButton = new System.Windows.Forms.Button();

            // "Update IFC file" button creation
            this.updateIfcButton = new System.Windows.Forms.Button();

            // global RT values DataGridView title creation
            this.globalRtDataGridViewLabel = new System.Windows.Forms.Label();

            // global RT values DataGridView creation
            this.globalRtDataGridView = new System.Windows.Forms.DataGridView();


            this.SuspendLayout();


            // element initialization

            // hall data group title initialization
            this.hallDataGroupLabel.Size = new System.Drawing.Size(300, 30);
            this.hallDataGroupLabel.Location = new System.Drawing.Point((this.ClientSize.Width - 300) / 2, 16);
            this.hallDataGroupLabel.Text = "Hall data";
            this.hallDataGroupLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.hallDataGroupLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.hallDataGroupLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;

            // title for hall stagecraft equipment DataGridView initialization
            this.hallStagecraftDataGridViewLabel.Location = new System.Drawing.Point(12, 56);
            this.hallStagecraftDataGridViewLabel.Size = new System.Drawing.Size(240, 20);
            this.hallStagecraftDataGridViewLabel.Text = "Stagecraft equipment positions";
            this.hallStagecraftDataGridViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // title for hall left panel array DataGridView initialization
            this.hallLeftPanelsDataGridViewLabel.Location = new System.Drawing.Point(270, 56);
            this.hallLeftPanelsDataGridViewLabel.Size = new System.Drawing.Size(240, 20);
            this.hallLeftPanelsDataGridViewLabel.Text = "Left panel array apertures";
            this.hallLeftPanelsDataGridViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // title for hall right panel array DataGridView initialization
            this.hallRightPanelsDataGridViewLabel.Location = new System.Drawing.Point(528, 56);
            this.hallRightPanelsDataGridViewLabel.Size = new System.Drawing.Size(240, 20);
            this.hallRightPanelsDataGridViewLabel.Text = "Right panel array apertures";
            this.hallRightPanelsDataGridViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // hall DataGridView for stagecraft equipment initialization
            this.hallStagecraftDataGridView.Location = new System.Drawing.Point(12, 76);
            this.hallStagecraftDataGridView.Size = new System.Drawing.Size(240, 120);
            this.hallStagecraftDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.hallStagecraftDataGridView.AllowUserToAddRows = false;
            this.hallStagecraftDataGridView.ColumnCount = 2;
            this.hallStagecraftDataGridView.Columns[0].Name = "Name";
            this.hallStagecraftDataGridView.Columns[0].Width = this.hallStagecraftDataGridView.Width / 2;
            this.hallStagecraftDataGridView.Columns[1].Name = "Value";
            this.hallStagecraftDataGridView.Columns[1].Width = this.hallStagecraftDataGridView.Width / 2;
            this.hallStagecraftDataGridView.Name = "hallStagecraftDataGridView";
            this.hallStagecraftDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_ColumnHeaderMouseDown);
            this.hallStagecraftDataGridView.Sorted += new System.EventHandler(this.DataGridView_Sorted);

            // hall DataGridView for left panel arrays initialization
            this.hallLeftPanelsDataGridView.Location = new System.Drawing.Point(270, 76);
            this.hallLeftPanelsDataGridView.Size = new System.Drawing.Size(240, 120);
            this.hallLeftPanelsDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.hallLeftPanelsDataGridView.AllowUserToAddRows = false;
            this.hallLeftPanelsDataGridView.ColumnCount = 2;
            this.hallLeftPanelsDataGridView.Columns[0].Name = "Name";
            this.hallLeftPanelsDataGridView.Columns[0].Width = this.hallLeftPanelsDataGridView.Width / 2;
            this.hallLeftPanelsDataGridView.Columns[1].Name = "Value";
            this.hallLeftPanelsDataGridView.Columns[1].Width = this.hallLeftPanelsDataGridView.Width / 2;
            this.hallLeftPanelsDataGridView.Name = "hallLeftPanelsDataGridView";
            this.hallLeftPanelsDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_ColumnHeaderMouseDown);
            this.hallLeftPanelsDataGridView.Sorted += new System.EventHandler(this.DataGridView_Sorted);

            // hall DataGridView for right panel arrays initialization
            this.hallRightPanelsDataGridView.Location = new System.Drawing.Point(528, 76);
            this.hallRightPanelsDataGridView.Size = new System.Drawing.Size(240, 120);
            this.hallRightPanelsDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.hallRightPanelsDataGridView.AllowUserToAddRows = false;
            this.hallRightPanelsDataGridView.ColumnCount = 2;
            this.hallRightPanelsDataGridView.Columns[0].Name = "Name";
            this.hallRightPanelsDataGridView.Columns[0].Width = this.hallRightPanelsDataGridView.Width / 2;
            this.hallRightPanelsDataGridView.Columns[1].Name = "Value";
            this.hallRightPanelsDataGridView.Columns[1].Width = this.hallRightPanelsDataGridView.Width / 2;
            this.hallRightPanelsDataGridView.Name = "hallRightPanelsDataGridView";
            this.hallRightPanelsDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_ColumnHeaderMouseDown);
            this.hallRightPanelsDataGridView.Sorted += new System.EventHandler(this.DataGridView_Sorted);

            // Horizontal separator initialization
            this.groupSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.groupSeparator.Location = new System.Drawing.Point(12, 206);
            this.groupSeparator.Size = new System.Drawing.Size(776, 2);
            this.groupSeparator.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // IFC model data group title initialization
            this.ifcDataGroupLabel.Size = new System.Drawing.Size(300, 30);
            this.ifcDataGroupLabel.Location = new System.Drawing.Point((this.ClientSize.Width - 300) / 2, 216);
            this.ifcDataGroupLabel.Text = "IFC model data";
            this.ifcDataGroupLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.ifcDataGroupLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.ifcDataGroupLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;

            // title for IFC model stagecraft equipment DataGridView initialization
            this.ifcStagecraftDataGridViewLabel.Location = new System.Drawing.Point(12, 256);
            this.ifcStagecraftDataGridViewLabel.Size = new System.Drawing.Size(240, 20);
            this.ifcStagecraftDataGridViewLabel.Text = "Stagecraft equipment positions";
            this.ifcStagecraftDataGridViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // title for IFC model left panel array DataGridView initialization
            this.ifcLeftPanelsDataGridViewLabel.Location = new System.Drawing.Point(270, 256);
            this.ifcLeftPanelsDataGridViewLabel.Size = new System.Drawing.Size(240, 20);
            this.ifcLeftPanelsDataGridViewLabel.Text = "Left panel array apertures";
            this.ifcLeftPanelsDataGridViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // title for IFC model right panel array DataGridView initialization
            this.ifcRightPanelsDataGridViewLabel.Location = new System.Drawing.Point(528, 256);
            this.ifcRightPanelsDataGridViewLabel.Size = new System.Drawing.Size(240, 20);
            this.ifcRightPanelsDataGridViewLabel.Text = "Right panel array apertures";
            this.ifcRightPanelsDataGridViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // IFC model DataGridView for stagecraft equipment initialization
            this.ifcStagecraftDataGridView.Location = new System.Drawing.Point(12, 276);
            this.ifcStagecraftDataGridView.Size = new System.Drawing.Size(240, 120);
            this.ifcStagecraftDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.ifcStagecraftDataGridView.AllowUserToAddRows = false;
            this.ifcStagecraftDataGridView.ColumnCount = 2;
            this.ifcStagecraftDataGridView.Columns[0].Name = "Name";
            this.ifcStagecraftDataGridView.Columns[0].Width = this.ifcStagecraftDataGridView.Width / 2;
            this.ifcStagecraftDataGridView.Columns[1].Name = "Value";
            this.ifcStagecraftDataGridView.Columns[1].Width = this.ifcStagecraftDataGridView.Width / 2;
            this.ifcStagecraftDataGridView.Name = "ifcStagecraftDataGridView";
            this.ifcStagecraftDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_ColumnHeaderMouseDown);
            this.ifcStagecraftDataGridView.Sorted += new System.EventHandler(this.DataGridView_Sorted);

            // IFC model DataGridView for left panel arrays initialization
            this.ifcLeftPanelsDataGridView.Location = new System.Drawing.Point(270, 276);
            this.ifcLeftPanelsDataGridView.Size = new System.Drawing.Size(240, 120);
            this.ifcLeftPanelsDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.ifcLeftPanelsDataGridView.AllowUserToAddRows = false;
            this.ifcLeftPanelsDataGridView.ColumnCount = 2;
            this.ifcLeftPanelsDataGridView.Columns[0].Name = "Name";
            this.ifcLeftPanelsDataGridView.Columns[0].Width = this.ifcLeftPanelsDataGridView.Width / 2;
            this.ifcLeftPanelsDataGridView.Columns[1].Name = "Value";
            this.ifcLeftPanelsDataGridView.Columns[1].Width = this.ifcLeftPanelsDataGridView.Width / 2;
            this.ifcLeftPanelsDataGridView.Name = "ifcLeftPanelDataGridView";
            this.ifcLeftPanelsDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_ColumnHeaderMouseDown);
            this.ifcLeftPanelsDataGridView.Sorted += new System.EventHandler(this.DataGridView_Sorted);

            // IFC model DataGridView for right panel arrays initialization
            this.ifcRightPanelsDataGridView.Location = new System.Drawing.Point(528, 276);
            this.ifcRightPanelsDataGridView.Size = new System.Drawing.Size(240, 120);
            this.ifcRightPanelsDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.ifcRightPanelsDataGridView.AllowUserToAddRows = false;
            this.ifcRightPanelsDataGridView.ColumnCount = 2;
            this.ifcRightPanelsDataGridView.Columns[0].Name = "Name";
            this.ifcRightPanelsDataGridView.Columns[0].Width = this.ifcRightPanelsDataGridView.Width / 2;
            this.ifcRightPanelsDataGridView.Columns[1].Name = "Value";
            this.ifcRightPanelsDataGridView.Columns[1].Width = this.ifcRightPanelsDataGridView.Width / 2;
            this.ifcRightPanelsDataGridView.Name = "ifcRightPanelDataGridView";
            this.ifcRightPanelsDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_ColumnHeaderMouseDown);
            this.ifcRightPanelsDataGridView.Sorted += new System.EventHandler(this.DataGridView_Sorted);

            // "Load hall file" button initialization
            this.loadHallButton.Location = new System.Drawing.Point(172, 410);
            this.loadHallButton.Anchor = AnchorStyles.Bottom;
            this.loadHallButton.Name = "loadHallButton";
            this.loadHallButton.Size = new System.Drawing.Size(120, 30);
            this.loadHallButton.TabIndex = 1;
            this.loadHallButton.Text = "Load hall file";
            this.loadHallButton.UseVisualStyleBackColor = true;
            this.loadHallButton.Click += new System.EventHandler(this.LoadHallButton_OnClick);

            // "Load IFC file" button initialization
            this.loadIfcButton.Location = new System.Drawing.Point(340, 410);
            this.loadIfcButton.Anchor = AnchorStyles.Bottom;
            this.loadIfcButton.Name = "loadIfcButton";
            this.loadIfcButton.Size = new System.Drawing.Size(120, 30);
            this.loadIfcButton.TabIndex = 2;
            this.loadIfcButton.Text = "Load IFC file";
            this.loadIfcButton.UseVisualStyleBackColor = true;
            this.loadIfcButton.Click += new System.EventHandler(this.LoadIfcButton_OnClick);

            // "Update IFC file" button initialization
            this.updateIfcButton.Location = new System.Drawing.Point(508, 410);
            this.updateIfcButton.Anchor = AnchorStyles.Bottom;
            this.updateIfcButton.Name = "updateIfcButton";
            this.updateIfcButton.Size = new System.Drawing.Size(120, 30);
            this.updateIfcButton.TabIndex = 3;
            this.updateIfcButton.Text = "Update IFC file";
            this.updateIfcButton.UseVisualStyleBackColor = true;
            this.updateIfcButton.Click += new System.EventHandler(this.UpdateIfcButton_OnClick);

            // title for global RT values DataGridView initialization
            this.globalRtDataGridViewLabel.Text = "T30 [s] global value trend";
            this.globalRtDataGridViewLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.globalRtDataGridViewLabel.AutoSize = true;
            this.globalRtDataGridViewLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // global RT values DataGridView initialization
            this.globalRtDataGridView.AllowUserToAddRows = false;
            this.globalRtDataGridView.AllowUserToDeleteRows = false;
            this.globalRtDataGridView.RowHeadersVisible = true;
            this.globalRtDataGridView.ReadOnly = true;
            this.globalRtDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.globalRtDataGridView.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.GlobalRtDataGridView_RowPostPaint);


            // Form initialization
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.mainTabControl);
            this.Name = "Hallbridger";
            this.Text = "Hallbridger";
            this.ResumeLayout(false);
            this.PerformLayout();

            // main TabControl and TabPages initialization
            this.mainTabControl.Controls.Add(this.movingElementsTab);
            this.mainTabControl.Controls.Add(this.acousticsTab);
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.SelectedIndexChanged += new System.EventHandler(this.TabControl_SelectedIndexChanged);
            this.movingElementsTab.Text = "Moving element data";
            this.acousticsTab.Text = "Acoustic data";

            // add form elements to respective TabPages
            this.movingElementsTab.Controls.Add(this.hallDataGroupLabel);
            this.movingElementsTab.Controls.Add(this.hallStagecraftDataGridViewLabel);
            this.movingElementsTab.Controls.Add(this.hallStagecraftDataGridView);
            this.movingElementsTab.Controls.Add(this.hallLeftPanelsDataGridViewLabel);
            this.movingElementsTab.Controls.Add(this.hallLeftPanelsDataGridView);
            this.movingElementsTab.Controls.Add(this.hallRightPanelsDataGridViewLabel);
            this.movingElementsTab.Controls.Add(this.hallRightPanelsDataGridView);
            this.movingElementsTab.Controls.Add(this.groupSeparator);
            this.movingElementsTab.Controls.Add(this.ifcDataGroupLabel);
            this.movingElementsTab.Controls.Add(this.ifcStagecraftDataGridViewLabel);
            this.movingElementsTab.Controls.Add(this.ifcStagecraftDataGridView);
            this.movingElementsTab.Controls.Add(this.ifcLeftPanelsDataGridViewLabel);
            this.movingElementsTab.Controls.Add(this.ifcLeftPanelsDataGridView);
            this.movingElementsTab.Controls.Add(this.ifcRightPanelsDataGridViewLabel);
            this.movingElementsTab.Controls.Add(this.ifcRightPanelsDataGridView);
            this.movingElementsTab.Controls.Add(this.loadHallButton);
            this.movingElementsTab.Controls.Add(this.loadIfcButton);
            this.movingElementsTab.Controls.Add(this.updateIfcButton);
            this.acousticsTab.Controls.Add(this.globalRtDataGridViewLabel);
            this.acousticsTab.Controls.Add(this.globalRtDataGridView);
        }
    }
}

