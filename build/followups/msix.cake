var canonicalMsixVersion = XDocument.Load("Version.props").Descendants("ComputatorBaseVersion").Single().Value + "." +
    XDocument.Load("Version.props").Descendants("ComputatorRevision").Single().Value;
var msixVersion = Argument("msix-version", canonicalMsixVersion);
Task("Build-MSIX").IsDependentOn("Distribution-Stage").Does(() =>
{
    if (msixVersion != canonicalMsixVersion) throw new Exception("Set the shared release identity in Version.props rather than overriding only the Store package version.");
    PowerShell("build/msix/Test-MsixInputs.ps1");
    PowerShell("build/msix/Build-Msix.ps1", "-PayloadDirectory " + distributionDirectory + " -Version \"" + msixVersion + "\"");
});
Task("Build-Uwp").IsDependentOn("Build-MSIX");
Task("MSIX-Validate").IsDependentOn("Build-MSIX");
Task("MSIX-Lifecycle").Does(() =>
{
    var signedPackage = Argument("signed-package", "");
    if (string.IsNullOrWhiteSpace(signedPackage)) throw new Exception("Pass --signed-package for an owner-signed package on an explicitly disposable machine.");
    PowerShell("build/msix/Test-MsixLifecycle.ps1", "-PackagePath \"" + signedPackage + "\" -DisposableMachine");
});
