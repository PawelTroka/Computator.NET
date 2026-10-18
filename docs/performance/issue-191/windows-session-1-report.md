# Computator.NET paired performance comparison

Validity: **INVALID — failures or missing observations**. Execution mode: `packaged-runtime-defaults`.

## Baselines and conditions

- **Baseline:** 2.3.2.0; Release; commit `unknown (Official shipped binary is authoritative; a source tag is not proof of build reproducibility.)`; source: https://github.com/PawelTroka/Computator.NET/releases/download/v2.3.2/Computator.NET.v2.3.2.zip.
- **Candidate:** 3.0.0.0; Release; commit `fac8e24d94f811ec4e005c876b9caf9151b623fa`; source: PawelTroka/Computator.NET.

- Requested process pairs per scenario: **20**; inference requires at least 15.
- fresh processes; operating-system file cache and native-library extraction caches are not reset.
- Trials run sequentially, counterbalancing baseline/candidate launch order and rotating scenario order.
- Each process contributes the median of its inner repetitions; inner repetitions are not independent samples.
- Milliseconds below describe the complete timed observation/batch. Operations per observation are fixed by the manifest.

## Measurements

| Scenario / metric | Ops | Baseline median ms | Candidate median ms | Paired Δ ms | Paired ratio | Adjusted interval | Classification |
|---|---:|---:|---:|---:|---:|---|---|
| real / compile.custom.changed.ms | 1 | 125.4 | 392.071 | +261.772 | 2.9924× | 2.5446–3.3581× | regression |
| real / compile.first.ms | 1 | 217.507 | 3192.87 | +2978.35 | 12.1267× | 7.3375–15.2730× | regression |
| real / compile.same.warm.ms | 1 | 120.86 | 679.357 | +550.086 | 5.3396× | 4.7563–5.8320× | regression |
| real / engine.initialize.ms | 1 | 105.712 | 87.9955 | -17.844 | 0.8285× | 0.7881–0.8666× | improvement |
| real / evaluate.first.ms | 1 | 0.1569 | 0.3071 | +0.14315 | 1.8094× | 1.4891–2.0013× | regression |
| real / evaluate.public.batch.ms | 65536 | 6.61225 | 2.05765 | -4.5255 | 0.3263× | 0.2954–0.3783× | improvement |
| real / evaluate.typed.batch.ms | 65536 | 6.3101 | 1.0696 | -5.2469 | 0.1699× | 0.1623–0.1829× | improvement |
| real / evaluator.create.ms | 1 | 3.5481 | 1.45915 | -2.06855 | 0.4179× | 0.3595–0.4719× | improvement |
| real / first.interaction.ms | 1 | 221.345 | 3194.93 | +2976.63 | 11.9454× | 7.2813–15.0370× | regression |
| real / integral.simpson.batch.ms | 32 | 2.9198 | 1.0588 | -1.855 | 0.3997× | 0.3536–0.4700× | improvement |
| complex / compile.first.ms | 1 | 195.468 | 3212.36 | +3004.71 | 15.8958× | 14.3294–16.8184× | regression |
| complex / compile.same.warm.ms | 1 | 120.637 | 690.844 | +570.771 | 5.6611× | 5.3357–5.9566× | regression |
| complex / engine.initialize.ms | 1 | 104.904 | 87.2917 | -18.1694 | 0.8177× | 0.7791–0.8548× | improvement |
| complex / evaluate.first.ms | 1 | 0.5094 | 0.71775 | +0.2011 | 1.3629× | 1.1611–1.5385× | regression |
| complex / evaluate.typed.batch.ms | 65536 | 7.8313 | 3.03535 | -4.80145 | 0.3870× | 0.3833–0.3898× | improvement |
| complex / evaluator.create.ms | 1 | 3.46745 | 1.4573 | -1.99565 | 0.4159× | 0.3830–0.4429× | improvement |
| complex / first.interaction.ms | 1 | 199.803 | 3214.88 | +3003.15 | 15.5756× | 14.0913–16.4735× | regression |
| surface / chart.grid.sample.ms | 4096 | 0.3458 | 0.0679 | -0.278 | 0.2085× | 0.1937–0.2473× | improvement |
| surface / compile.first.ms | 1 | 193.017 | 3174.72 | +2969.77 | 14.5569× | 9.6408–18.7705× | regression |
| surface / compile.same.warm.ms | 1 | 121.77 | 603.629 | +481.612 | 4.8298× | 4.1985–5.3170× | regression |
| surface / engine.initialize.ms | 1 | 104.422 | 86.8879 | -17.3782 | 0.8263× | 0.7778–0.8760× | improvement |
| surface / evaluate.first.ms | 1 | 0.15795 | 0.30105 | +0.14055 | 1.9038× | 1.7785–2.0237× | regression |
| surface / evaluator.create.ms | 1 | 3.40785 | 1.46595 | -1.89505 | 0.4319× | 0.4080–0.4582× | improvement |
| surface / first.interaction.ms | 1 | 196.642 | 3176.43 | +2968 | 14.3233× | 9.5316–18.4683× | regression |
| script / compile.first.ms | 1 | 311.125 | 3207.16 | +2858.66 | 9.5162× | 8.0636–10.3560× | regression |
| script / compile.same.warm.ms | 1 | 206.931 | 858.311 | +652.106 | 4.1276× | 3.6823–4.7340× | regression |
| script / engine.initialize.ms | 1 | 104.923 | 85.4464 | -19.4731 | 0.8107× | 0.7789–0.8392× | improvement |
| script / evaluator.create.ms | 1 | 24.7389 | 23.2338 | -1.92585 | 0.9140× | 0.8601–0.9618× | inconclusive |
| script / first.interaction.ms | 1 | 354.782 | 3254.62 | +2856.49 | 8.5124× | 7.2874–9.2337× | regression |
| script / script.first.ms | 1 | 19.0607 | 23.3636 | +4.2394 | 1.2023× | 1.0890–1.3259× | inconclusive |
| script / script.repeated.batch.ms | 32 | 0.621 | 0.16195 | -0.44775 | 0.2666× | 0.2327–0.3086× | improvement |
| math / engine.initialize.ms | 1 | 104.363 | 87.7035 | -16.3235 | 0.8392× | 0.7795–0.8989× | improvement |
| math / math.elementary.batch.ms | 16384 | 2.0172 | 0.6667 | -1.3486 | 0.3416× | 0.3303–0.3855× | improvement |
| math / math.native.gsl.batch.ms | 16384 | 1.69535 | 1.9198 | +0.2184 | 1.1372× | 1.0958–1.1837× | inconclusive |
| math / math.special.batch.ms | 16384 | 4.96125 | 1.8062 | -3.14545 | 0.3666× | 0.3391–0.3939× | improvement |
| translation / translation.empty.batch.ms | 256 | 0.3253 | 2.6978 | +2.38525 | 8.3968× | 6.7992–10.7110× | regression |
| translation / translation.first.ms | 1 | 60.146 | 273.849 | +213.234 | 4.3967× | 4.0563–4.5814× | regression |
| translation / translation.repeated.batch.ms | 256 | 3.11975 | 51.5115 | +48.4574 | 16.4019× | 14.4773–18.0585× | regression |
| startup / desktop.main.ready.ms | 1 | — | — | — | — | — | invalid (0/20 complete pairs) |

