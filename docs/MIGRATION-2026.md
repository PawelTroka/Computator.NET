# 2026 compatibility migration

Version inventory date: **2026-10-12**. This document distinguishes implemented upgrades, compatibility pins and release gates. It does not certify an untested release.

## Runtime boundaries

| Area | Target | Reason |
| --- | --- | --- |
| Desktop, charting, core, localization, data types | .NET Framework 4.8.1 | Preserve Windows Forms/WPF, binary settings, native numerical/editor dependencies and chart controls while updating development tools. |
| Mathematical Web API | .NET Framework 4.8.1 with Framework-compatible ASP.NET packages 2.3.13 | Its chart factory and computation core still depend on the desktop graph. These packages are not the obsolete .NET Core 2.1 runtime. |
| Web-client host | ASP.NET Core 10 / net10.0 | Independent of the desktop graph; migrated to the modern hosting model. |
| C# build and TSL-generated C# | C# 14; SDK 10.0.401; Roslyn 5.9.0 | Modern compiler for the application and runtime-generated expressions/scripts. |

**Using the .NET 10 SDK does not make the desktop a .NET 10 application.** A later desktop runtime port must replace or migrate the compatibility-bound components below. Do not merely change the target framework and assume the runtime behavior remains equivalent.

Current desktop builds require an operating system compatible with .NET Framework 4.8.1. Test releases on a supported Windows installation. The current branch no longer targets Windows XP/Vista/7/8.1 or the old Mono rendering path. Historical 2.3.2 downloads remain separate.

## Direct dependency and toolchain inventory

The authoritative exact values are in `global.json`, `.nvmrc`, `Directory.Packages.props` and `Computator.NET.WebClient/package.json`. NuGet and npm lockfiles preserve the resolved transitive graph and content hashes.

| Technology | Migration version / decision |
| --- | --- |
| SDK / C# / Roslyn | 10.0.401 / 14.0 / 5.9.0 |
| MathNet.Numerics / Meta.Numerics | 5.0.0 / 4.2.0 |
| NLog / Unity | 6.2.1 / 5.11.10 |
| AvalonEdit | 6.3.1.120 |
| NUnit / adapter / Microsoft.NET.Test.Sdk / Moq | 5.0.0 / 6.3.1 / 18.10.1 / 4.21.0 |
| Framework-compatible ASP.NET packages | 2.3.13 |
| Newtonsoft.Json / System.Text.Json | 13.0.4 / 10.0.12 |
| Application Insights | 3.2.0 on net10.0; compatible 2.23.0 on net481 |
| Swashbuckle | Compatible 8.0.0, including Newtonsoft support; newer major versions require modern .NET |
| React / React DOM / React types | 19.3.0 |
| React Router | 7.18.4 |
| TypeScript / Vite / Vite React plugin | 7.0.2 / 8.3.4 / 6.1.2 |
| React Ace / Ace | 15.0.0 / 1.44.0 |
| Bootstrap / Playwright | 5.3.8 / 1.64.0 |
| Node / npm | 24.21.0 LTS / 11.19.0; deliberately not the Node Current release line |

Old packages are not replaced by similarly named successors without API and runtime verification. Accord.Math 3.8.0, Guards 2.1.19292.12, jacobslusser.ScintillaNET 3.6.3 and TROKA.Software.SimpleCodeEditor 2.0.0 remain compatible pins. Bundled PowerPacks and native GSL/editor binaries are retained, **not claimed to be newly rebuilt or latest upstream**. Transitive versions reflect a tested, locked dependency graph, not a promise that every indirect dependency is its highest published version.

## Behavioral migration work

### Runtime compilation

`NativeCompiler` now uses Roslyn instead of an installed CodeDOM `csc.exe`. It resolves Framework assemblies by strong-name/type anchors, including the netstandard facade needed by current numerical packages. Modern .NET trusted platform references are recognized without claiming that the rest of the application has been ported.

Generated assemblies use unique identities. Compilation occurs in memory, avoiding old compiler temporary-file handling. The existing `CompilationException` contract, error codes, main-code/custom-function classification, editor-relative line offsets and localized headings are preserved. Warnings remain nonfatal. Tests cover C# 14 and dynamic binding, numerical library references, repeated and concurrent compilation, syntax diagnostics, and recovery after an invalid expression.

Compiled scripts still execute with full trust and generated assemblies are not a sandbox or a security boundary. This change does not implement process isolation or collectible assembly unloading.

### Dependency injection and resources

Unity extension points have been adapted to the current API. Interface registrations preserve the lifetime of their concrete view/presenter, rather than constructing a second singleton. Constructor-name overrides remain case-insensitive, distinguish scripting and custom-function editors, honor fallback values, and do not replace injected properties. Matching no longer uses mutable shared state.

SDK-style WinForms/WPF builds retain the original resource naming convention, generated public resource designers and satellite resources. WPF pages are updated rather than included twice. Runtime data and examples are copied transitively into output and publish directories. Binding redirects are generated from the restored graph rather than copied from old dependency versions.

### Web client and host

