# Shared portable scientific services

Roadmap item 9. Existing MatrixFunctions, NumericalExtensions, Integral, Derivative and FunctionRoot sources are compiled exactly once into the plain net10.0 engine. Windows Core forwards their public identities and continues using those exact implementations and TSL code fragments. No algorithm or tolerance is changed during extraction.

The portable language now imports the real matrix functions and numerical service namespace. The engine adds Integrate, Differentiate and FindRoot methods that validate work budgets, compile each expression/custom-function set once, execute the existing algorithms and request unloading of the owned generated context after completion. Runtime binder caches can retain dynamic programs until process exit; see [the measured lifetime limitation](../PROGRAM-LIFETIME.md). CLI expressions/scripts can use the imported matrix functions; desktop and portable clients share the numerical implementation rather than maintaining a second calculator dialect.

```sh
./build.sh --target=Portable-Science-Validate
```

The existing Windows/Linux/macOS workflow runs all old syntax/execution tests and new matrix, numerical-reference, custom-function and rejection tests. Windows regression CI checks the full desktop after extraction. Success must be established on the final head, not inferred from source-level portability.

## Numerical CLI commands

The published `computator.dll` exposes these existing engine methods directly; there is no duplicate implementation:

```sh
dotnet artifacts/portable/computator.dll integrate --expression 'x*x' --a 0 --b 3 --steps 100
dotnet artifacts/portable/computator.dll differentiate --expression 'x*x' --x 3 --order 1
dotnet artifacts/portable/computator.dll root --expression 'x*x-2' --a 0 --b 2 --tolerance 1e-10 --iterations 100
```

Expected results are 9, approximately 6 and approximately 1.41421356237. Every command accepts `--functions path.tslf`, including file paths with spaces or Unicode. `calculate`, `complex` and `script` remain supported. `--help` describes options and defaults. Integration uses Simpson's method (default 10,000 steps, even 2 through 1,000,000), differentiation accepts orders 1 through 10, and root finding uses Brent's method (default tolerance 1e-8 and 10,000 iterations). Root intervals must bracket a root; convergence is not guaranteed for arbitrary functions.

A successful command writes a JSON object containing a string `result` to stdout. Compilation errors write structured diagnostics to stderr and exit 2; other errors exit 1. Numeric arguments are finite invariant-culture values, and options that belong to another command are rejected rather than silently ignored. UTF-8 files accept an optional UTF-8 BOM, reject malformed UTF-8/UTF-16, and are bounded during the actual read to 256 KiB and 65,536 characters. Export/convert older non-UTF-8 files explicitly before using them here.

`Portable-Validate` and the existing actual Windows/Linux/macOS CI matrix run `build/portable/Test-Cli.ps1` against the published CLI. It checks reference answers, custom functions, Unicode documents, invalid command/option/numeric/file inputs, structured errors and recovery. Both output pipes are drained asynchronously with bounded waits so a broken CLI cannot defeat the test timeout by keeping stderr open.

Remaining scope is explicit: the native GSL/special-function catalog, interactive input, rendering and a complete Mac UI are not yet fully portable. The CLI is not full desktop parity. Execution work budgets are not wall-clock cancellation or a sandbox. CLI code is trusted, in-process code; worker-backed web execution is a separate path. These numerical commands do not port GSL, invent symbolic algebra, or change the existing algorithms/tolerances.
