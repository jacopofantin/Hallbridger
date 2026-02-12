using Hallbridger.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms; // for WinForms controls
using System.Windows.Forms.Integration; // for ElementHost (WPF in WinForms)
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;
using static Xbim.Presentation.DrawingControl3D;


namespace Hallbridger
{
    // enumeration for unit of measurement conversions (visible for all namespaces in the project)
    public enum UnitOfMeasurement
    {
        Millimeters,
        Meters,
        Arcdegrees,
        DecimalDegrees,
        None
    }

    public partial class HallbridgerForm : Form
    {
        /* CLASS LEVEL VARIABLES
         */

        // dictionaries to store moving element data coming from the real hall in
        private Dictionary<string, double?> realHallStagecraftEquipmentPositions = new Dictionary<string, double?>();
        private Dictionary<string, double?> realHallLeftPanelApertures = new Dictionary<string, double?>();
        private Dictionary<string, double?> realHallRightPanelApertures = new Dictionary<string, double?>();

        // dictionaries to store moving element data coming from the 3D hall in
        private Dictionary<string, double?> hall3DModelStagecraftEquipmentPositions = new Dictionary<string, double?>();
        private Dictionary<string, double?> hall3DModelLeftPanelApertures = new Dictionary<string, double?>();
        private Dictionary<string, double?> hall3DModelRightPanelApertures = new Dictionary<string, double?>();

        // dictionary to store 3D hall control unit compositions
        private readonly Dictionary<string, List<string>> hall3DModelStagecraftControlUnitsComposition = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, List<string>> hall3DModelLeftPanelControlUnitsComposition = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, List<string>> hall3DModelRightPanelControlUnitsComposition = new Dictionary<string, List<string>>();

        // class-level variables to parametrize maximum panel aperture values
        private static readonly double minPositionMillimeters = 0.0;
        private static readonly double maxPositionMillimeters = 99999.0;
        private static readonly double minApertureDegrees = 0.0;
        private static readonly double maxApertureDegrees = 60.0;
        private static readonly double minApertureMillimiters = 0.0;
        private static readonly double maxApertureMillimiters = 99999.0;

        // shared random number generator for generating random cell background colors for data discrepancy highlighting (otherwise the same seed of the pseudorandom generator is used in different data visualization method calls)
        private static readonly Random SharedRandom = new Random();

        // dictionary to store cell background colors in DataGridViews (needed for the show/hidden data discrepancy highlighting)
        Dictionary<DataGridView, Dictionary<string, Color>> cellColors = new Dictionary<DataGridView, Dictionary<string, Color>>();

        // dictionary to store selected cells in DataGridViews (needed since both sorting and data refresh cause the user-defined selection to be lost)
        Dictionary<DataGridView, List<(string RowId, int ColumnIndex)>> selectedCells = new Dictionary<DataGridView, List<(string RowId, int ColumnIndex)>>();

        // class-level variables for storing the 3D hall model and its file path
        private IfcStore hall3DModel;
        private string hall3DModelFilePath;

        // class-level dictionary to keep track of selected 3D model elements and corresponding popup windows for showing and editing their properties
        private readonly Dictionary<string, ModelElementPropertiesForm> selected3DElementsPropertyWindows = new Dictionary<string, ModelElementPropertiesForm>();

        // class-level variables for populating ComboNumericTextBox dropdown menu
        private List<decimal> positionDropdownItems;
        private List<decimal> apertureDropdownItems;

        // dictionary to store global RT values of Roberto de Silva hall for each panel configuration
        private readonly Dictionary<string, double[]> globalRtValues = new Dictionary<string, double[]>();

        // dictionary for storing correspondances between a panel configuration and the panel control units that are part of it
        private readonly Dictionary<string, HashSet<string>> panelConfigurations = new Dictionary<string, HashSet<string>>();

        // class-level variable to keep track of the current open/close panel configuration of the 3D hall
        private string currentHall3DModelPanelConfiguration = null;

        // class-level variable to keep track of first tab change (to clear cell selection of DataGridViews that are present in the other tabs)
        private bool isFirstTabChange = true;

        // INI file to store configuration for future software usage
        private readonly string iniPath = "D:\\Dateien\\Hallbridger\\Configuration\\conf.ini";

        // timers for automatic input file check
        private System.Windows.Forms.Timer realHallFileCheckTimer;
        private string realHallFileCheckDirectory = "D:\\Dateien\\Hallbridger\\IO_files"; // default directory, can be changed in options menu
        private string realHallFileCheckName = "Fotografia_sala_CURIO.txt"; // default file name, can be changed in options menu
        private int realHallFileCheckInterval = 1000; // default to 1 second, can be changed in options menu
        private bool realHallFileCheckActive = true; // default to true, can be changed in options menu

        private System.Windows.Forms.Timer hall3DModelFileCheckTimer;
        private string hall3DModelFileCheckDirectory = "D:\\Dateien\\Hallbridger\\IO_files"; // default directory, can be changed in options menu
        private string hall3DModelFileCheckName = "Model.ifc"; // default file name, can be changed in options menu
        private int hall3DModelFileCheckInterval = 1000; // default to 1 second, can be changed in options menu
        private string hall3DModelFileCheckOperation = "Load"; // default to "Load", can be changed to "Update" in options menu
        private bool hall3DModelFileCheckActive = true; // default to true, can be changed in options menu


        /* INITIALIZATION METHODS
         */

        public HallbridgerForm()
        {
            InitializeComponent();
            this.Icon = new Icon("icon.ico");
        }

        // method to read configuration saved in INI file
        private void ApplySavedConfiguration()
        {
            if (!File.Exists(iniPath))
            {
                return;
            }

            var iniFile = new IniFile(iniPath);

            // real hall file check settings
            realHallFileCheckDirectory = iniFile.Read("RealHallFileCheck", "Directory");
            realHallFileCheckName = iniFile.Read("RealHallFileCheck", "FileName");
            int.TryParse(iniFile.Read("RealHallFileCheck", "Interval"), out realHallFileCheckInterval);
            bool.TryParse(iniFile.Read("RealHallFileCheck", "Active"), out realHallFileCheckActive);

            // 3D hall file check settings
            hall3DModelFileCheckDirectory = iniFile.Read("Hall3DModelFileCheck", "Directory");
            hall3DModelFileCheckName = iniFile.Read("Hall3DModelFileCheck", "FileName");
            int.TryParse(iniFile.Read("Hall3DModelFileCheck", "Interval"), out hall3DModelFileCheckInterval);
            hall3DModelFileCheckOperation = iniFile.Read("Hall3DModelFileCheck", "Operation");
            bool.TryParse(iniFile.Read("Hall3DModelFileCheck", "Active"), out hall3DModelFileCheckActive);

            // automatic discrepancy highlighting settings
            automaticDiscrepancyHighlightingMenuEntry.Checked = bool.TryParse(iniFile.Read("AutomaticDiscrepancyHighlightingOption", "Enabled"), out var enabled) && enabled;
        }

        // method to (re)initialize timers for automatic input file check
        protected void InitializeTimers()
        {
            // dispose real hall file check timer if already initialized
            if (realHallFileCheckTimer != null)
            {
                realHallFileCheckTimer.Stop();
                realHallFileCheckTimer.Dispose();
                realHallFileCheckTimer = null;
            }

            // initialize real hall file check timer if active
            if (realHallFileCheckActive)
            {
                realHallFileCheckTimer = new System.Windows.Forms.Timer()
                {
                    Interval = realHallFileCheckInterval
                };
                realHallFileCheckTimer.Tick += RealHallFileCheckTimer_Tick;
                realHallFileCheckTimer.Start();
            }

            // dispose 3D hall file check timer if already initialized
            if (hall3DModelFileCheckTimer != null)
            {
                hall3DModelFileCheckTimer.Stop();
                hall3DModelFileCheckTimer.Dispose();
                hall3DModelFileCheckTimer = null;
            }

            // initialize 3D hall file check timer if active
            if (hall3DModelFileCheckActive)
            {
                hall3DModelFileCheckTimer = new System.Windows.Forms.Timer()
                {
                    Interval = hall3DModelFileCheckInterval
                };
                hall3DModelFileCheckTimer.Tick += Hall3DModelFileCheckTimer_Tick;
                hall3DModelFileCheckTimer.Start();
            }
        }

        // method to initialize the 3D model viewer
        private void Initialize3DModelViewer()
        {
            // create dummy WPF Control and discard it just to initialize the 3D viewer (workaround for WPF Controls needing to be viewed in order to be correctly included in the UI tree)
            var tempHost = new ElementHost
            {
                Child = hall3DModelViewer,
            };

            Controls.Add(tempHost);
            Application.DoEvents();
            Controls.Remove(tempHost);
            tempHost.Child = null;
            hall3DModelViewerHost.Child = hall3DModelViewer;
        }

        // method to initialize dropdown items for ComboNumericTextBox controls
        private void InitializeDropdownItems()
        {
            // positions [m]
            positionDropdownItems = new List<decimal>();
            double minPositionMeters = ConvertMillimetersToMeters(minPositionMillimeters);
            double maxPositionMeters = ConvertMillimetersToMeters(maxPositionMillimeters);

            for (int i = 0; i < 20; i++)
            {
                double item = minPositionMeters + i * (maxPositionMeters - minPositionMeters) / 19.0;
                positionDropdownItems.Add(Math.Round((decimal)item, 0, MidpointRounding.AwayFromZero));
            }

            // apertures [°]
            apertureDropdownItems = new List<decimal>();

            for (int i = 0; i < 20; i++)
            {
                double item = minApertureDegrees + i * (maxApertureDegrees - minApertureDegrees) / 19.0;
                apertureDropdownItems.Add(Math.Round((decimal)item, 0, MidpointRounding.AwayFromZero));
            }
        }


        /* BUSINESS LOGIC METHODS
         */

        /* import/export methods
         */

        // method to import real hall data through API
        private async Task LoadRealHallData(string apiUrl, string filePath)
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
                    System.Windows.Forms.MessageBox.Show("Error while reading hall data:\n" + errorMessage, "Import error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var realHallData = JsonConvert.DeserializeObject<Hallbridger_API.Models.CurioDataModel>(json);

                // empty and refill the class-level dictionaries
                realHallStagecraftEquipmentPositions.Clear();
                realHallStagecraftEquipmentPositions = realHallData.StagecraftEquipmentPositions;

                realHallLeftPanelApertures.Clear();
                realHallLeftPanelApertures = realHallData.LeftPanelApertures;

                realHallRightPanelApertures.Clear();
                realHallRightPanelApertures = realHallData.RightPanelApertures;
            }
        }

        // method to import 3D hall data
        private void Load3DHall(string filePath)
        {
            try
            {
                // empty the class-level dictionaries
                hall3DModelStagecraftEquipmentPositions.Clear();
                hall3DModelLeftPanelApertures.Clear();
                hall3DModelRightPanelApertures.Clear();

                // store the 3D model and its file path in the class-level variables
                hall3DModel = IfcStore.Open(filePath);
                hall3DModelFilePath = filePath;

                // using a temporary copy of the 3D model so the class-level one doesn't get disposed, otherwise the model would lose some key properties needed by the 3D viewer
                using (var temp3DModel = IfcStore.Open(filePath))
                {
                    // extract and store stagecraft equipment positions and panel apertures in the class-level dictionaries
                    foreach (var pair in hall3DModelStagecraftControlUnitsComposition)
                    {
                        var key = pair.Key;
                        var globalIdList = pair.Value;

                        hall3DModelStagecraftEquipmentPositions[key] = Get3DPropertyValue(temp3DModel, globalIdList[0], "PIECE POSITION");
                    }

                    foreach (var pair in hall3DModelLeftPanelControlUnitsComposition)
                    {
                        var key = pair.Key;
                        var globalIdList = pair.Value;

                        hall3DModelLeftPanelApertures[key] = Get3DPropertyValue(temp3DModel, globalIdList[0], "PANEL ANGLE");
                    }

                    foreach (var pair in hall3DModelRightPanelControlUnitsComposition)
                    {
                        var key = pair.Key;
                        var globalIdList = pair.Value;

                        hall3DModelRightPanelApertures[key] = Get3DPropertyValue(temp3DModel, globalIdList[0], "PANEL ANGLE");
                    }
                }
            }
            catch (Exception error)
            {
                System.Windows.Forms.MessageBox.Show("Error while reading 3D model:\n" + error.Message);
            }
        }

