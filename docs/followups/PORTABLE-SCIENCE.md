# Shared portable scientific services

Roadmap item 9. Existing MatrixFunctions, NumericalExtensions, Integral, Derivative and FunctionRoot sources are compiled exactly once into the plain net10.0 engine. Windows Core forwards their public identities and continues using those exact implementations and TSL code fragments. No algorithm or tolerance is changed during extraction.

The portable language now imports the real matrix functions and numerical service namespace. The engine adds Integrate, Differentiate and FindRoot methods that validate work budgets, compile each expression/custom-function set once, execute the existing algorithms and unload the owned generated program after completion. CLI expressions/scripts can use the imported matrix functions; desktop and portable clients share the numerical implementation rather than maintaining a second calculator dialect.

```sh
./build.sh --target=Portable-Science-Validate
```

The existing Windows/Linux/macOS workflow runs all old syntax/execution tests and new matrix, numerical-reference, custom-function and rejection tests. Windows regression CI checks the full desktop after extraction. Success must be established on the final head, not inferred from source-level portability.

Remaining scope is explicit: the native GSL/special-function catalog, interactive input, rendering and a complete Mac UI are not yet fully portable. The CLI is not advertised as full desktop parity. This PR implements the shared managed scientific slice; native catalog completion remains a separate integration task with #161's candidate/error handling. Execution budgets on inputs are not wall-clock cancellation or a sandbox. Worker isolation is the next ordered PR.
