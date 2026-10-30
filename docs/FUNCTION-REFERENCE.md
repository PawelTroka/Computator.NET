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

### Subject categories

`Type` identifies the runtime family; `Category` is the subject shown in menus and completion metadata. `build/reference/categories.py` supplies explicit subjects for elementary and special functions, mathematical values and physical constants. Function aliases resolve to the canonical name and share its subject across overloads. Constant groups preserve the engine's existing `CategoryAttribute` labels, including universal, electromagnetic, atomic/nuclear, geometry and basic mathematical groups.

Generation rejects missing subjects, labels that only repeat the runtime type, conflicting subjects for a shared overload key, and names without a reviewed assignment. A fixture taken from the catalogue before #172 independently protects all 318 still-supported function memberships that were flattened. Runtime tests separately compare every constant's materialized subject with its actual attribute and check that restored categories reach both completion modes. Changes to categories do not replace the authored explanations, equations, examples, stored values or source links.

## Primary editorial references

- [NIST Digital Library of Mathematical Functions](https://dlmf.nist.gov/): definitions, branch/normalization conventions and special-function relations. Individual articles link the relevant chapter/section.
- [GNU GSL special-function reference](https://www.gnu.org/software/gsl/doc/html/specfunc.html): the precise real-domain, scaling and argument conventions of the native functions used here.
- [Math.NET Numerics API](https://numerics.mathdotnet.com/api/) and [version 5.0.0 source](https://github.com/mathnet/mathnet-numerics/tree/v5.0.0/src/Numerics): distribution parameterizations, support, sampling mutation/laziness, estimators and operation limitations.
- [Accord source and documentation](https://github.com/accord-net/framework): Gabor, transformations and numerical wrappers where relevant.
- [NIST physical constants](https://physics.nist.gov/cuu/Constants/): quantities, unit conventions and the distinction between legacy stored approximations and current metrology.
- [OEIS](https://oeis.org/): named-constant definitions and original-reference bibliographies, including less common recurrence and geometric constants. Articles link individual records where verified, otherwise a specific named-constant query rather than an invented record number.
- The checked-in implementation is authoritative for **what this program currently returns**. Source links identify wrapper-specific restrictions that differ from the broader mathematical function.

Text is newly written and formulas are mathematical definitions; no third-party article or image bundle is copied into the application. Source pages are optional reference reading, not assets downloaded during execution. The editorial review date is **2026-10-16**. A source citation does not turn a rounded legacy number into an exact or continuously updated constant.

## Corrected incomplete-beta parameter order

`Beta(x,a,b)`, its `β(x,a,b)` alias and `BetaNorm(x,a,b)` now pass the two
shape parameters before the integration limit to Meta.Numerics/GSL, in both the
host implementation and the emitted TSL helpers. The public order remains
`x,a,b`: `Beta(0.5,1,1)` and `BetaNorm(0.5,1,1)` both return `0.5`.
The reference uses these corrected wrappers directly. Independent analytic
regressions cover uniform and polynomial integrals, asymmetric shapes, both
endpoints and the existing invalid-domain checks. The two-argument complete
beta functions retain their existing meaning.

## Corrected Wigner 9-j final argument

`Coupling9j(ja,jb,jc,jd,je,jf,jg,jh,ji)` now forwards all nine supplied
arguments to GSL, in both the host implementation and the emitted TSL helper.
The last argument selects the lower-right spin; it is no longer replaced by
`jf`. The public integer convention is unchanged: every value is **twice** the
physical spin, so `1` represents spin `1/2` and `2` represents spin `1`.
For example, `Coupling9j(2,2,2,2,2,2,2,2,0)` returns `1/18`; changing only the
last argument to `2` gives zero. The [GSL coupling conventions](https://www.gnu.org/software/gsl/doc/html/specfunc.html#coupling-coefficients)
and [DLMF zero-argument reduction](https://dlmf.nist.gov/34.7.E1) provide
independent definitions for the regression values. Tests exercise integer and
half-integer spins, allowed and forbidden couplings, and negative-input guards.

## Corrected irregular Coulomb derivative order

`CoulombGprime(L,η,ρ)` now returns the derivative of the irregular Coulomb
wave of the requested order `L`, in both the host and the emitted TSL helper.
The [GSL Coulomb-wave interface](https://www.gnu.org/software/gsl/doc/html/specfunc.html#coulomb-wave-functions)
returns its irregular wave at order `L-k`; the wrapper now supplies `k=0`
instead of `k=L`, which previously selected order zero for every call.
For example, `CoulombGprime(1,0,0.5)` returns approximately
`−3.591598762879524`, instead of the order-zero value `−0.479425538604203`.

Independent regressions use the [zero-charge spherical-Bessel identities](https://dlmf.nist.gov/33.5.E3)
and the [unit Coulomb Wronskian](https://dlmf.nist.gov/33.2.E12), including
nonzero positive and negative charge parameters. The existing requirements
`L≥0` and `ρ>0`, the derivative's meaning with respect to `ρ`, and the
overflow/error policy are unchanged. In particular, this correction does not
add reconstruction from GSL's overflow scaling exponents.

## Corrected inverse-error domains and tiny complementary tails

`inverseErf(x)` accepts the full real domain `[-1,1]`, and `inverseErfc(x)`
accepts `[0,2]`. Their endpoint infinities and invalid-input NaNs are retained.
Both `inverseErf(-0.5)` and `inverseErfc(1.5)` now return approximately
`−0.4769362762044699` in host calls and emitted TSL helpers.

Complementary inversion stays direct for small positive inputs. The existing
Math.NET routine handles the extreme positive tail below `1e-300`, using a
conservative transition before the Meta.Numerics refinement overflows farther
into the tail. At the smallest positive
binary64 value, `inverseErfc(double.Epsilon)` is approximately
`27.21329321081295`; it is finite. Independent fixtures use the bounded
[DLMF complementary-error asymptotic expansion](https://dlmf.nist.gov/7.12)
at high precision with the exact binary64 inputs, alongside cases immediately
on either side of the implementation transition.

## Corrected real inverse hyperbolic tangents

`artanh` and its `artgh` alias now use the runtime inverse hyperbolic tangent.
`arcoth` and `arctgh` use the reciprocal relation away from their poles and
retain the direct logarithmic ratio close to the poles, where rounding the
reciprocal first would amplify error. Tiny results, signed zeros and infinite
cotangent limits are preserved in both host calls and emitted helpers.
For example, `artanh(1e-20)` and `arcoth(1e20)` now both give approximately
`1e-20`. Real domains, endpoint infinities and complex overloads retain their
existing conventions. See the [DLMF inverse hyperbolic definitions](https://dlmf.nist.gov/4.37).

## Corrected spherical Riccati–Hankel convention

`RiccatiBesselξ(n,x)` and `RiccatiBesselζ(n,x)` use the
[spherical Hankel convention](https://dlmf.nist.gov/10.47):
`ξₙ = x·hₙ⁽¹⁾ = Sₙ − iCₙ` and `ζₙ = x·hₙ⁽²⁾ = Sₙ + iCₙ`,
where `Sₙ = x·jₙ` and `Cₙ = −x·yₙ`. Their reference articles now show the
corresponding radial equation, `u″+[1−n(n+1)/x²]u=0`.
For example, `RiccatiBesselξ(0,0.5)` is approximately
`0.4794255386 − 0.8775825619i`.

For `0<x≤1e-8`, scaled radial values are evaluated directly when their final
components fit in binary64. This avoids a spherical intermediate overflowing
before multiplication by `x`; for order zero the limit is approached through
`sin(x) − i·cos(x)`. Negative integer orders retain the spherical reflection
relations. Nonpositive inputs still return complex NaN, and the order-zero
limit at exactly zero remains unevaluated by this API. This correction does
not change the separate S/C or spherical-Hankel APIs or reconstruct truly
unrepresentable values.

## Corrected unitary-matrix shape requirement

`isUnitary(M)` requires a square matrix before applying the existing approximate
`M·Mᴴ≈I` check. A rectangular matrix with orthonormal rows is no longer reported
as unitary: `isUnitary(matrix({{1,0}}))` returns `false`, while the square rotation
`isUnitary(matrix({{0,-1},{1,0}}))` returns `true`.

This deliberately changes the former rectangular-row behavior documented in
older references. Scripts that need that row-orthonormality test can write
`isIndentity(M*HermitianTranspose(M))` explicitly. The separately documented
rectangular identity-pattern behavior of `isIndentity` and `IdentityMatrix`, the
relative comparison tolerance `1e-10`, and the public function names are unchanged.
The host, emitted helper and precompiled scripting surface use the same rule.

## Known numerical differences deliberately not changed here

The remaining distinctions below are preserved and documented. Each numerical correction above updates its tests and reference together; the other entries still describe current behavior:

| Existing identifier | Explicitly documented distinction |
| --- | --- |
| Modified spherical Bessel I/K | Returned values are exponentially **scaled** despite the public names omitting “scaled”; the K normalization includes the GSL π/2 convention. |
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
equations, sampled benchmark diagrams, unit-circle projections and normal
probability diagrams; remote images, scripts, SVG and
arbitrary active content remain banned. The diagram class and asset directory
are validated together. See the #224 section of the linked document for the
eight illustrated benchmark families, the six trigonometric/normal entries, and
the integration notes for PR #229.
