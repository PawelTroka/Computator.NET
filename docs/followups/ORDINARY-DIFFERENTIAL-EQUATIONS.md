# Ordinary differential equations

[Documentation directory](../README.md) · [Shared scientific services](PORTABLE-SCIENCE.md)

`DifferentialEquation.rungeKutta4` solves a real initial-value problem with the
classical fourth-order Runge–Kutta method from the pinned Math.NET Numerics
dependency. The same Engine-owned implementation is available to portable
expressions/scripts and the desktop/web evaluators through the shared TSL alias.
It appears in scripting completion and the offline function reference. There is
no separate numerical-form panel for ODEs.

## A scalar equation

For `y′=2t` and `y(0)=3`, the analytic solution is `y(t)=3+t²`:

```csharp
var solution = DifferentialEquation.rungeKutta4(
    (t,u) => 2*t, 3.0, 0.0, 2.0, 21);
write(solution[0]);  // 3: the supplied initial value
write(solution[20]); // approximately 7: y(2)
```

The arguments are the derivative callback, initial value, start coordinate,
end coordinate and number of stored samples. `samples` includes both endpoints,
so `21` requests **20 steps**. With `h=(end-start)/(samples-1)`, index `i`
corresponds to `start+i*h`, allowing for ordinary floating-point rounding. The
default is 101 samples. Descending intervals work with a negative step; equal
bounds return the initial state repeatedly without calling the derivative.

A real calculator expression can select one result directly:

```csharp
DifferentialEquation.rungeKutta4((t,u) => u,1.0,0.0,1.0,101)[100]
```

This approximates `exp(1)`, about `2.718281828`. Custom function definitions can
also provide the derivative; they have the same request/document scope as other
TSL functions.

## Coupled systems and higher-order equations

Represent each first-order component in a real Math.NET vector. For `y″=-y`,
introduce `v=y′` and solve `y′=v`, `v′=-y`:

```csharp
var states = DifferentialEquation.rungeKutta4(
    (t,state) => vector(state[1],-state[0]),
    vector(1.0,0.0), 0.0, 2*PI, 401);
write(states[400][0]); // approximately 1: position after one period
write(states[400][1]); // approximately 0: velocity after one period
```

The derivative must return a non-null vector with the same component count as
the initial state. Initial states, callback inputs, callback results and stored
samples are copied where ownership crosses the solver boundary. Mutating a
callback input or reusing one derivative-result buffer does not overwrite
earlier stages or samples. This adds allocations, so choose the sample count and
system dimension with the available memory in mind.

## Accuracy and limits

This is a fixed-step method for ordinary smooth, non-stiff initial-value
problems. It supplies neither an error estimate nor adaptive steps, event
detection, a boundary-value solver or wall-clock cancellation. Halve the step
(for example, use 201 samples after 101) and compare results. For a sufficiently
smooth well-behaved equation in the convergence regime, the global truncation
error is fourth order; floating-point error and stiffness can prevent that
behavior. A plausible result alone does not establish adequate accuracy.

The wrapper accepts 2 through 1,000,001 samples. Bounds and initial components
must be finite. Nonzero intervals must have a finite representable RK4
half-step; rescale the time coordinate when its offset or span causes overflow
or loss of resolution. Nonfinite callback values, intermediate states and final
states raise `ArithmeticException`. Wrong-sized derivative vectors raise
`InvalidOperationException`; exceptions from the callback remain visible.

Memory and time also scale with system dimension and callback cost. The sample
limit is a work-budget guard, not a sandbox or a global memory/time guarantee.
Native Windows and Mono acceptance remain separate from portable numerical
tests and a Windows cross-build.

## Implementation and verification

The implementation is compiled once into `Computator.NET.Engine`; Core excludes
the linked source and all TSL evaluators use a shared type alias. No new native
library, copied numerical algorithm or duplicated `ToCode` body is introduced.

Analytic tests cover polynomial rates at every sample, exponential convergence,
a full oscillator period, reverse and zero-length intervals, callback count,
vector ownership, invalid input and nonfinite results. Execution tests pass the
actual TSL compiler for expressions, scripts and custom definitions. Windows
Core tests cover both legacy and precompiled evaluator paths.

The pinned implementation is [Math.NET Numerics 5.0.0 RungeKutta.FourthOrder](https://github.com/mathnet/mathnet-numerics/blob/v5.0.0/src/Numerics/OdeSolvers/RungeKutta.cs).
