# Historical targets and active build tooling

**Cake is active, not retired.** `build.cake`, `build.ps1`, `build.cmd` and `build.sh` are the current build entry points. GitHub Actions, AppVeyor and Travis select tasks from the same Cake graph. The test/result and published-process helpers under `build/` are leaf checks; there is no second PowerShell build orchestrator. See [BUILD.md](BUILD.md).

The active project sets in `.sln` and `.slnx` match. These historical paths remain outside that project set:

| Path | Explicit boundary |
| --- | --- |
| `Computator.NET/Computator.NET.net40.csproj` | Historical Windows XP launcher. Build a historical tag to reproduce that platform; it cannot reference current runtime libraries. |
| `Computator.NET.Desktop.UITests` | VS2017 Coded UI recording. Modern NUnit workflow tests supplement it but do not claim to reproduce the entire old interactive suite. |
| `Computator.NET.Installer` | Old WiX bootstrapper/payload logic. Requires runtime-specific prerequisite and upgrade/uninstall validation before a new installer release. |
| `build-uwp` | Historical UWP manifest/registry/test-signing data; not a supported modern runtime package. |
| `build-mono` | Historical Mono-specific build/deployment source, not an active Windows or web-host pipeline. |

The Cake target names `Build-Installer`, `Build-Uwp` and `Install-Linux` remain discoverable and fail explicitly before invoking unsupported tooling. They do not sign artifacts, install machine-wide packages or silently produce a package with the wrong prerequisites. `Package`/`Publish` create portable desktop/API/web ZIPs instead. This is not a claim that installer migration is complete.

The old `tools/packages.config` Cake 0.30 bootstrap is removed. `.config/dotnet-tools.json` selects the modern Cake runner. Per-project NuGet lockfiles are removed; the single frontend npm lockfile remains for `npm ci`.

Existing tagged releases and published historical binaries are untouched. No migration CI task automatically publishes a GitHub release or deploys the full-trust scripting API.
