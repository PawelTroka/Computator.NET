Task("Program-Lifetime-Validate")
    .Description("Windows: build and run focused compiled-program lifetime regression tests.")
    .IsDependentOn("Build").Does(() =>
{
    var report = "TestResults/program-lifetime.trx";
    if (FileExists(report)) DeleteFile(report);
    DotNetTest("Computator.NET.Core.Tests/Computator.NET.Core.Tests.csproj", new DotNetTestSettings
    {
        Configuration = configuration, NoBuild = true, NoRestore = true,
        Filter = "FullyQualifiedName~ProgramLifetimeTests", ResultsDirectory = "TestResults",
        Loggers = new[] { "trx;LogFileName=program-lifetime.trx", "console;verbosity=normal" }
    });
    PowerShell("build/Assert-TestResults.ps1", "-Path " + report + " -MinimumTests 7");
});
