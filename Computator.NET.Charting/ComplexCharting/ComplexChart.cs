using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Forms;
using Computator.NET.DataTypes;

namespace Computator.NET.Charting.ComplexCharting
{
    public class ComplexChart : Control //, INotifyPropertyChanged
    {
        #region private fields

        private Graphics g;
        private double quality;
        private Task RedrawTask;
        private readonly Color[,] pointsColors;
        private readonly ComplexPoint[,] pointsValues;
        private bool drawn;
        private Function function;
        private Bitmap image;
        private ToolTip toolTip;
        private readonly BackgroundWorker worker;
        //private bool doNotRecalculate;

        #endregion

        #region public properties

        public double countourLinesStep { get; set; }
        public CountourLinesMode countourMode { get; set; }
        public AssignmentOfColorMethod colorAssignmentMethod { get; set; }
        public double axisArrowRelativeSize { get; set; }

        public double Quality
        {
            set
            {
                if (value <= 100 && value >= 0)
                {
                    calculateQuality(value);
                    reDraw();
                }
            }
            get { return quality*100; }
        }

        public double XYRatio
        {
            get { return ((XMax - XMin)/Width)/((YMax - YMin)/Height); }
        }

        public double XMin { get; set; }
        public double XMax { get; set; }
        public double YMin { get; set; }
        public double YMax { get; set; }

        public string xLabel { get; set; }
        public string yLabel { get; set; }
        public string title { get; set; }

        public Font titleFont { get; set; }
        public Font labelsFont { get; set; }
        public Color labelsColor { get; set; }
        public Color titleColor { get; set; }
        public Color axesColor { get; set; }

        public bool shouldDrawAxes { get; set; }
        public bool AxesEqual { get; set; }

        private void calculateQuality(double value)
        {
            if (value >= 50)
                quality = 1.0;
            else
                quality = (value/50);

            if (quality < 0.1)
                quality = 0.1;
        }

        #endregion

        #region constructing

        public ComplexChart()
        {
            pointsValues = new ComplexPoint[Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height];
            pointsColors = new Color[Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height];
            image = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            InitializeComponent();
            attachEventHandlers();
            quality = 1.0;
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += ((o, e) =>
            {
                var bw = o as BackgroundWorker;
                calculateValuesAndColors();
                for (var x = 0; x < drawWidth; x++)
                    for (var y = 0; y < drawHeight; y++)
                        image.SetPixel(x, y, pointsColors[x, y]);
                drawn = true;
                Invalidate();
            });
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
            toolTip = new ToolTip();
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 1000;
            toolTip.ReshowDelay = 500;
            drawn = false;
            axisArrowRelativeSize = 0.02;
            countourLinesStep = Math.E;
            countourMode = CountourLinesMode.Logarithmic;
            colorAssignmentMethod = AssignmentOfColorMethod.GreaterIsDarker;
            //doNotRecalculate = false;
            Dock = DockStyle.Fill;
            shouldDrawAxes = true;

            xLabel = "Re(z)";
            yLabel = "Im(z)";
            labelsFont = new Font("Cambria", 13, FontStyle.Regular);
            labelsColor = Color.Black;
            titleColor = Color.Black;
            axesColor = Color.Black;
            titleFont = new Font("Cambria", 13, FontStyle.Regular);
        }

        #endregion

        #region event handlers

        private void _Resize(object s, EventArgs e)
        {
            if (Width > 0 && Height > 0)
                reDraw();
            //else
            //doNotRecalculate = true;
        }

