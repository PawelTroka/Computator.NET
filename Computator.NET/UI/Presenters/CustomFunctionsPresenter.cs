namespace Computator.NET.UI.Views
{
    public class CustomFunctionsPresenter
    {
        private readonly ICustomFunctionsView _view;

        public CustomFunctionsPresenter(ICustomFunctionsView view)
        {
            _view = view;
            var solutionExplorerPresenter = new SolutionExplorerPresenter(_view.SolutionExplorerView,
                _view.CustomFunctionsEditor, false);
        }
    }
}