// Real subprocess I/O contracts only. These do not send or claim X11 GUI input.
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal static class NativeToolProcessTests
{
    private static int Main(string[] args)
    {
        if (args.Length == 1)
        {
            if (args[0] == "--flood")
            {
                var output = Task.Run(() => { Console.Out.Write(new string('o', 512 * 1024)); Console.Out.WriteLine("stdout complete"); });
                Console.Error.Write(new string('e', 512 * 1024)); Console.Error.WriteLine("stderr complete");
                output.GetAwaiter().GetResult(); return 0;
            }
            if (args[0] == "--environment") return Environment.GetEnvironmentVariable("DYLD_PRINT_LIBRARIES") == null ? 0 : 9;
            if (args[0] == "--failure") { Console.Error.WriteLine("intentional native-tool failure"); Console.Out.WriteLine("failure stdout"); return 7; }
            if (args[0] == "--hang") { Console.Error.WriteLine("child reached hang " + Process.GetCurrentProcess().Id); Thread.Sleep(10000); return 0; }
        }
        if (args.Length != 2) { Console.Error.WriteLine("Usage: NativeToolProcessTests.exe ABSOLUTE-MONO OUTPUT-DIRECTORY"); return 2; }
        try
        {
            Directory.CreateDirectory(args[1]);
            string self = Assembly.GetExecutingAssembly().Location;
            Func<string, ProcessStartInfo> child = option => new ProcessStartInfo(args[0], Quote(self) + " " + option);
            var flood = NativeToolProcess.Run(child("--flood"), 2000);
            // Preserve decisive evidence before an assertion can stop the run.
            // Do not infer reader truncation from a marker/suffix mismatch.
            WriteFloodEvidence(args[1], "stdout", flood.StandardOutput, 'o', "stdout complete");
            WriteFloodEvidence(args[1], "stderr", flood.StandardError, 'e', "stderr complete");
            File.WriteAllText(Path.Combine(args[1], "flood-sizes.txt"), flood.StandardOutput.Length + "," + flood.StandardError.Length);
            Require(flood.StandardOutput.Length > 512 * 1024 && flood.StandardOutput.EndsWith("stdout complete" + Environment.NewLine), "Complete stdout was not drained.");
            Require(flood.StandardError.Length > 512 * 1024 && flood.StandardError.EndsWith("stderr complete" + Environment.NewLine), "Complete stderr was not drained.");

            string inherited = Environment.GetEnvironmentVariable("DYLD_PRINT_LIBRARIES");
            Environment.SetEnvironmentVariable("DYLD_PRINT_LIBRARIES", "native-tool-parent-marker");
            try
            {
                var environment = child("--environment");
                NativeToolProcess.Run(environment, 2000);
                Require(!environment.EnvironmentVariables.ContainsKey("DYLD_PRINT_LIBRARIES"), "Native tracing was removed too late.");
                Require(Environment.GetEnvironmentVariable("DYLD_PRINT_LIBRARIES") == "native-tool-parent-marker", "Parent native tracing was changed.");
            }
            finally { Environment.SetEnvironmentVariable("DYLD_PRINT_LIBRARIES", inherited); }

            bool failed = false;
            try { NativeToolProcess.Run(child("--failure"), 2000); }
            catch (InvalidOperationException error)
            {
                File.WriteAllText(Path.Combine(args[1], "exit-failure.log"), error.ToString());
                failed = error.Message.Contains("7") && error.Message.Contains("intentional native-tool failure") && error.Message.Contains("failure stdout");
            }
            Require(failed, "A nonzero tool exit must fail with both streams retained.");
            bool timedOut = false; var watch = Stopwatch.StartNew();
            try { NativeToolProcess.Run(child("--hang"), 500); }
            catch (TimeoutException error)
            {
                File.WriteAllText(Path.Combine(args[1], "timeout.log"), error.ToString());
                timedOut = error.Message.Contains("child reached hang");
                if (timedOut)
                {
                    string pidText = error.Message.Substring(error.Message.IndexOf("child reached hang ", StringComparison.Ordinal) + "child reached hang ".Length).Trim();
                    int pid = Int32.Parse(pidText);
                    bool exited;
                    try { using (var owned = Process.GetProcessById(pid)) exited = owned.HasExited; }
                    catch (ArgumentException) { exited = true; }
                    Require(exited, "The timed-out owned tool process survived cleanup.");
                }
            }
            Require(timedOut && watch.Elapsed.TotalSeconds < 5, "An unresponsive child did not fail within bounded cleanup.");
            bool rejected = false;
            try { NativeToolProcess.Run(child("--environment"), 6001); }
            catch (ArgumentOutOfRangeException) { rejected = true; }
            Require(rejected, "The native tool's six-second maximum deadline was relaxed.");
            Console.WriteLine("Native-tool process contracts: simultaneous stdout/stderr floods, child-only trace isolation, retained exit failure, bounded timeout and maximum deadline passed. No GUI acceptance is claimed.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine("FAIL native-tool process contract: " + error); return 1; }
    }

    private static void WriteFloodEvidence(string directory, string stream, string actual, char fill, string marker)
    {
        string expected = new string(fill, 512 * 1024) + marker + Environment.NewLine;
        File.WriteAllText(Path.Combine(directory, "flood." + stream + ".log"), actual);
        File.WriteAllText(Path.Combine(directory, "flood." + stream + "-diagnostics.log"),
            "characters=" + actual.Length + Environment.NewLine +
            "utf8Bytes=" + Encoding.UTF8.GetByteCount(actual) + Environment.NewLine +
            "sha256=" + Sha256(actual) + Environment.NewLine +
            "expectedPayloadCharacters=" + expected.Length + Environment.NewLine +
            "expectedPayloadSha256=" + Sha256(expected) + Environment.NewLine +
            "expectedPayloadIndex=" + actual.IndexOf(expected, StringComparison.Ordinal) + Environment.NewLine +
            "firstMarkerIndex=" + actual.IndexOf(marker, StringComparison.Ordinal) + Environment.NewLine +
            "lastMarkerIndex=" + actual.LastIndexOf(marker, StringComparison.Ordinal) + Environment.NewLine +
            "environmentNewLine=" + Escaped(Environment.NewLine) + Environment.NewLine +
            "consoleNewLine=" + Escaped(Console.Out.NewLine) + Environment.NewLine +
            "consoleWriterType=" + Console.Out.GetType().FullName + Environment.NewLine +
            "monoPath=" + Environment.GetEnvironmentVariable("MONO_PATH") + Environment.NewLine +
            "prefix=" + Escaped(actual.Substring(0, Math.Min(320, actual.Length))) + Environment.NewLine +
            "tail=" + Escaped(actual.Substring(Math.Max(0, actual.Length - 320))) + Environment.NewLine);
    }

    private static string Sha256(string value)
    {
        using (var hash = SHA256.Create())
            return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant();
    }

    private static string Escaped(string value) { return value.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t"); }

    private static string Quote(string value) { return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\""; }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
}
