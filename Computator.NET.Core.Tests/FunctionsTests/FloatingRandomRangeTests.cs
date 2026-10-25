#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Loader;
using Computator.NET.Core.Functions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture(false)]
    [TestFixture(true)]
    public class FloatingRandomRangeTests
    {
        private readonly bool _generated;
        private Func<double, double, double> _real = null!;
        private Func<Complex, Complex, Complex> _complex = null!;
        private AssemblyLoadContext? _context;

        public FloatingRandomRangeTests(bool generated)
        {
            _generated = generated;
        }

        [OneTimeSetUp]
        public void SelectHostOrCompileTheCompleteEmittedStatisticsFunctions()
        {
            _real = StatisticsFunctions.random;
            _complex = StatisticsFunctions.random;
            if (!_generated) return;

            var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")
                ?? throw new InvalidOperationException("Runtime reference metadata is unavailable."))
                .Split(Path.PathSeparator)
                .Concat(new[] { typeof(StatisticsFunctions).Assembly.Location,
                    typeof(Computator.NET.Core.Compatibility.FrameworkRandom).Assembly.Location })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(path => MetadataReference.CreateFromFile(path));
            var compilation = CSharpCompilation.Create("EmittedRandomFunctions." + Guid.NewGuid().ToString("N"),
                new[] { CSharpSyntaxTree.ParseText("public static class EmittedRandomFunctions {" + StatisticsFunctions.ToCode + "}") },
                references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var stream = new MemoryStream();
            var result = compilation.Emit(stream);
            NUnit.Framework.Assert.That(result.Success, Is.True,
                string.Join(Environment.NewLine, result.Diagnostics));
            stream.Position = 0;
            _context = new AssemblyLoadContext("EmittedRandomFunctions", isCollectible: true);
            var type = _context.LoadFromStream(stream).GetType("EmittedRandomFunctions", throwOnError: true)!;
            _real = type.GetMethod("random", new[] { typeof(double), typeof(double) })!
                .CreateDelegate<Func<double, double, double>>();
            _complex = type.GetMethod("random", new[] { typeof(Complex), typeof(Complex) })!
                .CreateDelegate<Func<Complex, Complex, Complex>>();
        }

        [OneTimeTearDown]
        public void ReleaseEmittedFunctions()
        {
            _real = null!;
            _complex = null!;
            _context?.Unload();
            _context = null;
        }

        // These bounds test support, not statistical convergence. On the old
        // implementation every sample from an overflowing span is nonfinite,
        // including a possible zero random factor (infinity times zero).
        [TestCase(-double.MaxValue, double.MaxValue)]
        [TestCase(double.MaxValue, -double.MaxValue)]
        [TestCase(-1e308, 1e308)]
        [TestCase(1e308, -1e308)]
        [TestCase(-double.MaxValue, 1e307)]
        [TestCase(double.MaxValue, -1e307)]
        [TestCase(1e308, double.MaxValue)]
        [TestCase(-double.MaxValue, -1e308)]
        [TestCase(-3.0, 7.0)]
        [TestCase(7.0, -3.0)]
        [TestCase(-double.Epsilon, double.Epsilon)]
        public void FiniteRealEndpointsProduceOnlyFiniteValuesBetweenTheEndpoints(double start, double end)
        {
            for (var i = 0; i < 64; i++) AssertInInterval(_real(start, end), start, end);
        }

        [TestCase(double.MaxValue)]
        [TestCase(-double.MaxValue)]
        [TestCase(double.Epsilon)]
        [TestCase(0.0)]
        public void DegenerateRealIntervalRetainsItsOnlyPoint(double endpoint)
        {
            NUnit.Framework.Assert.That(_real(endpoint, endpoint), Is.EqualTo(endpoint));
        }

        private static IEnumerable<TestCaseData> ComplexSegments
        {
            get
            {
                yield return new TestCaseData(new Complex(-double.MaxValue, -double.MaxValue),
                    new Complex(double.MaxValue, double.MaxValue));
                yield return new TestCaseData(new Complex(double.MaxValue, -double.MaxValue),
                    new Complex(-double.MaxValue, double.MaxValue));
                yield return new TestCaseData(new Complex(-double.MaxValue, 4), new Complex(double.MaxValue, 4));
                yield return new TestCaseData(new Complex(4, -double.MaxValue), new Complex(4, double.MaxValue));
                yield return new TestCaseData(new Complex(1e308, -1e308), new Complex(double.MaxValue, -double.MaxValue));
                yield return new TestCaseData(new Complex(-2, 3), new Complex(6, -5));
            }
        }

        [TestCaseSource(nameof(ComplexSegments))]
        public void FiniteComplexEndpointsProduceOnlyFiniteCoordinates(Complex start, Complex end)
        {
            for (var i = 0; i < 64; i++)
            {
                var value = _complex(start, end);
                AssertInInterval(value.Real, start.Real, end.Real);
                AssertInInterval(value.Imaginary, start.Imaginary, end.Imaginary);
            }
        }

        [TestCase(1.0)]
        [TestCase(-1.0)]
        public void ExtremeComplexSamplesRemainOnTheLineSegmentInsteadOfFillingARectangle(double direction)
        {
            var start = new Complex(-double.MaxValue, -direction * double.MaxValue);
            var end = -start;
            for (var i = 0; i < 64; i++)
            {
                var value = _complex(start, end);
                AssertInInterval(value.Real, start.Real, end.Real);
                AssertInInterval(value.Imaginary, start.Imaginary, end.Imaginary);
                NUnit.Framework.Assert.That(value.Imaginary, Is.EqualTo(direction * value.Real));
            }
        }

        [Test]
        public void OneOverflowingCoordinateUsesTheSameInterpolationFactorAsTheOtherCoordinate()
        {
            var start = new Complex(-double.MaxValue, 0);
            var end = new Complex(double.MaxValue, 1);
            for (var i = 0; i < 64; i++)
            {
                var value = _complex(start, end);
                AssertInInterval(value.Real, start.Real, end.Real);
                AssertInInterval(value.Imaginary, start.Imaginary, end.Imaginary);
                // The segment equation is Re/MaxValue = 2*Im - 1.
                NUnit.Framework.Assert.That(value.Real / double.MaxValue,
                    Is.EqualTo(2 * value.Imaginary - 1).Within(4e-16));
            }
        }

        [Test]
        public void DegenerateComplexIntervalRetainsItsOnlyPoint()
        {
            var endpoint = new Complex(double.MaxValue, -double.MaxValue);
            NUnit.Framework.Assert.That(_complex(endpoint, endpoint), Is.EqualTo(endpoint));
        }

        private static void AssertInInterval(double value, double start, double end)
        {
            NUnit.Framework.Assert.That(double.IsNaN(value) || double.IsInfinity(value), Is.False);
            NUnit.Framework.Assert.That(value, Is.InRange(Math.Min(start, end), Math.Max(start, end)));
        }
    }
}
