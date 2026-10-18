using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Computator.NET.Charting.ComplexCharting
{
    internal static class ComplexAxesRenderer
    {
        public static ComplexAxesLayout Draw(Graphics graphics, Rectangle bounds,
            double xMin, double xMax, double yMin, double yMax, double arrowRelativeSize,
            string xLabel, string yLabel, Font labelsFont, Color axesColor, Color labelsColor)
        {
            var layout = ComplexAxesLayout.Create(bounds, graphics.DpiX, graphics.DpiY,
                xMin, xMax, yMin, yMax, arrowRelativeSize);
            if (layout == null)
                return null;

            var state = graphics.Save();
            try
            {
                graphics.PageUnit = GraphicsUnit.Pixel;
                graphics.PageScale = 1;
                graphics.SetClip(bounds, CombineMode.Intersect);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                var sourceFont = labelsFont ?? SystemFonts.DefaultFont;
                // Point fonts already account for target DPI in GetHeight. Applying
                // only the logical chart-size factor avoids scaling DPI twice, and
                // preserves explicitly selected pixel units and font styles.
                using (var font = new Font(sourceFont.FontFamily, sourceFont.Size * layout.FontScale,
                    sourceFont.Style, sourceFont.Unit, sourceFont.GdiCharSet, sourceFont.GdiVerticalFont))
                using (var format = (StringFormat)StringFormat.GenericTypographic.Clone())
                {
                    format.FormatFlags |= StringFormatFlags.NoWrap;
                    if (font.GdiVerticalFont)
                        format.FormatFlags |= StringFormatFlags.DirectionVertical;
                    var emSize = font.GetHeight(graphics.DpiY) * font.FontFamily.GetEmHeight(font.Style) /
                        font.FontFamily.GetLineSpacing(font.Style);
                    using (var xPath = LabelPath(xLabel, font, emSize, format))
                    using (var yPath = LabelPath(yLabel, font, emSize, format))
                    {
                        // The measured glyph paths are the paths we draw, including
                        // italic overhangs. Fit oversized custom labels together so
                        // neither gets cropped and both retain the same font scale.
                        var fit = Math.Min(FitScale(xPath, layout.SafeBounds.Size),
                            FitScale(yPath, layout.SafeBounds.Size));
                        if (fit < 1)
                            ScaleLabels(xPath, yPath, fit);
                        layout.PlaceLabels(PathSize(xPath), PathSize(yPath));
                        // Very large custom fonts can fit separately but not as a
                        // pair. Reduce both together until their ink is separate.
                        for (var attempt = 0; attempt < 12 &&
                            layout.XLabelBounds.IntersectsWith(layout.YLabelBounds); attempt++)
                        {
                            ScaleLabels(xPath, yPath, 0.8f);
                            layout.PlaceLabels(PathSize(xPath), PathSize(yPath));
                        }
                        var haloWidth = Math.Min(layout.StrokeWidth, 1.5f * layout.DpiScale);
                        DrawAxes(graphics, layout, axesColor, haloWidth);
                        DrawLabel(graphics, xPath, layout.XLabelBounds, labelsColor, haloWidth);
                        DrawLabel(graphics, yPath, layout.YLabelBounds, labelsColor, haloWidth);
                    }
                }
            }
            finally
            {
                graphics.Restore(state);
            }
            return layout;
        }

        private static GraphicsPath LabelPath(string text, Font font, float emSize, StringFormat format)
        {
            var path = new GraphicsPath();
            try
            {
                if (!string.IsNullOrWhiteSpace(text))
                    path.AddString(text, font.FontFamily, (int)font.Style, emSize, PointF.Empty, format);
                return path;
            }
            catch
            {
                path.Dispose();
                throw;
            }
        }

        private static SizeF PathSize(GraphicsPath path)
        {
            return path.PointCount == 0 ? SizeF.Empty : path.GetBounds().Size;
        }

        private static float FitScale(GraphicsPath path, SizeF available)
        {
            var size = PathSize(path);
            return size.IsEmpty ? 1 : Math.Min(1, Math.Min(available.Width / size.Width, available.Height / size.Height));
        }

        private static void ScaleLabels(GraphicsPath xPath, GraphicsPath yPath, float factor)
        {
            using (var scale = new Matrix())
            {
                scale.Scale(factor, factor);
                xPath.Transform(scale);
                yPath.Transform(scale);
            }
        }

        private static void DrawAxes(Graphics graphics, ComplexAxesLayout layout, Color color, float haloWidth)
        {
            using (var halo = new Pen(ContrastColor(color), layout.StrokeWidth + haloWidth))
            using (var pen = new Pen(color, layout.StrokeWidth))
            using (var brush = new SolidBrush(color))
            using (var outline = new Pen(ContrastColor(color), haloWidth))
            {
                halo.StartCap = halo.EndCap = pen.StartCap = pen.EndCap = LineCap.Round;
                outline.LineJoin = LineJoin.Round;
                graphics.DrawLine(halo, layout.XStart, layout.XEnd);
                graphics.DrawLine(halo, layout.YStart, layout.YEnd);
                graphics.DrawLine(pen, layout.XStart, layout.XEnd);
                graphics.DrawLine(pen, layout.YStart, layout.YEnd);
                if (layout.ArrowLength > 0)
                {
                    graphics.DrawPolygon(outline, layout.XArrow);
                    graphics.DrawPolygon(outline, layout.YArrow);
                    graphics.FillPolygon(brush, layout.XArrow);
                    graphics.FillPolygon(brush, layout.YArrow);
                }
            }
        }

        private static void DrawLabel(Graphics graphics, GraphicsPath path, RectangleF bounds, Color color, float haloWidth)
        {
            if (path.PointCount == 0 || bounds.IsEmpty)
                return;
            var inkBounds = path.GetBounds();
            using (var translation = new Matrix())
            using (var outline = new Pen(ContrastColor(color), haloWidth))
            using (var brush = new SolidBrush(color))
            {
                translation.Translate(bounds.Left - inkBounds.Left, bounds.Top - inkBounds.Top);
                path.Transform(translation);
                outline.LineJoin = LineJoin.Round;
                graphics.DrawPath(outline, path);
                graphics.FillPath(brush, path);
            }
        }

        private static Color ContrastColor(Color foreground)
        {
            var luminance = 0.2126 * foreground.R + 0.7152 * foreground.G + 0.0722 * foreground.B;
            return Color.FromArgb(foreground.A * 3 / 4, luminance < 128 ? Color.White : Color.Black);
        }
    }
}
