#define PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Computator.NET.Benchmarking;
using Computator.NET.Charting;
using Computator.NET.Charting.Chart3D;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.Config;
using Computator.NET.Data;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Evaluation;
using Computator.NET.Localization;
using Computator.NET.Logging;
using Computator.NET.Properties;
using Computator.NET.UI.AutocompleteMenu;
using Computator.NET.UI.Dialogs;
using Computator.NET.UI.MVP.Views;
using Computator.NET.UI.Views;

namespace Computator.NET
{
    public partial class GUI : LocalizedForm, IMainForm
    {

        #region IMainForm

        public IToolbarView MenuStripView { get; } = new MenuStripView() { Dock = DockStyle.Top };
        public void Restart()
        {
            Application.Restart();
        }

        public IToolbarView ToolbarView { get; } = new ToolBarView() {Dock=DockStyle.Top};
        public ICalculationsView CalculationsView { get; } = new CalculationsView {Dock = DockStyle.Fill};

        public INumericalCalculationsView NumericalCalculationsView { get; } = new NumericalCalculationsView
        {
            Dock = DockStyle.Fill
        };

        public IScriptingView ScriptingView { get; } = new ScriptingView {Dock = DockStyle.Fill};
        public ICustomFunctionsView CustomFunctionsView { get; } = new CustomFunctionsView {Dock = DockStyle.Fill};
        public IChartingView ChartingView { get; } = new ChartingView {Dock = DockStyle.Fill};

        public void SetLanguages(object[] languages)
        {
            languageToolStripComboBox.Items.Clear();
            languageToolStripComboBox.Items.AddRange(languages);
        }


        public string SelectedLanguage
        {
            get { return languageToolStripComboBox.SelectedItem.ToString(); }
            set { languageToolStripComboBox.SelectedItem = value; }
        }


        public string ModeText
        {
            get { return modeToolStripDropDownButton.Text; }
            set { modeToolStripDropDownButton.Text = value; }
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

        public string StatusText
        {
            set { toolStripStatusLabel1.Text = value; }
        }

        public int SelectedViewIndex
        {
            get { return tabControl1.SelectedIndex; }
            set { tabControl1.SelectedIndex = value; }
        }

        public IExpressionView ExpressionView { get; } = new ExpressionView {Dock = DockStyle.Top};


        public event EventHandler SelectedLanguageChanged
        {
            add { languageToolStripComboBox.SelectedIndexChanged += value; }
            remove { languageToolStripComboBox.SelectedIndexChanged -= value; }
        }

        public event EventHandler SelectedViewChanged
        {
            add { tabControl1.SelectedIndexChanged += value; }
            remove { tabControl1.SelectedIndexChanged -= value; }
        }

        #endregion

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var index = tabControl1.SelectedIndex;

            editChartMenus.chartToolStripMenuItem.Enabled = transformToolStripMenuItem.Enabled = index == 0;

            openToolStripMenuItem.Enabled = index == 0 || index == 5 || index == 4;

            saveToolStripMenuItem.Enabled = index == 4 || index == 5;
        }

        private readonly WebBrowserForm menuFunctionsToolTip = new WebBrowserForm();

        #region initialization and construction

