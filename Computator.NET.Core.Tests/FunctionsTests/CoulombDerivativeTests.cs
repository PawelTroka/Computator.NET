using System;
using System.Runtime.InteropServices;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes.Configuration;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture, NonParallelizable]
    public class CoulombDerivativeTests
    {
        private delegate double CoulombDerivative(int order, double eta, double rho);
        private NativeCompiler _compiler;
        private CoulombDerivative _emitted;
        private IntPtr _gsl;

        [OneTimeSetUp]
        public void CompileTheActualEmittedHelper()
        {
            GSLInitializer.Initialize();
            _compiler = new NativeCompiler();
            _compiler.AddDll(typeof(GslConfig).Assembly.Location);
            var assembly = _compiler.Compile("public static class EmittedFunctions {" + SpecialFunctions.ToCode + "}");
            _gsl = NativeLibrary.Load(GslConfig.GslLibraryName);
            NativeLibrary.SetDllImportResolver(assembly,
                (name, _, _) => name == GslConfig.GslDllName ? _gsl : IntPtr.Zero);
            _emitted = assembly.GetType("EmittedFunctions").GetMethod("CoulombGprime")
                .CreateDelegate<CoulombDerivative>();
        }

        [OneTimeTearDown]
        public void ReleaseTheEmittedHelper()
        {
            _emitted = null;
            _compiler?.Dispose();
            if (_gsl != IntPtr.Zero) NativeLibrary.Free(_gsl);
        }

        // DLMF 33.5.E3 gives G_L(0,rho) = -rho*y_L(rho). Expanding the
        // first three spherical Neumann functions (DLMF 10.49.E5) and
        // differentiating gives the elementary expressions below. These
        // expectations do not call GSL or the application's Bessel helpers.
        // Several radii matter: at L=1,rho=1 the wrong and right orders coincide.
        [TestCase(0, 0.5)]
        [TestCase(0, 1.5)]
        [TestCase(0, 3.0)]
        [TestCase(1, 0.5)]
        [TestCase(1, 1.5)]
        [TestCase(1, 3.0)]
        [TestCase(2, 0.5)]
        [TestCase(2, 1.5)]
        [TestCase(2, 3.0)]
        public void HostAndEmittedHelpersDifferentiateTheRequestedOrderAtZeroCharge(int order, double rho)
        {
            var sine = Math.Sin(rho);
            var cosine = Math.Cos(rho);
            var expected = order == 0 ? -sine
                : order == 1 ? cosine - sine / rho - cosine / (rho * rho)
                : (3 / rho - 6 / (rho * rho * rho)) * cosine + (1 - 6 / (rho * rho)) * sine;

            foreach (var function in new CoulombDerivative[] { SpecialFunctions.CoulombGprime, _emitted })
                NUnit.Framework.Assert.That(function(order, 0, rho),
                    Is.EqualTo(expected).Within(2e-12 * Math.Max(1, Math.Abs(expected))),
                    function.Method.DeclaringType.FullName);
        }

        // W{G_L,F_L} = G_L*F'_L - G'_L*F_L = 1 (DLMF 33.2.E12).
        // F_L and G_L come from Meta.Numerics, independently of the GSL
        // derivative routine. The identity must use the same order throughout.
        [TestCase(0, 1.0, 2.0)]
        [TestCase(1, -1.0, 0.75)]
        [TestCase(1, 0.5, 3.0)]
        [TestCase(2, 2.0, 6.0)]
        [TestCase(3, -0.5, 4.0)]
        public void NonzeroChargeDerivativesSatisfyTheIndependentWronskian(int order, double eta, double rho)
        {
            var regular = SpecialFunctions.CoulombF(order, eta, rho);
            var irregular = SpecialFunctions.CoulombG(order, eta, rho);
            var regularDerivative = SpecialFunctions.CoulombFprime(order, eta, rho);

            foreach (var function in new CoulombDerivative[] { SpecialFunctions.CoulombGprime, _emitted })
                NUnit.Framework.Assert.That(irregular * regularDerivative - regular * function(order, eta, rho),
                    Is.EqualTo(1.0).Within(2e-11), function.Method.DeclaringType.FullName);
        }

        [TestCase(-1, 0.0, 1.0)]
        [TestCase(0, 1.0, 0.0)]
        [TestCase(2, -0.5, -1.0)]
        public void ExistingInvalidDomainChecksArePreserved(int order, double eta, double rho)
        {
            foreach (var function in new CoulombDerivative[] { SpecialFunctions.CoulombGprime, _emitted })
                NUnit.Framework.Assert.That(function(order, eta, rho), Is.NaN,
                    function.Method.DeclaringType.FullName);
        }
    }
}
