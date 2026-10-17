# Numerical and reference integration

This 2026-10-17 snapshot records the combined local validation of PRs
[#192](https://github.com/PawelTroka/Computator.NET/pull/192),
[#199](https://github.com/PawelTroka/Computator.NET/pull/199) and
[#200](https://github.com/PawelTroka/Computator.NET/pull/200), preserving the
reference/menu stack through
[#197](https://github.com/PawelTroka/Computator.NET/pull/197).
The [evidence summary](evidence/numerical-reference-integration-2026-10-17.json)
contains source-tree identifiers, source and result hashes, and inventory
provenance. Consult the live PR checks for subsequent CI results; this document
is a fixed local evidence record, not a rolling CI status report.

## Dependency and merge order

**#178 → #183 → #196 → #197 → #192 → #199 → #200.**

| PR | Integration responsibility | Intended base branch |
| --- | --- | --- |
| #192 | Preserve recursive derivative step sizes and regenerate derivative articles | `fix/function-menu-groups` (#197) |
| #199 | Preserve bounded secant initialization, add six combined host/TSL cases and the compatibility correction below | `fix/higher-derivative-step` (#192) |
| #200 | Preserve cosine/Hartley corrections and regenerate the final combined catalogue, including a typeset Hartley definition | `fix/secant-root-initial-guesses` (#199) |

The reference dependency is #197 head
`e42d90c2a4806285539ebfa12442a7592a2e1769`. Integration commits retain the original
numerical heads as ancestors and permit fast-forward branch updates. They do
not require forced history replacement. Keep predecessor branches until their
successors have been retargeted. After an approved merge, retarget the next PR
to `master`, inspect its effective diff, and refresh its checks. If an upstream
squash/rebase merge changes the SHAs, reconcile the successor against the actual
resulting base and regenerate the catalogue again. Earlier head checks do not
certify that new candidate. This sequence does not assert that any PR was
merged or marked ready. Merging or publishing a release requires separate owner
approval.

## Authoritative reference preservation

All three numerical PRs edit `build/reference/scientific.py` and generated
`Computator.NET.Core/Static/functions.xml`. Their scientific article changes
are disjoint: four derivative articles, one bounded secant article, and one
shared transform article. #178 also changes the XML while enriching the
scientific authoring through `enrichment.py`, `enrichment_more.py` and
`generate.py`; #196 restores categories through the generator and authoring.

The integration combines those sources and regenerates XML. It retains #178's
teaching notes, worked examples, typeset mathematics and signature-free display,
and #196's subject categories. Typed catalogue metadata remains available to
coverage checks. Against the #197 dependency, **1,214 of 1,220 complete articles
are unchanged**; only the six affected articles differ. All **638 original
equation assets and their manifest records are preserved**. One new typeset
real orthonormal Hartley definition brings the total to **639**. The transform
article retains the N-by-2 real/imaginary convention and the other overloads'
mutation and invertibility limitations.

Final XML SHA-256:
`38a27e1de0511281328c8e3651af89fef4061920adb7b1386f3f18b2ba2ffea0`.

## Combined execution and compatibility correction

The original 32 derivative, 42 root-finding and 52 transform regression cases
remain independent. Six additional cases compose named derivative and bounded
secant operations through the real TSL pipeline: symmetric and translated
intervals, and rejection of a narrow interval before sampling its function.
Each caller runs against host types and against both emitted class definitions.
The transform fixture continues to compare coefficients against independent
defining sums; transforms have no separate `ToCode` implementation to reconcile.

Those new cases exposed a pre-existing compatibility issue when
`FunctionRoot.ToCode` is supplied through `PortableEngine`'s TSL
`customFunctions` pipeline: `AbsHandler` misinterprets the logical-OR chain in
`secantMethod`. Three emitted-source cases failed while the host cases passed.
Splitting that chain into three ordered guards in both host and emitted
definitions preserves short-circuit evaluation and numerical behavior. The
normal desktop path appends those C# definitions directly; this finding does
not establish a defect in ordinary desktop injection. No general parser,
workflow or reference/menu renderer was changed for this correction.

## Executed local validation

| Gate | Result |
| --- | --- |
| Complete portable engine suite, Linux, .NET SDK 10.0.402 | **276 passed, zero failed/skipped**: 144 baseline + 32 derivative + 42 root + 52 transform + 6 combined cases |
| Same suite before the compatibility correction | **273 passed, 3 failed**, all three new emitted-source composition cases |
| Python reference authoring tests | **31 passed** |
| Offline typesetter verification, Node 24.21.0 / npm 11.19.0 | **639 equations verified** |
| Reference generation with `--check` | Exact committed XML reproduction |
| Catalogue and asset preservation comparison | 1,214 unchanged articles; all 638 original equation assets retained |

Local generation used the real runtime inventory from #197's successful
[reference run 38966766207](https://github.com/PawelTroka/Computator.NET/actions/runs/38966766207).
Artifact `12074767938`, `function-reference-evidence`, was downloaded and its ZIP
SHA-256 checked against GitHub's digest. The job checked out
`1a9b49788eb11aa316fc24d2f046ea2c2d2cab77`, the merge of #197 into #196. No inventory
was inferred from the XML. The numerical changes preserve public signatures.
Fresh combined Windows reference/runtime export and application CI remain
separate gates; the local Linux results do not claim Windows execution.

The authoring unit checks operate on generated fragments before XML is written,
so they do not block an intentional regeneration because the old XML differs.
The existing generator `--check` verifies the entire materialized catalogue.

## Reproduce generation and validation

From the repository root with the pinned SDK and Node/npm on `PATH`, restore
the existing frontend authoring dependencies. For the exact local reproduction,
download the named artifact and verify the extracted inventory:

```sh
npm --prefix Computator.NET.WebClient ci
gh run download 38966766207 --repo PawelTroka/Computator.NET --name function-reference-evidence --dir artifacts/reference/pr197-origin
sha256sum artifacts/reference/pr197-origin/artifacts/reference/inventory.json
```

The inventory digest must be
`9211dafe6637d9185f54f436cb1e139c539f3bbe90ccfd6d6c5dbe319712d42b`.
GitHub's source artifact expires according to its retention policy; a later
fresh runtime export should be validated independently rather than represented
as this historical file.

```sh
python3 build/reference/generate.py --inventory artifacts/reference/pr197-origin/artifacts/reference/inventory.json --equations-only artifacts/reference/equations.json
node build/reference/typeset.cjs --input artifacts/reference/equations.json --output Computator.NET.Core/Static/reference-equations
python3 build/reference/generate.py --inventory artifacts/reference/pr197-origin/artifacts/reference/inventory.json --output Computator.NET.Core/Static/functions.xml --report artifacts/reference/coverage.json
python3 -m unittest discover -s build/reference -p 'test_*.py'
node build/reference/typeset.cjs --input artifacts/reference/equations.json --output Computator.NET.Core/Static/reference-equations --check
python3 build/reference/generate.py --inventory artifacts/reference/pr197-origin/artifacts/reference/inventory.json --output Computator.NET.Core/Static/functions.xml --check
dotnet test Computator.NET.Engine.Tests/Computator.NET.Engine.Tests.csproj --configuration Release --logger 'trx;LogFileName=portable-combined.trx' --results-directory artifacts/numerical-integration
```

On Windows, the existing Cake entry points provide an independent current-tree
runtime export and full reference verification:

```powershell
.\build.ps1 -Target Reference-Generate -Configuration Release
.\build.ps1 -Target Reference-Validate -Configuration Release
```

Review generated diffs before committing. Run the existing Windows workflows
for the integrated PR candidate; the downloaded dependency inventory does not
replace that gate.

## Separate reference/menu CI ownership

#183 already owns the pinned Node setup needed after #178. The pre-integration
release-evidence failures on #183/#196/#197, and
[#192 run 38965612903](https://github.com/PawelTroka/Computator.NET/actions/runs/38965612903),
occurred in `Release-Verify` → `Web-E2E` at the existing 5-second result
assertions in `real-stack.spec.ts`. Calculation/script status could still read
`Calculating…`; integration's `Number(status)` could read `NaN`. These are
separate application-acceptance observations, not failed numerical regression
fixtures or proof of a mathematical NaN returned by the engine. Their fixes
remain with the reference/menu CI work. Check the live dependency and numerical
PR checks before approving the merge sequence.
