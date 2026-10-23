# ![Computator.NET](Graphics/Assets/Wide310x150Logo.png)

Computator.NET is open-source numerical software for exploring mathematics through calculations, charts and scripts.

![Quick presentation](Graphics/Presentation/quick_presentation.gif)

It includes real, complex and two-variable function charts and calculations; numerical integration, derivatives and root finding; elementary, special and matrix functions; and the original TROKA Scripting Language (TSL), custom functions and mathematical notation.

## Develop and build with Cake

The original Cake build pattern is retained and modernized. `build.cake` owns restore, build, tests, coverage, frontend and packaging, with separate validation targets for the real web stack, portable engine, native libraries and installers. See [BUILD.md](docs/BUILD.md) for prerequisites, target descriptions and dependency policy.

```powershell
./build.ps1 -Target Build
./build.ps1 -Target AllTests
# All Windows checks and checked desktop/API publication:
./build.ps1 -Target CI
# All Windows and focused browser checks, plus local versioned ZIP packages:
./build.ps1 -Target Publish -Configuration Release
# Windows: browsers connected to the actual mathematical API:
./build.ps1 -Target Web-E2E
./build.ps1 -Target Web-CustomFunctions
```

Use the SDK from `global.json`, PowerShell 7, and Node/npm from `.nvmrc` and `Computator.NET.WebClient/package.json`. The wrapper restores the pinned Cake .NET tool; a global Cake install is unnecessary. `build.cmd -Target Build` and `dotnet tool restore` followed by `dotnet cake build.cake --target=Build` select the same graph.

### Current runtime

The Windows application now runs on **.NET 10** with its WinForms UI and WPF controls retained. Core, charting and the mathematical API target `net10.0-windows`; the independent ASP.NET Core SPA host and extracted portable engine target `net10.0`. The frontend uses the updated React, TypeScript and Vite stack.

The Framework 4.8.1 checkpoint (#139) and runtime port (#140) are merged. [MIGRATION-NET10.md](docs/MIGRATION-NET10.md) describes the current runtime, settings importer and remaining acceptance work; [MIGRATION-2026.md](docs/MIGRATION-2026.md) records the earlier Framework checkpoint. The [3.0 release checklist](docs/RELEASE-3.0.md) tracks the integrated candidate and distribution gates. No 3.0 release has been published.

NuGet versions are centralized in `Directory.Packages.props`, without per-project NuGet lockfiles. The single npm lockfile is retained for `npm ci`. See [dependency tradeoffs](docs/BUILD.md#dependency-policy-one-frontend-lockfile-not-twelve-nuget-graphs).

### Reduced Mono desktop

An explicit **`ComputatorMono=true`** build targets Framework `net48` for Mono using the same desktop projects, TSL compiler and mathematical sources. It restores the historical TextEditor, Unix 2D chart library and complex charts; WPF 3D, AvalonEdit and Scintilla are excluded. The ordinary Windows application and portable .NET 10 engine/CLI remain the default targets.

On a Linux host with the prerequisites in [MONO-DESKTOP.md](docs/MONO-DESKTOP.md):

```sh
./build.sh --target=Mono-Validate
mono Computator.NET/bin/mono/Release/net48/Computator.NET.exe
```

This is an experimental compatibility build, with separate restore/build outputs. See the guide for exact tested environments, runtime checks, macOS status and feature limitations. It is not included in the Windows installers or release packages.

### Web development

```sh
cd Computator.NET.WebClient
npm ci
npm run dev
# From the repository root, validate production assets/host and both browsers:
./build.sh --target=Web-Validate
```

Start the mathematical API separately **on Windows**:

```powershell
dotnet run --project Computator.NET.WebApi -- --urls http://localhost:60996
```

The Vite UI runs at `http://127.0.0.1:5173`; its compatibility API default is `http://localhost:60996/api`. For the protected local deployment, build with `VITE_API_BASE_URL=/api` and configure the server-side token and fixed loopback backend described in [WEB-E2E.md](docs/followups/WEB-E2E.md). Direct `dotnet publish` of the frontend host also delegates frontend compilation to Cake.

Web custom functions now support unsaved edits, explicit tab-session saves and UTF-8 `.tslf` import/export. JSON execution requests carry definitions separately for each request. The real browser suites cover calculation, scripts, numerical integration and chart images against the actual API. Symbolic calculation and server-side 3D rendering remain unfinished.

The frontend host runs on Linux/macOS, while the chart-backed mathematical API remains Windows-only. [The portable engine and CLI](docs/followups/PORTABLE-ENGINE.md) provide a first subset of TSL, arithmetic, constants and managed scripting; they do not yet contain the complete numerical/GSL catalog or a portable desktop UI.

### Custom functions and side-by-side testing

Definitions in the visible Custom Functions editor feed the real calculation and scripting presenters. Unsaved edits are used immediately; file commands and error highlighting use that same editor. The desktop regression suite tests the actual window composition with Scintilla, AvalonEdit and the legacy TextEditor option, rather than only evaluating a supplied string.

Back up real profiles and documents before trying a migration. For a disposable profile:

```powershell
$env:COMPUTATOR_DATA_DIRECTORY = Join-Path $env:TEMP 'Computator-Trial'
dotnet run --project Computator.NET
```

Cake validation creates its own temporary profile. A generated legacy fixture and smoke tests do not replace manual testing of real saved data, chart appearance, high-DPI layouts and complete editor interactions.

**Security:** TSL executes trusted code with the application's permissions. Keep both web hosts loopback-only and use the protected proxy for local deployment. A token, request limits and separate evaluators do not sandbox arbitrary scripts or create a public multi-user execution service.

## Releases and historical targets

Cake produces portable ZIPs, an unsigned MSIX preserving the existing Store identity, and MSI/Burn candidates through the migrated WixSharp/WiX authoring tool. MSI compilation has been validated after explicit owner acceptance of the WiX 7 terms. Signing, clean-machine installation/upgrade tests and Store certification remain release gates; building a package does not satisfy them. See [RELEASE-3.0.md](docs/RELEASE-3.0.md) and [historical tooling boundaries](docs/LEGACY-TOOLING.md).

A pinned GNU GSL 2.8 build is available as an opt-in candidate with provenance and ABI checks. The original embedded native payload remains the default until candidate acceptance. Existing [2.3.2 release downloads](https://github.com/PawelTroka/Computator.NET/releases/tag/v2.3.2) are unchanged.

## Working with Computator.NET

Use **Shift+6 (`^`)** or the Edit menu to write in an exponent.

![Writing in exponent](Graphics/Presentation/writing-in-exponent.gif)

To solve equations numerically, use **Numerical calculations → Function root** and choose an appropriate method. Graphical solutions can also be found by inspecting function intersections.

![Numerical root finding](Graphics/Presentation/solving%20equations%20using%20numerical%20calculations.gif)
![Graphical equation solving](Graphics/Presentation/solving%20equations%20graphically.gif)

## Contributing and license

Submit focused pull requests with regression tests and reviewed dependency changes. Report reproducible bugs in [Issues](https://github.com/PawelTroka/Computator.NET/issues); [TODO](TODO) contains additional ideas.

Computator.NET is distributed under [GNU GPL v3](LICENSE). Third-party dependencies retain their own licenses and distribution requirements.
