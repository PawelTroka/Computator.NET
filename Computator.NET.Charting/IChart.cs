using System.Drawing.Imaging;
using Computator.NET.DataTypes;
using Computator.NET.UI.Controls;

namespace Computator.NET.Charting
{
    public interface IChart : IPrinting, IAreaValues
    {
        double Quality { get; set; }

        bool Visible { get; set; }
        void Redraw();

        void AddFunction(Function function);

        void SetChartAreaValues(double x0, double xn, double y0, double yn);

        void SaveImage(string path, ImageFormat imageFormat);

        void ClearAll();
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