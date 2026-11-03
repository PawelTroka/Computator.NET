Task("Release-Prepare")
    .Description("Freeze classic .NET date/time build numbers once; review and commit before candidate CI.")
    .Does(() =>
{
    var args = new ProcessArgumentBuilder().Append("build/release/versioning.py --prepare");
    var timestamp = Argument<string>("release-timestamp", null);
    var published = Argument<string>("release-published-releases", null);
    if (timestamp != null) args.Append("--timestamp").AppendQuoted(timestamp);
    if (published != null) args.Append("--published-releases").AppendQuoted(published);
    CheckedProcess("python", new ProcessSettings { Arguments = args });
    Information("Release identity is frozen. Review and commit Version.props and SharedAssemblyInfo.cs before CI; this target does not publish or sign.");
});
Task("Release-Identity-Validate")
    .Description("Check the shared product version identity and release metadata contracts.")
    .Does(() =>
{
    CheckedProcess("python", new ProcessSettings { Arguments = "build/release/versioning.py" });
    CheckedProcess("python", new ProcessSettings { Arguments = "-m unittest discover -s build/release -p test_*.py" });
});
Task("Release-Verify")
    .Description("Windows: gather full candidate evidence; separate native/portable matrices remain required.")
    .IsDependentOn("Release-Identity-Validate").Does(() =>
{
    if (!IsRunningOnWindows()) throw new Exception("This candidate profile verifies Windows outputs. Portable/native OS matrices remain separate required evidence.");
    if (suite != "All") throw new Exception("A release candidate cannot silently verify only one selected suite.");
    CheckedProcess("python", new ProcessSettings { Arguments = "build/release/evidence.py begin" });
    // Separate Cake invocations deliberately isolate profile/env/output state:
    // focused browser tests use their original direct-API mocks; real-stack tests
    // rebuild with their protected same-origin profile. Neither conceals the other.
    var targets = new List<string> { "Reference-Validate", "Publish", "Web-E2E", "Web-CustomFunctions", "Portable-Validate", "Installer-Source", "MSIX-Validate" };
    if (FileExists("build/followups/local-web.cake")) targets.Add("Local-Web-Package");
    foreach (var selected in targets)
        CheckedProcess("dotnet", new ProcessSettings
        {
            Arguments = new ProcessArgumentBuilder().Append("cake build.cake").Append("--target=" + selected).Append("--configuration=" + configuration),
            // Reject focused tests and pre-existing dev servers even on a local
            // host: every report must exercise this candidate's own processes.
            EnvironmentVariables = new Dictionary<string, string> { { "CI", "true" } }
        });
    CheckedProcess("python", new ProcessSettings { Arguments = "build/release/evidence.py finish" });
});
