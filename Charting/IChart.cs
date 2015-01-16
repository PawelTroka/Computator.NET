using System;
using System.Drawing;
using System.Windows.Forms;

namespace Computator.NET.Charting
{
    internal interface IChart<T>
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
        double XMax { get; set; }
        double XMin { get; set; }
        string yLabel { get; set; }
        double YMax { get; set; }
        double YMin { get; set; }
        double Quality { set; }
        double XYRatio { get; }
        void addFx(Func<T, T> fx, string name);
        void ClearAll();
        void saveImage();
        void setChartAreaValues(double x0, double xn, double y0, double yn);
        void setupComboBoxes(params ToolStripComboBox[] owners);
        // void zoomIn();
        // void zoomOut();
    }
}