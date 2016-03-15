using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Computator.NET.DataTypes;

namespace Computator.NET.Charting.RealCharting
{
    public class Chart2D : Chart, IChart /*, IChart<double>*/, INotifyPropertyChanged
    {
        private const double OVERFLOW_VALUE = (double)decimal.MaxValue/10;//1073741951.0/500; //1111117;
        private const double UNDERFLOW_VALUE = (double) decimal.MinValue/10;//-1073741760.0/500; // 1111117;
        private const int MOVE_N = 100;
        private readonly List<Function> functions;
        // private readonly List<Function<double>> implicitFunctions;
        private readonly List<List<Point2D>> points;
        private Point _lastMouseLocation;
        private int _lineThickness;
        private double _oldN;
        private int _pointsSize;
        private bool _rightButtonPressed;
        private SeriesChartType defaultExplicitFunctionsChartType = SeriesChartType.Line;//SeriesChartType.FastLine;
        private SeriesChartType defaultImplicitFunctionsChartType = SeriesChartType.FastPoint;
        private double scalingFactor;
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
            functions = new List<Function>();
            MouseDoubleClick += _MouseDoubleClick;
            MouseClick += _MouseClick;
            MouseWheel += _MouseWheel;
            MouseDown += _MouseDown;
            MouseUp += _MouseUp;
            MouseMove += _MouseMove;
            Resize += (o, e) => NotifyPropertyChanged("XyRatio");
         //   defaultExplicitFunctionsChartType = SeriesChartType.Line;//////////////////////////////////
            scalingFactor = 1;
            xOnlyZoomMode = yOnlyZoomMode = false;
            initChart();
            AxesEqual = true;
            N = 3571;
            _pointsSize = 2;
            _lineThickness = 2;
            points = new List<List<Point2D>>();
            //implicitFunctions= new List<Function<double>>();
            //   addSampleImplicitFunction();
            //implicitFunctions.Add(new Function<double>((x, y) => Math.Sin(x*x-y*y) - Math.Sin(x+y)-Math.Cos(x*y), "|sin(x²-y²)| = sin(x+y)+cos(x·y)"));
        }

        public int lineThickness
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

        public int pointsSize
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

