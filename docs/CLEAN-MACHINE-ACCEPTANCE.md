# Clean Windows and historical-upgrade acceptance — roadmap 5

This adds a real offline Windows Sandbox acceptance harness. It does **not** represent SDK-equipped CI as a clean machine, automatically enable Windows features, install on the developer host, accept WiX terms or sign a package.

First obtain the compiled candidate MSI and its matching payload inventory through the approved installer target. To test an actual historical upgrade, separately obtain the authentic historical **MSI** and independently verify its SHA-256. The public 2.3.2 release offers a bootstrapper; do not pass that EXE as if it were an MSI or silently substitute a newly generated fixture. Extract/obtain its actual embedded MSI through an owner-reviewed process. Its UpgradeCode and ProductVersion must match the known 2.3.2 product.

```powershell
./build.ps1 -Target Clean-Machine-Prepare --msi=C:\Candidates\Computator.NET.msi --inventory=C:\Candidates\payload.xml --legacy-msi=C:\Historical\Computator.NET-2.3.2.msi --legacy-sha256=<verified-sha256>
```

The target creates a unique acceptance directory and a `.wsb` configuration. It maps only copied inputs read-only and a newly created empty results directory writable. Networking, clipboard, camera, microphone, printer and vGPU access are disabled. Review the generated paths/configuration, then explicitly open it on a Windows host where Sandbox is already available. Nothing launches automatically from preparation.

The PowerShell 5.1 guest checks that no modern shared runtime/SDK or Computator.NET installation exists. It verifies input hashes and identities, installs the historical product when supplied, and creates a guest-only deliberately failing candidate MSI by inserting a type-19 error after InstallFiles. It requires rollback to restore the old registration/executable and verifies that old application still launches. The actual unmodified candidate then upgrades/installs, all installed payload hashes are checked, the self-contained product launches and shuts down, a removed data file is repaired, and uninstall must retain an external Unicode-named user document.

The negative MSI is not a distribution artifact and is never copied back as a release output. Installer logs and a structured acceptance result remain in the newly mapped results directory. Cleanup only targets product codes established from the checked input packages in this disposable guest. Failure/cleanup failure is not reported as acceptance success.

The authentic v2.3.2 bundle's MSI ProductVersion is `2.3.2.0`. The rollback probe requires removal after InstallInitialize and before InstallFiles; removal before the transaction or after the deliberate failure cannot establish this rollback claim and is rejected. This tests rollback of the historical removal, not failure after every deferred candidate action has executed.

Without a historical MSI the harness can test fresh installation, but explicitly reports historical acceptance as not requested. `Clean-Machine-Verify` requires the real historical rollback/upgrade stages and the exact candidate hash. A successful fresh-install-only report cannot satisfy it.

Normal PR CI tests PowerShell syntax, read-only/writable mapping boundaries, legacy-hash requirements and rejection of wrong-hash, failed or incomplete evidence. **It does not launch Sandbox or execute the MSI lifecycle.** Actual guest execution, bootstrapper UX, standard-user launch, broader supported Windows editions, visual/profile acceptance and signed release review still require recorded runs. The report is evidence, not a cryptographic attestation against a malicious operator.

Windows Sandbox configuration and Windows Installer transaction behavior are documented by Microsoft. The harness intentionally fails if a package's removal schedule does not roll back safely; do not weaken that check merely to obtain a pass. Keep the original candidate and historical artifacts untouched for review.
