using Accord.Collections;
using Computator.NET.Core.Helpers;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Menu.Commands.ChartCommands
{
    public class EqualAxesCommand : BaseCommandForCharts
    {
        public EqualAxesCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState) : base(charts,sharedViewState)
        {
            Text = MenuStrings.equalAxes_Text;
            ToolTip = MenuStrings.equalAxes_Text;
            CheckOnClick = true;
            Checked = chart3d.EqualAxes;
            BindingUtils.TwoWayBinding(this, nameof(Checked), chart3d, nameof(chart3d.EqualAxes));
        }

        public override void Execute()
        {
            Checked = !Checked;
        }
    }
}