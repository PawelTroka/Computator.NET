using System;
using System.Runtime.CompilerServices;

namespace Computator.NET.Engine.Compatibility
{
    /// <summary>Framework equivalents for the adjacent IEEE-754 double APIs.</summary>
    public static class FrameworkMath
    {
#if NETFRAMEWORK
        public static double BitIncrement(double value)
        {
            if (double.IsNaN(value) || double.IsPositiveInfinity(value)) return value;
            if (value == 0) return double.Epsilon;
            var bits = BitConverter.DoubleToInt64Bits(value);
            return BitConverter.Int64BitsToDouble(bits + (bits < 0 ? -1 : 1));
        }

        public static double BitDecrement(double value)
        {
            if (double.IsNaN(value) || double.IsNegativeInfinity(value)) return value;
            if (value == 0) return -double.Epsilon;
            var bits = BitConverter.DoubleToInt64Bits(value);
            return BitConverter.Int64BitsToDouble(bits + (bits < 0 ? 1 : -1));
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double BitIncrement(double value) => Math.BitIncrement(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double BitDecrement(double value) => Math.BitDecrement(value);
#endif
    }
}
