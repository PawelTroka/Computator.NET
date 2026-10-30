using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Computator.NET.Core.NumericalCalculations;
using Computator.NET.DataTypes.Configuration;
using Computator.NET.Engine;

internal static class MonoNumericalProbe
{
    private delegate double Root(Func<double, double> f, double a, double b, double tolerance, uint iterations);
    private delegate double Quadrature(Func<double, double> f, double a, double b, double panels);
    private static readonly List<XElement> Results = new List<XElement>();
    private static int Failed;

    [STAThread]
    private static int Main(string[] args)
    {
        Run("actual-mono-runtime", () => Check(Type.GetType("Mono.Runtime") != null, "Mono runtime is required"));
        Console.WriteLine("Runtime: " + typeof(object).Assembly.Location);
        Console.WriteLine("Core: " + typeof(SpecialFunctions).Assembly.Location);
        Console.WriteLine("Engine: " + typeof(PortableEngine).Assembly.Location);
        NumericalCases("host", FunctionRoot.secantMethod, Integral.trapezoidalMethod);
        using (var compiler = new NativeCompiler())
        {
            Root root = null;
            Quadrature integral = null;
            Run("compile-complete-emitted-numerics", () =>
            {
                var assembly = compiler.Compile(FunctionRoot.ToCode + "\n" + Integral.ToCode);
                root = (Root)Delegate.CreateDelegate(typeof(Root), assembly.GetType("FunctionRoot").GetMethod("secantMethod"));
                integral = (Quadrature)Delegate.CreateDelegate(typeof(Quadrature), assembly.GetType("Integral").GetMethod("trapezoidalMethod"));
            });
            if (root != null && integral != null) NumericalCases("emitted", root, integral);
        }

        foreach (var emitted in new[] { false, true })
        {
            var label = emitted ? "emitted" : "host";
            var definitions = emitted ? FunctionRoot.ToCode + "\n" + Integral.ToCode : "";
            var engine = new PortableEngine();
            Run(label + "/tsl-scientific-power-secant", () => Near(engine.EvaluateReal(
                "FunctionRoot.secantMethod(t => t-2e-3²,0,1,1e-19,8)", customFunctions: definitions), 0.000004, 4e-20));
            Run(label + "/tsl-large-scientific-root", () => Near(engine.EvaluateReal(
                "FunctionRoot.secantMethod(t => t-1e100²,.5e200,2e200,0,8)", customFunctions: definitions), 1e200, 4e185));
            Run(label + "/tsl-euclid-panels-overflow-integral", () => Near(engine.EvaluateReal(
                "Integral.trapezoidalMethod(t => 1e154²,0,.5²,Euclid.Log2(16))", customFunctions: definitions), 2.5e307, 1e293));
            Run(label + "/tsl-euclid-subnormal-width", () => Near(engine.EvaluateReal(
                "Integral.trapezoidalMethod(t => System.Math.Pow(2.0,1022)*1e0²,0,System.Double.Epsilon*Euclid.Modulus(-7,5),1)",
                customFunctions: definitions), 3 * Math.Pow(2, -52), 0));
            Run(label + "/tsl-out-arguments-and-numerical-script", () =>
            {
                var output = engine.ExecuteScript("integer u,v; integer d=Euclid.ExtendedGreatestCommonDivisor(240,46,out u,out v); " +
                    "real value2=.5; real root=FunctionRoot.secantMethod(t => d*t-value2²,0,1,0,4); " +
                    "real area=Integral.trapezoidalMethod(t => Euclid.PowerOfTwo((integer)40)*root,0,.5²,4); " +
                    "write(root); write(\":\"); write(area); write(\":\"); write(240*u+46*v);", definitions);
                Check(output == "0.125:34359738368:2", output);
            });
        }

        Run("initialize-system-gsl", GSLInitializer.Initialize);
        DebyeCases("host", SpecialFunctions.Debye);
        foreach (var name in new[] { "PrecompiledExpressionSurface", "PrecompiledScriptMathSurface" })
        {
            Func<int, double, double> debye = null;
            Run("load/" + name, () =>
            {
                var type = typeof(SpecialFunctions).Assembly.GetType("Computator.NET.Core.Compilation.Builtins." + name, true);
                debye = (Func<int, double, double>)Delegate.CreateDelegate(typeof(Func<int, double, double>), type.GetMethod("Debye"));
            });
            if (debye != null) DebyeCases(name, debye);
        }
        using (var compiler = new NativeCompiler())
        {
            Func<int, double, double> debye = null;
            Run("compile-complete-emitted-special-functions", () =>
            {
                compiler.AddDll(typeof(GslConfig).Assembly.Location);
                var assembly = compiler.Compile("public static class EmittedSpecialFunctions {" + SpecialFunctions.ToCode + "}");
                debye = (Func<int, double, double>)Delegate.CreateDelegate(typeof(Func<int, double, double>),
                    assembly.GetType("EmittedSpecialFunctions").GetMethod("Debye"));
            });
            if (debye != null) DebyeCases("emitted", debye);
        }
        new XDocument(new XElement("tests", new XAttribute("total", Results.Count), new XAttribute("failed", Failed),
            new XAttribute("passed", Results.Count - Failed), new XAttribute("skipped", 0), Results)).Save(args[0]);
        Console.WriteLine("RESULT: " + (Results.Count - Failed) + " passed, " + Failed + " failed, 0 skipped");
        return Failed == 0 ? 0 : 1;
    }

