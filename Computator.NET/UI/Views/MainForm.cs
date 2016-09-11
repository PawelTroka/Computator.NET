#define PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
using System;
using System.Windows.Forms;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Views
{
    public partial class MainForm : Form, IMainForm
    {
        private ISharedViewState sharedViewState;

        #region initialization and construction

        public MainForm()
        {

            InitializeComponent();
            symbolicCalculationsTabPage.Enabled = false;
        }

        public MainForm(IMenuStripView menuStripView, IToolbarView toolbarView, ICalculationsView calculationsView, INumericalCalculationsView numericalCalculationsView, IScriptingView scriptingView, ICustomFunctionsView customFunctionsView, IChartingView chartingView, ISharedViewState sharedViewState, IExpressionView expressionView) : this()
        {
            this.sharedViewState = sharedViewState;
            MenuStripView = menuStripView;
            ToolbarView = toolbarView;
            CalculationsView = calculationsView;
            NumericalCalculationsView = numericalCalculationsView;
            ScriptingView = scriptingView;
            CustomFunctionsView = customFunctionsView;
            ChartingView = chartingView;
            ExpressionView = expressionView;


            (ExpressionView as Control).Dock=DockStyle.Top;
            Controls.Add(ExpressionView as Control);

            (ToolbarView as Control).Dock=DockStyle.Top;
            Controls.Add(ToolbarView as Control);

            (MenuStripView as Control).Dock = DockStyle.Top;
            Controls.Add(MenuStripView as Control);

            (ChartingView as Control).Dock = DockStyle.Fill;
            chartingTabPage.Controls.Add(ChartingView as Control);

            (CalculationsView as Control).Dock = DockStyle.Fill;
            calculationsTabPage.Controls.Add(CalculationsView as Control);

            (NumericalCalculationsView as Control).Dock = DockStyle.Fill;
            numericalCalculationsTabPage.Controls.Add(NumericalCalculationsView as Control);

            (ScriptingView as Control).Dock = DockStyle.Fill;
            scriptingTabPage.Controls.Add(ScriptingView as Control);

            (CustomFunctionsView as Control).Dock = DockStyle.Fill;
            customFunctionsTabPage.Controls.Add(CustomFunctionsView as Control);
        }

        #endregion

        #region IMainForm

        public IMenuStripView MenuStripView { get; }

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

        public IExpressionView ExpressionView { get; }


        public event EventHandler SelectedViewChanged
        {
            add { tabControl1.SelectedIndexChanged += value; }
            remove { tabControl1.SelectedIndexChanged -= value; }
        }

        #endregion
    }
}