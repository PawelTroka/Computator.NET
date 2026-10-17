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
    public class IncompleteBetaTests
    {
        private NativeCompiler _compiler;
        private Func<double, double, double, double> _emittedBeta;
        private Func<double, double, double, double> _emittedAlias;
        private Func<double, double, double, double> _emittedNormalized;
        private IntPtr _gsl;

        [OneTimeSetUp]
        public void CompileTheEmittedHelpers()
        {
            GSLInitializer.Initialize();
            _compiler = new NativeCompiler();
            _compiler.AddDll(typeof(GslConfig).Assembly.Location);
            var assembly = _compiler.Compile("public static class EmittedFunctions {" + SpecialFunctions.ToCode + "}");
            // Emitted helpers have their own assembly, so bind its imports to the
            // same native library that the application has already initialized.
            _gsl = NativeLibrary.Load(GslConfig.GslLibraryName);
            NativeLibrary.SetDllImportResolver(assembly,
                (name, _, _) => name == GslConfig.GslDllName ? _gsl : IntPtr.Zero);
            var type = assembly.GetType("EmittedFunctions");
            var arguments = new[] { typeof(double), typeof(double), typeof(double) };
            _emittedBeta = type.GetMethod("Beta", arguments).CreateDelegate<Func<double, double, double, double>>();
            _emittedAlias = type.GetMethod("β", arguments).CreateDelegate<Func<double, double, double, double>>();
            _emittedNormalized = type.GetMethod("BetaNorm", arguments).CreateDelegate<Func<double, double, double, double>>();
        }

        [OneTimeTearDown]
        public void ReleaseTheEmittedHelpers()
        {
            _emittedBeta = null;
            _emittedAlias = null;
            _emittedNormalized = null;
            _compiler?.Dispose();
            if (_gsl != IntPtr.Zero) NativeLibrary.Free(_gsl);
        }

        // Analytic values: for a=b=1, B_x=x; for a=2,b=3 the
        // antiderivative is x²/2 - 2x³/3 + x⁴/4 and B(2,3)=1/12.
        [TestCase(0.5, 1.0, 1.0, 0.5, 0.5)]
        [TestCase(0.0, 2.0, 3.0, 0.0, 0.0)]
        [TestCase(1.0, 2.0, 3.0, 1.0 / 12, 1.0)]
        [TestCase(0.5, 2.0, 3.0, 11.0 / 192, 11.0 / 16)]
        [TestCase(0.25, 2.0, 3.0, 67.0 / 3072, 67.0 / 256)]
        [TestCase(0.5, 3.0, 2.0, 5.0 / 192, 5.0 / 16)]
        [TestCase(0.25, 0.5, 1.0, 1.0, 0.5)]
        public void BothExecutionPathsUseTheFirstArgumentAsTheIntegrationLimit(
            double x, double a, double b, double incomplete, double normalized)
        {
            foreach (var function in new Func<double, double, double, double>[]
                { SpecialFunctions.Beta, SpecialFunctions.β, _emittedBeta, _emittedAlias })
                NUnit.Framework.Assert.That(function(x, a, b), Is.EqualTo(incomplete).Within(1e-12), function.Method.Name);
            foreach (var function in new Func<double, double, double, double>[]
                { SpecialFunctions.BetaNorm, _emittedNormalized })
                NUnit.Framework.Assert.That(function(x, a, b), Is.EqualTo(normalized).Within(1e-12), function.Method.Name);
        }

        [TestCase(-0.1, 2.0, 3.0)]
        [TestCase(1.1, 2.0, 3.0)]
        [TestCase(0.5, 0.0, 3.0)]
        [TestCase(0.5, 2.0, 0.0)]
        [TestCase(0.5, -1.0, 3.0)]
        [TestCase(0.5, 2.0, -1.0)]
        public void ExistingInvalidDomainChecksArePreserved(double x, double a, double b)
        {
            foreach (var function in new Func<double, double, double, double>[]
                { SpecialFunctions.Beta, SpecialFunctions.β, SpecialFunctions.BetaNorm,
                  _emittedBeta, _emittedAlias, _emittedNormalized })
                NUnit.Framework.Assert.That(function(x, a, b), Is.NaN, function.Method.Name);
        }

        [Test]
        public void CompleteBetaOverloadsKeepTheirExistingMeaning()
        {
            NUnit.Framework.Assert.That(SpecialFunctions.Beta(2, 3), Is.EqualTo(1.0 / 12).Within(1e-12));
            NUnit.Framework.Assert.That(SpecialFunctions.β(2, 3), Is.EqualTo(1.0 / 12).Within(1e-12));
        }
    }
}
