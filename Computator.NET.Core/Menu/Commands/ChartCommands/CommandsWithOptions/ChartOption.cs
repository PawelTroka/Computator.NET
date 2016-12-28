using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.DataTypes;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Menus.Commands.ChartCommands.CommandsWithOptions
{
    internal abstract class ChartOption : BaseCommandForCharts
    {
        protected ChartOption(object value, ReadOnlyDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState) : base(charts,sharedViewState)
        {
            IsOption = true;
            CheckOnClick = true;
            Text = value.ToString();
            ToolTip = value.ToString();
        }
    }
}