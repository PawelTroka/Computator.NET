using System;

namespace Computator.NET.Core.Helpers
{
    public static class DoubleExtensions
    {
        public static decimal RoundToSignificantDigits(this decimal d, int digits)
        {
            if (digits < 1 || digits > 28)
                throw new ArgumentOutOfRangeException(nameof(digits), "Significant digits must be between 1 and 28.");

            if (d == 0)
                return 0;

            // Normalize using decimal arithmetic. A floating-point log can round a
            // value just below a power of ten up to that power; casting 10^29 back
            // to decimal also overflows for otherwise representable inputs.
            var magnitude = Math.Abs(d);
            var scale = 1m;
            if (magnitude >= 10m)
            {
                while (magnitude >= 10m)
                {
                    magnitude /= 10m;
                    scale *= 10m;
                }
                return Math.Round(d / scale, digits - 1) * scale;
            }

            if (magnitude < 1m)
            {
                while (magnitude < 1m)
                {
                    magnitude *= 10m;
                    scale *= 10m;
                }
                return Math.Round(d * scale, digits - 1) / scale;
            }

            return Math.Round(d, digits - 1);
        }
    }
}
