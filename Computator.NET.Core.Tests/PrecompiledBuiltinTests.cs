using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Compilation.Builtins;
using Computator.NET.Core.Constants;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Computator.NET.Core.NumericalCalculations;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Functions;
using NUnit.Framework;

namespace Computator.NET.Core.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class PrecompiledBuiltinTests
    {
        private sealed class LegacyEvaluator : ExpressionsEvaluator
        { protected override bool UsePrecompiledBuiltins => false; }
        private sealed class LegacyScriptEvaluator : ScriptEvaluator
        { protected override bool UsePrecompiledBuiltins => false; }

        [OneTimeSetUp]
        public void Initialize() => GSLInitializer.Initialize();

        private static Delegate Program(BaseFunction function) => (Delegate)typeof(BaseFunction)
            .GetField("_function", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(function);
        private static bool UsesFacade(BaseFunction function) => Program(function).Method.DeclaringType.BaseType.IsGenericType;
        private static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

        [Test]
        public void GeneratedFacadesComeFromTheExactShippedSourceFragments()
        {
            string[] common = { MathematicalConstants.ToCode, PhysicalConstants.ToCode, ElementaryFunctions.ToCode,
                SpecialFunctions.ToCode, StatisticsFunctions.ToCode, Integral.ToCode, Derivative.ToCode, FunctionRoot.ToCode };
            NUnit.Framework.Assert.That(PrecompiledBuiltinSourceHashes.Expression, Is.EqualTo(Hash(string.Join("\0", common))));
            NUnit.Framework.Assert.That(PrecompiledBuiltinSourceHashes.ScriptMath,
                Is.EqualTo(Hash(string.Join("\0", common.Concat(new[] { MatrixFunctions.ToCode })))));
            NUnit.Framework.Assert.That(PrecompiledBuiltinSourceHashes.NumericalExtensions,
                Is.EqualTo(Hash(Computator.NET.Core.Evaluation.NumericalExtensions.ToCode)));
        }

        [TestCase("sin(x)+cos(x)+x*x/4")]
        [TestCase("sin(x)+x²/4")]
        [TestCase("sqrt(x*x+1)")]
        [TestCase("gamma(x+3)")]
        [TestCase("BesselJ0(x)")]
        [TestCase("exp(x)+ln(x*x+1)")]
        [TestCase("sinc(x)")]
        [TestCase("abs(x)+cosh(x)")]
        [TestCase("sinᶻ(x)")]
        public void RealExpressionsRetainTheLegacyResultOrDiagnostic(string source)
        {
            Function fast = null, legacy = null;
            Exception fastError = null, legacyError = null;
            try { fast = new ExpressionsEvaluator().Evaluate(source, "", CalculationsMode.Real); } catch (Exception e) { fastError = e; }
            try { legacy = new LegacyEvaluator().Evaluate(source, "", CalculationsMode.Real); } catch (Exception e) { legacyError = e; }
            NUnit.Framework.Assert.That(fastError?.GetType(), Is.EqualTo(legacyError?.GetType()));
            if (fastError != null) return;
            NUnit.Framework.Assert.That(UsesFacade(fast), Is.True, source);
            Func<double, double> actual = fast, expected = legacy;
            foreach (double x in new[] { -1.0, -0.0, 0.0, 0.25, 0.75, 1.0 })
                NUnit.Framework.Assert.That(actual(x), Is.EqualTo(expected(x)), source + " at " + x.ToString("R", CultureInfo.InvariantCulture));
        }

        [Test]
        public void OrdinaryRealExpressionHasAnIndependentOracle()
        {
            var function = new ExpressionsEvaluator().Evaluate("sin(x)+cos(x)+x*x/4", "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(UsesFacade(function), Is.True);
            Func<double, double> evaluate = function;
            for (int k = 0; k < 1024; k++)
            {
                double x = (k - 512) / 256.0;
                NUnit.Framework.Assert.That(evaluate(x), Is.EqualTo(Math.Sin(x) + Math.Cos(x) + x*x/4));
            }
            // A real structural reduction, not a timing assertion or a cache hit.
            var creator = Program(function).Method.DeclaringType;
            NUnit.Framework.Assert.That(creator.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Length, Is.EqualTo(1));
        }

        [Test]
        public void ComplexExpressionsRetainValues()
        {
            var fast = new ExpressionsEvaluator().Evaluate("sin(z)+z*z", "", CalculationsMode.Complex);
            Func<Complex, Complex> actual = fast, expected = new LegacyEvaluator().Evaluate("sin(z)+z*z", "", CalculationsMode.Complex);
            NUnit.Framework.Assert.That(UsesFacade(fast), Is.True);
            foreach (var z in new[] { Complex.Zero, new Complex(0.25, 0.5), new Complex(-1, 2) })
            {
                NUnit.Framework.Assert.That(actual(z), Is.EqualTo(expected(z)));
                NUnit.Framework.Assert.That(actual(z), Is.EqualTo(Complex.Sin(z)+z*z));
            }
        }

        [Test]
        public void SurfaceAndImplicitCoordinatesRetainValues()
        {
            Func<double, double, double> surface = new ExpressionsEvaluator().Evaluate("sin(x)*cos(y)+x*y", "", CalculationsMode.Fxy);
            NUnit.Framework.Assert.That(surface(0.25, 0.5), Is.EqualTo(Math.Sin(0.25)*Math.Cos(0.5)+0.125));
            Func<double, double, double> implicitFunction = new ExpressionsEvaluator().Evaluate("x²+y²=1", "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(implicitFunction(1, 0), Is.EqualTo(0));
        }

        [Test]
        public void NoncollidingCustomFunctionsAndStaticStateAreIsolated()
        {
            const string custom = "public static real counter=0; public static real advance(real x) { counter+=1; return counter+x; }";
            var evaluator = new ExpressionsEvaluator();
            var first = evaluator.Evaluate("advance(x)", custom, CalculationsMode.Real);
            var second = evaluator.Evaluate("advance(x)", custom, CalculationsMode.Real);
            NUnit.Framework.Assert.That(UsesFacade(first), Is.True);
            Func<double, double> a = first, b = second;
            NUnit.Framework.Assert.That(a(0), Is.EqualTo(1));
            NUnit.Framework.Assert.That(a(0), Is.EqualTo(2));
            NUnit.Framework.Assert.That(b(0), Is.EqualTo(1));
            NUnit.Framework.Assert.That(Program(first).Method.Module.Assembly, Is.Not.SameAs(Program(second).Method.Module.Assembly));
            NUnit.Framework.Assert.That(Program(first).Method.Module.Assembly.FullName, Is.Not.EqualTo(Program(second).Method.Module.Assembly.FullName));
        }

        [Test]
        public void CustomOverloadsKeepTheOriginalMergedOverloadGroup()
        {
            const string custom = "public static real sin(string value) { return 17; }";
            var fast = new ExpressionsEvaluator().Evaluate("sin(x)+sin(\"text\")", custom, CalculationsMode.Real);
            Func<double, double> actual = fast, expected = new LegacyEvaluator().Evaluate("sin(x)+sin(\"text\")", custom, CalculationsMode.Real);
            NUnit.Framework.Assert.That(UsesFacade(fast), Is.False);
            NUnit.Framework.Assert.That(actual(0.25), Is.EqualTo(expected(0.25)));
        }

        [Test]
        public void DuplicateBuiltinSignatureStillFails()
        {
            const string custom = "public static real sin(real x) { return x; }";
            NUnit.Framework.Assert.Throws<CompilationException>(() => new ExpressionsEvaluator().Evaluate("sin(x)", custom, CalculationsMode.Real));
        }

        [TestCase("m_random != null ? 1.0 : 0.0")]
        [TestCase("typeof(FunctionsCreator).GetMethods().Length")]
        public void PrivateHelperAccessAndGeneratedTypeInspectionUseCompatibilityPath(string source)
        {
            var fast = new ExpressionsEvaluator().Evaluate(source, "", CalculationsMode.Real);
            Func<double, double> actual = fast, expected = new LegacyEvaluator().Evaluate(source, "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(UsesFacade(fast), Is.False);
            NUnit.Framework.Assert.That(actual(0), Is.EqualTo(expected(0)));
        }

        [TestCase("write(sin(0));")]
        [TestCase("real sum=0; for(integer k=1;k<=1000;k++){sum+=k*k;} write(sum);")]
        [TestCase("var m=IdentityMatrix<real>(2,2); write(Tr(pow(m,2))+pow(2,3));")]
        [TestCase("write(\"Polski: 2x x² ∞\");")]
        public void ScriptsRetainOutputAndUseThePrecompiledMathSurface(string script)
        {
            var fast = new ScriptEvaluator().Evaluate(script, "");
            var legacy = new LegacyScriptEvaluator().Evaluate(script, "");
            var actual = new List<string>(); var expected = new List<string>();
            fast.Evaluate(actual.Add); legacy.Evaluate(expected.Add);
            NUnit.Framework.Assert.That(actual, Is.EqualTo(expected));
            NUnit.Framework.Assert.That(UsesFacade(fast), Is.True);
        }

        [Test]
        public void ScriptCallbacksRemainOwnedByEachProgram()
        {
            var first = new ScriptEvaluator().Evaluate("write(1);", "");
            var second = new ScriptEvaluator().Evaluate("write(2);", "");
            var a = new List<string>(); var b = new List<string>();
            first.Evaluate(a.Add); second.Evaluate(b.Add); first.Evaluate(a.Add);
            NUnit.Framework.Assert.That(a, Is.EqualTo(new[] { "1", "1" }));
            NUnit.Framework.Assert.That(b, Is.EqualTo(new[] { "2" }));
        }

        [Test]
        public void ChangingCustomDefinitionsCannotReturnAStaleProgram()
        {
            var evaluator = new ExpressionsEvaluator();
            for (int k = 1; k <= 4; k++)
            {
                Func<double, double> f = evaluator.Evaluate("shifted(x)", "public static real shifted(real x) { return x+" + k + "; }", CalculationsMode.Real);
                NUnit.Framework.Assert.That(f(0.25), Is.EqualTo(0.25+k));
            }
            NUnit.Framework.Assert.Throws<CompilationException>(() => evaluator.Evaluate("unknown(x)", "", CalculationsMode.Real));
        }

        [Test]
        public void GenericFacadeDoesNotRootCollectiblePrograms()
        {
            var reference = CreateUnrootedProgram();
            for (int i = 0; i < 20 && reference.IsAlive; i++)
            {
                GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            }
            NUnit.Framework.Assert.That(reference.IsAlive, Is.False);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference CreateUnrootedProgram()
        {
            var function = new ExpressionsEvaluator().Evaluate("sin(x)", "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(UsesFacade(function), Is.True);
            var context = AssemblyLoadContext.GetLoadContext(Program(function).Method.Module.Assembly);
            NUnit.Framework.Assert.That(context.IsCollectible, Is.True);
            return new WeakReference(context);
        }
    }
}
