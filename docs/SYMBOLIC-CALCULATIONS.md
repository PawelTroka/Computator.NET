# Symbolic calculations

[Overview](../README.md) · [Documentation](README.md) · [Portable CLI](followups/PORTABLE-ENGINE.md)

The shared `Computator.NET.Engine.Symbolic` service performs algebra on mathematical
expressions written in **TSL notation**. It returns an exact TSL expression where
the selected algorithm can find one, an explicit numerical approximation when
requested, or a visible unresolved expression. It does not compile the input as
C# or execute TSL scripts.

The service uses **AngouriMath 2.6.0**, centrally pinned in
`Directory.Packages.props`. Its managed `net10.0` and `netstandard2.0` builds allow
the same mathematical implementation to be consumed by modern .NET applications
and the explicit net48 Mono desktop. Python, a remote calculation service and
native GSL are not needed for symbolic operations. The library and its algorithms
are covered by their upstream licenses; AngouriMath is MIT-licensed.

The engine and CLI are one change. The desktop tab and browser/API integration
are separate dependent changes, using this same contract. Symbolic calculation
does not alter the existing numerical commands or the TSL compiler's historical
meaning of an ASCII caret.

## Operations

| Operation | Example input | Additional parameters / meaning |
| --- | --- | --- |
| `simplify` | `1/3 + 2/3`, `sin²(x) + cos²(x)` | Algebraic simplification |
| `expand` | `(x+y)³` | Expand products and powers |
| `factor` | `x²-1` | Factor the expression where supported |
| `differentiate` | `sin(x²)` | Variable; derivative order 1–20 |
| `integrate` | `x²` | Variable; symbolic antiderivative including an arbitrary additive constant |
| `definite-integrate` | `x²` | Variable and symbolic lower/upper bounds |
| `limit` | `sin(x)/x` | Variable, approach point, and `both`, `left` or `right` |
| `solve` | `x²=2`, `x²-2`, `x²<1` | Solve for one variable in the selected domain; a plain expression is equated to zero |
| `substitute` | `x²+y` | Simultaneous variable-to-expression replacements |
| `evaluate` | `sqrt(2)` | Approximate to the requested precision; optional simultaneous substitutions |
| `taylor` | `exp(x)` | Variable, expansion point and polynomial degree 0–20 |
| `solve-system` | `x+y=3`, `x-y=1` | Equations and an ordered list of variables |
| `determinant` | `[[1,2],[3,4]]` | Exact determinant of a square matrix |
| `inverse` | `[[1,2],[3,4]]` | Symbolic matrix inverse, when defined |
| `transpose` | `[[1,2,3],[4,5,6]]` | Transpose a rectangular matrix |
| `trace` | `[[1,2],[3,4]]` | Sum of diagonal entries of a square matrix |

Select `complex` or `real` for numerical results and equation solutions. This
does not declare every free symbol real or positive: the algebra remains
conservative where extra assumptions would be needed. Settings are scoped to each
request, including concurrent requests, rather than changing global behavior for
later calculations. A real-domain equation cannot acquire imaginary roots from
an earlier complex-domain request.

## TSL mathematical notation

Use actual superscripts for powers, `·` for multiplication, `/` for exact
rational division and parentheses for grouping. The symbolic adapter uses the
same `SpecialSymbols` alphabet as the TSL editor, including operators, letters,
decimal separators, argument commas and absolute-value bars inside exponents.

| TSL expression | Meaning |
| --- | --- |
| `2·x³` | Two times the cube of x |
| `x⁻²` | Negative exponent |
| `x⁽²˙ʸ⁺¹⁾` | x to the power `2·y+1` |
| `x¹˸²` | x to the power `1/2` |
| `4⁰ᱸ⁵` | A decimal exponent; `ᱸ` is TSL's superscript decimal point |
| `sin²(x)` | Square of the function value |
| `2ˡᵒᵍ⁽⁸ʹ²⁾` | A function inside an exponent, using the superscript argument comma |
| `2Ꞌ⁻³Ꞌ` | Absolute value inside an exponent |
| `x  ²  ˸  ʸ ˙  ²` | Whitespace inside a compound superscript exponent |

Named functions such as `sin`, `cos`, `exp`, `ln`, `sqrt` and `abs`, implicit
multiplication such as `2x`, and the constants `PI`, `e`, `i` and `∞` are
supported. `π` and `pi` remain accepted input aliases for `PI`. The symbolic
adapter also accepts ASCII `*` and `^` as multiplication and power aliases for
pasted expressions. An ASCII caret in a numerical TSL script retains its existing
meaning; the desktop editor's exponent mode produces actual superscripts.

