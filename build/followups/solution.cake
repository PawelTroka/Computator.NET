Task("Solution-Check")
    .Description("Check solution visibility and IDE project items without restoring or building projects.")
    .Does(() =>
{
    PowerShell("build/Assert-Solution.ps1");
    PowerShell("build/Test-SolutionGuard.ps1");
    PowerShell("build/Assert-ProjectItems.ps1");
    CheckedProcess("dotnet", new ProcessSettings { Arguments = "sln Computator.NET.slnx list" });
});

Task("Solution-Validate")
    .Description("Check the developer workspace and build every supported project on Windows.")
    .IsDependentOn("Solution-Check")
    .IsDependentOn("Build");
