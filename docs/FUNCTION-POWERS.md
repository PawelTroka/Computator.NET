# Function powers in TSL

Function-power notation raises the result of a call to the given exponent.
`cosᶻ(z)` means `pow(cos(z), z)`, not `cos(pow(z,z))` or repeated composition.
Real and complex evaluation use the existing `pow` implementations, including
the principal branch of complex exponentiation. Negative powers are reciprocals,
not inverse functions: `cos⁻¹(x)` means `1/cos(x)`.

| Input | Meaning |
| --- | --- |
| `cos²(x)` or `cos(x)²` | `pow(cos(x),2)` |
| `cosᶻ(z)` | `pow(cos(z),z)`, unless `cosᶻ` is a declared name |
| `cos⁻ᶻ(z)` | `pow(cos(z),-z)` |
| `cos⁽ᶻ⁺¹⁾(z)` | `pow(cos(z),(z+1))` |
| `System.Math.Cos⁽ʸ⁾(x)` | `pow(System.Math.Cos(x),(y))` |
| `Probe⁽ʸ⁾(x)` | `pow(Probe(x),(y))` |
| `ψⁿ(x)` | The existing function named `ψⁿ` |
| `ψⁿ⁽ʸ⁾(x)` or `ψⁿ(x)ʸ` | Raise the `ψⁿ` call to `y` |

The editor uses `^` to toggle exponent entry and stores Unicode superscripts.
A literal ASCII caret in imported TSL retains its existing C# XOR meaning.

## Distinguishing identifiers from exponents

Superscript letters are legal identifier characters. Bare symbolic prefixes
are therefore reserved for the explicit, case-sensitive elementary-function
spellings in `PowHandler.SymbolicPowerFunctions`: trigonometric, hyperbolic,
inverse, logarithmic, root and exponential functions and their existing aliases.
Other named/qualified functions use grouped superscripts or postfix notation.
This does not add functions to the portable engine's existing subset.

Declared names take precedence over the shorthand. The compiler protects both
declarations and their references within a source fragment, and callers receive
the direct member declarations from their separate custom-functions fragment.
For example, after defining `static real cosʸ(real x) => x+2;`, `cosʸ(3)` calls
that method and returns 5. `cos⁽ʸ⁾(3)` still explicitly requests a cosine power.
An invalid call to a declared name reports a compilation error rather than
silently selecting the mathematical shorthand.

Nested-type members, parameters and function-local declarations from the custom
fragment do not suppress shorthand in the caller. Names are collected afresh
for each translation; removing a definition cannot leave stale parser state.
An explicit `@cosᶻ` requests a literal identifier. Existing qualified names,
constructor/type contexts and `ψⁿ` remain identifiers.

The function call, arguments and exponent execute once in ordinary `pow`
argument order. Literal text and comments retain the existing protection;
interpolated expressions remain executable. Numeric prefix/postfix powers and
implicit multiplication continue to use the shared TSL pipeline.

## Integration and validation

PR #213 is the symbolic-power follow-up to #176 for issue #208. It targets
`fix/function-call-superscript-powers` so its diff does not repeat the numeric
function-power work. Review #176 first, then retarget and validate the combined
branch before merging. No merge or release is authorized by the tests.

Portable tests cover lowering, real/complex evaluation, custom functions,
ambiguous names, nested calls, literal protection and side effects. The Windows
Core.Tests fixture additionally exercises `ExpressionsEvaluator`, the actual
`ψⁿ` built-in and desktop calculation-mode detection. See the PR for results
at its current commit; adding a test is not proof that its platform run passed.
