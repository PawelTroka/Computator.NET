// Content authors use these explicitly. Ordinary application builds only copy
// committed functions.xml; they never scrape, rewrite content or contact sources.
void ReferenceTest(string filter, string logger)
{
    var old = EnvironmentVariable("COMPUTATOR_REFERENCE_INVENTORY");
    var inventory = System.IO.Path.GetFullPath("artifacts/reference/inventory.json");
    EnsureDirectoryExists("artifacts/reference");
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
    if (!FileExists(inventory)) throw new Exception("Runtime reference inventory was not produced.");
}
void MaterializeReference(bool check)
{
    CheckedProcess("python", new ProcessSettings { Arguments = "-m unittest discover -s build/reference -p test_*.py" });
    CheckedProcess("python", new ProcessSettings { Arguments = "build/reference/generate.py --inventory artifacts/reference/inventory.json --output Computator.NET.Core/Static/functions.xml --preview artifacts/reference/preview.html --report artifacts/reference/coverage.json" + (check ? " --check" : "") });
}
Task("Reference-Generate").IsDependentOn("Build").Does(() =>
{
    ReferenceTest("FullyQualifiedName~FunctionReferenceCoverageTests.ExportRuntimeCatalogForReview", "reference-inventory.trx");
    MaterializeReference(false);
});
Task("Reference-Validate").IsDependentOn("Build").Does(() =>
{
    ReferenceTest("FullyQualifiedName~FunctionReferenceCoverageTests", "reference.trx");
    MaterializeReference(true);
});
