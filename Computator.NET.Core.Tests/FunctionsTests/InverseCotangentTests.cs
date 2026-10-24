using System;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Compilation.Builtins;
using Computator.NET.Core.Functions;
using Computator.NET.DataTypes.Configuration;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture(0)]
    [TestFixture(1)]
    [TestFixture(2)]
    public class InverseCotangentTests
    {
        private readonly int _surface;
        private NativeCompiler _compiler;
        private Func<double, double>[] _functions;
        private sealed class ProgramTag { }

        public InverseCotangentTests(int surface) => _surface = surface;

        [OneTimeSetUp]
        public void SelectHostEmittedOrPrecompiledFunctions()
        {
            if (_surface == 0)
            {
                _functions = new Func<double, double>[] { ElementaryFunctions.arccot, ElementaryFunctions.arcctg };
                return;
            }
            if (_surface == 2)
            {
                _functions = new Func<double, double>[] { PrecompiledExpressionBuiltins<ProgramTag>.arccot,
                    PrecompiledExpressionBuiltins<ProgramTag>.arcctg };
                return;
            }

            _compiler = new NativeCompiler();
            _compiler.AddDll(typeof(GslConfig).Assembly.Location);
            var assembly = _compiler.Compile("public static class EmittedFunctions {" + ElementaryFunctions.ToCode + "}");
            var type = assembly.GetType("EmittedFunctions");
            Func<double, double> Emitted(string name) => type.GetMethod(name, new[] { typeof(double) })
                .CreateDelegate<Func<double, double>>();
            _functions = new[] { Emitted("arccot"), Emitted("arcctg") };
        }

        [OneTimeTearDown]
        public void ReleaseEmittedFunctions()
        {
            _functions = null;
            _compiler?.Dispose();
        }

        // Independent binary64 fixtures: evaluate PI/2 - atan(x) with 820
        // decimal digits from the exact binary64 inputs. PI uses Machin's
        // formula; atan uses its power series after half-angle reduction.
        [TestCase(-1e308, 3.141592653589793)]
        [TestCase(-1e16, 3.141592653589793)]
        [TestCase(-1e8, 3.141592643589793)]
        [TestCase(-100d, 3.131592986903128)]
        [TestCase(-10d, 3.0419240010986313)]
        [TestCase(-2d, 2.677945044588987)]
        [TestCase(-1d, 2.356194490192345)]
        [TestCase(-0.5, 2.0344439357957027)]
        [TestCase(-0.1, 1.6704649792860586)]
        [TestCase(0d, 1.5707963267948966)]
        [TestCase(0.1, 1.4711276743037345)]
        [TestCase(0.5, 1.1071487177940904)]
        [TestCase(1d, 0.7853981633974483)]
        [TestCase(2d, 0.4636476090008061)]
        [TestCase(10d, 0.09966865249116202)]
        [TestCase(100d, 0.009999666686665238)]
        [TestCase(1e4, 9.999999966666667e-5)]
        [TestCase(1e8, 1e-8)]
        [TestCase(1e10, 1e-10)]
        [TestCase(1e16, 1e-16)]
        [TestCase(1e20, 1e-20)]
        [TestCase(1e100, 1e-100)]
        [TestCase(1e300, 1e-300)]
        [TestCase(double.MaxValue, 5.562684646268003e-309)]
        public void BothNamesAgreeWithIndependentReferencesAcrossTheRealDomain(double input, double expected)
        {
            foreach (var function in _functions)
                NUnit.Framework.Assert.That(function(input), Is.EqualTo(expected).Within(2).Ulps, function.Method.Name);
        }

        [Test]
        public void BothSignedZerosAndTinyInputsRetainTheHalfPiLimit()
        {
            foreach (var function in _functions)
                foreach (var input in new[] { 0d, BitConverter.Int64BitsToDouble(long.MinValue),
                    double.Epsilon, -double.Epsilon, 1e-300, -1e-300 })
                    NUnit.Framework.Assert.That(function(input), Is.EqualTo(Math.PI / 2), function.Method.Name);
        }

        [Test]
        public void InfiniteInputsRetainTheirEndpointLimits()
        {
            foreach (var function in _functions)
            {
                NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits(function(double.PositiveInfinity)), Is.EqualTo(0L));
                NUnit.Framework.Assert.That(function(double.NegativeInfinity), Is.EqualTo(Math.PI));
            }
        }

        [Test]
        public void NaNRemainsNaN()
        {
            foreach (var function in _functions)
                NUnit.Framework.Assert.That(function(double.NaN), Is.NaN, function.Method.Name);
        }
    }
}
