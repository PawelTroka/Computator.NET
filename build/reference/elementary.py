"""Elementary-function editorial definitions and explicit alias conventions."""
from common import A, P, M, DLMF, MATHNET, SOURCE
from dataclasses import replace
from categories import subject_category
ALIASES={'tg':'tan','ctg':'cot','cosec':'csc','tgh':'tanh','ctgh':'coth','cosech':'csch',
         'arctg':'arctan','arcctg':'arccot','arccosec':'arccsc','artgh':'artanh',
         'arctgh':'arcoth','arcosech':'arcsch','H':'Heaviside','δ':'DiracDelta',
         'δij':'KroneckerDelta','nwd':'gcd','nww':'lcm'}
TRIG={
'sin':('Sine','The vertical coordinate on the unit circle; models a sinusoidal oscillation.','sin(t) = (exp(i·t) − exp(−i·t))/(2i)','All finite real arguments; real values lie in [−1,1].','sin(PI/2) ≈ 1'),
'cos':('Cosine','The horizontal coordinate on the unit circle; an even sinusoidal function.','cos(t) = (exp(i·t) + exp(−i·t))/2','All finite real arguments; real values lie in [−1,1].','cos(0) = 1'),
 'tan':('Tangent','The ratio of sine to cosine.','tan(t) = sin(t)/cos(t)','Poles occur where cos(t)=0; for real t these are PI/2+k·PI.','tan(PI/4) ≈ 1'),
 'cot':('Cotangent','The reciprocal of tangent, or cosine divided by sine.','cot(t) = cos(t)/sin(t)','Poles occur where sin(t)=0; for real t these are k·PI.','cot(PI/4) ≈ 1'),
 'sec':('Secant','The reciprocal of cosine.','sec(t) = 1/cos(t)','Undefined at the zeros of cos(t). Real values are outside (−1,1).','sec(0) = 1'),
 'csc':('Cosecant','The reciprocal of sine.','csc(t) = 1/sin(t)','Undefined at the zeros of sin(t). Real values are outside (−1,1).','csc(PI/2) ≈ 1'),
 'sinc':('Unnormalized cardinal sine','A removable-singularity extension of sine divided by its argument. Useful for oscillatory integrals and wave analysis.','sinc(t) = sin(t)/t for t≠0; sinc(0)=1','The value at zero is explicitly supplied, for both real and complex inputs.','sinc(0) = 1'),
 'sinh':('Hyperbolic sine','The odd hyperbolic counterpart of sine.','sinh(t) = (exp(t) − exp(−t))/2','All finite real inputs; magnitude grows exponentially.','sinh(0) = 0'),
 'cosh':('Hyperbolic cosine','The even hyperbolic counterpart of cosine.','cosh(t) = (exp(t) + exp(−t))/2','All finite real inputs; cosh(t)≥1 for real t.','cosh(0) = 1'),
 'tanh':('Hyperbolic tangent','The ratio of hyperbolic sine to hyperbolic cosine.','tanh(t) = sinh(t)/cosh(t)','For finite real t the mathematical result lies in (−1,1); floating-point saturation can reach an endpoint.','tanh(0) = 0'),
 'coth':('Hyperbolic cotangent','The reciprocal of hyperbolic tangent.','coth(t) = cosh(t)/sinh(t)','For real inputs t≠0; complex poles occur at zeros of sinh(t).','coth(1) ≈ 1.3130352855'),
 'sech':('Hyperbolic secant','The reciprocal of hyperbolic cosine.','sech(t) = 1/cosh(t)','All finite real inputs; complex poles occur at zeros of cosh(t).','sech(0) = 1'),
 'csch':('Hyperbolic cosecant','The reciprocal of hyperbolic sine.','csch(t) = 1/sinh(t)','For real inputs t≠0; complex poles occur at zeros of sinh(t).','csch(1) ≈ 0.8509181282'),
}
INVERSE={
 'arcsin':('Inverse sine','The principal inverse of sine.','sin(arcsin(t)) = t','Real domain [−1,1], range [−PI/2,PI/2].','arcsin(1) ≈ PI/2'),
 'arccos':('Inverse cosine','The principal inverse of cosine.','cos(arccos(t)) = t','Real domain [−1,1], range [0,PI].','arccos(1) = 0'),
 'arctan':('Inverse tangent','The principal inverse of tangent.','tan(arctan(t)) = t','Real domain all finite numbers, range (−PI/2,PI/2).','arctan(1) ≈ PI/4'),
 'arccot':('Inverse cotangent',P('The inverse cotangent in the application’s ', M('\\pi/2', 'PI/2'), '-minus-arctangent convention.'),'arccot(t) = PI/2 − arctan(t)','Real domain all finite numbers, range (0,PI). This differs from atan(1/t) for negative real t.','arccot(0) ≈ PI/2'),
 'arcsec':('Inverse secant','Inverse secant expressed through inverse cosine.','arcsec(t) = arccos(1/t)','Real domain |t|≥1; range [0,PI] excluding PI/2.','arcsec(1) = 0'),
 'arccsc':('Inverse cosecant','Inverse cosecant expressed through inverse sine.','arccsc(t) = arcsin(1/t)','Real domain |t|≥1; range [−PI/2,PI/2] excluding zero.','arccsc(1) ≈ PI/2'),
 'arsinh':('Inverse hyperbolic sine','The inverse of hyperbolic sine.','arsinh(t) = ln(t + sqrt(t²+1))','All finite real inputs. The direct logarithmic formula can lose accuracy for large negative arguments.','arsinh(0) = 0'),
 'arcosh':('Inverse hyperbolic cosine','The nonnegative real inverse of hyperbolic cosine.','arcosh(t) = ln(t + sqrt(t+1)·sqrt(t−1))','Real domain t≥1; nonnegative result.','arcosh(1) = 0'),
 'artanh':('Inverse hyperbolic tangent','The inverse of hyperbolic tangent.','artanh(t) = ½ ln((1+t)/(1−t))','Finite real values require −1<t<1. The endpoints are logarithmic singularities.','artanh(0) = 0'),
 'arcoth':('Inverse hyperbolic cotangent','The application’s logarithmic inverse-cotangent-hyperbolic convention.','arcoth(t) = ½ ln((t+1)/(t−1))','Finite real domain |t|>1. The spelling arctgh is an alias for this function, NOT for artanh.','arcoth(2) ≈ 0.5493061443'),
 'arcsch':('Inverse hyperbolic cosecant','Inverse hyperbolic sine of the reciprocal argument.','arcsch(t) = ln(1/t + sqrt(1/t²+1))','Real t≠0. Direct reciprocals and subtraction can overflow or cancel near extreme inputs.','arcsch(1) ≈ 0.8813735870'),
 'arsech':('Inverse hyperbolic secant','The inverse hyperbolic cosine of the reciprocal argument.','arsech(t) = ln(1/t + sqrt(1/t+1)·sqrt(1/t−1))','Finite real domain 0<t≤1.','arsech(1) = 0'),
}
OTHER={
 'Heaviside': A('Heaviside step','A unit step with the symmetric half-value convention at its discontinuity.','H(x)=0 for x<0; H(0)=½; H(x)=1 for x>0.','Use for piecewise real functions.','The value at zero is a convention; it is not inferred by numerical integration.','Heaviside(0) = 0.5',DLMF+'1.16'),
 'DiracDelta': A('Dirac-delta point helper','A pointwise infinity marker used by this application, not an implementation of a Dirac distribution.','δ(x)=+∞ when x=0; δ(x)=0 otherwise.','The mathematical Dirac delta is defined by its action on test functions, not as an ordinary sampled function.','Do not use this helper to expect unit-area plots or correct quadrature of an impulse. Finite-width approximations or analytic integration are needed.','DiracDelta(0) = Infinity',DLMF+'1.16'),
 'KroneckerDelta': A('Kronecker delta','An equality indicator, distinct from the Dirac delta.','δij=1 when i=j, and 0 otherwise.','Inputs are compared using exact floating-point equality, even though the usual notation uses integer indices.','','KroneckerDelta(2,2) = 1',DLMF+'1.2'),
 'sgn': A('Sign of a real number','Indicates whether a real argument is negative, zero or positive.','sgn(x) = −1, 0 or 1.','Use finite real x. NaN has no mathematical sign and follows the underlying Math.Sign error behavior.','','sgn(-4) = -1', 'https://learn.microsoft.com/dotnet/api/system.math.sign'),
 'gcd': A('Greatest common divisor','The largest positive integer dividing both integer inputs.','gcd(a,b) divides a and b; gcd(12,18)=6.','Use positive integers for this implementation.','The wrapper returns NaN if either input is zero, rather than the common gcd(a,0)=|a| convention; its Euclidean loop does not normalize negative inputs.','gcd(12,18) = 6',SOURCE+'Computator.NET.Core/Functions/ElementaryFunctions.cs'),
 'lcm': A('Least common multiple','The smallest positive integer divisible by both positive integer inputs.','lcm(a,b)=a·b/gcd(a,b).','Use positive integers. The intermediate integer product can overflow.','The wrapper returns NaN if either argument is zero; it does not implement the conventional zero least-common-multiple value.','lcm(4,6) = 12',SOURCE+'Computator.NET.Core/Functions/ElementaryFunctions.cs'),
 'factorial': A('Factorial',P('Product of the positive integers up to ', M('n', 'n'), ', with the empty product equal to one.'),'n! = 1·2·…·n; 0!=1.','Intended for nonnegative integer n. Returns a floating-point approximation; n≥171, including positive infinity, returns positive infinity immediately because 171! exceeds the finite range of a double.','This loop accepts a real n: fractional n is effectively truncated by the loop and negative n returns 1. It is not Γ(n+1) analytic continuation; use gamma explicitly for that.','factorial(5) = 120',DLMF+'5.4'),
 'DoubleFactorial': A('Double factorial',P('Product of positive integers with the same parity as ', M('n', 'n'), '.'),'n!!=n·(n−2)·…; 0!!=1 and 1!!=1.','Intended for nonnegative integers. Negative integer arguments return NaN.','The real-argument overload casts to an integer before evaluation; fractional inputs are not a generalized double factorial. Large orders overflow the numeric result.','DoubleFactorial(7) = 105', 'https://www.gnu.org/software/gsl/doc/html/specfunc.html#factorials'),
 'Eulerφ': A('Euler totient',P('Counts integers from 1 through ', M('n', 'n'), ' that are coprime to ', M('n', 'n'), '.'),'φ(n)=n·∏(1−1/p), over distinct primes p dividing n.','Positive integer n; φ(1)=1. Inputs below 1 return NaN.','The current implementation allocates a sieve of size proportional to n; this is not appropriate for very large integers.','Eulerφ(9) = 6',SOURCE+'Computator.NET.Core/Functions/ElementaryFunctions.cs'),
 'isPrime': A('Primality predicate','Tests whether an integer is a prime: a positive integer greater than one with exactly two positive divisors.','Prime n has no positive divisor other than 1 and n.','Integers; nonpositive numbers and 1 are not prime.','','isPrime(13) = true',SOURCE+'Computator.NET.Core/Functions/ElementaryFunctions.cs'),
 'prime': A('Numeric primality flag','A numeric wrapper around isPrime, useful in expressions that need a 0/1 indicator.','prime(x)=1 if the converted integer is prime; otherwise 0.','The wrapper returns −1 when x<1; otherwise it casts x to a 32-bit integer.','This does not return the x-th prime. Use integral values in the integer range; fractions are truncated.','prime(13) = 1',SOURCE+'Computator.NET.Core/Functions/ElementaryFunctions.cs'),
 'TaylorCoeff': A('Exponential-series coefficient',P('Evaluates the ', M('n', 'n'), '-th power divided by the ', M('n', 'n'), '-th factorial.'),'xⁿ/n!, the n-th term in exp(x)=Σ xⁿ/n!.','Integer n≥0 and real x≥0 in this wrapper; other arguments return NaN.','','TaylorCoeff(3,2) ≈ 1.3333333333', 'https://www.gnu.org/software/gsl/doc/html/specfunc.html#taylor-coefficients'),
 'BinomialCoeff': A('Binomial coefficient',P('Counts ', M('k', 'k'), '-element subsets of ', M('n', 'n'), ' distinct objects when order is irrelevant.'),'C(n,k)=n!/[k!(n−k)!].','Integers 0≤k≤n, representable by the underlying unsigned integer interface; invalid signed bounds return NaN.','The result is floating point rather than an arbitrary-precision integer.','BinomialCoeff(5,2) = 10','https://www.gnu.org/software/gsl/doc/html/specfunc.html#binomial-coefficients'),
 'Combinations': A('Combinations without repetition',P('Number of unordered selections of ', M('k', 'k'), ' distinct objects from ', M('n', 'n'), ' objects.'),'C(n,k)=n!/[k!(n−k)!].','Use integers n≥0 and 0≤k≤n.','Delegates to Math.NET combinatorics; floating-point overflow/rounding can occur for large inputs.','Combinations(5,2) = 10',MATHNET+'MathNet.Numerics/Combinatorics.htm'),
 'CombinationsWithRepetition': A('Combinations with repetition',P('Number of unordered ', M('k', 'k'), '-element selections from ', M('n', 'n'), ' kinds of object, allowing a kind to be selected repeatedly.'),'C(n+k−1,k) for n>0.','Use n>0 and k≥0.','','CombinationsWithRepetition(3,2) = 6',MATHNET+'MathNet.Numerics/Combinatorics.htm'),
 'Variations': A('Ordered selections without repetition',P('Number of ways to choose and order ', M('k', 'k'), ' different objects from ', M('n', 'n'), '.'),'n!/(n−k)!.','Use integers 0≤k≤n.','','Variations(5,2) = 20',MATHNET+'MathNet.Numerics/Combinatorics.htm'),
 'VariationsWithRepetition': A('Ordered selections with repetition',P('Number of length-', M('k', 'k'), ' ordered selections from ', M('n', 'n'), ' possible choices at each position.'),'nᵏ.','Use nonnegative integer n and k.','','VariationsWithRepetition(3,2) = 9',MATHNET+'MathNet.Numerics/Combinatorics.htm'),
 'Permutations': A('Permutations',P('Number of distinct orderings of ', M('n', 'n'), ' distinguishable objects.'),'n!.','Nonnegative integer n; zero objects have one empty ordering.','This wrapper returns 0 for negative n. This differs from the separate factorial loop.','Permutations(4) = 24',MATHNET+'MathNet.Numerics/Combinatorics.htm'),
}

