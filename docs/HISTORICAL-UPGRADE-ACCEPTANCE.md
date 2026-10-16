# Official 2.3.2 installer upgrade acceptance

This work extends the existing Cake installer and offline Sandbox harnesses. It does not change product packaging, sign an artifact, accept tool terms, publish a release or certify the unsigned `3.0.0-preview.1` candidate for release.

## Executed result: 2026-10-16

[Windows run 38906708104](https://github.com/PawelTroka/Computator.NET/actions/runs/38906708104) **passed** on a disposable, elevated GitHub-hosted `windows-2025` runner. It executed the authentic historical EXE and the retained candidate bytes described below, using acceptance code at `d852eaa7a683a4bf084b75af281ff9aad762f09c`. The lifecycle ran from 13:19:22 to 13:20:38 UTC. [The actual acceptance report](evidence/historical-upgrade-2026-10-16.json) is retained in the repository as a structured copy of the artifact report, not a synthetic contract fixture.

| Executed check | Observed result |
| --- | --- |
| Original official 2.3.2 EXE | Installed its x86 MSI and separate Burn registration, wrote real non-default settings and launched/shut down normally. |
| Deliberately failed upgrade | The private negative MSI returned 1603 after successful nested removal of the historical product. Actual removal, injected failure and rollback replay were established in order from its MSI log. |
| Functional rollback recovery | Historical MSI/Burn registrations, all 49 historical payload-file hashes, settings/documents and shell integration were restored; the old application launched and verified its real preferences again. |
| Unmodified candidate EXE upgrade | Removed the old MSI and Burn registrations, moved the default installation from `C:\Program Files (x86)\Computator.NET` to `C:\Program Files\Computator.NET`, and matched all 569 candidate payload hashes. |
| Settings and documents | Imported real non-default language, flags, fonts and directories into `settings.json`; preserved the original `settings.dat` and four `.tsl`/`.tslf` documents, including Unicode and external paths. |
| Shell integration, repair and uninstall | Checked shortcut and association targets; restored the exact deleted file by MSI repair; EXE uninstall removed registration/shell integration and preserved all six user-data files. Cleanup reported no errors. |
| Harness regressions | Cake contract tests, actual Windows Installer COM reader tests and negative-MSI generation/exclusive-access checks passed under PowerShell 7 and Windows PowerShell 5.1. |
| Offline no-SDK acceptance | **Not executed.** Preparation of the real offline Sandbox plan passed. The hosted machine contained SDK/shared-runtime locations and its report cannot satisfy the clean-machine verifier. |

The retained evidence artifact is `12048572484` (`historical-upgrade-evidence`), ZIP SHA-256 `1b198a1714a06c942c5a6e40fa49da7c235127857ab31ebde3859688441aa302`. It contains MSI/Burn logs, the report, candidate provenance, the prepared plan and the tested harness source. Its recorded expiry is 2026-10-30; preserve the full logs separately before expiry. The archived main harness hash matches the report: `7f42cb8051a932543614a0c1bf649e1adff3d690b407241855811ed72720a444`.

**Retained diagnostic:** the failed-upgrade MSI log contains `Error in rollback skipped. Return: 5`. The report preserves this line rather than suppressing it or assigning it an unverified meaning. Every listed recovery postcondition subsequently passed, including all historical payload hashes and the historical application's launch/settings checks. This is bounded functional recovery evidence, not a claim that every rollback-engine diagnostic has been explained or every possible failure mode has been tested.

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

These values were inspected from the actual downloaded package in [Windows run 38886691140](https://github.com/PawelTroka/Computator.NET/actions/runs/38886691140) and the selected payload was subsequently installed in the successful lifecycle. The SHA-256 is an independently observed pin, **not** a publisher-signed checksum. The fixture in `build/acceptance/historical-2.3.2.json` records provenance. Acquisition fails rather than silently refreshing a changed pin.

The current MSI is x64, retains the historical UpgradeCode and per-machine scope, and uses ProductCode `{5EE11F5A-4F1E-FC4E-A296-BC713710E32E}`, ProductVersion `3.0.0`. Its release label remains `3.0.0-preview.1`. Burn has a separate registration from its MSI. Therefore successful MSI replacement is not sufficient: the normal EXE-to-EXE test also requires the historical bundle registration to disappear. No test manually deletes uninstall registry entries to manufacture success. The default x86-to-x64 install-directory transition is checked rather than forcing both products into a synthetic `INSTALLDIR`.

## Reproducible fixture and assertions

`Installer-Historical-Validate` consumes an already built candidate MSI, its matching `payload.xml`, the corresponding candidate EXE, and the hash-pinned historical EXE. It does not depend on `Build-Installer` and therefore does not implicitly rebuild or accept WiX terms.

The lifecycle test requires an elevated, 64-bit, explicitly opted-in disposable GitHub-hosted Windows runner or Windows Sandbox guest. It refuses an existing Computator.NET installation or profile. It uses Windows PowerShell 5.1 and Windows Installer COM, without invoking a development SDK in the guest.

The test installs the historical EXE and checks both MSI and Burn registrations. A separate 32-bit Framework process loads the **installed 2.3.2 settings assembly** and writes non-default language, editor flags, fonts and directories using its original serializer. This produces a real `settings.dat`, not a hand-built approximation. Four guest-only `.tsl`/`.tslf` files cover profile and external locations, spaces and Unicode names.

After historical launch, it captures settings/document and installed-payload hashes, shortcut targets and both file-association commands. It then uses the existing `New-FailingUpgrade.ps1` to make a private negative-test copy of the candidate MSI. The original candidate remains unchanged. A type-19 failure after `InstallFiles` must return 1603. Ordered log checks require successful nested removal of the pinned historical product and executable, the injected error and rollback execution. Rollback must independently restore the historical MSI registration, payload, shell integration and usable historical settings; the old application must launch again.

The unmodified candidate EXE then upgrades the product. The test requires removal of the old MSI and Burn registrations, exact candidate payload hashes, current shortcut/association targets, successful startup and import of the historical non-default settings into `settings.json`. All original documents and `settings.dat` must remain intact. A deleted `Static/functions.xml` must be restored exactly by repair. EXE uninstall must remove installed registration/shell integration while preserving user data, including the new JSON preferences.

**Rollback scope:** this exercises transactional recovery of a real historical installation after its removal. The failure is an MSI failure inside the transaction, not a modified Burn failure-chain test. It does not prove recovery after every deferred candidate action, cancellation, power loss or an OS reboot. Earlier removal outside the transaction or removal after the failure cannot satisfy the sequence contract. Document preservation is a hash check, not full editor/file-activation acceptance.

## Verified harness defects fixed during execution

Windows PowerShell 5.1 required the UTF-8 BOM around the existing Unicode fixture text. Actual Windows Installer COM calls also exposed two interop defects: unconsumed void returns polluted scalar property/query results with null entries, and closing views without releasing their COM records/views kept the negative MSI locked. Both the historical and existing offline property readers now suppress side-effect outputs and release their resources. Regression tests use real COM databases in both engines, verify single-value reads/missing-property rejection, and require a generated negative copy to be immediately hashable and exclusively openable without changing its source.

[Run 38904017617](https://github.com/PawelTroka/Computator.NET/actions/runs/38904017617) established actual historical installation and launch before encountering the now-fixed file lock. [Run 38904951253](https://github.com/PawelTroka/Computator.NET/actions/runs/38904951253) passed the interop regressions and executed the intentional failure, but its log assertion incorrectly required a top-level `Removal completed successfully` summary that the successful nested uninstall did not emit. The replacement checks actual nested identities, removal actions and ordering. Mutation tests reject wrong identities, incomplete removal, unrelated errors, missing rollback and an error before removal; they do not weaken external-state recovery assertions.

These were acceptance-harness defects, not evidence of a wrong product UpgradeCode. No application or installer-authoring change was required to obtain the bounded successful lifecycle result.

## Cake commands

On the build/review host, acquire and verify the historical package without installing it:

```powershell
./build.ps1 -Target Installer-Historical-Fetch
./build.ps1 -Target Clean-Machine-Harness-Validate
```

On an explicitly disposable, elevated supported runner, with the existing unsigned candidate in `artifacts/installer/output`:

```powershell
$env:COMPUTATOR_DISPOSABLE_INSTALL_TEST = '1'
./build.ps1 -Target Installer-Historical-Validate
```

Overrides are `--historical-candidate-directory`, `--historical-bundle` and `--historical-results`, passed through `-CakeArguments`. A reused result directory containing `acceptance.json` is rejected. The existing installer workflow also has an off-by-default `validate_historical_upgrade` input for a separately approved build; that path still requires the owner's explicit WiX terms acceptance. A hosted runner is SDK-equipped and cannot certify clean no-SDK acceptance.

Use [Clean-machine acceptance](CLEAN-MACHINE-ACCEPTANCE.md) to prepare the offline Sandbox inputs and verify a no-SDK report against **both** candidate package hashes.

## Candidate provenance and evidence interpretation

The retained review candidate is artifact `12014458227` from [run 38825547427](https://github.com/PawelTroka/Computator.NET/actions/runs/38825547427), built at `d2062a33fd7af5faf3593846a589b7b5619cbf3b` before the #173 merge. Its Git tree `98a1027bad4f62c94c82f623eab6632cacabb354` is identical to master merge `575315952a219e4355b7f575ce0b165614ef6797`. Later acceptance-harness commits do not imply those candidate bytes were rebuilt.

| Input | SHA-256 |
| --- | --- |
| Candidate artifact archive | `6a9aa40b9dc34c14e1cd9c4bbeaf1d0570a6aa38cc89b9cd6d3f33bb40908e63` |
| Candidate MSI | `7bd90de7a16e0f1efed1cb825f4f740bfcb3163700c8993fa37d9b63cc0f51bd` |
| Candidate EXE | `2334a232d8a04959d04a3d34408ba3f73a88289f4d056dee8da644aaaab35851` |
| Payload inventory | `8cfa670a0d4e96d31582646084f1489ea7151b4661fe59fca8f9eb7b9b3bac4f` |

Package inspection, contract tests and actual lifecycle execution are separate results. Each actual run must retain package hashes, source/artifact provenance, host profile, completed stages, MSI/Burn logs, preservation hashes and cleanup outcome. A failed stage or failed cleanup is not acceptance success. Synthetic report fixtures in contract tests are never presented as machine acceptance. GitHub artifacts expire; retain approved input bytes and full logs privately for repeat runs rather than substituting another candidate under the old evidence.

Actual offline Windows Sandbox/no-SDK execution remains required: the successful report above intentionally fails that release-facing requirement. Standard-user launch, interactive bootstrapper UX, other supported Windows editions, prior-candidate upgrades, signed-package validation and owner-profile trials are separate acceptance work. Do not mark the release checklist complete based solely on a green contract or hosted-runner job.
