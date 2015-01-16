using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Forms;
using Computator.NET.Functions;

namespace Computator.NET.Charting.ComplexCharting
{
    public class ComplexChart : Control, IChart<Complex> //, INotifyPropertyChanged
    {
        #region private fields

        private readonly Color[,] pointsColors;
        private readonly ComplexPoint[,] pointsValues;
        private bool drawn;
        private Function<Complex> function;
        private Bitmap image;
        private ToolTip toolTip;
        private BackgroundWorker worker;
        //private bool doNotRecalculate;

        #endregion

        private Task RedrawTask;
        private Graphics g;
        private double quality;

        public void setupComboBoxes(params ToolStripComboBox[] comboBoxes)
        {
            comboBoxes[0].AutoSize = true;
            comboBoxes[1].AutoSize = true;
            List<CountourLinesMode> vartosci =
                Enum.GetValues(typeof (CountourLinesMode)).Cast<CountourLinesMode>().ToList<CountourLinesMode>();
            foreach (CountourLinesMode v in vartosci)
                comboBoxes[0].Items.Add(v);
            comboBoxes[0].SelectedItem = countourMode;
            comboBoxes[0].DropDownStyle = ComboBoxStyle.DropDownList;

            List<AssignmentOfColorMethod> vartosci2 =
                Enum.GetValues(typeof (AssignmentOfColorMethod))
                    .Cast<AssignmentOfColorMethod>()
                    .ToList<AssignmentOfColorMethod>();
            foreach (AssignmentOfColorMethod v in vartosci2)
                comboBoxes[1].Items.Add(v);
            comboBoxes[1].SelectedItem = colorAssignmentMethod;
            comboBoxes[1].DropDownStyle = ComboBoxStyle.DropDownList;
        }

        //   public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "")
        {
            // if (PropertyChanged != null)
            //  {
            //      PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            //   }
        }

