# Offline equations and explanatory reference

The existing `functions.xml` remains the only runtime article catalogue. This
change adds authored TeX, interpretation paragraphs and worked examples through
`build/reference/enrichment.py` and `enrichment_more.py`, alongside the original
subject modules. Original unit/domain warnings and known numerical-wrapper
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
examples remain selectable. Inline expressions also have selectable text equivalents
for copying and accessibility. PNG equations are not semantic MathML. The existing HTML engine is preserved rather than silently
introducing a WebView runtime requirement.

### Inline mathematics in sentences

Issue #207 reports the Laguerre introduction showing `x^α exp(−x)` as plain text
while the later display equation is typeset. Inline mathematics now uses the same
offline asset pipeline and appears within the original sentence. The editorial
pass annotates 339 articles, including polynomial weights and intervals, gamma
and beta notation, mathematical constants, probability definitions, physical
units and conversion identities. Replacing every new inline image with its text
alternative reconstructs the previous HTML exactly: definitions, cautions,
executable examples, stored values and overload metadata retain their content.

Authors compose a sentence explicitly in its subject module:

```python
from common import P, M

P('A degree-', M('n', 'n'), ' polynomial orthogonal on ',
  M(r'[0,\infty)', '[0,∞)'), ' with weight ',
  M(r'x^\alpha\exp(-x)', 'x^α exp(−x)'), '.')
```

`P` joins ordinary text and `M(TeX, readable_alternative)` fragments. It accepts
nested prose and ordinary string concatenation for shared family descriptions.
Ordinary strings, dollar signs, TeX-like text and executable code are never
interpreted heuristically. Every mathematical fragment requires a nonempty text
alternative. Long formulas belong in the existing display-equation field.

Inline assets use MathJax's inline layout, including its smaller integral/sum
limits, and are distinct from display assets even for identical TeX. Their hash
is SHA-256 of `inline\0` followed by TeX; all 639 existing display-equation
identities and PNGs remain unchanged. The manifest's schema 2 adds `mode: inline`
and a measured `depth` in thousandths of an em to inline records. The 125 added
PNGs total 323,825 bytes and are reused throughout the catalogue.

MathJax 4's inline break segmentation is explicitly disabled: it otherwise emits
multiple sibling SVGs at possible break positions. Generation requires one
complete SVG before rasterization and fails instead of discarding later pieces
of an expression. A regression specifically covers the Laguerre weight and the
previously omitted `−x)` segment.

Each inline PNG uses tight padding and three times the nominal 20px dimensions.
The final SVG viewBox exactly matches the integer pixel aspect ratio, with
rounding space added symmetrically, so no letterboxing moves the glyph baseline.
The SVG's baseline supplies its own descent; the viewers reconstruct width,
height and vertical alignment in `em` so they scale with surrounding prose. This
keeps the same mathematical baseline at ordinary and enlarged text sizes, rather
than vertically centering every expression. Inline math is an indivisible unit;
sentences wrap at normal text boundaries. Authoring limits inline assets to
320 × 100 nominal pixels and rejects out-of-range baseline metadata.

The runtime HTML contains only the fixed `reference-math-inline` class, the
existing hashed PNG path, an alternative, dimensions and integer `data-depth`.
Desktop and web reconstruct their numeric CSS from validated metadata; source
styles and handlers are discarded. The desktop still inlines only the selected
article's packaged PNGs. The editorial preview applies the same relative sizing
and baseline. After validation, each viewer adds a visually hidden text span from
the alternative and hides the visual image from the accessibility tree, so
sentences copy as complete text and screen readers read each expression once.
The rendered image has an empty alternative to prevent Firefox from copying it
a second time. The original catalogue alternative remains intact. Dark Ace
tooltips display the transparent inline glyphs in white for readable contrast.
Neither hidden classes nor ARIA attributes are accepted from authored HTML.
No remote image, font, JavaScript mathematics runtime or dependency
is added.

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

### Scientific benchmark diagrams (#224)

Ackley, Rastrigin, Bohachevsky1, Himmelblau, Matyas, Rosenbrock, dropWave and
sixHumpCamel now have original, labelled two-coordinate diagrams. Eight images
serve nine lookup entries, including the two Rosenbrock signatures. Each image
pairs sampled level curves with a marked path and its evaluated objective values.
Minima and axis values are shown explicitly; captions explain what a slice can
and cannot establish about a surface. Matyas follows its shallow diagonal and
Rosenbrock its curved valley. Himmelblau marks all four global minimizers; the
six-hump camel plot marks both symmetry-related minima and discloses the rounded
slice coordinate. The Bohachevsky drawing follows the actual returned value -0.7
at the origin, consistent with the existing implementation warning.

Ackley, Rastrigin and sixHumpCamel also gain executable worked examples,
discovered by the existing runtime example test. All prior article text,
formulas, examples, argument guidance and qualifications are retained.

