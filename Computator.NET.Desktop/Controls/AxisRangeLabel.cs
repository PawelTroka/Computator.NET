using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Computator.NET.Desktop.Controls
{
    /// <summary>A coordinate with a real typographic min/max subscript.</summary>
    public sealed class AxisRangeLabel : Label
    {
        private string _coordinateName = "x";
        private bool _isMaximum;

        public AxisRangeLabel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);
            TextAlign = ContentAlignment.MiddleRight;
            UpdateText();
        }

        [DefaultValue("x")]
        public string CoordinateName
        {
            get => _coordinateName;
            set
            {
                _coordinateName = value ?? string.Empty;
                UpdateText();
            }
        }

        [DefaultValue(false)]
        public bool IsMaximum
        {
            get => _isMaximum;
            set
            {
                _isMaximum = value;
                UpdateText();
            }
        }

        private string Bound => IsMaximum ? "max" : "min";

        private void UpdateText()
        {
            // Screen readers receive ordinary characters. The visual subscript
            // is drawn separately with a smaller font and a lower baseline.
            AccessibleName = CoordinateName + " " + Bound;
            Text = AccessibleName + " =";
        }

        private Font CreateSubscriptFont() => new Font(Font.FontFamily, Font.Size * 0.72f,
            Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);

        private static StringFormat CreateFormat()
        {
            var format = (StringFormat)StringFormat.GenericTypographic.Clone();
            format.FormatFlags |= StringFormatFlags.NoWrap | StringFormatFlags.MeasureTrailingSpaces;
            return format;
        }

        private SizeF Measure(Graphics graphics, Font subscriptFont, StringFormat format,
            out float coordinateWidth, out float subscriptWidth, out float subscriptY, out float inset)
        {
            float lineHeight = Font.GetHeight(graphics);
            inset = Math.Max(1f, lineHeight * 0.12f);
            subscriptY = lineHeight * 0.36f;
            coordinateWidth = graphics.MeasureString(CoordinateName, Font, PointF.Empty, format).Width;
            subscriptWidth = graphics.MeasureString(Bound, subscriptFont, PointF.Empty, format).Width;
            float suffixWidth = graphics.MeasureString(" =", Font, PointF.Empty, format).Width;
            return new SizeF(coordinateWidth + subscriptWidth + suffixWidth + inset * 2,
                Math.Max(lineHeight, subscriptY + subscriptFont.GetHeight(graphics)) + inset * 2);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            // Measure at the control's current DPI without creating a window
            // handle during its parent's layout or applying a second DPI factor.
            using var bitmap = new Bitmap(1, 1);
            bitmap.SetResolution(DeviceDpi, DeviceDpi);
            using var graphics = Graphics.FromImage(bitmap);
            using var subscriptFont = CreateSubscriptFont();
            using var format = CreateFormat();
            SizeF size = Measure(graphics, subscriptFont, format, out _, out _, out _, out _);
            return new Size((int)Math.Ceiling(size.Width) + Padding.Horizontal,
                (int)Math.Ceiling(size.Height) + Padding.Vertical);
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            if (AutoSize)
                Size = GetPreferredSize(Size.Empty);
            Parent?.PerformLayout(this, nameof(PreferredSize));
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using var subscriptFont = CreateSubscriptFont();
            using var format = CreateFormat();
            SizeF size = Measure(e.Graphics, subscriptFont, format,
                out float coordinateWidth, out float subscriptWidth, out float subscriptY, out float inset);
            float x = Padding.Left + Math.Max(0, ClientSize.Width - Padding.Horizontal - size.Width);
            float y = Padding.Top + Math.Max(0, (ClientSize.Height - Padding.Vertical - size.Height) / 2);
            using var brush = new SolidBrush(Enabled ? ForeColor : SystemColors.GrayText);
            e.Graphics.DrawString(CoordinateName, Font, brush, new PointF(x + inset, y + inset), format);
            e.Graphics.DrawString(Bound, subscriptFont, brush,
                new PointF(x + inset + coordinateWidth, y + inset + subscriptY), format);
            e.Graphics.DrawString(" =", Font, brush,
                new PointF(x + inset + coordinateWidth + subscriptWidth, y + inset), format);
        }
    }
}
