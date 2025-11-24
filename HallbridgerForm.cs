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
        private Dictionary<string, int> hallStagecraftEquipmentPositions = new Dictionary<string, int>();
        private Dictionary<string, int> hallLeftPanelApertures = new Dictionary<string, int>();
        private Dictionary<string, int> hallRightPanelApertures = new Dictionary<string, int>();

        // class-level dictionaries to store moving element data coming from the 3D model in
        private Dictionary<string, int> hall3dModelStagecraftEquipmentPositions = new Dictionary<string, int>();
        private Dictionary<string, int> hall3dModelLeftPanelApertures = new Dictionary<string, int>();
        private Dictionary<string, int> hall3dModelRightPanelApertures = new Dictionary<string, int>();

        // class-level dictionaries to store correspondences between hall IDs and 3D model element names
        private readonly Dictionary<string, string> stagecraftEquipmentMapping = new Dictionary<string, string>();
        private readonly Dictionary<string, string> leftPanelMapping = new Dictionary<string, string>();
        private readonly Dictionary<string, string> rightPanelMapping = new Dictionary<string, string>();

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

        // class-level variable to keep track of the current open/close panel configuration of the 3D model
        private string currentHall3dModelPanelConfiguration = null;

        // class-level variables to parametrize maximum panel aperture values
        private static double maxApertureDegrees = 30.0;
        private static double maxPositionMillimeters, maxApertureMillimiters = 99999.0;

        // class-level dictionary to store selected cells in DataGridViews (needed since both sorting and data refresh cause the user-defined selection to be lost)
        Dictionary<DataGridView, List<(string RowId, int ColumnIndex)>> selectedCells = new Dictionary<DataGridView, List<(string RowId, int ColumnIndex)>>();


        // INI file and timers for automatic file check and usage
        private readonly string iniPath = "D:\\Dateien\\Hallbridger\\Configuration\\conf.ini";

        private System.Windows.Forms.Timer hallDataFileCheckTimer;
        private string hallDataFileCheckDirectory = "D:\\Dateien\\Hallbridger\\IO_files"; // default directory, can be changed in INI file
        private string hallDataFileCheckName = "Fotografia_sala_CURIO.txt"; // default file name, can be changed in INI file
        private int hallDataFileCheckInterval = 5000; // default to 5 seconds, can be changed in INI file
        private bool hallDataFileCheckActive = true; // default to true, can be changed in INI file

        private System.Windows.Forms.Timer hall3dModelFileCheckTimer;
        private string hall3dModelFileCheckDirectory = "D:\\Dateien\\Hallbridger\\IO_files"; // default directory, can be changed in INI file
        private string hall3dModelFileCheckName = "Model.ifc"; // default file name, can be changed in INI file
        private int hall3dModelFileCheckInterval = 5000; // default to 5 seconds, can be changed in INI file
        private bool hall3dModelFileCheckActive = true; // default to true, can be changed in INI file


        public HallbridgerForm()
        {
            InitializeComponent();
            this.Icon = new Icon("icon.ico");
        }


        // timer methods
        protected void InitTimers()
        {
            // initialize hall file check timer if active
            if (hallDataFileCheckActive)
            {
                hallDataFileCheckTimer = new System.Windows.Forms.Timer()
                {
                    Interval = hallDataFileCheckInterval
                };
                hallDataFileCheckTimer.Tick += HallDataFileCheckTimer_Tick;
                hallDataFileCheckTimer.Start();
            }

            // initialize 3D model file check timer if active
            if (hall3dModelFileCheckActive)
            {
                hall3dModelFileCheckTimer = new System.Windows.Forms.Timer()
                {
                    Interval = hall3dModelFileCheckInterval
                };
                hall3dModelFileCheckTimer.Tick += Hall3dModelFileCheckTimer_Tick;
                hall3dModelFileCheckTimer.Start();
            }
        }

        private async void HallDataFileCheckTimer_Tick(object sender, EventArgs e)
        {
            string hallDataFilePath = Path.Combine(hallDataFileCheckDirectory, hallDataFileCheckName);
            if (File.Exists(hallDataFilePath))
            {
                string apiEndpoint;
                string hallDataFileExtension = Path.GetExtension(hallDataFilePath).ToLowerInvariant();

                switch (hallDataFileExtension)
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
                        MessageBox.Show("File extension not supported: " + hallDataFileExtension);
                        return;
                }

                // import data from the real hall and display them in the corresponding DataGridViews
                await LoadHallData(apiEndpoint, hallDataFilePath);

                ViewData(hallStagecraftDataGridView, hallStagecraftEquipmentPositions, hall3dModelStagecraftDataGridView, hall3dModelStagecraftEquipmentPositions, stagecraftEquipmentMapping, ConvertPositionToMeters, unitOfMeasurement: " m");
                ViewData(hallLeftPanelsDataGridView, hallLeftPanelApertures, hall3dModelLeftPanelsDataGridView, hall3dModelLeftPanelApertures, leftPanelMapping, ConvertApertureToArcdegrees);
                ViewData(hallRightPanelsDataGridView, hallRightPanelApertures, hall3dModelRightPanelsDataGridView, hall3dModelRightPanelApertures, rightPanelMapping, ConvertApertureToArcdegrees);
            }
        }

        private void Hall3dModelFileCheckTimer_Tick(object sender, EventArgs e)
        {
            string hall3dModelFilePath = Path.Combine(hall3dModelFileCheckDirectory, hall3dModelFileCheckName);
            if (File.Exists(hall3dModelFilePath))
            {
                string hall3dModelFileExtension = Path.GetExtension(hall3dModelFilePath).ToLowerInvariant();

                switch (hall3dModelFileExtension)
                {
                    case ".ifc":
                        break;
                    default:
                        MessageBox.Show("File extension not supported: " + hall3dModelFileExtension);
                        return;
                }

                // import data from the 3D model and display them in the corresponding DataGridViews
                LoadHall3dModel(hall3dModelFilePath);

                ViewData(hall3dModelStagecraftDataGridView, hall3dModelStagecraftEquipmentPositions, hallStagecraftDataGridView, hallStagecraftEquipmentPositions, stagecraftEquipmentMapping, ConvertPositionToMeters, unitOfMeasurement: " m");
                ViewData(hall3dModelLeftPanelsDataGridView, hall3dModelLeftPanelApertures, hallLeftPanelsDataGridView, hallLeftPanelApertures, leftPanelMapping, ConvertApertureToArcdegrees);
                ViewData(hall3dModelRightPanelsDataGridView, hall3dModelRightPanelApertures, hallRightPanelsDataGridView, hallRightPanelApertures, rightPanelMapping, ConvertApertureToArcdegrees);

                // identify current panel configuration in the 3D model, if possible
                IdentifyCurrentHall3dModelPanelConfiguration();

                // redraw the global RT DataGridView so that the current configuration gets highlighted, if it could be identified
                globalRtDataGridView.Invalidate();
            }
        }


        // button methods
        private async void LoadHallDataButton_OnClick(object sender, EventArgs e)
        {
            using (OpenFileDialog loadHallDataDialog = new OpenFileDialog())
            {
                loadHallDataDialog.Filter = "Text files (*.txt)|*.txt|Excel spreadsheet (*.xls;*.xlsx)|*.xls;*.xlsx|JavaScript Object Notation file (*.json)|*.json|eXtensible Markup Language file (*.xml)|*.xml| All files (*.*)|*.*";
                if (loadHallDataDialog.ShowDialog() == DialogResult.OK)
                {
                    string apiEndpoint;
                    string hallDataFileExtension = Path.GetExtension(loadHallDataDialog.FileName).ToLowerInvariant();

                    switch (hallDataFileExtension)
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
                            MessageBox.Show("File extension not supported: " + hallDataFileExtension);
                            return;
                    }

                    // import data from the real hall and display them in the corresponding DataGridViews
                    await LoadHallData(apiEndpoint, loadHallDataDialog.FileName);

                    ViewData(hallStagecraftDataGridView, hallStagecraftEquipmentPositions, hall3dModelStagecraftDataGridView, hall3dModelStagecraftEquipmentPositions, stagecraftEquipmentMapping, ConvertPositionToMeters, unitOfMeasurement: " m");
                    ViewData(hallLeftPanelsDataGridView, hallLeftPanelApertures, hall3dModelLeftPanelsDataGridView, hall3dModelLeftPanelApertures, leftPanelMapping, ConvertApertureToArcdegrees);
                    ViewData(hallRightPanelsDataGridView, hallRightPanelApertures, hall3dModelRightPanelsDataGridView, hall3dModelRightPanelApertures, rightPanelMapping, ConvertApertureToArcdegrees);
                }
            }
        }

        private void LoadHall3dModelButton_OnClick(object sender, EventArgs e)
        {
            using (OpenFileDialog loadHall3dModelDialog = new OpenFileDialog())
            {
                loadHall3dModelDialog.Filter = "3D model file (*.ifc)|*.ifc|All files (*.*)|*.*";
                if (loadHall3dModelDialog.ShowDialog() == DialogResult.OK)
                {
                    string hall3dModelFilePath = loadHall3dModelDialog.FileName;
                    string hall3dModelFileExtension = Path.GetExtension(hall3dModelFilePath).ToLowerInvariant();

                    switch (hall3dModelFileExtension)
                    {
                        case ".ifc":
                            break;
                        default:
                            MessageBox.Show("File extension not supported: " + hall3dModelFileExtension);
                            return;
                    }

                    // import data from the 3D model and display them in the corresponding DataGridViews
                    LoadHall3dModel(hall3dModelFilePath);

                    ViewData(hall3dModelStagecraftDataGridView, hall3dModelStagecraftEquipmentPositions, hallStagecraftDataGridView, hallStagecraftEquipmentPositions, stagecraftEquipmentMapping, ConvertPositionToMeters, unitOfMeasurement: " m");
                    ViewData(hall3dModelLeftPanelsDataGridView, hall3dModelLeftPanelApertures, hallLeftPanelsDataGridView, hallLeftPanelApertures, leftPanelMapping, ConvertApertureToArcdegrees);
                    ViewData(hall3dModelRightPanelsDataGridView, hall3dModelRightPanelApertures, hallRightPanelsDataGridView, hallRightPanelApertures, rightPanelMapping, ConvertApertureToArcdegrees);

                    // identify current panel configuration in the 3D model, if possible
                    IdentifyCurrentHall3dModelPanelConfiguration();

                    // redraw the global RT DataGridView so that the current configuration gets highlighted, if it could be identified
                    globalRtDataGridView.Invalidate();
                }
            }
        }

        private void UpdateHall3dModelButton_OnClick(object sender, EventArgs e)
        {
            // first, check if at least some hall data have been previously loaded
            if (hallStagecraftEquipmentPositions.Count == 0 && hallLeftPanelApertures.Count == 0 && hallRightPanelApertures.Count == 0) // no hall data loaded at all
            {
                MessageBox.Show("Please load real hall data before updating the 3D model.");
                return;
            }

            using (OpenFileDialog updateHall3dModelDialog = new OpenFileDialog())
            {
                updateHall3dModelDialog.Filter = "3D model file (*.ifc)|*.ifc|All files (*.*)|*.*";
                if (updateHall3dModelDialog.ShowDialog() == DialogResult.OK)
                {
                    string hall3dModelFilePath = updateHall3dModelDialog.FileName;
                    string hall3dModelFileExtension = Path.GetExtension(hall3dModelFilePath).ToLowerInvariant();

                    switch (hall3dModelFileExtension)
                    {
                        case ".ifc":
                            break;
                        default:
                            MessageBox.Show("File extension not supported: " + hall3dModelFileExtension);
                            return;
                    }

                    try
                    {
                        var xbimEditor = new XbimEditorCredentials
                        {
                            ApplicationDevelopersName = "Jacopo Fantin",
                            ApplicationFullName = "Hallbridger",
                            ApplicationIdentifier = "Hallbridger",
                            ApplicationVersion = "1.0",
                            EditorsFamilyName = "Fantin",
                            EditorsGivenName = "Jacopo",
                            EditorsOrganisationName = "Politecnico di Milano"
                        };

                        // open the 3D model
                        using (var model = IfcStore.Open(hall3dModelFilePath, xbimEditor))
                        {
                            // start a transaction to modify the 3D model for the panel aperture
                            using (var panelApertureUpdate = model.BeginTransaction("Pivoting panel aperture update"))
                            {
                                var firstPanel = model.Instances.OfType<IIfcFurnishingElement>().FirstOrDefault();
                                var rotationAngleProperty = (firstPanel?.IsTypedBy
                                    .FirstOrDefault(type => type.RelatingType.Name == "PANEL ROTATION:PANEL ROTATION 60")?.RelatingType as IIfcTypeObject)?
                                    .HasPropertySets.OfType<IIfcPropertySet>()
                                    .FirstOrDefault(pset => pset.Name == "Quote")?
                                    .HasProperties.OfType<IIfcPropertySingleValue>()
                                    .FirstOrDefault(prop => prop.Name == "PANEL ANGLE");

                                // writes the new aperture of the panel in the 3D model
                                rotationAngleProperty.NominalValue = new Xbim.Ifc4.MeasureResource.IfcPlaneAngleMeasure(22.79);

                                // commit the changes and save the 3D model
                                panelApertureUpdate.Commit();
                                model.SaveAs(hall3dModelFilePath);
                            }
                            /*
                            // start a transaction to modify the 3D model for right wall pivoting panel apertures
                            using (var rightPanelApertureUpdate = model.BeginTransaction("Right wall pivoting panel aperture update"))
                            {
                                //... extract right panel information into IEnumerable hall3dModelRightPanels like we already did for the loadHall3dModelButton...

                                //hall3dModelRightPanels: IEnumerable, rightPanelApertures: list
                                //to be used if the model does not have stagecraft equipment in it: in that case, it makes most sense to use lists for data imported from TXT since panels are identified from number 1 to 26 (we can forget about their IDs)
                                for (int i = 0; i < rightPanelApertures.Count; i++)
                                {
                                    var rightPanel = hall3dModelRightPanels.ElementAt(i);

                                    var aperture = rightPanel.IsDefinedBy
                                        .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                                        .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                                        .OfType<IIfcPropertySingleValue>()
                                        .FirstOrDefault(p => p.Name = "Aperture");

                                    // writes the aperture of each panel on the right wall in the 3D model
                                    aperture.NominalValue = new Xbim.Ifc4.MeasureResource.IfcLengthMeasure(rightPanelApertures[i]);
                                }

                                //hall3dModelRightPanels: IEnumerable, rightPanelMapping: dictionary<string, int> where key is the hall ID of the right wall panel and value is the index of the corresponding panel in hall3dModelRightPanels
                                //to be used if the model actually has stagecraft equipment in it: in that case, we must use a dictionary for its information and in this case we wanna use dictionaries for all of the data we import from TXT
                                foreach (var corr in rightPanelMapping)
                                {
                                    var rightPanel = hall3dModelRightPanels.ElementAt(corr.Value);

                                    var aperture = rightPanel.IsDefinedBy
                                        .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                                        .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                                        .OfType<IIfcPropertySingleValue>()
                                        .FirstOrDefault(p => p.Name = "Aperture");

                                    // writes the aperture of each panel on the right wall in the 3D model
                                    aperture.NominalValue = new Xbim.Ifc4.MeasureResource.IfcLengthMeasure(rightPanelApertures[corr.Key]);
                                }

                                // commit the changes to the 3D model
                                rightPanelApertureUpdate.Commit();
                            }

                            // start a transaction to modify the 3D model for stagecraft equipment positions
                            using (var stagecraftEquipmentPositionsUpdate = model.BeginTransaction("Stagecraft equipment position update"))
                            {
                                //... extract stagecraft equipment information into IEnumerable hall3dModelStagecraftEquipment like we already did for the loadHall3dModelButton...

                                //hall3dModelStagecraftEquipment: IEnumerable, stagecraftEquipmentMapping: dictionary<string, int> where key is the hall ID of the stagecraft equipment piece and value is the index of the corresponding piece in hall3dModelStagecraftEquipment
                                foreach (var corr in stagecraftEquipmentMapping)
                                {
                                    var piece = hall3dModelStagecraftEquipment.ElementAt(corr.Value);

                                    var position = piece.IsDefinedBy
                                        .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                                        .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                                        .OfType<IIfcPropertySingleValue>()
                                        .FirstOrDefault(p => p.Name = "Position");

                                    // writes the position of each stagecraft equipment piece in the 3D model
                                    position.NominalValue = new Xbim.Ifc4.MeasureResource.IfcLengthMeasure(stagecraftEquipmentPositions[corr.Key]);
                                }

                                // commit the changes to the 3D model
                                stagecraftEquipmentPositionsUpdate.Commit();
                            }*/
                        }
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show("Error while reading or modifying 3D model: " + error.Message);
                    }

                    // after updating the 3D model, reload it and update the displayed data
                    LoadHall3dModel(hall3dModelFilePath);

                    ViewData(hall3dModelStagecraftDataGridView, hall3dModelStagecraftEquipmentPositions, hallStagecraftDataGridView, hallStagecraftEquipmentPositions, stagecraftEquipmentMapping, ConvertPositionToMeters, unitOfMeasurement: " m");
                    ViewData(hall3dModelLeftPanelsDataGridView, hall3dModelLeftPanelApertures, hallLeftPanelsDataGridView, hallLeftPanelApertures, leftPanelMapping, ConvertApertureToArcdegrees);
                    ViewData(hall3dModelRightPanelsDataGridView, hall3dModelRightPanelApertures, hallRightPanelsDataGridView, hallRightPanelApertures, rightPanelMapping, ConvertApertureToArcdegrees);

                    // identify current panel configuration in the 3D model
                    IdentifyCurrentHall3dModelPanelConfiguration();

                    // redraw the global RT DataGridView so that the current configuration gets highlighted
                    globalRtDataGridView.Invalidate();
                }
            }
        }


        // other event methods
        // DataGridView column header methods
        private void DataGridView_ColumnHeaderMouseDown(object sender, MouseEventArgs e)
        {
            if (sender is DataGridView dgv)
            {
                var hit = dgv.HitTest(e.X, e.Y);
                if (hit.Type == DataGridViewHitTestType.ColumnHeader)
                {
                    selectedCells[dgv] = GetCellSelection(dgv);
                }
            }
        }

        private void DataGridView_Sorted(object sender, EventArgs e)
        {
            if (sender is DataGridView dgv)
            {
                if (selectedCells.TryGetValue(dgv, out var cellSelection))
                {
                    SetCellSelection(dgv, cellSelection);
                }
            }
        }

        // tab control method
        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            globalRtDataGridView.ClearSelection();
        }

        // post-paint method to highlight in the global RT DataGridView the current open/close panel configuration of the 3D model (gets called after each row is painted)
        private void GlobalRtDataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv == null || string.IsNullOrEmpty(currentHall3dModelPanelConfiguration))
            {
                return;
            }

            // if the row that has just been painted corresponds to the current open/close panel configuration of the 3D model, draw a red rectangle around it
            var configuration = dgv.Rows[e.RowIndex].Cells["Configuration"].Value?.ToString();
            if (configuration == currentHall3dModelPanelConfiguration)
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
        private async Task LoadHallData(string apiUrl, string filePath)
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

                // empty and refill the class-level dictionaries
                hallStagecraftEquipmentPositions.Clear();
                hallStagecraftEquipmentPositions = data.StagecraftEquipmentPositions;

                hallLeftPanelApertures.Clear();
                hallLeftPanelApertures = data.LeftPanelApertures;

                hallRightPanelApertures.Clear();
                hallRightPanelApertures = data.RightPanelApertures;
            }
        }

        // method to import 3D model data
        private void LoadHall3dModel(string filePath)
        {
            try
            {
                // empty the class-level dictionaries
                hall3dModelStagecraftEquipmentPositions.Clear();
                hall3dModelLeftPanelApertures.Clear();
                hall3dModelRightPanelApertures.Clear();

                using (var model = IfcStore.Open(filePath))
                {
                    //extract a list of panel blocks from the 3D model
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
                    foreach (var leftPanel in hall3dModelLeftPanels)
                    {
                        var aperture = leftPanel.IsDefinedBy
                            .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                            .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                            .OfType<IIfcPropertySingleValue>()
                            .Where(p => p.Name = "Aperture");

                        hall3dModelLeftPanelApertures.Add(leftPanel.Name, int.Parse(aperture.NominalValue));
                    }

                    foreach (var rightPanel in hall3dModelRightPanels)
                    {
                        var aperture = rightPanel.IsDefinedBy
                            .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                            .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                            .OfType<IIfcPropertySingleValue>()
                            .Where(p => p.Name = "Aperture");

                        hall3dModelRightPanelApertures.Add(rightPanel.Name, int.Parse(aperture.NominalValue));
                    }

                    // store stagecraft equipment names as keys and positions as values in the class-level dictionary
                    foreach (var piece in hall3dModelStagecraftEquipment)
                    {
                        var position = piece.IsDefinedBy
                            .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                            .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                            .OfType<IIfcPropertySingleValue>()
                            .Where(p => p.Name = "Position");

                        hall3dModelStagecraftEquipmentPositions.Add(piece.Name, int.Parse(position.NominalValue));
                    }*/
                }
            }
            catch (Exception error)
            {
                MessageBox.Show("Error while reading 3D model: " + error.Message);
            }
        }

        // method to populate a DataGridView
        // dgv: DataGridView to populate; data: dictionary with integer values to be displayed; compareDgv: DataGridView with data to compare with; compareData: dictionary with integer values to compare with; dataMap: dictionary with correspondences between keys of data and compareData; conversionMethod: method to convert integer values to desired type T for display; unitOfMeasurement: optional string to append to displayed values, remember to include a whitespace before the symbol as the string is going to be appended right after the measure (conversion methods yielding string values should already include the unit of measurement, so in this case this parameter should not be used)
        private void ViewData<T>(DataGridView dgv, Dictionary<string, int> data, DataGridView compareDgv, Dictionary<string, int> compareData, Dictionary<string, string> dataMap, Func<int, T> conversionMethod, string unitOfMeasurement = "")
        {
            // save current data sorting
	        DataGridViewColumn dataGridViewSortedColumn = dgv.SortedColumn;
            ListSortDirection? dataGridViewSortDirection = null;
            if (dataGridViewSortedColumn != null)
            {
                dataGridViewSortDirection = dgv.SortOrder == SortOrder.Descending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }

            // save current scroll position
            int dataGridViewTopRowIndex = dgv.FirstDisplayedScrollingRowIndex;

            // save current cell selection
            selectedCells[dgv] = GetCellSelection(dgv);

            // save current cell background colors
            Dictionary<string, Color> dataGridViewCellColors = GetCellBackgroundColors(dgv);

            // empty and refill the DataGridView
            dgv.Rows.Clear();
            foreach (var pair in data)
            {
                dgv.Rows.Add(pair.Key, $"{conversionMethod(pair.Value)}{unitOfMeasurement}");
            }

            // restore previous data sorting
            if (dataGridViewSortedColumn != null && dataGridViewSortDirection.HasValue)
            {
                dgv.Sort(dataGridViewSortedColumn, dataGridViewSortDirection.Value);
            }

            // restore previous scroll position
            if (dataGridViewTopRowIndex >= 0)
            {
                dgv.FirstDisplayedScrollingRowIndex = dataGridViewTopRowIndex;
            }

            // restore previous cell selection
            if (selectedCells.TryGetValue(dgv, out var cellSelection))
            {
                SetCellSelection(dgv, cellSelection);
            }

            // if hall/3D model data have already been loaded, highlight discrepancies between hall and 3D model data
            SetCellBackgroundColors(dgv, data, compareDgv, compareData, dataMap, dataGridViewCellColors);
        }

        // auxiliary methods (getters and setters) to save and restore DataGridView state
        private void GetDataSorting(DataGridView dgv, out DataGridViewColumn sortingColumn, out ListSortDirection? sortDirection)
        {
            sortingColumn = dgv.SortedColumn;
            sortDirection = null;
            if (sortingColumn != null)
            {
                sortDirection = dgv.SortOrder == SortOrder.Descending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }
        }

        private int GetScrollPositions(DataGridView dgv)
        {
            return dgv.FirstDisplayedScrollingRowIndex;
        }

        private List<(string rowId, int columnIndex)> GetCellSelection(DataGridView dgv)
        {
            return dgv.SelectedCells
                .Cast<DataGridViewCell>()
                .Select(cell => (
                    rowId: dgv.Rows[cell.RowIndex].Cells[0].Value?.ToString(),
                    columnIndex: cell.ColumnIndex))
                .Where(x => x.rowId != null)
                .ToList();
        }

        private Dictionary<string, Color> GetCellBackgroundColors(DataGridView dgv)
        {
            Dictionary<string, Color> cellColors = new Dictionary<string, Color>();
            foreach (DataGridViewRow row in dgv.Rows)
            {
                string rowId = row.Cells[0].Value?.ToString();
                cellColors[rowId] = row.Cells[0].Style.BackColor;
            }

            return cellColors;
        }

        private void SetDataSorting(DataGridView dgv, DataGridViewColumn sortingColumn, ListSortDirection? sortDirection)
        {
            if (sortingColumn != null && sortDirection.HasValue)
            {
                dgv.Sort(sortingColumn, sortDirection.Value);
            }
        }

        private void SetScrollPosition(DataGridView dgv, int topRowIndex)
        {
            if (topRowIndex >= 0)
            {
                dgv.FirstDisplayedScrollingRowIndex = topRowIndex;
            }
        }

        private void SetCellSelection(DataGridView dgv, List<(string rowId, int columnIndex)> cellSelection)
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

        private void SetCellBackgroundColors(DataGridView dgv, Dictionary<string, int> data, DataGridView cmpDgv, Dictionary<string, int> cmpData, Dictionary<string, string> dataMap, Dictionary<string, Color> cellColors)
        {
            if (cmpDgv.Rows.Count > 0)
            {
                foreach (var rel in dataMap)
                {
                    DataGridViewRow row = dgv.Rows
                        .Cast<DataGridViewRow>()
                        .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Key);
                    DataGridViewRow cmpRow = cmpDgv.Rows
                        .Cast<DataGridViewRow>()
                        .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Value);

                    if (data[rel.Key] != cmpData[rel.Value])
                    {
                        // discrepancy found
                        if (cellColors[rel.Key] == Color.Empty)
                        {
                            // new discrepancy, generate a new random color and store it in the dictionary
                            Random rnd = new Random();
                            Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

                            cellColors[rel.Key] = randomColor;
                        }
                    }
                    else
                    {
                        // no discrepancy, reset cell colors to default if they are not already
                        if (cellColors[rel.Key] != Color.Empty)
                        {
                            cellColors[rel.Key] = Color.Empty;
                        }
                    }

                    // apply color to the row couple
                    row.DefaultCellStyle.BackColor = cellColors[rel.Key];
                    cmpRow.DefaultCellStyle.BackColor = cellColors[rel.Key];
                }
            }
        }

        // auxiliary method to check if the current configuration of open/close panels in the 3D model matches one of those for which we have global RT values at disposal
        private void IdentifyCurrentHall3dModelPanelConfiguration()
        {
            // if no panel apertures have been loaded from the 3D model, we cannot identify any configuration
            if (hall3dModelLeftPanelApertures.Count == 0 && hall3dModelRightPanelApertures.Count == 0)
            {
                return;
            }

            // collect all panels that are completely open
            var openPanels = new HashSet<string>(
                hall3dModelLeftPanelApertures.Where(pair => pair.Value == maxApertureMillimiters).Select(pair => pair.Key)
                .Concat(hall3dModelRightPanelApertures.Where(pair => pair.Value == maxApertureMillimiters).Select(pair => pair.Key))
            );

            // find the panel configuration that matches the currently open/close panels of the 3D model
            foreach (var configuration in panelConfigurations)
            {
                if (configuration.Value.SetEquals(openPanels))
                {
                    currentHall3dModelPanelConfiguration = configuration.Key;
                    return;
                }
            }

            // if we haven't find one, the current configuration does not match any of those for which we have global RT values at disposal
            currentHall3dModelPanelConfiguration = null;
        }


        // auxiliary conversion methods:
        // ConvertPositionToMeters: converts stage equipment position from millimeters (0-maxPositionMillimeters) to meters
        // ConvertApertureToArcdegrees: converts pivoting panel aperture angle from natural number (0-maxApertureMillimiters) to arcdegrees and arcminutes (0-maxApertureDegrees)
        // ConvertApertureToDecimalDegrees: converts pivoting panel aperture angle from natural number (0-maxApertureMillimiters) to decimal degrees (0-maxApertureDegrees)
        // ConvertDecimalDegreesToAperture: converts a pivoting panel aperture angle from decimal degrees (0-maxApertureDegrees) to a natural number (0-maxApertureMillimiters)
        public static double ConvertPositionToMeters(int positionMillimeters)
        {
            return positionMillimeters / 1000.0;
        }

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

        public static string ConvertApertureToDecimalDegrees(int apertureMillimeters)
        {
            if (apertureMillimeters < 0 || apertureMillimeters > maxApertureMillimiters)
            {
                throw new ArgumentOutOfRangeException(nameof(apertureMillimeters), $"Input must be a natural number ranging from 0 to {maxApertureMillimiters}.");
            }
            
            double decimalDegrees = (apertureMillimeters / maxApertureMillimiters) * maxApertureDegrees;
            return $"{decimalDegrees:F2}°";
        }

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
            int buttonHeight = loadHallDataButton.Height;
            int buttonBottomMargin = 48;
            int DataGridViewSpacing = 12; // distance between DataGridViews
            int startX = 20; // starting positions for data groups

            // computed values
            int availableWidth = movingElementsTab.ClientSize.Width - 40;
            int DataGridViewWidth = (availableWidth - 2 * DataGridViewSpacing) / 3;
            int hallGroupHeight = groupTitleHeight + titleSpacing + labelHeight + labelSpacing;
            int hall3dModelGroupHeight = groupTitleHeight + titleSpacing + labelHeight + labelSpacing;
            int separatorMargin = groupSpacing / 2 + separatorHeight + 8;
            int totalDataGridViewHeight = movingElementsTab.ClientSize.Height - topMargin - hallGroupHeight - hall3dModelGroupHeight - separatorMargin - buttonHeight - buttonBottomMargin;
            int DataGridViewHeight = totalDataGridViewHeight / 2; //each group takes half of the available space

            // place elements
            PlaceGroupsAndSeparator_MovingElementsTab(topMargin, groupTitleHeight, titleSpacing, labelHeight, labelSpacing, groupSpacing, separatorHeight, startX, availableWidth, DataGridViewHeight);
            PlaceDataGridViews_MovingElementsTab(topMargin, groupTitleHeight, titleSpacing, labelHeight, labelSpacing, DataGridViewSpacing, startX, DataGridViewWidth, DataGridViewHeight, groupSpacing, separatorHeight);
            PlaceButtons_MovingElementsTab(buttonHeight);

            // adapt DataGridView columns to available width
            FitColumnsToWidth(hallStagecraftDataGridView);
            FitColumnsToWidth(hallLeftPanelsDataGridView);
            FitColumnsToWidth(hallRightPanelsDataGridView);
            FitColumnsToWidth(hall3dModelStagecraftDataGridView);
            FitColumnsToWidth(hall3dModelLeftPanelsDataGridView);
            FitColumnsToWidth(hall3dModelRightPanelsDataGridView);
        }

        private void PlaceGroupsAndSeparator_MovingElementsTab(int topMargin, int groupTitleHeight, int titleSpacing, int labelHeight, int labelSpacing, int groupSpacing, int separatorHeight, int startX, int availableWidth, int DataGridViewHeight)
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

            // 3D model group title
            int hall3dModelStartY = separatorY + separatorHeight + 8;
            hall3dModelDataGroupLabel.Left = (movingElementsTab.ClientSize.Width - hall3dModelDataGroupLabel.Width) / 2;
            hall3dModelDataGroupLabel.Top = hall3dModelStartY;
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

            // 3D model DataGridView labels
            int separatorY = hallDataGridViewY + DataGridViewHeight + groupSpacing / 2;
            int hall3dModelStartY = separatorY + separatorHeight + 8;
            int hall3dModelLabelY = hall3dModelStartY + groupTitleHeight + titleSpacing;
            hall3dModelStagecraftDataGridViewLabel.Left = startX;
            hall3dModelStagecraftDataGridViewLabel.Top = hall3dModelLabelY;
            hall3dModelLeftPanelsDataGridViewLabel.Left = startX + DataGridViewWidth + DataGridViewSpacing;
            hall3dModelLeftPanelsDataGridViewLabel.Top = hall3dModelLabelY;
            hall3dModelRightPanelsDataGridViewLabel.Left = startX + (DataGridViewWidth + DataGridViewSpacing) * 2;
            hall3dModelRightPanelsDataGridViewLabel.Top = hall3dModelLabelY;

            // 3D model DataGridViews
            int hall3dModelDataGridViewY = hall3dModelLabelY + labelHeight + labelSpacing;
            hall3dModelStagecraftDataGridView.Left = startX;
            hall3dModelStagecraftDataGridView.Top = hall3dModelDataGridViewY;
            hall3dModelStagecraftDataGridView.Width = DataGridViewWidth;
            hall3dModelStagecraftDataGridView.Height = DataGridViewHeight;

            hall3dModelLeftPanelsDataGridView.Left = hall3dModelLeftPanelsDataGridViewLabel.Left;
            hall3dModelLeftPanelsDataGridView.Top = hall3dModelDataGridViewY;
            hall3dModelLeftPanelsDataGridView.Width = DataGridViewWidth;
            hall3dModelLeftPanelsDataGridView.Height = DataGridViewHeight;

            hall3dModelRightPanelsDataGridView.Left = hall3dModelRightPanelsDataGridViewLabel.Left;
            hall3dModelRightPanelsDataGridView.Top = hall3dModelDataGridViewY;
            hall3dModelRightPanelsDataGridView.Width = DataGridViewWidth;
            hall3dModelRightPanelsDataGridView.Height = DataGridViewHeight;
        }

        private void PlaceButtons_MovingElementsTab(int buttonHeight)
        {
            // horizontally centering buttons and keeping them one beside the other at the bottom of the form
            int buttonSpacing = 16; // distance between buttons
            int buttonsTotalWidth = loadHallDataButton.Width + buttonSpacing + loadHall3dModelButton.Width + buttonSpacing + updateHall3dModelButton.Width;
            int buttonsStartX = (movingElementsTab.ClientSize.Width - buttonsTotalWidth) / 2;

            int dataGridViewsBottom = hall3dModelStagecraftDataGridView.Top + hall3dModelStagecraftDataGridView.Height;
            int tabPageBottom = movingElementsTab.Height; // bottom end of TabPage
            int buttonsY = dataGridViewsBottom + (tabPageBottom - dataGridViewsBottom - buttonHeight) / 2; // bottons in between DataGridViews end and form bottom end

            loadHallDataButton.Left = buttonsStartX;
            loadHallDataButton.Top = buttonsY;

            loadHall3dModelButton.Left = buttonsStartX + loadHallDataButton.Width + buttonSpacing;
            loadHall3dModelButton.Top = buttonsY;

            updateHall3dModelButton.Left = loadHall3dModelButton.Left + loadHall3dModelButton.Width + buttonSpacing;
            updateHall3dModelButton.Top = buttonsY;
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
                hallDataFileCheckDirectory = iniFile.Read("HallDataFileCheck", "Directory");
                hallDataFileCheckName = iniFile.Read("HallDataFileCheck", "FileName");
                int.TryParse(iniFile.Read("HallDataFileCheck", "Interval"), out hallDataFileCheckInterval);
                bool.TryParse(iniFile.Read("HallDataFileCheck", "Active"), out hallDataFileCheckActive);

                // 3D model file check parameters
                hall3dModelFileCheckDirectory = iniFile.Read("Hall3dModelFileCheck", "Directory");
                hall3dModelFileCheckName = iniFile.Read("Hall3dModelFileCheck", "FileName");
                int.TryParse(iniFile.Read("Hall3dModelFileCheck", "Interval"), out hall3dModelFileCheckInterval);
                bool.TryParse(iniFile.Read("Hall3dModelFileCheck", "Active"), out hall3dModelFileCheckActive);
            }

            // initialize timers
            InitTimers();

            // hard-coded correspondences between hall IDs and 3D model element names
            stagecraftEquipmentMapping.Add("TM.001", "Truss Motor 1"); // hypothetical 3D model element name
            stagecraftEquipmentMapping.Add("TM.002", "Truss Motor 2"); // hypothetical 3D model element name
            stagecraftEquipmentMapping.Add("TM.003", "Truss Motor 3"); // hypothetical 3D model element name
            stagecraftEquipmentMapping.Add("TM.004", "Truss Motor 4"); // hypothetical 3D model element name
            stagecraftEquipmentMapping.Add("TS.001", "Truss Stopper 1"); // hypothetical 3D model element name
            stagecraftEquipmentMapping.Add("TP.001", "Truss Position 1"); // hypothetical 3D model element name
            stagecraftEquipmentMapping.Add("TP.002", "Truss Position 2"); // hypothetical 3D model element name
            stagecraftEquipmentMapping.Add("TP.003", "Truss Position 3"); // hypothetical 3D model element name
            stagecraftEquipmentMapping.Add("TP.004", "Truss Position 4"); // hypothetical 3D model element name
            stagecraftEquipmentMapping.Add("TT.001", "Truss Tension 1"); // hypothetical 3D model element name
            stagecraftEquipmentMapping.Add("TD.001", "Truss Distance 1"); // hypothetical 3D model element name
            stagecraftEquipmentMapping.Add("TA.001", "Truss Angle 1"); // hypothetical 3D model element name
            stagecraftEquipmentMapping.Add("PO.001", "Platform Opening 1"); // hypothetical 3D model element name

            leftPanelMapping.Add("PS.001", "Left Panel 1"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.002", "Left Panel 2"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.003", "Left Panel 3"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.004", "Left Panel 4"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.005", "Left Panel 5"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.006", "Left Panel 6"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.007", "Left Panel 7"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.008", "Left Panel 8"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.009", "Left Panel 9"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.010", "Left Panel 10"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.011", "Left Panel 11"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.012", "Left Panel 12"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.013", "Left Panel 13"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.014", "Left Panel 14"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.015", "Left Panel 15"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.016", "Left Panel 16"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.017", "Left Panel 17"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.018", "Left Panel 18"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.019", "Left Panel 19"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.020", "Left Panel 20"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.021", "Left Panel 21"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.022", "Left Panel 22"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.023", "Left Panel 23"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.024", "Left Panel 24"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.025", "Left Panel 25"); // hypothetical 3D model element name
            leftPanelMapping.Add("PS.026", "Left Panel 26"); // hypothetical 3D model element name

            rightPanelMapping.Add("PD.001", "Right Panel 1"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.002", "Right Panel 2"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.003", "Right Panel 3"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.004", "Right Panel 4"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.005", "Right Panel 5"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.006", "Right Panel 6"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.007", "Right Panel 7"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.008", "Right Panel 8"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.009", "Right Panel 9"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.010", "Right Panel 10"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.011", "Right Panel 11"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.012", "Right Panel 12"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.013", "Right Panel 13"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.014", "Right Panel 14"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.015", "Right Panel 15"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.016", "Right Panel 16"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.017", "Right Panel 17"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.018", "Right Panel 18"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.019", "Right Panel 19"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.020", "Right Panel 20"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.021", "Right Panel 21"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.022", "Right Panel 22"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.023", "Right Panel 23"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.024", "Right Panel 24"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.025", "Right Panel 25"); // hypothetical 3D model element name
            rightPanelMapping.Add("PD.026", "Right Panel 26"); // hypothetical 3D model element name


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