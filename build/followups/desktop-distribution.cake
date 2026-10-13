// Shared distribution payload for MSI and MSIX; identical in both independent PRs.
var distributionDirectory = "artifacts/distribution/win-x64";
Task("Distribution-Stage").Does(() =>
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
    PowerShell("build/Test-PublishedNet10Desktop.ps1", "-PublishDirectory " + distributionDirectory);
});
