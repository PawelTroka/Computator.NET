using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Computator.NET.Core.NumericalCalculations;
using Computator.NET.WebApi.Controllers;

namespace Computator.NET.WebApi.Services
{
    /// <summary>One resource/parameter contract for JSON and legacy route adapters.</summary>
    public static class ExecutionPolicy
    {
        public const int MaximumSourceCharacters = 65536;
        public const int MaximumOutputCharacters = 65536;
        public const int MaximumDimension = 4096;
        public const long MaximumPixels = 4194304;
        public const uint MaximumIterations = 1000000;
        public const uint MaximumDerivativeOrder = 10;

        public static bool ValidNumbers(ExecutionRequest request) => request != null &&
            new[] { request.X, request.Y, request.A, request.B, request.Epsilon,
                request.XMin, request.XMax, request.YMin, request.YMax }.All(double.IsFinite) &&
            request.N >= 1 && request.N <= MaximumIterations && request.Order >= 1 &&
            request.Order <= MaximumDerivativeOrder && request.Epsilon > 0 && request.Epsilon <= 1;

        public static Dictionary<string, string[]> Validate(ExecutionRequest request, bool chart)
        {
            var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            if (request == null) { errors["request"] = new[] { "A JSON request is required." }; return errors; }
            Source(errors, "expression", request.Expression, false);
            Source(errors, "customFunctions", request.CustomFunctions, true);
            if (!ValidNumbers(request)) errors["parameters"] = new[] { "Numeric parameters must be finite and within the documented iteration/order/error limits." };
            if (chart) Chart(errors, request.Width, request.Height, request.XMin, request.XMax, request.YMin, request.YMax);
            else
            {
                if (!new[] { "calculate", "script", "integral", "derivative", "function-root" }.Contains(request.Operation))
                    errors["operation"] = new[] { "Unknown operation." };
                Method(errors, request.Operation, request.Method, request.N);
            }
            return errors;
        }

        public static Dictionary<string, string[]> ValidateLegacy(string controller, string action, IDictionary<string, object> arguments)
        {
            var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in arguments)
            {
                if (pair.Value is double number && !double.IsFinite(number)) errors[pair.Key] = new[] { "A finite number is required." };
                if (pair.Key == "equation" || pair.Key == "tslCode") Source(errors, pair.Key, pair.Value as string, false);
                if (pair.Key == "customFunctionsCode") Source(errors, pair.Key, pair.Value as string, true);
            }
            foreach (var name in new[] { "n", "maxIterations" })
                if (arguments.ContainsKey(name))
                {
                    var count = Number(arguments, name, 0);
                    if (!double.IsFinite(count) || count < 1 || count > MaximumIterations || count != Math.Truncate(count))
                        errors[name] = new[] { $"Iteration counts must be whole numbers from 1 to {MaximumIterations}." };
                }
            if (arguments.ContainsKey("order"))
            {
                var order = Number(arguments, "order", 0);
                if (order < 1 || order > MaximumDerivativeOrder) errors["order"] = new[] { "Derivative order must be from 1 to 10." };
            }
            foreach (var name in new[] { "epsilon", "maxError" })
                if (arguments.ContainsKey(name))
                {
                    var epsilon = Number(arguments, name, 0);
                    if (!double.IsFinite(epsilon) || epsilon <= 0 || epsilon > 1) errors[name] = new[] { "Error tolerance must be finite, positive and no greater than one." };
                }
            if (controller == "Chart")
                Chart(errors, Number(arguments, "width", 1920), Number(arguments, "height", 1080),
                    Number(arguments, "x0", -5), Number(arguments, "xn", 5), Number(arguments, "y0", -3), Number(arguments, "yn", 3));
            if (controller == "NumericalCalculations" && arguments.ContainsKey("equation"))
            {
                var operation = action == "GetRealIntegral" ? "integral" : action == "GetRealDerrivative" ? "derivative" : "function-root";
                Method(errors, operation, arguments.TryGetValue("method", out var method) ? method as string : null,
                    Number(arguments, "n", Number(arguments, "maxIterations", 10000)));
            }
            return errors;
        }

        private static double Number(IDictionary<string, object> values, string key, double fallback) =>
            values.TryGetValue(key, out var value) && value != null ? Convert.ToDouble(value, CultureInfo.InvariantCulture) : fallback;

        private static void Source(IDictionary<string, string[]> errors, string name, string source, bool allowEmpty)
        {
            if (source == null || (!allowEmpty && string.IsNullOrWhiteSpace(source)) || source.Length > MaximumSourceCharacters)
                errors[name] = new[] { $"Source must be {(allowEmpty ? "zero" : "one")} to {MaximumSourceCharacters} characters." };
        }
        private static void Chart(IDictionary<string, string[]> errors, double width, double height, double xMin, double xMax, double yMin, double yMax)
        {
            if (!double.IsFinite(width) || !double.IsFinite(height) || width < 100 || height < 100 ||
                width > MaximumDimension || height > MaximumDimension || width * height > MaximumPixels)
                errors["dimensions"] = new[] { $"Chart dimensions must be 100–{MaximumDimension}, with at most {MaximumPixels} pixels." };
            if (!new[] { xMin, xMax, yMin, yMax }.All(double.IsFinite) || xMin >= xMax || yMin >= yMax)
                errors["bounds"] = new[] { "Chart bounds must be finite and each minimum must be smaller than its maximum." };
        }
        private static void Method(IDictionary<string, string[]> errors, string operation, string name, double iterations)
        {
            if (operation != "integral" && operation != "derivative" && operation != "function-root") return;
            Delegate selected = null;
            if (name != null && name.Length <= 256)
            {
                if (operation == "integral" && NumericalMethodsInfo.Instance.IntegrationMethods.TryGetValue(name, out var integral)) selected = integral;
                if (operation == "derivative" && NumericalMethodsInfo.Instance.DerrivationMethods.TryGetValue(name, out var derivative)) selected = derivative;
                if (operation == "function-root" && NumericalMethodsInfo.Instance.FunctionRootMethods.TryGetValue(name, out var root)) selected = root;
            }
            if (selected == null) errors["method"] = new[] { "Unknown numerical method." };
            else if (selected.Method.Name == "rombergMethod" && iterations > 30)
                errors["n"] = new[] { "Romberg integration accepts at most 30 steps." };
        }
    }

    public sealed class BoundedExecutionOutput
    {
        private readonly StringBuilder _text = new StringBuilder();
        public void Append(string text)
        {
            if (text == null) return;
            if (text.Length > ExecutionPolicy.MaximumOutputCharacters - _text.Length)
                throw new ArgumentException($"Script output exceeds {ExecutionPolicy.MaximumOutputCharacters} characters.");
            _text.Append(text);
        }
        public override string ToString() => _text.ToString();
    }
}
