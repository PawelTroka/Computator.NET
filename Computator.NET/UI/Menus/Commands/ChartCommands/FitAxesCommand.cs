using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.DataTypes;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Menus.Commands.ChartCommands
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