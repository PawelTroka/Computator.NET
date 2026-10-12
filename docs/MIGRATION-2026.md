# Framework compatibility checkpoint (#139)

This guide describes `modernize/2026-compatible-toolchain`. The stacked #140 branch performs the actual .NET 10 runtime port and has its own `MIGRATION-NET10.md` guide. **The shared build entry point is Cake: see [BUILD.md](BUILD.md).** Earlier PowerShell build orchestration and per-project NuGet lockfiles have been removed; compatibility wrapper scripts now select Cake tasks.

## Runtime boundaries

Desktop/core/charting/localization/data types target **net481**. The mathematical API also targets net481 and uses Framework-compatible ASP.NET packages. The independent SPA host targets **net10.0**. SDK/C# versions are in `global.json` and shared props; using the .NET 10 SDK does not make this branch's desktop a modern-.NET application.

The exact dependency inventory is maintained in `Directory.Packages.props`, the web client's `package.json`, `.nvmrc` and the Cake tool manifest. Framework-bound Swagger/Application Insights versions are intentional compatibility pins, not claims to the newest major release. Native GSL, PowerPacks and the third editor binary are retained pending their separate runtime migration. No transitive NuGet lock graph is committed; auditing and regression tests remain gating.

## Behavioral work and the custom-function correction

The compiler uses Roslyn rather than an installed CodeDOM compiler. Generated assemblies have unique identities and retain the existing localized diagnostic structure and editor-relative offsets. Numerical-library, modern syntax, dynamic binding, repeated/concurrent compilation and error-recovery tests cover that boundary.

The first Unity migration preserved singleton lifetimes but lost editor-role overrides at nested factory calls. This created hidden editors: custom functions visible in the window were absent from the editor read by presenters and the exception highlighter. Compiler/API tests alone did not detect it. The corrected composition root now reapplies its own role bindings across factory aliases, shares the intended scripting/custom-function instances, rejects missing editor roles instead of inventing hidden controls, and avoids registering presenters twice for a repeated main-window request.

The desktop regression composes the real window, defines a custom function in each visible editor implementation, calculates, edits without saving, runs a script, saves and reopens the `.tslf`, and verifies highlighter identity. A test-only commit reproduced four failures before the wiring fix; the regression is kept in the normal Cake suite.

SDK project cleanup preserves resource names, localization satellites, WPF page handling and runtime content. The React/TypeScript/Vite migration retains mathematical notation, checks errors explicitly and tests the published .NET 10 SPA host in Chromium and Firefox. Browser tests mock the separate mathematical API; Windows API tests exercise real in-process endpoints.

## Verify and package

```powershell
./build.ps1 -Target CI
./build.ps1 -Target Publish
```

`CI` runs the four Windows suites and checked desktop/API publication. `Publish` additionally validates the frontend/browsers and produces local ZIP packages. All test runs use isolated temporary profiles and verified TRX reports. Check actual current-head CI, not a historical test count or a previously green commit.

## Acceptance and rollback

Before distribution, test backed-up real settings/documents, all editor interactions, high-DPI/multi-monitor rendering, real/complex/3D charts and export, and the real frontend connected to the Windows API. This branch still uses legacy BinaryFormatter settings; it has not acquired the safer JSON importer from the child runtime branch. Keep profile copies separate when testing rollback.

The historical installer needs updated prerequisites, recursive payload layout, shortcuts/file associations, upgrade/repair/uninstall and signing validation. Cake keeps the target name but will not silently produce the old invalid package. XP/Mono/UWP and VS2017 Coded UI are not supported active targets. Returning to old binaries and a backed-up profile is the rollback path; do not attempt to downgrade the in-place Framework runtime.

TSL remains full-trust code. Keep its anonymous API private/loopback-only. No package, build task or settings migration makes untrusted scripts safe.
