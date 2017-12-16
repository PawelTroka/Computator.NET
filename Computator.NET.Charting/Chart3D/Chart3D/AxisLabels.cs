#if !__MonoCS__
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
        private int _indexX, _indexY, _indexZ;
        private Point _offset;
        private TextBlock _textBlock;
        private Point _x;
        public Point3D X3D;
        private Point _y;
        public Point3D Y3D;
        private Point _z;
        public Point3D Z3D;

        public AxisLabels(Canvas canvas)
        {
            _canvas = canvas;
            _indexX = _indexY = _indexZ = -1;
            _activeX = _activeZ = _activeY = _activeLabels = true;
            _offset.X = 5;
            _offset.Y = 5;
            _color = Colors.Goldenrod; //= Colors.Blue;
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

            //remove the old labels from canvas if they exist:
            Remove();

            if (_activeLabels)
                Draw();
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
            //remove the old labels from canvas if they exist:
            Remove();

            //if (_activeLabels)
            Draw();
        }

        private void RenewText()
        {
            _textBlock = new TextBlock
            {
                Foreground = new SolidColorBrush(_color),
                FontSize = _fontSize,
                FontFamily = _fontFamily,
                FontStyle = _fontStyle,
                FontWeight = _fontWeight
            };
        }

        private void Draw()
        {
            if (_activeLabels)
            {
                if (_activeX)
                    DrawX();

                if (_activeY)
                    DrawY();

                if (_activeZ)
                    DrawZ();
            }
        }

        private void DrawX()
        {
            //x-label:
            RenewText();
            _textBlock.Text = _labelX;
            Canvas.SetLeft(_textBlock, _x.X + _offset.X);
            Canvas.SetTop(_textBlock, _x.Y + _offset.Y);
            _indexX = _canvas.Children.Add(_textBlock);
        }

        private void DrawY()
        {
            //y-label:
            RenewText();
            _textBlock.Text = _labelY;
            Canvas.SetLeft(_textBlock, _y.X + _offset.X);
            Canvas.SetTop(_textBlock, _y.Y + _offset.Y);
            _indexY = _canvas.Children.Add(_textBlock);
        }

        private void DrawZ()
        {
            //z-label:
            RenewText();
            _textBlock.Text = _labelZ;
            Canvas.SetLeft(_textBlock, _z.X + _offset.X);
            Canvas.SetTop(_textBlock, _z.Y + _offset.Y);
            _indexZ = _canvas.Children.Add(_textBlock);
        }

        public void Remove()
        {
            //remove the old labels from canvas if they exist:
            if (_indexZ != -1)
                _canvas.Children.RemoveAt(_indexZ);
            if (_indexY != -1)
                _canvas.Children.RemoveAt(_indexY);
            if (_indexX != -1)
                _canvas.Children.RemoveAt(_indexX);

            _indexX = _indexY = _indexZ = -1;
        }
    }
}
#endif