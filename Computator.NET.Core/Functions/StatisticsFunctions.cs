// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseStringInterpolation
namespace Computator.NET.Functions
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public static class StatisticsFunctions
    {
        private static readonly System.Random m_random = new System.Random();
        public static System.Random rnd = new System.Random();

        public static double random(double x, double y)
        {
            //  if (x == y)
            //    return x;
            //else if (y < x)
            //return double.NaN;
            return x + (y - x)*m_random.NextDouble();
        }

        public static double random(int x)
        {
            if (x <= -1)
                return double.NaN;
            return m_random.Next(x + 1);
        }

        public static double random(int x, int y)
        {
            return m_random.Next(x, y + 1);
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
            //return z * m_random.NextDouble();
            return new System.Numerics.Complex(z.Real*m_random.NextDouble(), z.Imaginary*m_random.NextDouble());
        }

        #region utils

        public const string ToCode = @"

        private static readonly System.Random m_random = new System.Random();
        public static System.Random rnd = new System.Random();

        public static double random(double x, double y)
        {
            //  if (x == y)
            //    return x;
            //else if (y < x)
            //return double.NaN;
            return x + (y - x)*m_random.NextDouble();
        }

        public static double random(int x)
        {
            if (x <= -1)
                return double.NaN;
            return m_random.Next(x + 1);
        }

        public static double random(int x, int y)
        {
            return m_random.Next(x, y + 1);
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
            //return z * m_random.NextDouble();
            return new System.Numerics.Complex(z.Real*m_random.NextDouble(), z.Imaginary*m_random.NextDouble());
        }

        ";

        #endregion
    }
}