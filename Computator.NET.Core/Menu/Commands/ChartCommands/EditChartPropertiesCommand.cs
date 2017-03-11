using System.Windows.Forms;
using Computator.NET.Charting;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Menu.Commands.ChartCommands
{
    public class EditChartPropertiesCommand : BaseCommandForCharts
    {
        public EditChartPropertiesCommand(IChart2D chart2d, IComplexChart complexChart, IChart3D chart3d, ISharedViewState sharedViewState) : base(chart2d,complexChart, chart3d, sharedViewState)
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