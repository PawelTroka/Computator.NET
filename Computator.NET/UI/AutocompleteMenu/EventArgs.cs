using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutocompleteMenuNS
{
    public class SelectingEventArgs : EventArgs
    {
        public AutocompleteItem Item { get; internal set; }
        public bool Cancel { get; set; }
        public int SelectedIndex { get; set; }
        public bool Handled { get; set; }
    }

    public class SelectedEventArgs : EventArgs
    {
        public AutocompleteItem Item { get; internal set; }
        public Control Control { get; set; }
    }

    public class HoveredEventArgs : EventArgs
    {
        public AutocompleteItem Item { get; internal set; }
    }


    public class PaintItemEventArgs : PaintEventArgs
    {
        public PaintItemEventArgs(Graphics graphics, Rectangle clipRect)
            : base(graphics, clipRect)
        {
        }

        public RectangleF TextRect { get; internal set; }
        public StringFormat StringFormat { get; internal set; }
        public Font Font { get; internal set; }
        public bool IsSelected { get; internal set; }
        public bool IsHovered { get; internal set; }
    }

    public class WrapperNeededEventArgs : EventArgs
    {
        public WrapperNeededEventArgs(Control targetControl)
        {
            TargetControl = targetControl;
        }

        public Control TargetControl { get; private set; }
        public ITextBoxWrapper Wrapper { get; set; }
    }
}