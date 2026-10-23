# Translation restoration diagnostic for issue #191

This directory preserves the source, correctness inputs, declared protocols and numerical results for a narrow translation optimization. The completed Linux experiment has mixed results: ordinary expressions and empty fragments improved under its declared protocol, while the literal-heavy control regressed. A separate Windows experiment places each fixture in its own process to investigate the concrete risk exposed by that control. These diagnostics do not establish a 2.4-versus-3.0 performance regression or measure desktop startup or end-to-end expression compilation.

## The change being investigated

`TslCompiler.TransformToCSharp` protects literal text and certain declared identifiers, performs every TSL handler, then restores the protected tokens with a regular expression. Each call uses a fresh GUID in its restoration pattern. If the protected-token list is empty, there is nothing to restore. The proposed change returns the already-transformed result immediately before that final restoration:

```csharp
var result = TslHandlers.Aggregate(protectedCode, (current, handler) =>
    handler is PowHandler powers ? powers.Replace(current, deferSymbolicFunctionPowers) : handler.Replace(current));
if (literals.Count == 0) return result;
return Regex.Replace(/* existing restoration, unchanged */);
```

The guard remains **after every handler**. Comments still become whitespace; powers inside an interpolation containing no literal-text tokens still translate. The list also protects declared and constructed identifiers, so it cannot be treated simply as a list of quoted strings. Calls that contain protected tokens execute the existing restoration code. The change does not remove lexing, GUID generation or Roslyn compilation.

The original Linux snapshot was recorded at `c558523a738ef13dea452195d301781427c6cdbe`. All 13 compiled translator-source files were subsequently checked against `ba57b8f08c1014363d26e7b87249c0e51fe74688` and `58982403dead54824810a96439b851d3123e0e03`; their bytes match at all three commits. `protocol.json` pins every file. Its generated guard variant contains only the additional return statement. The committed guard at `afa521ec577cdae2b5a3492bfe87fde1e5494b06` also has two explanatory comments; the independent Windows preparer verifies that exact change and no other change among its 13 compiled inputs.

## Completed Linux experiment

This was an isolated Linux x64, .NET 10.0.13 translation-source experiment built with SDK 10.0.402 and Roslyn 5.9.0 in Release. Both variants used one byte-identical probe DLL and the same runtime and dependencies. The library contained the actual translator, all its TSL handlers and `SpecialSymbols`; it was not a full Computator.NET product build.

There were 20 sequential pairs of fresh processes. Baseline-first and candidate-first order alternated. Each process ran all six phases in the fixed order below, with at least 1.5 seconds and three batches of warmup for each phase, followed by seven timed batches. One process contributed its median batch time, so the seven inner measurements were not treated as seven independent samples. Process-local state was reset between processes; operating-system caches were not reset. Runtime tiering defaults remained enabled.

The table reports per-operation values derived from the retained batch timings. Ratios are the geometric mean of paired process-median ratios; therefore, a ratio is not necessarily the quotient of the two displayed medians.

| Fixture | Operations per batch | Baseline median µs/op | Guard median µs/op | Paired guard/baseline ratio | Adjusted interval | Classification |
|---|---:|---:|---:|---:|---:|---|
| Ordinary expression | 512 | 463.13 | 284.65 | 0.5371 | 0.3986–0.7154 | Improvement |
| Empty fragment | 1,024 | 20.60 | 9.49 | 0.3685 | 0.2221–0.6026 | Improvement |
| Comment and function power | 256 | 68.44 | 69.95 | 0.8621 | 0.4817–1.4947 | Inconclusive |
| Symbolic function power | 512 | 41.82 | 33.95 | 0.8005 | 0.5189–1.2446 | Inconclusive |
| Literal-heavy script | 128 | 453.72 | 771.37 | 1.5974 | 1.1577–2.3789 | Regression |
| Large repeated script | 8 | 5,005.41 | 5,749.88 | 1.2233 | 0.8817–1.7984 | Inconclusive |

The intervals use 50,000 seeded paired-bootstrap draws and Bonferroni adjustment across all six declared metrics: nominal 99.16667% marginal intervals for an approximate 95% family confidence level. A practical regression requires the entire interval above 1.10; an improvement requires the entire interval below its reciprocal, 0.90909. These bootstrap intervals remain approximate with 20 independent pairs. An inconclusive result does not establish equivalence.

