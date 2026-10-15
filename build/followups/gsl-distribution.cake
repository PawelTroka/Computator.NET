// Every packaged Windows application adopts the same reviewed native payload.
void StageGslPayload(string outputDirectory)
{
    if (!IsRunningOnWindows()) throw new Exception("The packaged GSL adoption target requires Windows x64.");
    var source = "artifacts/gsl/win-x64/native";
    RequirePayload(source, new[] { "gsl.dll", "gslcblas.dll", "gsl-provenance.json", "COPYING.GSL" });
    var destination = outputDirectory + "/native/gsl";
    EnsureDirectoryExists(destination);
    CopyDirectory(source, destination);
    var report = "TestResults/packaged-gsl/" + Guid.NewGuid().ToString("N") + "/result.json";
    var args = new ProcessArgumentBuilder().Append("run --project build/native/PublishedProbe/PublishedProbe.csproj --configuration")
        .Append(configuration).Append("--").AppendQuoted(MakeAbsolute(Directory(outputDirectory)).FullPath)
        .AppendQuoted(MakeAbsolute(File(report)).FullPath);
    CheckedProcess("dotnet", new ProcessSettings { Arguments = args });
}
Task("GSL-Distribution-Validate").IsDependentOn("Native-GSL-Validate").IsDependentOn("Distribution-Stage");
