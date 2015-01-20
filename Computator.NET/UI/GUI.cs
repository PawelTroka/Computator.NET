using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Accord.Math;
using Computator.NET.Benchmarking;
using Computator.NET.Charting;
using Computator.NET.Charting.Chart3D;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.Config;
using Computator.NET.Evaluation;
using Computator.NET.Functions;
using Computator.NET.Localization;
using Computator.NET.NumericalCalculations;
using Computator.NET.NumericalCalculations.ElementaryMathematics;
using Computator.NET.NumericalCalculations.MathematicalAnalysis;
using Computator.NET.UI;
using Computator.NET.UI.AutocompleteMenu;
using EditChartWindow = Computator.NET.Charting.RealCharting.EditChartWindow;
using Settings = Computator.NET.Properties.Settings;

namespace Computator.NET
{
    public partial class GUI : LocalizedForm
    {
        private readonly CultureInfo[] AllCultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
        private readonly FunctionComplexEvaluator complexEvaluator;
        private readonly Function2DEvaluator evaluator2d;
        private readonly Function3DEvaluator evaluator3d;
        private readonly WebBrowserForm menuFunctionsToolTip;

        private Chart2D chart2d;
        private Chart3DControl chart3d;
        private ComplexChart complexChart;

        private ScriptingRichTextBox customFunctionsTextBox;

        private List<Action<object, EventArgs>> defaultActions;
        private ElementHost elementHostChart3d;
        private ScriptEvaluator scriptEvaluator;
        private ScriptingRichTextBox scriptingTextBox;

        #region initialization and construction

        public GUI()
        {
            evaluator2d = new Function2DEvaluator();
            evaluator3d = new Function3DEvaluator();
            complexEvaluator = new FunctionComplexEvaluator();
            scriptEvaluator = new ScriptEvaluator();
            menuFunctionsToolTip = new WebBrowserForm();
            //menuFunctionsToolTip.AutoClose = true;
            SetCulture();
            Task.Factory.StartNew(CopyDllFiles);
            InitializeComponent();
            InitializeFunctions();
            InitializeCharts(); //takes more time then it should
            expressionTextBox.refreshAutoComplete();
            setupAllComboBoxes();
                //System.Threading.Tasks.Task.Factory.StartNew(setupAllComboBoxes);//setupAllComboBoxes();
            attachEventHandlers();
            toolStripStatusLabel1.Text = GlobalConfig.version;
            customFunctionsDirectoryTree.Drive = GlobalConfig.fullPath("TSL Examples", "_CustomFunctions");
            directoryTree1.Drive = GlobalConfig.fullPath("TSL Examples", "_Scripts");
            UpdateXyRatio();
            InitializeScripting(); //takes a lot of time, TODO: optimize
            setMathFonts();
            InitializeDataBindings();
        }

        private void setMathFonts()
        {
            // GlobalConfig.changeFontToMathFont(expressionTextBox,chart2d);
            //expressionTextBox.changeAutocompleteFontToMathFont();

            //calculationsHistoryDataGridView.DefaultCellStyle.Font = new Font(calculationsHistoryDataGridView.DefaultCellStyle.Font.FontFamily, 12);
            //numericalCalculationsDataGridView.DefaultCellStyle.Font = new Font(numericalCalculationsDataGridView.DefaultCellStyle.Font.FontFamily, 12);

            chart2d.Legends[0].Font = GlobalConfig.GetMathFont(chart2d.Legends[0].Font.Size);

            const float fontsize = 18.0F;
            expressionTextBox.changeAutocompleteFontToMathFont(fontsize);
            expressionTextBox.Font = GlobalConfig.GetMathFont(fontsize);
            chart2d.Font = GlobalConfig.GetMathFont(fontsize);
            // GlobalConfig.changeFontToMathFont(this.numericalCalculationsDataGridView,this.calculationsHistoryDataGridView);
            //  result
            function.DefaultCellStyle.Font = GlobalConfig.GetMathFont(function.DefaultCellStyle.Font.Size);
            //this..DefaultCellStyle.Font = GlobalConfig.GetMathFont(this.function.DefaultCellStyle.Font.Size);

            // this.result.DefaultCellStyle.Font = GlobalConfig.GetMathFont(this.parameters.DefaultCellStyle.Font.Size);
            //  this.parameters.DefaultCellStyle.Font = GlobalConfig.GetMathFont(this.result.DefaultCellStyle.Font.Size);

            // for(int i=0;i<3;i++)
            calculationsHistoryDataGridView.Columns[0].DefaultCellStyle.Font =
                GlobalConfig.GetMathFont(calculationsHistoryDataGridView.Columns[0].DefaultCellStyle.Font.Size);
        }

        private void InitializeFunctions()
        {
            /* var relogarithms = new ToolStripMenuItem("Logarithms");
             var logarithms = new ToolStripMenuItem("log2(x)");
             logarithms.ToolTipText = "chuj chuj chuj chuj";
             logarithms.Click += (s, e) => { expressionTextBox.AppendText((s as ToolStripMenuItem).Text); };
             relogarithms.DropDownItems.Add(logarithms);
             elementaryFunctionsToolStripMenuItem.DropDownItems.Add(relogarithms);
             */
            KeyValuePair<string, FunctionInfo>[] functions = GlobalConfig.functionsDetails.ToArray();


            var dict = new Dictionary<string, ToolStripMenuItem>
            {
                {"ElementaryFunctions", elementaryFunctionsToolStripMenuItem},
                {"SpecialFunctions", specialFunctionsToolStripMenuItem},
                {"MathematicalConstants", mathematicalConstantsToolStripMenuItem},
                {"PhysicalConstants", physicalConstantsToolStripMenuItem}
            };

            foreach (var f in functions)
            {
                if (f.Value.Category == "")
                    f.Value.Category = "_empty_";

                if (!dict[f.Value.Type].DropDownItems.ContainsKey(f.Value.Category))
                {
                    var cat = new ToolStripMenuItem(f.Value.Category);
                    cat.Name = f.Value.Category;
                    dict[f.Value.Type].DropDownItems.Add(cat);
                }

                var item = new ToolStripMenuItem();
                item.Text = f.Value.Signature;
                item.ToolTipText = f.Value.Title;
                //item.Click += Item_Click;
                item.MouseDown += Item_Click;

                (dict[f.Value.Type].DropDownItems[f.Value.Category] as ToolStripMenuItem).DropDownItems.Add(item);
            }
        }

