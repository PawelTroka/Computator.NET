using System;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Evaluation;
using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Controls;
using Computator.NET.UI.Views;

namespace Computator.NET
{
    public interface IMainForm
    {
        IChartAreaValuesView chartAreaValuesView1 { get; }
        ICalculationsView CalculationsView { get; }

        INumericalCalculationsView NumericalCalculationsView { get; }

        IScriptingView ScriptingView { get; }

        ICustomFunctionsView CustomFunctionsView { get; }

        void SetLanguages(object[] languages);
        string SelectedLanguage { get; set; }

        ReadOnlyDictionary<CalculationsMode, IChart> charts { get; }
        IExpressionView ExpressionView { get; }
        string ModeText { get; set; }
        string StatusText { set; }
        int SelectedViewIndex { get; set; }

        event EventHandler ModeForcedToReal;
        event EventHandler ModeForcedToComplex;
        event EventHandler ModeForcedToFxy;

        event EventHandler PrintClicked;
        event EventHandler PrintPreviewClicked;

        void SendStringAsKey(string key);

        event EventHandler EnterClicked;

        event EventHandler SelectedLanguageChanged;
    }
}