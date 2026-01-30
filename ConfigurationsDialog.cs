using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Hallbridger
{
    public class ConfigurationsDialog : Form
    {
        private readonly string iniPath;
        private IniFile iniFile;

        // real hall controls declaration
        private GroupBox realHallFileCheckGroupBox;
        private Label realHallFileCheckPathLabel, realHallFileCheckIntervalLabel, realHallFileCheckActiveLabel;
        private TextBox realHallFileCheckPathTextBox, realHallFileCheckIntervalTextBox;
        private Button realHallFileCheckPathBrowseButton;
        private CheckBox realHallFileCheckActiveCheckBox;

        // 3D hall controls declaration
        private GroupBox hall3DModelFileCheckGroupBox;
        private Label hall3DModelFileCheckPathLabel, hall3DModelFileCheckIntervalLabel, hall3DModelFileCheckOperationLabel, hall3DModelFileCheckActiveLabel;
        private TextBox hall3DModelFileCheckPathTextBox, hall3DModelFileCheckIntervalTextBox;
        private Button hall3DModelFileCheckPathBrowseButton;
        private CheckBox hall3DModelFileCheckActiveCheckBox;
        private ComboBox hall3DModelFileCheckOperationComboBox;

        // buttons declaration
        private Button okButton, cancelButton;


        /* INITIALIZATION METHODS
         */

        public ConfigurationsDialog(string iniPath)
        {
            this.iniPath = iniPath;
            this.iniFile = new IniFile(iniPath);

            InitializeComponents();
            LoadIniValues();
        }

        private void InitializeComponents()
        {
            this.Text = "Configurations";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(480, 340);
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // real hall file check group elements creation and initialization
            realHallFileCheckGroupBox = new GroupBox
            {
                Text = "Real hall file check",
                Location = new Point(12, 12),
                Size = new Size(450, 90)
            };
            var realHallFileCheckGroupInfo = CreateInfoIcon(
                new Point(realHallFileCheckGroupBox.Width - 25, 0),
                "Configure the automatic file check for the real hall data");
            realHallFileCheckGroupBox.Controls.Add(realHallFileCheckGroupInfo);

            realHallFileCheckPathLabel = new Label
            {
                Text = "File path:",
                Location = new Point(16, 28),
                AutoSize = true
            };
            realHallFileCheckPathTextBox = new TextBox
            {
                Location = new Point(80, 25),
                Width = 260
            };
            realHallFileCheckPathBrowseButton = new Button
            {
                Text = "Browse...",
                Location = new Point(350, 24),
                Width = 75
            };
            realHallFileCheckPathBrowseButton.Click += RealHallFileBrowseButton_Click;

            realHallFileCheckIntervalLabel = new Label
            {
                Text = "Interval (ms):",
                Location = new Point(16, 56),
                AutoSize = true
            };
            var realHallIntervalInfo = CreateInfoIcon(
                new Point(realHallFileCheckIntervalLabel.Location.X + realHallFileCheckIntervalLabel.PreferredWidth + 5, realHallFileCheckIntervalLabel.Location.Y),
                "Time interval between two consecutive file checks.");
            realHallFileCheckGroupBox.Controls.Add(realHallIntervalInfo);
            realHallFileCheckIntervalTextBox = new TextBox
            {
                Location = new Point(realHallIntervalInfo.Location.X + 22, 53),
                Width = 100
            };

            realHallFileCheckActiveLabel = new Label
            {
                Text = "Active:",
                Location = new Point(240, 56),
                AutoSize = true
            };
            realHallFileCheckActiveCheckBox = new CheckBox
            {
                Location = new Point(300, 53)
            };

            realHallFileCheckGroupBox.Controls.AddRange(
                new Control[]
                {
                    realHallFileCheckPathLabel, realHallFileCheckPathTextBox, realHallFileCheckPathBrowseButton,
                    realHallFileCheckIntervalLabel, realHallFileCheckIntervalTextBox,
                    realHallFileCheckActiveLabel, realHallFileCheckActiveCheckBox
                });

            // 3D hall file check group elements creation and initialization
            hall3DModelFileCheckGroupBox = new GroupBox
            {
                Text = "3D hall file check",
                Location = new Point(12, 112),
                Size = new Size(450, 110)
            };
            var hall3DGroupInfo = CreateInfoIcon(
                new Point(hall3DModelFileCheckGroupBox.Width - 25, 0),
                "Configure the automatic file check for the 3D hall model");
            hall3DModelFileCheckGroupBox.Controls.Add(hall3DGroupInfo);

            hall3DModelFileCheckPathLabel = new Label
            {
                Text = "File path:",
                Location = new Point(16, 28),
                AutoSize = true
            };
            hall3DModelFileCheckPathTextBox = new TextBox
            {
                Location = new Point(80, 25),
                Width = 260
            };
            hall3DModelFileCheckPathBrowseButton = new Button
            {
                Text = "Browse...",
                Location = new Point(350, 24),
                Width = 75
            };
            hall3DModelFileCheckPathBrowseButton.Click += Hall3DModelFileBrowseButton_Click;

            hall3DModelFileCheckIntervalLabel = new Label
            {
                Text = "Interval (ms):",
                Location = new Point(16, 56),
                AutoSize = true
            };
            var hall3DIntervalInfo = CreateInfoIcon(
                new Point(hall3DModelFileCheckIntervalLabel.Location.X + hall3DModelFileCheckIntervalLabel.PreferredWidth + 5, hall3DModelFileCheckIntervalLabel.Location.Y),
                "Time interval between two consecutive file checks.");
            hall3DModelFileCheckGroupBox.Controls.Add(hall3DIntervalInfo);
            hall3DModelFileCheckIntervalTextBox = new TextBox
            {
                Location = new Point(hall3DIntervalInfo.Location.X + 22, 53),
                Width = 100
            };

            hall3DModelFileCheckOperationLabel = new Label
            {
                Text = "Operation:",
                Location = new Point(16, 82),
                AutoSize = true
            };
            var hall3DOperationInfo = CreateInfoIcon(
                new Point(hall3DModelFileCheckOperationLabel.Location.X + hall3DModelFileCheckOperationLabel.PreferredWidth + 5, hall3DModelFileCheckOperationLabel.Location.Y),
                "Specify the operation that will be performed with the file if it is found:\n- \"Load\": import the 3D model displaying the data about moving elements and the model itself in the 3D viewer\n- \"Update\": modify the 3D hall file using the real hall data and then import it for displaying");
            hall3DModelFileCheckGroupBox.Controls.Add(hall3DOperationInfo);
            hall3DModelFileCheckOperationComboBox = new ComboBox
            {
                Location = new Point(hall3DOperationInfo.Location.X + 22, 79),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            hall3DModelFileCheckOperationComboBox.Items.AddRange(
                new object[]
                {
                    "Load",
                    "Update"
                });

            hall3DModelFileCheckActiveLabel = new Label
            {
                Text = "Active:",
                Location = new Point(240, 56),
                AutoSize = true
            };
            hall3DModelFileCheckActiveCheckBox = new CheckBox
            {
                Location = new Point(300, 53)
            };

            hall3DModelFileCheckGroupBox.Controls.AddRange(
                new Control[]
                {
                    hall3DModelFileCheckPathLabel, hall3DModelFileCheckPathTextBox, hall3DModelFileCheckPathBrowseButton,
                    hall3DModelFileCheckIntervalLabel, hall3DModelFileCheckIntervalTextBox,
                    hall3DModelFileCheckOperationLabel, hall3DModelFileCheckOperationComboBox,
                    hall3DModelFileCheckActiveLabel, hall3DModelFileCheckActiveCheckBox
                });

            // button creation and initialization
            okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(270, 255),
                Width = 80
            };
            okButton.Click += OkButton_Click;

            cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(370, 255),
                Width = 80
            };

            // add to form
            this.Controls.Add(realHallFileCheckGroupBox);
            this.Controls.Add(hall3DModelFileCheckGroupBox);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);
        }

        // load real and 3D hall file check configurations
        private void LoadIniValues()
        {
            string realHallFileCheckDirectory = iniFile.Read("RealHallFileCheck", "Directory");
            string realHallFileCheckName = iniFile.Read("RealHallFileCheck", "FileName");
            realHallFileCheckPathTextBox.Text = Path.Combine(realHallFileCheckDirectory ?? "", realHallFileCheckName ?? "");
            realHallFileCheckIntervalTextBox.Text = iniFile.Read("RealHallFileCheck", "Interval");
            realHallFileCheckActiveCheckBox.Checked = iniFile.Read("RealHallFileCheck", "Active").Trim().ToLower() == "true";

            string hall3DModelFileCheckDirectory = iniFile.Read("Hall3DModelFileCheck", "Directory");
            string hall3DModelFileCheckName = iniFile.Read("Hall3DModelFileCheck", "FileName");
            hall3DModelFileCheckPathTextBox.Text = Path.Combine(hall3DModelFileCheckDirectory ?? "", hall3DModelFileCheckName ?? "");
            hall3DModelFileCheckIntervalTextBox.Text = iniFile.Read("Hall3DModelFileCheck", "Interval");
            string hall3DModelFileCheckOperation = iniFile.Read("Hall3DModelFileCheck", "Operation");
            hall3DModelFileCheckOperationComboBox.SelectedItem = (hall3DModelFileCheckOperation == "Load")
                ? "Load"
                : "Update";
            hall3DModelFileCheckActiveCheckBox.Checked = iniFile.Read("Hall3DModelFileCheck", "Active").Trim().ToLower() == "true";
        }


        /* EVENT HANDLERS
         */

        // button event handlers
        private void OkButton_Click(object sender, EventArgs e)
        {
            // check validity of inputs
            if (!int.TryParse(realHallFileCheckIntervalTextBox.Text, out int realHallFileCheckInterval) || realHallFileCheckInterval < 0)
            {
                MessageBox.Show("Insert a positive integer number for the time interval between file checks.", "Invalid value", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                realHallFileCheckIntervalTextBox.Focus();
                return;
            }

            if (!int.TryParse(hall3DModelFileCheckIntervalTextBox.Text, out int hall3DModelFileCheckInterval) || hall3DModelFileCheckInterval < 0)
            {
                MessageBox.Show("Insert a positive integer number for the time interval between file checks.", "Invalid value", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                hall3DModelFileCheckIntervalTextBox.Focus();
                return;
            }

            // save configurations to ini file
            iniFile.Write("RealHallFileCheck", "Directory", SplitPath(realHallFileCheckPathTextBox.Text).fileDirectory);
            iniFile.Write("RealHallFileCheck", "FileName", SplitPath(realHallFileCheckPathTextBox.Text).fileName);
            iniFile.Write("RealHallFileCheck", "Interval", realHallFileCheckInterval.ToString());
            iniFile.Write("RealHallFileCheck", "Active", realHallFileCheckActiveCheckBox.Checked
                ? "true"
                : "false");

            iniFile.Write("Hall3DModelFileCheck", "Directory", SplitPath(hall3DModelFileCheckPathTextBox.Text).fileDirectory);
            iniFile.Write("Hall3DModelFileCheck", "FileName", SplitPath(hall3DModelFileCheckPathTextBox.Text).fileName);
            iniFile.Write("Hall3DModelFileCheck", "Interval", hall3DModelFileCheckInterval.ToString());
            iniFile.Write("Hall3DModelFileCheck", "Active", hall3DModelFileCheckActiveCheckBox.Checked
                ? "true"
                : "false");
            iniFile.Write("Hall3DModelFileCheck", "Operation", hall3DModelFileCheckOperationComboBox.SelectedItem?.ToString() ?? "Load");

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void RealHallFileBrowseButton_Click(object sender, EventArgs e)
        {
            using (var realHallFileBrowseDialog = new OpenFileDialog())
            {
                realHallFileBrowseDialog.Title = "Select real hall data file";
                realHallFileBrowseDialog.Filter = "Text file (*.txt)|*.txt|Excel spreadsheet (*.xls;*.xlsx)|*.xls;*.xlsx|JavaScript Object Notation file (*.json)|*.json|eXtensible Markup Language file (*.xml)|*.xml| All files (*.*)|*.*";

                if (!string.IsNullOrWhiteSpace(realHallFileCheckPathTextBox.Text))
                {
                    try
                    {
                        realHallFileBrowseDialog.InitialDirectory = Path.GetDirectoryName(realHallFileCheckPathTextBox.Text);
                        realHallFileBrowseDialog.FileName = Path.GetFileName(realHallFileCheckPathTextBox.Text); // proposed file name, it appears in the file name text field. Property "FileName" actually contains the whole path, but we don't want to see the whole path here (improper use of the property, but MS wants it this way)
                    }
                    catch { }
                }

                if (realHallFileBrowseDialog.ShowDialog() == DialogResult.OK)
                {
                    string realHallDataFileExtension = Path.GetExtension(realHallFileBrowseDialog.FileName).ToLowerInvariant();

                    if (!new[] { ".txt", ".xls", ".xlsx", ".json", ".xml" }.Contains(realHallDataFileExtension))
                    {
                        System.Windows.Forms.MessageBox.Show("File extension not supported: " + realHallDataFileExtension);
                        return;
                    }

                    realHallFileCheckPathTextBox.Text = realHallFileBrowseDialog.FileName; // here it is correct to use the FileName property to get the full path
                }
            }
        }

        private void Hall3DModelFileBrowseButton_Click(object sender, EventArgs e)
        {
            using (var hall3DModelFileBrowseDialog = new OpenFileDialog())
            {
                hall3DModelFileBrowseDialog.Title = "Select 3D hall file";
                hall3DModelFileBrowseDialog.Filter = "3D model file (*.ifc)|*.ifc|All files (*.*)|*.*";

                if (!string.IsNullOrWhiteSpace(hall3DModelFileCheckPathTextBox.Text))
                {
                    try
                    {
                        hall3DModelFileBrowseDialog.InitialDirectory = Path.GetDirectoryName(hall3DModelFileCheckPathTextBox.Text);
                        hall3DModelFileBrowseDialog.FileName = Path.GetFileName(hall3DModelFileCheckPathTextBox.Text); // proposed file name, it appears in the file name text field. Property "FileName" actually contains the whole path, but we don't want to see the whole path here (improper use of the property, but MS wants it this way)
                    }
                    catch { }
                }
                if (hall3DModelFileBrowseDialog.ShowDialog() == DialogResult.OK)
                {
                    string hall3DModelFileExtension = Path.GetExtension(hall3DModelFileBrowseDialog.FileName).ToLowerInvariant();

                    if (!new[] { ".ifc" }.Contains(hall3DModelFileExtension))
                    {
                        System.Windows.Forms.MessageBox.Show("File extension not supported: " + hall3DModelFileExtension);
                        return;
                    }

                    hall3DModelFileCheckPathTextBox.Text = hall3DModelFileBrowseDialog.FileName; // here it is correct to use the FileName property to get the full path
                }
            }
        }


        /* AUXILIARY METHODS
         */

        // splits a full file path into directory and file name
        private (string fileDirectory, string fileName) SplitPath(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return ("", "");
            }

            string fileDirectory = Path.GetDirectoryName(fullPath) ?? "";
            string fileName = Path.GetFileName(fullPath) ?? "";
            return (fileDirectory, fileName);
        }

        // creates an info icon with tooltip
        private Label CreateInfoIcon(Point location, string tooltipText)
        {
            var infoLabel = new Label
            {
                Text = "🛈", // Unicode "information" symbol
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold),
                AutoSize = true,
                Location = location,
                Cursor = Cursors.Hand
            };
            var toolTip = new ToolTip();
            toolTip.SetToolTip(infoLabel, tooltipText);
            return infoLabel;
        }
    }
}