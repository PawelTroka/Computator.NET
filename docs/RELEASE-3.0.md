# Computator.NET 3.0 release preparation

**Review snapshot: 2026-10-16. No 3.0 release, public deployment or Store submission has been published.** The candidate retains the Windows Forms/WPF product on .NET 10 and integrates the reviewed modernization follow-ups. Release acceptance remains tracked below; passing CI certifies only the tested commit and checks.

The second review integrates #151–#164: one complete SLNX, source/IDE frontend preparation, the restrained dark interface, a self-contained protected local web package, shared execution policy, collectible generated programs, canonical candidate identity/evidence, clean-machine and signed-package acceptance harnesses, copied-profile trials, GSL error boundaries, portable scientific services and cancellable worker execution. SDK 10.0.402 and .NET/ASP.NET/JSON 10.0.13 align with October security servicing. The reviewed candidate identity is **3.0.0-preview.1** in `Version.props`.

Integration review additionally fixed concurrent script-output limits, stale assets after changing frontend profiles, surviving worker/descendant processes, genuine historical MSI version handling, rollback ordering, mixed-case profile preservation, predeployment MSIX identity checks and release-report preservation/provenance. GSL error/recovery tests exercise both host and generated imports. See the corresponding workflow results and `artifacts/release-evidence/manifest.json` for the exact candidate; the first-wave evidence below is historical.

Generated scalar programs and exceptions are reclaimable, but shared dynamic-binder caches can retain dynamic programs until process exit. Worker-backed web requests have a process cleanup boundary; desktop dynamic lifetime still needs follow-up. See [program lifetime](PROGRAM-LIFETIME.md). The acceptance harnesses are executable tooling; successful contract tests do not substitute for actual clean-machine, signed-package or private-profile trials.

The third review integrates #166–#172 through [#173](https://github.com/PawelTroka/Computator.NET/pull/173): script documents and drafts, portable numerical CLI commands, shutdown diagnostics, cancellable charts, import-race protection, legacy script workers and all 1,220 built-in reference entries. Review fixes preserve literal strings during TSL transformation, prevent navigation from overwriting an imported draft, render sanitized HTML in Ace tooltips, serialize real desktop tooltip loads, correct physical/distribution descriptions, and require fresh reference evidence. The combined pre-packaging-fix commit `5583086` passed local `Release-Verify`: 300 core, 947 numerical, 32 desktop, 116 API, 144 portable and 30 reference tests; the existing unsupported server-side 3D API case remained the sole documented skip. Its focused, real-stack, custom-function and packaged-local browser reports contain 108 passing cases. Later commits must supply their own fresh evidence.

That review also exposed an incomplete GSL rollout: the actual MSI and published API still selected embedded GSL 2.3. Normal Windows publishing now stages the reviewed 2.8 libraries with corresponding source/build material and notices, then checks the deployed resolver and numerical results. The API verifies the library inside an actual computation worker; installed MSI validation repeats the deployed resolver check. The existing desktop/API fallback remains available for development outputs without a packaged native directory. See [GSL adoption](followups/GSL-ADOPTION.md). The final candidate must pass the complete workflows and installer lifecycle again after this change.

## Evidence already reviewed

