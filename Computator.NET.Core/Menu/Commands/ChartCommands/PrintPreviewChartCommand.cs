using Computator.NET.Core.Model;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes.Charts;
using Computator.NET.Localization.Menu;

namespace Computator.NET.Core.Menu.Commands.ChartCommands
{
    public class PrintPreviewChartCommand : BaseCommandForCharts
    {
        public PrintPreviewChartCommand(IChart2D chart2d, IComplexChart complexChart, IChart3D chart3d, ISharedViewState sharedViewState) : base(chart2d,complexChart, chart3d, sharedViewState)
        {
            Icon = Resources.printPreviewToolStripMenuItemImage;
            Text = MenuStrings.printPreviewToolStripMenuItem_Text;
            ToolTip = MenuStrings.printPreviewToolStripMenuItem_Text;
            //this.ShortcutKeyString = "Ctrl+P";
        }

        public override void Execute()
        {
            currentChart.Print();
        }
    }
}