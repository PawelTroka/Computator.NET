using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
using NLog;

namespace Computator.NET.Charting.RealCharting
{
    public class Chart2D : Chart, IChart2D
    {
        private const double OVERFLOW_VALUE = (double) decimal.MaxValue/10; //1073741951.0/500; //1111117;
        private const double UNDERFLOW_VALUE = (double) decimal.MinValue/10; //-1073741760.0/500; // 1111117;
        private const int MOVE_N = 100;
        private readonly SeriesChartType defaultImplicitFunctionsChartType = SeriesChartType.FastPoint;
        private readonly List<Function> functions = new List<Function>();
        // private readonly List<Function<double>> implicitFunctions;
        private readonly List<List<Point2D>> points = new List<List<Point2D>>();
        private Point _lastMouseLocation;
        private int _lineThickness = 2;
        private double _oldN;
        private int _pointsSize = 2;
        private bool _rightButtonPressed;
        private SeriesChartType defaultExplicitFunctionsChartType = SeriesChartType.Line; //SeriesChartType.FastLine;

        private double quality;
        private double scalingFactor = 1;
        private ToolStripComboBox seriesComboBox;
        /*
         * It turns out that GDI+ has hard bounds for drawing coordinates for DrawLine method //http://stackoverflow.com/questions/3468495/what-are-the-hard-bounds-for-drawing-coordinates-in-gdi
         * positive:    1,073,741,951
         * negative:   -1,073,741,760
         * 
         * If it wouldnt be like that there is still a problem with scaling in MS Chart Class because of decimal values used to scaling
         * 7.92E+27
         * -7.92E+27
         */
        private double TOLERANCE = 1e-8;
        private bool xOnlyZoomMode;
        private bool yOnlyZoomMode;

        public Chart2D()
        {
            MouseDoubleClick += _MouseDoubleClick;
            MouseClick += _MouseClick;
            MouseWheel += _MouseWheel;
            MouseDown += _MouseDown;
            MouseUp += _MouseUp;
            MouseMove += _MouseMove;
            Resize += (o, e) => NotifyPropertyChanged("XyRatio");
            //   defaultExplicitFunctionsChartType = SeriesChartType.Line;//////////////////////////////////
            xOnlyZoomMode = yOnlyZoomMode = false;
            InitializeComponent();
            //implicitFunctions= new List<Function<double>>();
            //   addSampleImplicitFunction();
            //implicitFunctions.Add(new Function<double>((x, y) => Math.Sin(x*x-y*y) - Math.Sin(x+y)-Math.Cos(x*y), "|sin(x²-y²)| = sin(x+y)+cos(x·y)"));
            Quality = 50;
        }

        public int LineThickness
        {
            get { return _lineThickness; }
            set
            {
                foreach (var s in Series)
                {
                    s.BorderWidth = value;
                }
                _lineThickness = value;
            }
        }

        public int PointsSize
        {
            get { return _pointsSize; }
            set
            {
                foreach (var s in Series)
                {
                    s.MarkerSize = value;
                }
                _pointsSize = value;
            }
        }

        private double N { get; set; } = 3571;

        public string XLabel
        {
            get { return ChartAreas[0].AxisX.Title; }
            set { ChartAreas[0].AxisX.Title = value; }
        }

        public string YLabel
        {
            get { return ChartAreas[0].AxisY.Title; }
            set { ChartAreas[0].AxisY.Title = value; }
        }

        public string Title
        {
            get { return Titles[0].Text; }
            set { Titles[0].Text = value; }
        }

        public Font TitleFont
        {
            get { return Titles[0].Font; }
            set { Titles[0].Font = value; }
        }

        public Font LabelsFont
        {
            get { return ChartAreas[0].AxisY.TitleFont; }
            set { ChartAreas[0].AxisY.TitleFont = ChartAreas[0].AxisX.TitleFont = value; }
        }

