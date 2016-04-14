#define PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Accord.Collections;
using Computator.NET.Benchmarking;
using Computator.NET.Charting;
using Computator.NET.Charting.Chart3D;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.Compilation;
using Computator.NET.Config;
using Computator.NET.Data;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Evaluation;
using Computator.NET.Localization;
using Computator.NET.Logging;
using Computator.NET.NumericalCalculations;
using Computator.NET.Properties;
using Computator.NET.Transformations;
using Computator.NET.UI.AutocompleteMenu;
using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Controls;
using Computator.NET.UI.Views;

namespace Computator.NET
{
    public partial class GUI : LocalizedForm, IMainForm
    {

        private readonly WebBrowserForm menuFunctionsToolTip = new WebBrowserForm();


        private Chart2D chart2d => charts[CalculationsMode.Real] as Chart2D;
        private Chart3DControl chart3d => charts[CalculationsMode.Fxy] as Chart3DControl;
        private ComplexChart complexChart => charts[CalculationsMode.Complex] as ComplexChart;
        public IChartAreaValuesView chartAreaValuesView1 { get; } = new ChartAreaValuesView {Dock = DockStyle.Right};
        public ICalculationsView CalculationsView { get; } = new CalculationsView {Dock = DockStyle.Fill};
        public INumericalCalculationsView NumericalCalculationsView { get; } = new NumericalCalculationsView() { Dock = DockStyle.Fill };

        public IScriptingView ScriptingView { get; } = new ScriptingView() {Dock = DockStyle.Fill};
        public ICustomFunctionsView CustomFunctionsView { get; } = new CustomFunctionsView() {Dock = DockStyle.Fill};

        public void SetLanguages(object[] languages)
        {
            languageToolStripComboBox.Items.Clear();
            languageToolStripComboBox.Items.AddRange(languages);
        }



        public string SelectedLanguage
        {
            get
            {
                return languageToolStripComboBox.SelectedItem.ToString();
            } 
            set { languageToolStripComboBox.SelectedItem = value; }
        }

        public ReadOnlyDictionary<CalculationsMode, IChart> charts { get; } =
            new ReadOnlyDictionary<CalculationsMode, IChart>(new Dictionary<CalculationsMode, IChart>
            {
                {CalculationsMode.Real, new Chart2D()},
                {CalculationsMode.Complex, new ComplexChart()},
                {CalculationsMode.Fxy, new Chart3DControl()}
            });

        public IExpressionView ExpressionView
        {
            get { return expressionTextBox; }
        }

        public string ModeText
        {
            get { return modeToolStripDropDownButton.Text; }
            set { modeToolStripDropDownButton.Text = value; }
        }

        public event EventHandler PrintClicked
        {
            add
            {
                printToolStripButton.Click += value;
                printToolStripMenuItem.Click += value;
            }
            remove
            {
                printToolStripButton.Click -= value;
                printToolStripMenuItem.Click -= value;
            }
        }

        public event EventHandler PrintPreviewClicked
        {
            add { printPreviewToolStripMenuItem.Click += value; }
            remove { printPreviewToolStripMenuItem.Click -= value; }
        }


        public event EventHandler ModeForcedToReal
        {
            add { dd212ToolStripMenuItem.Click += value; }
            remove { dd212ToolStripMenuItem.Click -= value; }
        }

        public event EventHandler ModeForcedToComplex
        {
            add { fdsfdsToolStripMenuItem.Click += value; }
            remove { fdsfdsToolStripMenuItem.Click -= value; }
        }

        public event EventHandler ModeForcedToFxy
        {
            add { mode3DFxyToolStripMenuItem.Click += value; }
            remove { mode3DFxyToolStripMenuItem.Click -= value; }
        }

        public void SendStringAsKey(string key)
        {
            SendKeys.Send(key);
        }

        public string StatusText {set { toolStripStatusLabel1.Text = value; } }

        public int SelectedViewIndex
        {
            get { return tabControl1.SelectedIndex; }
            set { tabControl1.SelectedIndex = value; }
        }

