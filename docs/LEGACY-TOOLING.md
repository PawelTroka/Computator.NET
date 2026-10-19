# Historical targets and active build tooling

**Cake is active, not retired.** `build.cake`, `build.ps1`, `build.cmd` and `build.sh` are the current build entry points. GitHub Actions, AppVeyor and Travis select tasks from the same Cake graph. The test/result and published-process helpers under `build/` are leaf checks; there is no second PowerShell build orchestrator. See [BUILD.md](BUILD.md).

The active Windows project sets in `.sln` and `.slnx` match. `Computator.NET.Portable.slnx` builds the extracted engine/CLI separately. The following paths retain explicit boundaries:

| Path | Explicit boundary |
| --- | --- |
| `Computator.NET/Computator.NET.net40.csproj` | Historical Windows XP launcher. Build a historical tag to reproduce that platform; it cannot reference current runtime libraries. |
| `Computator.NET.Desktop.UITests` | VS2017 Coded UI recording. Modern NUnit workflow tests supplement it but do not claim to reproduce the entire old interactive suite. |
| `Computator.NET.Installer` | Active, isolated Framework-based WixSharp authoring tool for a self-contained .NET 10 product. Old helper sources are excluded; MSI/Burn compilation works. Clean-machine lifecycle and historical upgrade/signing acceptance remain. |
| `build-uwp` | Historical Desktop Bridge manifest/registry/test-signing data retained as the identity baseline. Current packaging lives in `build/msix`; no historical private key is used. |
| `build-mono` | Historical Mono-specific build/deployment source, not an active Windows or web-host pipeline. |
| `build/mono` and `Mono-Build` / `Mono-Validate` | Active, opt-in reduced Framework Mono desktop and runtime probes. See [MONO-DESKTOP.md](MONO-DESKTOP.md). This builds current shared sources; it does not revive the old install scripts. |

`Build-Installer` now builds MSI and Burn after explicit WiX 7 terms acceptance. `Build-Uwp` forwards to `Build-MSIX`, packaging the existing full-trust WinForms/WPF application with its original Store identity. `MSIX-Validate` performs Windows SDK validation and payload hash checks, producing an unsigned candidate. These targets share a self-contained win-x64 payload and do not sign or publish it. `Package`/`Publish` continue to create portable desktop/API/web ZIPs.

Only `Install-Linux` remains deliberately disabled: no machine-wide Mono installation is provided. The reduced Mono target is built separately with `ComputatorMono=true`; it excludes WPF 3D and the advanced Windows editors. The portable engine/CLI and independent web host continue to use modern .NET on supported non-Windows runners. Reviewed GSL 2.8 is staged in normal Windows packages; development outputs without it retain the embedded legacy fallback. The Mono build instead uses the host's GSL shared libraries. See [native acceptance](followups/GSL-ADOPTION.md).

Installer compilation and unsigned MSIX validation are recorded separately from installation certification. The guarded `Installer-Validate` and `MSIX-Lifecycle` harnesses require disposable machines; owner signing, old-product upgrade/rollback and Store activation tests remain release work. See [RELEASE-3.0.md](RELEASE-3.0.md).

The old `tools/packages.config` Cake 0.30 bootstrap is removed. `.config/dotnet-tools.json` selects the modern Cake runner. Per-project NuGet lockfiles are removed; the single frontend npm lockfile remains for `npm ci`.

Existing tagged releases and published historical binaries are untouched. No migration CI task automatically publishes a GitHub release or deploys the full-trust scripting API.