        public Color LabelsColor
        {
            get { return ChartAreas[0].AxisY.TitleForeColor; }
            set { ChartAreas[0].AxisY.TitleForeColor = ChartAreas[0].AxisX.TitleForeColor = value; }
        }

        public Color TitleColor
        {
            get { return Titles[0].ForeColor; }
            set { Titles[0].ForeColor = value; }
        }

        public Color AxesColor
        {
            get { return ChartAreas[0].AxisX.LineColor; }
            set { ChartAreas[0].AxisX.LineColor = value; }
        }

        //public double axisArrowRelativeSize { get; set; }

        public bool ShouldDrawAxes { get; set; }
        public bool AxesEqual { get; set; } = true;

        public double XyRatio
        {
            get
            {
                if (functions == null || functions.Count == 0)
                    return 1.0;
                return (XMax - XMin)/
                       Math.Abs(ChartAreas[0].AxisX.ValueToPixelPosition(XMax) -
                                ChartAreas[0].AxisX.ValueToPixelPosition(XMin))/
                       ((YMax - YMin)/
                        Math.Abs(ChartAreas[0].AxisY.ValueToPixelPosition(YMax) -
                                 ChartAreas[0].AxisY.ValueToPixelPosition(YMin)));
            }
        }

        #region changeMethods

        public SeriesChartType ChartType
        {
            get { return defaultExplicitFunctionsChartType; }
            set
            {
                if (value != defaultExplicitFunctionsChartType)
                {
                    defaultExplicitFunctionsChartType = value;
                    for (var i = 0; i < Series.Count; i++)
                        Series[i].ChartType = value;
                    NotifyPropertyChanged(nameof(ChartType));
                }
            }
        }


        /* //TODO: think about making it work good with menu / command pattern etc
        public void ShowAllSeries()
        {
            foreach (var s in Series)
                s.Enabled = true;
        }
        public void ShowSeries(string series)
        {

                foreach (var s in Series)
                    s.Enabled = false;
                Series[series].Enabled = true;
            
        }
        */

        #endregion

        public double XMin
        {
            get { return ChartAreas[0].AxisX.Minimum; }
            set
            {
                if (value < UNDERFLOW_VALUE)
                    value = UNDERFLOW_VALUE;

                if (value > OVERFLOW_VALUE)
                    value = OVERFLOW_VALUE;
                if (ChartAreas[0].AxisX.Minimum == value) return;


                ChartAreas[0].AxisX.Minimum = value;
                XMinChanged?.Invoke(this, new EventArgs());

                NotifyPropertyChanged("XyRatio");
                _refreshFunctions();
            }
        }

        public void Redraw()
        {
            Invalidate();
        }

        public double XMax
        {
            get { return ChartAreas[0].AxisX.Maximum; }
            set
            {
                if (value < UNDERFLOW_VALUE)
                    value = UNDERFLOW_VALUE;

                if (value > OVERFLOW_VALUE)
                    value = OVERFLOW_VALUE;
                if (ChartAreas[0].AxisX.Maximum == value) return;


                ChartAreas[0].AxisX.Maximum = value;
                XMaxChanged?.Invoke(this, new EventArgs());
                _refreshFunctions();
            }
        }

        public double YMin
        {
            get { return ChartAreas[0].AxisY.Minimum; }
            set
            {
                if (value < UNDERFLOW_VALUE)
                    value = UNDERFLOW_VALUE;

                if (value > OVERFLOW_VALUE)
                    value = OVERFLOW_VALUE;

                if (ChartAreas[0].AxisY.Minimum == value) return;


                ChartAreas[0].AxisY.Minimum = value;
                YMinChanged?.Invoke(this, new EventArgs());
                _refreshFunctions();
            }
        }

