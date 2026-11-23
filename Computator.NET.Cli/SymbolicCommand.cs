using System.Globalization;
using System.Text.Json;
using Computator.NET.Engine.Symbolic;

namespace Computator.NET.Cli;

internal static class SymbolicCommand
{
    public static readonly string[] Options = ["--operation", "--expression", "--variable", "--order",
        "--point", "--direction", "--domain", "--precision", "--lower", "--upper",
        "--substitutions", "--equations", "--variables"];

    public static SymbolicResult Execute(IReadOnlyDictionary<string, string> options)
    {
        string Required(string key) => options.TryGetValue(key, out var value) ? value
            : throw new ArgumentException("Missing " + key);
        string Optional(string key, string fallback) => options.TryGetValue(key, out var value) ? value : fallback;
        int Integer(string key, int fallback)
        {
            if (!options.TryGetValue(key, out var text)) return fallback;
            if (!int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var value))
                throw new ArgumentException(key + " must be a non-negative integer.");
            return value;
        }
        var operation = Required("--operation");
        string[] operationOptions = operation switch
        {
            "simplify" or "expand" or "factor" or "determinant" or "inverse" or "transpose" or "trace" => [],
            "differentiate" => ["--variable", "--order"],
            "integrate" => ["--variable"],
            "definite-integrate" => ["--variable", "--lower", "--upper"],
            "limit" => ["--variable", "--point", "--direction"],
            "solve" => ["--variable", "--domain"],
            "substitute" => ["--substitutions"],
            "evaluate" => ["--substitutions", "--precision"],
            "taylor" => ["--variable", "--point", "--order"],
            "solve-system" => ["--equations", "--variables", "--domain"],
            _ => throw new ArgumentException("Unknown symbolic operation: " + operation)
        };
        var allowed = new HashSet<string>(operationOptions, StringComparer.Ordinal) { "--operation", "--domain" };
        if (operation != "solve-system") allowed.Add("--expression");
        foreach (var option in options.Keys)
            if (!allowed.Contains(option)) throw new ArgumentException(option + " does not apply to symbolic " + operation + ".");

        var request = new SymbolicRequest
        {
            Operation = operation,
            Expression = operation == "solve-system" ? "" : Required("--expression"),
            Variable = Optional("--variable", "x"),
            Order = Integer("--order", 1),
            Point = Optional("--point", "0"),
            Direction = Optional("--direction", "both"),
            Domain = Optional("--domain", "complex"),
            Precision = Integer("--precision", 30),
            LowerBound = operation == "definite-integrate" ? Required("--lower") : "",
            UpperBound = operation == "definite-integrate" ? Required("--upper") : ""
        };
        if (operation == "solve-system")
        {
            request.Equations = StringArray(Required("--equations"), "--equations");
            request.Variables = StringArray(Required("--variables"), "--variables");
        }
        if (operation == "substitute" || options.ContainsKey("--substitutions"))
            request.Substitutions = Substitutions(Required("--substitutions"));
        return new SymbolicEngine().Execute(request);
    }

    private static JsonDocument Json(string text, string option)
    {
        if (text.Length > 65536) throw new ArgumentException(option + " exceeds 65,536 characters.");
        try { return JsonDocument.Parse(text, new JsonDocumentOptions { MaxDepth = 4 }); }
        catch (JsonException error) { throw new ArgumentException(option + " must contain valid JSON.", error); }
    }

    private static string[] StringArray(string text, string option)
    {
        using var json = Json(text, option);
        if (json.RootElement.ValueKind != JsonValueKind.Array)
            throw new ArgumentException(option + " must be a JSON array of mathematical strings.");
        var result = new List<string>();
        foreach (var item in json.RootElement.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String) throw new ArgumentException(option + " must contain strings only.");
            result.Add(item.GetString()!);
        }
        return result.ToArray();
    }

    private static Dictionary<string, string> Substitutions(string text)
    {
        using var json = Json(text, "--substitutions");
        if (json.RootElement.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("--substitutions must be a JSON object mapping variables to mathematical strings.");
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in json.RootElement.EnumerateObject())
        {
            if (item.Value.ValueKind != JsonValueKind.String || !result.TryAdd(item.Name, item.Value.GetString()!))
                throw new ArgumentException("Substitutions require unique variable names and string values.");
        }
        return result;
    }
}
