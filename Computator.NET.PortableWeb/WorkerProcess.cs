using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Computator.NET.PortableWeb;

/// <summary>Bounded lifetime for trusted execution, not a security sandbox or Unix job object.</summary>
internal sealed class WorkerProcess(TimeSpan maximumTime, CancellationToken applicationStopping)
{
    private readonly SemaphoreSlim _slots = new(2, 2);
    private int _active;
    public int ActiveCount => Volatile.Read(ref _active);

    public async Task<WorkerReply> RunAsync(ExecutionRequest request, CancellationToken cancellation)
    {
        using var limit = CancellationTokenSource.CreateLinkedTokenSource(cancellation, applicationStopping);
        limit.CancelAfter(maximumTime);
        var entered = false;
        var started = false;
        Process? process = null;
        Task<string>? output = null, errors = null;
        try
        {
            entered = await _slots.WaitAsync(0, limit.Token);
            if (!entered) return new WorkerReply(429,
                Detail: "Both local execution slots are busy. Cancel an operation or try again after it completes.");
            process = new Process { StartInfo = CreateStartInfo() };
            if (!process.Start()) throw new IOException("Could not start an execution worker.");
            started = true;
            Interlocked.Increment(ref _active);
            output = ReadBounded(process.StandardOutput, 1048576, limit);
            errors = ReadBounded(process.StandardError, 65536, limit);
            var input = JsonSerializer.Serialize(request, Execution.JsonOptions);
            await process.StandardInput.WriteAsync(input.AsMemory(), limit.Token);
            process.StandardInput.Close();
            await process.WaitForExitAsync(limit.Token);
            await Task.WhenAll(output, errors);
            if (process.ExitCode != 0) return new WorkerReply(502, Detail: "Execution worker exited unexpectedly; the local application remains available.");
            var reply = JsonSerializer.Deserialize<WorkerReply>(await output, Execution.JsonOptions);
            if (!ValidReply(reply, request))
                throw new IOException("Execution worker returned an invalid response.");
            return reply!;
        }
        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested && !applicationStopping.IsCancellationRequested)
        { return new WorkerReply(504, Detail: "Execution exceeded its time or output limit and was terminated."); }
        catch (Exception exception) when (exception is IOException or JsonException or System.ComponentModel.Win32Exception)
        { return new WorkerReply(502, Detail: "Execution worker failed; the local application remains available."); }
        finally
        {
            try
            {
                limit.Cancel();
                if (process != null && started)
                {
                    try
                    {
                        if (!process.HasExited) process.Kill(entireProcessTree: true);
                        using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        await process.WaitForExitAsync(cleanup.Token);
                    }
                    catch (InvalidOperationException) { }
                    catch (OperationCanceledException) { }
                    finally { Interlocked.Decrement(ref _active); process.Dispose(); }
                }
                else process?.Dispose();
                // Observe pipe tasks, including cancellation or output-limit errors.
                if (output != null && errors != null)
                {
                    try { await Task.WhenAll(output, errors); }
                    catch (Exception exception) when (exception is IOException or OperationCanceledException or ObjectDisposedException) { }
                }
            }
            finally
            {
                if (entered) _slots.Release();
            }
        }
    }

    private static bool ValidReply(WorkerReply? reply, ExecutionRequest request)
    {
        if (reply == null || (reply.Status != 200 && reply.Status != 400 &&
            !(request.Operation == "chart" && reply.Status == 422))) return false;
        if (reply.Status != 200) return reply.Result == null && reply.Curve == null && reply.Surface == null;
        if (request.Operation != "chart")
            return reply.Curve == null && reply.Surface == null && reply.Result is { Length: <= 65536 };
        if (reply.Result != null) return false;
        if (reply.Surface is { } surface)
        {
            if (reply.Curve != null || surface.X == null || surface.Y == null || surface.Z == null ||
                surface.X.Length != request.GridSize || surface.Y.Length != request.GridSize ||
                surface.Z.Length != request.GridSize ||
                surface.X[0] != request.XMin || surface.X[^1] != request.XMax ||
                surface.Y[0] != request.YMin || surface.Y[^1] != request.YMax)
                return false;
            for (var index = 0; index < request.GridSize; index++)
            {
                if (!double.IsFinite(surface.X[index]) || !double.IsFinite(surface.Y[index]) ||
                    (index > 0 && (surface.X[index] <= surface.X[index - 1] ||
                                   surface.Y[index] <= surface.Y[index - 1])))
                    return false;
            }
            var finiteSurface = 0;
            foreach (var row in surface.Z)
            {
                if (row == null || row.Length != request.GridSize) return false;
                foreach (var value in row)
                {
                    if (value is not { } height) continue;
                    if (!double.IsFinite(height)) return false;
                    finiteSurface++;
                }
            }
            return finiteSurface >= 3 && finiteSurface == surface.FiniteSamples &&
                surface.MissingSamples == request.GridSize * request.GridSize - finiteSurface;
        }
        if (reply.Curve is not { } curve || curve.X == null || curve.Y == null ||
            curve.X.Length < request.CurveSamples || curve.X.Length > 2 * request.CurveSamples - 1 ||
            curve.Y.Length != curve.X.Length || curve.X[0] != request.XMin || curve.X[^1] != request.XMax)
            return false;
        var finite = 0;
        for (var index = 0; index < curve.X.Length; index++)
        {
            if (!double.IsFinite(curve.X[index]) || (index > 0 && curve.X[index] <= curve.X[index - 1])) return false;
            if (curve.Y[index] is not { } value) continue;
            if (!double.IsFinite(value)) return false;
            finite++;
        }
        return finite == curve.FiniteSamples && curve.MissingSamples == curve.X.Length - finite &&
            (finite > 0 || request.AllowEmpty2D);
    }

    private static ProcessStartInfo CreateStartInfo()
    {
        var executable = Environment.ProcessPath ?? throw new IOException("Cannot determine the local application executable.");
        var info = new ProcessStartInfo(executable)
        {
            WorkingDirectory = AppContext.BaseDirectory, UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
            StandardInputEncoding = new UTF8Encoding(false), StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8
        };
        if (string.Equals(Path.GetFileNameWithoutExtension(executable), "dotnet", StringComparison.OrdinalIgnoreCase))
            info.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
        info.ArgumentList.Add("--execution-worker");
        return info;
    }

    private static async Task<string> ReadBounded(StreamReader reader, int maximum, CancellationTokenSource limit)
    {
        var text = new StringBuilder(); var buffer = new char[8192];
        try
        {
            int count;
            while ((count = await reader.ReadAsync(buffer.AsMemory(), limit.Token)) != 0)
            {
                if (count > maximum - text.Length) throw new IOException("Execution worker output exceeded its limit.");
                text.Append(buffer, 0, count);
            }
            return text.ToString();
        }
        catch { limit.Cancel(); throw; }
    }

    public static int RunWorker()
    {
        Console.InputEncoding = new UTF8Encoding(false, true);
        Console.OutputEncoding = new UTF8Encoding(false, true);
        var protocol = Console.Out;
        Console.SetOut(TextWriter.Null);
        Console.SetError(TextWriter.Null);
        WorkerReply reply;
        try
        {
            var input = new StringBuilder(); var buffer = new char[4096]; int count;
            while ((count = Console.In.Read(buffer, 0, buffer.Length)) != 0)
            {
                if (count > Execution.MaximumBodyBytes - input.Length) throw new ArgumentException("Worker request is too large.");
                input.Append(buffer, 0, count);
            }
            var request = JsonSerializer.Deserialize<ExecutionRequest>(input.ToString(), Execution.JsonOptions)
                ?? throw new ArgumentException("A JSON request is required.");
            reply = Execution.Run(request);
        }
        catch (Exception exception) { reply = new WorkerReply(400, Detail: exception.Message); }
        finally
        {
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }
        protocol.Write(JsonSerializer.Serialize(reply, Execution.JsonOptions));
        protocol.Flush();
        return 0;
    }
}
