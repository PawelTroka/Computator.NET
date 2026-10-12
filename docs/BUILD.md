# Building Computator.NET with Cake

`build.cake` is the single build orchestrator for both migration branches. It uses the original `Task(...).IsDependentOn(...).Does(...)` pattern and retains the familiar target names. The runner is Cake.Tool 6.3.0 in `.config/dotnet-tools.json`; obsolete Cake 0.30 and unpinned addins are no longer downloaded. The small shell/PowerShell entry points only restore that tool and select a Cake target.

## Commands

Install the .NET SDK selected by `global.json`, PowerShell 7 on Windows, and the Node/npm versions in `.nvmrc` and the web client's `package.json` for frontend tasks. Windows must also have the runtime appropriate to the branch: Framework 4.8.1 in #139, or .NET 10 Desktop and ASP.NET Core runtimes in #140. The SDK includes the modern development runtimes.

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
| `Web-Build` / `Web-Publish` / `Web-Validate` | npm install/audit, strict TypeScript and Vite; production .NET host; real-host Chromium/Firefox tests. |
| `Package` | Build and publish desktop/API/web; create versioned ZIPs in `publish/`. Does not run every test. |
| `Validate` / `Publish` | All Windows and browser tests, checked publications and ZIP packaging. Publish means local artifacts, never a public release or deployment. |
| `Calculate-Coverage` | All Windows tests with the test SDK's coverage collector; require Cobertura output in `TestResults/`. |

`Default` remains `UnitTests`. Individual suite targets (`CoreTests`, `DesktopTests`, `CoreIntegrationTests`, `ApiTests`) are also available. `--coverage=true` enables collection for a selected CI suite. `Upload-Coverage` preserves the old target name but uses AppVeyor artifact publication; GitHub uploads reports through its artifact step. External Coveralls/Codecov publication is not silently performed with old encrypted credentials.

CI setup and artifact-upload actions remain provider-specific; all compile/test/publish/package work belongs to Cake. GitHub runs the same tasks as local builds. AppVeyor uses the Windows `Publish` graph; Travis uses the supported cross-platform `Web-Validate` graph. Changing those YAML files does not activate an external provider or establish that it has run; check the provider's actual job before requiring its status.

## Dependency policy: one frontend lockfile, not twelve NuGet graphs

NuGet direct versions are maintained once in `Directory.Packages.props`. Per-project `packages.lock.json` files and locked-mode restore requirements are removed from both branches. Normal restores do not regenerate those files. Update central versions and run Cake; there is no lock-refresh workflow or write-enabled CI job.

A central exact direct version is **not** a frozen transitive dependency graph. This choice deliberately favors less repository churn; transitive dependency resolution can change. NuGet auditing remains enabled for the full restored graph, and CI treats advisory/audit failures as errors rather than pretending lockfiles were a security requirement.

The single `Computator.NET.WebClient/package-lock.json` remains because `npm ci` requires it and verifies consistency with `package.json`. It covers the frontend dependency tree, not one duplicate graph per project. Use `npm install` intentionally when updating frontend dependencies, review that one lockfile, then rerun Cake. SDK, Node and Cake runner versions remain explicitly selected.

## Safety and behavior checks

Both branches use isolated temporary test profiles selected through `COMPUTATOR_DATA_DIRECTORY`. .NET 10 Cake setup additionally creates a known legacy settings fixture with the Framework serializer; the production importer is still .NET 10's data-only decoder. Setup/teardown restore caller environment variables and only clean directories created for this invocation.

The result guard rejects failed/incomplete runs, missing reports, test-discovery loss, duplicate identities and undocumented skips. Three historical API skips are allowed by exact identity. Custom-function regressions compose the real desktop: visible editor content is used for calculation and scripting, unsaved changes take effect, and save/reopen and error-highlighter bindings are checked for all editor choices. Compilation of a manually supplied custom-function string is not enough evidence.

On .NET 10, Cake also launches the published desktop and Kestrel API through focused smoke-test helpers. These helpers and the TRX checker are leaf checks, not competing build orchestrators. Old `Validate-Migration.ps1`/`Validate-Net10.ps1` callers forward to Cake.

## Installer and historical platforms

The old `Build-Installer`, `Build-Uwp` and `Install-Linux` target names remain discoverable but deliberately fail with an actionable message. They must not generate an installer using old prerequisites, sign UWP with a historical test key, or run machine-wide Mono installation as a side effect. The installer itself still needs runtime/payload/upgrade/uninstall work; adding a Cake task does not certify it. Portable packages are produced by `Package`/`Publish` while that work remains outstanding. See `LEGACY-TOOLING.md` and the appropriate runtime migration guide.

## Primary references

- [Cake tasks and dependencies](https://cakebuild.net/docs/writing-builds/tasks/)
- [Cake .NET tool](https://cakebuild.net/docs/running-builds/runners/dotnet-tool)
- [npm ci and its lockfile requirement](https://docs.npmjs.com/cli/v11/commands/npm-ci/)
