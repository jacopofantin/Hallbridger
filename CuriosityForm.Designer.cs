using System.Windows.Forms;

namespace CURIOsity
{
    partial class CuriosityForm
    {
        // variable declaration

        // moving element data and acoustic data tabs
        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage movingElementDataTab;
        private System.Windows.Forms.TabPage acousticDataTab;

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

        // Horizontal separator between hall and BIM model data groups
        private System.Windows.Forms.Label groupSeparator;

        // BIM model data group title
        private System.Windows.Forms.Label bimDataGroupLabel;

        // BIM model DataGridView titles
        private System.Windows.Forms.Label bimStagecraftDataGridViewLabel;
        private System.Windows.Forms.Label bimLeftPanelsDataGridViewLabel;
        private System.Windows.Forms.Label bimRightPanelsDataGridViewLabel;

        // BIM model DataGridView for viewing stagecraft equipment positions and left/right panel array apertures coming from BIM model
        private System.Windows.Forms.DataGridView bimStagecraftDataGridView;
        private System.Windows.Forms.DataGridView bimLeftPanelsDataGridView;
        private System.Windows.Forms.DataGridView bimRightPanelsDataGridView;

        // "Load hall file" button
        private System.Windows.Forms.Button loadHallButton;

        // "Load BIM file" button
        private System.Windows.Forms.Button loadBimButton;

        // "Update BIM file" button
        private System.Windows.Forms.Button updateBimButton;


        private void InitializeComponent()
        {
            // element creation

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

            // BIM model data group title creation
            this.bimDataGroupLabel = new System.Windows.Forms.Label();

            // BIM model DataGridView titles creation
            this.bimStagecraftDataGridViewLabel = new System.Windows.Forms.Label();
            this.bimLeftPanelsDataGridViewLabel = new System.Windows.Forms.Label();
            this.bimRightPanelsDataGridViewLabel = new System.Windows.Forms.Label();

            // BIM model DataGridView creation
            this.bimStagecraftDataGridView = new System.Windows.Forms.DataGridView();
            this.bimLeftPanelsDataGridView = new System.Windows.Forms.DataGridView();
            this.bimRightPanelsDataGridView = new System.Windows.Forms.DataGridView();

            // "Load hall file" button creation
            this.loadHallButton = new System.Windows.Forms.Button();

            // "Load BIM file" button creation
            this.loadBimButton = new System.Windows.Forms.Button();

            // "Update BIM file" button creation
            this.updateBimButton = new System.Windows.Forms.Button();

            // main TabControl and TabPages creation
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.movingElementDataTab = new System.Windows.Forms.TabPage();
            this.acousticDataTab = new System.Windows.Forms.TabPage();

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
            this.hallLeftPanelsDataGridView.Name = "hallLeftPanelDataGridView";
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

            // BIM model data group title initialization
            this.bimDataGroupLabel.Size = new System.Drawing.Size(300, 30);
            this.bimDataGroupLabel.Location = new System.Drawing.Point((this.ClientSize.Width - 300) / 2, 216);
            this.bimDataGroupLabel.Text = "BIM model data";
            this.bimDataGroupLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.bimDataGroupLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.bimDataGroupLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;

            // title for BIM model stagecraft equipment DataGridView initialization
            this.bimStagecraftDataGridViewLabel.Location = new System.Drawing.Point(12, 256);
            this.bimStagecraftDataGridViewLabel.Size = new System.Drawing.Size(240, 20);
            this.bimStagecraftDataGridViewLabel.Text = "Stagecraft equipment positions";
            this.bimStagecraftDataGridViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // title for BIM model left panel array DataGridView initialization
            this.bimLeftPanelsDataGridViewLabel.Location = new System.Drawing.Point(270, 256);
            this.bimLeftPanelsDataGridViewLabel.Size = new System.Drawing.Size(240, 20);
            this.bimLeftPanelsDataGridViewLabel.Text = "Left panel array apertures";
            this.bimLeftPanelsDataGridViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // title for BIM model right panel array DataGridView initialization
            this.bimRightPanelsDataGridViewLabel.Location = new System.Drawing.Point(528, 256);
            this.bimRightPanelsDataGridViewLabel.Size = new System.Drawing.Size(240, 20);
            this.bimRightPanelsDataGridViewLabel.Text = "Right panel array apertures";
            this.bimRightPanelsDataGridViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // BIM model DataGridView for stagecraft equipment initialization
            this.bimStagecraftDataGridView.Location = new System.Drawing.Point(12, 276);
            this.bimStagecraftDataGridView.Size = new System.Drawing.Size(240, 120);
            this.bimStagecraftDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.bimStagecraftDataGridView.AllowUserToAddRows = false;
            this.bimStagecraftDataGridView.ColumnCount = 2;
            this.bimStagecraftDataGridView.Columns[0].Name = "Name";
            this.bimStagecraftDataGridView.Columns[0].Width = this.bimStagecraftDataGridView.Width / 2;
            this.bimStagecraftDataGridView.Columns[1].Name = "Value";
            this.bimStagecraftDataGridView.Columns[1].Width = this.bimStagecraftDataGridView.Width / 2;
            this.bimStagecraftDataGridView.Name = "bimStagecraftDataGridView";
            this.bimStagecraftDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_ColumnHeaderMouseDown);
            this.bimStagecraftDataGridView.Sorted += new System.EventHandler(this.DataGridView_Sorted);