**The logarithm order matches TSL:** `ln(x)` is natural logarithm, `log(x)` and
`log10(x)` are base 10, and the two-argument form is **`log(argument, base)`**.
For example, `log(8,2)` is `3`. The adapter translates the library's different
internal order in both directions, including nested expressions and outputs.

`arccot` and its TSL alias `arcctg` use the existing numerical TSL definition
`PI/2 - arctan(z)`. For real arguments this selects the branch between zero and
`PI`, so `arccot(-1)` is `3·PI/4`. An inverse cotangent produced internally by the
algebra library has a different branch. Its output retains that meaning with an
explicit piecewise `arctan(1/z)` expression and the value `PI/2` at zero, rather
than changing the value by naming it `arccot`.

A bare function name is not a free symbolic variable; unknown functions are
rejected instead of silently turning `f(x)` into multiplication. Use an explicit
`·` for a variable followed by a parenthesized factor, such as `x·(x+1)`.
Elementary absolute values can be written `abs(x)` or `|x|`. Indexed variables
use an underscore, for example `x_2`.

Use a decimal **point**, independently of the application or operating-system
culture. Matrix elements are comma-separated inside nested brackets.
Equation systems are separate expressions and a list of target variables; their
variable order determines the solution column order. Systems, solution sets and
unresolved calculus expressions extend the scalar mathematical notation. Custom
C# function declarations and imperative TSL scripts are handled by the existing
numerical/script commands.

Substitution is simultaneous: replacing `x` by `y` and `y` by `x` exchanges the
names. It does not first replace `x`, then replace the newly inserted `y`.
The order of JSON properties does not affect the answer, including when an
unused substitution key resembles an internal temporary variable name.

Results use the same TSL notation: for example, differentiating `sin(x²)` returns
`2·x·cos(x²)` (spacing may vary). Copying a result preserves those Unicode
characters. A power whose exponent cannot be represented safely with the TSL
superscript alphabet uses `pow(base, exponent)`, retaining its complete meaning.
Resolved scalar results using functions available in the selected numerical host
can also be reused in the TSL calculator; equation systems and sets belong in the
symbolic workspace. LaTeX remains available separately.

## CLI examples

Build and verify the published application with the repository's Cake entry point:

```sh
./build.sh --target=Portable-Validate
```

Then run symbolic commands through the published CLI:

```sh
dotnet artifacts/portable/computator.dll symbolic --operation simplify --expression '1/3+2/3'
dotnet artifacts/portable/computator.dll symbolic --operation expand --expression '(x+y)³'
dotnet artifacts/portable/computator.dll symbolic --operation differentiate --expression 'x³' --order 3
dotnet artifacts/portable/computator.dll symbolic --operation integrate --expression 'sin(x)'
dotnet artifacts/portable/computator.dll symbolic --operation definite-integrate --expression 'x²' --lower 0 --upper 3
dotnet artifacts/portable/computator.dll symbolic --operation limit --expression 'sin(x)/x' --point 0
dotnet artifacts/portable/computator.dll symbolic --operation solve --expression 'x²=2' --domain real
dotnet artifacts/portable/computator.dll symbolic --operation substitute --expression 'x+y' --substitutions '{"x":"1/3","y":"2/3"}'
dotnet artifacts/portable/computator.dll symbolic --operation evaluate --expression 'PI' --precision 40
dotnet artifacts/portable/computator.dll symbolic --operation taylor --expression 'exp(x)' --order 4
dotnet artifacts/portable/computator.dll symbolic --operation solve-system --equations '["x+y=3","x-y=1"]' --variables '["x","y"]'
dotnet artifacts/portable/computator.dll symbolic --operation determinant --expression '[[1,2],[3,4]]'
```

The ordinary `differentiate` and `integrate` commands continue to produce
floating-point numerical answers. `symbolic --operation differentiate` produces
a mathematical expression; `symbolic --operation integrate` produces an
antiderivative. All symbolic commands require `--operation`. The default variable
is `x`, point `0`, order `1`, precision `30`, direction `both` and domain `complex`.
Only options relevant to the selected operation are accepted. JSON substitutions
require unique keys and string values; duplicate or mistyped input is rejected.

Successful symbolic commands return a single JSON object with these fields:

| Field | Meaning |
| --- | --- |
| `result` | TSL mathematical text with superscript powers and `·`; scalar expressions can be reused as input, while systems use the separate equations/variables fields |
| `latex` | LaTeX representation when available; empty for an unsupported-operation fallback that retains the original request text |
| `status` | `exact`, `approximate`, `unevaluated`, or `undefined` |
| `warnings` | Result-specific mathematical caveats or unresolved-operation explanations |

