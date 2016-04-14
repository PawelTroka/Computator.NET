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
        private readonly ITextProvider CustomFunctionsEditorView;
        private readonly IErrorHandler errorHandler;

        public ScriptingViewPresenter(IScriptingView view, ITextProvider customFunctionsEditorView, IErrorHandler errorHandler)
        {
            _view = view;
            CustomFunctionsEditorView = customFunctionsEditorView;
            this.errorHandler = errorHandler;
            _view.DirectoryTree.Path = Settings.Default.ScriptingDirectory;
            _view.ProcessClicked += _view_ProcessClicked;
            _view.DirectoryChanged += _view_DirectoryChanged;
        }

        private void _view_DirectoryChanged(object sender, DirectorySelectedEventArgs e)
        {
            _view.DirectoryTree.Path = e.DirectoryName;
            Settings.Default.ScriptingDirectory = e.DirectoryName;
            Settings.Default.Save();
        }

        private readonly ScriptEvaluator _eval = new ScriptEvaluator();
        private void _view_ProcessClicked(object sender, EventArgs e)
        {
            _view.ConsoleOutput = Strings.ConsoleOutput;

            _view.CodeEditorView.ClearHighlightedErrors();

            try
            {
                var function = _eval.Evaluate(_view.CodeEditorView.Text, CustomFunctionsEditorView.Text);
                function.Evaluate(output => _view.AppendToConsole(output));

            }
            catch (Exception ex)
            {
                var exception = ex as CompilationException;
                if (exception != null)
                {
                    _view.CodeEditorView.HighlightErrors(exception.Errors[CompilationErrorPlace.MainCode]);
                }
                ExceptionsHandler.Instance.HandleException(ex, errorHandler);
            }
        }
    }
}