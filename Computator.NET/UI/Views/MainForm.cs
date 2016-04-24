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
    public partial class MainForm : LocalizedForm, IMainForm
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


        

        #region initialization and construction

        public MainForm()
        {
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


    }
}