# Historical build paths

The active 2026 project set is identical in `Computator.NET.sln` and `Computator.NET.Modern.slnx`. It contains the desktop, core libraries, both web hosts and modern NUnit test projects.

The following source is retained for historical reference, **not included in the active build**:

| Path | Status and reason |
| --- | --- |
| `Computator.NET/Computator.NET.net40.csproj` | Windows XP / .NET 4.0 launcher. It cannot consume the upgraded net481 libraries. Build an old release tag to reproduce the old platform, not this project against current libraries. |
| `Computator.NET.Desktop.UITests` | Visual Studio 2017 Enterprise Coded UI test and generated UI map. The new NUnit startup/resource suite is supplementary coverage, not a claim that the old end-to-end interaction recording has been replaced. |
| `Computator.NET.Installer` | Historical WiX bootstrapper with 4.0/4.6.1 prerequisites and old output paths. Its packages and deployment logic are not migrated or certified by this PR. Do not use it for a 4.8.1 release. |
| `build.cake`, root historical build scripts, `build-mono`, `build-uwp`, `appveyor.yml`, `.travis.yml` | Historical release/Mono/UWP/VS2017 automation. The current verified entry point is `build/Validate-Migration.ps1` plus the GitHub Actions workflows. Old external CI jobs should be retired or explicitly rebuilt around the new entry point before enabling them. |

Excluding these paths makes the normal developer solution coherent; it does not imply that the legacy targets, packages or operating systems have been updated. Their retirement and the remaining installer/native-runtime work are explicit migration scope decisions requiring review.

Existing tagged releases and published historical packages are not changed. The new CI produces a portable migration candidate, never an installer or a release upload.
