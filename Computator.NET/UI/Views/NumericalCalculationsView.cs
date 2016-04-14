using System;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Computator.NET.Config;

namespace Computator.NET.UI.Views
{
    public partial class NumericalCalculationsView : UserControl, INumericalCalculationsView
    {
        public NumericalCalculationsView()
        {
            InitializeComponent();
            if (DesignMode) return;
            function.DefaultCellStyle.Font = CustomFonts.GetMathFont(function.DefaultCellStyle.Font.Size);
            result.DefaultCellStyle.Font = CustomFonts.GetMathFont(result.DefaultCellStyle.Font.Size);

            resultNumericalCalculationsTextBox.Font =
                CustomFonts.GetMathFont(resultNumericalCalculationsTextBox.Font.Size);
        }

        public bool StepsVisible { set { stepsGroupBox.Visible = value; } }
        public bool ErrorVisible { set { maxErrorGroupBox.Visible = value; } }
        public bool IntervalVisible { set { intervalGroupBox.Visible = value; } }
        public bool DerrivativeVisible { set { derivativeAtPointGroupBox.Visible = value; } }

        public string SelectedMethod
        {
            get { return methodNumericalCalculationsComboBox.SelectedItem.ToString(); }
            set
            {
                methodNumericalCalculationsComboBox.SelectedItem = value;
            }
        }
        public string SelectedOperation
        {
            get { return operationNumericalCalculationsComboBox.SelectedItem.ToString(); }
            set
            {
                operationNumericalCalculationsComboBox.SelectedItem = value;
            }
        }
        public event EventHandler MethodChanged
        {
            add
            {
                methodNumericalCalculationsComboBox.SelectedIndexChanged += value;
            }
            remove { methodNumericalCalculationsComboBox.SelectedIndexChanged -= value; }
        }
        public event EventHandler OperationChanged
        {
            add
            {
                operationNumericalCalculationsComboBox.SelectedIndexChanged += value;
            }
            remove { operationNumericalCalculationsComboBox.SelectedIndexChanged -= value; }
        }
        public void SetMethods(object[] methods)
        {
            methodNumericalCalculationsComboBox.Items.Clear();
            methodNumericalCalculationsComboBox.Items.AddRange(methods);
        }

        public void SetOperations(object[] operations)
        {
            operationNumericalCalculationsComboBox.Items.Clear();
            operationNumericalCalculationsComboBox.Items.AddRange(operations);
        }

        public void AddResult(string expression, string operation, string method, string parameters, string result)
        {
            resultNumericalCalculationsTextBox.Text = result;
            numericalCalculationsDataGridView.Rows.Insert(0, expression, operation, method, parameters, result);
        }

        public double A { get { return (double)aIntervalNumericUpDown.Value; } }
        public double B { get { return (double)bIntervalNumericUpDown.Value; } }
        public double X { get { return (double)xDerivativePointNumericUpDown.Value; } }
        public uint Order { get { return (uint)nOrderDerivativeNumericUpDown.Value; } }
        public uint N { get { return (uint)nStepsNumericUpDown.Value; } }
        public double Epsilon
        {
            get
            {
                double eps;
                return double.TryParse(epsTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out eps) ? eps : double.NaN;
            }
        }

        public event EventHandler ComputeClicked
        {
            add { numericalOperationButton.Click += value; }
            remove { numericalOperationButton.Click -= value; }
        }
    }
}
