using System;
using Computator.NET.Compilation;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Logging;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.ErrorHandling
{
    public class ExceptionsHandler : IExceptionsHandler
    {
        private readonly IErrorHandler _errorHandler;
        public ExceptionsHandler(ISharedViewState sharedViewState, IErrorHandler errorHandler, ISupportsExceptionHighliting customFunctionsEditor)
        {
            _sharedViewState = sharedViewState;
            _errorHandler = errorHandler;
            this._customFunctionsEditor = customFunctionsEditor;
        }

        public void HandleException(Exception ex)
        {
            HandleCustomFunctionsErrors(ex as CompilationException);

            var message = ex.Message + Environment.NewLine + (ex.InnerException?.Message ?? "");

            _errorHandler.DispalyError(message, Strings.Error);

            if (!ex.IsInternal())
            {
                _errorHandler.LogError(message, ErrorType.General, ex);
            }
        }

        private readonly ISharedViewState _sharedViewState;
        private readonly ISupportsExceptionHighliting _customFunctionsEditor;

        private void HandleCustomFunctionsErrors(CompilationException exception)
        {
            //CustomFunctionsCodeEditorControl.ClearHighlightedErrors();
            //    EventAggregator.Instance.Publish(new NoErrorsInCustomFunctionsEvent());

            if (exception == null)
                return;

            //   EventAggregator.Instance.Publish<ErrorsInCustomFunctionsEvent>(new ErrorsInCustomFunctionsEvent(exception.Errors[CompilationErrorPlace.CustomFunctions]));

            _customFunctionsEditor.HighlightErrors(
                exception.Errors[CompilationErrorPlace.CustomFunctions]);

            // CustomFunctionsCodeEditorControl.HighlightErrors(
            //   exception.Errors[CompilationErrorPlace.CustomFunctions]);

            if (exception.HasCustomFunctionsErrors && !exception.HasMainCodeErrors)
                _sharedViewState.CurrentView = ViewName.CustomFunctions;
            ///////////////////////////EventAggregator.Instance.Publish(new ChangeViewEvent(ViewName.CustomFunctions));
            //_view.SelectedViewIndex = 5; //tabControl1.SelectedTab = customFunctionsTabPage;
        }
    }
}