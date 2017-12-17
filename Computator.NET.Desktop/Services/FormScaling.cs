using System;
using System.Drawing;
using System.Windows.Forms;

namespace Computator.NET.Desktop.Services
{
    public static class ScalingExtensions
    {
        public static Size DpiScale(this Size size)
        {
            var scaleX = Graphics.FromHwnd(IntPtr.Zero).DpiX / 96;
            var scaleY = Graphics.FromHwnd(IntPtr.Zero).DpiX / 96;

            var scaledWidth = (int)(size.Width * scaleX);
            var scaledHeight = (int)(size.Height * scaleY);
            return new Size(scaledWidth, scaledHeight);
        }

        public static void DpiScale(this Control control)
        {
            var scaleX = control.CreateGraphics().DpiX / 96;
            var scaleY = control.CreateGraphics().DpiX / 96;


            var scaledMinWidth = (int)(control.MinimumSize.Width * scaleX);
            var scaledMinHeight = (int)(control.MinimumSize.Height * scaleY);

            var scaledWidth = (int)(control.Size.Width * scaleX);
            var scaledHeight = (int)(control.Size.Height * scaleY);

            var resolutionWidth = Screen.PrimaryScreen.Bounds.Width;
            var resolutionHeight = Screen.PrimaryScreen.Bounds.Height;

            control.MinimumSize = new Size(
    Math.Min(scaledMinWidth, resolutionWidth),
    Math.Min(scaledMinHeight, resolutionHeight));

            control.Size = new Size(
                Math.Min(scaledWidth, resolutionWidth),
                Math.Min(scaledHeight, resolutionHeight));
        }

        public static void MakeSureItsNotBiggerThanScreen(this Form form)
        {
            var resolutionWidth = Screen.PrimaryScreen.Bounds.Width;
            var resolutionHeight = Screen.PrimaryScreen.Bounds.Height;


            form.MinimumSize = new Size(
                Math.Min(form.MinimumSize.Width, resolutionWidth),
                Math.Min(form.MinimumSize.Height, resolutionHeight));

            form.Size = new Size(
                Math.Min(form.Size.Width, resolutionWidth),
                Math.Min(form.Size.Height, resolutionHeight));
        }
    }
}