using System.Globalization;
using System.Reflection;
using Computator.NET.Core.Constants;
using Computator.NET.Core.Functions;
using Computator.NET.Core.NumericalCalculations;
using Computator.NET.Engine;
using MathNet.Numerics;

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
        AddScientificItems(entries);
        if (scripting) entries.Add(Item("write", "write(value)", "Script output"));
        return entries.ToArray();
    }

    private static void AddScientificItems(List<object> entries)
    {
        // These types already execute through shared aliases/imports. Keep their
        // qualifiers: unlike scalar/matrix functions, they are not static imports.
        // Object/array results can be composed in expressions, too.
        foreach (var (type, category) in new[]
        {
            (typeof(Euclid), "Integer utilities"), (typeof(Polynomial), "Polynomial operations"),
            (typeof(DifferentialEquation), "Ordinary differential equations"),
            (typeof(Integral), "Numerical integration"), (typeof(Derivative), "Numerical differentiation"),
            (typeof(FunctionRoot), "Function roots")
        })
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         .Where(method => !method.IsSpecialName).OrderBy(method => method.Name, StringComparer.Ordinal)
                         .ThenBy(MethodSignature, StringComparer.Ordinal))
            {
                var name = type.Name + "." + method.Name;
                entries.Add(Item(name, MethodSignature(method), category));
            }

        // Complete the type in `new Poly...`; inserting `new Polynomial` would
        // duplicate a constructor keyword the user has already entered.
        foreach (var constructor in typeof(Polynomial).GetConstructors()
                     .OrderBy(ParameterSignature, StringComparer.Ordinal))
            entries.Add(Item("Polynomial", "new Polynomial" + ParameterSignature(constructor), "Polynomial operations"));
        entries.Add(Item("Polynomial.Zero", "Polynomial Polynomial.Zero", "Polynomial operations"));
    }

    private static string MethodSignature(MethodInfo method) =>
        TypeName(method.ReturnType) + " " + method.DeclaringType!.Name + "." + method.Name + ParameterSignature(method);

    private static string ParameterSignature(MethodBase method) => "(" +
        string.Join(", ", method.GetParameters().Select(parameter =>
        {
            var type = parameter.ParameterType;
            var modifier = type.IsByRef ? (parameter.IsOut ? "out " : parameter.IsIn ? "in " : "ref ")
                : parameter.IsDefined(typeof(ParamArrayAttribute)) ? "params " : "";
            if (type.IsByRef) type = type.GetElementType()!;
            var optional = parameter.HasDefaultValue ? " = " + (parameter.DefaultValue == null ? "null"
                : type.IsEnum ? TypeName(type) + "." + parameter.DefaultValue
                : Convert.ToString(parameter.DefaultValue, CultureInfo.InvariantCulture)) : "";
            return modifier + TypeName(type) + " " + parameter.Name + optional;
        })) + ")";

    private static string TypeName(Type type)
    {
        if (type.IsArray) return TypeName(type.GetElementType()!) + "[]";
        if (type == typeof(double)) return "real";
        if (type == typeof(System.Numerics.Complex)) return "complex";
        if (type == typeof(int)) return "int";
        if (type == typeof(long)) return "integer";
        if (type == typeof(uint)) return "uint";
        if (type == typeof(ulong)) return "natural";
        if (type == typeof(float)) return "float";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(string)) return "string";
        if (type.IsGenericType)
        {
            var arguments = string.Join(", ", type.GetGenericArguments().Select(TypeName));
            return type.GetGenericTypeDefinition() == typeof(ValueTuple<,>) ? "(" + arguments + ")"
                : type.Name.Split('`')[0] + "<" + arguments + ">";
        }
        return type.Name;
    }

    private static object Item(string name, string signature, string category) => new
    {
        text = name, menuText = signature,
        details = new { title = name, category, description = "Available in the portable local application. " + System.Net.WebUtility.HtmlEncode(signature) }
    };
}
