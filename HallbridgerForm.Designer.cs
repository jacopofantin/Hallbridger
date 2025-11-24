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

        // Horizontal separator between hall and 3D model data groups
        private System.Windows.Forms.Label groupSeparator;

        // 3D model data group title
        private System.Windows.Forms.Label hall3dModelDataGroupLabel;

        // 3D model DataGridView titles
        private System.Windows.Forms.Label hall3dModelStagecraftDataGridViewLabel;
        private System.Windows.Forms.Label hall3dModelLeftPanelsDataGridViewLabel;
        private System.Windows.Forms.Label hall3dModelRightPanelsDataGridViewLabel;

        // 3D model DataGridView for viewing stagecraft equipment positions and left/right panel array apertures coming from 3D model
        private System.Windows.Forms.DataGridView hall3dModelStagecraftDataGridView;
        private System.Windows.Forms.DataGridView hall3dModelLeftPanelsDataGridView;
        private System.Windows.Forms.DataGridView hall3dModelRightPanelsDataGridView;

        // "Load hall data" button
        private System.Windows.Forms.Button loadHallDataButton;

        // "Load 3D model" button
        private System.Windows.Forms.Button loadHall3dModelButton;

        // "Update 3D model" button
        private System.Windows.Forms.Button updateHall3dModelButton;

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

            // 3D model data group title creation
            this.hall3dModelDataGroupLabel = new System.Windows.Forms.Label();

            // 3D model DataGridView titles creation
            this.hall3dModelStagecraftDataGridViewLabel = new System.Windows.Forms.Label();
            this.hall3dModelLeftPanelsDataGridViewLabel = new System.Windows.Forms.Label();
            this.hall3dModelRightPanelsDataGridViewLabel = new System.Windows.Forms.Label();

            // 3D model DataGridView creation
            this.hall3dModelStagecraftDataGridView = new System.Windows.Forms.DataGridView();
            this.hall3dModelLeftPanelsDataGridView = new System.Windows.Forms.DataGridView();
            this.hall3dModelRightPanelsDataGridView = new System.Windows.Forms.DataGridView();

            // "Load hall data" button creation
            this.loadHallDataButton = new System.Windows.Forms.Button();

            // "Load 3D model" button creation
            this.loadHall3dModelButton = new System.Windows.Forms.Button();

            // "Update 3D model" button creation
            this.updateHall3dModelButton = new System.Windows.Forms.Button();

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

            // 3D model data group title initialization
            this.hall3dModelDataGroupLabel.Size = new System.Drawing.Size(300, 30);
            this.hall3dModelDataGroupLabel.Location = new System.Drawing.Point((this.ClientSize.Width - 300) / 2, 216);
            this.hall3dModelDataGroupLabel.Text = "3D model data";
            this.hall3dModelDataGroupLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.hall3dModelDataGroupLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.hall3dModelDataGroupLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;

            // title for 3D model stagecraft equipment DataGridView initialization
            this.hall3dModelStagecraftDataGridViewLabel.Location = new System.Drawing.Point(12, 256);
            this.hall3dModelStagecraftDataGridViewLabel.Size = new System.Drawing.Size(240, 20);
            this.hall3dModelStagecraftDataGridViewLabel.Text = "Stagecraft equipment positions";
            this.hall3dModelStagecraftDataGridViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // title for 3D model left panel array DataGridView initialization
            this.hall3dModelLeftPanelsDataGridViewLabel.Location = new System.Drawing.Point(270, 256);
            this.hall3dModelLeftPanelsDataGridViewLabel.Size = new System.Drawing.Size(240, 20);
            this.hall3dModelLeftPanelsDataGridViewLabel.Text = "Left panel array apertures";
            this.hall3dModelLeftPanelsDataGridViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // title for 3D model right panel array DataGridView initialization
            this.hall3dModelRightPanelsDataGridViewLabel.Location = new System.Drawing.Point(528, 256);
            this.hall3dModelRightPanelsDataGridViewLabel.Size = new System.Drawing.Size(240, 20);
            this.hall3dModelRightPanelsDataGridViewLabel.Text = "Right panel array apertures";
            this.hall3dModelRightPanelsDataGridViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // 3D model DataGridView for stagecraft equipment initialization
            this.hall3dModelStagecraftDataGridView.Location = new System.Drawing.Point(12, 276);
            this.hall3dModelStagecraftDataGridView.Size = new System.Drawing.Size(240, 120);
            this.hall3dModelStagecraftDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.hall3dModelStagecraftDataGridView.AllowUserToAddRows = false;
            this.hall3dModelStagecraftDataGridView.ColumnCount = 2;
            this.hall3dModelStagecraftDataGridView.Columns[0].Name = "Name";
            this.hall3dModelStagecraftDataGridView.Columns[0].Width = this.hall3dModelStagecraftDataGridView.Width / 2;
            this.hall3dModelStagecraftDataGridView.Columns[1].Name = "Value";
            this.hall3dModelStagecraftDataGridView.Columns[1].Width = this.hall3dModelStagecraftDataGridView.Width / 2;
            this.hall3dModelStagecraftDataGridView.Name = "hall3dModelStagecraftDataGridView";
            this.hall3dModelStagecraftDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_ColumnHeaderMouseDown);
            this.hall3dModelStagecraftDataGridView.Sorted += new System.EventHandler(this.DataGridView_Sorted);

            // 3D model DataGridView for left panel arrays initialization
            this.hall3dModelLeftPanelsDataGridView.Location = new System.Drawing.Point(270, 276);
            this.hall3dModelLeftPanelsDataGridView.Size = new System.Drawing.Size(240, 120);
            this.hall3dModelLeftPanelsDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.hall3dModelLeftPanelsDataGridView.AllowUserToAddRows = false;
            this.hall3dModelLeftPanelsDataGridView.ColumnCount = 2;
            this.hall3dModelLeftPanelsDataGridView.Columns[0].Name = "Name";
            this.hall3dModelLeftPanelsDataGridView.Columns[0].Width = this.hall3dModelLeftPanelsDataGridView.Width / 2;
            this.hall3dModelLeftPanelsDataGridView.Columns[1].Name = "Value";
            this.hall3dModelLeftPanelsDataGridView.Columns[1].Width = this.hall3dModelLeftPanelsDataGridView.Width / 2;
            this.hall3dModelLeftPanelsDataGridView.Name = "hall3dModelLeftPanelDataGridView";
            this.hall3dModelLeftPanelsDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_ColumnHeaderMouseDown);
            this.hall3dModelLeftPanelsDataGridView.Sorted += new System.EventHandler(this.DataGridView_Sorted);

            // 3D model DataGridView for right panel arrays initialization
            this.hall3dModelRightPanelsDataGridView.Location = new System.Drawing.Point(528, 276);
            this.hall3dModelRightPanelsDataGridView.Size = new System.Drawing.Size(240, 120);
            this.hall3dModelRightPanelsDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.hall3dModelRightPanelsDataGridView.AllowUserToAddRows = false;
            this.hall3dModelRightPanelsDataGridView.ColumnCount = 2;
            this.hall3dModelRightPanelsDataGridView.Columns[0].Name = "Name";
            this.hall3dModelRightPanelsDataGridView.Columns[0].Width = this.hall3dModelRightPanelsDataGridView.Width / 2;
            this.hall3dModelRightPanelsDataGridView.Columns[1].Name = "Value";
            this.hall3dModelRightPanelsDataGridView.Columns[1].Width = this.hall3dModelRightPanelsDataGridView.Width / 2;
            this.hall3dModelRightPanelsDataGridView.Name = "hall3dModelRightPanelDataGridView";
            this.hall3dModelRightPanelsDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_ColumnHeaderMouseDown);
            this.hall3dModelRightPanelsDataGridView.Sorted += new System.EventHandler(this.DataGridView_Sorted);

            // "Load hall data" button initialization
            this.loadHallDataButton.Location = new System.Drawing.Point(172, 410);
            this.loadHallDataButton.Anchor = AnchorStyles.Bottom;
            this.loadHallDataButton.Name = "loadHallDataButton";
            this.loadHallDataButton.Size = new System.Drawing.Size(120, 30);
            this.loadHallDataButton.TabIndex = 1;
            this.loadHallDataButton.Text = "Load hall data";
            this.loadHallDataButton.UseVisualStyleBackColor = true;
            this.loadHallDataButton.Click += new System.EventHandler(this.LoadHallDataButton_OnClick);

            // "Load 3D model" button initialization
            this.loadHall3dModelButton.Location = new System.Drawing.Point(340, 410);
            this.loadHall3dModelButton.Anchor = AnchorStyles.Bottom;
            this.loadHall3dModelButton.Name = "loadHall3dModelButton";
            this.loadHall3dModelButton.Size = new System.Drawing.Size(120, 30);
            this.loadHall3dModelButton.TabIndex = 2;
            this.loadHall3dModelButton.Text = "Load 3D model";
            this.loadHall3dModelButton.UseVisualStyleBackColor = true;
            this.loadHall3dModelButton.Click += new System.EventHandler(this.LoadHall3dModelButton_OnClick);

            // "Update 3D model" button initialization
            this.updateHall3dModelButton.Location = new System.Drawing.Point(508, 410);
            this.updateHall3dModelButton.Anchor = AnchorStyles.Bottom;
            this.updateHall3dModelButton.Name = "updateHall3dModelButton";
            this.updateHall3dModelButton.Size = new System.Drawing.Size(120, 30);
            this.updateHall3dModelButton.TabIndex = 3;
            this.updateHall3dModelButton.Text = "Update 3D model";
            this.updateHall3dModelButton.UseVisualStyleBackColor = true;
            this.updateHall3dModelButton.Click += new System.EventHandler(this.UpdateHall3dModelButton_OnClick);

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
            this.movingElementsTab.Controls.Add(this.hall3dModelDataGroupLabel);
            this.movingElementsTab.Controls.Add(this.hall3dModelStagecraftDataGridViewLabel);
            this.movingElementsTab.Controls.Add(this.hall3dModelStagecraftDataGridView);
            this.movingElementsTab.Controls.Add(this.hall3dModelLeftPanelsDataGridViewLabel);
            this.movingElementsTab.Controls.Add(this.hall3dModelLeftPanelsDataGridView);
            this.movingElementsTab.Controls.Add(this.hall3dModelRightPanelsDataGridViewLabel);
            this.movingElementsTab.Controls.Add(this.hall3dModelRightPanelsDataGridView);
            this.movingElementsTab.Controls.Add(this.loadHallDataButton);
            this.movingElementsTab.Controls.Add(this.loadHall3dModelButton);
            this.movingElementsTab.Controls.Add(this.updateHall3dModelButton);
            this.acousticsTab.Controls.Add(this.globalRtDataGridViewLabel);
            this.acousticsTab.Controls.Add(this.globalRtDataGridView);
        }
    }
}

