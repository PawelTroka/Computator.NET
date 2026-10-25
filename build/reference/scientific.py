"""Shared numerical algorithms, linear algebra and scripting operations.

These descriptions follow the application wrappers, not a wish-list API.
"""
from common import A, P, M, SOURCE, MATHNET
from scientific_matrices import matrix
from scientific_scripting import scripting

def describe(item,values):
    kind=item['type'];sig=item['signature'];name=sig.split('(')[0].split('.')[-1].split('<')[0]
    args=sig[sig.find('(')+1:-1] if '(' in sig else ''
    if kind=='Integral':return integral(name,args)
    if kind=='Derivative':return derivative(name,args)
    if kind=='FunctionRoot':return root(name,args)
    if kind=='MatrixFunctions':return matrix(name,args,sig)
    if kind=='ScriptingFunctions':return scripting(name,args,sig)
    if kind=='MathematicalTransformations':
        return A('Discrete data transform','Applies a named transform to an array of equally spaced samples.',
          'Real DHT: H[k]=(1/sqrt(N))·Σₙ x[n]·(cos(2·PI·k·n/N)+sin(2·PI·k·n/N)); H(H(x))=x.',
          'Case-sensitive names: FFT, IFFT, DST, IDST, DCT, IDCT, DHT, FHT and IFHT. FHT here means Hilbert transform; DHT means Hartley.',
          'Complex DCT/IDCT/DHT treat the samples as an N-by-2 real array with columns Re and Im, transform BOTH axes with orthonormal scaling, and return N complex coefficients without modifying the input. DCT and IDCT are inverses; DHT is its own inverse. These three complex transforms and real DHT return empty arrays for empty input. Real DHT updates its input in place. Other overload differences: real FFT/IFFT uses Accord DFT and discards the imaginary result; complex FFT/IFFT uses Math.NET defaults and mutates the input array. Several other real transforms also mutate input. Complex FHT/IFHT starts from only the real component. Do not assume every overload is an invertible copy-producing transform.',url=SOURCE+'Computator.NET.Core/Transformations/TrigonometricTransformations.cs',category='Discrete transforms')
    if kind=='StatisticsFunctions':
        table={
          'rnd':('Exposed pseudorandom generator','The public System.Random instance available to scripts.',
                 'rnd.NextDouble() returns a pseudorandom value in [0,1).',
                 'This is stateful pseudorandomness, not cryptographic randomness. Assigning or seeding rnd does not seed the separate private generator used by random(...).'),
          'x':('Uniform random value from zero','Generates a pseudorandom real or integer-valued result according to overload selection.',
               'Real x: x·U; integer x: an integer from 0 THROUGH x; U∈[0,1).',
               'Use real x≥0 for [0,x), integer x≥0 for the inclusive integer range. Negative integer x returns NaN. Integer MaxValue cannot safely be used because the implementation adds 1.'),
          'x,y':('Uniform random value between bounds','Generates a pseudorandom value between two supplied bounds.',
                 'Real x,y: x+(y−x)U; integer x,y: an integer from x THROUGH y; U∈[0,1).',
                 'Use x≤y. The real and integer overloads differ at the upper endpoint; both are returned as real numbers. Integer MaxValue as y overflows the y+1 bound.'),
          'z':('Random point in a complex rectangle','Independently samples the real and imaginary coordinates between zero and the respective components of z.',
               'Re(z)·U+i·Im(z)·V, with independent uniform U,V∈[0,1).',
               'This fills an axis-aligned rectangle, not a disk. Negative components reverse the direction of the corresponding interval.'),
          'z1,z2':('Random point on a complex line segment','Samples a single interpolation factor between two complex endpoints.',
                   'z₁+(z₂−z₁)U, with uniform U∈[0,1).',
                   'This samples a LINE SEGMENT, not independent coordinates in a rectangle; z₂ is normally excluded by the half-open random factor.')
        }
        t,d,f,n=table[name if name=='rnd' else args]
        return A(t,d,f,'The generator changes state on each sample; repeated calls need not agree.',n,url=SOURCE+'Computator.NET.Core/Functions/StatisticsFunctions.cs',category='Random sampling')
    raise KeyError(kind+':'+sig)

