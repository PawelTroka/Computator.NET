using Computator.NET.DataTypes.Events;

namespace Computator.NET.UI.Menus.Commands.DummyCommands
{
    internal class ChartCommand : DummyCommand
    {
        private ISharedViewState _sharedViewState;
        public ChartCommand(ISharedViewState sharedViewState) : base(MenuStrings.chartToolStripMenuItem_Text)
        {
            _sharedViewState = sharedViewState;
            BindingUtils.OnPropertyChanged(_sharedViewState, nameof(_sharedViewState.CurrentView),
                () => IsEnabled = _sharedViewState.CurrentView == ViewName.Charting);
        }
    }

    internal class TransformCommand : DummyCommand
    {
        private ISharedViewState _sharedViewState;
        public TransformCommand(ISharedViewState sharedViewState) : base(MenuStrings.transformToolStripMenuItem_Text)
        {
            _sharedViewState = sharedViewState;
            BindingUtils.OnPropertyChanged(_sharedViewState, nameof(_sharedViewState.CurrentView),
                () => IsEnabled = _sharedViewState.CurrentView == ViewName.Charting);
        }
    }
}