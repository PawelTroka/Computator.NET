using System;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Compilation.Builtins;
using Computator.NET.Core.Functions;
using Computator.NET.DataTypes.Configuration;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture("host")]
    [TestFixture("generated")]
    [TestFixture("precompiled")]
    public class InverseHyperbolicCosineTests
    {
        private readonly string _surface;
        private NativeCompiler _compiler;
        private Func<double, double> _cosine;
        private Func<double, double> _secant;

        public InverseHyperbolicCosineTests(string surface)
        {
            _surface = surface;
        }

        [OneTimeSetUp]
        public void LoadTheHostOrActualShippedDefinitions()
        {
            _cosine = ElementaryFunctions.arcosh;
            _secant = ElementaryFunctions.arsech;
            if (_surface == "precompiled")
            {
                _cosine = PrecompiledExpressionSurface.arcosh;
                _secant = PrecompiledExpressionSurface.arsech;
            }
            else if (_surface == "generated")
            {
                _compiler = new NativeCompiler();
                _compiler.AddDll(typeof(GslConfig).Assembly.Location);
                var assembly = _compiler.Compile("public static class EmittedFunctions {" + ElementaryFunctions.ToCode + "}");
                var type = assembly.GetType("EmittedFunctions");
                _cosine = type.GetMethod("arcosh", new[] { typeof(double) }).CreateDelegate<Func<double, double>>();
                _secant = type.GetMethod("arsech", new[] { typeof(double) }).CreateDelegate<Func<double, double>>();
            }
        }

        [OneTimeTearDown]
        public void ReleaseTheEmittedDefinitions()
        {
            _cosine = null;
            _secant = null;
            _compiler?.Dispose();
        }

        // Independent fixtures: evaluate log(t + sqrt(t*t - 1)) with
        // 800 decimal digits from the exact binary64 input, using t=x for
        // arcosh and the high-precision t=1/x for arsech, then round to double.
        [TestCase(1.0000000000000002, 2.1073424255447014e-8)]
        [TestCase(1.00000001, 0.0001414213556897163)]
        [TestCase(1.01, 0.14130376948564863)]
        [TestCase(1.5, 0.9624236501192069)]
        [TestCase(2.0, 1.3169578969248168)]
        [TestCase(10.0, 2.993222846126381)]
        [TestCase(1e20, 46.74484904044086)]
        [TestCase(1e154, 355.291251501643)]
        [TestCase(1e308, 709.889355822726)]
        [TestCase(double.MaxValue, 710.475860073944)]
        public void CosineKeepsFiniteAccurateResultsAcrossItsRealDomain(double input, double expected)
        {
            NUnit.Framework.Assert.That(_cosine(input), Is.EqualTo(expected).Within(4).Ulps);
        }

        [TestCase(0.9999999999999999, 1.4901161193847656e-8)]
        [TestCase(0.99999999, 0.0001414213571818693)]
        [TestCase(0.9, 0.46714530810326194)]
        [TestCase(0.49999999999999994, 1.3169578969248168)]
        [TestCase(0.5, 1.3169578969248168)]
        [TestCase(0.5000000000000001, 1.3169578969248164)]
        [TestCase(0.1, 2.993222846126381)]
        [TestCase(1e-20, 46.74484904044086)]
        [TestCase(1e-300, 691.4686750787737)]
        [TestCase(1e-308, 709.889355822726)]
        [TestCase(2.2250738585072014e-308, 709.0895657128241)]
        [TestCase(3 * double.Epsilon, 744.0346068132731)]
        [TestCase(double.Epsilon, 745.1332191019412)]
        public void SecantKeepsTheDistanceFromOneAndFiniteSubnormalResults(double input, double expected)
        {
            NUnit.Framework.Assert.That(_secant(input), Is.EqualTo(expected).Within(4).Ulps);
        }

        [Test]
        public void UnitInputsReturnPositiveZero()
        {
            NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits(_cosine(1)), Is.Zero);
            NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits(_secant(1)), Is.Zero);
        }

        [Test]
        public void InfiniteLimitsAndSignedZeroRetainTheirExistingDomainBehavior()
        {
            NUnit.Framework.Assert.That(_cosine(double.PositiveInfinity), Is.EqualTo(double.PositiveInfinity));
            NUnit.Framework.Assert.That(_secant(0.0), Is.EqualTo(double.PositiveInfinity));
            NUnit.Framework.Assert.That(_secant(BitConverter.Int64BitsToDouble(long.MinValue)), Is.NaN);
        }

        [Test]
        public void CosineRejectsInputsOutsideTheRealDomain()
        {
            foreach (var value in new[] { double.NaN, double.NegativeInfinity, -1.0, -double.Epsilon,
                0.0, double.Epsilon, 0.5, Math.BitDecrement(1.0) })
                NUnit.Framework.Assert.That(_cosine(value), Is.NaN, value.ToString("R"));
        }

        [Test]
        public void SecantRejectsInputsOutsideTheRealDomain()
        {
            foreach (var value in new[] { double.NaN, double.NegativeInfinity, -1.0, -double.Epsilon,
                Math.BitIncrement(1.0), 2.0, double.MaxValue, double.PositiveInfinity })
                NUnit.Framework.Assert.That(_secant(value), Is.NaN, value.ToString("R"));
        }
    }
}
