// Process I/O contracts only; these never claim native display or GUI acceptance.
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

internal static class MacCaptureProcessTests
{
    private static int Main(string[] args)
    {
        if (args.Length == 1)
        {
            if (args[0] == "--flood") { Console.Error.Write(new string('x', 512 * 1024)); Console.Error.WriteLine("complete"); return 0; }
            if (args[0] == "--environment") return Environment.GetEnvironmentVariable("DYLD_PRINT_LIBRARIES") == null ? 0 : 9;
            if (args[0] == "--failure") { Console.Error.WriteLine("intentional capture-tool failure"); return 7; }
            if (args[0] == "--hang") { Thread.Sleep(10000); return 0; }
        }
        if (args.Length != 2) { Console.Error.WriteLine("Usage: MacCaptureProcessTests.exe ABSOLUTE-MONO OUTPUT-DIRECTORY"); return 2; }
        try
        {
            Directory.CreateDirectory(args[1]);
            string self = Assembly.GetExecutingAssembly().Location;
            Func<string, ProcessStartInfo> child = option => new ProcessStartInfo(args[0], Quote(self) + " " + option);
            string flood = MacDesktopCapture.RunCaptureProcess(child("--flood"), Path.Combine(args[1], "flood.log"), 2000);
            Require(flood.Length > 512 * 1024 && flood.EndsWith("complete" + Environment.NewLine), "Complete stderr larger than a pipe buffer was not drained.");
            var environment = child("--environment");
            environment.EnvironmentVariables["DYLD_PRINT_LIBRARIES"] = "1";
            string inherited = Environment.GetEnvironmentVariable("DYLD_PRINT_LIBRARIES");
            MacDesktopCapture.RunCaptureProcess(environment, Path.Combine(args[1], "environment.log"), 2000);
            Require(!environment.EnvironmentVariables.ContainsKey("DYLD_PRINT_LIBRARIES"), "The external screenshot child retained native tracing.");
            Require(Environment.GetEnvironmentVariable("DYLD_PRINT_LIBRARIES") == inherited, "Parent native tracing was changed.");
            bool failed = false;
            try { MacDesktopCapture.RunCaptureProcess(child("--failure"), Path.Combine(args[1], "failure.log"), 2000); }
            catch (InvalidOperationException error) { failed = error.Message.Contains("7") && error.Message.Contains("intentional capture-tool failure"); }
            Require(failed, "A nonzero capture process must fail and retain its error.");
            bool timedOut = false; var watch = Stopwatch.StartNew();
            try { MacDesktopCapture.RunCaptureProcess(child("--hang"), Path.Combine(args[1], "timeout.log"), 500); }
            catch (TimeoutException) { timedOut = true; }
            Require(timedOut && watch.Elapsed.TotalSeconds < 5, "An unresponsive child exceeded the bounded cleanup interval.");
            Console.WriteLine("Capture-process contracts: stderr flood, child-only trace isolation, nonzero exit and bounded timeout passed. No GUI acceptance is claimed.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine("FAIL capture-process contract: " + error); return 1; }
    }

    private static string Quote(string value) { return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\""; }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
}
