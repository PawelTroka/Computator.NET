var productVersionDocument = XDocument.Load("Version.props");
var canonicalMsiVersion = productVersionDocument.Descendants("ComputatorBaseVersion").Single().Value;
var installerVersion = Argument("package-version", canonicalMsiVersion);
var acceptWix7 = Argument("accept-wix7-eula", false);
var installerProject = "Computator.NET.Installer/Computator.NET.Installer.csproj";
void RunInstallerGenerator(string mode)
{
    if (installerVersion != canonicalMsiVersion) throw new Exception("Set the product identity in Version.props; an independent MSI version would create inconsistent release artifacts.");
    var args = new ProcessArgumentBuilder().Append("--mode=" + mode)
        .AppendQuoted("--root=" + MakeAbsolute(Directory(".")).FullPath)
        .AppendQuoted("--payload=" + MakeAbsolute(Directory(distributionDirectory)).FullPath)
        .AppendQuoted("--output=" + MakeAbsolute(Directory("artifacts/installer/output")).FullPath)
        .AppendQuoted("--version=" + installerVersion);
    if (acceptWix7) args.Append("--accept-wix7-eula=true");
    var executable = MakeAbsolute(File("Computator.NET.Installer/bin/" + configuration + "/net481/Computator.NET.Installer.exe"));
    CheckedProcess(executable.FullPath, new ProcessSettings { Arguments = args, WorkingDirectory = MakeAbsolute(Directory("Computator.NET.Installer")) });
}
Task("Installer-Stage")
    .Description("Windows: stage the shared self-contained desktop payload for installer authoring.")
    .IsDependentOn("Distribution-Stage");
Task("Installer-Authoring")
    .Description("Windows: restore and build installer authoring tools without installing a product.")
    .Does(() =>
{
    DotNetRestore(installerProject, RestoreSettings());
    DotNetBuild(installerProject, new DotNetBuildSettings { Configuration = configuration, NoRestore = true });
    CheckedProcess("dotnet", new ProcessSettings { Arguments = "tool restore --tool-manifest .config/dotnet-tools.json", WorkingDirectory = "Computator.NET.Installer" });
});
Task("Installer-Source")
    .Description("Windows: generate and validate WiX source from the staged payload.")
    .IsDependentOn("Installer-Stage").IsDependentOn("Installer-Authoring").Does(() => RunInstallerGenerator("source"));
Task("Build-Installer")
    .Description("Windows: build unsigned MSI/EXE; requires explicit WiX terms.")
    .IsDependentOn("Installer-Source").Does(() =>
{
    if (!acceptWix7) throw new Exception("Review WiX 7's current licence terms, then explicitly pass --accept-wix7-eula=true.");
    RunInstallerGenerator("bundle");
});
Task("Installer-Validate")
    .Description("Disposable Windows: build and install the candidate MSI for acceptance.")
    .IsDependentOn("Build-Installer").Does(() =>
    PowerShell("build/installer/Test-InstalledProduct.ps1", "-MsiPath artifacts/installer/output/Computator.NET.msi -InventoryPath artifacts/installer/output/payload.xml -DisposableMachine"));

void WindowsAcceptancePowerShell(ProcessArgumentBuilder args)
{
    if (!IsRunningOnWindows()) throw new Exception("Windows PowerShell acceptance requires Windows.");
    var home = System.IO.Path.Combine(EnvironmentVariable("WINDIR"), "System32", "WindowsPowerShell", "v1.0");
    // pwsh -> dotnet/Cake -> powershell does not apply pwsh's WinPSModulePath
    // filtering. Restrict these built-in-only tests to the guest engine's modules.
    CheckedProcess(System.IO.Path.Combine(home, "powershell.exe"), new ProcessSettings
    {
        Arguments = args,
        EnvironmentVariables = new Dictionary<string, string> { { "PSModulePath", System.IO.Path.Combine(home, "Modules") } }
    });
}
// Acceptance can consume an already approved, unsigned candidate. It does not
// rebuild, accept tool terms, sign, publish, or install on a developer machine.
Task("Installer-Historical-Fetch")
    .Description("Fetch and verify the authentic historical installer used as an upgrade control.")
    .Does(() =>
    PowerShell("build/acceptance/Get-HistoricalInstaller.ps1"));
Task("Installer-Historical-Validate")
    .Description("Disposable Windows: exercise candidate install and historical upgrade acceptance.")
    .Does(() =>
{
    if (!IsRunningOnWindows()) throw new Exception("Historical installer acceptance requires disposable Windows.");
    var artifacts = Argument("historical-candidate-directory", "artifacts/installer/output");
    var legacy = Argument("historical-bundle", "artifacts/installer/historical/Computator.NET.Installer.v2.3.2.exe");
    var results = Argument("historical-results", "artifacts/installer/logs/historical-" + Guid.NewGuid().ToString("N"));
    var args = new ProcessArgumentBuilder().Append("-NoProfile -File").AppendQuoted("build/acceptance/Test-HistoricalUpgrade.ps1")
        .Append("-MsiPath").AppendQuoted(artifacts + "/Computator.NET.msi")
        .Append("-InventoryPath").AppendQuoted(artifacts + "/payload.xml")
        .Append("-CandidateBundlePath").AppendQuoted(artifacts + "/Computator.NET.Installer.exe")
        .Append("-HistoricalBundlePath").AppendQuoted(legacy)
        .Append("-ResultDirectory").AppendQuoted(results).Append("-DisposableMachine");
    WindowsAcceptancePowerShell(args);
});
