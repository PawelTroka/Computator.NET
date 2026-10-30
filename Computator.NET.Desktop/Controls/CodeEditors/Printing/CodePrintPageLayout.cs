using System;
using System.Drawing;

namespace Computator.NET.Desktop.Controls.CodeEditors.Printing
{
    internal static class CodePrintPageLayout
    {
        // Both rectangles use physical page coordinates in hundredths of an
        // inch. OriginAtMargins=true makes drawing relative to the user's
        // margin origin, not the printer's asymmetric hard-margin origin.
        internal static RectangleF ContentBounds(Rectangle marginBounds, RectangleF printableArea)
        {
            if (marginBounds.Width <= 0 || marginBounds.Height <= 0 ||
                !IsFinite(printableArea.X) || !IsFinite(printableArea.Y) ||
                !IsFinite(printableArea.Width) || !IsFinite(printableArea.Height) ||
                printableArea.Width <= 0 || printableArea.Height <= 0)
                throw new CodePrintLayoutException("The printer has no usable page area. Check the paper size and margins.");

            var physical = RectangleF.Intersect(marginBounds, printableArea);
            if (physical.Width <= 0 || physical.Height <= 0)
                throw new CodePrintLayoutException("The selected margins are outside the printer's printable area.");
            physical.Offset(-marginBounds.Left, -marginBounds.Top);
            return physical;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
