# Building Computator.NET with Cake

[Overview](../README.md) · [Documentation](README.md) · [Rider and development](DEVELOPING.md) · [Contributing](../CONTRIBUTING.md)

`build.cake` is the single build orchestrator, including the modules in `build/followups/`. It uses the original `Task(...).IsDependentOn(...).Does(...)` pattern and retains the familiar target names. The runner is pinned in `.config/dotnet-tools.json`; the small shell/PowerShell entry points restore that tool and select a Cake target.

Need a command menu before setup? `./build.ps1 -Help` or `./build.sh --help` prints the same offline guide without requiring the SDK, Cake, Node or a restore. Once the SDK is available, `./build.ps1 --description` or `./build.sh --description` lists the actual registered tasks and their descriptions.

## Pick your next command

Run commands from the repository root. For a first setup, use [prerequisites by task](DEVELOPING.md#prerequisites); for a single test or debugger session, use the [developer test workflow](DEVELOPING.md#test-workflow).

| I need to… | Target | Platform |
| --- | --- | --- |
| Compile the complete solution | `Build` | Windows |
| Check solution inventory and evaluated project items without compiling | `Solution-Check` | Windows/Linux/macOS; SDK, Git and PowerShell required |
| Run shared engine tests | `Portable-Test` | Windows/Linux/macOS |
| Check portable tests and published CLI behavior | `Portable-Validate` | Windows/Linux/macOS |
| Run one Windows suite | `CoreTests`, `DesktopTests`, `CoreIntegrationTests` or `ApiTests` | Windows |
| Run all four Windows suites | `AllTests` | Windows |
| Compile TypeScript and production frontend assets | `Web-Build` | Windows/Linux/macOS with Node/npm |
| Check the frontend/host with mocked-API browser tests | `Web-Validate` | Windows/Linux/macOS with browser prerequisites |
| Check the actual source/IDE web startup | `Web-IDE-Validate` | Windows |
| Exercise the real mathematical API from browsers | `Web-E2E` or `Web-CustomFunctions` | Windows |

PowerShell: `./build.ps1 -Target <target> -Configuration Debug`. Bash: `./build.sh --target=<target> --configuration=Debug`. Omit the configuration to use Cake's **Release** default. The default target is `UnitTests`, so a bare wrapper invocation selects Windows Core and desktop tests; it is not a portable auto-detection mode.

`CI` additionally publishes and smoke-checks the selected desktop/API outputs. `Publish` validates and creates local ZIP packages. These targets can build native GSL and are intentionally broader than an ordinary code-edit/test cycle. None of them uploads a GitHub release.

## Commands

Install the .NET SDK selected by `global.json`, PowerShell 7, and the Node/npm versions in `.nvmrc` and the web client's `package.json` for frontend tasks. The default desktop/API graph requires Windows and .NET 10 Desktop/ASP.NET Core runtimes; the SDK supplies the development runtimes. Framework is also used by the legacy test-fixture writer, isolated installer authoring tool and explicit reduced [Mono desktop](MONO-DESKTOP.md) target. Windows publication also builds its pinned GSL runtime and requires Python 3.11 or newer, Git and Visual Studio C++ build tools. The pinned vcpkg checkout supplies verified CMake/CTest tools within the build directory. Windows SDK packaging tools are required for MSIX. Ordinary solution builds do not build native GSL.

```powershell
# Windows; the original invocation pattern remains supported.
./build.ps1 -Target Build
./build.ps1 -Target UnitTests
./build.ps1 -Target AllTests
./build.ps1 -Target CI
./build.ps1 -Target CI -CakeArguments '--suite=Computator.NET.Desktop.Tests'
./build.ps1 -Target Publish -Configuration Release
```

`build.cmd -Target Build` works as well. Direct invocation uses the same task graph:

```sh
dotnet tool restore
dotnet cake build.cake --target=Build
# Cross-platform frontend, production host and browser tests:
./build.sh --target=Web-Validate
# Discover targets without running them:
dotnet cake build.cake --description
# Inspect the task dependency graph:
dotnet cake build.cake --tree
# Inspect the selected target's execution plan without preparing/running it:
dotnet cake build.cake --target=Portable-Validate --dryrun
```

`--tree` displays the full registered dependency graph. `--dryrun` plans the selected target while skipping repository setup, output/profile preparation and task execution. Wrapper discovery commands still restore the pinned Cake tool; only wrapper `-Help`/`--help` is independent of that toolchain. The equivalent wrapper dry runs are `./build.ps1 -Target Portable-Validate --dryrun` and `./build.sh --target=Portable-Validate --dryrun`.

### Frontend tools when launching from an IDE

The development WebClient automatically invokes Cake `Web-Build` when its frontend assets are missing or stale. Cake checks the Node and npm versions inherited from the IDE before any frontend command. If they differ from `.nvmrc` and `packageManager`, it uses `npm exec` to run the exact pinned versions from npm's local tool cache. This also applies to frontend builds, publishing and browser tests started directly through Cake. The first such build needs registry access; later builds reuse the cache. Global Node/npm installations and the application lockfile are not changed, and `engine-strict=true`, `npm ci` and the audit remain enabled.

Automatic selection requires an installed Node from the repository's selected major version or newer (currently Node 24+) and npm 7 or newer. Earlier Node majors may not be able to install the pinned npm before the new Node runs, so they receive an actionable prerequisite error. If Node/npm are missing, too old, or the machine cannot access the registry, install the versions recorded in `.nvmrc` and `Computator.NET.WebClient/package.json`, restart the IDE so it inherits the updated `PATH`, and retry. Direct `npm ci` / `npm run dev` commands still use the tools on your shell's `PATH`; the automatic selection belongs to Cake. For example, Node 24.11.1 with npm 11.7.0 can start the source WebClient through Cake without globally upgrading either tool.

## Cake targets

| Target | Dependencies and result |
| --- | --- |
| `Clean`, `Restore`, `Build` | Clean generated output only; audited restore; Release/Debug solution compilation. Restore does not unexpectedly delete the previous build. |
| `UnitTests` / `IntegrationTests` / `AllTests` | Core + real desktop tests / numerical + API tests / all four Windows suites. |
| `WebTests` | Historical target name for mathematical API tests. `Web-Tests` is the separate browser suite. |
| `CI` | Selected Windows suites, result guards and desktop/API publication checks. `--suite=All` is the default. |
| `Web-Build` / `Web-Publish` / `Web-Validate` | npm install/audit, strict TypeScript and Vite; production .NET host; focused Chromium/Firefox tests with the mathematical API mocked. |
| `Web-E2E` / `Web-CustomFunctions` | Windows production hosts, protected local proxy and real mathematical API; Chromium/Firefox execution and custom-function workflows. Run sequentially: both use ports 4174 and 60997. |
| `Package` | Build and publish desktop/API/web; create versioned ZIPs in `publish/`. Does not run every test. |
| `Validate` / `Publish` | All four Windows suites, focused browser tests, checked publications and ZIP packaging. Follow-up validation targets below remain separate gates. Publish creates local artifacts. |
| `Calculate-Coverage` | Selected complete Windows suite(s), all by default, with the test SDK's coverage collector; require Cobertura output in `TestResults/`. Honors `--suite`. |
| `Portable-Validate` | Build the portable project entry points, validate the extracted engine and launch the published CLI. All supported projects remain in the single root SLNX. See [portable scope](followups/PORTABLE-ENGINE.md). |
| `Portable-Test` | Restore/build the portable projects and run engine tests with a TRX result guard, without publishing the CLI. |
| `Solution-Check` | Check linked developer files, project inventory, inventory guards and evaluated IDE items, then enumerate the SLNX. No project restore/build or test-profile setup. |
| `Solution-Validate` | Run `Solution-Check` and build the complete supported Windows solution. |
| `Web-IDE-Validate` | Build and start the actual Windows source projects; recreate missing/stale frontend assets and check real calculations. |
| `Local-Web-Validate` / `Local-Web-Package` | Build, unpack and test the self-contained protected local web application on Windows. See [local package](LOCAL-WEB-PACKAGE.md). |
| `Mono-Build` / `Mono-Validate` | Explicit `net48` compatibility graph using the current shared sources; real Mono startup and calculations/editor/chart acceptance. Requires the Linux GUI/native prerequisites in [MONO-DESKTOP.md](MONO-DESKTOP.md). Neither changes the default solution/package targets. |
| `Native-GSL-Validate` | Build the pinned native candidate and ABI probes; on Windows, run numerical and desktop suites with the candidate selected. See [GSL](followups/GSL.md). |
| `Installer-Source` / `Build-Installer` | Stage the self-contained desktop, validate WiX source; compile MSI/Burn with explicit WiX 7 terms acceptance. See [installer details](followups/INSTALLER.md). |
| `MSIX-Validate` | Stage the same self-contained desktop, validate/package unsigned MSIX and verify unpacked payload hashes. `Build-MSIX` and historical `Build-Uwp` select this packaging path. See [MSIX](followups/MSIX.md). |
| `AppVeyor-Mirror` / `Travis-Mirror` | Validate provider helpers and their assigned local/GitHub graph. These do not establish native external-provider execution. |

`Default` remains `UnitTests`. Individual suite targets (`CoreTests`, `DesktopTests`, `CoreIntegrationTests`, `ApiTests`) are also available. `--coverage=true` enables collection for a selected CI suite. `Upload-Coverage` preserves the old target name but uses AppVeyor artifact publication; GitHub uploads reports through its artifact step. External Coveralls/Codecov publication is not silently performed with old encrypted credentials.

CI setup and artifact-upload actions remain provider-specific; all compile/test/publish/package work belongs to Cake. GitHub runs the base and follow-up workflows on every pull request and push to `master` to catch interactions between migrations. The installer workflow validates source on ordinary PRs; MSI/Burn compilation remains an explicitly approved dispatch. Check the exact commit's results rather than assuming that configured jobs have passed.

`AppVeyor-Validate` runs the Windows `Publish` graph; `Travis-Validate` runs the cross-platform `Web-Validate` graph. Their preflight/completion records require genuine provider metadata. Actual provider runs, account connections and final-commit evidence remain separate from the GitHub mirrors. See [AppVeyor](followups/APPVEYOR.md) and [Travis](followups/TRAVIS.md).

## Outputs and diagnostics

| Output | Where to look |
| --- | --- |
| Full solution build log | `artifacts/build.binlog`; include the first failing target/error when reporting a build problem |
| Windows suite reports and coverage | `TestResults/<test-project>.trx` and `TestResults/**/*.cobertura.xml` when coverage is requested |
| Portable engine test report | `TestResults/portable.trx` |
| Published portable CLI | `artifacts/portable/computator.dll` |
| Component publications | `artifacts/desktop`, `artifacts/api`, `artifacts/webclient` |
| Frontend production assets | `Computator.NET.WebClient/dist` |
| Browser reports and retained failure evidence | `Computator.NET.WebClient/test-results` and `Computator.NET.WebClient/playwright-report`; exact subdirectories are defined by the selected `playwright.*.config.ts` |
| ZIP packages | `publish/`; the complete local-web candidate is `Computator.NET.LocalWeb.win-x64.zip` |
| Release evidence | `artifacts/release-evidence`, produced by the explicit release-verification workflow |

For a verbose build, use `./build.ps1 -Target Build -Verbosity Diagnostic` or `./build.sh --target=Build --verbosity=diagnostic`. Keep the exact command, commit and tool versions alongside a report. Re-running a failed target may replace its generated reports, so preserve useful diagnostics before the next attempt.

Cake creates/restores temporary test-profile variables for its own invocation. A separate Rider or `dotnet test` process does not inherit that setup; the focused settings-migration fixture needs the Cake-generated legacy input. See [test workflow](DEVELOPING.md#test-workflow).

## Dependency policy: one frontend lockfile, not twelve NuGet graphs

Application NuGet versions are maintained in `Directory.Packages.props`; isolated build-tool projects declare their own explicit pins. Per-project `packages.lock.json` files and locked-mode restore requirements are removed. Normal restores do not regenerate those files. Update the relevant versions and run Cake; there is no lock-refresh workflow or write-enabled CI job.

A central exact direct version is **not** a frozen transitive dependency graph. This choice deliberately favors less repository churn; transitive dependency resolution can change. NuGet auditing remains enabled for the full restored graph, and CI treats advisory/audit failures as errors rather than pretending lockfiles were a security requirement.

The single `Computator.NET.WebClient/package-lock.json` remains because `npm ci` requires it and verifies consistency with `package.json`. It covers the frontend dependency tree, not one duplicate graph per project. Use `npm install` intentionally when updating frontend dependencies, review that one lockfile, then rerun Cake. SDK, Node and Cake runner versions remain explicitly selected.

## Safety and behavior checks

Cake uses isolated temporary test profiles selected through `COMPUTATOR_DATA_DIRECTORY` and creates a known legacy settings fixture with the Framework serializer. The production importer is .NET 10's data-only decoder. Setup/teardown restore caller environment variables and only clean directories created for this invocation.

The result guard rejects failed/incomplete runs, missing reports, test-discovery loss, duplicate identities and undocumented skips. Any remaining historical skips must match the exact allowlist in `build/Assert-SuiteResults.ps1`; they are not a blanket exception. Custom-function regressions compose the real desktop: visible editor content is used for calculation and scripting, unsaved changes take effect, and save/reopen and error-highlighter bindings are checked for all editor choices.

On .NET 10, Cake also launches the published desktop and Kestrel API through focused smoke-test helpers. These helpers and the TRX checker are leaf checks, not competing build orchestrators. Old `Validate-Migration.ps1`/`Validate-Net10.ps1` callers forward to Cake.

## Installer and historical platforms

`Build-Installer` now compiles MSI/Burn candidates after explicit WiX 7 terms acceptance. `Build-Uwp` forwards to full-trust desktop MSIX packaging. Both use the shared self-contained win-x64 distribution payload; neither signs or publishes a release. Default package versions remain experimental candidates until a release version is deliberately selected.

`Installer-Validate` requires an elevated disposable Windows machine. `MSIX-Lifecycle` requires a disposable account/machine and an already trusted signed package. Both require `COMPUTATOR_DISPOSABLE_INSTALL_TEST=1`; do not run them on a normal development machine. Compilation and package/hash validation do not prove installation, historical upgrade, repair or uninstall. `Install-Linux` still fails explicitly: the opt-in Mono build does not install files machine-wide. See [RELEASE-3.0.md](RELEASE-3.0.md) and [LEGACY-TOOLING.md](LEGACY-TOOLING.md).

## Primary references

- [Cake tasks and dependencies](https://cakebuild.net/docs/writing-builds/tasks/)
- [Cake .NET tool](https://cakebuild.net/docs/running-builds/runners/dotnet-tool)
- [npm ci and its lockfile requirement](https://docs.npmjs.com/cli/v11/commands/npm-ci/)
