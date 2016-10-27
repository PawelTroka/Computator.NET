using Computator.NET.DataTypes.Events;

namespace Computator.NET.UI.Menus.Commands.DummyCommands
{
    internal class ChartCommand : DummyCommand
    {
        public ChartCommand() : base(MenuStrings.chartToolStripMenuItem_Text)
        {
            BindingUtils.OnPropertyChanged(SharedViewState.Instance, nameof(SharedViewState.Instance.CurrentView),
                () => IsEnabled = SharedViewState.Instance.CurrentView == ViewName.Charting);
        }
    }

    internal class TransformCommand : DummyCommand
    {
        public TransformCommand() : base(MenuStrings.transformToolStripMenuItem_Text)
        {
            BindingUtils.OnPropertyChanged(SharedViewState.Instance, nameof(SharedViewState.Instance.CurrentView),
                () => IsEnabled = SharedViewState.Instance.CurrentView == ViewName.Charting);
        }
    }
}