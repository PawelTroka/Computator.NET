# Current compiler metadata snapshots

This experiment addresses the remaining compiler costs in [issue #191](https://github.com/PawelTroka/Computator.NET/issues/191). Its unchanged baseline is **`fe1aba49b5f9538355ffaaa3d1683f69a23d2d9c`**. That baseline already contains precompiled mathematical helpers, ReadyToRun publishing, the bounded direct numeric compiler, and cached metadata-reference objects/options. Those optimizations are not attributed to this change.

The [original historical comparison](README.md), its authenticated v2.3.2 binary, and its results remain unchanged. This experiment compares two .NET 10 products. It does not supply the still-missing authenticated v2.4-versus-v3.0 result.

## Change under measurement

The full Roslyn path currently creates cached references by copying complete PE images. ReadyToRun code makes those files larger, although Roslyn needs their metadata to bind a new program. The proposed change snapshots metadata for ordinary single-module references and retains the existing complete-image behavior for assemblies with linked modules.

The reference set, C# language version, Release compiler options, numerical implementation, generated program identities, collectible loading and custom-reference scope remain unchanged. The existing cache still owns immutable reference snapshots; this is not a cache of user programs or results. A changed definition must still produce a new program with the new result.

## Two exact products and one common driver

[`Prepare-CompilerComparison.ps1`](../../../build/performance/Prepare-CompilerComparison.ps1) requires separate clean baseline and candidate checkouts and an exact candidate commit. It hashes tracked source files and permits only `NativeCompiler.cs` as a product change. Tests, documentation and the additive diagnostic harness are recorded separately. Shared SDK selectors, package pins, original workload source and build configuration must match.

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

Failed observations are retained, not retried or silently removed. An incomplete or invalid comparison does not support a performance claim. Results from the two hosts are reported separately; absolute times are not pooled.

Fresh process means fresh compiler/JIT state in that process. Profiles are conditioned and the OS filesystem cache is unflushed. These are not reboot-cold, installation or disk-cold observations. Desktop startup, browser requests and graphical rendering are outside this experiment.

## Compiler attribution

[`inspect_compilation.py`](../../../build/performance/inspect_compilation.py) adds an explicit `--modern-baseline` mode. Its default historical mode remains available. The new mode uses the same existing diagnostic template to capture actual generated source and inspect the first native compilation plus 32 repeats for both modern payloads.

It separates compilation creation/reference setup, symbolic-power resolution, emission, assembly loading and delegate binding. Captured source, emitted assembly size, references, correctness and GC counts remain available. Reference PE-file size and metadata size are inspected after timed phases. Those sizes describe input files, not actual process allocations. Instrumented phase observations are retained separately from the primary latency experiment.

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
