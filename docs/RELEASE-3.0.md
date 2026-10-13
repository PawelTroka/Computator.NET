# Computator.NET 3.0 release preparation

**Review snapshot: 2026-10-13. No 3.0 release, public deployment or Store submission has been published.** The candidate retains the Windows Forms/WPF product on .NET 10 and integrates the reviewed modernization follow-ups. Release acceptance remains tracked below; passing CI certifies only the tested commit and checks.

## Evidence already reviewed

| Area | Evidence and limit |
| --- | --- |
| Combined migration | Integration commit `bebc44b709f23ffb4c91a4dcc4a36aa543c7916e` passed all 11 GitHub workflows and local Cake `Publish`: 252 core, 947 numerical, 20 desktop, 81 API tests, one allowlisted server-side 3D skip, 16 focused browser checks, published desktop/API process checks and three ZIP payload checks. Real browser, portable, native, installer-source, MSIX and provider-mirror workflows also passed. [Integration PR and current checks](https://github.com/PawelTroka/Computator.NET/pull/149). Recheck later release commits. |
| Framework and .NET 10 checkpoints | #139 (`2b1d16f`) and #140 (`6944238`) passed local Cake Windows validation at their reviewed heads, including numerical, compiler, API and desktop workflow tests. Both checkpoints are merged. Later integrated changes require their own complete run. |
| Current editor/test packages | Audited build and 252 core / 20 desktop tests passed with the newer Scintilla and Test SDK pins, including published desktop startup and normal shutdown. Exact versions remain in the manifests. |
| Web workflows | 38 browser checks passed during component review: 16 focused checks with a mocked API, plus 22 checks against the actual protected local API (10 general workflows and 12 custom-function workflows), across Chromium and Firefox. This is bounded workflow coverage, not every numerical method or a public hosting certification. |
| MSI/Burn | Self-contained payload staging and installer compilation succeeded after explicit owner acceptance of the WiX 7 terms. Installation, historical upgrades and signing have not been certified. |
| MSIX | Windows SDK package validation and unpacked payload hash checks succeeded for an unsigned candidate retaining the existing Store identity. Installation/activation and Store acceptance remain. |
| Native/portable work | A pinned GSL 2.8 candidate and a partial portable engine/CLI have dedicated validation targets. The embedded GSL default and Windows desktop remain unchanged by those capability claims. Check each OS/RID's actual CI before claiming support. |

Review reports must identify the tested commit and retain TRX/browser reports, coverage, package inventories/hashes and native provenance as applicable. GitHub base and follow-up workflows now run for every PR and push to `master`. A green mirror job is not an actual AppVeyor or Travis run.

## Follow-up tracking

| PR | Scope | Cake validation / detail |
| --- | --- | --- |
| [#141](https://github.com/PawelTroka/Computator.NET/pull/141) | Real browser-to-API execution and protected loopback proxy | `Web-E2E`; [deployment contract](followups/WEB-E2E.md) |
| [#142](https://github.com/PawelTroka/Computator.NET/pull/142) | Web custom functions and request-specific JSON execution | `Web-CustomFunctions`; [workflows](followups/WEB-CUSTOM-FUNCTIONS.md) |
| [#143](https://github.com/PawelTroka/Computator.NET/pull/143) | Opt-in native GSL build, provenance and ABI checks | `Native-GSL-Validate`; [acceptance](followups/GSL.md) |
| [#144](https://github.com/PawelTroka/Computator.NET/pull/144) | WixSharp/WiX MSI and Burn migration | `Installer-Source`, approved `Build-Installer`; [lifecycle gates](followups/INSTALLER.md) |
| [#145](https://github.com/PawelTroka/Computator.NET/pull/145) | Full-trust desktop MSIX with preserved Store identity | `MSIX-Validate`; [signing/Store gates](followups/MSIX.md) |
| [#146](https://github.com/PawelTroka/Computator.NET/pull/146) | Shared TSL engine and portable CLI foundation | `Portable-Validate`; [supported subset](followups/PORTABLE-ENGINE.md) |
| [#147](https://github.com/PawelTroka/Computator.NET/pull/147) | AppVeyor Windows build and provider evidence | `AppVeyor-Mirror`, native `AppVeyor-Validate`; [provider acceptance](followups/APPVEYOR.md) |
| [#148](https://github.com/PawelTroka/Computator.NET/pull/148) | Travis Linux build and provider evidence | `Travis-Mirror`, native `Travis-Validate`; [provider acceptance](followups/TRAVIS.md) |

## Release gates

- [ ] Record passing final-commit GitHub results for Windows suites, coverage, portable ZIPs, focused and real browser suites, portable OS jobs, native candidate jobs, installer source, MSIX and provider mirrors. Remaining skipped tests must match the exact result-guard allowlist; do not weaken numerical expectations to obtain a pass.
- [ ] Test backed-up real settings and `.tsl`/`.tslf` documents, JSON import/backup/rollback, all editors, clipboard/IME input, custom functions and chart appearance/zoom/export. Complete high-DPI and multi-monitor acceptance.
- [ ] Choose the 3.0 release version consistently across assembly metadata, MSI, MSIX and release notes. Current 2.x values are historical or experimental candidate values; the checklist title does not change them. Confirm the MSIX version advances the actual Store package.
- [ ] On clean disposable Windows machines without a development SDK, test MSI installation, launch, repair, uninstall, failed-upgrade rollback and upgrade from the real 2.3.2 MSI. Verify shortcuts, file associations and preservation of user data.
- [ ] Sign with owner-controlled release credentials; validate MSIX installation and Start-menu/file activation, upgrade from the actual previous Store package, settings/native-cache writes, offline use and uninstall policy. Complete Store certification before submission/release claims.
- [ ] Decide whether to adopt GSL 2.8 as the packaged default after all intended native/managed ABI and numerical checks, difficult-input review and distribution-source/license obligations are satisfied. Keep the existing default until that decision.
- [ ] Verify genuine AppVeyor and Travis final-commit runs with native provider URLs/build IDs and completion evidence if those providers are advertised as active gates.
- [ ] Approve release notes that state the remaining scope: symbolic calculation and server-side 3D rendering are unfinished; the portable CLI is a subset, and the full Windows numerical/graphics engine is not yet a Mac/Linux desktop application.

The supported web deployment is a trusted local service: both hosts bind to loopback, the frontend uses the protected proxy, and the shared token stays server-side. Fresh evaluators isolate request state, not operating-system permissions. Public multi-user execution needs separate authorization and process/resource isolation work; request timeouts do not stop arbitrary scripts.

No target in this checklist implicitly signs an artifact, publishes a GitHub release or submits an application to the Store. Keep the existing releases and backed-up user data available for rollback.
