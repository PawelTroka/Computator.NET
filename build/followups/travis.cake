Task("Travis-Preflight")
    .Description("Travis Linux: verify helper contracts, exact tool versions and provider identity.")
    .Does(() =>
{
    if (IsRunningOnWindows()) throw new Exception("Travis's assigned role is the complementary Linux web/portable graph.");
    CheckedProcess("python3", new ProcessSettings { Arguments = "-m unittest discover -s build/ci -p test_ci_helpers.py" });
    CheckedProcess("python3", new ProcessSettings { Arguments = "build/ci/preflight.py --provider travis" });
});
Task("Travis-Validate")
    .Description("Travis Linux: validate the frontend and require genuine provider evidence.")
    .IsDependentOn("Travis-Preflight").IsDependentOn("Web-Validate").Does(() =>
    CheckedProcess("python3", new ProcessSettings { Arguments = "build/ci/preflight.py --provider travis --complete" }));
Task("Travis-Mirror-Preflight")
    .Description("Validate Travis helper scripts and tool versions with truthful local evidence.")
    .Does(() =>
{
    CheckedProcess("python3", new ProcessSettings { Arguments = "-m unittest discover -s build/ci -p test_ci_helpers.py" });
    CheckedProcess("python3", new ProcessSettings { Arguments = "build/ci/preflight.py --provider local" });
});
Task("Travis-Mirror")
    .Description("Run the frontend validation using Travis scripts; not a native Travis run.")
    .IsDependentOn("Travis-Mirror-Preflight").IsDependentOn("Web-Validate").Does(() =>
    CheckedProcess("python3", new ProcessSettings { Arguments = "build/ci/preflight.py --provider local --complete" }));
