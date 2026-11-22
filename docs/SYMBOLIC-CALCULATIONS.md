# Symbolic calculations

[Overview](../README.md) · [Documentation](README.md) · [Portable CLI](followups/PORTABLE-ENGINE.md)

The shared `Computator.NET.Engine.Symbolic` service performs algebra on mathematical
expressions. It returns an exact expression where the selected algorithm can find
one, an explicit numerical approximation when requested, or a visible unresolved
expression. It does not compile the input as C# or execute TSL scripts.

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
| `simplify` | `1/3 + 2/3`, `sin(x)^2 + cos(x)^2` | Algebraic simplification |
| `expand` | `(x+y)^3` | Expand products and powers |
| `factor` | `x^2-1` | Factor the expression where supported |
| `differentiate` | `sin(x^2)` | Variable; derivative order 1–20 |
| `integrate` | `x^2` | Variable; symbolic antiderivative including an arbitrary additive constant |
| `definite-integrate` | `x^2` | Variable and symbolic lower/upper bounds |
| `limit` | `sin(x)/x` | Variable, approach point, and `both`, `left` or `right` |
| `solve` | `x^2=2`, `x^2-2`, `x^2<1` | Solve for one variable in the selected domain; a plain expression is equated to zero |
| `substitute` | `x^2+y` | Simultaneous variable-to-expression replacements |
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

## Mathematical input

Use `^` for a power, `/` for exact rational division, `*` for multiplication,
parentheses for grouping, and names such as `sin`, `cos`, `exp`, `ln`, `sqrt`,
`abs`, `pi`, `e` and `i`. Implicit multiplication such as `2x` and Unicode
mathematical notation are accepted through the symbolic normalizer. Explicit
operators remain the clearest way to disambiguate an expression.

**Logarithm convention:** `ln(x)` is natural logarithm, `log(x)` and `log10(x)`
are base 10, and the symbolic two-argument form is **`log(base, argument)`**.
For example, `log(2,8)` is `3`. This order is deliberately the CAS's canonical
input/output syntax and differs from numerical TSL's `log(argument, base)`.
Use `ln(argument)/ln(base)` when moving formulas between the two grammars.

Function powers can be written `sin(x)^2` or `sin²(x)`. A bare function name is
not a free symbolic variable; unknown functions are rejected instead of silently
turning `f(x)` into multiplication. Use an explicit `*` for a variable followed
by a parenthesized factor, such as `x*(x+1)`. Elementary absolute values can be
written `abs(x)` or `|x|`. Indexed variables use an underscore, for example `x_2`.

Use a decimal **point**, independently of the application or operating-system
culture. Matrix elements are comma-separated inside nested brackets.
Equation systems are separate expressions and a list of target variables; their
variable order determines the solution column order. The symbolic grammar is
separate from custom C# function declarations and imperative TSL scripts.

Substitution is simultaneous: replacing `x` by `y` and `y` by `x` exchanges the
names. It does not first replace `x`, then replace the newly inserted `y`.

## CLI examples

Build and verify the published application with the repository's Cake entry point:

```sh
./build.sh --target=Portable-Validate
```

Then run symbolic commands through the published CLI:

```sh
dotnet artifacts/portable/computator.dll symbolic --operation simplify --expression '1/3+2/3'
dotnet artifacts/portable/computator.dll symbolic --operation expand --expression '(x+y)^3'
dotnet artifacts/portable/computator.dll symbolic --operation differentiate --expression 'x³' --order 3
dotnet artifacts/portable/computator.dll symbolic --operation integrate --expression 'sin(x)'
dotnet artifacts/portable/computator.dll symbolic --operation definite-integrate --expression 'x^2' --lower 0 --upper 3
dotnet artifacts/portable/computator.dll symbolic --operation limit --expression 'sin(x)/x' --point 0
dotnet artifacts/portable/computator.dll symbolic --operation solve --expression 'x^2=2' --domain real
dotnet artifacts/portable/computator.dll symbolic --operation substitute --expression 'x+y' --substitutions '{"x":"1/3","y":"2/3"}'
dotnet artifacts/portable/computator.dll symbolic --operation evaluate --expression 'pi' --precision 40
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
| `result` | Plain mathematical text; scalar expressions can be reused as input, while systems use the separate equations/variables fields |
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
