namespace AutocompleteMenuNS
{
    public class SelectingEventArgs : System.EventArgs
    {
        public AutocompleteItem Item { get; internal set; }
        public bool Cancel { get; set; }
        public int SelectedIndex { get; set; }
        public bool Handled { get; set; }
    }

    public class SelectedEventArgs : System.EventArgs
    {
        public AutocompleteItem Item { get; internal set; }
        public System.Windows.Forms.Control Control { get; set; }
    }

    public class HoveredEventArgs : System.EventArgs
    {
        public AutocompleteItem Item { get; internal set; }
    }


    public class PaintItemEventArgs : System.Windows.Forms.PaintEventArgs
    {
        public PaintItemEventArgs(System.Drawing.Graphics graphics, System.Drawing.Rectangle clipRect)
            : base(graphics, clipRect)
        {
        }

        public System.Drawing.RectangleF TextRect { get; internal set; }
        public System.Drawing.StringFormat StringFormat { get; internal set; }
        public System.Drawing.Font Font { get; internal set; }
        public bool IsSelected { get; internal set; }
        public bool IsHovered { get; internal set; }
    }

    public class WrapperNeededEventArgs : System.EventArgs
    {
        public WrapperNeededEventArgs(System.Windows.Forms.Control targetControl)
        {
            TargetControl = targetControl;
        }

        public System.Windows.Forms.Control TargetControl { get; private set; }
        public ITextBoxWrapper Wrapper { get; set; }
    }
}