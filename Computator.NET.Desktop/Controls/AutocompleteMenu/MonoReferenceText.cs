#if __MonoCS__
using System;
using System.Drawing;
using System.Windows.Forms;
using Computator.NET.Core.Autocompletion;

namespace Computator.NET.Desktop.Controls.AutocompleteMenu
{
    internal sealed class MonoReferenceText : UserControl
    {
        private readonly ReferenceCanvas canvas;
        private readonly RichTextBox readableText;
        private readonly TabControl tabs;
        private readonly Font readingFont;
        public bool ReadOnly => true;
        internal event EventHandler ReadingFocusLeft;

        public MonoReferenceText()
        {
            Dock = DockStyle.Fill;
            readingFont = new Font(FontFamily.GenericSansSerif, 12f);
            Font = readingFont;
            AccessibleName = "Function reference";
            tabs = new TabControl { Dock = DockStyle.Fill, TabStop = false };
            var description = new TabPage("Description") { Padding = Padding.Empty };
            var text = new TabPage("Text") { Padding = Padding.Empty };
            canvas = new ReferenceCanvas { Name = "referenceDescription", Dock = DockStyle.Fill, Font = Font };
            readableText = new RichTextBox
            {
                Name = "referenceText", Dock = DockStyle.Fill, ReadOnly = true, DetectUrls = false,
                BorderStyle = BorderStyle.None, BackColor = Color.White, ForeColor = Color.Black,
                Font = Font, ScrollBars = RichTextBoxScrollBars.Vertical, AccessibleName = "Reference text"
            };
            description.Controls.Add(canvas); text.Controls.Add(readableText);
            tabs.TabPages.Add(description); tabs.TabPages.Add(text); Controls.Add(tabs);
            // ToolStripDropDown does not propagate Leave from its hosted
            // WinForms children on Mono. Observe the actual focusable leaves.
            foreach (var control in new Control[] { this, tabs, canvas, readableText })
                control.LostFocus += (_, _) => ReadingFocusLeft?.Invoke(this, EventArgs.Empty);
        }

        public void ShowDocument(FunctionInfo info)
        {
            var document = MonoReferenceDocument.Parse(info);
            canvas.ShowDocument(document);
            base.Text = document.PlainText;
            AccessibleDescription = document.PlainText;
            readableText.Text = document.PlainText;
            readableText.SelectionStart = readableText.SelectionLength = 0;
            readableText.ScrollToCaret();
            tabs.SelectedIndex = 0;
        }
        internal int PreferredDocumentHeight(int width) => canvas.MeasureHeight(Math.Max(64, width - 24)) + tabs.ItemSize.Height + 10;
        internal bool ScrollDescriptionAtPointer(int delta, Point screenPosition)
        {
            if (!canvas.Visible || !canvas.Enabled || !canvas.IsHandleCreated) return false;
            var point = canvas.PointToClient(screenPosition);
            if (!canvas.ClientRectangle.Contains(point)) return false;
            canvas.ScrollWheel(delta, point);
            return true;
        }
        internal static string Render(FunctionInfo info)
        {
            using (var document = MonoReferenceDocument.Parse(info)) return document.PlainText;
        }
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (canvas != null) canvas.Font = Font;
            if (readableText != null) readableText.Font = Font;
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing) readingFont?.Dispose();
        }
        private sealed class ReferenceCanvas : ScrollableControl
        {
            private MonoReferenceDocument document;
            private MonoReferenceLayout layout;
            private int layoutWidth;
            private float layoutDpi;
            internal ReferenceCanvas()
            {
                BackColor = Color.White; ForeColor = Color.Black; AutoScroll = true;
                TabStop = true; AccessibleName = "Reference description";
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            }
            internal void ShowDocument(MonoReferenceDocument value)
            {
                layout?.Dispose(); layout = null; document?.Dispose(); document = value;
                AutoScrollPosition = Point.Empty; Invalidate();
            }
            internal void ScrollWheel(int delta, Point point)
            {
                OnMouseWheel(new MouseEventArgs(MouseButtons.None, 0, point.X, point.Y, delta));
            }
            internal int MeasureHeight(int width)
            {
                if (document == null) return 0;
                using (var bitmap = new Bitmap(1, 1))
                {
                    if (IsHandleCreated)
                        using (var screen = CreateGraphics()) bitmap.SetResolution(screen.DpiX, screen.DpiY);
                    using (var graphics = Graphics.FromImage(bitmap))
                    using (var measured = document.Layout(graphics, Font, width)) return measured.ContentSize.Height;
                }
            }
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                if (document == null) return;
                var width = Math.Max(64, ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
                if (layout == null || layoutWidth != width || layoutDpi != e.Graphics.DpiY)
                {
                    layout?.Dispose(); layout = document.Layout(e.Graphics, Font, width); layoutWidth = width; layoutDpi = e.Graphics.DpiY;
                    AutoScrollMinSize = layout.ContentSize;
                }
                layout.Draw(e.Graphics, AutoScrollPosition);
            }
            protected override void OnFontChanged(EventArgs e)
            {
                layout?.Dispose(); layout = null; base.OnFontChanged(e); Invalidate();
            }
            protected override void OnKeyDown(KeyEventArgs e)
            {
                var current = new Point(-AutoScrollPosition.X, -AutoScrollPosition.Y);
                switch (e.KeyCode)
                {
                    case Keys.Down: current.Y += Font.Height * 2; break;
                    case Keys.Up: current.Y -= Font.Height * 2; break;
                    case Keys.PageDown: current.Y += Math.Max(Font.Height, ClientSize.Height - Font.Height); break;
                    case Keys.PageUp: current.Y -= Math.Max(Font.Height, ClientSize.Height - Font.Height); break;
                    case Keys.Home: current = Point.Empty; break;
                    case Keys.End: current.Y = AutoScrollMinSize.Height; break;
                    case Keys.Left: current.X -= Font.Height * 2; break;
                    case Keys.Right: current.X += Font.Height * 2; break;
                    default: base.OnKeyDown(e); return;
                }
                AutoScrollPosition = new Point(Math.Max(0, current.X), Math.Max(0, current.Y)); e.Handled = true;
            }
            protected override bool IsInputKey(Keys keyData)
            {
                var key = keyData & Keys.KeyCode;
                return key == Keys.Up || key == Keys.Down || key == Keys.Left || key == Keys.Right ||
                    key == Keys.PageUp || key == Keys.PageDown || key == Keys.Home || key == Keys.End || base.IsInputKey(keyData);
            }
            protected override void Dispose(bool disposing)
            {
                if (disposing) { layout?.Dispose(); document?.Dispose(); layout = null; document = null; }
                base.Dispose(disposing);
            }
        }
    }
}
#endif
