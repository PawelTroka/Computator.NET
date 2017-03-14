using System;
using System.Drawing;
using System.Windows.Forms;

namespace Computator.NET.Services
{
    public static class FormScalingExtensions
    {
        public static void DpiScale(this Form form)
        {
            var scaleX = (1+form.CreateGraphics().DpiX / 96)/2;
            var scaleY = (1+form.CreateGraphics().DpiY / 96)/2;

            var scaledMinWidth = (int)(form.MinimumSize.Width * scaleX);
            var scaledMinHeight = (int)(form.MinimumSize.Height * scaleY);

            var scaledWidth = (int)(form.Size.Width * scaleX);
            var scaledHeight = (int)(form.Size.Height * scaleY);

            var resolutionWidth = Screen.PrimaryScreen.Bounds.Width;
            var resolutionHeight = Screen.PrimaryScreen.Bounds.Height;


            form.MinimumSize = new Size(
                Math.Min(scaledMinWidth,resolutionWidth),
                Math.Min(scaledMinHeight,resolutionHeight));

            form.Size = new Size(
                Math.Min(scaledWidth, resolutionWidth),
                Math.Min(scaledHeight, resolutionHeight));
        }
    }
}