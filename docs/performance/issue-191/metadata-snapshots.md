# Current Roslyn compilation latency and reference memory

This experiment addresses the remaining compiler costs in [issue #191](https://github.com/PawelTroka/Computator.NET/issues/191). Its unchanged baseline is **`fe1aba49b5f9538355ffaaa3d1683f69a23d2d9c`**. That baseline already contains precompiled mathematical helpers, ReadyToRun publishing, the bounded direct numeric compiler, and cached metadata-reference objects/options. Those optimizations are not attributed to this change.

The [original historical comparison](README.md), its authenticated v2.3.2 binary, and its results remain unchanged. This experiment compares two .NET 10 products. It does not supply the still-missing authenticated v2.4-versus-v3.0 result.

## Change under measurement

The full Roslyn path creates cached references by copying complete PE images. ReadyToRun code makes those files larger, although Roslyn needs their metadata to bind a new program. The first change snapshots metadata for ordinary single-module references and retains the existing complete-image behavior for assemblies with linked modules and malformed-image diagnostics.

The initial Windows attribution also established that script emission dominates repeated compilation. The second change builds fixed script helper implementations once. Each eligible generated script still declares its own `TypeDeducer`, `File`, and `ReadForm` types, with its own constructors, deriving from the original implementations. Its private `objectToString(object)` method and four `ToMathString` extension overloads retain their signatures and forward to precompiled formatting bodies. The existing build generator uses syntax nodes and audited source hashes; mathematical, formatting and I/O algorithms are not copied into a new implementation.

All other `ScriptingFunctions.ToCode` bodies stay program-local, including callback and chart fields, initialization, `write`/`read`/`plot` overload groups and the two private formatting constants. Six small non-formatting extension methods and `NumericalExtensions` remain unchanged. The formatter has two stateless versions containing the original bodies with the same notation enum literals previously substituted into generated source. This preserves each program's notation snapshot without making the four matrix/vector lambdas capture a runtime setting or a program object.

Only the exact shipped `ScriptEvaluator` uses compact helpers, after the existing compatibility policy has accepted the translated program. Specialized evaluators, custom nested types, recognized reflection and private helper-member access retain the original generated definitions. A separate decision from the same syntax inspection keeps the original formatting bodies for custom declarations that conflict with the formatter's constants or `System` qualifier; it does not change math/helper eligibility. Unknown notation values retain the existing source and compilation failure. As with the existing mathematical facade, this does not promise transparency to arbitrary external reflection into implementation details; the compact object helpers have different reflected base/declaring types, and exceptions originating in precompiled helpers can show different implementation frames through `Exception.ToString()`.

The reference set, C# language version, Release compiler options, numerical implementation, generated program identities, collectible loading and custom-reference scope remain unchanged. The existing cache still owns immutable reference snapshots; this is not a cache of user programs or results. A changed definition must still produce a new program with the new result.

## Two exact products and one common driver

[`Prepare-CompilerComparison.ps1`](../../../build/performance/Prepare-CompilerComparison.ps1) requires separate clean baseline and candidate checkouts and an exact candidate commit. It hashes tracked source files and permits only the reference loader, helper compatibility policy, two evaluator files and built-in generator as product changes. The generator's existing inline task moves to a C# source file so MSBuild does not interpret its C# escapes as paths. The existing Roslyn `5.9.0` package pin becomes a shared property for the build task and compiler; the source audit validates that exact refactor and rejects any effective package-version change. Tests, documentation and the additive diagnostic harness are recorded separately. Shared SDK selectors, effective package pins, original workload source and other build configuration must match.

Both product payloads are published Release, framework-dependent, win-x64 with non-composite ReadyToRun. The common driver is deliberately **IL-only** and copied byte-for-byte into both products. This avoids baking either product's dependency assumptions into a shared native driver. The preparer verifies the actual PE headers: no native header in the worker, and ReadyToRun signatures in Core, Engine, Microsoft.CodeAnalysis and Microsoft.CodeAnalysis.CSharp. It records the image hashes and sizes in `audit/publication-images.json`.

Every product retains its own compiled assemblies. Dependency/runtime configuration and third-party binary identities are checked before using the common driver. Full payload hashes, source snapshots, build commands, actual CLR/JIT hashes, SDK/runtime versions, CPU, OS, RAM, power scheme and profile paths are retained.

## Workloads and endpoints

The original [`Worker.cs`](../../../build/performance/Worker.cs) is unchanged. The new entry point selects these workloads and reports their existing correctness oracles:

| Scenario | Purpose |
|---|---|
| `real`, `complex`, `surface` | Default direct-compiler controls; retain cached evaluations, integration/grid sampling and changed-custom-definition observations |
| `script` | Normal script first compilation, first execution, bounded-warm recompilation and cached execution |
| `roslyn-real`, `roslyn-complex`, `roslyn-surface` | Identical original workloads with the existing direct-compiler opt-out enabled in both products |
| `memory-roslyn-real`, `memory-script` | Fixed 33 distinct custom-definition edits; verify every result and discard the resulting programs |

First compilation is distinguished from evaluator construction, first execution and the complete first interaction. Original repeated-compilation warmup is bounded by one second and a maximum of 32 compilations. This is a repeatable interaction workload, not a guarantee that every JIT/PGO path has reached an asymptotic steady state.

The fixed-count memory diagnostics record total managed allocation, managed memory, private bytes, working set and peak working set. Collection occurs only before and after those separate diagnostics. Byte counters remain diagnostic fields; they are never inserted into the millisecond metric family. Their descriptive process medians do not establish allocation throughput or prove absence of leaks. Deterministic collectible-lifetime tests provide the reclamation gate.

## Controlled collection

The workflow runs two independent Windows x64 sessions. Each session first passes the Core correctness suite and harness contract tests, then screens all workloads with one separately retained process pair per scenario. Screening observations do not enter the measured sample.

Each session collects 20 fresh-process pairs, sequentially on its own runner, alternating AB/BA order and rotating scenarios. Builds, tests and publication finish before measured collection. No workload runs concurrently with another measured workload on that runner. The existing `compare.py` reduces inner observations to process medians and computes deterministic paired bootstrap intervals, with the existing practical threshold and multiplicity adjustment across all declared latency comparisons.

The unchanged analyzer uses a 10% practical threshold, 50,000 bootstrap draws and approximate 95% family confidence across 59 declared latency comparisons. Its symmetric speed-factor band is `1 / 1.10` through `1.10` for candidate/baseline latency ratios. Invalid comparisons still count in that family. Point estimates alone do not receive a material-improvement classification; the adjusted interval must lie below that band and satisfy the analyzer's validity checks.

Failed observations are retained, not retried or silently removed. An incomplete or invalid comparison does not support a performance claim. Results from the two hosts are reported separately; absolute times are not pooled.

Fresh process means fresh compiler/JIT state in that process. Profiles are conditioned and the OS filesystem cache is unflushed. These are not reboot-cold, installation or disk-cold observations. Desktop startup, browser requests and graphical rendering are outside this experiment.

## Compatibility regression coverage

Six metadata-reference cases check closed file handles and snapshot survival after file replacement/GC, late `AddDll` and per-compiler scope, linked netmodules, malformed-image diagnostics, generic constraints and imported attributes. Thirteen script-helper cases check exact original source, distinct per-program helper types, delegate identity for all `TypeDeducer` arities, file I/O and handle closure, STA form construction/disposal, custom overloads, notation capture, callback isolation, reflection/private-member diagnostics, specialized evaluators and collection of discarded programs.

Thirteen additional formatting cases cover both notation modes for real and complex matrices/vectors, all four extension overloads and method groups, decimal precision, null, arbitrary objects and `ToString()` call count, custom overloads, local shadows, conflict diagnostics against the previous source shape, notation changes on one evaluator, invalid enum behavior and collection after matrix formatting. Source checks verify unchanged signatures, constants and every non-formatting method body.

The workflow requires all 32 cases in those exact fixtures to be discovered and passed as part of the full Core suite. It also runs the existing performance contract tests plus the new current-comparison and package-pin guards. These checks assert behavior and experimental integrity; unit tests do not use brittle millisecond thresholds. Statistical performance results are reported separately.

Before Windows execution, the candidate's .NET 10 Core/Core.Tests and `ComputatorMono=true` .NET Framework Core cross-builds passed with zero errors. The integrated .NET 10 build and Framework build each reported six existing warnings in MatrixFunctions and Charting. Source-fidelity fixture methods and 20 original-versus-compact generated-source emission checks also passed locally. These are build/source checks, not substitutes for Windows execution of WinForms and runtime behavior.

## Compiler attribution

[`inspect_compilation.py`](../../../build/performance/inspect_compilation.py) adds an explicit `--modern-baseline` mode. Its default historical mode remains available. The new mode uses the same existing diagnostic template to capture actual generated source and inspect the first native compilation plus 32 repeats for both modern payloads.

It separates compilation creation/reference setup, symbolic-power resolution, emission, assembly loading and delegate binding. `Emit` is an API boundary that includes Roslyn's deferred semantic binding and code generation; its first-call time also includes compiler/runtime initialization, so it is not a measurement of isolated IL generation. Captured source, emitted assembly size, references, correctness and GC counts remain available. Reference PE-file size and metadata size are inspected after timed phases. Those sizes describe input files, not actual process allocations. Instrumented phase observations are retained separately from the primary latency experiment.

### Work retained deliberately

The compiler already caches reference objects, parse options and compilation options in the baseline. Removing those allocations again would duplicate existing work. The full reference set remains available for custom code; choosing a narrower set based only on the benchmark expressions would change the language surface.

A shared `CSharpCompilation` template is not automatically a reusable bound-reference cache. In the [exact Roslyn source used by package 5.9.0](https://github.com/dotnet/roslyn/blob/35d9211b841e7613c1d2f8f5af6d628ace696c4c/src/Compilers/CSharp/Portable/Compilation/CSharpCompilation.cs), `WithAssemblyName` invalidates that cache when the name changes, because friend-assembly and circular-reference binding can depend on the name. This experiment preserves unique program identities and does not replace them with a constant assembly name to obtain that reuse.

Each genuinely changed user definition still requires binding, emission and a fresh program. No generated delegate, assembly or user type is added to a process-wide cache. The attribution identifies loading and binding as small repeated costs compared with emission, so this change keeps the existing collectible loader and delegate-binding behavior.

## Reproduce

Use Windows x64, the SDK pinned in `global.json`, PowerShell 7, Python and two clean checkouts:

```powershell
./candidate/build/performance/Prepare-CompilerComparison.ps1 `
  -BaselineSourceDirectory C:/Benchmarks/baseline `
  -CandidateSourceDirectory C:/Benchmarks/candidate `
  -ExpectedCandidateCommit '<exact 40-character candidate commit>' `
  -OutputDirectory C:/Benchmarks/compiler-input

python candidate/build/performance/compare.py run `
  --manifest C:/Benchmarks/compiler-input/manifest.json `
  --output C:/Benchmarks/compiler-results `
  --pairs 20 --timeout 120 --bootstrap 50000 --seed 191

python candidate/build/performance/compiler_comparison_manifest.py `
  --raw C:/Benchmarks/compiler-results/raw.json `
  --memory-output C:/Benchmarks/compiler-results/memory.json

python candidate/build/performance/inspect_compilation.py --modern-baseline `
  --input C:/Benchmarks/compiler-input --output C:/Benchmarks/compiler-phases
```

The workflow runs attribution on a separate Windows host. When reproducing on one host, run attribution after primary measurements. To reproduce inference without Windows execution, use the existing `compare.py analyze` command on the original `raw.json`.

## Exact-commit evidence

Completed Windows observations, candidate identity, run/artifact links, and measured effects belong here after validation. A successful build or the expected reduction in reference bytes alone is not a latency result.

### Attribution that selected the changes

The first [Windows attribution job](https://github.com/PawelTroka/Computator.NET/actions/runs/41105788879/job/124235121396), at metadata-only candidate `a184d84bbfd721a11ed754536f766d5c6f574f9e`, completed successfully. Its separate timing diagnostic identified the following costs in the unchanged current baseline. These are observations from one instrumented process per scenario, not paired performance estimates or confidence intervals.

| Baseline diagnostic phase | Real expression, first | Real expression, later median | Script, first | Script, later median |
|---|---:|---:|---:|---:|
| Create compilation and references | 111.793 ms | 0.155 ms | 135.735 ms | 3.575 ms |
| Symbolic-power inspection | 24.515 ms | 0.056 ms | 27.041 ms | 2.100 ms |
| Emit | 516.779 ms | 4.607 ms | 721.678 ms | 53.724 ms |
| Load generated assembly | 12.948 ms | 0.095 ms | 12.964 ms | 0.158 ms |
| Bind delegate | 0.485 ms | 0.127 ms | 0.715 ms | 0.217 ms |

There are 32 later observations per scenario. The real program contained 604 characters on 17 lines; the script contained 28,409 characters on 760 lines. The compiler referenced 246 and 247 files respectively. Their aggregate PE-file sizes were 216,573,328 and 216,593,592 bytes; metadata occupied 40,343,676 and 40,350,720 bytes. These input-file measurements motivated metadata-only snapshots; they are not process-memory measurements.

In this diagnostic, metadata-only snapshots reduced first compilation creation/reference setup to 79.284 ms for the real expression and 91.448 ms for the script. Later emission remained approximately 4.53 ms and 53.83 ms. This established that reference snapshots alone do not remove repeated script compilation cost. The helper change therefore removes fixed source and methods from that remaining emission work.

The reference set is retained because arbitrary compatible custom code can use it. New programs retain unique assembly identities and fresh collectible load contexts; caching a previous result would violate changed-definition and lifetime behavior. Binding and emitting genuinely new user code remain necessary. The phase data does not justify redesigning assembly loading or delegate binding, whose repeated costs are small compared with emission.

The pilot used SDK 10.0.403 and runtime 10.0.14. Its attribution artifact is `13000704587` (`current-compiler-phase-attribution`), ZIP SHA-256 `8f5683921403b6f9fad320a9e4989d9f550fd181ade3d8a8d22a477be4f0fecb`. The two pilot comparison jobs failed before benchmark collection because the genuine Framework settings fixture was not provisioned. They are not successful performance sessions. The corrected workflow follows the existing `build.cake` fixture-generation setup; the three-helper revision required the full Core suite and all 19 then-new regression cases before measurement. The formatting revision adds 13 more explicit cases to that gate.

### Three-helper candidate attribution

The [next attribution job](https://github.com/PawelTroka/Computator.NET/actions/runs/41109033052/job/124246239371) measured candidate **`32ae3c303b4ab28b14e1c733dfa4164f4eb4815b`**, containing metadata snapshots and the three precompiled object helpers. Every first/repeated numerical or script oracle passed, and payload identities matched before and after. The [compact observation file](compiler-phase-32ae3c3-observations.json) retains all 132 phase observations, GC counters, source hashes, assembly sizes and declared-type details; it omits only the verbose reference path lists from the phase records.

Each cell below is baseline → candidate. These are separate instrumented processes, not paired confidence estimates.

| Phase | Real, first | Real, later median | Script, first | Script, later median |
|---|---:|---:|---:|---:|
| Create compilation and references | 111.089 → 77.126 ms | 0.161 → 0.192 ms | 125.383 → 90.981 ms | 3.915 → 2.977 ms |
| Symbolic-power inspection | 24.171 → 24.612 ms | 0.056 → 0.062 ms | 28.365 → 27.085 ms | 2.230 → 1.871 ms |
| Emit | 525.212 → 517.439 ms | 4.606 → 4.794 ms | 736.530 → 699.564 ms | 50.921 → 51.566 ms |
| Load generated assembly | 13.234 → 13.452 ms | 0.106 → 0.127 ms | 13.088 → 16.575 ms | 0.200 → 0.197 ms |
| Bind delegate | 0.492 → 0.519 ms | 0.128 → 0.147 ms | 0.694 → 0.770 ms | 0.225 → 0.228 ms |

The generated script shrank from 28,409 characters / 760 lines to 21,144 characters / 539 lines. Its emitted assembly shrank from 15,872 to 11,776 bytes and declared methods from 73 to 54; all nine types and seven constructors remained. The real-expression source and its 2,560-byte assembly were unchanged. These size reductions do **not** establish a repeated-compilation speedup: the diagnostic's repeated script `Emit` median was essentially unchanged. Both traces also show changing per-iteration costs and collections, which preclude treating that median as a fully tiered steady-state estimate.

The retained script still emits the fixed `objectToString` formatting body, its four matrix/vector formatting lambdas and the original numerical/string extension methods. Those are additional candidates for removing repeated binding/code-generation work while keeping the program's callback state and overload groups.

This diagnostic used SDK 10.0.403 and runtime 10.0.14. Artifact `13004134762` has locally verified ZIP SHA-256 `a860afcefb987afbfd3c81bdd2408766c6aa792982d99f14f34c74fcd2b7a973`; the original `compilation-diagnostics.json` SHA-256 is `7e188354557c7234c072e787738a23a9fa1f8ff50dab0865270e17606630ab6f`. Independently extracted job-log JSON agrees with the artifact's parsed data.
