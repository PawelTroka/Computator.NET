using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Charting;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.Charts;
using Computator.NET.Localization;
using Computator.NET.Localization.Menu;

namespace Computator.NET.Core.Menu.Commands.ChartCommands
{
    public class ExportCommand : BaseCommandForCharts
    {
        private readonly SaveFileDialog saveChartImageFileDialog = new SaveFileDialog
        {
            Filter = Strings.Image_Filter,
            RestoreDirectory = true,
            DefaultExt = "png",
            AddExtension = true
        };

        public ExportCommand(IChart2D chart2d, IComplexChart complexChart, IChart3D chart3d, ISharedViewState sharedViewState) : base(chart2d,complexChart, chart3d, sharedViewState)
        {
            Text = MenuStrings.export_Text;
            ToolTip = MenuStrings.export_Text;
        }



        public override void Execute()
        {
            var chart = currentChart;
            ChartImageExport.ConfigureDialog(saveChartImageFileDialog, chart);
            saveChartImageFileDialog.FileName =
                $"{Strings.Chart} {DateTime.Now.ToString("u", CultureInfo.InvariantCulture).Replace(':', '-').Replace("Z", "")}";
            if (saveChartImageFileDialog.ShowDialog() == DialogResult.OK)
            {
                Thread.Sleep(20);
                chart.SaveImage(saveChartImageFileDialog.FileName,
                    ChartImageExport.FormatFromFilterIndex(chart, saveChartImageFileDialog.FilterIndex));
            }
        }
    }
}
