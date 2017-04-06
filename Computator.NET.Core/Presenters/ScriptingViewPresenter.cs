using System;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Model;
using Computator.NET.Core.Services;
using Computator.NET.Core.Services.ErrorHandling;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.Localization;
using NLog;

namespace Computator.NET.Core.Presenters
{
    public class ScriptingViewPresenter
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private ISharedViewState _sharedViewState;

        private readonly IScriptEvaluator _eval;
        private readonly IScriptingView _view;
        private readonly ICommandLineHandler _commandLineHandler;

        public ScriptingViewPresenter(IScriptingView view, ISharedViewState sharedViewState, IExceptionsHandler exceptionsHandler, ICodeEditorView customFunctionsEditor, IScriptEvaluator eval, ICommandLineHandler commandLineHandler)
        {
            _view = view;
            _sharedViewState = sharedViewState;
            _exceptionsHandler = exceptionsHandler;
            _customFunctionsEditor = customFunctionsEditor;
            _eval = eval;
            _commandLineHandler = commandLineHandler;
            _view.ProcessClicked += _view_ProcessClicked;
            _sharedViewState.DefaultActions[ViewName.Scripting] = _view_ProcessClicked;
            var solutionExplorerPresenter = new SolutionExplorerPresenter(_view.SolutionExplorerView,
                _view.CodeEditorView, true);

            //_view.Load += LoadFileFromCommandLine;
            LoadFileFromCommandLine();
        }

        private void LoadFileFromCommandLine()
        {
            Logger.Info($"{nameof(IScriptingView)} loaded");
            string filepath;
            if (_commandLineHandler.TryGetScriptingDocument(out filepath))
            {
                Logger.Info($"Got script document from command line '{filepath}'");
                _view.CodeEditorView.NewDocument(filepath);
                _sharedViewState.CurrentView = ViewName.Scripting;
            }
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