using System.Windows.Forms;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Evaluation;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class EditChartPropertiesCommand : BaseCommandForCharts
    {


        public override void Execute()
        {
            var editChartProperties = new EditChartProperties(currentChart);
            if (editChartProperties.ShowDialog() == DialogResult.OK)
            {
                currentChart.Redraw();
            }
        }

        public EditChartPropertiesCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(charts)
        {
            this.Text = MenuStrings.editProperties_Text;
            this.ToolTip = MenuStrings.editProperties_Text;
        }
    }
}