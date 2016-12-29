using Accord.Collections;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Menu.Commands.ChartCommands.CommandsWithOptions
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