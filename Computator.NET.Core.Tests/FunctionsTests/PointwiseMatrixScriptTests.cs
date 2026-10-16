using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Computator.NET.Core.Evaluation;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture, NonParallelizable]
    public class PointwiseMatrixScriptTests
    {
        // ScriptEvaluator emits MatrixFunctions.ToCode independently from the
        // host methods used by the portable engine. Exercise that real path.
        [TestCase("DenseOfArray")]
        [TestCase("SparseOfArray")]
        public void GeneratedRealFoldsPreserveTheFirstOperand(string storage)
        {
            const string script = @"
                var a = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.STORAGE(new double[,] { { 0, -6, 8 } });
                var b = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.STORAGE(new double[,] { { 2, -3, 4 } });
                var c = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.STORAGE(new double[,] { { 3, 2, -2 } });
                var results = new[] {
                    PointwiseMultiply(a), PointwiseDivide(a),
                    PointwiseMultiply(a,b), PointwiseDivide(a,b),
                    PointwiseMultiply(a,b,c), PointwiseDivide(a,b,c)
                };
                foreach (var result in results) {
                    for (var column = 0; column < 3; column++) {
                        write(result[0,column].ToString(""R"", System.Globalization.CultureInfo.InvariantCulture));
                        write("";"");
                    }
                    result[0,0] = 123;
                }
                foreach (var input in new[] { a, b, c }) {
                    for (var column = 0; column < 3; column++) {
                        write(input[0,column].ToString(""R"", System.Globalization.CultureInfo.InvariantCulture));
                        write("";"");
                    }
                }
            ";
            var expected = new double[] {
                0, -6, 8, 0, -6, 8,
                0, 18, 32, 0, 2, 2,
                0, 36, -64, 0, 1, -1,
                0, -6, 8, 2, -3, 4, 3, 2, -2
            };
            NUnit.Framework.Assert.That(Execute(script.Replace("STORAGE", storage)), Is.EqualTo(expected).Within(1e-12));
        }

        [TestCase("DenseOfArray")]
        [TestCase("SparseOfArray")]
        public void GeneratedComplexFoldsUseComplexArithmeticAndPreserveInputs(string storage)
        {
            const string script = @"
                var a = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>.Build.STORAGE(new System.Numerics.Complex[,] {
                    { new System.Numerics.Complex(2,1), System.Numerics.Complex.Zero, new System.Numerics.Complex(-3,2) }
                });
                var b = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>.Build.STORAGE(new System.Numerics.Complex[,] {
                    { new System.Numerics.Complex(1,-1), new System.Numerics.Complex(2,1), new System.Numerics.Complex(0,2) }
                });
                var c = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>.Build.STORAGE(new System.Numerics.Complex[,] {
                    { System.Numerics.Complex.ImaginaryOne, -System.Numerics.Complex.One, new System.Numerics.Complex(-1,1) }
                });
                var results = new[] {
                    PointwiseMultiply(a), PointwiseDivide(a),
                    PointwiseMultiply(a,b), PointwiseDivide(a,b),
                    PointwiseMultiply(a,b,c), PointwiseDivide(a,b,c)
                };
                foreach (var result in results) {
                    for (var column = 0; column < 3; column++) {
                        write(result[0,column].Real.ToString(""R"", System.Globalization.CultureInfo.InvariantCulture));
                        write("";"");
                        write(result[0,column].Imaginary.ToString(""R"", System.Globalization.CultureInfo.InvariantCulture));
                        write("";"");
                    }
                    result[0,0] = 123;
                }
                foreach (var input in new[] { a, b, c }) {
                    for (var column = 0; column < 3; column++) {
                        write(input[0,column].Real.ToString(""R"", System.Globalization.CultureInfo.InvariantCulture));
                        write("";"");
                        write(input[0,column].Imaginary.ToString(""R"", System.Globalization.CultureInfo.InvariantCulture));
                        write("";"");
                    }
                }
            ";
            var expected = new[] {
                2d, 1, 0, 0, -3, 2, 2, 1, 0, 0, -3, 2,
                3, -1, 0, 0, -4, -6, 0.5, 1.5, 0, 0, 1, 1.5,
                1, 3, 0, 0, 10, 2, 1.5, -0.5, 0, 0, 0.25, -1.25,
                2, 1, 0, 0, -3, 2, 1, -1, 2, 1, 0, 2, 0, 1, -1, 0, -1, 1
            };
            NUnit.Framework.Assert.That(Execute(script.Replace("STORAGE", storage)), Is.EqualTo(expected).Within(1e-12));
        }

        private static double[] Execute(string script)
        {
            var previousDirectory = Environment.CurrentDirectory;
            try
            {
                var output = new StringBuilder();
                new ScriptEvaluator().Evaluate(script, "").Evaluate(text => output.Append(text));
                return output.ToString().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => double.Parse(value, CultureInfo.InvariantCulture)).ToArray();
            }
            finally { Environment.CurrentDirectory = previousDirectory; }
        }
    }
}
