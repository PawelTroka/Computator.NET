using System.Collections.Generic;
using System.Windows.Forms;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Menu.Commands.ChartCommands
{
    public class EditChartPropertiesCommand : BaseCommandForCharts
    {
        public EditChartPropertiesCommand(IDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState) : base(charts,sharedViewState)
        {
            Text = MenuStrings.editProperties_Text;
            ToolTip = MenuStrings.editProperties_Text;
        }


        public override void Execute()
        {
            var editChartProperties = new EditChartProperties(currentChart);
            if (editChartProperties.ShowDialog() == DialogResult.OK)
            {
                currentChart.Redraw();
            }
        }
    }
}