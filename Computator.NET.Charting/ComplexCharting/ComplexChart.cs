using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Forms;
using Computator.NET.Charting.Printing;
using Computator.NET.DataTypes;

namespace Computator.NET.Charting.ComplexCharting
{
    public sealed class ComplexChart : Control, IChart, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region private fields

        // private Graphics g;
        private double quality;
        //  private Task RedrawTask;
        private readonly Color[,] pointsColors =
            new Color[Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height];

        private readonly ComplexPoint[,] pointsValues =
            new ComplexPoint[Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height];

        private bool drawn;
        private Function function;
        private Bitmap image = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

        private readonly ToolTip toolTip = new ToolTip
        {
            AutoPopDelay = 5000,
            InitialDelay = 1000,
            ReshowDelay = 500
        };

        private readonly BackgroundWorker worker;
        //private bool doNotRecalculate;

        #endregion

        #region public properties

        public double countourLinesStep { get; set; } = Math.E;

        public CountourLinesMode countourMode
        {
            get { return _countourMode; }
            set
            {
                if (_countourMode != value)
                {
                    _countourMode = value;
                    OnPropertyChanged(nameof(countourMode));
                }
            }
        }

        public AssignmentOfColorMethod colorAssignmentMethod
        {
            get { return _colorAssignmentMethod; }
            set
            {
                if (_colorAssignmentMethod != value)
                {
                    _colorAssignmentMethod = value;
                    OnPropertyChanged(nameof(colorAssignmentMethod));
                }
            }
        }

        public double axisArrowRelativeSize { get; set; } = 0.02;

        public double Quality
        {
            set
            {
                if (value > 100)
                    value = 100;
                if (value < 0)
                    value = 0;


                calculateQuality(value);
                Redraw();
            }
            get { return quality*100; }
        }

        public double XYRatio
        {
            get { return (XMax - XMin)/Width/((YMax - YMin)/Height); }
        }

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

        public string xLabel { get; set; } = "Re(z)";
        public string yLabel { get; set; } = "Im(z)";
        public string title { get; set; }

        public Font titleFont { get; set; } = CustomFonts.GetMathFont(13);
        public Font labelsFont { get; set; } = CustomFonts.GetMathFont(13);
        public Color labelsColor { get; set; } = Color.Black;
        public Color titleColor { get; set; } = Color.Black;
        public Color axesColor { get; set; } = Color.Black;

        public bool shouldDrawAxes { get; set; } = true;

        private void calculateQuality(double value)
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

