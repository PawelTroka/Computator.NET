using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.CompilationTests
{
    [TestFixture, NonParallelizable]
    public class ProgramLifetimeTests
    {
        [Test]
        public void ReleasedRawProgramsAreCollectibleWithoutBreakingAssemblyReturningApi()
        {
            var references = RawBatch(32);
            AssertCollected(references);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference[] RawBatch(int count)
        {
            using var compiler = new NativeCompiler();
            var references = new List<WeakReference>();
            for (var index = 0; index < count; index++)
            {
                var assembly = compiler.Compile($"public static class Probe {{ public static int Run() => {index}; }}");
                NUnit.Framework.Assert.That(assembly.GetType("Probe").GetMethod("Run").Invoke(null, null), Is.EqualTo(index));
                var context = AssemblyLoadContext.GetLoadContext(assembly);
                NUnit.Framework.Assert.That(context.IsCollectible, Is.True);
                references.Add(new WeakReference(context, trackResurrection: true));
            }
            return references.ToArray();
        }

        [Test]
        public void RetainedFunctionSurvivesCollectionWhileDiscardedEditsAreReclaimed()
        {
            GSLInitializer.Initialize();
            var evaluator = new ExpressionsEvaluator();
            var retained = evaluator.Evaluate("Probe(x)", "static real Probe(real x) { return 2*x; }", CalculationsMode.Real);
            var discarded = ExpressionBatch(evaluator, 12);
            AssertCollected(discarded);
            NUnit.Framework.Assert.That(retained.Evaluate(21.0), Is.EqualTo(42.0));
            GC.KeepAlive(retained);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference[] ExpressionBatch(ExpressionsEvaluator evaluator, int count)
        {
            var contexts = new List<WeakReference>();
            for (var index = 0; index < count; index++)
            {
                var function = evaluator.Evaluate("Probe(x)", $"static real Probe(real x) {{ return x+{index}; }}", CalculationsMode.Real);
                NUnit.Framework.Assert.That(function.Evaluate(2.0), Is.EqualTo(2.0 + index));
                Func<double, double> callback = function;
                contexts.Add(new WeakReference(AssemblyLoadContext.GetLoadContext(callback.Method.Module.Assembly), true));
            }
            return contexts.ToArray();
        }

        [Test]
        public void RepeatedApplicationCompilationDoesNotRetainOldContexts()
        {
            GSLInitializer.Initialize();
            var process = Process.GetCurrentProcess();
            var evaluator = new ExpressionsEvaluator();
            for (var batch = 0; batch < 4; batch++)
            {
                AssertCollected(ExpressionBatch(evaluator, 10));
                process.Refresh();
                TestContext.Progress.WriteLine($"Batch {batch}: managed={GC.GetTotalMemory(false)}, private={process.PrivateMemorySize64}, assemblies={AppDomain.CurrentDomain.GetAssemblies().Length}");
            }
            // Memory counters are diagnostic: a fixed byte threshold would depend
            // on JIT/allocator/runner behavior. Reclamation is the actual gate.
        }

        [Test]
        public void ActiveDynamicDelegatePreservesItsStateAndTypeIdentity()
        {
            var callback = CreateDynamicDelegate();
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            NUnit.Framework.Assert.That(callback(), Is.EqualTo(42));
            NUnit.Framework.Assert.That(callback(), Is.EqualTo(43));
            GC.KeepAlive(callback);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Func<int> CreateDynamicDelegate()
        {
            using var compiler = new NativeCompiler();
            var assembly = compiler.Compile("public static class Probe { static int n = 41; public static int Run() { dynamic value = ++n; return value; } }");
            return assembly.GetType("Probe").GetMethod("Run").CreateDelegate<Func<int>>();
        }

        private static void AssertCollected(IEnumerable<WeakReference> references)
        {
            var items = references.ToArray();
            for (var pass = 0; pass < 20 && items.Any(reference => reference.IsAlive); pass++)
            { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
            NUnit.Framework.Assert.That(items.Count(reference => reference.IsAlive), Is.Zero, "A discarded generated program remains rooted.");
        }
    }
}
