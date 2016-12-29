using Accord.Collections;
using Computator.NET.Core.Helpers;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Menu.Commands.ChartCommands
{
    public class FitAxesCommand : BaseCommandForCharts
    {
        public FitAxesCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState) : base(charts,sharedViewState)
        {
            Text = MenuStrings.fitAxes_Text;
            ToolTip = MenuStrings.fitAxes_Text;
            CheckOnClick = true;
            Checked = !chart3d.EqualAxes;
            BindingUtils.OnPropertyChanged(chart3d, nameof(chart3d.EqualAxes), () => Checked = !chart3d.EqualAxes);
        }

        public override void Execute()
        {
            Checked = !Checked;
            chart3d.EqualAxes = !Checked;
        }
    }
}