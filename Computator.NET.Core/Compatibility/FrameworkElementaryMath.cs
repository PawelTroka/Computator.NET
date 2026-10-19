#if NETFRAMEWORK
using System;
using System.Runtime.InteropServices;
using Computator.NET.DataTypes.Configuration;

namespace Computator.NET.Core.Compatibility
{
    /// <summary>
    /// BCL operations unavailable on Framework. Reuse the existing GSL dependency
    /// for asinh instead of introducing a second special-function implementation.
    /// </summary>
    public static class FrameworkElementaryMath
    {
        [DllImport(GslConfig.GslDllName, EntryPoint = "gsl_asinh", CallingConvention = CallingConvention.Cdecl)]
        public static extern double Asinh(double value);

        public static double CopySign(double value, double sign)
        {
            var magnitude = BitConverter.DoubleToInt64Bits(value) & long.MaxValue;
            var signBit = BitConverter.DoubleToInt64Bits(sign) & long.MinValue;
            return BitConverter.Int64BitsToDouble(magnitude | signBit);
        }
    }
}
#endif