        public double YMax
        {
            get { return ChartAreas[0].AxisY.Maximum; }
            set
            {
                if (value < UNDERFLOW_VALUE)
                    value = UNDERFLOW_VALUE;

                if (value > OVERFLOW_VALUE)
                    value = OVERFLOW_VALUE;

                if (ChartAreas[0].AxisY.Maximum == value) return;
                ChartAreas[0].AxisY.Maximum = value;
                YMaxChanged?.Invoke(this, new EventArgs());
                _refreshFunctions();
            }
        }

        public event EventHandler XMinChanged;
        public event EventHandler XMaxChanged;
        public event EventHandler YMinChanged;
        public event EventHandler YMaxChanged;

        public double Quality
        {
            get { return quality; }
            set
            {
                if (value > 100)
                    value = 100;
                if (value < 0)
                    value = 0;

                quality = value;
                calculateN(value);
                _refreshFunctions();
            }
        }

        public void SetChartAreaValues(double x0, double xn, double y0, double yn)
        {
            ChartAreas[0].AxisX.Minimum = x0;
            ChartAreas[0].AxisX.Maximum = xn;

            ChartAreas[0].AxisY.Minimum = y0; //min;
            ChartAreas[0].AxisY.Maximum = yn; //max;
        }

        public void AddFunction(Function f)
        {
            if (!Series.IsUniqueName(f.Name)) //nothing new to add
                return;

            functions.Add(f);
            _addNewFunction();
        }

        public void Print()
        {
            Printing.Print(true);
        }

        public void PrintPreview()
        {
            Printing.PrintPreview();
        }

        //exp(x/20)*(sin(1/2*x)+cos(3*x)+0.2*sin(4*x)*cos(40*x))

