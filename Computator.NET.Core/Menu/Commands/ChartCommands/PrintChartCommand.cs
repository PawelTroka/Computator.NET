using Computator.NET.Core.Model;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Menu.Commands.ChartCommands
{
    public class PrintChartCommand : BaseCommandForCharts
    {
        public PrintChartCommand(IChart2D chart2d, IComplexChart complexChart, IChart3D chart3d, ISharedViewState sharedViewState) : base(chart2d,complexChart, chart3d, sharedViewState)
        {
            Icon = Resources.printToolStripButtonImage;
            Text = MenuStrings.printToolStripButton_Text;
            ToolTip = MenuStrings.printToolStripButton_Text;
            ShortcutKeyString = "Ctrl+P";
        }

        public override void Execute()
        {
            currentChart.Print();
        }
    }
}