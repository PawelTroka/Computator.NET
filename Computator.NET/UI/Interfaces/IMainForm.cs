using System;
using System.Windows.Forms;
using Computator.NET.UI.Views;

namespace Computator.NET.UI.Interfaces
{
    public interface IMainForm
    {
        IToolbarView ToolbarView { get; }

        IMenuStripView MenuStripView { get; }
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
        FormWindowState WindowState { set; }
        void Restart();


        event EventHandler ModeForcedToReal;
        event EventHandler ModeForcedToComplex;
        event EventHandler ModeForcedToFxy;


        void SendStringAsKey(string key);


        event EventHandler Load;
        event EventHandler SelectedViewChanged;
    }
}