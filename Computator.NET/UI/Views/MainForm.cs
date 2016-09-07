#define PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
using System;
using System.Windows.Forms;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Views
{
    public partial class MainForm : Form, IMainForm
    {
        #region initialization and construction

        public MainForm()
        {
            InitializeComponent();
            symbolicCalculationsTabPage.Enabled = false;
        }

        public MainForm(IToolbarView menuStripView, IToolbarView toolbarView, ICalculationsView calculationsView, INumericalCalculationsView numericalCalculationsView, IScriptingView scriptingView, ICustomFunctionsView customFunctionsView, IChartingView chartingView) : this()
        {
            MenuStripView = menuStripView;
            ToolbarView = toolbarView;
            CalculationsView = calculationsView;
            NumericalCalculationsView = numericalCalculationsView;
            ScriptingView = scriptingView;
            CustomFunctionsView = customFunctionsView;
            ChartingView = chartingView;
            InitializeComponent();

            Controls.Add(ExpressionView as Control);
            Controls.Add(ToolbarView as Control);
            Controls.Add(MenuStripView as Control);

            chartingTabPage.Controls.Add(ChartingView as Control);
            calculationsTabPage.Controls.Add(CalculationsView as Control);
            numericalCalculationsTabPage.Controls.Add(NumericalCalculationsView as Control);
            scriptingTabPage.Controls.Add(ScriptingView as Control);
            customFunctionsTabPage.Controls.Add(CustomFunctionsView as Control);

            symbolicCalculationsTabPage.Enabled = false;
        }

        #endregion

        #region IMainForm

        public IToolbarView MenuStripView { get; }

        public void Restart()
        {
            Application.Restart();
        }

        public IToolbarView ToolbarView { get; }
        public ICalculationsView CalculationsView { get; }

        public INumericalCalculationsView NumericalCalculationsView { get; }

        public IScriptingView ScriptingView { get; }
        public ICustomFunctionsView CustomFunctionsView { get; }
        public IChartingView ChartingView { get; }


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


        public event EventHandler SelectedViewChanged
        {
            add { tabControl1.SelectedIndexChanged += value; }
            remove { tabControl1.SelectedIndexChanged -= value; }
        }

        #endregion
    }
}