using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Computator.NET.Charting.Controls
{
    /// <summary>A native WinForms color preview, replacing the retired PowerPacks shape.</summary>
    public sealed class ColorSwatch : Control
    {
        public ColorSwatch()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable |
                ControlStyles.SupportsTransparentBackColor, true);
            Cursor = Cursors.Hand;
            TabStop = true;
            AccessibleRole = AccessibleRole.PushButton;
        }

        [DefaultValue(typeof(Color), "Control")]
        public Color FillColor
        {
            get => BackColor;
            set { BackColor = value; Invalidate(); }
        }

        [DefaultValue(2)]
        public int CornerRadius { get; set; } = 2;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Width < 2 || Height < 2) return;
            // WinForms paints BackColor over the parent in OnPaintBackground.
            // Painting it again here would apply partial transparency twice.
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle, SystemColors.ControlDark, ButtonBorderStyle.Solid);
            if (Focused) ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -3, -3));
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                OnClick(System.EventArgs.Empty);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
    }
}
