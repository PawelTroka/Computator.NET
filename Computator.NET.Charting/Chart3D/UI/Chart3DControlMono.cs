using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
using Computator.NET.DataTypes.Functions;
using Computator.NET.Localization;

namespace Computator.NET.Charting.Chart3D.UI
{
    // This adapter keeps the shared presenters and chart factory composable.
    // It deliberately performs no 3D computation or image export: WPF is absent.
    public class
#if __MonoCS__
        Chart3DControl
#else
        Chart3DControlMono
#endif
        : Control, IChart3D
    {
        public
#if __MonoCS__
            Chart3DControl()
#else
            Chart3DControlMono()
#endif
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;
            ForeColor = Color.Black;
            Text = Strings.Chart3D_is_not_supported_on_Mono;
            AccessibleName = Text;
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var bounds = Rectangle.Inflate(ClientRectangle, -16, -16);
            if (bounds.Width > 0 && bounds.Height > 0)
                TextRenderer.DrawText(e.Graphics, Text, Font, bounds, ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }

        private static PlatformNotSupportedException Unsupported() =>
            new PlatformNotSupportedException(Strings.Chart3D_is_not_supported_on_Mono);

        public void Print() => throw Unsupported();
        public void PrintPreview() => throw Unsupported();
        public void ShowEditDialog() => throw Unsupported();
        public void AddFunction(Function function) => throw Unsupported();
        public void SaveImage(string path, ImageFormat imageFormat) => throw Unsupported();
        public Image GetImage(int width, int height) => throw Unsupported();
        public void ShowEditPropertiesDialog() => throw Unsupported();
        public void ShowPlotDialog() => throw Unsupported();
        public void AddPoints(IEnumerable<Point3D> points) => throw Unsupported();
        public void AddPoints(List<Point3D> points) => throw Unsupported();

        // Shared area, quality and clear actions reach every registered chart.
        // Accept them so selecting or clearing a supported chart keeps working.
        public double XMin { get; set; }
        public double XMax { get; set; }
        public double YMin { get; set; }
        public double YMax { get; set; }
        public double Quality { get; set; }
        public bool EqualAxes { get; set; }
        public Chart3DMode Mode { get; set; }
        public event EventHandler XMinChanged { add { } remove { } }
        public event EventHandler XMaxChanged { add { } remove { } }
        public event EventHandler YMinChanged { add { } remove { } }
        public event EventHandler YMaxChanged { add { } remove { } }
        public event PropertyChangedEventHandler PropertyChanged { add { } remove { } }

        public void SetChartAreaValues(double x0, double xn, double y0, double yn)
        {
            XMin = x0;
            XMax = xn;
            YMin = y0;
            YMax = yn;
        }

        public void Redraw() => Invalidate();
        public void ClearAll() => Invalidate();
    }
}
