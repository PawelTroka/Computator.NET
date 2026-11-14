#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Loader;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Compilation.Builtins;
using Computator.NET.DataTypes.Configuration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture(0)]
    [TestFixture(1)]
    [TestFixture(2)]
    public class ScaledComplementaryErrorTests
    {
        private readonly int _surface;
        private sealed class ProgramTag { }
        private Func<double, double> _real = null!;
        private Func<Complex, Complex> _complex = null!;
        private AssemblyLoadContext? _context;

        public ScaledComplementaryErrorTests(int surface) => _surface = surface;

        [OneTimeSetUp]
        public void SelectHostEmittedOrPrecompiledFunctions()
        {
            _real = SpecialFunctions.erfcx;
            _complex = SpecialFunctions.erfcx;
            if (_surface == 2)
            {
                _real = PrecompiledExpressionBuiltins<ProgramTag>.erfcx;
                _complex = PrecompiledExpressionBuiltins<ProgramTag>.erfcx;
            }
            if (_surface != 1) return;

            // Compile the complete shipping fragment, including its real and
            // complex overloads. None of these tests invokes a native GSL entry.
            var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")
                ?? throw new InvalidOperationException("Runtime reference metadata is unavailable."))
                .Split(Path.PathSeparator)
                .Concat(new[] { typeof(GslErrorBoundary).Assembly.Location,
                    typeof(Meta.Numerics.Functions.AdvancedMath).Assembly.Location,
                    typeof(MathNet.Numerics.SpecialFunctions).Assembly.Location,
                    typeof(Accord.Math.Gabor).Assembly.Location })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(path => MetadataReference.CreateFromFile(path));
            var compilation = CSharpCompilation.Create("EmittedScaledError." + Guid.NewGuid().ToString("N"),
                new[] { CSharpSyntaxTree.ParseText("public static class EmittedScaledError {" + SpecialFunctions.ToCode + "}") },
                references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var stream = new MemoryStream();
            var result = compilation.Emit(stream);
            NUnit.Framework.Assert.That(result.Success, Is.True,
                string.Join(Environment.NewLine, result.Diagnostics));
            stream.Position = 0;
            _context = new AssemblyLoadContext("EmittedScaledError", isCollectible: true);
            var type = _context.LoadFromStream(stream).GetType("EmittedScaledError", throwOnError: true)!;
            _real = type.GetMethod("erfcx", new[] { typeof(double) })!
                .CreateDelegate<Func<double, double>>();
            _complex = type.GetMethod("erfcx", new[] { typeof(Complex) })!
                .CreateDelegate<Func<Complex, Complex>>();
        }

        [OneTimeTearDown]
        public void ReleaseEmittedFunctions()
        {
            _real = null!;
            _complex = null!;
            _context?.Unload();
            _context = null;
        }

        // Independent fixtures: exp(x*x)*erfc(x) at 120 decimal digits using
        // the exact binary64 arguments (mpmath), rounded once. For x >= 1e20,
        // DLMF 7.12.1 bounds the relative correction to 1/(sqrt(pi)*x) by 5e-41.
        [TestCase(0.0, 1.0)]
        [TestCase(0.1, 0.8964569799691267)]
        [TestCase(0.5, 0.6156903441929259)]
        [TestCase(1.0, 0.427583576155807)]
        [TestCase(1.9, 0.26650937366167265)]
        [TestCase(1.9999999999999998, 0.25539567631050575)]
        [TestCase(2.0, 0.25539567631050575)]
        [TestCase(2.0000000000000004, 0.2553956763105057)]
        [TestCase(7.0, 0.07980005432915294)]
        [TestCase(10.0, 0.05614099274382259)]
        [TestCase(26.0, 0.021683584850562907)]
        [TestCase(27.0, 0.02088160799042094)]
        [TestCase(27.2, 0.02072827435234785)]
        [TestCase(30.0, 0.01879588886141675)]
        [TestCase(100.0, 0.005641613782989433)]
        [TestCase(1000.0, 0.0005641893014533876)]
        [TestCase(1e20, 5.6418958354775626e-21)]
        [TestCase(1e100, 5.6418958354775625e-101)]
        [TestCase(1e200, 5.641895835477563e-201)]
        [TestCase(double.MaxValue, 3.138408733985445e-309)]
        [TestCase(-0.5, 1.952360489182557)]
        [TestCase(-1.0, 5.008980080762283)]
        [TestCase(-2.0, 108.94090438997797)]
        [TestCase(-10.0, 5.376234283632271e43)]
        [TestCase(-20.0, 1.0442939379528289e174)]
        [TestCase(-26.0, 7.657724931490568e293)]
        [TestCase(-26.5, 1.924553162418569e305)]
        public void RealValuesRetainAccuracyBeforeAndAfterTheUnscaledTailUnderflows(double x, double expected)
        {
            AssertClose(_real(x), expected);
        }

        // Independent complex fixtures use the same 120-digit definition.
        // Huge right-half-plane cases use its leading asymptotic term; omitted
        // corrections are far below binary64 resolution. Both half-planes and
        // conjugate values exercise the library's reflection conventions.
        [TestCase(0.5, 0.5, 0.533156707912175, -0.2304882313844584)]
        [TestCase(1.0, 1.0, 0.3047442052569126, -0.20821893820283163)]
        [TestCase(2.0, 3.0, 0.09271076642644334, -0.12831696222826158)]
        [TestCase(-1.0, 1.0, -1.1370378783511974, -2.026813791854195)]
        [TestCase(0.0, 5.0, 1.3887943864964021e-11, -0.11524596183093659)]
        [TestCase(0.0, 30.0, 0.0, -0.018816784868660726)]
        [TestCase(5.0, 0.1, 0.11066424464977836, -0.0021325263291299993)]
        [TestCase(10.0, 1.0, 0.05559831964105537, -0.005506079556625048)]
        [TestCase(30.0, 1.0, 0.018775085315541475, -0.0006251434914879298)]
        [TestCase(1.0, 30.0, 0.0006272253836101256, -0.01879584239989071)]
        [TestCase(-1.0, 30.0, -0.0006272253836101256, -0.01879584239989071)]
        [TestCase(-20.0, 1.0, -2.562204431790986e173, -2.8625330472762664e173)]
        [TestCase(1e100, 1e100, 2.8209479177387813e-101, -2.8209479177387813e-101)]
        [TestCase(double.MaxValue, 1.0, 3.138408733985445e-309, 0.0)]
        public void ComplexValuesAvoidCancellationAndUnscaledExponentialOverflow(
            double real, double imaginary, double expectedReal, double expectedImaginary)
        {
            foreach (var sign in new[] { 1.0, -1.0 })
            {
                var actual = _complex(new Complex(real, sign * imaginary));
                AssertClose(actual.Real, expectedReal);
                AssertClose(actual.Imaginary, sign * expectedImaginary);
            }
        }

        [TestCase(double.PositiveInfinity, 0.0)]
        [TestCase(double.NegativeInfinity, double.PositiveInfinity)]
        [TestCase(-27.0, double.PositiveInfinity)]
        public void RealLimitsAndGenuineOverflowHaveTheirExpectedValues(double x, double expected)
        {
            NUnit.Framework.Assert.That(_real(x), Is.EqualTo(expected));
            var complex = _complex(new Complex(x, 0.0));
            NUnit.Framework.Assert.That(complex.Real, Is.EqualTo(expected));
            NUnit.Framework.Assert.That(complex.Imaginary, Is.Zero);
        }

        [Test]
        public void BothSignedZerosHaveTheExactNormalization()
        {
            foreach (var zero in new[] { 0.0, -0.0 })
            {
                NUnit.Framework.Assert.That(_real(zero), Is.EqualTo(1.0));
                NUnit.Framework.Assert.That(_complex(new Complex(zero, zero)), Is.EqualTo(Complex.One));
            }
        }

        [Test]
        public void PurelyRealComplexArgumentsAgreeWithTheRealOverload()
        {
            foreach (var x in new[] { -26.5, -1.0, 0.1, 1.9, 2.0, 7.0, 27.0, 30.0, 1e200, double.MaxValue })
            {
                var actual = _complex(new Complex(x, 0.0));
                NUnit.Framework.Assert.That(actual.Real, Is.EqualTo(_real(x)));
                NUnit.Framework.Assert.That(actual.Imaginary, Is.Zero);
            }
        }

        [Test]
        public void RealNaNRemainsNaN()
        {
            NUnit.Framework.Assert.That(_real(double.NaN), Is.NaN);
            NUnit.Framework.Assert.That(double.IsNaN(_complex(new Complex(double.NaN, 0.0)).Real), Is.True);
        }

        private static void AssertClose(double actual, double expected)
        {
            NUnit.Framework.Assert.That(double.IsFinite(actual), Is.True);
            NUnit.Framework.Assert.That(actual, Is.EqualTo(expected)
                .Within(3e-14 * Math.Abs(expected) + 4 * double.Epsilon));
        }
    }
}
