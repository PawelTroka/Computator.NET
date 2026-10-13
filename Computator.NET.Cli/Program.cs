using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.Json;
using Computator.NET.Engine;

try
{
    if (args.Length == 0 || args[0] is "--help" or "-h")
    {
        Console.WriteLine("Computator portable engine (managed first slice). Trusted code only.\ncalculate --expression <TSL> [--x <number>] [--y <number>] [--functions <file.tslf>]\ncomplex --expression <TSL> [--real <number>] [--imaginary <number>] [--functions <file.tslf>]\nscript --file <file.tsl> [--functions <file.tslf>]\nNo WinForms, native GSL or desktop interaction is available in this CLI yet.");
        return 0;
    }
    var allowed = new HashSet<string>(StringComparer.Ordinal) { "--expression", "--x", "--y", "--real", "--imaginary", "--functions", "--file" };
    var options = new Dictionary<string, string>(StringComparer.Ordinal);
    for (var index = 1; index < args.Length; index += 2)
    {
        if (index + 1 >= args.Length || !allowed.Contains(args[index]) || !options.TryAdd(args[index], args[index + 1]))
            throw new ArgumentException("Invalid, duplicated or incomplete option: " + args[index]);
    }
    string Required(string key) => options.TryGetValue(key, out var value) ? value : throw new ArgumentException("Missing " + key);
    double Number(string key) => options.TryGetValue(key, out var value) ? double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture) : 0;
    string ReadDocument(string path)
    {
        var file = new FileInfo(path);
        if (!file.Exists || file.Length > 262144) throw new ArgumentException("Document is missing or exceeds 256 KiB.");
        var text = File.ReadAllText(path, new UTF8Encoding(false, true));
        if (text.Length > PortableEngine.MaximumSourceCharacters) throw new ArgumentException("Document exceeds the source character limit.");
        return text;
    }
    var functions = options.TryGetValue("--functions", out var name) ? ReadDocument(name) : "";
    var engine = new PortableEngine();
    var result = args[0] switch
    {
        "calculate" => engine.EvaluateReal(Required("--expression"), Number("--x"), Number("--y"), functions).ToString("R", CultureInfo.InvariantCulture),
        "complex" => engine.EvaluateComplex(Required("--expression"), new Complex(Number("--real"), Number("--imaginary")), functions).ToString("R", CultureInfo.InvariantCulture),
        "script" => engine.ExecuteScript(ReadDocument(Required("--file")), functions),
        _ => throw new ArgumentException("Unknown command: " + args[0])
    };
    Console.WriteLine(JsonSerializer.Serialize(new { result }));
    return 0;
}
catch (EngineCompilationException error)
{
    Console.Error.WriteLine(JsonSerializer.Serialize(new { error = "compilation", diagnostics = error.Diagnostics }));
    return 2;
}
catch (Exception error)
{
    Console.Error.WriteLine(JsonSerializer.Serialize(new { error = error.GetType().Name, message = error.Message }));
    return 1;
}
