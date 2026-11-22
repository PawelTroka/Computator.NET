# Symbolic calculations in the desktop application

The **Symbolic calculations** tab uses the shared `Computator.NET.Engine.Symbolic`
computer algebra service on both modern Windows and the explicit Mono desktop
target. It replaces the disabled placeholder. The existing numerical calculations
and coefficient-based `Polynomial` APIs remain separate tools.

## Use the tab

1. Select **Symbolic calculations** and enter an expression in the shared editor
   above the tabs. Both `x^2` and `x²` are accepted. Use `x*(x+1)` for a product
   involving a named variable; an unknown function call is rejected.
2. Choose an operation and fill in the parameters it displays. **Load example**
   supplies an expression and parameters for every operation.
3. Select **Calculate**, the existing toolbar **Run**, or press **Enter** in the
   expression editor. Enter also runs from a single-line parameter; use
   **Ctrl+Enter** from a multiline field.
4. Read the **Result** and copyable **LaTeX** source. The status distinguishes
   exact, approximate, unevaluated and undefined outcomes. Warnings are displayed
   with the result; an unevaluated expression is not presented as a solved answer.

The symbolic parser reads mathematical expressions. It does not execute the
contents of the Custom Functions editor, scripts, C# statements or arbitrary
methods. Numeric calculation modes do not reinterpret the symbolic input; the
status bar shows `Mode[Symbolic]` while this tab is selected.

Symbolic logarithms use `ln(x)` for the natural logarithm, `log(x)` for base 10,
and `log(base, x)` for an explicit base. The two-argument order differs from the
numerical TSL `log(x, base)` convention, so this reminder is also visible in the tab.

## Operations and inputs

| Operation | Input or parameter example | Expected mathematical meaning |
| --- | --- | --- |
| Simplify | `sin(x)^2 + cos(x)^2` | `1` |
| Expand | `(x+1)^3` | `x³ + 3x² + 3x + 1` |
| Factor | `x^2-1` | `(x−1)(x+1)` |
| Differentiate | `sin(x^2)`, variable `x`, order `1` | `2x cos(x²)` |
| Indefinite integral | `x^2`, variable `x` | `x³/3 + C` |
| Definite integral | `x^2`, variable `x`, bounds `0`, `3` | `9` |
| Limit | `sin(x)/x`, point `0`, direction `both` | `1` |
| Solve equation | `x^2=2`, variable `x` | Exact solutions `−√2`, `√2` |
| Substitute | `x^2+y`; lines `x = 2` and `y = 3` | `7` |
| Numerical value | `sqrt(2)`, decimal digits `30` | A decimal approximation; replacements are optional |
| Taylor polynomial | `exp(x)`, point `0`, degree `5` | Polynomial through the `x⁵` term |
| Solve equation system | Lines `x+y=3`, `x-y=1`; unknowns `x, y` | `x=2`, `y=1`; the top expression is ignored |
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
  recovery, and captures the visible native window.
- Mono's `mono-symbolic-calculations-and-native-input` acceptance case proves an
  actual XTEST click on the displayed Calculate button, all operation examples,
  Run routing, invalid input recovery, visible screen capture and shutdown. The
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
