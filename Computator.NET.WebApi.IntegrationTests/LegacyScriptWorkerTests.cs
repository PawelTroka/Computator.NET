using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;

namespace Computator.NET.WebApi.IntegrationTests
{
    [TestFixture, NonParallelizable]
    public class LegacyScriptWorkerTests
    {
        private WebApplication _server;
        private HttpClient _client;
        [OneTimeSetUp]
        public async Task Start() { (_server, _client) = await ProtectedApiTestHost.Start(); }
        [OneTimeTearDown]
        public async Task Stop() { _client?.Dispose(); if (_server != null) await _server.DisposeAsync(); }
        private static string Url(string script, string definitions = null) => "/api/script/" + Uri.EscapeDataString(script) +
            (definitions == null ? "" : "/" + Uri.EscapeDataString(definitions));

        [TestCase(null)]
        [TestCase("text/plain")]
        [TestCase("application/json")]
        public async Task OriginalStringNegotiationAndUnicodeSurviveWorkerDispatch(string accept)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, Url("write(\"Zażółć λ\\n%2F \"); write(6/2);"));
            if (accept != null) request.Headers.Accept.ParseAdd(accept);
            using var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            NUnit.Framework.Assert.That(response.Content.Headers.ContentType.MediaType,
                Is.EqualTo(accept == "application/json" ? "application/json" : "text/plain"));
            NUnit.Framework.Assert.That(accept == "application/json" ? JsonSerializer.Deserialize<string>(body) : body,
                Is.EqualTo("Zażółć λ\n%2F 3"));
            NUnit.Framework.Assert.That(response.Headers.CacheControl.NoStore, Is.True);
        }

        [Test]
        public async Task EmptyOutputRemainsSuccessfulAndDoesNotBecomeNoContent()
        {
            using var response = await _client.GetAsync(Url("write(\"\");"));
            NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            NUnit.Framework.Assert.That(await response.Content.ReadAsStringAsync(), Is.Empty);
        }

        [Test]
        public async Task ScriptRunsInAChildThatHasExitedBeforeTheSuccessfulResponse()
        {
            var text = await _client.GetStringAsync(Url("write(System.Environment.ProcessId);"));
            var pid = int.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
            NUnit.Framework.Assert.That(pid, Is.Not.EqualTo(Environment.ProcessId));
            NUnit.Framework.Assert.That(IsRunning(pid), Is.False);
        }

        [Test]
        public async Task ConcurrentCustomFunctionsRemainRequestLocalAndBadCodeCanRecover()
        {
            async Task<string> Run(int constant) => await _client.GetStringAsync(Url("write(Probe(6));",
                "static real Probe(real x) { return x/2+" + constant + "; }"));
            var results = await Task.WhenAll(Run(10), Run(20));
            NUnit.Framework.Assert.That(results, Is.EqualTo(new[] { "13", "23" }));
            using var missing = await _client.GetAsync(Url("write(Probe(6));"));
            NUnit.Framework.Assert.That(missing.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            NUnit.Framework.Assert.That(await _client.GetStringAsync(Url("write(42);")), Is.EqualTo("42"));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task RunningLegacyLoopTerminatesOnCancellationOrDefaultDeadline(bool cancel)
        {
            // A separate owned API keeps this negative regression safe even if
            // legacy dispatch accidentally becomes in-process again.
            var directory = Path.Combine(Path.GetTempPath(), "Computator-Legacy-Worker-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var marker = Path.Combine(directory, "worker.txt");
            var assembly = typeof(Program).Assembly.Location;
            int port;
            using (var listener = new TcpListener(IPAddress.Loopback, 0))
            { listener.Start(); port = ((IPEndPoint)listener.LocalEndpoint).Port; }
            var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var info = new ProcessStartInfo(Path.ChangeExtension(assembly, ".exe")) {
                UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = Path.GetDirectoryName(assembly),
                RedirectStandardOutput = true, RedirectStandardError = true };
            info.ArgumentList.Add("--urls"); info.ArgumentList.Add("http://127.0.0.1:" + port);
            info.Environment["ExecutionAccess__Token"] = token;
            info.Environment["COMPUTATOR_DATA_DIRECTORY"] = Path.Combine(directory, "profile");
            info.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
            info.Environment["DOTNET_ENVIRONMENT"] = "Production";
            using var api = Process.Start(info);
            var output = api.StandardOutput.ReadToEndAsync(); var errors = api.StandardError.ReadToEndAsync();
            using var client = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:" + port), Timeout = TimeSpan.FromSeconds(45) };
            client.DefaultRequestHeaders.Add("X-Computator-Session", token);
            using var cancellation = new CancellationTokenSource();
            Task<HttpResponseMessage> execution = null;
            var worker = 0;
            try
            {
                var ready = false;
                for (var attempt = 0; attempt < 150 && !api.HasExited; attempt++)
                {
                    try { using var response = await client.GetAsync("/api/numerical-calculations/integral/list-methods"); ready = response.IsSuccessStatusCode; }
                    catch (HttpRequestException) { }
                    if (ready) break;
                    await Task.Delay(100);
                }
                NUnit.Framework.Assert.That(ready, Is.True, "The owned API did not start.");
                var script = "System.IO.File.WriteAllText(" + JsonSerializer.Serialize(marker) +
                    ", System.Environment.ProcessId.ToString()); while(true) {}";
                execution = client.GetAsync(Url(script), cancellation.Token);
                await WaitUntil(() => File.Exists(marker) && int.TryParse(File.ReadAllText(marker), out worker),
                    "The legacy script did not actually start.");
                NUnit.Framework.Assert.That(worker, Is.Not.EqualTo(api.Id), "Legacy code ran inside the API process.");
                NUnit.Framework.Assert.That(IsRunning(worker), Is.True);
                if (cancel)
                {
                    cancellation.Cancel();
                    try { (await execution).Dispose(); NUnit.Framework.Assert.Fail("Expected HTTP caller cancellation."); }
                    catch (OperationCanceledException) { }
                }
                else
                {
                    using var response = await execution;
                    NUnit.Framework.Assert.That((int)response.StatusCode, Is.EqualTo(504));
                }
                await WaitUntil(() => !IsRunning(worker), "The legacy worker remained alive after termination.");
                NUnit.Framework.Assert.That(api.HasExited, Is.False);
                NUnit.Framework.Assert.That(await client.GetStringAsync(Url("write(42);")), Is.EqualTo("42"));
            }
            finally
            {
                cancellation.Cancel();
                if (!api.HasExited) { api.Kill(entireProcessTree: true); await api.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10)); }
                if (execution != null) { try { (await execution).Dispose(); } catch (HttpRequestException) { } catch (OperationCanceledException) { } }
                await Task.WhenAll(output, errors).WaitAsync(TimeSpan.FromSeconds(10));
                Directory.Delete(directory, true);
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
        private static bool IsRunning(int pid)
        {
            try { using var process = Process.GetProcessById(pid); return !process.HasExited; }
            catch (ArgumentException) { return false; }
        }
    }
}
