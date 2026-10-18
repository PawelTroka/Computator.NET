# Built-in function and constant reference

This reference documents the existing application, not a replacement mathematical API. It covers every non-keyword entry returned by the runtime scripting-completion catalog, including the expression subset. The reviewed baseline has **1,220 unique lookup signatures** across **1,229 non-keyword completion items**: 120 elementary functions, 224 special-function signatures, 88 mathematical values, 342 physical constants, and the exposed distribution, numerical, matrix, random, transform and scripting operations. The other 65 completion items are language keywords, not mathematical functions/constants.

## Reading it

The existing desktop function-information window and HTML tooltip use the same materialized `Computator.NET.Core/Static/functions.xml`. Each article contains its actual typed signatures, a short explanation, a definition or formula where appropriate, argument/support/units information, relevant caveats, and source links. Authored LaTeX equations are typeset ahead of time into locally packaged, high-resolution PNGs, accompanied by accessible text and explanations: no network or runtime mathematics renderer is required to read an article. Optional source links provide deeper reference material.

The document shell supplies UTF-8 and local styling in both desktop views. The WebClient reconstructs an allowlisted HTML fragment for Ace's `docHTML` tooltip in both editors, preserving paragraphs, headings, emphasis, code, superscripts, subscripts, Unicode mathematics and safe HTTP(S) source links. Parsing happens in an inert template: scripts, event handlers, styles, frames, SVG and remote-loading content are excluded. Only strictly validated, content-addressed local equation PNGs are admitted. The existing Ace and desktop WebBrowser controls remain in use.

One insertion signature can represent several typed overloads. The article lists all such menus and separates differing semantics instead of allowing the last overload to erase earlier information. Examples such as integer-vs-real random bounds, doubled-vs-actual angular momenta and real-vs-complex branches must not be inferred solely from a function name.

## Authoring and verification

The editorial sources in `build/reference/` are original prose organized by subject, with explicit named articles and closed family/property mappings. Shared distribution operations reuse a mathematically specific family definition plus operation-specific semantics, rather than duplicating hundreds of nearly identical hand-edited XML fragments. Unknown categories and unknown named functions fail generation; there is no generic filler fallback and no runtime description generation.

```powershell
# Explicitly regenerate after reviewing prose or a changed runtime catalog:
./build.ps1 -Target Reference-Generate
# Check the committed reference, without rewriting it:
./build.ps1 -Target Reference-Validate
```

Cake owns the graph and the existing .NET tests export the *unaugmented* reflection catalog with fixed typed-signature display options. Prose generation uses Python's standard library and writes deterministic XML. Equation authoring uses the audited MathJax and resvg development dependencies in the existing WebClient lockfile. After dependency restore, typesetting and verification work offline. Normal application builds copy the committed XML and equation assets; Python, Node and MathJax are not required to read help in the application. The single SLNX exposes all reference sources and its build/docs items.

Coverage tests require exact agreement with the live non-keyword catalog, correct attachment in both completion modes, every typed overload, actual stored constant values, meaningful section structure and HTTPS source links. HTML checks reject active markup, network-loaded images and placeholder text. Independent negative tests verify that unsupported additions fail instead of acquiring fabricated descriptions. A read-only CI job also verifies byte-for-byte regeneration and retains an offline preview and coverage report. These checks prove coverage and contracts; they are not a machine proof of every mathematical statement.

Existing scientific numerical tests remain unchanged. Desktop tests exercise the actual popup and standalone reference window, rapid selection changes, content replacement and disposal of their browsers. Browser tests inspect rendered HTML in both Ace editors, including real API articles and malicious-markup rejection. Independent distribution and physical-constant regressions check the documented implementation and unit conventions. New functions or signature changes must ship matching reviewed help changes.

## Primary editorial references

