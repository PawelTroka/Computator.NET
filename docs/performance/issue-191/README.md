# Issue 191: reproducible performance investigation

## Result and baseline boundary

The verified **2.3.2.0 historical control** establishes a repeatable compilation-latency regression in the requested 3.0 candidate product source. It also establishes substantially faster cached evaluation, chart-grid sampling and several mathematical workloads. This is not evidence of a general slowdown in every operation.

**An authentic 2.4.0.0 distribution has not been obtained. This investigation does not relabel 2.3.2 as 2.4, and it does not close that part of [issue #191](https://github.com/PawelTroka/Computator.NET/issues/191).** The exact requested 2.4-versus-3.0 comparison remains pending an attributable original installer/archive. The baseline preparer supports an explicitly supplied archive, version, expected SHA256 and provenance, and checks the package's own assembly metadata before running.

The initial measured candidate is `fac8e24d94f811ec4e005c876b9caf9151b623fa`. Its product source is unchanged from the requested master `ba57b8f08c1014363d26e7b87249c0e51fe74688`; the additional commits introduce only the comparison harness. The focused translation candidate is `afa521ec577cdae2b5a3492bfe87fde1e5494b06`. Results for that proposed change are separate experiments, not differences between absolute times on unrelated runners.

The measurements and implementation are under review in [PR #235](https://github.com/PawelTroka/Computator.NET/pull/235). No merge or release is authorized by this investigation.

## Authenticity of the available historical control

The control is the unchanged [official v2.3.2 release ZIP](https://github.com/PawelTroka/Computator.NET/releases/download/v2.3.2/Computator.NET.v2.3.2.zip), GitHub asset `5646130`, 12,002,377 bytes. Both Windows sessions acquired the same bytes:

| Artifact | SHA256 |
|---|---|
| Official ZIP | `21a64b059d78176b975d066a6459e4467f9a523c997f57981ceca49485df8b2d` |
| Historical desktop EXE | `63fe05835b1b31ba16a4f2c7fd6101bda5f1a8dd3aba41eeef8e352f0862c290` |
| Historical Core DLL | `aefa901c609e40eafff49e5526e308d75557a82aeee2904db797659f88c58c2b` |
| Historical DataTypes DLL | `29ff67f8b09dacabd02e2662823bf5cd74d6f75447e3c213ffd704b658517c8b` |

No historical source rebuild substitutes for the shipped binary. The EXE, Core and DataTypes assembly/file versions are all `2.3.2.0`; target framework is `.NETFramework,Version=v4.6.1`, build configuration Retail, PE architecture AnyCPU/ILOnly with neither 32-bit-required nor 32-bit-preferred set. Actual benchmark and desktop target bitness was verified as 64-bit.

The public release/tag history, old source version metadata, public AppVeyor distribution history and Store catalog identified 2.3.2-family artifacts. A modern MSIX template bearing `2.4.0.0` was introduced during the .NET 10 migration and is not evidence of a historical Framework release. The release tag points at `56fed3363c07026d7fb290e40f852468dfe158ba`; the original binary's reproducible source-build identity is unknown and is labeled as such.

## Controlled protocol

The worker uses the same C# 7.3 workload source against the shipped desktop APIs, compiled Release/x64 for the old .NET Framework target and for .NET 10 Windows. It does not replace the desktop compiler with the portable engine. The full original historical package/configuration and the complete current publish are isolated in separate directories. Framework references also resolve from the GAC/shared framework, and native libraries can resolve from the conditioned extraction profile. The manifests pin the complete application/worker payloads, whose hashes are verified before and after measurement. The candidate source commit, supervisor SHA256 and actual runtime/version identities are recorded separately. The initial historical runs did not pin individual shared-runtime/JIT/BCL file hashes; the later focused modern diagnostic does add explicit CLR artifact hashes.

Each session contains 20 fresh process pairs per scenario. Pairs alternate AB/BA launch order and rotate scenario order. Processes run sequentially on the same runner; inner timed repetitions are reduced to one process median and are not counted as independent trials. The .NET Framework and .NET 10 products retain their packaged runtime/code-generation policies. Inherited runtime, GC, tiering, profiling and product-directory overrides are stripped. Optional tiering-off runs are explicitly separate diagnostics.

Cold means fresh process-local JIT/compiler caches. The OS filesystem cache is unflushed. Startup uses a fresh process after an explicitly retained conditioning launch that creates the profile and extracts native dependencies. It is neither an installation measurement nor a reboot/disk-cold measurement. The common observer requires the visible owned main window with the exact application title, six workflow tabs and a successful `WM_NULL` responsiveness check, and verifies normal shutdown.

Warm observations follow the recorded bounded warmup (at least one second, a minimum of three iterations, and at most 32 compilations for the legacy AppDomain lifetime). These are useful repeated-interaction observations, **not proof that all Roslyn/JIT/PGO activity has reached its asymptotic steady state**. Warmup traces and actual iteration counts are retained. In particular, the current compiler completed exactly three warmup compilations in all 160 initial candidate expression/script records before its five measured recompilations; the separate compiler-phase diagnostic examines later calls.

The two initial sessions used Windows Server 2025 x64, four vCPUs, approximately 16 GiB RAM and the High performance power plan. Session 1 reported AMD EPYC 7763; session 2 Intel Xeon 8370C. The old desktop ran on Framework CLR 4.8.9345.0 (release 533509); the current worker ran on .NET 10.0.13 with SDK 10.0.402. The differences between hosts are a replication check. Absolute observations are never pooled across them.

## Workloads and correctness

| Scenario | Representative workload and endpoint |
|---|---|
| Real expression | `sin(x)+cos(x)+x*x/4`; evaluator creation, first compile, first call, whole first interaction, 65,536 cached typed/public calls, repeated compile, changed custom function, 32 Simpson integrations with 1,024 steps |
| Complex expression | `sin(z)+z*z`; first phases, repeated compile and 65,536 cached complex evaluations |
| Surface/grid | `sin(x)*cos(y)+x*y`; first phases, repeated compile and a 64×64 sample grid; rendering/GPU work excluded |
| Script | Sum `k*k` for `k=1..1000`, with one invariant output callback; exact result `333833500`; first phases, repeated compilation, first execution and 32 cached executions |
| Math | 16,384-operation batches of elementary functions, managed special functions and native GSL `gammaP`, checked against independent identities/reference values |
| Translation | First translation, 256 repeated translations and 256 empty fragments; exact output stability and a numerical oracle |
| Desktop startup | Fresh-process latency to the same usable main-window condition, with actual target bitness/runtime and normal shutdown verified |

The 39 metrics overlap in places: first-interaction latency spans evaluator construction through the first result after the separately timed engine initialization, and includes compilation. Initialization is observed in multiple scenarios. Counts of regression labels must not be presented as counts of independent user problems. Mathematical correctness, output cardinality, finite results, tolerances, workload signatures, operation counts, timer resolution and child process cleanup are part of the acceptance contract. Failed invocations and their raw logs remain in the result; there are no silent retries or outlier deletions.

The older [`Benchmark.cs`](../../../Computator.NET.Core/Benchmarking/Benchmark.cs) was inspected. It uses Stopwatch, elapsed whole milliseconds, random-size approximately 80 MB array allocations and an aggregate mathematical-function loop. Those routines can still serve their original interactive benchmark UI. They do not provide a controlled release identity, fresh-process pairing, precise first/warm endpoints or independent statistical units. The new harness reuses Stopwatch and the shipped mathematical APIs while adding those controls; it does not turn retained process memory or GC counts into an allocation-throughput claim.

## Initial cross-version findings

Both sessions in [run 39143281109](https://github.com/PawelTroka/Computator.NET/actions/runs/39143281109) completed all 140 scenario pairs/280 children. All 560 children across the two sessions reported correctness and all post-run artifact checks passed. **Each whole matrix is nevertheless INVALID**, because all 20 candidate startup records in that session failed the declared runtime-prefix contract. Each of the other 38 metrics has 20 valid pairs. Their intervals still account for all 39 declared comparisons.

The startup observer used the CLR module's display `FileVersion`, which on .NET 10 contains commas and a commit suffix; the contract expects a dotted numeric version. The common main-window readiness and shutdown checks passed, but the failed runtime contract is not waived. The observer was corrected to record invariant numeric file-version parts and retain the original display string. The old records are preserved unchanged; startup is rerun with the corrected observer.

Ratios below are current/historical; below 1 means less latency. Parenthesized ranges are multiplicity-adjusted intervals. Point estimates are geometric means of paired process-median ratios; they need not equal the quotient of displayed marginal medians.

| Metric | Session 1 ratio (adjusted interval) | Session 2 ratio (adjusted interval) | Session 2 median ms, historical → current |
|---|---:|---:|---:|
| Real first compilation | 12.127 (7.338–15.273) | 10.449 (6.740–14.034) | 201.824 → 2,725.018 |
| Real repeated compilation | 5.340 (4.756–5.832) | 5.006 (3.657–5.932) | 108.449 → 613.605 |
| Changed custom-function compilation | 2.992 (2.545–3.358) | 2.821 (2.151–3.413) | 109.954 → 355.027 |
| Complex first compilation | 15.896 (14.329–16.818) | 15.126 (14.720–15.554) | 178.386 → 2,681.031 |
| Script repeated compilation | 4.128 (3.682–4.734) | 3.941 (3.688–4.142) | 182.973 → 729.254 |
| Cached typed real, 65,536 calls | 0.170 (0.162–0.183) | 0.201 (0.181–0.237) | 5.102 → 0.972 |
| Sample grid, 4,096 points | 0.208 (0.194–0.247) | 0.194 (0.192–0.195) | 0.315 → 0.061 |
| Cached script, 32 executions | 0.267 (0.233–0.309) | 0.312 (0.297–0.332) | 0.551 → 0.168 |
| Elementary math, 16,384 operations | 0.342 (0.330–0.385) | 0.327 (0.303–0.370) | 1.730 → 0.565 |
| Repeated translation, 256 calls | 16.402 (14.477–18.059) | 12.296 (10.372–14.700) | 2.978 → 37.723 |

Eighteen metric labels consistently regress in both sessions; 16 consistently improve, three are inconclusive in both, one differs between regression and inconclusive (first complex evaluation), and startup is invalid. Native GSL has a roughly 1.135 paired ratio, but the adjusted intervals cross the predeclared 1.10 practical threshold; this is not an established practical regression. All 39 results, including inconclusive and invalid metrics, appear in the unchanged [session 1 report](windows-session-1-report.md) and [session 2 report](windows-session-2-report.md).

## Statistics and interpretation limits

The estimate is `exp(mean(log(candidate process median / baseline process median)))`. Each interval uses 50,000 deterministic paired bootstrap draws. Bonferroni adjustment across all 39 declared metrics gives approximately 95% family confidence (99.87179% marginal intervals, about 32 expected draws in each adjusted tail). Intervals are approximate and still depend on independent process pairs and reasonably stable hosts.

A practical regression requires the entire adjusted interval above 1.10; improvement requires it below `1/1.10`, a reciprocal symmetric threshold. An interval crossing these boundaries is inconclusive, not proof of equivalence. Fewer than 15 pairs, inadequate adjusted bootstrap-tail resolution or an observation within ten timer ticks suppresses inferential labels. The virtual hosts were paired and counterbalanced, but frequency, temperature and hypervisor contention cannot be fixed perfectly.

The cold compilation gap cannot be explained by the first numerical evaluation: session 2's first real call changed from approximately 0.150 to 0.272 ms, while first compilation added about 2.5 seconds. The measured repeated translation cost is about 0.147 ms per call in that session, far smaller than the roughly 614 ms repeated compilation latency. A small translation optimization cannot plausibly eliminate the dominant compilation gap.

## Investigation and focused change

Both generations generate a class containing the full constants/mathematics/statistics/integration/root-finding wrapper for each expression; scripts also include matrix and scripting wrappers. The current compiler parses and emits that source using Roslyn Release optimization, references the trusted platform assembly set plus product references, and loads a uniquely named collectible program. The older compiler uses CodeDOM and a much smaller explicit reference list without an explicit optimizer option. These are observed implementation differences, not proof that the full measured cost is unavoidable.

The current compiler already caches portable metadata references. It would therefore be incorrect to attribute its warm cost simply to rereading every reference DLL. Separate sampled-thread traces and compiler-phase measurements are being used to distinguish parsing/reference setup, symbolic-power resolution, emission and loading. Sampled .NET thread stacks include time when a thread is not executing on CPU; their proportions are not precise CPU percentages. See [Microsoft's trace documentation](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace).

The focused proposed change skips the final literal-restoration regex when the parser found no protected tokens. The per-call GUID makes the regex pattern unique; in the empty-token case it cannot restore anything. All existing lexing, comment handling, notation translation, custom-name protection and handler passes still execute. This removes a deterministic approximately 4,112-byte allocation per ordinary/empty call in the isolated .NET 10 probe. It does not introduce an expression cache or relax correctness.

The complete initial Linux same-runtime experiment recorded improved ordinary and empty translations, but a regression in its literal-rich control. Its fixed within-process fixture order and continuing JIT/PGO changes require a follow-up with one fixture per fresh process and longer reported warmup. That unfavorable result is retained and prevents an unqualified speedup claim. The Windows packaged-worker before/after diagnostic and independently isolated fixture diagnostic are separate datasets. Final acceptance and measured effect are documented in the PR as those runs complete.

Deterministic regression coverage exercises alternating ordinary/empty/literal fragments on one compiler, empty fragments with separate custom power names in both binding modes, and execution of a translated script without literal text. Ordinary unit tests do not impose noisy stopwatch thresholds. Correctness tests and performance evidence serve different purposes.

## Reproduce and audit

Use a disposable Windows profile and follow the complete commands in [the harness README](../../../build/performance/README.md). The preparer downloads and verifies the historical package, publishes the current payload, conditions startup and freezes a manifest. Then run:

```powershell
python build/performance/compare.py run --manifest artifacts/performance-input/manifest.json --output artifacts/performance-results --pairs 20 --timeout 120 --bootstrap 50000 --seed 191
```

The full raw data and child logs remain in the original Actions artifacts (retention is finite). The compact committed tables preserve every valid process-median pair and every declared metric's statistical result without copying host paths or arbitrary process output. They are derived report data, not substitutes for the original validation/provenance records. Recompute their estimates, adjusted intervals and classifications with:

```powershell
python docs/performance/issue-191/verify_measurements.py
```

For full contract validation, download the original artifact and use `compare.py analyze --input <raw.json> --output <new-analysis-directory>`. The original invalid startup records must stay invalid; do not edit their runtime strings and present them as new observations.

| Initial dataset | Actions artifact ID | SHA256 of artifact ZIP | SHA256 of original raw.json |
|---|---:|---|---|
| Session 1 | 12143277419 | `45dcc538cb69175a489d96ac4e7fa816434cc735842904db808cd6392e5d6a42` | `bbcb8e21845b910de48f90b5143f9d5ba6725c722e3d3a5b40e8964bc997e1c9` |
| Session 2 | 12142274707 | `f5b7c668c8fd342b54f00e18aa05dd86f1a2786f3ab98f3111cfdd14710cc385` | `e61851cf622154c92aa9204408b251c53eefb7e418a012a5e5c35dbb4d06e6d9` |

## Outstanding scope

An authenticated 2.4 package is still required for the exact requested baseline. Web HTTP/browser latency, full GPU chart rendering, installation, reboot/disk-cold startup and long-running multi-user workloads are outside this desktop comparison. The measurements support a compilation responsiveness concern for the verified control; they do not justify removing the reviewed numerical/language correctness improvements or asserting that every architectural cost can be eliminated. This PR remains unmerged for owner review.
