using System.Reflection;
using Computator.NET.Core.Constants;
using Computator.NET.Core.Functions;
using Computator.NET.Engine;

namespace Computator.NET.PortableWeb;

internal static class PortableCatalog
{
    public static object[] Items(bool scripting)
    {
        var entries = new List<object>();
        foreach (var type in new[] { typeof(ScalarFunctions), typeof(MatrixFunctions) })
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         .Where(method => !method.IsSpecialName).OrderBy(method => method.Name))
            {
                var parameters = string.Join(", ", method.GetParameters().Select(parameter => parameter.Name));
                var generic = method.IsGenericMethodDefinition ? "<" + string.Join(", ", method.GetGenericArguments().Select(argument => argument.Name)) + ">" : "";
                var signature = method.Name + generic + "(" + parameters + ")";
                entries.Add(Item(method.Name, signature, type == typeof(ScalarFunctions) ? "Portable scalar functions" : "Managed matrix functions"));
            }
        foreach (var field in typeof(MathematicalConstants).GetFields(BindingFlags.Public | BindingFlags.Static)
                     .Where(field => field.FieldType == typeof(double) || field.FieldType == typeof(System.Numerics.Complex)))
            entries.Add(Item(field.Name, field.Name, "Mathematical constants"));
        foreach (var property in typeof(MathematicalConstants).GetProperties(BindingFlags.Public | BindingFlags.Static)
                     .Where(property => property.PropertyType == typeof(double) || property.PropertyType == typeof(System.Numerics.Complex)))
            entries.Add(Item(property.Name, property.Name, "Mathematical constants"));
        if (scripting) entries.Add(Item("write", "write(value)", "Script output"));
        return entries.ToArray();
    }
    private static object Item(string name, string signature, string category) => new
    {
        text = name, menuText = signature,
        details = new { title = name, category, description = "Available in the portable local application. " + signature }
    };
}
