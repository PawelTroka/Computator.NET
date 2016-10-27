using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.DataTypes;
using Computator.NET.Properties;

namespace Computator.NET.UI.Menus.Commands.ChartCommands
{
    internal class PrintPreviewChartCommand : BaseCommandForCharts
    {
        public PrintPreviewChartCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(charts)
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