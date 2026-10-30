# Computator.NET seven-PR integration validation

## Status

**Validation in progress.** The final combined source passes the local portable suite and native Mono desktop acceptance. Native Windows and repository CI completion are still required. This document will be updated with the exact workflow outcomes before the temporary validation PR is closed.

No contribution PR has been merged or retargeted. The seven original PRs remain independent drafts against master. Fixes were appended to their existing branches without force pushes, preserving review history.

## Reproducible candidate

| Identity | Commit or tree |
| --- | --- |
| Master base | `fe1aba49b5f9538355ffaaa3d1683f69a23d2d9c` |
| Final candidate head | `76bb146dc2f305c48c80013f3089e66a794d5c35` |
| GitHub CI merge | `897de969c2345ba4f3a12c87959f789f4c584143` |
| Candidate and CI merge tree | `270b1fb34463fbb6dde9d9748470e327f49415d6` |

The temporary draft [PR #312](https://github.com/PawelTroka/Computator.NET/pull/312) targets `validation/seven-pr-base-20261030`, pinned at the master base above. Its head is `validation/seven-pr-integration-20261030`. The complete integration contains all seven current contribution heads:

| PR | Contribution | Exact included head |
| --- | --- | --- |
| [#304](https://github.com/PawelTroka/Computator.NET/pull/304) | Numeric-expression power operands | `ceedea70c873ae4e6ac6072f739541ecdcc0d502` |
| [#305](https://github.com/PawelTroka/Computator.NET/pull/305) | Stable secant arithmetic | `d626208fb632834502d4153ec157cc3eec836e95` |
| [#306](https://github.com/PawelTroka/Computator.NET/pull/306) | German/Czech localization and resource-runtime checks | `3b83f4d88541e2cf497abedbb709ac05dc662945` |
| [#307](https://github.com/PawelTroka/Computator.NET/pull/307) | Debye supported-order behavior | `05fca5a6bb6f2f7a534825d026b48eb9786a02ce` |
| [#308](https://github.com/PawelTroka/Computator.NET/pull/308) | Trapezoidal finite arithmetic | `0b5c9930d59a4371d62061e27016dc150f4ae608` |
| [#309](https://github.com/PawelTroka/Computator.NET/pull/309) | Euclid utilities, completion and reference | `0ea6da7f804c06b502b174d38a005e9bfcb5de45` |
| [#310](https://github.com/PawelTroka/Computator.NET/pull/310) | Desktop Find/Replace | `7a944e5ba581c95a75a61efe4049804a2c66839e` |

The source-only evidence branch descends from the exact CI merge, retaining that commit for reproduction. Evidence files are outside the tested candidate and the seven contribution branches.

## Reproduced defects and repairs

### Trapezoidal integration: a finite interval could disappear

The original #308 addressed overflow when adding very large endpoint samples. A separate finite-sum path still evaluated the panel width divided by two before multiplying it by the endpoint sum. That ordering loses subnormal interval widths.

For a constant integrand `Math.ScaleB(1, 1022)`, bounds `0` and `double.Epsilon`, and one panel, the exact representable answer is `Math.ScaleB(1, -52)` (approximately `2.220446049250313e-16`). The old path returned zero. Widths of three and five minimum subnormal units also produced substantial relative error.

The appended #308 fix multiplies the full subnormal width by the finite endpoint average, in both the host and complete emitted implementation. Six new host/emitted regression cases failed before the change and pass afterward; they also exercise signs and reversed bounds. The independent #308 Engine suite passed 1,466 tests, including 28 trapezoidal cases.

### Euclid completion: overloads produced duplicate captions

The original #309 had three failed workflows: [native Windows/API validation](https://github.com/PawelTroka/Computator.NET/actions/runs/40979466103/job/123815597956), [real web custom-functions validation](https://github.com/PawelTroka/Computator.NET/actions/runs/40979466154/job/123815597692) and [release evidence](https://github.com/PawelTroka/Computator.NET/actions/runs/40979466211/job/123815597938). At merge `3ba47f734ff7ba9710aca6db1d78b312d45793c6`, their API assertions found 979 unique expression entries out of 983 and 1,323 unique scripting entries out of 1,331. Both Int32 and Int64 overloads were displayed with the alias `integer`, while some collection captions also lost their argument types. Compact excerpts are retained in `results/original-euclid-api-failures.json`.

The appended #309 fix preserves every overload and insertion expression while distinguishing `int` from `integer` and retaining collection element types in Euclid captions. All 37 overloads now have distinct typed captions; the 16 reference lookup keys remain stable. Two Core regression cases cover expression and scripting completion. The reference regeneration changes 11 Euclid articles and leaves all 1,220 non-Euclid articles byte-identical. The corrected independent PR's [reference workflow](https://github.com/PawelTroka/Computator.NET/actions/runs/41105924864/job/124235604493) passed 166 reference tests and 63 authoring tests at merge `535c618e6ec3d714397a8dfe5a7868b9b0ed16c4`.

### Whole-word Find/Replace: enclosing combining marks were excluded

The shared word-boundary classifier omitted `UnicodeCategory.EnclosingMark`. For text represented as `x\u20DD x x\u20E3`, a whole-word search for `x` incorrectly matched all three positions. Only the middle, unadorned `x` should match under the documented combining-mark behavior.

The appended #310 fix includes enclosing marks in word boundaries. All three new portable regressions failed before the change; the corrected portable search suite passes 39 cases. Existing native workflows now check the same boundary on Windows and Mono.

## Localization and desktop integration coverage

The original #306 translations did not exhibit a reproduced production defect. Validation was strengthened to test compiled resource loading rather than only source XML. A shared contract requires exact German and Czech satellite assemblies, checks every translated value against its source resource, and verifies fallback from `de-DE` to `de` and from `cs-CZ` to `cs`.

On the combined candidate this covers 16 resource sets and 338 UI strings per language, including the Find/Replace resources introduced by #310. The independent localization PR has 15 sets and 318 strings per language.

Six parameterized Windows test cases are defined to exercise the German and Czech Find/Replace dialogs with each of Scintilla, AvalonEdit and TextEditor; their native execution remains pending. They check translated menus, dialog title, document scope, whole-word controls, replacement button, result count and read-only message. They also perform Unicode replacement, inspect selection, undo the operation and confirm that the disk file was not implicitly saved.

The complete native Windows Find/Replace fixture contains 29 cases. Its wider coverage includes menu and keyboard routing, both editable workspaces, wrapping, UTF-16 selection, literal replacement text, single-step undo/redo, preservation of other dirty tabs, read-only enforcement, document changes and disposal. Native Mono exercises the supported reduced desktop's TextEditor path.

The first CI attempt for the new resource tests found that two added test-support files were missing from the repository's required solution inventory. This was an omission in the added validation coverage, not a pre-existing translation defect. A two-entry SLNX correction was appended to #306. The final candidate differs from its immediate predecessor only in those inventory entries; production and test source remain byte-identical.

## Completed local evidence

| Check | Observed result | Source identity |
| --- | --- | --- |
| Full portable Engine Release suite | 1,666 passed; 0 failed; 0 skipped | Final CI merge `897de969...`, tree `270b1fb3...` |
| Native Mono desktop acceptance | 46 passed; 0 failed; 0 skipped; production startup/render/graceful close passed | Built at CI merge `d6216f19...`, tree `58b013e7...`; rerun from final checkout `897de969...` reused these source-identical binaries |
| Supplemental compiled TSL integration | 30 passed; 0 failed; 0 skipped | Published candidate `189d28c0...`, tree `58b013e7...`; only SLNX inventory differs from final |
| Supplemental native Mono mathematics | 116 passed; 0 failed; 0 skipped | Actual net48 Core/Engine snapshot with recorded SHA-256 hashes and source equivalence to final |
| Debye reference-oracle verification | Six constants verified independently | Exact rational Bernoulli series, 60 and 80 terms |

The portable run used SDK 10.0.403 on Linux x64. The native Mono run used Mono 6.8.0.105 on Ubuntu 24.04 with Xvfb, libgdiplus and system GSL. Both resource scenarios verified all 338 German and 338 Czech compiled values and regional fallbacks.

The supplemental .NET fixture combines literal powers, digit-suffixed names, secant roots, trapezoidal integration, Euclid overloads and `out` arguments through actual TSL compilation. Both host-reference and complete emitted-numerics paths are covered, including German/Czech execution cultures. Its packaged reproduction command was also executed successfully.

The 116-check Mono probe covers extreme secant arithmetic, finite and subnormal trapezoidal boundaries, actual combined TSL expressions and Debye across host, emitted, precompiled-expression and precompiled-script surfaces. It uses actual production binaries; the manifest explicitly distinguishes a historical binary snapshot from a fresh build at the final merge. No additional defect was found.

## Final CI evidence

Pending. Record native Windows totals, resource and editor evidence, Mono fresh-build results, portable host matrices, reference determinism and the remaining repository workflows here once complete. Queued or running checks are not passing evidence.

## Scope and limits

- Validation applies to the exact seven contribution heads and base listed above. Later edits or a changed master require an appropriate new integration check.
- Cross-building Desktop.Tests on Linux does not establish native Windows GUI behavior. That remains a required final CI gate.
- Native Mono coverage is the supported reduced TextEditor desktop, not Scintilla or AvalonEdit on Mono.
- Debye orders 1 through 6 are supported; unsupported orders return NaN. This work does not implement higher orders.
- Workflow names explicitly distinguish script mirrors, activation contracts and clean-machine harness validation from actual external-service runs or installation acceptance. Their success must not be described as installation certification.

The exact supplemental fixtures, hashes, compact outcomes and reproduction commands are in [README.md](README.md) and [manifest.json](manifest.json). No master merge is authorized or performed.
