using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Computator.NET.WebApi.Controllers;
using Computator.NET.WebApi.Execution;
using NUnit.Framework;

namespace Computator.NET.WebApi.IntegrationTests
{
    [TestFixture, NonParallelizable]
    public class WorkerProcessTests
    {
        [Test]
        public async Task TimedOutWorkerIsTerminatedAndNextRequestRecovers()
        {
            var pid = 0;
            try
            {
                await WorkerProcess.RunAsync(new ExecutionRequest { Operation = "script", Expression = "while(true) {}" }, false,
                    CancellationToken.None, TimeSpan.FromSeconds(8), value => pid = value);
                NUnit.Framework.Assert.Fail("An infinite script unexpectedly returned.");
            }
            catch (TimeoutException) { }
            NUnit.Framework.Assert.That(pid, Is.GreaterThan(0));
            NUnit.Framework.Assert.That(IsRunning(pid), Is.False, "Cancellation must terminate the child, not only stop waiting.");
            var next = await WorkerProcess.RunAsync(new ExecutionRequest { Expression = "6/3" }, false, CancellationToken.None);
            NUnit.Framework.Assert.That(next.Status, Is.EqualTo(200));
            NUnit.Framework.Assert.That(next.Json, Does.Contain("\"2\""));
        }
        [Test]
        public async Task CallerCancellationKillsTheOwnedWorker()
        {
            var pid = 0;
            using var cancellation = new CancellationTokenSource();
            var task = WorkerProcess.RunAsync(new ExecutionRequest { Operation = "script", Expression = "while(true) {}" }, false,
                cancellation.Token, started: value => { pid = value; cancellation.CancelAfter(1500); });
            try { await task; NUnit.Framework.Assert.Fail("Expected cancellation."); }
            catch (OperationCanceledException) { }
            NUnit.Framework.Assert.That(pid, Is.GreaterThan(0));
            NUnit.Framework.Assert.That(IsRunning(pid), Is.False);
        }
        [Test]
        public async Task WorkerOutputAndDefinitionsDoNotLeakIntoAnotherRequest()
        {
            var first = await WorkerProcess.RunAsync(new ExecutionRequest { Operation = "script", Expression = "System.Console.WriteLine(999); write(Probe(0));",
                CustomFunctions = "static real Probe(real x) { return 42; }" }, false, CancellationToken.None);
            NUnit.Framework.Assert.That(first.Json, Does.Contain("42").And.Not.Contain("999"));
            var second = await WorkerProcess.RunAsync(new ExecutionRequest { Expression = "Probe(0)" }, false, CancellationToken.None);
            NUnit.Framework.Assert.That(second.Status, Is.EqualTo(400));
        }
        private static bool IsRunning(int pid)
        {
            try { using var process = Process.GetProcessById(pid); return !process.HasExited; }
            catch (ArgumentException) { return false; }
        }
    }
}
