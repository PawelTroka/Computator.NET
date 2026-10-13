Task("Portable-Restore").Does(() => DotNetRestore("Computator.NET.Portable.slnx", RestoreSettings()));
Task("Portable-Build").IsDependentOn("Portable-Restore").Does(() =>
    DotNetBuild("Computator.NET.Portable.slnx", new DotNetBuildSettings { Configuration = configuration, NoRestore = true }));
Task("Portable-Test").IsDependentOn("Portable-Build").Does(() =>
{
    var report = "TestResults/portable.trx";
    if (FileExists(report)) DeleteFile(report);
    DotNetTest("Computator.NET.Engine.Tests/Computator.NET.Engine.Tests.csproj", new DotNetTestSettings
    {
        Configuration = configuration, NoBuild = true, NoRestore = true, ResultsDirectory = "TestResults",
        Loggers = new[] { "trx;LogFileName=portable.trx" }
    });
    PowerShell("build/Assert-TestResults.ps1", "-Path " + report + " -MinimumTests 100");
});
Task("Portable-Publish").IsDependentOn("Portable-Test").Does(() =>
{
    CleanDirectory("artifacts/portable");
    DotNetPublish("Computator.NET.Cli/Computator.NET.Cli.csproj", new DotNetPublishSettings
    {
        Configuration = configuration, NoRestore = true, OutputDirectory = "artifacts/portable"
    });
    PowerShell("build/portable/Test-Cli.ps1", "-PublishDirectory artifacts/portable");
});
Task("Portable-Validate").IsDependentOn("Portable-Publish");
