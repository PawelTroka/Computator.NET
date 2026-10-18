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
    public class CouplingNineJTests
    {
        private delegate double NineJ(int ja, int jb, int jc, int jd, int je, int jf, int jg, int jh, int ji);
        private NativeCompiler _compiler;
        private NineJ _emitted;
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
            _emitted = assembly.GetType("EmittedFunctions").GetMethod("Coupling9j").CreateDelegate<NineJ>();
        }

        [OneTimeTearDown]
        public void ReleaseTheEmittedHelper()
        {
            _emitted = null;
            _compiler?.Dispose();
            if (_gsl != IntPtr.Zero) NativeLibrary.Free(_gsl);
        }

        // Arguments are twice the physical spins, in row order. The constants
        // below are independent Wigner values, not calls back into GSL:
        // https://dlmf.nist.gov/34.7.E1 reduces ji=0 cases to a 6-j symbol;
        // https://dlmf.nist.gov/34.4.E2 gives their finite factorial sums.
        // The fully aligned four-spin-1/2 state has recoupling overlap one,
        // yielding 1/sqrt(3*3*3*3) = 1/9 from the four triplet multiplicities.
        [TestCase(0, 0, 0, 0, 0, 0, 0, 0, 0, 1.0)]
        [TestCase(2, 2, 2, 2, 2, 2, 2, 2, 0, 1.0 / 18)]
        [TestCase(2, 2, 2, 2, 2, 2, 2, 2, 2, 0.0)]
        [TestCase(1, 1, 2, 1, 1, 2, 2, 2, 0, -1.0 / 18)]
        [TestCase(1, 1, 2, 1, 1, 2, 2, 2, 2, 0.0)]
        [TestCase(1, 1, 2, 1, 1, 2, 2, 2, 4, 1.0 / 9)]
        [TestCase(1, 1, 0, 1, 1, 0, 0, 0, 0, 0.5)]
        [TestCase(2, 2, 0, 2, 2, 0, 0, 0, 0, 1.0 / 3)]
        [TestCase(4, 4, 0, 4, 4, 0, 0, 0, 0, 0.2)]
        public void HostAndEmittedHelpersMatchIndependentWignerValues(
            int ja, int jb, int jc, int jd, int je, int jf, int jg, int jh, int ji, double expected)
        {
            foreach (var function in new NineJ[] { SpecialFunctions.Coupling9j, _emitted })
                NUnit.Framework.Assert.That(function(ja, jb, jc, jd, je, jf, jg, jh, ji),
                    Is.EqualTo(expected).Within(1e-13), function.Method.DeclaringType.FullName);
        }

        // With jg=jh=0, the bottom row permits only ji=0. Changing just ji
        // used to retain the allowed value 1/2 instead of enforcing this rule.
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        public void TheNinthArgumentDeterminesWhetherTheBottomRowCanCouple(int ji)
        {
            foreach (var function in new NineJ[] { SpecialFunctions.Coupling9j, _emitted })
                NUnit.Framework.Assert.That(function(1, 1, 0, 1, 1, 0, 0, 0, ji),
                    Is.EqualTo(0.0).Within(1e-13), function.Method.DeclaringType.FullName);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        public void NegativeSpinsKeepTheExistingNaNContract(int position)
        {
            var arguments = new int[9];
            arguments[position] = -1;
            foreach (var function in new NineJ[] { SpecialFunctions.Coupling9j, _emitted })
                NUnit.Framework.Assert.That(function(arguments[0], arguments[1], arguments[2],
                    arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8]),
                    Is.NaN, function.Method.DeclaringType.FullName);
        }
    }
}
