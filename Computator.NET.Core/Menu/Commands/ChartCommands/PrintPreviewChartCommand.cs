using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Core.Menu;
using Computator.NET.DataTypes;
using Computator.NET.Properties;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Menus.Commands.ChartCommands
{
    public class PrintPreviewChartCommand : BaseCommandForCharts
    {
        public PrintPreviewChartCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState) : base(charts,sharedViewState)
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