        public double XMin
        {
            get { return ChartAreas[0].AxisX.Minimum; }
            set
            {                if (value < UNDERFLOW_VALUE)
                    value = UNDERFLOW_VALUE;

                if (value > OVERFLOW_VALUE)
                    value = OVERFLOW_VALUE;
                if (ChartAreas[0].AxisX.Minimum == value) return;



                ChartAreas[0].AxisX.Minimum = value;
                NotifyPropertyChanged("XMin");
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
            {                if (value < UNDERFLOW_VALUE)
                    value = UNDERFLOW_VALUE;

                if (value > OVERFLOW_VALUE)
                    value = OVERFLOW_VALUE;
                if (ChartAreas[0].AxisX.Maximum == value) return;




                ChartAreas[0].AxisX.Maximum = value;
                NotifyPropertyChanged("XMax");
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
                NotifyPropertyChanged("YMin");
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
                NotifyPropertyChanged("YMax");
                _refreshFunctions();
            }
        }

        private double N { get; set; }

        public double Quality
        {
            set
            {
                if (value <= 100 && value >= 0)
                {
                    calculateN(value);
                    _refreshFunctions();
                }
            }
        }

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
        public bool AxesEqual { get; set; }

        public double XyRatio
        {
            get
            {
                if (functions == null || functions.Count == 0)
                    return 1.0;
                return ((XMax - XMin)/
                        Math.Abs(ChartAreas[0].AxisX.ValueToPixelPosition(XMax) -
                                 ChartAreas[0].AxisX.ValueToPixelPosition(XMin)))/
                       ((YMax - YMin)/
                        Math.Abs(ChartAreas[0].AxisY.ValueToPixelPosition(YMax) -
                                 ChartAreas[0].AxisY.ValueToPixelPosition(YMin)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static IEnumerable<double> Iterate(
            double fromInclusive, double toExclusive, double step)
        {
            for (var d = fromInclusive; d <= toExclusive; d += step) yield return d;
        }

        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
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
            if (elements == null || elements.Object == null)
                return;

            var deltaX = (ChartAreas[0].AxisX.PixelPositionToValue(_lastMouseLocation.X) -
                          ChartAreas[0].AxisX.PixelPositionToValue(e.Location.X));

            var deltaY = (ChartAreas[0].AxisY.PixelPositionToValue(_lastMouseLocation.Y) -
                          ChartAreas[0].AxisY.PixelPositionToValue(e.Location.Y));


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

        public void SetChartAreaValues(double x0, double xn, double y0, double yn)
        {
            ChartAreas[0].AxisX.Minimum = x0;
            ChartAreas[0].AxisX.Maximum = xn;

            ChartAreas[0].AxisY.Minimum = y0; //min;
            ChartAreas[0].AxisY.Maximum = yn; //max;
        }

        private void Refresh2()
        {
            NotifyPropertyChanged("XMin");
            NotifyPropertyChanged("XMax");
            NotifyPropertyChanged("YMin");
            NotifyPropertyChanged("YMax");
            _refreshFunctions();
        }

        private void initChart()
        {
            var chartArea1 = new ChartArea();
            var legend1 = new Legend();
            var title1 = new Title();
            chartArea1.AxisX.ArrowStyle = AxisArrowStyle.Lines;
            chartArea1.AxisX.Crossing = 0D;
            chartArea1.AxisX.InterlacedColor = Color.White;
            chartArea1.AxisX.IsLabelAutoFit = false;
            chartArea1.AxisX.LabelAutoFitMaxFontSize = 13;
            chartArea1.AxisX.LabelStyle.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular,
                GraphicsUnit.Point, 238);
            chartArea1.AxisX.LineWidth = 2;
            chartArea1.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            chartArea1.AxisX.MajorTickMark.Size = 2F;
            chartArea1.AxisX.MajorTickMark.TickMarkStyle = TickMarkStyle.AcrossAxis;
            chartArea1.AxisX.MinorGrid.Enabled = true;
            chartArea1.AxisX.MinorGrid.LineColor = Color.DarkGray;
            chartArea1.AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
            chartArea1.AxisX.MinorTickMark.Enabled = true;
            chartArea1.AxisX.MinorTickMark.TickMarkStyle = TickMarkStyle.AcrossAxis;
            chartArea1.AxisX.Title = "X";
            chartArea1.AxisX.TitleAlignment = StringAlignment.Far;
            chartArea1.AxisX.TitleFont = new Font("Microsoft Sans Serif", 18F, FontStyle.Bold, GraphicsUnit.Point, 238);
            chartArea1.AxisY.ArrowStyle = AxisArrowStyle.Lines;
            chartArea1.AxisY.Crossing = 0D;
            chartArea1.AxisY.InterlacedColor = Color.White;
            chartArea1.AxisY.IsLabelAutoFit = false;
            chartArea1.AxisY.LabelAutoFitMaxFontSize = 13;
            chartArea1.AxisY.LabelStyle.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular,
                GraphicsUnit.Point, 238);
            chartArea1.AxisY.LineWidth = 2;
            chartArea1.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            chartArea1.AxisY.MajorTickMark.Size = 2F;
            chartArea1.AxisY.MajorTickMark.TickMarkStyle = TickMarkStyle.AcrossAxis;
            chartArea1.AxisY.MinorGrid.Enabled = true;
            chartArea1.AxisY.MinorGrid.LineColor = Color.DarkGray;
            chartArea1.AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
            chartArea1.AxisY.MinorTickMark.Enabled = true;
            chartArea1.AxisY.MinorTickMark.TickMarkStyle = TickMarkStyle.AcrossAxis;
            chartArea1.AxisY.TextOrientation = TextOrientation.Horizontal;
            chartArea1.AxisY.Title = "Y";
            chartArea1.AxisY.TitleAlignment = StringAlignment.Far;
            chartArea1.AxisY.TitleFont = new Font("Microsoft Sans Serif", 18F, FontStyle.Bold, GraphicsUnit.Point, 238);
            chartArea1.CursorX.IsUserEnabled = true;
            chartArea1.CursorX.IsUserSelectionEnabled = true;
            chartArea1.CursorY.IsUserEnabled = true;
            chartArea1.CursorY.IsUserSelectionEnabled = true;
            chartArea1.Name = "ChartArea1";
            ChartAreas.Add(chartArea1);
            legend1.Font = new Font("Cambria", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 238);
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
        }

        private void _MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.XButton2)
                xOnlyZoomMode = false;

            if (e.Button == MouseButtons.XButton1)
                yOnlyZoomMode = false;

            if (e.Button == MouseButtons.Right)
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

            if (e.Button == MouseButtons.Right)
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
                autoScaleHard();
        }

        private void _MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
                autoScaleSmooth();
            if (e.Button == MouseButtons.XButton2)
                xOnlyZoomMode = true;
        }

