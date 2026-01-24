using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Hallbridger.Controls
{
    public partial class ModelElementPropertiesForm : Form
    {
        private Label namePrefixLabel;
        private Label nameValueLabel;
        private Label globalIdPrefixLabel;
        private Label globalIdValueLabel;
        private Label instanceTypePrefixLabel;
        private Label instanceTypeValueLabel;
        private Label typeNamePrefixLabel;
        private Label typeNameValueLabel;
        private Label controlUnitsPrefixLabel;
        private Label controlUnitsValueLabel;
        private Label positionAperturePrefixLabel;
        private ComboNumericTextBox positionApertureValueTextBox;

        // event to notify when value is confirmed (forwarded from ComboNumericTextBox object)
        public event EventHandler<ValueConfirmedEventArgs> ValueConfirmed;


        /* INITIALIZATION METHODS
         */

        public ModelElementPropertiesForm()
        {
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MinimizeBox = true;
            this.MaximizeBox = false;
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            this.Padding = new Padding(16, 16, 16, 16);

            namePrefixLabel = new Label
            {
                Left = 10,
                Top = 10,
                AutoSize = true,
                Font = new System.Drawing.Font(DefaultFont, System.Drawing.FontStyle.Bold)
            };
            nameValueLabel = new Label
            {
                Left = namePrefixLabel.Right + 4,
                Top = 10,
                AutoSize = true
            };

            globalIdPrefixLabel = new Label
            {
                Left = 10,
                Top = 35,
                AutoSize = true,
                Font = new System.Drawing.Font(DefaultFont, System.Drawing.FontStyle.Bold)
            };
            globalIdValueLabel = new Label
            {
                Left = globalIdPrefixLabel.Right + 4,
                Top = 35,
                AutoSize = true
            };

            instanceTypePrefixLabel = new Label
            {
                Left = 10,
                Top = 60,
                AutoSize = true,
                Font = new System.Drawing.Font(DefaultFont, System.Drawing.FontStyle.Bold)
            };
            instanceTypeValueLabel = new Label
            {
                Left = instanceTypePrefixLabel.Right + 4,
                Top = 60,
                AutoSize = true
            };

            typeNamePrefixLabel = new Label
            {
                Left = 10,
                Top = 85,
                AutoSize = true,
                Font = new System.Drawing.Font(DefaultFont, System.Drawing.FontStyle.Bold)
            };
            typeNameValueLabel = new Label
            {
                Left = typeNamePrefixLabel.Right + 4,
                Top = 85,
                AutoSize = true
            };

            controlUnitsPrefixLabel = new Label
            {
                Left = 10,
                Top = 110,
                AutoSize = true,
                Font = new System.Drawing.Font(DefaultFont, System.Drawing.FontStyle.Bold)
            };
            controlUnitsValueLabel = new Label
            {
                Left = controlUnitsPrefixLabel.Right + 4,
                Top = 110,
                AutoSize = true
            };

            positionAperturePrefixLabel = new Label
            {
                Left = 10,
                Top = 140,
                AutoSize = true,
                Font = new System.Drawing.Font(DefaultFont, System.Drawing.FontStyle.Bold)
            };

            positionApertureValueTextBox = new ComboNumericTextBox
            {
                Left = positionAperturePrefixLabel.Right + 4,
                Top = 140,
                Width = 200,
                Height = 24
            };
            positionApertureValueTextBox.ValueConfirmed += ComboNumericTextBox_ValueConfirmed;

            this.Controls.Add(namePrefixLabel);
            this.Controls.Add(nameValueLabel);
            this.Controls.Add(globalIdPrefixLabel);
            this.Controls.Add(globalIdValueLabel);
            this.Controls.Add(instanceTypePrefixLabel);
            this.Controls.Add(instanceTypeValueLabel);
            this.Controls.Add(typeNamePrefixLabel);
            this.Controls.Add(typeNameValueLabel);
            this.Controls.Add(controlUnitsPrefixLabel);
            this.Controls.Add(controlUnitsValueLabel);
            this.Controls.Add(positionAperturePrefixLabel);
            this.Controls.Add(positionApertureValueTextBox);
        }

        // fill 3D element property window with its data (only non-empty values)
        public void Fill3DElementProperties(string name, string globalId, string instanceType, string typeName, List<string> controlUnits, ComboNumericTextBox.ComboNumericValueType? valueType, decimal positionApertureValue, UnitOfMeasurement unitOfMeasurement, decimal minimumValue, decimal maximumValue, List<decimal> dropdownItems)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                this.Text = $"3D element properties: {name}"; // include name in form title
                namePrefixLabel.Text = "Name:";
                namePrefixLabel.Visible = true;
                nameValueLabel.Text = name;
                nameValueLabel.Visible = true;
                nameValueLabel.Left = namePrefixLabel.Left + namePrefixLabel.Width + 4;
            }
            else
            {
                this.Text = "3D element properties";
                namePrefixLabel.Visible = false;
                nameValueLabel.Visible = false;
            }

            if (!string.IsNullOrWhiteSpace(globalId))
            {
                globalIdPrefixLabel.Text = "Global ID:";
                globalIdPrefixLabel.Visible = true;
                globalIdValueLabel.Text = globalId;
                globalIdValueLabel.Visible = true;
                globalIdValueLabel.Left = globalIdPrefixLabel.Left + globalIdPrefixLabel.Width + 4;
            }
            else
            {
                globalIdPrefixLabel.Visible = false;
                globalIdValueLabel.Visible = false;
            }

            if (!string.IsNullOrWhiteSpace(instanceType))
            {
                instanceTypePrefixLabel.Text = "IFC instance type:";
                instanceTypePrefixLabel.Visible = true;
                instanceTypeValueLabel.Text = instanceType;
                instanceTypeValueLabel.Visible = true;
                instanceTypeValueLabel.Left = instanceTypePrefixLabel.Left + instanceTypePrefixLabel.Width + 4;
            }
            else
            {
                instanceTypePrefixLabel.Visible = false;
                instanceTypeValueLabel.Visible = false;
            }

            if (!string.IsNullOrWhiteSpace(typeName))
            {
                typeNamePrefixLabel.Text = "3D element type:";
                typeNamePrefixLabel.Visible = true;
                typeNameValueLabel.Text = typeName;
                typeNameValueLabel.Visible = true;
                typeNameValueLabel.Left = typeNamePrefixLabel.Left + typeNamePrefixLabel.Width + 4;
            }
            else
            {
                typeNamePrefixLabel.Visible = false;
                typeNameValueLabel.Visible = false;
            }

            string controlUnitsString = string.Join(", ", controlUnits);
            if (!string.IsNullOrWhiteSpace(controlUnitsString))
            {
                controlUnitsPrefixLabel.Text = "Control units:";
                controlUnitsPrefixLabel.Visible = true;
                controlUnitsValueLabel.Text = controlUnitsString;
                controlUnitsValueLabel.Visible = true;
                controlUnitsValueLabel.Left = controlUnitsPrefixLabel.Left + controlUnitsPrefixLabel.Width + 4;
            }
            else
            {
                controlUnitsPrefixLabel.Visible = false;
                controlUnitsValueLabel.Visible = false;
            }

            if (valueType.HasValue)
            {
                positionAperturePrefixLabel.Text =
                    valueType == ComboNumericTextBox.ComboNumericValueType.Position
                        ? "Position:"
                    : valueType == ComboNumericTextBox.ComboNumericValueType.Aperture
                        ? "Aperture:"
                    : "";
                positionApertureValueTextBox.ValueType = valueType;
                positionApertureValueTextBox.UnitOfMeasurement = unitOfMeasurement;
                positionApertureValueTextBox.Minimum = minimumValue;
                positionApertureValueTextBox.Maximum = maximumValue;
                positionApertureValueTextBox.NumericValue = positionApertureValue;
                positionApertureValueTextBox.DropdownItems = dropdownItems;
                positionAperturePrefixLabel.Visible = true;
                positionApertureValueTextBox.Visible = true;
            }
            else
            {
                positionAperturePrefixLabel.Visible = false;
                positionApertureValueTextBox.Visible = false;
            }

            positionApertureValueTextBox.UpdateLayout();
        }


        /* EVENT HANDLERS
         */

        // handler for value confirmed event from ComboNumericTextBox control
        private void ComboNumericTextBox_ValueConfirmed(object sender, ValueConfirmedEventArgs eventArgs)
        {
            // add control units and global ID to event args
            eventArgs.ControlUnits = controlUnitsValueLabel.Text
                .Replace("Control units: ", "")
                .Trim()
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim())
                .Where(k => !string.IsNullOrEmpty(k))
                .ToList();
            eventArgs.GlobalId = globalIdValueLabel.Text;

            // forward to caller form
            ValueConfirmed?.Invoke(this, eventArgs);
        }
    }
}