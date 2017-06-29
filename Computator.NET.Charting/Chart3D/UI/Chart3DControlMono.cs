
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
using Computator.NET.Localization;

namespace Computator.NET.Charting.Chart3D.UI
{
    public class
#if __MonoCS__
        Chart3DControl
#else
        Chart3DControlMono
#endif
        : Control, IChart3D
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawString(Strings.Chart3D_is_not_supported_on_Mono, new Font(FontFamily.GenericSansSerif, 30),Brushes.Black, this.Width/2,this.Height/2 );
        }

        public void Print()
        {
        }

        public void PrintPreview()
        {
        }

        public double XMin { get; set; }
        public double XMax { get; set; }
        public double YMin { get; set; }
        public double YMax { get; set; }
        public event EventHandler XMinChanged;
        public event EventHandler XMaxChanged;
        public event EventHandler YMinChanged;
        public event EventHandler YMaxChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public void ShowEditDialog()
        {
        }

        public double Quality { get; set; }
        public void Redraw()
        {
        }

        public void AddFunction(Function function)
        {
        }

        public void SetChartAreaValues(double x0, double xn, double y0, double yn)
        {
        }

        public void SaveImage(string path, ImageFormat imageFormat)
        {
        }

        public void ClearAll()
        {
        }

        public bool EqualAxes { get; set; }
        public Chart3DMode Mode { get; set; }

        public void AddPoints(List<Point3D> points)
        {
            throw new NotImplementedException();
        }
    }
}