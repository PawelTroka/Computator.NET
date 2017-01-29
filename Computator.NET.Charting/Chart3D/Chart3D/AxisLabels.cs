#if !__MonoCS__
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Computator.NET.Charting.Chart3D.Chart3D
{
    public class AxisLabels
    {
        private readonly Canvas canvas;
        private bool _activeLabels;
        private string _labelX;
        private string _labelY;
        private string _labelZ;
        private bool active_x, active_y, active_z;
        private Color color;
        private FontFamily fontFamily;
        private double fontSize;
        private FontStyle fontStyle;
        private FontWeight fontWeight;
        private int index_x, index_y, index_z;
        private Point offset;
        private TextBlock textBlock;
        private Point x;
        public Point3D x3D;
        private Point y;
        public Point3D y3D;
        private Point z;
        public Point3D z3D;

        public AxisLabels(Canvas canvas)
        {
            this.canvas = canvas;
            index_x = index_y = index_z = -1;
            active_x = active_z = active_y = _activeLabels = true;
            offset.X = 5;
            offset.Y = 5;
            color = Colors.Goldenrod; //= Colors.Blue;
            fontSize = 22;
            fontFamily = new FontFamily("Cambria");
            fontStyle = FontStyles.Normal;
            fontWeight = FontWeights.Normal;
            _labelX = "x";
            _labelY = "y";
            _labelZ = "z";
        }

        public string LabelX
        {
            get { return _labelX; }
            set
            {
                _labelX = value;
                Reload();
            }
        }

        public string LabelY
        {
            get { return _labelY; }
            set
            {
                _labelY = value;
                Reload();
            }
        }

        public string LabelZ
        {
            get { return _labelZ; }
            set
            {
                _labelZ = value;
                Reload();
            }
        }

        public bool ActiveLabels
        {
            get { return _activeLabels; }
            set
            {
                _activeLabels = value;
                Reload();
            }
        }

        public bool ActiveXLabel
        {
            get { return active_x; }
            set
            {
                active_x = value;
                Reload();
            }
        }

        public bool ActiveYLabel
        {
            get { return active_y; }
            set
            {
                active_y = value;
                Reload();
            }
        }

        public bool ActiveZLabel
        {
            get { return active_z; }
            set
            {
                active_z = value;
                Reload();
            }
        }

        public Color Color
        {
            get { return color; }
            set
            {
                color = value;
                Reload();
            }
        }

        public Point Offset
        {
            get { return offset; }
            set
            {
                offset = value;
                Reload();
            }
        }

        public void Reload(Point x, Point y, Point z)
        {
            this.x = x;
            this.y = y;
            this.z = z;

            //remove the old labels from canvas if they exist:
            Remove();

            if (_activeLabels)
                draw();
        }

        public void setProporties(FontFamily fontFamily, double? fontSize, Color? color, FontStyle? fontStyle,
            FontWeight? fontWight)
        {
            if (color.HasValue)
                this.color = color.Value;

            this.fontFamily = fontFamily;

            if (fontSize.HasValue)
                this.fontSize = fontSize.Value;

            if (fontStyle.HasValue)
                this.fontStyle = fontStyle.Value;

            if (fontWight.HasValue)
                fontWeight = fontWight.Value;

            Reload();
        }

        public void Reload()
        {
            //remove the old labels from canvas if they exist:
            Remove();

            //if (_activeLabels)
            draw();
        }

        private void renewText()
        {
            textBlock = new TextBlock();
            textBlock.Foreground = new SolidColorBrush(color);
            textBlock.FontSize = fontSize;
            textBlock.FontFamily = fontFamily;
            textBlock.FontStyle = fontStyle;
            textBlock.FontWeight = fontWeight;
        }

        private void draw()
        {
            if (_activeLabels)
            {
                if (active_x)
                    drawX();

                if (active_y)
                    drawY();

                if (active_z)
                    drawZ();
            }
        }

        private void drawX()
        {
            //x-label:
            renewText();
            textBlock.Text = _labelX;
            Canvas.SetLeft(textBlock, x.X + offset.X);
            Canvas.SetTop(textBlock, x.Y + offset.Y);
            index_x = canvas.Children.Add(textBlock);
        }

        private void drawY()
        {
            //y-label:
            renewText();
            textBlock.Text = _labelY;
            Canvas.SetLeft(textBlock, y.X + offset.X);
            Canvas.SetTop(textBlock, y.Y + offset.Y);
            index_y = canvas.Children.Add(textBlock);
        }

        private void drawZ()
        {
            //z-label:
            renewText();
            textBlock.Text = _labelZ;
            Canvas.SetLeft(textBlock, z.X + offset.X);
            Canvas.SetTop(textBlock, z.Y + offset.Y);
            index_z = canvas.Children.Add(textBlock);
        }

        public void Remove()
        {
            //remove the old labels from canvas if they exist:
            if (index_z != -1)
                canvas.Children.RemoveAt(index_z);
            if (index_y != -1)
                canvas.Children.RemoveAt(index_y);
            if (index_x != -1)
                canvas.Children.RemoveAt(index_x);

            index_x = index_y = index_z = -1;
        }
    }
}
#endif