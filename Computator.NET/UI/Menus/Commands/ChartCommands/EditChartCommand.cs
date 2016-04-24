using System.Windows.Forms;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Charting.Chart3D;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.Evaluation;
using Computator.NET.UI.Menus;
using EditChartWindow = Computator.NET.Charting.RealCharting.EditChartWindow;

namespace Computator.NET.UI.Commands
{
    class EditChartCommand : BaseCommandForCharts
    {


        public override void Execute()
        {
            var editChartWindow = new Form();
            if (currentChart is Chart2D)
                editChartWindow = new EditChartWindow(chart2d);
            else if (currentChart is ComplexChart)
                editChartWindow = new EditComplexChartWindow(complexChart);
            else if (currentChart is Chart3DControl)
                editChartWindow = new Charting.Chart3D.EditChartWindow(chart3d, chart3d.ParentControl);


            editChartWindow.ShowDialog();
        }

        public EditChartCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(charts)
        {
            this.Text = MenuStrings.edit_Text;
            this.ToolTip = MenuStrings.edit_Text;
        }
    }
}