using System;
using System.Collections.Generic;
using System.Text.Json;
using Computator.NET.Engine.Symbolic;

namespace Computator.NET.Hosting
{
    /// <summary>
    /// The raw JSON contract of the symbolic endpoint, linked into both hosts.
    /// Read properties before constructing dictionaries so duplicate assignments
    /// cannot silently change the mathematics. Numerical endpoints keep their
    /// existing serializers and contracts.
    /// </summary>
    internal static class SymbolicRequestJson
    {
        internal const int MaximumBodyBytes = 262144;

        internal static SymbolicRequest Parse(ReadOnlyMemory<byte> utf8)
        {
            using var json = JsonDocument.Parse(utf8, new JsonDocumentOptions { MaxDepth = 4 });
            if (json.RootElement.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("A symbolic request must be a JSON object.");

            var request = new SymbolicRequest();
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in json.RootElement.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new ArgumentException("The symbolic request field '" + property.Name + "' is repeated.");
                var value = property.Value;
                switch (property.Name.ToLowerInvariant())
                {
                    case "operation": request.Operation = Text(value, property.Name); break;
                    case "expression": request.Expression = Text(value, property.Name); break;
                    case "variable": request.Variable = Text(value, property.Name); break;
                    case "point": request.Point = Text(value, property.Name); break;
                    case "direction": request.Direction = Text(value, property.Name); break;
                    case "domain": request.Domain = Text(value, property.Name); break;
                    case "lowerbound": request.LowerBound = Text(value, property.Name); break;
                    case "upperbound": request.UpperBound = Text(value, property.Name); break;
                    case "order": request.Order = Integer(value, property.Name); break;
                    case "precision": request.Precision = Integer(value, property.Name); break;
                    case "equations": request.Equations = Strings(value, property.Name); break;
                    case "variables": request.Variables = Strings(value, property.Name); break;
                    case "substitutions": request.Substitutions = Substitutions(value); break;
                    default: throw new ArgumentException("Unknown symbolic request field '" + property.Name + "'.");
                }
            }
            return request;
        }

        private static string Text(JsonElement value, string name)
        {
            if (value.ValueKind != JsonValueKind.String)
                throw new ArgumentException(name + " must be a JSON string containing mathematical text.");
            return value.GetString()!;
        }

        private static int Integer(JsonElement value, string name)
        {
            if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var number))
                throw new ArgumentException(name + " must be a JSON integer number.");
            return number;
        }

        private static string[] Strings(JsonElement value, string name)
        {
            if (value.ValueKind != JsonValueKind.Array)
                throw new ArgumentException(name + " must be a JSON array of mathematical strings.");
            var values = new List<string>();
            foreach (var item in value.EnumerateArray()) values.Add(Text(item, name + " entries"));
            return values.ToArray();
        }

        private static Dictionary<string, string> Substitutions(JsonElement value)
        {
            if (value.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("substitutions must be a JSON object mapping variables to mathematical strings.");
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var item in value.EnumerateObject())
            {
                if (!values.TryAdd(item.Name, Text(item.Value, "The substitution for '" + item.Name + "'")))
                    throw new ArgumentException("The substitution for '" + item.Name + "' is repeated.");
            }
            return values;
        }
    }
}
