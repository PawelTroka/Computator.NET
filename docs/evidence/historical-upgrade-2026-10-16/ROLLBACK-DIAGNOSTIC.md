# Historical upgrade rollback diagnostic

## Finding

**The retained run demonstrates recovery of the historical installation despite
the message `Error in rollback skipped. Return: 5`. This occurrence is classified
as a nonfatal diagnostic on the deliberately induced rollback path, with no
observed recovery defect warranting an installer change.**

This is an evidence-based assessment of the tested recovery contract. The exact
internal meaning of `Return: 5` in this free-text Windows Installer message
remains unproven. The number alone establishes neither an access-denied failure
nor a documented guarantee that the message is harmless. Preserve the diagnostic
and investigate it again if a future run shows a failed recovery assertion.

This review inspects the existing [October 16 acceptance run](https://github.com/PawelTroka/Computator.NET/actions/runs/38906708104)
and the harness at commit
[`d852eaa7a683a4bf084b75af281ff9aad762f09c`](https://github.com/PawelTroka/Computator.NET/commit/d852eaa7a683a4bf084b75af281ff9aad762f09c),
which underlies [PR #174](https://github.com/PawelTroka/Computator.NET/pull/174).
It does not represent a new installer or acceptance run. No validated installer
package was modified for this analysis.

## Retained sources

The [evidence index](README.md) explains preservation and sanitization.
[`manifest.json`](manifest.json) records the original and sanitized file hashes.
[`installer-logs.zip`](installer-logs.zip) contains the full sanitized logs;
line order and line counts are preserved. All log line references below refer
to `failed-upgrade.log` inside that archive. The existing
[acceptance report](../historical-upgrade-2026-10-16.json) records the run's
results and the original diagnostic.

## What the log actually shows

| Event | Log lines | Evidence and significance |
| --- | --- | --- |
| Historical product detected | 122–123 | `WIX_UPGRADE_DETECTED` and `MIGRATE` identify the historical MSI. |
| Transaction begins before removal | 836–848 | Parent `InstallInitialize` succeeds, followed by `RemoveExistingProducts`. |
| Historical files removed | 1587–1789 | The nested historical uninstall executes 49 `FileRemove` operations, including `Computator.NET.exe` at 1789. |
| Historical uninstall succeeds | 1814–1815 | Nested `InstallFinalize` and `INSTALL` both return 1. |
| Parent removal succeeds | 1937–1938 | Parent `RemoveExistingProducts` returns 1 before the intentional failure. |
| Intended failure triggers | 2020–2029 | `ComputatorAcceptanceFailure` emits the injected message and returns 3. |
| Rollback executes | 2031–2034 | Parent execution starts a rollback script and emits `RollbackInfo`. |
| Historical files restored | 2120–2320 | 49 `FileCopy` operations target the historical application tree. The executable is restored at 2120. |
| Shortcuts restored | 2327, 2335 | The Start Menu and public Desktop shortcut backups are copied back. |
| File associations restored | 2354–2355, 2365–2366 | `.tslf` and `.tsl` open commands again point to the historical executable. |
| Historical registration restored | 2375–2478, 2564, 2599 | Component registration uses the historical product code, and installed-version entries return to `2.3.2.0`. |
| Rollback reaches its end | 2610–2611 | `Executing op: End(...)` immediately precedes the investigated diagnostic at the same timestamp. |
| Intended installation fails | 2615, 2755, 2758, 2770 | Parent `INSTALL` returns 3; the product result and engine exit are 1603. |

Selected message text follows. Logging prefixes are omitted and whitespace is
normalized for readability; the full retained log is authoritative.

```text
2023: Product: Computator.NET -- Intentional disposable-machine rollback test
2029: Action ended 13:19:49: ComputatorAcceptanceFailure. Return value 3.
2610: Executing op: End(Checksum=0,ProgressTotalHDWord=0,ProgressTotalLDWord=0)
2611: Error in rollback skipped. Return: 5
2615: Action ended 13:19:49: INSTALL. Return value 3.
```

The trigger is established before the rollback diagnostic appears. The
[negative-fixture generator](https://github.com/PawelTroka/Computator.NET/blob/d852eaa7a683a4bf084b75af281ff9aad762f09c/build/acceptance/New-FailingUpgrade.ps1#L39-L60)
inserts Type 19 `ComputatorAcceptanceFailure` with condition `1` and the exact
message shown above. It operates on a separate
[copy of the candidate MSI](https://github.com/PawelTroka/Computator.NET/blob/d852eaa7a683a4bf084b75af281ff9aad762f09c/build/acceptance/New-FailingUpgrade.ps1#L6-L8).
Microsoft documents [Custom Action Type 19](https://learn.microsoft.com/en-us/windows/win32/msi/custom-action-type-19)
as displaying the supplied message and terminating installation with failure.
The harness [explicitly expects exit code 1603](https://github.com/PawelTroka/Computator.NET/blob/d852eaa7a683a4bf084b75af281ff9aad762f09c/build/acceptance/Test-HistoricalUpgrade.ps1#L235-L240)
from this negative fixture.

## Recovery is checked independently of the diagnostic

The harness does not infer recovery from a rollback marker or suppress a failed
assertion because failure was intended. Its
[post-failure checks](https://github.com/PawelTroka/Computator.NET/blob/d852eaa7a683a4bf084b75af281ff9aad762f09c/build/acceptance/Test-HistoricalUpgrade.ps1#L241-L249)
run before the candidate bundle is installed and require:

- The historical MSI to be installed, the candidate MSI to be absent, and the
  historical bundle registration to remain present.
- All historical application files and pre-upgrade user-data files to retain
  their baseline hashes.
- Historical shortcut targets and file-association commands to match the
  baseline snapshot.
- The historical application to open a responsive main window and close
  normally, using the
  [launch assertions](https://github.com/PawelTroka/Computator.NET/blob/d852eaa7a683a4bf084b75af281ff9aad762f09c/build/acceptance/Test-HistoricalUpgrade.ps1#L137-L157).
- The historical settings fixture to remain readable with its expected values,
  followed by another user-data hash comparison.

The acceptance report records these stages as passed, counts 49 historical
application files, and reports no cleanup errors. The log's 49 restoration
copies independently agree with that payload count. Subsequent successful
installation, launch, migration, repair and uninstall provide additional
evidence that the machine remained usable for the tested application lifecycle.

The inspected committed harness also matches the report's
`harnessSha256` when encoded with the Windows checkout's CRLF line endings:

```text
7f42cb8051a932543614a0c1bf649e1adff3d690b407241855811ed72720a444
```

The [log contract](https://github.com/PawelTroka/Computator.NET/blob/d852eaa7a683a4bf084b75af281ff9aad762f09c/build/acceptance/AcceptanceContracts.psm1#L11-L42)
requires actual old-product removal before the deliberate failure, preserves
engine diagnostics, and leaves recovery to the independent assertions above.

## Return codes and neighboring notes

Microsoft's [action-log return-value table](https://learn.microsoft.com/en-us/windows/win32/msi/logging-of-action-return-values)
maps logged action value 3 to installation failure and logged action value 5 to
successful completion. Separately, the
[Win32 system-error table](https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-)
maps `GetLastError` value 5 to access denied. These are different namespaces.
The investigated free-text line is not an `Action ended ... Return value ...`
record, and the available documentation does not establish its internal
namespace. Neither table alone conclusively decodes this exact message.

Immediately preceding it, lines 2606 and 2609 contain `1402` registry-open notes
with system error **2**, after registry cleanup operations. Microsoft defines
[MSI message 1402](https://learn.microsoft.com/en-us/windows/win32/msi/windows-installer-error-messages)
as an unsuccessful registry-key open and system error 2 as a missing object.
Those entries do not report system error 5. Rollback continues to `End`.

The separate `2265 ... -2147287035` notes are present in successful phases too:

| Retained log | Example note | Successful engine result |
| --- | --- | --- |
| `historical-bundle-install_000_Computator.NET.msi.log` | 554 | 1104, return 0 |
| `candidate-bundle-upgrade_000_Computator.NET.msi.log` | 855 | 4626, return 0 |
| `candidate-repair.log` | 2008 | 7332, return 0 |
| `candidate-bundle-uninstall_000_Computator.NET.msi.log` | 931 | 4624, return 0 |

This comparison does not prove that every such note is benign. It shows that
these notes are not distinctive evidence of a recovery defect in the failed
upgrade. The preserved evidence supports the observed recovery result while
retaining the unexplained internal detail.

## Scope of the accepted recovery proof

**The negative test fails after historical removal, before execution of the
parent candidate payload script.** Its Type 19 action is immediate and is
sequenced after the `InstallFiles` entry but before parent `InstallFinalize`.
Before the intentional failure, the log's `Executing op:` entries belong to
the nested historical uninstall. The parent first emits execution operations
when its rollback begins.

Microsoft documents [InstallFinalize](https://learn.microsoft.com/en-us/windows/win32/msi/installfinalize-action)
as executing accumulated script operations, and
[RemoveExistingProducts](https://learn.microsoft.com/en-us/windows/win32/msi/removeexistingproducts-action)
as supporting the observed placement after `InstallInitialize`, before actions
that generate the parent's execution script. Thus this run proves recovery
after real removal of the historical installation. It does not establish
recovery from a failure after partially writing the candidate's 569 files.
The subsequent unmodified candidate installation independently checks those
[569 payload hashes](https://github.com/PawelTroka/Computator.NET/blob/d852eaa7a683a4bf084b75af281ff9aad762f09c/build/acceptance/Test-HistoricalUpgrade.ps1#L249-L264).

**Five user-data files are checked at rollback; six are preserved by the end of
the complete acceptance sequence.** The
[pre-upgrade snapshot](https://github.com/PawelTroka/Computator.NET/blob/d852eaa7a683a4bf084b75af281ff9aad762f09c/build/acceptance/Test-HistoricalUpgrade.ps1#L217-L233)
contains four document fixtures and `settings.dat`. The candidate later creates
`settings.json`, which the harness
[adds to the preservation set](https://github.com/PawelTroka/Computator.NET/blob/d852eaa7a683a4bf084b75af281ff9aad762f09c/build/acceptance/Test-HistoricalUpgrade.ps1#L265-L274)
before repair and uninstall. The report's six-file total is correct for the
complete sequence; it is not a count of files restored during rollback.

This SDK-equipped GitHub-hosted Windows run does not certify no-SDK installation,
arbitrary failure points, power-loss recovery, or complete restoration of all
machine state. The preserved logs and original passing assertions justify the
bounded finding above without another acceptance run or changes to the validated
installer packages.
