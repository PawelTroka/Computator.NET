using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Computator.NET.DataTypes;

namespace Computator.NET.Charting
{

    public interface IChart
    {
        void Redraw();
        double XMax { get; set; }
        double XMin { get; set; }
        double YMax { get; set; }
        double YMin { get; set; }
        void addFx(Function function);

        void Print();
        void PrintPreview();
        void SetChartAreaValues(double x0, double xn, double y0, double yn);

        void SaveImage(string path, ImageFormat imageFormat);
    }
  /*  internal interface IChart<T>
    {
        Color axesColor { get; set; }
        bool AxesEqual { get; set; }
        Color labelsColor { get; set; }
        Font labelsFont { get; set; }
        bool shouldDrawAxes { get; set; }
        string title { get; set; }
        Color titleColor { get; set; }
        Font titleFont { get; set; }
        string xLabel { get; set; }

        string yLabel { get; set; }

        double Quality { set; }
        double XYRatio { get; }
        void addFx(Func<T, T> fx, string name);
        void ClearAll();
        void saveImage();
        void SetChartAreaValues(double x0, double xn, double y0, double yn);
        void setupComboBoxes(params ToolStripComboBox[] owners);
    }*/
}