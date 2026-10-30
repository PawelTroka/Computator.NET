# Portable engine and scientific CLI

[Overview](../../README.md) · [Documentation](../README.md) · [Developer setup](../DEVELOPING.md) · [Scientific service details](PORTABLE-SCIENCE.md)

`Computator.NET.Engine` and `Computator.NET.Cli` run on modern .NET on Windows, Linux and macOS. They share the production TSL compiler, mathematical notation, constants and managed matrix/numerical implementations with the Windows application. They do not require WinForms, WPF, Windows drawing or native GSL.

Open the single **[Computator.NET.slnx](../../Computator.NET.slnx)** to browse all projects. Portable Cake targets build the portable project graph directly, so they do not restore the Windows application graph. No separate portable solution is required or maintained. The [Engine project file](../../Computator.NET.Engine/Computator.NET.Engine.csproj) records which original Core/DataTypes files are linked into the shared assembly.

## Build and run

Install the SDK selected by [global.json](../../global.json). From the repository root, a direct development run is:

```sh
dotnet run --project Computator.NET.Cli -- --help
dotnet run --project Computator.NET.Cli -- calculate --expression '2x+3' --x 7
```

To run engine tests, publish the CLI and check its process/JSON contracts, use Cake with PowerShell 7 available:

```sh
./build.sh --target=Portable-Validate
# PowerShell equivalent: ./build.ps1 -Target Portable-Validate

dotnet artifacts/portable/computator.dll calculate --expression '2x+3' --x 7
dotnet artifacts/portable/computator.dll integrate --expression 'x*x' --a 0 --b 3 --steps 100
dotnet artifacts/portable/computator.dll differentiate --expression 'x*x' --x 3
dotnet artifacts/portable/computator.dll root --expression 'x*x-2' --a 0 --b 2 --tolerance 1e-10
```

Expected results are `17`, approximately `9`, approximately `6` and approximately `1.41421356237`. Numerical results retain their floating-point representation: the integral example returns `{"result":"8.999999999999998"}`. Each successful published-CLI calculation writes one JSON object with a string result. `dotnet run` may also show SDK build messages; use the published DLL when consuming the CLI as a JSON process.

`Portable-Test` stops after the guarded engine test run; `Portable-Validate` adds publication and CLI process checks. The original syntax regressions and production-code tests run through the portable suite. Windows tests remain relevant when changing shared code used by the desktop.

## Command reference

| Command | Required input | Optional input / defaults |
| --- | --- | --- |
| `calculate` | `--expression <TSL>` | `--x 0`, `--y 0` |
| `complex` | `--expression <TSL>` using `z` | `--real 0`, `--imaginary 0` |
| `script` | `--file <file.tsl>` | Use `write(...)` to produce result text |
| `integrate` | `--expression <TSL> --a <number> --b <number>` | `--steps 10000`; Simpson's method requires an even count from 2 to 1,000,000 |
| `differentiate` | `--expression <TSL> --x <number>` | `--order 1`; supported orders 1 through 10 |
| `root` | `--expression <TSL> --a <number> --b <number>` | `--tolerance 1e-8`, `--iterations 10000`; Brent's method requires a root bracket |

Every command accepts `--functions <file.tslf>`. Numeric options use an invariant decimal point and must be finite. Integration and root bounds must satisfy `a < b`; root tolerance must be greater than 0 and at most 1, with 1 through 1,000,000 iterations. Invalid, duplicated, incomplete or command-inapplicable options are rejected. Use `--help` for the CLI's authoritative option summary; [Program.cs](../../Computator.NET.Cli/Program.cs) and [PortableEngine.Science.cs](../../Computator.NET.Engine/PortableEngine.Science.cs) define these contracts.

### Custom functions and scripts

Save this UTF-8 definition as `functions.tslf`:

```csharp
static real Probe(real x) { return x/2; }
```

Save this as `example.tsl`:

```csharp
write(Probe(6));
```

Then run:

```sh
dotnet artifacts/portable/computator.dll calculate --expression 'Probe(6)' --functions functions.tslf
dotnet artifacts/portable/computator.dll script --file example.tsl --functions functions.tslf
```

