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
        public void OnlyTheReviewedCoulombScratchLocalityDiffersFromTheSourceFragment()
        {
            string expected = SpecialFunctions.ToCode.Replace("private static gsl_sf_result sfResult;", "")
                .Replace("gsl_sf_coulomb_CL_e(L, η, out sfResult);",
                    "gsl_sf_result sfResult;\n            gsl_sf_coulomb_CL_e(L, η, out sfResult);");
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
            for (int k = 1; k <= 64; k++)
            {
                double eta = k / 32.0;
                NUnit.Framework.Assert.That(actual(eta), Is.EqualTo(legacy(eta)));
            }
            // DLMF 33.2.6 at l=0: C_0(eta)^2 = 2*pi*eta/(exp(2*pi*eta)-1).
            // Moderate positive eta avoids cancellation in this independent oracle.
            Parallel.For(0, 4096, new ParallelOptions { MaxDegreeOfParallelism = 4 }, k =>
            {
                double eta = 0.125 + (k % 64) / 32.0;
                double t = 2*Math.PI*eta;
                double expected = Math.Sqrt(t/(Math.Exp(t)-1));
                double value = actual(eta);
                if (!double.IsFinite(value) || Math.Abs(value-expected) > 2e-13*Math.Max(1, Math.Abs(expected)))
                    throw new InvalidOperationException("Concurrent Coulomb output differs from the independent identity.");
            });
            NUnit.Framework.Assert.That(actual(0), Is.EqualTo(1).Within(2e-13));
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
