using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using Computator.NET.Core.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal static class Probe
{
    private const int Repetitions = 7;
    private const int WarmupMilliseconds = 1500;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly (string Fixture, string Metric, int Count)[] Phases =
    {
        ("plain", "translation.expression.batch.ms", 512),
        ("empty", "translation.empty.batch.ms", 1024),
        ("comment_power", "translation.comment.batch.ms", 256),
        ("symbolic", "translation.symbolic.batch.ms", 512),
        ("literal_script", "translation.literal.batch.ms", 128),
        ("large_script", "translation.large.script.batch.ms", 8)
    };

    private sealed class Fixture
    {
        public string id { get; set; } = "";
        public string? source { get; set; }
        public string? custom { get; set; } = "";
        public bool defer { get; set; }
        public int api { get; set; }
        public string? explicitExpected { get; set; }
    }

    private sealed class Observation
    {
        public string? output { get; set; }
        public string? exceptionType { get; set; }
        public string? argumentName { get; set; }
    }

    private static string GetArgument(string[] arguments, string name)
    {
        int index = Array.IndexOf(arguments, name);
        if (index < 0 || index + 1 >= arguments.Length) throw new ArgumentException("Missing " + name);
        return arguments[index + 1];
    }

    private static string Transform(TslCompiler compiler, Fixture fixture) => fixture.api switch
    {
        1 => compiler.TransformToCSharp(fixture.source!),
        2 => compiler.TransformToCSharp(fixture.source!, fixture.custom!),
        3 => compiler.TransformToCSharp(fixture.source!, fixture.custom!, fixture.defer),
        _ => throw new ArgumentException("Unknown overload")
    };

    private static Observation Observe(TslCompiler compiler, Fixture fixture)
    {
        try { return new Observation { output = Transform(compiler, fixture) }; }
        catch (Exception error)
        {
            return new Observation { exceptionType = error.GetType().FullName, argumentName = (error as ArgumentException)?.ParamName };
        }
    }

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static int Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        try
        {
            string fixturePath = GetArgument(args, "--fixtures");
            var fixtures = JsonSerializer.Deserialize<List<Fixture>>(File.ReadAllText(fixturePath))!;
            var compiler = new TslCompiler();
            var observed = fixtures.ToDictionary(fixture => fixture.id, fixture => Observe(compiler, fixture));
            foreach (var fixture in fixtures)
            {
                if (fixture.explicitExpected is not null && observed[fixture.id].output != fixture.explicitExpected)
                    throw new InvalidOperationException("Independent expected output failed for " + fixture.id + ": " + observed[fixture.id].output);
                if (fixture.source is null && observed[fixture.id].exceptionType != typeof(ArgumentNullException).FullName)
                    throw new InvalidOperationException("Null input must preserve ArgumentNullException.");
            }

            if (args.Contains("--export-oracle"))
            {
                string oraclePath = GetArgument(args, "--export-oracle");
                File.WriteAllText(oraclePath, JsonSerializer.Serialize(observed, JsonOptions) + "\n");
                Console.WriteLine(JsonSerializer.Serialize(new { fixtures = fixtures.Count, oracle = oraclePath, sha256 = Hash(oraclePath) }));
                return 0;
            }

            string scenario = GetArgument(args, "--scenario");
            if (scenario != "translation") throw new ArgumentException("Unsupported diagnostic scenario");
            string expectedPath = GetArgument(args, "--oracle");
            var expected = JsonSerializer.Deserialize<Dictionary<string, Observation>>(File.ReadAllText(expectedPath))!;
            if (!expected.Keys.Order().SequenceEqual(observed.Keys.Order())) throw new InvalidOperationException("Oracle fixture completeness differs.");
            foreach (var (id, actual) in observed)
            {
                var oracle = expected[id];
                if (actual.output != oracle.output || actual.exceptionType != oracle.exceptionType || actual.argumentName != oracle.argumentName)
                    throw new InvalidOperationException("Semantic output or exception changed: " + id);
            }
            VerifyNumericExpression(observed["plain"].output!);

            var metrics = new Dictionary<string, List<double>>();
            var operations = new Dictionary<string, int>();
            var details = new Dictionary<string, object?>
            {
                ["scope"] = "isolated Linux .NET 10 translation diagnostic; no historical/desktop/startup measurement",
                ["stopwatchFrequency"] = Stopwatch.Frequency,
                ["warmupMinimumMs"] = WarmupMilliseconds,
                ["measurementRepetitions"] = Repetitions,
                ["semanticFixtureCount"] = fixtures.Count,
                ["fixtureSha256"] = Hash(fixturePath),
                ["oracleSha256"] = Hash(expectedPath),
                ["numericOracleSamples"] = 1024,
                ["serverGC"] = GCSettings.IsServerGC,
                ["coreSha256"] = Hash(typeof(TslCompiler).Assembly.Location),
                ["workerSha256"] = Hash(typeof(Probe).Assembly.Location),
                ["roslynVersion"] = typeof(CSharpCompilation).Assembly.GetName().Version!.ToString(),
                ["coreLibrarySha256"] = Hash(typeof(object).Assembly.Location),
                ["semanticChecksPrecedeWarmup"] = true,
                ["allocatedBytesPerObservation"] = new Dictionary<string, List<long>>()
            };
            var allocations = (Dictionary<string, List<long>>)details["allocatedBytesPerObservation"]!;
            foreach (var phase in Phases)
            {
                var fixture = fixtures.Single(item => item.id == phase.Fixture);
                string oracle = expected[phase.Fixture].output ?? throw new InvalidOperationException("Timed fixture must have output.");
                var outputs = new string[phase.Count];
                void Batch()
                {
                    for (int index = 0; index < outputs.Length; index++) outputs[index] = Transform(compiler, fixture);
                }
                void Validate()
                {
                    foreach (string output in outputs)
                        if (output != oracle) throw new InvalidOperationException("Timed output changed for " + phase.Fixture);
                }
                var warmup = Stopwatch.StartNew();
                int warmupCount = 0;
                var warmupTrace = new List<double>();
                do
                {
                    long start = Stopwatch.GetTimestamp();
                    Batch();
                    double milliseconds = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
                    Validate();
                    if (warmupTrace.Count < 32) warmupTrace.Add(milliseconds);
                    else warmupTrace[warmupCount % 16 + 16] = milliseconds;
                    warmupCount++;
                } while (warmup.ElapsedMilliseconds < WarmupMilliseconds || warmupCount < 3);
                details["warmup." + phase.Fixture + ".iterations"] = warmupCount;
                details["warmup." + phase.Fixture + ".elapsedMs"] = warmup.Elapsed.TotalMilliseconds;
                details["warmup." + phase.Fixture + ".ms"] = warmupTrace;
                details["warmup." + phase.Fixture + ".budgetReached"] = true;
                metrics[phase.Metric] = new List<double>();
                operations[phase.Metric] = phase.Count;
                allocations[phase.Metric] = new List<long>();
                for (int repetition = 0; repetition < Repetitions; repetition++)
                {
                    long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
                    long start = Stopwatch.GetTimestamp();
                    Batch();
                    double milliseconds = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
                    long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
                    Validate();
                    if (!(milliseconds > 0) || !double.IsFinite(milliseconds)) throw new InvalidOperationException("Invalid timing");
                    metrics[phase.Metric].Add(milliseconds);
                    allocations[phase.Metric].Add(allocatedAfter - allocatedBefore);
                }
            }
            details["collections"] = new[] { GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2) };
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                schema = 1, scenario,
                runtime = ".NET " + Environment.Version,
                bitness = IntPtr.Size * 8,
                coreVersion = typeof(TslCompiler).Assembly.GetName().Version!.ToString(),
                correctness = true, metrics, operations, details
            }));
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            Console.WriteLine(JsonSerializer.Serialize(new { schema = 1, correctness = false, error = error.Message }));
            return 1;
        }
    }

    private static void VerifyNumericExpression(string translated)
    {
        string program = "using System; public static class NumericProof { public static double Eval(double x) => "
            + translated + "; private static double sin(double x) => Math.Sin(x); private static double pow(double x,double y)=>Math.Pow(x,y); }";
        var tree = CSharpSyntaxTree.ParseText(program, new CSharpParseOptions(LanguageVersion.CSharp14));
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
        };
        var compilation = CSharpCompilation.Create("TranslationNumericProof", new[] { tree }, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        using var stream = new MemoryStream();
        var emitted = compilation.Emit(stream);
        if (!emitted.Success) throw new InvalidOperationException(string.Join("\n", emitted.Diagnostics));
        stream.Position = 0;
        var loaded = AssemblyLoadContext.Default.LoadFromStream(stream);
        var function = loaded.GetType("NumericProof")!.GetMethod("Eval")!.CreateDelegate<Func<double, double>>();
        for (int index = 0; index < 1024; index++)
        {
            double x = (index - 512) / 256.0;
            double expected = Math.Sin(x) + x * x / 4;
            double actual = function(x);
            if (!double.IsFinite(actual) || Math.Abs(actual - expected) > 2e-13 * Math.Max(1, Math.Abs(expected)))
                throw new InvalidOperationException("Independent numerical expression oracle failed");
        }
    }
}
