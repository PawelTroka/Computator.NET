# Computator.NET documentation

[Project overview](../README.md) · [Developer setup](DEVELOPING.md) · [Build and test](BUILD.md) · [Contributing](../CONTRIBUTING.md)

Start with the task you want to complete. The entry-point guides describe the current source tree; dated integration reports and benchmark captures record what was verified at a particular revision.

## Start here

| Your task | Guide |
| --- | --- |
| Open the repository in Rider or Visual Studio, find projects, run/debug, or discover tests | **[Developer guide](DEVELOPING.md)** |
| Pick a build target, run validation, find output or update dependencies | **[Build and test](BUILD.md)** |
| Run calculations, scripts and numerical commands on Windows, Linux or macOS | **[Portable engine and CLI](followups/PORTABLE-ENGINE.md)** |
| Launch the web API and frontend from source | **[Web development](WEB-DEVELOPMENT.md)** |
| Build the complete local browser package | **[Local web package](LOCAL-WEB-PACKAGE.md)** |
| Make a focused contribution or report a reproducible bug | **[Contributing](../CONTRIBUTING.md)** |

## Language, mathematics and reference content

| Guide | What it covers |
| --- | --- |
| [Function powers](FUNCTION-POWERS.md) | Mathematical power notation, disambiguation and compiler behavior |
| [Shared portable scientific services](followups/PORTABLE-SCIENCE.md) | Shared matrix/numerical implementations and strict CLI input contracts |
| [Ordinary differential equations](followups/ORDINARY-DIFFERENTIAL-EQUATIONS.md) | Scalar and coupled initial-value problems with fixed-step RK4 |
| [Built-in function reference](FUNCTION-REFERENCE.md) | Authoring, generation, article identities and validation |
| [Reference equations and diagrams](REFERENCE-EQUATIONS.md) | Typesetting, figures and rendering contracts |
| [Numerical reference integration](NUMERICAL-REFERENCE-INTEGRATION.md) | Integration evidence and remaining reference scope |
| [Complex chart presentation](followups/COMPLEX-CHART-PRESENTATION.md) | Complex axes, ticks and rendering checks |
| [Numerical chart axes](followups/NUMERICAL-CHART-AXES.md) | Axis layout, labels and chart acceptance |
| [3D axis presentation](followups/THREE-DIMENSIONAL-AXIS-PRESENTATION.md) | Surface occlusion, labels, DPI and export evidence |

## Application behavior and architecture

| Guide | What it covers |
| --- | --- |
| [Project map and source sharing](DEVELOPING.md#project-map) | Application layers, source ownership, test projects and tooling |
| [Shared execution policy](SHARED-EXECUTION-POLICY.md) | Input and output limits across web execution routes |
| [Execution workers](followups/EXECUTION-WORKERS.md) | Cancellation, process ownership and recovery boundaries |
| [Generated-program lifetime](PROGRAM-LIFETIME.md) | Collectible code and remaining dynamic-binder retention |
| [Desktop document safety](followups/EDITOR-DOCUMENT-SAFETY.md) | Editor document identity, persistence and interactions |
| [Web custom functions](followups/WEB-CUSTOM-FUNCTIONS.md) | Editing, session saves, import/export and request-local definitions |
| [Web interface](WEB-UI.md) | Web shell and editor design decisions |
| [Real browser-to-API tests](followups/WEB-E2E.md) | Protected local proxy and actual mathematical execution |

## Platforms and native libraries

| Guide | What it covers |
| --- | --- |
| [Reduced Mono desktop](MONO-DESKTOP.md) | Current opt-in build, prerequisites, tested environments and limits |
| [Mono restoration background](followups/MONO-DESKTOP-RESTORATION.md) | Rationale and boundaries for the compatibility work |
| [Native GSL build](followups/GSL.md) | Source pins, provenance and native/managed ABI checks |
| [GSL adoption](followups/GSL-ADOPTION.md) | Packaged GSL 2.8 and development fallback behavior |
| [Historical tooling boundaries](LEGACY-TOOLING.md) | Retired projects and compatibility sources that remain visible |
| [.NET 10 migration](MIGRATION-NET10.md) | Runtime migration and settings-import design |

## Performance

| Guide | What it covers |
| --- | --- |
| [Measurement and comparison guide](../build/performance/README.md) | Running benchmarks, historical baselines and interpreting results |
| [Issue #191 investigation](performance/issue-191/README.md) | Recorded modern/historical measurements and reproducible evidence |
| [Compilation diagnostic experiments](../build/performance/diagnostics/translation/README.md) | Detailed translation experiments and their scope |
| [Precompiled built-ins](performance/PRECOMPILED-BUILTINS.md) | Avoiding repeated built-in compilation |
| [Direct numerical compiler](performance/DIRECT-NUMERIC-COMPILER.md) | Direct compilation path and fallback behavior |
| [ReadyToRun publication](performance/READY-TO-RUN.md) | Startup measurements and publication choices |

Benchmark reports are tied to their measured revision, machine and scenario. Use the reproduction commands before generalizing a recorded result to current code.

## Distribution and release acceptance

| Guide | What it covers |
| --- | --- |
| [Release preparation](RELEASE-3.0.md) | Candidate status, reviewed evidence and remaining gates |
| [Release identity](RELEASE-IDENTITY.md) | Version mapping and commit-bound evidence |
| [MSI/Burn installer](followups/INSTALLER.md) | Authoring, build inputs and lifecycle validation |
| [Desktop MSIX](followups/MSIX.md) | Package identity, payload validation and Store boundaries |
| [Signed MSIX acceptance](followups/SIGNED-MSIX-ACCEPTANCE.md) | Actual package activation and upgrade trials |
| [Clean-machine acceptance](CLEAN-MACHINE-ACCEPTANCE.md) | Offline Windows Sandbox workflow |
| [Historical upgrade acceptance](HISTORICAL-UPGRADE-ACCEPTANCE.md) | Upgrade, repair, uninstall and rollback checks |
| [Real-profile trials](followups/REAL-PROFILE-ACCEPTANCE.md) | Copied settings/documents and migration acceptance |
| [AppVeyor](followups/APPVEYOR.md) / [Travis](followups/TRAVIS.md) | Provider-specific setup, mirrors and evidence boundaries |

## Historical records

These documents retain the versions and results from their original checkpoints. Use [DEVELOPING.md](DEVELOPING.md) and [BUILD.md](BUILD.md) for current setup and commands.

- [Framework migration checkpoint](MIGRATION-2026.md) and [2026 validation record](VALIDATION-2026.md).
- [Historical-upgrade evidence](evidence/historical-upgrade-2026-10-16/README.md) and [rollback diagnostic](evidence/historical-upgrade-2026-10-16/ROLLBACK-DIAGNOSTIC.md).
- Issue #191 measurement reports: [session 1](performance/issue-191/windows-session-1-report.md) and [session 2](performance/issue-191/windows-session-2-report.md).
- Original Polish engineering thesis: [PDF](EngineeringThesis-pl.pdf), [Word source](EngineeringThesis-pl.docx), and [presentation](EngineeringThesisPresentation-pl.pptx).

When adding a guide, link it here and expose it in the canonical solution. Keep instructions in the relevant current guide and evidence in a clearly dated report, so readers can tell what to run today.
