// The Store reserves its fourth component; the assembly's date/time revision
// remains meaningful in the application but must not enter package identity.
var canonicalMsixVersion = XDocument.Load("Version.props").Descendants("ComputatorBaseVersion").Single().Value + ".0";
var msixVersion = Argument("msix-version", canonicalMsixVersion);
Task("Build-MSIX")
    .Description("Windows: stage the self-contained desktop and build its unsigned MSIX.")
    .IsDependentOn("Distribution-Stage").Does(() =>
{
    if (msixVersion != canonicalMsixVersion) throw new Exception("Set the shared release identity in Version.props rather than overriding only the Store package version.");
    PowerShell("build/msix/Test-MsixInputs.ps1");
    PowerShell("build/msix/Build-Msix.ps1", "-PayloadDirectory " + distributionDirectory + " -Version \"" + msixVersion + "\"");
});
Task("Build-Uwp")
    .Description("Historical alias for Build-MSIX; creates the unsigned desktop MSIX.")
    .IsDependentOn("Build-MSIX");
Task("MSIX-Validate")
    .Description("Windows: build and validate unsigned MSIX inputs and package layout.")
    .IsDependentOn("Build-MSIX");
Task("MSIX-Lifecycle")
    .Description("Disposable Windows: install, activate and remove an explicitly supplied signed MSIX.")
    .Does(() =>
{
    var signedPackage = Argument("signed-package", "");
    if (string.IsNullOrWhiteSpace(signedPackage)) throw new Exception("Pass --signed-package for an owner-signed package on an explicitly disposable machine.");
    PowerShell("build/msix/Test-MsixLifecycle.ps1", "-PackagePath \"" + signedPackage + "\" -DisposableMachine");
});
