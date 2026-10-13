Task("Clean-Machine-Harness-Validate").Does(() => PowerShell("build/acceptance/Test-Harness.ps1"));
Task("Clean-Machine-Prepare").IsDependentOn("Clean-Machine-Harness-Validate").Does(() =>
{
    var msi = Argument("msi", "artifacts/installer/output/Computator.NET.msi");
    var inventory = Argument("inventory", "artifacts/installer/output/payload.xml");
    var legacy = Argument("legacy-msi", "");
    var hash = Argument("legacy-sha256", "");
    PowerShell("build/acceptance/Prepare-CleanMachine.ps1", "-MsiPath \"" + msi + "\" -InventoryPath \"" + inventory + "\"" +
        (string.IsNullOrWhiteSpace(legacy) ? "" : " -LegacyMsiPath \"" + legacy + "\" -LegacySha256 \"" + hash + "\""));
});
Task("Clean-Machine-Verify").Does(() =>
{
    var result = Argument("acceptance-result", "");
    var hash = Argument("candidate-sha256", "");
    if (string.IsNullOrWhiteSpace(result) || string.IsNullOrWhiteSpace(hash)) throw new Exception("Provide the guest result and the exact candidate MSI hash.");
    PowerShell("build/acceptance/Assert-CleanMachineResult.ps1", "-ResultPath \"" + result + "\" -CandidateSha256 \"" + hash + "\" -RequireHistoricalUpgrade");
});
