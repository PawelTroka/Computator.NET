using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Evaluation;
using Computator.NET.Properties;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class PrintPreviewChartCommand : BaseCommandForCharts
    {
        public override void Execute()
        {
            currentChart.Print();
        }

        public PrintPreviewChartCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(charts)
        {
            this.Icon = Resources.printPreviewToolStripMenuItemImage;
            this.Text = MenuStrings.printPreviewToolStripMenuItem_Text;
            this.ToolTip = MenuStrings.printPreviewToolStripMenuItem_Text;
            //this.ShortcutKeyString = "Ctrl+P";
        }
    }
}