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
| `System.Math.Cosʸ(x)` | `pow(System.Math.Cos(x),y)` when no exact `Cosʸ` member exists |
| `Probe⁽ʸ⁾(x)` | `pow(Probe(x),(y))` |
| `Probeʸ(x)` | `pow(Probe(x),y)` when that is the only valid name/exponent split |
| `ψⁿ(x)` | The existing function named `ψⁿ` |
| `ψⁿ⁽ʸ⁾(x)` or `ψⁿ(x)ʸ` | Raise the `ψⁿ` call to `y` |

The editor uses `^` to toggle exponent entry and stores Unicode superscripts.
A literal ASCII caret in imported TSL retains its existing C# XOR meaning.

## Distinguishing identifiers from exponents

Superscript letters are legal identifier characters. During evaluation, bare
symbolic calls are resolved against the complete program, including built-ins,
custom definitions, local scopes and referenced assemblies. Qualified and
Unicode function names use the same resolution rule. This does not add
functions to the portable engine's existing subset.

Existing complete callable or value names take precedence over the shorthand.
For example, after defining `static real cosʸ(real x) => x+2;`, `cosʸ(3)` calls
that method and returns 5. `cos⁽ʸ⁾(3)` still explicitly requests a cosine power.
An invalid call to a declared name reports a compilation error rather than
silently selecting the mathematical shorthand. Inherited methods and methods
from referenced assemblies receive the same protection. Constructors and type
names remain unchanged; a type named `cosᶻ` does not itself define a callable
method named `cosᶻ`.

Names in unrelated local or nested-type scopes do not suppress a power in the
caller. Resolution uses each new compilation; removing a definition cannot
leave stale parser state. An explicit `@cosᶻ` requests a literal identifier.
Dynamic calls retain runtime binding to their complete member names.

When no complete name exists, each possible trailing superscript-letter suffix
is decoded as an exponent identifier. The base call and resulting `pow` must
bind successfully. For example, `cosᴾᴵ(x)` uses the constant `PI`. Exactly one
valid split is required. If `F` and `Fⁿ` both exist and both `nm` and `m` are in
scope, `Fⁿᵐ(x)` is ambiguous: it could mean `pow(F(x),nm)` or `pow(Fⁿ(x),m)`.
Use `F⁽ⁿᵐ⁾(x)`, `Fⁿ⁽ᵐ⁾(x)` or postfix notation to choose the intended meaning.
Unresolved or ambiguous forms produce ordinary compilation diagnostics.

Explicit prefix boundaries retain their existing meaning. In particular,
`Probeʸ⁺¹(x)` means the numeric power of the complete callable `Probeʸ`; it is
an error if that callable does not exist. Write `Probe⁽ʸ⁺¹⁾(x)` for the compound
exponent `y+1`. The conventional elementary-function compound spellings added
by #213, including `cosᶻ⁺¹(z)` and `cos²˙ᶻ(z)`, remain supported. Grouping is the
uniform spelling that works for arbitrary function names.

The function call, arguments and exponent execute once in ordinary `pow`
argument order. Literal text and comments retain the existing protection;
interpolated expressions remain executable. Numeric prefix/postfix powers and
implicit multiplication continue to use the shared TSL pipeline.

## Integration and validation

PR #213 is the initial symbolic-power follow-up to #176 for issue #208. The
semantic binding correction builds on #213, preserving its numeric, compound,
grouped and literal-protection work. Review the dependent branches in order and
revalidate after retargeting. No merge or release is authorized by the tests.

`TslCompiler`'s original overloads retain syntax-only translation for editor
previews and calculation-mode analysis. The desktop and portable evaluators
select deferred function-name resolution through its additional overload, then
call the shared `FunctionPowerResolver` immediately before Roslyn emits the
complete program. Programs without candidate symbolic calls avoid the semantic
pass. Nested candidates are visited from the inside out. A changed tree is
rebound when necessary, so a variable whose type is inferred from one power
can be used by a later symbolic call. Candidate selection waits when a declared
value still has an unresolved inferred type, so inference cannot hide another
valid split. Each replacement consumes an exponent suffix; unchanged trees
terminate the pass, and the program is emitted once.

Portable tests cover lowering, real/complex evaluation, custom functions,
ambiguous names, nested calls, literal protection and side effects. The Windows
Core.Tests fixture additionally exercises `ExpressionsEvaluator`, the actual
`ψⁿ` built-in and desktop calculation-mode detection. See the PR for results
at its current commit; adding a test is not proof that its platform run passed.
