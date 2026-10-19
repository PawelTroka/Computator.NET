# Mono desktop compatibility — issue #209 (work in progress)

This branch is a **draft**, not a working Mono desktop release. The historical build used a Framework target, `__MonoCS__`, an alternative 2D chart library and a 3D placeholder. PRs #139 and #140 removed those build branches and moved the Windows application to .NET 10.

## Scope and non-regression requirements

- Preserve the default `net10.0-windows` build, Windows charting and existing portable `net10.0` engine.
- Add a separate opt-in Framework/Mono target, without claiming that a `net10.0-windows` binary executes under Framework Mono.
- Reuse shared TSL, matrix, integration, derivative and root source. Resolve runtime differences for `AssemblyLoadContext`, `NativeLibrary`, startup and settings rather than duplicating mathematical algorithms.
- Restore a Mono-specific 2D chart implementation and non-WPF 3D placeholder. Preserve complex-chart functionality; do not silently omit it.
- Keep scripting, custom functions, numerical method selection, document load/save and GSL functional.
- Test actual Linux Mono WinForms startup, calculation, scripting, 2D and complex chart rendering, native GSL and shutdown. Validate macOS GUI separately; a passing macOS engine test does not prove a working WinForms application.
- Keep the PR draft until these gates pass and the owner reviews the differences.

## Baseline / outstanding work

This initial commit only establishes a reviewable implementation branch and records the acceptance contract. **It does not contain or claim the earlier unpushed 164-test implementation.** That local patch is unavailable in the current workspace and must be recreated or recovered before the PR can be considered functional.

The current master at `ba57b8f08c1014363d26e7b87249c0e51fe74688` still uses `net10.0-windows`, unconditional WPF in Charting and Desktop, and a `net10.0`-only Engine. None of those are resolved by this document.

Historical references: issue #25, issue #209, PR #139, PR #140, PR #146, and `docs/LEGACY-TOOLING.md`.
