// One developer solution contains everything. Portable CI intentionally builds
// the portable project graph, not Windows projects or a second solution file.
var portableProjects = new[] { "Computator.NET.Engine.Tests/Computator.NET.Engine.Tests.csproj", "Computator.NET.Cli/Computator.NET.Cli.csproj" };
Task("Portable-Restore")
    .Description("Any OS: restore the portable engine/test and CLI project graphs.")
    .Does(() =>
{
    foreach (var project in portableProjects) DotNetRestore(project, RestoreSettings());
});
Task("Portable-Build")
    .Description("Any OS: build the portable engine/test and CLI project graphs.")
    .IsDependentOn("Portable-Restore").Does(() =>
{
    foreach (var project in portableProjects)
        DotNetBuild(project, new DotNetBuildSettings { Configuration = configuration, NoRestore = true });
});
Task("Portable-Test")
    .Description("Any OS: build and test the managed engine with a test-discovery guard.")
    .IsDependentOn("Portable-Build").Does(() =>
{
    var report = "TestResults/portable.trx";
    if (FileExists(report)) DeleteFile(report);
    DotNetTest(portableProjects[0], new DotNetTestSettings
    {
        Configuration = configuration, NoBuild = true, NoRestore = true, ResultsDirectory = "TestResults",
        Loggers = new[] { "trx;LogFileName=portable.trx" }
    });
    PowerShell("build/Assert-TestResults.ps1", "-Path " + report + " -MinimumTests 605");
});
Task("Portable-Publish")
    .Description("Any OS: test, publish and exercise the portable CLI process interface.")
    .IsDependentOn("Portable-Test").Does(() =>
{
    CleanDirectory("artifacts/portable");
    DotNetPublish(portableProjects[1], new DotNetPublishSettings
    {
        Configuration = configuration, NoRestore = true, OutputDirectory = "artifacts/portable"
    });
    PowerShell("build/portable/Test-Cli.ps1", "-PublishDirectory artifacts/portable");
});
Task("Portable-Validate")
    .Description("Any OS: run the complete portable engine and published CLI validation.")
    .IsDependentOn("Portable-Publish");
