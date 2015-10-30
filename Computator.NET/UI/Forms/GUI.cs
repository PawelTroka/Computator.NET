#define PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
using Enumerable = System.Linq.Enumerable;
using ExceptionExtensions = Computator.NET.Compilation.ExceptionExtensions;

namespace Computator.NET
{
    public partial class GUI : Localization.LocalizedForm
    {
        private readonly System.Globalization.CultureInfo[] AllCultures =
            System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.NeutralCultures);

        private readonly Evaluation.FunctionComplexEvaluator complexEvaluator;
        private readonly Evaluation.Function2DEvaluator evaluator2d;
        private readonly Evaluation.Function3DEvaluator evaluator3d;
        private readonly Logging.SimpleLogger logger;
        private readonly UI.AutocompleteMenu.WebBrowserForm menuFunctionsToolTip;
        private Charting.RealCharting.Chart2D chart2d;
        private Charting.Chart3D.Chart3DControl chart3d;
        private Charting.ComplexCharting.ComplexChart complexChart;
        private UI.CodeEditors.CodeEditorControlWrapper customFunctionsCodeEditor;
        private System.Collections.Generic.List<System.Action<object, System.EventArgs>> defaultActions;
        private System.Windows.Forms.Integration.ElementHost elementHostChart3d;
        private Evaluation.ScriptEvaluator scriptEvaluator;
        private UI.CodeEditors.CodeEditorControlWrapper scriptingCodeEditor;

        private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            var index = tabControl1.SelectedIndex;

            transformToolStripMenuItem.Enabled = chartToolStripMenuItem.Enabled =
                chart3dToolStripMenuItem.Enabled = comlexChartToolStripMenuItem.Enabled = (index == 0);

            openToolStripMenuItem.Enabled = (index == 0 || index == 5 || index == 4);

            saveToolStripMenuItem.Enabled = (index == 4 || index == 5);

            //expressionTextBox.Visible = !(index ==5||index==4);
            tableLayoutPanel1.Visible = !(index == 5 || index == 4);
        }

        private void languageToolStripComboBox_SelectedIndexChanged_1(object sender, System.EventArgs e)
        {
            var selectedCulture = Enumerable.First(AllCultures,
                c => c.NativeName == (string) languageToolStripComboBox.SelectedItem);
            System.Threading.Thread.CurrentThread.CurrentCulture = selectedCulture;
            Localization.LocalizationManager.GlobalUICulture = selectedCulture;
            Properties.Settings.Default.Language = selectedCulture;
            Properties.Settings.Default.Save();
        }

        private void preferencesToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            (new Config.Settings()).Show();
        }

        private void logsToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (System.IO.Directory.Exists(Logging.SimpleLogger.LogsDirectory))
                System.Diagnostics.Process.Start(Logging.SimpleLogger.LogsDirectory);
            else
                System.Windows.Forms.MessageBox.Show(
                    Localization.Strings.GUI_logsToolStripMenuItem_Click_You_dont_have_any_logs_yet_);
        }

        private void modeRealToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            var txt = (sender as System.Windows.Forms.ToolStripMenuItem).Text;
            switch (txt)
            {
                case "Real : f(x)":
                    SetMode(Evaluation.CalculationsMode.Real);
                    break;
                case "Complex : f(z)":
                    SetMode(Evaluation.CalculationsMode.Complex);
                    break;
                case "3D : f(x,y)":
                    SetMode(Evaluation.CalculationsMode.Fxy);
                    break;
            }
        }

        private void runToolStripButton_Click(object sender, System.EventArgs e)
        {
            defaultActions[tabControl1.SelectedIndex].Invoke(sender, e);
        }

        private void cutToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            switch (tabControl1.SelectedIndex)
            {
                case 4:
                    if (scriptingCodeEditor.Focused)
                        scriptingCodeEditor.Cut();
                    else
                        System.Windows.Forms.SendKeys.Send("^X");
                    break;
                case 5:
                    if (customFunctionsCodeEditor.Focused)
                        customFunctionsCodeEditor.Cut();
                    else
                        System.Windows.Forms.SendKeys.Send("^X");
                    break;

                default: //if (tabControl1.SelectedIndex < 4)
                    System.Windows.Forms.SendKeys.Send("^X"); //expressionTextBox.Cut();
                    break;
            }
#else
            SendKeys.Send("^X");
#endif
        }

        private void undoToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            if (tabControl1.SelectedIndex < 4)
                System.Windows.Forms.SendKeys.Send("^Z"); //expressionTextBox.Undo();
            else if (tabControl1.SelectedIndex == 4)
            {
                if (scriptingCodeEditor.Focused)
                    scriptingCodeEditor.Undo();
                else
                    System.Windows.Forms.SendKeys.Send("^Z");
            }
            else
            {
                if (customFunctionsCodeEditor.Focused)
                    customFunctionsCodeEditor.Undo();
                else
                    System.Windows.Forms.SendKeys.Send("^Z");
            }
#else
            SendKeys.Send("^Z");

#endif
        }

        private void redoToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            if (tabControl1.SelectedIndex < 4)
            {
                System.Windows.Forms.SendKeys.Send("^Y");
                //expressionTextBox.do()
            }
            else if (tabControl1.SelectedIndex == 4)
                //scriptingCodeEditor.Focus();
            {
                if (scriptingCodeEditor.Focused)
                    scriptingCodeEditor.Redo();
                else
                    System.Windows.Forms.SendKeys.Send("^Y");
            }
            else
            {
                if (customFunctionsCodeEditor.Focused)
                    customFunctionsCodeEditor.Redo();
                else
                    System.Windows.Forms.SendKeys.Send("^Y");
            }
#else
              SendKeys.Send("^Y");
#endif
        }

        private void copyToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            if (tabControl1.SelectedIndex < 4)
            {
                System.Windows.Forms.SendKeys.Send("^C"); //expressionTextBox.Copy();
            }
            else if (tabControl1.SelectedIndex == 4)
            {
                if (scriptingCodeEditor.Focused)
                    scriptingCodeEditor.Copy();
                else
                    System.Windows.Forms.SendKeys.Send("^C");
            }
            else
            {
                if (customFunctionsCodeEditor.Focused)
                    customFunctionsCodeEditor.Copy();
                else
                    System.Windows.Forms.SendKeys.Send("^C");
            }
#else
            SendKeys.Send("^C");
#endif
        }

        private void pasteToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            if (tabControl1.SelectedIndex < 4)
            {
                System.Windows.Forms.SendKeys.Send("^V"); //expressionTextBox.Paste();
            }
            else if (tabControl1.SelectedIndex == 4)
            {
                if (scriptingCodeEditor.Focused)
                    scriptingCodeEditor.Paste();
                else
                    System.Windows.Forms.SendKeys.Send("^V");
            }
            else
            {
                if (customFunctionsCodeEditor.Focused)
                    customFunctionsCodeEditor.Paste();
                else
                    System.Windows.Forms.SendKeys.Send("^V");
            }

