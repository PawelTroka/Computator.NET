using System;

namespace Computator.NET.UI.Controls
{
    public static class DoubleExtensions
    {
        public static decimal RoundToSignificantDigits(this decimal d, int digits)
        {
            if (d == 0)
                return 0;

            var scale = (decimal) Math.Pow(10, (double) (Math.Floor((decimal) Math.Log10(Math.Abs((double) d))) + 1.0m));
            return scale*Math.Round(d/scale, digits);
        }
    }
}