`linux-summary.json` retains all 20 baseline and candidate process medians for every metric, the intervals, all classifications and descriptive order/drift values. `verify-linux-summary.py` recomputes the timing statistics from those published numerical samples. With the untouched original raw file supplied, it also verifies its SHA256 and every published process median.

### Correctness and allocation observations

All 40 completed processes passed 43 exact semantic fixtures and 1,024 independent numerical checks before warmup. Every timed output was retained and compared with its expected full string after the stopwatch stopped. Validation time was excluded; the fixed output-array writes and common adapter call were included in both variants.

The semantic fixtures cover normal, verbatim, raw and UTF-8 strings; characters; comments and disabled text; interpolation with and without literal text; a user-written placeholder-looking string; symbolic powers in both binding modes; declared, constructed and shared function/type identifiers; empty input; and null-input exception behavior. A separately compiled translated expression was checked at 1,024 values against a BCL mathematical oracle. This combination avoids relying only on an output-length checksum or on one implementation agreeing with itself.

The current-thread allocation counter showed a reduction of approximately **4,112 bytes per translation** for the five phases that took the guard. The literal-heavy control allocated the same median 14,568 bytes per translation in both variants. These are descriptive `GC.GetAllocatedBytesForCurrentThread` counts, not total process allocation or a separate confidence-interval result. They support the mechanism of avoiding the final restoration operation; they do not prove an end-to-end latency improvement.

### Why the literal result needs a focused follow-up

The literal regression is retained without filtering. Its paired ratio was about 1.42 in baseline-first pairs and 1.80 in candidate-first pairs, so launch order alone does not explain it. It also occurred in both halves of the run: ratios about 1.36 and 1.87.

The ordinary-expression phase continued changing during the measured repetitions. The median ratio of the last three to the first three timed batches was about 0.67 for the baseline and 0.58 for the guard. The fixed 1.5-second warmup therefore did not demonstrate a settled state for that phase. This is an observed limitation, not a reason to discard its measurements.

One testable explanation for the later literal result is the fixed phase order: the baseline repeatedly invokes the final restoration regex during the four preceding phases, while the guard avoids those calls. The literal phase can consequently begin with a different BCL regex tiering/profile history. Garbage-collection history and host contention could also contribute. No per-run CPU-throttling counter delta was collected, so later cumulative kernel counters cannot establish that throttling caused any result. The data does not yet determine the cause.

The separate Windows protocol below changes the specific implicated boundary: each fixture receives a fresh process and its own longer warmup. It does not reuse, relabel or pool the original mixed-phase observations. The guard should not be presented as a universal translation speedup on the strength of the Linux table.

### Interrupted first collection

An earlier collection was interrupted by disk exhaustion after 17 complete pairs and one additional successful invocation. It did not complete its post-run artifact verification. The incomplete raw file remains unchanged and is excluded from inference. The reported experiment is a fresh, complete 20-pair collection; no samples were spliced between runs. `interruption.json` records the status and both raw-file hashes.

A two-pair preparatory pilot was also exploratory and is not part of the reported inferential results. An optional empty-input specialization was prepared but never measured or included in the proposed product change.

## Reproduce the original Linux protocol

Use a clean checkout of `58982403dead54824810a96439b851d3123e0e03`, an explicit Linux x64 SDK 10.0.402 installation containing runtime 10.0.13 and its .NET 10 reference pack, and a NuGet cache containing `Microsoft.CodeAnalysis.Common` and `Microsoft.CodeAnalysis.CSharp` 5.9.0. The cache can be populated by restoring the baseline Engine project with that SDK and the chosen `--packages` directory. The preparer verifies the actual Roslyn DLL hashes before compiling.

Keep this diagnostic directory available from the PR checkout and pass a separate baseline source checkout. Replace the illustrative paths below:

```sh
python /path/to/diagnostic/reproduce.py \
  --repo /path/to/clean-baseline \
  --dotnet /path/to/dotnet-10.0.402/dotnet \
  --packages /path/to/nuget-cache \
  --output /path/to/new-isolated-output
```

