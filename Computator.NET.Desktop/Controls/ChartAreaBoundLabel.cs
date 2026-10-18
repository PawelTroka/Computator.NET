using System;
using System.Drawing;
using System.Windows.Forms;

namespace Computator.NET.Desktop.Controls
{
    // Render the bound with a smaller, lowered font instead of relying on a
    // font's incomplete/inconsistent Unicode subscript alphabet.
    internal sealed class ChartAreaBoundLabel : Label
    {
        private const TextFormatFlags TextFlags = TextFormatFlags.NoPadding |
            TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
        private const string Suffix = " =";
        private readonly string _bound;
        private string _axis = string.Empty;

        public ChartAreaBoundLabel(string axis, string bound)
        {
            _bound = bound;
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            AutoSize = true;
            UseMnemonic = false;
            Axis = axis;
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string Axis
        {
            get => _axis;
            set
            {
                if (_axis == value) return;
                _axis = value;
                // Keep a complete plain-text equivalent for accessibility and
                // UI automation even though the visible text is typeset.
                Text = $"{_axis} {_bound} =";
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            using var graphics = CreateGraphics();
            using var subscriptFont = CreateSubscriptFont();
            var size = Measure(graphics, subscriptFont, out _, out _, out _);
            return size + Padding.Size;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using var subscriptFont = CreateSubscriptFont();
            var size = Measure(e.Graphics, subscriptFont, out var axisWidth,
                out var boundWidth, out var subscriptOffset);
            var x = ClientSize.Width - Padding.Right - size.Width;
            var y = Padding.Top + (ClientSize.Height - Padding.Vertical - size.Height) / 2;
            var color = Enabled ? ForeColor : SystemColors.GrayText;
            TextRenderer.DrawText(e.Graphics, _axis, Font, new Point(x, y), color, TextFlags);
            TextRenderer.DrawText(e.Graphics, _bound, subscriptFont,
                new Point(x + axisWidth, y + subscriptOffset), color, TextFlags);
            TextRenderer.DrawText(e.Graphics, Suffix, Font,
                new Point(x + axisWidth + boundWidth, y), color, TextFlags);
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            if (AutoSize) Size = GetPreferredSize(Size.Empty);
            Invalidate();
        }

        private Font CreateSubscriptFont() =>
            new Font(Font.FontFamily, Font.Size * 0.7f, Font.Style, Font.Unit);

        private Size Measure(Graphics graphics, Font subscriptFont, out int axisWidth,
            out int boundWidth, out int subscriptOffset)
        {
            var axis = TextRenderer.MeasureText(graphics, _axis, Font, Size.Empty, TextFlags);
            var bound = TextRenderer.MeasureText(graphics, _bound, subscriptFont, Size.Empty, TextFlags);
            var suffix = TextRenderer.MeasureText(graphics, Suffix, Font, Size.Empty, TextFlags);
            axisWidth = axis.Width;
            boundWidth = bound.Width;
            subscriptOffset = (int)Math.Ceiling(suffix.Height * 0.4f);
            return new Size(axis.Width + bound.Width + suffix.Width,
                Math.Max(suffix.Height, bound.Height + subscriptOffset));
        }
    }
}