- [NIST Digital Library of Mathematical Functions](https://dlmf.nist.gov/): definitions, branch/normalization conventions and special-function relations. Individual articles link the relevant chapter/section.
- [GNU GSL special-function reference](https://www.gnu.org/software/gsl/doc/html/specfunc.html): the precise real-domain, scaling and argument conventions of the native functions used here.
- [Math.NET Numerics API](https://numerics.mathdotnet.com/api/) and [version 5.0.0 source](https://github.com/mathnet/mathnet-numerics/tree/v5.0.0/src/Numerics): distribution parameterizations, support, sampling mutation/laziness, estimators and operation limitations.
- [Accord source and documentation](https://github.com/accord-net/framework): Gabor, transformations and numerical wrappers where relevant.
- [NIST physical constants](https://physics.nist.gov/cuu/Constants/): quantities, unit conventions and the distinction between legacy stored approximations and current metrology.
- [OEIS](https://oeis.org/): named-constant definitions and original-reference bibliographies, including less common recurrence and geometric constants. Articles link individual records where verified, otherwise a specific named-constant query rather than an invented record number.
- The checked-in implementation is authoritative for **what this program currently returns**. Source links identify wrapper-specific restrictions that differ from the broader mathematical function.

Text is newly written and formulas are mathematical definitions; no third-party article or image bundle is copied into the application. Source pages are optional reference reading, not assets downloaded during execution. The editorial review date is **2026-10-16**. A source citation does not turn a rounded legacy number into an exact or continuously updated constant.

## Known numerical differences deliberately not changed here

Several descriptions would be misleading without reporting existing behavior. This content PR does not silently correct algorithms, update constant values or relax numerical tests. Separate numerical fixes should update both tests and the corresponding reference:

| Existing identifier | Explicitly documented distinction |
| --- | --- |
| `Beta(x,a,b)`, `β(x,a,b)`, `BetaNorm(x,a,b)` | Both incomplete-beta wrappers forward the wrong parameter order. An independent example test reproduced Beta(0.5,1,1)=2 instead of 0.5. The help distinguishes actual/intended results and gives the tested Math.NET BetaIncomplete/BetaRegularized alternatives; dividing the broken three-argument wrapper by complete beta is not a fix. |
| Modified spherical Bessel I/K | Returned values are exponentially **scaled** despite the public names omitting “scaled”; the K normalization includes the GSL π/2 convention. |
| `CoulombGprime` / `Coupling9j` | Current forwarded order / ninth-argument behavior differs from the advertised general formula. |
| Riccati-labelled ξ/ζ | Current forwarding uses cylindrical rather than spherical Hankel functions. |
| `Debye` outside orders 1–6 | The fallback is not the normalized higher-order Debye definition. |
| `minor` | Extracts a rectangular submatrix; it does not compute a determinant or cofactor. |
| `Geometric` | Counts trials including success, on 1,2,…; NegativeBinomial counts failures starting at zero. |
| `Stable` density/CDF | Math.NET 5.0 supports only its Gaussian, Cauchy and Lévy special cases for evaluation, despite broader sampling support. |
| `Categorical.InvCDF` | The cumulative-array binary search can select a later zero-weight category at a tied boundary, rather than the smallest category satisfying the mathematical quantile definition. |
| `MatrixNormal.Sample` | The Math.NET 5.0 sampler uses an inverse covariance and a different Kronecker ordering from the stated distribution; the reference distinguishes density and actual sampling behavior. |
| `Levy` evaluation | The mathematical limiting density at the support boundary is zero, but the current PDF/log-PDF returns NaN there; its CDF also returns NaN below support. |
| `g`, `h`, `G` and named counterparts | Some short and long names have different legacy stored approximations. |
| `WeakMixingAngle` | Stores sin²θ, not an angle. |
| Gyromagnetic `Over2Pi` values | MHz/T rather than Hz/T. |
| `PlanckLength`, `PlanckMass`, `PlanckTime` | Historical stored values differ slightly even from the 2010 central values; the reference preserves and identifies those discrepancies. |
| `SilverConstant` / `SilverRatio` | Plastic number versus 1+sqrt(2), respectively. |
| Hammersley sofa / complementary tetrahedral names | A feasible lower-bound construction and a supplementary angle, despite their historical names. |
| Gibbs names | Si(π) and 2Si(π)/π rather than the usual 0.08949 fractional overshoot. |

Physical descriptions always identify units and the actual runtime-stored binary64 value. They distinguish modern exact SI definitions (for example h, e, kB and NA) from the code's older approximations. μ₀, ε₀ and the molar mass constant are not presented as exact under the post-2019 SI. Historical conventional electrical/X-ray units remain labeled historical. No uncertainty is invented for a constant whose source code stores none.

Coverage means all supported autocomplete lookup entries, not every method in every referenced library, a complete special-functions textbook, a new symbolic system, translations into every locale, or an update of the numerical implementation. Technical editorial review remains welcome, particularly before changing a legacy value or convention.

## Offline typeset equations and teaching notes

See [REFERENCE-EQUATIONS.md](REFERENCE-EQUATIONS.md) for the authored TeX, packaged
PNG pipeline, contextual signatures, teaching content and executable examples.
The old no-image HTML policy is superseded only for strictly local hashed PNG
equations; remote images, scripts, SVG and arbitrary active content remain banned.
