// Shared distribution payload for MSI and MSIX.
var distributionDirectory = "artifacts/distribution/win-x64";
Task("Desktop-Smoke-Contracts")
    .Description("Windows: validate published-desktop smoke-test contracts.")
    .Does(() =>
{
    if (!IsRunningOnWindows()) throw new Exception("Desktop smoke contracts require Windows.");
    PowerShell("build/Test-PublishedNet10Desktop.ps1", "-TestContracts");
});
Task("Distribution-Stage")
    .Description("Windows: publish and smoke-test the self-contained desktop distribution.")
    .IsDependentOn("Native-GSL").IsDependentOn("Desktop-Smoke-Contracts").Does(() =>
{
    if (!IsRunningOnWindows() || !modernRuntime) throw new Exception("Desktop distribution staging requires .NET 10 on Windows.");
    CleanDirectory(distributionDirectory);
    DotNetPublish("Computator.NET/Computator.NET.csproj", new DotNetPublishSettings
    {
        Configuration = configuration, Runtime = "win-x64", SelfContained = true, OutputDirectory = distributionDirectory,
        ArgumentCustomization = a => a.Append("-p:PublishTrimmed=false -p:PublishSingleFile=false -p:PublishAot=false")
    });
    CopyFile("LICENSE", distributionDirectory + "/LICENSE");
    RequirePayload(distributionDirectory, new[] { "Computator.NET.exe", "Computator.NET.dll", "coreclr.dll", "hostfxr.dll",
        "Microsoft.CodeAnalysis.CSharp.dll", "Static/functions.xml", "pl/Computator.NET.Localization.resources.dll", "LICENSE" });
    StageGslPayload(distributionDirectory);
    PowerShell("build/Test-PublishedNet10Desktop.ps1", "-PublishDirectory " + distributionDirectory);
});
