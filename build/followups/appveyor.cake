Task("AppVeyor-Preflight").Does(() =>
{
    if (!IsRunningOnWindows()) throw new Exception("AppVeyor's assigned role is the Windows build/packaging graph.");
    CheckedProcess("python", new ProcessSettings { Arguments = "-m unittest discover -s build/ci -p test_ci_helpers.py" });
    CheckedProcess("python", new ProcessSettings { Arguments = "build/ci/preflight.py --provider appveyor" });
});
Task("AppVeyor-Validate").IsDependentOn("AppVeyor-Preflight").IsDependentOn("Publish").Does(() =>
{
    CheckedProcess("python", new ProcessSettings { Arguments = "build/ci/preflight.py --provider appveyor --complete" });
    foreach (var report in GetFiles("TestResults/*.trx")) AppVeyor.UploadTestResults(report, AppVeyorTestResultsType.MSTest);
});
Task("AppVeyor-Mirror").Does(() =>
{
    // A GitHub/local mirror tests scripts and artifacts, never fabricates an AppVeyor run.
    CheckedProcess("python", new ProcessSettings { Arguments = "-m unittest discover -s build/ci -p test_ci_helpers.py" });
    CheckedProcess("python", new ProcessSettings { Arguments = "build/ci/preflight.py --provider local" });
});