        public event EventHandler EnterClicked;
        public event EventHandler SelectedLanguageChanged
        {
            add { languageToolStripComboBox.SelectedIndexChanged += value; }
            remove { languageToolStripComboBox.SelectedIndexChanged -= value; }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var index = tabControl1.SelectedIndex;

            editChartMenus.chartToolStripMenuItem.Enabled = transformToolStripMenuItem.Enabled = index == 0;

            openToolStripMenuItem.Enabled = index == 0 || index == 5 || index == 4;

            saveToolStripMenuItem.Enabled = index == 4 || index == 5;

            //expressionTextBox.Visible = !(index ==5||index==4);
            tableLayoutPanel1.Visible = !(index == 5 || index == 4);
        }


        private void runToolStripButton_Click(object s, EventArgs e)
        {
            EnterClicked?.Invoke(s, e);
        }

        private void expressionTextBox_KeyPress(object s, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) 13)
                EnterClicked?.Invoke(s, e); //defaultActions[tabControl1.SelectedIndex].Invoke(s, e);
        }

        #region edit menu eventsi

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            switch (tabControl1.SelectedIndex)
            {
                case 4:
                    //TODO: MVP//        if (scriptingCodeEditor.Focused)
                    //TODO: MVP//        scriptingCodeEditor.Cut();
                    //TODO: MVP//       else
                    SendStringAsKey("^X");
                    break;

                case 5:
                    //TODO: MVP//        if (customFunctionsCodeEditor.Focused)
                    //TODO: MVP//         customFunctionsCodeEditor.Cut();
                    //TODO: MVP//         else
                    SendStringAsKey("^X");
                    break;

                default: //if (tabControl1.SelectedIndex < 4)
                    SendStringAsKey("^X"); //expressionTextBox.Cut();
                    break;
            }
#else
            SendStringAsKey("^X");
#endif
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            if (tabControl1.SelectedIndex < 4)
                SendStringAsKey("^Z"); //expressionTextBox.Undo();
            else if (tabControl1.SelectedIndex == 4)
            {
                //TODO: MVP//     if (scriptingCodeEditor.Focused)
                //TODO: MVP//      scriptingCodeEditor.Undo();
                //TODO: MVP//     else
                SendStringAsKey("^Z");
            }
            else
            {
                //TODO: MVP//      if (customFunctionsCodeEditor.Focused)
                //TODO: MVP//    customFunctionsCodeEditor.Undo();
                //TODO: MVP//    else
                SendStringAsKey("^Z");
            }
#else
            SendStringAsKey("^Z");

#endif
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            if (tabControl1.SelectedIndex < 4)
            {
                SendStringAsKey("^Y");
                //expressionTextBox.do()
            }
            else if (tabControl1.SelectedIndex == 4)
            //scriptingCodeEditor.Focus();
            {
                //TODO: MVP//     if (scriptingCodeEditor.Focused)
                //TODO: MVP//     scriptingCodeEditor.Redo();
                //TODO: MVP//    else
                SendStringAsKey("^Y");
            }
            else
            {
                //TODO: MVP//        if (customFunctionsCodeEditor.Focused)
                //TODO: MVP//     customFunctionsCodeEditor.Redo();
                //TODO: MVP//    else
                SendStringAsKey("^Y");
            }
#else
              SendStringAsKey("^Y");
#endif
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            if (tabControl1.SelectedIndex < 4)
            {
                SendStringAsKey("^C"); //expressionTextBox.Copy();
            }
            else if (tabControl1.SelectedIndex == 4)
            {
                //TODO: MVP//    if (scriptingCodeEditor.Focused)
                //TODO: MVP//     scriptingCodeEditor.Copy();
                //TODO: MVP//     else
                SendStringAsKey("^C");
            }
            else
            {
                //TODO: MVP//    if (customFunctionsCodeEditor.Focused)
                //TODO: MVP//    customFunctionsCodeEditor.Copy();
                //TODO: MVP//    else
                SendStringAsKey("^C");
            }
#else
            SendStringAsKey("^C");