def integral(name,args):
    data={
      'rectangleMethod':('Composite midpoint rule','Approximates area by equal-width rectangles evaluated at their midpoints.', 'h·Σⱼ₌₀ᴺ⁻¹ f(a+(j+1/2)h), h=(b−a)/N.',
                         'N must be a finite positive integer no larger than 2,147,483,647. Invalid counts throw ArgumentOutOfRangeException before evaluating fx. For smooth f, error is generally of order h².'),
      'trapezoidalMethod':('Composite trapezoidal rule','Approximates the integral by straight-line interpolation on each panel.',
                           'h·[f(a)/2+Σⱼ₌₁ᴺ⁻¹ f(a+jh)+f(b)/2], h=(b−a)/N.',
                           'N must be a finite positive integer no larger than 2,147,483,647. Invalid counts throw ArgumentOutOfRangeException before evaluating fx. Smooth-function error is generally of order h².'),
      'simpsonMethod':('Composite Simpson rule','Integrates piecewise quadratic interpolation on pairs of equal-width panels.',
                       'h/3·[f(a)+4f(a+h)+2f(a+2h)+…+4f(b−h)+f(b)].',
                       'Use a finite positive integer N no larger than 2,147,483,646. This wrapper increments odd integers by 1 to obtain an even panel count. Invalid counts throw ArgumentOutOfRangeException before evaluating fx. Smooth-function error is generally of order h⁴.'),
      'monteCarloMethod':('Monte Carlo quadrature','Estimates a one-dimensional integral by random sampling over its interval.',
                         'Integral estimate ≈ (b−a)·sample mean of f(U), U uniform on [a,b].',
                         'N is the sample count, cast to an integer. Sampling error is stochastic, usually proportional to 1/sqrt(N), not a guaranteed deterministic tolerance.'),
      'infiniteAdaptiveGaussKronrodMethod':('Adaptive Gauss–Kronrod quadrature','Adaptively subdivides the integral using Gauss–Kronrod estimates; the Accord algorithm also supports transformed infinite intervals.',
                                           '', 'N configures the Accord integration workspace/subinterval limit, not an exact number of function calls. Uses library default tolerances; returns NaN when Compute reports failure.'),
      'nonAdaptiveGaussKronrodMethod':('Nonadaptive Gauss–Kronrod quadrature','Uses embedded quadrature rules and their difference as an error estimate without adaptive interval subdivision.',
                                      '', 'The wrapper passes 1/N as the library tolerance, NOT N as a panel count. Use finite N>0; nonsmooth or singular integrands may not meet the requested accuracy.'),
      'doubleExponentialTransformation':('Double-exponential quadrature','Transforms a finite-interval integral to rapidly decaying tails, often useful for endpoint singularities.',
                                           '', 'The wrapper passes target relative error 1/N to Math.NET, not a literal number of samples. Endpoint behavior and finite precision still limit attainable accuracy.'),
      'rombergMethod':('Romberg extrapolation','Extrapolates a sequence of trapezoidal estimates toward zero mesh spacing.',
                        'Richardson extrapolation cancels successive even powers of the panel width.',
                        'N is the integer extrapolation depth, not the panel count; default 6 and N>30 is explicitly rejected. Work grows rapidly with depth.')
    }
    if name=='integrate':
        return A('Select a numerical integration method','Dispatches to one of the existing scalar quadrature implementations.',
                 'Approximates ∫ₐᵇ fx(t) dt.', 'Supply a real callable fx, a<b and an appropriate positive integral N.',
                 'Exact method names: trapezoidal method; rectangle method; Simpson\'s method; double exponential transformation; non-adaptive Gauss–Kronrod method; infinity-adaptive Gauss–Kronrod method; Monte Carlo method; Romberg\'s method. Unknown names return NaN. N means panels, tolerance inverse or depth depending on the selected method; the default 10,000 is invalid for Romberg.',url=SOURCE+'Computator.NET.Core/NumericalCalculations/Integral.cs',category='Numerical integration')
    t,d,f,n=data[name]
    oriented=name in ('rectangleMethod','trapezoidalMethod','simpsonMethod')
    arguments=('fx is a real-valued callable. Supply integral N>0 and finite bounds a,b in either order.'
               if oriented else 'fx is a real-valued callable. Supply integral N>0 and a<b, except supported infinite endpoints of the adaptive method.')
    orientation=' A signed step (b−a)/N follows the interval from a to b. Reversing the limits reverses the integral sign; equal bounds give zero for a finite integrand.' if oriented else ''
    return A(t,d,f,arguments,
             n+orientation+' These are numerical estimates, not symbolic antiderivatives or certified error bounds.',url=SOURCE+'Computator.NET.Core/NumericalCalculations/Integral.cs',category='Numerical integration')