        public GUI()
        {


            InitializeComponent();
            
            Controls.Add(ExpressionView as Control);
            Controls.Add(ToolbarView as Control);
            Controls.Add(MenuStripView as Control);


            Controls.SetChildIndex(ExpressionView as Control, 2);
            Controls.SetChildIndex(ToolbarView as Control, 3);
            Controls.SetChildIndex(ToolbarView as Control, 4);

            chartingTabPage.Controls.Add(ChartingView as Control);
            calculationsTabPage.Controls.Add(CalculationsView as Control);
            numericalCalculationsTabPage.Controls.Add(NumericalCalculationsView as Control);
            scriptingTabPage.Controls.Add(ScriptingView as Control);
            customFunctionsTabPage.Controls.Add(CustomFunctionsView as Control);

            InitializeFunctions();


            editChartMenus = new EditChartMenus(ChartingView.Charts[CalculationsMode.Real] as Chart2D,
                ChartingView.Charts[CalculationsMode.Complex] as ComplexChart,
                ChartingView.Charts[CalculationsMode.Fxy] as Chart3DControl,
                (( ChartingView.Charts[CalculationsMode.Fxy] as Chart3DControl).ParentControl));

            menuStrip2.Items.Insert(4, editChartMenus.chartToolStripMenuItem);


        //    exponentiationToolStripMenuItem.DataBindings.Add("Checked", SharedViewState.Instance, "IsExponent", false,
             //   DataSourceUpdateMode.OnPropertyChanged);

            Icon = Resources.computator_net_icon;

            symbolicCalculationsTabPage.Enabled = false;

            HandleCommandLine();

            toolStripMenuItem40.Click += (s, e) => new BenchmarkForm().ShowDialog(this);
            
            aboutToolStripMenuItem1.Click += (s, e2) => new AboutBox1().ShowDialog(this);

            toolStripMenuItem46.Click += (o, e) => new BugReportingForm().ShowDialog(this);
            toolStripMenuItem45.Click += (o, e) => new ChangelogForm().ShowDialog(this);
            optionsToolStripMenuItem1.Click += (sender, e) => new SettingsForm().ShowDialog(this);

            exitToolStripMenuItem.Click += (o, e) => Application.Exit();

            toolStripMenuItem43.Click += (s, e) => MessageBox.Show(Strings.featuresInclude, Strings.Features);

            toolStripMenuItem44.Click += (s, e) => MessageBox.Show(
                GlobalConfig.betatesters + Environment.NewLine + Environment.NewLine + GlobalConfig.translators +
                Environment.NewLine + Environment.NewLine +
                GlobalConfig.libraries + Environment.NewLine + Environment.NewLine +
                GlobalConfig.others, Strings.SpecialThanksTo);

            toolStripMenuItem41.Click += (o, e) =>
            {
                if (Directory.Exists(SimpleLogger.LogsDirectory))
                    Process.Start(SimpleLogger.LogsDirectory);
                else
                    MessageBox.Show(
                        Strings.GUI_logsToolStripMenuItem_Click_You_dont_have_any_logs_yet_);
            };

            fullscreenToolStripMenuItem.Click += (o, e) =>
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
            };
        }


        private readonly EditChartMenus editChartMenus;
        
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

        #endregion

        #region eventHandlers

        private void Item_Click(object sender, MouseEventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (e.Button == MouseButtons.Left)
            {
                if (tabControl1.SelectedIndex < 4)
                {
                } //TODO: MVP//   expressionTextBox.AppendText(menuItem.Text);
                else if (tabControl1.SelectedIndex == 4)
                {
                    //TODO: MVP
                    //scriptingCodeEditor.AppendText(menuItem.Text);
                }
                else if (tabControl1.SelectedIndex == 5)
                {
                }
                //TODO: MVP
                //customFunctionsCodeEditor.AppendText(menuItem.Text);
            }
            else if (e.Button == MouseButtons.Right && FunctionsDetails.Details.ContainsKey(menuItem.Text))
            {
                menuFunctionsToolTip.SetFunctionInfo(FunctionsDetails.Details[menuItem.Text]);
                //menuFunctionsToolTip.Show(this, menuItem.Width + 3, 0);
                menuFunctionsToolTip.Show();
            }
        }


        private void transformToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuitem = sender as ToolStripDropDownItem;

            ////  if (_calculationsMode == CalculationsMode.Real)
            /// //TODO: MVP
            //    chart2d.Transform(
            //      points => MathematicalTransformations.Transform(points, menuitem.Text),
            //    menuitem.Text);
            //  else if (complexNumbersModeRadioBox.Checked)
            //    else if(fxyModeRadioBox.Checked)
        }

        #endregion
    }
}