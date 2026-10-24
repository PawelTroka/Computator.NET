using System;
using System.Collections.Generic;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Functions;
using Computator.NET.DataTypes.Configuration;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture(false)]
    [TestFixture(true)]
    public class InverseHyperbolicCosecantTests
    {
        private readonly bool generated;
        private NativeCompiler compiler;
        private Func<double, double>[] functions;

        public InverseHyperbolicCosecantTests(bool generated)
        {
            this.generated = generated;
        }

        [OneTimeSetUp]
        public void CompileTheCompleteEmittedElementaryFunctions()
        {
            functions = new Func<double, double>[] { ElementaryFunctions.arcsch, ElementaryFunctions.arcosech };
            if (!generated) return;

            compiler = new NativeCompiler();
            compiler.AddDll(typeof(GslConfig).Assembly.Location);
            var assembly = compiler.Compile("public static class EmittedFunctions {" + ElementaryFunctions.ToCode + "}");
            var type = assembly.GetType("EmittedFunctions");
            Func<double, double> Emitted(string name) => type.GetMethod(name, new[] { typeof(double) })
                .CreateDelegate<Func<double, double>>();
            functions = new[] { Emitted("arcsch"), Emitted("arcosech") };
        }

        [OneTimeTearDown]
        public void ReleaseTheEmittedFunctions()
        {
            functions = null;
            compiler?.Dispose();
        }

        private static IEnumerable<TestCaseData> IndependentReferences
        {
            get
            {
                // Evaluate ln(1/x + sqrt(1/x² + 1)) with 700 decimal digits,
                // starting from each exact binary64 input, then round to double.
                // The negative references follow the real function's oddness.
                yield return new TestCaseData(1d, 0.881373587019543);
                yield return new TestCaseData(2d, 0.48121182505960347);
                yield return new TestCaseData(0.5, 1.4436354751788103);
                yield return new TestCaseData(1e-10, 23.7189981105004);
                yield return new TestCaseData(1e-20, 46.74484904044086);
                yield return new TestCaseData(1e-100, 230.95165647996453);
                yield return new TestCaseData(Math.BitDecrement(1e-150), 346.0809111296668);
                yield return new TestCaseData(1e-150, 346.0809111296668);
                yield return new TestCaseData(Math.BitIncrement(1e-150), 346.0809111296668);
                yield return new TestCaseData(1e-200, 461.2101657793691);
                yield return new TestCaseData(1e-300, 691.4686750787737);
                yield return new TestCaseData(1e-308, 709.889355822726);
                yield return new TestCaseData(2.2250738585072014e-308, 709.0895657128241);
                yield return new TestCaseData(double.Epsilon, 745.1332191019412);
                yield return new TestCaseData(1e10, 1e-10);
                yield return new TestCaseData(1e20, 1e-20);
                yield return new TestCaseData(1e100, 1e-100);
                yield return new TestCaseData(1e200, 1e-200);
                yield return new TestCaseData(1e300, 1e-300);
                yield return new TestCaseData(double.MaxValue, 5.562684646268003e-309);
            }
        }

        [TestCaseSource(nameof(IndependentReferences))]
        public void NonzeroFiniteInputsRemainFiniteAndAgreeWithIndependentReferences(double input, double expected)
        {
            foreach (var function in functions)
                foreach (var sign in new[] { 1d, -1d })
                {
                    var result = function(sign * input);
                    NUnit.Framework.Assert.That(double.IsFinite(result), Is.True, function.Method.Name);
                    NUnit.Framework.Assert.That(result, Is.EqualTo(sign * expected).Within(4).Ulps, function.Method.Name);
                }
        }

        [Test]
        public void BothSignedZerosReturnTheirSignedInfiniteLimits()
        {
            foreach (var function in functions)
                foreach (var sign in new[] { 1d, -1d })
                    AssertBits(function(Math.CopySign(0d, sign)), Math.CopySign(double.PositiveInfinity, sign));
        }

        [Test]
        public void InfiniteInputsApproachSignedZero()
        {
            foreach (var function in functions)
                foreach (var sign in new[] { 1d, -1d })
                    AssertBits(function(Math.CopySign(double.PositiveInfinity, sign)), Math.CopySign(0d, sign));
        }

        [Test]
        public void NaNRemainsNaN()
        {
            foreach (var function in functions)
                NUnit.Framework.Assert.That(function(double.NaN), Is.NaN);
        }

        private static void AssertBits(double actual, double expected) =>
            NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits(actual),
                Is.EqualTo(BitConverter.DoubleToInt64Bits(expected)));
    }
}
