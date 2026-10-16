# Offline equations and explanatory reference

The existing `functions.xml` remains the only runtime article catalogue. This
change adds authored TeX, interpretation paragraphs and worked examples through
`build/reference/enrichment.py` and `enrichment_more.py`, alongside the original
subject modules and the focused constant, inverse-function, normalization,
distribution-example and matrix-example lesson modules. Original unit/domain warnings and known numerical-wrapper
limitations are retained. No numerical implementation is silently corrected by
editing its help text.

## Rendering and deployment

MathJax 4.1.3 typesets reviewed TeX into SVG glyph outlines during the explicit
editorial build. resvg 2.6.2 rasterizes those outlines at three times the displayed
pixel dimensions. The committed PNGs and source/hash/dimension manifest live in
`Computator.NET.Core/Static/reference-equations`. The source hash names each image,
so repeated equations share one asset. No browser MathJax runtime, remote equation
service, web font, SVG execution or application-startup generation is introduced.

The desktop copies the same asset directory as other Core content. Its reference
renderer permits only the exact hashed relative PNG path and inlines the few
images in the selected article for the existing `about:blank` Windows WebBrowser.
The web build copies the single Core asset set into its own distribution. Ace
receives rebuilt, allowlisted HTML: only hashed `reference-equations/*.png` images
with bounded dimensions and nonempty alternatives survive. Remote/data images,
SVG, arbitrary styles and handlers remain excluded. No image bytes are repeated
in the full autocomplete JSON catalogue.

Wide equations scroll at readable size rather than shrinking into thumbnails.
Alternative text preserves the mathematical description; normal article text and
examples remain selectable. PNG equations are not selectable mathematical text or
semantic MathML. The existing HTML engine is preserved rather than silently
introducing a WebView runtime requirement.

## Signatures belong in autocomplete, not descriptions

Descriptions start with the article title and explanation. Desktop tooltips,
the standalone reader, both web editors and the offline editorial preview remove
the entire `reference-signature` block, including alternate overload signatures.
There is no exception for missing, untyped or differently formatted captions.
The autocomplete list and insertion text are unchanged; type/domain explanations,
mathematical equations, overload-specific prose and worked-example code remain.

XML retains typed overload metadata for the existing catalogue coverage checks;
rendering never mutates that source data. Legacy signature blocks with a Syntax
heading or extra classes are removed as well. The renderers' optional caption
argument is retained for compatibility with existing callers, not as a condition
for showing redundant signatures.

## Content boundaries

All 1,220 existing entries gain explanatory guidance, with subject-specific
interpretations and explicit numerical/physical caveats. 1,171 entries include
rendered mathematics; the remaining 49 are storage, I/O, plotting or related
helpers for which a decorative mathematical formula would be misleading.

The rendered content includes definitions, underlying equations, family
identities and stored constant values. Those are deliberately labelled: an
underlying Bessel equation is not an assertion that a scaled spherical wrapper
returns its cylindrical solution. Definitions that have additional normalization
or implementation qualifications remain readable as conventions. Constants keep
their actual historical stored values alongside any current-SI definition.

Worked examples contain executable input, an explicit answer and an explanation
of why that answer is meaningful. The runtime test discovers each distinct new
numeric example from the shipped XML and evaluates it through the actual TSL
compiler, rather than checking a duplicate hardcoded example list. Printed
answers must satisfy the existing fixed relative budget of 1e-9 (1e-12 for zero,
with a 1e-300 floor for nonzero values); adding fewer displayed digits does not
relax it. Existing numerical-suite tolerances are unchanged. Coverage counts are not a claim that
all mathematical sentences have been independently proved or that every routine
has a new worked example.

## Reviewed educational follow-up

The current authoring snapshot has the following counts, measured from the local
generated XML and equation inventory. Shared examples are counted once by editor
and complete displayed code, using the same key as the runtime test.

| Measure | Count |
| --- | ---: |
| Catalogue entries | 1,220 |
| Distinct authored TeX equations | 1,068 |
| Entries with worked examples | 561 |
| Distinct executable examples | 383 |
| Expression / scripting examples | 338 / 45 |
| Mathematical constants with a defining equation and stored value | 88 / 88 |
| Physical constants with a defining equation and stored value | 342 / 342 |

