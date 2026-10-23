namespace Computator.NET.Engine;

public sealed partial class PortableEngine
{
    // Compile once per numerical operation, never once per integrand evaluation.
    private static Compiled<Func<double, double>> CompileScalar(string expression, string functions) =>
        Compile<Func<double, double>>(expression, functions,
            "public static double Run(double x) { return\n#line 1 \"expression.tsl\"\n", ";\n#line default\n}");

    public double Integrate(string expression, double a, double b, int steps = 10000, string customFunctions = "")
    {
        Bounds(a, b);
        if (steps < 2 || steps > 1000000 || steps % 2 != 0) throw new ArgumentOutOfRangeException(nameof(steps), "Simpson integration requires an even count from 2 to 1,000,000.");
        using var program = CompileScalar(expression, customFunctions);
        return Core.NumericalCalculations.Integral.simpsonMethod(program.Invoke, a, b, steps);
    }
    public double Differentiate(string expression, double x, uint order = 1, string customFunctions = "")
    {
        if (!IsFinite(x) || order < 1 || order > 10) throw new ArgumentOutOfRangeException(nameof(order));
        using var program = CompileScalar(expression, customFunctions);
        return Core.NumericalCalculations.Derivative.derivative(program.Invoke, x, order);
    }
    public double FindRoot(string expression, double a, double b, double tolerance = 1e-8, uint iterations = 10000, string customFunctions = "")
    {
        Bounds(a, b);
        if (!IsFinite(tolerance) || tolerance <= 0 || tolerance > 1 || iterations < 1 || iterations > 1000000)
            throw new ArgumentOutOfRangeException(nameof(tolerance));
        using var program = CompileScalar(expression, customFunctions);
        return Core.NumericalCalculations.FunctionRoot.BrentMethod(program.Invoke, a, b, tolerance, iterations);
    }
    private static void Bounds(double a, double b)
    {
        if (!IsFinite(a) || !IsFinite(b) || a >= b) throw new ArgumentException("Expected finite increasing interval bounds.");
    }

#if NETFRAMEWORK
    private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
#else
    private static bool IsFinite(double value) => double.IsFinite(value);
#endif
}
