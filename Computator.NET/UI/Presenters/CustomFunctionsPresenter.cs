using Computator.NET.DataTypes.Events;
using Computator.NET.UI.Interfaces;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Presenters
{
    public class CustomFunctionsPresenter
    {
        private readonly ICustomFunctionsView _view;
        private readonly ICommandLineHandler _commandLineHandler;
        private readonly ISharedViewState _sharedViewState;

        public CustomFunctionsPresenter(ICustomFunctionsView view, ICommandLineHandler commandLineHandler, ISharedViewState sharedViewState)
        {
            _view = view;
            _commandLineHandler = commandLineHandler;
            _sharedViewState = sharedViewState;
            var solutionExplorerPresenter = new SolutionExplorerPresenter(_view.SolutionExplorerView,
                _view.CustomFunctionsEditor, false);
            _view.Load += (o, e) =>
            {
                string filepath;
                if (_commandLineHandler.TryGetCustomFunctionsDocument(out filepath))
                {
                    _view.CustomFunctionsEditor.NewDocument(filepath);
                    _sharedViewState.CurrentView = ViewName.CustomFunctions;
                }
            };
        }
    }
}