        private void autoScaleSmooth()
        {
            ChartAreas[0].AxisY.Minimum = double.NaN; //min;
            ChartAreas[0].AxisY.Maximum = double.NaN; //max;
            ChartAreas[0].RecalculateAxesScale(); //this.Update();
            NotifyPropertyChanged("XMin");
            NotifyPropertyChanged("XMax");
            NotifyPropertyChanged("YMin");
            NotifyPropertyChanged("YMax");
            NotifyPropertyChanged("XyRatio");
        }

        private void autoScaleHard()
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
            NotifyPropertyChanged("XMin");
            NotifyPropertyChanged("XMax");
            NotifyPropertyChanged("YMin");
            NotifyPropertyChanged("YMax");
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
                NotifyPropertyChanged("XMin");
                NotifyPropertyChanged("XMax");
                NotifyPropertyChanged("YMin");
                NotifyPropertyChanged("YMax");
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
                NotifyPropertyChanged("XMin");
                NotifyPropertyChanged("XMax");
            }
            if (!xOnlyZoomMode)
            {
                ChartAreas[0].AxisY.Minimum -= yScale;
                ChartAreas[0].AxisY.Maximum += yScale;
                NotifyPropertyChanged("YMin");
                NotifyPropertyChanged("YMax");
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

        public void addFx(Function f)
        {
            if (!Series.IsUniqueName(f.Name)) //nothing new to add
                return;

            functions.Add(f);
            _addNewFunction(f.IsImplicit);
        }

        public void Print()
        {
            Printing.Print(true);
        }

        public void PrintPreview()
        {
            Printing.PrintPreview();
        }

        public void addFx(Func<double, double> f, string name)
        {
            if (!Series.IsUniqueName(name)) //nothing new to add
                return;

            functions.Add(new Function(Delegate.CreateDelegate(typeof (Func<double, double>), f.Method), name, name));
            _addNewFunction(false);
        }

        private void _refreshFunctions()
        {
            Series.Clear();

            double dx = (Math.Abs(XMin - XMax))/N, dy = (Math.Abs(YMin - YMax))/(N);
            ;
            TOLERANCE = dx + dy;

            foreach (var fx in functions)
            {
                var series = new Series
                {
                    ChartType = fx.IsImplicit ? defaultImplicitFunctionsChartType : defaultExplicitFunctionsChartType,
                    MarkerSize = pointsSize,
                    BorderWidth = lineThickness,
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
                    MarkerSize = pointsSize,
                    BorderWidth = lineThickness
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

        private void _addNewFunction(bool isImplicit)
        {
            double dx = (Math.Abs(XMin - XMax))/N, dy = (Math.Abs(YMin - YMax))/(N);
            ;
            TOLERANCE = dx + dy;

            var series = new Series
            {
                ChartType = functions.Last().IsImplicit ? defaultImplicitFunctionsChartType : defaultExplicitFunctionsChartType,
                MarkerSize = pointsSize,
                BorderWidth = lineThickness,
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
            return (Math.Abs(value) < TOLERANCE);
        }

        private bool IsPointValid(double x, double y)
        {
            return !double.IsInfinity(x) && !double.IsNaN(x) && !double.IsInfinity(y) && !double.IsNaN(y) &&
                   !(x > OVERFLOW_VALUE) && !(y > OVERFLOW_VALUE) && !(x < UNDERFLOW_VALUE) && !(y < UNDERFLOW_VALUE);
        }

        public void addPoints(List<Point2D> points, string name = "", bool newPoints = false)
        {
            var series = new Series();
            if (name != "")
                series.Name = name;

            series.ChartType = SeriesChartType.FastPoint;
            ;

            foreach (var p in points)
                series.Points.AddXY(p.x, p.y);

            if (newPoints)
                Series.Clear();

            Series.Add(series);

            foreach (var s in Series)
                s.ToolTip = "x = #VALX\ny = #VALY";

            reloadChartSeriesComboBox();
            this.points.Add(points);
        }

        //exp(x/20)*(sin(1/2*x)+cos(3*x)+0.2*sin(4*x)*cos(40*x))

        public void ClearAll()
        {
            Series.Clear();
            functions.Clear();
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
                Enum.GetValues(typeof (StringAlignment)).Cast<StringAlignment>().ToList<StringAlignment>();

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
                Enum.GetValues(typeof (Docking))
                    .Cast<Docking>()
                    .ToList<Docking>();

            foreach (var v in items)
                owner.Items.Add(v.ToString());

            owner.DropDownStyle = ComboBoxStyle.DropDownList;
            owner.AutoSize = true;
            owner.SelectedItem = "Right";
        }

        private void setupChartColors(ToolStripComboBox owner)
        {
            Palette = ChartColorPalette.Berry;
            var items =
                Enum.GetValues(typeof (ChartColorPalette))
                    .Cast<ChartColorPalette>()
                    .ToList<ChartColorPalette>();

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
                Enum.GetValues(typeof (SeriesChartType))
                    .Cast<SeriesChartType>()
                    .ToList<SeriesChartType>();
            owner.Items.Clear();

            foreach (var v in items)
                owner.Items.Add(v.ToString());

            owner.DropDownStyle = ComboBoxStyle.DropDownList;
            owner.AutoSize = true;

            owner.SelectedItem = defaultExplicitFunctionsChartType.ToString();
            //  chartType = SeriesChartType.FastLine;
        }

        private void loadChartData(List<List<double>> Y, List<double> t, int mode = 0, bool firstTime = false,
            bool newLoad = true)
        {
            var visibleLegend = true;
            if (Series.Count > 0)
                visibleLegend = Series[0].IsVisibleInLegend;

            var seriesNames = new List<string>();

            foreach (var serieName in Series)
                seriesNames.Add(serieName.LegendText);

            Series.Clear();

            for (var i = 0; i < Y.Count; i++)
            {
                if (mode == 0 || i == mode - 1)
                {
                    Series.Add("Seria " + (i + 1));
                    for (var j = 0; j < Y[i].Count; j++)
                    {
                        Series.Last().Points.AddXY(t[j], Y[i][j]);
                        Series.Last().ChartType = defaultExplicitFunctionsChartType;
                        Series.Last().ToolTip = "x=#VALX\ny=#VAL";
                        Series.Last().Font = new Font("Times New Roman", 2.0f);
                        if (Series.Count > 0)
                            Series.Last().IsVisibleInLegend = visibleLegend;
                        if (seriesNames.Count >= i + 1)
                            Series.Last().LegendText = seriesNames[i];
                    }
                }
            }

            if (firstTime)
            {
                /*owner.Items.Clear();
                owner.Items.Add("Wszystkie");
                foreach (var serie in this.Series)
                    owner.Items.Add(serie);
                owner.SelectedIndex = 0;*/

                ChartAreas[0].AxisY.Title = "Y";
                ChartAreas[0].AxisX.Title = "X";
                Titles[0].Text = "Wykres 1";
            }
            if (newLoad)
                autoScaleSmooth();
        }

        public void addChartDataPoints(List<double> y, List<double> x)
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

        public void saveImage()
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Portable Network Graphics (*.png)|*.png";
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SaveImage(dialog.FileName, ImageFormat.Png);
            }
        }

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

        #endregion

        #region changeMethods

        public void changeChartLegendPosition(string chartLegendPosition)
        {
            Legends[0].Docking = (Docking) Enum.Parse(typeof (Docking), chartLegendPosition);
        }

        public void changeChartLegendAligment(string chartLegendAligment)
        {
            Legends[0].Alignment = (StringAlignment) Enum.Parse(typeof (StringAlignment), chartLegendAligment);
        }

        public void changeChartType(string chartType)
        {
            this.defaultExplicitFunctionsChartType =
                (SeriesChartType) Enum.Parse(typeof (SeriesChartType), chartType);
            for (var i = 0; i < Series.Count; i++)
                Series[i].ChartType =
                    (SeriesChartType) Enum.Parse(typeof (SeriesChartType), chartType);
        }

        public void changeChartColor(string chartColor)
        {
            Palette = (ChartColorPalette) Enum.Parse(typeof (ChartColorPalette), chartColor);
        }

        public void changeSeries(string serie)
        {
            if (serie == "Wszystkie serie" || serie == "All series")
                foreach (var s in Series)
                    s.Enabled = true;
            else
            {
                foreach (var s in Series)
                    s.Enabled = false;
                Series[serie].Enabled = true;
            }
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