        private static Color ColorFromHSV(double hue, double saturation, double value)
        {
            double h = hue;
            double s = saturation;
            double v = value;

            double r, g, b;
            if (s == 0)
            {
                r = g = b = v;
            }
            else
            {
                if (h == 1.0) h = 0.0;
                double z = Math.Truncate(6*h);
                var i = (int) z;
                double f = h*6 - z;
                double p = v*(1 - s);
                double q = v*(1 - s*f);
                double t = v*(1 - s*(1 - f));

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
                for (int x = 0; x < drawWidth; x++)
                    for (int y = 0; y < drawHeight; y++)
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
                Complex fz = pointsValues[x, y].Fz;
                Complex z = pointsValues[x, y].Z;

                toolTip.SetToolTip(this,
                    String.Format(
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

        public void addFx(Func<Complex, Complex> Fz, string name)
        {
            function = new Function<Complex>(Fz, name);
            title = name;
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
                    using (Stream writer = dialog.OpenFile())
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
            if (function != null)
            {
                //if (!worker.IsBusy)
                //worker.RunWorkerAsync();
                calculateValuesAndColors();
                image = new Bitmap(drawWidth, drawHeight);
                for (int x = 0; x < drawWidth; x++)
                    for (int y = 0; y < drawHeight; y++)
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
                Point middlePoint = getMiddlePoint();

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
                double re = XMin + x*(XMax - XMin)/drawWidth;
                for (int y = 0; y < drawHeight; y++)
                {
                    double im = YMax - y*(YMax - YMin)/drawHeight;

                    var z = new Complex(re, im);

                    Complex fz = function.eval(z);
                    pointsValues[x, y] = new ComplexPoint(z, fz);

                    if (Double.IsInfinity(fz.Real) || Double.IsNaN(fz.Real) || Double.IsInfinity(fz.Imaginary) ||
                        Double.IsNaN(fz.Imaginary))
                    {
                        pointsColors[x, y] = Color.White;
                    }
                    else
                    {
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
                double r = (m - r0)/(r1 - r0);
                double p = r < 0.5 ? 2.0*r : 2.0*(1.0 - r);
                double q = 1.0 - p;
                double p1 = 1 - q*q*q;
                double q1 = 1 - p*p*p;
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

        #region tests

        private void testParallelCalculation()
        {
            var timer = new Stopwatch();

            int parameterT = 20;

            timer.Start();
            for (int i = 0; i < parameterT; i++)
                method1_x_parallel();
            timer.Stop();
            MessageBox.Show(((timer.ElapsedMilliseconds)/1000.0).ToString());
            timer.Reset();


            timer.Start();
            for (int i = 0; i < parameterT; i++)
                method2_y_parallel();
            timer.Stop();
            MessageBox.Show(((timer.ElapsedMilliseconds)/1000.0).ToString());
            timer.Reset();


            timer.Start();
            for (int i = 0; i < parameterT; i++)
                method3_NO_parallel();
            timer.Stop();
            MessageBox.Show(((timer.ElapsedMilliseconds)/1000.0).ToString());
            timer.Reset();


            timer.Start();
            for (int i = 0; i < parameterT; i++)
                method4_BOTH_parallel();
            timer.Stop();
            MessageBox.Show(((timer.ElapsedMilliseconds)/1000.0).ToString());
            timer.Reset();
        }

        private void method1_x_parallel()
        {
            Parallel.For(0, Width, x => //for (int x = 0; x < Width; x++)
            {
                double re = XMin + x*(XMax - XMin)/Width;
                for (int y = 0; y < Height; y++) //Parallel.For(0, Height, (y) =>
                {
                    double im = YMax - y*(YMax - YMin)/Height;

                    var z = new Complex(re, im);


                    Complex fz = function.eval(z);
                    pointsValues[x, y] = new ComplexPoint(z, fz);


                    if (Double.IsInfinity(fz.Real) || Double.IsNaN(fz.Real) || Double.IsInfinity(fz.Imaginary) ||
                        Double.IsNaN(fz.Imaginary))
                    {
                    } //continue;//pointsColors[x, y] = default(Color);
                    else
                    {
                        pointsColors[x, y] = ComplexToColor(fz);
                        //image.SetPixel(x, y, ComplexToColor(fz)/*pointsColors[x, y]*/);
                    }
                } //);
            });
        }


        private void method2_y_parallel()
        {
            for (int x = 0; x < Width; x++)
            {
                double re = XMin + x*(XMax - XMin)/Width;
                Parallel.For(0, Height, y =>
                {
                    double im = YMax - y*(YMax - YMin)/Height;

                    var z = new Complex(re, im);


                    Complex fz = function.eval(z);
                    pointsValues[x, y] = new ComplexPoint(z, fz);


                    if (Double.IsInfinity(fz.Real) || Double.IsNaN(fz.Real) || Double.IsInfinity(fz.Imaginary) ||
                        Double.IsNaN(fz.Imaginary))
                    {
                    } //continue;//pointsColors[x, y] = default(Color);
                    else
                    {
                        pointsColors[x, y] = ComplexToColor(fz);
                        //image.SetPixel(x, y, ComplexToColor(fz)/*pointsColors[x, y]*/);
                    }
                });
            }
        }

        private void method3_NO_parallel()
        {
            for (int x = 0; x < Width; x++)
            {
                double re = XMin + x*(XMax - XMin)/Width;
                for (int y = 0; y < Height; y++)
                {
                    double im = YMax - y*(YMax - YMin)/Height;

                    var z = new Complex(re, im);


                    Complex fz = function.eval(z);
                    pointsValues[x, y] = new ComplexPoint(z, fz);


                    if (Double.IsInfinity(fz.Real) || Double.IsNaN(fz.Real) || Double.IsInfinity(fz.Imaginary) ||
                        Double.IsNaN(fz.Imaginary))
                    {
                    } //continue;//pointsColors[x, y] = default(Color);
                    else
                    {
                        pointsColors[x, y] = ComplexToColor(fz);
                        //image.SetPixel(x, y, ComplexToColor(fz)/*pointsColors[x, y]*/);
                    }
                }
            }
        }


        private void method4_BOTH_parallel()
        {
            Parallel.For(0, Width, x =>
            {
                double re = XMin + x*(XMax - XMin)/Width;
                Parallel.For(0, Height, y =>
                {
                    double im = YMax - y*(YMax - YMin)/Height;

                    var z = new Complex(re, im);


                    Complex fz = function.eval(z);
                    pointsValues[x, y] = new ComplexPoint(z, fz);


                    if (Double.IsInfinity(fz.Real) || Double.IsNaN(fz.Real) || Double.IsInfinity(fz.Imaginary) ||
                        Double.IsNaN(fz.Imaginary))
                    {
                    } //continue;//pointsColors[x, y] = default(Color);
                    else
                    {
                        pointsColors[x, y] = ComplexToColor(fz);
                        //image.SetPixel(x, y, ComplexToColor(fz)/*pointsColors[x, y]*/);
                    }
                });
            });
        }

        #endregion
    }
}