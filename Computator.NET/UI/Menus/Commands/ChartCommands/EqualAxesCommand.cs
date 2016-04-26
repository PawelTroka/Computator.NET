using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.DataTypes;

namespace Computator.NET.UI.Menus.Commands.ChartCommands
{
    internal class EqualAxesCommand : BaseCommandForCharts
    {
        public EqualAxesCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(charts)
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