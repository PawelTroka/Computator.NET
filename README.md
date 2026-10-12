# ![Computator.NET](Graphics/Assets/Wide310x150Logo.png)

Computator.NET is open-source numerical software for exploring mathematics through calculations, charts and scripts.

![Quick presentation](Graphics/Presentation/quick_presentation.gif)

It includes real, complex and two-variable function charts and calculations; numerical integration, derivatives and root finding; elementary, special and matrix functions; and the original TROKA Scripting Language (TSL), custom functions and mathematical notation.

## Develop and build with Cake

The original Cake build pattern is retained and modernized. `build.cake` owns restore, build, tests, coverage, frontend, publication and portable packaging. Shell scripts and CI only invoke its tasks. See [BUILD.md](docs/BUILD.md) for prerequisites, target descriptions and dependency policy.

```powershell
./build.ps1 -Target Build
./build.ps1 -Target AllTests
# All Windows checks and checked desktop/API publication:
./build.ps1 -Target CI
# All Windows and browser checks, plus local versioned ZIP packages:
./build.ps1 -Target Publish -Configuration Release
```

Use the SDK from `global.json`, PowerShell 7, and Node/npm from `.nvmrc` and `Computator.NET.WebClient/package.json`. The wrapper restores the pinned Cake .NET tool; a global Cake install is unnecessary. `build.cmd -Target Build` and `dotnet tool restore` followed by `dotnet cake build.cake --target=Build` select the same graph.

### Migration branches

| Branch / PR | Desktop and mathematical API runtime |
| --- | --- |
| `modernize/2026-compatible-toolchain` / #139 | .NET Framework 4.8.1, built with the .NET 10 SDK and C# 14 |
| `modernize/net10-runtime` / #140, stacked on #139 | Actual .NET 10, targeting `net10.0-windows` |

The independent web-client host uses ASP.NET Core 10 on both branches. These are migration candidates, not installer-certified releases. Both branches use the same Cake commands; their runtime is declared in `build/ModernDesktop.props`. The [Framework migration guide](docs/MIGRATION-2026.md) describes #139; the .NET 10 branch additionally contains `docs/MIGRATION-NET10.md` for its settings importer and runtime-specific acceptance checks.

NuGet versions are centralized in `Directory.Packages.props`, without per-project NuGet lockfiles. The single npm lockfile is retained for `npm ci`. See [dependency tradeoffs](docs/BUILD.md#dependency-policy-one-frontend-lockfile-not-twelve-nuget-graphs).

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

The Vite UI runs at `http://127.0.0.1:5173`; its API default is `http://localhost:60996/api`. `.env.example` documents the build-time override. The frontend and its independent host can run on Linux/macOS, but the chart-backed mathematical API remains Windows-only. Direct `dotnet publish` of the frontend host also delegates frontend compilation to Cake.

### Custom functions and side-by-side testing

Definitions in the visible Custom Functions editor feed the real calculation and scripting presenters. Unsaved edits are used immediately; file commands and error highlighting use that same editor. The desktop regression suite tests the actual window composition with Scintilla, AvalonEdit and the legacy TextEditor option, rather than only evaluating a supplied string.

Back up real profiles and documents before trying a migration. For a disposable profile:

```powershell
$env:COMPUTATOR_DATA_DIRECTORY = Join-Path $env:TEMP 'Computator-Trial'
dotnet run --project Computator.NET
```

Cake validation creates its own temporary profile. A generated legacy fixture and smoke tests do not replace manual testing of real saved data, chart appearance, high-DPI layouts and complete editor interactions.

**Security:** TSL executes trusted code with the application's permissions. Keep the anonymous mathematical API private and loopback-only. CORS is not authentication or a sandbox.

## Releases and historical targets

The old WiX installer and UWP packaging are not certified for the new targets. Their Cake entry points fail explicitly rather than generating an incorrect package. [Historical tooling boundaries](docs/LEGACY-TOOLING.md) describe the remaining installer/platform work. Existing [2.3.2 release downloads](https://github.com/PawelTroka/Computator.NET/releases/tag/v2.3.2) are unchanged.

## Working with Computator.NET

Use **Shift+6 (`^`)** or the Edit menu to write in an exponent.

![Writing in exponent](Graphics/Presentation/writing-in-exponent.gif)

To solve equations numerically, use **Numerical calculations → Function root** and choose an appropriate method. Graphical solutions can also be found by inspecting function intersections.

![Numerical root finding](Graphics/Presentation/solving%20equations%20using%20numerical%20calculations.gif)
![Graphical equation solving](Graphics/Presentation/solving%20equations%20graphically.gif)

## Contributing and license

Submit focused pull requests with regression tests and reviewed dependency changes. Report reproducible bugs in [Issues](https://github.com/PawelTroka/Computator.NET/issues); [TODO](TODO) contains additional ideas.

Computator.NET is distributed under [GNU GPL v3](LICENSE). Third-party dependencies retain their own licenses and distribution requirements.
