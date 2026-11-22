# Symbolic calculations in the desktop application

The **Symbolic calculations** tab uses the shared `Computator.NET.Engine.Symbolic`
computer algebra service on both modern Windows and the explicit Mono desktop
target. It replaces the disabled placeholder. The existing numerical calculations
and coefficient-based `Polynomial` APIs remain separate tools.

## Use the tab

1. Select **Symbolic calculations** and enter an expression in the shared editor
   above the tabs using TSL notation: `x²`, `2·x` or `x⁽ʸ⁺¹⁾`. Type **^** to
   toggle superscript entry and type it again to return to ordinary characters.
   Typing **\*** inserts **·**. Use `x·(x+1)` for an explicit product involving a
   named variable; an unknown function call is rejected.
2. Choose an operation and fill in the parameters it displays. **Load example**
   supplies an expression and parameters for every operation.
3. Select **Calculate**, the existing toolbar **Run**, or press **Enter** in the
   expression editor. Enter also runs from a single-line parameter; use
   **Ctrl+Enter** from a multiline field.
4. Read the **Result** in native TSL notation and copyable **LaTeX** source. An
   algebraic result such as `3·x²` can be copied and pasted back into the shared
   expression editor for another operation. The status distinguishes
   exact, approximate, unevaluated and undefined outcomes. Warnings are displayed
   with the result; an unevaluated expression is not presented as a solved answer.

The symbolic parser reads mathematical expressions. It does not execute the
contents of the Custom Functions editor, scripts, C# statements or arbitrary
methods. Numeric calculation modes do not reinterpret the symbolic input; the
status bar shows `Mode[Symbolic]` while this tab is selected.

Symbolic logarithms follow TSL: `ln(x)` is the natural logarithm, `log10(x)` is
base 10, and `log(x, base)` puts the argument first. For example, `log(8, 2)`
returns `3`. The same reminder is visible in the tab.

Pasted or imported ASCII expressions such as `x^2` and `2*x` remain accepted as
compatibility input. The examples and returned algebraic expressions use native
TSL. The shared engine formats results once; the desktop displays its result
unchanged. When a power cannot be expressed safely with the available superscript
alphabet, the engine uses TSL's `pow(base, exponent)` form.

Compound exponents use the editor's existing Unicode alphabet, including `⁽⁾`
for grouping, `˙` for multiplication, `˸` for division, `ᱸ` for a decimal point
and `Ꞌ` for absolute-value bars. For example, `x²˙ʸ` means `x` raised to `2·y`,
`x⁰ᱸ⁵` is a square root, and `xᶜᵒˢ⁽ʸ⁾` has `cos(y)` as its exponent. Spaces
inside a superscript expression are preserved as whitespace. Function powers
such as `sin²(x)` raise the complete function value to that power.
A following postfix power applies to that whole result: `sin³(PI/6)²` is
`(sin(PI/6)³)²`, which equals `1/64`.

## Operations and inputs

| Operation | Input or parameter example | Expected mathematical meaning |
| --- | --- | --- |
| Simplify | `sin²(x) + cos²(x)` | `1` |
| Expand | `(x+1)³` | `x³ + 3·x² + 3·x + 1` |
| Factor | `x²-1` | `(x−1)·(x+1)` |
| Differentiate | `sin(x²)`, variable `x`, order `1` | `2·x·cos(x²)` |
| Indefinite integral | `x²`, variable `x` | `x³/3 + C` |
| Definite integral | `x²`, variable `x`, bounds `0`, `3` | `9` |
| Limit | `sin(x)/x`, point `0`, direction `both` | `1` |
| Solve equation | `x²=2`, variable `x` | Exact solutions `−√2`, `√2` |
| Substitute | `x²+y`; lines `x = 2` and `y = 3` | `7` |
| Numerical value | `sqrt(2)`, decimal digits `30` | A decimal approximation; replacements are optional |
| Taylor polynomial | `exp(x)`, point `0`, degree `5` | Polynomial through the `x⁵` term |
| Solve equation system | Lines `2·x+y=5`, `x-y=1`; unknowns `x, y` | `x=2`, `y=1`; the top expression is ignored |
| Matrix determinant | `[[1,2],[3,4]]` | `−2` |
| Matrix inverse | `[[1,2],[3,4]]` | Exact inverse, when it exists |
| Matrix transpose | `[[1,2,3],[4,5,6]]` | A 3×2 transpose |
| Matrix trace | `[[1,2],[3,4]]` | `5` |

