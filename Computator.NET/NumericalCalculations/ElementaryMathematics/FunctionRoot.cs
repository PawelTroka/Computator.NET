using System;
using MathNet.Numerics.RootFinding;

namespace Computator.NET.NumericalCalculations.ElementaryMathematics
{
    internal class FunctionRoot
    {
        private const double N_MAX = 1e5;

        public static double BrentMethod(Func<double, double> fx, double a, double b, double N = N_MAX)
        {
            double err = ((a + b)/2.0)*1e-15;

            if (a + b == 0)
                err = 1e-20;
            double root;

            if (Brent.TryFindRoot(fx, a, b, err, (int) N, out root))
                return root;
            return double.NaN;
        }

        public static double BroydenMethod(Func<double, double> fx, double a, double b, double N = N_MAX)
        {
            double err = ((a + b)/2.0)*1e-15;

            if (a + b == 0)
                err = 1e-20;
            double[] root;

            Func<double[], double[]> function = (variables =>
            {
                var results = new double[variables.Length];

                for (int i = 0; i < variables.Length; i++)
                    results[i] = fx(variables[i]);

                return results;
            });

            if (Broyden.TryFindRoot(function, new[] {(b + a)/2}, err, (int) N, out root))
                return root[0];
            return double.NaN;
        }

        public static double secantMethod(Func<double, double> fx, double a, double b, double N = N_MAX)
        {
            double x1 = a, x2 = b, xOld;

            for (int i = 0; i < N; i++)
            {
                if (x2 - x1 == 0.0 || (fx(x2) - fx(x1)) == 0.0 || fx(x2) == 0.0)
                    break;
                xOld = x2;
                x2 = x2 - (fx(x2)*(x2 - x1))/(fx(x2) - fx(x1));
                x1 = xOld;
            }
            return x2;
        }

        public static double bisectionMethod(Func<double, double> fx, double a, double b, double N = N_MAX)
        {
            double x = (a + b)/2.0;

            for (int i = 0; i < N; i++)
            {
                //first interval - [a,x]
                if (fx(a)*fx(x) < 0)
                    b = x;

                    //second interval - [x,b]
                else if (fx(b)*fx(x) < 0)
                    a = x;
                else
                {
                    if (fx(a) == 0)
                        return a;
                    if (fx(b) == 0)
                        return b;
                }

                if (a == b) return a;
                x = (a + b)/2.0;
            }
            return x;
        }
    }
}