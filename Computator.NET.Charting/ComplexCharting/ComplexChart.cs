using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Computator.NET.Charting.Printing;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
using Computator.NET.DataTypes.Functions;
using Computator.NET.DataTypes.Text;
using NLog;

namespace Computator.NET.Charting.ComplexCharting
{
    public sealed class ComplexChart : Control, IComplexChart
    {

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region private fields

        // private Graphics g;
        private double quality;
        //  private Task RedrawTask;
        private  Color[,] _pointsColors;

        private  ComplexPoint[,] _pointsValues;
        private bool _redrawing = false;
        private Function function;
        private Bitmap image = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

        private readonly ToolTip toolTip = new ToolTip
        {
            AutoPopDelay = 5000,
            InitialDelay = 1000,
            ReshowDelay = 500
        };


        #endregion

        #region public properties

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public double CountourLinesStep { get; set; } = Math.E;

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public CountourLinesMode CountourMode
        {
            get { return _countourMode; }
            set
            {
                if (_countourMode != value)
                {
                    _countourMode = value;
                    OnPropertyChanged(nameof(CountourMode));
                }
            }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public AssignmentOfColorMethod ColorAssignmentMethod
        {
            get { return _colorAssignmentMethod; }
            set
            {
                if (_colorAssignmentMethod != value)
                {
                    _colorAssignmentMethod = value;
                    OnPropertyChanged(nameof(ColorAssignmentMethod));
                }
            }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public double AxisArrowRelativeSize { get; set; } = 0.02;

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public double Quality
        {
            set
            {
                if (value > 100)
                    value = 100;
                if (value < 0)
                    value = 0;


                CalculateQuality(value);
                Redraw();
            }
            get { return quality*100; }
        }

        public double XYRatio
        {
            get { return (XMax - XMin)/Width/((YMax - YMin)/Height); }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public double XMin
        {
            get { return _xMin; }
            set
            {
                if (value != _xMin)
                {
                    _xMin = value;
                    XMinChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public double XMax
        {
            get { return _xMax; }
            set
            {
                if (value != _xMax)
                {
                    _xMax = value;
                    XMaxChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public double YMin
        {
            get { return _yMin; }
            set
            {
                if (value != _yMin)
                {
                    _yMin = value;
                    YMinChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public double YMax
        {
            get { return _yMax; }
            set
            {
                if (value != _yMax)
                {
                    _yMax = value;
                    YMaxChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        public event EventHandler XMinChanged;
        public event EventHandler XMaxChanged;
        public event EventHandler YMinChanged;
        public event EventHandler YMaxChanged;

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public string XLabel { get; set; } = "Re(z)";
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public string YLabel { get; set; } = "Im(z)";
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public string Title { get; set; }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Font TitleFont { get; set; } = CustomFonts.GetMathFont(13);
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Font LabelsFont { get; set; } = CustomFonts.GetMathFont(13);
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color LabelsColor { get; set; } = Color.Black;
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color TitleColor { get; set; } = Color.Black;
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color AxesColor { get; set; } = Color.Black;

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public bool ShouldDrawAxes { get; set; } = true;

        private void CalculateQuality(double value)
        {
            if (value >= 50)
                quality = 1.0;
            else
                quality = value/50;

            if (quality < 0.1)
                quality = 0.1;
        }

        #endregion

        #region constructing

        private Task<Bitmap> CalculateValuesAndColorsAsync()
        {
            return Task.Factory.StartNew(CalculateValuesAndColors);
        }

        private Bitmap CalculateValuesAndColors()
        {
            return CalculateValuesAndColors(DrawWidth, DrawHeight, true);
        }

        private Bitmap CalculateValuesAndColors(int width, int height, bool updatePoints)
        {
            var pointsValues = new ComplexPoint[width, height];
            var pointsColors = new Color[width, height];


            Parallel.For(0, pointsValues.GetLength(0), x =>
            {
                var re = XMin + x * (XMax - XMin) / pointsValues.GetLength(0);
                for (var y = 0; y < pointsValues.GetLength(1); y++)
                {
                    var im = YMax - y * (YMax - YMin) / pointsValues.GetLength(1);

                    var z = new Complex(re, im);

                    var fz = function.Evaluate(z);
                    pointsValues[x, y] = new ComplexPoint(z, fz);

                    if (double.IsInfinity(fz.Real) || double.IsNaN(fz.Real) || double.IsInfinity(fz.Imaginary) ||
                        double.IsNaN(fz.Imaginary))
                    {
                        pointsColors[x, y] = Color.White;
                    }
                    else
                    {
                        if (function.IsImplicit)
                        {
                            pointsColors[x, y] = ComplexToColor(fz);
                        }
                        else
                            pointsColors[x, y] = ComplexToColor(fz);
                        //image.SetPixel(x, y, ComplexToColor(fz)/*pointsColors[x, y]*/);
                    }

                }
            });

            var bitmap = new Bitmap(pointsColors.GetLength(0), pointsColors.GetLength(1));

            for (int x = 0; x < bitmap.Width; x++)
                for (int y = 0; y < bitmap.Height; y++)
                    bitmap.SetPixel(x, y, pointsColors[x, y]);
            if (updatePoints)
            {
                _pointsValues = pointsValues;
                _pointsColors = pointsColors;
            }
            return bitmap;
        }

        public ComplexChart()
        {
            InitializeComponent();
      
            DoubleBuffered = true;
            attachEventHandlers();
            Quality = 50;       
        }


        

        private void attachEventHandlers()
        {
            MouseClick += _MouseClick;
            Resize += _Resize;
        }

        private void InitializeComponent()
        {
            Name = "complexChart";
            BackColor = Color.White;
            //doNotRecalculate = false;
            Dock = DockStyle.Fill;
            
        }

        #endregion

        #region event handlers

        private void _Resize(object s, EventArgs e)
        {
            if (Width > 0 && Height > 0)
                Redraw();
            //else
            //doNotRecalculate = true;
        }

        private void _MouseClick(object s, MouseEventArgs e)
        {
            var x = (int)(e.Location.X * quality);
            var y = (int)(e.Location.Y * quality);
            if (_pointsValues!=null && x< _pointsValues.GetLength(0) && y<_pointsValues.GetLength(1))
            {
                var fz = _pointsValues[x, y].Fz;
                var z = _pointsValues[x, y].Z;

                toolTip.SetToolTip(this,
                    $"f(z) = {fz.Real:0.###}{fz.Imaginary:+0.###;-0.###}i = {fz.Magnitude:0.###} exp({fz.Phase:0.###})\nz = {z.Real:0.###}{z.Imaginary:+0.###;-#0.###}i = {z.Magnitude:0.###} exp({z.Phase:0.###})");
                toolTip.ShowAlways = true;
            }
        }

        #endregion

        #region public methods

        public void AddFunction(Function Fz)
        {
            function = Fz;
            Title = Fz.Name;
            Redraw();
        }

        private readonly ImagePrinter imagePrinter = new ImagePrinter();
        private double _xMin;
        private double _xMax;
        private double _yMin;
        private double _yMax;
        private CountourLinesMode _countourMode = CountourLinesMode.Logarithmic;
        private AssignmentOfColorMethod _colorAssignmentMethod = AssignmentOfColorMethod.GreaterIsDarker;

        public void Print()
        {
            imagePrinter.Print(image);
        }

        public void PrintPreview()
        {
            imagePrinter.PrintPreview(image);
        }

        public void SetChartAreaValues(double x0, double xn, double y0, double yn)
        {
            XMin = x0;
            XMax = xn;
            YMin = y0; //min;
            YMax = yn; //max;
        }

        public void SaveImage(string path, ImageFormat imageFormat)
        {
            if (image == null)
                return;

            var size = ClientSize.Width > 0 && ClientSize.Height > 0 ? ClientSize : image.Size;
            using (var snapshot = CreateImage(image, size))
            {
                snapshot.Save(path, imageFormat);
            }
        }

        public Image GetImage(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            var size = new Size(width, height);
            if (function == null)
                return CreateImage(image, size);

            // Export sampling must not resize the control or replace the point grid
            // used by its on-screen mouse tooltips.
            using (var sampled = CalculateValuesAndColors(Math.Max(1, (int)(width * quality)),
                Math.Max(1, (int)(height * quality)), false))
            {
                return CreateImage(sampled, size);
            }
        }

        private Bitmap CreateImage(Image source, Size size)
        {
            var bitmap = new Bitmap(size.Width, size.Height);
            try
            {
                bitmap.SetResolution(DeviceDpi, DeviceDpi);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(BackColor);
                    PaintOn(graphics, new Rectangle(Point.Empty, size), source);
                }
                return bitmap;
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
        }

        #endregion

        #region drawing

        private int DrawWidth => (int) (ClientRectangle.Width * quality);

        private int DrawHeight => (int) (ClientRectangle.Height * quality);

        public void ClearAll()
        {
            function = null;
            image = new Bitmap(DrawWidth, DrawHeight);
            Invalidate();
        }

        public void ShowEditPropertiesDialog()
        {
            var editChartProperties = new EditChartProperties(this);
            if (editChartProperties.ShowDialog() == DialogResult.OK)
            {
                this.Redraw();
            }
        }

        public void ShowPlotDialog()
        {
            var plotForm = new PlotForm(this);
            plotForm.Show();
        }

        public async void Redraw()
        {
            if (function == null || DrawWidth <= 0 || DrawHeight <= 0 || _redrawing)
                return;
            _redrawing = true;
            Cursor = Cursors.WaitCursor;
            image = await CalculateValuesAndColorsAsync();
            Cursor = Cursors.Default;
            Invalidate();
            _redrawing = false;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            PaintOn(pe.Graphics, ClientRectangle, image);
        }

        private void PaintOn(Graphics graphics, Rectangle bounds, Image source)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;
            if (source != null)
                graphics.DrawImage(source, bounds);
            if (ShouldDrawAxes)
                DrawAxes(graphics, bounds);
        }

        private void DrawAxes(Graphics graphics, Rectangle bounds)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;
            var layout = ComplexChartAxesLayout.Create(bounds.Size, XMin, XMax, YMin, YMax,
                Math.Max(graphics.DpiX, graphics.DpiY), AxisArrowRelativeSize);
            var state = graphics.Save();
            try
            {
                graphics.PageUnit = GraphicsUnit.Pixel;
                graphics.PageScale = 1;
                graphics.SetClip(bounds, CombineMode.Intersect);
                graphics.TranslateTransform(bounds.X, bounds.Y);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                var baseFont = LabelsFont ?? Font;
                // Physical font units already receive DPI from GDI+. Pixel fonts
                // need the full scale; applying DPI twice makes labels too large.
                var fontScale = baseFont.Unit == GraphicsUnit.Pixel
                    ? layout.Scale : layout.Scale * 96f / graphics.DpiY;
                using (var pen = new Pen(AxesColor, layout.StrokeWidth))
                using (var axesBrush = new SolidBrush(AxesColor))
                using (var labelsBrush = new SolidBrush(LabelsColor))
                using (var font = new Font(baseFont.FontFamily, baseFont.Size * fontScale,
                    baseFont.Style, baseFont.Unit, baseFont.GdiCharSet, baseFont.GdiVerticalFont))
                using (var format = new StringFormat(StringFormatFlags.NoWrap)
                {
                    Trimming = StringTrimming.EllipsisCharacter
                })
                {
                    graphics.DrawLine(pen, layout.XStart, layout.XEnd);
                    graphics.DrawLine(pen, layout.YStart, layout.YEnd);
                    if (layout.ArrowLength > 0)
                    {
                        graphics.FillPolygon(axesBrush, layout.XArrow);
                        graphics.FillPolygon(axesBrush, layout.YArrow);
                    }

                    var xLabel = XLabel ?? string.Empty;
                    var yLabel = YLabel ?? string.Empty;
                    var xSize = graphics.MeasureString(xLabel, font, int.MaxValue, format);
                    var ySize = graphics.MeasureString(yLabel, font, int.MaxValue, format);
                    layout.GetLabelBounds(xSize, ySize, out var xBounds, out var yBounds);
                    if (xBounds.Width > 0 && xBounds.Height > 0)
                        graphics.DrawString(xLabel, font, labelsBrush, xBounds, format);
                    if (yBounds.Width > 0 && yBounds.Height > 0)
                        graphics.DrawString(yLabel, font, labelsBrush, yBounds, format);
                }
            }
            finally
            {
                graphics.Restore(state);
            }
        }

        #endregion

        #region calculations
        private Color ComplexToColor(Complex z)
        {
            double m = z.Magnitude, t = z.Phase;

            while (t < 0.0) t += 2*Math.PI;
            while (t >= 2*Math.PI) t -= 2*Math.PI;

            double h = t/(2*Math.PI), s = 0, v = 0;
            double r0 = 0, r1 = 1;

            switch (CountourMode)
            {
                case CountourLinesMode.Logarithmic:
                    //Based on Claudio Rocchini C++ algorithm for complex functions domain coloring http://en.wikipedia.org/wiki/File:Color_complex_plot.jpg
                    while (m > r1)
                    {
                        r0 = r1;
                        r1 = r1*CountourLinesStep;
                    }
                    break;

                case CountourLinesMode.Linear:
                    r1 = CountourLinesStep;
                    while (m > r1)
                    {
                        r0 = r1;
                        r1 = r1 + CountourLinesStep;
                    }
                    break;

                case CountourLinesMode.Exponential:
                    r1 = Math.Log(Math.Pow(m, 1/m), CountourLinesStep);
                    while (m > r1)
                    {
                        r0 = r1;
                        r1 = r1 + Math.Log(Math.Pow(m, 1/m), CountourLinesStep);
                    }
                    break;
            }

            if (CountourMode != CountourLinesMode.NoCountourLines)
            {
                //Based on Claudio Rocchini C++ algorithm for complex functions domain coloring http://en.wikipedia.org/wiki/File:Color_complex_plot.jpg
                var r = (m - r0)/(r1 - r0);
                var p = r < 0.5 ? 2.0*r : 2.0*(1.0 - r);
                var q = 1.0 - p;
                var p1 = 1 - q*q*q;
                var q1 = 1 - p*p*p;
                s = 0.4 + 0.6*p1;
                v = 0.6 + 0.4*q1;
            }
            else
            {
                //if(m>1)
                var coefficient = Math.Log(m + 1, 2)/Math.Log(double.MaxValue, 2);
                //else
                //coefficient = (Math.Pow(m,1/100))/(Math.Pow(double.MaxValue,1/100));
                switch (ColorAssignmentMethod)
                {
                    case AssignmentOfColorMethod.GreaterIsDarker:
                        //s = 0.4 + 0.6*coefficient;
                        //v = 0.6 + 0.4*(1 - coefficient);

                        v = 1 - 2*Math.Atan(m)/Math.PI;
                        s = v <= 0.5 ? 2*v : 2 - 2*v;

                        break;
                    case AssignmentOfColorMethod.GreaterIsLighter:
                        s = 1 - 2*Math.Atan(m)/Math.PI;
                        v = s <= 0.5 ? 2*s : 2 - 2*s;
                        break;
                }
            }

            return ColorFromHSV(h, s, v);
        }

        #endregion

        #region helpers

        public void setupComboBoxes(params ToolStripComboBox[] comboBoxes)
        {
            comboBoxes[0].AutoSize = true;
            comboBoxes[1].AutoSize = true;
            var vartosci =
                Enum.GetValues(typeof(CountourLinesMode)).Cast<CountourLinesMode>().ToList();
            foreach (var v in vartosci)
                comboBoxes[0].Items.Add(v);
            comboBoxes[0].SelectedItem = CountourMode;
            comboBoxes[0].DropDownStyle = ComboBoxStyle.DropDownList;

            var vartosci2 =
                Enum.GetValues(typeof(AssignmentOfColorMethod))
                    .Cast<AssignmentOfColorMethod>()
                    .ToList();
            foreach (var v in vartosci2)
                comboBoxes[1].Items.Add(v);
            comboBoxes[1].SelectedItem = ColorAssignmentMethod;
            comboBoxes[1].DropDownStyle = ComboBoxStyle.DropDownList;
        }

        //   public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName = "")
        {
            // if (PropertyChanged != null)
            //  {
            //      PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            //   }
        }

        private static Color ColorFromHSV(double hue, double saturation, double value)
        {
            var h = hue;
            var s = saturation;
            var v = value;

            double r, g, b;
            if (s == 0)
            {
                r = g = b = v;
            }
            else
            {
                if (h == 1.0) h = 0.0;
                var z = Math.Truncate(6*h);
                var i = (int) z;
                var f = h*6 - z;
                var p = v*(1 - s);
                var q = v*(1 - s*f);
                var t = v*(1 - s*(1 - f));

                switch (i)
                {
                    case 0:
                        r = v;
                        g = t;
                        b = p;
                        break;
                    case 1:
                        r = q;
                        g = v;
                        b = p;
                        break;
                    case 2:
                        r = p;
                        g = v;
                        b = t;
                        break;
                    case 3:
                        r = p;
                        g = q;
                        b = v;
                        break;
                    case 4:
                        r = t;
                        g = p;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = p;
                        b = q;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }



            var ir = (int) Math.Truncate(255.0*r);
            var ig = (int) Math.Truncate(255.0*g);
            var ib = (int) Math.Truncate(255.0*b);
            if (ir < byte.MinValue || ir > byte.MaxValue || ig < byte.MinValue || ig > byte.MaxValue ||
                ib < byte.MinValue || ib > byte.MaxValue)
            {
                //ultra rarely this can happen for coefficients r,g,b being NaNs or infinities
                //in this case we treat it as no value in this place and log error
                Logger.Warn($"Cannot create color because for (h, s, v) ({hue}, {saturation}, {value}) we get coefficients (r, g, b) being ({r}, {g}, {b}) which means that RGB is {ir} {ib} {ig}");
                return Color.White;
            }
            return Color.FromArgb(ir, ig, ib);
        }

        public void ShowEditDialog()
        {
            var editChartWindow = new EditComplexChartWindow(this);
            editChartWindow.ShowDialog();
        }

        #endregion
    }
}