            // BIM model DataGridView for left panel arrays initialization
            this.bimLeftPanelsDataGridView.Location = new System.Drawing.Point(270, 276);
            this.bimLeftPanelsDataGridView.Size = new System.Drawing.Size(240, 120);
            this.bimLeftPanelsDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.bimLeftPanelsDataGridView.AllowUserToAddRows = false;
            this.bimLeftPanelsDataGridView.ColumnCount = 2;
            this.bimLeftPanelsDataGridView.Columns[0].Name = "Name";
            this.bimLeftPanelsDataGridView.Columns[0].Width = this.bimLeftPanelsDataGridView.Width / 2;
            this.bimLeftPanelsDataGridView.Columns[1].Name = "Value";
            this.bimLeftPanelsDataGridView.Columns[1].Width = this.bimLeftPanelsDataGridView.Width / 2;
            this.bimLeftPanelsDataGridView.Name = "bimLeftPanelDataGridView";
            this.bimLeftPanelsDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_ColumnHeaderMouseDown);
            this.bimLeftPanelsDataGridView.Sorted += new System.EventHandler(this.DataGridView_Sorted);

            // BIM model DataGridView for right panel arrays initialization
            this.bimRightPanelsDataGridView.Location = new System.Drawing.Point(528, 276);
            this.bimRightPanelsDataGridView.Size = new System.Drawing.Size(240, 120);
            this.bimRightPanelsDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.bimRightPanelsDataGridView.AllowUserToAddRows = false;
            this.bimRightPanelsDataGridView.ColumnCount = 2;
            this.bimRightPanelsDataGridView.Columns[0].Name = "Name";
            this.bimRightPanelsDataGridView.Columns[0].Width = this.bimRightPanelsDataGridView.Width / 2;
            this.bimRightPanelsDataGridView.Columns[1].Name = "Value";
            this.bimRightPanelsDataGridView.Columns[1].Width = this.bimRightPanelsDataGridView.Width / 2;
            this.bimRightPanelsDataGridView.Name = "bimRightPanelDataGridView";
            this.bimRightPanelsDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_ColumnHeaderMouseDown);
            this.bimRightPanelsDataGridView.Sorted += new System.EventHandler(this.DataGridView_Sorted);

            // "Load hall file" button initialization
            this.loadHallButton.Location = new System.Drawing.Point(172, 410);
            this.loadHallButton.Anchor = AnchorStyles.Bottom;
            this.loadHallButton.Name = "loadHallButton";
            this.loadHallButton.Size = new System.Drawing.Size(120, 30);
            this.loadHallButton.TabIndex = 1;
            this.loadHallButton.Text = "Load hall file";
            this.loadHallButton.UseVisualStyleBackColor = true;
            this.loadHallButton.Click += new System.EventHandler(this.LoadHallButton_OnClick);

            // "Load BIM file" button initialization
            this.loadBimButton.Location = new System.Drawing.Point(340, 410);
            this.loadBimButton.Anchor = AnchorStyles.Bottom;
            this.loadBimButton.Name = "loadBimButton";
            this.loadBimButton.Size = new System.Drawing.Size(120, 30);
            this.loadBimButton.TabIndex = 2;
            this.loadBimButton.Text = "Load BIM file";
            this.loadBimButton.UseVisualStyleBackColor = true;
            this.loadBimButton.Click += new System.EventHandler(this.LoadBimButton_OnClick);

            // "Update BIM file" button initialization
            this.updateBimButton.Location = new System.Drawing.Point(508, 410);
            this.updateBimButton.Anchor = AnchorStyles.Bottom;
            this.updateBimButton.Name = "updateBimButton";
            this.updateBimButton.Size = new System.Drawing.Size(120, 30);
            this.updateBimButton.TabIndex = 3;
            this.updateBimButton.Text = "Update BIM file";
            this.updateBimButton.UseVisualStyleBackColor = true;
            this.updateBimButton.Click += new System.EventHandler(this.UpdateBimButton_OnClick);

            // main TabControl and TabPages initialization
            this.mainTabControl.Controls.Add(this.movingElementDataTab);
            this.mainTabControl.Controls.Add(this.acousticDataTab);
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.movingElementDataTab.Text = "Moving element data";
            this.acousticDataTab.Text = "Acoustic data";

            // Form initialization
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.mainTabControl);
            this.Name = "CURIOsity";
            this.Text = "CURIOsity";
            this.ResumeLayout(false);
            this.PerformLayout();

            // add form elements to respective TabPages
            this.movingElementDataTab.Controls.Add(this.hallDataGroupLabel);
            this.movingElementDataTab.Controls.Add(this.hallStagecraftDataGridViewLabel);
            this.movingElementDataTab.Controls.Add(this.hallStagecraftDataGridView);
            this.movingElementDataTab.Controls.Add(this.hallLeftPanelsDataGridViewLabel);
            this.movingElementDataTab.Controls.Add(this.hallLeftPanelsDataGridView);
            this.movingElementDataTab.Controls.Add(this.hallRightPanelsDataGridViewLabel);
            this.movingElementDataTab.Controls.Add(this.hallRightPanelsDataGridView);
            this.movingElementDataTab.Controls.Add(this.groupSeparator);
            this.movingElementDataTab.Controls.Add(this.bimDataGroupLabel);
            this.movingElementDataTab.Controls.Add(this.bimStagecraftDataGridViewLabel);
            this.movingElementDataTab.Controls.Add(this.bimStagecraftDataGridView);
            this.movingElementDataTab.Controls.Add(this.bimLeftPanelsDataGridViewLabel);
            this.movingElementDataTab.Controls.Add(this.bimLeftPanelsDataGridView);
            this.movingElementDataTab.Controls.Add(this.bimRightPanelsDataGridViewLabel);
            this.movingElementDataTab.Controls.Add(this.bimRightPanelsDataGridView);
            this.movingElementDataTab.Controls.Add(this.loadHallButton);
            this.movingElementDataTab.Controls.Add(this.loadBimButton);
            this.movingElementDataTab.Controls.Add(this.updateBimButton);
        }
    }
}

