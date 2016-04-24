using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Evaluation;
using Computator.NET.Properties;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class PrintChartCommand : BaseCommandForCharts
    {
        public override void Execute()
        {
            currentChart.Print();
        }

        public PrintChartCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(charts)
        {
            this.Icon = Resources.printToolStripButtonImage;
            this.Text = MenuStrings.printToolStripButton_Text;
            this.ToolTip = MenuStrings.printToolStripButton_Text;
            this.ShortcutKeyString = "Ctrl+P";
        }
    }
}