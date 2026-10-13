var installerVersion = Argument("package-version", "2.4.0"); // Candidate version only; no release is published.
var acceptWix7 = Argument("accept-wix7-eula", false);
var installerProject = "Computator.NET.Installer/Computator.NET.Installer.csproj";
void RunInstallerGenerator(string mode)
{
    var args = new ProcessArgumentBuilder().Append("--mode=" + mode)
        .AppendQuoted("--root=" + MakeAbsolute(Directory(".")).FullPath)
        .AppendQuoted("--payload=" + MakeAbsolute(Directory("artifacts/installer/payload")).FullPath)
        .AppendQuoted("--output=" + MakeAbsolute(Directory("artifacts/installer/output")).FullPath)
        .AppendQuoted("--version=" + installerVersion);
    if (acceptWix7) args.Append("--accept-wix7-eula=true");
    var executable = MakeAbsolute(File("Computator.NET.Installer/bin/" + configuration + "/net481/Computator.NET.Installer.exe"));
    CheckedProcess(executable.FullPath, new ProcessSettings { Arguments = args, WorkingDirectory = MakeAbsolute(Directory("Computator.NET.Installer")) });
}
Task("Installer-Stage").Does(() =>
{
    if (!IsRunningOnWindows() || !modernRuntime) throw new Exception("The installer migration requires the .NET 10 Windows branch.");
    CleanDirectory("artifacts/installer/payload");
    DotNetPublish("Computator.NET/Computator.NET.csproj", new DotNetPublishSettings
    {
        Configuration = configuration, Runtime = "win-x64", SelfContained = true, OutputDirectory = "artifacts/installer/payload",
        ArgumentCustomization = a => a.Append("-p:PublishTrimmed=false -p:PublishSingleFile=false -p:PublishAot=false")
    });
    CopyFile("LICENSE", "artifacts/installer/payload/LICENSE");
    PowerShell("build/Test-PublishedNet10Desktop.ps1", "-PublishDirectory artifacts/installer/payload");
});
Task("Installer-Authoring").Does(() =>
{
    DotNetRestore(installerProject, RestoreSettings());
    DotNetBuild(installerProject, new DotNetBuildSettings { Configuration = configuration, NoRestore = true });
    CheckedProcess("dotnet", new ProcessSettings
    {
        Arguments = "tool restore --tool-manifest .config/dotnet-tools.json", WorkingDirectory = "Computator.NET.Installer"
    });
});
Task("Installer-Source").IsDependentOn("Installer-Stage").IsDependentOn("Installer-Authoring").Does(() => RunInstallerGenerator("source"));
Task("Build-Installer").IsDependentOn("Installer-Source").Does(() =>
{
    if (!acceptWix7) throw new Exception("Review WiX 7's current licence terms, then explicitly pass --accept-wix7-eula=true. No acceptance is implied by opening this PR.");
    RunInstallerGenerator("bundle");
});
Task("Installer-Validate").IsDependentOn("Build-Installer").Does(() =>
    PowerShell("build/installer/Test-InstalledProduct.ps1", "-MsiPath artifacts/installer/output/Computator.NET.msi -InventoryPath artifacts/installer/output/payload.xml -DisposableMachine"));
