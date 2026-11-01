using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using Computator.NET.Core.Evaluation;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.AutocompletionTests
{
    [TestFixture(false)]
    [TestFixture(true)]
    [NonParallelizable, SetCulture("en-US")]
    public class FunctionReferencePolynomialExamplesTests
    {
        private const string InspectionSeparator = "\n---polynomial-reference-values---\n";
        private readonly bool _legacy;

        public FunctionReferencePolynomialExamplesTests(bool legacy) { _legacy = legacy; }

        private sealed class LegacyScriptEvaluator : ScriptEvaluator
        {
            protected override bool UsePrecompiledBuiltins => false;
        }

        // Use every explicitly classified script from the shipped reference, so
        // adding one without an executable validator fails instead of skipping it.
        [TestCaseSource(typeof(FunctionReferenceRichContentTests), nameof(FunctionReferenceRichContentTests.ScriptExamples))]
        public void EveryDocumentedScriptExecutesThroughTheActualTslEvaluator(string signature, string code, string displayedResult)
        {
            switch (signature)
            {
                case "new Polynomial(coefficients)":
                    AssertRoots(code, displayedResult);
                    break;
                case "Polynomial.Fit(x,y,order,method)":
                    AssertFit(code, displayedResult);
                    break;
                case "Polynomial.DivideRemainder(a,b)":
                    AssertDivision(code, displayedResult);
                    break;
                default:
                    NUnit.Framework.Assert.Fail("No executable validator for reference script: " + signature);
                    break;
            }
        }

        private void AssertRoots(string code, string displayedResult)
        {
            var result = Inspect(code, @"
                var referenceValues = new real[2 * roots.Length];
                for (var index = 0; index < roots.Length; index++) {
                    referenceValues[2 * index] = roots[index].Real;
                    referenceValues[2 * index + 1] = roots[index].Imaginary;
                }
            ");
            NUnit.Framework.Assert.That(result.Output, Is.Empty,
                "The example assigns the root array; its displayed result describes that array, not console output.");
            NUnit.Framework.Assert.That(result.Values, Has.Length.EqualTo(6), displayedResult);
            var roots = Enumerable.Range(0, 3)
                .Select(index => new Complex(result.Values[2 * index], result.Values[2 * index + 1]))
                .OrderBy(root => root.Real).ToArray();
            for (var index = 0; index < roots.Length; index++)
            {
                NUnit.Framework.Assert.That(Complex.Abs(roots[index] - (index + 1)), Is.LessThan(1e-12), displayedResult);
                // Independently evaluate the factored polynomial, without relying
                // on the same root routine or on an assumed returned root order.
                var residual = (roots[index] - 1) * (roots[index] - 2) * (roots[index] - 3);
                NUnit.Framework.Assert.That(Complex.Abs(residual), Is.LessThan(1e-11), displayedResult);
            }
        }

        private void AssertFit(string code, string displayedResult)
        {
            var result = Inspect(code, "var referenceValues = p.Coefficients;");
            var expected = FunctionReferenceRichContentTests.ParseExpectedScalar(displayedResult);
            NUnit.Framework.Assert.That(expected, Is.EqualTo(3.0), "The samples follow 1 - 3x + 2x².");
            NUnit.Framework.Assert.That(double.Parse(result.Output, NumberStyles.Float, CultureInfo.InvariantCulture),
                Is.EqualTo(expected).Within(1e-12), displayedResult);
            NUnit.Framework.Assert.That(result.Values, Is.EqualTo(new[] { 1.0, -3.0, 2.0 }).Within(1e-12));
        }

        private void AssertDivision(string code, string displayedResult)
        {
            var result = Inspect(code, @"
                var reconstructed = parts.Item1 * new Polynomial(1,1) + parts.Item2;
                var referenceValues = new real[] {
                    parts.Item1.Degree, parts.Item2.Degree,
                    parts.Item1.Coefficients[0], parts.Item1.Coefficients[1], parts.Item2.Coefficients[0],
                    reconstructed.Evaluate(0), reconstructed.Evaluate(1), reconstructed.Evaluate(3)
                };
            ");
            NUnit.Framework.Assert.That(result.Output, Is.EqualTo("2;2"), displayedResult);
            NUnit.Framework.Assert.That(displayedResult, Does.Contain("2;2"));
            NUnit.Framework.Assert.That(result.Values, Is.EqualTo(new[] { 1.0, 0.0, -1.0, 1.0, 2.0, 1.0, 2.0, 10.0 }),
                "The quotient is x - 1, the remainder is 2, and quotient * (x + 1) + remainder is x² + 1.");
        }

        private (string Output, double[] Values) Inspect(string code, string inspection)
        {
            var previousDirectory = Environment.CurrentDirectory;
            try
            {
                ScriptEvaluator evaluator = _legacy ? new LegacyScriptEvaluator() : new ScriptEvaluator();
                var output = new StringBuilder();
                // Run the actual documented code unchanged, then inspect its
                // variables. Invariant round-trip strings avoid display notation
                // affecting comparisons of the underlying numerical results.
                evaluator.Evaluate(code + "\nwrite(\"\\n---polynomial-reference-values---\\n\");\n" + inspection + @"
                    foreach (var referenceValue in referenceValues) {
                        write(referenceValue.ToString(""R"", System.Globalization.CultureInfo.InvariantCulture));
                        write("";"");
                    }
                ", "").Evaluate(text => output.Append(text));
                var sections = output.ToString().Split(new[] { InspectionSeparator }, StringSplitOptions.None);
                NUnit.Framework.Assert.That(sections, Has.Length.EqualTo(2));
                return (sections[0], sections[1].Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture)).ToArray());
            }
            finally { Environment.CurrentDirectory = previousDirectory; }
        }
    }
}
