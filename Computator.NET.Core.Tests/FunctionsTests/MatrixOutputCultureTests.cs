using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.DataTypes.SettingsTypes;
using MathNet.Numerics.LinearAlgebra;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture]
    [NonParallelizable]
    public class MatrixOutputCultureTests
    {
        private const string Separator = "\n---matrix-output-test---\n";
        private static readonly NumericalOutputNotationType[] Notations =
        {
            NumericalOutputNotationType.MathematicalNotation,
            NumericalOutputNotationType.EngineeringNotation
        };

        // Exercise the generated ScriptingFunctions/extension implementations,
        // not a test-only formatter. Each of the eight objects is small enough
        // that the production output must contain every element without ellipses.
        private const string Script = @"
            var realMatrix = new double[,] { { 1.25, 0.0 }, { -2.5, 3e20 } };
            var realVector = new double[] { 1.25, 0.0, -2.5, 3e20 };
            var complexMatrix = new System.Numerics.Complex[,] {
                { new System.Numerics.Complex(1.25, 2.5), System.Numerics.Complex.Zero },
                { new System.Numerics.Complex(-2.5, -3.75), new System.Numerics.Complex(3e20, -4e-20) }
            };
            var complexVector = new System.Numerics.Complex[] {
                new System.Numerics.Complex(1.25, 2.5), System.Numerics.Complex.Zero,
                new System.Numerics.Complex(-2.5, -3.75), new System.Numerics.Complex(3e20, -4e-20)
            };
            var values = new object[] {
                MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.DenseOfArray(realMatrix),
                MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.SparseOfArray(realMatrix),
                MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray(realVector),
                MathNet.Numerics.LinearAlgebra.Vector<double>.Build.SparseOfArray(realVector),
                MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>.Build.DenseOfArray(complexMatrix),
                MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>.Build.SparseOfArray(complexMatrix),
                MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex>.Build.DenseOfArray(complexVector),
                MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex>.Build.SparseOfArray(complexVector)
            };
            foreach (var value in values) {
                write(value);
                write(""\n---matrix-output-test---\n"");
            }
        ";

        [TestCase("en-US")]
        [TestCase("pl-PL")]
        [TestCase("de-DE")]
        [TestCase("fr-FR")]
        public void HostWritesAllMatrixAndVectorValuesWithInvariantFormatting(string culture)
        {
            var previousCulture = CultureInfo.CurrentCulture;
            var previousUiCulture = CultureInfo.CurrentUICulture;
            var settings = Computator.NET.Core.Properties.Settings.Default;
            var previousNotation = settings.NumericalOutputNotation;
            try
            {
                var values = CreateValues();
                foreach (var notation in Notations)
                {
                    settings.NumericalOutputNotation = notation;
                    SetCulture(CultureInfo.InvariantCulture);
                    var expected = values.Select(WriteHost).ToArray();
                    SetCulture(CultureInfo.GetCultureInfo(culture));
                    for (var index = 0; index < values.Length; index++)
                    {
                        var actual = WriteHost(values[index]);
                        NUnit.Framework.Assert.That(actual, Is.EqualTo(expected[index]),
                            values[index].GetType().FullName + ": " + notation);
                        AssertData(actual, index >= 4, notation);
                    }
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
                CultureInfo.CurrentUICulture = previousUiCulture;
                settings.NumericalOutputNotation = previousNotation;
            }
        }

        [TestCase("en-US")]
        [TestCase("pl-PL")]
        [TestCase("de-DE")]
        [TestCase("fr-FR")]
        public void GeneratedScriptOutputMatchesTheInvariantHostIncludingAllElements(string culture)
        {
            var previousCulture = CultureInfo.CurrentCulture;
            var previousUiCulture = CultureInfo.CurrentUICulture;
            var previousDirectory = Environment.CurrentDirectory;
            var settings = Computator.NET.Core.Properties.Settings.Default;
            var previousNotation = settings.NumericalOutputNotation;
            try
            {
                var values = CreateValues();
                foreach (var notation in Notations)
                {
                    settings.NumericalOutputNotation = notation;
                    SetCulture(CultureInfo.InvariantCulture);
                    var expected = values.Select(WriteHost).ToArray();
                    SetCulture(CultureInfo.GetCultureInfo(culture));
                    var output = new StringBuilder();
                    new ScriptEvaluator().Evaluate(Script, "").Evaluate(text => output.Append(text));
                    var sections = output.ToString().Split(new[] { Separator }, StringSplitOptions.None);
                    NUnit.Framework.Assert.That(sections.Length, Is.EqualTo(values.Length + 1));
                    NUnit.Framework.Assert.That(sections[values.Length], Is.Empty);
                    for (var index = 0; index < values.Length; index++)
                    {
                        NUnit.Framework.Assert.That(sections[index], Is.EqualTo(expected[index]),
                            values[index].GetType().FullName + ": " + notation);
                        AssertData(sections[index], index >= 4, notation);
                    }
                }
            }
            finally
            {
                Environment.CurrentDirectory = previousDirectory;
                CultureInfo.CurrentCulture = previousCulture;
                CultureInfo.CurrentUICulture = previousUiCulture;
                settings.NumericalOutputNotation = previousNotation;
            }
        }

        private static object[] CreateValues()
        {
            var realMatrix = new[,] { { 1.25, 0d }, { -2.5, 3e20 } };
            var realVector = new[] { 1.25, 0d, -2.5, 3e20 };
            var complexMatrix = new[,] {
                { new Complex(1.25, 2.5), Complex.Zero },
                { new Complex(-2.5, -3.75), new Complex(3e20, -4e-20) }
            };
            var complexVector = new[] { new Complex(1.25, 2.5), Complex.Zero,
                new Complex(-2.5, -3.75), new Complex(3e20, -4e-20) };
            return new object[] {
                Matrix<double>.Build.DenseOfArray(realMatrix),
                Matrix<double>.Build.SparseOfArray(realMatrix),
                MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray(realVector),
                MathNet.Numerics.LinearAlgebra.Vector<double>.Build.SparseOfArray(realVector),
                Matrix<Complex>.Build.DenseOfArray(complexMatrix),
                Matrix<Complex>.Build.SparseOfArray(complexMatrix),
                MathNet.Numerics.LinearAlgebra.Vector<Complex>.Build.DenseOfArray(complexVector),
                MathNet.Numerics.LinearAlgebra.Vector<Complex>.Build.SparseOfArray(complexVector)
            };
        }

        private static string WriteHost(object value)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                // This public path uses the same objectToString as console write.
                ScriptingFunctions.write(writer, value);
                writer.Flush();
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private static void AssertData(string output, bool complex, NumericalOutputNotationType notation)
        {
            var headerEnd = output.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            NUnit.Framework.Assert.That(headerEnd, Is.GreaterThan(0), "Missing matrix/vector type header.");
            var data = output.Substring(headerEnd + Environment.NewLine.Length);
            string[] expected;
            if (!complex)
                expected = new[] { "1.25", "0", "-2.5",
                    notation == NumericalOutputNotationType.MathematicalNotation ? "3·10²⁰" : "3E+20" };
            else if (notation == NumericalOutputNotationType.MathematicalNotation)
                expected = new[] { "1.25+2.5·i", "0", "-2.5-3.75·i", "3·10²⁰-4·10⁻²⁰·i" };
            else
                // Engineering complex output intentionally follows the runtime's
                // invariant Complex format, rather than a locale-specific tuple.
                expected = new[] { new Complex(1.25, 2.5), Complex.Zero,
                    new Complex(-2.5, -3.75), new Complex(3e20, -4e-20) }
                    .Select(value => value.ToString(CultureInfo.InvariantCulture)).ToArray();

            // Normalize only presentation whitespace. Signs, decimal points,
            // exponents, imaginary units, value order and omitted zeros all count.
            NUnit.Framework.Assert.That(Regex.Replace(data, @"\s+", ""),
                Is.EqualTo(Regex.Replace(string.Concat(expected), @"\s+", "")));
        }

        private static void SetCulture(CultureInfo culture)
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }
    }
}
