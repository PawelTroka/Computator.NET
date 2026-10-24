#nullable disable

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Computator.NET.Charting.Axes
{
    // Coordinates and text only: the WinForms and WPF renderers measure their
    // own fonts and pass the available axis length and desired label spacing in
    // the same units (physical pixels or DIPs). No display or GDI state is used.
    internal static class AxisTickGenerator
    {
        private const int MaximumMajorIntervals = 128;
        private static readonly double[] NiceMultipliers = { 1, 2, 2.5, 5, 10 };

        public static AxisTickSet Create(double minimum, double maximum, double length,
            double minimumMajorSpacing = 64, CultureInfo culture = null)
        {
            if (!IsFinite(minimum) || !IsFinite(maximum) || minimum >= maximum ||
                !IsFinite(length) || !IsFinite(minimumMajorSpacing) ||
                minimumMajorSpacing <= 0 || length < minimumMajorSpacing / 2 || length <= 0)
                return AxisTickSet.Empty;

            culture = culture ?? CultureInfo.InvariantCulture;
            // Two target intervals ensure at least one globally aligned tick in
            // short viewports. The renderer can omit labels that do not fit.
            // Keep the budget fractional: flooring it before choosing a nice
            // interval can discard values whose actual pixel separation fits.
            var intervals = Math.Max(2, Math.Min(MaximumMajorIntervals,
                length / minimumMajorSpacing));
            var span = maximum - minimum;
            var requestedStep = IsFinite(span)
                ? span / intervals
                : maximum / intervals - minimum / intervals;
            var representableStep = Math.Max(Ulp(minimum), Ulp(maximum));
            var majorStep = NiceStep(Math.Max(requestedStep, representableStep));
            var majorTicks = Generate(minimum, maximum, majorStep,
                MaximumMajorIntervals + 4, 1, culture);

            // At the precision limit, retaining enough significant digits is
            // preferable to displaying two different coordinates with one label.
            var labels = new HashSet<string>(StringComparer.Ordinal);
            var repeatedLabel = false;
            foreach (var tick in majorTicks)
                repeatedLabel |= !labels.Add(tick.Label);
            if (repeatedLabel)
            {
                for (var index = 0; index < majorTicks.Count; index++)
                {
                    var tick = majorTicks[index];
                    majorTicks[index] = new AxisTick(tick.Value, tick.Fraction,
                        RoundTripLabel(tick.Value, culture));
                }
            }

            var majorPixelSpacing = IsFinite(span)
                ? majorStep / span * length
                : majorStep / Math.Max(Math.Abs(minimum), Math.Abs(maximum)) /
                  (maximum / Math.Max(Math.Abs(minimum), Math.Abs(maximum)) -
                   minimum / Math.Max(Math.Abs(minimum), Math.Abs(maximum))) * length;
            var divisions = PreferredSubdivisions(majorStep);
            // The cutoff scales with the requested major spacing, preserving
            // subdivisions when both the viewport and font scale change with DPI.
            if (majorPixelSpacing / divisions < minimumMajorSpacing / 6 ||
                majorStep / divisions < representableStep)
                divisions = 2;
            if (majorPixelSpacing / divisions < minimumMajorSpacing / 6 ||
                majorStep / divisions < representableStep)
                divisions = 1;

            var minorTicks = divisions > 1
                ? Generate(minimum, maximum, majorStep / divisions,
                    MaximumMajorIntervals * divisions + 12, divisions, culture)
                : new List<AxisTick>();
            return new AxisTickSet(majorTicks.AsReadOnly(), minorTicks.AsReadOnly(), majorStep);
        }

        // Also used for a single constant coordinate on a collapsed 3D axis.
        // A zero/invalid step requests six significant digits instead of guessing
        // a fictitious interval. Ordinary ticks always supply their actual step.
        public static string Format(double value, double step, CultureInfo culture = null)
        {
            culture = culture ?? CultureInfo.InvariantCulture;
            if (value == 0) return "0";
            if (!IsFinite(value)) return value.ToString(culture);

            var magnitude = Math.Abs(value);
            var valueExponent = (int)Math.Floor(Math.Log10(magnitude));
            var hasStep = IsFinite(step) && step > 0;
            var stepExponent = hasStep ? (int)Math.Floor(Math.Log10(step)) : valueExponent - 5;
            // 2.5-decade intervals need the extra decimal digit (0.25, 0.75,
            // etc.). Other nice intervals are integral multiples of their decade.
            var extraDigit = hasStep && IsHalfDecade(step, stepExponent) ? 1 : 0;
            string result;
            if (magnitude >= 1e6 || magnitude < 1e-4)
            {
                var decimals = Math.Max(0, Math.Min(16, valueExponent - stepExponent + extraDigit));
                result = CompactScientific(value.ToString("E" + decimals, culture), culture);
            }
            else
            {
                var decimals = Math.Max(0, Math.Min(340, -stepExponent + extraDigit));
                result = TrimFractionZeros(value.ToString("F" + decimals, culture), culture);
            }

            // Rounding double.MaxValue can describe an infinite value. Close to
            // a large offset, even one ULP of rounding can be a visible fraction
            // of the interval. Preserve the actual coordinate in both cases.
            if (double.TryParse(result, NumberStyles.Float, culture, out var parsed) &&
                (!IsFinite(parsed) || parsed == 0 ||
                 (hasStep && Math.Abs(parsed - value) / step > 1e-8)))
                result = RoundTripLabel(value, culture);
            return result;
        }

        private static string RoundTripLabel(double value, CultureInfo culture)
        {
            if (value == 0) return "0";
            var magnitude = Math.Abs(value);
            return magnitude >= 1e6 || magnitude < 1e-4
                ? CompactScientific(value.ToString("E16", culture), culture)
                : value.ToString("R", culture);
        }

        private static List<AxisTick> Generate(double minimum, double maximum, double step,
            int maximumCandidates, int majorDivisions, CultureInfo culture)
        {
            var ticks = new List<AxisTick>();
            var firstIndex = Math.Floor(minimum / step);
            var previousIndex = double.NaN;
            var previousValue = double.NegativeInfinity;
            // Compute each coordinate from an integer grid index; adding step to
            // a floating point coordinate accumulates drift and can stop advancing.
            for (var offset = 0; offset < maximumCandidates; offset++)
            {
                var index = firstIndex + offset;
                if (index == previousIndex) continue;
                previousIndex = index;
                var value = index * step;
                if (value != 0 && value < minimum && value >= BitDecrement(minimum) &&
                    (minimum - value) / step <= 1e-10)
                    value = minimum;
                if (value != 0 && value > maximum && value <= BitIncrement(maximum) &&
                    (value - maximum) / step <= 1e-10)
                    value = maximum;
                if (value > maximum) break;
                if (!IsFinite(value) || value < minimum || value <= previousValue ||
                    (majorDivisions > 1 && index % majorDivisions == 0))
                    continue;
                // In particular, suppress the sign bit of a mathematical zero.
                if (value == 0) value = 0;
                ticks.Add(new AxisTick(value, Fraction(value, minimum, maximum),
                    majorDivisions > 1 ? string.Empty : Format(value, step, culture)));
                previousValue = value;
            }
            return ticks;
        }

        private static double Fraction(double value, double minimum, double maximum)
        {
            var span = maximum - minimum;
            double fraction;
            if (IsFinite(span))
                fraction = (value - minimum) / span;
            else
            {
                var scale = Math.Max(Math.Abs(minimum), Math.Abs(maximum));
                fraction = (value / scale - minimum / scale) /
                    (maximum / scale - minimum / scale);
            }
            return Math.Max(0, Math.Min(1, fraction));
        }

        private static double NiceStep(double minimum)
        {
            if (minimum <= double.Epsilon) return double.Epsilon;
            if (!IsFinite(minimum)) return double.MaxValue;
            var exponent = Math.Floor(Math.Log10(minimum));
            var decade = Math.Pow(10, exponent);
            // The smallest representable doubles have no representable 10^-324.
            if (decade == 0) decade = Math.Pow(10, exponent + 1);
            foreach (var multiplier in NiceMultipliers)
            {
                var candidate = multiplier * decade;
                if (!IsFinite(candidate)) return double.MaxValue;
                if (candidate >= minimum || (minimum - candidate) / minimum < 1e-12)
                    return candidate;
            }
            return double.MaxValue;
        }

        private static int PreferredSubdivisions(double step)
        {
            var decade = Math.Pow(10, Math.Floor(Math.Log10(step)));
            return decade > 0 && Math.Abs(step / decade - 2) < 1e-10 ? 4 : 5;
        }

        private static bool IsHalfDecade(double step, int exponent)
        {
            var decade = Math.Pow(10, exponent);
            return decade > 0 && Math.Abs(step / decade - 2.5) < 1e-10;
        }

        private static double Ulp(double value)
        {
            var magnitude = Math.Abs(value);
            var next = BitIncrement(magnitude);
            return IsFinite(next) ? next - magnitude : magnitude - BitDecrement(magnitude);
        }

        private static string CompactScientific(string text, CultureInfo culture)
        {
            var exponentPosition = text.IndexOf('E');
            if (exponentPosition < 0) return text;
            var mantissa = TrimFractionZeros(text.Substring(0, exponentPosition), culture);
            if (!int.TryParse(text.Substring(exponentPosition + 1), NumberStyles.Integer,
                culture, out var exponent))
                return text;
            return mantissa + "e" + (exponent >= 0 ? culture.NumberFormat.PositiveSign : string.Empty) +
                exponent.ToString(culture);
        }

        private static string TrimFractionZeros(string text, CultureInfo culture)
        {
            var separator = culture.NumberFormat.NumberDecimalSeparator;
            if (text.IndexOf(separator, StringComparison.Ordinal) < 0) return text;
            text = text.TrimEnd('0');
            return text.EndsWith(separator, StringComparison.Ordinal)
                ? text.Substring(0, text.Length - separator.Length) : text;
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

#if NETFRAMEWORK
        private static double BitIncrement(double value)
        {
            if (double.IsNaN(value) || double.IsPositiveInfinity(value)) return value;
            if (value == 0) return double.Epsilon;
            var bits = BitConverter.DoubleToInt64Bits(value);
            return BitConverter.Int64BitsToDouble(bits + (bits < 0 ? -1 : 1));
        }

        private static double BitDecrement(double value)
        {
            if (double.IsNaN(value) || double.IsNegativeInfinity(value)) return value;
            if (value == 0) return -double.Epsilon;
            var bits = BitConverter.DoubleToInt64Bits(value);
            return BitConverter.Int64BitsToDouble(bits + (bits < 0 ? 1 : -1));
        }
#else
        private static double BitIncrement(double value) => Math.BitIncrement(value);
        private static double BitDecrement(double value) => Math.BitDecrement(value);
#endif
    }

    internal sealed class AxisTickSet
    {
        public static readonly AxisTickSet Empty = new AxisTickSet(
            Array.Empty<AxisTick>(), Array.Empty<AxisTick>(), 0);

        public AxisTickSet(IReadOnlyList<AxisTick> majorTicks, IReadOnlyList<AxisTick> minorTicks, double majorStep)
        {
            MajorTicks = majorTicks;
            MinorTicks = minorTicks;
            MajorStep = majorStep;
        }

        public IReadOnlyList<AxisTick> MajorTicks { get; }
        public IReadOnlyList<AxisTick> MinorTicks { get; }
        public double MajorStep { get; }
    }

    internal sealed class AxisTick
    {
        public AxisTick(double value, double fraction, string label)
        {
            Value = value;
            Fraction = fraction;
            Label = label;
        }

        public double Value { get; }
        // Mathematical minimum-to-maximum position. Renderers invert the Y axis.
        public double Fraction { get; }
        public string Label { get; }
    }
}
