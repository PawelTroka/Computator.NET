using System;
using System.Collections.Generic;
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
    public class InverseCotangentTests
    {
        private readonly string _surface;
        private NativeCompiler _compiler;
        private Func<double, double>[] _functions;

        public InverseCotangentTests(string surface)
        {
            _surface = surface;
        }

        [OneTimeSetUp]
        public void LoadBothNamesFromTheActualMathematicalSurface()
        {
            if (_surface == "host")
            {
                _functions = new Func<double, double>[] { ElementaryFunctions.arccot, ElementaryFunctions.arcctg };
                return;
            }
            if (_surface == "precompiled")
            {
                _functions = new Func<double, double>[] { PrecompiledExpressionSurface.arccot, PrecompiledExpressionSurface.arcctg };
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
        public void ReleaseTheEmittedFunctions()
        {
            _functions = null;
            _compiler?.Dispose();
        }

        // Independent references calculated with 430 decimal digits from each
        // exact binary64 input, then rounded to double. The oracle uses the
        // convergent arctangent series after half-angle reductions, with Machin's
        // formula for pi; it does not call the implementation's atan2 operation.
        private static IEnumerable<TestCaseData> ReferenceValues
        {
            get
            {
                double[,] values = {
                    { double.Epsilon, 1.5707963267948966, 1.5707963267948966 },
                    { 1e-300, 1.5707963267948966, 1.5707963267948966 },
                    { 1e-20, 1.5707963267948966, 1.5707963267948966 },
                    { 1e-8, 1.5707963167948966, 1.5707963367948967 },
                    { 0.125, 1.446441332248135, 1.695151321341658 },
                    { 0.5, 1.1071487177940904, 2.0344439357957027 },
                    { 1.0, 0.7853981633974483, 2.356194490192345 },
                    { 2.0, 0.4636476090008061, 2.677945044588987 },
                    { 3.0, 0.3217505543966422, 2.819842099193151 },
                    { 10.0, 0.09966865249116202, 3.0419240010986313 },
                    { 1e3, 0.0009999996666668666, 3.1405926539231266 },
                    { 1e8, 1e-8, 3.141592643589793 },
                    { 1e10, 1e-10, 3.141592653489793 },
                    { 1e16, 1e-16, 3.141592653589793 },
                    { 1e20, 1e-20, 3.141592653589793 },
                    { 1e100, 1e-100, 3.141592653589793 },
                    { 1e300, 1e-300, 3.141592653589793 },
                    { double.MaxValue, 5.562684646268003e-309, 3.141592653589793 }
                };
                for (var index = 0; index < values.GetLength(0); index++)
                {
                    yield return new TestCaseData(values[index, 0], values[index, 1]);
                    yield return new TestCaseData(-values[index, 0], values[index, 2]);
                }
            }
        }

        [TestCaseSource(nameof(ReferenceValues))]
        public void BothNamesPreserveAccuracyAndTheEstablishedRealBranch(double input, double expected)
        {
            foreach (var function in _functions)
                NUnit.Framework.Assert.That(function(input), Is.EqualTo(expected).Within(4).Ulps, function.Method.Name);
        }

        [Test]
        public void BothSignedZerosReturnHalfPi()
        {
            foreach (var function in _functions)
                foreach (var zero in new[] { 0d, BitConverter.Int64BitsToDouble(long.MinValue) })
                    AssertBits(function(zero), Math.PI / 2);
        }

        [Test]
        public void InfiniteArgumentsRetainTheirEndpointLimits()
        {
            foreach (var function in _functions)
            {
                AssertBits(function(double.PositiveInfinity), 0d);
                AssertBits(function(double.NegativeInfinity), Math.PI);
            }
        }

        [Test]
        public void NaNRemainsNaN()
        {
            foreach (var function in _functions)
                NUnit.Framework.Assert.That(function(double.NaN), Is.NaN, function.Method.Name);
        }

        [Test]
        public void IncreasingPositiveArgumentsKeepStrictlyDecreasingNonzeroAngles()
        {
            foreach (var function in _functions)
            {
                var previous = Math.PI / 2;
                foreach (var input in new[] { 0.125, 1d, 10d, 1e3, 1e8, 1e16, 1e20, 1e100, 1e300, double.MaxValue })
                {
                    var actual = function(input);
                    NUnit.Framework.Assert.That(actual, Is.GreaterThan(0d).And.LessThan(previous), function.Method.Name);
                    previous = actual;
                }
            }
        }

        private static void AssertBits(double actual, double expected)
        {
            NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits(actual),
                Is.EqualTo(BitConverter.DoubleToInt64Bits(expected)));
        }
    }
}
