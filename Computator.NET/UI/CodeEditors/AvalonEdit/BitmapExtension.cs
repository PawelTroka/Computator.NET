namespace Computator.NET.UI.CodeEditors
{
    public static class BitmapExtension
    {
        /// <summary>
        ///     Converts a <see cref="System.Drawing.Image" /> into a WPF <see cref="System.Windows.Media.Imaging.BitmapSource" />.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <returns>A BitmapSource</returns>
        public static System.Windows.Media.Imaging.BitmapSource ToBitmapSource(this System.Drawing.Image source)
        {
            var bitmap = new System.Drawing.Bitmap(source);

            var bitSrc = ToBitmapSource((System.Drawing.Image) bitmap);

            bitmap.Dispose();
            bitmap = null;

            return bitSrc;
        }

        /// <summary>
        ///     Converts a <see cref="System.Drawing.Bitmap" /> into a WPF <see cref="System.Windows.Media.Imaging.BitmapSource" />
        ///     .
        /// </summary>
        /// <remarks>
        ///     Uses GDI to do the conversion. Hence the call to the marshalled DeleteObject.
        /// </remarks>
        /// <param name="source">The source bitmap.</param>
        /// <returns>A BitmapSource</returns>
        public static System.Windows.Media.Imaging.BitmapSource ToBitmapSource(this System.Drawing.Bitmap source)
        {
            System.Windows.Media.Imaging.BitmapSource bitSrc = null;

            var hBitmap = source.GetHbitmap();

            try
            {
                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    System.IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            catch (System.ComponentModel.Win32Exception)
            {
                bitSrc = null;
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            }

            return bitSrc;
        }
    }
}