using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;

namespace Computator.NET.Desktop.Controls.CodeEditors.Printing
{
    internal sealed class CodePrintDocument : PrintDocument
    {
        private readonly Font _bodyFont;
        private readonly Font _labelFont;
        private readonly StringFormat _bodyFormat;
        private CodePrintPaginator _paginator;
        private int _pageNumber;

        internal CodePrintDocument(string name, string text, Font editorFont)
        {
            DocumentName = name;
            SnapshotText = text ?? string.Empty;
            OriginAtMargins = true;
            // Keep the editor's glyph family, with a readable paper size that
            // does not depend on screen zoom. Own all disposable resources.
            _bodyFont = new Font(editorFont.FontFamily, 10, editorFont.Style, GraphicsUnit.Point);
            _labelFont = new Font(FontFamily.GenericSansSerif, 9, FontStyle.Regular, GraphicsUnit.Point);
            _bodyFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
            _bodyFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap;
            _bodyFormat.Trimming = StringTrimming.None;
        }

        internal string SnapshotText { get; }
        internal event Action<string> PrintFailed;
        private bool _reportedError;

        protected override void OnBeginPrint(PrintEventArgs e)
        {
            ResetPagination();
            try
            {
                if (!PrinterSettings.IsValid) throw new InvalidPrinterException(PrinterSettings);
            }
            catch (Exception exception) when (IsPrintError(exception))
            {
                e.Cancel = true;
                ReportError(exception);
                return;
            }
            base.OnBeginPrint(e);
        }

        internal void ResetPagination()
        {
            _paginator = new CodePrintPaginator(SnapshotText);
            _pageNumber = 0;
            _reportedError = false;
        }

        protected override void OnPrintPage(PrintPageEventArgs e)
        {
            // PrintPreviewControl renders in posted UI callbacks, outside the
            // ShowDialog call stack. Cancel expected page failures here so they
            // cannot become an unhandled thread exception in the preview.
            try
            {
                var bounds = CodePrintPageLayout.ContentBounds(e.MarginBounds, e.PageSettings.PrintableArea);
                e.HasMorePages = RenderPage(e.Graphics, bounds);
            }
            catch (Exception exception) when (IsPrintError(exception))
            {
                e.Cancel = true;
                e.HasMorePages = false;
                ReportError(exception);
                return;
            }
            base.OnPrintPage(e);
        }

        internal static bool IsPrintError(Exception exception) => exception is InvalidPrinterException ||
            exception is System.Runtime.InteropServices.ExternalException || exception is CodePrintLayoutException;

        internal void ReportError(Exception exception)
        {
            if (_reportedError) return;
            _reportedError = true;
            PrintFailed?.Invoke(exception.Message);
        }

        internal bool RenderPage(Graphics graphics, RectangleF bounds)
        {
            if (_paginator == null) ResetPagination();
            var lineHeight = _bodyFont.GetHeight(graphics);
            var labelHeight = _labelFont.GetHeight(graphics) * 1.5f;
            var rows = (int)Math.Floor((bounds.Height - 2 * labelHeight) / lineHeight);
            if (bounds.Width <= 0 || rows < 1)
                throw new CodePrintLayoutException("The printable page is too small. Increase the paper size or reduce the margins.");

            var lines = _paginator.ReadPage(bounds.Width, rows,
                text => graphics.MeasureString(text, _bodyFont, int.MaxValue, _bodyFormat).Width);
            _pageNumber++;
            var state = graphics.Save();
            try
            {
                graphics.SetClip(bounds, System.Drawing.Drawing2D.CombineMode.Intersect);
                using (var labelFormat = new StringFormat(StringFormat.GenericTypographic)
                {
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                })
                {
                    graphics.DrawString(DocumentName, _labelFont, Brushes.Black,
                        new RectangleF(bounds.Left, bounds.Top, bounds.Width, labelHeight), labelFormat);
                    labelFormat.Alignment = StringAlignment.Far;
                    graphics.DrawString(_pageNumber.ToString(CultureInfo.CurrentCulture), _labelFont, Brushes.Black,
                        new RectangleF(bounds.Left, bounds.Bottom - labelHeight, bounds.Width, labelHeight), labelFormat);
                }
                var y = bounds.Top + labelHeight;
                foreach (var line in lines)
                {
                    graphics.DrawString(line, _bodyFont, Brushes.Black, new PointF(bounds.Left, y), _bodyFormat);
                    y += lineHeight;
                }
            }
            finally { graphics.Restore(state); }
            return _paginator.HasMoreLines;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bodyFont.Dispose();
                _labelFont.Dispose();
                _bodyFormat.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
