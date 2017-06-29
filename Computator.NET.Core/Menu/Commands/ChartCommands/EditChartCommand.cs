using Computator.NET.Core.Model;
using Computator.NET.DataTypes.Charts;
using Computator.NET.Localization.Menu;

namespace Computator.NET.Core.Menu.Commands.ChartCommands
{
    public class EditChartCommand : BaseCommandForCharts
    {
        public EditChartCommand(IChart2D chart2d, IComplexChart complexChart, IChart3D chart3d, ISharedViewState sharedViewState) : base(chart2d,complexChart, chart3d, sharedViewState)
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