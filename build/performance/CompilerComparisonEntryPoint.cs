using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;

// Current-before/current-after only. Worker.cs and its historical observations
// are unchanged. Prefixes select the same workload with the documented opt-out.
internal static class CompilerComparisonEntryPoint
{
    [STAThread]
    private static int Main(string[] args)
    {
        int index = Array.IndexOf(args, "--scenario");
        if (index < 0 || index + 1 == args.Length) throw new ArgumentException("--scenario is required.");
        string scenario = args[index + 1];
        if (scenario.StartsWith("memory-", StringComparison.Ordinal)) return Memory(scenario);
        bool roslyn = scenario.StartsWith("roslyn-", StringComparison.Ordinal);
        var forwarded = new List<string>(args);
        if (roslyn) { forwarded[index + 1] = scenario.Substring(7); forwarded.Add("--roslyn-only"); }
        var output = Console.Out;
        using (var captured = new StringWriter(CultureInfo.InvariantCulture))
        {
            int result;
            Console.SetOut(captured);
            try
            {
                result = (int)typeof(DirectExpressionEntryPoint).GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static)
                    .Invoke(null, new object[] { forwarded.ToArray() });
            }
            finally { Console.SetOut(output); }
            var record = JsonNode.Parse(captured.ToString()).AsObject();
            record["scenario"] = scenario;
            if (record["details"] is JsonObject details) details["directCompilerDisabled"] = roslyn;
            Console.WriteLine(record.ToJsonString());
            return result;
        }
    }

    // Fixed-count memory diagnostic, separate from time-budgeted warm workloads.
    // Every edit is distinct; outputs are checked and discarded. Forced GC occurs
    // only before/after this diagnostic, never inside ordinary latency scenarios.
    private static int Memory(string scenario)
    {
        try
        {
            bool script = scenario == "memory-script";
            if (!script && scenario != "memory-roslyn-real") throw new ArgumentException("Unknown memory scenario.");
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            AppContext.SetSwitch("Computator.Tsl.DisableDirectExpressions", true);
            if (IntPtr.Size != 8) throw new InvalidOperationException("x64 required.");
            GSLInitializer.Initialize();
            Collect();
            long managedBefore = GC.GetTotalMemory(false), allocatedBefore = GC.GetTotalAllocatedBytes(true);
            long privateBefore, workingBefore;
            using (var process = Process.GetCurrentProcess())
            { privateBefore = process.PrivateMemorySize64; workingBefore = process.WorkingSet64; }
            var first = new double[1];
            var clock = Stopwatch.StartNew();
            CompileEdits(script, first);
            double elapsed = clock.Elapsed.TotalMilliseconds;
            long allocated = GC.GetTotalAllocatedBytes(true) - allocatedBefore;
            long managedBeforeCollection = GC.GetTotalMemory(false);
            long privateBeforeCollection, workingBeforeCollection, peakWorkingSet;
            using (var process = Process.GetCurrentProcess())
            {
                privateBeforeCollection = process.PrivateMemorySize64;
                workingBeforeCollection = process.WorkingSet64;
                peakWorkingSet = process.PeakWorkingSet64;
            }
            Collect();
            long managedAfter = GC.GetTotalMemory(false);
            long privateAfter, workingAfter;
            using (var process = Process.GetCurrentProcess())
            { privateAfter = process.PrivateMemorySize64; workingAfter = process.WorkingSet64; }
            var record = new Dictionary<string, object>
            {
                { "schema", 1 }, { "scenario", scenario }, { "correctness", true }, { "bitness", 64 },
                { "runtime", ".NET " + Environment.Version },
                { "coreVersion", typeof(NativeCompiler).Assembly.GetName().Version.ToString() },
                { "metrics", new Dictionary<string, double[]> { { "edits.first.compile.ms", first }, { "edits.compile.and.check.batch.ms", new[] { elapsed } } } },
                { "operations", new Dictionary<string, int> { { "edits.first.compile.ms", 1 }, { "edits.compile.and.check.batch.ms", 33 } } },
                { "details", new Dictionary<string, object>
                    {
                        { "stopwatchFrequency", Stopwatch.Frequency }, { "compilations", 33 }, { "directCompilerDisabled", true },
                        { "memoryScope", "Fixed 33 distinct edits, including evaluator creation, first compiler load and output oracles. Byte counters are diagnostic, not latency metrics or leak assertions." },
                        { "allocatedManagedBytes", allocated }, { "managedBytesBefore", managedBefore },
                        { "managedBytesBeforeCollection", managedBeforeCollection }, { "managedBytesAfterCollection", managedAfter },
                        { "privateBytesBefore", privateBefore }, { "privateBytesBeforeCollection", privateBeforeCollection }, { "privateBytesAfterCollection", privateAfter },
                        { "workingSetBytesBefore", workingBefore }, { "workingSetBytesBeforeCollection", workingBeforeCollection }, { "workingSetBytesAfterCollection", workingAfter },
                        { "peakWorkingSetBytes", peakWorkingSet }
                    } }
            };
            Console.WriteLine(typeof(Worker).GetMethod("Json", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { record }));
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CompileEdits(bool script, double[] first)
    {
        var evaluator = script ? new ScriptEvaluator() : new ExpressionsEvaluator();
        for (int index = 0; index < 33; index++)
        {
            var clock = Stopwatch.StartNew();
            if (script)
            {
                var function = ((ScriptEvaluator)evaluator).Evaluate("write(shifted(2.0).ToString(\"R\",System.Globalization.CultureInfo.InvariantCulture));",
                    "public static real shifted(real x) { return x+" + index + ".0; }");
                if (index == 0) first[0] = clock.Elapsed.TotalMilliseconds;
                int calls = 0;
                function.Evaluate(value => { calls++; if (value != (index + 2).ToString(CultureInfo.InvariantCulture)) throw new InvalidOperationException("Stale script definition."); });
                if (calls != 1) throw new InvalidOperationException("Wrong script output cardinality.");
            }
            else
            {
                Func<double, double> function = evaluator.Evaluate("shifted(x)+sin(x)",
                    "public static real shifted(real x) { return x+" + index + ".0; }", CalculationsMode.Real);
                if (index == 0) first[0] = clock.Elapsed.TotalMilliseconds;
                double expected = 0.25 + index + Math.Sin(0.25), actual = function(0.25);
                if (double.IsNaN(actual) || double.IsInfinity(actual) || Math.Abs(actual - expected) > 2e-13 * Math.Max(1, Math.Abs(expected)))
                    throw new InvalidOperationException("Stale or incorrect edited expression.");
            }
        }
    }
    private static void Collect() { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
}
