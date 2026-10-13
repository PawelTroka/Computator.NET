# Stacked .NET 10 runtime migration (#140)

This branch targets `modernize/2026-compatible-toolchain` (#139), not `master`. Shared custom-function fixes and Cake build changes are merged from the parent while preserving the runtime-specific implementation. Neither PR is merged or released automatically.

**Use [BUILD.md](BUILD.md) for all build/test/package commands and dependency policy.** Cake is the orchestrator on both branches. The old `Validate-Net10.ps1` and `Validate-Migration.ps1` entry points are forwarding shims, not independent pipelines. Twelve per-project NuGet lockfiles and locked-restore requirements have been removed; the one frontend npm lockfile remains for `npm ci`.

## Runtime targets

Desktop, controls, core, charting, data types, localization, mathematical API and the four Windows test projects target **net10.0-windows**. The independent SPA host targets **net10.0**. This is the actual modern runtime, not Framework compiled with a new SDK. Windows Forms/WPF and the existing mathematical UI are retained. The mathematical API remains Windows-only because its renderer uses desktop controls.

SDK, C#, Cake, NuGet and frontend versions are declared in `global.json`, shared props, `.config/dotnet-tools.json`, `Directory.Packages.props`, `.nvmrc` and `package.json`. The modern chart/editor dependencies include WinForms.DataVisualization and Scintilla5.NET; the API uses the ASP.NET Core 10 shared framework and current compatible Newtonsoft/Swagger/telemetry packages. Inspect the manifests rather than maintaining another duplicate version table.

## Settings and rollback

Production code no longer serializes/deserializes settings with BinaryFormatter or re-enables its unsafe implementation. It uses bounded, versioned JSON with explicit enum, culture, directory and font descriptors. The known legacy NRBF field layout is read with `NrbfDecoder` into data records; payload constructors, callbacks and arbitrary object types are not activated.

Successful import writes `settings.json` separately and leaves `settings.dat` unchanged. Existing JSON takes precedence. Normal writes use flushed temporary files and atomic replacement with a uniquely named backup. Malformed, oversized and newer-schema data is not silently deleted or overwritten. After a failed load, automatic persistence is disabled; an explicit user reset can recover while preserving the rejected JSON backup. Invalid replacement data remains rejected.

Cake generates the known fixture with the real Framework serializer and tests import under .NET 10. Framework is needed only by that fixture writer, not production import. This fixture is not proof of compatibility with every historical real profile. Test copies of actual settings/documents before accepting the migration. Rollback to the parent uses the retained binary preferences, not later JSON changes.

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

All three editor choices remain available and have Unicode/document-switch/rename smoke coverage. `TROKA.Software.SimpleCodeEditor 2.0.0` is still a Framework-targeted vendor binary: its NU1701 warning remains visible, and passing tests are not official modern-runtime vendor support. GSL uses a rooted callback and explicit native resolver/cache instead of mutating global PATH. Its bundled binaries are retained, not claimed to be newly rebuilt upstream versions.

Decimal display formats decimal values directly rather than first converting them to double; generated TSL and host share the formatter. Double precision is not reduced. Chart rendering releases per-request resources and restores the calling synchronization context so HTTP response writes do not wait on a nonexistent WinForms message loop. Image tests decode and check PNG/BMP/TIFF/JPEG/GIF formats and dimensions, including large BMP responses. The mathematical API honors explicit `--urls` while retaining its loopback default.

## Validation and distribution

```powershell
./build.ps1 -Target CI
# All Windows and browser tests, plus portable desktop/API/web ZIPs:
./build.ps1 -Target Publish
```

Cake owns audited restore, build, all four Windows suites, result guards, frontend npm/audit/TypeScript/Vite/browser tasks and local packaging. Coverage is available through `Calculate-Coverage`; ZIP payloads and names are checked. Current-head CI is the authority, not an earlier green commit or a count copied into documentation.

The .NET 10 publication checks launch the actual desktop EXE, require a responsive main window and normal shutdown, and launch the API as a real Kestrel process to calculate, execute a Roslyn script, return PNG/large BMP and generate OpenAPI. Browser tests use the published SPA host but mock the mathematical API; the separate Kestrel and in-process tests do not establish complete browser-to-real-API end-to-end coverage.

Three historical API skips remain allowlisted: division escaping in calculation/script URLs and server-side 3D rendering. No new skips or relaxed numerical tolerances are used to obtain passing tests. CI exercises Windows x64; x86 remains uncertified, native ARM64 is unsupported, and trimming/NativeAOT/single-file publication are not supported by this candidate.

Manual acceptance still covers backed-up real profiles, documents, custom functions, all editor/clipboard/IME interactions, chart appearance/zoom/export, high-DPI/multi-monitor behavior and the real browser connected to the real Windows API. Framework-dependent distribution requires the .NET 10 Desktop runtime and ASP.NET Core 10 for the API.

The historical MSI/WiX/UWP installer path remains incomplete. Its Cake targets fail explicitly instead of packaging wrong prerequisites or signing with an old test key. Runtime-specific prerequisites, payloads, upgrade/repair/uninstall, file associations and signing need implementation/validation before an installer release. Portable ZIPs are not installer certification.

TSL executes fully trusted code. Keep the anonymous mathematical API private and loopback-only; CORS and settings-format changes do not sandbox scripts. No build target deploys it publicly or creates a GitHub release.

## Primary references

- [Read NRBF payloads without BinaryFormatter deserialization](https://learn.microsoft.com/dotnet/standard/serialization/binaryformatter-migration-guide/read-nrbf-payloads)
- [Cake tasks](https://cakebuild.net/docs/writing-builds/tasks/)
- [Numeric format strings](https://learn.microsoft.com/dotnet/standard/base-types/standard-numeric-format-strings)
