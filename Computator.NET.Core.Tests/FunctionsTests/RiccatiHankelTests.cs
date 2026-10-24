using System;
using System.Numerics;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Functions;
using Computator.NET.DataTypes.Configuration;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture(false, false), TestFixture(false, true)]
    [TestFixture(true, false), TestFixture(true, true)]
    public class RiccatiHankelTests
    {
        private readonly bool _emitted;
        private readonly bool _secondKind;
        private NativeCompiler _compiler;
        private Func<int, double, Complex> _function;

        public RiccatiHankelTests(bool emitted, bool secondKind)
        {
            _emitted = emitted;
            _secondKind = secondKind;
        }

        [OneTimeSetUp]
        public void CompileTheActualEmittedHelpers()
        {
            if (!_emitted)
            {
                _function = _secondKind ? SpecialFunctions.RiccatiBesselζ : SpecialFunctions.RiccatiBesselξ;
                return;
            }
            _compiler = new NativeCompiler();
            _compiler.AddDll(typeof(GslConfig).Assembly.Location);
            var assembly = _compiler.Compile("public static class EmittedFunctions {" + SpecialFunctions.ToCode + "}");
            var type = assembly.GetType("EmittedFunctions");
            _function = type.GetMethod(_secondKind ? "RiccatiBesselζ" : "RiccatiBesselξ")
                .CreateDelegate<Func<int, double, Complex>>();
        }

        [OneTimeTearDown]
        public void ReleaseTheEmittedHelpers()
        {
            _function = null;
            _compiler?.Dispose();
        }

        // Independent elementary formulas for x*j_n and -x*y_n, from
        // https://dlmf.nist.gov/10.49.E3 and https://dlmf.nist.gov/10.49.E5.
        // In particular, xi_0 = sin(x)-i*cos(x), zeta_0 = sin(x)+i*cos(x).
        // Negative orders use j_{-m-1}=(-1)^(m+1)*y_m and
        // y_{-m-1}=(-1)^m*j_m, from the half-integer relations in
        // https://dlmf.nist.gov/10.47.E3 and https://dlmf.nist.gov/10.47.E4.
        [TestCase(-3, 2.0)]
        [TestCase(-2, 2.0)]
        [TestCase(-1, 2.0)]
        [TestCase(0, 0.5)]
        [TestCase(0, 2.0)]
        [TestCase(0, 5.0)]
        [TestCase(1, 0.5)]
        [TestCase(1, 2.0)]
        [TestCase(1, 5.0)]
        [TestCase(2, 0.5)]
        [TestCase(2, 2.0)]
        [TestCase(2, 5.0)]
        public void HostAndEmittedHelpersMatchIndependentSphericalValues(int n, double x)
        {
            var sine = Math.Sin(x);
            var cosine = Math.Cos(x);
            var order = n < 0 ? -n - 1 : n;
            var s = order switch
            {
                0 => sine,
                1 => sine / x - cosine,
                _ => (3 / (x * x) - 1) * sine - 3 * cosine / x
            };
            var c = order switch
            {
                0 => cosine,
                1 => cosine / x + sine,
                _ => (3 / (x * x) - 1) * cosine + 3 * sine / x
            };
            if (n < 0)
            {
                var sign = order % 2 == 0 ? 1 : -1;
                (s, c) = (sign * c, -sign * s);
            }
            AssertComponents(n, x, s, _secondKind ? c : -c);
        }

        // Values follow the DLMF elementary formulas and small-argument
        // series, evaluated with 180-digit decimal arithmetic at these exact
        // binary64 arguments. In these cases x*h_n is representable although
        // h_n itself can overflow. A tiny regular component may round to zero.
        [TestCase(0, double.Epsilon, double.Epsilon, 1.0)]
        [TestCase(-1, double.Epsilon, 1.0, -double.Epsilon)]
        [TestCase(0, 1e-309, 1e-309, 1.0)]
        [TestCase(-1, 1e-309, 1.0, -1e-309)]
        [TestCase(1, 1e-160, 3.335e-321, 1e160)]
        [TestCase(-2, 1e-160, -1e160, 3.335e-321)]
        [TestCase(1, 6e-309, 0.0, 1.6666666666666664e308)]
        [TestCase(-2, 6e-309, -1.6666666666666664e308, 0.0)]
        [TestCase(2, 1e-100, 6.666666666666667e-302, 3e200)]
        [TestCase(-3, 1e-100, 3e200, -6.666666666666667e-302)]
        [TestCase(2, 1e-120, 0.0, 3.0000000000000002e240)]
        [TestCase(-3, 1e-120, 3.0000000000000002e240, 0.0)]
        [TestCase(3, 1e-100, 0.0, 1.5e301)]
        [TestCase(-4, 1e-100, -1.5e301, 0.0)]
        [TestCase(6, 1e-50, 0.0, 1.0395e304)]
        [TestCase(-7, 1e-50, 1.0395e304, 0.0)]
        public void TinyPositiveArgumentsKeepRepresentableScaledValues(int n, double x, double s, double c)
        {
            AssertScaledComponents(n, x, s, _secondKind ? c : -c);
        }

        // Independent 180-digit evaluations of the sine/cosine formulas at
        // the switch and its immediately adjacent binary64 values. Direct
        // double evaluation of S_1/S_2/S_3 would cancel away these components.
        [TestCase(0, 9.999999999999999e-9, 9.999999999999999e-9, 1.0)]
        [TestCase(1, 9.999999999999999e-9, 3.333333333333332e-17, 100000000.00000001)]
        [TestCase(2, 9.999999999999999e-9, 6.666666666666664e-26, 3.000000000000001e16)]
        [TestCase(3, 9.999999999999999e-9, 9.523809523809518e-35, 1.5000000000000007e25)]
        [TestCase(0, 1e-8, 1e-8, 1.0)]
        [TestCase(1, 1e-8, 3.3333333333333335e-17, 1e8)]
        [TestCase(2, 1e-8, 6.666666666666667e-26, 3e16)]
        [TestCase(3, 1e-8, 9.523809523809525e-35, 1.4999999999999998e25)]
        [TestCase(-2, 1e-8, -1e8, 3.3333333333333335e-17)]
        [TestCase(-3, 1e-8, 3e16, -6.666666666666667e-26)]
        [TestCase(-4, 1e-8, -1.4999999999999998e25, 9.523809523809525e-35)]
        [TestCase(0, 1.0000000000000002e-8, 1.0000000000000002e-8, 1.0)]
        [TestCase(1, 1.0000000000000002e-8, 3.333333333333335e-17, 99999999.99999999)]
        [TestCase(2, 1.0000000000000002e-8, 6.666666666666671e-26, 2.9999999999999988e16)]
        [TestCase(3, 1.0000000000000002e-8, 9.523809523809531e-35, 1.4999999999999992e25)]
        public void TinyArgumentSwitchKeepsAccurateValuesOnBothSides(int n, double x, double s, double c)
        {
            AssertScaledComponents(n, x, s, _secondKind ? c : -c);
        }

        [TestCase(int.MinValue, 0.0)]
        [TestCase(-3, 0.0)]
        [TestCase(-2, 0.0)]
        [TestCase(-1, 0.0)]
        [TestCase(0, 0.0)]
        [TestCase(1, 0.0)]
        [TestCase(2, 0.0)]
        [TestCase(0, -0.5)]
        [TestCase(1, -0.5)]
        [TestCase(2, -0.5)]
        public void NonpositiveArgumentsKeepTheExistingNaNContract(int n, double x)
        {
            var actual = _function(n, x);
            NUnit.Framework.Assert.That(actual.Real, Is.NaN);
            NUnit.Framework.Assert.That(actual.Imaginary, Is.NaN);
        }

        private void AssertScaledComponents(int n, double x, double real, double imaginary)
        {
            var actual = _function(n, x);
            var context = _function.Method.DeclaringType.FullName + "." + _function.Method.Name;
            AssertScaledComponent(actual.Real, real, context + " real part");
            AssertScaledComponent(actual.Imaginary, imaginary, context + " imaginary part");
        }

        private static void AssertScaledComponent(double actual, double expected, string context)
        {
            if (expected == 0 || Math.Abs(expected) == double.Epsilon)
                NUnit.Framework.Assert.That(actual, Is.EqualTo(expected), context);
            else
                NUnit.Framework.Assert.That(actual,
                    Is.EqualTo(expected).Within(Math.Max(double.Epsilon, Math.Abs(expected) * 8e-15)), context);
        }

        private void AssertComponents(int n, double x, double real, double imaginary)
        {
            var actual = _function(n, x);
            var context = _function.Method.DeclaringType.FullName + "." + _function.Method.Name;
            NUnit.Framework.Assert.That(actual.Real, Is.EqualTo(real).Within(1e-12), context + " real part");
            NUnit.Framework.Assert.That(actual.Imaginary, Is.EqualTo(imaginary).Within(1e-12), context + " imaginary part");
        }
    }
}