        public ComplexChart()
        {
            InitializeComponent();
            DoubleBuffered = true;
            attachEventHandlers();
            Quality = 50;

            worker = new BackgroundWorker {WorkerReportsProgress = true};
            worker.DoWork += (o, e) =>
            {
                var bw = o as BackgroundWorker;
                calculateValuesAndColors();
                for (var x = 0; x < DrawWidth; x++)
                    for (var y = 0; y < DrawHeight; y++)
                        image.SetPixel(x, y, pointsColors[x, y]);
                drawn = true;
                Invalidate();
            };
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
            if (drawn)
            {
                var x = (int) (e.Location.X*quality);
                var y = (int) (e.Location.Y*quality);
                var fz = pointsValues[x, y].Fz;
                var z = pointsValues[x, y].Z;

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
            title = Fz.Name;
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
            //  var dialog = new SaveFileDialog();
            //   dialog.Filter = "Portable Network Graphics (*.png)|*.png";
            //   dialog.RestoreDirectory = true;

            if (image == null) return;
            //  using (var writer = dialog.OpenFile())
            {
                //     if (writer == null) return;
                image.Save(path, imageFormat);
            }
        }

        #endregion

        #region drawing

        private int DrawWidth
        {
            get { return (int) (Width*quality); }
        }

        private int DrawHeight
        {
            get { return (int) (Height*quality); }
        }

        public void ClearAll()
        {
            function = null;
            image = new Bitmap(DrawWidth, DrawHeight);
            Invalidate();
        }

        public void Redraw()
        {
            if (function != null && DrawWidth > 0 && DrawHeight > 0)
            {
                //if (!worker.IsBusy)
                //worker.RunWorkerAsync();
                calculateValuesAndColors();
                image = new Bitmap(DrawWidth, DrawHeight);
                for (var x = 0; x < DrawWidth; x++)
                    for (var y = 0; y < DrawHeight; y++)
                        image.SetPixel(x, y, pointsColors[x, y]);
                drawn = true;
            }
            Invalidate();
            Update();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (function != null)
            {
                //if(!worker.IsBusy)
                pe.Graphics.DrawImage(image, new Rectangle(0, 0, Width, Height),
                    new Rectangle(0, 0, DrawWidth, DrawHeight), GraphicsUnit.Pixel);
            }
            else
                drawn = false;
            if (shouldDrawAxes)
                DrawAxes(pe);
        }


        private void DrawAxes(PaintEventArgs pe)
        {
            using (var myPen = new Pen(axesColor))
            {
                var middlePoint = GetMiddlePoint();

                var xEnd = new Point(Width, middlePoint.Y);
                var yEnd = new Point(middlePoint.X, Height);

                var xEnd1Axis = new Point((int) (Width*(1 - axisArrowRelativeSize)),
                    (int) (middlePoint.Y*(1 - axisArrowRelativeSize)));
                var xEnd2Axis = new Point((int) (Width*(1 - axisArrowRelativeSize)),
                    (int) (middlePoint.Y*(1 + axisArrowRelativeSize)));

                var yEnd1Axis = new Point((int) (middlePoint.X*(1 - axisArrowRelativeSize)),
                    (int) (Height*axisArrowRelativeSize));
                var yEnd2Axis = new Point((int) (middlePoint.X*(1 + axisArrowRelativeSize)),
                    (int) (Height*axisArrowRelativeSize));


                var xStart = new Point(0, middlePoint.Y);
                var yStart = new Point(middlePoint.X, 0);

                //Y axis (ImZ)
                pe.Graphics.DrawLine(myPen, middlePoint, yEnd);
                pe.Graphics.DrawLine(myPen, middlePoint, yStart);
                pe.Graphics.DrawLine(myPen, yStart, yEnd1Axis);
                pe.Graphics.DrawLine(myPen, yStart, yEnd2Axis);
                pe.Graphics.DrawString(yLabel, labelsFont, new SolidBrush(labelsColor), middlePoint.X - 60, 15);

                //X axis(ReZ)
                pe.Graphics.DrawLine(myPen, middlePoint, xEnd);
                pe.Graphics.DrawLine(myPen, middlePoint, xStart);
                pe.Graphics.DrawLine(myPen, xEnd, xEnd1Axis);
                pe.Graphics.DrawLine(myPen, xEnd, xEnd2Axis);
                pe.Graphics.DrawString(xLabel, labelsFont, new SolidBrush(labelsColor), Width - 60, middlePoint.Y + 10);
            }
        }

        private Point GetMiddlePoint()
        {
            var x = (int) (Math.Abs(XMin)/(XMax - XMin)*Width);
            var y = (int) (Math.Abs(YMax)/(YMax - YMin)*Height);

            if (XMax <= 0) //we have only negative numbers for x
                x = Width - 1;
            else if (XMin >= 0) //we have only positive numbers for x
                x = 0;

            if (YMax <= 0)
                y = 0;
            else if (YMin >= 0)
                y = Height - 1;

            return new Point(x, y);
        }

        #endregion

        #region calculations

        private void calculateValuesAndColors()
        {
            Parallel.For(0, DrawWidth, x =>
            {
                var re = XMin + x*(XMax - XMin)/DrawWidth;
                for (var y = 0; y < DrawHeight; y++)
                {
                    var im = YMax - y*(YMax - YMin)/DrawHeight;

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
        }

        private Color ComplexToColor(Complex z)
        {
            double m = z.Magnitude, t = z.Phase;

            while (t < 0.0) t += 2*Math.PI;
            while (t >= 2*Math.PI) t -= 2*Math.PI;

            double h = t/(2*Math.PI), s = 0, v = 0;
            double r0 = 0, r1 = 1;

            switch (countourMode)
            {
                case CountourLinesMode.Logarithmic:
                    //Based on Claudio Rocchini C++ algorithm for complex functions domain coloring http://en.wikipedia.org/wiki/File:Color_complex_plot.jpg
                    while (m > r1)
                    {
                        r0 = r1;
                        r1 = r1*countourLinesStep;
                    }
                    break;

                case CountourLinesMode.Linear:
                    r1 = countourLinesStep;
                    while (m > r1)
                    {
                        r0 = r1;
                        r1 = r1 + countourLinesStep;
                    }
                    break;

                case CountourLinesMode.Exponential:
                    r1 = Math.Log(Math.Pow(m, 1/m), countourLinesStep);
                    while (m > r1)
                    {
                        r0 = r1;
                        r1 = r1 + Math.Log(Math.Pow(m, 1/m), countourLinesStep);
                    }
                    break;
            }

            if (countourMode != CountourLinesMode.NoCountourLines)
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
                switch (colorAssignmentMethod)
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
            comboBoxes[0].SelectedItem = countourMode;
            comboBoxes[0].DropDownStyle = ComboBoxStyle.DropDownList;

            var vartosci2 =
                Enum.GetValues(typeof(AssignmentOfColorMethod))
                    .Cast<AssignmentOfColorMethod>()
                    .ToList();
            foreach (var v in vartosci2)
                comboBoxes[1].Items.Add(v);
            comboBoxes[1].SelectedItem = colorAssignmentMethod;
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
            return Color.FromArgb(ir, ig, ib);
        }

        #endregion
    }
}