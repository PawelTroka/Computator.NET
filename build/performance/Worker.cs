// Identical source is compiled for the shipped Framework assemblies and .NET 10.
// No product sources, compiler options, delegates, or caches are substituted.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Computator.NET.Core.NumericalCalculations;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Functions;

internal static class Worker
{
    private const int Repetitions = 5;
    private const int EvaluationCount = 65536;
    private const int GridSide = 64;
    private const int MathCount = 16384;
    private const int ScriptCount = 32;
    private static readonly Dictionary<string, List<double>> Metrics = new Dictionary<string, List<double>>();
    private static readonly Dictionary<string, int> Operations = new Dictionary<string, int>();
    private static readonly Dictionary<string, object> Details = new Dictionary<string, object>();
    private static double sink;

    [STAThread]
    private static int Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        string scenario = Argument(args, "--scenario");
        try
        {
            if (IntPtr.Size != 8) throw new InvalidOperationException("The comparison requires x64 processes.");
            Details["stopwatchFrequency"] = Stopwatch.Frequency;
            Details["warmupMinimumMs"] = 1000;
            Details["compileWarmupMaximumIterations"] = 32;
            Details["measurementRepetitions"] = Repetitions;
            Details["serverGC"] = GCSettings.IsServerGC;
            Details["gcLatencyMode"] = GCSettings.LatencyMode.ToString();
            Details["coreTargetFramework"] = typeof(ExpressionsEvaluator).Assembly
                .GetCustomAttributes(typeof(TargetFrameworkAttribute), false)
                .Cast<TargetFrameworkAttribute>().Single().FrameworkName;
            string coreVersion = typeof(ExpressionsEvaluator).Assembly.GetName().Version.ToString();
            string runtime = RuntimeName();
            if (scenario == "startup")
            {
                var result = DesktopStartup.Measure(Argument(args, "--desktop-exe"));
                Add("desktop.main.ready.ms", result.ElapsedMilliseconds, 1);
                Details["desktop"] = result.Details;
                Details["observerRuntime"] = runtime;
                Details["observerCoreVersion"] = coreVersion;
                coreVersion = result.CoreVersion;
                runtime = result.Runtime;
            }
            else if (scenario == "translation") Translation();
            else
            {
                // Real desktop initialization loads GSL before accepting expressions.
                // Time it separately; never mix native loaders from two versions.
                Add("engine.initialize.ms", Time(GSLInitializer.Initialize), 1);
                if (scenario == "real") Real();
                else if (scenario == "complex") ComplexExpression();
                else if (scenario == "surface") Surface();
                else if (scenario == "script") Script();
                else if (scenario == "math") Mathematics();
                else throw new ArgumentException("Unknown scenario: " + scenario);
            }
            Details["sink"] = sink;
            Details["collections"] = new[] { GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2) };
            using (var process = Process.GetCurrentProcess())
            {
                Details["privateBytesAtEnd"] = process.PrivateMemorySize64;
                Details["workingSetBytesAtEnd"] = process.WorkingSet64;
            }
            Console.WriteLine(Json(new Dictionary<string, object>
            {
                { "schema", 1 }, { "scenario", scenario }, { "runtime", runtime },
                { "bitness", IntPtr.Size * 8 },
                { "coreVersion", coreVersion },
                { "correctness", true }, { "metrics", Metrics }, { "operations", Operations }, { "details", Details }
            }));
            return 0;
        }
        catch (Exception error)
        {
            // A failed oracle/phase invalidates the observation. It is not a sample
            // to discard or replace with a faster workload.
            Console.Error.WriteLine(error);
            Console.WriteLine(Json(new Dictionary<string, object>
            {
                { "schema", 1 }, { "scenario", scenario }, { "correctness", false },
                { "error", error.GetType().FullName }, { "metrics", Metrics }, { "operations", Operations }
            }));
            return 1;
        }
    }

    private static string Argument(string[] args, string key)
    {
        int index = Array.IndexOf(args, key);
        if (index < 0 || index + 1 >= args.Length) throw new ArgumentException("Missing " + key);
        return args[index + 1];
    }

    private static string RuntimeName()
    {
        // Framework's Environment.Version is a CLR version, not its product version.
        return (typeof(object).Assembly.GetName().Name == "mscorlib" ? ".NET Framework CLR " : ".NET ")
            + Environment.Version;
    }

    private static void Real()
    {
        const string expression = "sin(x)+cos(x)+x*x/4";
        Details["expression"] = expression;
        long interactionStart = Stopwatch.GetTimestamp();
        ExpressionsEvaluator evaluator = null;
        Add("evaluator.create.ms", Time(() => evaluator = new ExpressionsEvaluator()), 1);
        Function function = null;
        Add("compile.first.ms", Time(() => function = evaluator.Evaluate(expression, "", CalculationsMode.Real)), 1);
        Func<double, double> compiled = function;
        double first = 0;
        Add("evaluate.first.ms", Time(() => first = compiled(0.25)), 1);
        Add("first.interaction.ms", (Stopwatch.GetTimestamp() - interactionStart) * 1000.0 / Stopwatch.Frequency, 1);
        RecordGeneratedProgram(function);
        Check(first, RealOracle(0.25));
        for (int i = 0; i < 1024; i++) Check(compiled(Input(i)), RealOracle(Input(i)));

        double expected = Sum(EvaluationCount, i => RealOracle(Input(i)));
        Action typed = () => { double sum = 0; for (int i = 0; i < EvaluationCount; i++) sum += compiled(Input(i)); sink = sum; };
        Warm("evaluate", typed);
        Repeat("evaluate.typed.batch.ms", EvaluationCount, typed, () => Check(sink, expected));
        Action publicPath = () => { double sum = 0; for (int i = 0; i < EvaluationCount; i++) sum += function.Evaluate<double>(Input(i)); sink = sum; };
        Warm("public-evaluate", publicPath);
        Repeat("evaluate.public.batch.ms", EvaluationCount, publicPath, () => Check(sink, expected));
        Warm("compile", () => { Func<double, double> f = evaluator.Evaluate(expression, "", CalculationsMode.Real); sink = f(0.25); });
        Repeat("compile.same.warm.ms", 1,
            () => function = evaluator.Evaluate(expression, "", CalculationsMode.Real),
            () => { Func<double, double> f = function; Check(f(0.25), RealOracle(0.25)); });
        for (int i = 0; i < Repetitions; i++)
        {
            string definitions = "public static real shifted(real x) { return x+" + i.ToString(CultureInfo.InvariantCulture) + ".0; }";
            Add("compile.custom.changed.ms", Time(() => function = evaluator.Evaluate("shifted(x)+sin(x)", definitions, CalculationsMode.Real)), 1);
            Func<double, double> f = function;
            Check(f(0.25), 0.25 + i + Math.Sin(0.25));
        }
        // A fixed, correct numerical method with fixed sample count, not adaptive
        // tolerance differences or a raw System.Math microbenchmark.
        double integral = 1 - Math.Cos(1) + Math.Sin(1) + 1.0 / 12;
        Action integrate = () => sink = Sum(32, i => Integral.simpsonMethod(compiled, 0, 1, 1024));
        Warm("integrate", integrate);
        Repeat("integral.simpson.batch.ms", 32, integrate, () => Check(sink, 32 * integral, 1e-10));
    }

    private static double RealOracle(double x) { return Math.Sin(x) + Math.Cos(x) + x * x / 4; }
    private static double Input(int i) { return ((i % 1024) - 512) / 256.0; }

    private static void ComplexExpression()
    {
        const string expression = "sin(z)+z*z";
        Details["expression"] = expression;
        long interactionStart = Stopwatch.GetTimestamp();
        ExpressionsEvaluator evaluator = null;
        Add("evaluator.create.ms", Time(() => evaluator = new ExpressionsEvaluator()), 1);
        Function function = null;
        Add("compile.first.ms", Time(() => function = evaluator.Evaluate(expression, "", CalculationsMode.Complex)), 1);
        Func<Complex, Complex> compiled = function;
        Complex first = Complex.Zero;
        Complex point = new Complex(0.25, 0.5);
        Add("evaluate.first.ms", Time(() => first = compiled(point)), 1);
        Add("first.interaction.ms", (Stopwatch.GetTimestamp() - interactionStart) * 1000.0 / Stopwatch.Frequency, 1);
        RecordGeneratedProgram(function);
        Check(first, Complex.Sin(point) + point * point);
        Func<int, Complex> input = i => new Complex(Input(i), Input(i / 7) / 2);
        for (int i = 0; i < 1024; i++) Check(compiled(input(i)), Complex.Sin(input(i)) + input(i) * input(i));
        double expected = Sum(EvaluationCount, i => { var z = input(i); var value = Complex.Sin(z) + z * z; return value.Real + value.Imaginary; });
        Action evaluate = () =>
        {
            double sum = 0;
            for (int i = 0; i < EvaluationCount; i++)
            {
                var value = compiled(new Complex(Input(i), Input(i / 7) / 2));
                sum += value.Real + value.Imaginary;
            }
            sink = sum;
        };
        Warm("evaluate", evaluate);
        Repeat("evaluate.typed.batch.ms", EvaluationCount, evaluate, () => Check(sink, expected));
        Warm("compile", () => { Func<Complex, Complex> f = evaluator.Evaluate(expression, "", CalculationsMode.Complex); sink = f(point).Real; });
        Repeat("compile.same.warm.ms", 1, () => function = evaluator.Evaluate(expression, "", CalculationsMode.Complex),
            () => { Func<Complex, Complex> f = function; Check(f(point), Complex.Sin(point) + point * point); });
    }

    private static void Surface()
    {
        const string expression = "sin(x)*cos(y)+x*y";
        Details["expression"] = expression;
        Details["gridSide"] = GridSide;
        Details["renderingIncluded"] = false;
        long interactionStart = Stopwatch.GetTimestamp();
        ExpressionsEvaluator evaluator = null;
        Add("evaluator.create.ms", Time(() => evaluator = new ExpressionsEvaluator()), 1);
        Function function = null;
        Add("compile.first.ms", Time(() => function = evaluator.Evaluate(expression, "", CalculationsMode.Fxy)), 1);
        Func<double, double, double> compiled = function;
        double first = 0;
        Add("evaluate.first.ms", Time(() => first = compiled(0.25, 0.5)), 1);
        Add("first.interaction.ms", (Stopwatch.GetTimestamp() - interactionStart) * 1000.0 / Stopwatch.Frequency, 1);
        RecordGeneratedProgram(function);
        Check(first, Math.Sin(0.25) * Math.Cos(0.5) + 0.125);
        Func<int, double> x = i => (i % GridSide - GridSide / 2) / 16.0;
        Func<int, double> y = i => (i / GridSide - GridSide / 2) / 16.0;
        for (int i = 0; i < GridSide * GridSide; i++) Check(compiled(x(i), y(i)), Math.Sin(x(i)) * Math.Cos(y(i)) + x(i) * y(i));
        double expected = Sum(GridSide * GridSide, i => Math.Sin(x(i)) * Math.Cos(y(i)) + x(i) * y(i));
        Action grid = () =>
        {
            double sum = 0;
            for (int i = 0; i < GridSide * GridSide; i++)
                sum += compiled((i % GridSide - GridSide / 2) / 16.0, (i / GridSide - GridSide / 2) / 16.0);
            sink = sum;
        };
        Warm("grid", grid);
        Repeat("chart.grid.sample.ms", GridSide * GridSide, grid, () => Check(sink, expected));
        Warm("compile", () => { Func<double, double, double> f = evaluator.Evaluate(expression, "", CalculationsMode.Fxy); sink = f(0.25, 0.5); });
        Repeat("compile.same.warm.ms", 1, () => function = evaluator.Evaluate(expression, "", CalculationsMode.Fxy),
            () => { Func<double, double, double> f = function; Check(f(0.25, 0.5), Math.Sin(0.25) * Math.Cos(0.5) + 0.125); });
    }

    private static void Script()
    {
        // write (console), not show (a modal dialog). Invariant string formatting
        // keeps the checksum independent from display-notation improvements.
        const string script = "real sum=0; for(integer k=1;k<=1000;k++){sum+=k*k;} write(sum.ToString(\"R\", System.Globalization.CultureInfo.InvariantCulture));";
        Details["script"] = script;
        long interactionStart = Stopwatch.GetTimestamp();
        ScriptEvaluator evaluator = null;
        Add("evaluator.create.ms", Time(() => evaluator = new ScriptEvaluator()), 1);
        ScriptFunction function = null;
        Add("compile.first.ms", Time(() => function = evaluator.Evaluate(script, "")), 1);
        string output = "";
        int calls = 0;
        Action<string> capture = value => { output = value; calls++; };
        Add("script.first.ms", Time(() => function.Evaluate(capture)), 1);
        Add("first.interaction.ms", (Stopwatch.GetTimestamp() - interactionStart) * 1000.0 / Stopwatch.Frequency, 1);
        RecordGeneratedProgram(function);
        CheckScript(output, calls);
        var outputs = new string[ScriptCount];
        var callbacks = new int[ScriptCount];
        int current = 0;
        Action<string> captureBatch = value => { outputs[current] = value; callbacks[current]++; };
        Action run = () =>
        {
            for (current = 0; current < ScriptCount; current++)
            {
                outputs[current] = null; callbacks[current] = 0;
                function.Evaluate(captureBatch);
            }
        };
        Action validate = () => { for (int i = 0; i < ScriptCount; i++) CheckScript(outputs[i], callbacks[i]); sink = ScriptCount; };
        Warm("script", run);
        validate();
        Repeat("script.repeated.batch.ms", ScriptCount, run, validate);
        Warm("compile", () => { var f = evaluator.Evaluate(script, ""); output = ""; calls = 0; f.Evaluate(capture); CheckScript(output, calls); });
        Repeat("compile.same.warm.ms", 1, () => function = evaluator.Evaluate(script, ""),
            () => { output = ""; calls = 0; function.Evaluate(capture); CheckScript(output, calls); });
    }

    private static void CheckScript(string output, int calls)
    {
        if (calls != 1) throw new InvalidOperationException("Expected exactly one script output.");
        Check(double.Parse(output, CultureInfo.InvariantCulture), 333833500);
    }

    private static void Mathematics()
    {
        // Stable inputs from the same function families as the existing GUI
        // benchmark; individual reference values prevent compensating errors.
        double[] gamma = { 1, 1, 2, 6, 24, 120 };
        for (int i = 0; i < gamma.Length; i++) Check(SpecialFunctions.gamma(i + 1.0), gamma[i]);
        Check(SpecialFunctions.BesselJ0(1), 0.76519768655796655145);
        Check(SpecialFunctions.BesselJ0(2), 0.22389077914123566805);
        Check(SpecialFunctions.BesselJ0(3), -0.26005195490193343762);
        Check(SpecialFunctions.Ai(0), 0.35502805388781723926);
        double[] bessel = { 0.76519768655796655145, 0.22389077914123566805, -0.26005195490193343762 };
        double expected = Sum(MathCount, i => gamma[i % 6] + bessel[i % 3] + 0.35502805388781723926);
        Action special = () => sink = Sum(MathCount, i => SpecialFunctions.gamma(i % 6 + 1.0)
            + SpecialFunctions.BesselJ0(i % 3 + 1.0) + SpecialFunctions.Ai(0));
        Warm("special", special);
        Repeat("math.special.batch.ms", MathCount, special, () => Check(sink, expected));
        double nativeExpected = Sum(MathCount, i => 1 - Math.Exp(-(0.25 + (i % 256) / 64.0)));
        for (int i = 0; i < 256; i++)
        {
            double value = 0.25 + i / 64.0;
            Check(SpecialFunctions.gammaP(1, value), 1 - Math.Exp(-value));
        }
        Action native = () =>
        {
            double sum = 0;
            for (int i = 0; i < MathCount; i++) sum += SpecialFunctions.gammaP(1, 0.25 + (i % 256) / 64.0);
            sink = sum;
        };
        Warm("native-gsl", native);
        Repeat("math.native.gsl.batch.ms", MathCount, native, () => Check(sink, nativeExpected));
        Func<int, double> x = i => 0.5 + (i % 1024) / 256.0;
        expected = Sum(MathCount, i => Math.Sin(x(i)) + Math.Cos(x(i)) + Math.Exp(x(i)) + Math.Log(x(i)));
        Action elementary = () => sink = Sum(MathCount, i => ElementaryFunctions.sin(x(i)) + ElementaryFunctions.cos(x(i))
            + ElementaryFunctions.exp(x(i)) + ElementaryFunctions.ln(x(i)));
        Warm("elementary", elementary);
        Repeat("math.elementary.batch.ms", MathCount, elementary, () => Check(sink, expected));
    }

    private static void Translation()
    {
        var compiler = new TslCompiler();
        const string expression = "sin(x)+x²/4";
        // Historical/current translators may generate differently formatted C#.
        // Validate the transformed expression through the product evaluator and
        // a numeric oracle, never require byte equality with the old compiler.
        string translated = null;
        Add("translation.first.ms", Time(() => translated = compiler.TransformToCSharp(expression)), 1);
        if (string.IsNullOrWhiteSpace(translated)) throw new InvalidOperationException("Empty translation.");
        Func<double, double> f = new ExpressionsEvaluator().Evaluate(expression, "", CalculationsMode.Real);
        Check(f(0.5), Math.Sin(0.5) + 0.0625);
        Details["expression"] = expression;
        Details["translated"] = translated;
        var translations = new string[256];
        Action transform = () =>
        {
            for (int i = 0; i < 256; i++) translations[i] = compiler.TransformToCSharp(expression);
        };
        Warm("translation", transform);
        Repeat("translation.repeated.batch.ms", 256, transform, () =>
        {
            if (translations.Any(value => value != translated)) throw new InvalidOperationException("Translation changed within a process.");
            sink = translated.Length;
        });
        Action empty = () =>
        {
            int length = 0;
            for (int i = 0; i < 256; i++) length += compiler.TransformToCSharp("").Length;
            sink = length;
        };
        Warm("translation-empty", empty);
        Repeat("translation.empty.batch.ms", 256, empty, () => Check(sink, 0));
    }

    private static void Warm(string name, Action action)
    {
        var observations = new List<double>();
        var elapsed = Stopwatch.StartNew();
        int count = 0;
        do
        {
            double observation = Time(action);
            // Retain a bounded trace including early and recent observations.
            if (observations.Count < 64) observations.Add(observation);
            else observations[count % 32 + 32] = observation;
            count++;
        }
        while ((elapsed.ElapsedMilliseconds < 1000 || count < 3) && (name != "compile" || count < 32));
        Details["warmup." + name + ".ms"] = observations;
        Details["warmup." + name + ".iterations"] = count;
        Details["warmup." + name + ".elapsedMs"] = elapsed.Elapsed.TotalMilliseconds;
        Details["warmup." + name + ".budgetReached"] = elapsed.ElapsedMilliseconds >= 1000;
    }

    private static void Repeat(string name, int operations, Action action, Action validate)
    {
        for (int i = 0; i < Repetitions; i++)
        {
            Add(name, Time(action), operations);
            validate();
        }
    }

    private static double Time(Action action)
    {
        long start = Stopwatch.GetTimestamp();
        action();
        return (Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency;
    }

    private static void Add(string name, double milliseconds, int count)
    {
        if (!Metrics.ContainsKey(name)) { Metrics[name] = new List<double>(); Operations[name] = count; }
        if (Operations[name] != count || milliseconds <= 0 || double.IsNaN(milliseconds) || double.IsInfinity(milliseconds))
            throw new InvalidOperationException("Invalid measurement " + name);
        Metrics[name].Add(milliseconds);
    }

    private static double Sum(int count, Func<int, double> item)
    {
        double result = 0;
        for (int i = 0; i < count; i++) result += item(i);
        return result;
    }

    private static void Check(double actual, double expected, double tolerance = 2e-11)
    {
        if (double.IsNaN(actual) || double.IsInfinity(actual) || Math.Abs(actual - expected) > tolerance * Math.Max(1, Math.Abs(expected)))
            throw new InvalidOperationException("Correctness oracle failed: actual=" + actual.ToString("R", CultureInfo.InvariantCulture)
                + ", expected=" + expected.ToString("R", CultureInfo.InvariantCulture));
    }

    private static void Check(Complex actual, Complex expected) { Check(actual.Real, expected.Real); Check(actual.Imaginary, expected.Imaginary); }

    private static void RecordGeneratedProgram(BaseFunction function)
    {
        var compiled = (Delegate)typeof(BaseFunction).GetField("_function", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(function);
        var flags = compiled.Method.Module.Assembly.GetCustomAttributes(typeof(DebuggableAttribute), false).Cast<DebuggableAttribute>().SingleOrDefault();
        Details["generatedProgram"] = new Dictionary<string, object>
        {
            { "debuggableAttributePresent", flags != null },
            { "jitOptimizerDisabled", flags == null ? (object)null : flags.IsJITOptimizerDisabled },
            { "debuggingFlags", flags == null ? null : flags.DebuggingFlags.ToString() }
        };
    }

    // A small JSON writer keeps the identical driver independent of either
    // product's serializer/dependency versions. Only finite numbers are emitted.
    private static string Json(object value)
    {
        if (value == null) return "null";
        if (value is string)
        {
            var result = new StringBuilder("\"");
            foreach (char character in (string)value)
            {
                if (character == '"' || character == '\\') result.Append('\\').Append(character);
                else if (character < 32) result.Append("\\u").Append(((int)character).ToString("x4"));
                else result.Append(character);
            }
            return result.Append('"').ToString();
        }
        if (value is bool) return (bool)value ? "true" : "false";
        var dictionary = value as System.Collections.IDictionary;
        if (dictionary != null)
        {
            var items = new List<string>();
            foreach (System.Collections.DictionaryEntry item in dictionary) items.Add(Json(item.Key.ToString()) + ":" + Json(item.Value));
            return "{" + string.Join(",", items) + "}";
        }
        var sequence = value as System.Collections.IEnumerable;
        if (sequence != null) return "[" + string.Join(",", sequence.Cast<object>().Select(Json)) + "]";
        return Convert.ToString(value, CultureInfo.InvariantCulture);
    }
}