Both return `{"result":"3"}`. Files accept an optional UTF-8 BOM and are bounded during reading to 256 KiB and 65,536 characters. Convert older UTF-16 or non-UTF-8 files explicitly. Quote paths containing spaces.

### Results and failures

| Exit code | Stream | Meaning |
| --- | --- | --- |
| `0` | stdout | A JSON object with a string `result`; help is human-readable text instead |
| `1` | stderr | JSON error for invalid input, file access or an execution failure |
| `2` | stderr | JSON compilation error with document-relative diagnostic file/line/column information |

Use TSL `write(...)` for script result text. Ordinary managed Console writes are suppressed during execution and shutdown to preserve the JSON protocol. [Published CLI checks](../../build/portable/Test-Cli.ps1) exercise this behavior, numerical answers, UTF-8 documents, custom functions and recovery after errors.

## Supported capabilities and boundaries

The portable surface includes real/two-variable and complex arithmetic, TSL notation, mathematical constants, custom definitions, managed scripts, the imported matrix functions, qualified `Euclid.*` integer utilities, and the shared integration/differentiation/root services. The scalar adapters (`sin`, `cos`, `tan`, `sqrt`, `exp`, `ln`, `log`, `abs`, `pow`, `floor`, `ceil`, `round`) delegate to .NET's functions. [ScalarFunctions.cs](../../Computator.NET.Engine/ScalarFunctions.cs) defines available scalar spellings; [portable science](PORTABLE-SCIENCE.md) explains shared numerical implementations.

`Euclid.GreatestCommonDivisor(-84,30)` returns `6`, and
`Euclid.Modulus(-7,3)` returns `2`. These qualified calls use the existing pinned
Math.NET library, including its `System.Numerics.BigInteger` overloads and error
contracts. See [integer utilities and edge cases](../FUNCTION-REFERENCE.md#qualified-integer-utilities)
for script examples, exact-integer output and the distinction between `int` and
64-bit `integer` overloads.

The full native GSL/special-function catalog, interactive input dialogs, desktop settings/fonts and chart rendering remain outside this portable surface. An unavailable function produces a compiler diagnostic. The frontend running on macOS/Linux with a Windows API is a separate deployment path. The [reduced Mono desktop](../MONO-DESKTOP.md) is also a separate explicit compatibility target, built from the current shared sources.

All executed code remains trusted, in-process code with the user's permissions. Work budgets and collectible load contexts are **not a sandbox, wall-clock timeout or filesystem/network isolation**. Generated assemblies use unique identities and request unloading when released; dynamic-binder caches can still retain programs until process exit. See [program lifetime](../PROGRAM-LIFETIME.md) for that boundary.

## Boolean operators and absolute values

Conditions, scripts and custom functions support short-circuit logical OR chains such as `true || false || false` and `|x| > 0 || |y| > 0 || |x+y| > 0`. Later operands are evaluated only when the earlier operands are false. Single bars still denote absolute values; nested `||x||` and the supported adjacent expression `|x||y|` retain their mathematical meanings.

Double bars can be ambiguous when an absolute value sits between other expressions. Complete Boolean and comparison chains follow C# logical precedence: for Boolean variables `a` and `b`, `a||b||+z>1` means `a || b || (+z > 1)`. A numerical comparison previously written as `x||y||+z>1` must use explicit multiplication or function notation, for example `x*abs(y)+z>1`.

Whole arithmetic operands retain established nested-absolute-value forms, including `x||y||+z`, `x||y||*z` and `x||y||+z+1`. Grouping or a function argument keeps an inner mathematical expression separate from the surrounding Boolean chain, as in `(x||y||+z)>1 || a || b` or `abs(x||y||+z)>1 || a || b`. Prefer explicit multiplication and `abs(...)` when combining this notation with Boolean conditions. The parser classifies syntax before compiling types; it does not infer whether an arbitrary variable or function returns a Boolean or a number.

The [portable CI workflow](../../.github/workflows/portable.yml) identifies the actual tested operating systems/runners. Complete desktop parity, native architecture packaging and signing remain separate work from this managed CLI.