React 19's root API and current router replace the old React 15 setup. Vite replaces webpack 3 and obsolete ASP.NET SPA middleware. Strict TypeScript checking is part of the production build. UI state updates are immutable; numeric fields preserve normal typing; pending/failed requests are explicit and requests are aborted on navigation.

The dark navigation layout and existing operations are retained. Chart resizing now uses the actual Y minimum instead of accidentally reusing the X minimum. Ace completers are per-editor, editor modes are bundled locally, and autocomplete descriptions are rendered as text rather than executable HTML. Mathematical multiplication notation remains `·`.

The .NET 10 host serves the published assets, preserves direct SPA routes and the sample controller, and returns 404 for unknown API endpoints instead of HTML with status 200. Telemetry remains optional. The mathematical API stays separate; the frontend defaults to its original local port and can override it through `VITE_API_BASE_URL` at build time.

## Reproducible validation

On Windows, with PowerShell 7, the pinned SDK and Framework 4.8.1:

```powershell
pwsh ./build/Validate-Migration.ps1
```

The script checks `.sln`/`.slnx` project parity, performs a locked restore and Release build, runs core unit tests, numerical integration tests, Web API integration tests and desktop smoke tests, then publishes a portable desktop candidate and checks critical files. It fails on a failed command. It does not install the candidate or alter a release.

For deliberate dependency changes only, update central versions and run the script with `-UpdateLocks`; review and commit every changed lockfile. Normal CI and normal development verification must use locked restores. Do not delete lockfiles to get past an incompatible update.

Web validation uses `npm ci`, `npm audit`, strict TypeScript compilation, production bundling, a locked .NET host restore/publish and Playwright in Chromium and Firefox. CI browser tests run against the actual published ASP.NET Core 10 host. **The separate mathematical API is mocked in these browser tests**; its actual behavior is covered by the Windows API integration suite. This is not a claim of full cross-process, real-browser-to-real-mathematical-API coverage.

The original ignored API test for escaped division in path segments is a pre-existing route limitation, not silently fixed by upgrading the UI. Tests that only verify client URL encoding do not establish end-to-end support for that case.

At commit `fa6e23de7ce41c9b9b4921bc7b1762aa6d15e293`, Windows CI passed **223 core tests and 947 core integration tests**, with no failures or skips. This is a historical checkpoint, not a substitute for the checks on the final PR head. The PR and Actions artifacts record subsequent API, desktop and browser results.

## Before merge and before release

Keep the migration draft until its current-head checks pass and the compatibility decisions have been reviewed. Before distributing it, record the following manual gates:

- Back up a real existing installation, user configuration, custom-function directory and representative `.tsl`/`.tslf` files. Launch the candidate side-by-side; verify settings, fonts, languages, file opening/saving, custom functions and restart persistence. No serializer format or assembly identity bump is intentionally performed here, but this still needs real-profile evidence.
- Exercise real, complex, implicit and 3D charts; zoom/resize/export them; test high DPI and multiple monitors; switch all desktop editor implementations. A startup smoke test is not visual rendering or interaction coverage.
- Run the real web UI against the real Windows mathematical API, including special symbols, errors, numerical-method selection and representative scripts. Keep the API private and loopback-only. CORS is not authentication, and anonymous full-trust script execution must not be publicly exposed.
- Rework and validate MSI/bootstrapper prerequisites, recursive payload files, shortcuts, file associations, upgrade, repair and uninstall before shipping a new installer. The legacy installer still targets historical layouts/framework prerequisites and is outside the active solution. Do not wrap the new binaries in the old bootstrapper.

Rollback means returning to the previous application binaries and the backed-up user data, not trying to downgrade Windows' in-place Framework runtime. Do not overwrite a real user profile while testing rollback. Preserve a copy of every original document used in migration tests.

## Follow-on work with explicit boundaries

A full `net10.0-windows` desktop migration requires an independently reviewed settings-format migration, replacement/verification of PowerPacks and native controls, and corresponding chart/editor and installer coverage. The old Coded UI suite needs a supported replacement rather than a cosmetic MSTest package bump. Native library upgrades need reproducible builds and numerical regression evidence. These are not represented as completed work in this PR.

## Version and platform sources

- [.NET 10 download and release metadata](https://dotnet.microsoft.com/download/dotnet/10.0)
- [.NET Framework 4.8.1 system requirements](https://learn.microsoft.com/dotnet/framework/get-started/system-requirements)
- [Framework-compatible Microsoft.AspNetCore 2.3.13](https://www.nuget.org/packages/Microsoft.AspNetCore/2.3.13)
- [Swashbuckle 8 framework assets](https://www.nuget.org/packages/Swashbuckle.AspNetCore.SwaggerGen/8.0.0) and [9 framework boundary](https://www.nuget.org/packages/Swashbuckle.AspNetCore.SwaggerGen/9.0.6)
- [Application Insights packages](https://www.nuget.org/packages/Microsoft.ApplicationInsights.AspNetCore)
- [Official Node release metadata](https://nodejs.org/dist/index.json)
- [Official npm registry](https://registry.npmjs.org/) and [NuGet](https://www.nuget.org/), queried during the migration; exact resolved artifacts and hashes are in the committed lockfiles.
