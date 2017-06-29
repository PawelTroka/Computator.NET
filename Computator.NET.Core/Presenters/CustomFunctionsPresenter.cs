using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Model;
using Computator.NET.Core.Services;
using Computator.NET.DataTypes.Events;
using NLog;

namespace Computator.NET.Core.Presenters
{
    public class CustomFunctionsPresenter
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

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
            //_view.Load += LoadFileFromCommandLine;
            LoadFileFromCommandLine();
        }

        private void LoadFileFromCommandLine()
        {
            //Logger.Info($"{nameof(ICustomFunctionsView)} loaded");
            string filepath;
            if (_commandLineHandler.TryGetCustomFunctionsDocument(out filepath))
            {
                Logger.Info($"Got custom functions document from command line '{filepath}'");
                _view.CustomFunctionsEditor.NewDocument(filepath);
                _sharedViewState.CurrentView = ViewName.CustomFunctions;
            }
        }
    }
}