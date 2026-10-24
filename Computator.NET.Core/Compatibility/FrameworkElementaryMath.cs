using System;
using System.Runtime.CompilerServices;
#if NETFRAMEWORK
using System.Runtime.InteropServices;
using Computator.NET.DataTypes.Configuration;
#endif

namespace Computator.NET.Core.Compatibility
{
    /// <summary>
    /// BCL operations unavailable on Framework. Reuse the existing GSL dependency
    /// for asinh/acosh instead of introducing more special-function implementations.
    /// </summary>
    public static class FrameworkElementaryMath
    {
#if NETFRAMEWORK
        [DllImport(GslConfig.GslDllName, EntryPoint = "gsl_asinh", CallingConvention = CallingConvention.Cdecl)]
        public static extern double Asinh(double value);

        [DllImport(GslConfig.GslDllName, EntryPoint = "gsl_acosh", CallingConvention = CallingConvention.Cdecl)]
        private static extern double AcoshNative(double value);

        public static double Acosh(double value)
        {
            // Match the BCL domain contract without invoking a native domain-error handler.
            if (double.IsNaN(value) || value < 1.0)
                return double.NaN;
            return AcoshNative(value);
        }

        public static double Atanh(double value)
        {
            var a = Math.Abs(value);
            if (double.IsNaN(value) || a > 1.0)
                return double.NaN;
            if (a == 1.0)
                return value > 0 ? double.PositiveInfinity : double.NegativeInfinity;
            if (a < 1e-8)
                return value;
            if (a < 0.125)
            {
                var x2 = value * value;
                return value * (1.0 + x2 * (1.0 / 3.0 + x2 * (1.0 / 5.0 + x2 * (1.0 / 7.0 + x2 * (1.0 / 9.0 +
                    x2 * (1.0 / 11.0 + x2 * (1.0 / 13.0 + x2 * (1.0 / 15.0 + x2 * (1.0 / 17.0 + x2 * (1.0 / 19.0))))))))));
            }
            return CopySign(0.5 * Math.Log((1.0 + a) / (1.0 - a)), value);
        }

        public static double CopySign(double value, double sign)
        {
            var magnitude = BitConverter.DoubleToInt64Bits(value) & long.MaxValue;
            var signBit = BitConverter.DoubleToInt64Bits(sign) & long.MinValue;
            return BitConverter.Int64BitsToDouble(magnitude | signBit);
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Asinh(double value) => Math.Asinh(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Acosh(double value) => Math.Acosh(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Atanh(double value) => Math.Atanh(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double CopySign(double value, double sign) => Math.CopySign(value, sign);
#endif
    }
}