The domain selector distinguishes real and principal complex calculations. It
does not assert that a variable is positive. Supported operations depend on the
shared engine's algorithms; no general symbolic system can promise a closed form
for every expression. Preserve and read domain warnings and unevaluated results.

Replacement entries and system equations accept one entry per line (a semicolon
also separates entries). Unknowns use commas. Duplicate replacement names produce
an error instead of silently overwriting a value. Hidden inputs from another
operation are excluded from the request.

## Responsiveness and lifetime

The presenter captures the displayed inputs and runs the shared service on a
background task. The engine applies input, structural, output and cooperative
time limits. **Cancel** or editing the input requests cancellation; the tab
remains responsive. Calculate stays disabled until the worker finishes, avoiding
an accumulation of abandoned computations. This is cooperative cancellation, not
a process-level hard deadline.

Every request has a generation number. Editing an expression or parameter,
requesting cancellation or disposing the tab invalidates its result. A late
answer or error cannot replace the current input's status. All UI updates go
through the displayed control's dispatcher, and disposal cancels work, detaches
events and removes only the presenter's own toolbar action. Invalid input clears
an earlier answer; the same tab can immediately recover on the next valid input.

## Verification

- `SymbolicCalculationsPresenterTests` exercises asynchronous dispatch, snapshots,
  cancellation, stale answers and errors, repeated Run, recovery, example loading,
  disposal and action ownership with a controlled service.
- `SymbolicExpressionRoutingTests` verifies that symbolic typing and Enter bypass
  numeric TSL classification, including matrices, equations, free variables and
  incomplete input; switching back restores numeric routing.
- `SymbolicCalculationsWorkflowTests` opens the actual composed `MainView`, uses
  its displayed expression editor, native buttons, toolbar command and Enter
  path, and executes the actual shared engine. It checks independent known
  mathematical answers, all operation examples, structured input, errors and
  recovery, and captures the visible native window. Native editor key events,
  clipboard paste of compound TSL exponents and copy/paste of a calculated TSL
  result are exercised through the actual displayed controls.
- Mono's `mono-symbolic-calculations-and-native-input` acceptance case proves an
  actual XTEST click on the displayed Calculate button and real keystrokes that
  turn `x^2^+2*x` into `x²+2·x`, followed by Enter and the known result `15` at
  `x=3`. It also checks TSL clipboard round trips, all operation examples, Run
  routing, invalid input recovery, visible screen capture and shutdown. The
  evidence validator requires this case and at least 56 total passing cases.

```sh
# Native Windows execution of the project fixtures:
dotnet test Computator.NET.Core.Tests/Computator.NET.Core.Tests.csproj -c Release \
  --filter 'FullyQualifiedName~SymbolicCalculationsPresenterTests|FullyQualifiedName~SymbolicExpressionRoutingTests'
dotnet test Computator.NET.Desktop.Tests/Computator.NET.Desktop.Tests.csproj -c Release \
  --filter FullyQualifiedName~SymbolicCalculationsWorkflowTests

# Existing native Linux/Mono application acceptance:
./build.sh --target=Mono-Validate
```

Windows cross-compilation is not native Windows execution. Mono source and
package acceptance remain distinct, and neither proves physical-machine input,
accessibility or DPI behavior on every supported platform.

The [native before/after evidence](evidence/symbolic-desktop/README.md) records
the original disabled tab, the running implementation, source and binary hashes,
and the focused Mono test results captured for this change.
