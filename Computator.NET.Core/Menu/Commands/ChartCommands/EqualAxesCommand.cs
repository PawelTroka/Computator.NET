using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Core.Menu;
using Computator.NET.DataTypes;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Menus.Commands.ChartCommands
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