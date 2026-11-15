# Learn TSL: a practical first session

[Home](../README.md) · [Documentation index](README.md) · [Portable CLI reference](followups/PORTABLE-ENGINE.md) · [Desktop example scripts](../Computator.NET.Core/TSL%20Examples/_Scripts/Example00_TypesAndSyntax.tsl)

TROKA Scripting Language (TSL) accepts familiar mathematical notation and C#-style statements. This tutorial starts with a calculation, progresses to reusable functions and matrices, and finishes with calculus and equation solving. Each command uses the **portable .NET 10 CLI**, so the core lessons can be followed on Windows, Linux or macOS without a graphical desktop. The same expressions and the supported scripts can also be used in the full Windows application.

**Prerequisites:** clone the repository, install the SDK selected by [`global.json`](../global.json), open a terminal at the repository root, and use an editor that saves UTF-8 text. No native GSL library or graphics stack is needed for these exercises.

## 1. Calculate with variables and mathematical notation

Run:

~~~sh
dotnet run --project Computator.NET.Cli -- calculate --expression '2x+3' --x 7
dotnet run --project Computator.NET.Cli -- calculate --expression 'sin(PI/2)+x²' --x 3
~~~

The mathematical results are `17` and `10`, respectively. The first example uses implicit multiplication (`2x`), and the second uses a superscript (`x²`). TSL also accepts ordinary multiplication (`x*x`), which is convenient for source files. `PI` is a built-in mathematical constant.

The portable CLI returns a JSON object such as `{"result":"17"}` rather than printing a bare number. On the first run, `dotnet run` may also print build messages; use the [published CLI](followups/PORTABLE-ENGINE.md#build-and-run) when parsing stdout as a strict JSON protocol.

**Try it:** change `--x 3` to `--x 4` in the second command. The result should be `17`.

## 2. Evaluate complex numbers

~~~sh
dotnet run --project Computator.NET.Cli -- complex --expression 'z²' --real 1 --imaginary 1
~~~

For `z = 1 + i`, squaring yields `2i` (real part 0, imaginary part 2). The CLI formats the complex value using .NET's invariant culture. You can also try `sin(z)` or `exp(z)`.

Use `complex` for expressions with a complex input `z`. The `calculate` command is for a real-valued expression with `x` and optionally `y`. An operation unsupported by the portable scalar catalog is a compilation error, not a reason to silently substitute another function.

## 3. Write your first script

Create a UTF-8 file named `first.tsl` in the repository root:

~~~csharp
real width = 3;
real height = 4;
var diagonalSquared = width*width + height*height;
write(diagonalSquared);
~~~

Then execute it:

~~~sh
dotnet run --project Computator.NET.Cli -- script --file first.tsl
~~~

Expected result: `{"result":"25"}`. `real` declares a floating-point number. `var` infers the type from its initializer. A script can also use conditions, loops, and local functions.

Replace the contents of `first.tsl` with a local function:

~~~csharp
var f(real x) = x*x+1;
write(f(3));
~~~

The script now returns `{"result":"10"}`. The shortcut `var f(real x) = ...` is TSL function syntax; it is not a standard C# local function declaration.

**Important distinction:** the portable script exposes `write(...)` to collect result text. Desktop-only `show(...)` dialogs, `read(...)` prompts, and `plot(...)` windows are not available in the headless CLI. For graphics and interactive desktop tutorials, use the [original example scripts](../Computator.NET.Core/TSL%20Examples/_Scripts/Example07_FunctionsAndPlotting.tsl) in the Windows desktop.

## 4. Reuse a custom function

Create another UTF-8 file named `formulas.tslf`:

~~~csharp
static real OffsetSquare(real x) { return x*x+1; }
~~~

Invoke it from a real calculation:

~~~sh
dotnet run --project Computator.NET.Cli -- calculate --expression 'OffsetSquare(3)' --functions formulas.tslf
~~~

Expected result: `{"result":"10"}`. The same definition can be reused from a script. Save this as `using-formulas.tsl`:

~~~csharp
write(OffsetSquare(3));
~~~

~~~sh
dotnet run --project Computator.NET.Cli -- script --file using-formulas.tsl --functions formulas.tslf
~~~

Both routes use the same `.tslf` source and return `10`. This is a useful way to separate reusable mathematical definitions from calculations.

## 5. Work with a matrix

Save the following as `matrix.tsl`:

~~~csharp
var a = matrix({{1,2},{3,4}});
write(det(a));
~~~

~~~sh
dotnet run --project Computator.NET.Cli -- script --file matrix.tsl
~~~

The determinant is `-2` because `1*4 - 2*3 = -2`. Matrices use double braces to group rows. For larger examples, consult [matrix operations](../Computator.NET.Core/TSL%20Examples/_Scripts/Example04_MatrixOperations.tsl) and [linear equation solving](../Computator.NET.Core/TSL%20Examples/_Scripts/Example08_SolvingEquations.tsl).

The examples in that historical folder were written for the **full desktop runtime**, including native special functions and graphical APIs. Copy only supported managed operations when working through the portable CLI.

## 6. Integrate, differentiate, and find a root

These commands compile the expression once, then use the shared managed numerical methods:

~~~sh
dotnet run --project Computator.NET.Cli -- integrate --expression 'x*x' --a 0 --b 3 --steps 100
dotnet run --project Computator.NET.Cli -- differentiate --expression 'x*x' --x 3
dotnet run --project Computator.NET.Cli -- root --expression 'x*x-2' --a 0 --b 2
~~~

| Task | Mathematical answer | Notes |
| --- | --- | --- |
| Integrate `x²` from 0 to 3 | `9` | Composite Simpson's method; the CLI may return `8.999999999999998` |
| Differentiate `x²` at 3 | `6` | Numerical derivative, approximately 6 |
| Solve `x² = 2` on [0, 2] | `√2 ≈ 1.41421356237` | Brent root finder requires a bracket |

The integral's step count must be **even** (2–1,000,000). Bounds for `integrate` and `root` must be finite and increasing. Numerical output is approximate; do not compare it as an exact decimal string. The [CLI reference](followups/PORTABLE-ENGINE.md#command-reference) lists all input constraints, including derivative order and root tolerance.

**Challenge:** change the root problem to `x*x-9` with bounds `0` and `5`. The answer should be approximately `3`. Then try a non-bracketing interval such as `0` to `2` and observe the error rather than expecting an invented root.

## 7. Troubleshooting and next steps

- **Compilation diagnostic:** confirm the function is available to the portable engine and that `--functions` points to the correct file. The JSON diagnostic includes the file, line, and column.
- **Invalid source file:** save in UTF-8. A UTF-8 BOM is allowed; UTF-16 is not. File input is limited to 256 KiB and 65,536 characters.
- **Different numerical last digits:** floating-point approximation is expected, especially for integrals and derivatives.
- **A command needs a window or user interaction:** use the full Windows desktop. Portable CLI scripts execute trusted code in-process; they are **not sandboxed**. Run only code you trust.

Continue with the existing [TSL syntax examples](../Computator.NET.Core/TSL%20Examples/_Scripts/Example00_TypesAndSyntax.tsl), the [portable scientific services](followups/PORTABLE-SCIENCE.md), and the [built-in function reference](FUNCTION-REFERENCE.md). Those resources cover more advanced operations without requiring you to guess which features exist on a particular platform.
