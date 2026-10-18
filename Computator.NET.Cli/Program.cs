using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.Json;
using Computator.NET.Engine;

const string help = """
Computator portable scientific engine. Run only code you trust.
calculate --expression <TSL> [--x <number>] [--y <number>] [--functions <file.tslf>]
complex --expression <TSL> [--real <number>] [--imaginary <number>] [--functions <file.tslf>]
script --file <file.tsl> [--functions <file.tslf>]
integrate --expression <TSL> --a <number> --b <number> [--steps <even integer>] [--functions <file.tslf>]
differentiate --expression <TSL> --x <number> [--order <integer>] [--functions <file.tslf>]
root --expression <TSL> --a <number> --b <number> [--tolerance <number>] [--iterations <integer>] [--functions <file.tslf>]

Numbers use an invariant decimal point and must be finite. Integration uses the shared Simpson method
(default 10,000 steps, even 2..1,000,000). Derivatives use the shared derivative method (order 1..10).
Roots use the shared Brent method (default tolerance 1e-8, 10,000 iterations; bracket a root with a < b).
Success writes one JSON object with a string result to stdout. Errors go to stderr: exit 2 for
compilation diagnostics, exit 1 for invalid input or execution failure. --help exits successfully.
Use write(...) for script result text. Ordinary Console output is ignored through shutdown so
messages from scripts or custom functions cannot mix with the CLI's JSON responses.
UTF-8 documents are limited to 256 KiB and 65,536 characters; an optional UTF-8 BOM is accepted.
No native GSL, charts or desktop interaction is available here. Work budgets are not a sandbox or timeout.
""";
var protocolOutput = Console.Out;
var protocolError = Console.Error;
try
{
    Console.OutputEncoding = new UTF8Encoding(false);
    // Preserve the UTF-8 protocol writers privately. The ordinary Console sinks
    // remain disconnected after execution, including while ProcessExit runs.
    protocolOutput = Console.Out;
    protocolError = Console.Error;
    if (args.Length == 0 || (args.Length == 1 && args[0] is "--help" or "-h"))
    {
        protocolOutput.WriteLine(help);
        return 0;
    }
    string[] supported = args[0] switch
    {
        "calculate" => ["--expression", "--x", "--y", "--functions"],
        "complex" => ["--expression", "--real", "--imaginary", "--functions"],
        "script" => ["--file", "--functions"],
        "integrate" => ["--expression", "--a", "--b", "--steps", "--functions"],
        "differentiate" => ["--expression", "--x", "--order", "--functions"],
        "root" => ["--expression", "--a", "--b", "--tolerance", "--iterations", "--functions"],
        _ => throw new ArgumentException("Unknown command: " + args[0])
    };
    if (args.Length == 2 && args[1] is "--help" or "-h") { protocolOutput.WriteLine(help); return 0; }
    var allowed = new HashSet<string>(supported, StringComparer.Ordinal);
    var options = new Dictionary<string, string>(StringComparer.Ordinal);
    for (var index = 1; index < args.Length; index += 2)
    {
        if (index + 1 >= args.Length || !allowed.Contains(args[index]) || !options.TryAdd(args[index], args[index + 1]))
            throw new ArgumentException("Invalid, duplicated or incomplete option for " + args[0] + ": " + args[index]);
    }
    string Required(string key) => options.TryGetValue(key, out var value) ? value : throw new ArgumentException("Missing " + key);
    double Number(string key, double fallback = 0, bool required = false)
    {
        if (!options.TryGetValue(key, out var text)) return required ? throw new ArgumentException("Missing " + key) : fallback;
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || !double.IsFinite(value))
            throw new ArgumentException(key + " must be a finite number using a decimal point.");
        return value;
    }
    uint Integer(string key, uint fallback)
    {
        if (!options.TryGetValue(key, out var text)) return fallback;
        if (!uint.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var value))
            throw new ArgumentException(key + " must be a non-negative integer.");
        return value;
    }
    var functions = options.TryGetValue("--functions", out var name) ? ReadDocument(name) : "";
    var engine = new PortableEngine();
    string Format(double value) => value.ToString("R", CultureInfo.InvariantCulture);
    var result = ExecuteWithIsolatedConsole(() => args[0] switch
    {
        "calculate" => Format(engine.EvaluateReal(Required("--expression"), Number("--x"), Number("--y"), functions)),
        "complex" => engine.EvaluateComplex(Required("--expression"), new Complex(Number("--real"), Number("--imaginary")), functions).ToString("R", CultureInfo.InvariantCulture),
        "script" => engine.ExecuteScript(ReadDocument(Required("--file")), functions),
        "integrate" => Format(engine.Integrate(Required("--expression"), Number("--a", required: true), Number("--b", required: true), checked((int)Integer("--steps", 10000)), functions)),
        "differentiate" => Format(engine.Differentiate(Required("--expression"), Number("--x", required: true), Integer("--order", 1), functions)),
        "root" => Format(engine.FindRoot(Required("--expression"), Number("--a", required: true), Number("--b", required: true), Number("--tolerance", 1e-8), Integer("--iterations", 10000), functions)),
        _ => throw new ArgumentException("Unknown command: " + args[0])
    });
    protocolOutput.WriteLine(JsonSerializer.Serialize(new { result }));
    return 0;
}
catch (EngineCompilationException error)
{
    protocolError.WriteLine(JsonSerializer.Serialize(new { error = "compilation", diagnostics = error.Diagnostics }));
    return 2;
}
catch (Exception error)
{
    protocolError.WriteLine(JsonSerializer.Serialize(new { error = error.GetType().Name, message = error.Message }));
    return 1;
}

static string ExecuteWithIsolatedConsole(Func<string> execute)
{
    // TSL write(...) still returns its bounded script output.
    try
    {
        Console.SetOut(TextWriter.Null);
        Console.SetError(TextWriter.Null);
        return execute();
    }
    finally
    {
        // Trusted code may replace either writer. Reset both sinks before JSON
        // emission and keep them suppressed for ordinary shutdown/background logs.
        Console.SetOut(TextWriter.Null);
        Console.SetError(TextWriter.Null);
    }
}

static string ReadDocument(string path)
{
    const int maximumBytes = 262144;
    // Bound the actual read, not just a racy length check before ReadAllText.
    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    var bytes = new byte[maximumBytes + 1];
    var count = 0;
    while (count < bytes.Length)
    {
        var read = stream.Read(bytes, count, bytes.Length - count);
        if (read == 0) break;
        count += read;
    }
    if (count > maximumBytes) throw new ArgumentException("Document exceeds 256 KiB.");
    var offset = count >= 3 && bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf ? 3 : 0;
    var text = new UTF8Encoding(false, true).GetString(bytes, offset, count - offset);
    if (text.Length > PortableEngine.MaximumSourceCharacters) throw new ArgumentException("Document exceeds the source character limit.");
    return text;
}
