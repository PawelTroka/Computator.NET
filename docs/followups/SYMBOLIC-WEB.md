# Symbolic browser and API workflow

[Documentation](../README.md) · [Mathematics and notation](../SYMBOLIC-CALCULATIONS.md)

Open **Symbolic calculations** at `/symbolic-calculations`. Choose an operation,
enter an expression, and select **Calculate** or press **Ctrl+Enter** (Cmd+Enter
on macOS). **Load example** supplies the relevant fields for the selected
operation. Inputs survive navigation between tools within the current page
session; a reload resets this draft.

The screen exposes the shared engine's sixteen operations, variable and order,
real/complex result domain, exact integration bounds, one-sided limits, Taylor
center/degree, systems, substitutions and numerical precision. Equation systems
use one equation per line with comma-separated target variables. Substitutions
use one `name = expression` per line and apply simultaneously. Numerical
evaluation also accepts optional substitutions.

Only fields used by the selected operation are submitted. Results show their
`exact`, `approximate`, `unevaluated` or `undefined` status, mathematical warnings,
plain text and available LaTeX. Both representations can be copied. LaTeX and
warnings are displayed as text. Editing inputs, changing operation, cancelling,
or leaving the page aborts an outstanding request; late responses cannot replace
a later answer.

The mathematical grammar is separate from numerical TSL. In particular,
`log(base, argument)` puts the base first, `log(x)` is base ten and `ln(x)` is
natural logarithm. Custom C# function definitions are not applied. See the
[shared mathematical guide](../SYMBOLIC-CALCULATIONS.md) for domains, exactness,
unsupported cases and work limits.

## JSON endpoint

Both the Windows API and portable local host accept `POST /api/v2/symbolic` with
`Content-Type: application/json`. The request is the shared `SymbolicRequest`;
the response is the complete `SymbolicResult` described in the mathematical guide.

```json
{
  "operation": "differentiate",
  "expression": "sin(x^2)",
  "variable": "x",
  "order": 1,
  "domain": "complex"
}
```

The result is the exact expression `2 * x * cos(x ^ 2)`, with its LaTeX and status.
HTTP success means a result was returned; inspect the status to distinguish a
completed operation from an unresolved or undefined expression.

The request uses the existing protected local proxy and Windows session-token
policy, or the portable host's loopback/Host/Origin checks. Every computation
runs in the existing bounded worker queue with its process deadline, output
limits, cancellation and cleanup. Cheap mathematical validation happens before
allocating the worker. The existing numerical `/api/v2/execute` operation contract
is unchanged.

Errors use JSON problem details: invalid mathematical requests return 400,
oversized HTTP bodies 413, unsupported media 415, exhausted worker capacity 429,
worker failure 502, and exceeded deadlines 504. The portable status endpoint
advertises `symbolic: true` and the actual `symbolicOperations` list separately
from the existing numerical operations.

## Verification

The existing Cake targets discover the added tests:

```sh
# Published portable host, real HTTP/process checks and real browser workflows.
./build.sh --target=PortableWeb-Validate --portable-web-rid=linux-x64

# Frontend interactions with controlled API responses.
./build.sh --target=Web-Validate
```

On Windows, `./build.ps1 -Target ApiTests` includes the new protected-Kestrel and
owned-worker cases in `SymbolicApiTests`. These verify exact mathematical values,
reusable algebraic results, Unicode and simultaneous substitutions, calculus,
precision, domain handling, unresolved integrals, request errors, authentication
and actual worker cancellation.

`portable-tests/symbolic.spec.ts` executes all sixteen example operations through
the published UI and real engine. Additional HTTP round trips check independent
exact values, domains, invalid input and request boundaries. Focused browser
tests cover keyboard submission, retained drafts, relevant-field serialization,
optional substitutions, cancellation races, inert result text and error recovery.

The [recorded before/after evidence](../evidence/symbolic-web/README.md) identifies
the actual captures and separates local verification from remaining platform CI.