`build/reference/plots.cjs` samples the mathematical formulas and draws the
geometry, including contour interpolation. It uses the existing pinned
MathJax/resvg development dependencies to turn axis-label glyphs and paths into
PNG images. No new dependency, operating-system font or image-generation model
is used. The function conventions are checked against the
[pinned Math.NET implementation](https://github.com/mathnet/mathnet-numerics/blob/v5.0.0/src/Numerics/SpecialFunctions/TestFunctions.cs).
The diagrams are numerical illustrations, not proofs of global optimality or
runtime optimizer results. Tests independently check analytic values, minima,
the valley identities and contour geometry on a circle and saddle.

The eight 600 × 374 diagrams have 3× pixel resolution. Their SVG-source hashes
name the PNG files in `Static/reference-figures`; the manifest records image
hashes, dimensions, axis ranges, contour values, minima, captions and text
alternatives. Both `Reference-Generate` and `Reference-Validate` run their
deterministic generation and geometry tests. Ordinary builds only copy the
committed images. The desktop renderer and web sanitizer accept only the exact
`reference-plot` class with hashed `reference-figures/*.png` paths, bounded sizes
and a nonempty alternative; the directory and class must agree. They still
reject remote, data, SVG, traversal and active-attribute input. All previous
equation and inline-math source identities remain unchanged.

This implements the missing benchmark illustrations from #224. The separate
[PR #229](https://github.com/PawelTroka/Computator.NET/pull/229) handles definition
placement, benchmark formula promotion and desktop reading space. This diagram
change independently targets the same master revision without duplicating those
changes. When integrating the authoring changes, retain #229's
`definition_label='Definition'` alongside each new worked-example tuple, then
regenerate `functions.xml` from the combined sources. Illustrations for other
mathematical domains remain further editorial work.

All 1,220 existing entries gain explanatory guidance, with subject-specific
interpretations and explicit numerical/physical caveats. Display equations appear
in 1,172 entries; together with inline mathematics, 1,173 entries include typeset
notation. The remaining 47 are storage, I/O, plotting or related helpers for
which a decorative mathematical formula would be misleading.

The rendered content includes definitions, underlying equations, family
identities and stored constant values. Those are deliberately labelled: an
underlying Bessel equation is not an assertion that a scaled spherical wrapper
returns its cylindrical solution. Definitions that have additional normalization
or implementation qualifications remain readable as conventions. Constants keep
their actual historical stored values alongside any current-SI definition.

Worked examples contain executable input, an explicit answer and an explanation
of why that answer is meaningful. The runtime test discovers each distinct new
numeric example from the shipped XML and evaluates it through the actual TSL
compiler, rather than checking a duplicate hardcoded example list. Rounded
printed answers are tested with a documented display-precision budget; existing
numerical-suite tolerances are unchanged. Coverage counts are not a claim that
all mathematical sentences have been independently proved or that every routine
has a new worked example.

## Authoring and verification

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

Inline regression checks cover explicit authoring and literal code, separate
source identities, complete expressions, all shipped image hashes and dimensions,
metadata validation and locale-independent desktop CSS. Browser tests use the
actual shipped Laguerre article in both Ace editors at 360px and 1280px widths,
measure image/text baseline agreement and scaling at 100% and 150% text sizes
at 1× and 3× device pixel ratios, verify complete text selection and accessibility,
load every inline asset through the production allowlist, and independently
check the exported glyph ink of `n` and `x` against the recorded baseline. The Windows desktop
fixture measures its own HTML control at 360px/700px widths and 18px/27px fonts;
a Linux cross-build does not substitute for executing those Windows UI checks.

Primary references are retained with each article (NIST DLMF, GNU GSL, versioned
Math.NET implementations and NIST constants). Formulas and new teaching prose are
authored locally, not screenshots or copied article bundles. Tool/font-outline
notices accompany the asset manifest; no system font files are distributed.

## Issue #207 validation checkpoint

This focused change is based on #211 at `cf49d1ef`, preserving #204 and the
reference stack (#178 → #183 → #196 → #197 and subsequent numerical reference fixes).
The inline implementation was recovered from an unpublished local commit;
copy/accessibility, dark-theme contrast, live-API image checks and additional
regressions were completed here. No new numerical behavior is included.

Local validation on Linux:

- 41 Python authoring/preview tests passed.
- All 764 PNGs and the manifest regenerated byte for byte with the pinned tools.
- The existing runtime-exported inventory first reproduced the parent XML exactly;
  it then reproduced the new XML exactly. Fresh native export remains a CI gate.
- Replacing the new inline images with escaped alternatives reconstructs the
  entire parent catalogue byte for byte. All 639 original display assets and
  their manifest records are unchanged. The 881 unannotated articles are unchanged.
- 34 focused browser executions passed in Chromium and Firefox. These use the
  production renderer and shipped article/PNG content with autocomplete response
  fixtures, at 360/1280px, 1×/3× density and 100%/150% text sizes. Selection and
  accessibility assertions cover the complete Laguerre sentence.
- Windows-targeted Core.Tests and Desktop.Tests Release cross-builds passed.
  Desktop retains the existing SimpleCodeEditor compatibility warnings.
- 29 cases from the actual rich-content NUnit fixture passed in a temporary
  managed-only harness; the compiled desktop document renderer also rendered all
  1,220 articles without mutation. This does not execute the Windows HTML control.

The browser fixtures save screenshots; native desktop tests additionally capture
paint-checked Laguerre screenshots at two widths and two text sizes. Actual
MSHTML UI execution and the live Windows API/editor suite remain CI/acceptance
gates. Linux Chromium previews of desktop-generated HTML are useful layout
checks, not native Windows evidence. No merge or release is performed.
