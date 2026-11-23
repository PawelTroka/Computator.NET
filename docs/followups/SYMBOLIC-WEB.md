# Symbolic browser and API workflow

[Documentation](../README.md) · [Mathematics and notation](../SYMBOLIC-CALCULATIONS.md)

Open **Symbolic calculations** at `/symbolic-calculations`. Choose an operation,
enter an expression, and select **Calculate** or press **Ctrl+Enter** (Cmd+Enter
on macOS). **Load example** supplies the relevant fields for the selected
operation. Inputs survive navigation between tools within the current page
session; a reload resets this draft.

Expressions and results use TSL notation: `2·x²`, `x⁻²`, `x⁽ⁿ⁺¹⁾` and `sin²(x)`.
The **·**, **²** and **³** insertion buttons replace the current selection or
insert at the caret, then return focus to the expression or equation editor.
Compound superscripts can be pasted directly. The symbolic input also accepts
ASCII `2*x^2` and `x^(n+1)`. The engine produces the native TSL result itself;
the browser displays and copies that exact text. Exponents without a complete
superscript spelling use `pow(base, exponent)`.

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

Two-argument `log(argument, base)` follows the numerical TSL convention:
`log(8, 2)` is `3`. Unary `log(x)` is base ten and `ln(x)` is the natural logarithm.
This screen handles mathematical TSL expressions; custom C# function definitions
and scripts are not applied. See the
[shared mathematical guide](../SYMBOLIC-CALCULATIONS.md) for domains, exactness,
unsupported cases and work limits.

## JSON endpoint

Both the Windows API and portable local host accept `POST /api/v2/symbolic` with
`Content-Type: application/json`. The request is the shared `SymbolicRequest`;
the response is the complete `SymbolicResult` described in the mathematical guide.

Both hosts read the same strict UTF-8 JSON contract. Expressions, bounds, points,
variables, equation entries and substitution values must be JSON strings;
`order` and `precision` must be integer JSON numbers. Nulls, unknown fields and
duplicate members are rejected with 400 problem details before starting a worker.
Request field names remain case-insensitive, so `expression` and `Expression`
refer to the same field and cannot both occur. Mathematical variable names are
case-sensitive: `x` and `X` are distinct substitutions. Duplicate names are checked
after JSON escape decoding, so `x` and `\u0078` cannot overwrite one another.

```json
{
  "operation": "differentiate",
  "expression": "sin(x²)",
  "variable": "x",
  "order": 1,
  "domain": "complex"
}
```

The result is the exact TSL expression `2·x·cos(x²)`, with its LaTeX and status.
HTTP success means a result was returned; inspect the status to distinguish a
completed operation from an unresolved or undefined expression.

The request uses the existing protected local proxy and Windows session-token
policy, or the portable host's loopback/Host/Origin checks. Every computation
runs in the existing bounded worker queue with its process deadline, output
limits, cancellation and cleanup. Cheap mathematical validation happens before
allocating the worker. The existing numerical `/api/v2/execute` operation contract
is unchanged.

Invalid mathematical requests return 400 with JSON problem details. Other HTTP
statuses identify oversized bodies (413), unsupported media (415), exhausted
worker capacity (429), worker failure (502), and exceeded deadlines (504). The portable status endpoint
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
optional substitutions, insertion caret/selection behavior, verbatim TSL copying,
cancellation races, inert result text and error recovery. Real API regressions
exercise middle dots, negative and compound superscripts, function powers,
TSL logarithm order and reuse of the returned native expression.
The shared raw-JSON fixture checks the same duplicate-member and type-coercion
failures in both hosts. Additional worker round trips cover internal substitution
name collisions, original radical domains in real systems and inverse-cotangent
branch parity. Oversized HTTP checks wait for the actual 413 response before
uploading bytes, avoiding the macOS early-response/reset race without accepting
a reset as a passing result.
Scalar derivative and logarithm results also pass through the ordinary numerical
`/api/v2/execute` endpoint with known answers, verifying the shared TSL notation
across the two workflows.

The [recorded before/after evidence](../evidence/symbolic-web/README.md) identifies
the actual captures and separates local verification from remaining platform CI.
