using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using Computator.NET.Charting.Printing;
using Computator.NET.DataTypes.Charts;
using Computator.NET.DataTypes.Functions;

namespace Computator.NET.Charting.Chart3D.UI
{
    /// <summary>
    /// Native WinForms Mono f(x,y) chart. Rendering uses libgdiplus through
    /// System.Drawing; expression compilation/evaluation remains in shared Core.
    /// The WPF Chart3DControl is completely separate and unaffected.
    /// </summary>
    public class
#if __MonoCS__
        Chart3DControl
#else
        Chart3DControlMono
#endif
        : Control, IChart3D
    {
        private readonly List<Function> _functions = new List<Function>();
        private readonly List<MonoSurfaceGrid> _surfaces = new List<MonoSurfaceGrid>();
        private readonly List<List<Point3D>> _points = new List<List<Point3D>>();
        private double _xMin = -5, _xMax = 5, _yMin = -5, _yMax = 5;
        private double _quality = 50;
        private double _yaw = .76, _tilt = .55, _zoom = 1, _panX, _panY;
        private bool _equalAxes = true;
        private Chart3DMode _mode = Chart3DMode.Surface;
        private MouseButtons _dragButton;
        private Point _lastPointer;

        public
#if __MonoCS__
            Chart3DControl()
#else
            Chart3DControlMono()
#endif
        {
            Name = "chart3D";
            Dock = DockStyle.Fill;
            BackColor = Color.White;
            ForeColor = Color.FromArgb(30, 41, 59);
            TabStop = true;
            AccessibleName = "Interactive 3D chart";
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                ControlStyles.Selectable, true);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int FiniteSurfaceSamples => _surfaces.Sum(surface => surface.FiniteCount);

        [Category("3D view")]
        [Description("Sampling resolution is limited to 17–97 vertices per axis.")]
        public double Quality
        {
            get => _quality;
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value));
                var clamped = Math.Max(0, Math.Min(100, value));
                if (_quality == clamped) return;
                _quality = clamped;
                Resample();
                Changed(nameof(Quality));
            }
        }

        [Category("3D view")]
        public bool EqualAxes
        {
            get => _equalAxes;
            set { if (_equalAxes == value) return; _equalAxes = value; Invalidate(); Changed(nameof(EqualAxes)); }
        }

        [Category("3D view")]
        public Chart3DMode Mode
        {
            get => _mode;
            set
            {
                if (!Enum.IsDefined(typeof(Chart3DMode), value))
                    throw new ArgumentOutOfRangeException(nameof(value));
                if (_mode == value) return;
                _mode = value;
                Invalidate();
                Changed(nameof(Mode));
            }
        }

        [Category("3D view")]
        public double Zoom
        {
            get => _zoom;
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value));
                _zoom = Math.Max(.15, Math.Min(7, value));
                Invalidate();
                Changed(nameof(Zoom));
            }
        }

        [Category("3D view")]
        public double YawDegrees
        {
            get => _yaw * 180 / Math.PI;
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value));
                _yaw = (value % 360) * Math.PI / 180;
                Invalidate();
                Changed(nameof(YawDegrees));
            }
        }

        [Category("3D view")]
        public double TiltDegrees
        {
            get => _tilt * 180 / Math.PI;
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value));
                _tilt = Math.Max(-80, Math.Min(80, value)) * Math.PI / 180;
                Invalidate();
                Changed(nameof(TiltDegrees));
            }
        }

        [Category("3D area")]
        public double XMin
        {
            get => _xMin;
            set
            {
                if (_xMin == value) return;
                _xMin = value;
                Resample();
                XMinChanged?.Invoke(this, EventArgs.Empty);
                Changed(nameof(XMin));
            }
        }

        [Category("3D area")]
        public double XMax
        {
            get => _xMax;
            set
            {
                if (_xMax == value) return;
                _xMax = value;
                Resample();
                XMaxChanged?.Invoke(this, EventArgs.Empty);
                Changed(nameof(XMax));
            }
        }

        [Category("3D area")]
        public double YMin
        {
            get => _yMin;
            set
            {
                if (_yMin == value) return;
                _yMin = value;
                Resample();
                YMinChanged?.Invoke(this, EventArgs.Empty);
                Changed(nameof(YMin));
            }
        }

        [Category("3D area")]
        public double YMax
        {
            get => _yMax;
            set
            {
                if (_yMax == value) return;
                _yMax = value;
                Resample();
                YMaxChanged?.Invoke(this, EventArgs.Empty);
                Changed(nameof(YMax));
            }
        }

        public event EventHandler XMinChanged;
        public event EventHandler XMaxChanged;
        public event EventHandler YMinChanged;
        public event EventHandler YMaxChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void Changed(string property) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        public void SetChartAreaValues(double x0, double xn, double y0, double yn)
        {
            if (!MonoSurfaceSampler.ValidBounds(x0, xn, y0, yn))
                throw new ArgumentException("3D axes require finite, increasing x and y bounds.");
            var changeXMin = _xMin != x0;
            var changeXMax = _xMax != xn;
            var changeYMin = _yMin != y0;
            var changeYMax = _yMax != yn;
            _xMin = x0; _xMax = xn; _yMin = y0; _yMax = yn;
            Resample(); // atomic; avoids intermediate invalid bounds
            if (changeXMin) { XMinChanged?.Invoke(this, EventArgs.Empty); Changed(nameof(XMin)); }
            if (changeXMax) { XMaxChanged?.Invoke(this, EventArgs.Empty); Changed(nameof(XMax)); }
            if (changeYMin) { YMinChanged?.Invoke(this, EventArgs.Empty); Changed(nameof(YMin)); }
            if (changeYMax) { YMaxChanged?.Invoke(this, EventArgs.Empty); Changed(nameof(YMax)); }
        }

        private int GridSize => 17 + (int)Math.Round(_quality * .8);

        private void Resample()
        {
            if (IsDisposed || Disposing) return;
            if (!MonoSurfaceSampler.ValidBounds(_xMin, _xMax, _yMin, _yMax))
            {
                _surfaces.Clear();
                Invalidate();
                return;
            }

            // Build a complete new snapshot first. A failed evaluation never
            // leaves a half-filled mesh or invalid GDI resources behind.
            var replacement = new List<MonoSurfaceGrid>();
            foreach (var function in _functions)
                replacement.Add(MonoSurfaceSampler.Sample(function, _xMin, _xMax, _yMin, _yMax, GridSize));
            _surfaces.Clear();
            _surfaces.AddRange(replacement);
            Invalidate();
        }

        public void AddFunction(Function function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            if (_functions.Count >= 8)
                throw new InvalidOperationException("A Mono chart can contain at most eight surfaces.");
            var sampled = MonoSurfaceSampler.Sample(function, _xMin, _xMax, _yMin, _yMax, GridSize);
            _functions.Add(function);
            _surfaces.Add(sampled);
            Invalidate();
        }

        public void AddPoints(IEnumerable<Point3D> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            var values = new List<Point3D>();
            foreach (var point in points)
            {
                if (values.Count == 20000)
                    throw new ArgumentOutOfRangeException(nameof(points), "A 3D series supports at most 20,000 points.");
                if (point != null && MonoSurfaceSampler.IsFinite(point.x) &&
                    MonoSurfaceSampler.IsFinite(point.y) && MonoSurfaceSampler.IsFinite(point.z))
                    values.Add(new Point3D(point.x, point.y, point.z));
            }
            if (_points.Sum(series => series.Count) + values.Count > 40000)
                throw new ArgumentOutOfRangeException(nameof(points), "A chart supports at most 40,000 points.");
            _points.Add(values);
            Mode = Chart3DMode.Points;
            Invalidate();
        }

        // Retain the list overload used by the Windows/WPF chart.
        public void AddPoints(List<Point3D> points) => AddPoints((IEnumerable<Point3D>)points);

        public void ClearAll()
        {
            _functions.Clear();
            _surfaces.Clear();
            _points.Clear();
            Invalidate();
        }

        public void Redraw() => Resample();

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            MonoSceneRenderer.Draw(e.Graphics, ClientRectangle, Font,
                _surfaces, _points, _mode, _xMin, _xMax, _yMin, _yMax,
                _equalAxes, _yaw, _tilt, _zoom, _panX, _panY);
        }

        public Image GetImage(int width, int height)
        {
            if (width < 1 || width > 4096) throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 1 || height > 4096 || (long)width * height > 8388608)
                throw new ArgumentOutOfRangeException(nameof(height));
            var image = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            try
            {
                image.SetResolution(96, 96);
                // Preserve the user's camera composition when exporting. Pan is
                // specified in live control pixels, so scale it for another output size.
                var panX = ClientSize.Width > 0 ? _panX * width / ClientSize.Width : _panX;
                var panY = ClientSize.Height > 0 ? _panY * height / ClientSize.Height : _panY;
                using (var graphics = Graphics.FromImage(image))
                    MonoSceneRenderer.Draw(graphics, new Rectangle(0, 0, width, height),
                        Font, _surfaces, _points, _mode, _xMin, _xMax, _yMin, _yMax,
                        _equalAxes, _yaw, _tilt, _zoom, panX, panY);
                return image;
            }
            catch { image.Dispose(); throw; }
        }

        public void SaveImage(string path, ImageFormat imageFormat)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("An image path is required.", nameof(path));
            if (imageFormat == null) throw new ArgumentNullException(nameof(imageFormat));
            var width = Math.Max(1, ClientSize.Width);
            var height = Math.Max(1, ClientSize.Height);
            using (var image = GetImage(width, height))
                image.Save(path, imageFormat);
        }

        public void Print()
        {
            using (var image = GetImage(Math.Max(1, Width), Math.Max(1, Height)))
                new ImagePrinter().Print(image);
        }

        public void PrintPreview()
        {
            using (var image = GetImage(Math.Max(1, Width), Math.Max(1, Height)))
                new ImagePrinter().PrintPreview(image);
        }

        public void ShowEditDialog() => ShowEditPropertiesDialog();

        public void ShowEditPropertiesDialog()
        {
            using (var editor = new EditChartProperties(this))
                editor.ShowDialog(FindForm());
            Redraw();
        }

        public void ShowPlotDialog()
        {
            // Do not reparent/dispose the live chart from the main desktop view.
            var preview = new Chart3DControl
            {
                Quality = Quality, EqualAxes = EqualAxes, Mode = Mode,
                YawDegrees = YawDegrees, TiltDegrees = TiltDegrees, Zoom = Zoom
            };
            preview.SetChartAreaValues(_xMin, _xMax, _yMin, _yMax);
            foreach (var function in _functions) preview.AddFunction(function);
            foreach (var series in _points) preview.AddPoints(series);
            preview.Mode = Mode;
            new PlotForm(preview).Show();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) return;
            Focus();
            _dragButton = e.Button;
            _lastPointer = e.Location;
            Capture = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_dragButton == MouseButtons.None) return;
            var dx = e.X - _lastPointer.X;
            var dy = e.Y - _lastPointer.Y;
            _lastPointer = e.Location;
            if (_dragButton == MouseButtons.Left)
            {
                _yaw += dx * .012;
                _tilt = Math.Max(-1.4, Math.Min(1.4, _tilt + dy * .01));
            }
            else
            {
                _panX += dx;
                _panY += dy;
            }
            Invalidate(); // camera changes do not reevaluate any expression
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != _dragButton) return;
            _dragButton = MouseButtons.None;
            Capture = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Zoom *= Math.Pow(1.15, e.Delta / 120.0);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            var key = keyData & Keys.KeyCode;
            return key == Keys.Up || key == Keys.Down || key == Keys.Left || key == Keys.Right ||
                   base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.Left: _yaw -= .12; break;
                case Keys.Right: _yaw += .12; break;
                case Keys.Up: _tilt = Math.Min(1.4, _tilt + .12); break;
                case Keys.Down: _tilt = Math.Max(-1.4, _tilt - .12); break;
                case Keys.Add:
                case Keys.Oemplus: Zoom *= 1.15; break;
                case Keys.Subtract:
                case Keys.OemMinus: Zoom /= 1.15; break;
                case Keys.Home:
                    _yaw = .76; _tilt = .55; _panX = _panY = 0; Zoom = 1;
                    break;
                default: return;
            }
            e.Handled = true;
            Invalidate();
        }
    }
}
