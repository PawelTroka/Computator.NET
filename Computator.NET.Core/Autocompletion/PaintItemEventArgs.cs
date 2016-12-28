using System.Drawing;
using System.Windows.Forms;

namespace Computator.NET.UI.Controls.AutocompleteMenu
{
    public class PaintItemEventArgs : PaintEventArgs
    {
        public PaintItemEventArgs(Graphics graphics, Rectangle clipRect) : base(graphics, clipRect)
        {
        }

        public RectangleF TextRect { get; set; }
        public StringFormat StringFormat { get; set; }
        public Font Font { get; set; }
        public bool IsSelected { get; set; }
        public bool IsHovered { get; set; }
        public Colors Colors { get; set; }
    }
}