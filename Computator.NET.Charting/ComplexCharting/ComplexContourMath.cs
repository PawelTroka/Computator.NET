using System;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Charting.ComplexCharting
{
    // Pure numerical portion of complex domain coloring. Shared by the native
    // chart and portable tests; no WinForms, GDI+ or native math dependencies.
    internal static class ComplexContourMath
    {
        internal static bool TryGetPosition(double magnitude, double step, CountourLinesMode mode,
            out double position)
        {
            position = 0;
            if (!IsFinite(magnitude) || magnitude < 0 || !IsFinite(step))
                return false;

            double lower, upper;
            switch (mode)
            {
                case CountourLinesMode.Logarithmic:
                    if (step <= 1) return false;
                    if (magnitude <= 1)
                    {
                        lower = 0;
                        upper = 1;
                        break;
                    }
                    // The previous loop multiplied by step until >= magnitude.
                    var logBand = Math.Max(1, Math.Ceiling(Math.Log(magnitude) / Math.Log(step)));
                    lower = Math.Pow(step, logBand - 1);
                    upper = Math.Pow(step, logBand);
                    break;

                case CountourLinesMode.Linear:
                    if (step <= 0) return false;
                    // The previous loop added step until >= magnitude; a very
                    // small positive step could require trillions of iterations.
                    var linearBand = Math.Max(1, Math.Ceiling(magnitude / step));
                    lower = (linearBand - 1) * step;
                    upper = linearBand * step;
                    break;

                case CountourLinesMode.Exponential:
                    if (step <= 1 || magnitude <= 0 || magnitude == 1) return false;
                    // log(m^(1/m), step) = log(m) / (m * log(step)).
                    // For m < 1 the original increment was negative, so its
                    // unbounded loop could never reach a positive magnitude.
                    // Use a positive interval width for both sides of |f(z)|=1.
                    var width = Math.Abs(Math.Log(magnitude) / (magnitude * Math.Log(step)));
                    if (!(width > 0) || !IsFinite(width)) return false;
                    var exponentialBand = Math.Max(1, Math.Ceiling(magnitude / width));
                    lower = (exponentialBand - 1) * width;
                    upper = exponentialBand * width;
                    break;

                default:
                    return false;
            }

            // Degenerate, overflowing and unrepresentably narrow bands should
            // use the chart's smooth coloring instead of looping or emitting NaN.
            var range = upper - lower;
            if (!IsFinite(lower) || !IsFinite(upper) || !(range > 0) || !IsFinite(range))
                return false;
            var fraction = (magnitude - lower) / range;
            if (!IsFinite(fraction)) return false;
            position = Math.Min(1, Math.Max(0, fraction));
            return true;
        }

        private static bool IsFinite(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
