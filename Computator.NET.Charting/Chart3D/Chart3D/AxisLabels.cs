#if !__MonoCS__
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Computator.NET.Charting.Chart3D.Chart3D
{
    public class AxisLabels
    {
        private readonly Canvas _canvas;
        private bool _activeLabels;
        private string _labelX;
        private string _labelY;
        private string _labelZ;
        private bool _activeX, _activeY, _activeZ;
        private Color _color;
        private FontFamily _fontFamily;
        private double _fontSize;
        private FontStyle _fontStyle;
        private FontWeight _fontWeight;
        private Point _offset;
        private readonly Chart3DAxisAnnotations _annotations;
        private IReadOnlyList<Chart3DAxis> _axes;
        private TransformMatrix _transform;
        private Viewport3D _viewport;
        private bool _drawAxes;
        private bool _hasPositions;
        private Color _axesColor;
        private MeshGeometry3D _geometry;
        private double? _pixelsPerDip;
        private bool _occludeBackFaces = true;
        private Point _x;
        public Point3D X3D;
        private Point _y;
        public Point3D Y3D;
        private Point _z;
        public Point3D Z3D;

        public AxisLabels(Canvas canvas)
        {
            _canvas = canvas;
            _annotations = new Chart3DAxisAnnotations { IsHitTestVisible = false };
            _annotations.DpiChanged = Reload;
            _activeX = _activeZ = _activeY = _activeLabels = true;
            _offset.X = 5;
            _offset.Y = 5;
            _color = Color.FromRgb(51, 65, 85);
            _fontSize = 22;
            _fontFamily = new FontFamily("Arial");
            _fontStyle = FontStyles.Normal;
            _fontWeight = FontWeights.Normal;
            _labelX = "x";
            _labelY = "y";
            _labelZ = "z";
        }

        public string LabelX
        {
            get => _labelX;
            set
            {
                _labelX = value;
                Reload();
            }
        }

        public string LabelY
        {
            get => _labelY;
            set
            {
                _labelY = value;
                Reload();
            }
        }

        public string LabelZ
        {
            get => _labelZ;
            set
            {
                _labelZ = value;
                Reload();
            }
        }

        public bool ActiveLabels
        {
            get => _activeLabels;
            set
            {
                _activeLabels = value;
                Reload();
            }
        }

        public bool ActiveXLabel
        {
            get => _activeX;
            set
            {
                _activeX = value;
                Reload();
            }
        }

        public bool ActiveYLabel
        {
            get => _activeY;
            set
            {
                _activeY = value;
                Reload();
            }
        }

        public bool ActiveZLabel
        {
            get => _activeZ;
            set
            {
                _activeZ = value;
                Reload();
            }
        }

        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                Reload();
            }
        }

        public Point Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                Reload();
            }
        }

        public void Reload(Point x, Point y, Point z)
        {
            _x = x;
            _y = y;
            _z = z;
            _hasPositions = true;
            _axes = null;
            _transform = null;
            _viewport = null;
            _geometry = null;
            _pixelsPerDip = null;
            _drawAxes = false;
            Draw();
        }

        internal void Reload(IReadOnlyList<Chart3DAxis> axes, TransformMatrix transform,
            Viewport3D viewport, bool drawAxes, Color axesColor, MeshGeometry3D geometry = null,
            double? pixelsPerDip = null, bool occludeBackFaces = true)
        {
            _axes = axes;
            _transform = transform;
            _viewport = viewport;
            _drawAxes = drawAxes;
            _axesColor = axesColor;
            _geometry = geometry;
            _pixelsPerDip = pixelsPerDip;
            _occludeBackFaces = occludeBackFaces;
            X3D = axes[0].LabelAnchor;
            Y3D = axes[1].LabelAnchor;
            Z3D = axes[2].LabelAnchor;
            _x = transform.VertexToScreenPt(X3D, viewport);
            _y = transform.VertexToScreenPt(Y3D, viewport);
            _z = transform.VertexToScreenPt(Z3D, viewport);
            _hasPositions = true;
            Draw();
        }

        internal Chart3DAxisAnnotations Annotations => _annotations;

        internal AxisLabels CopyTo(Canvas canvas)
        {
            // Copy presentation settings for an isolated export. The live
            // positions, visual children and measured label boxes stay untouched.
            return new AxisLabels(canvas)
            {
                _labelX = _labelX, _labelY = _labelY, _labelZ = _labelZ,
                _activeX = _activeX, _activeY = _activeY, _activeZ = _activeZ,
                _activeLabels = _activeLabels, _offset = _offset, _color = _color,
                _fontFamily = _fontFamily, _fontSize = _fontSize,
                _fontStyle = _fontStyle, _fontWeight = _fontWeight
            };
        }

        public void SetProporties(FontFamily fontFamily, double? fontSize, Color? color, FontStyle? fontStyle,
            FontWeight? fontWeight)
        {
            if (color.HasValue)
                _color = color.Value;

            _fontFamily = fontFamily;

            if (fontSize.HasValue)
                _fontSize = fontSize.Value;

            if (fontStyle.HasValue)
                _fontStyle = fontStyle.Value;

            if (fontWeight.HasValue)
                _fontWeight = fontWeight.Value;

            Reload();
        }

        public void Reload()
        {
            Draw();
        }

        private void Draw()
        {
            if (!_hasPositions) return;
            if (!_canvas.Children.Contains(_annotations))
                _canvas.Children.Add(_annotations);
            _annotations.Update(_axes, _transform, _viewport, _canvas.RenderSize,
                new[] { _labelX, _labelY, _labelZ }, new[] { _x, _y, _z },
                new[] { _activeX, _activeY, _activeZ }, _activeLabels, _drawAxes, _offset,
                _fontFamily, _fontSize, _fontStyle, _fontWeight, _color, _axesColor,
                _geometry, _pixelsPerDip, _occludeBackFaces);
        }

        public void Remove()
        {
            _annotations.Clear();
            _canvas.Children.Remove(_annotations);
            _hasPositions = false;
            _axes = null;
            _transform = null;
            _viewport = null;
            _geometry = null;
            _pixelsPerDip = null;
        }
    }
}
#endif