| Area | Evidence and limit |
| --- | --- |
| Combined migration | Integration commit `bebc44b709f23ffb4c91a4dcc4a36aa543c7916e` passed all 11 GitHub workflows and local Cake `Publish`: 252 core, 947 numerical, 20 desktop, 81 API tests, one allowlisted server-side 3D skip, 16 focused browser checks, published desktop/API process checks and three ZIP payload checks. Real browser, portable, native, installer-source, MSIX and provider-mirror workflows also passed. [Integration PR and current checks](https://github.com/PawelTroka/Computator.NET/pull/149). Recheck later release commits. |
| Framework and .NET 10 checkpoints | #139 (`2b1d16f`) and #140 (`6944238`) passed local Cake Windows validation at their reviewed heads, including numerical, compiler, API and desktop workflow tests. Both checkpoints are merged. Later integrated changes require their own complete run. |
| Current editor/test packages | Audited build and 252 core / 20 desktop tests passed with the newer Scintilla and Test SDK pins, including published desktop startup and normal shutdown. Exact versions remain in the manifests. |
| Web workflows | 38 browser checks passed during component review: 16 focused checks with a mocked API, plus 22 checks against the actual protected local API (10 general workflows and 12 custom-function workflows), across Chromium and Firefox. This is bounded workflow coverage, not every numerical method or a public hosting certification. |
| MSI/Burn | Self-contained payload staging and installer compilation succeeded after explicit owner acceptance of the WiX 7 terms. Installation, historical upgrades and signing have not been certified by this first-wave evidence. See the separate historical acceptance scope below. |
| MSIX | Windows SDK package validation and unpacked payload hash checks succeeded for an unsigned candidate retaining the existing Store identity. Installation/activation and Store acceptance remain. |
| Native/portable work | The first-wave GSL 2.8 build and partial portable engine/CLI had dedicated validation targets. The third review now adopts GSL 2.8 for Windows packages as described above. Check each OS/RID's actual final-commit CI before claiming support. |

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

## Historical installer acceptance follow-up

The original official 2.3.2 download is a Burn **EXE**, with a 32-bit MSI and a separate bundle registration. [Historical upgrade acceptance](HISTORICAL-UPGRADE-ACCEPTANCE.md) records the inspected package identities and SHA-256, real settings-serializer fixtures, Cake targets, EXE-to-EXE lifecycle assertions, failed-MSI rollback scope and candidate provenance. [Offline Sandbox acceptance](CLEAN-MACHINE-ACCEPTANCE.md) packages the same test without a guest SDK requirement and verifies both candidate MSI and EXE hashes.

The acceptance implementation and its contract tests are not machine-execution evidence. SDK-equipped hosted Windows results, when recorded for exact candidate bytes, remain distinct from the actual offline no-SDK gate. Neither historical package inspection nor a successful hosted run authorizes signing or publication. Keep the release checkbox below open until the required clean-machine result is recorded.

## Release gates

- [ ] Record passing final-commit GitHub results for Windows suites, coverage, portable ZIPs, focused and real browser suites, portable OS jobs, native candidate jobs, installer source, MSIX and provider mirrors. Remaining skipped tests must match the exact result-guard allowlist; do not weaken numerical expectations to obtain a pass.
- [ ] Test backed-up real settings and `.tsl`/`.tslf` documents, JSON import/backup/rollback, all editors, clipboard/IME input, custom functions and chart appearance/zoom/export. Complete high-DPI and multi-monitor acceptance.
- [ ] Approve the final release version in `Version.props` and refresh the shared attributes. The current 3.0.0-preview.1 identity is a review candidate. Confirm the MSIX version advances the actual Store package.
- [ ] On clean disposable Windows machines without a development SDK, test installation, launch, repair, uninstall, failed-upgrade rollback and upgrade from the original official 2.3.2 EXE and its embedded MSI. Verify MSI/Burn registrations, shortcuts, file associations and preservation/migration of user data using the exact candidate MSI and EXE hashes.
- [ ] Sign with owner-controlled release credentials; validate MSIX installation and Start-menu/file activation, upgrade from the actual previous Store package, settings/native-cache writes, offline use and uninstall policy. Complete Store certification before submission/release claims.
- [ ] Verify the final package's adopted GSL 2.8 payload, matching source/build archive and notices, actual resolver reports, intended native/managed ABI and numerical jobs, and difficult-input behavior. Keep the existing published releases available for rollback.
- [ ] Verify genuine AppVeyor and Travis final-commit runs with native provider URLs/build IDs and completion evidence if those providers are advertised as active gates.
- [ ] Approve release notes that state the remaining scope: symbolic calculation and server-side 3D rendering are unfinished; the portable CLI is a subset, and the full Windows numerical/graphics engine is not yet a Mac/Linux desktop application.

The supported web deployment is a trusted local service: both hosts bind to loopback, the frontend uses the protected proxy, and the shared token stays server-side. JSON computation/chart requests execute in owned workers that are retired on cancellation or deadline; legacy routes retain their compatibility behavior. These workers run with the user's permissions and are not a public multi-user sandbox. Public hosting still requires a separately reviewed isolation and authorization design.

No target in this checklist implicitly signs an artifact, publishes a GitHub release or submits an application to the Store. Keep the existing releases and backed-up user data available for rollback.