        private void _MouseLeave(object s, EventArgs e)
        {
            toolTip.Hide(this);
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
                    string.Format(
                        "f(z) = {0:0.###}{1:+0.###;-0.###}i = {2:0.###} exp({3:0.###})\nz = {4:0.###}{5:+0.###;-#0.###}i = {6:0.###} exp({7:0.###})",
                        fz.Real, fz.Imaginary, fz.Magnitude, fz.Phase, z.Real, z.Imaginary, z.Magnitude, z.Phase));
                toolTip.ShowAlways = true;
            }
        }

        private void _MouseMove(object s, MouseEventArgs e)
        {
            /* if (drawn)
             {
                 // this.Capture = true;
                 Complex fz = pointsValues[e.Location.X, e.Location.Y].Fz;
                 Complex z = pointsValues[e.Location.X, e.Location.Y].Z;

                 //toolTip.SetToolTip(this, String.Format("f(z)={0}{1:+#;-#;0}i = {2} exp({3})\nz={4}{5:+#;-#;0}i = {6} exp({7})", fz.Real, fz.Imaginary, fz.Magnitude, fz.Phase, z.Real, z.Imaginary, z.Magnitude, z.Phase));
                 toolTip.Show(String.Format("f(z) = {0:0.###}{1:+0.###;-0.###}i = {2:0.###} exp({3:0.###})\nz = {4:0.###}{5:+0.###;-#0.###}i = {6:0.###} exp({7:0.###})", fz.Real, fz.Imaginary, fz.Magnitude, fz.Phase, z.Real, z.Imaginary, z.Magnitude, z.Phase), this, new Point(e.X + 31, e.Y + 31), int.MaxValue);
                 toolTip.ShowAlways = true;
             }*/
            // toolTip.SetToolTip(this, "x = " + ((double)e.Location.X) / ((double)(Width)) + "\ny = " + ((double)e.Location.Y) / ((double)(Height)));
        }

        #endregion

        #region public methods

        public void addFx(Function Fz)
        {
            function = Fz;
            title = Fz.Name;
            reDraw();
        }

        public void addFx(Func<Complex, Complex> Fz, string name)
        {
            function = new Function(Delegate.CreateDelegate(typeof (Func<Complex, Complex>), Fz.Method), name, name);
            title = function.Name;
            reDraw();
        }

        public void setChartAreaValues(double x0, double xn, double y0, double yn)
        {
            XMin = x0;
            XMax = xn;
            YMin = y0; //min;
            YMax = yn; //max;
        }

        public void saveImage()
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Portable Network Graphics (*.png)|*.png";
            dialog.RestoreDirectory = true;

            if (image != null)
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    using (var writer = dialog.OpenFile())
                    {
                        if (writer == null) return;
                        image.Save(writer, ImageFormat.Png);
                    }
                }
        }

        #endregion

        #region drawing

        private int drawWidth
        {
            get { return (int) (Width*quality); }
        }

        private int drawHeight
        {
            get { return (int) (Height*quality); }
        }

        public void ClearAll()
        {
            function = null;
            image = new Bitmap(drawWidth, drawHeight);
            Invalidate();
        }

        public void reDraw()
        {
            if (function != null && drawWidth > 0 && drawHeight > 0)
            {
                //if (!worker.IsBusy)
                //worker.RunWorkerAsync();
                calculateValuesAndColors();
                image = new Bitmap(drawWidth, drawHeight);
                for (var x = 0; x < drawWidth; x++)
                    for (var y = 0; y < drawHeight; y++)
                        image.SetPixel(x, y, pointsColors[x, y]);
                drawn = true;
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (function != null)
            {
                //if(!worker.IsBusy)
                pe.Graphics.DrawImage(image, new Rectangle(0, 0, Width, Height),
                    new Rectangle(0, 0, drawWidth, drawHeight), GraphicsUnit.Pixel);
            }
            else
                drawn = false;
            if (shouldDrawAxes)
                drawAxes(pe);
        }


        private void drawAxes(PaintEventArgs pe)
        {
            using (var myPen = new Pen(axesColor))
            {
                var middlePoint = getMiddlePoint();

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

        private Point getMiddlePoint()
        {
            var x = (int) ((Math.Abs(XMin)/(XMax - XMin))*Width);
            var y = (int) ((Math.Abs(YMax)/(YMax - YMin))*Height);

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
            Parallel.For(0, drawWidth, x =>
            {
                var re = XMin + x*(XMax - XMin)/drawWidth;
                for (var y = 0; y < drawHeight; y++)
                {
                    var im = YMax - y*(YMax - YMin)/drawHeight;

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
                double coefficient;
                //if(m>1)
                coefficient = (Math.Log(m + 1, 2))/(Math.Log(double.MaxValue, 2));
                //else
                //coefficient = (Math.Pow(m,1/100))/(Math.Pow(double.MaxValue,1/100));
                switch (colorAssignmentMethod)
                {
                    case AssignmentOfColorMethod.GreaterIsDarker:
                        s = 0.4 + 0.6*coefficient;
                        v = 0.6 + 0.4*(1 - coefficient);

                        break;
                    case AssignmentOfColorMethod.GreaterIsLighter:
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
                Enum.GetValues(typeof (CountourLinesMode)).Cast<CountourLinesMode>().ToList<CountourLinesMode>();
            foreach (var v in vartosci)
                comboBoxes[0].Items.Add(v);
            comboBoxes[0].SelectedItem = countourMode;
            comboBoxes[0].DropDownStyle = ComboBoxStyle.DropDownList;

            var vartosci2 =
                Enum.GetValues(typeof (AssignmentOfColorMethod))
                    .Cast<AssignmentOfColorMethod>()
                    .ToList<AssignmentOfColorMethod>();
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