## Interpretation

Ratios are candidate/baseline; values above 1 mean higher latency. The point estimate is the geometric mean of paired process-median ratios. The displayed baseline and candidate milliseconds are medians across processes; their quotient need not equal the paired ratio.

Intervals use 50,000 seeded paired-bootstrap draws, with Bonferroni adjustment across 39 declared comparisons for an approximate 95.0% family confidence level. Each interval has nominal 99.87179% marginal confidence. Bootstrap intervals remain approximate, especially with few process pairs.

A regression requires the entire interval above 1.100×; an improvement requires the entire interval below 0.909×. The practical band is reciprocal, so the threshold is symmetric when baseline and candidate are exchanged. `within-practical-band` means the complete interval is inside those boundaries; `inconclusive` does not prove equivalence. Fewer than 15 pairs, fewer than 20 expected bootstrap draws per adjusted tail, or observations within ten timer ticks suppress inferential labels.

Failures are retained and invalidate all comparisons for the affected scenario; successful pairs are never silently substituted for a complete run. Runtime, bitness, operation counts, metric completeness and correctness must all match their declared contracts. A declared source/version is provenance supplied by the person preparing the manifest; SHA256 verification establishes binary identity, not source authenticity.

Fresh processes reset process-local JIT/compiler caches. They do not create a cold disk or reset the operating system file cache. Thermal state, host contention and power management can still affect a run. Diagnostic tiering-off results require separate interpretation from the application defaults.

## Invalid observations

- startup pair 0 candidate: Invocation has recorded validation/launch errors
- startup pair 1 candidate: Invocation has recorded validation/launch errors
- startup pair 10 candidate: Invocation has recorded validation/launch errors
- startup pair 11 candidate: Invocation has recorded validation/launch errors
- startup pair 12 candidate: Invocation has recorded validation/launch errors
- startup pair 13 candidate: Invocation has recorded validation/launch errors
- startup pair 14 candidate: Invocation has recorded validation/launch errors
- startup pair 15 candidate: Invocation has recorded validation/launch errors
- startup pair 16 candidate: Invocation has recorded validation/launch errors
- startup pair 17 candidate: Invocation has recorded validation/launch errors
- startup pair 18 candidate: Invocation has recorded validation/launch errors
- startup pair 19 candidate: Invocation has recorded validation/launch errors
- startup pair 2 candidate: Invocation has recorded validation/launch errors
- startup pair 3 candidate: Invocation has recorded validation/launch errors
- startup pair 4 candidate: Invocation has recorded validation/launch errors
- startup pair 5 candidate: Invocation has recorded validation/launch errors
- startup pair 6 candidate: Invocation has recorded validation/launch errors
- startup pair 7 candidate: Invocation has recorded validation/launch errors
- startup pair 8 candidate: Invocation has recorded validation/launch errors
- startup pair 9 candidate: Invocation has recorded validation/launch errors

Raw worker output, stderr, individual timings, binary hashes, host metadata, controls and all failed invocations are retained in `raw.json` and `child-logs/`. `analysis.json` also includes process medians and milliseconds per operation.
