# Reproducible historical performance comparison

This harness investigates [issue #191](https://github.com/PawelTroka/Computator.NET/issues/191). It measures the shipped Windows desktop evaluator and an authentic historical binary in separate processes on the same machine. The portable engine is not substituted for either desktop product.

## Baseline identity matters

The issue describes **2.4.0.0**, but an attributable Framework 2.4 distribution has not been located. The public GitHub releases/tags, pre-modernization source, public AppVeyor history and Microsoft Store catalog inspected for this investigation identify **2.3.2**. The `2.4.0.0` in the modern MSIX manifest is a packaging template introduced during the .NET 10 migration, not evidence of a historical Framework release.

The default is therefore explicitly **official 2.3.2 control versus the current 3.0 candidate**. It must never be described as measured 2.4 performance. Supply the original 2.4 archive and its attributable provenance to answer that exact part of the issue; the acquisition helper validates its actual versions before accepting it.

The authentic control is the unmodified [official v2.3.2 portable ZIP](https://github.com/PawelTroka/Computator.NET/releases/download/v2.3.2/Computator.NET.v2.3.2.zip):

- GitHub release asset: `5646130`, 12,002,377 bytes.
- Archive SHA-256: `21a64b059d78176b975d066a6459e4467f9a523c997f57981ceca49485df8b2d`.
- Core SHA-256: `aefa901c609e40eafff49e5526e308d75557a82aeee2904db797659f88c58c2b`.
- EXE/Core/DataTypes assembly and file versions: `2.3.2.0`; target `.NETFramework,Version=v4.6.1`; Retail configuration.
- AnyCPU IL-only binaries, without required/preferred 32-bit flags. The harness checks actual x64 execution as well.
- Source release tag: `56fed3363c07026d7fb290e40f852468dfe158ba`. This is context, not proof that a present-day rebuild reproduces the shipped ZIP.

The original product and its dependency DLLs are not rebuilt, retargeted, patched, or upgraded. The same C# driver is compiled against those binaries on `net461` and against the candidate desktop Core on `net10.0-windows`. Both drivers are Release/x64. The modern product is published framework-dependent for the controlled comparison. Its self-contained installer, packaging size and first installation are separate measurements.

Historical CodeDOM does not set an explicit `/optimize` option for generated functions; current Roslyn requests `OptimizationLevel.Release`. Both policies are preserved. Generated assemblies' actual `DebuggableAttribute` flags are recorded; host Release settings alone do not establish equal generated-code optimization.

## Run on a disposable Windows account or VM

The authentic historical product uses `Documents/Computator.NET` and does not understand the current profile override. Use a disposable Windows account/VM. Preparation puts the candidate in a separate supported `COMPUTATOR_DATA_DIRECTORY` and clears inherited native-library overrides. It never installs either product.

Requirements: Windows x64 with an interactive desktop, the installed .NET Framework runtime, Windows PowerShell 5.1 for metadata-only inspection, PowerShell 7, Python 3.10+, Git, and the SDK pinned by `global.json`. Build from a clean checkout of the commit to measure.

```powershell
python -m unittest discover -s build/performance -p 'test_*.py' -v
./build/performance/Prepare-Comparison.ps1 -OutputDirectory C:/Benchmarks/run-input -DisposableProfile
python build/performance/compare.py run --manifest C:/Benchmarks/run-input/manifest.json --output C:/Benchmarks/run-results --pairs 20 --timeout 120 --bootstrap 50000 --seed 191
```

For a verified owner-supplied 2.4 archive:

```powershell
./build/performance/Prepare-Comparison.ps1 -OutputDirectory C:/Benchmarks/run-24-input -DisposableProfile `
  -HistoricalArchive C:/Inputs/Computator.NET.2.4.zip `
  -HistoricalSHA256 '<actual 64-character SHA-256>' -HistoricalVersion '2.4.0.0' `
  -HistoricalSource 'Description of the original build and where these bytes came from'
```

The downloaded default is pinned in code. A supplied bundle's EXE/Core/DataTypes must agree with the claimed version, Framework target, Release/Retail configuration and supported architecture. The current driver targets the verified control's `net461`; a supplied build targeting a later Framework may require an explicitly reviewed matching driver target. A caller-supplied hash establishes byte identity, not publisher authenticity.

The workflow runs two independent Windows sessions. Builds, downloads, payload hashing and profile/native-cache conditioning finish before collection. Build servers are shut down. No tests/builds run concurrently with collection within the benchmark job. Every process pair runs sequentially with AB/BA ordering, and scenario order rotates between pairs. No machine-wide power policy or cache purge is silently performed.

## Workloads and boundaries

| Scenario | Timed operations | Correctness check |
| --- | --- | --- |
| Real expression | Constructor; first compile; first typed invocation; total first interaction; unchanged warm recompilation; changed custom definitions; cached typed and public evaluation; fixed Simpson integration | Independent BCL formula at 1,024 inputs, batch checksum, analytic integral and changed custom-function values |
| Complex expression | Constructor, first interaction, warm recompilation and 65,536 cached evaluations | Independent `System.Numerics.Complex` expression at varying inputs and checksum |
| Surface | Constructor, first interaction, warm recompilation, 64×64 fixed sample grid | Every grid value and checksum against the independent analytic expression |
| Script | Constructor, first compile/run, total first interaction, warm recompilation and 32 repeated executions | Exactly one captured console result per execution, integer square sum `333833500` |
| Mathematics | 16,384-call batches of elementary functions, managed gamma/Bessel/Airy functions, and native GSL regularized incomplete gamma | Individual finite reference values and checksums; native `P(1,x)` checked against `1-exp(-x)` |
| Translation | First transform, 256 repeated ordinary-expression transforms and empty-fragment transforms | Product-compiled numerical oracle, each within-process transform equal to its verified first result, empty fragments stay empty |
| Desktop startup | Process start to visible, responsive main window with the six main workflow tabs | Exact shipped main-form title, PID-owned visible window, native tab count and `WM_NULL` reply; x64 target and normal shutdown verified |

`compile.first.ms` begins after evaluator construction. `evaluator.create.ms` records that setup separately, and `first.interaction.ms` includes construction, compilation and the first result, with small instrumentation bookkeeping. Native engine initialization is timed separately, as it precedes expression entry in the desktop application. First invocation occurs before oracle calls or evaluation warmup.

Cached evaluations use the actual generated delegate; the public evaluation metric also includes the product's params-array/casting path. Recompilation does not pretend to be a compiled-result cache hit: neither baseline nor candidate has a whole-program cache in this path. Custom definitions change between measurements and are checked to detect stale programs.

For startup, the same modern observer binary launches both desktop executables. Its runtime is recorded separately from the target's actual loaded CLR module file version. The readiness predicate excludes the separate splash-screen message loop. The 10 ms polling interval bounds the observer's sampling granularity. Startup is measured after conditioning with a reused profile/native extraction cache and an **unflushed OS filesystem cache**; it is not a disk-cold or first-install measurement.

The fixed chart grid measures computation, **not pixels rendered or GPU presentation**. API request latency is not inferred from these data. The modern API's one-shot worker/cancellation architecture ([#164](https://github.com/PawelTroka/Computator.NET/pull/164)) adds a separate intentional process boundary and requires its own workload comparison. Browser/page rendering, chart rendering, installation and clean-machine startup remain separate acceptance work.

The existing GUI `Computator.NET.Core/Benchmarking/Benchmark.cs` informed the mathematical function families. Its whole-millisecond timing, random memory workload, single-process observations, missing correctness oracles and lack of compiler/script/startup boundaries make its aggregate score unsuitable as this comparison's statistics. No user-facing benchmark behavior is changed.

## Warmup, cache and statistical controls

The driver records high-resolution `Stopwatch` timings and frequency. First-use observations are retained separately. Repeated phases warm for at least one second; compilation warmup is capped at 32 iterations because the historical AppDomain retains generated assemblies. Warmup traces, iteration counts and whether the budget was reached are retained. Five inner observations are reduced to **one median per fresh process**. Inner loop iterations and inner observations are not treated as independent samples.

Runtime default tiered JIT/PGO behavior is preserved for the headline comparison. An elapsed warmup budget does not prove universal steady state; compiler cache measurements and recorded traces should be read accordingly. `--diagnostic-tiering-off` is a separate causal diagnostic and is never combined with default-runtime results. Microsoft documents background tiered optimization in its [runtime compilation configuration](https://learn.microsoft.com/en-us/dotnet/core/runtime-config/compilation).

The analyzer computes paired log ratios (candidate/baseline) and deterministic pair-resampled bootstrap intervals. It reports absolute median milliseconds, normalized time per operation, independent pair counts and a practical threshold of 10%. The interval is adjusted conservatively for the declared metric family; all declared metrics remain in that family even if a scenario fails. At least 15 complete process pairs and adequate bootstrap-tail/timer resolution are required before inferential labels. A wide interval is inconclusive, not proof of equivalence. Both independent Windows sessions should agree before attributing a material effect broadly to the product.

Failed or missing children, wrong output, nonfinite/zero timings, changed operation counts, schema/configuration drift, runtime changes and modified pinned binaries invalidate the affected comparison. Failures are retained, not replaced by retries or discarded as outliers. The supervisor bounds children and owns their process trees, including historical `csc.exe` descendants.

## Evidence and replay

Outputs include:

- `manifest.json`: exact commands, workload definitions, operation counts, target versions, profiles, source identity and hashes of the payloads.
- `windows-environment.json`: CPU, OS build, RAM, installed runtime/Framework identity, active power scheme and cache conditions.
- `raw.json` and child stdout/stderr logs: all individual process observations and failures, saved as collection progresses.
- `analysis.json` and `report.md`: derived paired comparisons and explicit validity/uncertainty classifications.
- Startup conditioning records and baseline acquisition metadata.

Raw records can be reanalyzed on another operating system without re-running the application:

```text
python build/performance/compare.py analyze --input raw.json --output replay
```

Do not add a timing threshold to ordinary unit tests. Correctness regressions belong in deterministic tests; performance claims require controlled repeated measurement. No speedup is claimed merely because the harness builds or its analyzer tests pass. Findings and any measured improvements are documented separately with the exact candidate and observation artifacts.