#endif
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            if (tabControl1.SelectedIndex < 4)
            {
                SendStringAsKey("^V"); //expressionTextBox.Paste();
            }
            else if (tabControl1.SelectedIndex == 4)
            {
                //TODO: MVP//     if (scriptingCodeEditor.Focused)
                //TODO: MVP//     scriptingCodeEditor.Paste();
                //TODO: MVP//       else
                SendStringAsKey("^V");
            }
            else
            {
                //TODO: MVP//    if (customFunctionsCodeEditor.Focused)
                //TODO: MVP//     customFunctionsCodeEditor.Paste();
                //TODO: MVP//     else
                SendStringAsKey("^V");
            }

#else
            SendStringAsKey("^V");
#endif
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            if (tabControl1.SelectedIndex < 4)
            {
                SendStringAsKey("^A"); //expressionTextBox.SelectAll();
            }
            else if (tabControl1.SelectedIndex == 4)
            {
                //TODO: MVP//      if (scriptingCodeEditor.Focused)
                //TODO: MVP//      scriptingCodeEditor.SelectAll();
                //TODO: MVP//      else
                SendStringAsKey("^A");
            }
            else
            {
             //TODO: MVP//   if (customFunctionsCodeEditor.Focused)
                 //TODO: MVP//   customFunctionsCodeEditor.SelectAll();
              //  else
                    SendStringAsKey("^A");
            }
#else
            SendStringAsKey("^A");
#endif
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            switch (tabControl1.SelectedIndex)
            {
                case 0:

                    //SendStringAsKey("^S");
                    break;

                case 4:
                    //TODO: MVP//    scriptingCodeEditor.NewDocument();
                    break;

                case 5:
                    //TODO: MVP//   customFunctionsCodeEditor.NewDocument();
                    break;

                default:
                    //SendStringAsKey("^S");
                    break;
            }
#else
    //SendStringAsKey("^S");
#endif
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog {Filter = GlobalConfig.tslFilesFIlter};

            if (ofd.ShowDialog(this) != DialogResult.OK)
                return;


#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            switch (tabControl1.SelectedIndex)
            {
                case 0:

                    //SendStringAsKey("^S");
                    break;

                case 4:
                    //TODO: MVP//    scriptingCodeEditor.NewDocument(ofd.FileName);
                    break;

                case 5:
                    //TODO: MVP//    customFunctionsCodeEditor.NewDocument(ofd.FileName);
                    break;

                default:
                    //SendStringAsKey("^S");
                    break;
            }
#else
    //SendStringAsKey("^S");
#endif
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            switch (tabControl1.SelectedIndex)
            {
                case 0:

                    //SendStringAsKey("^S");
                    break;

                case 4:
                    //TODO: MVP// scriptingCodeEditor.Save();
                    break;

                case 5:
                    //TODO: MVP//customFunctionsCodeEditor.Save();
                    break;

                default:
                    //SendStringAsKey("^S");
                    break;
            }
#else
            SendStringAsKey("^S");
#endif
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
            switch (tabControl1.SelectedIndex)
            {
                case 0:

                    //SendStringAsKey("^S");
                    break;

                case 4:
                    //TODO: MVP//scriptingCodeEditor.SaveAs();
                    break;

                case 5:
                    //TODO: MVP//customFunctionsCodeEditor.SaveAs();
                    break;

                default:
                    //SendStringAsKey("^S");
                    break;
            }
#else
            SendStringAsKey("^S");