The output must be absent or empty. An output inside a source checkout must be ignored by Git. The script generates every `.csproj`, copied source, binary, manifest and result there; it does not edit product source or overwrite earlier observations. `--prepare-only` stops before timing. `--pairs` defaults to 20; fewer than 15 is exploratory under the shared analyzer.

The baseline supervisor must retain the exact hash in `protocol.json`. The preparer validates 13 product-source hashes, the common probe/fixture/oracle hashes, SDK/runtime identities and Roslyn dependencies. It builds both source variants with common options and one common probe, then pins the resulting files before paired collection. Relocating the build can change binary/PDB hashes, which are newly recorded; the input source identities remain fixed.

To reproduce the published Linux timing calculations without executing the benchmark:

```sh
python /path/to/diagnostic/verify-linux-summary.py \
  --supervisor /path/to/clean-baseline/build/performance/compare.py
```

An optional `--raw /path/to/original-raw.json` additionally checks the original full-file identity and the numerical extraction. The unmodified original raw, child output and host metadata are retained separately from this public source/numeric bundle. `identities.json` records the actual probe/core and raw hashes. The original collection pinned CoreLib and `libcoreclr`; the additional Regex and JIT-library identities were captured separately afterward and are explicitly labeled as supplementary. Future reproductions pin all four before collection.

## Separate Windows fixture protocol

`FixtureProbe.cs`, `windows-protocol.json` and `reproduce-windows-fixtures.py` define an independent Windows x64 source experiment. Its three scenarios are the ordinary expression, empty fragment and literal-heavy control. **Each scenario runs alone in a fresh process.** Each process performs the same 43 semantic checks and 1,024 numerical checks, then warms its one selected fixture for at least ten seconds and 20 batches before nine timed batches. The warmup has explicit maximums of 30 seconds and 100,000 batches; every warmup observation is retained.

Descriptive stability checks compare the last two groups of ten warmup batches and their relative median absolute deviation. They are declared in `windows-protocol.json`. A warning does not remove a process or trigger retries. Every observation remains in the paired analysis; warnings prevent an unconditional claim that the experiment established steady state. `stability.json` additionally reports the measured last-three/first-three ratio for each process.

The Windows preparer requires an exact clean baseline commit and an explicitly supplied candidate commit. It reads both product and diagnostic inputs from Git blobs, so Windows checkout newline conversion cannot alter the pinned bytes. All 13 compiled source files must agree except for the exact reviewed guard and its two comments. It builds source-only libraries and one common probe under a new isolated output, pins SDK 10.0.402/runtime 10.0.13 and actual CLR/JIT/Regex identities, shuts down build servers, and runs the same paired supervisor. It does not modify the ordinary Worker or previously frozen product payloads.

```powershell
python .\build\performance\diagnostics\translation\reproduce-windows-fixtures.py `
  --baseline-repo C:\path\to\clean-baseline `
  --candidate-repo C:\path\to\clean-candidate `
  --candidate-commit FULL_CANDIDATE_COMMIT_SHA `
  --dotnet 'C:\path\to\dotnet.exe' `
  --packages C:\path\to\package-cache `
  --output C:\path\to\new-fixture-output `
  --pairs 20
```

Run the preparer from the explicitly pinned candidate checkout. Its generated dependency-restore project fetches Roslyn 5.9.0 into the supplied cache and checks the DLL hashes. `--prepare-only` is available for build validation. The separately scoped workflow publishes original successful and failed observations as a reviewable Actions artifact. Results from this Windows protocol must be reported separately from the completed Linux experiment and the historical/product measurements.

## Validation performed for these reproducibility files

The Linux `--prepare-only` entrypoint was executed against the clean recorded baseline with the pinned SDK/cache. Both generated translator projects and the common probe built successfully in Release, with no warnings or errors. No additional Linux timing experiment was started for that check. The numeric verifier recomputed all six classifications and adjusted intervals and matched the published process medians to the unchanged complete raw file.

The independent-fixture C# probe also compiled successfully in Release, with no warnings or errors. Its Windows preparer, manifest contract and exact-source gates received an independent read-only review; the Windows-specific entrypoint still requires execution in its declared Windows workflow. The public bundle contains no full original stdout, host/environment inventory or recorded absolute workspace paths, and contains no active `.csproj`, `.props` or `.targets` files.
