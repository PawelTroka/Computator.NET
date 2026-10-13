Task("Solution-Validate").IsDependentOn("Build").Does(() =>
{
    PowerShell("build/Assert-Solution.ps1");
    CheckedProcess("dotnet", new ProcessSettings { Arguments = "sln Computator.NET.slnx list" });
});
