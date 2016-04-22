using System;
using System.Windows.Forms;
using Computator.NET.UI.MVP.Views;
using Computator.NET.UI.Views;

namespace Computator.NET
{
    public interface IMainForm
    {
        IToolbarView ToolbarView { get; }

        IToolbarView MenuStripView { get; }
        IChartingView ChartingView { get; }

        ICalculationsView CalculationsView { get; }

        INumericalCalculationsView NumericalCalculationsView { get; }

        IScriptingView ScriptingView { get; }

        ICustomFunctionsView CustomFunctionsView { get; }
        string SelectedLanguage { get; set; }


        IExpressionView ExpressionView { get; }
        string ModeText { get; set; }
        string StatusText { set; }
        int SelectedViewIndex { get; set; }
        FormBorderStyle FormBorderStyle { set; }
        FormWindowState WindowState {  set; }

        void SetLanguages(object[] languages);

        event EventHandler ModeForcedToReal;
        event EventHandler ModeForcedToComplex;
        event EventHandler ModeForcedToFxy;


        void SendStringAsKey(string key);

     //   event EventHandler EnterClicked;

        event EventHandler SelectedLanguageChanged;

        event EventHandler SelectedViewChanged;
    }
}