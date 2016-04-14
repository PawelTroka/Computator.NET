using System;
using Computator.NET.Compilation;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Evaluation;
using Computator.NET.Properties;
using Computator.NET.UI.CodeEditors;

namespace Computator.NET.UI.Views
{
    public class ScriptingViewPresenter
    {
        private readonly IScriptingView _view;
        private readonly ITextProvider _customFunctionsEditorView;
        private readonly IErrorHandler _errorHandler;

        public ScriptingViewPresenter(IScriptingView view, ITextProvider customFunctionsEditorView, IErrorHandler errorHandler)
        {
            _view = view;
            _customFunctionsEditorView = customFunctionsEditorView;
            this._errorHandler = errorHandler;
            _view.ProcessClicked += _view_ProcessClicked;
            var solutionExplorerPresenter = new SolutionExplorerPresenter(_view.SolutionExplorerView,_view.CodeEditorView,true);
}



        private readonly ScriptEvaluator _eval = new ScriptEvaluator();
        private void _view_ProcessClicked(object sender, EventArgs e)
        {
            _view.ConsoleOutput = Strings.ConsoleOutput;

            _view.CodeEditorView.ClearHighlightedErrors();

            try
            {
                var function = _eval.Evaluate(_view.CodeEditorView.Text, _customFunctionsEditorView.Text);
                function.Evaluate(output => _view.AppendToConsole(output));

            }
            catch (Exception ex)
            {
                var exception = ex as CompilationException;
                if (exception != null)
                {
                    _view.CodeEditorView.HighlightErrors(exception.Errors[CompilationErrorPlace.MainCode]);
                }
                ExceptionsHandler.Instance.HandleException(ex, _errorHandler);
            }
        }
    }
}