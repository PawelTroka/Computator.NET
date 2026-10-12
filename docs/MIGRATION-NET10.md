# Stacked .NET 10 runtime migration

This experimental PR targets **`modernize/2026-compatible-toolchain` (PR #139)**, not `master`. Its head is `modernize/net10-runtime` (PR #140). The parent Framework 4.8.1 checkpoint is left unchanged. Review the runtime changes independently and merge the parent first if both are accepted.

## Actual runtime targets

| Component | Target |
| --- | --- |
| Desktop launcher, desktop controls, core, charting, data types, localization | `net10.0-windows` |
| Mathematical API | `net10.0-windows`, ASP.NET Core 10 shared framework |
| Independent SPA host | `net10.0`, ASP.NET Core 10 |
| All four active Windows test projects | `net10.0-windows` |

The mathematical API remains Windows-only because its chart renderer uses WinForms/WPF. This is a full modern-.NET runtime target for the active application graph, **not** a claim that every dependency was rewritten, every legacy platform retained, or the old installer replaced. The SDK remains pinned in `global.json` and both application and runtime-generated code use C# 14/Roslyn.

## Settings migration and rollback

Production code no longer uses BinaryFormatter serialization/deserialization or an unsafe re-enablement switch. Settings are an explicit, bounded JSON schema with `SchemaVersion = 1`, enum validation and font descriptors. The importer reads the old NRBF records using `System.Formats.Nrbf.NrbfDecoder`, validates expected types and members, and constructs only the known settings/font objects. It does not activate payload types or callbacks.

On first successful import, `settings.json` is written separately and **`settings.dat` remains byte-for-byte unchanged**. Existing JSON takes precedence on later starts. Normal writes use a flushed temporary file and atomic replacement, preserving the previous JSON in a uniquely named backup. A malformed, oversized or newer-schema file is not deleted or silently replaced. Persistence is disabled after a failed load; an explicit settings reset can recover while backing up the rejected JSON. Invalid replacement data is still rejected.

Back up your complete real profile and representative documents before testing. The automated fixture reproduces the known legacy field layout and is generated using the real .NET Framework serializer, but this is not evidence for every historical user profile. Validate your own language/fonts, directory preferences, custom functions and restart persistence before acceptance. Editing the new JSON does not update the retained binary settings, so rollback to the parent branch will use the old preferences.

To avoid your normal profile altogether:

```powershell
$env:COMPUTATOR_DATA_DIRECTORY = Join-Path $env:TEMP 'Computator-NET10-Trial'
dotnet run --project Computator.NET
```

The variable selects a data directory for this process and children. Copy a **backup**, not the only original, into that directory when testing legacy import. `.tsl` and `.tslf` document formats are not intentionally changed. Keep separate copies of documents when comparing the two branches.

## Compatibility changes

### UI, charting and native libraries

PowerPacks shapes are replaced by standard WinForms color controls in the chart dialogs. Framework `ContextMenu`/`MenuItem` calls are replaced with `ContextMenuStrip`/`ToolStripMenuItem`. Root-directory actions are guarded, stale tree nodes are discarded on refresh, and file renaming no longer deletes an existing destination. Icon ownership uses public `Icon.FromHandle`/`Clone` APIs and native handle cleanup, not private Framework constructors. PerMonitorV2 DPI awareness is selected before creating windows, preserving the earlier manifest's intended mode.

The modern chart package is **WinForms.DataVisualization 1.10.2**. Scintilla uses **Scintilla5.NET 7.0.0** with the appropriate modern API changes. AvalonEdit remains available. The original `TROKA.Software.SimpleCodeEditor 2.0.0` binary is retained so the third editor is not silently removed; NuGet's **NU1701 warning remains visible**. Unicode, multiple documents, switching and renaming are exercised for all three editors, but that is not a guarantee of every editing feature or official vendor support for .NET 10.

GSL is loaded through an explicit `NativeLibrary` resolver from the known embedded x64/x86 payload, with a rooted callback and per-architecture cache. Global PATH modification is not required. These native binaries are retained rather than claimed to be new upstream builds. CI validates **x64**; x86 needs its own release evidence. Native ARM64, trimming, NativeAOT and single-file publication are not supported by this candidate.

### Numerical output and HTTP behavior

Modern .NET's double formatting can reveal binary-rounding artifacts previously hidden by a decimal-to-double conversion. Decimal output now formats the decimal directly, preserving all its digits and historical compact/scientific display boundaries. Both the host and generated TSL helper use the same formatter. Double precision is not reduced to disguise differences, and the mathematical `·10` superscript notation is retained.

The mathematical API uses modern hosting and its original routes/JSON contracts. Explicit `--urls` configuration is honored; the default is still local port 60996. ASP.NET Core MVC Newtonsoft/TestHost packages are **10.0.12**, Swagger/Newtonsoft integration **10.3.0**, Application Insights **3.2.0**, and Newtonsoft.Json **13.0.4** is explicitly referenced rather than accidentally resolving an older transitive version. Exact versions and hashes are in central props and lockfiles.

Chart rendering releases per-request image/control resources and restores the caller's synchronization context. Creating WinForms handles on an HTTP thread must not leave a UI context behind: that thread has no WinForms message pump for later asynchronous response writes. Tests exercise large BMP responses as well as PNG/TIFF/JPEG/GIF, decode the images and check their format and dimensions. The nonconstant-payload assertion is linear rather than a general-purpose multiset comparison of hundreds of thousands of bytes.

## Reproducible verification

```powershell
pwsh ./build/Validate-Net10.ps1
```

The wrapper generates a real legacy fixture using Windows PowerShell/.NET Framework, then runs .NET 10 tests with an isolated temporary profile. **Framework is needed by the test-fixture writer, not by the production importer or applications.** An SDK installation supplies the modern development/runtime requirements; framework-dependent distributed candidates need the .NET 10 Windows Desktop runtime, plus ASP.NET Core 10 for the API.

CI performs locked restores, package audits, a Release solution build and independently runs four suites. Result guards reject missing reports, incomplete/failed runs, duplicate test identities, discovery below the recorded minimum and undocumented skips. The current minimums are 250 core tests, 947 numerical integration tests, 53 API tests (including three historical skips), and 16 desktop tests. These are **validation contracts, not a substitute for the current PR head's actual results**.

Publication is also checked: the desktop's actual executable must create a responsive main window and exit normally; the published API is launched as a real Kestrel process on an ephemeral loopback port and must calculate, execute a Roslyn script, return PNG and a large BMP, and generate its OpenAPI document. Runtime configuration files must identify .NET 10. These checks do not require the test runner to host the application.

The inherited Linux web-client job separately performs `npm ci`, an audit, strict TypeScript checking, production bundling, locked .NET 10 host publication and Chromium/Firefox browser tests. Those browser tests **mock the mathematical API**. Real Kestrel smoke checks are complementary, not full browser-to-real-mathematical-API end-to-end coverage.

The three pre-existing API skips remain visible: escaped division in calculation/script URL paths, and server-side 3D chart rendering. No additional skips or numerical tolerance reductions are introduced to make the runtime migration pass. Desktop WPF 3D dialog coverage is not equivalent to implementing server-side 3D rendering.

For deliberate dependency changes only, use `-UpdateLocks`, review all changed dependency graphs, then commit them and repeat normal locked validation. Do not make unlocked restores the permanent CI path.

## Before acceptance and distribution

Keep this PR experimental until its current-head checks and the following manual gates are satisfied: a backed-up real profile and document corpus; actual chart appearance, zoom/export and 3D interaction; high-DPI and multi-monitor layouts; all editor functions and clipboard/IME behavior; and the real web UI connected to the real Windows API. A green smoke test does not certify visual or interaction parity.

The historical WiX/MSI/bootstrapper remains outside the active solution. New .NET 10 prerequisites, installation layout, recursive payloads, shortcuts/file associations, upgrade/repair/uninstall and signing need a separate installer implementation and validation before a release. Do not wrap these new binaries in the old installer. Existing tagged release assets are not modified.

TSL remains fully trusted executable code. Keep the anonymous mathematical API private and loopback-only: CORS is not authentication or a sandbox. Replacing settings deserialization does not make untrusted scripts safe. No public deployment or release is performed by this migration's CI.

## Primary references

- [Microsoft: read NRBF payloads without BinaryFormatter deserialization](https://learn.microsoft.com/dotnet/standard/serialization/binaryformatter-migration-guide/read-nrbf-payloads)
- [Microsoft: numeric format strings and runtime differences](https://learn.microsoft.com/dotnet/standard/base-types/standard-numeric-format-strings)
- [Microsoft: process DPI awareness](https://learn.microsoft.com/dotnet/api/system.windows.forms.application.sethighdpimode)
- [WinForms.DataVisualization package](https://www.nuget.org/packages/WinForms.DataVisualization/1.10.2)
- [Scintilla5.NET package](https://www.nuget.org/packages/Scintilla5.NET/7.0.0)
- [Swagger package](https://www.nuget.org/packages/Swashbuckle.AspNetCore/10.3.0)