        public void ClearAll()
        {
            Series.Clear();
            functions.Clear();
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
            var plotForm = new Computator.NET.Charting.PlotForm(this);
            plotForm.Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static IEnumerable<double> Iterate(
            double fromInclusive, double toExclusive, double step)
        {
            for (var d = fromInclusive; d <= toExclusive; d += step) yield return d;
        }

        private void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void calculateN(double quality)
        {
            if (quality < 51)
                N = 3488*(quality/50) + 83;
            else if (quality < 61)
                N = 7873/2*(quality/45) + 3571;
            else if (quality < 71)
                N = 7919/2*(quality/40) + 7907;
            else if (quality < 81)
                N = 7919/2*(quality/30) + 7907;
            else if (quality < 90)
                N = 7919/2*(quality/20) + 7907;
            else
                N = 7919/2*(quality/10) + 7907;
        }

        private void _MouseMove(object sender, MouseEventArgs e)
        {
            if (!_rightButtonPressed) return;

            if (Math.Abs(_lastMouseLocation.X - e.Location.X) <= 1 &&
                Math.Abs(_lastMouseLocation.Y - e.Location.Y) <= 1) return;
            var elements = HitTest(e.X, e.Y, ChartElementType.PlottingArea);
            if (elements?.Object == null)
                return;

            var deltaX = ChartAreas[0].AxisX.PixelPositionToValue(_lastMouseLocation.X) -
                         ChartAreas[0].AxisX.PixelPositionToValue(e.Location.X);

            var deltaY = ChartAreas[0].AxisY.PixelPositionToValue(_lastMouseLocation.Y) -
                         ChartAreas[0].AxisY.PixelPositionToValue(e.Location.Y);


            SetChartAreaValues(XMin + deltaX.RoundToSignificantDigits(1),
                XMax + deltaX.RoundToSignificantDigits(1), YMin + deltaY.RoundToSignificantDigits(1),
                YMax + deltaY.RoundToSignificantDigits(1));
            Refresh2();

            _lastMouseLocation = e.Location;
        }

        public void Transform(Func<double[], double[]> transformate, string transformateName)
        {
            foreach (var serie in Series)
            {
                var yvalues = new List<double>();
                foreach (var p in serie.Points)
                    yvalues.Add(p.YValues[0]);


                var yvaluesAfterTransform = transformate(yvalues.ToArray());
                for (var i = 0; i < serie.Points.Count; i++)
                    serie.Points[i].YValues[0] = yvaluesAfterTransform[i];
                serie.Name = transformateName + "(" + serie.Name + ")";
            }
        }

        private void Refresh2()
        {
            XMinChanged?.Invoke(this, new EventArgs());
            XMaxChanged?.Invoke(this, new EventArgs());
            YMinChanged?.Invoke(this, new EventArgs());
            YMaxChanged?.Invoke(this, new EventArgs());
            _refreshFunctions();
        }

        private void InitializeComponent()
        {
            var chartArea1  = this.ChartAreas.Add("ChartArea1");
            var legend1 = new Legend();
            var title1 = new Title();
            chartArea1.AxisX = new Axis(chartArea1, AxisName.X)
            {
                ArrowStyle = AxisArrowStyle.Lines,
                Crossing = 0D,
                InterlacedColor = Color.White,
                IsLabelAutoFit = false,
                LabelAutoFitMaxFontSize = 13,
                LabelStyle = new LabelStyle()
                {
                    Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular,
                        GraphicsUnit.Point, 238)
                },
                LineWidth = 2,
                MajorGrid = new Grid() {LineDashStyle = ChartDashStyle.Dash},
                MajorTickMark = new TickMark()
                {
                    Size = 2F,
                    TickMarkStyle = TickMarkStyle.AcrossAxis
                },
                MinorGrid = new Grid()
                {
                    Enabled = true,
                    LineColor = Color.DarkGray,
                    LineDashStyle = ChartDashStyle.Dot
                },
                MinorTickMark = new TickMark()
                {
                    Enabled = true,
                    TickMarkStyle = TickMarkStyle.AcrossAxis
                },
                Title = "X",
                TitleAlignment = StringAlignment.Far,
                TitleFont = new Font("Microsoft Sans Serif", 18F, FontStyle.Bold, GraphicsUnit.Point, 238)
            };

            chartArea1.AxisY = new Axis(chartArea1, AxisName.Y)
            {
                ArrowStyle = AxisArrowStyle.Lines,
                Crossing = 0D,
                InterlacedColor = Color.White,
                IsLabelAutoFit = false,
                LabelAutoFitMaxFontSize = 13,
                LabelStyle = new LabelStyle()
                {
                    Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular,
                        GraphicsUnit.Point, 238)
                },
                LineWidth = 2,
                MajorGrid = new Grid() {LineDashStyle = ChartDashStyle.Dash},
                MajorTickMark = new TickMark()
                {
                    Size = 2F,
                    TickMarkStyle = TickMarkStyle.AcrossAxis
                },
                MinorGrid = new Grid()
                {
                    Enabled = true,
                    LineColor = Color.DarkGray,
                    LineDashStyle = ChartDashStyle.Dot
                },
                MinorTickMark = new TickMark()
                {
                    Enabled = true,
                    TickMarkStyle = TickMarkStyle.AcrossAxis
                },
                TextOrientation = TextOrientation.Horizontal,
                Title = "Y",
                TitleAlignment = StringAlignment.Far,
                TitleFont = new Font("Microsoft Sans Serif", 18F, FontStyle.Bold, GraphicsUnit.Point, 238)
            };

            //By default, if I select a rectangular area using the mouse, the chart will zoom to the selected area.
            //But this is quite annoying because it is prone to false operation
            chartArea1.CursorX = new System.Windows.Forms.DataVisualization.Charting.Cursor
            {
                IsUserEnabled = false,
                IsUserSelectionEnabled = false
            };

            
            
            legend1.Font = CustomFonts.GetMathFont(13.8F); //new Font("Cambria", 13.8F);
            legend1.IsTextAutoFit = false;
            legend1.Name = "Legend1";

            Legends.Add(legend1);