def describe(item, values):
    raw = item['signature'].split('(')[0]
    category = subject_category('ElementaryFunctions', ALIASES.get(raw, raw))
    return replace(_describe(item, values), category=category)


def _describe(item, values):
    signature=item['signature']; original=signature.split('(')[0]; name=ALIASES.get(original,original)
    complex_input='complex' in item['menu'] and '(z' in signature
    alias=('Alias of '+name+'; it uses the same convention. ') if name!=original else ''
    if name in TRIG:
        title,definition,formula,domain,example=TRIG[name]
        if complex_input:
            domain='Complex argument z; the formula gives its complex continuation. Reciprocal functions retain poles at zeros of their denominators.'
        note=alias + ('This is sin(t)/t, NOT sin(PI·t)/(PI·t).' if name=='sinc' else '')
        return A(title+(' — complex argument' if complex_input else ''),definition,formula,
                 'Angles are measured in radians. '+domain,note,example,DLMF+('4.28' if name in ('sinh','cosh','tanh','coth','sech','csch') else '4.14'))
    if name in INVERSE:
        title,definition,formula,domain,example=INVERSE[name]
        note=alias
        if complex_input:
            domain='Complex z. The implementation uses System.Numerics principal inverse/logarithm/square-root values in the displayed formula; branch cuts and endpoint side matter. It returns one value, not all inverse branches.'
            if name=='arccot':note+=' The PI/2−atan convention need not equal atan(1/z) across branches.'
        return A(title+(' — complex argument' if complex_input else ''),definition,formula,
                 'Results are in radians where interpreted as angles. '+domain,note,example,DLMF+('4.37' if name in ('arsinh','arcosh','artanh','arcoth','arcsch','arsech') else '4.23'))
    if name in OTHER:
        a=OTHER[name]
        return A(a.title,a.definition,a.formula,a.arguments,alias+a.note,a.example,a.url,a.category)
    if name in ('Re','Im','Magnitude','Phase','conj','abs'):
        title,definition,formula,example={
          'Re':('Real component',P('Extracts the real coordinate of ', M('z=a+ib', 'z=a+i·b'), '.'),'Re(a+i·b)=a.','Re(3+4i) = 3'),
          'Im':('Imaginary component',P('Extracts the coefficient of ', M('i', 'i'), ', not an imaginary-valued number.'),'Im(a+i·b)=b.','Im(3+4i) = 4'),
          'Magnitude':('Complex modulus','The distance of a complex number from the origin.','|a+i·b|=sqrt(a²+b²).','Magnitude(3+4i) = 5'),
          'Phase':('Complex argument','The polar angle measured from the positive real axis.','Phase(a+i·b)=atan2(b,a).','Phase(i) ≈ PI/2'),
          'conj':('Complex conjugate','Reflects a complex number in the real axis.','conj(a+i·b)=a−i·b.','conj(3+4i) = 3−4i'),
          'abs':('Absolute value','The nonnegative magnitude of a real or complex number.','abs(x)=|x|; abs(a+i·b)=sqrt(a²+b²).','abs(-3) = 3')
        }[name]
        note='The abs(complex) overload returns a complex value with zero imaginary part; Magnitude returns a real number.' if name=='abs' else ''
        domain='Phase is in radians on the principal interval [−PI,PI]; the mathematical phase at zero is undefined even if the library returns a convention.' if name=='Phase' else 'Finite scalar input; usual floating-point NaN/infinity propagation applies.'
        return A(title,definition,formula,domain,note,example,'https://learn.microsoft.com/dotnet/api/system.numerics.complex')
    if name in ('ln','log','log2','log10','exp','pow','sqrt','root'):
        title,definition,formula,domain,example={
          'ln':('Natural logarithm',P('Logarithm to base ', M('e', 'e'), '; inverse of the real exponential.'),'ln(exp(x))=x.','Finite real domain x>0; x=0 gives −∞. Complex input uses the principal logarithm, with imaginary part given by the principal argument.','ln(e) ≈ 1'),
          'log':('Logarithm to a supplied base','Returns the logarithm of the FIRST argument in the base supplied as the SECOND argument.','log(x,y)=ln(x)/ln(y).','Real x>0, base y>0 and y≠1. Complex z uses its principal logarithm divided by the logarithm of the real base.','log(8,2) = 3'),
          'log2':('Binary logarithm','Logarithm to base two.','log2(t)=ln(t)/ln(2).','Real argument positive, or complex argument interpreted through its principal logarithm.','log2(8) = 3'),
          'log10':('Common logarithm','Logarithm to base ten.','log10(t)=ln(t)/ln(10).','Real argument positive, or complex argument interpreted through its principal logarithm.','log10(100) = 2'),
          'exp':('Exponential','Raises Euler’s number to a real or complex power.','exp(x+i·y)=exp(x)·(cos(y)+i·sin(y)).','Finite real or complex argument; large real parts can overflow or underflow.','exp(0) = 1'),
          'pow':('Power','Raises the first argument to the second. The real and complex overloads share this completion signature.','pow(x,y)=xʸ; for nonzero complex x, exp(y·Log(x)).','Real negative bases with nonintegral exponents can produce NaN. The complex overload selects a branch, not all possible powers.','pow(2,3) = 8'),
          'sqrt':('Square root','Selects the nonnegative real square root, or the principal complex square root.','sqrt(t)²=t.','The real-input wrapper promotes negative x to a complex result; its return type is dynamic. Complex input returns the principal root.','sqrt(-1) = i'),
          'root':('Root by reciprocal exponent',P('Computes a power with exponent ', M('1/n', '1/n'), '; it does not enumerate all roots.'),'root(t,n)=pow(t,1/n).','Use n≠0. Negative real t is promoted to a complex power when its reciprocal exponent is nonintegral. Thus an odd real n does not guarantee the negative real root.','root(8,3) ≈ 2')
        }[name]
        return A(title,definition,formula,domain,alias,example,DLMF+'4.2')
    raise KeyError('Undocumented elementary function '+signature)
