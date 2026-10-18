// Content authors use these explicitly. Ordinary application builds only copy
// committed functions.xml; they never scrape, rewrite content or contact sources.
void ReferenceTest(string filter, string logger, int minimumTests)
{
    var old = EnvironmentVariable("COMPUTATOR_REFERENCE_INVENTORY");
    var inventory = System.IO.Path.GetFullPath("artifacts/reference/inventory.json");
    var report = "TestResults/" + logger;
    EnsureDirectoryExists("artifacts/reference");
    // A filter that discovers no tests must not reuse a previous runtime catalog.
    if (FileExists(inventory)) DeleteFile(inventory);
    if (FileExists(report)) DeleteFile(report);
    System.Environment.SetEnvironmentVariable("COMPUTATOR_REFERENCE_INVENTORY", inventory);
    try
    {
        DotNetTest("Computator.NET.Core.Tests/Computator.NET.Core.Tests.csproj", new DotNetTestSettings
        {
            Configuration = configuration, NoBuild = true, NoRestore = true,
            Filter = filter, ResultsDirectory = "TestResults",
            Loggers = new[] { "trx;LogFileName=" + logger },
            ArgumentCustomization = a => a.Append("--blame-hang-timeout 120s --blame-hang-dump-type none")
        });
    }
    finally { System.Environment.SetEnvironmentVariable("COMPUTATOR_REFERENCE_INVENTORY", old); }
    PowerShell("build/Assert-TestResults.ps1", "-Path " + report + " -MinimumTests " + minimumTests);
    if (!FileExists(inventory)) throw new Exception("Runtime reference inventory was not produced.");
}
void MaterializeReference(bool check)
{
    CheckedProcess("python", new ProcessSettings { Arguments = "build/reference/generate.py --inventory artifacts/reference/inventory.json --equations-only artifacts/reference/equations.json" });
    CheckedProcess("node", new ProcessSettings { Arguments = "build/reference/typeset.cjs --input artifacts/reference/equations.json --output Computator.NET.Core/Static/reference-equations" + (check ? " --check" : "") });
    CheckedProcess("python", new ProcessSettings { Arguments = "-m unittest discover -s build/reference -p test_*.py" });
    CheckedProcess("python", new ProcessSettings { Arguments = "build/reference/generate.py --inventory artifacts/reference/inventory.json --output Computator.NET.Core/Static/functions.xml --preview artifacts/reference/preview.html --report artifacts/reference/coverage.json" + (check ? " --check" : "") });
}
Task("Reference-Generate").IsDependentOn("Build").IsDependentOn("Web-Restore").Does(() =>
{
    ReferenceTest("FullyQualifiedName~FunctionReferenceCoverageTests.ExportRuntimeCatalogForReview", "reference-inventory.trx", 1);
    MaterializeReference(false);
});
Task("Reference-Validate").IsDependentOn("Build").IsDependentOn("Web-Restore").Does(() =>
{
    ReferenceTest("FullyQualifiedName~AutocompletionTests.FunctionReference", "reference.trx", 153);
    MaterializeReference(true);
});
