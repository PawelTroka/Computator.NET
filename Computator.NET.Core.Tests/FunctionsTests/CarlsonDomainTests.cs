using System;
using System.Runtime.InteropServices;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Compilation.Builtins;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes.Configuration;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture("host")]
    [TestFixture("generated")]
    [TestFixture("precompiled expression")]
    [TestFixture("precompiled script")]
    [NonParallelizable]
    public class CarlsonDomainTests
    {
        private readonly string _surface;
        private NativeCompiler _compiler;
        private IntPtr _gsl;
        private Func<double, double, double> _c;
        private Func<double, double, double, double> _d;
        private Func<double, double, double, double> _f;
        private Func<double, double, double, double, double> _j;

        public CarlsonDomainTests(string surface) => _surface = surface;

        [OneTimeSetUp]
        public void LoadTheProductionHostOrShippedTslDefinitions()
        {
            GSLInitializer.Initialize();
            var type = typeof(SpecialFunctions);
            if (_surface == "precompiled expression")
                type = typeof(PrecompiledExpressionSurface);
            else if (_surface == "precompiled script")
                type = typeof(PrecompiledScriptMathSurface);
            else if (_surface == "generated")
            {
                _compiler = new NativeCompiler();
                _compiler.AddDll(typeof(GslConfig).Assembly.Location);
                var assembly = _compiler.Compile("public static class EmittedFunctions {" + SpecialFunctions.ToCode + "}");
                _gsl = NativeLibrary.Load(GslConfig.GslLibraryName);
                NativeLibrary.SetDllImportResolver(assembly,
                    (name, _, _) => name == GslConfig.GslDllName ? _gsl : IntPtr.Zero);
                type = assembly.GetType("EmittedFunctions");
            }

            _c = type.GetMethod("CarlsonC").CreateDelegate<Func<double, double, double>>();
            _d = type.GetMethod("CarlsonD").CreateDelegate<Func<double, double, double, double>>();
            _f = type.GetMethod("CarlsonF").CreateDelegate<Func<double, double, double, double>>();
            _j = type.GetMethod("CarlsonJ").CreateDelegate<Func<double, double, double, double, double>>();
        }

        [OneTimeTearDown]
        public void ReleaseTheEmittedDefinitions()
        {
            _c = null;
            _d = _f = null;
            _j = null;
            _compiler?.Dispose();
            if (_gsl != IntPtr.Zero) NativeLibrary.Free(_gsl);
        }

        // Substituting t=s*u*u into the defining integrals gives these exact
        // boundary values; expected results do not call another Carlson backend.
        // https://dlmf.nist.gov/19.16.E1, E2, E5 and E6.
        [TestCase(0.25, false)]
        [TestCase(0.25, true)]
        [TestCase(1.0, false)]
        [TestCase(1.0, true)]
        [TestCase(4.0, false)]
        [TestCase(4.0, true)]
        [TestCase(256.0, false)]
        [TestCase(256.0, true)]
        public void AdmissibleZeroArgumentsReturnTheCompleteIntegral(double scale, bool negativeZero)
        {
            var zero = negativeZero ? BitConverter.Int64BitsToDouble(long.MinValue) : 0.0;
            var firstKind = Math.PI / (2 * Math.Sqrt(scale));
            var secondKind = 3 * Math.PI / (4 * scale * Math.Sqrt(scale));

            AssertClose(_c(zero, scale), firstKind);
            AssertClose(_d(zero, scale, scale), secondKind);
            AssertClose(_d(scale, zero, scale), secondKind);
            AssertClose(_f(zero, scale, scale), firstKind);
            AssertClose(_f(scale, zero, scale), firstKind);
            AssertClose(_f(scale, scale, zero), firstKind);
            AssertClose(_j(zero, scale, scale, scale), secondKind);
            AssertClose(_j(scale, zero, scale, scale), secondKind);
            AssertClose(_j(scale, scale, zero, scale), secondKind);
        }

        [Test]
        public void ThePositiveInteriorKeepsItsNormalizedValue()
        {
            AssertClose(_c(1, 1), 1);
            AssertClose(_d(1, 1, 1), 1);
            AssertClose(_f(1, 1, 1), 1);
            AssertClose(_j(1, 1, 1, 1), 1);
        }

        [TestCase(0.0, 0.0)]
        [TestCase(1.0, 0.0)]
        [TestCase(-1.0, 1.0)]
        [TestCase(1.0, -1.0)]
        public void CStillRejectsSingularAndNegativeArguments(double x, double y) =>
            NUnit.Framework.Assert.That(_c(x, y), Is.NaN);

        [TestCase(0.0, 0.0, 1.0)]
        [TestCase(0.0, 1.0, 0.0)]
        [TestCase(1.0, 0.0, 0.0)]
        [TestCase(1.0, 1.0, 0.0)]
        [TestCase(-1.0, 1.0, 1.0)]
        [TestCase(1.0, -1.0, 1.0)]
        [TestCase(1.0, 1.0, -1.0)]
        public void DStillRequiresPositiveZAndNotBothOtherArgumentsZero(double x, double y, double z) =>
            NUnit.Framework.Assert.That(_d(x, y, z), Is.NaN);

        [TestCase(0.0, 0.0, 1.0)]
        [TestCase(0.0, 1.0, 0.0)]
        [TestCase(1.0, 0.0, 0.0)]
        [TestCase(0.0, 0.0, 0.0)]
        [TestCase(-1.0, 1.0, 1.0)]
        [TestCase(1.0, -1.0, 1.0)]
        [TestCase(1.0, 1.0, -1.0)]
        public void FStillRejectsTwoZerosAndNegativeArguments(double x, double y, double z) =>
            NUnit.Framework.Assert.That(_f(x, y, z), Is.NaN);

        [TestCase(0.0, 0.0, 1.0, 1.0)]
        [TestCase(0.0, 1.0, 0.0, 1.0)]
        [TestCase(1.0, 0.0, 0.0, 1.0)]
        [TestCase(0.0, 1.0, 1.0, 0.0)]
        [TestCase(1.0, 1.0, 1.0, 0.0)]
        [TestCase(-1.0, 1.0, 1.0, 1.0)]
        [TestCase(1.0, -1.0, 1.0, 1.0)]
        [TestCase(1.0, 1.0, -1.0, 1.0)]
        [TestCase(1.0, 1.0, 1.0, -1.0)]
        public void JStillRequiresPositivePAndAtMostOneOtherZero(double x, double y, double z, double p) =>
            NUnit.Framework.Assert.That(_j(x, y, z, p), Is.NaN);

        private static void AssertClose(double actual, double expected) =>
            NUnit.Framework.Assert.That(actual, Is.EqualTo(expected).Within(2e-13 * Math.Abs(expected)));
    }
}