    private static void NumericalCases(string surface, Root root, Quadrature integral)
    {
        foreach (var exponent in new[] { 700, -700 })
        {
            var scale = Math.Pow(2, exponent);
            Run(surface + "/secant-binary-scale-" + exponent, () => Near(root(x => x - 1.5 * scale, scale, 2 * scale, 0, 3), 1.5 * scale, 0));
        }
        foreach (var scale in new[] { 1e308, 1e-308 })
            Run(surface + "/secant-residual-scale-" + scale.ToString("R", CultureInfo.InvariantCulture),
                () => Near(root(x => scale * x, -1, 1, 0, 3), 0, 0));
        Run(surface + "/secant-opposite-max-endpoints", () => Near(root(x => x / double.MaxValue, -double.MaxValue, double.MaxValue, 0, 3), 0, 0));
        var large = Math.Pow(2, 1022);
        Run(surface + "/secant-extreme-extrapolation", () => Near(root(x => x / large - 3, -3 * large, -2 * large, 0, 3), 3 * large, 0));
        Run(surface + "/secant-underflowing-residual-ratio", () => Near(root(x => x - 1e-200, 0, 1e200, 0, 3), 1e-200, 4e-215));
        Run(surface + "/secant-adjacent-residuals", () => Near(root(x => x - 1e16, 0, 2, 0, 2), 1e16, 0));
        Run(surface + "/trapezoid-overflowing-sum", () => Near(integral(x => 1e308, 0, 1, 2), 1e308, 0));
        Run(surface + "/trapezoid-max-value", () => Near(integral(x => double.MaxValue, 0, 1, 1), double.MaxValue, 0));
        var epsilon = BitConverter.Int64BitsToDouble(1);
        foreach (var units in new[] { 1, 3, 5 })
            Run(surface + "/trapezoid-subnormal-width-" + units,
                () => Near(integral(x => large, 0, units * epsilon, 1), units * Math.Pow(2, -52), 0));
        Run(surface + "/trapezoid-subnormal-negative-reversed", () => Near(integral(x => -large, 3 * epsilon, 0, 1), 3 * Math.Pow(2, -52), 0));
        Run(surface + "/trapezoid-small-samples", () => Near(integral(x => epsilon, 0, 1, 1), epsilon, 0));
        Run(surface + "/trapezoid-actual-infinity", () => Check(double.IsPositiveInfinity(integral(x => double.PositiveInfinity, 0, 1, 1)), "Expected positive infinity"));
    }

    private static void DebyeCases(string surface, Func<int, double, double> debye)
    {
        // Independently integrated normalized values at x=1, also used by the
        // committed numerical regressions; each value is already rounded to binary64.
        var values = new[] { 0.7775046341122482, 0.7078784756278292, 0.6744155640778147, 0.6548740688867369, 0.6421002580217791, 0.6331114258349511 };
        for (var order = 1; order <= 6; order++)
        {
            var n = order;
            Run(surface + "/debye-x1-order-" + n, () => Near(debye(n, 1), values[n - 1], 2e-14));
            Run(surface + "/debye-zero-order-" + n, () => Near(debye(n, 0), 1, 0));
        }
        foreach (var order in new[] { -1, 0, 7, int.MaxValue })
            Run(surface + "/debye-unsupported-order-" + order, () => Check(double.IsNaN(debye(order, 10)), "Unsupported Debye order must return NaN"));
        Run(surface + "/debye-negative-x", () => Check(double.IsNaN(debye(3, -1)), "Negative argument must return NaN"));
    }

    private static void Near(double actual, double expected, double tolerance)
    {
        Check(!double.IsNaN(actual) && !double.IsInfinity(actual) && Math.Abs(actual - expected) <= tolerance,
            "Actual " + actual.ToString("R", CultureInfo.InvariantCulture) + "; expected " + expected.ToString("R", CultureInfo.InvariantCulture));
    }
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static void Run(string name, Action test)
    {
        try { test(); Results.Add(new XElement("test", new XAttribute("name", name), new XAttribute("result", "passed"))); Console.WriteLine("PASS " + name); }
        catch (Exception error) { Failed++; Results.Add(new XElement("test", new XAttribute("name", name), new XAttribute("result", "failed"), new XElement("error", error.ToString()))); Console.WriteLine("FAIL " + name + ": " + error); }
    }
}
