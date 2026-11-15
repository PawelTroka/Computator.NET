using System;

namespace Computator.NET.Core.Helpers
{
    public static class DoubleExtensions
    {
        public static decimal RoundToSignificantDigits(this decimal value, int digits)
        {
            if (digits < 1 || digits > 29)
                throw new ArgumentOutOfRangeException(nameof(digits), "A decimal has at most 29 significant digits.");

            if (value == 0m)
                return 0m;

            // Work out the decimal order of magnitude without converting to double.
            // Converting near 10^28 to double can round the value up to the next
            // power of ten, making the old 10^(order + 1) cast overflow.
            var magnitude = Math.Abs(value);
            var order = 0;
            while (magnitude >= 10m)
            {
                magnitude /= 10m;
                order++;
            }

            while (magnitude < 1m)
            {
                magnitude *= 10m;
                order--;
            }

            var decimalPlaces = digits - 1 - order;
            if (decimalPlaces >= 28)
                return value; // No more fractional precision exists in decimal.

            if (decimalPlaces >= 0)
                return Math.Round(value, decimalPlaces, MidpointRounding.ToEven);

            // Math.Round only accepts nonnegative decimal places. For large values,
            // round the scaled decimal instead; 10^28 fits within decimal's range.
            var scale = 1m;
            for (var i = 0; i < -decimalPlaces; i++)
                scale *= 10m;

            return Math.Round(value / scale, 0, MidpointRounding.ToEven) * scale;
        }
    }
}
