// One developer solution contains everything. Portable CI intentionally builds
// the portable project graph, not Windows projects or a second solution file.
var portableProjects = new[] { "Computator.NET.Engine.Tests/Computator.NET.Engine.Tests.csproj", "Computator.NET.Cli/Computator.NET.Cli.csproj" };
Task("Portable-Restore").Does(() =>
{
    foreach (var project in portableProjects) DotNetRestore(project, RestoreSettings());
});
Task("Portable-Build").IsDependentOn("Portable-Restore").Does(() =>
{
    foreach (var project in portableProjects)
        DotNetBuild(project, new DotNetBuildSettings { Configuration = configuration, NoRestore = true });
});
Task("Portable-Test").IsDependentOn("Portable-Build").Does(() =>
{
    var report = "TestResults/portable.trx";
    if (FileExists(report)) DeleteFile(report);
    DotNetTest(portableProjects[0], new DotNetTestSettings
    {
        Configuration = configuration, NoBuild = true, NoRestore = true, ResultsDirectory = "TestResults",
        Loggers = new[] { "trx;LogFileName=portable.trx" }
    });
    PowerShell("build/Assert-TestResults.ps1", "-Path " + report + " -MinimumTests 144");
});
Task("Portable-Publish").IsDependentOn("Portable-Test").Does(() =>
{
    CleanDirectory("artifacts/portable");
    DotNetPublish(portableProjects[1], new DotNetPublishSettings
    {
        Configuration = configuration, NoRestore = true, OutputDirectory = "artifacts/portable"
    });
    PowerShell("build/portable/Test-Cli.ps1", "-PublishDirectory artifacts/portable");
});
Task("Portable-Validate").IsDependentOn("Portable-Publish");
