Task("MSIX-Activation-Contracts")
    .Description("Validate MSIX activation and launch contracts without installing a package.")
    .Does(() => PowerShell("build/msix/Test-ActivationContracts.ps1"));
Task("MSIX-Signed-Acceptance")
    .Description("Disposable Windows: install and test an explicitly supplied signed MSIX.")
    .IsDependentOn("MSIX-Activation-Contracts").Does(() =>
{
    var package = Argument("signed-package", "");
    var signer = Argument("signer-thumbprint", "");
    var baseline = Argument("baseline-package", "");
    if (string.IsNullOrWhiteSpace(package) || string.IsNullOrWhiteSpace(signer)) throw new Exception("Pass an owner-approved signed package and signer thumbprint.");
    var args = new ProcessArgumentBuilder().Append("-NoProfile -File build/msix/Test-PackageActivation.ps1")
        .Append("-PackagePath").AppendQuoted(package).Append("-ExpectedSignerThumbprint").AppendQuoted(signer).Append("-DisposableMachine");
    if (!string.IsNullOrWhiteSpace(baseline)) args.Append("-BaselinePackagePath").AppendQuoted(baseline);
    CheckedProcess("pwsh", new ProcessSettings { Arguments = args });
});
