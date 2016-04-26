using System.Windows.Forms;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.DataTypes;

namespace Computator.NET.UI.Menus.Commands.ChartCommands
{
    internal class EditChartPropertiesCommand : BaseCommandForCharts
    {
        public EditChartPropertiesCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(charts)
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