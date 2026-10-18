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
| real / compile.custom.changed.ms | 1 | 109.954 | 355.027 | +236.277 | 2.8212× | 2.1507–3.4134× | regression |
| real / compile.first.ms | 1 | 201.824 | 2725.02 | +2519.42 | 10.4487× | 6.7397–14.0337× | regression |
| real / compile.same.warm.ms | 1 | 108.449 | 613.605 | +490.28 | 5.0058× | 3.6570–5.9321× | regression |
| real / engine.initialize.ms | 1 | 102.019 | 79.1853 | -22.6578 | 0.7787× | 0.7366–0.8340× | improvement |
| real / evaluate.first.ms | 1 | 0.14985 | 0.2715 | +0.1212 | 1.8498× | 1.6542–2.0832× | regression |
| real / evaluate.public.batch.ms | 65536 | 5.8658 | 1.65115 | -4.2146 | 0.3191× | 0.2760–0.4360× | improvement |
| real / evaluate.typed.batch.ms | 65536 | 5.1024 | 0.9718 | -4.13735 | 0.2006× | 0.1812–0.2370× | improvement |
| real / evaluator.create.ms | 1 | 3.1788 | 1.229 | -1.98395 | 0.3808× | 0.3539–0.4202× | improvement |
| real / first.interaction.ms | 1 | 205.197 | 2726.65 | +2517.02 | 10.3048× | 6.6493–13.8537× | regression |
| real / integral.simpson.batch.ms | 32 | 2.50525 | 0.90555 | -1.60585 | 0.3800× | 0.3441–0.4401× | improvement |
| complex / compile.first.ms | 1 | 178.386 | 2681.03 | +2493.07 | 15.1257× | 14.7195–15.5542× | regression |
| complex / compile.same.warm.ms | 1 | 107.867 | 608.064 | +493.559 | 5.4654× | 4.5552–5.9870× | regression |
| complex / engine.initialize.ms | 1 | 100.961 | 77.4855 | -23.8664 | 0.7580× | 0.7231–0.8021× | improvement |
| complex / evaluate.first.ms | 1 | 0.4635 | 0.65025 | +0.17715 | 1.2550× | 0.8460–1.4769× | inconclusive |
| complex / evaluate.typed.batch.ms | 65536 | 7.8094 | 2.2138 | -5.59755 | 0.2810× | 0.2634–0.2921× | improvement |
| complex / evaluator.create.ms | 1 | 3.16145 | 1.18055 | -2.01265 | 0.3761× | 0.3512–0.4074× | improvement |
| complex / first.interaction.ms | 1 | 182.087 | 2683.02 | +2490.94 | 14.7846× | 14.2983–15.2450× | regression |
| surface / chart.grid.sample.ms | 4096 | 0.31495 | 0.06135 | -0.2539 | 0.1936× | 0.1915–0.1952× | improvement |
| surface / compile.first.ms | 1 | 179.527 | 2713.06 | +2518.21 | 13.4651× | 9.4539–15.9343× | regression |
| surface / compile.same.warm.ms | 1 | 109.044 | 597.463 | +489.972 | 4.8876× | 3.1290–5.7946× | regression |
| surface / engine.initialize.ms | 1 | 101.176 | 78.0202 | -23.5448 | 0.7692× | 0.7411–0.8027× | improvement |
| surface / evaluate.first.ms | 1 | 0.152 | 0.27 | +0.11885 | 1.8452× | 1.6993–2.0847× | regression |
| surface / evaluator.create.ms | 1 | 3.16195 | 1.24715 | -1.8894 | 0.4164× | 0.3843–0.4602× | improvement |
| surface / first.interaction.ms | 1 | 182.837 | 2714.62 | +2516.65 | 13.2479× | 9.5177–15.6714× | regression |
| script / compile.first.ms | 1 | 283.146 | 2744.34 | +2445.16 | 8.4245× | 4.7702–10.1067× | regression |
| script / compile.same.warm.ms | 1 | 182.973 | 729.254 | +544.197 | 3.9407× | 3.6875–4.1421× | regression |
| script / engine.initialize.ms | 1 | 100.042 | 77.29 | -22.9579 | 0.7734× | 0.7261–0.8387× | improvement |
| script / evaluator.create.ms | 1 | 22.9067 | 21.483 | -1.7896 | 0.9497× | 0.8760–1.0438× | inconclusive |
| script / first.interaction.ms | 1 | 323.855 | 2786.38 | +2445.32 | 7.5428× | 4.3554–8.9911× | regression |
| script / script.first.ms | 1 | 17.089 | 20.4032 | +3.4336 | 1.1006× | 0.7644–1.2372× | inconclusive |
| script / script.repeated.batch.ms | 32 | 0.55135 | 0.1679 | -0.38305 | 0.3122× | 0.2966–0.3319× | improvement |
| math / engine.initialize.ms | 1 | 100.946 | 77.2266 | -23.3336 | 0.7638× | 0.7343–0.7849× | improvement |
| math / math.elementary.batch.ms | 16384 | 1.7302 | 0.5646 | -1.16845 | 0.3273× | 0.3029–0.3696× | improvement |
| math / math.native.gsl.batch.ms | 16384 | 1.4716 | 1.5262 | +0.0566 | 1.1348× | 1.0295–1.3240× | inconclusive |
| math / math.special.batch.ms | 16384 | 4.71285 | 1.78845 | -2.9208 | 0.3813× | 0.3594–0.4098× | improvement |
| translation / translation.empty.batch.ms | 256 | 0.2362 | 2.3507 | +2.1145 | 9.0727× | 6.6778–12.0864× | regression |
| translation / translation.first.ms | 1 | 54.6549 | 234.173 | +178.516 | 4.2333× | 4.0562–4.4123× | regression |
| translation / translation.repeated.batch.ms | 256 | 2.97795 | 37.7234 | +34.7816 | 12.2960× | 10.3717–14.6998× | regression |
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
