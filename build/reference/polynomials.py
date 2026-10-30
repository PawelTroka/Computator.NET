"""Reviewed coefficient-polynomial API from the pinned Math.NET 5.0.0 dependency."""
from common import A

SOURCE = 'https://github.com/mathnet/mathnet-numerics/blob/v5.0.0/src/Numerics/Polynomial.cs'
CATEGORY = 'Polynomial operations'


def article(title, definition, formula, arguments, note):
    return A(title, definition, formula=formula, arguments=arguments, note=note,
             url=SOURCE, category=CATEGORY)


ARTICLES = {
    'new Polynomial(coefficients)': article(
        'Construct a coefficient polynomial',
        'Create a real-coefficient polynomial for arithmetic, calculus, evaluation and numerical complex roots.',
        'p(x) = c[0] + c[1] x + c[2] x² + …',
        'Pass a real array or several real coefficients in ascending exponent order. Coefficients exposes the mutable array; Degree ignores trailing zero coefficients and is −1 for the zero polynomial.',
        'A single integer selects the array-length constructor: new Polynomial(3) creates a zero polynomial with three coefficient slots. Use new Polynomial(3.0) for the constant three. The array constructor shares the supplied array; call Clone() when independent storage is needed.'),
    'Polynomial.Zero': article(
        'Zero polynomial',
        'Obtain a zero polynomial, the additive identity for polynomial arithmetic.',
        '0(x) = 0; Degree = −1',
        'This property takes no arguments and returns a new polynomial with an empty coefficient array. Its evaluation is zero for ordinary finite arguments.',
        'Roots() returns an empty complex array for both zero and nonzero constant polynomials. For zero this is an API convention: mathematically every input is a root, not an absence of roots.'),
    'Polynomial.Evaluate(z,coefficients1,coefficients2, ...)': article(
        'Evaluate a polynomial from its coefficients',
        'Compute the polynomial value directly using Horner evaluation without constructing a polynomial object.',
        'p(z) = c[0] + c[1] z + c[2] z² + …',
        'The first argument is the evaluation point, followed by a coefficient array or individual coefficients in ascending exponent order. Overloads accept a real point with real coefficients, a complex point with real coefficients, or a complex point with complex coefficients.',
        'An empty coefficient array evaluates to zero; a null array throws ArgumentNullException. The Polynomial object itself stores real coefficients; use this static overload to evaluate complex coefficients. Floating-point rounding and cancellation still apply.'),
    'Polynomial.Fit(x,y,order,method)': article(
        'Least-squares polynomial fit',
        'Fit a polynomial of the requested order to paired observations by minimizing squared residuals.',
        'minimize Σᵢ (p(x[i]) − y[i])²',
        'x and y are equally sized real arrays; order is the nonnegative integer degree. The optional DirectRegressionMethod selects QR, Svd or NormalEquations, and defaults to QR. The returned coefficients use ascending exponent order.',
        'Provide enough independent observations to determine order+1 coefficients. Repeated abscissas, high orders and badly scaled x can make the problem rank deficient or ill conditioned. A fit is not necessarily an interpolant, and extrapolation can be unstable.'),
    'Polynomial.Add(a,b)': article(
        'Add polynomials or a constant',
        'Add two polynomials, or add a real constant to a polynomial.',
        '(a + b)(x) = a(x) + b(x)',
        'a is a Polynomial; b is either another Polynomial or a real scalar. The scalar changes only the constant coefficient. The + operator provides the same arithmetic.',
        'Neither operand is changed. With polynomial operands, missing higher coefficients are treated as zero; cancellation can reduce the degree.'),
    'Polynomial.Subtract(a,b)': article(
        'Subtract a polynomial or constant',
        'Subtract a polynomial or a real constant from a polynomial.',
        '(a − b)(x) = a(x) − b(x)',
        'a is a Polynomial; b is a Polynomial or a real scalar. Subtracting a scalar changes only the constant coefficient. The - operator provides the same arithmetic.',
        'Operand order matters. Neither operand is changed; cancellation can reduce the resulting degree.'),
    'Polynomial.Subtract(b,a)': article(
        'Subtract a polynomial from a constant',
        'Subtract the entire polynomial from a real constant, reversing all nonconstant coefficient signs.',
        '(b − a)(x) = b − a(x)',
        'b is the real constant and a is the Polynomial. This overload follows the displayed argument order; b - a is its operator equivalent.',
        'This differs from a - b. The constant coefficient becomes b-a.Coefficients[0], while all higher coefficients are negated; a is unchanged.'),
    'Polynomial.Negate(a)': article(
        'Negate a polynomial',
        'Return the additive inverse by changing every coefficient sign.',
        '(−a)(x) = −a(x)',
        'a is a Polynomial. The unary - operator is equivalent.',
        'The input coefficients are not changed. Negation keeps the nonzero degree and maps the zero polynomial to zero.'),
    'Polynomial.Multiply(a,b)': article(
        'Multiply polynomials',
        'Form the ordinary polynomial product by convolving the coefficient arrays.',
        '(a b)(x) = a(x) b(x); c[k] = Σᵢ a[i] b[k−i]',
        'Both a and b are Polynomial values. The * operator performs the same product. For nonzero operands, the degree normally equals the sum of the input degrees.',
        'This is polynomial multiplication, not coefficientwise multiplication. Neither input is changed; floating-point overflow, underflow or cancellation can affect coefficients.'),
    'Polynomial.Multiply(a,k)': article(
        'Scale a polynomial',
        'Multiply every coefficient by the same real scalar.',
        '(k a)(x) = k a(x)',
        'a is a Polynomial and k is a real scalar. Either a*k or k*a provides the operator form.',
        'The input is unchanged. Scaling a finite polynomial by zero produces the zero polynomial; a large factor can overflow coefficients.'),
    'Polynomial.Divide(a,k)': article(
        'Divide a polynomial by a scalar',
        'Divide each polynomial coefficient by a real scalar.',
        '(a / k)(x) = a(x) / k',
        'a is a Polynomial and k is the scalar divisor. The a/k operator is equivalent. Use DivideRemainder for a polynomial divisor.',
        'Choose a finite nonzero divisor for ordinary arithmetic. Zero scalar division follows floating-point coefficient division and can yield infinity or NaN; this routine does not implement rational functions.'),
    'Polynomial.DivideRemainder(a,b)': article(
        'Polynomial quotient and remainder',
        'Perform Euclidean long division and return both polynomial parts of the result.',
        'a = q b + r; degree(r) < degree(b), unless r = 0',
        'a is the dividend and b is the nonzero polynomial divisor. The returned tuple exposes the quotient as Item1 and remainder as Item2; floating-point arithmetic may leave small residual coefficients.',
        'A zero polynomial divisor throws DivideByZeroException. The operation does not mutate either input, but some short-circuit results share an input object: when the dividend degree is smaller, the remainder is a itself. Clone() before modifying result coefficients if independence is required.'),
    'Polynomial.PointwiseMultiply(a,b)': article(
        'Multiply corresponding coefficients',
        'Multiply coefficient pairs with the same exponent, without forming the ordinary polynomial product.',
        'c[k] = a[k] b[k]',
        'a and b are Polynomial values. The output extends only through the smaller input degree; neither input is changed.',
        'The output evaluated at x generally differs from a(x)*b(x). Use Polynomial.Multiply or a*b for that identity.'),
    'Polynomial.PointwiseDivide(a,b)': article(
        'Divide corresponding coefficients',
        'Divide coefficients of the dividend by coefficients of the divisor at the same exponent.',
        'c[k] = a[k] / b[k]',
        'a and b are Polynomial values. The output extends through the degree of a. Missing divisor coefficients are treated as zero, and neither input is changed.',
        'Zero divisor coefficients follow floating-point division and can produce infinity or NaN. This is not long division and does not represent a(x)/b(x); use DivideRemainder for polynomial quotient and remainder.'),
}

