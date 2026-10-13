using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Computator.NET.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Computator.NET.WebApi.Execution
{
    public sealed class WorkerRequest { public ExecutionRequest Request { get; set; } public bool Chart { get; set; } }
    public sealed class WorkerReply { public int Status { get; set; } public string Json { get; set; } public string Image { get; set; } }

    /// <summary>One owned child per execution; process lifetime is not a security sandbox.</summary>
    public static class WorkerProcess
    {
        private static readonly SemaphoreSlim Slots = new SemaphoreSlim(4, 4);
        private static int _waiting;
        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }; // private JSON transport, never HTML

        public static async Task<WorkerReply> RunAsync(ExecutionRequest request, bool chart, CancellationToken cancellation,
            TimeSpan? maximumTime = null, Action<int> started = null)
        {
            var duration = maximumTime ?? TimeSpan.FromSeconds(30);
            if (duration <= TimeSpan.Zero || duration > TimeSpan.FromMinutes(5)) throw new ArgumentOutOfRangeException(nameof(maximumTime));
            if (Interlocked.Increment(ref _waiting) > 32)
            {
                Interlocked.Decrement(ref _waiting); throw new WorkerBusyException();
            }
            using var limit = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            limit.CancelAfter(duration);
            var entered = false;
            var processStarted = false;
            Process process = null;
            try
            {
                await Slots.WaitAsync(limit.Token);
                entered = true;
                var assembly = typeof(Program).Assembly.Location;
                var directory = Path.GetDirectoryName(assembly);
                var appHost = Path.ChangeExtension(assembly, ".exe");
                var ownRuntime = Path.ChangeExtension(assembly, ".runtimeconfig.json");
                var hasAppHost = File.Exists(appHost) && File.Exists(ownRuntime);
                var info = new ProcessStartInfo(hasAppHost ? appHost : "dotnet") {
                    UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = directory,
                    RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
                    StandardInputEncoding = new UTF8Encoding(false), StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8 };
                if (!hasAppHost)
                {
                    // Test project references may copy the API DLL without its
                    // apphost/runtimeconfig. Use the containing test runtime there.
                    var runtime = Path.Combine(directory, "Computator.NET.WebApi.IntegrationTests.runtimeconfig.json");
                    if (!File.Exists(ownRuntime) && File.Exists(runtime))
                    {
                        info.ArgumentList.Add("exec"); info.ArgumentList.Add("--runtimeconfig"); info.ArgumentList.Add(runtime);
                        info.ArgumentList.Add("--depsfile"); info.ArgumentList.Add(runtime.Replace(".runtimeconfig.json", ".deps.json"));
                    }
                    info.ArgumentList.Add(assembly);
                }
                info.ArgumentList.Add("--execution-worker");
                info.Environment.Remove("ExecutionAccess__Token");
                info.Environment.Remove("MathematicalApi__Token");
                process = new Process { StartInfo = info };
                processStarted = process.Start();
                if (!processStarted) throw new IOException("Could not start an execution worker.");
                started?.Invoke(process.Id);
                var output = ReadBounded(process.StandardOutput, 32 * 1024 * 1024, limit);
                var errors = ReadBounded(process.StandardError, 65536, limit);
                var input = JsonSerializer.Serialize(new WorkerRequest { Request = request, Chart = chart }, JsonOptions);
                if (Encoding.UTF8.GetByteCount(input) > 262144) throw new ArgumentException("Execution request is too large.");
                await process.StandardInput.WriteAsync(input.AsMemory(), limit.Token);
                process.StandardInput.Close();
                await Task.WhenAll(output, errors, process.WaitForExitAsync(limit.Token));
                if (process.ExitCode != 0) throw new IOException("Execution worker failed; the API remains available.");
                var reply = JsonSerializer.Deserialize<WorkerReply>(await output, JsonOptions);
                if (reply == null || reply.Status < 200 || reply.Status > 599 || (reply.Json == null && reply.Image == null))
                    throw new IOException("Execution worker returned an invalid response.");
                return reply;
            }
            catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
            { throw new TimeoutException("Execution exceeded its time or output limit and was terminated."); }
            finally
            {
                try
                {
                    if (processStarted && !process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                        using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        await process.WaitForExitAsync(cleanup.Token);
                    }
                }
                finally
                {
                    limit.Cancel();
                    process?.Dispose();
                    if (entered) Slots.Release();
                    Interlocked.Decrement(ref _waiting);
                }
            }
        }
        private static async Task<string> ReadBounded(StreamReader reader, int maximum, CancellationTokenSource limit)
        {
            var result = new StringBuilder(); var buffer = new char[8192];
            try
            {
                int count;
                while ((count = await reader.ReadAsync(buffer.AsMemory(), limit.Token)) != 0)
                {
                    if (result.Length + count > maximum) throw new IOException("Execution worker output exceeded its limit.");
                    result.Append(buffer, 0, count);
                }
                return result.ToString();
            }
            catch { limit.Cancel(); throw; }
        }
        public static IActionResult ToResult(WorkerReply reply) => reply.Image != null
            ? new FileContentResult(Convert.FromBase64String(reply.Image), "image/png")
            : new ContentResult { Content = reply.Json, ContentType = "application/json; charset=utf-8", StatusCode = reply.Status };
    }
    public sealed class WorkerBusyException : Exception { }
}
