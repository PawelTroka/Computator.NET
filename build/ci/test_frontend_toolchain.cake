// Run from the repository root: dotnet cake build/ci/test_frontend_toolchain.cake
// Optional --expected-node / --expected-npm assert the native CI matrix versions.
#load "../frontend-toolchain.cake"

using System.Threading;

var passed = 0;
// Cake runs with the script directory as its working directory.
var repositoryDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "../.."));
var frontendDirectory = System.IO.Path.Combine(repositoryDirectory, "Computator.NET.WebClient");
var expectedNode = Argument("expected-node", "");
var expectedNpm = Argument("expected-npm", "");
const string nodePin = "24.21.0";
const string npmPin = "11.19.0";
const string pinnedPrefix = "exec --yes --package=node@24.21.0 --package=npm@11.19.0 -- ";

void Assert(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

void Equal<T>(T expected, T actual, string message)
{
    Assert(EqualityComparer<T>.Default.Equals(expected, actual),
        message + ": expected <" + expected + ">, got <" + actual + ">.");
}

Exception ExpectFailure(Action action, params string[] diagnosticFragments)
{
    Exception failure = null;
    try { action(); }
    catch (Exception ex) { failure = ex; }
    Assert(failure != null, "The invalid prerequisite was accepted.");
    foreach (var fragment in diagnosticFragments)
        Assert(failure.ToString().IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0,
            "Diagnostic did not identify '" + fragment + "': " + failure);
    return failure;
}

void Check(string name, Action test)
{
    test();
    passed++;
    Information("PASS: {0}", name);
}

Check("exact pinned versions use the inherited tools", () =>
    Equal("", SelectFrontendCommandPrefix(nodePin, npmPin, nodePin, npmPin), "Pinned tool selection"));

Check("reported Node 24.11.1 and npm 11.7.0 select both exact pins", () =>
    Equal(pinnedPrefix, SelectFrontendCommandPrefix(nodePin, npmPin, "24.11.1", "11.7.0"), "Older supported tools"));

Check("supported minimum Node major and npm 7 still bootstrap", () =>
    Equal(pinnedPrefix, SelectFrontendCommandPrefix(nodePin, npmPin, "24.0.0", "7.0.0"), "Bootstrap floor"));

Check("newer Node still selects the repository's exact version", () =>
    Equal(pinnedPrefix, SelectFrontendCommandPrefix(nodePin, npmPin, "25.0.0", npmPin), "Newer Node"));

Check("a mismatch in either tool selects both pins", () =>
{
    Equal(pinnedPrefix, SelectFrontendCommandPrefix(nodePin, npmPin, "24.11.1", npmPin), "Node mismatch");
    Equal(pinnedPrefix, SelectFrontendCommandPrefix(nodePin, npmPin, nodePin, "11.7.0"), "npm mismatch");
});

Check("Node below the selected major is rejected", () =>
    ExpectFailure(() => SelectFrontendCommandPrefix(nodePin, npmPin, "23.11.0", npmPin), "Node", "24"));

Check("npm before 7 is rejected", () =>
    ExpectFailure(() => SelectFrontendCommandPrefix(nodePin, npmPin, nodePin, "6.14.18"), "npm", "7"));

Check("invalid repository pins cannot become command arguments", () =>
{
    foreach (var invalid in new[] { "", "24", "24.21", "24.21.0; echo invalid", "24.21.0-rc.1", "24.21.0.1" })
        ExpectFailure(() => SelectFrontendCommandPrefix(invalid, npmPin, nodePin, npmPin));
    foreach (var invalid in new[] { "", "11", "11.19", "11.19.0 --ignore-scripts", "11.19.0.1" })
        ExpectFailure(() => SelectFrontendCommandPrefix(nodePin, invalid, nodePin, npmPin));
});

Check("invalid installed versions fail closed", () =>
{
    foreach (var invalid in new[] { "", "not-a-version", "24", "24.21", "24.21.0-rc.1", "24.21.0.1" })
        ExpectFailure(() => SelectFrontendCommandPrefix(nodePin, npmPin, invalid, npmPin));
    foreach (var invalid in new[] { "", "not-a-version", "11", "11.19", "11.19.0-rc.1", "11.19.0.1" })
        ExpectFailure(() => SelectFrontendCommandPrefix(nodePin, npmPin, nodePin, invalid));
});

Check("version capture waits for asynchronous stdout and stderr", () =>
{
    foreach (var value in new[] { "v24.21.0", "24.11.1", "11.19.0", "11.7.0" })
    {
        using (var process = new VersionProcess(new[] { value }))
        {
            Equal(value.TrimStart('v'), ReadFrontendToolVersion(process, "node"), "Captured version");
            Equal(30000, process.TimedWaitMilliseconds, "Version probe timeout");
            Equal(1, process.DrainCalls, "Asynchronous capture drain");
            Assert(!process.ReadBeforeDrain, "Output was inspected before its handlers completed.");
            Equal(1, process.OutputReads, "stdout must be captured once");
            Equal(1, process.ErrorReads, "stderr must be captured once");
            Assert(!process.Disposed, "The reader must leave disposal to its caller.");
        }
    }
});

Check("blank lines do not conceal an otherwise unambiguous version", () =>
{
    using (var process = new VersionProcess(new[] { "", "  ", " v24.21.0 ", "", "\t" }))
        Equal(nodePin, ReadFrontendToolVersion(process, "node"), "Version surrounded by whitespace");
});

Check("empty, malformed, prerelease and ambiguous version output is rejected", () =>
{
    foreach (var output in new[]
    {
        new string[0], new[] { "" }, new[] { "not-a-version" }, new[] { "24.21" },
        new[] { "24.21.0.1" }, new[] { "24.21.0-rc.1" }, new[] { "vv24.21.0" },
        new[] { "node v24.21.0" }, new[] { "24.21.0", "25.0.0" },
        new[] { "24.21.0", "unexpected trailing output" }
    })
    {
        using (var process = new VersionProcess(output))
            ExpectFailure(() => ReadFrontendToolVersion(process, "node"), "node", "version");
    }
});

Check("nonzero exit is rejected even when stdout looks like a valid version", () =>
{
    using (var process = new VersionProcess(new[] { npmPin }, new[] { "probe execution failed" }, 23))
        ExpectFailure(() => ReadFrontendToolVersion(process, "npm"), "npm", "23", "probe execution failed");
});

Check("stderr does not replace a missing stdout version", () =>
{
    using (var process = new VersionProcess(new string[0], new[] { npmPin }))
        ExpectFailure(() => ReadFrontendToolVersion(process, "npm"), "npm", "version");
});

Check("successful version capture tolerates diagnostics on stderr", () =>
{
    using (var process = new VersionProcess(new[] { npmPin }, new[] { "diagnostic on stderr" }))
        Equal(npmPin, ReadFrontendToolVersion(process, "npm"), "Version with stderr diagnostic");
});

Check("timeout terminates the probe and reports the failed executable", () =>
{
    using (var process = new VersionProcess(new[] { nodePin }, completes: false))
    {
        ExpectFailure(() => ReadFrontendToolVersion(process, "node", 17), "node", "tim");
        Equal(17, process.TimedWaitMilliseconds, "Requested timeout");
        Assert(process.KillCalls > 0, "Timed out process was not terminated.");
        Equal(0, process.OutputReads, "Timed out stdout must not be accepted");
    }
});

Check("a missing executable produces an actionable failure", () =>
{
    var executable = "computator-absent-tool-" + Guid.NewGuid().ToString("N");
    ExpectFailure(() => FrontendToolVersion(executable, frontendDirectory), executable, "version", "Install", "PATH");
});

Check("a process that could not start identifies the missing tool and PATH", () =>
{
    foreach (var executable in new[] { "node", "npm" })
        ExpectFailure(() => ReadFrontendToolVersion(null, executable), executable, "PATH");
});

Check("missing Node and npm on PATH are both rejected with install guidance", () =>
{
    var originalPath = System.Environment.GetEnvironmentVariable("PATH");
    try
    {
        System.Environment.SetEnvironmentVariable("PATH", System.IO.Path.Combine(System.IO.Path.GetTempPath(),
            "computator-absent-path-" + Guid.NewGuid().ToString("N")));
        foreach (var executable in new[] { "node", "npm" })
            ExpectFailure(() => FrontendToolVersion(executable, frontendDirectory), executable, "version", "Install", "PATH");
    }
    finally { System.Environment.SetEnvironmentVariable("PATH", originalPath); }
});

Check("real inherited Node/npm probes match the requested CI combination", () =>
{
    var node = FrontendToolVersion("node", frontendDirectory);
    var npm = FrontendToolVersion("npm", frontendDirectory);
    if (expectedNode.Length != 0) Equal(expectedNode, node, "Inherited Node");
    if (expectedNpm.Length != 0) Equal(expectedNpm, npm, "Inherited npm");
    var expectedPrefix = node == nodePin && npm == npmPin ? "" : pinnedPrefix;
    Equal(expectedPrefix, SelectFrontendCommandPrefix(nodePin, npmPin, node, npm), "Native command selection");
    Information("Native probe evidence: Node {0}; npm {1}.", node, npm);
});

Check("real Cake process reproduces the timed-wait race and the reader drains it", () =>
{
    using (var entered = new ManualResetEventSlim(false))
    using (var release = new ManualResetEventSlim(false))
    using (var process = StartAndReturnProcess("node", new ProcessSettings
    {
        Arguments = "--version", WorkingDirectory = frontendDirectory,
        RedirectStandardOutput = true, RedirectStandardError = true,
        RedirectedStandardOutputHandler = line =>
        {
            if (line != null)
            {
                entered.Set();
                release.Wait(10000); // Safety bound if an assertion fails before releasing the handler.
            }
            return line;
        }
    }))
    {
        try
        {
            Assert(entered.Wait(5000), "The real stdout callback did not begin.");
            Assert(process.WaitForExit(5000), "The short-lived version process did not exit.");
            Equal(0, process.GetStandardOutput().Count(),
                "Timed WaitForExit must expose the reproduced empty-queue window");
            // Release the real callback only when the production reader requests
            // the parameterless drain. Removing that drain deterministically fails.
            var gated = new DrainGatedProcess(process, release);
            var actual = ReadFrontendToolVersion(gated, "node", 5000);
            Equal(FrontendToolVersion("node", frontendDirectory), actual, "Drained real process output");
        }
        finally
        {
            release.Set();
            if (!process.WaitForExit(5000)) process.Kill();
            process.WaitForExit();
        }
    }
});

Information("Frontend toolchain regression suite: {0} checks passed.", passed);

class VersionProcess : Cake.Core.IO.IProcess
{
    private readonly string[] output;
    private readonly string[] errors;
    private readonly int exitCode;
    private readonly bool completes;
    public int TimedWaitMilliseconds { get; private set; }
    public int DrainCalls { get; private set; }
    public int KillCalls { get; private set; }
    public int OutputReads { get; private set; }
    public int ErrorReads { get; private set; }
    public bool ReadBeforeDrain { get; private set; }
    public bool Disposed { get; private set; }

    public VersionProcess(string[] output, string[] errors = null, int exitCode = 0, bool completes = true)
    {
        this.output = output;
        this.errors = errors ?? new string[0];
        this.exitCode = exitCode;
        this.completes = completes;
    }

    public bool WaitForExit(int milliseconds) { TimedWaitMilliseconds = milliseconds; return completes; }
    public void WaitForExit() { DrainCalls++; }
    public int GetExitCode() { return exitCode; }
    public void Kill() { KillCalls++; }
    public void Dispose() { Disposed = true; }
    public IEnumerable<string> GetStandardOutput()
    {
        OutputReads++;
        ReadBeforeDrain |= DrainCalls == 0;
        return DrainCalls == 0 ? new string[0] : output;
    }
    public IEnumerable<string> GetStandardError()
    {
        ErrorReads++;
        ReadBeforeDrain |= DrainCalls == 0;
        return DrainCalls == 0 ? new string[0] : errors;
    }
}

class DrainGatedProcess : Cake.Core.IO.IProcess
{
    private readonly Cake.Core.IO.IProcess inner;
    private readonly ManualResetEventSlim release;
    public DrainGatedProcess(Cake.Core.IO.IProcess inner, ManualResetEventSlim release)
    {
        this.inner = inner;
        this.release = release;
    }
    public bool WaitForExit(int milliseconds) { return inner.WaitForExit(milliseconds); }
    public void WaitForExit() { release.Set(); inner.WaitForExit(); }
    public int GetExitCode() { return inner.GetExitCode(); }
    public IEnumerable<string> GetStandardOutput() { return inner.GetStandardOutput(); }
    public IEnumerable<string> GetStandardError() { return inner.GetStandardError(); }
    public void Kill() { release.Set(); inner.Kill(); }
    public void Dispose() { release.Set(); inner.Dispose(); }
}
