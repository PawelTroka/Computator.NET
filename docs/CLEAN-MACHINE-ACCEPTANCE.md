# Clean Windows and historical-upgrade acceptance

The offline Windows Sandbox harness does **not** represent SDK-equipped CI as a clean machine, automatically enable Windows features, install on the developer host, accept WiX terms or sign a package. Preparation and report verification are Cake tasks on the host; the guest needs only Windows PowerShell 5.1 and Windows' built-in installer facilities, not Cake or a development SDK.

## Preferred historical path: the real official EXE

Obtain the approved unsigned candidate MSI, its matching `payload.xml` and corresponding candidate EXE. The official historical release publishes a Burn EXE, not a separately listed MSI. The pinned acquisition task downloads and verifies the real release asset without executing it:

```powershell
./build.ps1 -Target Installer-Historical-Fetch
./build.ps1 -Target Clean-Machine-Prepare -CakeArguments @(
    '--msi=C:\Candidates\Computator.NET.msi',
    '--inventory=C:\Candidates\payload.xml',
    '--candidate-bundle=C:\Candidates\Computator.NET.Installer.exe',
    '--historical-bundle=artifacts/installer/historical/Computator.NET.Installer.v2.3.2.exe'
)
```

Both EXEs are required together. Preparation verifies the historical EXE's reviewed SHA-256 and size and copies all inputs into a unique run directory. `plan.json` binds the MSI, EXEs, inventory and guest scripts to hashes. See [historical package identities and test scope](HISTORICAL-UPGRADE-ACCEPTANCE.md).

The `.wsb` configuration maps only copied inputs read-only and a newly created empty results directory writable. Networking, clipboard, camera, microphone, printer and vGPU access are disabled. Review the generated paths/configuration, then explicitly open the `.wsb` on a Windows host where Sandbox is already available. Nothing launches automatically from preparation. Do not map a real user's profile or documents into the guest.

## What the guest checks

The schema-2 guest checks common and configured modern runtime/SDK locations, PATH and runtime registration. Finding a modern runtime/SDK is a hard failure. It also refuses an existing Computator.NET installation or profile. The OS-supplied .NET Framework needed by the historical application and Windows PowerShell is not a development SDK.

The shared historical harness installs the original 2.3.2 EXE, creates a profile with the installed historical settings serializer, writes reproducible `.tsl`/`.tslf` files, checks launch and shell integration, and tests intentional MSI-upgrade failure and rollback. It then upgrades using the unmodified candidate EXE, checks the complete candidate payload, semantic settings migration, unchanged original documents/settings, repair and EXE uninstall. MSI and Burn registrations are checked separately. See the linked scope document for the precise assertions.

The negative MSI stays inside the disposable guest and is not a release output. Rollback requires removal after `InstallInitialize` and before `InstallFiles`, with the intentional failure after `InstallFiles`. It verifies recovery after historical removal, **not** failure after every deferred candidate action or Burn-engine failure-chain recovery. Preserve this distinction when writing acceptance reports.

MSI/Burn logs and a structured `acceptance.json` remain in the new writable results directory. Cleanup targets only known test-product identities after the harness's clean-machine preflight. Failure or cleanup failure returns failure; discard the guest after inspecting retained diagnostics. User fixtures stay in the disposable guest rather than being copied into host profile directories.

## Verify the exact candidate

After actual guest execution, compute hashes from the same candidate inputs used to prepare that guest and verify the resulting report:

```powershell
$msiHash = (Get-FileHash C:\Candidates\Computator.NET.msi -Algorithm SHA256).Hash
$exeHash = (Get-FileHash C:\Candidates\Computator.NET.Installer.exe -Algorithm SHA256).Hash
$result = 'C:\path\to\this-run\results\acceptance.json'
./build.ps1 -Target Clean-Machine-Verify -CakeArguments @(
    "--acceptance-result=$result",
    "--candidate-sha256=$msiHash",
    "--candidate-bundle-sha256=$exeHash"
)
```

The release-facing verifier requires the official EXE schema-2 historical flow, all mandatory stages, matching candidate MSI **and** EXE hashes, the pinned historical EXE identity, no modern runtime/SDK and successful cleanup. Fresh-install-only, extracted-MSI-only and SDK-equipped hosted-runner reports do not satisfy it. A report is recorded evidence, not a cryptographic attestation against a malicious operator.

## Retained MSI-only diagnostic mode

The older `--legacy-msi` plus owner-verified `--legacy-sha256` preparation mode is retained for targeted MSI diagnosis, and omitting historical inputs still permits fresh-install testing. An EXE must never be passed as an MSI. These modes produce schema-1 reports and cannot satisfy the official-bundle release gate. Do not combine legacy-MSI arguments with the two-EXE mode.

## CI versus actual machine acceptance

Normal PR CI runs `Clean-Machine-Harness-Validate`, covering plan safety and rejection of wrong-package, incomplete, SDK-equipped or cleanup-failed evidence. On Windows it executes contract tests with PowerShell 7 and the actual Windows PowerShell 5.1 guest engine. A UTF-8 BOM is required for guest scripts with Unicode source text; PS7-only parsing previously missed a real PS5.1 failure. Cake's Windows acceptance launcher also isolates module resolution from inherited PS7 module paths.

**Normal PR CI does not launch Sandbox or install the historical product.** The separately opted-in installer workflow can run the lifecycle on a disposable hosted Windows runner, but that runner has an SDK. Actual offline Sandbox execution, standard-user launch, interactive bootstrapper UX, other supported Windows editions, owner-profile trials and signed-release acceptance still require their own recorded results. None of these tasks authorizes signing, publication or Store submission.
