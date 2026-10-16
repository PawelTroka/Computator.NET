# Official 2.3.2 installer upgrade acceptance

This work extends the existing Cake installer and offline Sandbox harnesses. It does not change product packaging, sign an artifact, accept tool terms, publish a release or certify the unsigned `3.0.0-preview.1` candidate for release.

## Historical input and registration

The [official v2.3.2 release](https://github.com/PawelTroka/Computator.NET/releases/tag/v2.3.2) publishes `Computator.NET.Installer.v2.3.2.exe`, not a standalone MSI. The original EXE is a WiX 3 Burn bootstrapper with two conditional, per-machine MSI payloads: the .NET Framework 4 build for pre-Vista Windows and the .NET Framework 4.6.1 build for newer Windows. Acceptance installs that unmodified EXE; extracted metadata alone is not an upgrade test.

| Identity | Observed historical value |
| --- | --- |
| GitHub release asset | `5646129`; 28,069,455 bytes |
| EXE SHA-256 | `30b107dced1c147b6f3ba9089b97d2131dd43aa195d5a23c3a95d9b150149d4a` |
| UpgradeCode | `{86D89770-B31B-4467-BC5E-B54B0FF263A1}` |
| MSI ProductCode | `{BDA7D43C-B1D1-4844-AC3F-198D33E3F237}` |
| MSI ProductVersion | `2.3.2.0` |
| MSI template | `Intel;1033` (32-bit) |
| Burn bundle registration | `{252E03F2-85DB-4A38-A29C-14E31AF5AAE1}` |

These values were inspected from the actual downloaded package in [Windows run 38886691140](https://github.com/PawelTroka/Computator.NET/actions/runs/38886691140). The SHA-256 is an independently observed pin, **not** a publisher-signed checksum. The fixture in `build/acceptance/historical-2.3.2.json` records provenance. Acquisition fails rather than silently refreshing a changed pin.

The current MSI is x64, retains the historical UpgradeCode and per-machine scope, and uses a different version/architecture-specific ProductCode. Burn has a separate registration from its MSI. Therefore successful MSI replacement is not sufficient: the normal EXE-to-EXE test also requires the historical bundle registration to disappear. No test manually deletes uninstall registry entries to manufacture success. The default x86-to-x64 install-directory transition is checked rather than forcing both products into a synthetic `INSTALLDIR`.

## Reproducible fixture and assertions

`Installer-Historical-Validate` consumes an already built candidate MSI, its matching `payload.xml`, the corresponding candidate EXE, and the hash-pinned historical EXE. It does not depend on `Build-Installer` and therefore does not implicitly rebuild or accept WiX terms.

The lifecycle test requires an elevated, 64-bit, explicitly opted-in disposable GitHub-hosted Windows runner or Windows Sandbox guest. It refuses an existing Computator.NET installation or profile. It uses Windows PowerShell 5.1 and Windows Installer COM, without invoking a development SDK in the guest.

The test installs the historical EXE and checks both MSI and Burn registrations. A separate 32-bit Framework process loads the **installed 2.3.2 settings assembly** and writes non-default language, editor flags, fonts and directories using its original serializer. This produces a real `settings.dat`, not a hand-built approximation. Four guest-only `.tsl`/`.tslf` files cover profile and external locations, spaces and Unicode names.

After historical launch, it captures settings/document and installed-payload hashes, shortcut targets and both file-association commands. It then uses the existing `New-FailingUpgrade.ps1` to make a private negative-test copy of the candidate MSI. The original candidate remains unchanged. A type-19 failure after `InstallFiles` must return 1603, with logs establishing actual historical removal and the intentional failure. Rollback must restore the historical MSI registration, payload, shell integration and usable historical settings; the old application must launch again.

The unmodified candidate EXE then upgrades the product. The test requires removal of the old MSI and Burn registrations, exact candidate payload hashes, current shortcut/association targets, successful startup and import of the historical non-default settings into `settings.json`. All original documents and `settings.dat` must remain intact. A deleted `Static/functions.xml` must be restored exactly by repair. EXE uninstall must remove installed registration/shell integration while preserving user data, including the new JSON preferences.

**Rollback scope:** this exercises transactional recovery of a real historical installation after its removal. The failure is an MSI failure inside the transaction, not a modified Burn failure-chain test. It does not prove recovery after every deferred candidate action, cancellation, power loss or an OS reboot. Earlier removal outside the transaction or removal after the failure cannot satisfy the sequence contract.

## Cake commands

On the build/review host, acquire and verify the historical package without installing it:

```powershell
./build.ps1 -Target Installer-Historical-Fetch
```

On an explicitly disposable, elevated supported runner, with the existing unsigned candidate in `artifacts/installer/output`:

```powershell
$env:COMPUTATOR_DISPOSABLE_INSTALL_TEST = '1'
./build.ps1 -Target Installer-Historical-Validate
```

Overrides are `--historical-candidate-directory`, `--historical-bundle` and `--historical-results`, passed through `-CakeArguments`. A reused result directory containing `acceptance.json` is rejected. The existing installer workflow also has an off-by-default `validate_historical_upgrade` input for a separately approved build; that path still requires the owner's explicit WiX terms acceptance. A hosted runner is SDK-equipped and cannot certify clean no-SDK acceptance.

Use [Clean-machine acceptance](CLEAN-MACHINE-ACCEPTANCE.md) to prepare the offline Sandbox inputs and verify a no-SDK report against **both** candidate package hashes.

## Evidence interpretation

Package inspection, contract tests and actual lifecycle execution are separate results. Each actual run must retain candidate MSI/EXE hashes, historical EXE hash, source/artifact provenance, host profile, completed stages, MSI/Burn logs, preservation hashes and cleanup outcome. A failed stage or failed cleanup is not acceptance success. Synthetic report fixtures in contract tests are never presented as machine acceptance.

The retained review candidate is artifact `12014458227` from [run 38825547427](https://github.com/PawelTroka/Computator.NET/actions/runs/38825547427), built at `d2062a33fd7af5faf3593846a589b7b5619cbf3b` before the #173 merge. Archive SHA-256: `6a9aa40b9dc34c14e1cd9c4bbeaf1d0570a6aa38cc89b9cd6d3f33bb40908e63`. Later acceptance-harness commits do not imply those candidate bytes were rebuilt. GitHub artifacts expire; retain approved input bytes and hashes privately for repeat runs rather than substituting another candidate under the old evidence.

Actual offline Windows Sandbox/no-SDK execution remains required unless a matching successful schema-2 report has been recorded. Standard-user launch, interactive bootstrapper UX, other supported Windows editions, prior-candidate upgrades, signed-package validation and owner-profile trials are separate acceptance work. Do not mark the release checklist complete based solely on a green contract or hosted-runner job.
