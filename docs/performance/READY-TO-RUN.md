# Reduce compiler first-use JIT work with an audited ReadyToRun publish

This independent change builds on the precompiled mathematical facade in #249. The facade removes repeated emission of built-ins. ReadyToRun addresses a different cost: loading and JIT-compiling Roslyn and application methods before the first small program can be emitted.

Release `win-x64` desktop publishes use non-composite ReadyToRun by default, unless the caller supplies `-p:PublishReadyToRun=false`. Other platforms/configurations retain their policy. IL is not removed; no trimming, NativeAOT, restricted expression language, user-code cache or disabled tiered compilation is introduced. The original Framework baseline is neither retargeted nor republished.

## Why measurement is required

Microsoft documents reduced startup/first-use JIT work, larger files, possible working-set costs, and later replacement of hot R2R methods by tiered JIT code. This is a tradeoff, not an automatic speedup guarantee: https://learn.microsoft.com/en-us/dotnet/core/deploying/ready-to-run . R2R does not compile the user's future TSL source; Roslyn still emits each fresh uniquely named collectible program.

## Paired experiment

`Prepare-ReadyToRun.ps1` publishes both IL and R2R from the exact same clean commit and SDK, checks identical runtime policies, records native-header signatures and payload sizes, and verifies R2R images for Core, Engine, Microsoft.CodeAnalysis and Microsoft.CodeAnalysis.CSharp. It emits two manifests:

- `historical-manifest.json`: authenticated official 2.3.2.0 versus the R2R candidate, on one host.
- `modern-manifest.json`: the same candidate's IL-only versus R2R deployment on one host. Both startup targets use one byte-identical IL observer. This isolates publishing from the facade's source change.

The workflow runs these experiments on independent runners and never pools their observations. Each comparison uses 20 counterbalanced fresh-process pairs, full correctness checks and failure retention. Builds, native-image inspection and profile conditioning happen before the measured processes. Compiler/runtime caches within each measured process start fresh; filesystem caches are explicitly not flushed. Headline runtime tiering/PGO settings remain at their packaged defaults.

```powershell
./build/performance/Prepare-ReadyToRun.ps1 -OutputDirectory C:/Benchmarks/r2r-input -DisposableProfile
python build/performance/compare.py run --manifest C:/Benchmarks/r2r-input/historical-manifest.json --output C:/Benchmarks/r2r-historical --pairs 20 --timeout 120 --bootstrap 50000 --seed 191
python build/performance/compare.py run --manifest C:/Benchmarks/r2r-input/modern-manifest.json --output C:/Benchmarks/r2r-modern --pairs 20 --timeout 120 --bootstrap 50000 --seed 191
```

## Acceptance

Report absolute first compilation/interaction, bounded-warm compilation, repeated evaluation, native mathematics and responsive-main-window startup separately, together with intervals and payload/working-set cost. A successful publish and nonzero native header alone do not establish performance parity. Correctness tests and all published workload oracles must pass first. The exact historical 2.4 package is still unavailable; 2.3.2 measurements cannot answer the exact 2.4 comparison. Do not merge without owner approval.
