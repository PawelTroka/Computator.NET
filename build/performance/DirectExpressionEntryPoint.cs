using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Functions;

// Identical source for the historical and modern worker. The original Worker.cs
// workloads and oracles are unchanged; automatic routing is measured separately.
internal static class DirectExpressionEntryPoint
{
    [STAThread]
    private static int Main(string[] args)
    {
        AppContext.SetSwitch("Computator.Tsl.DisableDirectExpressions", Array.IndexOf(args, "--roslyn-only") >= 0);
        int scenarioIndex = Array.IndexOf(args, "--scenario");
        if (scenarioIndex >= 0 && scenarioIndex + 1 < args.Length && args[scenarioIndex + 1].StartsWith("auto-", StringComparison.Ordinal))
            return Automatic(args[scenarioIndex + 1]);
        // Invoke before the original worker's first recorded clock. No product
        // translation, compilation or evaluation is performed in this entry path.
        return (int)typeof(Worker).GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new object[] { args });
    }

    private static int Automatic(string scenario)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        try
        {
            if (IntPtr.Size != 8) throw new InvalidOperationException("x64 required.");
            string source;
            CalculationsMode expectedMode;
            switch (scenario)
            {
                case "auto-real": source = "sin(x)+cos(x)+x*x/4"; expectedMode = CalculationsMode.Real; break;
                case "auto-complex": source = "sin(z)+z*z"; expectedMode = CalculationsMode.Complex; break;
                case "auto-surface": source = "sin(x)*cos(y)+x*y"; expectedMode = CalculationsMode.Fxy; break;
                default: throw new ArgumentException("Unsupported automatic scenario.");
            }
            var clock = Stopwatch.StartNew();
            GSLInitializer.Initialize();
            double initialization = clock.Elapsed.TotalMilliseconds;
            // Includes constructors, genuine automatic routing, compilation and
            // first result. No prior parse, oracle call or expression prewarming.
            clock.Restart();
            var determiner = new ModeDeterminer(new TslCompiler());
            var mode = determiner.DetermineMode(source);
            double routing = clock.Elapsed.TotalMilliseconds;
            var evaluator = new ExpressionsEvaluator();
            Function function = evaluator.Evaluate(source, "", mode);
            double real = 0;
            Complex complex = Complex.Zero;
            if (mode == CalculationsMode.Real) { Func<double, double> f = function; real = f(.25); }
            else if (mode == CalculationsMode.Fxy) { Func<double, double, double> f = function; real = f(.25, .5); }
            else { Func<Complex, Complex> f = function; complex = f(new Complex(.25, .5)); }
            double interaction = clock.Elapsed.TotalMilliseconds;
            if (mode != expectedMode) throw new InvalidOperationException("Automatic routing mismatch.");
            if (mode == CalculationsMode.Complex)
            {
                var z = new Complex(.25, .5);
                var expected = Complex.Sin(z) + z*z;
                Check(complex.Real, expected.Real); Check(complex.Imaginary, expected.Imaginary);
            }
            else Check(real, mode == CalculationsMode.Real ? Math.Sin(.25)+Math.Cos(.25)+.25*.25/4 : Math.Sin(.25)*Math.Cos(.5)+.125);
            var record = new Dictionary<string, object>
            {
                { "schema", 1 }, { "scenario", scenario }, { "correctness", true }, { "bitness", 64 },
                { "coreVersion", typeof(ExpressionsEvaluator).Assembly.GetName().Version.ToString() },
                { "runtime", (typeof(object).Assembly.GetName().Name == "mscorlib" ? ".NET Framework CLR " : ".NET ")+Environment.Version },
                { "metrics", new Dictionary<string, double[]>
                    { { "engine.initialize.ms", new[] { initialization } }, { "automatic.mode.ms", new[] { routing } },
                      { "automatic.first.interaction.ms", new[] { interaction } } } },
                { "operations", new Dictionary<string, int>
                    { { "engine.initialize.ms", 1 }, { "automatic.mode.ms", 1 }, { "automatic.first.interaction.ms", 1 } } },
                { "details", new Dictionary<string, object>
                    { { "expression", source }, { "stopwatchFrequency", Stopwatch.Frequency },
                      { "boundary", "Native initialization separate; mode/compiler construction, automatic routing, compile and first result included. UI rendering excluded." } } }
            };
            Console.WriteLine(typeof(Worker).GetMethod("Json", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { record }));
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
    private static void Check(double actual, double expected)
    {
        if (double.IsNaN(actual) || double.IsInfinity(actual) || Math.Abs(actual-expected)>2e-13*Math.Max(1,Math.Abs(expected)))
            throw new InvalidOperationException("Independent automatic interaction oracle failed.");
    }
}