#else
            SendKeys.Send("^V");
#endif
        }

        private void selectAllToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            if (tabControl1.SelectedIndex < 4)
            {
                System.Windows.Forms.SendKeys.Send("^A"); //expressionTextBox.SelectAll();
            }
            else if (tabControl1.SelectedIndex == 4)
            {
                if (scriptingCodeEditor.Focused)
                    scriptingCodeEditor.SelectAll();
                else
                    System.Windows.Forms.SendKeys.Send("^A");
            }
            else
            {
                if (customFunctionsCodeEditor.Focused)
                    customFunctionsCodeEditor.SelectAll();
                else
                    System.Windows.Forms.SendKeys.Send("^A");
            }
#else
            SendKeys.Send("^A");
#endif
        }

        private void newToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            switch (tabControl1.SelectedIndex)
            {
                case 0:

                    //SendKeys.Send("^S");
                    break;

                case 4:
                    scriptingCodeEditor.NewDocument();
                    break;

                case 5:
                    customFunctionsCodeEditor.NewDocument();
                    break;

                default:
                    //SendKeys.Send("^S");
                    break;
            }
#else
    //SendKeys.Send("^S");
#endif
        }

        private void openToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            var ofd = new System.Windows.Forms.OpenFileDialog {Filter = Config.GlobalConfig.tslFilesFIlter};

            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;


#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            switch (tabControl1.SelectedIndex)
            {
                case 0:

                    //SendKeys.Send("^S");
                    break;

                case 4:
                    scriptingCodeEditor.NewDocument(ofd.FileName);
                    break;

                case 5:
                    customFunctionsCodeEditor.NewDocument(ofd.FileName);
                    break;

                default:
                    //SendKeys.Send("^S");
                    break;
            }
#else
    //SendKeys.Send("^S");
#endif
        }

        private void saveToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            switch (tabControl1.SelectedIndex)
            {
                case 0:

                    //SendKeys.Send("^S");
                    break;

                case 4:
                    scriptingCodeEditor.Save();
                    break;

                case 5:
                    customFunctionsCodeEditor.Save();
                    break;

                default:
                    //SendKeys.Send("^S");
                    break;
            }
#else
            SendKeys.Send("^S");
#endif
        }

        private void saveAsToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            switch (tabControl1.SelectedIndex)
            {
                case 0:

                    //SendKeys.Send("^S");
                    break;

                case 4:
                    scriptingCodeEditor.SaveAs();
                    break;

                case 5:
                    customFunctionsCodeEditor.SaveAs();
                    break;

                default:
                    //SendKeys.Send("^S");
                    break;
            }
#else
            SendKeys.Send("^S");
#endif
        }

        private void printToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    if (_calculationsMode == Evaluation.CalculationsMode.Real)
                        chart2d.Printing.Print(true);
                    else
                        System.Windows.Forms.SendKeys.Send("^P");
                    break;

                case 4:
                    //scriptingCodeEditor();
                    break;

                case 5:
                    //this.customFunctionsCodeEditor
                    break;

                default:
                    System.Windows.Forms.SendKeys.Send("^P"); //this.chart2d.Printing.PrintPreview();
                    break;
            }
#else
            SendKeys.Send("^P");
#endif
        }

        private void printPreviewToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    if (_calculationsMode == Evaluation.CalculationsMode.Real)
                        chart2d.Printing.PrintPreview();
                    else
                        System.Windows.Forms.SendKeys.Send("^P");
                    break;

                case 4:
                    //scriptingCodeEditor();
                    break;

                case 5:
                    //this.customFunctionsCodeEditor
                    break;

                default:
                    System.Windows.Forms.SendKeys.Send("^P"); //this.chart2d.Printing.PrintPreview();
                    break;
            }
#else
            SendKeys.Send("^P");
