using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms.DataVisualization.Charting;
using Computator.NET.Charting.ComplexCharting;

namespace Computator.NET.DataTypes.Charts
{
    public interface IChartFactory
    {
        IChart2D CreateChart2D();
        IComplexChart CreateComplexChart();
        IChart3D CreateChart3D();
        IChart Create(CalculationsMode calculationsMode);
        IChart Create(FunctionType functionType);
    }

    public interface IChart2D : IChart
    {
        StringAlignment LegendAlignment { get; set; }
        ChartColorPalette Palette { get; set; }
        SeriesChartType ChartType { get; set; }
        Docking LegendDocking { get; set; }
        void Transform(Func<double[], double[]> func, string text);
    }

    public interface IComplexChart : IChart
    {
        AssignmentOfColorMethod ColorAssignmentMethod { get; set; }
        CountourLinesMode CountourMode { get; set; }
    }

    public interface IChart3D : IChart
    {
        bool EqualAxes { get; set; }
    }

    public interface IChart : IPrinting, IAreaValues, INotifyPropertyChanged
    {
        void ShowEditDialog();
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