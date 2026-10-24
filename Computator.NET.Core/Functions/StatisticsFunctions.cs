// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseStringInterpolation
// ReSharper disable InconsistentNaming
namespace Computator.NET.Core.Functions
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public static class StatisticsFunctions
    {
        // Built-in functions may be evaluated concurrently. Keep the separately
        // exposed rnd field unchanged for scripts which configure it themselves.
#if NETFRAMEWORK
        private static readonly Compatibility.FrameworkRandom m_random = Compatibility.FrameworkRandom.Shared;
#else
        private static readonly System.Random m_random = System.Random.Shared;
#endif
        public static System.Random rnd = new System.Random();

        public static double random(double x, double y)
        {
            return x + (y - x)*m_random.NextDouble();
        }

        public static double random(int x)
        {
            if (x < 0)
                return double.NaN;
            // The TSL integer overload includes the upper endpoint. Widen before
            // adding one so Int32.MaxValue does not wrap to Int32.MinValue.
            return m_random.NextInt64((long)x + 1);
        }

        public static double random(int x, int y)
        {
            if (x > y)
                throw new System.ArgumentOutOfRangeException(nameof(x));
            return m_random.NextInt64(x, (long)y + 1);
        }

        public static double random(double x)
        {
            return x*m_random.NextDouble();
        }

        public static System.Numerics.Complex random(System.Numerics.Complex z1, System.Numerics.Complex z2)
        {
            return z1 + (z2 - z1)*m_random.NextDouble();
        }

        public static System.Numerics.Complex random(System.Numerics.Complex z)
        {
            return new System.Numerics.Complex(z.Real*m_random.NextDouble(), z.Imaginary*m_random.NextDouble());
        }

        #region utils

        // The legacy generated-script surface must retain the same bounds and
        // concurrency behavior as the directly callable functions above.
        public const string ToCode = @"

        private static readonly Computator.NET.Core.Compatibility.FrameworkRandom m_random = Computator.NET.Core.Compatibility.FrameworkRandom.Shared;
        public static System.Random rnd = new System.Random();

        public static double random(double x, double y)
        {
            return x + (y - x)*m_random.NextDouble();
        }

        public static double random(int x)
        {
            if (x < 0)
                return double.NaN;
            return m_random.NextInt64((long)x + 1);
        }

        public static double random(int x, int y)
        {
            if (x > y)
                throw new System.ArgumentOutOfRangeException(nameof(x));
            return m_random.NextInt64(x, (long)y + 1);
        }

        public static double random(double x)
        {
            return x*m_random.NextDouble();
        }

        public static System.Numerics.Complex random(System.Numerics.Complex z1, System.Numerics.Complex z2)
        {
            return z1 + (z2 - z1)*m_random.NextDouble();
        }

        public static System.Numerics.Complex random(System.Numerics.Complex z)
        {
            return new System.Numerics.Complex(z.Real*m_random.NextDouble(), z.Imaginary*m_random.NextDouble());
        }

        ";

        #endregion
    }
}
