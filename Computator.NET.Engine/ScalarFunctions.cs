using System.Numerics;

namespace Computator.NET.Engine;

/// <summary>TSL spelling adapters to the BCL for the managed portable execution surface.</summary>
public static class ScalarFunctions
{
    public static double sin(double x) => Math.Sin(x);
    public static Complex sin(Complex z) => Complex.Sin(z);
    public static double cos(double x) => Math.Cos(x);
    public static Complex cos(Complex z) => Complex.Cos(z);
    public static double tan(double x) => Math.Tan(x);
    public static Complex tan(Complex z) => Complex.Tan(z);
    public static double tg(double x) => tan(x);
    public static Complex tg(Complex z) => tan(z);
    public static double arcsin(double x) => Math.Asin(x);
    public static Complex arcsin(Complex z) => Complex.Asin(z);
    public static double arccos(double x) => Math.Acos(x);
    public static Complex arccos(Complex z) => Complex.Acos(z);
    public static double arctan(double x) => Math.Atan(x);
    public static Complex arctan(Complex z) => Complex.Atan(z);
    public static double arctg(double x) => arctan(x);
    public static Complex arctg(Complex z) => arctan(z);
    public static double sinh(double x) => Math.Sinh(x);
    public static Complex sinh(Complex z) => Complex.Sinh(z);
    public static double cosh(double x) => Math.Cosh(x);
    public static Complex cosh(Complex z) => Complex.Cosh(z);
    public static double tanh(double x) => Math.Tanh(x);
    public static Complex tanh(Complex z) => Complex.Tanh(z);
    public static double tgh(double x) => tanh(x);
    public static Complex tgh(Complex z) => tanh(z);
    public static double sqrt(double x) => Math.Sqrt(x);
    public static Complex sqrt(Complex z) => Complex.Sqrt(z);
    public static double exp(double x) => Math.Exp(x);
    public static Complex exp(Complex z) => Complex.Exp(z);
    public static double ln(double x) => Math.Log(x);
    public static Complex ln(Complex z) => Complex.Log(z);
    public static double log(double x) => Math.Log10(x);
    public static Complex log(Complex z) => Complex.Log10(z);
    public static double log2(double x) => Math.Log(x, 2);
    public static Complex log2(Complex z) => Complex.Log(z, 2);
    public static double log10(double x) => Math.Log10(x);
    public static Complex log10(Complex z) => Complex.Log10(z);
    public static double log(double x, double y) => Math.Log(x, y);
    public static Complex log(Complex z, double nbase) => Complex.Log(z, nbase);
    public static double abs(double x) => Math.Abs(x);
    public static double abs(Complex z) => z.Magnitude;
    public static double pow(double x, double y) => Math.Pow(x, y);
    public static Complex pow(Complex x, Complex y) => Complex.Pow(x, y);
    public static double floor(double x) => Math.Floor(x);
    public static double ceil(double x) => Math.Ceiling(x);
    public static double round(double x) => Math.Round(x);
    public static Complex conj(Complex z) => Complex.Conjugate(z);
    public static double Re(Complex z) => z.Real;
    public static double Im(Complex z) => z.Imaginary;
    public static double Phase(Complex z) => z.Phase;
    public static double Magnitude(Complex z) => z.Magnitude;
}

// The existing TSL transformer emits TypeDeducer.Func for local function syntax.
public static class TypeDeducer
{
    public static Func<R> Func<R>(Func<R> value) => value;
    public static Func<A, R> Func<A, R>(Func<A, R> value) => value;
    public static Func<A, B, R> Func<A, B, R>(Func<A, B, R> value) => value;
    public static Func<A, B, C, R> Func<A, B, C, R>(Func<A, B, C, R> value) => value;
    public static Func<A, B, C, D, R> Func<A, B, C, D, R>(Func<A, B, C, D, R> value) => value;
    public static Func<A, B, C, D, E, R> Func<A, B, C, D, E, R>(Func<A, B, C, D, E, R> value) => value;
    public static Func<A, B, C, D, E, F, R> Func<A, B, C, D, E, F, R>(Func<A, B, C, D, E, F, R> value) => value;
    public static Func<A, B, C, D, E, F, G, R> Func<A, B, C, D, E, F, G, R>(Func<A, B, C, D, E, F, G, R> value) => value;
    public static Func<A, B, C, D, E, F, G, H, R> Func<A, B, C, D, E, F, G, H, R>(Func<A, B, C, D, E, F, G, H, R> value) => value;
}
