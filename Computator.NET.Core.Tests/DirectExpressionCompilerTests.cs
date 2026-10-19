using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Functions;
using NUnit.Framework;

namespace Computator.NET.Core.Tests
{
    [TestFixture, NonParallelizable]
    public class DirectExpressionCompilerTests
    {
        private sealed class RoslynEvaluator : ExpressionsEvaluator { protected override bool UseDirectExpressions => false; }
        private static Delegate Program(BaseFunction value) => (Delegate)typeof(BaseFunction).GetField("_function", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(value);
        private static bool Direct(BaseFunction value) => Program(value).Method.Module.Assembly.GetName().Name.StartsWith("Computator.Direct.", StringComparison.Ordinal);
        private static void EqualBits(double actual, double expected, string context)
        {
            if (double.IsNaN(expected)) NUnit.Framework.Assert.That(double.IsNaN(actual), Is.True, context);
            else NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits(actual), Is.EqualTo(BitConverter.DoubleToInt64Bits(expected)), context);
        }
        [OneTimeSetUp] public void Initialize() => GSLInitializer.Initialize();

        [TestCase("sin(x)+cos(x)+x*x/4")]
        [TestCase("x")]
        [TestCase("-x")]
        [TestCase("+x")]
        [TestCase("x*0")]
        [TestCase("x/-0")]
        [TestCase("x/-0.0")]
        [TestCase("x/(x-x)")]
        [TestCase("1/2")]
        [TestCase("1/-2")]
        [TestCase("0")]
        [TestCase("-0")]
        [TestCase("-0.0")]
        [TestCase("x+2147483647")]
        [TestCase("1.25e-3*x+.5")]
        [TestCase("sin(cos(x))")]
        [TestCase("exp(x)+ln(abs(x)+1)")]
        [TestCase("sinh(x)-cosh(x)")]
        [TestCase("x/(2.0+3.0)")]
        [TestCase("x + - 2")]
        public void RealSubsetHasBitExactRoslynResultsIncludingIeeeEdges(string source)
        {
            var value = new ExpressionsEvaluator().Evaluate(source, "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(Direct(value), Is.True, source);
            Func<double, double> actual = value, expected = new RoslynEvaluator().Evaluate(source, "", CalculationsMode.Real);
            foreach (double x in new[] { -3.0, -.25, -0.0, 0.0, .25, 1.0, 3.0, double.Epsilon, -double.Epsilon, double.MaxValue, double.PositiveInfinity, double.NegativeInfinity, double.NaN })
                EqualBits(actual(x), expected(x), source + ": " + x.ToString("R", CultureInfo.InvariantCulture));
        }

        [TestCase("sin(z)+z*z")]
        [TestCase("z+2")]
        [TestCase("2-z")]
        [TestCase("z*3")]
        [TestCase("-z")]
        [TestCase("x+y")]
        [TestCase("Re(z)+Im(z)")]
        [TestCase("cos(z)*sin(z)")]
        public void ComplexSubsetUsesTheSameRuntimeOperators(string source)
        {
            var value = new ExpressionsEvaluator().Evaluate(source, "", CalculationsMode.Complex);
            NUnit.Framework.Assert.That(Direct(value), Is.True, source);
            Func<Complex, Complex> actual = value, expected = new RoslynEvaluator().Evaluate(source, "", CalculationsMode.Complex);
            foreach (var z in new[] { Complex.Zero, new Complex(-0.0, -0.0), new Complex(.25, .5), new Complex(-2, 3), new Complex(double.PositiveInfinity, 0), new Complex(0, double.NaN) })
            {
                var a = actual(z); var b = expected(z);
                EqualBits(a.Real, b.Real, source + " real"); EqualBits(a.Imaginary, b.Imaginary, source + " imaginary");
            }
        }

        [Test]
        public void VariedSurfaceGridHasAnIndependentOracle()
        {
            var value = new ExpressionsEvaluator().Evaluate("sin(x)*cos(y)+x*y", "", CalculationsMode.Fxy);
            NUnit.Framework.Assert.That(Direct(value), Is.True);
            Func<double, double, double> actual = value;
            for (int x = -16; x <= 16; x++) for (int y = -16; y <= 16; y++)
                EqualBits(actual(x/8.0, y/8.0), Math.Sin(x/8.0)*Math.Cos(y/8.0)+(x/8.0)*(y/8.0), "grid");
        }

        [TestCase("sqrt(x*x+1)")]
        [TestCase("x²")]
        [TestCase("2x")]
        [TestCase("abs(x)>0 ? x : 0")]
        [TestCase("Math.Sin(x)")]
        [TestCase("x+(1+2)")]
        [TestCase("x+(2*3)")]
        [TestCase("x+2147483648")]
        [TestCase("x+1.0f")]
        [TestCase("sin(x) /* a comment */")]
        [TestCase("x + true")]
        [TestCase("x +")]
        [TestCase("unknown(x)")]
        [TestCase("x--1")]
        [TestCase("1e309")]
        public void OutsideTheSubsetRetainsTheCompleteCompilerResultOrDiagnostic(string source)
        {
            Function actual = null, expected = null; Exception a = null, b = null;
            try { actual = new ExpressionsEvaluator().Evaluate(source, "", CalculationsMode.Real); } catch (Exception e) { a = e; }
            try { expected = new RoslynEvaluator().Evaluate(source, "", CalculationsMode.Real); } catch (Exception e) { b = e; }
            NUnit.Framework.Assert.That(a?.GetType(), Is.EqualTo(b?.GetType()), source);
            if (a != null) { NUnit.Framework.Assert.That(a.Message, Is.EqualTo(b.Message), source); return; }
            NUnit.Framework.Assert.That(Direct(actual), Is.False, source);
            Func<double, double> f = actual, g = expected;
            EqualBits(f(.5), g(.5), source);
        }

        [Test]
        public void CustomDefinitionsAreNeverBypassedAndChangingDefinitionsCannotUseStaleCode()
        {
            var evaluator = new ExpressionsEvaluator();
            for (int k = 1; k <= 3; k++)
            {
                string definitions = "public static real shifted(real x) { return x+" + k + "; }";
                var result = evaluator.Evaluate("shifted(x)", definitions, CalculationsMode.Real);
                NUnit.Framework.Assert.That(Direct(result), Is.False);
                Func<double, double> f = result; EqualBits(f(.5), k+.5, "custom");
            }
            NUnit.Framework.Assert.Throws<CompilationException>(() => evaluator.Evaluate("sin(x)", "public static real sin(real x){return x;}", CalculationsMode.Real));
            NUnit.Framework.Assert.That(Direct(evaluator.Evaluate("sin(x)", "", CalculationsMode.Real)), Is.True);
        }

        [Test]
        public void DirectProgramsRemainUniqueAndCollectibleWithoutAnyOutputCache()
        {
            var first = new ExpressionsEvaluator().Evaluate("sin(x)", "", CalculationsMode.Real);
            var second = new ExpressionsEvaluator().Evaluate("sin(x)", "", CalculationsMode.Real);
            var a = Program(first).Method.Module.Assembly; var b = Program(second).Method.Module.Assembly;
            NUnit.Framework.Assert.That(a.FullName, Is.Not.EqualTo(b.FullName));
            NUnit.Framework.Assert.That(AssemblyLoadContext.GetLoadContext(a).IsCollectible, Is.True);
            var weak = Unrooted();
            for (int i = 0; i < 20 && weak.IsAlive; i++) { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
            NUnit.Framework.Assert.That(weak.IsAlive, Is.False);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference Unrooted()
        {
            var function = new ExpressionsEvaluator().Evaluate("sin(x)+x/3", "", CalculationsMode.Real);
            return new WeakReference(AssemblyLoadContext.GetLoadContext(Program(function).Method.Module.Assembly));
        }
    }
}
