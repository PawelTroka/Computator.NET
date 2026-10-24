using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Computator.NET.Desktop.Controls
{
    // A normal accessible Label with a typeset min/max subscript. Unicode does not
    // provide all the letters in "max", so both bounds use the same font treatment.
    internal sealed class ChartRangeLabel : Label
    {
        private bool _isMaximum;

        public ChartRangeLabel()
        {
            UseMnemonic = false;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        [DefaultValue(false)]
        public bool IsMaximum
        {
            get => _isMaximum;
            set
            {
                if (_isMaximum == value) return;
                _isMaximum = value;
                if (AutoSize) Size = GetPreferredSize(Size.Empty);
                Parent?.PerformLayout(this, nameof(IsMaximum));
                Invalidate();
            }
        }

        private string Subscript => IsMaximum ? "max" : "min";

        public override Size GetPreferredSize(Size proposedSize)
        {
            using var graphics = CreateGraphics();
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            using var subscriptFont = CreateSubscriptFont();
            using var format = CreateFormat();
            var layout = Measure(graphics, subscriptFont, format);
            return Size.Ceiling(new SizeF(layout.Width + Padding.Horizontal,
                layout.Height + Padding.Vertical));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using var subscriptFont = CreateSubscriptFont();
            using var format = CreateFormat();
            using var brush = new SolidBrush(Enabled ? ForeColor : SystemColors.GrayText);
            var state = e.Graphics.Save();
            try
            {
                e.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                var layout = Measure(e.Graphics, subscriptFont, format);
                var x = Padding.Left + Math.Max(0, ClientSize.Width - Padding.Horizontal - layout.Width)
                    + layout.Inset;
                var y = Padding.Top + Math.Max(0, (ClientSize.Height - Padding.Vertical - layout.Height) / 2)
                    + layout.Inset;
                e.Graphics.DrawString(Text, Font, brush, x, y, format);
                e.Graphics.DrawString(Subscript, subscriptFont, brush, x + layout.AxisWidth,
                    y + layout.SubscriptDrop, format);
                e.Graphics.DrawString(" =", Font, brush, x + layout.AxisWidth + layout.SubscriptWidth,
                    y, format);
            }
            finally
            {
                e.Graphics.Restore(state);
            }
        }

#if !__MonoCS__
        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            if (AutoSize) Size = GetPreferredSize(Size.Empty);
            Parent?.PerformLayout(this, nameof(DeviceDpi));
            Invalidate();
        }
#endif

        private Font CreateSubscriptFont() => new Font(Font.FontFamily, Font.Size * 0.75f,
            Font.Style, Font.Unit);

        private static StringFormat CreateFormat()
        {
            var format = StringFormat.GenericTypographic;
            format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
            return format;
        }

        private (float AxisWidth, float SubscriptWidth, float SubscriptDrop, float Inset, float Width, float Height)
            Measure(Graphics graphics, Font subscriptFont, StringFormat format)
        {
            var axis = graphics.MeasureString(Text, Font, PointF.Empty, format);
            var subscript = graphics.MeasureString(Subscript, subscriptFont, PointF.Empty, format);
            var equals = graphics.MeasureString(" =", Font, PointF.Empty, format);
            var drop = Font.GetHeight(graphics) * 0.4f;
            var inset = graphics.DpiX / 96f;
            return (axis.Width, subscript.Width, drop, inset,
                axis.Width + subscript.Width + equals.Width + 2 * inset,
                Math.Max(axis.Height, subscript.Height + drop) + 2 * inset);
        }
    }
}
