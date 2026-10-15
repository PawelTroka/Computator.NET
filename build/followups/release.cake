Task("Release-Identity-Validate").Does(() =>
{
    CheckedProcess("python", new ProcessSettings { Arguments = "build/release/versioning.py" });
    CheckedProcess("python", new ProcessSettings { Arguments = "-m unittest discover -s build/release -p test_release.py" });
});
Task("Release-Verify").IsDependentOn("Release-Identity-Validate").Does(() =>
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
