using System.Windows.Forms;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.Core.Menu;
using Computator.NET.DataTypes;
using Computator.NET.UI.Models;
using EditChartWindow = Computator.NET.Charting.RealCharting.EditChartWindow;

namespace Computator.NET.UI.Menus.Commands.ChartCommands
{
    public class EditChartCommand : BaseCommandForCharts
    {
        public EditChartCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState) : base(charts,sharedViewState)
        {
            Text = MenuStrings.edit_Text;
            ToolTip = MenuStrings.edit_Text;
        }


        public override void Execute()
        {
            var editChartWindow = new Form();
            if (currentChart is Chart2D)
                editChartWindow = new EditChartWindow(chart2d);
            else if (currentChart is ComplexChart)
                editChartWindow = new EditComplexChartWindow(complexChart);
            else if (currentChart is Chart3DControl)
                editChartWindow = new Charting.Chart3D.UI.EditChartWindow(chart3d, chart3d.ParentControl);


            editChartWindow.ShowDialog();
        }
    }
}