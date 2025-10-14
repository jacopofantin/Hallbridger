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

namespace CURIOsity
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

    public partial class CuriosityForm : Form
    {
        // class-level dictionaries to store moving element data coming from the real hall in
        Dictionary<string, int> hallStagecraftEquipmentPositions = new Dictionary<string, int>();
        Dictionary<string, int> hallLeftPanelApertures = new Dictionary<string, int>();
        Dictionary<string, int> hallRightPanelApertures = new Dictionary<string, int>();

        // class-level dictionaries to store moving element data coming from the BIM model in
        Dictionary<string, int> bimStagecraftEquipmentPositions = new Dictionary<string, int>();
        Dictionary<string, int> bimLeftPanelApertures = new Dictionary<string, int>();
        Dictionary<string, int> bimRightPanelApertures = new Dictionary<string, int>();

        // class-level dictionaries to store correspondences between hall IDs and BIM model element names
        Dictionary<string, string> stagecraftEquipmentMapping = new Dictionary<string, string>();
        Dictionary<string, string> leftPanelMapping = new Dictionary<string, string>();
        Dictionary<string, string> rightPanelMapping = new Dictionary<string, string>();

        // class-level dictionary to store selected cells in DataGridViews (needed since both sorting and data refresh cause the user-defined selection to be lost)
        Dictionary<DataGridView, List<(string RowId, int ColumnIndex)>> selectedCells = new Dictionary<DataGridView, List<(string RowId, int ColumnIndex)>>();


        // INI file and timers for automatic file check and usage
        private string iniPath = "D:\\Dateien\\CURIOsity\\Configuration\\conf.ini";

        private System.Windows.Forms.Timer hallFileCheckTimer;
        private string hallFileCheckDirectory = "D:\\Dateien\\CURIOsity\\IO_files"; // default directory, can be changed in INI file
        private string hallFileCheckName = "Fotografia_sala_CURIO.txt"; // default file name, can be changed in INI file
        private int hallFileCheckInterval = 5000; // default to 5 seconds, can be changed in INI file
        private bool hallFileCheckActive = true; // default to true, can be changed in INI file

        private System.Windows.Forms.Timer bimFileCheckTimer;
        private string bimFileCheckDirectory = "D:\\Dateien\\CURIOsity\\IO_files"; // default directory, can be changed in INI file
        private string bimFileCheckName = "Model.ifc"; // default file name, can be changed in INI file
        private int bimFileCheckInterval = 5000; // default to 5 seconds, can be changed in INI file
        private bool bimFileCheckActive = true; // default to true, can be changed in INI file


        public CuriosityForm()
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

            // initialize BIM file timer if active
            if (bimFileCheckActive)
            {
                bimFileCheckTimer = new System.Windows.Forms.Timer();
                bimFileCheckTimer.Interval = bimFileCheckInterval;
                bimFileCheckTimer.Tick += BimFileCheckTimer_Tick;
                bimFileCheckTimer.Start();
            }
        }

        private async void HallFileCheckTimer_Tick(object sender, EventArgs e)
        {
            string filePath = Path.Combine(hallFileCheckDirectory, hallFileCheckName);
            if (File.Exists(filePath))
            {
                string apiUrl;
                string fileExtension = Path.GetExtension(filePath).ToLowerInvariant();

                switch (fileExtension)
                {
                    case ".txt":
                        apiUrl = "https://localhost:44307/api/import/txt";
                        break;
                    case ".xls":
                    case ".xlsx":
                        apiUrl = "https://localhost:44307/api/import/excel";
                        break;
                    case ".json":
                        apiUrl = "https://localhost:44307/api/import/json";
                        break;
                    case ".xml":
                        apiUrl = "https://localhost:44307/api/import/xml";
                        break;
                    default:
                        MessageBox.Show("File extension not supported: " + fileExtension);
                        return;
                }

                await LoadHallFile(apiUrl, filePath);
            }
        }

        private void BimFileCheckTimer_Tick(object sender, EventArgs e)
        {
            string filePath = Path.Combine(bimFileCheckDirectory, bimFileCheckName);
            if (File.Exists(filePath))
            {
                // file found, load it
                LoadBimFile(filePath);
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
                    string apiUrl;
                    string fileExtension = Path.GetExtension(loadHallDialog.FileName).ToLowerInvariant();

                    switch (fileExtension)
                    {
                        case ".txt":
                            apiUrl = "https://localhost:44307/api/import/txt";
                            break;
                        case ".xls":
                        case ".xlsx":
                            apiUrl = "https://localhost:44307/api/import/excel";
                            break;
                        case ".json":
                            apiUrl = "https://localhost:44307/api/import/json";
                            break;
                        case ".xml":
                            apiUrl = "https://localhost:44307/api/import/xml";
                            break;
                        default:
                            MessageBox.Show("File extension not supported: " + fileExtension);
                            return;
                    }

                    await LoadHallFile(apiUrl, loadHallDialog.FileName);
                }
            }
        }

        private void LoadBimButton_OnClick(object sender, EventArgs e)
        {
            using (OpenFileDialog loadBimDialog = new OpenFileDialog())
            {
                loadBimDialog.Filter = "BIM file (*.ifc)|*.ifc|All files (*.*)|*.*";
                if (loadBimDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadBimFile(loadBimDialog.FileName);
                }
            }
        }

        private void UpdateBimButton_OnClick(object sender, EventArgs e)
        {
            using (OpenFileDialog updateBimDialog = new OpenFileDialog())
            {
                updateBimDialog.Filter = "BIM file (*.ifc)|*.ifc|All files (*.*)|*.*";
                if (updateBimDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = updateBimDialog.FileName;
                    try
                    {
                        var editor = new XbimEditorCredentials
                        {
                            ApplicationDevelopersName = "Jacopo Fantin",
                            ApplicationFullName = "CURIOsity",
                            ApplicationIdentifier = "CURIOsity",
                            ApplicationVersion = "1.0",
                            EditorsFamilyName = "Fantin",
                            EditorsGivenName = "Jacopo",
                            EditorsOrganisationName = "Politecnico di Milano"
                        };

                        // open the BIM file
                        using (var model = IfcStore.Open(filePath, editor))
                        {/*
                            // start a transaction to modify the BIM file for left wall pivoting panel apertures
                            using (var leftPanelApertureUpdate = model.BeginTransaction("Left wall pivoting panel aperture update"))
                            {
                                //... extract left panel information into IEnumerable bimLeftPanels like we already did for the loadBimButton...

                                //bimLeftPanels: IEnumerable, leftPanelApertures: list
                                //to be used if the model does not have stagecraft equipment in it: in that case, it makes most sense to use lists for data imported from TXT since panels are identified from number 1 to 26 (we can forget about their IDs)
                                for (int i = 0; i < leftPanelApertures.Count; i++)
                                {
                                    var leftPanel = bimLeftPanels.ElementAt(i);
                                
                                    var aperture = leftPanel.IsDefinedBy
                                        .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                                        .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                                        .OfType<IIfcPropertySingleValue>()
                                        .FirstOrDefault(p => p.Name = "Aperture");

                                    // writes the aperture of each panel on the left wall in the BIM model
                                    aperture.NominalValue = new Xbim.Ifc4.MeasureResource.IfcLengthMeasure(leftPanelApertures[i]);
                                }
                                
                                //bimLeftPanels: IEnumerable, leftPanelMapping: dictionary<string, int> where key is the hall ID of the left wall panel and value is the index of the corresponding panel in bimLeftPanels
                                //to be used if the model actually has stagecraft equipment in it: in that case, we must use a dictionary for its information and in this case we wanna use dictionaries for all of the data we import from TXT
                                foreach (var corr in leftPanelMapping)
                                {
                                    var leftPanel = bimLeftPanels.ElementAt(corr.Value);

                                    var aperture = leftPanel.IsDefinedBy
                                        .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                                        .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                                        .OfType<IIfcPropertySingleValue>()
                                        .FirstOrDefault(p => p.Name = "Aperture");

                                    // writes the aperture of each panel on the left wall in the BIM model
                                    aperture.NominalValue = new Xbim.Ifc4.MeasureResource.IfcLengthMeasure(leftPanelApertures[corr.Key]);
                                }
                                
                                // commit the changes to the BIM file
                                leftPanelApertureUpdate.Commit();
                            }

                            // start a transaction to modify the BIM file for right wall pivoting panel apertures
                            using (var rightPanelApertureUpdate = model.BeginTransaction("Right wall pivoting panel aperture update"))
                            {
                                //... extract right panel information into IEnumerable bimRightPanels like we already did for the loadBimButton...

                                //bimRightPanels: IEnumerable, rightPanelApertures: list
                                //to be used if the model does not have stagecraft equipment in it: in that case, it makes most sense to use lists for data imported from TXT since panels are identified from number 1 to 26 (we can forget about their IDs)
                                for (int i = 0; i < rightPanelApertures.Count; i++)
                                {
                                    var rightPanel = bimRightPanels.ElementAt(i);

                                    var aperture = rightPanel.IsDefinedBy
                                        .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                                        .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                                        .OfType<IIfcPropertySingleValue>()
                                        .FirstOrDefault(p => p.Name = "Aperture");

                                    // writes the aperture of each panel on the right wall in the BIM model
                                    aperture.NominalValue = new Xbim.Ifc4.MeasureResource.IfcLengthMeasure(rightPanelApertures[i]);
                                }

                                //bimRightPanels: IEnumerable, rightPanelMapping: dictionary<string, int> where key is the hall ID of the right wall panel and value is the index of the corresponding panel in bimRightPanels
                                //to be used if the model actually has stagecraft equipment in it: in that case, we must use a dictionary for its information and in this case we wanna use dictionaries for all of the data we import from TXT
                                foreach (var corr in rightPanelMapping)
                                {
                                    var rightPanel = bimRightPanels.ElementAt(corr.Value);

                                    var aperture = rightPanel.IsDefinedBy
                                        .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                                        .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                                        .OfType<IIfcPropertySingleValue>()
                                        .FirstOrDefault(p => p.Name = "Aperture");

                                    // writes the aperture of each panel on the right wall in the BIM model
                                    aperture.NominalValue = new Xbim.Ifc4.MeasureResource.IfcLengthMeasure(rightPanelApertures[corr.Key]);
                                }

                                // commit the changes to the BIM file
                                rightPanelApertureUpdate.Commit();
                            }

                            // start a transaction to modify the BIM file for stagecraft equipment positions
                            using (var stagecraftEquipmentPositionsUpdate = model.BeginTransaction("Stagecraft equipment position update"))
                            {
                                //... extract stagecraft equipment information into IEnumerable bimStagecraftEquipment like we already did for the loadBimButton...

                                //bimStagecraftEquipment: IEnumerable, stagecraftEquipmentMapping: dictionary<string, int> where key is the hall ID of the stagecraft equipment piece and value is the index of the corresponding piece in bimStagecraftEquipment
                                foreach (var corr in stagecraftEquipmentMapping)
                                {
                                    var piece = bimStagecraftEquipment.ElementAt(corr.Value);

                                    var position = piece.IsDefinedBy
                                        .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                                        .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                                        .OfType<IIfcPropertySingleValue>()
                                        .FirstOrDefault(p => p.Name = "Position");

                                    // writes the position of each stagecraft equipment piece in the BIM model
                                    position.NominalValue = new Xbim.Ifc4.MeasureResource.IfcLengthMeasure(stagecraftEquipmentPositions[corr.Key]);
                                }

                                // commit the changes to the BIM file
                                stagecraftEquipmentPositionsUpdate.Commit();
                            }*/
                        }
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show("Error while reading BIM file: " + error.Message);
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
                var data = JsonConvert.DeserializeObject<CURIOsity_API.Models.CuriosityDataModel>(json);

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
                    hallStagecraftDataGridView.Rows.Add(pair.Key, $"{pair.Value} mm");
                }

                hallLeftPanelsDataGridView.Rows.Clear();
                foreach (var pair in hallLeftPanelApertures)
                {
                    hallLeftPanelsDataGridView.Rows.Add(pair.Key, $"{pair.Value} mm");
                }

                hallRightPanelsDataGridView.Rows.Clear();
                foreach (var pair in hallRightPanelApertures)
                {
                    hallRightPanelsDataGridView.Rows.Add(pair.Key, $"{pair.Value} mm");
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

                // if a BIM file has already been loaded, highlight discrepancies between hall and BIM data
                if (bimStagecraftDataGridView.Rows.Count > 0)
                {
                    foreach (var rel in stagecraftEquipmentMapping)
                    {
                        DataGridViewRow hallRow = hallStagecraftDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Key);
                        DataGridViewRow bimRow = bimStagecraftDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Value);

                        if (hallStagecraftEquipmentPositions[rel.Key] != bimStagecraftEquipmentPositions[rel.Value])
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
                        bimRow.DefaultCellStyle.BackColor = stagecraftCellColors[rel.Key];
                    }
                }

                if (bimLeftPanelsDataGridView.Rows.Count > 0)
                {
                    foreach (var rel in leftPanelMapping)
                    {
                        DataGridViewRow hallRow = hallLeftPanelsDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Key);
                        DataGridViewRow bimRow = bimLeftPanelsDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Value);

                        if (hallLeftPanelApertures[rel.Key] != bimLeftPanelApertures[rel.Value])
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
                        bimRow.DefaultCellStyle.BackColor = leftPanelsCellColors[rel.Key];
                    }
                }

                if (bimRightPanelsDataGridView.Rows.Count > 0)
                {
                    foreach (var rel in rightPanelMapping)
                    {
                        DataGridViewRow hallRow = hallRightPanelsDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Key);
                        DataGridViewRow bimRow = bimRightPanelsDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Value);

                        if (hallRightPanelApertures[rel.Key] != bimRightPanelApertures[rel.Value])
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
                        bimRow.DefaultCellStyle.BackColor = rightPanelsCellColors[rel.Key];
                    }
                }
            }
        }

        // method to import BIM data
        private void LoadBimFile(string filePath)
        {
            try
            {
                // save current scroll positions
                int bimStagecraftTopRowIndex = bimStagecraftDataGridView.FirstDisplayedScrollingRowIndex;
                int bimLeftPanelsTopRowIndex = bimLeftPanelsDataGridView.FirstDisplayedScrollingRowIndex;
                int bimRightPanelsTopRowIndex = bimRightPanelsDataGridView.FirstDisplayedScrollingRowIndex;

                // save current cell selection
                selectedCells[bimStagecraftDataGridView] = GetSelectedCells(bimStagecraftDataGridView);
                selectedCells[bimLeftPanelsDataGridView] = GetSelectedCells(bimLeftPanelsDataGridView);
                selectedCells[bimRightPanelsDataGridView] = GetSelectedCells(bimRightPanelsDataGridView);

                // save current data sorting
                DataGridViewColumn bimStagecraftSortedColumn = bimStagecraftDataGridView.SortedColumn;
                ListSortDirection? bimStagecraftSortDirection = null;
                if (bimStagecraftSortedColumn != null)
                {
                    bimStagecraftSortDirection = bimStagecraftDataGridView.SortOrder == SortOrder.Descending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                }

                DataGridViewColumn bimLeftPanelsSortedColumn = bimLeftPanelsDataGridView.SortedColumn;
                ListSortDirection? bimLeftPanelsSortDirection = null;
                if (bimLeftPanelsSortedColumn != null)
                {
                    bimLeftPanelsSortDirection = bimLeftPanelsDataGridView.SortOrder == SortOrder.Descending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                }

                DataGridViewColumn bimRightPanelsSortedColumn = bimRightPanelsDataGridView.SortedColumn;
                ListSortDirection? bimRightPanelsSortDirection = null;
                if (bimRightPanelsSortedColumn != null)
                {
                    bimRightPanelsSortDirection = bimRightPanelsDataGridView.SortOrder == SortOrder.Descending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                }

                // save current cell background colors
                Dictionary<string, Color> stagecraftCellColors = new Dictionary<string, Color>();
                foreach (DataGridViewRow row in bimStagecraftDataGridView.Rows)
                {
                    string rowId = row.Cells[0].Value?.ToString();
                    stagecraftCellColors[rowId] = row.Cells[0].Style.BackColor;
                }

                Dictionary<string, Color> leftPanelsCellColors = new Dictionary<string, Color>();
                foreach (DataGridViewRow row in bimLeftPanelsDataGridView.Rows)
                {
                    string rowId = row.Cells[0].Value?.ToString();
                    leftPanelsCellColors[rowId] = row.Cells[0].Style.BackColor;
                }

                Dictionary<string, Color> rightPanelsCellColors = new Dictionary<string, Color>();
                foreach (DataGridViewRow row in bimRightPanelsDataGridView.Rows)
                {
                    string rowId = row.Cells[0].Value?.ToString();
                    rightPanelsCellColors[rowId] = row.Cells[0].Style.BackColor;
                }

                // empty the class-level dictionaries
                bimStagecraftEquipmentPositions.Clear();
                bimLeftPanelApertures.Clear();
                bimRightPanelApertures.Clear();

                using (var model = IfcStore.Open(filePath))
                {
                    /*
                    //let's explore the model to find the correct entity for the pivoting panels (and the stagecraft equipment pieces, possibly)
                    var allInstances = model.Instances;
                    var interfaceNames = new HashSet<string>();
                    foreach (var instance in allInstances)
                    {
                        var type = instance.GetType();
                        interfaceNames.Add(type.Name);
                    }
                    foreach (var name in interfaceNames.OrderBy(n => n))
                    {
                        Console.WriteLine(name);
                    }

                    // get pivoting panels from the model (extracted data should be IEnumerable objects whose elements are sorted from 1 to 26)
                    //var pivotingPanels = model.Instances.OfType<IIfcPanel>();
                    //var bimLeftPanels = model.Instances.Where<IIfcPanel>(p => p.IsTypedBy.LeftSide());
                    //var bimRightPanels = model.Instances.Where<IIfcPanel>(p => p.IsTypedBy.RightSide());

                    //let's explore the panels in the IFC file to find the correct property set and property name for the pivoting panel aperture

                    //get information about one of the panels
                    //var firstPanel = model.Instances.FirstOrDefault<IIfcPanel>();
                    //Console.WriteLine($"Panel ID: {firstPanel.GlobalId}, Name: {firstPanel.Name}");

                    //get all single-value properties of the panel
                    //var properties = firstPanel.IsDefinedBy
                    //    .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                    //    .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                    //    .OfType<IIfcPropertySingleValue>();
                    //foreach (var property in properties)
                    //    Console.WriteLine($"Property: {property.Name}, Value: {property.NominalValue}");

                    // store panel names as keys and aperture values as values in the class-level dictionaries

                    foreach (var leftPanel in bimLeftPanels)
                    {
                        var aperture = leftPanel.IsDefinedBy
                            .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                            .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                            .OfType<IIfcPropertySingleValue>()
                            .Where(p => p.Name = "Aperture");

                        bimLeftPanelApertures.Add(leftPanel.Name, int.Parse(aperture.NominalValue));
                    }

                    foreach (var rightPanel in bimRightPanels)
                    {
                        var aperture = rightPanel.IsDefinedBy
                            .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                            .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                            .OfType<IIfcPropertySingleValue>()
                            .Where(p => p.Name = "Aperture");

                        bimRightPanelApertures.Add(rightPanel.Name, int.Parse(aperture.NominalValue));
                    }

                    // store stagecraft equipment names as keys and positions as values in the class-level dictionary
                    foreach (var piece in bimStagecraftEquipment)
                    {
                        var position = piece.IsDefinedBy
                            .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                            .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                            .OfType<IIfcPropertySingleValue>()
                            .Where(p => p.Name = "Position");

                        bimStagecraftEquipmentPositions.Add(piece.Name, int.Parse(position.NominalValue));
                    }*/
                }

                // empty and refill corresponding DataGridViews
                bimStagecraftDataGridView.Rows.Clear();
                foreach (var pair in bimStagecraftEquipmentPositions)
                {
                    string[] row = { pair.Key, $"{pair.Value} mm" };
                    bimStagecraftDataGridView.Rows.Add(row);
                }

                bimLeftPanelsDataGridView.Rows.Clear();
                foreach (var pair in bimLeftPanelApertures)
                {
                    string[] row = { pair.Key, $"{pair.Value} mm" };
                    bimStagecraftDataGridView.Rows.Add(row);
                }

                bimRightPanelsDataGridView.Rows.Clear();
                foreach (var pair in bimRightPanelApertures)
                {
                    string[] row = { pair.Key, $"{pair.Value} mm" };
                    bimStagecraftDataGridView.Rows.Add(row);
                }

                // restore previous scroll positions
                if (bimStagecraftTopRowIndex >= 0)
                {
                    bimStagecraftDataGridView.FirstDisplayedScrollingRowIndex = bimStagecraftTopRowIndex;
                }

                if (bimLeftPanelsTopRowIndex >= 0)
                {
                    bimLeftPanelsDataGridView.FirstDisplayedScrollingRowIndex = bimLeftPanelsTopRowIndex;
                }

                if (bimRightPanelsTopRowIndex >= 0)
                {
                    bimRightPanelsDataGridView.FirstDisplayedScrollingRowIndex = bimRightPanelsTopRowIndex;
                }

                // restore previous cell selection
                if (selectedCells.TryGetValue(bimStagecraftDataGridView, out var selection))
                {
                    RestoreSelectedCells(bimStagecraftDataGridView, selection);
                }

                if (selectedCells.TryGetValue(bimLeftPanelsDataGridView, out selection))
                {
                    RestoreSelectedCells(bimLeftPanelsDataGridView, selection);
                }

                if (selectedCells.TryGetValue(bimRightPanelsDataGridView, out selection))
                {
                    RestoreSelectedCells(bimRightPanelsDataGridView, selection);
                }

                // restore previous data sorting
                if (bimStagecraftSortedColumn != null && bimStagecraftSortDirection.HasValue)
                {
                    bimStagecraftDataGridView.Sort(bimStagecraftSortedColumn, bimStagecraftSortDirection.Value);
                }
                if (bimLeftPanelsSortedColumn != null && bimLeftPanelsSortDirection.HasValue)
                {
                    bimLeftPanelsDataGridView.Sort(bimLeftPanelsSortedColumn, bimLeftPanelsSortDirection.Value);
                }
                if (bimRightPanelsSortedColumn != null && bimRightPanelsSortDirection.HasValue)
                {
                    bimRightPanelsDataGridView.Sort(bimRightPanelsSortedColumn, bimRightPanelsSortDirection.Value);
                }

                // if a hall file has already been loaded, highlight discrepancies between hall and BIM data
                if (hallStagecraftDataGridView.Rows.Count > 0)
                {
                    foreach (var rel in stagecraftEquipmentMapping)
                    {
                        DataGridViewRow hallRow = hallStagecraftDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Key);
                        DataGridViewRow bimRow = bimStagecraftDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Value);

                        if (hallStagecraftEquipmentPositions[rel.Key] != bimStagecraftEquipmentPositions[rel.Value])
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
                        bimRow.DefaultCellStyle.BackColor = stagecraftCellColors[rel.Key];
                    }
                }

                if (hallLeftPanelsDataGridView.Rows.Count > 0)
                {
                    foreach (var rel in leftPanelMapping)
                    {
                        DataGridViewRow hallRow = hallLeftPanelsDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Key);
                        DataGridViewRow bimRow = bimLeftPanelsDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Value);

                        if (hallLeftPanelApertures[rel.Key] != bimLeftPanelApertures[rel.Value])
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
                        bimRow.DefaultCellStyle.BackColor = leftPanelsCellColors[rel.Key];
                    }
                }

                if (hallRightPanelsDataGridView.Rows.Count > 0)
                {
                    foreach (var rel in rightPanelMapping)
                    {
                        DataGridViewRow hallRow = hallRightPanelsDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Key);
                        DataGridViewRow bimRow = bimRightPanelsDataGridView.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.Cells[0].Value != null && r.Cells[0].Value.ToString() == rel.Value);

                        if (hallRightPanelApertures[rel.Key] != bimRightPanelApertures[rel.Value])
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
                        bimRow.DefaultCellStyle.BackColor = rightPanelsCellColors[rel.Key];
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show("Error while reading BIM file: " + error.Message);
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

        // auxiliary form element layout methods
        private void LayOutElements()
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
            int availableWidth = this.ClientSize.Width - 40;
            int DataGridViewWidth = (availableWidth - 2 * DataGridViewSpacing) / 3;
            int hallGroupHeight = groupTitleHeight + titleSpacing + labelHeight + labelSpacing;
            int bimGroupHeight = groupTitleHeight + titleSpacing + labelHeight + labelSpacing;
            int separatorMargin = groupSpacing / 2 + separatorHeight + 8;
            int totalDataGridViewHeight = this.ClientSize.Height - topMargin - hallGroupHeight - bimGroupHeight - separatorMargin - buttonHeight - buttonBottomMargin;
            int DataGridViewHeight = totalDataGridViewHeight / 2; //each group takes half of the available space

            // place elements
            PlaceGroupsAndSeparator(topMargin, groupTitleHeight, titleSpacing, labelHeight, labelSpacing, groupSpacing, separatorHeight, startX, availableWidth, hallGroupHeight, DataGridViewHeight);
            PlaceDataGridViews(topMargin, groupTitleHeight, titleSpacing, labelHeight, labelSpacing, DataGridViewSpacing, startX, DataGridViewWidth, DataGridViewHeight, groupSpacing, separatorHeight);
            PlaceButtons(startX, DataGridViewWidth, DataGridViewSpacing, buttonHeight);

            // adapt DataGridView columns to available width
            FitColumnsToWidth(hallStagecraftDataGridView);
            FitColumnsToWidth(hallLeftPanelsDataGridView);
            FitColumnsToWidth(hallRightPanelsDataGridView);
            FitColumnsToWidth(bimStagecraftDataGridView);
            FitColumnsToWidth(bimLeftPanelsDataGridView);
            FitColumnsToWidth(bimRightPanelsDataGridView);
        }

        private void PlaceGroupsAndSeparator(int topMargin, int groupTitleHeight, int titleSpacing, int labelHeight, int labelSpacing, int groupSpacing, int separatorHeight, int startX, int availableWidth, int hallGroupHeight, int DataGridViewHeight)
        {
            // hall group title
            hallDataGroupLabel.Left = (this.ClientSize.Width - hallDataGroupLabel.Width) / 2;
            hallDataGroupLabel.Top = topMargin;
            int hallDataGridViewY = topMargin + groupTitleHeight + titleSpacing + labelHeight + labelSpacing;

            // horizontal separator
            int separatorY = hallDataGridViewY + DataGridViewHeight + groupSpacing / 2;
            groupSeparator.Left = startX;
            groupSeparator.Top = separatorY;
            groupSeparator.Width = availableWidth;
            groupSeparator.Height = separatorHeight;

            // BIM group title
            int bimStartY = separatorY + separatorHeight + 8;
            bimDataGroupLabel.Left = (this.ClientSize.Width - bimDataGroupLabel.Width) / 2;
            bimDataGroupLabel.Top = bimStartY;
        }

        private void PlaceDataGridViews(int topMargin, int groupTitleHeight, int titleSpacing, int labelHeight, int labelSpacing, int DataGridViewSpacing, int startX, int DataGridViewWidth, int DataGridViewHeight, int groupSpacing, int separatorHeight)
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

            // BIM DataGridView labels
            int separatorY = hallDataGridViewY + DataGridViewHeight + groupSpacing / 2;
            int bimStartY = separatorY + separatorHeight + 8;
            int bimLabelY = bimStartY + groupTitleHeight + titleSpacing;
            bimStagecraftDataGridViewLabel.Left = startX;
            bimStagecraftDataGridViewLabel.Top = bimLabelY;
            bimLeftPanelsDataGridViewLabel.Left = startX + DataGridViewWidth + DataGridViewSpacing;
            bimLeftPanelsDataGridViewLabel.Top = bimLabelY;
            bimRightPanelsDataGridViewLabel.Left = startX + (DataGridViewWidth + DataGridViewSpacing) * 2;
            bimRightPanelsDataGridViewLabel.Top = bimLabelY;

            // BIM DataGridViews
            int bimDataGridViewY = bimLabelY + labelHeight + labelSpacing;
            bimStagecraftDataGridView.Left = startX;
            bimStagecraftDataGridView.Top = bimDataGridViewY;
            bimStagecraftDataGridView.Width = DataGridViewWidth;
            bimStagecraftDataGridView.Height = DataGridViewHeight;

            bimLeftPanelsDataGridView.Left = bimLeftPanelsDataGridViewLabel.Left;
            bimLeftPanelsDataGridView.Top = bimDataGridViewY;
            bimLeftPanelsDataGridView.Width = DataGridViewWidth;
            bimLeftPanelsDataGridView.Height = DataGridViewHeight;

            bimRightPanelsDataGridView.Left = bimRightPanelsDataGridViewLabel.Left;
            bimRightPanelsDataGridView.Top = bimDataGridViewY;
            bimRightPanelsDataGridView.Width = DataGridViewWidth;
            bimRightPanelsDataGridView.Height = DataGridViewHeight;
        }

        private void PlaceButtons(int startX, int DataGridViewWidth, int DataGridViewSpacing, int buttonHeight)
        {
            // horizontally centering buttons and keeping them one beside the other at the bottom of the form
            int buttonSpacing = 16; // distance between buttons
            int buttonsTotalWidth = loadHallButton.Width + buttonSpacing + loadBimButton.Width + buttonSpacing + updateBimButton.Width;
            int buttonsStartX = (this.ClientSize.Width - buttonsTotalWidth) / 2;

            int dataGridViewsBottom = bimStagecraftDataGridView.Top + bimStagecraftDataGridView.Height;
            int tabPageBottom = movingElementDataTab.Height; // bottom end of TabPage
            int buttonsY = dataGridViewsBottom + (tabPageBottom - dataGridViewsBottom - buttonHeight) / 2; // bottons in between DataGridViews end and form bottom end

            loadHallButton.Left = buttonsStartX;
            loadHallButton.Top = buttonsY;

            loadBimButton.Left = buttonsStartX + loadHallButton.Width + buttonSpacing;
            loadBimButton.Top = buttonsY;

            updateBimButton.Left = loadBimButton.Left + loadBimButton.Width + buttonSpacing;
            updateBimButton.Top = buttonsY;
        }

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

                // BIM file check parameters
                bimFileCheckDirectory = iniFile.Read("BimFileCheck", "Directory");
                bimFileCheckName = iniFile.Read("BimFileCheck", "FileName");
                int.TryParse(iniFile.Read("BimFileCheck", "Interval"), out bimFileCheckInterval);
                bool.TryParse(iniFile.Read("BimFileCheck", "Active"), out bimFileCheckActive);
            }

            // initialize timers
            InitTimers();

            // hard-coded correspondences between hall IDs and BIM model element names
            stagecraftEquipmentMapping.Add("TM.001", "Truss Motor 1"); // hypothetical BIM element name
            stagecraftEquipmentMapping.Add("TM.002", "Truss Motor 2"); // hypothetical BIM element name
            stagecraftEquipmentMapping.Add("TM.003", "Truss Motor 3"); // hypothetical BIM element name
            stagecraftEquipmentMapping.Add("TM.004", "Truss Motor 4"); // hypothetical BIM element name
            stagecraftEquipmentMapping.Add("TS.001", "Truss Stopper 1"); // hypothetical BIM element name
            stagecraftEquipmentMapping.Add("TP.001", "Truss Position 1"); // hypothetical BIM element name
            stagecraftEquipmentMapping.Add("TP.002", "Truss Position 2"); // hypothetical BIM element name
            stagecraftEquipmentMapping.Add("TP.003", "Truss Position 3"); // hypothetical BIM element name
            stagecraftEquipmentMapping.Add("TP.004", "Truss Position 4"); // hypothetical BIM element name
            stagecraftEquipmentMapping.Add("TT.001", "Truss Tension 1"); // hypothetical BIM element name
            stagecraftEquipmentMapping.Add("TD.001", "Truss Distance 1"); // hypothetical BIM element name
            stagecraftEquipmentMapping.Add("TA.001", "Truss Angle 1"); // hypothetical BIM element name
            stagecraftEquipmentMapping.Add("PO.001", "Platform Opening 1"); // hypothetical BIM element name

            leftPanelMapping.Add("PS.001", "Left Panel 1"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.002", "Left Panel 2"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.003", "Left Panel 3"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.004", "Left Panel 4"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.005", "Left Panel 5"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.006", "Left Panel 6"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.007", "Left Panel 7"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.008", "Left Panel 8"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.009", "Left Panel 9"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.010", "Left Panel 10"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.011", "Left Panel 11"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.012", "Left Panel 12"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.013", "Left Panel 13"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.014", "Left Panel 14"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.015", "Left Panel 15"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.016", "Left Panel 16"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.017", "Left Panel 17"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.018", "Left Panel 18"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.019", "Left Panel 19"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.020", "Left Panel 20"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.021", "Left Panel 21"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.022", "Left Panel 22"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.023", "Left Panel 23"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.024", "Left Panel 24"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.025", "Left Panel 25"); // hypothetical BIM element name
            leftPanelMapping.Add("PS.026", "Left Panel 26"); // hypothetical BIM element name

            rightPanelMapping.Add("PD.001", "Right Panel 1"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.002", "Right Panel 2"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.003", "Right Panel 3"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.004", "Right Panel 4"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.005", "Right Panel 5"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.006", "Right Panel 6"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.007", "Right Panel 7"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.008", "Right Panel 8"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.009", "Right Panel 9"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.010", "Right Panel 10"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.011", "Right Panel 11"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.012", "Right Panel 12"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.013", "Right Panel 13"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.014", "Right Panel 14"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.015", "Right Panel 15"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.016", "Right Panel 16"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.017", "Right Panel 17"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.018", "Right Panel 18"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.019", "Right Panel 19"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.020", "Right Panel 20"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.021", "Right Panel 21"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.022", "Right Panel 22"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.023", "Right Panel 23"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.024", "Right Panel 24"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.025", "Right Panel 25"); // hypothetical BIM element name
            rightPanelMapping.Add("PD.026", "Right Panel 26"); // hypothetical BIM element name

            // lay form elements out
            LayOutElements();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayOutElements();
        }
    }
}