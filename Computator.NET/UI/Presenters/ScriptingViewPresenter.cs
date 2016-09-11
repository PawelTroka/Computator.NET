using System;
using Computator.NET.Compilation;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Evaluation;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.ErrorHandling;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Presenters
{
    public class ScriptingViewPresenter
    {
        private ISharedViewState _sharedViewState;

        private readonly IScriptEvaluator _eval;
        private readonly IScriptingView _view;

        public ScriptingViewPresenter(IScriptingView view, ISharedViewState sharedViewState, IExceptionsHandler exceptionsHandler, ICodeEditorView customFunctionsEditor, IScriptEvaluator eval)
        {
            _view = view;
            _sharedViewState = sharedViewState;
            _exceptionsHandler = exceptionsHandler;
            _customFunctionsEditor = customFunctionsEditor;
            _eval = eval;
            _view.ProcessClicked += _view_ProcessClicked;
            _sharedViewState.DefaultActions[ViewName.Scripting] = _view_ProcessClicked;
            var solutionExplorerPresenter = new SolutionExplorerPresenter(_view.SolutionExplorerView,
                _view.CodeEditorView, true);
        }
        private readonly IExceptionsHandler _exceptionsHandler;
        private readonly ICodeEditorView _customFunctionsEditor;

        private void _view_ProcessClicked(object sender, EventArgs e)
        {
            _view.ConsoleOutput = Strings.ConsoleOutput;

            _view.CodeEditorView.ClearHighlightedErrors();
            _customFunctionsEditor.ClearHighlightedErrors();

            try
            {
                var function = _eval.Evaluate(_view.CodeEditorView.Text, _customFunctionsEditor.Text);
                function.Evaluate(output => _view.AppendToConsole(output));
            }
            catch (Exception ex)
            {
                var exception = ex as CompilationException;
                if (exception != null)
                {
                    _view.CodeEditorView.HighlightErrors(exception.Errors[CompilationErrorPlace.MainCode]);
                }
                _exceptionsHandler.HandleException(ex);
            }
        }
    }
}