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
examples remain selectable. PNG equations are not selectable mathematical text or
semantic MathML. The existing HTML engine is preserved rather than silently
introducing a WebView runtime requirement.

## Context-aware signatures

XML retains every typed overload once. The redundant “Syntax” heading and second
copies in overload section headings are removed at authoring time. A tooltip
removes a signature only when it exactly matches the visible completion caption
(after whitespace normalization). Untyped captions do not hide typed argument
information, sibling overloads remain, and the standalone details reader keeps all
signatures because no completion caption is visible there. Case and punctuation
are meaningful and are not normalized away.

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
loading, both Ace editors, safe attributes and context-aware sibling overloads.
Actual API tests continue to exercise the shipped constant and Airy articles.

Primary references are retained with each article (NIST DLMF, GNU GSL, versioned
Math.NET implementations and NIST constants). Formulas and new teaching prose are
authored locally, not screenshots or copied article bundles. Tool/font-outline
notices accompany the asset manifest; no system font files are distributed.
