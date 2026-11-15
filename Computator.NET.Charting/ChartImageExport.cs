using System;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Computator.NET.DataTypes.Charts;
using Computator.NET.Localization;

namespace Computator.NET.Charting
{
    /// <summary>Keep export choices aligned with the formats the native renderers encode.</summary>
    public static class ChartImageExport
    {
        // This order matches the localized Image_Filter resource.
        private static readonly ImageFormat[] RasterFormats =
            { ImageFormat.Png, ImageFormat.Gif, ImageFormat.Jpeg, ImageFormat.Bmp, ImageFormat.Tiff };

        public static void ConfigureDialog(SaveFileDialog dialog, IChart chart)
        {
            if (dialog == null) throw new ArgumentNullException(nameof(dialog));
            if (chart == null) throw new ArgumentNullException(nameof(chart));
            var index = dialog.FilterIndex;
            var metafile = SupportsMetafile(chart);
            dialog.Filter = Strings.Image_Filter + (metafile ? "|" + Strings.Emf_Image_Filter : string.Empty);
            // The dialog persists between uses. A prior real-chart EMF choice
            // must not remain selected after switching to another renderer.
            dialog.FilterIndex = index >= 1 && index <= RasterFormats.Length + (metafile ? 1 : 0) ? index : 1;
        }

        public static ImageFormat FormatFromFilterIndex(IChart chart, int filterIndex)
        {
            if (chart == null) throw new ArgumentNullException(nameof(chart));
            if (filterIndex >= 1 && filterIndex <= RasterFormats.Length)
                return RasterFormats[filterIndex - 1];
            if (filterIndex == RasterFormats.Length + 1 && SupportsMetafile(chart))
                return ImageFormat.Emf;
            throw new ArgumentOutOfRangeException(nameof(filterIndex));
        }

        public static ImageFormat RequireSupportedFormat(IChart chart, ImageFormat imageFormat)
        {
            if (chart == null) throw new ArgumentNullException(nameof(chart));
            if (imageFormat == null) throw new ArgumentNullException(nameof(imageFormat));
            // The legacy real-chart backend compares metafile formats by reference.
            // Preserve valid GUID-equivalent inputs while passing it the canonical value.
            foreach (var format in RasterFormats)
                if (format.Equals(imageFormat)) return format;
            if (ImageFormat.Emf.Equals(imageFormat) && SupportsMetafile(chart)) return ImageFormat.Emf;
            // GDI+ can silently save PNG for an unsupported format; WPF's WMP
            // encoder is JPEG XR, not WMF. Reject before creating/truncating a file.
            throw new NotSupportedException("The selected image format is not supported by this chart.");
        }

        private static bool SupportsMetafile(IChart chart)
        {
#if __MonoCS__
            // libgdiplus cannot create the real chart's enhanced metafile.
            return false;
#else
            return chart is IChart2D;
#endif
        }
    }
}
