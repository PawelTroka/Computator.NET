using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.DataTypes;
using Computator.NET.Properties;

namespace Computator.NET.UI.Menus.Commands.ChartCommands
{
    internal class PrintChartCommand : BaseCommandForCharts
    {
        public PrintChartCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(charts)
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