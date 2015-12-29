// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody

namespace Computator.NET.Functions
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public static class ElementaryFunctions
        //TODO: make functions appear in right order (etc first sin(x) then sin(z) then cos(x) ...)
    {
        #region trigonometric functions


        public static double sinc(double x)
        {
            if (x == 0)
                return 1;
            return (System.Math.Sin((x)))/(x);
        }


        public static double sin(double x)
        {
            return System.Math.Sin((x));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double cos(double x)
        {
            return System.Math.Cos((x));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double tan(double x)
        {
            return System.Math.Tan((x));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double tg(double x)
        {
            return System.Math.Tan((x));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double ctg(double x)
        {
            return 1.0/System.Math.Tan((x));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double cot(double x)
        {
            return 1.0/System.Math.Tan((x));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double sec(double x)
        {
            return 1.0/System.Math.Cos((x));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double csc(double x)
        {
            return 1.0/System.Math.Sin((x));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double cosec(double x)
        {
            return 1.0/System.Math.Sin((x));
        }

        #endregion

        #region hyperbolic trygonometric functions


        public static double sinh(double x)
        {
            return System.Math.Sinh((x));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double cosh(double x)
        {
            return System.Math.Cosh((x));
        }


        public static double tanh(double x)
        {
            return System.Math.Tanh((x));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double tgh(double x)
        {
            return System.Math.Tanh((x));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double coth(double x)
        {
            return (1.0/System.Math.Tanh((x)));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double ctgh(double x)
        {
            return (1.0/System.Math.Tanh((x)));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double sech(double x)
        {
            return (1.0/System.Math.Cosh((x)));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double csch(double x)
        {
            return (1.0/System.Math.Sinh((x)));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double cosech(double x)
        {
            return (1.0/System.Math.Sinh((x)));
        }

        #endregion

        #region arcus trigonometric functions

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double arcsin(double x)
        {
            return System.Math.Asin((x));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double arccos(double x)
        {
            return System.Math.Acos((x));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double arctan(double x)
        {
            return System.Math.Atan((x));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double arctg(double x)
        {
            return System.Math.Atan((x));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double arccot(double x)
        {
            return (System.Math.PI/2 - System.Math.Atan((x)));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double arcctg(double x)
        {
            return (System.Math.PI/2 - System.Math.Atan((x)));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double arcsec(double x)
        {
            return (System.Math.Acos(1.0/(x)));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double arccsc(double x)
        {
            return (System.Math.Asin(1.0/(x)));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double arccosec(double x)
        {
            return (System.Math.Asin(1.0/(x)));
        }

        #endregion

        #region area hyperbolic trigonometric functions

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double arsinh(double x)
        {
            return System.Math.Log((x)) + System.Math.Sqrt((x)*(x) + 1);
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double arcosh(double x)
        {
            return System.Math.Log((x) + System.Math.Sqrt(x + 1)*System.Math.Sqrt(x - 1));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double artanh(double x)
        {
            return 0.5*System.Math.Log((1 + x)/(1 - x));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double artgh(double x)
        {
            return 0.5*System.Math.Log((1 + x)/(1 - x));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double arcoth(double x)
        {
            return 0.5*System.Math.Log((x + 1)/(x - 1));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double arctgh(double x)
        {
            return 0.5*System.Math.Log((x + 1)/(x - 1));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double arcsch(double x)
        {
            return System.Math.Log((1.0/x) + System.Math.Sqrt((1.0/(x*x)) + 1));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double arcosech(double x)
        {
            return arcsch(x);
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double arsech(double x)
        {
            return System.Math.Log((1.0/x) + System.Math.Sqrt((1.0/x) + 1)*System.Math.Sqrt((1.0/x) - 1));
        }

        #endregion

        #region logarithmic functions

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double ln(double x)
        {
            return System.Math.Log(x);
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double log2(double x)
        {
            return System.Math.Log(x, 2);
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double log10(double x)
        {
            return System.Math.Log10(x);
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double log(double value, double nbase)
        {
            return System.Math.Log(value, nbase);
        }

        #endregion

        #region root functions

        /*Square root"),
         System.ComponentModel.Description(
             "square root of a number a is a number y such that y2 = x, in other words, a number y whose square (the result of multiplying the number by itself, or y × y) is x."
             ), System.ComponentModel.DisplayName("Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)*/
       /* public static double sqrt(double x)
        {
            return System.Math.Sqrt(x);
        }*/

        public static dynamic sqrt(double x)
        {
            if (x < 0)
                return System.Numerics.Complex.Sqrt(x);
            return System.Math.Sqrt(x);
        }

        /*Root function"), System.ComponentModel.Description("root function of the given value"),
         System.ComponentModel.DisplayName("Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)*/
        public static double root(double value, double n)
        {
            //n-based root
            return System.Math.Pow(value, 1.0/n);
        }

        #endregion

        #region power functions

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double pow(double x, double y)
        {
            return System.Math.Pow(x, y);
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("doubleu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double exp(double x)
        {
            return System.Math.Exp(x);
        }

        #endregion

        #region trigonometric functions

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex sinc(System.Numerics.Complex z)
        {
            if (z == 0)
                return 1;
            return (System.Numerics.Complex.Sin((z)))/(z);
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex sin(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Sin((z));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex cos(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Cos((z));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex tan(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Tan((z));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex tg(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Tan((z));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex ctg(System.Numerics.Complex z)
        {
            return 1.0/System.Numerics.Complex.Tan((z));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex cot(System.Numerics.Complex z)
        {
            return 1.0/System.Numerics.Complex.Tan((z));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex sec(System.Numerics.Complex z)
        {
            return 1.0/System.Numerics.Complex.Cos((z));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex csc(System.Numerics.Complex z)
        {
            return 1.0/System.Numerics.Complex.Sin((z));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex cosec(System.Numerics.Complex z)
        {
            return 1.0/System.Numerics.Complex.Sin((z));
        }

        #endregion

        #region hyperbolic trygonometric functions

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex sinh(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Sinh((z));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex cosh(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Cosh((z));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex tanh(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Tanh((z));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex tgh(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Tanh((z));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex coth(System.Numerics.Complex z)
        {
            return (1.0/System.Numerics.Complex.Tanh((z)));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex ctgh(System.Numerics.Complex z)
        {
            return (1.0/System.Numerics.Complex.Tanh((z)));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex sech(System.Numerics.Complex z)
        {
            return (1.0/System.Numerics.Complex.Cosh((z)));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex csch(System.Numerics.Complex z)
        {
            return (1.0/System.Numerics.Complex.Sinh((z)));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex cosech(System.Numerics.Complex z)
        {
            return (1.0/System.Numerics.Complex.Sinh((z)));
        }

        #endregion

        #region arcus trigonometric functions

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex arcsin(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Asin((z));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex arccos(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Acos((z));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex arctan(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Atan((z));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex arctg(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Atan((z));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex arccot(System.Numerics.Complex z)
        {
            return (System.Math.PI/2 - System.Numerics.Complex.Atan((z)));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex arcctg(System.Numerics.Complex z)
        {
            return (System.Math.PI/2 - System.Numerics.Complex.Atan((z)));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex arcsec(System.Numerics.Complex z)
        {
            return (System.Numerics.Complex.Acos(1.0/(z)));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex arccsc(System.Numerics.Complex z)
        {
            return (System.Numerics.Complex.Asin(1.0/(z)));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex arccosec(System.Numerics.Complex z)
        {
            return (System.Numerics.Complex.Asin(1.0/(z)));
        }

        #endregion

        #region area hyperbolic trigonometric functions

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex arsinh(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Log((z)) + System.Numerics.Complex.Sqrt((z)*(z) + 1);
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex arcosh(System.Numerics.Complex z)
        {
            return
                System.Numerics.Complex.Log((z) +
                                            System.Numerics.Complex.Sqrt(z + 1)*System.Numerics.Complex.Sqrt(z - 1));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex artanh(System.Numerics.Complex z)
        {
            return 0.5*System.Numerics.Complex.Log((1 + z)/(1 - z));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex artgh(System.Numerics.Complex z)
        {
            return 0.5*System.Numerics.Complex.Log((1 + z)/(1 - z));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex arcoth(System.Numerics.Complex z)
        {
            return 0.5*System.Numerics.Complex.Log((z + 1)/(z - 1));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex arctgh(System.Numerics.Complex z)
        {
            return 0.5*System.Numerics.Complex.Log((z + 1)/(z - 1));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex arcsch(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Log((1.0/z) + System.Numerics.Complex.Sqrt((1.0/(z*z)) + 1));
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex arcosech(System.Numerics.Complex z)
        {
            return arcsch(z);
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex arsech(System.Numerics.Complex z)
        {
            return
                System.Numerics.Complex.Log((1.0/z) +
                                            System.Numerics.Complex.Sqrt((1.0/z) + 1)*
                                            System.Numerics.Complex.Sqrt((1.0/z) - 1));
        }

        #endregion

        #region logarithmic functions

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex ln(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Log(z);
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex log2(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Log(z, 2);
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex log10(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Log10(z);
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex log(System.Numerics.Complex z, double nbase)
        {
            return System.Numerics.Complex.Log(z, nbase);
        }

        #endregion

        #region root functions

        /*Square root"),
         System.ComponentModel.Description(
             "square root of a number a is a number y such that y2 = x, in other words, a number y whose square (the result of multiplying the number by itself, or y × y) is x."
             ), System.ComponentModel.DisplayName("Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)*/
        public static System.Numerics.Complex sqrt(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Sqrt(z);
        }

        /*Root function"), System.ComponentModel.Description("root function of the given value"),
         System.ComponentModel.DisplayName("Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)*/
        public static System.Numerics.Complex root(System.Numerics.Complex z, System.Numerics.Complex n)
        {
            //n-based root
            return System.Numerics.Complex.Pow(z, 1.0/n);
        }

        #endregion

        #region power functions

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex pow(System.Numerics.Complex x, System.Numerics.Complex y)
        {
            return System.Numerics.Complex.Pow(x, y);
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Complexu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static System.Numerics.Complex exp(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Exp(z);
        }

        #endregion

        #region integer functions

        public static double sgn(double x)
        {
            return (double.IsNaN(x)) ? double.NaN : (double) (System.Math.Sign(x));
        }



        public static double gcd(long a, long b)
        {
            return Meta.Numerics.Functions.AdvancedIntegerMath.GCF(a,b);
        }

        public static double nwd(long a, long b)
        {
            return gcd(a, b);
        }



        public static double lcm(long a, long b)
        {
            if (a == 0 || b == 0)
                return double.NaN;
            return Meta.Numerics.Functions.AdvancedIntegerMath.LCM(a, b);
        }

        public static double nww(long a, long b)
        {
            return lcm(a, b);
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double factorial(double n)
        {
            double f = 1;
            for (ulong i = 1; i <= n; i++)
                f = f*i;
            return f;
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static int prime(double d)
        {
            if (d < 1)
                return -1;
            if (Meta.Numerics.Functions.AdvancedIntegerMath.IsPrime((int) (d)))
                return 1;
            return 0;
        }

        public static bool isPrime(int n)
        {
            if(n <= 0)
                return false;
            else
                return Meta.Numerics.Functions.AdvancedIntegerMath.IsPrime(n);
        }

        private static bool[] GetPrimeSieve(long upTo)
        {
            long sieveSize = upTo + 1;
            bool[] sieve = new bool[sieveSize];
            System.Array.Clear(sieve, 0, (int)sieveSize);
            sieve[0] = true;
            sieve[1] = true;
            long p, max = (long)System.Math.Sqrt(sieveSize) + 1;
            for (long i = 2; i <= max; i++)
            {
                if (sieve[i]) continue;
                p = i + i;
                while (p < sieveSize) { sieve[p] = true; p += i; }
            }
            return sieve;
        }

        private static long[] GetPrimesUpTo(long upTo)
        {
            if (upTo < 2) return null;
            bool[] sieve = GetPrimeSieve(upTo);
            long[] primes = new long[upTo + 1];

            long index = 0;
            for (long i = 2; i <= upTo; i++) if (!sieve[i]) primes[index++] = i;

            System.Array.Resize(ref primes, (int)index);
            return primes;
        }


        public static double Eulerφ(long n)
        {
            if (n < 1)
                return double.NaN;
            var primes = GetPrimesUpTo(n + 1);    //this can be precalculated beforehand
            int numPrimes = primes.Length;

            long totient = n;
            long currentNum = n, temp, p, prevP = 0;
            for (int i = 0; i < numPrimes; i++)
            {
                p = (int)primes[i];
                if (p > currentNum) break;
                temp = currentNum / p;
                if (temp * p == currentNum)
                {
                    currentNum = temp;
                    i--;
                    if (prevP != p) { prevP = p; totient -= (totient / p); }
                }
            }
            return totient;
        }

        #endregion

        #region complex specific functions


        public static System.Numerics.Complex conj(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Conjugate(z);
        }

        public static double Re(System.Numerics.Complex z)
        {
            return z.Real;
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/

        public static double Im(System.Numerics.Complex z)
        {
            return z.Imaginary;
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/

        public static double Phase(System.Numerics.Complex z)
        {
            return z.Phase;
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/

        public static double Magnitude(System.Numerics.Complex z)
        {
            return z.Magnitude;
        }

        #endregion

        #region a step like functions (non continuous)




        public static double H(double x)
        {
            return Heaviside(x);
        }
        public static double δ(double x)
        {
            return DiracDelta(x);
        }

    
    public static double δij(double i, double j)
    {
        return KroneckerDelta(i,j);
    }

        public static double abs(double x)
        {
            if (x >= 0.0)
                return x;
            return -x;
        }

        public static System.Numerics.Complex abs(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Abs(z);
        }

        public static double Heaviside(double x)
        {
            if (x > 0.0)
                return 1;
            if (x < 0.0)
                return 0;
            return 0.5;
        }



        public static double DiracDelta(double x)
        {
            if (x != 0.0)
                return 0;
            return double.PositiveInfinity;
        }

        /*Angielska nazwa funkcji"),
         System.ComponentModel.Category("Tu wpisz nazwę regionu (np. trigonometric functions)"),
         System.ComponentModel.Description("angielski opis funkcji*/
        public static double KroneckerDelta(double i, double j)
        {
            if (i != j)
                return 0;
            return 1;
        }

        #endregion

        #region coefficients and special values

        [System.Runtime.InteropServices.DllImport(gslSfLibDir2,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_taylorcoeff(int n, double x);

        [System.Runtime.InteropServices.DllImport(gslSfLibDir2,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_doublefact(uint n);

        [System.Runtime.InteropServices.DllImport(gslSfLibDir2,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_choose(uint n, uint m);

        public static double TaylorCoeff(int n, double x)
        {
            if (n < 0 || x < 0) return double.NaN;
            return gsl_sf_taylorcoeff(n, x);
        }

        public static double DoubleFactorial(double x)
        {
            return DoubleFactorial((long) x);
        }

        public static double DoubleFactorial(long n)
        {
            if (n < 0)
                return double.NaN;

            return gsl_sf_doublefact((uint) n);
        }

        public static double BinomialCoeff(long n, long k)
        {
            if (n < 0 || k < 0 || k > n)
            {
                return double.NaN;
            }
            return gsl_sf_choose((uint) n, (uint) k);
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

        public static double Permutations(int n)
        {
            if (n < 0)
                return 0;
            return MathNet.Numerics.Combinatorics.Permutations(n);
            //MathNet.Numerics.Combinatorics' and 'Accord.Math.Combinatorics'
        }

        #endregion

        #region utils

        private const string gslSfLibDir2 = "gsl.dll"; //"libgsl-0.dll";

        #endregion

        public const string ToCode =
            @"
 
        #region trigonometric functions


        public static double sinc(double x)
        {
            if (x == 0)
                return 1;
            return (System.Math.Sin((x)))/(x);
        }


        public static double sin(double x)
        {
            return System.Math.Sin((x));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double cos(double x)
        {
            return System.Math.Cos((x));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double tan(double x)
        {
            return System.Math.Tan((x));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double tg(double x)
        {
            return System.Math.Tan((x));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double ctg(double x)
        {
            return 1.0/System.Math.Tan((x));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double cot(double x)
        {
            return 1.0/System.Math.Tan((x));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double sec(double x)
        {
            return 1.0/System.Math.Cos((x));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double csc(double x)
        {
            return 1.0/System.Math.Sin((x));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double cosec(double x)
        {
            return 1.0/System.Math.Sin((x));
        }

        #endregion

        #region hyperbolic trygonometric functions


        public static double sinh(double x)
        {
            return System.Math.Sinh((x));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double cosh(double x)
        {
            return System.Math.Cosh((x));
        }


        public static double tanh(double x)
        {
            return System.Math.Tanh((x));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double tgh(double x)
        {
            return System.Math.Tanh((x));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double coth(double x)
        {
            return (1.0/System.Math.Tanh((x)));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double ctgh(double x)
        {
            return (1.0/System.Math.Tanh((x)));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double sech(double x)
        {
            return (1.0/System.Math.Cosh((x)));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double csch(double x)
        {
            return (1.0/System.Math.Sinh((x)));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double cosech(double x)
        {
            return (1.0/System.Math.Sinh((x)));
        }

        #endregion

        #region arcus trigonometric functions

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double arcsin(double x)
        {
            return System.Math.Asin((x));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double arccos(double x)
        {
            return System.Math.Acos((x));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double arctan(double x)
        {
            return System.Math.Atan((x));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double arctg(double x)
        {
            return System.Math.Atan((x));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double arccot(double x)
        {
            return (System.Math.PI/2 - System.Math.Atan((x)));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double arcctg(double x)
        {
            return (System.Math.PI/2 - System.Math.Atan((x)));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double arcsec(double x)
        {
            return (System.Math.Acos(1.0/(x)));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double arccsc(double x)
        {
            return (System.Math.Asin(1.0/(x)));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double arccosec(double x)
        {
            return (System.Math.Asin(1.0/(x)));
        }

        #endregion

        #region area hyperbolic trigonometric functions

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double arsinh(double x)
        {
            return System.Math.Log((x)) + System.Math.Sqrt((x)*(x) + 1);
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double arcosh(double x)
        {
            return System.Math.Log((x) + System.Math.Sqrt(x + 1)*System.Math.Sqrt(x - 1));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double artanh(double x)
        {
            return 0.5*System.Math.Log((1 + x)/(1 - x));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double artgh(double x)
        {
            return 0.5*System.Math.Log((1 + x)/(1 - x));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double arcoth(double x)
        {
            return 0.5*System.Math.Log((x + 1)/(x - 1));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double arctgh(double x)
        {
            return 0.5*System.Math.Log((x + 1)/(x - 1));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double arcsch(double x)
        {
            return System.Math.Log((1.0/x) + System.Math.Sqrt((1.0/(x*x)) + 1));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double arcosech(double x)
        {
            return arcsch(x);
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double arsech(double x)
        {
            return System.Math.Log((1.0/x) + System.Math.Sqrt((1.0/x) + 1)*System.Math.Sqrt((1.0/x) - 1));
        }

        #endregion

        #region logarithmic functions

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double ln(double x)
        {
            return System.Math.Log(x);
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double log2(double x)
        {
            return System.Math.Log(x, 2);
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double log10(double x)
        {
            return System.Math.Log10(x);
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double log(double value, double nbase)
        {
            return System.Math.Log(value, nbase);
        }

        #endregion

        #region root functions

        /*Square root""),
         System.ComponentModel.Description(
             ""square root of a number a is a number y such that y2 = x, in other words, a number y whose square (the result of multiplying the number by itself, or y × y) is x.""
             ), System.ComponentModel.DisplayName(""Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)*/
       /* public static double sqrt(double x)
        {
            return System.Math.Sqrt(x);
        }*/

        public static dynamic sqrt(double x)
        {
            if (x < 0)
                return System.Numerics.Complex.Sqrt(x);
            return System.Math.Sqrt(x);
        }

        /*Root function""), System.ComponentModel.Description(""root function of the given value""),
         System.ComponentModel.DisplayName(""Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)*/
        public static double root(double value, double n)
        {
            //n-based root
            return System.Math.Pow(value, 1.0/n);
        }

        #endregion

        #region power functions

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double pow(double x, double y)
        {
            return System.Math.Pow(x, y);
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""doubleu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double exp(double x)
        {
            return System.Math.Exp(x);
        }

        #endregion

        #region trigonometric functions

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex sinc(System.Numerics.Complex z)
        {
            if (z == 0)
                return 1;
            return (System.Numerics.Complex.Sin((z)))/(z);
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex sin(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Sin((z));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex cos(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Cos((z));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex tan(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Tan((z));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex tg(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Tan((z));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex ctg(System.Numerics.Complex z)
        {
            return 1.0/System.Numerics.Complex.Tan((z));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex cot(System.Numerics.Complex z)
        {
            return 1.0/System.Numerics.Complex.Tan((z));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex sec(System.Numerics.Complex z)
        {
            return 1.0/System.Numerics.Complex.Cos((z));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex csc(System.Numerics.Complex z)
        {
            return 1.0/System.Numerics.Complex.Sin((z));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex cosec(System.Numerics.Complex z)
        {
            return 1.0/System.Numerics.Complex.Sin((z));
        }

        #endregion

        #region hyperbolic trygonometric functions

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex sinh(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Sinh((z));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex cosh(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Cosh((z));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex tanh(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Tanh((z));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex tgh(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Tanh((z));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex coth(System.Numerics.Complex z)
        {
            return (1.0/System.Numerics.Complex.Tanh((z)));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex ctgh(System.Numerics.Complex z)
        {
            return (1.0/System.Numerics.Complex.Tanh((z)));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex sech(System.Numerics.Complex z)
        {
            return (1.0/System.Numerics.Complex.Cosh((z)));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex csch(System.Numerics.Complex z)
        {
            return (1.0/System.Numerics.Complex.Sinh((z)));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex cosech(System.Numerics.Complex z)
        {
            return (1.0/System.Numerics.Complex.Sinh((z)));
        }

        #endregion

        #region arcus trigonometric functions

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex arcsin(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Asin((z));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex arccos(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Acos((z));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex arctan(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Atan((z));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex arctg(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Atan((z));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex arccot(System.Numerics.Complex z)
        {
            return (System.Math.PI/2 - System.Numerics.Complex.Atan((z)));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex arcctg(System.Numerics.Complex z)
        {
            return (System.Math.PI/2 - System.Numerics.Complex.Atan((z)));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex arcsec(System.Numerics.Complex z)
        {
            return (System.Numerics.Complex.Acos(1.0/(z)));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex arccsc(System.Numerics.Complex z)
        {
            return (System.Numerics.Complex.Asin(1.0/(z)));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex arccosec(System.Numerics.Complex z)
        {
            return (System.Numerics.Complex.Asin(1.0/(z)));
        }

        #endregion

        #region area hyperbolic trigonometric functions

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex arsinh(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Log((z)) + System.Numerics.Complex.Sqrt((z)*(z) + 1);
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex arcosh(System.Numerics.Complex z)
        {
            return
                System.Numerics.Complex.Log((z) +
                                            System.Numerics.Complex.Sqrt(z + 1)*System.Numerics.Complex.Sqrt(z - 1));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex artanh(System.Numerics.Complex z)
        {
            return 0.5*System.Numerics.Complex.Log((1 + z)/(1 - z));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex artgh(System.Numerics.Complex z)
        {
            return 0.5*System.Numerics.Complex.Log((1 + z)/(1 - z));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex arcoth(System.Numerics.Complex z)
        {
            return 0.5*System.Numerics.Complex.Log((z + 1)/(z - 1));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex arctgh(System.Numerics.Complex z)
        {
            return 0.5*System.Numerics.Complex.Log((z + 1)/(z - 1));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex arcsch(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Log((1.0/z) + System.Numerics.Complex.Sqrt((1.0/(z*z)) + 1));
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex arcosech(System.Numerics.Complex z)
        {
            return arcsch(z);
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex arsech(System.Numerics.Complex z)
        {
            return
                System.Numerics.Complex.Log((1.0/z) +
                                            System.Numerics.Complex.Sqrt((1.0/z) + 1)*
                                            System.Numerics.Complex.Sqrt((1.0/z) - 1));
        }

        #endregion

        #region logarithmic functions

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex ln(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Log(z);
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex log2(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Log(z, 2);
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex log10(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Log10(z);
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex log(System.Numerics.Complex z, double nbase)
        {
            return System.Numerics.Complex.Log(z, nbase);
        }

        #endregion

        #region root functions

        /*Square root""),
         System.ComponentModel.Description(
             ""square root of a number a is a number y such that y2 = x, in other words, a number y whose square (the result of multiplying the number by itself, or y × y) is x.""
             ), System.ComponentModel.DisplayName(""Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)*/
        public static System.Numerics.Complex sqrt(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Sqrt(z);
        }

        /*Root function""), System.ComponentModel.Description(""root function of the given value""),
         System.ComponentModel.DisplayName(""Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)*/
        public static System.Numerics.Complex root(System.Numerics.Complex z, System.Numerics.Complex n)
        {
            //n-based root
            return System.Numerics.Complex.Pow(z, 1.0/n);
        }

        #endregion

        #region power functions

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex pow(System.Numerics.Complex x, System.Numerics.Complex y)
        {
            return System.Numerics.Complex.Pow(x, y);
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Complexu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static System.Numerics.Complex exp(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Exp(z);
        }

        #endregion

        #region integer functions

        public static double sgn(double x)
        {
            return (double.IsNaN(x)) ? double.NaN : (double) (System.Math.Sign(x));
        }



        public static double gcd(long a, long b)
        {
            return Meta.Numerics.Functions.AdvancedIntegerMath.GCF(a,b);
        }

        public static double nwd(long a, long b)
        {
            return gcd(a, b);
        }



        public static double lcm(long a, long b)
        {
            if (a == 0 || b == 0)
                return double.NaN;
            return Meta.Numerics.Functions.AdvancedIntegerMath.LCM(a, b);
        }

        public static double nww(long a, long b)
        {
            return lcm(a, b);
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double factorial(double n)
        {
            double f = 1;
            for (ulong i = 1; i <= n; i++)
                f = f*i;
            return f;
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static int prime(double d)
        {
            if (d < 1)
                return -1;
            if (Meta.Numerics.Functions.AdvancedIntegerMath.IsPrime((int) (d)))
                return 1;
            return 0;
        }

        public static bool isPrime(int n)
        {
            if(n <= 0)
                return false;
            else
                return Meta.Numerics.Functions.AdvancedIntegerMath.IsPrime(n);
        }

        private static bool[] GetPrimeSieve(long upTo)
        {
            long sieveSize = upTo + 1;
            bool[] sieve = new bool[sieveSize];
            System.Array.Clear(sieve, 0, (int)sieveSize);
            sieve[0] = true;
            sieve[1] = true;
            long p, max = (long)System.Math.Sqrt(sieveSize) + 1;
            for (long i = 2; i <= max; i++)
            {
                if (sieve[i]) continue;
                p = i + i;
                while (p < sieveSize) { sieve[p] = true; p += i; }
            }
            return sieve;
        }

        private static long[] GetPrimesUpTo(long upTo)
        {
            if (upTo < 2) return null;
            bool[] sieve = GetPrimeSieve(upTo);
            long[] primes = new long[upTo + 1];

            long index = 0;
            for (long i = 2; i <= upTo; i++) if (!sieve[i]) primes[index++] = i;

            System.Array.Resize(ref primes, (int)index);
            return primes;
        }


        public static double Eulerφ(long n)
        {
            if (n < 1)
                return double.NaN;
            var primes = GetPrimesUpTo(n + 1);    //this can be precalculated beforehand
            int numPrimes = primes.Length;

            long totient = n;
            long currentNum = n, temp, p, prevP = 0;
            for (int i = 0; i < numPrimes; i++)
            {
                p = (int)primes[i];
                if (p > currentNum) break;
                temp = currentNum / p;
                if (temp * p == currentNum)
                {
                    currentNum = temp;
                    i--;
                    if (prevP != p) { prevP = p; totient -= (totient / p); }
                }
            }
            return totient;
        }

        #endregion

        #region complex specific functions


        public static System.Numerics.Complex conj(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Conjugate(z);
        }

        public static double Re(System.Numerics.Complex z)
        {
            return z.Real;
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/

        public static double Im(System.Numerics.Complex z)
        {
            return z.Imaginary;
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/

        public static double Phase(System.Numerics.Complex z)
        {
            return z.Phase;
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/

        public static double Magnitude(System.Numerics.Complex z)
        {
            return z.Magnitude;
        }

        #endregion

        #region a step like functions (non continuous)




        public static double H(double x)
        {
            return Heaviside(x);
        }
        public static double δ(double x)
        {
            return DiracDelta(x);
        }

    
    public static double δij(double i, double j)
    {
        return KroneckerDelta(i,j);
    }

        public static double abs(double x)
        {
            if (x >= 0.0)
                return x;
            return -x;
        }

        public static System.Numerics.Complex abs(System.Numerics.Complex z)
        {
            return System.Numerics.Complex.Abs(z);
        }

        public static double Heaviside(double x)
        {
            if (x > 0.0)
                return 1;
            if (x < 0.0)
                return 0;
            return 0.5;
        }



        public static double DiracDelta(double x)
        {
            if (x != 0.0)
                return 0;
            return double.PositiveInfinity;
        }

        /*Angielska nazwa funkcji""),
         System.ComponentModel.Category(""Tu wpisz nazwę regionu (np. trigonometric functions)""),
         System.ComponentModel.Description(""angielski opis funkcji*/
        public static double KroneckerDelta(double i, double j)
        {
            if (i != j)
                return 0;
            return 1;
        }

        #endregion

        #region coefficients and special values

        [System.Runtime.InteropServices.DllImport(gslSfLibDir2,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_taylorcoeff(int n, double x);

        [System.Runtime.InteropServices.DllImport(gslSfLibDir2,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_doublefact(uint n);

        [System.Runtime.InteropServices.DllImport(gslSfLibDir2,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_choose(uint n, uint m);

        public static double TaylorCoeff(int n, double x)
        {
            if (n < 0 || x < 0) return double.NaN;
            return gsl_sf_taylorcoeff(n, x);
        }

        public static double DoubleFactorial(double x)
        {
            return DoubleFactorial((long) x);
        }

        public static double DoubleFactorial(long n)
        {
            if (n < 0)
                return double.NaN;

            return gsl_sf_doublefact((uint) n);
        }

        public static double BinomialCoeff(long n, long k)
        {
            if (n < 0 || k < 0 || k > n)
            {
                return double.NaN;
            }
            return gsl_sf_choose((uint) n, (uint) k);
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

        public static double Permutations(int n)
        {
            if (n < 0)
                return 0;
            return MathNet.Numerics.Combinatorics.Permutations(n);
            //MathNet.Numerics.Combinatorics' and 'Accord.Math.Combinatorics'
        }

        #endregion

        #region utils

        private const string gslSfLibDir2 = ""gsl.dll""; //""libgsl-0.dll"";

        #endregion

";
    }
}