using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Computator.NET.Core.Compilation.Builtins;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Functions;
using NUnit.Framework;

namespace Computator.NET.Core.Tests
{
    [TestFixture, NonParallelizable]
    public class PrecompiledBuiltinCoulombTests
    {
        private sealed class LegacyEvaluator : ExpressionsEvaluator
        { protected override bool UsePrecompiledBuiltins => false; }
        private static Delegate Program(BaseFunction function) => (Delegate)typeof(BaseFunction)
            .GetField("_function", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(function);
        [OneTimeSetUp] public void Initialize() => GSLInitializer.Initialize();

        [Test]
        public void OnlyTheThreeReviewedCoulombScratchUsesDifferFromTheSourceFragment()
        {
            string expected = SpecialFunctions.ToCode.Replace("private static gsl_sf_result sfResult;", "");
            string[] calls = {
                "gsl_sf_coulomb_CL_e(L, η, out sfResult);",
                "gsl_sf_coulomb_wave_FG_e(η, ρ, L, L, out r1, out r2, out r3, out sfResult, out d1, out d2);",
                "gsl_sf_coulomb_wave_FG_e(η, ρ, L, L, out r1, out sfResult, out r3, out r2, out d1, out d2);"
            };
            foreach (string call in calls) expected = expected.Replace(call, "gsl_sf_result sfResult;\n            " + call);
            string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(expected))).ToLowerInvariant();
            NUnit.Framework.Assert.That(PrecompiledBuiltinSourceHashes.LocalizedSpecial, Is.EqualTo(hash));
            NUnit.Framework.Assert.That(typeof(PrecompiledExpressionSurface).GetField("sfResult", BindingFlags.NonPublic | BindingFlags.Static), Is.Null);
        }

        [Test]
        public void CoulombNormalizationIsIndependentAcrossConcurrentCalls()
        {
            var f = new ExpressionsEvaluator().Evaluate("CoulombC(0,x)", "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(Program(f).Method.DeclaringType.BaseType.IsGenericType, Is.True);
            Func<double,double> actual = f;
            Func<double,double> legacy = new LegacyEvaluator().Evaluate("CoulombC(0,x)", "", CalculationsMode.Real);
            for (int k = 1; k <= 64; k++) NUnit.Framework.Assert.That(actual(k/32.0), Is.EqualTo(legacy(k/32.0)));
            // DLMF 33.2.6 at l=0. Moderate positive eta avoids cancellation.
            Parallel.For(0, 4096, new ParallelOptions { MaxDegreeOfParallelism = 4 }, k =>
            {
                double eta = 0.125 + (k % 64) / 32.0;
                double t = 2*Math.PI*eta;
                Check(actual(eta), Math.Sqrt(t/(Math.Exp(t)-1)));
            });
            NUnit.Framework.Assert.That(actual(0), Is.EqualTo(1).Within(2e-13));
        }

        [TestCase("CoulombFprime(0,0,x)", false)]
        [TestCase("CoulombGprime(0,0,x)", true)]
        public void CoulombDerivativeScratchIsLocalWithoutChangingTheNativeOrderArguments(string source, bool irregular)
        {
            var f = new ExpressionsEvaluator().Evaluate(source, "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(Program(f).Method.DeclaringType.BaseType.IsGenericType, Is.True);
            Func<double,double> actual = f;
            Func<double,double> legacy = new LegacyEvaluator().Evaluate(source, "", CalculationsMode.Real);
            for (int k = 1; k <= 32; k++) NUnit.Framework.Assert.That(actual(k/8.0), Is.EqualTo(legacy(k/8.0)));
            Parallel.For(0, 4096, new ParallelOptions { MaxDegreeOfParallelism = 4 }, k =>
            {
                double x = 0.125 + (k % 64)/16.0;
                Check(actual(x), irregular ? -Math.Sin(x) : Math.Cos(x));
            });
        }

        private static void Check(double actual, double expected)
        {
            if (!double.IsFinite(actual) || Math.Abs(actual-expected) > 2e-13*Math.Max(1, Math.Abs(expected)))
                throw new InvalidOperationException("Concurrent Coulomb output differs from the independent identity.");
        }

        [Test]
        public void ExplicitAccessToTheLegacyScratchFieldRetainsItsOriginalSemantics()
        {
            var f = new ExpressionsEvaluator().Evaluate("CoulombC(0,0)+sfResult.val", "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(Program(f).Method.DeclaringType.BaseType.IsGenericType, Is.False);
            Func<double,double> actual = f;
            NUnit.Framework.Assert.That(actual(0), Is.EqualTo(2).Within(2e-13));
        }
    }
}
