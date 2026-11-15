using System;
using System.Collections.Generic;

namespace Computator.NET.Charting.RealCharting
{
    internal static class ChartSamplingRange
    {
        // The highest UI quality uses 47,497 intervals. Keep malformed resolutions
        // bounded too, before converting the floating-point quality result to int.
        internal const int MaximumIntervals = 50000;

        internal static double[] Create(double minimum, double maximum, double intervals)
        {
            var width = maximum - minimum;
            if (!IsFinite(minimum) || !IsFinite(maximum) || !IsFinite(width) || width <= 0 ||
                !IsFinite(intervals) || intervals < 1 || intervals > MaximumIntervals)
                return new double[0];

            var count = (int)Math.Ceiling(intervals);
            var values = new List<double>(count + 1) { minimum };
            for (var index = 1; index < count; index++)
            {
                // An integer index always advances, even when width/count is
                // smaller than one representable step at the current coordinate.
                var value = minimum + width * ((double)index / count);
                if (value > values[values.Count - 1] && value < maximum)
                    values.Add(value);
            }
            values.Add(maximum);
            return values.ToArray();
        }

        private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
