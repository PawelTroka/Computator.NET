using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.DataTypes;

namespace Computator.NET.UI.Menus.Commands.ChartCommands
{
    internal class FitAxesCommand : BaseCommandForCharts
    {
        public FitAxesCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(charts)
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