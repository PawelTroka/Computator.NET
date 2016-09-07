using System;
using Computator.NET.Compilation;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Logging;

namespace Computator.NET.UI.ErrorHandling
{
    public interface IErrorHandler
    {
        void DispalyError(string message, string title);
        void LogError(string message, ErrorType errorType, Exception ex);
    }

    public interface IExceptionsHandler
    {
        void HandleException(Exception ex);
    }

    public class ExceptionsHandler : IExceptionsHandler
    {
        private readonly IErrorHandler _errorHandler;
        private ExceptionsHandler(ISharedViewState sharedViewState, IErrorHandler errorHandler)
        {
            _sharedViewState = sharedViewState;
            _errorHandler = errorHandler;
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

        private ISharedViewState _sharedViewState;
        private void HandleCustomFunctionsErrors(CompilationException exception)
        {
            //CustomFunctionsCodeEditorControl.ClearHighlightedErrors();
            //    EventAggregator.Instance.Publish(new NoErrorsInCustomFunctionsEvent());

            if (exception == null)
                return;

            //   EventAggregator.Instance.Publish<ErrorsInCustomFunctionsEvent>(new ErrorsInCustomFunctionsEvent(exception.Errors[CompilationErrorPlace.CustomFunctions]));

            _sharedViewState.CustomFunctionsEditor.HighlightErrors(
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