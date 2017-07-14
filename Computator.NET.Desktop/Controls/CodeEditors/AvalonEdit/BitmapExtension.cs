#if !__MonoCS__
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Computator.NET.Core.Natives;

namespace Computator.NET.Desktop.Controls.CodeEditors.AvalonEdit
{
    public static class BitmapExtension
    {
        /// <summary>
        ///     Converts a <see cref="System.Drawing.Image" /> into a WPF <see cref="System.Windows.Media.Imaging.BitmapSource" />.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <returns>A BitmapSource</returns>
        public static BitmapSource ToBitmapSource(this Image source)
        {
            var bitmap = new Bitmap(source);

            var bitSrc = ToBitmapSource((Image) bitmap);

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
        public static BitmapSource ToBitmapSource(this Bitmap source)
        {
            BitmapSource bitSrc = null;

            var hBitmap = source.GetHbitmap();

            try
            {
                bitSrc = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception)
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
#endif