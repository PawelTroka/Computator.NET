# ![Computator.NET](Graphics/Assets/Wide310x150Logo.png)

Computator.NET is open-source numerical software for exploring mathematics through calculations, charts and scripts.

![Quick presentation](Graphics/Presentation/quick_presentation.gif)

It includes real, complex and two-variable function charts and calculations; numerical integration, derivatives and root finding; elementary, special and matrix functions; and the original TROKA Scripting Language (TSL), custom functions and mathematical notation.

## Experimental .NET 10 runtime migration

This is the **stacked runtime migration branch**, targeting the compatibility work in PR #139. The desktop, core libraries and mathematical API target **`net10.0-windows`**; the independent web-client host targets **`net10.0`**. This is not a .NET Framework application built with a newer SDK.

The branch retains Windows Forms/WPF and the existing mathematical UI rather than replacing them with a new product. **Read [the .NET 10 migration guide](docs/MIGRATION-NET10.md) before using an existing profile.** CI artifacts are experimental portable candidates, not certified installers.

### Build and validate on Windows

Install the SDK in `global.json`, a matching .NET 10 Windows Desktop Runtime, and PowerShell 7. The API also needs the ASP.NET Core 10 runtime (included in an SDK installation). Use a current Visual Studio installation supporting this SDK and Windows desktop development, or the command line.

```powershell
dotnet restore Computator.NET.sln --locked-mode
dotnet build Computator.NET.sln -c Release --no-restore
pwsh ./build/Validate-Net10.ps1
```

The validation command uses temporary profiles, generates a legacy fixture, runs all four Windows suites, checks test discovery/skips, publishes both applications, and launches the real desktop and Kestrel API. Its legacy fixture writer uses Windows PowerShell/.NET Framework **only to generate test input**; production settings import uses .NET 10's data-only NRBF decoder.

Both `Computator.NET.sln` and `Computator.NET.Modern.slnx` contain the same twelve active projects. Specify the solution filename explicitly. Dependency versions and resolved content hashes are committed; use `-UpdateLocks` only for deliberate dependency updates that you will review and commit.

For a side-by-side run without touching your normal profile:

```powershell
$env:COMPUTATOR_DATA_DIRECTORY = Join-Path $env:TEMP 'Computator-NET10-Trial'
dotnet run --project Computator.NET
```

### Web development

Use the Node LTS version in `.nvmrc` and npm version in `Computator.NET.WebClient/package.json`.

```sh
cd Computator.NET.WebClient
npm ci
npm run dev
```

Start the mathematical API separately on Windows:

```powershell
dotnet run --project Computator.NET.WebApi -- --urls http://localhost:60996
```

The Vite UI is at `http://127.0.0.1:5173`. Its API default is `http://localhost:60996/api`; `.env.example` documents the build-time override. The frontend and its independent ASP.NET Core host can be built on Linux/macOS, but the chart-backed mathematical API remains Windows-only.

```sh
# From Computator.NET.WebClient:
npm run build
npx playwright install chromium firefox
npm test
dotnet publish -c Release -o ../artifacts/webclient
```

**Security:** TSL executes trusted code with the application's permissions. The anonymous mathematical API is not a sandbox and must not be exposed to the Internet. Keep it private and loopback-only; CORS is not authentication.

## Compatibility and release status

The .NET 10 migration replaces BinaryFormatter settings with versioned JSON, preserves original data during import, replaces PowerPacks color controls and retired menus, and ports charting/Scintilla/native loading. The original third editor still uses a Framework-targeted vendor binary; its warning is visible and its smoke test is not a vendor-support guarantee. Native GSL binaries are retained, not claimed to be new upstream builds. Windows ARM64, trimming, NativeAOT and single-file publication are not supported by this candidate.

See [MIGRATION-NET10.md](docs/MIGRATION-NET10.md) for automated coverage, manual acceptance, retained dependencies and rollback. [MIGRATION-2026.md](docs/MIGRATION-2026.md) and [VALIDATION-2026.md](docs/VALIDATION-2026.md) describe the **parent Framework 4.8.1 checkpoint**, not this branch's runtime. [LEGACY-TOOLING.md](docs/LEGACY-TOOLING.md) records historical build/installer exclusions.

The old WiX installer has not been migrated or certified for .NET 10. Do not ship new binaries using its old prerequisites. Tagged 2.3.2 packages remain unchanged: [historical release notes and downloads](https://github.com/PawelTroka/Computator.NET/releases/tag/v2.3.2).

## Working with Computator.NET

To write in an exponent, use **Shift+6 (`^`)** or the corresponding Edit menu command.

![Writing in exponent](Graphics/Presentation/writing-in-exponent.gif)

For numerical equation solving, use the **Numerical calculations** tab and **Function root** operation. Graphical solutions are also possible by inspecting function intersections.

![Numerical root finding](Graphics/Presentation/solving%20equations%20using%20numerical%20calculations.gif)
![Graphical equation solving](Graphics/Presentation/solving%20equations%20graphically.gif)

## Contributing and license

Submit focused pull requests with regression tests, and review every dependency lockfile change. Report reproducible bugs in [Issues](https://github.com/PawelTroka/Computator.NET/issues); the [TODO file](TODO) contains additional project ideas.

Computator.NET is distributed under [GNU GPL v3](LICENSE). Third-party dependencies retain their own licenses; review their notices before distributing a new build.