#endif
        }

        #endregion

        #region initialization and construction



        public GUI()
        {
            InitializeComponent();
            calculationsTabPage.Controls.Add(CalculationsView as Control);
            numericalCalculationsTabPage.Controls.Add(NumericalCalculationsView as Control);
            scriptingTabPage.Controls.Add(ScriptingView as Control);
            customFunctionsTabPage.Controls.Add(CustomFunctionsView as Control);

            InitializeFunctions();
            InitializeCharts(); //takes more time then it should


            InitializeScripting(); //takes a lot of time, TODO: optimize
            InitializeDataBindings();
           // BringToFront();
           // Focus();
            Icon = Resources.computator_net_icon;

            symbolicCalculationsTabPage.Enabled = false;

            HandleCommandLine();
        }


        private EditChartMenus editChartMenus;


        private void HandleCommandLine()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length < 2) return;
            if (!args[1].Contains(".tsl")) return;

            if (args[1].Contains(".tslf"))
            {
                //TODO: MVP
             //   customFunctionsCodeEditor.NewDocument(args[1]);
                //customFunctionsCodeEditor.CurrentFileName = args[1];
                // customFunctionsCodeEditor.Text = code;
                tabControl1.SelectedIndex = 5;
                tabControl1_SelectedIndexChanged(null, null);
            }
            else
            {
                //TODO: MVP
               // scriptingCodeEditor.NewDocument(args[1]);
                //scriptingCodeEditor.CurrentFileName = args[1];
                //scriptingCodeEditor.Text = code;

                tabControl1.SelectedIndex = 4;
                tabControl1_SelectedIndexChanged(null, null);
            }
        }


        private void InitializeFunctions()
        {
            var functions = FunctionsDetails.Details.ToArray();

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
                    var cat = new ToolStripMenuItem(f.Value.Category) {Name = f.Value.Category};
                    dict[f.Value.Type].DropDownItems.Add(cat);
                }

                var item = new ToolStripMenuItem
                {
                    Text = f.Value.Signature,
                    ToolTipText = f.Value.Title
                };
                //item.Click += Item_Click;
                item.MouseDown += Item_Click;

                (dict[f.Value.Type].DropDownItems[f.Value.Category] as ToolStripMenuItem)
                    .DropDownItems.Add(item);
            }
        }

        private void InitializeDataBindings()
        {
            exponentiationToolStripMenuItem.DataBindings.Add("Checked", expressionTextBox, "ExponentMode", false,
                DataSourceUpdateMode.OnPropertyChanged);

            //TODO: MVP
     //       scriptingCodeEditor.DataBindings.Add("ExponentMode", exponentiationToolStripMenuItem, "Checked", false,
 //               DataSourceUpdateMode.OnPropertyChanged);
   //         customFunctionsCodeEditor.DataBindings.Add("ExponentMode", exponentiationToolStripMenuItem, "Checked", false,
     //           DataSourceUpdateMode.OnPropertyChanged);
        }

        private void InitializeCharts()
        {
            panel2.Controls.Add(chartAreaValuesView1 as Control);

            panel2.Controls.Add(chart2d);
            panel2.Controls.Add(complexChart);
            panel2.Controls.Add(chart3d.ParentControl);
            chart2d.BringToFront();
            complexChart.BringToFront();
            chart3d.ParentControl.BringToFront();
            complexChart.Visible = false;
            chart3d.ParentControl.Visible = false;

            editChartMenus = new EditChartMenus(chart2d, complexChart, chart3d, chart3d.ParentControl);

            menuStrip2.Items.Insert(4, editChartMenus.chartToolStripMenuItem);
        }
        

        private void InitializeScripting()
        {            //TODO: MVP
           // customFunctionsCodeEditor 

            //splitContainer3.Panel1.Controls.Add(customFunctionsCodeEditor);

            //scriptingDirectoryTree.CodeEditorWrapper = scriptingCodeEditor;
            //customFunctionsDirectoryTree.CodeEditorWrapper = customFunctionsCodeEditor;
        }

        #endregion

        #region main events

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void symbolicOperationButton_Click(object sender, EventArgs e)
        {
        }

        #endregion

        #region eventHandlers

        private void Item_Click(object sender, MouseEventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (e.Button == MouseButtons.Left)
            {
                if (tabControl1.SelectedIndex < 4)
                    expressionTextBox.AppendText(menuItem.Text);
                else if (tabControl1.SelectedIndex == 4)
                {
                    //TODO: MVP
                    //scriptingCodeEditor.AppendText(menuItem.Text);
                }
                else if (tabControl1.SelectedIndex == 5) { }
                    //TODO: MVP
                    //customFunctionsCodeEditor.AppendText(menuItem.Text);
            }
            else if (e.Button == MouseButtons.Right && FunctionsDetails.Details.ContainsKey(menuItem.Text))
            {
                menuFunctionsToolTip.setFunctionInfo(FunctionsDetails.Details[menuItem.Text]);
                //menuFunctionsToolTip.Show(this, menuItem.Width + 3, 0);
                menuFunctionsToolTip.Show();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Computator.NET "+GlobalConfig.version+"\nthis is beta version, some functions may not work properly\n\nAuthor: Paweł Troka\nE-mail: ptroka@fizyka.dk\nWebsite: http://fizyka.dk", "About Computator.NET");
            var about = new AboutBox1();
            about.ShowDialog(this);
        }


        private void featuresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Strings.featuresInclude, Strings.Features);
        }

        private void thanksToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                GlobalConfig.betatesters + Environment.NewLine + Environment.NewLine + GlobalConfig.translators +
                Environment.NewLine + Environment.NewLine +
                GlobalConfig.libraries + Environment.NewLine + Environment.NewLine +
                GlobalConfig.others, Strings.SpecialThanksTo);
        }

        private void changelogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var sr = new StreamReader(GlobalConfig.FullPath("CHANGELOG")))
            {
                var changelogForm = new Form
                {
                    Text = Strings.GUI_changelogToolStripMenuItem_Click_Changelog,
                    Size = Size
                };
                changelogForm.Controls.Add(new RichTextBox
                {
                    Text = sr.ReadToEnd(),
                    ReadOnly = true,
                    Dock = DockStyle.Fill
                });
                changelogForm.ShowDialog(this);
            }
        }

        private void bugReportingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var richtextbox = new RichTextBox
            {
                Text =
                    $@"{Environment.NewLine}{Strings.PleaseReportAnyBugsToPawełTrokaPtrokaFizykaDk}{Environment.NewLine
                        }{Environment.NewLine}{
                        Strings
                            .GUI_bugReportingToolStripMenuItem_Click_Or_even_better_report_them_on_project_site__using_link_below
                        }{Environment.NewLine}{GlobalConfig.issuesUrl}",
                Dock = DockStyle.Fill,
                ReadOnly = true
            };

            richtextbox.LinkClicked += (ooo, eee) => Process.Start(GlobalConfig.issuesUrl);

            new Form
            {
                Size = new Size(650, 300),
                FormBorderStyle = FormBorderStyle.FixedDialog,

                // Set the MaximizeBox to false to remove the maximize box.
                MaximizeBox = false,

                // Set the MinimizeBox to false to remove the minimize box.
                MinimizeBox = false,

                // Set the start position of the form to the center of the screen.
                //StartPosition = FormStartPosition.CenterScreen,
                Controls =
                {
                    richtextbox
                },
                Font = new Font(FontFamily.GenericSansSerif, 17.0F),
                Text = Strings.BugReporting
            }.ShowDialog(this);
        }

        private void transformToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuitem = sender as ToolStripDropDownItem;

            ////  if (_calculationsMode == CalculationsMode.Real)//TODO: MVP
            chart2d.Transform(
                points => MathematicalTransformations.Transform(points, menuitem.Text),
                menuitem.Text);
            //  else if (complexNumbersModeRadioBox.Checked)
            //    else if(fxyModeRadioBox.Checked)
        }

        private void benchmarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var bch = new BenchmarkForm();
            bch.ShowDialog(this);
        }

        #endregion

        #region tools menu events



        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new SettingsForm().ShowDialog(this);
        }

        private void logsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(SimpleLogger.LogsDirectory))
                Process.Start(SimpleLogger.LogsDirectory);
            else
                MessageBox.Show(
                    Strings.GUI_logsToolStripMenuItem_Click_You_dont_have_any_logs_yet_);
        }

        private void fullscreenToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (fullscreenToolStripMenuItem.Checked)
            {
                // this.TopMost = true;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                // this.TopMost = false;
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = FormWindowState.Normal;
            }
        }

        #endregion
    }
}