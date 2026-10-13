Task("Travis-Preflight").Does(() =>
{
    if (IsRunningOnWindows()) throw new Exception("Travis's assigned role is the complementary Linux web/portable graph.");
    CheckedProcess("python3", new ProcessSettings { Arguments = "-m unittest discover -s build/ci -p test_ci_helpers.py" });
    CheckedProcess("python3", new ProcessSettings { Arguments = "build/ci/preflight.py --provider travis" });
});
Task("Travis-Validate").IsDependentOn("Travis-Preflight").IsDependentOn("Web-Validate").Does(() =>
    CheckedProcess("python3", new ProcessSettings { Arguments = "build/ci/preflight.py --provider travis --complete" }));
Task("Travis-Mirror-Preflight").Does(() =>
{
    CheckedProcess("python3", new ProcessSettings { Arguments = "-m unittest discover -s build/ci -p test_ci_helpers.py" });
    CheckedProcess("python3", new ProcessSettings { Arguments = "build/ci/preflight.py --provider local" });
});
Task("Travis-Mirror").IsDependentOn("Travis-Mirror-Preflight").IsDependentOn("Web-Validate").Does(() =>
    CheckedProcess("python3", new ProcessSettings { Arguments = "build/ci/preflight.py --provider local --complete" }));
