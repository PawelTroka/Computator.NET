using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
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
            var marker = Path.Combine(Path.GetTempPath(), "Computator-worker-" + Guid.NewGuid().ToString("N"));
            var previous = Environment.GetEnvironmentVariable("COMPUTATOR_WORKER_MARKER");
            Environment.SetEnvironmentVariable("COMPUTATOR_WORKER_MARKER", marker);
            using var cancellation = new CancellationTokenSource();
            var task = WorkerProcess.RunAsync(new ExecutionRequest { Operation = "script", Expression = RunningScript }, false,
                cancellation.Token, started: value => pid = value);
            try
            {
                // Do not cancel during child startup: prove the user's loop began.
                await WaitUntil(() => File.Exists(marker), "The worker did not enter the script.");
                cancellation.Cancel();
                try { await task; NUnit.Framework.Assert.Fail("Expected cancellation."); }
                catch (OperationCanceledException) { }
                NUnit.Framework.Assert.That(pid, Is.GreaterThan(0));
                NUnit.Framework.Assert.That(IsRunning(pid), Is.False);
            }
            finally
            {
                cancellation.Cancel();
                try { await task; } catch (OperationCanceledException) { }
                Environment.SetEnvironmentVariable("COMPUTATOR_WORKER_MARKER", previous);
                if (File.Exists(marker)) File.Delete(marker);
            }
        }
        private const string RunningScript = "System.IO.File.WriteAllText(System.Environment.GetEnvironmentVariable(\"COMPUTATOR_WORKER_MARKER\"), System.Environment.ProcessId.ToString()); while(true) {}";

        [Test]
        public async Task AbruptApiExitTerminatesItsRunningWorker()
        {
            var marker = Path.Combine(Path.GetTempPath(), "Computator-worker-" + Guid.NewGuid().ToString("N"));
            var assembly = typeof(Program).Assembly.Location;
            var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            int port;
            using (var listener = new TcpListener(IPAddress.Loopback, 0))
            { listener.Start(); port = ((IPEndPoint)listener.LocalEndpoint).Port; }
            var info = new ProcessStartInfo(Path.ChangeExtension(assembly, ".exe"))
            {
                WorkingDirectory = Path.GetDirectoryName(assembly), UseShellExecute = false,
                CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true
            };
            info.ArgumentList.Add("--urls"); info.ArgumentList.Add("http://127.0.0.1:" + port);
            info.Environment["ExecutionAccess__Token"] = token;
            info.Environment["COMPUTATOR_WORKER_MARKER"] = marker;
            info.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
            info.Environment["DOTNET_ENVIRONMENT"] = "Production";
            using var api = Process.Start(info);
            var output = api.StandardOutput.ReadToEndAsync(); var errors = api.StandardError.ReadToEndAsync();
            using var client = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:" + port), Timeout = TimeSpan.FromSeconds(40) };
            client.DefaultRequestHeaders.Add("X-Computator-Session", token);
            Task<HttpResponseMessage> execution = null;
            var worker = 0;
            try
            {
                var ready = false;
                for (var attempt = 0; attempt < 100 && !api.HasExited; attempt++)
                {
                    try { using var response = await client.GetAsync("/api/numerical-calculations/integral/list-methods"); ready = response.IsSuccessStatusCode; }
                    catch (HttpRequestException) { }
                    if (ready) break;
                    await Task.Delay(100);
                }
                NUnit.Framework.Assert.That(ready, Is.True, "The owned API did not become ready.");
                execution = client.PostAsJsonAsync("/api/v2/execute", new ExecutionRequest { Operation = "script", Expression = RunningScript });
                await WaitUntil(() => File.Exists(marker) && int.TryParse(File.ReadAllText(marker), out worker), "The worker did not enter the script.");
                api.Kill(); // Deliberately kill only the parent, exercising OS job cleanup.
                await api.WaitForExitAsync();
                await WaitUntil(() => !IsRunning(worker), "The stopped API left an execution worker running.");
            }
            finally
            {
                if (!api.HasExited) { api.Kill(entireProcessTree: true); await api.WaitForExitAsync(); }
                if (worker != 0 && IsRunning(worker)) { using var child = Process.GetProcessById(worker); child.Kill(entireProcessTree: true); await child.WaitForExitAsync(); }
                if (execution != null) { try { (await execution).Dispose(); } catch (HttpRequestException) { } catch (TaskCanceledException) { } }
                await Task.WhenAll(output, errors);
                if (File.Exists(marker)) File.Delete(marker);
            }
        }

        private static async Task WaitUntil(Func<bool> condition, string message)
        {
            for (var attempt = 0; attempt < 150; attempt++)
            {
                try { if (condition()) return; } catch (IOException) { }
                await Task.Delay(100);
            }
            NUnit.Framework.Assert.Fail(message);
        }
        [Test]
        public async Task WorkerProtocolPreservesUnicodeInBothDirections()
        {
            const string text = "Zażółć gęślą jaźń αβγ";
            var reply = await WorkerProcess.RunAsync(new ExecutionRequest { Operation = "script", Expression = "write(\"" + text + "\");" },
                false, CancellationToken.None);
            NUnit.Framework.Assert.That(reply.Status, Is.EqualTo(200));
            using var json = System.Text.Json.JsonDocument.Parse(reply.Json);
            NUnit.Framework.Assert.That(json.RootElement.GetProperty("result").GetString(), Does.Contain(text));
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
