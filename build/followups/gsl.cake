Task("Native-GSL").Does(() =>
    CheckedProcess(IsRunningOnWindows() ? "python" : "python3", new ProcessSettings
    {
        Arguments = "build/native/build_gsl.py",
        EnvironmentVariables = new Dictionary<string, string> { { "VCPKG_DISABLE_METRICS", "1" } }
    }));
Task("Native-GSL-Validate").IsDependentOn("Native-GSL").Does(() =>
{
    if (!IsRunningOnWindows()) return; // Native C/managed ABI probes already ran on this OS.
    DotNetRestore(solution, RestoreSettings());
    DotNetBuild(solution, new DotNetBuildSettings { Configuration = configuration, NoRestore = true });
    var previous = EnvironmentVariable("COMPUTATOR_GSL_DIRECTORY");
    try
    {
        System.Environment.SetEnvironmentVariable("COMPUTATOR_GSL_DIRECTORY", MakeAbsolute(Directory("artifacts/gsl/win-x64/native")).FullPath);
        RunSuite("Computator.NET.Core.IntegrationTests");
        RunSuite("Computator.NET.Desktop.Tests");
    }
    finally { System.Environment.SetEnvironmentVariable("COMPUTATOR_GSL_DIRECTORY", previous); }
});
