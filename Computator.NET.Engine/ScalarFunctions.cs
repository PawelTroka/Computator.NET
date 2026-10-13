using System.Numerics;

namespace Computator.NET.Engine;

/// <summary>TSL spelling adapters to the BCL for the initial portable execution surface.</summary>
public static class ScalarFunctions
{
    public static double sin(double x) => Math.Sin(x);
    public static Complex sin(Complex z) => Complex.Sin(z);
    public static double cos(double x) => Math.Cos(x);
    public static Complex cos(Complex z) => Complex.Cos(z);
    public static double tan(double x) => Math.Tan(x);
    public static Complex tan(Complex z) => Complex.Tan(z);
    public static double sqrt(double x) => Math.Sqrt(x);
    public static Complex sqrt(Complex z) => Complex.Sqrt(z);
    public static double exp(double x) => Math.Exp(x);
    public static Complex exp(Complex z) => Complex.Exp(z);
    public static double ln(double x) => Math.Log(x);
    public static Complex ln(Complex z) => Complex.Log(z);
    public static double log(double x) => Math.Log10(x);
    public static Complex log(Complex z) => Complex.Log10(z);
    public static double abs(double x) => Math.Abs(x);
    public static double abs(Complex z) => z.Magnitude;
    public static double pow(double x, double y) => Math.Pow(x, y);
    public static Complex pow(Complex x, Complex y) => Complex.Pow(x, y);
    public static double floor(double x) => Math.Floor(x);
    public static double ceil(double x) => Math.Ceiling(x);
    public static double round(double x) => Math.Round(x);
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
