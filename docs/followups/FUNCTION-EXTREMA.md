# Function minima and maxima

[Documentation directory](../README.md) · [Shared scientific services](PORTABLE-SCIENCE.md)

`FunctionExtremum.minimize` and `FunctionExtremum.maximize` find the **coordinate**
of an extremum of a real function on a closed interval. This adds the scripting
and expression capability requested in [issue #44](https://github.com/PawelTroka/Computator.NET/issues/44).
The functions use one shared managed implementation in the portable engine,
CLI, desktop, Mono and web evaluators. Both operations have completion entries
and offline reference articles. There is no separate numerical-form panel.

## Find a location, then its value

The quadratic `(t-2)*(t-2)+7` has its minimum at `t=2`, where its value is `7`:

```csharp
FunctionExtremum.minimize(t => (t-2)*(t-2)+7,0,5)
```

This returns approximately **2**. The corresponding maximum example is:

```csharp
FunctionExtremum.maximize(t => 7-(t-2)*(t-2),0,5)
```

It also returns approximately **2**. In a script, retain the function and evaluate
it at the returned coordinate to obtain the extremum value:

```csharp
function f(real t) = 7-(t-2)*(t-2);
var location = FunctionExtremum.maximize(f,0,5);
write(location);    // approximately 2
write(f(location)); // approximately 7
```

Custom function definitions can supply the objective in the same way. The
portable CLI uses its existing `calculate` or `script` command:

```sh
dotnet run --project Computator.NET.Cli -- calculate --expression 'FunctionExtremum.minimize(t => (t-2)*(t-2)+7,0,5)'
```

## Choose an interval and accuracy

The full signatures are:

```csharp
FunctionExtremum.minimize(fx,a,b,tolerance,iterations)
FunctionExtremum.maximize(fx,a,b,tolerance,iterations)
```

| Argument | Meaning |
| --- | --- |
| `fx` | A real function of one real coordinate; use a continuous unimodal function on the chosen interval |
| `a`, `b` | Finite lower and upper bounds, with `a<=b` and a finite representable width |
| `tolerance` | Optional positive finite coordinate tolerance; default `1e-8` |
| `iterations` | Optional iteration budget from 1 through 1,000,000; default 1,000 |

Both endpoints are evaluated and compared with the interior candidate.
Consequently, `FunctionExtremum.minimize(t => t,0,5)` returns exactly `0`, and
`FunctionExtremum.maximize(t => t,0,5)` returns exactly `5`. A constant function
selects `a`. Equal bounds evaluate that single point once and return its
coordinate. The function must be finite at every sampled point, including both
endpoints; do not include a singular endpoint in the interval.

The wrapper normalizes coordinates to `[0,1]` before using the existing
[Accord 3.8.0 Brent optimizer](https://github.com/accord-net/framework/blob/v3.8.0/Sources/Accord.Math/Optimization/BrentSearch.cs).
This prevents a large coordinate offset from causing immediate premature
convergence, and keeps every callback inside the supplied interval. Tolerance
is scaled into that normalized coordinate. The optimizer also has a relative
stopping term, so attainable coordinate accuracy is limited by roughly
`sqrt(machine epsilon)` times the interval width (about `1e-8` times the width),
plus ordinary floating-point resolution and the requested tolerance. A smaller
tolerance is not a guarantee of more accurate digits or convergence, and is not an error bound
on the function value. Narrow the interval when finer location accuracy matters.

Use an interval containing one trough for minimization or one peak for
maximization. For a function with multiple extrema, the search can find a local
candidate; it does not certify the global minimum or maximum. Inspect the plot,
split the domain into suitable intervals, and compare function values at the
resulting coordinates and boundaries.

Invalid arguments throw before sampling. Nonfinite sampled values throw
`ArithmeticException`; callback exceptions propagate. Exhausting the iteration
budget throws `InvalidOperationException` rather than returning an unconverged
candidate. The budget bounds optimizer iterations, not callback execution time.

## Implementation and verification

The implementation is compiled once into `Computator.NET.Engine`. Core excludes
the linked source, and the shared TSL preamble supplies the alias to both legacy
and precompiled evaluators. No dependency or duplicated `ToCode` algorithm is
added.

Portable tests cover interior and near-endpoint quadratics, a trigonometric
peak, an absolute-value trough, monotone/constant functions, equal/adjacent
bounds, translated/scaled intervals, callback boundaries, invalid inputs and
nonconvergence. Execution tests use real TSL expressions, scripts and custom
functions. Windows Core tests cover both evaluator paths and completion/help
attachment; native desktop/Mono execution remains a separate validation step.
