using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Computator.NET.Charting.Chart3D.Splines;
using Computator.NET.DataTypes;

namespace Computator.NET.Charting.Chart3D
{
    /// <summary>
    ///     Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Chart3DControl : UserControl, IChart
    {
        private readonly List<Function> functions;
        private Color _axesColor;
        private double _dotSize;
        private bool _equalAxes;
        private double _scale;
        private uint _splineSmothness;
        private bool _useSpline;
        private bool _visibilityAxes;
        private AxisLabels axisLabels;
        private bool firstTimeAdd = true;
        private Chart3D m_3dChart; // data for 3d chart
        public int m_nChartModelIndex = -1; // model index in the Viewport3d
        public TransformMatrix m_transformMatrix = new TransformMatrix();
        private Chart3DMode mode;
        private double N;
        private double xmax;
        private double xmin;
        private double ymax;
        private double ymin;

        public Chart3DControl()
        {
            InitializeComponent();

            axisLabels = new AxisLabels(canvasOn3D);
            functions = new List<Function>();

            _visibilityAxes = _equalAxes = true;
            Focusable = true;
            _axesColor = Colors.MediumSlateBlue;

            _scale = 0.5;
            _dotSize = 0.02;
            xmax = ymax = 5;
            xmin = ymin = -5;
            N = 100;

            Mode = Chart3DMode.Surface;

            MouseDown += OnViewportMouseDown;
            MouseMove += OnViewportMouseMove;
            MouseUp += OnViewportMouseUp;
            // this.MouseEnter += new MouseEventHandler(Chart3DControl_MouseEnter);
            MouseWheel += OnViewportMouseWheel;
            KeyDown += OnKeyDown;
        }

        public double Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                rescaleProjectionMatrix();
            }
        }

        ///
        public double DotSize
        {
            get { return _dotSize*0.1*0.5*(Math.Abs(xmax - xmin) + Math.Abs(ymax - ymin)); }
            set
            {
                _dotSize = value;
                reloadPoints();
            }
        }

        ///
        public Color AxesColor
        {
            get { return _axesColor; }
            set
            {
                _axesColor = value;
                reloadPoints();
            }
        }

        public bool EqualAxes
        {
            get { return _equalAxes; }
            set
            {
                _equalAxes = value;
                rescaleProjectionMatrix();
            }
        }

        ///
        public bool VisibilityAxes
        {
            get { return _visibilityAxes; }
            set
            {
                _visibilityAxes = value;
                if (m_3dChart != null) m_3dChart.UseAxes = value;
                reloadPoints();
            }
        }

        public AxisLabels AxisLabels
        {
            get { return axisLabels; }
            set
            {
                axisLabels = value;
                TransformChart();
            }
        }

        public double XMin
        {
            get { return xmin; }
            set
            {
                if (value != xmin)
                {
                    xmin = value;
                    Redraw();
                }
            }
        }



        public double XMax
        {
            get { return xmax; }
            set
            {
                if (value != xmax)
                {
                    xmax = value;
                    Redraw();
                }
            }
        }

        public double YMin
        {
            get { return ymin; }
            set
            {
                if (value != ymin)
                {
                    ymin = value;
                    Redraw();
                }
            }
        }

        public double YMax
        {
            get { return ymax; }
            set
            {
                if (value != ymax)
                {
                    ymax = value;
                    Redraw();
                }
            }
        }

        public double Quality
        {
            set
            {
                if (value <= 100.0 && value >= 0.0)
                {
                    calculateN(value);
                    Redraw();
                }
            }
        }

        public Chart3DMode Mode
        {
            get { return mode; }
            set
            {
                if (value != mode)
                {
                    mode = value;
                    if (value == Chart3DMode.Points)
                        TestScatterPlot(1);
                    else if (value == Chart3DMode.Surface)
                        TestSurfacePlot(1);
                    TransformChart();
                    Redraw();
                }
            }
        }

        private void Chart3DControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Focus();
        }

        private void calculateN(double value)
        {
            if (value <= 50.0)
            {
                N = (int) (1.5*value + 25);
            }
            else
                N = 100 + (int) value;

            if (mode == Chart3DMode.Surface)
                N += 25;
        }

        public void Clear()
        {
            functions.Clear();

            if (mode == Chart3DMode.Points)
                TestScatterPlot(1);
            else if (mode == Chart3DMode.Surface)
                TestSurfacePlot(1);
            axisLabels.Remove();
            TransformChart();
            firstTimeAdd = true;
        }

        public void addFx(Function fxy)
        {

            functions.Add(fxy);
            Redraw();
        }

        public void addFx(Func<double,double,double> fxy, double n)
        {
            N = n;
            functions.Add(new Function(fxy,"function1","function1"));
            Redraw();
        }

        public void Print()
        {
            throw new NotImplementedException();
        }

        public void PrintPreview()
        {
            throw new NotImplementedException();
        }

        public void SetChartAreaValues(double x0, double xn, double y0, double yn)
        {
            xmax = xn;
            xmin = x0;
            ymax = yn;
            ymin = y0;
            Redraw();
        }

        public void SaveImage(string path, ImageFormat imageFormat)
        {
            BitmapEncoder encoder;

            if(Equals(imageFormat, ImageFormat.Png))
                encoder= new PngBitmapEncoder();
            else if(Equals(imageFormat, ImageFormat.Bmp))
                encoder = new BmpBitmapEncoder();
            else if (Equals(imageFormat, ImageFormat.Gif))
                encoder = new GifBitmapEncoder();
            else if (Equals(imageFormat, ImageFormat.Jpeg))
                encoder = new JpegBitmapEncoder();
            else if (Equals(imageFormat, ImageFormat.Tiff))
                encoder = new TiffBitmapEncoder();
            else if (Equals(imageFormat, ImageFormat.Wmf))
                encoder = new WmpBitmapEncoder();
            else
                encoder = new PngBitmapEncoder();


            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)this.ActualWidth, (int)this.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(this);
            BitmapFrame frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);

            using (var stream = File.Create(path))
            {
                encoder.Save(stream);
            }
        }





        public void Redraw()
        {
            firstTimeAdd = true;
            foreach (var f in functions)
                drawFunction((x, y) => f.Evaluate(x, y), xmin, xmax, ymin, ymax, N);
        }

        private void drawFunction(Func<double, double, double> fxy, double XMin, double XMax, double YMin, double YMax,
            double N = 1e2)
        {
            if (mode == Chart3DMode.Surface)
            {
                AddSurface(fxy, XMin, XMax, YMin, YMax, N);
                return;
            }

            double dx = (XMax - XMin)/N, dy = (YMax - YMin)/N;
            double x, y, z;
            var spline3d = new Spline3D();
            for (var ix = 0; ix <= N; ix++)
                for (var iy = 0; iy <= N; iy++)
                {
                    x = XMin + ix*dx;
                    y = YMin + iy*dy;
                    z = fxy(x, y);
                    if (!double.IsNaN(z) && !double.IsInfinity(z))
                        spline3d.addPoint(new Point3D(x, y, z));
                }

            var rnd = new Random();


            if (firstTimeAdd)
            {
                SetData(spline3d,
                    new Color {R = (byte) rnd.Next(0, 256), G = (byte) rnd.Next(0, 256), B = (byte) rnd.Next(0, 256)});
                firstTimeAdd = false;
            }
            else
                AddData(spline3d,
                    new Color {R = (byte) rnd.Next(0, 256), G = (byte) rnd.Next(0, 256), B = (byte) rnd.Next(0, 256)});
        }

        private void reloadPoints() //changeDotSize, change
        {
            if (m_3dChart == null)
                return;

            if (mode == Chart3DMode.Points)
            {
                // 2. set the properties of each dot
                for (var i = 0; i < m_3dChart.GetDataNo(); i++)
                {
                    var plotItem = ((ScatterChart3D) m_3dChart).GetVertex(i);

                    plotItem.w = (float) DotSize; //size of plotItem
                    plotItem.h = (float) DotSize; //size of plotItem

                    ((ScatterChart3D) m_3dChart).SetVertex(i, plotItem);
                }
            }
            m_3dChart.UseAxes = VisibilityAxes;
            m_3dChart.SetAxesColor(AxesColor);
            // 3. set axes
            m_3dChart.GetDataRange();
            m_3dChart.SetAxes();

            // 4. Get Mesh3D array from scatter plot
            ArrayList meshs = null;
            if (mode == Chart3DMode.Points)
                meshs = ((ScatterChart3D) m_3dChart).GetMeshes();
            else if (mode == Chart3DMode.Surface)
                meshs = ((UniformSurfaceChart3D) m_3dChart).GetMeshes();

            // 5. display vertex no and triangle no.
            UpdateModelSizeInfo(meshs);

            // 6. show 3D scatter plot in Viewport3d
            var model3d = new Model3D();

            Material backMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.DimGray));

            if (mode == Chart3DMode.Points)
                m_nChartModelIndex = model3d.UpdateModel(meshs, null, m_nChartModelIndex, mainViewport);
            else if (mode == Chart3DMode.Surface)
                m_nChartModelIndex = model3d.UpdateModel(meshs, backMaterial, m_nChartModelIndex, mainViewport);
            // 7. set projection matrix
            rescaleProjectionMatrix();
            TransformChart();
        }

        private void AddSurface(Func<double, double, double> fxy, double XMin, double XMax, double YMin, double YMax,
            double N = 1e2)
        {
            ((UniformSurfaceChart3D) m_3dChart).SetGrid((int) N, (int) N, (float) XMin, (float) XMax, (float) YMin,
                (float) YMax);

            // 2. set surface chart z value
            var oldSize = 0;
            var nVertNo = (int) (N*N);

            //m_3dChart.IncreaseDataSize(nVertNo);

            for (var i = 0; i < nVertNo; i++)
            {
                var vert = m_3dChart[oldSize + i];

                var z = fxy(vert.x, vert.y);
                //if (!double.IsNaN(z) && !double.IsInfinity(z))
                m_3dChart[oldSize + i].z = (float) z;
            }
            m_3dChart.GetDataRange();

            // 3. set the surface chart color according to z vaule
            double zMin = m_3dChart.ZMin();
            double zMax = m_3dChart.ZMax();
            for (var i = 0; i < nVertNo; i++)
            {
                var vert = m_3dChart[oldSize + i];
                var h = (vert.z - zMin)/(zMax - zMin);


                var color = TextureMapping.PseudoColor(h);
                m_3dChart[oldSize + i].color = color;

                if (double.IsInfinity(vert.z) || double.IsNaN(vert.z))
                {
                    vert.z = 0;
                    // MessageBox.Show("chuj2");
                }
            }

            m_3dChart.UseAxes = VisibilityAxes;
            m_3dChart.SetAxesColor(AxesColor);
            // 3. set axes
            m_3dChart.GetDataRange();
            m_3dChart.SetAxes();

            // 4. Get Mesh3D array from scatter plot
            var meshs = ((UniformSurfaceChart3D) m_3dChart).GetMeshes();
            //ArrayList meshs = (((Computator.NET.Charting.Chart3D.ScatterChart3D)m_3dChart).GetMeshes());

            // 5. display vertex no and triangle no.
            UpdateModelSizeInfo(meshs);

            // 6. show 3D scatter plot in Viewport3d
            var model3d = new Model3D();
            Material backMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.DimGray));
            m_nChartModelIndex = model3d.UpdateModel(meshs, backMaterial, m_nChartModelIndex, mainViewport);
            //m_nChartModelIndex = model3d.UpdateModel(meshs, null, m_nChartModelIndex, this.mainViewport);

            // 7. set projection matrix
            rescaleProjectionMatrix();
            TransformChart();
        }

        private void AddData(Spline3D spline3D, Color color)
        {
            var points = spline3D.getPoints();
            var oldSize = m_3dChart.GetDataNo();

            if (_useSpline)
                m_3dChart.IncreaseDataSize(points.Count + (int) (_splineSmothness));
            else
                m_3dChart.IncreaseDataSize(points.Count);

            // 2. set the properties of each dot
            for (var i = 0; i < points.Count; i++)
            {
                var plotItem = new ScatterPlotItem();

                plotItem.w = (float) DotSize; //size of plotItem
                plotItem.h = (float) DotSize; //size of plotItem

                plotItem.x = (float) points[i].X;
                plotItem.y = (float) points[i].Y;
                plotItem.z = (float) points[i].Z;
                plotItem.shape = (int) Chart3D.SHAPE.ELLIPSE;

                plotItem.color = color;
                ((ScatterChart3D) m_3dChart).SetVertex(oldSize + i, plotItem);
            }


            if (_useSpline)
            {
                Point3D p1, p2;
                double h, deltaX, deltaY, deltaZ, aX, aZ;

                for (var i = 0; i < _splineSmothness; i++)
                {
                    p1 = spline3D.getPoint(i/(double) (_splineSmothness));
                    p2 = spline3D.getPoint((i + 1)/(double) (_splineSmothness));
                    deltaX = p2.X - p1.X;
                    deltaY = p2.Y - p1.Y;
                    deltaZ = p2.Z - p1.Z;
                    h = Math.Sqrt(deltaX*deltaX + deltaY*deltaY + deltaZ*deltaZ);

                    aX = Math.Atan2(deltaY, deltaX);
                    aZ = Math.Atan2(deltaX, deltaZ);

                    var plotItem = new ScatterPlotItem();

                    plotItem.w = (float) DotSize; //size of plotItem
                    plotItem.h = (float) h; //size of plotItem

                    plotItem.x = (float) (0.5*(p1.X + p2.X));
                    plotItem.y = (float) (0.5*(p1.Y + p2.Y));
                    plotItem.z = (float) (0.5*(p1.Z + p2.Z));
                    plotItem.shape = (int) Chart3D.SHAPE.CYLINDER;

                    plotItem.aX = aX + 90;
                    plotItem.aZ = aZ;


                    if (i == 0)
                        plotItem.color = Colors.White;
                    else if (i == _splineSmothness - 1)
                        plotItem.color = Colors.Aqua;
                    else
                        plotItem.color = color; ////
                    ((ScatterChart3D) m_3dChart).SetVertex(oldSize + points.Count + i, plotItem);
                }
            }

            m_3dChart.UseAxes = VisibilityAxes;
            m_3dChart.SetAxesColor(AxesColor);
            // 3. set axes
            m_3dChart.GetDataRange();
            m_3dChart.SetAxes();

            // 4. Get Mesh3D array from scatter plot
            var meshs = (((ScatterChart3D) m_3dChart).GetMeshes());

            // if (_useSpline)
            //  drawSpline(points, color, meshs);

            // 5. display vertex no and triangle no.
            UpdateModelSizeInfo(meshs);

            // 6. show 3D scatter plot in Viewport3d
            var model3d = new Model3D();
            m_nChartModelIndex = model3d.UpdateModel(meshs, null, m_nChartModelIndex, mainViewport);

            // 7. set projection matrix
            rescaleProjectionMatrix();
            TransformChart();
        }

        private void SetData(Spline3D spline3D, Color color)
        {
            var points = spline3D.getPoints();
            var oldSize = 0;

            if (_useSpline)
                m_3dChart.SetDataNo(points.Count + (int) (_splineSmothness));
            else
                m_3dChart.SetDataNo(points.Count);

            // 2. set the properties of each dot
            for (var i = 0; i < points.Count; i++)
            {
                var plotItem = new ScatterPlotItem();

                plotItem.w = (float) DotSize; //size of plotItem
                plotItem.h = (float) DotSize; //size of plotItem

                plotItem.x = (float) points[i].X;
                plotItem.y = (float) points[i].Y;
                plotItem.z = (float) points[i].Z;
                plotItem.shape = (int) Chart3D.SHAPE.ELLIPSE;

                plotItem.color = color;
                ((ScatterChart3D) m_3dChart).SetVertex(oldSize + i, plotItem);
            }



            m_3dChart.UseAxes = VisibilityAxes;
            m_3dChart.SetAxesColor(AxesColor);
            // 3. set axes
            m_3dChart.GetDataRange();
            m_3dChart.SetAxes();

            // 4. Get Mesh3D array from scatter plot
            var meshs = (((ScatterChart3D) m_3dChart).GetMeshes());

            // 5. display vertex no and triangle no.
            UpdateModelSizeInfo(meshs);

            // 6. show 3D scatter plot in Viewport3d
            var model3d = new Model3D();
            m_nChartModelIndex = model3d.UpdateModel(meshs, null, m_nChartModelIndex, mainViewport);

            // 7. set projection matrix
            rescaleProjectionMatrix();
            TransformChart();
        }

        public void rescaleProjectionMatrix()
        {
            if (EqualAxes)
                m_transformMatrix.CalculateProjectionMatrix(min(m_3dChart.XMin(), m_3dChart.YMin(), m_3dChart.ZMin()),
                    max(m_3dChart.XMax(), m_3dChart.YMax(), m_3dChart.ZMax()), Scale);
            else
                m_transformMatrix.CalculateProjectionMatrix(m_3dChart.XMin(), m_3dChart.XMax(), m_3dChart.YMin(),
                    m_3dChart.YMax(), m_3dChart.ZMin(), m_3dChart.ZMax(), Scale);
            TransformChart();
        }

        private float max(float a, float b, float c)
        {
            if (a > b && a > c)
                return a;
            if (c > b)
                return c;
            return b;
        }

        private float min(float a, float b, float c)
        {
            if (a < b && a < c)
                return a;
            if (c < b)
                return c;
            return b;
        }

        public void OnViewportMouseDown(object sender, MouseButtonEventArgs args)
        {
            var pt = args.GetPosition(mainViewport);

            switch (args.ChangedButton)
            {
                case MouseButton.Left:
                    m_transformMatrix.OnLBtnDown(pt); // rotate 3d model
                    break;

                case MouseButton.Right:
                    m_transformMatrix.OnRBtnDown(pt); //drag 3d model
                    break;

                case MouseButton.Middle:
                    m_transformMatrix.OnMBtnDown();
                    TransformChart();
                    break;

                case MouseButton.XButton1:
                case MouseButton.XButton2:
                    //m_selectRect.OnMouseDown(pt, mainViewport, m_nRectModelIndex);// select rect
                    break;
            }
        }

        public void OnViewportMouseMove(object sender, MouseEventArgs args)
        {
            var pt = args.GetPosition(mainViewport);


            if (args.LeftButton == MouseButtonState.Pressed || args.RightButton == MouseButtonState.Pressed)
                // rotate or drag 3d model
            {
                m_transformMatrix.OnMouseMove(pt, mainViewport);

                TransformChart();
            }
            else if (args.XButton1 == MouseButtonState.Pressed || args.XButton2 == MouseButtonState.Pressed)
                // select rect
            {
                //m_selectRect.OnMouseMove(pt, mainViewport, m_nRectModelIndex);
                /*
                String s1;
                Point pt2 = m_transformMatrix.VertexToScreenPt(new Point3D(0.5, 0.5, 0.3), mainViewport);
                s1 = string.Format("Screen:({0:d},{1:d}), Predicated: ({2:d}, H:{3:d})", 
                    (int)pt.X, (int)pt.Y, (int)pt2.X, (int)pt2.Y);
                this.statusPane.Text = s1;
                */
            }
        }

        public void OnViewportMouseUp(object sender, MouseButtonEventArgs args)
        {
            var pt = args.GetPosition(mainViewport);
            if (args.ChangedButton == MouseButton.Left)
            {
                m_transformMatrix.OnLBtnUp();
            }

            else if (args.ChangedButton == MouseButton.Right)
            {
                m_transformMatrix.OnRBtnUp();
            }
            else if (args.ChangedButton == MouseButton.XButton1 || args.ChangedButton == MouseButton.XButton2)
            {
                /*if (m_nChartModelIndex == -1) return;
                // 1. get the mesh structure related to the selection rect
                MeshGeometry3D meshGeometry = Computator.NET.Charting.Chart3D.Model3D.GetGeometry(mainViewport, m_nChartModelIndex);
                if (meshGeometry == null) return;

                // 2. set selection in 3d chart
                m_3dChart.Select(m_selectRect, m_transformMatrix, mainViewport);

                // 3. update selection display
                m_3dChart.HighlightSelection(meshGeometry, Color.FromRgb(200, 200, 200));*/
            }
        }

        // zoom in 3d display
        public void OnViewportMouseWheel(object sender, MouseWheelEventArgs e)
        {
            m_transformMatrix.OnMouseWheel(e);
            TransformChart();
        }

        // zoom in 3d display
        public void OnKeyDown(object sender, KeyEventArgs args)
        {
            m_transformMatrix.OnKeyDown(args);
            TransformChart();
        }

        private void UpdateModelSizeInfo(ArrayList meshs)
        {
            var nMeshNo = meshs.Count;
            var nChartVertNo = 0;
            var nChartTriangelNo = 0;
            var whichTimeCone3dAppear = 0;

            for (var i = 0; i < nMeshNo; i++)
            {
                nChartVertNo += ((Mesh3D) meshs[i]).GetVertexNo();
                nChartTriangelNo += ((Mesh3D) meshs[i]).GetTriangleNo();
                if (meshs[i].GetType() == typeof (Cone3D))
                {
                    whichTimeCone3dAppear++;
                    //axisLabels.ActiveLabels = true;
                    if (whichTimeCone3dAppear == 1)
                    {
                        axisLabels.x3D = ((Cone3D) meshs[i]).GetLastPoint();
                    }
                    else if (whichTimeCone3dAppear == 2)
                    {
                        axisLabels.y3D = ((Cone3D) meshs[i]).GetLastPoint();
                    }
                    else if (whichTimeCone3dAppear == 3)
                    {
                        axisLabels.z3D = ((Cone3D) meshs[i]).GetLastPoint();
                    }
                }
            }
        }

        // this function is used to rotate, drag and zoom the 3d chart
        private void TransformChart()
        {
            if (m_nChartModelIndex == -1) return;
            var visual3d = (ModelVisual3D) (mainViewport.Children[m_nChartModelIndex]);

            if (visual3d.Content == null) return;
            var group1 = visual3d.Content.Transform as Transform3DGroup;
            group1.Children.Clear();
            group1.Children.Add(new MatrixTransform3D(m_transformMatrix.m_totalMatrix));

            if (axisLabels.ActiveLabels && functions != null && functions.Count > 0)
            {
                var x = axisLabels.x3D;
                var y = axisLabels.y3D;
                var z = axisLabels.z3D;

                axisLabels.Reload(m_transformMatrix.VertexToScreenPt(x, mainViewport),
                    m_transformMatrix.VertexToScreenPt(y, mainViewport),
                    m_transformMatrix.VertexToScreenPt(z, mainViewport));
            }
        }
    }
}