Task("AppVeyor-Preflight")
    .Description("AppVeyor Windows: verify exact tool versions and native provider identity.")
    .Does(() =>
{
    if (!IsRunningOnWindows()) throw new Exception("AppVeyor's assigned role is the Windows build/packaging graph.");
    CheckedProcess("python", new ProcessSettings { Arguments = "-m unittest discover -s build/ci -p test_ci_helpers.py" });
    CheckedProcess("python", new ProcessSettings { Arguments = "build/ci/preflight.py --provider appveyor" });
});
Task("AppVeyor-Validate")
    .Description("AppVeyor Windows: validate packages, suites and native provider evidence.")
    .IsDependentOn("AppVeyor-Preflight").IsDependentOn("Publish").Does(() =>
{
    CheckedProcess("python", new ProcessSettings { Arguments = "build/ci/preflight.py --provider appveyor --complete" });
    foreach (var report in GetFiles("TestResults/*.trx")) AppVeyor.UploadTestResults(report, AppVeyorTestResultsType.MSTest);
});
Task("AppVeyor-Mirror")
    .Description("Validate provider helper scripts and local evidence; not an AppVeyor run.")
    .Does(() =>
{
    // A GitHub/local mirror tests scripts and artifacts, never fabricates an AppVeyor run.
    CheckedProcess("python", new ProcessSettings { Arguments = "-m unittest discover -s build/ci -p test_ci_helpers.py" });
    CheckedProcess("python", new ProcessSettings { Arguments = "build/ci/preflight.py --provider local" });
});
