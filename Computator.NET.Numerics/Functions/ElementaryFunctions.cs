using System;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.InteropServices;
using MathNet.Numerics;
using Meta.Numerics.Functions;

namespace Computator.NET.Functions
{
    internal static class ElementaryFunctions
    {
        #region utils

        private const string gslSfLibDir2 = "gsl.dll"; //"libgsl-0.dll";

        #endregion

        public const string ToCode =
            @"
      
        public static double abs(double x)
        {
            if (x >= 0.0)
                return x;
            else
                return -x;
        }

        public static Complex abs(Complex z)
        {
            return Complex.Abs(z);
        }
        
        #region trigonometric functions

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double sinc(double value)
        {
            if (value == 0)
                return 1;
            return (Math.Sin((value))) / (value);
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double sin(double value)
        {
            return Math.Sin((value));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double cos(double value)
        {
            return Math.Cos((value));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double tan(double value)
        {
            return Math.Tan((value));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double tg(double value)
        {
            return Math.Tan((value));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double ctg(double value)
        {
            return 1.0 / Math.Tan((value));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double cot(double value)
        {
            return 1.0 / Math.Tan((value));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double sec(double value)
        {
            return 1.0 / Math.Cos((value));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double csc(double value)
        {
            return 1.0 / Math.Sin((value));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double cosec(double value)
        {
            return 1.0 / Math.Sin((value));
        }

        #endregion

        #region hyperbolic trygonometric functions

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double sinh(double value)
        {
            return Math.Sinh((value));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double cosh(double value)
        {
            return Math.Cosh((value));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double tanh(double value)
        {
            return Math.Tanh((value));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double tgh(double value)
        {
            return Math.Tanh((value));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double coth(double value)
        {
            return (1.0 / Math.Tanh((value)));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double ctgh(double value)
        {
            return (1.0 / Math.Tanh((value)));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double sech(double value)
        {
            return (1.0 / Math.Cosh((value)));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double csch(double value)
        {
            return (1.0 / Math.Sinh((value)));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double cosech(double value)
        {
            return (1.0 / Math.Sinh((value)));
        }

        #endregion

        #region arcus trigonometric functions
        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double arcsin(double value)
        {
            return Math.Asin((value));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double arccos(double value)
        {
            return Math.Acos((value));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double arctan(double value)
        {
            return Math.Atan((value));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double arctg(double value)
        {
            return Math.Atan((value));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double arccot(double value)
        {
            return (Math.PI / 2 - Math.Atan((value)));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double arcctg(double value)
        {
            return (Math.PI / 2 - Math.Atan((value)));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double arcsec(double value)
        {
            return (Math.Acos(1.0 / (value)));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double arccsc(double value)
        {
            return (Math.Asin(1.0 / (value)));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double arccosec(double value)
        {
            return (Math.Asin(1.0 / (value)));
        }

        #endregion

        #region area hyperbolic trigonometric functions

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double arsinh(double value)
        {
            return Math.Log((value)) + Math.Sqrt((value) * (value) + 1);
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double arcosh(double value)
        {
            return Math.Log((value) + Math.Sqrt(value + 1) * Math.Sqrt(value - 1));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double artanh(double value)
        {
            return 0.5 * Math.Log((1 + value) / (1 - value));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double artgh(double value)
        {
            return 0.5 * Math.Log((1 + value) / (1 - value));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double arcoth(double value)
        {
            return 0.5 * Math.Log((value + 1) / (value - 1));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double arctgh(double value)
        {
            return 0.5 * Math.Log((value + 1) / (value - 1));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double arcsch(double value)
        {
            return Math.Log((1.0 / value) + Math.Sqrt((1.0 / (value * value)) + 1));
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double arcosech(double value)
        {
            return arcsch(value);
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double arsech(double value)
        {
            return Math.Log((1.0 / value) + Math.Sqrt((1.0 / value) + 1) * Math.Sqrt((1.0 / value) - 1));
        }

        #endregion

        #region logarithmic functions
        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double ln(double value)
        {
            return Math.Log(value);
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double log2(double value)
        {
            return Math.Log(value, 2);
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double log10(double value)
        {
            return Math.Log10(value);
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double log(double value, double nbase)
        {
            return Math.Log(value, nbase);
        }
        #endregion

        #region root functions
        /*Square root""), Description(""square root of a number a is a number y such that y2 = x, in other words, a number y whose square (the result of multiplying the number by itself, or y × y) is x.""), DisplayName(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)*/
        public static double sqrt(double value)
        {
            return Math.Sqrt(value);
        }
        /*Root function""), Description(""root function of the given value""), DisplayName(""Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)*/
        public static double root(double value, double n)
        {
            //n-based root
            return Math.Pow(value, 1.0 / n);
        }
        #endregion

        #region power functions
        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double pow(double x, double y)
        {
            return Math.Pow(x, y);
        }

        /*Angielska nazwa funkcji""), Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double exp(double value)
        {
            return Math.Exp(value);
        }
        #endregion


        #region trigonometric functions
        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex sinc(Complex value)
        {
            if (value == 0)
                return 1;
            return (Complex.Sin((value))) / (value);
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex sin(Complex value)
        {
            return Complex.Sin((value));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex cos(Complex value)
        {
            return Complex.Cos((value));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex tan(Complex value)
        {
            return Complex.Tan((value));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex tg(Complex value)
        {
            return Complex.Tan((value));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex ctg(Complex value)
        {
            return 1.0 / Complex.Tan((value));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex cot(Complex value)
        {
            return 1.0 / Complex.Tan((value));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex sec(Complex value)
        {
            return 1.0 / Complex.Cos((value));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex csc(Complex value)
        {
            return 1.0 / Complex.Sin((value));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex cosec(Complex value)
        {
            return 1.0 / Complex.Sin((value));
        }

        #endregion

        #region hyperbolic trygonometric functions

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex sinh(Complex value)
        {
            return Complex.Sinh((value));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex cosh(Complex value)
        {
            return Complex.Cosh((value));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex tanh(Complex value)
        {
            return Complex.Tanh((value));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex tgh(Complex value)
        {
            return Complex.Tanh((value));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex coth(Complex value)
        {
            return (1.0 / Complex.Tanh((value)));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex ctgh(Complex value)
        {
            return (1.0 / Complex.Tanh((value)));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex sech(Complex value)
        {
            return (1.0 / Complex.Cosh((value)));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex csch(Complex value)
        {
            return (1.0 / Complex.Sinh((value)));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex cosech(Complex value)
        {
            return (1.0 / Complex.Sinh((value)));
        }

        #endregion

        #region arcus trigonometric functions
        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex arcsin(Complex value)
        {
            return Complex.Asin((value));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex arccos(Complex value)
        {
            return Complex.Acos((value));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex arctan(Complex value)
        {
            return Complex.Atan((value));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex arctg(Complex value)
        {
            return Complex.Atan((value));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex arccot(Complex value)
        {
            return (Math.PI / 2 - Complex.Atan((value)));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex arcctg(Complex value)
        {
            return (Math.PI / 2 - Complex.Atan((value)));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex arcsec(Complex value)
        {
            return (Complex.Acos(1.0 / (value)));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex arccsc(Complex value)
        {
            return (Complex.Asin(1.0 / (value)));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex arccosec(Complex value)
        {
            return (Complex.Asin(1.0 / (value)));
        }

        #endregion

        #region area hyperbolic trigonometric functions

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex arsinh(Complex value)
        {
            return Complex.Log((value)) + Complex.Sqrt((value) * (value) + 1);
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex arcosh(Complex value)
        {
            return Complex.Log((value) + Complex.Sqrt(value + 1) * Complex.Sqrt(value - 1));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex artanh(Complex value)
        {
            return 0.5 * Complex.Log((1 + value) / (1 - value));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex artgh(Complex value)
        {
            return 0.5 * Complex.Log((1 + value) / (1 - value));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex arcoth(Complex value)
        {
            return 0.5 * Complex.Log((value + 1) / (value - 1));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex arctgh(Complex value)
        {
            return 0.5 * Complex.Log((value + 1) / (value - 1));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex arcsch(Complex value)
        {
            return Complex.Log((1.0 / value) + Complex.Sqrt((1.0 / (value * value)) + 1));
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex arcosech(Complex value)
        {
            return arcsch(value);
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex arsech(Complex value)
        {
            return Complex.Log((1.0 / value) + Complex.Sqrt((1.0 / value) + 1) * Complex.Sqrt((1.0 / value) - 1));
        }

        #endregion

        #region logarithmic functions
        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex ln(Complex value)
        {
            return Complex.Log(value);
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex log2(Complex value)
        {
            return Complex.Log(value, 2);
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex log10(Complex value)
        {
            return Complex.Log10(value);
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex log(Complex value, double nbase)
        {
            return Complex.Log(value, nbase);
        }
        #endregion

        #region root functions
        /*Square root""), Description(""square root of a number a is a number y such that y2 = x, in other words, a number y whose square (the result of multiplying the number by itself, or y × y) is x.""), DisplayName(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)*/
        public static Complex sqrt(Complex value)
        {
            return Complex.Sqrt(value);
        }
        /*Root function""), Description(""root function of the given value""), DisplayName(""Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)*/
        public static Complex root(Complex value, Complex n)
        {
            //n-based root
            return Complex.Pow(value, 1.0 / n);
        }
        #endregion

        #region power functions
        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex pow(Complex x, Complex y)
        {
            return Complex.Pow(x, y);
        }

        /*Angielska nazwa funkcji""), Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Complex exp(Complex value)
        {
            return Complex.Exp(value);
        }
        #endregion



        #region integer functions
        /*Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double factorial(double n)
        {
            double f = 1;
            for (ulong i = 1; i <= n; i++)
                f = f * i;
            return f;
        }

        /*Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static int prime(double d)
        {
            if (d < 1)
                return -1;
            if (AdvancedIntegerMath.IsPrime((int)(d)))
                return 1;
            else
                return 0;
        }

        /*Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Func<double, double> sgn = (x) => (double.IsNaN(x)) ? double.NaN : (double)(Math.Sign(x));

        /*Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Func<long, long, long> GCF = AdvancedIntegerMath.GCF, NWD = AdvancedIntegerMath.GCF;

        /*Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Func<long, long, long> LCM = AdvancedIntegerMath.LCM, NWW = AdvancedIntegerMath.LCM;

        #endregion

        #region complex specific functions
        /*Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Func<Complex, Complex> conj = Complex.Conjugate;

        /*Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Func<Complex, double> Re = (z) => z.Real;

        /*Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Func<Complex, double> Im = (z) => z.Imaginary;

        /*Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Func<Complex, double> Phase = (z) => z.Phase;

        /*Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Func<Complex, double> Magnitude = (z) => z.Magnitude;
        #endregion

        #region a step like functions (non continuous)
        /*Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double Heaviside(double x)
        {
            if (x > 0.0)
                return 1;
            else if (x < 0.0)
                return 0;
            else
                return 0.5;
        }
        /*Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Func<double, double> H = Heaviside;


        /*Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double DiracDelta(double d)
        {
            if (d != 0.0)
                return 0;
            else
                return double.PositiveInfinity;
        }

        /*Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Func<double, double> δ = DiracDelta;

        /*Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static double KroneckerDelta(double i, double j)
        {
            if (i != j)
                return 0;
            else
                return 1;
        }

        /*Angielska nazwa funkcji""), Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""), Description(""angielski opis funkcji*/
        public static Func<double, double, double> δij = KroneckerDelta;
        #endregion

        #region coefficients and special values
        [DllImport(gslSfLibDir2, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_taylorcoeff(int n, double x);

        [DllImport(gslSfLibDir2, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_doublefact(uint n);

        [DllImport(gslSfLibDir2, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_choose(uint n, uint m);

        public static double TaylorCoeff(int n, double x) { if (n < 0 || x < 0) return double.NaN; return gsl_sf_taylorcoeff(n, x); }

        public static double DoubleFactorial(uint n) { return gsl_sf_doublefact(n); }

        public static double BinomialCoeff(uint n, uint k) { return gsl_sf_choose(n, k); }
        #endregion

        #region combinatorics
        public static double VariationsWithRepetition(int n, int k) { return MathNet.Numerics.Combinatorics.VariationsWithRepetition(n, k); }
        public static double Variations(int n, int k) { return MathNet.Numerics.Combinatorics.Variations(n, k); }
        public static double CombinationsWithRepetition(int n, int k) { return MathNet.Numerics.Combinatorics.CombinationsWithRepetition(n, k); }
        public static double Combinations(int n, int k) { return MathNet.Numerics.Combinatorics.Combinations(n, k); }
        public static double Permutations(int n, int k) { return MathNet.Numerics.Combinatorics.Permutations(n); }
        #endregion

        #region utils
        private const string gslSfLibDir2 = ""gsl.dll"";//""libgsl-0.dll"";
        #endregion

";

        #region trigonometric functions

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double sinc(double value)
        {
            if (value == 0)
                return 1;
            return (Math.Sin((value)))/(value);
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double sin(double value)
        {
            return Math.Sin((value));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double cos(double value)
        {
            return Math.Cos((value));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double tan(double value)
        {
            return Math.Tan((value));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double tg(double value)
        {
            return Math.Tan((value));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double ctg(double value)
        {
            return 1.0/Math.Tan((value));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double cot(double value)
        {
            return 1.0/Math.Tan((value));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double sec(double value)
        {
            return 1.0/Math.Cos((value));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double csc(double value)
        {
            return 1.0/Math.Sin((value));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double cosec(double value)
        {
            return 1.0/Math.Sin((value));
        }

        #endregion

        #region hyperbolic trygonometric functions

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double sinh(double value)
        {
            return Math.Sinh((value));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double cosh(double value)
        {
            return Math.Cosh((value));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double tanh(double value)
        {
            return Math.Tanh((value));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double tgh(double value)
        {
            return Math.Tanh((value));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double coth(double value)
        {
            return (1.0/Math.Tanh((value)));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double ctgh(double value)
        {
            return (1.0/Math.Tanh((value)));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double sech(double value)
        {
            return (1.0/Math.Cosh((value)));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double csch(double value)
        {
            return (1.0/Math.Sinh((value)));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double cosech(double value)
        {
            return (1.0/Math.Sinh((value)));
        }

        #endregion

        #region arcus trigonometric functions

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arcsin(double value)
        {
            return Math.Asin((value));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arccos(double value)
        {
            return Math.Acos((value));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arctan(double value)
        {
            return Math.Atan((value));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arctg(double value)
        {
            return Math.Atan((value));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arccot(double value)
        {
            return (Math.PI/2 - Math.Atan((value)));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arcctg(double value)
        {
            return (Math.PI/2 - Math.Atan((value)));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arcsec(double value)
        {
            return (Math.Acos(1.0/(value)));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arccsc(double value)
        {
            return (Math.Asin(1.0/(value)));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arccosec(double value)
        {
            return (Math.Asin(1.0/(value)));
        }

        #endregion

        #region area hyperbolic trigonometric functions

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arsinh(double value)
        {
            return Math.Log((value)) + Math.Sqrt((value)*(value) + 1);
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arcosh(double value)
        {
            return Math.Log((value) + Math.Sqrt(value + 1)*Math.Sqrt(value - 1));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double artanh(double value)
        {
            return 0.5*Math.Log((1 + value)/(1 - value));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double artgh(double value)
        {
            return 0.5*Math.Log((1 + value)/(1 - value));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arcoth(double value)
        {
            return 0.5*Math.Log((value + 1)/(value - 1));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arctgh(double value)
        {
            return 0.5*Math.Log((value + 1)/(value - 1));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arcsch(double value)
        {
            return Math.Log((1.0/value) + Math.Sqrt((1.0/(value*value)) + 1));
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arcosech(double value)
        {
            return arcsch(value);
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double arsech(double value)
        {
            return Math.Log((1.0/value) + Math.Sqrt((1.0/value) + 1)*Math.Sqrt((1.0/value) - 1));
        }

        #endregion

        #region logarithmic functions

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double ln(double value)
        {
            return Math.Log(value);
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double log2(double value)
        {
            return Math.Log(value, 2);
        }

        [Name("Angielska nazwa funkcji"), Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static double log10(double value)
        {
            return Math.Log10(value);
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
        public static double sqrt(double value)
        {
            return Math.Sqrt(value);
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
        public static double exp(double value)
        {
            return Math.Exp(value);
        }

        #endregion

        #region trigonometric functions

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex sinc(Complex value)
        {
            if (value == 0)
                return 1;
            return (Complex.Sin((value)))/(value);
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex sin(Complex value)
        {
            return Complex.Sin((value));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex cos(Complex value)
        {
            return Complex.Cos((value));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex tan(Complex value)
        {
            return Complex.Tan((value));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex tg(Complex value)
        {
            return Complex.Tan((value));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex ctg(Complex value)
        {
            return 1.0/Complex.Tan((value));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex cot(Complex value)
        {
            return 1.0/Complex.Tan((value));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex sec(Complex value)
        {
            return 1.0/Complex.Cos((value));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex csc(Complex value)
        {
            return 1.0/Complex.Sin((value));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex cosec(Complex value)
        {
            return 1.0/Complex.Sin((value));
        }

        #endregion

        #region hyperbolic trygonometric functions

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex sinh(Complex value)
        {
            return Complex.Sinh((value));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex cosh(Complex value)
        {
            return Complex.Cosh((value));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex tanh(Complex value)
        {
            return Complex.Tanh((value));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex tgh(Complex value)
        {
            return Complex.Tanh((value));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex coth(Complex value)
        {
            return (1.0/Complex.Tanh((value)));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex ctgh(Complex value)
        {
            return (1.0/Complex.Tanh((value)));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex sech(Complex value)
        {
            return (1.0/Complex.Cosh((value)));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex csch(Complex value)
        {
            return (1.0/Complex.Sinh((value)));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex cosech(Complex value)
        {
            return (1.0/Complex.Sinh((value)));
        }

        #endregion

        #region arcus trigonometric functions

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arcsin(Complex value)
        {
            return Complex.Asin((value));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arccos(Complex value)
        {
            return Complex.Acos((value));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arctan(Complex value)
        {
            return Complex.Atan((value));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arctg(Complex value)
        {
            return Complex.Atan((value));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arccot(Complex value)
        {
            return (Math.PI/2 - Complex.Atan((value)));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arcctg(Complex value)
        {
            return (Math.PI/2 - Complex.Atan((value)));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arcsec(Complex value)
        {
            return (Complex.Acos(1.0/(value)));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arccsc(Complex value)
        {
            return (Complex.Asin(1.0/(value)));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arccosec(Complex value)
        {
            return (Complex.Asin(1.0/(value)));
        }

        #endregion

        #region area hyperbolic trigonometric functions

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arsinh(Complex value)
        {
            return Complex.Log((value)) + Complex.Sqrt((value)*(value) + 1);
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arcosh(Complex value)
        {
            return Complex.Log((value) + Complex.Sqrt(value + 1)*Complex.Sqrt(value - 1));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex artanh(Complex value)
        {
            return 0.5*Complex.Log((1 + value)/(1 - value));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex artgh(Complex value)
        {
            return 0.5*Complex.Log((1 + value)/(1 - value));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arcoth(Complex value)
        {
            return 0.5*Complex.Log((value + 1)/(value - 1));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arctgh(Complex value)
        {
            return 0.5*Complex.Log((value + 1)/(value - 1));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arcsch(Complex value)
        {
            return Complex.Log((1.0/value) + Complex.Sqrt((1.0/(value*value)) + 1));
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arcosech(Complex value)
        {
            return arcsch(value);
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex arsech(Complex value)
        {
            return Complex.Log((1.0/value) + Complex.Sqrt((1.0/value) + 1)*Complex.Sqrt((1.0/value) - 1));
        }

        #endregion

        #region logarithmic functions

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex ln(Complex value)
        {
            return Complex.Log(value);
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex log2(Complex value)
        {
            return Complex.Log(value, 2);
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex log10(Complex value)
        {
            return Complex.Log10(value);
        }

        [Name("Angielska nazwa funkcji"), Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         Description("angielski opis funkcji")]
        public static Complex log(Complex value, double nbase)
        {
            return Complex.Log(value, nbase);
        }

        #endregion

        #region root functions

        [Name("Square root"),
         Description(
             "square root of a number a is a number y such that y2 = x, in other words, a number y whose square (the result of multiplying the number by itself, or y × y) is x."
             ), DisplayName("Angielska nazwa funkcji"),
         Category("Complexu wpisz nazwę regionu (np. trigonometric functions)")]
        public static Complex sqrt(Complex value)
        {
            return Complex.Sqrt(value);
        }

        [Name("Root function"), Description("root function of the given value"), DisplayName("Angielska nazwa funkcji"),
         Category("Complexu wpisz nazwę regionu (np. trigonometric functions)")]
        public static Complex root(Complex value, Complex n)
        {
            //n-based root
            return Complex.Pow(value, 1.0/n);
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
        public static Complex exp(Complex value)
        {
            return Complex.Exp(value);
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
    }
}