using System.Collections.Generic;
using Accord.Collections;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Menu.Commands.ChartCommands
{
    public class EditChartCommand : BaseCommandForCharts
    {
        public EditChartCommand(IDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState) : base(charts,sharedViewState)
        {
            Text = MenuStrings.edit_Text;
            ToolTip = MenuStrings.edit_Text;
        }


        public override void Execute()
        {
            currentChart.ShowEditDialog();
        }
    }
}