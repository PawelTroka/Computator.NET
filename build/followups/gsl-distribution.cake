// Explicit adoption target: source-build provenance accompanies the tested payload.
Task("GSL-Distribution-Validate").IsDependentOn("Native-GSL-Validate").IsDependentOn("Distribution-Stage").Does(() =>
{
    var source = "artifacts/gsl/win-x64/native";
    var destination = distributionDirectory + "/native/gsl";
    EnsureDirectoryExists(destination);
    CopyDirectory(source, destination);
    RequirePayload(destination, new[] { "gsl.dll", "gslcblas.dll", "gsl-provenance.json" });
    // Clear the opt-in override: this must select the packaged default, not the build tree.
    var old = EnvironmentVariable("COMPUTATOR_GSL_DIRECTORY");
    try
    {
        System.Environment.SetEnvironmentVariable("COMPUTATOR_GSL_DIRECTORY", null);
        PowerShell("build/Test-PublishedNet10Desktop.ps1", "-PublishDirectory " + distributionDirectory);
    }
    finally { System.Environment.SetEnvironmentVariable("COMPUTATOR_GSL_DIRECTORY", old); }
});