# Each teaching note adds an interpretation or an executable, analytically checked
# example. These are independent of the runtime reflection inventory.
LESSONS = {
    'new Polynomial(coefficients)': (
        'For a reusable object p, p.Evaluate(x) evaluates it; p.Differentiate() returns its derivative and p.Integrate() chooses antiderivative constant zero. p.Roots() returns numerical complex roots in unspecified order. General roots use companion-matrix eigenvalues; repeated or poorly conditioned roots may lose accuracy.',
        'var p = new Polynomial(-6,11,-6,1);\nvar roots = p.Roots();',
        'Three roots approximately 1, 2 and 3, in unspecified order.',
        'The coefficients describe (x−1)(x−2)(x−3). Check p.Evaluate(root) and the scale of the coefficients when assessing residuals.'),
    'Polynomial.Zero': (
        'Use zero to start an accumulated sum of polynomials. Its degree sentinel lets the library distinguish an identically zero expression from an ordinary constant of degree zero.',
        'Polynomial.Zero.Evaluate(5)', '0', 'The empty coefficient array denotes the additive identity.'),
    'Polynomial.Evaluate(z,coefficients1,coefficients2, ...)': (
        'Horner evaluation starts at the highest coefficient and repeatedly multiplies by the point before adding the next coefficient. Coefficients remain ordered from constant term upward in the input, despite this reverse traversal.',
        'Polynomial.Evaluate(2,1,-3,2)', '3', 'The polynomial is 1−3x+2x²; at x=2 it gives 1−6+8.'),
    'Polynomial.Fit(x,y,order,method)': (
        'Order selects model complexity; it is not the number of samples. Centering and scaling the x coordinates before fitting can improve conditioning, but evaluation must then use the same transformed coordinate.',
        'var p = Polynomial.Fit(new real[]{-2,-1,0,1,2}, new real[]{15,6,1,0,3}, 2);\nwrite(p.Evaluate(2));',
        'Approximately 3', 'These exact samples follow 1−3x+2x², so the fitted coefficients are approximately 1, −3 and 2.'),
    'Polynomial.Add(a,b)': (
        'Adding a constant shifts the graph vertically; adding a polynomial combines coefficients of equal powers. Polynomial objects are reusable after addition.',
        'Polynomial.Add(new Polynomial(1,2), new Polynomial(3,-1)).Evaluate(2)', '6',
        'The sum is (1+2x)+(3−x)=4+x.'),
    'Polynomial.Subtract(a,b)': (
        'Subtracting expressions is useful for finding their intersections: roots of a-b are points where both polynomials have the same value.',
        'Polynomial.Subtract(new Polynomial(1,2), new Polynomial(3,-1)).Evaluate(2)', '4',
        'The difference is (1+2x)−(3−x)=−2+3x.'),
    'Polynomial.Subtract(b,a)': (
        'The reversed scalar overload supports equations such as target-p(x). Negating only the constant term would leave the wrong polynomial.',
        'Polynomial.Subtract(3.0,new Polynomial(1,2)).Evaluate(2)', '-2',
        'At x=2, the result is 3−(1+4).'),
    'Polynomial.Negate(a)': (
        'Negation reflects the polynomial graph through the horizontal axis. For finite coefficients it preserves every root.',
        'Polynomial.Negate(new Polynomial(1,2)).Evaluate(2)', '-5',
        'The input value is 5; the negated value is −5.'),
    'Polynomial.Multiply(a,b)': (
        'Convolution combines every pair of terms whose exponents add to the output exponent. Multiplying linear factors is a convenient way to construct a polynomial with known roots.',
        'Polynomial.Multiply(new Polynomial(-1,1), new Polynomial(-2,1)).Evaluate(3)', '2',
        'The product is (x−1)(x−2)=2−3x+x², which has value 2 at x=3.'),
    'Polynomial.Multiply(a,k)': (
        'A finite nonzero scale preserves roots while changing the graph height. Multiplying both numerator and denominator by the same scale can be useful when inspecting coefficients.',
        'Polynomial.Multiply(new Polynomial(1,2),3.0).Evaluate(2)', '15',
        'The value 1+2·2=5 is multiplied by three.'),
    'Polynomial.Divide(a,k)': (
        'To normalize a nonzero polynomial to leading coefficient one, divide by its leading coefficient. This keeps its roots, subject to rounding; a tiny leading coefficient can amplify other coefficients.',
        'Polynomial.Divide(new Polynomial(1,2),2.0).Evaluate(2)', '2.5',
        'The result is 0.5+x.'),
    'Polynomial.DivideRemainder(a,b)': (
        'Reconstructing quotient*divisor+remainder checks the direction of the division. A nonzero remainder explains why the rational expression is not generally itself a polynomial.',
        'var parts = Polynomial.DivideRemainder(new Polynomial(1,0,1),new Polynomial(1,1));\nwrite(parts.Item1.Evaluate(3));\nwrite(parts.Item2.Evaluate(3));',
        'Quotient value 2 and remainder value 2, printed consecutively.',
        'x²+1=(x−1)(x+1)+2. Item1 is x−1 and Item2 is the constant two.'),
    'Polynomial.PointwiseMultiply(a,b)': (
        'Coefficientwise operations work on the stored representation. For example, multiplying two linear coefficient arrays this way cannot create the quadratic cross terms of a polynomial product.',
        'Polynomial.PointwiseMultiply(new Polynomial(1,2),new Polynomial(3,-1)).Evaluate(2)', '-1',
        'The coefficients become [3,−2], giving 3−2x, rather than the ordinary product 3+5x−2x².'),
    'Polynomial.PointwiseDivide(a,b)': (
        'Check divisor coefficients individually. A divisor polynomial can be nonzero as a whole while still containing a zero coefficient that causes a nonfinite coefficientwise quotient.',
        'Polynomial.PointwiseDivide(new Polynomial(1,2),new Polynomial(3,-1)).Evaluate(2)',
        'Approximately -3.6666666666666665', 'The coefficients are [1/3,−2], giving 1/3−2x.'),
}


def describe(item, values):
    return ARTICLES[item['signature']]


def enrich(item):
    from enrichment import Enrichment, WorkedExample
    note, code, result, explanation = LESSONS[item['signature']]
    return Enrichment(paragraphs=(note,), examples=(WorkedExample(code, result, explanation),))
