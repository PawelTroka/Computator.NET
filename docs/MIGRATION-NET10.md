# .NET 10 runtime migration and integrated follow-ups

The Framework compatibility checkpoint (#139) and .NET 10 runtime port (#140) are merged into `master`. This guide covers that runtime and the integrated web, native-library, portable-engine and distribution follow-ups. It describes a release candidate, not a published 3.0 release. See [RELEASE-3.0.md](RELEASE-3.0.md) for review evidence and outstanding gates.

**Use [BUILD.md](BUILD.md) for build/test/package commands and dependency policy.** Cake owns the base and follow-up task graphs. The old `Validate-Net10.ps1` and `Validate-Migration.ps1` entry points are forwarding shims. Per-project NuGet lockfiles and locked-restore requirements are removed; the one frontend npm lockfile remains for `npm ci`.

## Runtime targets

Desktop, controls, core, charting, data types, localization, mathematical API and the four Windows test projects target **net10.0-windows**. The independent SPA host and extracted engine/CLI target **net10.0**. Windows Forms/WPF and the existing mathematical UI are retained. The mathematical API remains Windows-only because its renderer uses desktop controls.

The portable engine shares the existing TSL transformations, notation and constants with Windows code and adds a headless CLI. Its first supported subset includes arithmetic, scalar functions and managed scripts with custom definitions. The complete GSL/special-function catalog, matrix/numerical services and renderer are still Windows-bound; a portable full desktop application is not claimed. See [PORTABLE-ENGINE.md](followups/PORTABLE-ENGINE.md).

SDK, C#, Cake, NuGet and frontend versions are declared in `global.json`, shared props, `.config/dotnet-tools.json`, `Directory.Packages.props`, `.nvmrc` and `package.json`. The modern chart/editor dependencies include WinForms.DataVisualization and Scintilla5.NET; the API uses the ASP.NET Core 10 shared framework and current compatible Newtonsoft/Swagger/telemetry packages. Inspect the manifests rather than maintaining another duplicate version table.

## Settings and rollback

Production code no longer serializes/deserializes settings with BinaryFormatter or re-enables its unsafe implementation. It uses bounded, versioned JSON with explicit enum, culture, directory and font descriptors. The known legacy NRBF field layout is read with `NrbfDecoder` into data records; payload constructors, callbacks and arbitrary object types are not activated.

Successful import writes `settings.json` separately and leaves `settings.dat` unchanged. Existing JSON takes precedence. Normal writes use flushed temporary files and atomic replacement with a uniquely named backup. Malformed, oversized and newer-schema data is not silently deleted or overwritten. After a failed load, automatic persistence is disabled; an explicit user reset can recover while preserving the rejected JSON backup. Invalid replacement data remains rejected.

Cake generates the known fixture with the real Framework serializer and tests import under .NET 10. Framework is needed by that fixture writer, not production import. This fixture is not proof of compatibility with every historical real profile. Test copies of actual settings/documents before accepting the migration. Rollback to historical Framework binaries uses the retained binary preferences and backups, not later JSON changes.

For a disposable manual profile:

```powershell
$env:COMPUTATOR_DATA_DIRECTORY = Join-Path $env:TEMP 'Computator-NET10-Trial'
dotnet run --project Computator.NET
```

Copy a backup rather than the only original into the trial directory. TSL/TSLF document formats are not intentionally changed. Cake's automated tests create and clean separate temporary profiles and restore the caller's environment.

## Custom functions: full desktop regression

The initial migration's nested Unity factory aliases dropped constructor-role overrides. The visible Custom Functions editor differed from the hidden editor used by presenters/file commands/error handling. Earlier compiler and API tests could therefore pass despite the real desktop being broken.

The shared fix preserves composition-root role bindings across factory boundaries, shares exactly the intended scripting/custom-function instances and fails explicitly for missing role bindings rather than silently creating clones. Repeated main-window requests do not duplicate presenter registrations.

The normal Cake desktop suite now loads a real `.tslf` into each visible editor implementation, calculates with it, applies unsaved edits, executes a script, saves/reopens the file and verifies the highlighter uses the same displayed control. These regressions were demonstrated failing before the fix. Compiler-only tests remain useful but are not a substitute for this composition-level coverage.

## Other runtime compatibility work retained

PowerPacks color shapes and retired context menus are replaced with supported WinForms controls. File-tree operations guard root/stale nodes and avoid deleting a rename destination. Icon ownership uses public FromHandle/Clone APIs and native cleanup rather than private Framework constructors. PerMonitorV2 DPI mode and text rendering are selected before window creation.

All three editor choices remain available and have Unicode/document-switch/rename smoke coverage. `TROKA.Software.SimpleCodeEditor 2.0.0` is still a Framework-targeted vendor binary: its NU1701 warning remains visible, and passing tests are not official modern-runtime vendor support. GSL uses a rooted callback and explicit native resolver/cache instead of mutating global PATH.

Windows publication now stages the reviewed GSL 2.8 build with pinned source/toolchain inputs, matching source material, provenance and actual resolver probes. `Native-GSL-Validate` additionally exercises the numerical and desktop suites on Windows. Development outputs without a staged payload retain the embedded legacy fallback; an absolute `COMPUTATOR_GSL_DIRECTORY` can select a trusted staged build, with hash/architecture/version validation. Final release acceptance still requires the intended native jobs and difficult-input checks. See [GSL.md](followups/GSL.md).

Decimal display formats decimal values directly rather than first converting them to double; generated TSL and host share the formatter. Double precision is not reduced. Chart rendering releases per-request resources and restores the calling synchronization context so HTTP response writes do not wait on a nonexistent WinForms message loop. Image tests decode and check PNG/BMP/TIFF/JPEG/GIF formats and dimensions, including large BMP responses. The mathematical API honors explicit `--urls` while retaining its loopback default.

## Validation and distribution

```powershell
./build.ps1 -Target CI
# Windows suites, focused browser tests and portable desktop/API/web ZIPs:
./build.ps1 -Target Publish
# Production browser-to-API workflows; run these sequentially:
./build.ps1 -Target Web-E2E
./build.ps1 -Target Web-CustomFunctions
```

Cake owns audited restore, build, all four Windows suites, result guards, frontend npm/audit/TypeScript/Vite/browser tasks and local packaging. Coverage is available through `Calculate-Coverage`; ZIP payloads and names are checked. Follow-up workflows run on every PR and `master` push. Exact-commit CI remains the authority; the release checklist must be updated after final combined validation.

The .NET 10 publication checks launch the actual desktop EXE, require a responsive main window and normal shutdown, and launch the API as a real Kestrel process to calculate, execute a Roslyn script, return PNG/large BMP and generate OpenAPI. `Web-Validate` retains focused tests with a mocked mathematical API. `Web-E2E` and `Web-CustomFunctions` additionally drive Chromium and Firefox through the published SPA, protected loopback proxy and real Windows API.

Web custom definitions now follow unsaved editor changes, explicit session saves/reloads and UTF-8 `.tslf` import/export. JSON execution/chart requests carry definitions in the body; fresh evaluators prevent mutable compiler or generated-function state being shared between callers. Legacy GET compatibility remains covered separately. See [WEB-CUSTOM-FUNCTIONS.md](followups/WEB-CUSTOM-FUNCTIONS.md).

Any remaining historical skips must match the exact allowlist in `build/Assert-SuiteResults.ps1`; no relaxed numerical tolerances or blanket skip allowance establish parity. Symbolic calculation and server-side 3D rendering remain unfinished. Windows desktop CI exercises x64; x86 remains uncertified, native Windows ARM64 is unsupported, and trimming/NativeAOT/single-file publication are not supported.

Manual acceptance still covers backed-up real profiles/documents, complete editor/clipboard/IME interactions, chart appearance/zoom/export and high-DPI/multi-monitor behavior. The automated browser coverage does not certify every numerical method or deployment configuration. Framework-dependent ZIPs require the .NET 10 Desktop runtime and ASP.NET Core 10 for the API.

MSI/Burn and MSIX now share a self-contained win-x64 desktop payload. MSI/Burn compilation has succeeded with explicitly accepted WiX 7 terms. MSIX packaging validates the manifest, preserves the existing Store identity and checks unpacked file hashes; the result is unsigned. Clean-machine install/repair/uninstall, historical upgrades, signing and Store acceptance remain unexecuted release gates. See [INSTALLER.md](followups/INSTALLER.md), [MSIX.md](followups/MSIX.md) and [RELEASE-3.0.md](RELEASE-3.0.md).

TSL executes fully trusted code. The protected local proxy uses a server-side token and fixed loopback backend, with origin/Host/body checks; both hosts must remain loopback-only. These controls, fresh evaluators and cancellation do not provide process/resource isolation for public multi-user execution. No build target deploys the API publicly or creates a GitHub release.

## Primary references

- [Read NRBF payloads without BinaryFormatter deserialization](https://learn.microsoft.com/dotnet/standard/serialization/binaryformatter-migration-guide/read-nrbf-payloads)
- [Cake tasks](https://cakebuild.net/docs/writing-builds/tasks/)
- [Numeric format strings](https://learn.microsoft.com/dotnet/standard/base-types/standard-numeric-format-strings)
