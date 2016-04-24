using Computator.NET.DataTypes;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class ChartCommand : DummyCommand
    {
        public ChartCommand() : base(MenuStrings.chartToolStripMenuItem_Text)
        {
            BindingUtils.OnPropertyChanged(SharedViewState.Instance, nameof(SharedViewState.Instance.CurrentView),
                () => this.IsEnabled = SharedViewState.Instance.CurrentView==ViewName.Charting);


        }
    }
}