        // method to update the loaded 3D hall using loaded real hall data
        private void Update3DHall()
        {
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

                // using a temporary copy of the 3D model since we're opening it in editing mode
                using (var temp3DModel = IfcStore.Open(hall3DModelFilePath, xbimEditor))
                {
                    // start a transaction to overwrite the stagecraft equipment positions and panel apertures in the 3D model with those of the real hall
                    using (var hall3DModelUpdate = temp3DModel.BeginTransaction("3D hall update"))
                    {
                        // overwrite the stagecraft equipment positions and panel apertures in the 3D model with the loaded real hall data (even if some real hall data are missing)
                        foreach (var pair in hall3DModelStagecraftControlUnitsComposition)
                        {
                            var key = pair.Key;
                            var globalIdList = pair.Value;

                            foreach (var globalId in globalIdList)
                            {
                                Set3DPropertyValue(
                                    temp3DModel,
                                    globalId,
                                    "PIECE POSITION",
                                    realHallStagecraftEquipmentPositions[key] ?? null);
                            }
                        }

                        foreach (var pair in hall3DModelLeftPanelControlUnitsComposition)
                        {
                            var key = pair.Key;
                            var globalIdList = pair.Value;

                            foreach (var globalId in globalIdList)
                            {
                                Set3DPropertyValue(
                                    temp3DModel,
                                    globalId,
                                    "PANEL ANGLE",
                                    realHallLeftPanelApertures[key] ?? null);
                            }
                        }

                        foreach (var pair in hall3DModelRightPanelControlUnitsComposition)
                        {
                            var key = pair.Key;
                            var globalIdList = pair.Value;
                            foreach (var globalId in globalIdList)
                            {
                                Set3DPropertyValue(
                                    temp3DModel,
                                    globalId,
                                    "PANEL ANGLE",
                                    realHallRightPanelApertures[key] ?? null);
                            }
                        }

                        // commit the changes and save the 3D model
                        hall3DModelUpdate.Commit();
                        temp3DModel.SaveAs(hall3DModelFilePath);
                    }
                }
            }
            catch (Exception error)
            {
                System.Windows.Forms.MessageBox.Show("Error while modifying 3D model:\n" + error.Message);
            }
        }

        // method to export 3D hall data through API
        private async Task Export3DHallData(string apiUrl, string filePath)
        {
            // create data structure for exporting to Curio
            var dataModel = new Hallbridger_API.Models.CurioDataModel
            {
                StagecraftEquipmentPositions = new Dictionary<string, double?>(),
                LeftPanelApertures = new Dictionary<string, double?>(),
                RightPanelApertures = new Dictionary<string, double?>()
            };

            // extract keys from dictionaries so the order of the moving elements is preserved
            var stagecraftKeys = hall3DModelStagecraftEquipmentPositions.Keys.ToList();
            var leftPanelKeys = hall3DModelLeftPanelApertures.Keys.ToList();
            var rightPanelKeys = hall3DModelRightPanelApertures.Keys.ToList();

            // extract moving element data from the 3D model
            foreach (var key in stagecraftKeys)
            {
                double? value = hall3DModelStagecraftEquipmentPositions.TryGetValue(key, out var val)
                    ? val
                    : 0;
                dataModel.StagecraftEquipmentPositions[key] = value;
            }

            foreach (var key in leftPanelKeys)
            {
                double? value = hall3DModelLeftPanelApertures.TryGetValue(key, out var val)
                    ? val
                    : 0;
                dataModel.LeftPanelApertures[key] = value;
            }

            foreach (var key in rightPanelKeys)
            {
                double? value = hall3DModelRightPanelApertures.TryGetValue(key, out var val)
                    ? val
                    : 0;
                dataModel.RightPanelApertures[key] = value;
            }

            // build JSON for the API request
            var requestPayload = new
            {
                dataModel.StagecraftEquipmentPositions,
                dataModel.LeftPanelApertures,
                dataModel.RightPanelApertures,
                StagecraftKeys = stagecraftKeys,
                LeftPanelKeys = leftPanelKeys,
                RightPanelKeys = rightPanelKeys
            };

            string json = JsonConvert.SerializeObject(requestPayload);

            // send API request
            using (var client = new HttpClient())
            {
                var fileContent = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, fileContent);

                if (!response.IsSuccessStatusCode)
                {
                    // read and show error message coming from API
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    System.Windows.Forms.MessageBox.Show("Error while exporting 3D model data to file:\n" + errorMessage, "Export error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return;
                }

                // save the exported file
                var fileByteArray = await response.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes(filePath, fileByteArray);
            }
        }

        /* data and 3D model visualization methods
         */

        /* method to populate a DataGridView:
         * dgv: DataGridView to populate;
         * data: dictionary containing the values to populate dgv with;
         * conversionMethod: method to convert numerical values from one unit of measurement to another;
         * formattingMethod: method to format numerical values into strings with unit of measurement for display
         */
        private void ViewLoadedData(DataGridView dgv, Dictionary<string, double?> data, Func<double?, double> conversionMethod, Func<double, string> formattingMethod)
        {
            // save current data sorting
            GetDataSorting(dgv, out var dataGridViewSortedColumn, out var dataGridViewSortDirection);

            // save current scroll position
            int dataGridViewTopRowIndex = GetScrollPosition(dgv);

            // save current cell selection
            selectedCells[dgv] = GetCellSelection(dgv);

            // empty and refill the DataGridView
            dgv.Rows.Clear();
            foreach (var pair in data)
            {
                dgv.Rows.Add(pair.Key, pair.Value.HasValue
                    ? $"{formattingMethod(conversionMethod(pair.Value.Value))}"
                    : "");
            }

            // restore previous data sorting
            SetDataSorting(dgv, dataGridViewSortedColumn, dataGridViewSortDirection);

            // restore previous scroll position
            SetScrollPosition(dgv, dataGridViewTopRowIndex);

            // restore previous cell selection
            if (selectedCells.TryGetValue(dgv, out var cellSelection))
            {
                SetCellSelection(dgv, cellSelection);
            }
        }

        // method to display the 3D hall on the viewer
        private void View3DHall()
        {
            if (hall3DModel == null)
            {
                System.Windows.Forms.MessageBox.Show("No 3D hall model loaded. Please load a 3D hall before viewing it.");
                return;
            }

            var hall3DModelContext = new Xbim3DModelContext(hall3DModel);
            hall3DModelContext.CreateContext();

            hall3DModelViewer.Model = hall3DModel;
            hall3DModelViewer.LoadGeometry(hall3DModel);

            // enable "Reposition" button
            repositionButton.Enabled = true;
        }


        /* EVENT HANDLERS
         */

        /* main event handlers
         */

        // button event handlers
        private async void LoadRealHallDataButton_OnClick(object sender, EventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog loadRealHallDataDialog = new System.Windows.Forms.OpenFileDialog())
            {
                loadRealHallDataDialog.Title = "Import real hall data";
                loadRealHallDataDialog.Filter = "Text file (*.txt)|*.txt|Excel spreadsheet (*.xls;*.xlsx)|*.xls;*.xlsx|JavaScript Object Notation file (*.json)|*.json|eXtensible Markup Language file (*.xml)|*.xml| All files (*.*)|*.*";
                if (loadRealHallDataDialog.ShowDialog() == DialogResult.OK)
                {
                    string apiEndpoint;
                    string realHallDataFileExtension = Path.GetExtension(loadRealHallDataDialog.FileName).ToLowerInvariant();

                    switch (realHallDataFileExtension)
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
                            System.Windows.Forms.MessageBox.Show("File extension not supported: " + realHallDataFileExtension);
                            return;
                    }

                    // import data from the real hall and display them in the corresponding DataGridViews
                    await LoadRealHallData(apiEndpoint, loadRealHallDataDialog.FileName);

                    ViewLoadedData(realHallStagecraftDataGridView, realHallStagecraftEquipmentPositions, ConvertMillimetersToMeters, FormatMeters);
                    ViewLoadedData(realHallLeftPanelsDataGridView, realHallLeftPanelApertures, ConvertMillimetersToDecimalDegrees, FormatDecimalDegrees);
                    ViewLoadedData(realHallRightPanelsDataGridView, realHallRightPanelApertures, ConvertMillimetersToDecimalDegrees, FormatDecimalDegrees);

                    // update data discrepancies highlighting
                    UpdateDataDiscrepancyHighlighting(realHallStagecraftDataGridView, realHallStagecraftEquipmentPositions, hall3DModelStagecraftDataGridView, hall3DModelStagecraftEquipmentPositions);
                    UpdateDataDiscrepancyHighlighting(realHallLeftPanelsDataGridView, realHallLeftPanelApertures, hall3DModelLeftPanelsDataGridView, hall3DModelLeftPanelApertures);
                    UpdateDataDiscrepancyHighlighting(realHallRightPanelsDataGridView, realHallRightPanelApertures, hall3DModelRightPanelsDataGridView, hall3DModelRightPanelApertures);
                }
            }
        }

        private async void LoadHall3DModelButton_OnClick(object sender, EventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog loadHall3DModelDialog = new System.Windows.Forms.OpenFileDialog())
            {
                loadHall3DModelDialog.Title = "Import 3D hall";
                loadHall3DModelDialog.Filter = "3D model file (*.ifc)|*.ifc|All files (*.*)|*.*";
                
                if (loadHall3DModelDialog.ShowDialog() == DialogResult.OK)
                {
                    string hall3DModelFileCheckPath = loadHall3DModelDialog.FileName;
                    string hall3DModelFileExtension = Path.GetExtension(hall3DModelFileCheckPath).ToLowerInvariant();

                    switch (hall3DModelFileExtension)
                    {
                        case ".ifc":
                            break;
                        default:
                            System.Windows.Forms.MessageBox.Show("File extension not supported: " + hall3DModelFileExtension);
                            return;
                    }

                    // import the 3D hall and display its data in the corresponding DataGridViews
                    Load3DHall(hall3DModelFileCheckPath);

                    ViewLoadedData(hall3DModelStagecraftDataGridView, hall3DModelStagecraftEquipmentPositions, ConvertMillimetersToMeters, FormatMeters);
                    ViewLoadedData(hall3DModelLeftPanelsDataGridView, hall3DModelLeftPanelApertures, ConvertMillimetersToDecimalDegrees, FormatDecimalDegrees);
                    ViewLoadedData(hall3DModelRightPanelsDataGridView, hall3DModelRightPanelApertures, ConvertMillimetersToDecimalDegrees, FormatDecimalDegrees);

                    // update data discrepancies highlighting
                    UpdateDataDiscrepancyHighlighting(hall3DModelStagecraftDataGridView, hall3DModelStagecraftEquipmentPositions, realHallStagecraftDataGridView, realHallStagecraftEquipmentPositions);
                    UpdateDataDiscrepancyHighlighting(hall3DModelLeftPanelsDataGridView, hall3DModelLeftPanelApertures, realHallLeftPanelsDataGridView, realHallLeftPanelApertures);
                    UpdateDataDiscrepancyHighlighting(hall3DModelRightPanelsDataGridView, hall3DModelRightPanelApertures, realHallRightPanelsDataGridView, realHallRightPanelApertures);

                    // load 3D hall on viewer
                    View3DHall();

                    // identify current panel configuration in the 3D hall, if possible, and highlight it in the global RT values DataGridView and in the 3D model
                    IdentifyCurrent3DHallPanelConfiguration();
                }
            }
        }

        private async void UpdateHall3DModelButton_OnClick(object sender, EventArgs e)
        {
            // first, check if at least some real hall data have been previously loaded
            if (realHallStagecraftEquipmentPositions.Count == 0 && realHallLeftPanelApertures.Count == 0 && realHallRightPanelApertures.Count == 0)
            {
                // no real hall data loaded at all
                System.Windows.Forms.MessageBox.Show("No real hall data loaded. Please load hall data before updating the 3D model.");
                return;
            }

            // second, check if a 3D hall has been previously loaded, and if not, open a dialog to let the user choose it
            if (hall3DModel == null)
            {
                using (System.Windows.Forms.OpenFileDialog updateHall3DModelDialog = new System.Windows.Forms.OpenFileDialog())
                {
                    updateHall3DModelDialog.Title = "Update 3D hall";
                    updateHall3DModelDialog.Filter = "3D model file (*.ifc)|*.ifc|All files (*.*)|*.*";
                    if (updateHall3DModelDialog.ShowDialog() == DialogResult.OK)
                    {
                        string modelFilePath = updateHall3DModelDialog.FileName;
                        string hall3DModelFileExtension = Path.GetExtension(modelFilePath).ToLowerInvariant();
                        switch (hall3DModelFileExtension)
                        {
                            case ".ifc":
                                break;
                            default:
                                System.Windows.Forms.MessageBox.Show("File extension not supported: " + hall3DModelFileExtension);
                                return;
                        }

                        // import the 3D hall
                        Load3DHall(modelFilePath);
                    }
                }
            }

            // update the 3D hall with the loaded real hall data
            Update3DHall();

            // after the update, re-import the 3D hall and display its data in the corresponding DataGridViews
            Load3DHall(hall3DModelFilePath);

            ViewLoadedData(hall3DModelStagecraftDataGridView, hall3DModelStagecraftEquipmentPositions, ConvertMillimetersToMeters, FormatMeters);
            ViewLoadedData(hall3DModelLeftPanelsDataGridView, hall3DModelLeftPanelApertures, ConvertMillimetersToDecimalDegrees, FormatDecimalDegrees);
            ViewLoadedData(hall3DModelRightPanelsDataGridView, hall3DModelRightPanelApertures, ConvertMillimetersToDecimalDegrees, FormatDecimalDegrees);

            // update data discrepancies highlighting
            UpdateDataDiscrepancyHighlighting(hall3DModelStagecraftDataGridView, hall3DModelStagecraftEquipmentPositions, realHallStagecraftDataGridView, realHallStagecraftEquipmentPositions);
            UpdateDataDiscrepancyHighlighting(hall3DModelLeftPanelsDataGridView, hall3DModelLeftPanelApertures, realHallLeftPanelsDataGridView, realHallLeftPanelApertures);
            UpdateDataDiscrepancyHighlighting(hall3DModelRightPanelsDataGridView, hall3DModelRightPanelApertures, realHallRightPanelsDataGridView, realHallRightPanelApertures);

            // load 3D hall on viewer
            View3DHall();

            // identify current panel configuration in the 3D hall, if possible, and highlight it in the global RT values DataGridView and in the 3D model
            IdentifyCurrent3DHallPanelConfiguration();
        }

        private async void ExportHall3DModelDataButton_OnClick(object sender, EventArgs e)
        {
            // check if at least some 3D hall data have been previously loaded
            if (hall3DModelStagecraftEquipmentPositions.Count == 0 && hall3DModelLeftPanelApertures.Count == 0 && hall3DModelRightPanelApertures.Count == 0)
            {
                // no 3D hall data loaded at all
                System.Windows.Forms.MessageBox.Show("No 3D hall data loaded. Please load 3D hall data before exporting them.");
                return;
            }
            using (System.Windows.Forms.SaveFileDialog saveHall3DModelDataDialog = new System.Windows.Forms.SaveFileDialog())
            {
                saveHall3DModelDataDialog.Title = "Export 3D hall data";
                saveHall3DModelDataDialog.Filter = "Text file (*.txt)|*.txt|Excel spreadsheet (*.xls;*.xlsx)|*.xls;*.xlsx|JavaScript Object Notation file (*.json)|*.json|eXtensible Markup Language file (*.xml)|*.xml| All files (*.*)|*.*";
                
                if (saveHall3DModelDataDialog.ShowDialog() == DialogResult.OK)
                {
                    string apiEndpoint;
                    string hall3DModelDataFileExtension = Path.GetExtension(saveHall3DModelDataDialog.FileName).ToLowerInvariant();
                    
                    switch (hall3DModelDataFileExtension)
                    {
                        case ".txt":
                            apiEndpoint = "https://localhost:44307/api/export/txt";
                            break;
                        case ".xls":
                        case ".xlsx":
                            apiEndpoint = "https://localhost:44307/api/export/excel";
                            break;
                        case ".json":
                            apiEndpoint = "https://localhost:44307/api/export/json";
                            break;
                        case ".xml":
                            apiEndpoint = "https://localhost:44307/api/export/xml";
                            break;
                        default:
                            System.Windows.Forms.MessageBox.Show("File extension not supported: " + hall3DModelDataFileExtension);
                            return;
                    }

                    await Export3DHallData(apiEndpoint, saveHall3DModelDataDialog.FileName);
                }
            }
        }

        // timer event handlers
        private async void RealHallFileCheckTimer_Tick(object sender, EventArgs e)
        {
            string realHallFileCheckPath = Path.Combine(realHallFileCheckDirectory, realHallFileCheckName);

            if (File.Exists(realHallFileCheckPath))
            {
                string apiEndpoint;
                string realHallFileExtension = Path.GetExtension(realHallFileCheckPath).ToLowerInvariant();

                switch (realHallFileExtension)
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
                        System.Windows.Forms.MessageBox.Show("Real hall file extension not supported: " + realHallFileExtension);
                        return;
                }

                // import data from the real hall and display them in the corresponding DataGridViews
                await LoadRealHallData(apiEndpoint, realHallFileCheckPath);

                ViewLoadedData(realHallStagecraftDataGridView, realHallStagecraftEquipmentPositions, ConvertMillimetersToMeters, FormatMeters);
                ViewLoadedData(realHallLeftPanelsDataGridView, realHallLeftPanelApertures, ConvertMillimetersToDecimalDegrees, FormatDecimalDegrees);
                ViewLoadedData(realHallRightPanelsDataGridView, realHallRightPanelApertures, ConvertMillimetersToDecimalDegrees, FormatDecimalDegrees);

                // update data discrepancies highlighting
                UpdateDataDiscrepancyHighlighting(realHallStagecraftDataGridView, realHallStagecraftEquipmentPositions, hall3DModelStagecraftDataGridView, hall3DModelStagecraftEquipmentPositions);
                UpdateDataDiscrepancyHighlighting(realHallLeftPanelsDataGridView, realHallLeftPanelApertures, hall3DModelLeftPanelsDataGridView, hall3DModelLeftPanelApertures);
                UpdateDataDiscrepancyHighlighting(realHallRightPanelsDataGridView, realHallRightPanelApertures, hall3DModelRightPanelsDataGridView, hall3DModelRightPanelApertures);
            }
        }

        private async void Hall3DModelFileCheckTimer_Tick(object sender, EventArgs e)
        {
            string hall3DModelFileCheckPath = Path.Combine(hall3DModelFileCheckDirectory, hall3DModelFileCheckName);

            if (File.Exists(hall3DModelFileCheckPath))
            {
                string hall3DModelFileExtension = Path.GetExtension(hall3DModelFileCheckPath).ToLowerInvariant();

                switch (hall3DModelFileExtension)
                {
                    case ".ifc":
                        break;
                    default:
                        System.Windows.Forms.MessageBox.Show("3D hall file extension not supported: " + hall3DModelFileExtension);
                        return;
                }

                switch (hall3DModelFileCheckOperation)
                {
                    case "Load":
                        break;
                    case "Update":
                        // first, check if at least some real hall data have been previously loaded
                        if (realHallStagecraftEquipmentPositions.Count == 0 && realHallLeftPanelApertures.Count == 0 && realHallRightPanelApertures.Count == 0)
                        {
                            // no real hall data loaded at all
                            System.Windows.Forms.MessageBox.Show("No real hall data loaded. Please load hall data before updating the 3D model.");
                            return;
                        }

                        // second, load a 3D hall if it hasn't been previously loaded
                        if (hall3DModel == null)
                        {
                            Load3DHall(hall3DModelFileCheckPath);
                        }

                        // update the 3D hall with the loaded real hall data
                        Update3DHall();
                        break;
                    default:
                        System.Windows.Forms.MessageBox.Show("Operation on 3D hall file not supported: " + hall3DModelFileCheckOperation);
                        return;
                }

                // import the 3D hall and display its in the corresponding DataGridViews
                Load3DHall(hall3DModelFileCheckPath);

                ViewLoadedData(hall3DModelStagecraftDataGridView, hall3DModelStagecraftEquipmentPositions, ConvertMillimetersToMeters, FormatMeters);
                ViewLoadedData(hall3DModelLeftPanelsDataGridView, hall3DModelLeftPanelApertures, ConvertMillimetersToDecimalDegrees, FormatDecimalDegrees);
                ViewLoadedData(hall3DModelRightPanelsDataGridView, hall3DModelRightPanelApertures, ConvertMillimetersToDecimalDegrees, FormatDecimalDegrees);

                // update data discrepancies highlighting
                UpdateDataDiscrepancyHighlighting(hall3DModelStagecraftDataGridView, hall3DModelStagecraftEquipmentPositions, realHallStagecraftDataGridView, realHallStagecraftEquipmentPositions);
                UpdateDataDiscrepancyHighlighting(hall3DModelLeftPanelsDataGridView, hall3DModelLeftPanelApertures, realHallLeftPanelsDataGridView, realHallLeftPanelApertures);
                UpdateDataDiscrepancyHighlighting(hall3DModelRightPanelsDataGridView, hall3DModelRightPanelApertures, realHallRightPanelsDataGridView, realHallRightPanelApertures);

                // load 3D hall on viewer
                View3DHall();

                // identify current panel configuration in the 3D hall, if possible, and highlight it in the global RT values DataGridView and in the 3D model
                IdentifyCurrent3DHallPanelConfiguration();
            }
        }

        // checkbox event handlers
        private void HighlightDataDiscrepanciesCheckBox_Click(object sender, EventArgs e)
        {
            if (!highlightDataDiscrepanciesCheckBox.Checked)
            {
                // discrepancies highlighting used to be switched on and is now off, hide it
                ResetCellBackgroundColors(realHallStagecraftDataGridView);
                ResetCellBackgroundColors(hall3DModelStagecraftDataGridView);
                ResetCellBackgroundColors(realHallLeftPanelsDataGridView);
                ResetCellBackgroundColors(hall3DModelLeftPanelsDataGridView);
                ResetCellBackgroundColors(realHallRightPanelsDataGridView);
                ResetCellBackgroundColors(hall3DModelRightPanelsDataGridView);
            }
            else
            {
                // discrepancies highlighting used to be switched off and is now on, show it
                ApplyCellBackgroundColors(realHallStagecraftDataGridView);
                ApplyCellBackgroundColors(hall3DModelStagecraftDataGridView);
                ApplyCellBackgroundColors(realHallLeftPanelsDataGridView);
                ApplyCellBackgroundColors(hall3DModelLeftPanelsDataGridView);
                ApplyCellBackgroundColors(realHallRightPanelsDataGridView);
                ApplyCellBackgroundColors(hall3DModelRightPanelsDataGridView);
            }
        }

        private void HighSpeedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            hall3DModelViewer.HighSpeed = highSpeedCheckBox.Checked;
        }

        // menu event handlers
        private void AutomaticDiscrepancyHighlightingMenuEntry_CheckedChanged(object sender, EventArgs e)
        {
            // store the new setting into the configuration file
            var iniFile = new IniFile(iniPath);
            iniFile.Write("AutomaticDiscrepancyHighlightingOption", "Enabled", automaticDiscrepancyHighlightingMenuEntry.Checked
                ? "true"
                : "false");
        }

        private void ConfigurationsMenuEntry_Click(object sender, EventArgs e)
        {
            using (var configurationsDialog = new ConfigurationsDialog(iniPath))
            {
                if (configurationsDialog.ShowDialog(this) == DialogResult.OK)
                {
                    ApplySavedConfiguration();
                    InitializeTimers();
                }
            }
        }

        /* other event handlers
         */

        // DataGridView column header event handlers
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

        // mouse-move event for tab Moving elements (workaround for showing the tooltip on the "Highlight data discrepancies" checkbox when it's disabled)
        private void MovingElementsTab_MouseMove(object sender, MouseEventArgs e)
        {
            // checkbox position relative to its Parent control (movingElementsTab)
            var checkboxRectangle = new Rectangle(
                highlightDataDiscrepanciesCheckBox.Location,
                highlightDataDiscrepanciesCheckBox.Size);

            if (!highlightDataDiscrepanciesCheckBox.Enabled && checkboxRectangle.Contains(e.Location))
            {
                highlightDataDiscrepanciesToolTip.Show(
                    "Load at least some of both real and 3D hall data to highlight differences between them.",
                    movingElementsTab,
                    highlightDataDiscrepanciesCheckBox.Left,
                    highlightDataDiscrepanciesCheckBox.Bottom + 5,
                    5000);
            }
            else
            {
                highlightDataDiscrepanciesToolTip.Hide(movingElementsTab);
            }
        }

        // 3D model element selection event handler
        private void Hall3DModelViewer_SelectedEntityChanged(object sender, EventArgs e)
        {
            var selected3DElements = hall3DModelViewer.Selection.ToList().OfType<IIfcRoot>().ToList();
            var selected3DElementsGlobalIds = selected3DElements
                .Select(elem => elem.GlobalId.ToString())
                .ToHashSet();

            // close and dispose property windows for deselected 3D elements
            var deselected3DElementsGlobalIds = selected3DElementsPropertyWindows.Keys
                .Except(selected3DElementsGlobalIds)
                .ToList();

            foreach (var globalId in deselected3DElementsGlobalIds)
            {
                selected3DElementsPropertyWindows[globalId].Close();
                selected3DElementsPropertyWindows[globalId].Dispose();
                selected3DElementsPropertyWindows.Remove(globalId);
            }

        // open property windows for newly selected 3D elements
        int propertyWindowIndex = 0;

            foreach (var element in selected3DElements)
            {
                if (!selected3DElementsPropertyWindows.ContainsKey(element.GlobalId))
                {
                    // extract basic info from selected entity
                    string name = element.Name ?? element.ToString();
                    string globalId = element?.GlobalId ?? "";
                    string instanceType = element.GetType().Name ?? "";

                    // extract additional info depending on element type
                    string typeName = "";

                    if (element is IIfcBuildingElementProxy buildingElement && buildingElement.IsTypedBy.First().RelatingType != null)
                    {
                        typeName = buildingElement.IsTypedBy.First().RelatingType.Name ?? "";
                    }
                    else if (element is IIfcFurnishingElement furnishingElement && furnishingElement.IsTypedBy.First().RelatingType != null)
                    {
                        typeName = furnishingElement.IsTypedBy.First().RelatingType.Name ?? "";
                    }

                    List<string> controlUnits = new List<string>();

                    foreach (var pair in hall3DModelStagecraftControlUnitsComposition)
                    {
                        if (pair.Value.Contains(globalId))
                        {
                            controlUnits.Add(pair.Key);
                        }
                    }

                    foreach (var pair in hall3DModelLeftPanelControlUnitsComposition)
                    {
                        if (pair.Value.Contains(globalId))
                        {
                            controlUnits.Add(pair.Key);
                        }
                    }

                    foreach (var pair in hall3DModelRightPanelControlUnitsComposition)
                    {
                        if (pair.Value.Contains(globalId))
                        {
                            controlUnits.Add(pair.Key);
                        }
                    }

                    ComboNumericTextBox.ComboNumericValueType? valueType = null;
                    decimal positionApertureValue = 0m;
                    UnitOfMeasurement unitOfMeasurement = UnitOfMeasurement.None;
                    decimal minimumValue = 0m;
                    decimal maximumValue = 0m;
                    List<decimal> dropdownItems = null;

                    if (typeName.StartsWith("EQUIPMENT PIECE"))
                    {
                        valueType = ComboNumericTextBox.ComboNumericValueType.Position;
                        unitOfMeasurement = UnitOfMeasurement.Meters;
                        minimumValue = (decimal) ConvertMillimetersToMeters(minPositionMillimeters);
                        maximumValue = (decimal) ConvertMillimetersToMeters(maxPositionMillimeters);
                        var positionValue = Get3DPropertyValue(hall3DModel, globalId, "PIECE POSITION");
                        
                        if (positionValue.HasValue)
                        {
                            positionApertureValue = (decimal) positionValue.Value;
                        }
                        
                        dropdownItems = positionDropdownItems;
                    }
                    else if (typeName.StartsWith("PANEL ROTATION"))
                    {
                        valueType = ComboNumericTextBox.ComboNumericValueType.Aperture;
                        unitOfMeasurement = UnitOfMeasurement.DecimalDegrees;
                        minimumValue = (decimal)minApertureDegrees;
                        maximumValue = (decimal)maxApertureDegrees;
                        var apertureValue = Get3DPropertyValue(hall3DModel, globalId, "PANEL ANGLE");

                        if (apertureValue.HasValue)
                        {
                            positionApertureValue = (decimal) apertureValue.Value;
                        }
                        
                        dropdownItems = apertureDropdownItems;
                    }

                    // property window initialisation
                    var modelElementPropertiesForm = new ModelElementPropertiesForm();
                    modelElementPropertiesForm.ValueConfirmed += ComboNumericTextBox_ValueConfirmed;

                    // fill property window with data
                    modelElementPropertiesForm.Fill3DElementProperties(
                        name: string.IsNullOrWhiteSpace(name)
                            ? null
                            : name,
                        globalId: string.IsNullOrWhiteSpace(globalId)
                            ? null
                            : globalId,
                        instanceType: string.IsNullOrWhiteSpace(instanceType)
                            ? null
                            : instanceType,
                        typeName: string.IsNullOrWhiteSpace(typeName)
                            ? null
                            : typeName,
                        controlUnits: controlUnits,
                        valueType: valueType,
                        positionApertureValue: positionApertureValue,
                        unitOfMeasurement: unitOfMeasurement,
                        minimumValue: minimumValue,
                        maximumValue: maximumValue,
                        dropdownItems: dropdownItems
                    );

                    // show property window near the 3D viewer, offsetting each new window to avoid overlap
                    int locationOffset = 30 * propertyWindowIndex;
                    var baseLocation = acousticsTab.PointToScreen(new Point(10, 10));
                    modelElementPropertiesForm.Location = new Point(baseLocation.X + locationOffset, baseLocation.Y + locationOffset);
                    modelElementPropertiesForm.Show(); // non-modal window
                    modelElementPropertiesForm.BringToFront();

                    // store reference to the new 3D element-property window pair
                    selected3DElementsPropertyWindows[element.GlobalId] = modelElementPropertiesForm;
                }

                propertyWindowIndex++;
            }
        }

        // event handler for confirmed value in the ComboNumericTextBox inside the 3D element properties window
        private void ComboNumericTextBox_ValueConfirmed(object sender, ValueConfirmedEventArgs valueConfirmedArguments)
        {
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

                // using a temporary copy of the 3D model since we're opening it in editing mode
                using (var temp3DModel = IfcStore.Open(hall3DModelFilePath, xbimEditor))
                {
                    // start a transaction to update the element of the 3D hall with the new value from its property window
                    using (var modelElementUpdate = temp3DModel.BeginTransaction("3D element update"))
                    {
                        var controlUnits = valueConfirmedArguments.ControlUnits;
                        var globalId = valueConfirmedArguments.GlobalId;
                        var valueType = valueConfirmedArguments.ValueType;
                        var positionApertureValue = (double) valueConfirmedArguments.PositionApertureValue;

                        if (valueType == ComboNumericTextBox.ComboNumericValueType.Position)
                        {
                            double positionValue = ConvertMetersToMillimeters(positionApertureValue);

                            if (controlUnits.Count == 0)
                            {
                                // single element update not linked to any control unit
                                Set3DPropertyValue(
                                    temp3DModel,
                                    globalId,
                                    "PIECE POSITION",
                                    positionApertureValue);
                            }
                            else
                            {
                                // multiple element update linked to control units
                                foreach (var unit in controlUnits)
                                {
                                    // set the new value in 3D model for each of the elements that are part of the control unit
                                    if (hall3DModelStagecraftControlUnitsComposition.TryGetValue(unit, out var globalIdList))
                                    {
                                        foreach (var id in globalIdList)
                                        {
                                            Set3DPropertyValue(
                                                temp3DModel,
                                                id,
                                                "PIECE POSITION",
                                                positionApertureValue);
                                        }
                                    }

                                    // update values in dictionary
                                    if (hall3DModelStagecraftEquipmentPositions.TryGetValue(unit, out _))
                                    {
                                        hall3DModelStagecraftEquipmentPositions[unit] = positionValue;
                                    }

                                    // refresh data view
                                    ViewLoadedData(hall3DModelStagecraftDataGridView, hall3DModelStagecraftEquipmentPositions, ConvertMillimetersToMeters, FormatMeters);

                                    // update data discrepancies highlighting
                                    UpdateDataDiscrepancyHighlighting(hall3DModelStagecraftDataGridView, hall3DModelStagecraftEquipmentPositions, realHallStagecraftDataGridView, realHallStagecraftEquipmentPositions);
                                }
                            }
                        }
                        else if (valueConfirmedArguments.ValueType == ComboNumericTextBox.ComboNumericValueType.Aperture)
                        {
                            double apertureValue = ConvertDecimalDegreesToMillimeters(positionApertureValue);

                            if (controlUnits.Count == 0)
                            {
                                // single element update not linked to any control unit
                                Set3DPropertyValue(
                                    temp3DModel,
                                    globalId,
                                    "PANEL ANGLE",
                                    positionApertureValue);
                            }
                            else
                            {
                                // multiple element update linked to control units
                                foreach (var unit in controlUnits)
                                {
                                    // set the new value in 3D model for each of the elements that are part of the control unit
                                    if (TryGetPanelGlobalIdList(unit, out var globalIdList))
                                    {
                                        foreach (var id in globalIdList)
                                        {
                                            Set3DPropertyValue(
                                                temp3DModel,
                                                id,
                                                "PANEL ANGLE",
                                                positionApertureValue);
                                        }
                                    }

                                    // update values in dictionary
                                    if (hall3DModelLeftPanelApertures.ContainsKey(unit))
                                    {
                                        hall3DModelLeftPanelApertures[unit] = apertureValue;
                                    }
                                    else if (hall3DModelRightPanelApertures.ContainsKey(unit))
                                    {
                                        hall3DModelRightPanelApertures[unit] = apertureValue;
                                    }

                                    // refresh views
                                    ViewLoadedData(hall3DModelLeftPanelsDataGridView, hall3DModelLeftPanelApertures, ConvertMillimetersToDecimalDegrees, FormatDecimalDegrees);
                                    ViewLoadedData(hall3DModelRightPanelsDataGridView, hall3DModelRightPanelApertures, ConvertMillimetersToDecimalDegrees, FormatDecimalDegrees);

                                    // update data discrepancies highlighting
                                    UpdateDataDiscrepancyHighlighting(hall3DModelLeftPanelsDataGridView, hall3DModelLeftPanelApertures, realHallLeftPanelsDataGridView, realHallLeftPanelApertures);
                                    UpdateDataDiscrepancyHighlighting(hall3DModelRightPanelsDataGridView, hall3DModelRightPanelApertures, realHallRightPanelsDataGridView, realHallRightPanelApertures);
                                }
                            }
                        }

                        // commit the changes and save the 3D model
                        modelElementUpdate.Commit();
                        temp3DModel.SaveAs(hall3DModelFilePath); // overwrite 3D model file
                        hall3DModel = IfcStore.Open(hall3DModelFilePath); // update class-level reference

                        // refresh 3D hall viewer
                        hall3DModelViewer.ReloadModel(ModelRefreshOptions.ViewPreserveCameraPosition | ModelRefreshOptions.ViewPreserveSelection);

                        // identify current panel configuration in the 3D hall, if possible, and highlight it in the global RT values DataGridView and in the 3D model
                        IdentifyCurrent3DHallPanelConfiguration();

                        MessageBox.Show("Value successfully modified", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while modifying value:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // post-paint event handler to highlight in the global RT values DataGridView the current open/close panel configuration of the 3D hall (gets called after each row is painted)
        private void GlobalRtDataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (!(sender is DataGridView dgv) || string.IsNullOrEmpty(currentHall3DModelPanelConfiguration))
            {
                return;
            }

            // if the row that has just been painted corresponds to the current open/close panel configuration of the 3D hall, draw a red rectangle around it
            var configuration = dgv.Rows[e.RowIndex].Cells["Configuration"].Value?.ToString();
            if (configuration == currentHall3DModelPanelConfiguration)
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
     
        // tab change event handler
        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            // refresh component views for the tab that has just been selected
            TabPage currentTab = mainTabControl.SelectedTab;

            LayOutComponents(currentTab);

            // show 3D element properties only in acoustics tab
            if (currentTab == acousticsTab)
            {
                foreach (var modelElementPropertiesForm in selected3DElementsPropertyWindows.Values)
                {
                    modelElementPropertiesForm?.Show();
                }
            }
            else
            {
                foreach (var modelElementPropertiesForm in selected3DElementsPropertyWindows.Values)
                {
                    modelElementPropertiesForm?.Hide();
                }
            }

            // clear cell selection of DataGridViews in the other tabs at the first tab change
            if (isFirstTabChange)
            {
                globalRtDataGridView.ClearSelection();
                isFirstTabChange = false;
            }
        }


        /* AUXILIARY METHODS
         */

        /* import/export auxiliary methods
         */

        // extracts the value of a given property from a 3D model element identified by its IFC GlobalId inside a given IFC model
        private double? Get3DPropertyValue(IfcStore ifcModel, string globalId, string propertyName)
        {
            switch (propertyName)
            {
                case "PIECE POSITION":
                    // extract all stagecraft equipment pieces from the 3D model
                    var equipmentPieces = ifcModel.Instances
                        .OfType<IIfcBuildingElementProxy>()
                        .Where(p => p.IsTypedBy
                            .Any(t => t.RelatingType != null && t.RelatingType.Name == "STAGECRAFT EQUIPMENT:STAGECRAFT EQUIPMENT"))
                        .ToList();

                    if (!(equipmentPieces.FirstOrDefault(pc => pc.GlobalId == globalId) is IIfcBuildingElementProxy piece))
                    {
                        return null;
                    }

                    // extract "PIECE POSITION" property from the 3D element
                    var positionProperty = (piece.IsTypedBy
                        .FirstOrDefault(type => type.RelatingType.Name == "STAGECRAFT EQUIPMENT:STAGECRAFT EQUIPMENT")?.RelatingType as IIfcTypeObject)?
                        .HasPropertySets.OfType<IIfcPropertySet>()
                        .FirstOrDefault(pset => pset.Name == "Quote")?
                        .HasProperties.OfType<IIfcPropertySingleValue>()
                        .FirstOrDefault(prop => prop.Name == "PIECE POSITION");

                    // get value of the 3D property
                    if (double.TryParse(positionProperty?.NominalValue?.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double position))
                    {
                            return position;
                    }

                    return null;
                case "PANEL ANGLE":
                    // extract all pivoting panels from the 3D model
                    var pivotingPanels = ifcModel.Instances
                        .OfType<IIfcFurnishingElement>()
                        .Where(p => p.IsTypedBy
                            .Any(t => t.RelatingType != null && t.RelatingType.Name == "PANEL ROTATION:PANEL ROTATION 60"))
                        .ToList();

                    if (!(pivotingPanels.FirstOrDefault(pan => pan.GlobalId == globalId) is IIfcFurnishingElement panel))
                    {
                        return null;
                    }

                    // extract "PANEL ANGLE" property from the 3D element
                    var angleProperty = (panel.IsTypedBy
                        .FirstOrDefault(type => type.RelatingType.Name == "PANEL ROTATION:PANEL ROTATION 60")?.RelatingType as IIfcTypeObject)?
                        .HasPropertySets.OfType<IIfcPropertySet>()
                        .FirstOrDefault(pset => pset.Name == "Quote")?
                        .HasProperties.OfType<IIfcPropertySingleValue>()
                        .FirstOrDefault(prop => prop.Name == "PANEL ANGLE");

                    // get value of the 3D property
                    if (double.TryParse(angleProperty?.NominalValue?.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double angle))
                    {
                        return angle;
                    }

                    return null;
                default:
                    return null;
            }
        }

        // assigns a new value to a given property of a 3D model element identified by its IFC GlobalId inside a given IFC model
        private void Set3DPropertyValue(IfcStore ifcModel, string globalId, string propertyName, double? propertyValue)
        {
            switch (propertyName)
            {
                case "PIECE POSITION":
                    // extract all stagecraft equipment pieces from the 3D model
                    var equipmentPieces = ifcModel.Instances
                        .OfType<IIfcBuildingElementProxy>()
                        .Where(p => p.IsTypedBy
                            .Any(t => t.RelatingType != null && t.RelatingType.Name == "STAGECRAFT EQUIPMENT:STAGECRAFT EQUIPMENT"))
                        .ToList();

                    if (!(equipmentPieces.FirstOrDefault(pc => pc.GlobalId == globalId) is IIfcBuildingElementProxy piece))
                    {
                        return;
                    }

                    // extract "PIECE POSITION" property from the 3D element
                    var positionProperty = (piece.IsTypedBy
                        .FirstOrDefault(type => type.RelatingType.Name == "STAGECRAFT EQUIPMENT:STAGECRAFT EQUIPMENT")?.RelatingType as IIfcTypeObject)?
                        .HasPropertySets.OfType<IIfcPropertySet>()
                        .FirstOrDefault(pset => pset.Name == "Quote")?
                        .HasProperties.OfType<IIfcPropertySingleValue>()
                        .FirstOrDefault(prop => prop.Name == "PIECE POSITION");

                    // set new value to the 3D property
                    if (positionProperty != null)
                    {
                        if (propertyValue.HasValue)
                        {
                            positionProperty.NominalValue = new Xbim.Ifc4.MeasureResource.IfcLengthMeasure(propertyValue.Value);
                        }
                        else
                        {
                            positionProperty.NominalValue = null;
                        }
                    }
                    break;
                case "PANEL ANGLE":
                    // extract all pivoting panels from the 3D model
                    var pivotingPanels = ifcModel.Instances
                        .OfType<IIfcFurnishingElement>()
                        .Where(p => p.IsTypedBy
                            .Any(t => t.RelatingType != null && t.RelatingType.Name == "PANEL ROTATION:PANEL ROTATION 60"))
                        .ToList();

                    if (!(pivotingPanels.FirstOrDefault(pan => pan.GlobalId == globalId) is IIfcFurnishingElement panel))
                    {
                        return;
                    }

                    // extract "PANEL ANGLE" property from the 3D element
                    var angleProperty = (panel.IsTypedBy
                        .FirstOrDefault(type => type.RelatingType.Name == "PANEL ROTATION:PANEL ROTATION 60")?.RelatingType as IIfcTypeObject)?
                        .HasPropertySets.OfType<IIfcPropertySet>()
                        .FirstOrDefault(pset => pset.Name == "Quote")?
                        .HasProperties.OfType<IIfcPropertySingleValue>()
                        .FirstOrDefault(prop => prop.Name == "PANEL ANGLE");

                    // set new value to the 3D property
                    if (angleProperty != null)
                    {
                        if (propertyValue.HasValue)
                        {
                            angleProperty.NominalValue = new Xbim.Ifc4.MeasureResource.IfcPlaneAngleMeasure(propertyValue.Value);
                        }
                        else
                        {
                            angleProperty.NominalValue = null;
                        }
                    }
                    break;
                default:
                    return;
            }
        }

        // auxiliary method to get the list of IFC GlobalIds of the panels composing a given control unit (searches both in left and right panel control units composition dictionaries)
        private bool TryGetPanelGlobalIdList(string unit, out List<string> globalIdList)
        {
            return hall3DModelLeftPanelControlUnitsComposition.TryGetValue(unit, out globalIdList)
                || hall3DModelRightPanelControlUnitsComposition.TryGetValue(unit, out globalIdList);
        }

        /* data and 3D model visualization auxiliary methods
         */

        // getters and setters to save and restore DataGridView state
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

        private int GetScrollPosition(DataGridView dgv)
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

            foreach (var (rowId, columnIndex) in cellSelection)
            {
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.Cells[0].Value?.ToString() == rowId)
                    {
                        if (columnIndex < row.Cells.Count)
                            row.Cells[columnIndex].Selected = true;
                        break;
                    }
                }
            }
        }

        // auxiliary method to paint cell background of all rows of a DataGridView using the class-level dictionary of cell colors (not a setter because it modifies the DataGridView using the class-level dictionary and doesn't take the value to be set as input)
        private void ApplyCellBackgroundColors(DataGridView dgv)
        {
            if (cellColors[dgv] == null)
            {
                return;
            }

            foreach (DataGridViewRow row in dgv.Rows)
            {
                string rowId = row.Cells[0].Value?.ToString();

                if (rowId != null && cellColors[dgv].TryGetValue(rowId, out var color) && row.DefaultCellStyle.BackColor != color)
                {
                    row.DefaultCellStyle.BackColor = color;
                }
            }
        }

        // auxiliary method to update cell background colors of a DataGridView couple containing corresponding data (highlights data discrepancies between two DataGridViews given their data dictionaries having corresponding keys)
        private void UpdateCellBackgroundColors(DataGridView dgv, Dictionary<string, double?> data, DataGridView cmpDgv, Dictionary<string, double?> cmpData)
        {
            if (!cellColors.ContainsKey(dgv) || cellColors[dgv] == null)
            {
                cellColors[dgv] = new Dictionary<string, Color>();

                foreach (var key in data.Keys)
                {
                    cellColors[dgv][key] = Color.Empty;
                }
            }

            if (!cellColors.ContainsKey(cmpDgv) || cellColors[cmpDgv] == null)
            {
                cellColors[cmpDgv] = new Dictionary<string, Color>();

                foreach (var key in cmpData.Keys)
                {
                    cellColors[cmpDgv][key] = Color.Empty;
                }
            }

            foreach (var controlUnit in data.Keys)
            {
                if (cmpData.ContainsKey(controlUnit) && data[controlUnit] != cmpData[controlUnit]
                    && (cellColors[dgv][controlUnit] == Color.Empty || cellColors[cmpDgv][controlUnit] == Color.Empty))
                {
                    // new discrepancy found, generate a new random color and store it in the dictionary for both DataGridViews
                    Color randomColor;
                    lock (SharedRandom)
                    {
                        randomColor = Color.FromArgb(
                            SharedRandom.Next(256),
                            SharedRandom.Next(256),
                            SharedRandom.Next(256));
                    }

                    cellColors[dgv][controlUnit] = randomColor;
                    cellColors[cmpDgv][controlUnit] = randomColor;
                }
                else
                {
                    // no discrepancy, reset cell colors to default if they are not already
                    if (cellColors[dgv][controlUnit] != Color.Empty)
                    {
                        cellColors[dgv][controlUnit] = Color.Empty;
                        cellColors[cmpDgv][controlUnit] = Color.Empty;
                    }
                }
            }
        }

        // auxiliary method to reset cell background colors of a DataGridView to default
        private void ResetCellBackgroundColors(DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                row.DefaultCellStyle.BackColor = Color.Empty;
            }
        }

        /* auxiliary method to update the highlighting of data discrepancies
         * dgv: the just populated DataGridView;
         * data: dictionary containing the values of dgv;
         * compareDgv: DataGridView populated with compareData;
         * compareData: dictionary containing the values to compare data with;
         */
        private void UpdateDataDiscrepancyHighlighting(DataGridView dgv, Dictionary<string, double?> data, DataGridView compareDgv, Dictionary<string, double?> compareData)
        {
            // proceed only if both real and 3D hall data have already been loaded
            if (dgv.Rows.Count > 0 && compareDgv.Rows.Count > 0)
            {
                // update cell background colors based on data comparison
                UpdateCellBackgroundColors(dgv, data, compareDgv, compareData);

                // if "Automatic discrepancy highlighting" menu entry is checked, apply the new background colors to the DataGridView couple
                if (automaticDiscrepancyHighlightingMenuEntry.Checked)
                {
                    ApplyCellBackgroundColors(dgv);
                    ApplyCellBackgroundColors(compareDgv);
                }

                // enable "Highlight data discrepancies" checkbox if it's not already
                if (!highlightDataDiscrepanciesCheckBox.Enabled)
                {
                    highlightDataDiscrepanciesCheckBox.Enabled = true;
                }

                // align "Highlight data discrepancies" checkbox state with "Automatic discrepancy highlighting" menu entry state
                highlightDataDiscrepanciesCheckBox.Checked = automaticDiscrepancyHighlightingMenuEntry.Checked;
            }
        }

        // auxiliary method to check if the current configuration of open/close panels in the 3D hall matches one of those for which we have global RT values at disposal
        private void IdentifyCurrent3DHallPanelConfiguration()
        {
            // if no panel apertures have been loaded from the 3D hall, we cannot identify any configuration
            if (hall3DModelLeftPanelApertures.Values.Count(value => value != null) == 0 &&
                hall3DModelRightPanelApertures.Values.Count(value => value != null) == 0)
            {
                return;
            }

            // collect all panels that are completely open
            var openPanels = new HashSet<string>(
                hall3DModelLeftPanelApertures.Where(pair => pair.Value == maxApertureMillimiters).Select(pair => pair.Key)
                .Concat(hall3DModelRightPanelApertures.Where(pair => pair.Value == maxApertureMillimiters).Select(pair => pair.Key))
            );

            // find the panel configuration that matches the currently open/close panels of the 3D hall
            foreach (var configuration in panelConfigurations)
            {
                if (configuration.Value.SetEquals(openPanels))
                {
                    // configuration found
                    currentHall3DModelPanelConfiguration = configuration.Key;

                    // select the first panel column of the current configuration in the 3D viewer (setting a list of entities to property Selection of the viewer doesn't highlight them, so we select only one entity)
                    var currentPanelSet = configuration.Value;

                    if (hall3DModelLeftPanelControlUnitsComposition.TryGetValue(currentPanelSet.First(), out var globalIdList) && globalIdList.Count > 0)
                    {
                        var panelArray = hall3DModel.Instances
                            .OfType<IIfcFurnishingElement>()
                            .FirstOrDefault(pa => pa.GlobalId == globalIdList[0]);

                        if (panelArray != null)
                        {
                            hall3DModelViewer.SelectedEntity = panelArray;
                        }
                    }

                    // redraw the global RT values DataGridView so that the current configuration gets highlighted
                    globalRtDataGridView.Invalidate();

                    return;
                }
            }

            // if we haven't find one, the current configuration does not match any of those for which we have global RT values at disposal
            currentHall3DModelPanelConfiguration = null;
        }

        /* conversion and formatting auxiliary methods:
         * ConvertMetersToMillimeters: converts a length measure from meters (minPositionMeters-maxPositionMeters) to millimeters
         * ConvertMillimetersToMeters: converts a length measure from millimeters (minPositionMillimeters-maxPositionMillimeters) to meters
         * ConvertMillimetersToDecimalDegrees: converts an angular measure in millimeters (length of the cable controlling the rotation) (minApertureMillimiters-maxApertureMillimiters) to decimal degrees (minApertureDegrees-maxApertureDegrees)
         * ConvertDecimalDegreesToMillimeters: converts an angular measure in decimal degrees (minApertureDegrees-maxApertureDegrees) to millimeters (length of the cable controlling the rotation) (minApertureMillimiters-maxApertureMillimiters)
         * FormatMeters: formats a measure in meters adding "m" at the end of it
         * FormatMillimeters: formats a measure in millimeters adding "mm" at the end of it
         * FormatDecimalDegrees: formats a measure in decimal degrees adding "°" at the end of it
         * FormatArcdegrees: formats a measure in decimal degrees into degrees and arcminutes adding "°" and "′" at the end of integer and fractional parts respectively
         */

        public static double ConvertMetersToMillimeters(double? meters)
        {
            double minPositionMeters = ConvertMillimetersToMeters(minPositionMillimeters);
            double maxPositionMeters = ConvertMillimetersToMeters(maxPositionMillimeters);

            if (!meters.HasValue)
            {
                throw new ArgumentNullException(nameof(meters), "Input must not be null.");
            }
            if (meters.Value < minPositionMeters || meters.Value > maxPositionMeters)
            {
                throw new ArgumentOutOfRangeException(nameof(meters.Value), $"Input must be a number ranging from {minPositionMeters} to {maxPositionMeters}.");
            }
            return meters.Value * 1000.0;
        }

        public static double ConvertMillimetersToMeters(double? millimeters)
        {
            if (!millimeters.HasValue)
            {
                throw new ArgumentNullException(nameof(millimeters), "Input must not be null.");
            }

            if (millimeters.Value < minPositionMillimeters || millimeters.Value > maxPositionMillimeters)
            {
                throw new ArgumentOutOfRangeException(nameof(millimeters.Value), $"Input must be a number ranging from {minPositionMillimeters} to {maxPositionMillimeters}.");
            }

            return millimeters.Value / 1000.0;
        }

        public static double ConvertMillimetersToDecimalDegrees(double? millimeters)
        {
            if (!millimeters.HasValue)
            {
                throw new ArgumentNullException(nameof(millimeters), "Input must not be null.");
            }

            if (millimeters.Value < minApertureMillimiters || millimeters.Value > maxApertureMillimiters)
            {
                throw new ArgumentOutOfRangeException(nameof(millimeters), $"Input must be a number ranging from {minApertureMillimiters} to {maxApertureMillimiters}.");
            }

            return Math.Round((millimeters.Value / maxApertureMillimiters) * maxApertureDegrees, 2);
        }

        public static double ConvertDecimalDegreesToMillimeters(double? decimalDegrees)
        {
            if (!decimalDegrees.HasValue)
            {
                throw new ArgumentNullException(nameof(decimalDegrees), "Input must not be null.");
            }
            if (decimalDegrees.Value < minApertureDegrees || decimalDegrees.Value > maxApertureDegrees)
            {
                throw new ArgumentOutOfRangeException(nameof(decimalDegrees), $"Input must be a number ranging from {minApertureDegrees} to {maxApertureDegrees}.");
            }
            return (decimalDegrees.Value / maxApertureDegrees) * maxApertureMillimiters;
        }

        public static string FormatMeters(double meters)
        {
            return $"{meters:F3} m";
        }

        public static string FormatMillimeters(double millimeters)
        {
            return $"{millimeters} mm";
        }

        public static string FormatDecimalDegrees(double decimalDegrees)
        {
            return $"{decimalDegrees:F2}°";
        }

        public static string FormatArcdegrees(double decimalDegrees)
        {
            int degrees = (int) decimalDegrees;
            int arcminutes = (int) Math.Round((decimalDegrees - degrees) * 60);
            if (arcminutes == 60)
            {
                degrees += 1;
                arcminutes = 0;
            }
            return $"{degrees}° {arcminutes}′";
        }

        /* graphical layout auxiliary methods
         */

        // auxiliary method for laying out components in a specified tab of the form
        private void LayOutComponents(TabPage tab)
        {
            string tabName = tab?.Name;

            switch (tabName)
            {
                case "movingElementsTab":
                { // braces to limit scope of variables
                    // layout constants
                    int topMargin = 16;
                    int groupTitleHeight = 30; // height reserved for group titles
                    int titleSpacing = 6; // distance between group title and DataGridViews
                    int labelHeight = 20;
                    int labelSpacing = 4; // distance between DataGridView labels and DataGridViews
                    int groupSpacing = 32; // distance between data groups
                    int separatorHeight = 2; // height of the separator line
                    int buttonHeight = loadRealHallDataButton.Height;
                    int buttonBottomMargin = 48;
                    int DataGridViewSpacing = 12; // distance between DataGridViews
                    int startX = 20; // starting positions for data groups

                    // computed values
                    int availableWidth = movingElementsTab.ClientSize.Width - 40;
                    int DataGridViewWidth = (availableWidth - 2 * DataGridViewSpacing) / 3;
                    int realHallGroupHeight = groupTitleHeight + titleSpacing + labelHeight + labelSpacing;
                    int realHall3DModelGroupHeight = groupTitleHeight + titleSpacing + labelHeight + labelSpacing;
                    int separatorMargin = groupSpacing / 2 + separatorHeight + 8;
                    int totalDataGridViewHeight = movingElementsTab.ClientSize.Height - topMargin - realHallGroupHeight - realHall3DModelGroupHeight - separatorMargin - buttonHeight - buttonBottomMargin;
                    int DataGridViewHeight = totalDataGridViewHeight / 2; //each group takes half of the available space

                    // place elements
                    PlaceGroupsAndSeparator_MovingElementsTab(topMargin, groupTitleHeight, titleSpacing, labelHeight, labelSpacing, groupSpacing, separatorHeight, startX, availableWidth, DataGridViewHeight);
                    PlaceDataGridViews_MovingElementsTab(topMargin, groupTitleHeight, titleSpacing, labelHeight, labelSpacing, DataGridViewSpacing, startX, DataGridViewWidth, DataGridViewHeight, groupSpacing, separatorHeight);
                    PlaceButtons_MovingElementsTab(buttonHeight);

                    // adapt DataGridView columns to available width
                    AdaptDataGridViewSizes(realHallStagecraftDataGridView);
                    AdaptDataGridViewSizes(realHallLeftPanelsDataGridView);
                    AdaptDataGridViewSizes(realHallRightPanelsDataGridView);
                    AdaptDataGridViewSizes(hall3DModelStagecraftDataGridView);
                    AdaptDataGridViewSizes(hall3DModelLeftPanelsDataGridView);
                    AdaptDataGridViewSizes(hall3DModelRightPanelsDataGridView);

                    break;
                }
                case "acousticsTab":
                { // braces to limit scope of variables
                  // layout constants
                    int margin = 20;
                    int labelHeight = 28;
                    int labelSpacing = 8;
                    int viewerMargin = 24;
                    int minGridWidth = 240;
                    int minViewerWidth = 320;
                    int gridHeight = 300;
                    int buttonSpacing = 16;
                    int controlsMargin = 12;

                    // proportionally adapt the width of the elements depending on the window size, from minWidth (by minWidth of the window) to maxWidth (by maxWidth of the window)
                    int tabWidth = acousticsTab.Width;
                    int tabHeight = acousticsTab.Height;

                    double minWidth = 800.0, maxWidth = 1920.0;
                    double minProportion = 0.5, maxProportion = 0.3;
                    double proportion = minProportion;

                    if (tabWidth > minWidth)
                    {
                        proportion = minProportion - (tabWidth - minWidth) * (minProportion - maxProportion) / (maxWidth - minWidth);

                        if (proportion < maxProportion)
                        {
                            proportion = maxProportion;
                        }
                    }

                    int gridWidth = (int)Math.Round(tabWidth * proportion);

                    if (gridWidth < minGridWidth)
                    {
                        gridWidth = minGridWidth;
                    }

                    int viewerWidth = tabWidth - gridWidth - 3 * viewerMargin;

                    if (viewerWidth < minViewerWidth)
                    {
                        viewerWidth = minViewerWidth;
                    }

                    // place global RT values DataGridView
                    globalRtDataGridView.Left = margin;
                    globalRtDataGridView.Top = tabHeight - gridHeight - margin;
                    globalRtDataGridView.Width = gridWidth;
                    globalRtDataGridView.Height = gridHeight;

                    // place global RT values DataGridView title
                    globalRtDataGridViewLabel.Left = globalRtDataGridView.Left + (globalRtDataGridView.Width - globalRtDataGridViewLabel.Width) / 2;
                    globalRtDataGridViewLabel.Width = gridWidth;
                    globalRtDataGridViewLabel.Height = labelHeight;
                    globalRtDataGridViewLabel.Top = globalRtDataGridView.Top - labelHeight - labelSpacing;

                    // place 3D hall viewer
                    int controlsHeight = Math.Max(repositionButton.Height, highSpeedCheckBox.Height);

                    hall3DModelViewerHost.Left = gridWidth + 2 * viewerMargin;
                    hall3DModelViewerHost.Top = margin;
                    hall3DModelViewerHost.Width = viewerWidth;
                    hall3DModelViewerHost.Height = tabHeight - 2 * margin - controlsHeight - controlsMargin; // compute height to leave room for Button and Checkbox

                    // place "Reposition" button and "High performance mode" checkbox one aside the other in the bottom-right part of the tab below the 3D viewer
                    int totalControlsWidth = repositionButton.Width + buttonSpacing + highSpeedCheckBox.Width;
                    int controlsLeft = hall3DModelViewerHost.Left + hall3DModelViewerHost.Width - totalControlsWidth;
                    int controlsTop = hall3DModelViewerHost.Top + hall3DModelViewerHost.Height + controlsMargin;

                    repositionButton.Left = controlsLeft;
                    repositionButton.Top = controlsTop;

                    highSpeedCheckBox.Left = repositionButton.Right + buttonSpacing;
                    highSpeedCheckBox.Top = controlsTop + (repositionButton.Height - highSpeedCheckBox.Height) / 2;

                    // adapt DataGridView sizes to available room
                    AdaptDataGridViewSizes(globalRtDataGridView);

                    break;
                }
                default:
                    break;
            }
        }

        // auxiliary methods for Moving element tab component layout
        private void PlaceGroupsAndSeparator_MovingElementsTab(int topMargin, int groupTitleHeight, int titleSpacing, int labelHeight, int labelSpacing, int groupSpacing, int separatorHeight, int startX, int availableWidth, int DataGridViewHeight)
        {
            // real hall group title
            realHallDataGroupLabel.Left = (movingElementsTab.ClientSize.Width - realHallDataGroupLabel.Width) / 2;
            realHallDataGroupLabel.Top = topMargin;
            int realHallDataGridViewY = topMargin + groupTitleHeight + titleSpacing + labelHeight + labelSpacing;

            // horizontal separator
            int separatorY = realHallDataGridViewY + DataGridViewHeight + groupSpacing / 2;
            groupSeparator.Left = startX;
            groupSeparator.Top = separatorY;
            groupSeparator.Width = availableWidth;
            groupSeparator.Height = separatorHeight;

            // 3D hall data group title
            int hall3DModelStartY = separatorY + separatorHeight + 8;
            hall3DModelDataGroupLabel.Left = (movingElementsTab.ClientSize.Width - hall3DModelDataGroupLabel.Width) / 2;
            hall3DModelDataGroupLabel.Top = hall3DModelStartY;
        }

        private void PlaceDataGridViews_MovingElementsTab(int topMargin, int groupTitleHeight, int titleSpacing, int labelHeight, int labelSpacing, int DataGridViewSpacing, int startX, int DataGridViewWidth, int DataGridViewHeight, int groupSpacing, int separatorHeight)
        {
            // real hall DataGridView labels
            int realHallLabelY = topMargin + groupTitleHeight + titleSpacing;
            realHallStagecraftDataGridViewLabel.Left = startX;
            realHallStagecraftDataGridViewLabel.Top = realHallLabelY;
            realHallLeftPanelsDataGridViewLabel.Left = startX + DataGridViewWidth + DataGridViewSpacing;
            realHallLeftPanelsDataGridViewLabel.Top = realHallLabelY;
            realHallRightPanelsDataGridViewLabel.Left = startX + (DataGridViewWidth + DataGridViewSpacing) * 2;
            realHallRightPanelsDataGridViewLabel.Top = realHallLabelY;

            // real hall DataGridViews
            int realHallDataGridViewY = realHallLabelY + labelHeight + labelSpacing;
            realHallStagecraftDataGridView.Left = startX;
            realHallStagecraftDataGridView.Top = realHallDataGridViewY;
            realHallStagecraftDataGridView.Width = DataGridViewWidth;
            realHallStagecraftDataGridView.Height = DataGridViewHeight;

            realHallLeftPanelsDataGridView.Left = realHallLeftPanelsDataGridViewLabel.Left;
            realHallLeftPanelsDataGridView.Top = realHallDataGridViewY;
            realHallLeftPanelsDataGridView.Width = DataGridViewWidth;
            realHallLeftPanelsDataGridView.Height = DataGridViewHeight;

            realHallRightPanelsDataGridView.Left = realHallRightPanelsDataGridViewLabel.Left;
            realHallRightPanelsDataGridView.Top = realHallDataGridViewY;
            realHallRightPanelsDataGridView.Width = DataGridViewWidth;
            realHallRightPanelsDataGridView.Height = DataGridViewHeight;

            // 3D hall DataGridView labels
            int separatorY = realHallDataGridViewY + DataGridViewHeight + groupSpacing / 2;
            int hall3DModelStartY = separatorY + separatorHeight + 8;
            int hall3DModelLabelY = hall3DModelStartY + groupTitleHeight + titleSpacing;
            hall3DModelStagecraftDataGridViewLabel.Left = startX;
            hall3DModelStagecraftDataGridViewLabel.Top = hall3DModelLabelY;
            hall3DModelLeftPanelsDataGridViewLabel.Left = startX + DataGridViewWidth + DataGridViewSpacing;
            hall3DModelLeftPanelsDataGridViewLabel.Top = hall3DModelLabelY;
            hall3DModelRightPanelsDataGridViewLabel.Left = startX + (DataGridViewWidth + DataGridViewSpacing) * 2;
            hall3DModelRightPanelsDataGridViewLabel.Top = hall3DModelLabelY;

            // 3D hall DataGridViews
            int hall3DModelDataGridViewY = hall3DModelLabelY + labelHeight + labelSpacing;
            hall3DModelStagecraftDataGridView.Left = startX;
            hall3DModelStagecraftDataGridView.Top = hall3DModelDataGridViewY;
            hall3DModelStagecraftDataGridView.Width = DataGridViewWidth;
            hall3DModelStagecraftDataGridView.Height = DataGridViewHeight;

            hall3DModelLeftPanelsDataGridView.Left = hall3DModelLeftPanelsDataGridViewLabel.Left;
            hall3DModelLeftPanelsDataGridView.Top = hall3DModelDataGridViewY;
            hall3DModelLeftPanelsDataGridView.Width = DataGridViewWidth;
            hall3DModelLeftPanelsDataGridView.Height = DataGridViewHeight;

            hall3DModelRightPanelsDataGridView.Left = hall3DModelRightPanelsDataGridViewLabel.Left;
            hall3DModelRightPanelsDataGridView.Top = hall3DModelDataGridViewY;
            hall3DModelRightPanelsDataGridView.Width = DataGridViewWidth;
            hall3DModelRightPanelsDataGridView.Height = DataGridViewHeight;
        }

        private void PlaceButtons_MovingElementsTab(int buttonHeight)
        {
            // horizontally centering buttons and keeping them one beside the other at the bottom of the form
            int buttonSpacing = 16; // distance between buttons
            int buttonsTotalWidth =
                loadRealHallDataButton.Width +
                buttonSpacing +
                loadHall3DModelButton.Width +
                buttonSpacing +
                updateHall3DModelButton.Width +
                buttonSpacing +
                exportHall3DModelDataButton.Width;

            int buttonsStartX = (movingElementsTab.ClientSize.Width - buttonsTotalWidth) / 2;

            int dataGridViewsBottom = hall3DModelStagecraftDataGridView.Top + hall3DModelStagecraftDataGridView.Height;
            int tabPageBottom = movingElementsTab.Height; // bottom end of TabPage
            int buttonsY = dataGridViewsBottom + (tabPageBottom - dataGridViewsBottom - buttonHeight) / 2;

            loadRealHallDataButton.Left = buttonsStartX;
            loadRealHallDataButton.Top = buttonsY;

            loadHall3DModelButton.Left = loadRealHallDataButton.Left + loadRealHallDataButton.Width + buttonSpacing;
            loadHall3DModelButton.Top = buttonsY;

            updateHall3DModelButton.Left = loadHall3DModelButton.Left + loadHall3DModelButton.Width + buttonSpacing;
            updateHall3DModelButton.Top = buttonsY;

            exportHall3DModelDataButton.Left = updateHall3DModelButton.Left + updateHall3DModelButton.Width + buttonSpacing;
            exportHall3DModelDataButton.Top = buttonsY;

            // keep the "Highlight data discrepancies" checkbox to the right of the "Export 3D hall data" button
            int checkBoxSpacing = 24;

            highlightDataDiscrepanciesCheckBox.Left = exportHall3DModelDataButton.Right + checkBoxSpacing;
            highlightDataDiscrepanciesCheckBox.Top = buttonsY + (buttonHeight - highlightDataDiscrepanciesCheckBox.Height) / 2;
        }

        /* auxiliary method to adapt row and column sizes of a DataGridView to content:
         * - fills available width with columns
         * - makes row heights fit content
         * - makes column widths fit content (minimum width is longest content in the column)
         */
        private void AdaptDataGridViewSizes(DataGridView dgv)
        {
            if (dgv.ColumnCount == 0)
            {
                return;
            }

            // compute maximum needed width for each column and height for each column header
            int[] minWidths = new int[dgv.ColumnCount];
            int minHeaderHeight = 20;
            int headerHeight = 0;

            using (Graphics graph = dgv.CreateGraphics())
            {
                for (int i = 0; i < dgv.ColumnCount; i++)
                {
                    var column = dgv.Columns[i];

                    // maximum header width
                    int maxHeaderWidth = (int)graph.MeasureString(column.HeaderText, dgv.Font).Width + 16;

                    // maximum header height
                    int h = (int)Math.Ceiling(graph.MeasureString(column.HeaderText, dgv.Font).Height) + 8;
                    if (h > headerHeight)
                    {
                        headerHeight = h;
                    }

                    // compute column widths based on cell contents
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        if (row.IsNewRow)
                        {
                            continue;
                        }

                        var value = row.Cells[i].FormattedValue?.ToString() ?? "";
                        int cellWidth = (int)graph.MeasureString(value, dgv.Font).Width + 16;
                        if (cellWidth > maxHeaderWidth)
                        {
                            maxHeaderWidth = cellWidth;
                        }
                    }

                    minWidths[i] = maxHeaderWidth;
                }

                // compute row heights based on cell contents
                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    int maxCellHeight = dgv.Rows[i].Height;
                    for (int j = 0; j < dgv.ColumnCount; j++)
                    {
                        var cellValue = dgv.Rows[i].Cells[j].FormattedValue?.ToString() ?? "";
                        int cellHeight = (int)Math.Ceiling(graph.MeasureString(cellValue, dgv.Font).Height) + 8;
                        if (cellHeight > maxCellHeight)
                        {
                            maxCellHeight = cellHeight;
                        }
                    }

                    dgv.Rows[i].Height = maxCellHeight;
                }
            }

            // distribute remaining width proportionally if there is extra space
            int totalMinWidth = minWidths.Sum();
            int availableWidth = dgv.ClientSize.Width;
            int scrollbarWidth = dgv.Controls.OfType<VScrollBar>().FirstOrDefault()?.Visible == true
                ? SystemInformation.VerticalScrollBarWidth
                : 0;
            availableWidth -= scrollbarWidth;

            int[] finalWidths = new int[dgv.ColumnCount];
            if (totalMinWidth < availableWidth)
            {
                // scale proportionally
                double scale = (double)availableWidth / totalMinWidth;
                int usedWidth = 0;
                for (int i = 0; i < dgv.ColumnCount - 1; i++)
                {
                    finalWidths[i] = (int)Math.Round(minWidths[i] * scale);
                    usedWidth += finalWidths[i];
                }
                // last column takes remaining width to avoid rounding issues
                finalWidths[dgv.ColumnCount - 1] = availableWidth - usedWidth;
            }
            else
            {
                // use minimum widths
                Array.Copy(minWidths, finalWidths, dgv.ColumnCount);
            }

            // set column widths
            for (int i = 0; i < dgv.ColumnCount; i++)
            {
                dgv.Columns[i].Width = finalWidths[i];
            }

            // set header height
            if (headerHeight < minHeaderHeight)
            {
                headerHeight = minHeaderHeight;
            }

            dgv.ColumnHeadersHeight = headerHeight;
        }


        /* OVERRIDING METHODS
         */

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // read INI file and restore saved settings
            ApplySavedConfiguration();

            // initialize timers
            InitializeTimers();

            // hard-coded panel configurations in Roberto de Silva hall (keys: configuration names, values: IDs of the panel control units that are part of that configuration)
            panelConfigurations.Add("CLOSED", new HashSet<string> { });
            panelConfigurations.Add("2F", new HashSet<string> { "PS.001", "PS.002", "PS.003", "PD.001", "PD.002", "PD.003" });
            panelConfigurations.Add("2C", new HashSet<string> { "PS.004", "PS.005", "PS.006", "PS.007", "PD.004", "PD.005", "PD.006", "PD.007" });
            panelConfigurations.Add("1F", new HashSet<string> { "PS.012", "PS.013", "PS.014", "PS.015", "PD.012", "PD.013", "PD.014", "PD.015" });
            panelConfigurations.Add("1C", new HashSet<string> { "PS.016", "PS.017", "PS.018", "PS.019", "PD.016", "PD.017", "PD.018", "PD.019" });
            panelConfigurations.Add("1P", new HashSet<string> { "PS.020", "PS.021", "PS.022", "PS.023", "PD.020", "PD.021", "PD.022", "PD.023" });
            panelConfigurations.Add("0C", new HashSet<string> { "PS.024", "PS.025", "PS.026", "PD.024", "PD.025", "PD.026" });
            panelConfigurations.Add("1FC", new HashSet<string> { "PS.012", "PS.013", "PS.014", "PS.015", "PS.016", "PS.017", "PS.018", "PS.019", "PD.012", "PD.013", "PD.014", "PD.015", "PD.016", "PD.017", "PD.018", "PD.019" });
            panelConfigurations.Add("1FCP", new HashSet<string> { "PS.012", "PS.013", "PS.014", "PS.015", "PS.016", "PS.017", "PS.018", "PS.019", "PS.020", "PS.021", "PS.022", "PS.023", "PD.012", "PD.013", "PD.014", "PD.015", "PD.016", "PD.017", "PD.018", "PD.019", "PD.020", "PD.021", "PD.022", "PD.023" });
            panelConfigurations.Add("2FCP", new HashSet<string> { "PS.001", "PS.002", "PS.003", "PS.004", "PS.005", "PS.006", "PS.007", "PS.008", "PS.009", "PS.010", "PS.011", "PD.001", "PD.002", "PD.003", "PD.004", "PD.005", "PD.006", "PD.007", "PD.008", "PD.009", "PD.010", "PD.011" });
            panelConfigurations.Add("OPEN", new HashSet<string> { "PS.001", "PS.002", "PS.003", "PS.004", "PS.005", "PS.006", "PS.007", "PS.008", "PS.009", "PS.010", "PS.011", "PS.012", "PS.013", "PS.014", "PS.015", "PS.016", "PS.017", "PS.018", "PS.019", "PS.020", "PS.021", "PS.022", "PS.023", "PS.024", "PS.025", "PS.026", "PD.001", "PD.002", "PD.003", "PD.004", "PD.005", "PD.006", "PD.007", "PD.008", "PD.009", "PD.010", "PD.011", "PD.012", "PD.013", "PD.014", "PD.015", "PD.016", "PD.017", "PD.018", "PD.019", "PD.020", "PD.021", "PD.022", "PD.023", "PD.024", "PD.025", "PD.026" });

            // hard-coded mapping between IDs of the hall control units and IDs (IFC GlobalId) of their composing 3D model elements
            hall3DModelStagecraftControlUnitsComposition.Add("TM.001", new List<string> { "IfcStage_001" });
            hall3DModelStagecraftControlUnitsComposition.Add("TM.002", new List<string> { "IfcStage_002" });
            hall3DModelStagecraftControlUnitsComposition.Add("TM.003", new List<string> { "IfcStage_003" });
            hall3DModelStagecraftControlUnitsComposition.Add("TM.004", new List<string> { "IfcStage_004" });
            hall3DModelStagecraftControlUnitsComposition.Add("TS.001", new List<string> { "IfcStage_005" });
            hall3DModelStagecraftControlUnitsComposition.Add("TP.001", new List<string> { "IfcStage_006" });
            hall3DModelStagecraftControlUnitsComposition.Add("TP.002", new List<string> { "IfcStage_007" });
            hall3DModelStagecraftControlUnitsComposition.Add("TP.003", new List<string> { "IfcStage_008" });
            hall3DModelStagecraftControlUnitsComposition.Add("TP.004", new List<string> { "IfcStage_009" });
            hall3DModelStagecraftControlUnitsComposition.Add("TT.001", new List<string> { "IfcStage_010" });
            hall3DModelStagecraftControlUnitsComposition.Add("TD.001", new List<string> { "IfcStage_011" });
            hall3DModelStagecraftControlUnitsComposition.Add("TA.001", new List<string> { "IfcStage_012" });
            hall3DModelStagecraftControlUnitsComposition.Add("PO.001", new List<string> { "IfcStage_013" });

            hall3DModelLeftPanelControlUnitsComposition.Add("PS.001", new List<string> { "IfcPanel_001", "IfcPanel_002" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.002", new List<string> { "IfcPanel_003", "IfcPanel_004" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.003", new List<string> { "IfcPanel_005" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.004", new List<string> { "IfcPanel_006", "IfcPanel_007" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.005", new List<string> { "IfcPanel_008", "IfcPanel_009" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.006", new List<string> { "IfcPanel_010", "IfcPanel_011" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.007", new List<string> { "IfcPanel_012", "IfcPanel_013" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.008", new List<string> { "IfcPanel_014", "IfcPanel_015" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.009", new List<string> { "IfcPanel_016", "IfcPanel_017" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.010", new List<string> { "IfcPanel_018" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.011", new List<string> { "IfcPanel_019" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.012", new List<string> { "IfcPanel_020", "IfcPanel_021" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.013", new List<string> { "IfcPanel_022", "IfcPanel_023" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.014", new List<string> { "IfcPanel_024", "IfcPanel_025" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.015", new List<string> { "IfcPanel_026" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.016", new List<string> { "IfcPanel_027", "IfcPanel_028" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.017", new List<string> { "IfcPanel_029", "IfcPanel_030" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.018", new List<string> { "IfcPanel_031", "IfcPanel_032" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.019", new List<string> { "IfcPanel_033", "IfcPanel_034" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.020", new List<string> { "IfcPanel_035", "IfcPanel_036" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.021", new List<string> { "IfcPanel_037", "IfcPanel_038" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.022", new List<string> { "IfcPanel_039", "IfcPanel_040" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.023", new List<string> { "IfcPanel_041", "IfcPanel_042" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.024", new List<string> { "0eZ9VepF9FQO1sL1JkDygu", "0eZ9VepF9FQO1sL1JkD$9U" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.025", new List<string> { "0eZ9VepF9FQO1sL1JkD$92", "0eZ9VepF9FQO1sL1JkD$9v" });
            hall3DModelLeftPanelControlUnitsComposition.Add("PS.026", new List<string> { "0eZ9VepF9FQO1sL1JkD$9c", "0eZ9VepF9FQO1sL1JkD$9d" }); // the second panel array has no real correspondence

            hall3DModelRightPanelControlUnitsComposition.Add("PD.001", new List<string> { "IfcPanel_001", "IfcPanel_002" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.002", new List<string> { "IfcPanel_003", "IfcPanel_004" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.003", new List<string> { "IfcPanel_005" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.004", new List<string> { "IfcPanel_006", "IfcPanel_007" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.005", new List<string> { "IfcPanel_008", "IfcPanel_009" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.006", new List<string> { "IfcPanel_010", "IfcPanel_011" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.007", new List<string> { "IfcPanel_012", "IfcPanel_013" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.008", new List<string> { "IfcPanel_014", "IfcPanel_015" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.009", new List<string> { "IfcPanel_016", "IfcPanel_017" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.010", new List<string> { "IfcPanel_018" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.011", new List<string> { "IfcPanel_019" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.012", new List<string> { "IfcPanel_020", "IfcPanel_021" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.013", new List<string> { "IfcPanel_022", "IfcPanel_023" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.014", new List<string> { "IfcPanel_024", "IfcPanel_025" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.015", new List<string> { "IfcPanel_026" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.016", new List<string> { "IfcPanel_027", "IfcPanel_028" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.017", new List<string> { "IfcPanel_029", "IfcPanel_030" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.018", new List<string> { "IfcPanel_031", "IfcPanel_032" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.019", new List<string> { "IfcPanel_033", "IfcPanel_034" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.020", new List<string> { "IfcPanel_035", "IfcPanel_036" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.021", new List<string> { "IfcPanel_037", "IfcPanel_038" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.022", new List<string> { "IfcPanel_039", "IfcPanel_040" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.023", new List<string> { "IfcPanel_041", "IfcPanel_042" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.024", new List<string> { "0eZ9VepF9FQO1sL1JkDygu", "0eZ9VepF9FQO1sL1JkD$9U" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.025", new List<string> { "0eZ9VepF9FQO1sL1JkD$92", "0eZ9VepF9FQO1sL1JkD$9v" });
            hall3DModelRightPanelControlUnitsComposition.Add("PD.026", new List<string> { "0eZ9VepF9FQO1sL1JkD$9c", "0eZ9VepF9FQO1sL1JkD$9d" }); // the second panel array has no real correspondence

            // initialize 3D model viewer
            Initialize3DModelViewer();

            // initialize dropdown items for the ComboNumericTextBox control
            InitializeDropdownItems();

            // lay form elements out
            foreach (TabPage tab in mainTabControl.TabPages)
            {
                LayOutComponents(tab);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // refresh component views of the current tab
            LayOutComponents(mainTabControl.SelectedTab);
        }
    }

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

        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, Path);
        }
    }
}