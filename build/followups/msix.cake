var msixVersion = Argument("msix-version", "2.4.0.0"); // Candidate, never automatically published to the Store.
Task("Build-MSIX").IsDependentOn("Distribution-Stage").Does(() =>
{
    PowerShell("build/msix/Test-MsixInputs.ps1");
    PowerShell("build/msix/Build-Msix.ps1", "-PayloadDirectory " + distributionDirectory + " -Version \"" + msixVersion + "\"");
});
Task("Build-Uwp").IsDependentOn("Build-MSIX"); // Historical target now produces a full-trust desktop MSIX, not a UWP rewrite.
Task("MSIX-Validate").IsDependentOn("Build-MSIX"); // Packaging includes SDK validation and unpacked hash verification.
Task("MSIX-Lifecycle").Does(() =>
{
    var signedPackage = Argument("signed-package", "");
    if (string.IsNullOrWhiteSpace(signedPackage)) throw new Exception("Pass --signed-package for an owner-signed package on an explicitly disposable machine.");
    PowerShell("build/msix/Test-MsixLifecycle.ps1", "-PackagePath \"" + signedPackage + "\" -DisposableMachine");
});