Distribution articles contain 152 distinct deterministic examples across 276
entries, covering densities, masses, logarithms, cumulative probabilities,
quantiles, parameter conversions, estimation and input rejection. Random samples
are not assigned fixed answers. Each of the 42 matrix entries has its own
scripting example, observing a scalar entry, dimension, determinant, norm or
predicate so its answer can be checked. The other three scripting examples check
matrix-valued distribution parameters. Additional special-function examples
distinguish normalized, scaled and incomplete variants.

Maintain these lessons in the authored Python modules, then regenerate through
Cake. `constant_lessons.py` holds explicit named lessons and aliases;
`physical_equations.py` covers the remaining canonical `physical.BASE` keys;
`physical_family_lessons.py` covers only the declared atomic, natural, conversion
and particle families. A new physical identifier must receive a reviewed defining
equation, compatible units and appropriate qualifications; an unknown identifier
fails generation. Keep the partition and alias tests current. Extend
`inverse_lessons.py`, `normalization_examples.py`, `distribution_examples.py` and
`matrix_examples.py` for operation-specific explanations and examples. Review the
actual parameter order, domain, normalization and required editor, and derive
expected answers independently of the wrapper being tested. Do not infer TeX
definitions from arbitrary prose or stored decimal strings, or edit generated
XML/images by hand.

Historical stored constants remain unchanged and are distinguished from modern
SI definitions. The known `arsinh` parenthesis defect remains documented: the
typeset intended inverse and its explicit-formula example do not claim that the
wrapper is repaired. Historical X-ray units use the assigned line multipliers in
the [CODATA 2010 report, §9.4, equations (255)–(257)](https://physics.nist.gov/cuu/pdf/JPCRD2010CODATA.pdf#page=41),
including `λ(Mo Kα₁) = 707.831 xu(Mo)`. Their stored conversions to meters
remain unchanged. Content counts do not establish runtime success; the corresponding
Windows commit must pass Cake `Reference-Validate`.

When integrating the separate numerical fixes, review their affected lessons:
[#182](https://github.com/PawelTroka/Computator.NET/pull/182) changes `arsinh`,
[#184](https://github.com/PawelTroka/Computator.NET/pull/184) changes reversed-limit
quadrature, and [#185](https://github.com/PawelTroka/Computator.NET/pull/185) changes
the pointwise matrix folds. Remove a compatibility warning only after the
corresponding implementation is present, update affected examples, and regenerate
the reference against that combined revision.

## Authoring and verification

Author `WorkedExample.code` as a scalar expression. Its `editor` field defaults
to `expression`; set `editor='scripting'` when the example needs scripting-only
functions, including the matrix helpers. Generation validates these two modes,
labels the latter **Scripting editor**, and displays a complete `writeln(...);`
statement. The test executes that exact displayed code through `ScriptEvaluator`
and captures its output. Expression examples continue through `ExpressionsEvaluator`
in real mode. This prevents a mathematically correct answer from concealing an
example that cannot run in the indicated editor. Test setup selects invariant
culture and engineering notation and restores both settings and the process
working directory afterward.

```powershell
./build.ps1 -Target Reference-Generate
./build.ps1 -Target Reference-Validate
```

Cake owns the audited Node restore using the existing WebClient manifest and
single lockfile. MathJax/resvg are build-only development dependencies. The
explicit generation target exports the real reflected catalogue, collects TeX,
renders assets, tests the authoring modules and materializes XML. Normal builds
only copy checked-in content. Validation rerenders deterministically and requires
exact PNG, manifest and XML equality. A malformed/unsupported equation fails;
there is no silent raw-TeX or missing-image fallback.

Core tests verify source/image hashes, 3x pixel dimensions, no orphan assets,
complete overload retention, safe paths, content counts and actual worked-example
results. Desktop tests inspect image completion in the real Windows HTML control,
font sizes and window persistence. Hosted web tests verify real PNG bytes,
loading, both Ace editors, safe attributes and signature-free descriptions with
unchanged autocomplete captions. Preview tests enforce the same presentation.
Actual API tests continue to exercise the shipped constant and Airy articles.

Primary references are retained with each article (NIST DLMF, GNU GSL, versioned
Math.NET implementations and NIST constants). Formulas and new teaching prose are
authored locally, not screenshots or copied article bundles. Tool/font-outline
notices accompany the asset manifest; no system font files are distributed.
