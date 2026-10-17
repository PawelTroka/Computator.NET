using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes.Configuration;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture, NonParallelizable]
    public class Coupling9jTests
    {
        private Func<int, int, int, int, int, int, int, int, int, double> _generated;
        private AssemblyLoadContext _generatedContext;

        [OneTimeSetUp]
        public void InitializeHostAndCompileTheShippingGeneratedFunctions()
        {
            GSLInitializer.Initialize();
            using (var compiler = new NativeCompiler())
            {
                compiler.AddDll(typeof(GslErrorBoundary).Assembly.Location);
                var assembly = compiler.Compile("public static class CouplingProbe {" + SpecialFunctions.ToCode + "}");
                _generatedContext = AssemblyLoadContext.GetLoadContext(assembly);
                _generated = assembly.GetType("CouplingProbe").GetMethod(nameof(SpecialFunctions.Coupling9j))
                    .CreateDelegate<Func<int, int, int, int, int, int, int, int, int, double>>();
            }
        }

        [OneTimeTearDown]
        public void ReleaseTheGeneratedFunctions()
        {
            _generated = null;
            _generatedContext?.Unload();
            _generatedContext = null;
        }

        public static IEnumerable<TestCaseData> KnownSymbols()
        {
            // Values independently evaluated exactly with SymPy 1.14.0 wigner_9j,
            // after dividing each argument by two to obtain the physical spin.
            // Definition: https://dlmf.nist.gov/34.6.E2
            // Oracle: https://docs.sympy.org/latest/modules/physics/wigner.html
            // These constants do not call the native implementation being tested.
            var cases = new[]
            {
                (new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 1.0, "ZeroSpins"),
                (new[] { 0, 0, 0, 0, 0, 0, 0, 0, 2 }, 0.0, "ForbiddenFinalSpin"),
                (new[] { 1, 1, 0, 1, 1, 0, 0, 0, 0 }, 0.5, "HalfIntegerSinglets"),
                (new[] { 2, 2, 2, 2, 2, 2, 2, 2, 0 }, 1.0 / 18, "IntegerFinalSpinZero"),
                (new[] { 2, 2, 2, 2, 2, 2, 2, 2, 2 }, 0.0, "IntegerFinalSpinOne"),
                (new[] { 2, 2, 2, 2, 2, 2, 2, 2, 4 }, 1.0 / 18, "IntegerFinalSpinTwo"),
                (new[] { 1, 1, 2, 1, 1, 2, 2, 2, 0 }, -1.0 / 18, "HalfIntegerFinalSpinZero"),
                (new[] { 1, 1, 2, 1, 1, 2, 2, 2, 2 }, 0.0, "HalfIntegerFinalSpinOne"),
                (new[] { 1, 1, 2, 1, 1, 2, 2, 2, 4 }, 1.0 / 9, "HalfIntegerFinalSpinTwo"),
                (new[] { 1, 2, 3, 2, 1, 3, 3, 3, 0 }, 1.0 / 48, "AsymmetricRows"),
                (new[] { 1, 1, 2, 2, 2, 2, 1, 1, 0 }, Math.Sqrt(6) / 18, "IrrationalCoefficient"),
                (new[] { 1, 1, 2, 1, 1, 0, 2, 0, 2 }, 1.0 / 6, "DifferentSixthAndNinthSpins")
            };

            foreach (var generated in new[] { false, true })
            foreach (var (arguments, expected, name) in cases)
                yield return new TestCaseData(arguments, expected, generated)
                    .SetName($"Coupling9j_{(generated ? "Generated" : "Host")}_{name}");
        }

        [TestCaseSource(nameof(KnownSymbols))]
        public void UsesAllNineSpinsForTheWignerSymbol(int[] arguments, double expected, bool generated)
        {
            NUnit.Framework.Assert.That(Evaluate(arguments, generated), Is.EqualTo(expected).Within(2e-14));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RejectsANegativeNinthSpin(bool generated)
        {
            NUnit.Framework.Assert.That(Evaluate(new[] { 0, 0, 0, 0, 0, 0, 0, 0, -1 }, generated), Is.NaN);
        }

        private double Evaluate(int[] arguments, bool generated)
        {
            var function = generated ? _generated : SpecialFunctions.Coupling9j;
            return function(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4],
                arguments[5], arguments[6], arguments[7], arguments[8]);
        }
    }
}
