using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
        private  ComplexPoint[,] _pointsValues;
        private bool _redrawing = false;
        public string LastRenderError { get; private set; }
        private long _redrawVersion;
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

        private sealed class ChartSamples
        {
            public ChartSamples(Bitmap bitmap, ComplexPoint[,] pointsValues)
            {
                Bitmap = bitmap;
                PointsValues = pointsValues;
            }

            public Bitmap Bitmap { get; }
            public ComplexPoint[,] PointsValues { get; }
        }

        private Task<ChartSamples> CalculateValuesAndColorsAsync(int width, int height)
        {
            return Task.Factory.StartNew(() => CalculateValuesAndColors(width, height));
        }

        private ChartSamples CalculateValuesAndColors(int width, int height)
        {
            // Exports sample independently of the live control and must not replace
            // the points used by its mouse-coordinate tooltip.
            var plottedFunction = function;
            if (plottedFunction == null)
                return new ChartSamples(new Bitmap(width, height), null);

            var pointsValues = new ComplexPoint[width, height];
            var pointsColors = new Color[width, height];
            var xMin = XMin;
            var xMax = XMax;
            var yMin = YMin;
            var yMax = YMax;

            Parallel.For(0, width, x =>
            {
                var re = xMin + x * (xMax - xMin) / width;
                for (var y = 0; y < height; y++)
                {
                    var im = yMax - y * (yMax - yMin) / height;

                    var z = new Complex(re, im);

                    var fz = plottedFunction.Evaluate(z);
                    pointsValues[x, y] = new ComplexPoint(z, fz);

                    if (double.IsInfinity(fz.Real) || double.IsNaN(fz.Real) || double.IsInfinity(fz.Imaginary) ||
                        double.IsNaN(fz.Imaginary))
                    {
                        pointsColors[x, y] = Color.White;
                    }
                    else
                    {
                        pointsColors[x, y] = ComplexToColor(fz);
                    }

                }
            });

            var bitmap = new Bitmap(width, height);
            try
            {
                for (int x = 0; x < bitmap.Width; x++)
                    for (int y = 0; y < bitmap.Height; y++)
                        bitmap.SetPixel(x, y, pointsColors[x, y]);
                return new ChartSamples(bitmap, pointsValues);
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
        }

        public ComplexChart()
        {
            InitializeComponent();
      
            DoubleBuffered = true;
            attachEventHandlers();
            Quality = 50;       
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ++_redrawVersion;
                function = null;
                _pointsValues = null;
                image?.Dispose();
                image = null;
                toolTip.Dispose();
            }
            base.Dispose(disposing);
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
            if (_pointsValues == null || !ClientRectangle.Contains(e.Location))
                return;
            // The visible image can still have the previous size/quality while a
            // replacement renders. Sample its actual grid, as PaintOn scales it.
            var x = (int)((double)e.X * _pointsValues.GetLength(0) / ClientSize.Width);
            var y = (int)((double)e.Y * _pointsValues.GetLength(1) / ClientSize.Height);
            var fz = _pointsValues[x, y].Fz;
            var z = _pointsValues[x, y].Z;

            toolTip.SetToolTip(this,
                $"f(z) = {fz.Real:0.###}{fz.Imaginary:+0.###;-0.###}i = {fz.Magnitude:0.###} exp({fz.Phase:0.###})\nz = {z.Real:0.###}{z.Imaginary:+0.###;-#0.###}i = {z.Magnitude:0.###} exp({z.Phase:0.###})");
            toolTip.ShowAlways = true;
        }

        #endregion

        #region public methods

        public void AddFunction(Function Fz)
        {
            LastRenderError = null;
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
            if (image == null)
                return;

            var size = SnapshotSize;
            using (var bitmap = ComposeImage(image, size.Width, size.Height))
                imagePrinter.Print(bitmap);
        }

        public void PrintPreview()
        {
            if (image == null)
                return;

            var size = SnapshotSize;
            using (var bitmap = ComposeImage(image, size.Width, size.Height))
                imagePrinter.PrintPreview(bitmap);
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

            // Preserve full display dimensions even when the plot was sampled at
            // lower quality, and never burn the overlay into the cached colors.
            var size = SnapshotSize;
            using (var bitmap = ComposeImage(image, size.Width, size.Height))
                bitmap.Save(path, imageFormat);
        }

        private Size SnapshotSize => ClientSize.Width > 0 && ClientSize.Height > 0 ? ClientSize : image.Size;

        public Image GetImage(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            var samples = CalculateValuesAndColors(Math.Max(1, (int)(width * quality)),
                Math.Max(1, (int)(height * quality)));
            using (var sampledImage = samples.Bitmap)
                return ComposeImage(sampledImage, width, height);
        }

        private Bitmap ComposeImage(Image source, int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            try
            {
                // Offscreen output has a stable scale on every monitor and server.
                bitmap.SetResolution(96, 96);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(BackColor);
                    PaintOn(graphics, new Rectangle(0, 0, width, height), source);
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
            LastRenderError = null;
            ++_redrawVersion;
            function = null;
            _pointsValues = null;
            var previousImage = image;
            image = new Bitmap(Math.Max(1, DrawWidth), Math.Max(1, DrawHeight));
            previousImage?.Dispose();
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
            if (IsDisposed || Disposing)
                return;
            ++_redrawVersion;
            if (function == null || DrawWidth <= 0 || DrawHeight <= 0 || _redrawing)
                return;
            _redrawing = true;
            Cursor = Cursors.WaitCursor;
            try
            {
                // Changes during evaluation coalesce into one new render. Never
                // publish obsolete functions, dimensions or mouse-sampling data.
                while (function != null && !IsDisposed && DrawWidth > 0 && DrawHeight > 0)
                {
                    var version = _redrawVersion;
                    ChartSamples samples;
                    try
                    {
                        samples = await CalculateValuesAndColorsAsync(DrawWidth, DrawHeight);
                    }
                    catch (Exception) when (function == null || IsDisposed || version != _redrawVersion)
                    {
                        // Superseded evaluations should never report an error.
                        continue;
                    }
#if __MonoCS__
                    catch (Exception exception)
                    {
                        // Mono: keep async-void errors inside the chart to avoid
                        // killing the production message loop. The diagnostic is
                        // painted on screen and a later Draw can recover.
                        // Windows retains its established ThreadException route.
                        Logger.Error(exception, "Complex chart asynchronous redraw failed.");
                        LastRenderError = exception.Message;
                        Invalidate();
                        break;
                    }
#endif
                    if (function == null || IsDisposed || version != _redrawVersion)
                    {
                        samples.Bitmap.Dispose();
                        continue;
                    }
                    var previousImage = image;
                    image = samples.Bitmap;
                    _pointsValues = samples.PointsValues;
                    previousImage?.Dispose();
                    Invalidate();
                    break;
                }
            }
            finally
            {
                if (!IsDisposed)
                    Cursor = Cursors.Default;
                _redrawing = false;
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            PaintOn(pe.Graphics, ClientRectangle, image);
            if (!string.IsNullOrWhiteSpace(LastRenderError))
            {
                var bounds = Rectangle.Inflate(ClientRectangle, -12, -12);
                if (bounds.Width > 0 && bounds.Height > 0)
                    TextRenderer.DrawText(pe.Graphics, "Complex chart rendering failed: " + LastRenderError,
                        Font, new Rectangle(bounds.Left, bounds.Top, bounds.Width, Math.Min(90, bounds.Height)),
                        Color.DarkRed, BackColor,
                        TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.Top);
            }
        }


        private void PaintOn(Graphics g, Rectangle r, Image source)
        {
            if (r.Width <= 0 || r.Height <= 0)
                return;
            if (source != null)
                g.DrawImage(source, r);

            if (ShouldDrawAxes)
                DrawAxes(g, r);
        }

        private void DrawAxes(Graphics g, Rectangle bounds)
        {
            ComplexAxesRenderer.Draw(g, bounds, XMin, XMax, YMin, YMax,
                AxisArrowRelativeSize, XLabel, YLabel, LabelsFont, AxesColor, LabelsColor);
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