            //this.N = 0D;
            Name = "chart2d";
            title1.Font = new Font("Microsoft Sans Serif", 22.2F, FontStyle.Bold, GraphicsUnit.Point, 238);
            title1.Name = "Title1";
            Titles.Add(title1);
            Dock = DockStyle.Fill;


            ChartAreas[0].AxisX.ScaleView.MinSize = 0.1;
            ChartAreas[0].AxisY.ScaleView.MinSize = 0.1;


            Legends[0].Font = CustomFonts.GetMathFont(Legends[0].Font.Size);
            const float fontsize = 17.0F;

            Font = CustomFonts.GetMathFont(fontsize);
        }

        private void _MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.XButton2)
                xOnlyZoomMode = false;

            if (e.Button == MouseButtons.XButton1)
                yOnlyZoomMode = false;

            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Left)
                //TODO: find better usage for one of these buttons
            {
                _rightButtonPressed = false;
                N = _oldN;
                _refreshFunctions();
            }
        }

        private void _MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.XButton2)
                xOnlyZoomMode = true;

            if (e.Button == MouseButtons.XButton1)
                yOnlyZoomMode = true;

            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Left)
                //TODO: find better usage for one of these buttons
            {
                _oldN = N;
                N = MOVE_N;
                _rightButtonPressed = true;
                _lastMouseLocation = e.Location;
            }
        }

        private void _MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
                AutoScaleHard();
        }

        private void _MouseClick(object sender, MouseEventArgs e)
        {
            //   if (e.Button == MouseButtons.Right)
            {
            }
            if (e.Button == MouseButtons.Middle)
                AutoScaleSmooth();
            if (e.Button == MouseButtons.XButton2)
                xOnlyZoomMode = true;
        }

        private void AutoScaleSmooth()
        {
            ChartAreas[0].AxisY.Minimum = double.NaN; //min;
            ChartAreas[0].AxisY.Maximum = double.NaN; //max;
            ChartAreas[0].RecalculateAxesScale(); //this.Update();
            XMinChanged?.Invoke(this, new EventArgs());
            XMaxChanged?.Invoke(this, new EventArgs());
            YMinChanged?.Invoke(this, new EventArgs());
            YMaxChanged?.Invoke(this, new EventArgs());
            NotifyPropertyChanged("XyRatio");
        }

        private void AutoScaleHard()
        {
            double Xmax = double.MinValue, Xmin = double.MaxValue;
            double Ymax = double.MinValue, Ymin = double.MaxValue;

            foreach (var serie in Series)
            {
                if (serie.Points.Count > 0)
                {
                    var f = serie.Points.Max(p => p.YValues.Max());
                    if (f > Ymax)
                        Ymax = f;

                    f = serie.Points.Min(p => p.YValues.Min());
                    if (f < Ymin)
                        Ymin = f;

                    f = serie.Points.Max(p => p.XValue);

                    if (f > Xmax)
                        Xmax = f;

                    f = serie.Points.Min(p => p.XValue);
                    if (f < Xmin)
                        Xmin = f;
                }
            }
            ChartAreas[0].AxisX.Minimum = Xmin;
            ChartAreas[0].AxisX.Maximum = Xmax;

            ChartAreas[0].AxisY.Minimum = Ymin; //min;
            ChartAreas[0].AxisY.Maximum = Ymax; //max;
            XMinChanged?.Invoke(this, new EventArgs());
            XMaxChanged?.Invoke(this, new EventArgs());
            YMinChanged?.Invoke(this, new EventArgs());
            YMaxChanged?.Invoke(this, new EventArgs());
            NotifyPropertyChanged("XyRatio");
        }

        private void _MouseLeave(object sender, EventArgs e)
        {
            if (Focused)
                Parent.Focus();
        }

        private void _MouseEnter(object sender, EventArgs e)
        {
            if (!Focused)
                Focus();
        }

        private void zoomIn()
        {
            var any = false;

            /*
             * >> floor(log10(756))
             * ans = 3
             */

            var xScale = Math.Pow(10, Math.Ceiling(Math.Log10(Math.Abs(XMax - XMin)/2.0) - 1));
            var yScale = Math.Pow(10, Math.Ceiling(Math.Log10(Math.Abs(YMax - YMin)/2.0) - 1));

            /*
             * distance ϵ (0,2) => scale = floor(log10(distance))
             * distance ϵ <2,200> => scale = 1.0
             * (0,2) => 
             */


            scalingFactor = Math.Pow(10, Math.Ceiling(Math.Log10(Math.Abs(XMax - XMin))));
            //(0.1*Math.Abs(XMax - XMin)).RoundToSignificantDigits(0);

            if ( /*Math.Abs(XMax - XMin) > 2 * scalingFactor &&*/ !yOnlyZoomMode)
            {
                ChartAreas[0].AxisX.Minimum += xScale;
                ChartAreas[0].AxisX.Maximum -= xScale;
                any = true;
            }
            if ( /*Math.Abs(YMax - YMin) > 2 * scalingFactor &&*/ !xOnlyZoomMode)
            {
                ChartAreas[0].AxisY.Minimum += yScale;
                ChartAreas[0].AxisY.Maximum -= yScale;
                any = true;
            }
            if (any)
            {
                _refreshFunctions();
                XMinChanged?.Invoke(this, new EventArgs());
                XMaxChanged?.Invoke(this, new EventArgs());
                YMinChanged?.Invoke(this, new EventArgs());
                YMaxChanged?.Invoke(this, new EventArgs());
            }
        }

        private double maxCeiling(double d)
        {
            var ceiling = Math.Ceiling(d);
            if (ceiling == d)
                return ceiling + 1;
            return ceiling;
        }

        private void zoomOut()
        {
            var xScale = Math.Pow(10, maxCeiling(Math.Log10(Math.Abs(XMax - XMin)/2.0) - 1));
            var yScale = Math.Pow(10, maxCeiling(Math.Log10(Math.Abs(YMax - YMin)/2.0) - 1));


            if (!yOnlyZoomMode)
            {
                ChartAreas[0].AxisX.Minimum -= xScale;
                ChartAreas[0].AxisX.Maximum += xScale;
                XMinChanged?.Invoke(this, new EventArgs());
                XMaxChanged?.Invoke(this, new EventArgs());
            }
            if (!xOnlyZoomMode)
            {
                ChartAreas[0].AxisY.Minimum -= yScale;
                ChartAreas[0].AxisY.Maximum += yScale;
                YMinChanged?.Invoke(this, new EventArgs());
                YMaxChanged?.Invoke(this, new EventArgs());
            }
            _refreshFunctions();
        }

        private void _MouseWheel(object s, MouseEventArgs e)
        {
            if (e.Delta > 0)
                zoomIn();
            else if (e.Delta < 0)
                zoomOut();
        }

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private void _refreshFunctions()
        {
            Series.Clear();

            double dx = (XMax - XMin) / N;
            double dy = (YMax - YMin) / N;

            if (dx <= 0 || dy <= 0)
            {
                Logger.Warn($"dx '{dx}' or dy '{dy}' less than 0. Chart will not be drawn.");
                return;
            }

            TOLERANCE = dx + dy;

            foreach (var fx in functions)
            {
                var series = new Series
                {
                    ChartType = fx.IsImplicit ? defaultImplicitFunctionsChartType : defaultExplicitFunctionsChartType,
                    MarkerSize = PointsSize,
                    BorderWidth = LineThickness,
                    Name = fx.Name
                };

                for (var x = XMin; x <= XMax; x += dx) //Parallel.ForEach(Iterate(XMin, XMax, dx).ToArray(), x =>
                {
                    if (fx.IsImplicit)
                    {
                        for (var y = YMin; y <= YMax; y += dy)
                        {
                            if (IsNearZero(fx.Evaluate(x, y)))
                                series.Points.AddXY(x, y);
                        }
                    }
                    else
                    {
                        var y = fx.Evaluate(x);
                        if (IsPointValid(x, y))
                            series.Points.AddXY(x, y);
                    }
                } //);
                Series.Add(series);
            }

            foreach (var point in points)
            {
                var series = new Series
                {
                    ChartType = SeriesChartType.FastPoint,
                    MarkerSize = PointsSize,
                    BorderWidth = LineThickness
                };

                foreach (var p in point)
                    series.Points.AddXY(p.x, p.y);

                Series.Add(series);
            }


            foreach (var s in Series)
                s.ToolTip = "x = #VALX\ny = #VALY";

            reloadChartSeriesComboBox();
            NotifyPropertyChanged("XyRatio");
        }

        private void _addNewFunction()
        {
            double dx =  (XMax- XMin )/ N, dy = (YMax - YMin) /N;

            if (dx <= 0 || dy <= 0)
            {
                Logger.Warn($"dx '{dx}' or dy '{dy}' less than 0. Chart will not be drawn.");
                return;
            }

            TOLERANCE = dx + dy;

            var series = new Series
            {
                ChartType =
                    functions.Last().IsImplicit ? defaultImplicitFunctionsChartType : defaultExplicitFunctionsChartType,
                MarkerSize = PointsSize,
                BorderWidth = LineThickness,
                Name = functions.Last().Name
            };

            double y;
            for (var x = XMin; x <= XMax; x += dx)
            {
                if (functions.Last().IsImplicit)
                {
                    for (y = YMin; y <= YMax; y += dy)
                    {
                        if (IsNearZero(functions.Last().Evaluate(x, y)))
                            series.Points.AddXY(x, y);
                    }
                }
                else
                {
                    y = functions.Last().Evaluate(x);
                    if (IsPointValid(x, y))
                        series.Points.AddXY(x, y);
                }
            }
            Series.Add(series);


            foreach (var s in Series)
                s.ToolTip = "x = #VALX\ny = #VALY";
            reloadChartSeriesComboBox();
        }

        private bool IsNearZero(double value)
        {
            return Math.Abs(value) < TOLERANCE;
        }

        private bool IsPointValid(double x, double y)
        {
            return !double.IsInfinity(x) && !double.IsNaN(x) && !double.IsInfinity(y) && !double.IsNaN(y) &&
                   !(x > OVERFLOW_VALUE) && !(y > OVERFLOW_VALUE) && !(x < UNDERFLOW_VALUE) && !(y < UNDERFLOW_VALUE);
        }

        public void setupComboBoxes(params ToolStripComboBox[] owners)
        {
            setupChartTypes(owners[0]);
            setupChartSeries(owners[1]);
            setupChartColors(owners[2]);
            setupChartLegendPositions(owners[3]);
            setupChartLegendAligments(owners[4]);
        }

        private void setupChartLegendAligments(ToolStripComboBox owner)
        {
            var items =
                Enum.GetValues(typeof(StringAlignment)).Cast<StringAlignment>().ToList();

            foreach (var v in items)
                owner.Items.Add(v.ToString());

            owner.DropDownStyle = ComboBoxStyle.DropDownList;
            owner.AutoSize = true;
            owner.SelectedItem = "Near";
        }

        private void setupChartLegendPositions(ToolStripComboBox owner)
        {
            //legend1.Alignment = System.Drawing.StringAlignment.Center;

            var items =
                Enum.GetValues(typeof(Docking))
                    .Cast<Docking>()
                    .ToList();

            foreach (var v in items)
                owner.Items.Add(v.ToString());

            owner.DropDownStyle = ComboBoxStyle.DropDownList;
            owner.AutoSize = true;
            owner.SelectedItem = "Right";
        }

        private void setupChartColors(ToolStripComboBox owner)
        {
            // Palette = ChartColorPalette.Berry;
            var items =
                Enum.GetValues(typeof(ChartColorPalette))
                    .Cast<ChartColorPalette>()
                    .ToList();

            foreach (var v in items)
                owner.Items.Add(v.ToString());

            owner.DropDownStyle = ComboBoxStyle.DropDownList;
            owner.AutoSize = true;

            owner.SelectedItem = "BrightPastel";
        }

        private void setupChartSeries(ToolStripComboBox owner)
        {
            owner.DropDownStyle = ComboBoxStyle.DropDownList;
            owner.Items.Add("All series");
            owner.SelectedIndex = 0;
            seriesComboBox = owner;
        }

        private void setupChartTypes(ToolStripComboBox owner)
        {
            var items =
                Enum.GetValues(typeof(SeriesChartType))
                    .Cast<SeriesChartType>()
                    .ToList();
            owner.Items.Clear();

            foreach (var v in items)
                owner.Items.Add(v.ToString());

            owner.DropDownStyle = ComboBoxStyle.DropDownList;
            owner.AutoSize = true;

            owner.SelectedItem = defaultExplicitFunctionsChartType.ToString();
            //  chartType = SeriesChartType.FastLine;
        }

        public void AddDataPoints(IEnumerable<double> y, IEnumerable<double> x)
        {
            AddDataPoints(y.ToList(),x.ToList());
        }

        public void AddDataPoints(List<double> y, List<double> x) //TODO: fix bugs in scripting or revert to old version
        {
            var visibleLegend = true;
            if (Series.Count > 0)
                visibleLegend = Series[0].IsVisibleInLegend;

            var seriesNames = new List<string>();

            foreach (var serieName in Series)
                seriesNames.Add(serieName.LegendText);

            Series.Add("No " + (Series.Count + 1));

            points.Add(new List<Point2D>());

            for (var i = 0; i < y.Count; i++)
            {
                Series.Last().Points.AddXY(x[i], y[i]);
                points.Last().Add(new Point2D(x[i], y[i]));
                Series.Last().ChartType = SeriesChartType.FastPoint;
                Series.Last().ToolTip = "x=#VALX\ny=#VALY";
                Series.Last().Font = new Font("Times New Roman", 2.0f);
                if (Series.Count > 0)
                    Series.Last().IsVisibleInLegend = visibleLegend;
                if (seriesNames.Count >= i + 1)
                    Series.Last().LegendText = seriesNames[i];
            }

            ChartAreas[0].AxisY.Title = "Y";
            ChartAreas[0].AxisX.Title = "X";
            //Titles[0].Text = "Wykres 1";
        }

        /*  public void saveImage()
        {
            
            var dialog = new SaveFileDialog();
            dialog.Filter = "Portable Network Graphics (*.png)|*.png";
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SaveImage(dialog.FileName, ImageFormat.Png);
            }
        }*/

        #region reloaders

        private void reloadChartSeriesComboBox()
        {
            if (seriesComboBox != null)
            {
                seriesComboBox.Items.Clear();
                seriesComboBox.Items.Add("All series");
                foreach (var serie in Series)
                    seriesComboBox.Items.Add(serie.Name);
                seriesComboBox.SelectedIndex = 0;
            }
        }

        public void ShowEditDialog()
        {
            var editChartWindow = new EditChartWindow(this);
            editChartWindow.ShowDialog();
        }

        public StringAlignment LegendAlignment {
            get { return Legends[0].Alignment; }
            set { Legends[0].Alignment = value; } }
        public Docking LegendDocking
        {
            get { return Legends[0].Docking; }
            set { Legends[0].Docking = value; }
        }

        #endregion
    }

    public static class DoubleExtensions
    {
        public static double RoundToSignificantDigits(this double d, int digits)
        {
            if (d == 0)
                return 0;

            var scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
            return scale*Math.Round(d/scale, digits);
        }
    }
}