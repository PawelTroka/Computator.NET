using System;
using System.Globalization;

namespace Computator.NET.Core.Evaluation
{
    /// <summary>Invariant decimal display without an intermediate binary floating-point value.</summary>
    public static class DecimalDisplay
    {
        public static string Format(decimal value)
        {
            var magnitude = Math.Abs(value);
            // Retain the original compact display boundaries, while preserving all
            // decimal digits instead of rounding them through a double conversion.
            var scientific = magnitude != 0 && (magnitude < 0.0001m || magnitude >= 1_000_000_000_000_000m);
            return value.ToString(scientific ? "0.############################E+0" : "0.############################",
                CultureInfo.InvariantCulture);
        }
    }
}
