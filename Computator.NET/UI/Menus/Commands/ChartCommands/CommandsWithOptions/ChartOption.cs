using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.DataTypes;

namespace Computator.NET.UI.Menus.Commands.ChartCommands.CommandsWithOptions
{
    internal abstract class ChartOption : BaseCommandForCharts
    {
        protected ChartOption(object value, ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(charts)
        {
            IsOption = true;
            CheckOnClick = true;
            Text = value.ToString();
            ToolTip = value.ToString();
        }
    }
}