#endif
        }

        private void exponentiationToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
        }

        #region initialization and construction

        private readonly Evaluation.ModeDeterminer modeDeterminer;

        public GUI()
        {
            logger = new Logging.SimpleLogger(this);
            evaluator2d = new Evaluation.Function2DEvaluator();
            evaluator3d = new Evaluation.Function3DEvaluator();
            complexEvaluator = new Evaluation.FunctionComplexEvaluator();
            scriptEvaluator = new Evaluation.ScriptEvaluator();
            menuFunctionsToolTip = new UI.AutocompleteMenu.WebBrowserForm();
            modeDeterminer = new Evaluation.ModeDeterminer();

            InitializeComponent();
            InitializeFunctions();
            InitializeCharts(); //takes more time then it should
            expressionTextBox.RefreshAutoComplete();
            SetupAllComboBoxes();
            attachEventHandlers();
            toolStripStatusLabel1.Text = Config.GlobalConfig.version;
            customFunctionsDirectoryTree.Drive = Config.GlobalConfig.FullPath("TSL Examples", "_CustomFunctions");
            directoryTree1.Drive = Config.GlobalConfig.FullPath("TSL Examples", "_Scripts");
            UpdateXyRatio();
            InitializeScripting(); //takes a lot of time, TODO: optimize
            SetMathFonts();
            InitializeDataBindings();
            BringToFront();
            Focus();
            Icon = Properties.Resources.computator_net_icon;

            HandleCommandLine();
            Properties.Settings.Default.PropertyChanged += Default_PropertyChanged;

            tabPage4.Enabled = false;

            expressionTextBox.TextChanged += ExpressionTextBox_TextChanged;
        }

        private Evaluation.CalculationsMode _calculationsMode = Evaluation.CalculationsMode.Fxy;

        private void ExpressionTextBox_TextChanged(object sender, System.EventArgs e)
        {
            var mode = modeDeterminer.DetermineMode(expressionTextBox.Text);
            if (mode == _calculationsMode) return;

            SetMode(mode);
        }

        private void SetMode(Evaluation.CalculationsMode mode)
        {
            chartToolStripMenuItem.Visible =
                chart2d.Visible = mode == Evaluation.CalculationsMode.Real;

            comlexChartToolStripMenuItem.Visible =
                calculationsComplexLabel.Visible =
                    calculationsImZnumericUpDown.Visible =
                        complexChart.Visible = mode == Evaluation.CalculationsMode.Complex;

            chart3dToolStripMenuItem.Visible =
                elementHostChart3d.Visible = mode == Evaluation.CalculationsMode.Fxy;

            switch (mode)
            {
                case Evaluation.CalculationsMode.Complex:
                    calculationsRealLabel.Text = "Re(z) =";
                    calculationsComplexLabel.Text = "Im(z) =";
                    addToChartButton.Text = Localization.Strings.DrawChart;
                    // chart2d.ClearAll();
                    //chart3d.Clear();
                    modeToolStripDropDownButton.Text = "Mode[Complex : f(z)]";
                    break;
                case Evaluation.CalculationsMode.Fxy:
                    calculationsComplexLabel.Visible = calculationsImZnumericUpDown.Visible = true;
                    calculationsRealLabel.Text = "       x =";
                    calculationsComplexLabel.Text = "       y =";
                    addToChartButton.Text = Localization.Strings.AddToChart;
                    //complexChart.ClearAll();
                    //chart2d.ClearAll();
                    modeToolStripDropDownButton.Text = "Mode[3D : f(x,y)]";
                    break;
                case Evaluation.CalculationsMode.Real:
                    calculationsRealLabel.Text = "       x =";
                    addToChartButton.Text = Localization.Strings.AddToChart;
                    //complexChart.ClearAll();
                    //chart3d.Clear();
                    modeToolStripDropDownButton.Text = "Mode[Real : f(x)]";
                    break;
            }

            _calculationsMode = mode;
            UpdateXyRatio();
        }

        private void HandleCommandLine()
        {
            var args = System.Environment.GetCommandLineArgs();
            if (args.Length < 2) return;
            if (!args[1].Contains(".tsl")) return;

            var code = System.IO.File.ReadAllText(args[1].Replace(@"""", ""));

            if (args[1].Contains(".tslf"))
            {
                customFunctionsCodeEditor.Text = code;
                tabControl1.SelectedIndex = 5;
            }
            else
            {
                scriptingCodeEditor.Text = code;
                tabControl1.SelectedIndex = 4;
            }
        }

        private void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Language":
                    System.Threading.Thread.CurrentThread.CurrentCulture = Properties.Settings.Default.Language;
                    Localization.LocalizationManager.GlobalUICulture = Properties.Settings.Default.Language;
                    break;

                case "CodeEditor":
                    scriptingCodeEditor.ChangeEditorType();
                    customFunctionsCodeEditor.ChangeEditorType();
                    break;

                case "FunctionsOrder":
                    expressionTextBox.RefreshAutoComplete();
                    break;

                case "ExpressionFont":
                    expressionTextBox.SetFont(Properties.Settings.Default.ExpressionFont);
                    break;


                case "ScriptingFont":
                    scriptingCodeEditor.SetFont(Properties.Settings.Default.ScriptingFont);
                    customFunctionsCodeEditor.SetFont(Properties.Settings.Default.ScriptingFont);
                    break;
            }
        }

        private void SetMathFonts()
        {
            chart2d.Legends[0].Font = Config.MathCustomFonts.GetMathFont(chart2d.Legends[0].Font.Size);
            const float fontsize = 17.0F;

            chart2d.Font = Config.MathCustomFonts.GetMathFont(fontsize);

            function.DefaultCellStyle.Font = Config.MathCustomFonts.GetMathFont(function.DefaultCellStyle.Font.Size);
            result.DefaultCellStyle.Font = Config.MathCustomFonts.GetMathFont(result.DefaultCellStyle.Font.Size);

            calculationValueTextBox.Font = Config.MathCustomFonts.GetMathFont(calculationValueTextBox.Font.Size);
            resultNumericalCalculationsTextBox.Font =
                Config.MathCustomFonts.GetMathFont(resultNumericalCalculationsTextBox.Font.Size);

            consoleOutputTextBox.Font = Config.MathCustomFonts.GetMathFont(consoleOutputTextBox.Font.Size);

            calculationsHistoryDataGridView.Columns[0].DefaultCellStyle.Font =
                Config.MathCustomFonts.GetMathFont(calculationsHistoryDataGridView.Columns[0].DefaultCellStyle.Font.Size);

            calculationsHistoryDataGridView.Columns[calculationsHistoryDataGridView.Columns.Count - 1].DefaultCellStyle
                .Font =
                Config.MathCustomFonts.GetMathFont(
                    calculationsHistoryDataGridView.Columns[calculationsHistoryDataGridView.Columns.Count - 1]
                        .DefaultCellStyle.Font.Size);
        }

        private void InitializeFunctions()
        {
            var functions = Config.GlobalConfig.functionsDetails.ToArray();

            var dict = new System.Collections.Generic.Dictionary<string, System.Windows.Forms.ToolStripMenuItem>
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
                    var cat = new System.Windows.Forms.ToolStripMenuItem(f.Value.Category) {Name = f.Value.Category};
                    dict[f.Value.Type].DropDownItems.Add(cat);
                }

                var item = new System.Windows.Forms.ToolStripMenuItem
                {
                    Text = f.Value.Signature,
                    ToolTipText = f.Value.Title
                };
                //item.Click += Item_Click;
                item.MouseDown += Item_Click;

                (dict[f.Value.Type].DropDownItems[f.Value.Category] as System.Windows.Forms.ToolStripMenuItem)
                    .DropDownItems.Add(item);
            }
        }

        private void InitializeDataBindings()
        {
            exponentiationToolStripMenuItem.DataBindings.Add("Checked", expressionTextBox, "ExponentMode", false,
                System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged);

            //TODO: somehow manage to get codeeditors ExponentMode bind to exponentToolStripMenuItem Checked property
            // scriptingCodeEditor.DataBindings.Add("ExponentMode", exponentiationToolStripMenuItem, "ExponentMode",false,DataSourceUpdateMode.OnPropertyChanged);
            // customFunctionsCodeEditor.DataBindings.Add("ExponentMode", scriptingCodeEditor, "ExponentMode",false,DataSourceUpdateMode.OnPropertyChanged);


            y0NumericUpDown.DataBindings.Add("Value", chart3d, "YMin", false,
                System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged);
            yNNumericUpDown.DataBindings.Add("Value", chart3d, "YMax", false,
                System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged);
            x0NumericUpDown.DataBindings.Add("Value", chart3d, "XMin", false,
                System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged);
            xnNumericUpDown.DataBindings.Add("Value", chart3d, "XMax", false,
                System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged);
            //working OK
            complexChart.DataBindings.Add("YMin", y0NumericUpDown, "Value", false,
                System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged);
            complexChart.DataBindings.Add("YMax", yNNumericUpDown, "Value", false,
                System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged);
            complexChart.DataBindings.Add("XMin", x0NumericUpDown, "Value", false,
                System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged);
            complexChart.DataBindings.Add("XMax", xnNumericUpDown, "Value", false,
                System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged);

            BindField(chart2d, "YMin", y0NumericUpDown, "Value");
            BindField(chart2d, "YMax", yNNumericUpDown, "Value");
            BindField(chart2d, "XMin", x0NumericUpDown, "Value");
            BindField(chart2d, "XMax", xnNumericUpDown, "Value");

            chart2d.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == "XyRatio")
                    XYRatioToolStripStatusLabel.Text = string.Format(Localization.Strings.XYRatio0, chart2d.XyRatio);
            };
        }

        private void InitializeCharts()
        {
            chart2d = new Charting.RealCharting.Chart2D();
            complexChart = new Charting.ComplexCharting.ComplexChart();
            chart3d = new Charting.Chart3D.Chart3DControl();
            elementHostChart3d = new System.Windows.Forms.Integration.ElementHost
            {
                BackColor = System.Drawing.Color.White,
                Dock = System.Windows.Forms.DockStyle.Fill,
                Child = chart3d
            };
            ((System.ComponentModel.ISupportInitialize) (chart2d)).BeginInit();

            panel2.Controls.Add(chart2d);
            panel2.Controls.Add(complexChart);
            panel2.Controls.Add(elementHostChart3d);
            chart2d.BringToFront();
            complexChart.BringToFront();
            elementHostChart3d.BringToFront();
            complexChart.Visible = false;
            elementHostChart3d.Visible = false;
            ((System.ComponentModel.ISupportInitialize) (chart2d)).EndInit();
            chart2d.setChartAreaValues((double) x0NumericUpDown.Value, (double) xnNumericUpDown.Value,
                (double) y0NumericUpDown.Value, (double) yNNumericUpDown.Value);
            complexChart.setChartAreaValues((double) x0NumericUpDown.Value, (double) xnNumericUpDown.Value,
                (double) y0NumericUpDown.Value, (double) yNNumericUpDown.Value);
        }

        private void InitializeScripting()
        {
            scriptingCodeEditor = new UI.CodeEditors.CodeEditorControlWrapper();
            customFunctionsCodeEditor = new UI.CodeEditors.CodeEditorControlWrapper();

            splitContainer2.Panel1.Controls.Add(scriptingCodeEditor);
            scriptingCodeEditor.Name = "scriptingCodeEditor";
            scriptingCodeEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            scriptingCodeEditor.BringToFront();

            splitContainer3.Panel1.Controls.Add(customFunctionsCodeEditor);
            customFunctionsCodeEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            customFunctionsCodeEditor.Name = "customFunctionsCodeEditor";
        }

        private void attachEventHandlers()
        {
            Resize += (o, e) => UpdateXyRatio();
            defaultActions = new System.Collections.Generic.List<System.Action<object, System.EventArgs>>
            {
                addToChartButton_Click,
                calculateButton_Click,
                numericalOperationButton_Click,
                symbolicOperationButton_Click,
                processButton_Click
            };
        }

        private void SetupAllComboBoxes()
        {
            chart2d.setupComboBoxes(typeOfChartComboBox, seriesOfChartComboBox, colorsOfChartComboBox,
                positionLegendComboBox, aligmentLegendComboBox);
            complexChart.setupComboBoxes(countourLinesToolStripComboBox, colorAssignmentToolStripComboBox);

            NumericalCalculations.NumericalCalculation.setupOperations(operationNumericalCalculationsComboBox);
            NumericalCalculations.NumericalCalculation.setupMethods(methodNumericalCalculationsComboBox,
                operationNumericalCalculationsComboBox);
            NumericalCalculations.NumericalCalculation.setupGroupBoxes(operationNumericalCalculationsComboBox,
                derivativeAtPointGroupBox,
                rootOfFunctionGroupBox, numericalIntegrationGroupBox);


            languageToolStripComboBox.Items.Add((new System.Globalization.CultureInfo("en")).NativeName);
            languageToolStripComboBox.Items.Add((new System.Globalization.CultureInfo("pl")).NativeName);
            languageToolStripComboBox.Items.Add((new System.Globalization.CultureInfo("de")).NativeName);
            languageToolStripComboBox.Items.Add((new System.Globalization.CultureInfo("cs")).NativeName);


            languageToolStripComboBox.AutoSize = true;
            languageToolStripComboBox.Invalidate();

            languageToolStripComboBox.SelectedItem =
                Enumerable.First(AllCultures,
                    c =>
                        c.TwoLetterISOLanguageName ==
                        System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName)
                    .NativeName;
        }

        #endregion

        #region helpers

        private void UpdateXyRatio()
        {
            var ratio = 1.0;
            ratio = _calculationsMode != Evaluation.CalculationsMode.Complex ? chart2d.XyRatio : complexChart.XYRatio;
            XYRatioToolStripStatusLabel.Text = string.Format(Localization.Strings.XYRatio0, ratio);
        }

        public static void BindField(System.Windows.Forms.Control control, string propertyName,
            object dataSource, string dataMember)
        {
            System.Windows.Forms.Binding bd;

            for (var index = control.DataBindings.Count - 1; (index == 0); index--)
            {
                bd = control.DataBindings[index];
                if (bd.PropertyName == propertyName)
                    control.DataBindings.Remove(bd);
            }
            control.DataBindings.Add(propertyName, dataSource, dataMember, false,
                System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged);
        }

        private void ExportChartImage(string filename, System.Windows.Forms.SaveFileDialog saveFileDialog2)
        {
            var srcDC = NativeMethods.GetDC(elementHostChart3d.Handle);

            var bm = new System.Drawing.Bitmap(elementHostChart3d.Width, elementHostChart3d.Height);
            var g = System.Drawing.Graphics.FromImage(bm);
            var bmDC = g.GetHdc();
            NativeMethods.BitBlt(bmDC, 0, 0, bm.Width, bm.Height, srcDC, 0, 0, 0x00CC0020 /*SRCCOPY*/);
            NativeMethods.ReleaseDC(elementHostChart3d.Handle, srcDC);
            g.ReleaseHdc(bmDC);
            g.Dispose();
            System.Drawing.Imaging.ImageFormat format;

            switch (saveFileDialog2.FilterIndex)
            {
                case 1:
                    format = System.Drawing.Imaging.ImageFormat.Png;
                    break;
                case 2:
                    format = System.Drawing.Imaging.ImageFormat.Gif;
                    break;
                case 3:
                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                    break;
                case 4:
                    format = System.Drawing.Imaging.ImageFormat.Bmp;
                    break;
                case 5:
                    format = System.Drawing.Imaging.ImageFormat.Tiff;
                    break;
                case 6:
                    format = System.Drawing.Imaging.ImageFormat.Wmf;
                    break;
                default:
                    format = System.Drawing.Imaging.ImageFormat.Png;
                    break;
            }

            bm.Save(filename, format);
        }

        #endregion

        #region eventHandlers

        private void addToChartButton_Click(object sender, System.EventArgs e)
        {
            if (expressionTextBox.Text != "")
            {
                try
                {
                    switch (_calculationsMode)
                    {
                        case Evaluation.CalculationsMode.Real:
                            chart2d.addFx(
                                evaluator2d.Evaluate(expressionTextBox.Text, customFunctionsCodeEditor.Text));
                            break;
                        case Evaluation.CalculationsMode.Complex:
                            complexChart.addFx(
                                complexEvaluator.Evaluate(expressionTextBox.Text, customFunctionsCodeEditor.Text));
                            break;

                        case Evaluation.CalculationsMode.Fxy:
                            chart3d.addFx(
                                evaluator3d.Evaluate(expressionTextBox.Text, customFunctionsCodeEditor.Text));
                            break;
                    }
                }
                catch (System.Exception ex)
                {
                    var message = ex.Message + " " + (ex.InnerException?.Message ?? "");
                    System.Windows.Forms.MessageBox.Show(message, Localization.Strings.Error);

                    if (!ExceptionExtensions.IsInternal(ex))
                    {
                        logger.MethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                        logger.Log(message, Config.ErrorType.General, ex);
                    }
                }
            }
            else
                System.Windows.Forms.MessageBox.Show(
                    Localization.Strings.GUI_addToChartButton_Click_Expression_should_not_be_empty_);
        }

        private void calculateButton_Click(object sender, System.EventArgs e)
        {
            if (expressionTextBox.Text != "")
            {
                try
                {
                    switch (_calculationsMode)
                    {
                        case Evaluation.CalculationsMode.Complex:
                        {
                            var function = complexEvaluator.Evaluate(expressionTextBox.Text,
                                customFunctionsCodeEditor.Text);
                            var z = new System.Numerics.Complex((double) valueForCalculationNumericUpDown.Value,
                                (double) calculationsImZnumericUpDown.Value);
                            var fz = function.Evaluate(z);

                            calculationValueTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(fz);

                            calculationsHistoryDataGridView.Rows.Insert(0, expressionTextBox.Text,
                                Evaluation.ScriptingExtensions.ToMathString(z),
                                calculationValueTextBox.Text);
                        }
                            break;
                        case Evaluation.CalculationsMode.Real:
                        {
                            var function = evaluator2d.Evaluate(expressionTextBox.Text, customFunctionsCodeEditor.Text);
                            var x = function.Evaluate((double) (valueForCalculationNumericUpDown.Value));
                            calculationValueTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(x);
                            calculationsHistoryDataGridView.Rows.Insert(0, expressionTextBox.Text,
                                valueForCalculationNumericUpDown.Value, calculationValueTextBox.Text);
                        }
                            break;
                        case Evaluation.CalculationsMode.Fxy:
                        {
                            var function = evaluator3d.Evaluate(expressionTextBox.Text, customFunctionsCodeEditor.Text);
                            var fxy = function.Evaluate((double) (valueForCalculationNumericUpDown.Value),
                                (double) calculationsImZnumericUpDown.Value);
                            calculationValueTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(fxy);
                            calculationsHistoryDataGridView.Rows.Insert(0, expressionTextBox.Text,
                                valueForCalculationNumericUpDown.Value + ", " + calculationsImZnumericUpDown.Value,
                                calculationValueTextBox.Text);
                        }
                            break;
                    }
                }
                catch (System.Exception ex)
                {
                    var message = ex.Message + " " + (ex.InnerException?.Message ?? "");
                    System.Windows.Forms.MessageBox.Show(message, Localization.Strings.Error);

                    if (!ExceptionExtensions.IsInternal(ex))
                    {
                        logger.MethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                        logger.Log(message, Config.ErrorType.General, ex);
                    }
                }
            }
            else
                System.Windows.Forms.MessageBox.Show(
                    Localization.Strings.GUI_addToChartButton_Click_Expression_should_not_be_empty_);
        }

        private void editChartToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            var editChartWindow = new Charting.RealCharting.EditChartWindow(chart2d);
            editChartWindow.Show();
        }

        private void exportChartToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            chart2d.saveImage();
        }

        public void aligmentLegendComboBox_SelectedIndexChanged(object s, System.EventArgs e)
        {
            chart2d.changeChartLegendAligment(((System.Windows.Forms.ToolStripComboBox) (s)).SelectedItem.ToString());
        }

        public void positionLegendComboBox_SelectedIndexChanged(object s, System.EventArgs e)
        {
            chart2d.changeChartLegendPosition(((System.Windows.Forms.ToolStripComboBox) (s)).SelectedItem.ToString());
        }

        public void colorsOfChartComboBox_SelectedIndexChanged(object s, System.EventArgs e)
        {
            chart2d.changeChartColor(((System.Windows.Forms.ToolStripComboBox) (s)).SelectedItem.ToString());
        }

        public void seriesOfChartComboBox_SelectedIndexChanged(object s, System.EventArgs e)
        {
            chart2d.changeSeries(((System.Windows.Forms.ToolStripComboBox) (s)).SelectedItem.ToString());
        }

        public void typeOfChartComboBox_SelectedIndexChanged(object s, System.EventArgs e)
        {
            chart2d.changeChartType(((System.Windows.Forms.ToolStripComboBox) (s)).SelectedItem.ToString());
        }

        private void closeToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void expressionTextBox_KeyPress(object s, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) 13)
                defaultActions[tabControl1.SelectedIndex].Invoke(s, e);
        }

        private void symbolicOperationButton_Click(object sender, System.EventArgs e)
        {
        }

        private void clearChartButton_Click(object sender, System.EventArgs e)
        {
            // this.chart2d.DataBindings
            chart2d.ClearAll();
            complexChart.ClearAll();
            chart3d.Clear();
        }

        private void exportToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            complexChart.saveImage();
        }

        private void editToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            var editComplexChartWindow = new Charting.ComplexCharting.EditComplexChartWindow(complexChart);
            editComplexChartWindow.Show();
        }

        private void Item_Click(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var menuItem = (sender as System.Windows.Forms.ToolStripMenuItem);
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (tabControl1.SelectedIndex < 4)
                    expressionTextBox.AppendText(menuItem.Text);
                else if (tabControl1.SelectedIndex == 4)
                {
                    scriptingCodeEditor.AppendText(menuItem.Text);
                }
                else if (tabControl1.SelectedIndex == 5)
                    customFunctionsCodeEditor.AppendText(menuItem.Text);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (Config.GlobalConfig.functionsDetails.ContainsKey(menuItem.Text))
                {
                    menuFunctionsToolTip.setFunctionInfo(Config.GlobalConfig.functionsDetails[menuItem.Text]);
                    //menuFunctionsToolTip.Show(this, menuItem.Width + 3, 0);
                    menuFunctionsToolTip.Show();
                }
            }
        }

        private void operationNumericalCalculationsComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            NumericalCalculations.NumericalCalculation.setupMethods(methodNumericalCalculationsComboBox,
                operationNumericalCalculationsComboBox);

            NumericalCalculations.NumericalCalculation.setupGroupBoxes(operationNumericalCalculationsComboBox,
                derivativeAtPointGroupBox,
                rootOfFunctionGroupBox, numericalIntegrationGroupBox);
        }

        private void numericalOperationButton_Click(object sender, System.EventArgs e)
        {
            var method = methodNumericalCalculationsComboBox.SelectedItem.ToString();

            if (_calculationsMode == Evaluation.CalculationsMode.Real)
            {
                var function = evaluator2d.Evaluate(expressionTextBox.Text,
                    customFunctionsCodeEditor.Text);
                System.Func<double, double> fx = (double x) => function.Evaluate(x);

                double result;
                switch (method)
                {
                    case "trapezoidal method":
                        result = NumericalCalculations.MathematicalAnalysis.Integral.trapezoidalMethod(fx,
                            (double) (aIntervalIntegrationNumericUpDown.Value),
                            (double) (bIntervalIntegrationNumericUpDown.Value));
                        resultNumericalCalculationsTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(result);
                        numericalCalculationsDataGridView.Rows.Insert(0, expressionTextBox.Text,
                            operationNumericalCalculationsComboBox.SelectedItem,
                            methodNumericalCalculationsComboBox.SelectedItem,
                            "a=" + aIntervalIntegrationNumericUpDown.Value + "; b=" +
                            bIntervalIntegrationNumericUpDown.Value, resultNumericalCalculationsTextBox.Text);
                        break;

                    case "rectangle method":
                        result = NumericalCalculations.MathematicalAnalysis.Integral.rectangleMethod(fx,
                            (double) (aIntervalIntegrationNumericUpDown.Value),
                            (double) (bIntervalIntegrationNumericUpDown.Value));
                        resultNumericalCalculationsTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(result);
                        numericalCalculationsDataGridView.Rows.Insert(0, expressionTextBox.Text,
                            operationNumericalCalculationsComboBox.SelectedItem,
                            methodNumericalCalculationsComboBox.SelectedItem,
                            "a=" + aIntervalIntegrationNumericUpDown.Value + "; b=" +
                            bIntervalIntegrationNumericUpDown.Value, resultNumericalCalculationsTextBox.Text);
                        break;

                    case "Simpson's method":
                        result = NumericalCalculations.MathematicalAnalysis.Integral.simpsonMethod(fx,
                            (double) (aIntervalIntegrationNumericUpDown.Value),
                            (double) (bIntervalIntegrationNumericUpDown.Value));
                        resultNumericalCalculationsTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(result);
                        numericalCalculationsDataGridView.Rows.Insert(0, expressionTextBox.Text,
                            operationNumericalCalculationsComboBox.SelectedItem,
                            methodNumericalCalculationsComboBox.SelectedItem,
                            "a=" + aIntervalIntegrationNumericUpDown.Value + "; b=" +
                            bIntervalIntegrationNumericUpDown.Value, resultNumericalCalculationsTextBox.Text);
                        break;

                    case "double exponential transformation":
                        result = NumericalCalculations.MathematicalAnalysis.Integral.doubleExponentialTransformation(fx,
                            (double) (aIntervalIntegrationNumericUpDown.Value),
                            (double) (bIntervalIntegrationNumericUpDown.Value));
                        resultNumericalCalculationsTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(result);
                        numericalCalculationsDataGridView.Rows.Insert(0, expressionTextBox.Text,
                            operationNumericalCalculationsComboBox.SelectedItem,
                            methodNumericalCalculationsComboBox.SelectedItem,
                            "a=" + aIntervalIntegrationNumericUpDown.Value + "; b=" +
                            bIntervalIntegrationNumericUpDown.Value, resultNumericalCalculationsTextBox.Text);
                        break;


                    case "non-adaptive Gauss–Kronrod method":
                        result = NumericalCalculations.MathematicalAnalysis.Integral.nonAdaptiveGaussKronrodMethod(fx,
                            (double) (aIntervalIntegrationNumericUpDown.Value),
                            (double) (bIntervalIntegrationNumericUpDown.Value));
                        resultNumericalCalculationsTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(result);
                        numericalCalculationsDataGridView.Rows.Insert(0, expressionTextBox.Text,
                            operationNumericalCalculationsComboBox.SelectedItem,
                            methodNumericalCalculationsComboBox.SelectedItem,
                            "a=" + aIntervalIntegrationNumericUpDown.Value + "; b=" +
                            bIntervalIntegrationNumericUpDown.Value, resultNumericalCalculationsTextBox.Text);
                        break;


                    case "infinity-adaptive Gauss–Kronrod method":
                        result =
                            NumericalCalculations.MathematicalAnalysis.Integral.infiniteAdaptiveGaussKronrodMethod(fx,
                                (double) (aIntervalIntegrationNumericUpDown.Value),
                                (double) (bIntervalIntegrationNumericUpDown.Value));
                        resultNumericalCalculationsTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(result);
                        numericalCalculationsDataGridView.Rows.Insert(0, expressionTextBox.Text,
                            operationNumericalCalculationsComboBox.SelectedItem,
                            methodNumericalCalculationsComboBox.SelectedItem,
                            "a=" + aIntervalIntegrationNumericUpDown.Value + "; b=" +
                            bIntervalIntegrationNumericUpDown.Value, resultNumericalCalculationsTextBox.Text);
                        break;

                    case "Monte Carlo method":
                        result = NumericalCalculations.MathematicalAnalysis.Integral.monteCarloMethod(fx,
                            (double) (aIntervalIntegrationNumericUpDown.Value),
                            (double) (bIntervalIntegrationNumericUpDown.Value));
                        resultNumericalCalculationsTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(result);
                        numericalCalculationsDataGridView.Rows.Insert(0, expressionTextBox.Text,
                            operationNumericalCalculationsComboBox.SelectedItem,
                            methodNumericalCalculationsComboBox.SelectedItem,
                            "a=" + aIntervalIntegrationNumericUpDown.Value + "; b=" +
                            bIntervalIntegrationNumericUpDown.Value, resultNumericalCalculationsTextBox.Text);
                        break;

                    case "Romberg's method":
                        result = NumericalCalculations.MathematicalAnalysis.Integral.rombergMethod(fx,
                            (double) (aIntervalIntegrationNumericUpDown.Value),
                            (double) (bIntervalIntegrationNumericUpDown.Value));
                        resultNumericalCalculationsTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(result);
                        numericalCalculationsDataGridView.Rows.Insert(0, expressionTextBox.Text,
                            operationNumericalCalculationsComboBox.SelectedItem,
                            methodNumericalCalculationsComboBox.SelectedItem,
                            "a=" + aIntervalIntegrationNumericUpDown.Value + "; b=" +
                            bIntervalIntegrationNumericUpDown.Value, resultNumericalCalculationsTextBox.Text);
                        break;


                    case "finite difference formula":
                        result = NumericalCalculations.MathematicalAnalysis.Derivative.finiteDifferenceFormula(fx,
                            (double) xDerivativePointNumericUpDown.Value,
                            (uint) nOrderDerivativeNumericUpDown.Value);
                        resultNumericalCalculationsTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(result);
                        numericalCalculationsDataGridView.Rows.Insert(0, expressionTextBox.Text,
                            operationNumericalCalculationsComboBox.SelectedItem,
                            methodNumericalCalculationsComboBox.SelectedItem,
                            "n=" + nOrderDerivativeNumericUpDown.Value + "; x=" +
                            xDerivativePointNumericUpDown.Value, resultNumericalCalculationsTextBox.Text);
                        break;

                    case "stable finite difference formula":
                        result = NumericalCalculations.MathematicalAnalysis.Derivative.stableFiniteDifferenceFormula(fx,
                            (double) xDerivativePointNumericUpDown.Value, (uint) nOrderDerivativeNumericUpDown.Value);
                        resultNumericalCalculationsTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(result);
                        numericalCalculationsDataGridView.Rows.Insert(0, expressionTextBox.Text,
                            operationNumericalCalculationsComboBox.SelectedItem,
                            methodNumericalCalculationsComboBox.SelectedItem,
                            "n=" + nOrderDerivativeNumericUpDown.Value + "; x=" +
                            xDerivativePointNumericUpDown.Value, resultNumericalCalculationsTextBox.Text);

                        break;

                    case "two-point finite difference formula":
                        result =
                            NumericalCalculations.MathematicalAnalysis.Derivative.twoPointfiniteDifferenceFormula(fx,
                                (double) xDerivativePointNumericUpDown.Value, (uint) nOrderDerivativeNumericUpDown.Value);
                        resultNumericalCalculationsTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(result);
                        numericalCalculationsDataGridView.Rows.Insert(0, expressionTextBox.Text,
                            operationNumericalCalculationsComboBox.SelectedItem,
                            methodNumericalCalculationsComboBox.SelectedItem,
                            "n=" + nOrderDerivativeNumericUpDown.Value + "; x=" +
                            xDerivativePointNumericUpDown.Value, resultNumericalCalculationsTextBox.Text);
                        break;

                    case "centered five-point method":
                        result = NumericalCalculations.MathematicalAnalysis.Derivative.centeredFivePointMethod(fx,
                            (double) xDerivativePointNumericUpDown.Value,
                            (uint) nOrderDerivativeNumericUpDown.Value);
                        resultNumericalCalculationsTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(result);
                        numericalCalculationsDataGridView.Rows.Insert(0, expressionTextBox.Text,
                            operationNumericalCalculationsComboBox.SelectedItem,
                            methodNumericalCalculationsComboBox.SelectedItem,
                            "n=" + nOrderDerivativeNumericUpDown.Value + "; x=" +
                            xDerivativePointNumericUpDown.Value, resultNumericalCalculationsTextBox.Text);
                        break;

                    case "bisection method":
                        result = NumericalCalculations.ElementaryMathematics.FunctionRoot.bisectionMethod(fx,
                            (double) aFunctionRootNumericUpDown.Value,
                            (double) bFunctionRootNumericUpDown.Value);
                        resultNumericalCalculationsTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(result);
                        numericalCalculationsDataGridView.Rows.Insert(0, expressionTextBox.Text,
                            operationNumericalCalculationsComboBox.SelectedItem,
                            methodNumericalCalculationsComboBox.SelectedItem,
                            "a=" + aFunctionRootNumericUpDown.Value + "; b=" + bFunctionRootNumericUpDown.Value,
                            resultNumericalCalculationsTextBox.Text);
                        break;

                    case "secant method":
                        result = NumericalCalculations.ElementaryMathematics.FunctionRoot.secantMethod(fx,
                            (double) aFunctionRootNumericUpDown.Value,
                            (double) bFunctionRootNumericUpDown.Value);
                        resultNumericalCalculationsTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(result);
                        numericalCalculationsDataGridView.Rows.Insert(0, expressionTextBox.Text,
                            operationNumericalCalculationsComboBox.SelectedItem,
                            methodNumericalCalculationsComboBox.SelectedItem,
                            "a=" + aFunctionRootNumericUpDown.Value + "; b=" + bFunctionRootNumericUpDown.Value,
                            resultNumericalCalculationsTextBox.Text);
                        break;

                    case "Brent's method":
                        result = NumericalCalculations.ElementaryMathematics.FunctionRoot.BrentMethod(fx,
                            (double) aFunctionRootNumericUpDown.Value,
                            (double) bFunctionRootNumericUpDown.Value);
                        resultNumericalCalculationsTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(result);
                        numericalCalculationsDataGridView.Rows.Insert(0, expressionTextBox.Text,
                            operationNumericalCalculationsComboBox.SelectedItem,
                            methodNumericalCalculationsComboBox.SelectedItem,
                            "a=" + aFunctionRootNumericUpDown.Value + "; b=" + bFunctionRootNumericUpDown.Value,
                            resultNumericalCalculationsTextBox.Text);
                        break;

                    case "Broyden's method":
                        result = NumericalCalculations.ElementaryMathematics.FunctionRoot.BroydenMethod(fx,
                            (double) aFunctionRootNumericUpDown.Value,
                            (double) bFunctionRootNumericUpDown.Value);
                        resultNumericalCalculationsTextBox.Text = Evaluation.ScriptingExtensions.ToMathString(result);
                        numericalCalculationsDataGridView.Rows.Insert(0, expressionTextBox.Text,
                            operationNumericalCalculationsComboBox.SelectedItem,
                            methodNumericalCalculationsComboBox.SelectedItem,
                            "a=" + aFunctionRootNumericUpDown.Value + "; b=" + bFunctionRootNumericUpDown.Value,
                            resultNumericalCalculationsTextBox.Text);
                        break;
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show(
                    Localization.Strings
                        .GUI_numericalOperationButton_Click_Only_Real_mode_is_supported_in_Numerical_calculations_right_now__more_to_come_in_next_versions_ +
                    System.Environment.NewLine +
                    Localization.Strings.GUI_numericalOperationButton_Click__Check__Real___f_x___mode,
                    Localization.Strings.GUI_numericalOperationButton_Click_Warning_);
            }
        }

        private void epsilonDerrivativeTextBox_TextChanged(object sender, System.EventArgs e)
        {
            double result;
            if (double.TryParse(epsilonDerrivativeTextBox.Text, out result))
            {
                if (result > 0.0 && result < 1)
                    NumericalCalculations.MathematicalAnalysis.Derivative.EPS = result;
                else
                    System.Windows.Forms.MessageBox.Show(
                        Localization.Strings.GivenΕIsNotValidΕShouldBeSmallPositiveNumber, Localization.Strings.Error);
            }
            else
                System.Windows.Forms.MessageBox.Show(Localization.Strings.GivenΕIsNotValid, Localization.Strings.Error);
        }

        private void aboutToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            //MessageBox.Show("Computator.NET "+GlobalConfig.version+"\nthis is beta version, some functions may not work properly\n\nAuthor: Paweł Troka\nE-mail: ptroka@fizyka.dk\nWebsite: http://fizyka.dk", "About Computator.NET");
            var about = new AboutBox1();
            about.ShowDialog();
        }

        private void processButton_Click(object sender, System.EventArgs e)
        {
            consoleOutputTextBox.Text = (Localization.Strings.ConsoleOutput);
            try
            {
                scriptingCodeEditor.ProcessScript(consoleOutputTextBox, customFunctionsCodeEditor.Text);
            }
            catch (System.Exception ex)
            {
                var message = ex.Message + " " + (ex.InnerException?.Message ?? "");
                System.Windows.Forms.MessageBox.Show(message, Localization.Strings.Error);

                if (!ExceptionExtensions.IsInternal(ex))
                {
                    logger.MethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                    logger.Log(message, Config.ErrorType.General, ex);
                }
            }
        }

        private void featuresToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show(Config.GlobalConfig.features, Localization.Strings.Features);
        }

        private void thanksToToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show(
                Config.GlobalConfig.betatesters + System.Environment.NewLine + Config.GlobalConfig.translators +
                System.Environment.NewLine +
                Config.GlobalConfig.libraries + System.Environment.NewLine +
                Config.GlobalConfig.others, Localization.Strings.SpecialThanksTo);
        }

        private void changelogToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            using (var sr = new System.IO.StreamReader(Config.GlobalConfig.FullPath("CHANGELOG")))
            {
                var changelogForm = new System.Windows.Forms.Form
                {
                    Text = Localization.Strings.GUI_changelogToolStripMenuItem_Click_Changelog,
                    Size = Size
                };
                changelogForm.Controls.Add(new System.Windows.Forms.RichTextBox
                {
                    Text = sr.ReadToEnd(),
                    ReadOnly = true,
                    Dock = System.Windows.Forms.DockStyle.Fill
                });
                changelogForm.ShowDialog();
            }
        }

        private void bugReportingToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show(Localization.Strings.PleaseReportAnyBugsToPawełTrokaPtrokaFizykaDk,
                Localization.Strings.BugReporting);
        }

        private void saveScriptButton_Click(object sender, System.EventArgs e)
        {
            var result = saveScriptFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                using (var sw = new System.IO.StreamWriter(saveScriptFileDialog.FileName))
                {
                    sw.Write(scriptingCodeEditor.Text);
                }
                directoryTree1.Refresh();
                directoryTree1.Invalidate();
            }
        }

        private void openScriptButton_Click(object sender, System.EventArgs e)
        {
            var result = openScriptFileDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                using (var sr = new System.IO.StreamReader(openScriptFileDialog.FileName))
                {
                    scriptingCodeEditor.Text = sr.ReadToEnd();
                }
            }
        }

        private void transformToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            var menuitem = sender as System.Windows.Forms.ToolStripDropDownItem;

            if (_calculationsMode == Evaluation.CalculationsMode.Real)
                chart2d.Transform(
                    points => Transformations.MathematicalTransformations.Transform(points, menuitem.Text),
                    menuitem.Text);
            //  else if (complexNumbersModeRadioBox.Checked)
            //    else if(fxyModeRadioBox.Checked)
        }

        private void openCustomFunctions_Click(object sender, System.EventArgs e)
        {
            var result = openCustomFunctionsFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                using (var sr = new System.IO.StreamReader(openCustomFunctionsFileDialog.FileName))
                {
                    customFunctionsCodeEditor.Text = sr.ReadToEnd();
                }
            }
        }

        private void saveCustomFunctions_Click(object sender, System.EventArgs e)
        {
            var result = saveCustomFunctionsFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                using (var sw = new System.IO.StreamWriter(saveCustomFunctionsFileDialog.FileName))
                {
                    sw.Write(customFunctionsCodeEditor.Text);
                }
                customFunctionsDirectoryTree.Refresh();
                customFunctionsDirectoryTree.Invalidate();
            }
        }

        private void customFunctionsDirectoryTree_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            if (System.IO.File.Exists(customFunctionsDirectoryTree.SelectedNode.FullPath))
            {
                using (var sr = new System.IO.StreamReader(customFunctionsDirectoryTree.SelectedNode.FullPath))
                {
                    customFunctionsCodeEditor.Text = sr.ReadToEnd();
                }
            }
        }

        private void countourLinesToolStripComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            complexChart.countourMode =
                (Charting.ComplexCharting.CountourLinesMode) countourLinesToolStripComboBox.SelectedItem;
            complexChart.reDraw();
        }

        private void colorAssignmentToolStripComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            complexChart.colorAssignmentMethod =
                (Charting.ComplexCharting.AssignmentOfColorMethod) colorAssignmentToolStripComboBox.SelectedItem;
            complexChart.reDraw();
        }

        private void directoryTree1_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            if (System.IO.File.Exists(directoryTree1.SelectedNode.FullPath))
            {
                using (var sr = new System.IO.StreamReader(directoryTree1.SelectedNode.FullPath))
                {
                    scriptingCodeEditor.Text = sr.ReadToEnd();
                }
            }
        }

        private void trackBar1_Scroll(object sender, System.EventArgs e)
        {
            chart3d.Quality =
                chart2d.Quality = complexChart.Quality = (trackBar1.Value/((double) trackBar1.Maximum))*100.0;
        }

        private void exportChart3dToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            var saveFileDialog2 = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "Png Image (.png)|*.png|Gif Image (.gif)|*.gif|JPEG Image (.jpeg)|*.jpeg|Bitmap Im" +
                         "age (.bmp)|*.bmp|Tiff Image (.tiff)|*.tiff|Wmf Image (.wmf)|*.wmf",
                FileName = Localization.Strings.Chart + System.DateTime.Now.ToShortDateString() + " "
                           + System.DateTime.Now.ToLongTimeString().Replace(':', '-')
                           + ".png"
            };


            var dialogResult = saveFileDialog2.ShowDialog();
            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                System.Threading.Thread.Sleep(20);
                ExportChartImage(saveFileDialog2.FileName, saveFileDialog2);
            }
        }

        private void editChart3dToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            var editChartWindow = new Charting.Chart3D.EditChartWindow(chart3d, elementHostChart3d);
            editChartWindow.ShowDialog();
        }

        private void editChart3dPropertiesToolStripMenuItem2_Click(object sender, System.EventArgs e)
        {
            var editChartProperties = new Charting.EditChartProperties(chart3d);
            if (editChartProperties.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
            }
        }

        private void chart3dEqualAxesToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            chart3d.EqualAxes = true;
        }

        private void chart3dFitAxesToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            chart3d.EqualAxes = false;
        }

        private void editPropertiesToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            var editChartProperties = new Charting.EditChartProperties(complexChart);
            if (editChartProperties.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                complexChart.reDraw();
            }
        }

        private void editPropertiesToolStripMenuItem1_Click(object sender, System.EventArgs e)
        {
            var editChartProperties = new Charting.EditChartProperties(chart2d);
            if (editChartProperties.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                chart2d.Invalidate();
            }
        }

        private void benchmarkToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            var bch = new Benchmarking.BenchmarkForm();
            bch.Show();
        }

        #endregion
    }
}