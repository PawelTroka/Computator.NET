using System;
using System.Numerics;
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
    public class ComplexInverseHyperbolicCosineTests
    {
        private readonly string _surface;
        private NativeCompiler _compiler;
        private Func<Complex, Complex> _cosine;

        public ComplexInverseHyperbolicCosineTests(string surface)
        {
            _surface = surface;
        }

        [OneTimeSetUp]
        public void LoadTheHostOrCompleteShippedDefinitions()
        {
            _cosine = ElementaryFunctions.arcosh;
            if (_surface == "precompiled")
                _cosine = PrecompiledExpressionSurface.arcosh;
            else if (_surface == "generated")
            {
                _compiler = new NativeCompiler();
                _compiler.AddDll(typeof(GslConfig).Assembly.Location);
                var assembly = _compiler.Compile("public static class EmittedFunctions {" + ElementaryFunctions.ToCode + "}");
                _cosine = assembly.GetType("EmittedFunctions").GetMethod("arcosh", new[] { typeof(Complex) })
                    .CreateDelegate<Func<Complex, Complex>>();
            }
        }

        [OneTimeTearDown]
        public void ReleaseEmittedDefinitions()
        {
            _cosine = null;
            _compiler?.Dispose();
        }

        // Independent principal-branch values evaluated with mpmath at 800
        // decimal digits using exact binary64 inputs, then rounded to double.
        // In particular, tiny components are checked relatively, not swallowed
        // by an absolute tolerance based on the other component's magnitude.
        [TestCase(0.0, 1e-20, 1e-20, 1.5707963267948966)]
        [TestCase(0.0, -1e-20, 1e-20, -1.5707963267948966)]
        [TestCase(0.5, 1e-20, 1.1547005383792515e-20, 1.0471975511965979)]
        [TestCase(-0.5, -1e-20, 1.1547005383792515e-20, -2.0943951023931957)]
        [TestCase(1.0, 1e-20, 1e-10, 1e-10)]
        [TestCase(-1.0, 1e-20, 1e-10, 3.141592653489793)]
        [TestCase(1.0, double.Epsilon, 2.2227587494850775e-162, 2.2227587494850775e-162)]
        [TestCase(-1.0, double.Epsilon, 2.2227587494850775e-162, 3.141592653589793)]
        [TestCase(1.0, -double.Epsilon, 2.2227587494850775e-162, -2.2227587494850775e-162)]
        [TestCase(-1.0, -double.Epsilon, 2.2227587494850775e-162, -3.141592653589793)]
        [TestCase(1.0, 1e-150, 1e-75, 1e-75)]
        [TestCase(-1.0, 1e-150, 1e-75, 3.141592653589793)]
        [TestCase(1.0, 9.999999999999999e-151, 1e-75, 1e-75)]
        [TestCase(-1.0, -9.999999999999999e-151, 1e-75, -3.141592653589793)]
        [TestCase(2.0, 1e-20, 1.3169578969248168, 5.7735026918962575e-21)]
        [TestCase(-2.0, -1e-20, 1.3169578969248168, -3.141592653589793)]
        [TestCase(1.0, 1.0, 1.0612750619050357, 0.9045568943023814)]
        [TestCase(1.0, -1.0, 1.0612750619050357, -0.9045568943023814)]
        [TestCase(-1.0, 1.0, 1.0612750619050357, 2.2370357592874117)]
        [TestCase(-1.0, -1.0, 1.0612750619050357, -2.2370357592874117)]
        [TestCase(2.3, 1.4, 1.6689791518994095, 0.5788429624121609)]
        [TestCase(-2.3, -1.4, 1.6689791518994095, -2.562749691177632)]
        [TestCase(1e200, 1e200, 461.55673936964905, 0.7853981633974483)]
        [TestCase(-1e308, 1e308, 710.235929413006, 2.356194490192345)]
        [TestCase(-1e308, -1e308, 710.235929413006, -2.356194490192345)]
        [TestCase(1e308, 1e308, 710.235929413006, 0.7853981633974483)]
        [TestCase(1e308, -1e308, 710.235929413006, -0.7853981633974483)]
        [TestCase(double.MaxValue, -double.MaxValue, 710.8224336642239, -0.7853981633974483)]
        [TestCase(1e150, 1e150, 346.4274847199468, 0.7853981633974483)]
        [TestCase(1.0000000000000002e150, 1e150, 346.4274847199468, 0.7853981633974482)]
        [TestCase(-1.0000000000000002e150, 1e150, 346.4274847199468, 2.356194490192345)]
        [TestCase(1.0000000000000002e150, -1e150, 346.4274847199468, -0.7853981633974482)]
        [TestCase(-1.0000000000000002e150, -1e150, 346.4274847199468, -2.356194490192345)]
        [TestCase(1.0000000000000002, 2.220446049250313e-16, 2.315303644582685e-8, 9.590301705980041e-9)]
        public void PrincipalValuesRetainFiniteAndSmallComponents(double real, double imaginary,
            double expectedReal, double expectedImaginary)
        {
            AssertComplex(_cosine(new Complex(real, imaginary)), new Complex(expectedReal, expectedImaginary));
        }

        [TestCase(-double.MaxValue)]
        [TestCase(-2.0)]
        [TestCase(-1.0)]
        [TestCase(-0.5)]
        [TestCase(0.0)]
        [TestCase(0.5)]
        [TestCase(1.0)]
        [TestCase(1.0000000000000002)]
        [TestCase(2.0)]
        [TestCase(double.MaxValue)]
        public void RealAxisRetainsBothSignedZeroSidesOfTheBranchCut(double real)
        {
            foreach (var side in new[] { 1d, -1d })
            {
                var zero = Math.CopySign(0d, side);
                var expected = real >= 1
                    ? new Complex(Math.Acosh(real), zero)
                    : real <= -1
                        ? new Complex(Math.Acosh(-real), Math.CopySign(Math.PI, side))
                        : new Complex(0d, Math.CopySign(Math.Acos(real), side));
                var actual = _cosine(new Complex(real, zero));
                AssertComplex(actual, expected);
                if (real >= 1)
                    NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits(actual.Imaginary),
                        Is.EqualTo(BitConverter.DoubleToInt64Bits(zero)));
                NUnit.Framework.Assert.That(actual.Real, Is.GreaterThanOrEqualTo(0));
            }
        }

        [TestCase(double.PositiveInfinity, 1d, 0d)]
        [TestCase(double.PositiveInfinity, -1d, -0d)]
        [TestCase(double.NegativeInfinity, 1d, Math.PI)]
        [TestCase(double.NegativeInfinity, -1d, -Math.PI)]
        [TestCase(1d, double.PositiveInfinity, Math.PI / 2)]
        [TestCase(1d, double.NegativeInfinity, -Math.PI / 2)]
        [TestCase(double.PositiveInfinity, double.PositiveInfinity, Math.PI / 4)]
        [TestCase(double.PositiveInfinity, double.NegativeInfinity, -Math.PI / 4)]
        [TestCase(double.NegativeInfinity, double.PositiveInfinity, 3 * Math.PI / 4)]
        [TestCase(double.NegativeInfinity, double.NegativeInfinity, -3 * Math.PI / 4)]
        public void InfiniteCoordinatesRetainTheQuadrantLimit(double real, double imaginary, double angle)
        {
            var actual = _cosine(new Complex(real, imaginary));
            NUnit.Framework.Assert.That(actual.Real, Is.EqualTo(double.PositiveInfinity));
            AssertScalar(actual.Imaginary, angle);
        }

        [TestCase(double.NaN, 0d)]
        [TestCase(0d, double.NaN)]
        [TestCase(double.NaN, double.NaN)]
        public void NaNWithoutAnInfiniteCoordinatePropagates(double real, double imaginary)
        {
            var actual = _cosine(new Complex(real, imaginary));
            NUnit.Framework.Assert.That(actual.Real, Is.NaN);
            NUnit.Framework.Assert.That(actual.Imaginary, Is.NaN);
        }

        [TestCase(0.2, 0.3)]
        [TestCase(-2.3, 1.4)]
        [TestCase(2.3, -1.4)]
        [TestCase(-2.3, -1.4)]
        public void TakingCoshRecoversOrdinaryComplexInputs(double real, double imaginary)
        {
            var input = new Complex(real, imaginary);
            AssertComplex(Complex.Cosh(_cosine(input)), input);
        }

        private static void AssertComplex(Complex actual, Complex expected)
        {
            AssertScalar(actual.Real, expected.Real);
            AssertScalar(actual.Imaginary, expected.Imaginary);
        }

        private static void AssertScalar(double actual, double expected)
        {
            NUnit.Framework.Assert.That(actual, Is.EqualTo(expected).Within(Math.Max(double.Epsilon, Math.Abs(expected) * 4e-15)));
        }
    }

    [TestFixture("Framework")]
    [TestFixture("Mono")]
    public class LegacyComplexInverseHyperbolicCosineTests
    {
        private readonly string _runtime;
        private NativeCompiler _compiler;
        private Func<Complex, Complex> _cosine;

        public LegacyComplexInverseHyperbolicCosineTests(string runtime)
        {
            _runtime = runtime;
        }

        [OneTimeSetUp]
        public void CompileTheCompleteHelperWithTheLegacyInverseCosineFormula()
        {
            _compiler = new NativeCompiler();
            _compiler.AddDll(typeof(GslConfig).Assembly.Location);
            _compiler.AddDll(typeof(LegacyComplexInverseCosine).Assembly.Location);
            // Only replace the runtime primitive. Exercise the actual complete
            // emitted helper, including its guards and branch selection.
            var source = ElementaryFunctions.ToCode.Replace("System.Numerics.Complex.Acos(",
                typeof(LegacyComplexInverseCosine).FullName + "." + _runtime + "Acos(");
            var assembly = _compiler.Compile("public static class EmittedLegacyFunctions {" + source + "}");
            _cosine = assembly.GetType("EmittedLegacyFunctions").GetMethod("arcosh", new[] { typeof(Complex) })
                .CreateDelegate<Func<Complex, Complex>>();
        }

        [OneTimeTearDown]
        public void ReleaseEmittedDefinitions()
        {
            _cosine = null;
            _compiler?.Dispose();
        }

        // Independent 800-digit references, including lower-half-plane inputs
        // where Framework's direct logarithmic inverse cosine cancels to zero.
        [TestCase(1.0, 1.0, 1.0612750619050357, 0.9045568943023814)]
        [TestCase(-1.0, 1.0, 1.0612750619050357, 2.2370357592874117)]
        [TestCase(1.0, -1.0, 1.0612750619050357, -0.9045568943023814)]
        [TestCase(-1.0, -1.0, 1.0612750619050357, -2.2370357592874117)]
        [TestCase(1e6, 1e6, 14.855231328804193, 0.7853981633975733)]
        [TestCase(1e6, -1e6, 14.855231328804193, -0.7853981633975733)]
        [TestCase(-1e6, -1e6, 14.855231328804193, -2.3561944901922197)]
        [TestCase(1e20, 1e20, 47.09142263072083, 0.7853981633974483)]
        [TestCase(-1e20, 1e20, 47.09142263072083, 2.356194490192345)]
        [TestCase(1e20, -1e20, 47.09142263072083, -0.7853981633974483)]
        [TestCase(-1e20, -1e20, 47.09142263072083, -2.356194490192345)]
        [TestCase(1e100, 1e100, 231.2982300702445, 0.7853981633974483)]
        [TestCase(-1e100, 1e100, 231.2982300702445, 2.356194490192345)]
        [TestCase(1e100, -1e100, 231.2982300702445, -0.7853981633974483)]
        [TestCase(-1e100, -1e100, 231.2982300702445, -2.356194490192345)]
        [TestCase(1e150, 1e150, 346.4274847199468, 0.7853981633974483)]
        [TestCase(-1e150, -1e150, 346.4274847199468, -2.356194490192345)]
        [TestCase(1.0000000000000002e150, -1e150, 346.4274847199468, -0.7853981633974482)]
        [TestCase(1e308, -1e308, 710.235929413006, -0.7853981633974483)]
        public void LegacyArithmeticRetainsFinitePrincipalValuesInEveryQuadrant(double real, double imaginary,
            double expectedReal, double expectedImaginary)
        {
            var result = _cosine(new Complex(real, imaginary));
            NUnit.Framework.Assert.That(result.Real, Is.EqualTo(expectedReal).Within(Math.Abs(expectedReal) * 4e-15));
            NUnit.Framework.Assert.That(result.Imaginary, Is.EqualTo(expectedImaginary).Within(Math.Abs(expectedImaginary) * 4e-15));
        }
    }

    // Arithmetic compatibility model, not a claim to execute the native runtime.
    // Preserve the reference-source Acos, polar Sqrt, scaled Abs and Log formulas:
    // https://github.com/microsoft/referencesource/blob/main/System.Numerics/System/Numerics/Complex.cs
    // https://github.com/mono/mono/blob/mono-6.12.0.206/mcs/class/referencesource/System.Numerics/System/Numerics/Complex.cs
    public static class LegacyComplexInverseCosine
    {
        public static Complex FrameworkAcos(Complex value)
        {
            return -Complex.ImaginaryOne * LegacyLog(value + Complex.ImaginaryOne * LegacySqrt(Complex.One - value * value));
        }

        public static Complex MonoAcos(Complex value)
        {
            if (value.Imaginary < 0 || (value.Imaginary == 0 && value.Real > 0))
                return Math.PI - MonoAcos(-value);
            return FrameworkAcos(value);
        }

        private static Complex LegacySqrt(Complex value)
        {
            var magnitude = Math.Sqrt(LegacyAbs(value));
            var phase = Math.Atan2(value.Imaginary, value.Real) / 2;
            return new Complex(magnitude * Math.Cos(phase), magnitude * Math.Sin(phase));
        }

        private static Complex LegacyLog(Complex value)
        {
            return new Complex(Math.Log(LegacyAbs(value)), Math.Atan2(value.Imaginary, value.Real));
        }

        private static double LegacyAbs(Complex value)
        {
            if (double.IsInfinity(value.Real) || double.IsInfinity(value.Imaginary))
                return double.PositiveInfinity;
            var real = Math.Abs(value.Real);
            var imaginary = Math.Abs(value.Imaginary);
            if (real > imaginary)
            {
                var ratio = imaginary / real;
                return real * Math.Sqrt(1 + ratio * ratio);
            }
            if (imaginary == 0)
                return real;
            var inverseRatio = real / imaginary;
            return imaginary * Math.Sqrt(1 + inverseRatio * inverseRatio);
        }
    }
}
