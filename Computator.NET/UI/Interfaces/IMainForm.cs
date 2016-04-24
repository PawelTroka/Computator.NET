using System;
using System.Windows.Forms;
using Computator.NET.UI.MVP.Views;
using Computator.NET.UI.Views;

namespace Computator.NET
{
    public interface IMainForm
    {
        void Restart();
        IToolbarView ToolbarView { get; }

        IToolbarView MenuStripView { get; }
        IChartingView ChartingView { get; }

        ICalculationsView CalculationsView { get; }

        INumericalCalculationsView NumericalCalculationsView { get; }

        IScriptingView ScriptingView { get; }

        ICustomFunctionsView CustomFunctionsView { get; }

        IExpressionView ExpressionView { get; }
        string ModeText { get; set; }
        string StatusText { set; }
        int SelectedViewIndex { get; set; }
        FormBorderStyle FormBorderStyle { set; }
        FormWindowState WindowState {  set; }

   

        event EventHandler ModeForcedToReal;
        event EventHandler ModeForcedToComplex;
        event EventHandler ModeForcedToFxy;


        void SendStringAsKey(string key);


        event EventHandler Load;
        event EventHandler SelectedViewChanged;
    }
}