        private void Item_Click(object sender, MouseEventArgs e)
        {
            var menuItem = (sender as ToolStripMenuItem);
            if (e.Button == MouseButtons.Left)
                expressionTextBox.AppendText(menuItem.Text);
            else if (e.Button == MouseButtons.Right)
            {
                if (GlobalConfig.functionsDetails.ContainsKey(menuItem.Text))
                {
                    menuFunctionsToolTip.setFunctionInfo(GlobalConfig.functionsDetails[menuItem.Text]);
                    //menuFunctionsToolTip.Show(this, menuItem.Width + 3, 0);
                    menuFunctionsToolTip.Show();
                }
            }
        }


        private void InitializeDataBindings()
        {
            //  y0NumericUpDown.DataBindings.Add("Value", chart2d, "YMin", false, DataSourceUpdateMode.OnPropertyChanged);
            //  yNNumericUpDown.DataBindings.Add("Value", chart2d, "YMax", false, DataSourceUpdateMode.OnPropertyChanged);
            //  x0NumericUpDown.DataBindings.Add("Value", chart2d, "XMin", false, DataSourceUpdateMode.OnPropertyChanged);
            //  xnNumericUpDown.DataBindings.Add("Value", chart2d, "XMax", false, DataSourceUpdateMode.OnPropertyChanged);

            //   y0NumericUpDown.DataBindings[0].ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;
            //    yNNumericUpDown.DataBindings[0].ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;
            //    x0NumericUpDown.DataBindings[0].ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;
            //   xnNumericUpDown.DataBindings[0].ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;

            //chart2d.DataBindings.Add("YMin", y0NumericUpDown, "Value", false, DataSourceUpdateMode.OnPropertyChanged);
            // chart2d.DataBindings.Add("YMax", yNNumericUpDown, "Value", false, DataSourceUpdateMode.OnPropertyChanged);
            //chart2d.DataBindings.Add("XMin", x0NumericUpDown, "Value", false, DataSourceUpdateMode.OnPropertyChanged);
            //chart2d.DataBindings.Add("XMax", xnNumericUpDown, "Value", false, DataSourceUpdateMode.OnPropertyChanged);


            //chart2d.DataBindings.Add("YMin", y0NumericUpDown, "Value", false, DataSourceUpdateMode.OnPropertyChanged);
            //   chart2d.DataBindings.Add("YMax", yNNumericUpDown, "Value", false, DataSourceUpdateMode.OnPropertyChanged);
            //  chart2d.DataBindings.Add("XMin", x0NumericUpDown, "Value", false, DataSourceUpdateMode.OnPropertyChanged);
            //   chart2d.DataBindings.Add("XMax", xnNumericUpDown, "Value", false, DataSourceUpdateMode.OnPropertyChanged);


            //working OK
            y0NumericUpDown.DataBindings.Add("Value", chart3d, "YMin", false, DataSourceUpdateMode.OnPropertyChanged);
            yNNumericUpDown.DataBindings.Add("Value", chart3d, "YMax", false, DataSourceUpdateMode.OnPropertyChanged);
            x0NumericUpDown.DataBindings.Add("Value", chart3d, "XMin", false, DataSourceUpdateMode.OnPropertyChanged);
            xnNumericUpDown.DataBindings.Add("Value", chart3d, "XMax", false, DataSourceUpdateMode.OnPropertyChanged);
            //working OK
            complexChart.DataBindings.Add("YMin", y0NumericUpDown, "Value", false,
                DataSourceUpdateMode.OnPropertyChanged);
            complexChart.DataBindings.Add("YMax", yNNumericUpDown, "Value", false,
                DataSourceUpdateMode.OnPropertyChanged);
            complexChart.DataBindings.Add("XMin", x0NumericUpDown, "Value", false,
                DataSourceUpdateMode.OnPropertyChanged);
            complexChart.DataBindings.Add("XMax", xnNumericUpDown, "Value", false,
                DataSourceUpdateMode.OnPropertyChanged);

            //     chart2d.DataBindings.Add("YMin", chart3d, "YMin", false, DataSourceUpdateMode.OnPropertyChanged);
            //      chart2d.DataBindings.Add("YMax", chart3d, "YMax", false, DataSourceUpdateMode.OnPropertyChanged);
            //     chart2d.DataBindings.Add("XMin", chart3d, "XMin", false, DataSourceUpdateMode.OnPropertyChanged);
            //    chart2d.DataBindings.Add("XMax", chart3d, "XMax", false, DataSourceUpdateMode.OnPropertyChanged);

            BindField(chart2d, "YMin", y0NumericUpDown, "Value");
            BindField(chart2d, "YMax", yNNumericUpDown, "Value");
            BindField(chart2d, "XMin", x0NumericUpDown, "Value");
            BindField(chart2d, "XMax", xnNumericUpDown, "Value");
            //   chart2d.DataBindings.Add("YMin", y0NumericUpDown, "Value", false, DataSourceUpdateMode.OnPropertyChanged);
            //     chart2d.DataBindings.Add("YMax", yNNumericUpDown, "Value", false, DataSourceUpdateMode.OnPropertyChanged);
            //    chart2d.DataBindings.Add("XMin", x0NumericUpDown, "Value", false, DataSourceUpdateMode.OnPropertyChanged);
            //    chart2d.DataBindings.Add("XMax", xnNumericUpDown, "Value", false, DataSourceUpdateMode.OnPropertyChanged);

            chart2d.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == "XyRatio")
                    XYRatioToolStripStatusLabel.Text = String.Format(Strings.XYRatio0, chart2d.XyRatio);
            };
        }

        public static void BindField(Control control, string propertyName,
            object dataSource, string dataMember)
        {
            Binding bd;

            for (int index = control.DataBindings.Count - 1; (index == 0); index--)
            {
                bd = control.DataBindings[index];
                if (bd.PropertyName == propertyName)
                    control.DataBindings.Remove(bd);
            }
            control.DataBindings.Add(propertyName, dataSource, dataMember, false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void InitializeCharts()
        {
            chart2d = new Chart2D();
            complexChart = new ComplexChart();
            chart3d = new Chart3DControl();
            elementHostChart3d = new ElementHost();
            elementHostChart3d.BackColor = Color.White;
            elementHostChart3d.Dock = DockStyle.Fill;
            elementHostChart3d.Child = chart3d;
            ((ISupportInitialize) (chart2d)).BeginInit();

            panel2.Controls.Add(chart2d);
            panel2.Controls.Add(complexChart);
            panel2.Controls.Add(elementHostChart3d);
            chart2d.BringToFront();
            complexChart.BringToFront();
            elementHostChart3d.BringToFront();
            complexChart.Visible = false;
            elementHostChart3d.Visible = false;
            ((ISupportInitialize) (chart2d)).EndInit();
            chart2d.setChartAreaValues((double) x0NumericUpDown.Value, (double) xnNumericUpDown.Value,
                (double) y0NumericUpDown.Value, (double) yNNumericUpDown.Value);
            complexChart.setChartAreaValues((double) x0NumericUpDown.Value, (double) xnNumericUpDown.Value,
                (double) y0NumericUpDown.Value, (double) yNNumericUpDown.Value);
        }

        private void InitializeScripting()
        {
            scriptingTextBox = new ScriptingRichTextBox();
            splitContainer2.Panel1.Controls.Add(scriptingTextBox);
            scriptingTextBox.Name = "scriptingTextBox";
            scriptingTextBox.Dock = DockStyle.Fill;

            customFunctionsTextBox = new ScriptingRichTextBox();
            splitContainer3.Panel1.Controls.Add(customFunctionsTextBox);
            customFunctionsTextBox.Dock = DockStyle.Fill;
            customFunctionsTextBox.Name = "customFunctionsTextBox";
        }

        private void attachEventHandlers()
        {
            Resize += (o, e) => UpdateXyRatio();
            defaultActions = new List<Action<object, EventArgs>>
            {
                addToChartButton_Click,
                calculateButton_Click,
                numericalOperationButton_Click,
                symbolicOperationButton_Click
            };
        }

        private void setupAllComboBoxes()
        {
            chart2d.setupComboBoxes(new[]
            {
                typeOfChartComboBox, seriesOfChartComboBox, colorsOfChartComboBox, positionLegendComboBox,
                aligmentLegendComboBox
            });
            complexChart.setupComboBoxes(countourLinesToolStripComboBox, colorAssignmentToolStripComboBox);

            NumericalCalculation.setupOperations(operationNumericalCalculationsComboBox);
            NumericalCalculation.setupMethods(methodNumericalCalculationsComboBox,
                operationNumericalCalculationsComboBox);
            NumericalCalculation.setupGroupBoxes(operationNumericalCalculationsComboBox, derivativeAtPointGroupBox,
                rootOfFunctionGroupBox, numericalIntegrationGroupBox);


            languageToolStripComboBox.Items.Add((new CultureInfo("en")).NativeName);
            languageToolStripComboBox.Items.Add((new CultureInfo("pl")).NativeName);
            languageToolStripComboBox.Items.Add((new CultureInfo("de")).NativeName);
            languageToolStripComboBox.Items.Add((new CultureInfo("cs")).NativeName);


            languageToolStripComboBox.AutoSize = true;
            languageToolStripComboBox.Invalidate();

            languageToolStripComboBox.SelectedItem =
                Enumerable.First(AllCultures, c => c.TwoLetterISOLanguageName == Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName)
                    .NativeName;
        }

        private static void SetCulture()
        {
            Thread.CurrentThread.CurrentCulture = Settings.Default.Language;
            Thread.CurrentThread.CurrentUICulture = Settings.Default.Language;
        }

        private static void CopyDllFiles()
        {
            if (!File.Exists(GlobalConfig.basePath + GlobalConfig.gslDllName))
                File.Copy(GlobalConfig.gslLibPath, GlobalConfig.basePath + GlobalConfig.gslDllName);
            if (!File.Exists(GlobalConfig.basePath + GlobalConfig.gslCblasDllName))
                File.Copy(GlobalConfig.gslBlasPath, GlobalConfig.basePath + GlobalConfig.gslCblasDllName);
        }

        #endregion

        #region helpers

        private void UpdateXyRatio()
        {
            double ratio = 1.0;
            if (!complexNumbersModeRadioBox.Checked)
                ratio = chart2d.XyRatio;
            else
                ratio = complexChart.XYRatio;
            XYRatioToolStripStatusLabel.Text = string.Format(Strings.XYRatio0, ratio);
        }

        private void ExportChartImage(string filename, SaveFileDialog saveFileDialog2)
        {
            IntPtr srcDC = GetDC(elementHostChart3d.Handle);

            var bm = new Bitmap(elementHostChart3d.Width, elementHostChart3d.Height);
            Graphics g = Graphics.FromImage(bm);
            IntPtr bmDC = g.GetHdc();
            BitBlt(bmDC, 0, 0, bm.Width, bm.Height, srcDC, 0, 0, 0x00CC0020 /*SRCCOPY*/);
            ReleaseDC(elementHostChart3d.Handle, srcDC);
            g.ReleaseHdc(bmDC);
            g.Dispose();
            ImageFormat format;

            switch (saveFileDialog2.FilterIndex)
            {
                case 1:
                    format = ImageFormat.Png;
                    break;
                case 2:
                    format = ImageFormat.Gif;
                    break;
                case 3:
                    format = ImageFormat.Jpeg;
                    break;
                case 4:
                    format = ImageFormat.Bmp;
                    break;
                case 5:
                    format = ImageFormat.Tiff;
                    break;
                case 6:
                    format = ImageFormat.Wmf;
                    break;
                default:
                    format = ImageFormat.Png;
                    break;
            }

            bm.Save(filename, format);
        }

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(
            IntPtr hdcDest, // handle to destination DC
            int nXDest, // x-coord of destination upper-left corner
            int nYDest, // y-coord of destination upper-left corner
            int nWidth, // width of destination rectangle
            int nHeight, // height of destination rectangle
            IntPtr hdcSrc, // handle to source DC
            int nXSrc, // x-coordinate of source upper-left corner
            int nYSrc, // y-coordinate of source upper-left corner
            Int32 dwRop // raster operation code
            );

        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC); //modified to include hWnd

        #endregion

        #region eventHandlers

        private void languageToolStripComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            var selectedCulture = Enumerable.First(AllCultures, c => c.NativeName == (string) languageToolStripComboBox.SelectedItem);
            Thread.CurrentThread.CurrentCulture = selectedCulture;
            LocalizationManager.GlobalUICulture = selectedCulture;
            Settings.Default.Language = selectedCulture;
            Settings.Default.Save();
        }

        private void addToChartButton_Click(object sender, EventArgs e)
        {
            if (expressionTextBox.Text != "")
            {
                try
                {
                    if (realNumbersRadioButton.Checked)
                    {
                        chart2d.addFx(
                            evaluator2d.Evaluate(expressionTextBox.Expression, customFunctionsTextBox.Expression),
                            expressionTextBox.Text);
                    }
                    else if (complexNumbersModeRadioBox.Checked)
                        complexChart.addFx(
                            complexEvaluator.Evaluate(expressionTextBox.Expression, customFunctionsTextBox.Expression),
                            expressionTextBox.Text);
                    else
                        chart3d.addFx(
                            evaluator3d.Evaluate(expressionTextBox.Expression, customFunctionsTextBox.Expression),
                            expressionTextBox.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Strings.Error);
                }
            }
            else
                MessageBox.Show(Strings.GUI_addToChartButton_Click_Expression_should_not_be_empty_);
        }

        private void calculateButton_Click(object sender, EventArgs e)
        {
            if (expressionTextBox.Text != "")
            {
                try
                {
                    if (complexNumbersModeRadioBox.Checked)
                    {
                        complexEvaluator.Evaluate(expressionTextBox.Expression, customFunctionsTextBox.Expression);
                        var z = new Complex((double) valueForCalculationNumericUpDown.Value,
                            (double) calculationsImZnumericUpDown.Value);
                        Complex fz = complexEvaluator.Invoke(z);

                        calculationValueTextBox.Text = fz.ToMathString();

                        calculationsHistoryDataGridView.Rows.Insert(0,
                            new object[] {expressionTextBox.Text, z.ToMathString(), calculationValueTextBox.Text});
                    }
                    else if (realNumbersRadioButton.Checked)
                    {
                        evaluator2d.Evaluate(expressionTextBox.Expression, customFunctionsTextBox.Expression);
                        double x = evaluator2d.Invoke((double) (valueForCalculationNumericUpDown.Value));
                        calculationValueTextBox.Text = x.ToString();
                        calculationsHistoryDataGridView.Rows.Insert(0,
                            new object[]
                            {
                                expressionTextBox.Text, valueForCalculationNumericUpDown.Value,
                                calculationValueTextBox.Text
                            });
                    }
                    else if (fxyModeRadioBox.Checked)
                    {
                        evaluator3d.Evaluate(expressionTextBox.Expression, customFunctionsTextBox.Expression);
                        double fxy = evaluator3d.Invoke((double) (valueForCalculationNumericUpDown.Value),
                            (double) calculationsImZnumericUpDown.Value);
                        calculationValueTextBox.Text = fxy.ToString();
                        calculationsHistoryDataGridView.Rows.Insert(0,
                            new object[]
                            {
                                expressionTextBox.Text,
                                valueForCalculationNumericUpDown.Value + ", " + calculationsImZnumericUpDown.Value,
                                calculationValueTextBox.Text
                            });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Strings.Error);
                }
            }
            else
                MessageBox.Show(Strings.GUI_addToChartButton_Click_Expression_should_not_be_empty_);
        }

        private void editChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var editChartWindow = new EditChartWindow(chart2d);
            editChartWindow.Show();
        }

        private void exportChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chart2d.saveImage();
        }

        public void aligmentLegendComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            chart2d.changeChartLegendAligment(((ToolStripComboBox) (s)).SelectedItem.ToString());
        }

        public void positionLegendComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            chart2d.changeChartLegendPosition(((ToolStripComboBox) (s)).SelectedItem.ToString());
        }

        public void colorsOfChartComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            chart2d.changeChartColor(((ToolStripComboBox) (s)).SelectedItem.ToString());
        }

        public void seriesOfChartComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            chart2d.changeSeries(((ToolStripComboBox) (s)).SelectedItem.ToString());
        }

        public void typeOfChartComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            chart2d.changeChartType(((ToolStripComboBox) (s)).SelectedItem.ToString());
        }

        private void realNumbersRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            chartToolStripMenuItem.Visible = chart2d.Visible = realNumbersRadioButton.Checked;
            comlexChartToolStripMenuItem.Visible =
                calculationsComplexLabel.Visible =
                    calculationsImZnumericUpDown.Visible = complexChart.Visible = complexNumbersModeRadioBox.Checked;
            chart3dToolStripMenuItem.Visible = elementHostChart3d.Visible = fxyModeRadioBox.Checked;

            if (realNumbersRadioButton.Checked)
            {
                calculationsRealLabel.Text = "       x =";
                addToChartButton.Text = Strings.AddToChart;
                complexChart.ClearAll();
                chart3d.Clear();
            }
            else if (complexNumbersModeRadioBox.Checked)
            {
                calculationsRealLabel.Text = "Re(z) =";
                calculationsComplexLabel.Text = "Im(z) =";
                addToChartButton.Text = Strings.DrawChart;
                chart2d.ClearAll();
                chart3d.Clear();
            }
            else if (fxyModeRadioBox.Checked)
            {
                calculationsComplexLabel.Visible = calculationsImZnumericUpDown.Visible = true;
                calculationsRealLabel.Text = "       x =";
                calculationsComplexLabel.Text = "       y =";
                addToChartButton.Text = Strings.AddToChart;
                complexChart.ClearAll();
                chart2d.ClearAll();
            }

            UpdateXyRatio();
            //y0NumericUpDown.Visible = y0label.Visible = yNNumericUpDown.Visible = yNlabel.Visible = complexNumbersModeRadioBox.Checked;
        }

        private void zakończToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void expressionTextBox_KeyPress(object s, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) 13)
                defaultActions[tabControl1.SelectedIndex].Invoke(s, e);
        }

        private void symbolicOperationButton_Click(object sender, EventArgs e)
        {
        }

        private void clearChartButton_Click(object sender, EventArgs e)
        {
            // this.chart2d.DataBindings
            chart2d.ClearAll();
            complexChart.ClearAll();
            chart3d.Clear();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            complexChart.saveImage();
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var editComplexChartWindow = new EditComplexChartWindow(complexChart);
            editComplexChartWindow.Show();
        }

        private void operationNumericalCalculationsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            NumericalCalculation.setupMethods(methodNumericalCalculationsComboBox,
                operationNumericalCalculationsComboBox);

            NumericalCalculation.setupGroupBoxes(operationNumericalCalculationsComboBox, derivativeAtPointGroupBox,
                rootOfFunctionGroupBox, numericalIntegrationGroupBox);
        }

        private void numericalOperationButton_Click(object sender, EventArgs e)
        {
            string method = methodNumericalCalculationsComboBox.SelectedItem.ToString();

            if (!complexNumbersModeRadioBox.Checked && !fxyModeRadioBox.Checked)
            {
                Func<double, double> fx = evaluator2d.Evaluate(expressionTextBox.Expression,
                    customFunctionsTextBox.Expression);
                double result;
                switch (method)
                {
                    case "trapezoidal method":
                        result = Integral.trapezoidalMethod(fx, (double) (aIntervalIntegrationNumericUpDown.Value),
                            (double) (bIntervalIntegrationNumericUpDown.Value));
                        resultNumericalCalculationsTextBox.Text = result.ToString();
                        numericalCalculationsDataGridView.Rows.Insert(0,
                            new[]
                            {
                                expressionTextBox.Text, operationNumericalCalculationsComboBox.SelectedItem,
                                methodNumericalCalculationsComboBox.SelectedItem,
                                "a=" + aIntervalIntegrationNumericUpDown.Value + "; b=" +
                                bIntervalIntegrationNumericUpDown.Value,
                                resultNumericalCalculationsTextBox.Text
                            });
                        break;

                    case "rectangle method":
                        result = Integral.rectangleMethod(fx, (double) (aIntervalIntegrationNumericUpDown.Value),
                            (double) (bIntervalIntegrationNumericUpDown.Value));
                        resultNumericalCalculationsTextBox.Text = result.ToString();
                        numericalCalculationsDataGridView.Rows.Insert(0,
                            new[]
                            {
                                expressionTextBox.Text, operationNumericalCalculationsComboBox.SelectedItem,
                                methodNumericalCalculationsComboBox.SelectedItem,
                                "a=" + aIntervalIntegrationNumericUpDown.Value + "; b=" +
                                bIntervalIntegrationNumericUpDown.Value,
                                resultNumericalCalculationsTextBox.Text
                            });
                        break;

                    case "Simpson's method":
                        result = Integral.simpsonMethod(fx, (double) (aIntervalIntegrationNumericUpDown.Value),
                            (double) (bIntervalIntegrationNumericUpDown.Value));
                        resultNumericalCalculationsTextBox.Text = result.ToString();
                        numericalCalculationsDataGridView.Rows.Insert(0,
                            new[]
                            {
                                expressionTextBox.Text, operationNumericalCalculationsComboBox.SelectedItem,
                                methodNumericalCalculationsComboBox.SelectedItem,
                                "a=" + aIntervalIntegrationNumericUpDown.Value + "; b=" +
                                bIntervalIntegrationNumericUpDown.Value,
                                resultNumericalCalculationsTextBox.Text
                            });
                        break;

                    case "double exponential transformation":
                        result = Integral.doubleExponentialTransformation(fx,
                            (double) (aIntervalIntegrationNumericUpDown.Value),
                            (double) (bIntervalIntegrationNumericUpDown.Value));
                        resultNumericalCalculationsTextBox.Text = result.ToString();
                        numericalCalculationsDataGridView.Rows.Insert(0,
                            new[]
                            {
                                expressionTextBox.Text, operationNumericalCalculationsComboBox.SelectedItem,
                                methodNumericalCalculationsComboBox.SelectedItem,
                                "a=" + aIntervalIntegrationNumericUpDown.Value + "; b=" +
                                bIntervalIntegrationNumericUpDown.Value,
                                resultNumericalCalculationsTextBox.Text
                            });
                        break;


                    case "non-adaptive Gauss–Kronrod method":
                        result = Integral.nonAdaptiveGaussKronrodMethod(fx,
                            (double) (aIntervalIntegrationNumericUpDown.Value),
                            (double) (bIntervalIntegrationNumericUpDown.Value));
                        resultNumericalCalculationsTextBox.Text = result.ToString();
                        numericalCalculationsDataGridView.Rows.Insert(0,
                            new[]
                            {
                                expressionTextBox.Text, operationNumericalCalculationsComboBox.SelectedItem,
                                methodNumericalCalculationsComboBox.SelectedItem,
                                "a=" + aIntervalIntegrationNumericUpDown.Value + "; b=" +
                                bIntervalIntegrationNumericUpDown.Value,
                                resultNumericalCalculationsTextBox.Text
                            });
                        break;


                    case "infinity-adaptive Gauss–Kronrod method":
                        result = Integral.infiniteAdaptiveGaussKronrodMethod(fx,
                            (double) (aIntervalIntegrationNumericUpDown.Value),
                            (double) (bIntervalIntegrationNumericUpDown.Value));
                        resultNumericalCalculationsTextBox.Text = result.ToString();
                        numericalCalculationsDataGridView.Rows.Insert(0,
                            new[]
                            {
                                expressionTextBox.Text, operationNumericalCalculationsComboBox.SelectedItem,
                                methodNumericalCalculationsComboBox.SelectedItem,
                                "a=" + aIntervalIntegrationNumericUpDown.Value + "; b=" +
                                bIntervalIntegrationNumericUpDown.Value,
                                resultNumericalCalculationsTextBox.Text
                            });
                        break;

                    case "Monte Carlo method":
                        result = Integral.monteCarloMethod(fx, (double) (aIntervalIntegrationNumericUpDown.Value),
                            (double) (bIntervalIntegrationNumericUpDown.Value));
                        resultNumericalCalculationsTextBox.Text = result.ToString();
                        numericalCalculationsDataGridView.Rows.Insert(0,
                            new[]
                            {
                                expressionTextBox.Text, operationNumericalCalculationsComboBox.SelectedItem,
                                methodNumericalCalculationsComboBox.SelectedItem,
                                "a=" + aIntervalIntegrationNumericUpDown.Value + "; b=" +
                                bIntervalIntegrationNumericUpDown.Value,
                                resultNumericalCalculationsTextBox.Text
                            });
                        break;

                    case "Romberg's method":
                        result = Integral.rombergMethod(fx, (double) (aIntervalIntegrationNumericUpDown.Value),
                            (double) (bIntervalIntegrationNumericUpDown.Value));
                        resultNumericalCalculationsTextBox.Text = result.ToString();
                        numericalCalculationsDataGridView.Rows.Insert(0,
                            new[]
                            {
                                expressionTextBox.Text, operationNumericalCalculationsComboBox.SelectedItem,
                                methodNumericalCalculationsComboBox.SelectedItem,
                                "a=" + aIntervalIntegrationNumericUpDown.Value + "; b=" +
                                bIntervalIntegrationNumericUpDown.Value,
                                resultNumericalCalculationsTextBox.Text
                            });
                        break;


                    case "finite difference formula":
                        result = Derivative.finiteDifferenceFormula(fx, (double) xDerivativePointNumericUpDown.Value,
                            (uint) nOrderDerivativeNumericUpDown.Value);
                        resultNumericalCalculationsTextBox.Text = result.ToString();
                        numericalCalculationsDataGridView.Rows.Insert(0,
                            new[]
                            {
                                expressionTextBox.Text, operationNumericalCalculationsComboBox.SelectedItem,
                                methodNumericalCalculationsComboBox.SelectedItem,
                                "n=" + nOrderDerivativeNumericUpDown.Value + "; x=" +
                                xDerivativePointNumericUpDown.Value,
                                resultNumericalCalculationsTextBox.Text
                            });
                        break;

                    case "stable finite difference formula":
                        result = Derivative.stableFiniteDifferenceFormula(fx,
                            (double) xDerivativePointNumericUpDown.Value, (uint) nOrderDerivativeNumericUpDown.Value);
                        resultNumericalCalculationsTextBox.Text = result.ToString();
                        numericalCalculationsDataGridView.Rows.Insert(0,
                            new[]
                            {
                                expressionTextBox.Text, operationNumericalCalculationsComboBox.SelectedItem,
                                methodNumericalCalculationsComboBox.SelectedItem,
                                "n=" + nOrderDerivativeNumericUpDown.Value + "; x=" +
                                xDerivativePointNumericUpDown.Value,
                                resultNumericalCalculationsTextBox.Text
                            });

                        break;

                    case "two-point finite difference formula":
                        result = Derivative.twoPointfiniteDifferenceFormula(fx,
                            (double) xDerivativePointNumericUpDown.Value, (uint) nOrderDerivativeNumericUpDown.Value);
                        resultNumericalCalculationsTextBox.Text = result.ToString();
                        numericalCalculationsDataGridView.Rows.Insert(0,
                            new[]
                            {
                                expressionTextBox.Text, operationNumericalCalculationsComboBox.SelectedItem,
                                methodNumericalCalculationsComboBox.SelectedItem,
                                "n=" + nOrderDerivativeNumericUpDown.Value + "; x=" +
                                xDerivativePointNumericUpDown.Value,
                                resultNumericalCalculationsTextBox.Text
                            });
                        break;

                    case "centered five-point method":
                        result = Derivative.centeredFivePointMethod(fx, (double) xDerivativePointNumericUpDown.Value,
                            (uint) nOrderDerivativeNumericUpDown.Value);
                        resultNumericalCalculationsTextBox.Text = result.ToString();
                        numericalCalculationsDataGridView.Rows.Insert(0,
                            new[]
                            {
                                expressionTextBox.Text, operationNumericalCalculationsComboBox.SelectedItem,
                                methodNumericalCalculationsComboBox.SelectedItem,
                                "n=" + nOrderDerivativeNumericUpDown.Value + "; x=" +
                                xDerivativePointNumericUpDown.Value,
                                resultNumericalCalculationsTextBox.Text
                            });
                        break;

                    case "bisection method":
                        result = FunctionRoot.bisectionMethod(fx, (double) aFunctionRootNumericUpDown.Value,
                            (double) bFunctionRootNumericUpDown.Value);
                        resultNumericalCalculationsTextBox.Text = result.ToString();
                        numericalCalculationsDataGridView.Rows.Insert(0,
                            new[]
                            {
                                expressionTextBox.Text, operationNumericalCalculationsComboBox.SelectedItem,
                                methodNumericalCalculationsComboBox.SelectedItem,
                                "a=" + aFunctionRootNumericUpDown.Value + "; b=" + bFunctionRootNumericUpDown.Value,
                                resultNumericalCalculationsTextBox.Text
                            });
                        break;

                    case "secant method":
                        result = FunctionRoot.secantMethod(fx, (double) aFunctionRootNumericUpDown.Value,
                            (double) bFunctionRootNumericUpDown.Value);
                        resultNumericalCalculationsTextBox.Text = result.ToString();
                        numericalCalculationsDataGridView.Rows.Insert(0,
                            new[]
                            {
                                expressionTextBox.Text, operationNumericalCalculationsComboBox.SelectedItem,
                                methodNumericalCalculationsComboBox.SelectedItem,
                                "a=" + aFunctionRootNumericUpDown.Value + "; b=" + bFunctionRootNumericUpDown.Value,
                                resultNumericalCalculationsTextBox.Text
                            });
                        break;

                    case "Brent's method":
                        result = FunctionRoot.BrentMethod(fx, (double) aFunctionRootNumericUpDown.Value,
                            (double) bFunctionRootNumericUpDown.Value);
                        resultNumericalCalculationsTextBox.Text = result.ToString();
                        numericalCalculationsDataGridView.Rows.Insert(0,
                            new[]
                            {
                                expressionTextBox.Text, operationNumericalCalculationsComboBox.SelectedItem,
                                methodNumericalCalculationsComboBox.SelectedItem,
                                "a=" + aFunctionRootNumericUpDown.Value + "; b=" + bFunctionRootNumericUpDown.Value,
                                resultNumericalCalculationsTextBox.Text
                            });
                        break;

                    case "Broyden's method":
                        result = FunctionRoot.BroydenMethod(fx, (double) aFunctionRootNumericUpDown.Value,
                            (double) bFunctionRootNumericUpDown.Value);
                        resultNumericalCalculationsTextBox.Text = result.ToString();
                        numericalCalculationsDataGridView.Rows.Insert(0,
                            new[]
                            {
                                expressionTextBox.Text, operationNumericalCalculationsComboBox.SelectedItem,
                                methodNumericalCalculationsComboBox.SelectedItem,
                                "a=" + aFunctionRootNumericUpDown.Value + "; b=" + bFunctionRootNumericUpDown.Value,
                                resultNumericalCalculationsTextBox.Text
                            });
                        break;
                }
            }
            else
            {
                MessageBox.Show(
                    Strings
                        .GUI_numericalOperationButton_Click_Only_Real_mode_is_supported_in_Numerical_calculations_right_now__more_to_come_in_next_versions_ +
                    Environment.NewLine + Strings.GUI_numericalOperationButton_Click__Check__Real___f_x___mode,
                    Strings.GUI_numericalOperationButton_Click_Warning_);
            }
        }

        private void epsilonDerrivativeTextBox_TextChanged(object sender, EventArgs e)
        {
            double result;
            if (double.TryParse(epsilonDerrivativeTextBox.Text, out result))
            {
                if (result > 0.0 && result < 1)
                    Derivative.EPS = result;
                else
                    MessageBox.Show(Strings.GivenΕIsNotValidΕShouldBeSmallPositiveNumber, Strings.Error);
            }
            else
                MessageBox.Show(Strings.GivenΕIsNotValid, Strings.Error);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Computator.NET "+GlobalConfig.version+"\nthis is beta version, some functions may not work properly\n\nAuthor: Paweł Troka\nE-mail: ptroka@fizyka.dk\nWebsite: http://fizyka.dk", "About Computator.NET");
            var about = new AboutBox1();
            about.ShowDialog();
        }

        private void processButton_Click(object sender, EventArgs e)
        {
            consoleOutputTextBox.Text = (Strings.ConsoleOutput);
            try
            {
                scriptingTextBox.ProcessScript(consoleOutputTextBox, customFunctionsTextBox.Expression);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Strings.Error);
            }
        }

        private void featuresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(GlobalConfig.features, Strings.Features);
        }

        private void thanksToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                GlobalConfig.betatesters + "\n\n" + GlobalConfig.translators + "\n\n" + GlobalConfig.libraries + "\n\n" +
                GlobalConfig.others, Strings.SpecialThanksTo);
        }

        private void changelogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var sr = new StreamReader("changelog.txt"))
            {
                var changelogForm = new Form() { Text = "Changelog", Size=this.Size };
                changelogForm.Controls.Add(new RichTextBox() { Text = sr.ReadToEnd(),ReadOnly = true,Dock = DockStyle.Fill});
                changelogForm.ShowDialog();
            }
        }

        private void bugReportingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Strings.PleaseReportAnyBugsToPawełTrokaPtrokaFizykaDk, Strings.BugReporting);
        }

        private void saveScriptButton_Click(object sender, EventArgs e)
        {
            DialogResult result = saveScriptFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                using (var sw = new StreamWriter(saveScriptFileDialog.FileName))
                {
                    sw.Write(scriptingTextBox.Text);
                }
                directoryTree1.Refresh();
                directoryTree1.Invalidate();
            }
        }

        private void openScriptButton_Click(object sender, EventArgs e)
        {
            DialogResult result = openScriptFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                using (var sr = new StreamReader(openScriptFileDialog.FileName))
                {
                    scriptingTextBox.Text = sr.ReadToEnd();
                }
            }
        }

        private void openCustomFunctions_Click(object sender, EventArgs e)
        {
            DialogResult result = openCustomFunctionsFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                using (var sr = new StreamReader(openCustomFunctionsFileDialog.FileName))
                {
                    customFunctionsTextBox.Text = sr.ReadToEnd();
                }
            }
        }

        private void saveCustomFunctions_Click(object sender, EventArgs e)
        {
            DialogResult result = saveCustomFunctionsFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                using (var sw = new StreamWriter(saveCustomFunctionsFileDialog.FileName))
                {
                    sw.Write(customFunctionsTextBox.Text);
                }
                customFunctionsDirectoryTree.Refresh();
                customFunctionsDirectoryTree.Invalidate();
            }
        }

        private void customFunctionsDirectoryTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (File.Exists(customFunctionsDirectoryTree.SelectedNode.FullPath))
            {
                using (var sr = new StreamReader(customFunctionsDirectoryTree.SelectedNode.FullPath))
                {
                    customFunctionsTextBox.Text = sr.ReadToEnd();
                }
            }
        }

        private void countourLinesToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            complexChart.countourMode = (CountourLinesMode) countourLinesToolStripComboBox.SelectedItem;
            complexChart.reDraw();
        }

        private void colorAssignmentToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            complexChart.colorAssignmentMethod = (AssignmentOfColorMethod) colorAssignmentToolStripComboBox.SelectedItem;
            complexChart.reDraw();
        }

        private void directoryTree1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (File.Exists(directoryTree1.SelectedNode.FullPath))
            {
                using (var sr = new StreamReader(directoryTree1.SelectedNode.FullPath))
                {
                    scriptingTextBox.Text = sr.ReadToEnd();
                }
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            chart3d.Quality =
                chart2d.Quality = complexChart.Quality = (trackBar1.Value/((double) trackBar1.Maximum))*100.0;
        }

        private void exportChart3dToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveFileDialog2 = new SaveFileDialog();

            saveFileDialog2.Filter =
                "Png Image (.png)|*.png|Gif Image (.gif)|*.gif|JPEG Image (.jpeg)|*.jpeg|Bitmap Im" +
                "age (.bmp)|*.bmp|Tiff Image (.tiff)|*.tiff|Wmf Image (.wmf)|*.wmf";

            saveFileDialog2.FileName = Strings.Chart + DateTime.Now.ToShortDateString() + " "
                                       + DateTime.Now.ToLongTimeString().Replace(':', '-')
                                       + ".png";

            DialogResult dialogResult = saveFileDialog2.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                Thread.Sleep(20);
                ExportChartImage(saveFileDialog2.FileName, saveFileDialog2);
            }
        }

        private void editChart3dToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var editChartWindow = new Charting.Chart3D.EditChartWindow(chart3d, elementHostChart3d);
            editChartWindow.ShowDialog();
        }

        private void editChart3dPropertiesToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            var editChartProperties = new EditChartProperties(chart3d);
            if (editChartProperties.ShowDialog() == DialogResult.OK)
            {
            }
        }

        private void chart3dEqualAxesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chart3d.EqualAxes = true;
        }

        private void chart3dFitAxesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chart3d.EqualAxes = false;
        }

        private void editPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var editChartProperties = new EditChartProperties(complexChart);
            if (editChartProperties.ShowDialog() == DialogResult.OK)
            {
                complexChart.reDraw();
            }
        }

        private void editPropertiesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var editChartProperties = new EditChartProperties(chart2d);
            if (editChartProperties.ShowDialog() == DialogResult.OK)
            {
                chart2d.Invalidate();
            }
        }

        private void benchmarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var bch = new BenchmarkForm();
            bch.Show();
        }

        #endregion

        private void sortExpressionsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            expressionTextBox.Sort = sortExpressionsCheckBox.Checked;
            expressionTextBox.refreshAutoComplete();
        }

        private void transformToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuitem = sender as ToolStripDropDownItem;

            /*  if(realNumbersRadioButton.Checked)
                  chart2d.
              else if (complexNumbersModeRadioBox.Checked)
                  else if(fxyModeRadioBox.Checked)*/
        }
    }
}