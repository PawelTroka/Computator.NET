# Coefficient polynomials

`Polynomial` is the shared TSL alias for the existing Math.NET Numerics 5.0.0
`MathNet.Numerics.Polynomial` type. Use it in desktop/web expressions, scripts,
custom definitions and the portable engine. Scripting completion offers the
coefficient constructor, `Polynomial.Zero` and callable static operations;
expression completion offers the numeric `Evaluate` overloads. Instance methods
such as `p.Roots()` are callable; completion after an arbitrary variable is a
separate capability tracked in [#54](https://github.com/PawelTroka/Computator.NET/issues/54).

These general coefficient polynomials complement the existing named orthogonal
polynomials. They support the workflow requested in
[#111](https://github.com/PawelTroka/Computator.NET/issues/111) using the dependency
already shipped with the application.

## Coefficients, construction and evaluation

Coefficients run from the constant term upward. `[1,-3,2]` means
`1 - 3x + 2x²`, not `x² - 3x + 2`.

```csharp
var p = new Polynomial(1,-3,2);
write(p.Evaluate(2));                 // 3
write(Polynomial.Evaluate(2,1,-3,2)); // 3, without constructing p
write(p.Evaluate(new complex(2,3))); // -15 + 15i (format depends on the host)
```

For an expression or plot, enter `Polynomial.Evaluate(x,1,-3,2)` or
`new Polynomial(1,-3,2).Evaluate(x)`. For a custom definition, use:

```csharp
static real p(real t) => new Polynomial(1,-3,2).Evaluate(t);
```

The object stores **real coefficients**. The static `Polynomial.Evaluate`
overload can also evaluate complex coefficients:

```csharp
Polynomial.Evaluate(new complex(2,3),
    new complex[] { new complex(1,2), new complex(3,-1) }) // 10 + 9i
```

A single integer has a different meaning from a single real value:

| Construction | Meaning |
| --- | --- |
| `new Polynomial(3)` | Zero polynomial with three coefficient slots |
| `new Polynomial(3.0)` | Constant polynomial with value three |
| `new Polynomial()` or `Polynomial.Zero` | Zero polynomial with an empty coefficient array |
| `new Polynomial(new real[]{3})` | Constant polynomial with value three |

`Coefficients` exposes the mutable storage. When passed a `real[]`, the
constructor shares that array; modifying either view changes the same
polynomial. Use `p.Clone()` for independent coefficient storage. `Degree` ignores
trailing zero coefficients and uses `-1` for the zero polynomial.

## Arithmetic and division

```csharp
var a = new Polynomial(1,2);  // 1 + 2x
var b = new Polynomial(3,-1); // 3 - x
write((a+b).Evaluate(2));     // 6
write((a-b).Evaluate(2));     // 4
write((a*b).Evaluate(2));     // 5
write((-a).Evaluate(2));      // -5
write((3.0*a).Evaluate(2));   // 15
write((a/2.0).Evaluate(2));   // 2.5
```

Static forms include `Polynomial.Add`, `Subtract`, `Negate`, `Multiply` and
`Divide`. Addition/subtraction accept constants as well as polynomials.
`Polynomial.Divide(a,k)` and `a/k` divide by a **scalar**, not a polynomial.
A zero scalar can produce nonfinite coefficients under floating-point division.

For polynomial division, read both parts of the tuple:

```csharp
var dividend = new Polynomial(1,0,1); // x² + 1
var divisor = new Polynomial(1,1);    // x + 1
var parts = Polynomial.DivideRemainder(dividend,divisor);
var quotient = parts.Item1;  // x - 1
var remainder = parts.Item2; // 2
write((quotient*divisor+remainder).Evaluate(3)); // 10, same as dividend(3)
```

The defining relationship is `dividend = quotient*divisor + remainder`, up to
floating-point rounding. Dividing by a zero polynomial throws
`DivideByZeroException`. The operation does not mutate its inputs, but some
short-circuit results share an input object: when the dividend degree is lower
than the divisor degree, the remainder is the original dividend. Clone a result
before editing its coefficients when independent storage is required.

`PointwiseMultiply` and `PointwiseDivide` operate on coefficients of equal
exponent. They do not compute `a(x)*b(x)` or `a(x)/b(x)`. Pointwise multiplication
stops at the smaller degree. Pointwise division extends through the dividend
degree and treats missing divisor coefficients as zero, possibly producing
infinity or NaN. Use ordinary multiplication or `DivideRemainder` for polynomial
algebra.

## Derivatives and antiderivatives

```csharp
var p = new Polynomial(1,-3,2);
var derivative = p.Differentiate();  // -3 + 4x
var integral = p.Integrate();        // x - 1.5x² + (2/3)x³
write(derivative.Evaluate(2));        // 5
write(integral.Evaluate(3));          // 7.5
write(integral.Differentiate().Evaluate(2)); // 3
```

`Integrate()` selects the antiderivative with constant zero. Add a constant if
another member of the antiderivative family is needed. For a definite integral,
evaluate this antiderivative at both endpoints and subtract.

## General numerical roots

```csharp
var cubic = new Polynomial(-6,11,-6,1); // (x-1)(x-2)(x-3)
var roots = cubic.Roots();              // approximately 1, 2, 3
foreach (var root in roots) {
    write(root);
    write("; ");
}

var nonreal = new Polynomial(1,0,0,0,1).Roots(); // four complex roots of x⁴+1
```

The return value is a `complex[]`; **root order is unspecified**. General roots
come from eigenvalues of a companion-like matrix in Math.NET. Results are
numerical approximations, including repeated roots. Large coefficient scale
ratios, high degree and nearly repeated roots can substantially reduce accuracy.
Check residuals relative to coefficient and argument scale, and do not infer
exact multiplicity or exact realness from rounded output alone.

Trailing zero coefficients do not add roots. A nonzero constant returns an empty
root array. The zero polynomial also returns an empty array by library
convention, even though mathematically every input is a root. This API does not
provide symbolic factorization or a new GSL root solver.

## Least-squares fitting

```csharp
var fit = Polynomial.Fit(
    new real[]{-2,-1,0,1,2},
    new real[]{15,6,1,0,3}, 2);
write(fit.Evaluate(2)); // approximately 3
```

These samples follow `1 - 3x + 2x²`; the returned coefficients are approximately
`[1,-3,2]`. `order` is the requested degree, so the model has `order+1`
coefficients. The optional fourth argument is
`MathNet.Numerics.LinearRegression.DirectRegressionMethod` and defaults to `QR`.
The arrays must have matching lengths and enough independent observations to
determine the coefficients. Fitting noisy data minimizes squared residuals and
does not generally pass through every point. Center and scale abscissas where
appropriate, and assess residuals and extrapolation separately.

## Reference and validation

The [pinned Math.NET implementation](https://github.com/mathnet/mathnet-numerics/blob/v5.0.0/src/Numerics/Polynomial.cs)
is the source of these conventions. The application adds an alias, curated
completion and authored help; it reuses the existing numerical algorithms.
The [function-reference guide](FUNCTION-REFERENCE.md) describes deterministic
catalog generation and validation.

`PolynomialExecutionTests` exercises the portable expression, custom-definition
and scripting paths with analytic values, roots and reconstruction identities.
`PolynomialEvaluatorTests` covers the actual precompiled and legacy Core
paths; `PolynomialAutocompleteTests` checks the typed catalog, expression
filter and authored reference. Native Core tests run on Windows; a Linux
Windows-targeting build only checks that those projects compile.