def derivative(name,args):
    if name=='derivative' and args.startswith('methodName'):
        return A('Select a numerical derivative method','Dispatches to an existing finite-difference or Math.NET derivative implementation.',
                 'Estimate d^order fx(x)/dx^order.', 'fx is a real callable; x the evaluation point; order is a nonnegative integer; eps>0 controls the hand-written steps.',
                 'Exact names: centered order-point method; finite difference formula; stable finite difference formula; two-point finite difference formula; centered five-point method. Unknown names return NaN. The centered order-point method delegates to Math.NET and ignores eps.',url=SOURCE+'Computator.NET.Core/NumericalCalculations/Derivative.cs',category='Numerical differentiation')
    if name in ('derivative','derivativeAtPoint','derivativeAsFunction'):
        asfunction=name=='derivativeAsFunction'
        return A('Derivative as a callable' if asfunction else 'Numerical derivative at a point',
                 'Builds a real callable evaluating the order-th derivative at its own argument.' if asfunction else 'Uses Math.NET’s finite-difference derivative implementation.',
                 'g(t)=d^order fx(t)/dt^order' if asfunction else 'd^order fx(x)/dx^order',
                 'Nonnegative integer order; order zero returns the original function'+(' itself.' if asfunction else ' value.'),
                 ('The x parameter is ignored: the RETURNED function’s argument selects the evaluation point.' if asfunction else
                  'The eps parameter is ignored by derivativeAtPoint; the Math.NET step selection is used.' if name=='derivativeAtPoint' else 'Step selection is delegated to Math.NET; high orders can be sensitive to roundoff and nonsmooth inputs.'),url=MATHNET+'MathNet.Numerics/Differentiate.htm',category='Numerical differentiation')
    data={
      'finiteDifferenceFormula':('Forward finite difference','[f(x+h)−f(x)]/h'),
      'stableFiniteDifferenceFormula':('Representable-step forward difference','[f(x+h)−f(x)]/((x+h)−x)'),
      'twoPointfiniteDifferenceFormula':('Centered two-point difference','[f(x+h)−f(x−h)]/(2h)'),
      'centeredFivePointMethod':('Centered five-point difference','[f(x−2h)−8f(x−h)+8f(x+h)−f(x+2h)]/(12h)')
    }
    t,f=data[name]
    return A(t,'A finite-difference derivative estimate, recursively extended to higher orders.',f+' for order=1.',
             'Nonnegative integer order; order zero returns fx(x). '+('The step magnitude begins at sqrt(eps)·max(1,|x|) and uses the actual representable difference.' if name=='stableFiniteDifferenceFormula' else 'Step magnitude is eps·max(1,|x|).'),
             'For finite x and positive eps, the signed step h is positive when x≥0 and negative when x<0; centered rules still sample both sides. The unit scale near zero prevents the default step from collapsing with x. eps is a step-size parameter, not a guaranteed result error. Extremely small steps lose subtraction accuracy. Recursive higher-order calls retain the chosen eps at every level, with the absolute-or-relative step recalculated at each evaluation point. Choose an appropriate step for the function scale, domain and derivative order; repeated subtraction can still lose accuracy. Prefer the Math.NET derivative entry when appropriate.',url=SOURCE+'Computator.NET.Core/NumericalCalculations/Derivative.cs',category='Numerical differentiation')

def root(name,args):
    data={
      'BrentMethod':('Brent bracketed root finder','Combines bracketing, interpolation and bisection to find a real zero.',
                     'Use a continuous fx and a<b with opposite endpoint signs. Returns NaN when TryFindRoot fails.'),
      'bisectionMethod':('Bisection root finder','Repeatedly halves an interval while retaining a sign-changing bracket.',
                         'Use a continuous fx and a<b bracketing a root. It converges predictably but usually more slowly than Brent; TryFindRoot failure returns NaN.'),
      'secantMethod':('Secant root finder','Uses the line through the two most recent function values to estimate the next root.',
                     'a,b are initial guesses, not enforced bounds. Iteration can leave the interval. This implementation returns a value only if |fx(result)|≤errorMax after its loop; otherwise NaN.'),
      'secantNewtonRaphsonMethod':('Bounded secant convenience wrapper','Uses Math.NET’s secant method without an analytic derivative.',
                                  'Use a<b. Two distinct interior starting guesses are chosen near the midpoint and one-third point, and [a,b] is supplied as the interval. Intervals too narrow for two interior floating-point values return NaN, as does failure to converge. A sign-changing interval alone does not guarantee secant convergence. Despite the name no exact derivative is used.'),
      'BroydenMethod':('One-variable Broyden wrapper','Applies the multidimensional Broyden solver to a one-element system.',
                       'Only the midpoint of a,b is used as the starting guess; finite endpoints produce a finite midpoint even when their sum would overflow. The endpoints are NOT enforced bounds or a guaranteed bracket. TryFindRoot failure returns NaN.')
    }
    if name=='findRoot':
        return A('Select a numerical root method',P('Dispatches a real equation ',M(r'\operatorname{fx}(x)=0','fx(x)=0'),' to a named root-finding algorithm.'),
                 'Find x such that fx(x)=0.', 'Positive errorMax and positive iteration count N, in the supported integer range; callable fx must return meaningful finite values near the root.',
                 'Exact names: bisection method; secant method; secant Newton Raphson method; Brent\'s method; Broyden\'s method. Unknown names return NaN. The interpretation of a,b and of errorMax depends on the algorithm; it is not a universal certified root-error bound.',url=SOURCE+'Computator.NET.Core/NumericalCalculations/FunctionRoot.cs',category='Root finding')
    t,d,n=data[name]
    return A(t,d,'fx(root) ≈ 0.', 'errorMax>0 is the requested solver accuracy; N>0 limits iterations (default 10,000).',
             n+' Repeated roots, discontinuities and nonfinite evaluations may defeat convergence.',url=SOURCE+'Computator.NET.Core/NumericalCalculations/FunctionRoot.cs',category='Root finding')
