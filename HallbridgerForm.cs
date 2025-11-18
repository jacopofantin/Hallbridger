using Microsoft.Isam.Esent.Interop;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace Hallbridger
{
    // auxiliary class to read INI files
    public class IniFile
    {
        public string Path;
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public IniFile(string path)
        {
            Path = path;
        }

        public string Read(string section, string key)
        {
            StringBuilder stringBuilder = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", stringBuilder, 255, Path);
            return stringBuilder.ToString();
        }
    }

    public partial class HallbridgerForm : Form
    {
        // class-level dictionaries to store moving element data coming from the real hall in
        Dictionary<string, int> hallStagecraftEquipmentPositions = new Dictionary<string, int>();
        Dictionary<string, int> hallLeftPanelApertures = new Dictionary<string, int>();
        Dictionary<string, int> hallRightPanelApertures = new Dictionary<string, int>();

        // class-level dictionaries to store moving element data coming from the IFC model in
        Dictionary<string, int> ifcStagecraftEquipmentPositions = new Dictionary<string, int>();
        Dictionary<string, int> ifcLeftPanelApertures = new Dictionary<string, int>();
        Dictionary<string, int> ifcRightPanelApertures = new Dictionary<string, int>();

        // class-level dictionaries to store correspondences between hall IDs and IFC model element names
        Dictionary<string, string> stagecraftEquipmentMapping = new Dictionary<string, string>();
        Dictionary<string, string> leftPanelMapping = new Dictionary<string, string>();
        Dictionary<string, string> rightPanelMapping = new Dictionary<string, string>();

        // hard-coded panel configurations
        private readonly Dictionary<string, HashSet<string>> panelConfigurations = new Dictionary<string, HashSet<string>>
                {
                    {"CLOSED", new HashSet<string> {}},
                    {"2F", new HashSet<string> {"PS.001", "PS.002", "PS.003", "PD.001", "PD.002", "PD.003"}},
                    {"2C", new HashSet<string> {"PS.004", "PS.005", "PS.006", "PS.007", "PD.004", "PD.005", "PD.006", "PD.007"}},
                    {"1F", new HashSet<string> {"PS.012", "PS.013", "PS.014", "PS.015", "PD.012", "PD.013", "PD.014", "PD.015"}},
                    {"1C", new HashSet<string> {"PS.016", "PS.017", "PS.018", "PS.019", "PD.016", "PD.017", "PD.018", "PD.019"}},
                    {"1P", new HashSet<string> {"PS.020", "PS.021", "PS.022", "PS.023", "PD.020", "PD.021", "PD.022", "PD.023"}},
                    {"0C", new HashSet<string> {"PS.024", "PS.025", "PS.026", "PD.024", "PD.025", "PD.026"}},
                    {"1FC", new HashSet<string> {"PS.012", "PS.013", "PS.014", "PS.015", "PS.016", "PS.017", "PS.018", "PS.019", "PD.012", "PD.013", "PD.014", "PD.015", "PD.016", "PD.017", "PD.018", "PD.019"}},
                    {"1FCP", new HashSet<string> {"PS.012", "PS.013", "PS.014", "PS.015", "PS.016", "PS.017", "PS.018", "PS.019", "PS.020", "PS.021", "PS.022", "PS.023", "PD.012", "PD.013", "PD.014", "PD.015", "PD.016", "PD.017", "PD.018", "PD.019", "PD.020", "PD.021", "PD.022", "PD.023"}},
                    {"2FCP", new HashSet<string> {"PS.001", "PS.002", "PS.003", "PS.004", "PS.005", "PS.006", "PS.007", "PS.008", "PS.009", "PS.010", "PS.011", "PD.001", "PD.002", "PD.003", "PD.004", "PD.005", "PD.006", "PD.007", "PD.008", "PD.009", "PD.010", "PD.011"}},
                    {"OPEN", new HashSet<string> {"PS.001", "PS.002", "PS.003", "PS.004", "PS.005", "PS.006", "PS.007", "PS.008", "PS.009", "PS.010", "PS.011", "PS.012", "PS.013", "PS.014", "PS.015", "PS.016", "PS.017", "PS.018", "PS.019", "PS.020", "PS.021", "PS.022", "PS.023", "PS.024", "PS.025", "PS.026", "PD.001", "PD.002", "PD.003", "PD.004", "PD.005", "PD.006", "PD.007", "PD.008", "PD.009", "PD.010", "PD.011", "PD.012", "PD.013", "PD.014", "PD.015", "PD.016", "PD.017", "PD.018", "PD.019", "PD.020", "PD.021", "PD.022", "PD.023", "PD.024", "PD.025", "PD.026"}}
                };

        // class-level variable to keep track of the current open/close panel configuration of the IFC model
        private string currentIfcPanelConfiguration = null;

        // class-level variables to parametrize maximum panel aperture values
        private static double maxApertureDegrees = 30.0;
        private static double maxPositionMillimeters, maxApertureMillimiters = 99999.0;

        // class-level dictionary to store selected cells in DataGridViews (needed since both sorting and data refresh cause the user-defined selection to be lost)
        Dictionary<DataGridView, List<(string RowId, int ColumnIndex)>> selectedCells = new Dictionary<DataGridView, List<(string RowId, int ColumnIndex)>>();


        // INI file and timers for automatic file check and usage
        private string iniPath = "D:\\Dateien\\Hallbridger\\Configuration\\conf.ini";

        private System.Windows.Forms.Timer hallFileCheckTimer;
        private string hallFileCheckDirectory = "D:\\Dateien\\Hallbridger\\IO_files"; // default directory, can be changed in INI file
        private string hallFileCheckName = "Fotografia_sala_CURIO.txt"; // default file name, can be changed in INI file
        private int hallFileCheckInterval = 5000; // default to 5 seconds, can be changed in INI file
        private bool hallFileCheckActive = true; // default to true, can be changed in INI file

        private System.Windows.Forms.Timer ifcFileCheckTimer;
        private string ifcFileCheckDirectory = "D:\\Dateien\\Hallbridger\\IO_files"; // default directory, can be changed in INI file
        private string ifcFileCheckName = "Model.ifc"; // default file name, can be changed in INI file
        private int ifcFileCheckInterval = 5000; // default to 5 seconds, can be changed in INI file
        private bool ifcFileCheckActive = true; // default to true, can be changed in INI file


        public HallbridgerForm()
        {
            InitializeComponent();
            this.Icon = new Icon("icon.ico");
        }


        // timer methods
        protected void InitTimers()
        {
            // initialize hall file timer if active
            if (hallFileCheckActive)
            {
                hallFileCheckTimer = new System.Windows.Forms.Timer();
                hallFileCheckTimer.Interval = hallFileCheckInterval;
                hallFileCheckTimer.Tick += HallFileCheckTimer_Tick;
                hallFileCheckTimer.Start();
            }

            // initialize IFC file timer if active
            if (ifcFileCheckActive)
            {
                ifcFileCheckTimer = new System.Windows.Forms.Timer();
                ifcFileCheckTimer.Interval = ifcFileCheckInterval;
                ifcFileCheckTimer.Tick += IfcFileCheckTimer_Tick;
                ifcFileCheckTimer.Start();
            }
        }

        private async void HallFileCheckTimer_Tick(object sender, EventArgs e)
        {
            string filePath = Path.Combine(hallFileCheckDirectory, hallFileCheckName);
            if (File.Exists(filePath))
            {
                string apiEndpoint;
                string fileExtension = Path.GetExtension(filePath).ToLowerInvariant();

                switch (fileExtension)
                {
                    case ".txt":
                        apiEndpoint = "https://localhost:44307/api/import/txt";
                        break;
                    case ".xls":
                    case ".xlsx":
                        apiEndpoint = "https://localhost:44307/api/import/excel";
                        break;
                    case ".json":
                        apiEndpoint = "https://localhost:44307/api/import/json";
                        break;
                    case ".xml":
                        apiEndpoint = "https://localhost:44307/api/import/xml";
                        break;
                    default:
                        MessageBox.Show("File extension not supported: " + fileExtension);
                        return;
                }

                await LoadHallFile(apiEndpoint, filePath);
            }
        }

        private void IfcFileCheckTimer_Tick(object sender, EventArgs e)
        {
            string filePath = Path.Combine(ifcFileCheckDirectory, ifcFileCheckName);
            if (File.Exists(filePath))
            {
                // file found, load it
                LoadIfcFile(filePath);
            }
        }


        // button methods
        private async void LoadHallButton_OnClick(object sender, EventArgs e)
        {
            using (OpenFileDialog loadHallDialog = new OpenFileDialog())
            {
                loadHallDialog.Filter = "Text files (*.txt)|*.txt|Excel spreadsheet (*.xls;*.xlsx)|*.xls;*.xlsx|JavaScript Object Notation file (*.json)|*.json|eXtensible Markup Language file (*.xml)|*.xml| All files (*.*)|*.*";
                if (loadHallDialog.ShowDialog() == DialogResult.OK)
                {
                    string apiEndpoint;
                    string fileExtension = Path.GetExtension(loadHallDialog.FileName).ToLowerInvariant();

                    switch (fileExtension)
                    {
                        case ".txt":
                            apiEndpoint = "https://localhost:44307/api/import/txt";
                            break;
                        case ".xls":
                        case ".xlsx":
                            apiEndpoint = "https://localhost:44307/api/import/excel";
                            break;
                        case ".json":
                            apiEndpoint = "https://localhost:44307/api/import/json";
                            break;
                        case ".xml":
                            apiEndpoint = "https://localhost:44307/api/import/xml";
                            break;
                        default:
                            MessageBox.Show("File extension not supported: " + fileExtension);
                            return;
                    }

                    await LoadHallFile(apiEndpoint, loadHallDialog.FileName);
                }
            }
        }

        private void LoadIfcButton_OnClick(object sender, EventArgs e)
        {
            using (OpenFileDialog loadIfcDialog = new OpenFileDialog())
            {
                loadIfcDialog.Filter = "IFC file (*.ifc)|*.ifc|All files (*.*)|*.*";
                if (loadIfcDialog.ShowDialog() == DialogResult.OK)
                {
                    string ifcFilePath = loadIfcDialog.FileName;
                    LoadIfcFile(ifcFilePath);
                }
            }
        }

        private void UpdateIfcButton_OnClick(object sender, EventArgs e)
        {
            using (OpenFileDialog updateIfcDialog = new OpenFileDialog())
            {
                updateIfcDialog.Filter = "IFC model (*.ifc)|*.ifc|All files (*.*)|*.*";
                if (updateIfcDialog.ShowDialog() == DialogResult.OK)
                {
                    string ifcFilePath = updateIfcDialog.FileName;
                    try
                    {
                        var editor = new XbimEditorCredentials
                        {
                            ApplicationDevelopersName = "Jacopo Fantin",
                            ApplicationFullName = "Hallbridger",
                            ApplicationIdentifier = "Hallbridger",
                            ApplicationVersion = "1.0",
                            EditorsFamilyName = "Fantin",
                            EditorsGivenName = "Jacopo",
                            EditorsOrganisationName = "Politecnico di Milano"
                        };

                        // open the IFC file
                        using (var model = IfcStore.Open(ifcFilePath, editor))
                        {
                            // start a transaction to modify the IFC file for the panel aperture
                            using (var panelApertureUpdate = model.BeginTransaction("Pivoting panel aperture update"))
                            {
                                var firstPanel = model.Instances.OfType<IIfcFurnishingElement>().FirstOrDefault();
                                var rotationAngleProperty = (firstPanel?.IsTypedBy
                                    .FirstOrDefault(type => type.RelatingType.Name == "PANEL ROTATION:PANEL ROTATION 60")?.RelatingType as IIfcTypeObject)?
                                    .HasPropertySets.OfType<IIfcPropertySet>()
                                    .FirstOrDefault(pset => pset.Name == "Quote")?
                                    .HasProperties.OfType<IIfcPropertySingleValue>()
                                    .FirstOrDefault(prop => prop.Name == "PANEL ANGLE");

                                // writes the new aperture of the panel in the IFC model
                                rotationAngleProperty.NominalValue = new Xbim.Ifc4.MeasureResource.IfcPlaneAngleMeasure(22.79);
                                
                                // commit the changes and save the IFC file
                                panelApertureUpdate.Commit();
                                model.SaveAs(ifcFilePath);
                            }
                            /*
                            // start a transaction to modify the IFC file for right wall pivoting panel apertures
                            using (var rightPanelApertureUpdate = model.BeginTransaction("Right wall pivoting panel aperture update"))
                            {
                                //... extract right panel information into IEnumerable ifcRightPanels like we already did for the loadIfcButton...

                                //ifcRightPanels: IEnumerable, rightPanelApertures: list
                                //to be used if the model does not have stagecraft equipment in it: in that case, it makes most sense to use lists for data imported from TXT since panels are identified from number 1 to 26 (we can forget about their IDs)
                                for (int i = 0; i < rightPanelApertures.Count; i++)
                                {
                                    var rightPanel = ifcRightPanels.ElementAt(i);

                                    var aperture = rightPanel.IsDefinedBy
                                        .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                                        .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                                        .OfType<IIfcPropertySingleValue>()
                                        .FirstOrDefault(p => p.Name = "Aperture");

                                    // writes the aperture of each panel on the right wall in the IFC model
                                    aperture.NominalValue = new Xbim.Ifc4.MeasureResource.IfcLengthMeasure(rightPanelApertures[i]);
                                }

                                //ifcRightPanels: IEnumerable, rightPanelMapping: dictionary<string, int> where key is the hall ID of the right wall panel and value is the index of the corresponding panel in ifcRightPanels
                                //to be used if the model actually has stagecraft equipment in it: in that case, we must use a dictionary for its information and in this case we wanna use dictionaries for all of the data we import from TXT
                                foreach (var corr in rightPanelMapping)
                                {
                                    var rightPanel = ifcRightPanels.ElementAt(corr.Value);

                                    var aperture = rightPanel.IsDefinedBy
                                        .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                                        .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                                        .OfType<IIfcPropertySingleValue>()
                                        .FirstOrDefault(p => p.Name = "Aperture");

                                    // writes the aperture of each panel on the right wall in the IFC model
                                    aperture.NominalValue = new Xbim.Ifc4.MeasureResource.IfcLengthMeasure(rightPanelApertures[corr.Key]);
                                }

                                // commit the changes to the IFC file
                                rightPanelApertureUpdate.Commit();
                            }

                            // start a transaction to modify the IFC file for stagecraft equipment positions
                            using (var stagecraftEquipmentPositionsUpdate = model.BeginTransaction("Stagecraft equipment position update"))
                            {
                                //... extract stagecraft equipment information into IEnumerable ifcStagecraftEquipment like we already did for the loadIfcButton...

                                //ifcStagecraftEquipment: IEnumerable, stagecraftEquipmentMapping: dictionary<string, int> where key is the hall ID of the stagecraft equipment piece and value is the index of the corresponding piece in ifcStagecraftEquipment
                                foreach (var corr in stagecraftEquipmentMapping)
                                {
                                    var piece = ifcStagecraftEquipment.ElementAt(corr.Value);

                                    var position = piece.IsDefinedBy
                                        .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                                        .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                                        .OfType<IIfcPropertySingleValue>()
                                        .FirstOrDefault(p => p.Name = "Position");

                                    // writes the position of each stagecraft equipment piece in the IFC model
                                    position.NominalValue = new Xbim.Ifc4.MeasureResource.IfcLengthMeasure(stagecraftEquipmentPositions[corr.Key]);
                                }

                                // commit the changes to the IFC file
                                stagecraftEquipmentPositionsUpdate.Commit();
                            }*/
                        }
                        // after updating the IFC file, reload it to update the displayed data
                        LoadIfcFile(ifcFilePath);
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show("Error while reading IFC file: " + error.Message);
                    }
                }
            }
        }

        // DataGridView column header methods
        private void DataGridView_ColumnHeaderMouseDown(object sender, MouseEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv == null) return;

            var hit = dgv.HitTest(e.X, e.Y);
            if (hit.Type == DataGridViewHitTestType.ColumnHeader)
            {
                selectedCells[dgv] = GetSelectedCells(dgv);
            }
        }

        private void DataGridView_Sorted(object sender, EventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv == null) return;

            if (selectedCells.TryGetValue(dgv, out var cellSelection))
            {
                RestoreSelectedCells(dgv, cellSelection);
            }
        }

        // tab control method
        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            globalRtDataGridView.ClearSelection();
        }

        // post-paint method to highlight in the global RT DataGridView the current open/close panel configuration of the IFC model (gets called after each row is painted)
        private void GlobalRtDataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv == null || string.IsNullOrEmpty(currentIfcPanelConfiguration))
                return;

            // if the row that has just been painted corresponds to the current open/close panel configuration of the IFC model, draw a red rectangle around it
            var configuration = dgv.Rows[e.RowIndex].Cells["Configuration"].Value?.ToString();
            if (configuration == currentIfcPanelConfiguration)
            {
                Rectangle rowFrame = new Rectangle(
                    e.RowBounds.Left,
                    e.RowBounds.Top,
                    e.RowBounds.Width - 1,
                    e.RowBounds.Height - 1);

                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, rowFrame);
                }
            }
        }

        // method to import hall data through API
        private async Task LoadHallFile(string apiUrl, string filePath)
        {
            using (var client = new HttpClient())
            using (var form = new MultipartFormDataContent())
            using (var fileStream = File.OpenRead(filePath))
            {
                form.Add(new StreamContent(fileStream), "file", Path.GetFileName(filePath));
                var response = await client.PostAsync(apiUrl, form);

                if (!response.IsSuccessStatusCode)
                {
                    // read and show error message coming from API
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("Error importing hall data:\n" + errorMessage, "Import error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<Hallbridger_API.Models.CurioDataModel>(json);

                // save current scroll positions
                int hallStagecraftTopRowIndex = hallStagecraftDataGridView.FirstDisplayedScrollingRowIndex;
                int hallLeftPanelsTopRowIndex = hallLeftPanelsDataGridView.FirstDisplayedScrollingRowIndex;
                int hallRightPanelsTopRowIndex = hallRightPanelsDataGridView.FirstDisplayedScrollingRowIndex;

                // save current cell selection
                selectedCells[hallStagecraftDataGridView] = GetSelectedCells(hallStagecraftDataGridView);
                selectedCells[hallLeftPanelsDataGridView] = GetSelectedCells(hallLeftPanelsDataGridView);
                selectedCells[hallRightPanelsDataGridView] = GetSelectedCells(hallRightPanelsDataGridView);

                // save current data sorting
                DataGridViewColumn hallStagecraftSortedColumn = hallStagecraftDataGridView.SortedColumn;
                ListSortDirection? hallStagecraftSortDirection = null;
                if (hallStagecraftSortedColumn != null)
                {
                    hallStagecraftSortDirection = hallStagecraftDataGridView.SortOrder == SortOrder.Descending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                }

                DataGridViewColumn hallLeftPanelsSortedColumn = hallLeftPanelsDataGridView.SortedColumn;
                ListSortDirection? hallLeftPanelsSortDirection = null;
                if (hallLeftPanelsSortedColumn != null)
                {
                    hallLeftPanelsSortDirection = hallLeftPanelsDataGridView.SortOrder == SortOrder.Descending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                }

                DataGridViewColumn hallRightPanelsSortedColumn = hallRightPanelsDataGridView.SortedColumn;
                ListSortDirection? hallRightPanelsSortDirection = null;
                if (hallRightPanelsSortedColumn != null)
                {
                    hallRightPanelsSortDirection = hallRightPanelsDataGridView.SortOrder == SortOrder.Descending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                }

                // save current cell background colors
                Dictionary<string, Color> stagecraftCellColors = new Dictionary<string, Color>();
                foreach (DataGridViewRow row in hallStagecraftDataGridView.Rows)
                {
                    string rowId = row.Cells[0].Value?.ToString();
                    stagecraftCellColors[rowId] = row.Cells[0].Style.BackColor;
                }

                Dictionary<string, Color> leftPanelsCellColors = new Dictionary<string, Color>();
                foreach (DataGridViewRow row in hallLeftPanelsDataGridView.Rows)
                {
                    string rowId = row.Cells[0].Value?.ToString();
                    leftPanelsCellColors[rowId] = row.Cells[0].Style.BackColor;
                }

                Dictionary<string, Color> rightPanelsCellColors = new Dictionary<string, Color>();
                foreach (DataGridViewRow row in hallRightPanelsDataGridView.Rows)
                {
                    string rowId = row.Cells[0].Value?.ToString();
                    rightPanelsCellColors[rowId] = row.Cells[0].Style.BackColor;
                }

                // empty and refill the class-level dictionaries
                hallStagecraftEquipmentPositions.Clear();
                hallStagecraftEquipmentPositions = data.StagecraftEquipmentPositions;

                hallLeftPanelApertures.Clear();
                hallLeftPanelApertures = data.LeftPanelApertures;

                hallRightPanelApertures.Clear();
                hallRightPanelApertures = data.RightPanelApertures;

                // empty and refill corresponding DataGridViews
                hallStagecraftDataGridView.Rows.Clear();
                foreach (var pair in hallStagecraftEquipmentPositions)
                {
                    hallStagecraftDataGridView.Rows.Add(pair.Key, $"{ConvertPositionToMeters(pair.Value)} m");
                }

                hallLeftPanelsDataGridView.Rows.Clear();
                foreach (var pair in hallLeftPanelApertures)
                {
                    hallLeftPanelsDataGridView.Rows.Add(pair.Key, $"{ConvertApertureToArcdegrees(pair.Value)}");
                }

                hallRightPanelsDataGridView.Rows.Clear();
                foreach (var pair in hallRightPanelApertures)
                {
                    hallRightPanelsDataGridView.Rows.Add(pair.Key, $"{ConvertApertureToArcdegrees(pair.Value)}");
                }

                // restore previous scroll positions
                if (hallStagecraftTopRowIndex >= 0)
                {
                    hallStagecraftDataGridView.FirstDisplayedScrollingRowIndex = hallStagecraftTopRowIndex;
                }

                if (hallLeftPanelsTopRowIndex >= 0)
                {
                    hallLeftPanelsDataGridView.FirstDisplayedScrollingRowIndex = hallLeftPanelsTopRowIndex;
                }

                if (hallRightPanelsTopRowIndex >= 0)
                {
                    hallRightPanelsDataGridView.FirstDisplayedScrollingRowIndex = hallRightPanelsTopRowIndex;
                }

                // restore previous cell selection
                if (selectedCells.TryGetValue(hallStagecraftDataGridView, out var selection))
                {
                    RestoreSelectedCells(hallStagecraftDataGridView, selection);
                }

                if (selectedCells.TryGetValue(hallLeftPanelsDataGridView, out selection))
                {
                    RestoreSelectedCells(hallLeftPanelsDataGridView, selection);
                }

                if (selectedCells.TryGetValue(hallRightPanelsDataGridView, out selection))
                {
                    RestoreSelectedCells(hallRightPanelsDataGridView, selection);
                }

                // restore previous data sorting
                if (hallStagecraftSortedColumn != null && hallStagecraftSortDirection.HasValue)
                {
                    hallStagecraftDataGridView.Sort(hallStagecraftSortedColumn, hallStagecraftSortDirection.Value);
                }
                if (hallLeftPanelsSortedColumn != null && hallLeftPanelsSortDirection.HasValue)
                {
                    hallLeftPanelsDataGridView.Sort(hallLeftPanelsSortedColumn, hallLeftPanelsSortDirection.Value);
                }
                if (hallRightPanelsSortedColumn != null && hallRightPanelsSortDirection.HasValue)
                {
                    hallRightPanelsDataGridView.Sort(hallRightPanelsSortedColumn, hallRightPanelsSortDirection.Value);
                }

                // if a IFC file has already been loaded, highlight discrepancies between hall and IFC data
                if (ifcStagecraftDataGridView.Rows.Count > 0)
                {
                    foreach (var rel in stagecraftEquipmentMapping)
                    {
                        DataGridViewRow hallRow = hallStagecraftDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Key);
                        DataGridViewRow ifcRow = ifcStagecraftDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Value);

                        if (hallStagecraftEquipmentPositions[rel.Key] != ifcStagecraftEquipmentPositions[rel.Value])
                        {
                            // discrepancy found
                            if (stagecraftCellColors[rel.Key] == Color.Empty)
                            {
                                // new discrepancy, generate a new random color and store it in the dictionary
                                Random rnd = new Random();
                                Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

                                stagecraftCellColors[rel.Key] = randomColor;
                            }
                        }
                        else
                        {
                            // no discrepancy, reset cell colors to default if they are not already
                            if (stagecraftCellColors[rel.Key] != Color.Empty)
                            {
                                stagecraftCellColors[rel.Key] = Color.Empty;
                            }
                        }

                        // apply color to the row couple
                        hallRow.DefaultCellStyle.BackColor = stagecraftCellColors[rel.Key];
                        ifcRow.DefaultCellStyle.BackColor = stagecraftCellColors[rel.Key];
                    }
                }

                if (ifcLeftPanelsDataGridView.Rows.Count > 0)
                {
                    foreach (var rel in leftPanelMapping)
                    {
                        DataGridViewRow hallRow = hallLeftPanelsDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Key);
                        DataGridViewRow ifcRow = ifcLeftPanelsDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Value);

                        if (hallLeftPanelApertures[rel.Key] != ifcLeftPanelApertures[rel.Value])
                        {
                            // discrepancy found
                            if (leftPanelsCellColors[rel.Key] == Color.Empty)
                            {
                                // new discrepancy, generate a new random color and store it in the dictionary
                                Random rnd = new Random();
                                Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

                                leftPanelsCellColors[rel.Key] = randomColor;
                            }
                        }
                        else
                        {
                            // no discrepancy, reset cell colors to default if they are not already
                            if (leftPanelsCellColors[rel.Key] != Color.Empty)
                            {
                                leftPanelsCellColors[rel.Key] = Color.Empty;
                            }
                        }

                        // apply color to the row couple
                        hallRow.DefaultCellStyle.BackColor = leftPanelsCellColors[rel.Key];
                        ifcRow.DefaultCellStyle.BackColor = leftPanelsCellColors[rel.Key];
                    }
                }

                if (ifcRightPanelsDataGridView.Rows.Count > 0)
                {
                    foreach (var rel in rightPanelMapping)
                    {
                        DataGridViewRow hallRow = hallRightPanelsDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Key);
                        DataGridViewRow ifcRow = ifcRightPanelsDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Value);

                        if (hallRightPanelApertures[rel.Key] != ifcRightPanelApertures[rel.Value])
                        {
                            // discrepancy found
                            if (rightPanelsCellColors[rel.Key] == Color.Empty)
                            {
                                // new discrepancy, generate a new random color and store it in the dictionary
                                Random rnd = new Random();
                                Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

                                rightPanelsCellColors[rel.Key] = randomColor;
                            }
                        }
                        else
                        {
                            // no discrepancy, reset cell colors to default if they are not already
                            if (rightPanelsCellColors[rel.Key] != Color.Empty)
                            {
                                rightPanelsCellColors[rel.Key] = Color.Empty;
                            }
                        }

                        // apply color to the row couple
                        hallRow.DefaultCellStyle.BackColor = rightPanelsCellColors[rel.Key];
                        ifcRow.DefaultCellStyle.BackColor = rightPanelsCellColors[rel.Key];
                    }
                }
            }
        }

        // method to import IFC data
        private void LoadIfcFile(string filePath)
        {
            try
            {
                // save current scroll positions
                int ifcStagecraftTopRowIndex = ifcStagecraftDataGridView.FirstDisplayedScrollingRowIndex;
                int ifcLeftPanelsTopRowIndex = ifcLeftPanelsDataGridView.FirstDisplayedScrollingRowIndex;
                int ifcRightPanelsTopRowIndex = ifcRightPanelsDataGridView.FirstDisplayedScrollingRowIndex;

                // save current cell selection
                selectedCells[ifcStagecraftDataGridView] = GetSelectedCells(ifcStagecraftDataGridView);
                selectedCells[ifcLeftPanelsDataGridView] = GetSelectedCells(ifcLeftPanelsDataGridView);
                selectedCells[ifcRightPanelsDataGridView] = GetSelectedCells(ifcRightPanelsDataGridView);

                // save current data sorting
                DataGridViewColumn ifcStagecraftSortedColumn = ifcStagecraftDataGridView.SortedColumn;
                ListSortDirection? ifcStagecraftSortDirection = null;
                if (ifcStagecraftSortedColumn != null)
                {
                    ifcStagecraftSortDirection = ifcStagecraftDataGridView.SortOrder == SortOrder.Descending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                }

                DataGridViewColumn ifcLeftPanelsSortedColumn = ifcLeftPanelsDataGridView.SortedColumn;
                ListSortDirection? ifcLeftPanelsSortDirection = null;
                if (ifcLeftPanelsSortedColumn != null)
                {
                    ifcLeftPanelsSortDirection = ifcLeftPanelsDataGridView.SortOrder == SortOrder.Descending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                }

                DataGridViewColumn ifcRightPanelsSortedColumn = ifcRightPanelsDataGridView.SortedColumn;
                ListSortDirection? ifcRightPanelsSortDirection = null;
                if (ifcRightPanelsSortedColumn != null)
                {
                    ifcRightPanelsSortDirection = ifcRightPanelsDataGridView.SortOrder == SortOrder.Descending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                }

                // save current cell background colors
                Dictionary<string, Color> stagecraftCellColors = new Dictionary<string, Color>();
                foreach (DataGridViewRow row in ifcStagecraftDataGridView.Rows)
                {
                    string rowId = row.Cells[0].Value?.ToString();
                    stagecraftCellColors[rowId] = row.Cells[0].Style.BackColor;
                }

                Dictionary<string, Color> leftPanelsCellColors = new Dictionary<string, Color>();
                foreach (DataGridViewRow row in ifcLeftPanelsDataGridView.Rows)
                {
                    string rowId = row.Cells[0].Value?.ToString();
                    leftPanelsCellColors[rowId] = row.Cells[0].Style.BackColor;
                }

                Dictionary<string, Color> rightPanelsCellColors = new Dictionary<string, Color>();
                foreach (DataGridViewRow row in ifcRightPanelsDataGridView.Rows)
                {
                    string rowId = row.Cells[0].Value?.ToString();
                    rightPanelsCellColors[rowId] = row.Cells[0].Style.BackColor;
                }

                // empty the class-level dictionaries
                ifcStagecraftEquipmentPositions.Clear();
                ifcLeftPanelApertures.Clear();
                ifcRightPanelApertures.Clear();

                using (var model = IfcStore.Open(filePath))
                {
                    //extract a list of panel blocks from the IFC model
                    var pivotingPanels = model.Instances
                        .OfType<IIfcBuildingElementProxy>()
                        .Where(p => p.IsTypedBy
                            .Any(t => t.RelatingType != null && t.RelatingType.Name == "PANEL BLOCK:PANEL BLOCK"))
                        .ToList();

                    //extract the panel and its aperture angle
                    var firstPanel = model.Instances.OfType<IIfcFurnishingElement>().FirstOrDefault();
                    var rotationAngleProperty = (firstPanel?.IsTypedBy
                        .FirstOrDefault(type => type.RelatingType.Name == "PANEL ROTATION:PANEL ROTATION 60")?.RelatingType as IIfcTypeObject)?
                        .HasPropertySets.OfType<IIfcPropertySet>()
                        .FirstOrDefault(pset => pset.Name == "Quote")?
                        .HasProperties.OfType<IIfcPropertySingleValue>()
                        .FirstOrDefault(prop => prop.Name == "PANEL ANGLE");

                    Console.WriteLine($"Panel ID: {firstPanel?.GlobalId}, Name: {firstPanel?.Name}, PANEL ANGLE: {rotationAngleProperty?.NominalValue}");

                    // store panel names as keys and aperture values as values in the class-level dictionaries
                    /*
                    foreach (var leftPanel in ifcLeftPanels)
                    {
                        var aperture = leftPanel.IsDefinedBy
                            .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                            .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                            .OfType<IIfcPropertySingleValue>()
                            .Where(p => p.Name = "Aperture");

                        ifcLeftPanelApertures.Add(leftPanel.Name, int.Parse(aperture.NominalValue));
                    }

                    foreach (var rightPanel in ifcRightPanels)
                    {
                        var aperture = rightPanel.IsDefinedBy
                            .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                            .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                            .OfType<IIfcPropertySingleValue>()
                            .Where(p => p.Name = "Aperture");

                        ifcRightPanelApertures.Add(rightPanel.Name, int.Parse(aperture.NominalValue));
                    }

                    // store stagecraft equipment names as keys and positions as values in the class-level dictionary
                    foreach (var piece in ifcStagecraftEquipment)
                    {
                        var position = piece.IsDefinedBy
                            .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                            .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                            .OfType<IIfcPropertySingleValue>()
                            .Where(p => p.Name = "Position");

                        ifcStagecraftEquipmentPositions.Add(piece.Name, int.Parse(position.NominalValue));
                    }*/
                }

                // empty and refill corresponding DataGridViews
                ifcStagecraftDataGridView.Rows.Clear();
                foreach (var pair in ifcStagecraftEquipmentPositions)
                {
                    string[] row = {pair.Key, $"{ConvertPositionToMeters(pair.Value)} m"};
                    ifcStagecraftDataGridView.Rows.Add(row);
                }

                ifcLeftPanelsDataGridView.Rows.Clear();
                foreach (var pair in ifcLeftPanelApertures)
                {
                    string[] row = {pair.Key, $"{ConvertApertureToArcdegrees(pair.Value)}"};
                    ifcStagecraftDataGridView.Rows.Add(row);
                }

                ifcRightPanelsDataGridView.Rows.Clear();
                foreach (var pair in ifcRightPanelApertures)
                {
                    string[] row = {pair.Key, $"{ConvertApertureToArcdegrees(pair.Value)}"};
                    ifcStagecraftDataGridView.Rows.Add(row);
                }

                // restore previous scroll positions
                if (ifcStagecraftTopRowIndex >= 0)
                {
                    ifcStagecraftDataGridView.FirstDisplayedScrollingRowIndex = ifcStagecraftTopRowIndex;
                }

                if (ifcLeftPanelsTopRowIndex >= 0)
                {
                    ifcLeftPanelsDataGridView.FirstDisplayedScrollingRowIndex = ifcLeftPanelsTopRowIndex;
                }

                if (ifcRightPanelsTopRowIndex >= 0)
                {
                    ifcRightPanelsDataGridView.FirstDisplayedScrollingRowIndex = ifcRightPanelsTopRowIndex;
                }

                // restore previous cell selection
                if (selectedCells.TryGetValue(ifcStagecraftDataGridView, out var selection))
                {
                    RestoreSelectedCells(ifcStagecraftDataGridView, selection);
                }

                if (selectedCells.TryGetValue(ifcLeftPanelsDataGridView, out selection))
                {
                    RestoreSelectedCells(ifcLeftPanelsDataGridView, selection);
                }

                if (selectedCells.TryGetValue(ifcRightPanelsDataGridView, out selection))
                {
                    RestoreSelectedCells(ifcRightPanelsDataGridView, selection);
                }

                // restore previous data sorting
                if (ifcStagecraftSortedColumn != null && ifcStagecraftSortDirection.HasValue)
                {
                    ifcStagecraftDataGridView.Sort(ifcStagecraftSortedColumn, ifcStagecraftSortDirection.Value);
                }
                if (ifcLeftPanelsSortedColumn != null && ifcLeftPanelsSortDirection.HasValue)
                {
                    ifcLeftPanelsDataGridView.Sort(ifcLeftPanelsSortedColumn, ifcLeftPanelsSortDirection.Value);
                }
                if (ifcRightPanelsSortedColumn != null && ifcRightPanelsSortDirection.HasValue)
                {
                    ifcRightPanelsDataGridView.Sort(ifcRightPanelsSortedColumn, ifcRightPanelsSortDirection.Value);
                }

                // if a hall file has already been loaded, highlight discrepancies between hall and IFC data
                if (hallStagecraftDataGridView.Rows.Count > 0)
                {
                    foreach (var rel in stagecraftEquipmentMapping)
                    {
                        DataGridViewRow hallRow = hallStagecraftDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Key);
                        DataGridViewRow ifcRow = ifcStagecraftDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Value);

                        if (hallStagecraftEquipmentPositions[rel.Key] != ifcStagecraftEquipmentPositions[rel.Value])
                        {
                            // discrepancy found
                            if (stagecraftCellColors[rel.Key] == Color.Empty)
                            {
                                // new discrepancy, generate a new random color and store it in the dictionary
                                Random rnd = new Random();
                                Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

                                stagecraftCellColors[rel.Key] = randomColor;
                            }
                        }
                        else
                        {
                            // no discrepancy, reset cell colors to default if they are not already
                            if (stagecraftCellColors[rel.Key] != Color.Empty)
                            {
                                stagecraftCellColors[rel.Key] = Color.Empty;
                            }
                        }

                        // apply color to the row couple
                        hallRow.DefaultCellStyle.BackColor = stagecraftCellColors[rel.Key];
                        ifcRow.DefaultCellStyle.BackColor = stagecraftCellColors[rel.Key];
                    }
                }

                if (hallLeftPanelsDataGridView.Rows.Count > 0)
                {
                    foreach (var rel in leftPanelMapping)
                    {
                        DataGridViewRow hallRow = hallLeftPanelsDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Key);
                        DataGridViewRow ifcRow = ifcLeftPanelsDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Value);

                        if (hallLeftPanelApertures[rel.Key] != ifcLeftPanelApertures[rel.Value])
                        {
                            // discrepancy found
                            if (leftPanelsCellColors[rel.Key] == Color.Empty)
                            {
                                // new discrepancy, generate a new random color and store it in the dictionary
                                Random rnd = new Random();
                                Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

                                leftPanelsCellColors[rel.Key] = randomColor;
                            }
                        }
                        else
                        {
                            // no discrepancy, reset cell colors to default if they are not already
                            if (leftPanelsCellColors[rel.Key] != Color.Empty)
                            {
                                leftPanelsCellColors[rel.Key] = Color.Empty;
                            }
                        }

                        // apply color to the row couple
                        hallRow.DefaultCellStyle.BackColor = leftPanelsCellColors[rel.Key];
                        ifcRow.DefaultCellStyle.BackColor = leftPanelsCellColors[rel.Key];
                    }
                }

                if (hallRightPanelsDataGridView.Rows.Count > 0)
                {
                    foreach (var rel in rightPanelMapping)
                    {
                        DataGridViewRow hallRow = hallRightPanelsDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Key);
                        DataGridViewRow ifcRow = ifcRightPanelsDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Value);

                        if (hallRightPanelApertures[rel.Key] != ifcRightPanelApertures[rel.Value])
                        {
                            // discrepancy found
                            if (rightPanelsCellColors[rel.Key] == Color.Empty)
                            {
                                // new discrepancy, generate a new random color and store it in the dictionary
                                Random rnd = new Random();
                                Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

                                rightPanelsCellColors[rel.Key] = randomColor;
                            }
                        }
                        else
                        {
                            // no discrepancy, reset cell colors to default if they are not already
                            if (rightPanelsCellColors[rel.Key] != Color.Empty)
                            {
                                rightPanelsCellColors[rel.Key] = Color.Empty;
                            }
                        }

                        // apply color to the row couple
                        hallRow.DefaultCellStyle.BackColor = rightPanelsCellColors[rel.Key];
                        ifcRow.DefaultCellStyle.BackColor = rightPanelsCellColors[rel.Key];
                    }
                }

                // identify current panel configuration in the IFC model
                IdentifyCurrentIfcPanelConfiguration();

                // redraw the global RT DataGridView so that the current configuration gets highlighted
                globalRtDataGridView.Invalidate();
            }
            catch (Exception error)
            {
                MessageBox.Show("Error while reading IFC file: " + error.Message);
            }

        }

        // auxiliary methods to retrieve and restore cell selection of a DataGridView (needed since both sorting and data refresh cause the user-defined selection to be lost)
        private List<(string rowId, int columnIndex)> GetSelectedCells(DataGridView dgv)
        {
            return dgv.SelectedCells
                .Cast<DataGridViewCell>()
                .Select(cell => (
                    rowId: dgv.Rows[cell.RowIndex].Cells[0].Value?.ToString(),
                    columnIndex: cell.ColumnIndex))
                .Where(x => x.rowId != null)
                .ToList();
        }

        private void RestoreSelectedCells(DataGridView dgv, List<(string rowId, int columnIndex)> cellSelection)
        {
            dgv.ClearSelection(); // data refresh causes the user-defined selection to be lost and the default selection to be adopted (first cell of the grid), so we wanna clear that
            if (cellSelection == null) return;

            foreach (var cell in cellSelection)
            {
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.Cells[0].Value?.ToString() == cell.rowId)
                    {
                        if (cell.columnIndex < row.Cells.Count)
                            row.Cells[cell.columnIndex].Selected = true;
                        break;
                    }
                }
            }
        }

        // auxiliary method to check if the current configuration of open/close panels in the IFC model matches one of those for which we have global RT values at disposal
        private void IdentifyCurrentIfcPanelConfiguration()
        {
            // if no panel apertures have been loaded from the IFC model, we cannot identify any configuration
            if (ifcLeftPanelApertures.Count == 0 && ifcRightPanelApertures.Count == 0)
            {
                return;
            }

            // collect all panels that are completely open
            var openPanels = new HashSet<string>(
                ifcLeftPanelApertures.Where(pair => pair.Value == maxApertureMillimiters).Select(pair => pair.Key)
                .Concat(ifcRightPanelApertures.Where(pair => pair.Value == maxApertureMillimiters).Select(pair => pair.Key))
            );

            // find the panel configuration that matches the currently open/close panels of the IFC model
            foreach (var configuration in panelConfigurations)
            {
                if (configuration.Value.SetEquals(openPanels))
                {
                    currentIfcPanelConfiguration = configuration.Key;
                    return;
                }
            }

            // the current configuration does not match any of the ones for which we have global RT values at disposal
            currentIfcPanelConfiguration = null;
        }

        // auxiliary method to convert stage equipment position from millimeters (0-maxPositionMillimeters) to meters
        public static double ConvertPositionToMeters(int positionMillimeters)
        {
            return positionMillimeters / 1000.0;
        }

        // auxiliary method to convert pivoting panel aperture angle from natural number (0-maxApertureMillimiters) to arcdegrees and arcminutes (0-maxApertureDegrees)
        public static string ConvertApertureToArcdegrees(int apertureMillimeters)
        {

            if (apertureMillimeters < 0 || apertureMillimeters > maxApertureMillimiters)
            {
                throw new ArgumentOutOfRangeException(nameof(apertureMillimeters), $"Input must be a natural number ranging from 0 to {maxApertureMillimiters}.");
            }

            double totalDegrees = (apertureMillimeters / maxApertureMillimiters) * maxApertureDegrees;
            int degrees = (int) totalDegrees;
            int arcminutes = (int) Math.Round((totalDegrees - degrees) * 60);

            if (arcminutes == 60)
            {
                degrees += 1;
                arcminutes = 0;
            }

            return $"{degrees}° {arcminutes}′";
        }

        // auxiliary method to convert pivoting panel aperture angle from natural number (0-maxApertureMillimiters) to decimal degrees (0-maxApertureDegrees)
        public static double ConvertApertureToDecimalDegrees(int apertureMillimeters)
        {
            if (apertureMillimeters < 0 || apertureMillimeters > maxApertureMillimiters)
            {
                throw new ArgumentOutOfRangeException(nameof(apertureMillimeters), $"Input must be a natural number ranging from 0 to {maxApertureMillimiters}.");
            }
                
            return (apertureMillimeters / maxApertureMillimiters) * maxApertureDegrees;
        }

        // auxiliary method to convert a pivoting panel aperture angle in decimal degrees (0-maxApertureDegrees) to a natural number (0-maxApertureMillimiters)
        public static int ConvertDecimalDegreesToAperture(double decimalDegrees)
        {
            if (decimalDegrees < 0.0 || decimalDegrees > maxApertureDegrees)
            {
                throw new ArgumentOutOfRangeException(nameof(decimalDegrees), $"Input must be a double value ranging from 0 to {maxApertureDegrees}.");
            }
                
            return (int) Math.Round((decimalDegrees / maxApertureDegrees) * maxApertureMillimiters);
        }


        // auxiliary methods for moving element tab component layout
        private void LayOutComponents_MovingElementsTab()
        {
            // layout constants
            int topMargin = 16;
            int groupTitleHeight = 30; // height reserved for group titles
            int titleSpacing = 6; // distance between group title and DataGridViews
            int labelHeight = 20;
            int labelSpacing = 4; // distance between DataGridView labels and DataGridViews
            int groupSpacing = 32; // distance between data groups
            int separatorHeight = 2; // height of the separator line
            int buttonHeight = loadHallButton.Height;
            int buttonBottomMargin = 48;
            int DataGridViewSpacing = 12; // distance between DataGridViews
            int startX = 20; // starting positions for data groups

            // computed values
            int availableWidth = movingElementsTab.ClientSize.Width - 40;
            int DataGridViewWidth = (availableWidth - 2 * DataGridViewSpacing) / 3;
            int hallGroupHeight = groupTitleHeight + titleSpacing + labelHeight + labelSpacing;
            int ifcGroupHeight = groupTitleHeight + titleSpacing + labelHeight + labelSpacing;
            int separatorMargin = groupSpacing / 2 + separatorHeight + 8;
            int totalDataGridViewHeight = movingElementsTab.ClientSize.Height - topMargin - hallGroupHeight - ifcGroupHeight - separatorMargin - buttonHeight - buttonBottomMargin;
            int DataGridViewHeight = totalDataGridViewHeight / 2; //each group takes half of the available space

            // place elements
            PlaceGroupsAndSeparator_MovingElementsTab(topMargin, groupTitleHeight, titleSpacing, labelHeight, labelSpacing, groupSpacing, separatorHeight, startX, availableWidth, hallGroupHeight, DataGridViewHeight);
            PlaceDataGridViews_MovingElementsTab(topMargin, groupTitleHeight, titleSpacing, labelHeight, labelSpacing, DataGridViewSpacing, startX, DataGridViewWidth, DataGridViewHeight, groupSpacing, separatorHeight);
            PlaceButtons_MovingElementsTab(startX, DataGridViewWidth, DataGridViewSpacing, buttonHeight);

            // adapt DataGridView columns to available width
            FitColumnsToWidth(hallStagecraftDataGridView);
            FitColumnsToWidth(hallLeftPanelsDataGridView);
            FitColumnsToWidth(hallRightPanelsDataGridView);
            FitColumnsToWidth(ifcStagecraftDataGridView);
            FitColumnsToWidth(ifcLeftPanelsDataGridView);
            FitColumnsToWidth(ifcRightPanelsDataGridView);
        }

        private void PlaceGroupsAndSeparator_MovingElementsTab(int topMargin, int groupTitleHeight, int titleSpacing, int labelHeight, int labelSpacing, int groupSpacing, int separatorHeight, int startX, int availableWidth, int hallGroupHeight, int DataGridViewHeight)
        {
            // hall group title
            hallDataGroupLabel.Left = (movingElementsTab.ClientSize.Width - hallDataGroupLabel.Width) / 2;
            hallDataGroupLabel.Top = topMargin;
            int hallDataGridViewY = topMargin + groupTitleHeight + titleSpacing + labelHeight + labelSpacing;

            // horizontal separator
            int separatorY = hallDataGridViewY + DataGridViewHeight + groupSpacing / 2;
            groupSeparator.Left = startX;
            groupSeparator.Top = separatorY;
            groupSeparator.Width = availableWidth;
            groupSeparator.Height = separatorHeight;

            // IFC group title
            int ifcStartY = separatorY + separatorHeight + 8;
            ifcDataGroupLabel.Left = (movingElementsTab.ClientSize.Width - ifcDataGroupLabel.Width) / 2;
            ifcDataGroupLabel.Top = ifcStartY;
        }

        private void PlaceDataGridViews_MovingElementsTab(int topMargin, int groupTitleHeight, int titleSpacing, int labelHeight, int labelSpacing, int DataGridViewSpacing, int startX, int DataGridViewWidth, int DataGridViewHeight, int groupSpacing, int separatorHeight)
        {
            // hall DataGridView labels
            int hallLabelY = topMargin + groupTitleHeight + titleSpacing;
            hallStagecraftDataGridViewLabel.Left = startX;
            hallStagecraftDataGridViewLabel.Top = hallLabelY;
            hallLeftPanelsDataGridViewLabel.Left = startX + DataGridViewWidth + DataGridViewSpacing;
            hallLeftPanelsDataGridViewLabel.Top = hallLabelY;
            hallRightPanelsDataGridViewLabel.Left = startX + (DataGridViewWidth + DataGridViewSpacing) * 2;
            hallRightPanelsDataGridViewLabel.Top = hallLabelY;

            // hall DataGridViews
            int hallDataGridViewY = hallLabelY + labelHeight + labelSpacing;
            hallStagecraftDataGridView.Left = startX;
            hallStagecraftDataGridView.Top = hallDataGridViewY;
            hallStagecraftDataGridView.Width = DataGridViewWidth;
            hallStagecraftDataGridView.Height = DataGridViewHeight;

            hallLeftPanelsDataGridView.Left = hallLeftPanelsDataGridViewLabel.Left;
            hallLeftPanelsDataGridView.Top = hallDataGridViewY;
            hallLeftPanelsDataGridView.Width = DataGridViewWidth;
            hallLeftPanelsDataGridView.Height = DataGridViewHeight;

            hallRightPanelsDataGridView.Left = hallRightPanelsDataGridViewLabel.Left;
            hallRightPanelsDataGridView.Top = hallDataGridViewY;
            hallRightPanelsDataGridView.Width = DataGridViewWidth;
            hallRightPanelsDataGridView.Height = DataGridViewHeight;

            // IFC DataGridView labels
            int separatorY = hallDataGridViewY + DataGridViewHeight + groupSpacing / 2;
            int ifcStartY = separatorY + separatorHeight + 8;
            int ifcLabelY = ifcStartY + groupTitleHeight + titleSpacing;
            ifcStagecraftDataGridViewLabel.Left = startX;
            ifcStagecraftDataGridViewLabel.Top = ifcLabelY;
            ifcLeftPanelsDataGridViewLabel.Left = startX + DataGridViewWidth + DataGridViewSpacing;
            ifcLeftPanelsDataGridViewLabel.Top = ifcLabelY;
            ifcRightPanelsDataGridViewLabel.Left = startX + (DataGridViewWidth + DataGridViewSpacing) * 2;
            ifcRightPanelsDataGridViewLabel.Top = ifcLabelY;

            // IFC DataGridViews
            int ifcDataGridViewY = ifcLabelY + labelHeight + labelSpacing;
            ifcStagecraftDataGridView.Left = startX;
            ifcStagecraftDataGridView.Top = ifcDataGridViewY;
            ifcStagecraftDataGridView.Width = DataGridViewWidth;
            ifcStagecraftDataGridView.Height = DataGridViewHeight;

            ifcLeftPanelsDataGridView.Left = ifcLeftPanelsDataGridViewLabel.Left;
            ifcLeftPanelsDataGridView.Top = ifcDataGridViewY;
            ifcLeftPanelsDataGridView.Width = DataGridViewWidth;
            ifcLeftPanelsDataGridView.Height = DataGridViewHeight;

            ifcRightPanelsDataGridView.Left = ifcRightPanelsDataGridViewLabel.Left;
            ifcRightPanelsDataGridView.Top = ifcDataGridViewY;
            ifcRightPanelsDataGridView.Width = DataGridViewWidth;
            ifcRightPanelsDataGridView.Height = DataGridViewHeight;
        }

        private void PlaceButtons_MovingElementsTab(int startX, int DataGridViewWidth, int DataGridViewSpacing, int buttonHeight)
        {
            // horizontally centering buttons and keeping them one beside the other at the bottom of the form
            int buttonSpacing = 16; // distance between buttons
            int buttonsTotalWidth = loadHallButton.Width + buttonSpacing + loadIfcButton.Width + buttonSpacing + updateIfcButton.Width;
            int buttonsStartX = (movingElementsTab.ClientSize.Width - buttonsTotalWidth) / 2;

            int dataGridViewsBottom = ifcStagecraftDataGridView.Top + ifcStagecraftDataGridView.Height;
            int tabPageBottom = movingElementsTab.Height; // bottom end of TabPage
            int buttonsY = dataGridViewsBottom + (tabPageBottom - dataGridViewsBottom - buttonHeight) / 2; // bottons in between DataGridViews end and form bottom end

            loadHallButton.Left = buttonsStartX;
            loadHallButton.Top = buttonsY;

            loadIfcButton.Left = buttonsStartX + loadHallButton.Width + buttonSpacing;
            loadIfcButton.Top = buttonsY;

            updateIfcButton.Left = loadIfcButton.Left + loadIfcButton.Width + buttonSpacing;
            updateIfcButton.Top = buttonsY;
        }

        // auxiliary method for acoustics tab component layout
        private void LayOutComponents_AcousticsTab()
        {
            int margin = 20;
            int labelHeight = 28;
            int labelSpacing = 8;

            // place title
            globalRtDataGridViewLabel.Top = margin;
            globalRtDataGridViewLabel.Height = labelHeight;
            globalRtDataGridViewLabel.Left = (acousticsTab.Width - globalRtDataGridViewLabel.Width) / 2;
            globalRtDataGridViewLabel.TextAlign = ContentAlignment.MiddleCenter;

            // place DataGridView
            globalRtDataGridView.Left = margin;
            globalRtDataGridView.Top = globalRtDataGridViewLabel.Bottom + labelSpacing;
            globalRtDataGridView.Width = acousticsTab.Width - 2 * margin;

            // fit DataGridView height to content
            int totalRowHeight = 0;
            for (int i = 0; i < globalRtDataGridView.Rows.Count; i++)
                totalRowHeight += globalRtDataGridView.Rows[i].Height;

            int headerHeight = globalRtDataGridView.ColumnHeadersVisible ? globalRtDataGridView.ColumnHeadersHeight : 0;

            globalRtDataGridView.Height = totalRowHeight + headerHeight;
        }

        // auxiliary method to fit DataGridView columns to available width
        private void FitColumnsToWidth(DataGridView dgv)
        {
            if (dgv.ColumnCount == 0)
                return;

            int totalGridWidth = dgv.ClientSize.Width;
            int scrollbarWidth = dgv.Controls.OfType<VScrollBar>().FirstOrDefault()?.Visible == true ? SystemInformation.VerticalScrollBarWidth : 0;
            int availableGridWidth = totalGridWidth - scrollbarWidth;

            // uniform column width
            int columnWidth = availableGridWidth / dgv.ColumnCount;
            for (int i = 0; i < dgv.ColumnCount; i++)
            {
                // last column takes the remaining space, to avoid approximation errors
                if (i == dgv.ColumnCount - 1)
                    dgv.Columns[i].Width = availableGridWidth - columnWidth * (dgv.ColumnCount - 1);
                else
                    dgv.Columns[i].Width = columnWidth;
            }
        }


        // override standard form methods
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // read INI file
            if (File.Exists(iniPath))
            {
                var iniFile = new IniFile(iniPath);

                // hall file check parameters
                hallFileCheckDirectory = iniFile.Read("HallFileCheck", "Directory");
                hallFileCheckName = iniFile.Read("HallFileCheck", "FileName");
                int.TryParse(iniFile.Read("HallFileCheck", "Interval"), out hallFileCheckInterval);
                bool.TryParse(iniFile.Read("HallFileCheck", "Active"), out hallFileCheckActive);

                // IFC file check parameters
                ifcFileCheckDirectory = iniFile.Read("IfcFileCheck", "Directory");
                ifcFileCheckName = iniFile.Read("IfcFileCheck", "FileName");
                int.TryParse(iniFile.Read("IfcFileCheck", "Interval"), out ifcFileCheckInterval);
                bool.TryParse(iniFile.Read("IfcFileCheck", "Active"), out ifcFileCheckActive);
            }

            // initialize timers
            InitTimers();

            // hard-coded correspondences between hall IDs and IFC model element names
            stagecraftEquipmentMapping.Add("TM.001", "Truss Motor 1"); // hypothetical IFC element name
            stagecraftEquipmentMapping.Add("TM.002", "Truss Motor 2"); // hypothetical IFC element name
            stagecraftEquipmentMapping.Add("TM.003", "Truss Motor 3"); // hypothetical IFC element name
            stagecraftEquipmentMapping.Add("TM.004", "Truss Motor 4"); // hypothetical IFC element name
            stagecraftEquipmentMapping.Add("TS.001", "Truss Stopper 1"); // hypothetical IFC element name
            stagecraftEquipmentMapping.Add("TP.001", "Truss Position 1"); // hypothetical IFC element name
            stagecraftEquipmentMapping.Add("TP.002", "Truss Position 2"); // hypothetical IFC element name
            stagecraftEquipmentMapping.Add("TP.003", "Truss Position 3"); // hypothetical IFC element name
            stagecraftEquipmentMapping.Add("TP.004", "Truss Position 4"); // hypothetical IFC element name
            stagecraftEquipmentMapping.Add("TT.001", "Truss Tension 1"); // hypothetical IFC element name
            stagecraftEquipmentMapping.Add("TD.001", "Truss Distance 1"); // hypothetical IFC element name
            stagecraftEquipmentMapping.Add("TA.001", "Truss Angle 1"); // hypothetical IFC element name
            stagecraftEquipmentMapping.Add("PO.001", "Platform Opening 1"); // hypothetical IFC element name

            leftPanelMapping.Add("PS.001", "Left Panel 1"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.002", "Left Panel 2"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.003", "Left Panel 3"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.004", "Left Panel 4"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.005", "Left Panel 5"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.006", "Left Panel 6"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.007", "Left Panel 7"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.008", "Left Panel 8"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.009", "Left Panel 9"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.010", "Left Panel 10"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.011", "Left Panel 11"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.012", "Left Panel 12"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.013", "Left Panel 13"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.014", "Left Panel 14"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.015", "Left Panel 15"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.016", "Left Panel 16"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.017", "Left Panel 17"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.018", "Left Panel 18"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.019", "Left Panel 19"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.020", "Left Panel 20"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.021", "Left Panel 21"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.022", "Left Panel 22"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.023", "Left Panel 23"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.024", "Left Panel 24"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.025", "Left Panel 25"); // hypothetical IFC element name
            leftPanelMapping.Add("PS.026", "Left Panel 26"); // hypothetical IFC element name

            rightPanelMapping.Add("PD.001", "Right Panel 1"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.002", "Right Panel 2"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.003", "Right Panel 3"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.004", "Right Panel 4"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.005", "Right Panel 5"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.006", "Right Panel 6"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.007", "Right Panel 7"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.008", "Right Panel 8"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.009", "Right Panel 9"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.010", "Right Panel 10"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.011", "Right Panel 11"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.012", "Right Panel 12"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.013", "Right Panel 13"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.014", "Right Panel 14"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.015", "Right Panel 15"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.016", "Right Panel 16"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.017", "Right Panel 17"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.018", "Right Panel 18"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.019", "Right Panel 19"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.020", "Right Panel 20"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.021", "Right Panel 21"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.022", "Right Panel 22"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.023", "Right Panel 23"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.024", "Right Panel 24"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.025", "Right Panel 25"); // hypothetical IFC element name
            rightPanelMapping.Add("PD.026", "Right Panel 26"); // hypothetical IFC element name


            // initialize global RT values DataGridView
            var globalRtDataTable = new DataTable();
            globalRtDataTable.Columns.Add("Configuration");
            globalRtDataTable.Columns.Add("125 Hz");
            globalRtDataTable.Columns.Add("250 Hz");
            globalRtDataTable.Columns.Add("500 Hz");
            globalRtDataTable.Columns.Add("1000 Hz");
            globalRtDataTable.Columns.Add("2000 Hz");
            globalRtDataTable.Columns.Add("4000 Hz");

            var globalRtValues = new Dictionary<string, double[]>
            {
                {"CLOSED", new[] {2.09, 2.02, 1.90, 1.95, 1.92, 1.69}},
                {"0C", new[] {2.07, 2.00, 1.89, 1.94, 1.91, 1.69}},
                {"1F", new[] {2.07, 2.00, 1.89, 1.92, 1.90, 1.68}},
                {"1C", new[] {2.06, 1.99, 1.88, 1.91, 1.89, 1.67}},
                {"1FC", new[] {2.06, 1.99, 1.88, 1.88, 1.88, 1.66}},
                {"1P", new[] {2.06, 1.99, 1.88, 1.92, 1.90, 1.68}},
                {"1FCP", new[] {2.03, 1.96, 1.85, 1.86, 1.85, 1.65}},
                {"2F", new[] {2.06, 1.99, 1.88, 1.93, 1.91, 1.69}},
                {"2C", new[] {2.05, 1.98, 1.87, 1.91, 1.89, 1.68}},
                {"2FCP", new[] {2.02, 1.95, 1.84, 1.88, 1.86, 1.67}},
                {"OPEN", new[] {1.96, 1.89, 1.78, 1.81, 1.81, 1.62}}
            };

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

            globalRtDataGridView.DataSource = globalRtDataTable;


            // lay form elements out
            LayOutComponents_MovingElementsTab();
            LayOutComponents_AcousticsTab();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayOutComponents_MovingElementsTab();
            LayOutComponents_AcousticsTab();
        }
    }
}