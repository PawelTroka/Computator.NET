# Seven-PR integration evidence

The main findings, repaired defects, exact final candidate and native-platform validation are recorded in [integration-report.md](integration-report.md). The `results/` directory contains compact CI evidence, resource-reader regressions, runtime records and selected desktop screenshots. Each result retains the identity of the commit or binary snapshot actually tested.

## Supplemental numerical evidence

This bundle records **30 supplemental .NET checks** and **116 native Mono checks** for Computator.NET PRs **#304–#310**, with source and reproduction commands. Every recorded check passed; neither run skipped a case. It supplements the repository's full suites and the native desktop workflows tracked in [temporary validation PR #312](https://github.com/PawelTroka/Computator.NET/pull/312).

## Exact inputs

| Identity | SHA |
| --- | --- |
| Master base used for the integration | `fe1aba49b5f9538355ffaaa3d1683f69a23d2d9c` |
| Published candidate used to reproduce the 30 .NET checks | `189d28c0ec7acbf77d380f14f95ef450e7bfbd84` |
| That candidate's tree | `58b013e7e3f81ec62858a481860b73de0a88a7ff` |
| Historical Mono snapshot checkout | `897de969c2345ba4f3a12c87959f789f4c584143` |
| Historical snapshot checkout tree | `270b1fb34463fbb6dde9d9748470e327f49415d6` |
| Final candidate head, including inventory and resource-helper corrections | `f5f1fd0697e0665f56f3d2e4fd516f49860c0497` |
| Final CI merge commit | `a854ee8d9cdd9f598b6c9fe4c00471faf3c5f290` |
| Final candidate and merge tree | `40cc4713ef0bdb6b49a5cc32fab35b49e17997ce` |

`manifest.json` records all seven exact input PR heads for the original observation, the preceding candidate and the final candidate. Only #306's head changes across these revisions. From the original probe tree to the final tree, the only changed files are `Computator.NET.slnx` (two solution-inventory entries) and `build/validation/LocalizationResourceContract.cs` (the test helper now preserves literal CRLF when reading source resources). Production numerical sources are unchanged. `results/historical-probes-to-final.diff` contains the complete comparison; `results/candidate-inventory-only.diff` preserves the earlier comparison ending at `897de969…`.

The original .NET run used local commit `8b03bf130031c5a91dbfb80fe26a45a4a2173f64`, whose complete tree exactly matches published candidate `189d28c…`. The reproduction script selects that published commit and verifies the tree before testing.

### Mono provenance qualification

The recorded native Mono probe used a snapshot of already-built net48 binaries. The checkout was at `897de969…` (tree `270b1fb3…`) when those files were copied, but the snapshot date is **not a claim that they were freshly built at that commit**. Their production source is equivalent to the final candidate: the intervening changes from the previously built tree comprise the two SLNX inventory entries and the localization test-helper correction described above. All copied DLL hashes are recorded in the manifest. The 116-check observation remains attached to that historical snapshot; it is not a fresh execution at the final merge.

The Mono reproduction command is now pinned to final merge `a854ee8d…`: it performs a fresh build, records its new DLL hashes, and executes the same harness. This updated pin has not been executed as part of this bundle revision. Fresh build hashes can differ from the historical snapshot because build paths or assembly metadata differ; the source and input tree remain explicit.

## Coverage

| Probe | Checks | Scope |
| --- | ---: | --- |
| .NET cross-feature fixture | 30 | Twelve combined expressions, one script, and two German/Czech culture cases, each in host-reference and complete emitted-numerics modes. |
| Native Mono numerical cases | 32 | Extreme secant arithmetic and trapezoidal overflow/subnormal boundaries, in host and complete emitted implementations. |
| Native Mono compiled TSL | 10 | Literal powers, secant, integration, Euclid and `out` arguments through actual compilation. |
| Native Mono Debye | 68 | Supported orders, normalized values/zero limits and unsupported inputs across host, emitted, precompiled-expression and precompiled-script surfaces. |
| Native Mono runtime/setup | 6 | Actual Mono identity, emitted-source compilation, GSL initialization and surface loading. |

The fixtures use independent exact constants, linear antiderivatives, known polynomial roots and independently calculated normalized Debye reference values. They execute the production algorithms and emitted source; they do not substitute alternate numerical implementations.

`python3 verify_debye_oracles.py` independently verifies the six Debye constants using exact rational Bernoulli-series arithmetic. The 60-term and 80-term sums differ by less than `1e-48`, and both round to every stored binary64 reference value. This check uses only Python's standard library and does not call the production functions or GSL.

## Prerequisites

- Bash, Git, Python 3 and **.NET SDK 10.0.403**. Set `COMPUTATOR_DOTNET` to select a particular SDK installation when necessary.
- A local Computator.NET clone with the recorded commits available. NuGet restore needs access to the repository's pinned packages.
- For native Mono: Linux x64, `mono-devel`/`mcs`, `xvfb`, `xauth`, `libgdiplus`, `libgsl-dev`, and `fonts-dejavu-core`. The recorded environment used **Mono 6.8.0.105** on Ubuntu 24.04 and system GSL `libgsl.so.27.0.0`.
- Local process and IPC access for VSTest and Xvfb. No application credentials or user settings are required; the Mono run uses a separate test profile.

## Reproduce

Fetch the two fixed commits into an existing clone if they are not present:

```bash
git -C /path/to/Computator.NET fetch origin 189d28c0ec7acbf77d380f14f95ef450e7bfbd84
git -C /path/to/Computator.NET fetch origin a854ee8d9cdd9f598b6c9fe4c00471faf3c5f290
```

Run from this bundle's directory, choosing output directories that do not yet exist:

```bash
bash reproduce.sh dotnet /path/to/Computator.NET /tmp/computator-dotnet-reproduction
bash reproduce.sh mono /path/to/Computator.NET /tmp/computator-mono-reproduction
```

Each command creates a detached temporary worktree at its recorded commit, verifies the tree and fixture hashes, runs the probe and enforces the expected passing count. It removes that temporary worktree when finished; the clone's working branch is not changed. Results, summaries, logs and reproduction metadata remain in the chosen output directory. The Mono command also records hashes of the freshly built DLLs.

The essential .NET command is `dotnet test Computator.NET.Engine.Tests/Computator.NET.Engine.Tests.csproj -c Release --filter FullyQualifiedName~SevenPrCrossFeatureTests` after copying the supplied fixture into that temporary project's source directory. The Mono command builds the repository's opt-in net48 graph, compiles the supplied C# harness with `mcs` against its actual DLLs, and runs it with `mono` under an isolated Xvfb display. The complete flags and configuration-copy step are in `reproduce.sh`.

## Bundle contents

- `manifest.json`: exact commits, PR heads, trees, observed counts, prerequisites and binary/fixture hashes.
- `SevenPrCrossFeatureTests.cs` and `MonoNumericalProbe.cs`: the exact executed supplemental sources.
- `reproduce.sh`: isolated reproduction commands with expected-result checks.
- `verify_debye_oracles.py`: independent exact-arithmetic verification of the six Debye reference constants.
- `results/`: compact named outcomes, candidate comparisons and supporting integration evidence. Each observation retains its own commit identity.
- `SHA256SUMS`: hashes of this bundle's files.

No compiled DLLs or large diagnostic archives are included. Native Windows Find/Replace and GUI/resource acceptance are reported with their own workflow identities in the integration report and supporting result files.
