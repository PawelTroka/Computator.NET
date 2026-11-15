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
    public class PochhammerDomainTests
    {
        private NativeCompiler _compiler;
        private Func<double, double, double> _emittedPochhammer;
        private Func<double, double, double> _emittedRelative;
        private IntPtr _gsl;

        [OneTimeSetUp]
        public void CompileTheActualEmittedHelpers()
        {
            GSLInitializer.Initialize();
            _compiler = new NativeCompiler();
            _compiler.AddDll(typeof(GslConfig).Assembly.Location);
            var assembly = _compiler.Compile("public static class EmittedFunctions {" + SpecialFunctions.ToCode + "}");
            _gsl = NativeLibrary.Load(GslConfig.GslLibraryName);
            NativeLibrary.SetDllImportResolver(assembly,
                (name, _, _) => name == GslConfig.GslDllName ? _gsl : IntPtr.Zero);
            var functions = assembly.GetType("EmittedFunctions");
            _emittedPochhammer = functions.GetMethod("Pochhammer").CreateDelegate<Func<double, double, double>>();
            _emittedRelative = functions.GetMethod("PochhammerRelative").CreateDelegate<Func<double, double, double>>();
        }

        [OneTimeTearDown]
        public void ReleaseTheEmittedHelpers()
        {
            _emittedPochhammer = null;
            _emittedRelative = null;
            _compiler?.Dispose();
            if (_gsl != IntPtr.Zero) NativeLibrary.Free(_gsl);
        }

        // Gamma(a)/Gamma(a)=1. For negative integer orders, the gamma
        // recurrence gives reciprocals of descending factors of a-1.
        [TestCase(0.5, 0, 1)]
        [TestCase(1, 0, 1)]
        [TestCase(3, 0, 1)]
        [TestCase(100, 0, 1)]
        [TestCase(3, -1, 0.5)]
        [TestCase(3, -2, 0.5)]
        [TestCase(5, -3, 1.0 / 24)]
        [TestCase(0.5, -1, -2)]
        [TestCase(0.5, -2, 4.0 / 3)]
        [TestCase(3, 2, 12)]
        public void ValidOrdersMatchGammaRecurrence(double a, double x, double expected)
        {
            foreach (var function in new[] { (Func<double, double, double>)SpecialFunctions.Pochhammer, _emittedPochhammer })
                NUnit.Framework.Assert.That(function(a, x), Is.EqualTo(expected).Within(1e-12));
        }

        // The x=0 limit is digamma(a): psi(1)=-EulerGamma and
        // psi(a+1)=psi(a)+1/a. Nonzero x uses ((a)_x-1)/x.
        [TestCase(1, 0, -0.5772156649015329)]
        [TestCase(2, 0, 0.4227843350984671)]
        [TestCase(3, 0, 0.9227843350984671)]
        [TestCase(0.5, 0, -1.9635100260214235)]
        [TestCase(3, -1, 0.5)]
        [TestCase(3, -2, 0.25)]
        [TestCase(5, -3, 23.0 / 72)]
        [TestCase(0.5, -1, 3)]
        [TestCase(3, 2, 5.5)]
        public void RelativeFormIncludesItsZeroOrderLimit(double a, double x, double expected)
        {
            foreach (var function in new[] { (Func<double, double, double>)SpecialFunctions.PochhammerRelative, _emittedRelative })
                NUnit.Framework.Assert.That(function(a, x), Is.EqualTo(expected).Within(1e-12));
        }

        [TestCase(3, -3)]
        [TestCase(3, -4)]
        [TestCase(-0.5, 0.5)]
        [TestCase(-3, 0)]
        public void ExistingShiftedArgumentPoleGuardsRemain(double a, double x)
        {
            foreach (var function in new[] { (Func<double, double, double>)SpecialFunctions.Pochhammer,
                SpecialFunctions.PochhammerRelative, _emittedPochhammer, _emittedRelative })
                NUnit.Framework.Assert.That(function(a, x), Is.NaN);
        }
    }
}
