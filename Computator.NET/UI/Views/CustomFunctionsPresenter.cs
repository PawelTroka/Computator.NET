using Computator.NET.DataTypes;

namespace Computator.NET.UI.Views
{
    public class CustomFunctionsPresenter
    {
        private ICustomFunctionsView _view;

        public CustomFunctionsPresenter(ICustomFunctionsView view)
        {
            _view = view;
            var solutionExplorerPresenter = new SolutionExplorerPresenter(_view.SolutionExplorerView, _view.CustomFunctionsEditor, false);
            EventAggregator.Instance.Subscribe<ErrorsInCustomFunctionsEvent>(eicc =>
            {
                
                _view.CustomFunctionsEditor.HighlightErrors(eicc.Errors);
            });
            EventAggregator.Instance.Subscribe<NoErrorsInCustomFunctionsEvent>((a) =>
            {
                _view.CustomFunctionsEditor.ClearHighlightedErrors();
            });
        }
    }
}