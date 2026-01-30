using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Hallbridger.Controls
{
    public class ComboNumericTextBox : UserControl
    {
        private Button dropdownButton;
        private bool isDropdownListOpened = false;
        private ToolStripDropDown toolStripDropdown;
        private ListBox dropdownListBox;
        private List<decimal> dropdownItems = new List<decimal>();
        private NumericUpDown numericUpDown;
        private ComboNumericValueType? valueType = null;
        private decimal lastValidValue;
        private ToolTip errorToolTip;
        private Timer repeatTimer;
        private const int repeatTimerInterval = 80; // milliseconds
        private RepeatDirection repeatDirection = RepeatDirection.None;
        private Label unitOfMeasurementLabel;
        private UnitOfMeasurement unitOfMeasurement;
        private Button applyButton;

        public enum ComboNumericValueType
        {
            Position,
            Aperture
        }
        private enum RepeatDirection
        {
            None,
            Up,
            Down
        }

        public event EventHandler<ValueConfirmedEventArgs> ValueConfirmed;


        /* PROPERTIES
         */

        [Browsable(true)]
        public List<decimal> DropdownItems
        {
            get => new List<decimal>(dropdownItems);
            set
            {
                dropdownItems = value ?? new List<decimal>();
                dropdownListBox.Items.Clear();
                foreach (var item in dropdownItems)
                {
                    dropdownListBox.Items.Add(item);
                }
            }
        }

        [Browsable(true)]
        public ComboNumericValueType? ValueType
        {
            get => valueType;
            set
            {
                valueType = value;
                switch (valueType)
                {
                    case ComboNumericValueType.Position:
                        numericUpDown.DecimalPlaces = 3;
                        numericUpDown.Increment = 0.001m;
                        break;
                    case ComboNumericValueType.Aperture:
                        numericUpDown.DecimalPlaces = 2;
                        numericUpDown.Increment = 0.01m;
                        break;
                }
            }
        }

        [Browsable(true)]
        public decimal NumericValue
        {
            get => numericUpDown.Value;
            set => numericUpDown.Value = value;
        }

        [Browsable(true)]
        public decimal Minimum
        {
            get => numericUpDown.Minimum;
            set => numericUpDown.Minimum = value;
        }

        [Browsable(true)]
        public decimal Maximum
        {
            get => numericUpDown.Maximum;
            set => numericUpDown.Maximum = value;
        }

        [Browsable(true)]
        public UnitOfMeasurement UnitOfMeasurement
        {
            get => unitOfMeasurement;
            set
            {
                unitOfMeasurement = value;
            }
        }


        /* INITIALIZATION METHODS
         */

        public ComboNumericTextBox()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            dropdownButton = new Button
            {
                Text = "▼",
                Left = 0,
                Top = 2,
                Width = 24,
                Height = 22,
                TabStop = false
            };
            dropdownButton.Click += DropdownButton_OnClick;

            dropdownListBox = new ListBox
            {
                BorderStyle = BorderStyle.None,
                IntegralHeight = false,
                Height = 100,
                Width = this.Width
            };
            dropdownListBox.Click += DropdownList_Click;

            var dropdownListHost = new ToolStripControlHost(dropdownListBox)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AutoSize = false
            };

            toolStripDropdown = new ToolStripDropDown
            {
                Padding = Padding.Empty
            };
            toolStripDropdown.Items.Add(dropdownListHost);

            numericUpDown = new NumericUpDown
            {
                Left = dropdownButton.Right,
                Top = 2,
                Width = 60,
                Height = 22
            };
            numericUpDown.KeyDown += NumericUpDown_KeyDown;
            numericUpDown.Controls[0].MouseDown += NumericUpDown_MouseDown;
            numericUpDown.Controls[0].MouseUp += NumericUpDown_MouseUp;

            repeatTimer = new Timer
            {
                Interval = repeatTimerInterval
            };
            repeatTimer.Tick += RepeatTimer_Tick;

            unitOfMeasurementLabel = new Label
            {
                TextAlign = ContentAlignment.MiddleLeft,
                Top = 2,
                Width = 18,
                Height = 16
            };

            errorToolTip = new ToolTip();

            applyButton = new Button
            {
                Width = 60,
                Height = 25,
                Top = 0
            };
            applyButton.Click += (s, e) => ConfirmValue();

            Controls.Add(dropdownButton);
            Controls.Add(numericUpDown);
            Controls.Add(unitOfMeasurementLabel);
            Controls.Add(applyButton);            

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        }


        /* EVENT HANDLERS
         */

        // dropdown event handlers
        private void DropdownButton_OnClick(object sender, EventArgs e)
        {
            if (!isDropdownListOpened)
            {
                ShowDropdownList();
            }
            else
            {
                HideDropdownList();
            }
        }

        private void DropdownList_Click(object sender, EventArgs e)
        {
            if (dropdownListBox.SelectedItem != null)
            {
                numericUpDown.Value = Convert.ToDecimal(dropdownListBox.SelectedItem);
                HideDropdownList();
            }
        }

        // numeric up-down event handlers
        private void NumericUpDown_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ConfirmValue();
                e.Handled = true;
            }
        }

        private void NumericUpDown_MouseDown(object sender, MouseEventArgs e)
        {
            // check which button was pressed
            var point = numericUpDown.PointToClient(Cursor.Position);
            var upButtonRectangle = new Rectangle(numericUpDown.Width - 18, 2, 16, numericUpDown.Height / 2 - 2);
            var downButtonRectangle = new Rectangle(numericUpDown.Width - 18, numericUpDown.Height / 2, 16, numericUpDown.Height / 2 - 2);

            if (upButtonRectangle.Contains(point))
                repeatDirection = RepeatDirection.Up;
            else if (downButtonRectangle.Contains(point))
                repeatDirection = RepeatDirection.Down;
            else
                repeatDirection = RepeatDirection.None;

            if (repeatDirection != RepeatDirection.None)
                repeatTimer.Start();
        }

        private void NumericUpDown_MouseUp(object sender, MouseEventArgs e)
        {
            repeatTimer.Stop();
            repeatDirection = RepeatDirection.None;
        }

        private void RepeatTimer_Tick(object sender, EventArgs e)
        {
            if (repeatDirection == RepeatDirection.Up)
            {
                if (numericUpDown.Value + numericUpDown.Increment <= numericUpDown.Maximum)
                    numericUpDown.Value += numericUpDown.Increment;
            }
            else if (repeatDirection == RepeatDirection.Down)
            {
                if (numericUpDown.Value - numericUpDown.Increment >= numericUpDown.Minimum)
                    numericUpDown.Value -= numericUpDown.Increment;
            }
        }


        /* AUXILIARY METHODS
         */

        // confirms and validates the input value
        private void ConfirmValue()
        {
            string text = numericUpDown.Text?.Trim();

            if (string.IsNullOrEmpty(text) || !decimal.TryParse(text, out decimal value))
            {
                numericUpDown.Value = lastValidValue;
                ShowError($"Invalid input. The value must be a number in the [{Minimum}-{Maximum}] interval.");
                return;
            }

            if (value == lastValidValue)
            {
                // no change
                return;
            }

            if (value < Minimum || value > Maximum)
            {
                numericUpDown.Value = lastValidValue;
                ShowError($"Input falls outside of the allowed interval: [{Minimum}-{Maximum}].");
                return;
            }

            // value validation
            numericUpDown.Value = value;
            lastValidValue = value;
            ValueConfirmed?.Invoke(this, new ValueConfirmedEventArgs(ValueType.Value, numericUpDown.Value));
        }

        // shows the dropdown list
        private void ShowDropdownList()
        {
            if (dropdownListBox.Items.Count == 0)
            {
                return;
            }

            var dropdownListlocation = this.PointToScreen(new Point(dropdownButton.Left, dropdownButton.Bottom));
            toolStripDropdown.Show(dropdownListlocation);
            isDropdownListOpened = true;
        }

        // hides the dropdown list
        private void HideDropdownList()
        {
            toolStripDropdown?.Close();
            isDropdownListOpened = false;
        }

        // shows an error tooltip near a control
        private void ShowError(string message)
        {
            errorToolTip.Show(message, numericUpDown, 0, numericUpDown.Height, 5000);
        }

        // updates layout elements to correctly show them in the UI
        public void SetUpComponent()
        {
            unitOfMeasurementLabel.Text =
                UnitOfMeasurement == UnitOfMeasurement.Meters
                    ? "m"
                : UnitOfMeasurement == UnitOfMeasurement.DecimalDegrees
                    ? "°"
                : "";
            unitOfMeasurementLabel.Left = numericUpDown.Right;

            applyButton.Text = "Apply";
            applyButton.Left = unitOfMeasurementLabel.Right + 12;

            lastValidValue = numericUpDown.Value;
        }
    }

    // event args for the ValueConfirmed event
    public class ValueConfirmedEventArgs : EventArgs
    {
        public List<string> ControlUnits
        {
            get;
            set;
        }      
        
        public string GlobalId
        {
            get;
            set;
        }

        public ComboNumericTextBox.ComboNumericValueType ValueType
        {
            get;
        }
        
        public decimal PositionApertureValue
        {
            get;
        }

        public ValueConfirmedEventArgs(ComboNumericTextBox.ComboNumericValueType valueType, decimal positionApertureValue)
        {
            ControlUnits = null;
            GlobalId = null;
            ValueType = valueType;
            PositionApertureValue = positionApertureValue;
        }
    }
}