using System.Windows.Forms;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Core.UI.Menus;
using Computator.NET.DataTypes;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Menus.Commands.ChartCommands
{
    public class EditChartPropertiesCommand : BaseCommandForCharts
    {
        public EditChartPropertiesCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState) : base(charts,sharedViewState)
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