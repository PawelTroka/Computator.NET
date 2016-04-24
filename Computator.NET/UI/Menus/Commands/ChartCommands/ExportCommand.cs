using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Evaluation;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class ExportCommand : BaseCommandForCharts
    {

        private readonly SaveFileDialog saveChartImageFileDialog = new SaveFileDialog
        {
            Filter = Strings.GUI_exportChart3dToolStripMenuItem_Click_Image_FIlter,
            RestoreDirectory = true,
            DefaultExt = "png",
            AddExtension = true
        };


        private static ImageFormat FilterIndexToImageFormat(int filterIndex)
        {
            ImageFormat format;

            switch (filterIndex)
            {
                case 1:
                    format = ImageFormat.Png;
                    break;
                case 2:
                    format = ImageFormat.Gif;
                    break;
                case 3:
                    format = ImageFormat.Jpeg;
                    break;
                case 4:
                    format = ImageFormat.Bmp;
                    break;
                case 5:
                    format = ImageFormat.Tiff;
                    break;
                case 6:
                    format = ImageFormat.Wmf;
                    break;
                default:
                    format = ImageFormat.Png;
                    break;
            }
            return format;
        }
        public override void Execute()
        {
            saveChartImageFileDialog.FileName =
                $"{Strings.Chart} {DateTime.Now.ToString("u", CultureInfo.InvariantCulture).Replace(':', '-').Replace("Z", "")}";
            if (saveChartImageFileDialog.ShowDialog() == DialogResult.OK)
            {
                Thread.Sleep(20);
                currentChart.SaveImage(saveChartImageFileDialog.FileName,
                    FilterIndexToImageFormat(saveChartImageFileDialog.FilterIndex));
            }
            
        }

        public ExportCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(charts)
        {
            this.Text = MenuStrings.export_Text;
            this.ToolTip = MenuStrings.export_Text;
        }
    }
}