// Shared by the build and the process-level regression suite.
string FrontendToolVersion(string executable, string workingDirectory)
{
    try
    {
        var commandScript = IsRunningOnWindows() && executable == "npm";
        using (var process = StartAndReturnProcess(commandScript ? "cmd.exe" : executable, new ProcessSettings
        {
            Arguments = commandScript ? "/d /c npm --version" : "--version",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        }))
            return ReadFrontendToolVersion(process, executable);
    }
    catch (Exception ex)
    {
        throw new Exception("Could not determine " + executable + " --version: " + ex.Message +
            " Install the Node/npm versions from .nvmrc and packageManager, restart the IDE so it sees the updated PATH, and retry.", ex);
    }
}

string ReadFrontendToolVersion(IProcess process, string executable, int timeoutMilliseconds = 30000)
{
    if (process == null) throw new Exception("Could not start " + executable + " --version. Check that it is on PATH.");
    if (!process.WaitForExit(timeoutMilliseconds))
    {
        process.Kill();
        throw new TimeoutException(executable + " --version timed out after " + timeoutMilliseconds + " ms.");
    }

    // Cake 6.3's timed StartProcess overload only waits for process exit. Its
    // asynchronous output callbacks may still be pending, and the output queue
    // can look empty. The parameterless wait drains both redirected streams.
    process.WaitForExit();
    var output = string.Join("\n", process.GetStandardOutput()).Trim();
    var errors = string.Join("\n", process.GetStandardError()).Trim();
    var exitCode = process.GetExitCode();
    if (exitCode != 0)
        throw new Exception(executable + " --version failed with exit code " + exitCode +
            ". stderr: " + FrontendProbeDiagnostic(errors));

    var match = System.Text.RegularExpressions.Regex.Match(output, @"\Av?(?<version>[0-9]+\.[0-9]+\.[0-9]+)\z");
    Version parsed;
    if (!match.Success || !Version.TryParse(match.Groups["version"].Value, out parsed))
        throw new Exception(executable + " --version returned invalid version output: " + FrontendProbeDiagnostic(output) +
            ". stderr: " + FrontendProbeDiagnostic(errors));
    return match.Groups["version"].Value;
}

string FrontendProbeDiagnostic(string text)
{
    if (text.Length == 0) return "<empty>";
    return text.Length > 1024 ? text.Substring(0, 1024) + "..." : text;
}

string SelectFrontendCommandPrefix(string node, string npm, string installedNode, string installedNpm)
{
    // Pins become command arguments. Reject anything except exact stable versions.
    var exactVersion = @"\A[0-9]+\.[0-9]+\.[0-9]+\z";
    Version pinnedNode;
    Version pinnedNpm;
    if (!System.Text.RegularExpressions.Regex.IsMatch(node, exactVersion) || !Version.TryParse(node, out pinnedNode) ||
        !System.Text.RegularExpressions.Regex.IsMatch(npm, exactVersion) || !Version.TryParse(npm, out pinnedNpm))
        throw new Exception("Frontend tools require an exact Node version in .nvmrc and npm@x.y.z in packageManager.");

    Version nodeVersion;
    Version npmVersion;
    // Older releases within the selected Node major can bootstrap the exact pins.
    if (!System.Text.RegularExpressions.Regex.IsMatch(installedNode, exactVersion) || !Version.TryParse(installedNode, out nodeVersion) ||
        nodeVersion.Major < pinnedNode.Major ||
        !System.Text.RegularExpressions.Regex.IsMatch(installedNpm, exactVersion) || !Version.TryParse(installedNpm, out npmVersion) ||
        npmVersion.Major < 7)
        throw new Exception("Automatic frontend tool selection needs Node " + pinnedNode.Major +
            " or newer and npm 7 or newer on PATH. Detected Node '" + installedNode + "', npm '" + installedNpm +
            "'. Install Node " + node + " (npm " + npm + "), restart the IDE so it sees the updated PATH, and retry.");

    if (installedNode == node && installedNpm == npm) return "";
    return "exec --yes --package=node@" + node + " --package=npm@" + npm + " -- ";
}
