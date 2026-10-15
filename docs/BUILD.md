# Building Computator.NET with Cake

`build.cake` is the single build orchestrator, including the modules in `build/followups/`. It uses the original `Task(...).IsDependentOn(...).Does(...)` pattern and retains the familiar target names. The runner is pinned in `.config/dotnet-tools.json`; the small shell/PowerShell entry points restore that tool and select a Cake target.

## Commands

Install the .NET SDK selected by `global.json`, PowerShell 7, and the Node/npm versions in `.nvmrc` and the web client's `package.json` for frontend tasks. The desktop/API graph requires Windows and .NET 10 Desktop/ASP.NET Core runtimes; the SDK supplies the development runtimes. Framework is used only by the legacy test-fixture writer and isolated installer authoring tool. Windows publication also builds its pinned GSL runtime and requires Python 3.11 or newer, Git and Visual Studio C++ build tools. The pinned vcpkg checkout supplies verified CMake/CTest tools within the build directory. Windows SDK packaging tools are required for MSIX. Ordinary solution builds do not build native GSL.

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
dotnet cake build.cake --showdescription
```

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
| `Calculate-Coverage` | All Windows tests with the test SDK's coverage collector; require Cobertura output in `TestResults/`. |
| `Portable-Validate` | Build the portable project entry points, validate the extracted engine and launch the published CLI. All supported projects remain in the single root SLNX. See [portable scope](followups/PORTABLE-ENGINE.md). |
| `Native-GSL-Validate` | Build the pinned native candidate and ABI probes; on Windows, run numerical and desktop suites with the candidate selected. See [GSL](followups/GSL.md). |
| `Installer-Source` / `Build-Installer` | Stage the self-contained desktop, validate WiX source; compile MSI/Burn with explicit WiX 7 terms acceptance. See [installer details](followups/INSTALLER.md). |
| `MSIX-Validate` | Stage the same self-contained desktop, validate/package unsigned MSIX and verify unpacked payload hashes. `Build-MSIX` and historical `Build-Uwp` select this packaging path. See [MSIX](followups/MSIX.md). |
| `AppVeyor-Mirror` / `Travis-Mirror` | Validate provider helpers and their assigned local/GitHub graph. These do not establish native external-provider execution. |

`Default` remains `UnitTests`. Individual suite targets (`CoreTests`, `DesktopTests`, `CoreIntegrationTests`, `ApiTests`) are also available. `--coverage=true` enables collection for a selected CI suite. `Upload-Coverage` preserves the old target name but uses AppVeyor artifact publication; GitHub uploads reports through its artifact step. External Coveralls/Codecov publication is not silently performed with old encrypted credentials.

CI setup and artifact-upload actions remain provider-specific; all compile/test/publish/package work belongs to Cake. GitHub runs the base and follow-up workflows on every pull request and push to `master` to catch interactions between migrations. The installer workflow validates source on ordinary PRs; MSI/Burn compilation remains an explicitly approved dispatch. Check the exact commit's results rather than assuming that configured jobs have passed.

`AppVeyor-Validate` runs the Windows `Publish` graph; `Travis-Validate` runs the cross-platform `Web-Validate` graph. Their preflight/completion records require genuine provider metadata. Actual provider runs, account connections and final-commit evidence remain separate from the GitHub mirrors. See [AppVeyor](followups/APPVEYOR.md) and [Travis](followups/TRAVIS.md).

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

`Installer-Validate` requires an elevated disposable Windows machine. `MSIX-Lifecycle` requires a disposable account/machine and an already trusted signed package. Both require `COMPUTATOR_DISPOSABLE_INSTALL_TEST=1`; do not run them on a normal development machine. Compilation and package/hash validation do not prove installation, historical upgrade, repair or uninstall. `Install-Linux` still fails explicitly because machine-wide Mono installation is retired. See [RELEASE-3.0.md](RELEASE-3.0.md) and [LEGACY-TOOLING.md](LEGACY-TOOLING.md).

## Primary references

- [Cake tasks and dependencies](https://cakebuild.net/docs/writing-builds/tasks/)
- [Cake .NET tool](https://cakebuild.net/docs/running-builds/runners/dotnet-tool)
- [npm ci and its lockfile requirement](https://docs.npmjs.com/cli/v11/commands/npm-ci/)
