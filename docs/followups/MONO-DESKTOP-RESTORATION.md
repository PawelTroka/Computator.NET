# Mono desktop compatibility — issue #209

This branch remains a **draft**, with an opt-in Framework target implemented alongside the .NET 10 Windows application. Build instructions, the shared-source dependency boundaries, runtime limitations and acceptance evidence are maintained in [Reduced Mono desktop](../MONO-DESKTOP.md).

The historical build used a Framework target, `__MonoCS__`, an alternative 2D chart library and a 3D placeholder. PRs #139 and #140 removed those build branches and moved the Windows application to .NET 10.

## Scope and non-regression requirements

- Preserve the default `net10.0-windows` build, Windows charting and existing portable `net10.0` engine.
- Add a separate opt-in Framework/Mono target, without claiming that a `net10.0-windows` binary executes under Framework Mono.
- Reuse shared TSL, matrix, integration, derivative and root source. Resolve runtime differences for `AssemblyLoadContext`, `NativeLibrary`, startup and settings rather than duplicating mathematical algorithms.
- Restore a Mono-specific 2D chart implementation and non-WPF 3D placeholder. Preserve complex-chart functionality; do not silently omit it.
- Keep scripting, custom functions, numerical method selection, document load/save and GSL functional.
- Test actual Linux Mono WinForms startup, calculation, scripting, 2D and complex chart rendering, native GSL and shutdown. Validate macOS GUI separately; a passing macOS engine test does not prove a working WinForms application.
- Keep the PR draft until these gates pass and the owner reviews the differences.

## Implementation and remaining platform acceptance

The initial commit established this branch and its acceptance contract. Subsequent implementation uses current shared Engine, Core, Charting and Desktop sources with an explicit `ComputatorMono=true` build property. The reproducible Linux gate is `./build.sh --target=Mono-Validate`; it builds the actual executable and exercises production startup and supported desktop workflows under Mono/X11.

The baseline is master `ba57b8f08c1014363d26e7b87249c0e51fe74688`. Default builds retain .NET 10, WPF and the existing portable Engine/CLI. macOS GUI execution remains unverified; the Mono target does not certify every Linux distribution, newer runtime version or desktop integration. The pull request records only results reproduced from its implementation, with no reliance on previously reported unavailable patches or test counts.

Historical references: issue #25, issue #209, PR #139, PR #140, PR #146, and `docs/LEGACY-TOOLING.md`.
