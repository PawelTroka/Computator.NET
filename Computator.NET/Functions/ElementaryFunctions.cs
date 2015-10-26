using System;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.InteropServices;
using MathNet.Numerics;
using Meta.Numerics.Functions;

namespace Computator.NET.Functions
{
    [StructLayout(LayoutKind.Sequential)]
    internal static class ElementaryFunctions
        //TODO: make functions appear in right order (etc first sin(x) then sin(z) then cos(x) ...)
    {
        #region trigonometric functions

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double sinc(double x)
        {
            if (x == 0)
                return 1;
            return (Math.Sin((x)))/(x);
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double sin(double x)
        {
            return Math.Sin((x));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double cos(double x)
        {
            return Math.Cos((x));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double tan(double x)
        {
            return Math.Tan((x));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double tg(double x)
        {
            return Math.Tan((x));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double ctg(double x)
        {
            return 1.0/Math.Tan((x));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double cot(double x)
        {
            return 1.0/Math.Tan((x));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double sec(double x)
        {
            return 1.0/Math.Cos((x));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double csc(double x)
        {
            return 1.0/Math.Sin((x));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double cosec(double x)
        {
            return 1.0/Math.Sin((x));
        }

        #endregion

        #region hyperbolic trygonometric functions

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double sinh(double x)
        {
            return Math.Sinh((x));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double cosh(double x)
        {
            return Math.Cosh((x));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double tanh(double x)
        {
            return Math.Tanh((x));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double tgh(double x)
        {
            return Math.Tanh((x));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double coth(double x)
        {
            return (1.0/Math.Tanh((x)));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double ctgh(double x)
        {
            return (1.0/Math.Tanh((x)));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double sech(double x)
        {
            return (1.0/Math.Cosh((x)));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double csch(double x)
        {
            return (1.0/Math.Sinh((x)));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double cosech(double x)
        {
            return (1.0/Math.Sinh((x)));
        }

        #endregion

        #region arcus trigonometric functions

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arcsin(double x)
        {
            return Math.Asin((x));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arccos(double x)
        {
            return Math.Acos((x));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arctan(double x)
        {
            return Math.Atan((x));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arctg(double x)
        {
            return Math.Atan((x));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arccot(double x)
        {
            return (Math.PI/2 - Math.Atan((x)));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arcctg(double x)
        {
            return (Math.PI/2 - Math.Atan((x)));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arcsec(double x)
        {
            return (Math.Acos(1.0/(x)));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arccsc(double x)
        {
            return (Math.Asin(1.0/(x)));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arccosec(double x)
        {
            return (Math.Asin(1.0/(x)));
        }

        #endregion

        #region area hyperbolic trigonometric functions

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arsinh(double x)
        {
            return Math.Log((x)) + Math.Sqrt((x)*(x) + 1);
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arcosh(double x)
        {
            return Math.Log((x) + Math.Sqrt(x + 1)*Math.Sqrt(x - 1));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double artanh(double x)
        {
            return 0.5*Math.Log((1 + x)/(1 - x));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double artgh(double x)
        {
            return 0.5*Math.Log((1 + x)/(1 - x));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arcoth(double x)
        {
            return 0.5*Math.Log((x + 1)/(x - 1));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arctgh(double x)
        {
            return 0.5*Math.Log((x + 1)/(x - 1));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arcsch(double x)
        {
            return Math.Log((1.0/x) + Math.Sqrt((1.0/(x*x)) + 1));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arcosech(double x)
        {
            return arcsch(x);
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arsech(double x)
        {
            return Math.Log((1.0/x) + Math.Sqrt((1.0/x) + 1)*Math.Sqrt((1.0/x) - 1));
        }

        #endregion

        #region logarithmic functions

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double ln(double x)
        {
            return Math.Log(x);
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double log2(double x)
        {
            return Math.Log(x, 2);
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double log10(double x)
        {
            return Math.Log10(x);
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double log(double value, double nbase)
        {
            return Math.Log(value, nbase);
        }

        #endregion

        #region root functions

        [Name("Square root"),
         Description(
             "square root of a number a is a number y such that y2 = x, in other words, a number y whose square (the result of multiplying the number by itself, or y × y) is x."
             ), DisplayName("Angielska nazwa funkcji"),
         Category("doubleu wpisz nazwę regionu (np. trigonometric functions)")]
        public static double sqrt(double x)
        {
            return Math.Sqrt(x);
        }

        [Name("Root function"), Description("root function of the given value"), DisplayName("Angielska nazwa funkcji"),
         Category("doubleu wpisz nazwę regionu (np. trigonometric functions)")]
        public static double root(double value, double n)
        {
            //n-based root
            return Math.Pow(value, 1.0/n);
        }

        #endregion

        #region power functions

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double pow(double x, double y)
        {
            return Math.Pow(x, y);
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double exp(double x)
        {
            return Math.Exp(x);
        }

        #endregion

        #region trigonometric functions

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex sinc(Complex z)
        {
            if (z == 0)
                return 1;
            return (Complex.Sin((z)))/(z);
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex sin(Complex z)
        {
            return Complex.Sin((z));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex cos(Complex z)
        {
            return Complex.Cos((z));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex tan(Complex z)
        {
            return Complex.Tan((z));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex tg(Complex z)
        {
            return Complex.Tan((z));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex ctg(Complex z)
        {
            return 1.0/Complex.Tan((z));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex cot(Complex z)
        {
            return 1.0/Complex.Tan((z));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex sec(Complex z)
        {
            return 1.0/Complex.Cos((z));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex csc(Complex z)
        {
            return 1.0/Complex.Sin((z));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex cosec(Complex z)
        {
            return 1.0/Complex.Sin((z));
        }

        #endregion

        #region hyperbolic trygonometric functions

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex sinh(Complex z)
        {
            return Complex.Sinh((z));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex cosh(Complex z)
        {
            return Complex.Cosh((z));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex tanh(Complex z)
        {
            return Complex.Tanh((z));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex tgh(Complex z)
        {
            return Complex.Tanh((z));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex coth(Complex z)
        {
            return (1.0/Complex.Tanh((z)));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex ctgh(Complex z)
        {
            return (1.0/Complex.Tanh((z)));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex sech(Complex z)
        {
            return (1.0/Complex.Cosh((z)));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex csch(Complex z)
        {
            return (1.0/Complex.Sinh((z)));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex cosech(Complex z)
        {
            return (1.0/Complex.Sinh((z)));
        }

        #endregion

        #region arcus trigonometric functions

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arcsin(Complex z)
        {
            return Complex.Asin((z));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arccos(Complex z)
        {
            return Complex.Acos((z));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arctan(Complex z)
        {
            return Complex.Atan((z));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arctg(Complex z)
        {
            return Complex.Atan((z));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arccot(Complex z)
        {
            return (Math.PI/2 - Complex.Atan((z)));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arcctg(Complex z)
        {
            return (Math.PI/2 - Complex.Atan((z)));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arcsec(Complex z)
        {
            return (Complex.Acos(1.0/(z)));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arccsc(Complex z)
        {
            return (Complex.Asin(1.0/(z)));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arccosec(Complex z)
        {
            return (Complex.Asin(1.0/(z)));
        }

        #endregion

        #region area hyperbolic trigonometric functions

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arsinh(Complex z)
        {
            return Complex.Log((z)) + Complex.Sqrt((z)*(z) + 1);
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arcosh(Complex z)
        {
            return Complex.Log((z) + Complex.Sqrt(z + 1)*Complex.Sqrt(z - 1));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex artanh(Complex z)
        {
            return 0.5*Complex.Log((1 + z)/(1 - z));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex artgh(Complex z)
        {
            return 0.5*Complex.Log((1 + z)/(1 - z));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arcoth(Complex z)
        {
            return 0.5*Complex.Log((z + 1)/(z - 1));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arctgh(Complex z)
        {
            return 0.5*Complex.Log((z + 1)/(z - 1));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arcsch(Complex z)
        {
            return Complex.Log((1.0/z) + Complex.Sqrt((1.0/(z*z)) + 1));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arcosech(Complex z)
        {
            return arcsch(z);
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arsech(Complex z)
        {
            return Complex.Log((1.0/z) + Complex.Sqrt((1.0/z) + 1)*Complex.Sqrt((1.0/z) - 1));
        }

        #endregion

        #region logarithmic functions

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex ln(Complex z)
        {
            return Complex.Log(z);
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex log2(Complex z)
        {
            return Complex.Log(z, 2);
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex log10(Complex z)
        {
            return Complex.Log10(z);
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex log(Complex z, double nbase)
        {
            return Complex.Log(z, nbase);
        }

        #endregion

        #region root functions

        [Name("Square root"),
         Description(
             "square root of a number a is a number y such that y2 = x, in other words, a number y whose square (the result of multiplying the number by itself, or y × y) is x."
             ), DisplayName("Angielska nazwa funkcji"),
         Category("Complexu wpisz nazwę regionu (np. trigonometric functions)")]
        public static Complex sqrt(Complex z)
        {
            return Complex.Sqrt(z);
        }

        [Name("Root function"), Description("root function of the given value"), DisplayName("Angielska nazwa funkcji"),
         Category("Complexu wpisz nazwę regionu (np. trigonometric functions)")]
        public static Complex root(Complex z, Complex n)
        {
            //n-based root
            return Complex.Pow(z, 1.0/n);
        }

        #endregion

        #region power functions

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex pow(Complex x, Complex y)
        {
            return Complex.Pow(x, y);
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex exp(Complex z)
        {
            return Complex.Exp(z);
        }

        #endregion

        #region integer functions

        [Name("Angielska nazwa funkcji"), Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")] public static Func<double, double> sgn =
             x => (double.IsNaN(x)) ? double.NaN : (double) (Math.Sign(x));

        [Name("Angielska nazwa funkcji"), Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")] public static Func<long, long, long> GCF = AdvancedIntegerMath.GCF,
            NWD = AdvancedIntegerMath.GCF;

        [Name("Angielska nazwa funkcji"), Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")] public static Func<long, long, long> LCM = AdvancedIntegerMath.LCM,
            NWW = AdvancedIntegerMath.LCM;

        [Name("Angielska nazwa funkcji"), Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double factorial(double n)
        {
            double f = 1;
            for (ulong i = 1; i <= n; i++)
                f = f*i;
            return f;
        }

        [Name("Angielska nazwa funkcji"), Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static int prime(double d)
        {
            if (d < 1)
                return -1;
            if (AdvancedIntegerMath.IsPrime((int) (d)))
                return 1;
            return 0;
        }

        #endregion

        #region complex specific functions

        [Name("Angielska nazwa funkcji"), Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")] public static Func<Complex, Complex> conj = Complex.Conjugate;

        [Name("Angielska nazwa funkcji"), Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")] public static Func<Complex, double> Re = z => z.Real;

        [Name("Angielska nazwa funkcji"), Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")] public static Func<Complex, double> Im = z => z.Imaginary;

        [Name("Angielska nazwa funkcji"), Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")] public static Func<Complex, double> Phase = z => z.Phase;

        [Name("Angielska nazwa funkcji"), Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")] public static Func<Complex, double> Magnitude = z => z.Magnitude;

        #endregion

        #region a step like functions (non continuous)

        [Name("Angielska nazwa funkcji"), Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")] public static Func<double, double> H = Heaviside;

        [Name("Angielska nazwa funkcji"), Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")] public static Func<double, double> δ = DiracDelta;

        [Name("Angielska nazwa funkcji"), Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")] public static Func<double, double, double> δij = KroneckerDelta;

        public static double abs(double x)
        {
            if (x >= 0.0)
                return x;
            return -x;
        }

        public static Complex abs(Complex z)
        {
            return Complex.Abs(z);
        }

        [Name("Angielska nazwa funkcji"), Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double Heaviside(double x)
        {
            if (x > 0.0)
                return 1;
            if (x < 0.0)
                return 0;
            return 0.5;
        }


        [Name("Angielska nazwa funkcji"), Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double DiracDelta(double d)
        {
            if (d != 0.0)
                return 0;
            return double.PositiveInfinity;
        }

        [Name("Angielska nazwa funkcji"), Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double KroneckerDelta(double i, double j)
        {
            if (i != j)
                return 0;
            return 1;
        }

        #endregion

        #region coefficients and special values

        [DllImport(gslSfLibDir2, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_taylorcoeff(int n, double x);

        [DllImport(gslSfLibDir2, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_doublefact(uint n);

        [DllImport(gslSfLibDir2, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_choose(uint n, uint m);

        public static double TaylorCoeff(int n, double x)
        {
            if (n < 0 || x < 0) return double.NaN;
            return gsl_sf_taylorcoeff(n, x);
        }

        public static double DoubleFactorial(uint n)
        {
            return gsl_sf_doublefact(n);
        }

        public static double BinomialCoeff(uint n, uint k)
        {
            return gsl_sf_choose(n, k);
        }

        #endregion

        #region combinatorics

        public static double VariationsWithRepetition(int n, int k)
        {
            return Combinatorics.VariationsWithRepetition(n, k);
        }

        public static double Variations(int n, int k)
        {
            return Combinatorics.Variations(n, k);
        }

        public static double CombinationsWithRepetition(int n, int k)
        {
            return Combinatorics.CombinationsWithRepetition(n, k);
        }

        public static double Combinations(int n, int k)
        {
            return Combinatorics.Combinations(n, k);
        }

        public static double Permutations(int n, int k)
        {
            return Combinatorics.Permutations(n);
        }

        #endregion

        #region utils

        private const string gslSfLibDir2 = "gsl.dll"; //"libgsl-0.dll";

        public const string ToCode =
            @"

        #region utils

        private const string gslSfLibDir2 = ""gsl.dll""; //""libgsl-0.dll"";

        #endregion


        #region trigonometric functions

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double sinc(double x)
        {
            if (x == 0)
                return 1;
            return (Math.Sin((x)))/(x);
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double sin(double x)
        {
            return Math.Sin((x));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double cos(double x)
        {
            return Math.Cos((x));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double tan(double x)
        {
            return Math.Tan((x));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double tg(double x)
        {
            return Math.Tan((x));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double ctg(double x)
        {
            return 1.0/Math.Tan((x));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double cot(double x)
        {
            return 1.0/Math.Tan((x));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double sec(double x)
        {
            return 1.0/Math.Cos((x));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double csc(double x)
        {
            return 1.0/Math.Sin((x));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double cosec(double x)
        {
            return 1.0/Math.Sin((x));
        }

        #endregion

        #region hyperbolic trygonometric functions

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double sinh(double x)
        {
            return Math.Sinh((x));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double cosh(double x)
        {
            return Math.Cosh((x));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double tanh(double x)
        {
            return Math.Tanh((x));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double tgh(double x)
        {
            return Math.Tanh((x));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double coth(double x)
        {
            return (1.0/Math.Tanh((x)));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double ctgh(double x)
        {
            return (1.0/Math.Tanh((x)));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double sech(double x)
        {
            return (1.0/Math.Cosh((x)));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double csch(double x)
        {
            return (1.0/Math.Sinh((x)));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double cosech(double x)
        {
            return (1.0/Math.Sinh((x)));
        }

        #endregion

        #region arcus trigonometric functions

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double arcsin(double x)
        {
            return Math.Asin((x));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double arccos(double x)
        {
            return Math.Acos((x));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double arctan(double x)
        {
            return Math.Atan((x));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double arctg(double x)
        {
            return Math.Atan((x));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double arccot(double x)
        {
            return (Math.PI/2 - Math.Atan((x)));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double arcctg(double x)
        {
            return (Math.PI/2 - Math.Atan((x)));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double arcsec(double x)
        {
            return (Math.Acos(1.0/(x)));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double arccsc(double x)
        {
            return (Math.Asin(1.0/(x)));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double arccosec(double x)
        {
            return (Math.Asin(1.0/(x)));
        }

        #endregion

        #region area hyperbolic trigonometric functions

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double arsinh(double x)
        {
            return Math.Log((x)) + Math.Sqrt((x)*(x) + 1);
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double arcosh(double x)
        {
            return Math.Log((x) + Math.Sqrt(x + 1)*Math.Sqrt(x - 1));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double artanh(double x)
        {
            return 0.5*Math.Log((1 + x)/(1 - x));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double artgh(double x)
        {
            return 0.5*Math.Log((1 + x)/(1 - x));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double arcoth(double x)
        {
            return 0.5*Math.Log((x + 1)/(x - 1));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double arctgh(double x)
        {
            return 0.5*Math.Log((x + 1)/(x - 1));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double arcsch(double x)
        {
            return Math.Log((1.0/x) + Math.Sqrt((1.0/(x*x)) + 1));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double arcosech(double x)
        {
            return arcsch(x);
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double arsech(double x)
        {
            return Math.Log((1.0/x) + Math.Sqrt((1.0/x) + 1)*Math.Sqrt((1.0/x) - 1));
        }

        #endregion

        #region logarithmic functions

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double ln(double x)
        {
            return Math.Log(x);
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double log2(double x)
        {
            return Math.Log(x, 2);
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double log10(double x)
        {
            return Math.Log10(x);
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double log(double value, double nbase)
        {
            return Math.Log(value, nbase);
        }

        #endregion

        #region root functions

        /*[Name(""Square root""),
         Description(
             ""square root of a number a is a number y such that y2 = x, in other words, a number y whose square (the result of multiplying the number by itself, or y × y) is x.""
             ), DisplayName(""Angielska nazwa funkcji""),
         Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)"")]*/
        public static double sqrt(double x)
        {
            return Math.Sqrt(x);
        }

        /*[Name(""Root function""), Description(""root function of the given value""), DisplayName(""Angielska nazwa funkcji""),
         Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)"")]*/
        public static double root(double value, double n)
        {
            //n-based root
            return Math.Pow(value, 1.0/n);
        }

        #endregion

        #region power functions

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double pow(double x, double y)
        {
            return Math.Pow(x, y);
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double exp(double x)
        {
            return Math.Exp(x);
        }

        #endregion

        #region trigonometric functions

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex sinc(Complex z)
        {
            if (z == 0)
                return 1;
            return (Complex.Sin((z))) / (z);
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex sin(Complex z)
        {
            return Complex.Sin((z));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex cos(Complex z)
        {
            return Complex.Cos((z));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex tan(Complex z)
        {
            return Complex.Tan((z));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex tg(Complex z)
        {
            return Complex.Tan((z));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex ctg(Complex z)
        {
            return 1.0 / Complex.Tan((z));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex cot(Complex z)
        {
            return 1.0 / Complex.Tan((z));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex sec(Complex z)
        {
            return 1.0 / Complex.Cos((z));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex csc(Complex z)
        {
            return 1.0 / Complex.Sin((z));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex cosec(Complex z)
        {
            return 1.0 / Complex.Sin((z));
        }

        #endregion

        #region hyperbolic trygonometric functions

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex sinh(Complex z)
        {
            return Complex.Sinh((z));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex cosh(Complex z)
        {
            return Complex.Cosh((z));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex tanh(Complex z)
        {
            return Complex.Tanh((z));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex tgh(Complex z)
        {
            return Complex.Tanh((z));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex coth(Complex z)
        {
            return (1.0 / Complex.Tanh((z)));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex ctgh(Complex z)
        {
            return (1.0 / Complex.Tanh((z)));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex sech(Complex z)
        {
            return (1.0 / Complex.Cosh((z)));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex csch(Complex z)
        {
            return (1.0 / Complex.Sinh((z)));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex cosech(Complex z)
        {
            return (1.0 / Complex.Sinh((z)));
        }

        #endregion

        #region arcus trigonometric functions

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex arcsin(Complex z)
        {
            return Complex.Asin((z));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex arccos(Complex z)
        {
            return Complex.Acos((z));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex arctan(Complex z)
        {
            return Complex.Atan((z));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex arctg(Complex z)
        {
            return Complex.Atan((z));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex arccot(Complex z)
        {
            return (Math.PI / 2 - Complex.Atan((z)));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex arcctg(Complex z)
        {
            return (Math.PI / 2 - Complex.Atan((z)));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex arcsec(Complex z)
        {
            return (Complex.Acos(1.0 / (z)));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex arccsc(Complex z)
        {
            return (Complex.Asin(1.0 / (z)));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex arccosec(Complex z)
        {
            return (Complex.Asin(1.0 / (z)));
        }

        #endregion

        #region area hyperbolic trigonometric functions

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex arsinh(Complex z)
        {
            return Complex.Log((z)) + Complex.Sqrt((z) * (z) + 1);
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex arcosh(Complex z)
        {
            return Complex.Log((z) + Complex.Sqrt(z + 1) * Complex.Sqrt(z - 1));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex artanh(Complex z)
        {
            return 0.5 * Complex.Log((1 + z) / (1 - z));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex artgh(Complex z)
        {
            return 0.5 * Complex.Log((1 + z) / (1 - z));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex arcoth(Complex z)
        {
            return 0.5 * Complex.Log((z + 1) / (z - 1));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex arctgh(Complex z)
        {
            return 0.5 * Complex.Log((z + 1) / (z - 1));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex arcsch(Complex z)
        {
            return Complex.Log((1.0 / z) + Complex.Sqrt((1.0 / (z * z)) + 1));
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex arcosech(Complex z)
        {
            return arcsch(z);
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex arsech(Complex z)
        {
            return Complex.Log((1.0 / z) + Complex.Sqrt((1.0 / z) + 1) * Complex.Sqrt((1.0 / z) - 1));
        }

        #endregion

        #region logarithmic functions

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex ln(Complex z)
        {
            return Complex.Log(z);
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex log2(Complex z)
        {
            return Complex.Log(z, 2);
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex log10(Complex z)
        {
            return Complex.Log10(z);
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex log(Complex z, double nbase)
        {
            return Complex.Log(z, nbase);
        }

        #endregion

        #region root functions

        /*[Name(""Square root""),
         Description(
             ""square root of a number a is a number y such that y2 = x, in other words, a number y whose square (the result of multiplying the number by itself, or y × y) is x.""
             ), DisplayName(""Angielska nazwa funkcji""),
         Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)"")]*/
        public static Complex sqrt(Complex z)
        {
            return Complex.Sqrt(z);
        }

        /*[Name(""Root function""), Description(""root function of the given value""), DisplayName(""Angielska nazwa funkcji""),
         Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)"")]*/
        public static Complex root(Complex z, Complex n)
        {
            //n-based root
            return Complex.Pow(z, 1.0 / n);
        }

        #endregion

        #region power functions

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex pow(Complex x, Complex y)
        {
            return Complex.Pow(x, y);
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static Complex exp(Complex z)
        {
            return Complex.Exp(z);
        }

        #endregion

        #region integer functions

        /*[Name(""Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/ public static Func<double, double> sgn =
             x => (double.IsNaN(x)) ? double.NaN : (double) (Math.Sign(x));

        /*[Name(""Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/ public static Func<long, long, long> GCF = AdvancedIntegerMath.GCF,
            NWD = AdvancedIntegerMath.GCF;

        /*[Name(""Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/ public static Func<long, long, long> LCM = AdvancedIntegerMath.LCM,
            NWW = AdvancedIntegerMath.LCM;

        /*[Name(""Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double factorial(double n)
        {
            double f = 1;
            for (ulong i = 1; i <= n; i++)
                f = f*i;
            return f;
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static int prime(double d)
        {
            if (d < 1)
                return -1;
            if (AdvancedIntegerMath.IsPrime((int) (d)))
                return 1;
            return 0;
        }

        #endregion

        #region complex specific functions

        /*[Name(""Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/ public static Func<Complex, Complex> conj = Complex.Conjugate;

        /*[Name(""Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/ public static Func<Complex, double> Re = z => z.Real;

        /*[Name(""Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/ public static Func<Complex, double> Im = z => z.Imaginary;

        /*[Name(""Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/ public static Func<Complex, double> Phase = z => z.Phase;

        /*[Name(""Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/ public static Func<Complex, double> Magnitude = z => z.Magnitude;

        #endregion

        #region a step like functions (non continuous)

        /*[Name(""Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/ public static Func<double, double> H = Heaviside;

        /*[Name(""Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/ public static Func<double, double> δ = DiracDelta;

        /*[Name(""Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/ public static Func<double, double, double> δij = KroneckerDelta;

        public static double abs(double x)
        {
            if (x >= 0.0)
                return x;
            return -x;
        }

        public static Complex abs(Complex z)
        {
            return Complex.Abs(z);
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double Heaviside(double x)
        {
            if (x > 0.0)
                return 1;
            if (x < 0.0)
                return 0;
            return 0.5;
        }


        /*[Name(""Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double DiracDelta(double d)
        {
            if (d != 0.0)
                return 0;
            return double.PositiveInfinity;
        }

        /*[Name(""Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         Description(""angielski opis funkcji"")]*/
        public static double KroneckerDelta(double i, double j)
        {
            if (i != j)
                return 0;
            return 1;
        }

        #endregion

        #region coefficients and special values

        [DllImport(gslSfLibDir2, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_taylorcoeff(int n, double x);

        [DllImport(gslSfLibDir2, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_doublefact(uint n);

        [DllImport(gslSfLibDir2, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_choose(uint n, uint m);

        public static double TaylorCoeff(int n, double x)
        {
            if (n < 0 || x < 0) return double.NaN;
            return gsl_sf_taylorcoeff(n, x);
        }

        public static double DoubleFactorial(uint n)
        {
            return gsl_sf_doublefact(n);
        }

        public static double BinomialCoeff(uint n, uint k)
        {
            return gsl_sf_choose(n, k);
        }

        #endregion

        #region combinatorics

        public static double VariationsWithRepetition(int n, int k)
        {
            return MathNet.Numerics.Combinatorics.VariationsWithRepetition(n, k);
        }

        public static double Variations(int n, int k)
        {
            return MathNet.Numerics.Combinatorics.Variations(n, k);
        }

        public static double CombinationsWithRepetition(int n, int k)
        {
            return MathNet.Numerics.Combinatorics.CombinationsWithRepetition(n, k);
        }

        public static double Combinations(int n, int k)
        {
            return MathNet.Numerics.Combinatorics.Combinations(n, k);
        }

        public static double Permutations(int n, int k)
        {
            return MathNet.Numerics.Combinatorics.Permutations(n);
        }

        #endregion

";

        #endregion
    }
}