CLI exit code `0` means that a symbolic result was returned; inspect its status
before assuming that the requested operation was solved. Invalid input and
calculation failures use exit code `1` and a JSON error on stderr. The TSL
compilation-error exit code `2` remains reserved for the existing compiled-code
commands. No status value claims a proof of a simplification's equivalence at
every singular point.

## Limits and result conventions

Symbolic computation has mathematical and computational limits. Some integrals,
limits and equations have no answer in the engine's supported closed forms;
others require conditions on parameters. Such results must remain unresolved
rather than becoming a fabricated zero or an empty solution set. Numerical
root search is disabled for symbolic solving: a few approximate roots are not
presented as a complete exact solution set.

Equations with free parameters may remain conditional rather than dropping
degenerate cases such as `a=0` in `a*x=1`. When the upstream system solver cannot
distinguish an inconsistent system from an unsupported one, the result remains
`unevaluated`; this is not a claim that the solution set is empty. Retained system
text is explanatory output, and should be resubmitted using the equations and
variables fields rather than parsed as a single scalar expression.
System candidates are checked against the original equations and their domain
restrictions. Real-valued coordinates alone do not make a valid real solution:
for example, `sqrt(x)^2=-1` cannot admit `x=-1` in the real domain. Undefined
coordinates and excluded poles are rejected. If a candidate family cannot be
verified, the original system remains `unevaluated` with an explanation.

When a generic integral introduces a denominator that can vanish for a free
parameter, the result retains the generic formula under its condition and the
original integral for the exceptional case. The status remains `unevaluated`
while such a case is unresolved. For example, integrating `exp(a*x)` from zero
to one must retain the `a=0` case: substituting `a=0` into the returned result
gives `1`, not an undefined division. Likewise `x^a` requires a separate case
at `a=-1`. Keep the piecewise conditions when copying or simplifying these
results; parameter specialization can resolve the retained integral.

Factorization and simplification can change how removable singularities are
displayed. Check the original expression's domain when reusing a transformed
expression. Principal complex branches and real-domain restrictions matter for
roots, powers and logarithms. A returned antiderivative is local to a domain
where it is defined, and an indefinite integral's constant is arbitrary.
Definite integration uses the library's definite-integral operation rather than
blindly substituting endpoints into an arbitrary antiderivative.

Requests allow at most 8,192 characters per expression, derivative/series orders
up to 20, precision from 1 to 100 digits, input matrices up to 8×8, and up to eight
equations and eight variables per system. Output solution matrices may have more
rows, subject to the expression-complexity and 65,536-character result caps. The
engine checks cancellation and applies a cooperative time budget. A cooperative
cancellation token is not a hard process deadline. Web hosts additionally retain
their existing isolated-worker deadline and process cleanup. Desktop hosts keep
computations away from the UI thread and discard results from cancelled or
superseded requests.

LaTeX is returned as data. Consumers must render it using a mathematical renderer
or show it as text; it must not be injected into HTML as executable markup.

## Verification

The portable engine fixtures exercise actual mathematical behavior, including
algebraic values, derivatives and antiderivative identities, real/complex domains,
one-sided limits, equation/system results, matrices, simultaneous substitution,
notation, culture, malformed input, work budgets, cancellation and recovery.
TSL notation regressions cover the shared superscript alphabet, middle dots,
compound exponents and logarithm order. Independent numerical TSL evaluation of
symbolic results verifies that formatting preserves mathematical meaning,
including inverse-cotangent branches. Mathematical safety regressions also cover
original system domains, exceptional integral parameters and substitution keys
that could otherwise capture internal names.

`build/portable/Test-SymbolicCli.ps1` runs the **published CLI as child processes**
and covers all exposed operation names, exact and approximate results, Unicode,
strict options, JSON parameters, and recovery after failures. Cake runs these
checks as part of `Portable-Validate`, after the existing portable test suite and
ordinary CLI process checks. Desktop and browser dependent changes add their own
actual application-composition and HTTP/browser tests.

## Primary references

- [AngouriMath source and documentation](https://github.com/asc-community/AngouriMath).
- [The pinned 2.6.0 NuGet package, targets and dependencies](https://www.nuget.org/packages/AngouriMath/2.6.0).
- [Exact upstream source revision for the package](https://github.com/asc-community/AngouriMath/tree/ec0d96338e8bfa5d313acc53c03750353497f6fd).
- [Original symbolic-calculations issue #43](https://github.com/PawelTroka/Computator.NET/issues/43).
