"""Reviewed teaching notes and TeX for the existing catalogue, not a second API.

Definitions still come from the original subject modules. TeX is authored here,
not guessed from arbitrary HTML. Exact stored values are never replaced by SI
updates. Examples use the public spelling and preserve documented wrapper bugs.
"""
from dataclasses import dataclass, replace
import re
from common import A
import elementary, special, distributions, physical
from constant_lessons import CONSTANT_LESSONS
from physical_equations import BASE_EQUATIONS
from physical_family_lessons import family_lesson

@dataclass(frozen=True)
class WorkedExample:
    code: str
    result: str
    explanation: str
    # Scalar authoring expressions become complete writeln statements in the
    # scripting editor, so readers and runtime tests execute identical text.
    editor: str = 'expression'

@dataclass(frozen=True)
class Equation:
    tex: str
    text: str
    label: str = 'Definition'

@dataclass(frozen=True)
class Enrichment:
    equations: tuple = ()
    paragraphs: tuple = ()
    examples: tuple = ()
    replaces_formula: bool = False

# Each explanation describes an interpretation or a useful calculation, rather
# than repeating the argument list or appending catalogue-wide generic filler.
LESSONS = {}
def lesson(names, tex, paragraphs, examples=()):
    for name in names.split('|'):
        LESSONS[name] = (tex, tuple(paragraphs), tuple(WorkedExample(*e) for e in examples))

lesson('sin', r'\sin x=\frac{e^{ix}-e^{-ix}}{2i}', [
    'On a circle of radius one, sin(x) is the vertical coordinate after rotating through x radians. Increasing the angle by a full turn, 2π, returns to the same point; changing its sign reflects the vertical coordinate.',
    'For an oscillation A·sin(ω·t+φ), A sets the amplitude, ω is angular frequency in radians per unit time and φ is the starting phase. Convert degrees by multiplying by PI/180 before evaluation.'
], [('sin(30*PI/180)', '0.5', 'Thirty degrees is π/6 radians. Passing 30 without the conversion would ask for the sine of 30 radians, a different angle.')])
lesson('cos', r'\cos x=\frac{e^{ix}+e^{-ix}}{2}', [
    'Cosine is the horizontal coordinate on the unit circle. It starts at one at zero angle, reaches zero at a quarter-turn and returns to one after 2π radians.',
    'Cosine is even: changing the sign of the angle leaves its value unchanged. Together, sine and cosine resolve a vector into perpendicular components; their squared components sum to one.'
], [('cos(60*PI/180)', '0.5', 'Convert the angle to radians first. This is also the horizontal component of a unit vector inclined at sixty degrees.')])
for name, numerator, denominator, text in [
    ('tan','sin','cos','Tangent gives the slope of a line at the specified angle. Near a vertical line, cosine approaches zero and the slope becomes very large.'),
    ('cot','cos','sin','Cotangent exchanges the adjacent and opposite sides of the tangent ratio. Its singularities are at the zeros of sine, not the zeros of cosine.'),
    ('sec','1','cos','Secant is the reciprocal of the horizontal unit-circle coordinate. Where that coordinate vanishes, there is no finite real secant.'),
    ('csc','1','sin','Cosecant is the reciprocal of the vertical unit-circle coordinate. Its reciprocal interpretation explains the poles at integer multiples of π.')]:
    a = '1' if numerator=='1' else '\\'+numerator+' x'
    lesson(name, rf'\{name} x=\frac{{{a}}}{{\{denominator} x}}', [text,
        'A floating-point value near a pole is not a reliable representation of an infinite mathematical limit. On a plot, keep the branches separated rather than treating a large sampled value as a connection across the singularity.'])
lesson('sinc', r'\operatorname{sinc}x=\begin{cases}\frac{\sin x}{x},&x\ne0,\\1,&x=0.\end{cases}', [
    'The ratio sin(x)/x has a removable singularity: both numerator and denominator approach zero, but their ratio approaches one. The function supplies that limiting value explicitly, so a graph does not have a hole at the origin.',
    'The nonzero real zeros occur at integer multiples of π. Signal-processing texts often use a normalized sinc whose zeros occur at nonzero integers; that is a different scaling of the argument.'
], [('sinc(0)', '1', 'Use the supplied continuous value instead of evaluating 0/0.'), ('sinc(PI)', 'approximately 0', 'This is the first positive zero. Floating-point evaluation may leave a tiny residual.')])
for name, tex, text in [
    ('sinh',r'\sinh x=(e^x-e^{-x})/2','Hyperbolic sine is an odd function: the sign changes with x. Close to zero it behaves like x, but its magnitude grows exponentially rather than oscillating.'),
    ('cosh',r'\cosh x=(e^x+e^{-x})/2','Hyperbolic cosine is even, has its minimum value one at zero and describes the shape of a hanging uniform chain in ideal conditions.'),
    ('tanh',r'\tanh x=\frac{\sinh x}{\cosh x}','Hyperbolic tangent smoothly compresses the whole real line into the interval between minus one and one. Its slope at zero is one and its tails saturate.'),
    ('coth',r'\coth x=\frac{\cosh x}{\sinh x}','Unlike tanh, hyperbolic cotangent has a pole at zero. For positive x it decreases toward one from above.'),
    ('sech',r'\operatorname{sech}x=\frac{1}{\cosh x}','The reciprocal of cosh is a positive, even bell-shaped curve. It is one at zero and decays toward zero in either real direction.'),
    ('csch',r'\operatorname{csch}x=\frac{1}{\sinh x}','The reciprocal of sinh is odd and singular at zero. It decays in magnitude for large positive or negative real arguments.')]:
    lesson(name,tex,[text,'Hyperbolic functions use exponentials, not angles on the unit circle. Their names do not imply periodicity on the real axis.'])
from inverse_lessons import REAL_INVERSES, COMPLEX_INVERSES
for name, (tex, paragraphs, examples) in REAL_INVERSES.items():
    lesson(name, tex, paragraphs, examples)
lesson('exp',r'e^z=\sum_{n=0}^{\infty}\frac{z^n}{n!}',[
    'The exponential describes growth or decay at a rate proportional to the current value. In A·exp(k·t), a positive k means growth and a negative k means decay; changing A rescales the initial amount.',
    'A real exponential is positive. For imaginary arguments it describes rotation, which connects exponentials to sine and cosine. Very large real parts can overflow floating-point range.'
],[('exp(0)', '1', 'Zero elapsed time leaves the initial multiplier unchanged.'),('exp(-1)', 'approximately 0.3678794412', 'After one time constant, exponential decay has about 36.8 percent of its initial amount.')])
lesson('ln|log',r'\ln x=y\quad\Longleftrightarrow\quad e^y=x',[
    'A logarithm turns multiplication into addition: ln(a·b)=ln(a)+ln(b) for positive real inputs. This is useful for combining likelihoods or working with products too small to represent directly.',
    'The real natural logarithm requires x>0. A complex logarithm also records phase and chooses a branch; adding 2πi describes other branches rather than another value returned by the same call.'
],[('ln(e)', '1', 'The natural logarithm and exponential undo one another on positive real values.')])
lesson('sqrt',r'(\sqrt z)^2=z',[
    'A positive real number has two square roots with opposite signs; sqrt selects the nonnegative one. It does not return a pair of roots.',
    'The real-input wrapper automatically returns a complex value for a negative argument: sqrt(-1) is i. Squaring a principal complex square root recovers its argument, but taking the principal square root of z² does not always recover z.'
],[('sqrt(9)', '3', 'The other solution of y²=9 is −3; it is not the value selected by sqrt.'),
   ('Im(sqrt(-9))', '3', 'No explicit complex literal is needed: the negative real argument is promoted and the selected result is 3i.')])
lesson('abs',r'|a+ib|=\sqrt{a^2+b^2}',[
    'Absolute value is distance from zero. For real numbers it removes the sign; for complex numbers it combines both coordinates into a nonnegative magnitude.',
    'Magnitude does not retain phase. Two different complex numbers can therefore have the same absolute value, and abs(z) must not be used when the direction of z is important.'
])
lesson('factorial',r'n!=\prod_{k=1}^n k,\qquad 0!=1',[
    'Factorial counts the orderings of n distinct objects. At each position there is one fewer choice, giving n·(n−1)·…·1. The empty arrangement explains the convention 0!=1.',
    'This routine is an integer-product helper even though its argument type permits nonintegers. Use the separate gamma function for analytic continuation, and read the implementation warning before supplying fractional or negative inputs.'
],[('factorial(5)', '120', 'Five distinct objects have 5·4·3·2·1 possible orders.')])
lesson('BinomialCoeff|Combinations',r'\binom nk=\frac{n!}{k!(n-k)!}',[
    'Start with the ordered selections of k objects from n, then divide by the k! ways of ordering each selected group. The result counts groups, not sequences.',
    'The equality C(n,k)=C(n,n−k) follows by exchanging the chosen and unchosen objects. These values grow rapidly, so a floating-point result is not an arbitrary-precision integer count.'
],[('Combinations(5,2)', '10', 'There are 5·4=20 ordered pairs, but each unordered pair is counted twice.')])
lesson('Heaviside',r'H(x)=\begin{cases}0,&x<0,\\\frac12,&x=0,\\1,&x>0.\end{cases}',[
    'A step switches a contribution on at a threshold. H(x−a), for example, switches at a rather than at zero.',
    'The half-value at the jump is a convention. It does not smooth the discontinuity or turn a numerical derivative into a finite approximation to an ideal impulse.'
],[('Heaviside(0)', '0.5', 'The value right at the switch is explicitly chosen; either side still approaches a different limit.')])
lesson('gamma',r'\begin{aligned}\Gamma(z)&=\int_0^\infty t^{z-1}e^{-t}\,dt\quad(\Re z>0),\\\Gamma(z+1)&=z\Gamma(z).\end{aligned}',[
    'Gamma extends the factorial relation beyond whole numbers. The shift matters: Γ(n)=(n−1)!, so the continuous counterpart of n! is Γ(n+1), not Γ(n).',
    'In the integral, t^(z−1) controls the behavior near zero and e^(−t) suppresses the far tail. Outside the integral’s convergence region the function is defined by analytic continuation, except at its poles. For large positive arguments, logGamma can avoid forming an overflowing gamma value.'
],[('gamma(5)', '24', 'Apply the recurrence four times: Γ(5)=4·3·2·1·Γ(1), and Γ(1)=1.'),('gamma(0.5)', 'approximately 1.7724538509', 'The half-integer value is √π, showing why gamma appears in Gaussian integrals.')])
lesson('logGamma',r'\exp(\operatorname{Log}\Gamma(z))=\Gamma(z)',[
    'Log-gamma keeps a rapidly growing gamma value in logarithmic form. It is useful in factorial ratios and statistical log-likelihoods, where adding and subtracting log-gamma values can replace products and quotients.',
    'For a positive real argument the result is the natural logarithm of Γ(x). A complex logarithm has a branch convention; it need not agree with taking the principal log of an already rounded gamma result.'
],[('logGamma(5)', 'approximately 3.1780538303', 'This is ln(24). Using the direct log-gamma routine becomes more important when Γ(x) itself overflows.')])
lesson('psi',r'\psi(z)=\frac{\Gamma^{\prime}(z)}{\Gamma(z)},\qquad\psi(n)=H_{n-1}-\gamma',[
    'Digamma measures how quickly log-gamma changes. It appears when differentiating gamma-based likelihoods or normalization factors with respect to a shape parameter.',
    'At positive integers it is a harmonic sum minus the Euler–Mascheroni constant. This provides an independent way to check values without differentiating gamma numerically.'
],[('psi(1)', 'approximately -0.5772156649', 'The harmonic sum H₀ is empty, leaving minus the Euler–Mascheroni constant.')])
lesson('polyGamma',r'\psi^{(n)}(x)=\frac{d^{n+1}}{dx^{n+1}}\ln\Gamma(x)',[
    'The order counts derivatives of digamma. Order zero is digamma itself; order one is trigamma, the curvature of log-gamma.',
    'Different orders describe different functions, not powers of one function. In particular, the superscript on ψ⁽ⁿ⁾ denotes differentiation, not exponentiation.'
],[('polyGamma(1,1)', 'approximately 1.6449340668', 'Trigamma at one equals the reciprocal-square sum π²/6.')])
lesson('Beta',r'B(a,b)=\int_0^1 t^{a-1}(1-t)^{b-1}\,dt=\frac{\Gamma(a)\Gamma(b)}{\Gamma(a+b)}',[
    'The beta integral balances powers of the distances to the two ends of the unit interval. Interchanging a and b reflects t about one half, so the complete beta function is symmetric.',
    'Dividing the integrand by B(a,b) produces a probability density on [0,1]. The complete special function is a normalization constant, not the Beta distribution object and not a cumulative probability.'
],[('Beta(2,3)', 'approximately 0.0833333333', 'Γ(2)Γ(3)/Γ(5)=1·2/24=1/12.')])
for name, description, example in [
    ('Ai', 'Ai is the Airy solution that decays as x becomes large and positive. Its normalization is fixed; an arbitrary multiple also solves the equation but is not the value of Ai.',
        ('Ai(0)', 'approximately 0.3550280539', 'At the turning point, Ai(0)=1/[3^(2/3)Γ(2/3)]. Decay on the positive side does not mean a zero value at the origin.')),
    ('Bi', 'Bi is the independent Airy solution that grows as x becomes large and positive. It is not the reciprocal of Ai; both become oscillatory for negative arguments.',
        ('Bi(0)', 'approximately 0.6149266274', 'At zero, Bi(0)=sqrt(3)·Ai(0). The normalization distinguishes the two standard solutions.')),
    ('AiPrime', 'AiPrime differentiates the decaying Airy solution with respect to x. The negative slope at zero shows that Ai is decreasing there; the Prime suffix is not a square or an order parameter.',
        ('AiPrime(0)', 'approximately -0.2588194038', 'The initial slope is −1/[3^(1/3)Γ(1/3)], rather than the initial function value Ai(0).')),
    ('BiPrime', 'BiPrime differentiates the growing Airy solution with respect to x. Its positive slope at zero distinguishes it from AiPrime. The derivative itself does not satisfy the same homogeneous Airy equation.',
        ('BiPrime(0)', 'approximately 0.4482883574', 'The slope is sqrt(3) times the magnitude of AiPrime(0), and is positive.'))]:
    lesson(name, r'y^{\prime\prime}(x)-x\,y(x)=0', [
        'The Airy equation models a turning point between oscillatory and growing or decaying behavior, such as the boundary between propagating and evanescent waves.', description], [example])
lesson('erf',r'\operatorname{erf}(z)=\frac{2}{\sqrt\pi}\int_0^z e^{-t^2}\,dt',[
    'The error function accumulates a Gaussian-shaped curve from zero to its argument. It is odd, equals zero at the origin and approaches ±1 along the real axis.',
    'For a standard normal variable, Φ(x)=(1+erf(x/√2))/2. The scaling by √2 and the shift by one half are essential: erf(x) itself is not the standard normal CDF.'
],[('erf(0)', '0', 'The integration interval has zero length.'),('erf(1)', 'approximately 0.8427007929', 'This is the accumulated Gaussian integral in the erf convention, not the probability that a standard normal is below one.')])
lesson('erfc',r'\operatorname{erfc}(z)=1-\operatorname{erf}(z)',[
    'The complementary error function represents the remaining Gaussian tail. For a large positive real argument it is small even when erf(x) has rounded to one.',
    'Prefer a direct tail routine when subtracting a CDF from one would lose precision. The separate implementation warning explains which complex wrapper still performs that subtraction internally.'
],[('erfc(0)', '1', 'At zero, erf is zero, so its complement is one.')])
lesson('RiemannZeta',r'\zeta(s)=\sum_{n=1}^\infty n^{-s}\quad(\Re s>1)',[
    'Zeta adds reciprocal powers of the positive integers. The displayed series converges only to the right of Re(s)=1; values elsewhere require analytic continuation rather than summing the same series.',
    'The pole at one is not an ordinary large finite value. At even positive integers there are exact formulas involving powers of π that make useful checks of the numerical implementation.'
],[('RiemannZeta(2)', 'approximately 1.6449340668', 'The reciprocal-square series sums to π²/6.')])
lesson('Logistic',r'\sigma(x)=\frac1{1+e^{-x}}',[
    'Logistic turns a log-odds value into a probability. Zero log-odds means equal odds and therefore probability one half; positive inputs favor success.',
    'An output near zero or one can be extremely sensitive when converted back to log-odds. Finite precision may round to an endpoint, so do not assume that applying Logit after saturation recovers the original input.'
],[('Logistic(0)', '0.5', 'Equal odds correspond to a probability of one half.')])
lesson('Logit',r'\operatorname{logit}(p)=\ln\frac{p}{1-p}',[
    'Odds compare the probability of success with the probability of failure. Logit takes their natural logarithm, mapping probabilities strictly between zero and one to the real line.',
    'The endpoint probabilities correspond to infinite log-odds. A small numerical change close to an endpoint produces a much larger change in logit than the same change near one half.'
],[('Logit(0.5)', '0', 'The odds are 0.5/0.5=1 and ln(1)=0.')])

# Distribution equations describe the MODEL. The operation explanation below
# separately explains density, accumulated probability, quantile and sampling.
MODEL_TEX = {
'Normal': r'f(x)=\frac{e^{-(x-\mu)^2/(2\sigma^2)}}{\sigma\sqrt{2\pi}},\quad\sigma>0',
'Bernoulli':r'\Pr(X=1)=p,\quad\Pr(X=0)=1-p',
'Beta':r'f(x)=\frac{x^{a-1}(1-x)^{b-1}}{B(a,b)},\quad0<x<1',
'Binomial':r'\Pr(X=k)=\binom nk p^k(1-p)^{n-k}',
'Categorical':r'\Pr(X=k)=\frac{w_k}{\sum_j w_j}',
'Cauchy':r'f(x)=\frac{1}{\pi s\left[1+((x-\ell)/s)^2\right]}',
'Chi':r'f(x)=\frac{2^{1-\nu/2}x^{\nu-1}e^{-x^2/2}}{\Gamma(\nu/2)},\quad x>0',
'ChiSquared':r'f(x)=\frac{x^{\nu/2-1}e^{-x/2}}{2^{\nu/2}\Gamma(\nu/2)}',
'ContinuousUniform':r'f(x)=\begin{cases}(b-a)^{-1},&a\le x\le b,\\0,&\text{otherwise}.\end{cases}',
'ConwayMaxwellPoisson':r'\Pr(X=k)=\frac{\lambda^k}{(k!)^\nu Z},\quad Z=\sum_{j=0}^{\infty}\frac{\lambda^j}{(j!)^\nu}',
'Dirichlet':r'f(\mathbf{x})=\frac{\Gamma(\sum_i\alpha_i)}{\prod_i\Gamma(\alpha_i)}\prod_i x_i^{\alpha_i-1},\quad\sum_i x_i=1',
'DiscreteUniform':r'\Pr(X=k)=\frac1{b-a+1},\quad k=a,\ldots,b',
'Erlang':r'f(x)=\frac{r^k x^{k-1}e^{-rx}}{\Gamma(k)},\quad k\in\mathbb{N},\quad r>0',
'Exponential':r'f(x)=r e^{-rx},\quad F(x)=1-e^{-rx}\quad(x\ge0)',
'FisherSnedecor':r'X=\frac{U/d_1}{V/d_2},\quad U\sim\chi^2_{d_1},\quad V\sim\chi^2_{d_2}',
'Gamma':r'f(x)=\frac{r^a x^{a-1}e^{-rx}}{\Gamma(a)},\quad\mathrm{E}[X]=\frac ar',
'Geometric':r'\Pr(X=k)=p(1-p)^{k-1},\quad k=1,2,\ldots',
'Hypergeometric':r'\Pr(X=k)=\frac{\binom Kk\binom{N-K}{n-k}}{\binom Nn}',
'InverseGamma':r'f(x)=\frac{\beta^\alpha x^{-\alpha-1}e^{-\beta/x}}{\Gamma(\alpha)},\quad x>0',
'InverseWishart':r'f(X)\propto|X|^{-(\nu+d+1)/2}e^{-\operatorname{tr}(\Psi X^{-1})/2}',
'Laplace':r'f(x)=\frac{e^{-|x-\ell|/s}}{2s},\quad\operatorname{Var}(X)=2s^2',
'LogNormal':r'f(x)=\frac{e^{-(\ln x-\mu)^2/(2\sigma^2)}}{x\sigma\sqrt{2\pi}},\quad x>0',
'MatrixNormal':r'\operatorname{vec}(X)\sim\mathcal{N}\!\left(\operatorname{vec}(M),K\otimes V\right)',
'Multinomial':r'\Pr(\mathbf{X}=\mathbf{x})=\frac{n!}{\prod_i x_i!}\prod_i p_i^{x_i},\quad\sum_i x_i=n',
'NegativeBinomial':r'\Pr(X=k)=\frac{\Gamma(k+r)}{\Gamma(r)k!}p^r(1-p)^k,\quad k=0,1,\ldots',
'NormalGamma':r'\tau\sim\operatorname{Gamma}(\alpha,\beta),\quad\mu\mid\tau\sim\mathcal{N}(m,(\lambda\tau)^{-1})',
'Pareto':r'f(x)=\frac{\alpha x_m^\alpha}{x^{\alpha+1}},\quad x\ge x_m',
'Poisson':r'\Pr(X=k)=e^{-\lambda}\frac{\lambda^k}{k!},\quad k=0,1,\ldots',
'Rayleigh':r'f(x)=\frac{x}{\sigma^2}e^{-x^2/(2\sigma^2)},\quad x\ge0',
'Stable':r'\varphi(t)=\mathrm{E}[e^{itX}]',
'StudentT':r'f(x)=\frac{\Gamma((\nu+1)/2)}{s\sqrt{\nu\pi}\Gamma(\nu/2)}\left(1+\frac{(x-\ell)^2}{\nu s^2}\right)^{-(\nu+1)/2}',
'Triangular':r'f(x)=\begin{cases}\frac{2(x-a)}{(b-a)(c-a)},&a\le x<c,\\\frac{2(b-x)}{(b-a)(b-c)},&c\le x\le b.\end{cases}',
'Weibull':r'f(x)=\frac{k}{\lambda}\left(\frac{x}{\lambda}\right)^{k-1}e^{-(x/\lambda)^k}',
'Wishart':r'X=\sum_{i=1}^{\nu}z_i z_i^\mathsf{T},\quad z_i\sim\mathcal N(0,V)',
'Zipf':r'\Pr(X=k)=\frac{k^{-s}}{\sum_{j=1}^N j^{-s}},\quad k=1,\ldots,N'
}
MODEL_LESSONS = {
'Normal':'Use a Gaussian model for fluctuations around a center when symmetric light tails are appropriate. A change of units rescales both the mean and standard deviation; it does not turn a standard deviation into a variance.',
'Bernoulli':'A Bernoulli variable records one yes/no outcome. Averaging many independent trials estimates the success probability; adding a fixed number of them gives a binomial count.',
'Beta':'The two shapes control behavior near zero and one. Equal shapes make the density symmetric, while shapes below one can concentrate density near the endpoints. A density spike is not a positive point probability at that endpoint.',
'Binomial':'Use this model for a fixed number of independent trials with the same success chance. When sampling without replacement from a small population, those trials are dependent and the hypergeometric model is more appropriate.',
'Categorical':'The array positions are the outcome labels. Weights [1,3] mean that label 1 is three times as likely as label 0; they do not mean the possible returned values are 1 and 3.',
'Cauchy':'Large outliers are an intrinsic part of this model. Its location is a median, but the population mean and variance are undefined; reporting a finite sample variance does not create finite theoretical moments.',
'Chi':'Think of independent standard normal coordinates and measure their distance from the origin. The resulting nonnegative length has a chi law; squaring that length gives a chi-squared variable.',
'ChiSquared':'A sum of squared standardized Gaussian residuals produces this law when the independence and parameter-count assumptions hold. The degrees of freedom describe independent contributions, not a scale to multiply the observed value by.',
'ContinuousUniform':'Equal-length pieces of the interval have equal probability. Halving the interval doubles the density height so that the total area remains one; a density above one is therefore perfectly possible.',
'ConwayMaxwellPoisson':'This model changes the relative weight of larger counts through the factorial term. The ordinary Poisson law is the special case ν=1. A parameter check alone does not establish convergence of its normalizing series.',
'Dirichlet':'A draw is a whole vector of proportions that must sum to one. Increasing all concentrations while preserving their ratios concentrates draws around the same mean vector; changing the ratios moves that mean.',
'DiscreteUniform':'Count the possible integer values before computing a mass. A fair six-sided die has six outcomes, including both 1 and 6; this differs from the exclusive upper bound used by many random-number APIs.',
'Erlang':'If independent exponential waiting times have the same rate, their sum has an Erlang distribution. The integer shape counts events and the mean waiting time is shape/rate.',
'Exponential':'The rate is an event frequency, so the average wait is its reciprocal. The memoryless property says that time already spent waiting does not change the distribution of the remaining wait in this ideal model.',
'FisherSnedecor':'This ratio compares two independent estimates of Gaussian variability. The order of the two degrees of freedom matters: reversing numerator and denominator produces a reciprocal random variable.',
'Gamma':'The shape changes the curve while the rate sets its time or length scale. Doubling the rate halves the mean at fixed shape. The shape-one case is exponential; a positive integer shape is Erlang.',
'Geometric':'Count the successful trial itself. With success chance one half, the probability of success on trial one is one half, and success on trial two has probability one quarter. This is a count, not a continuous waiting time.',
'Hypergeometric':'Each draw changes the remaining population. That dependence distinguishes sampling without replacement from a binomial experiment and constrains which counts are actually possible.',
'InverseGamma':'Reciprocating a gamma variable reverses small and large values. The resulting upper tail can be heavy enough that its mean or variance does not exist even though sampling and the density remain defined.',
'InverseWishart':'A sample is a covariance-shaped positive-definite matrix. The matrix scale is not automatically its mean; degrees of freedom control concentration and whether the mean exists.',
'Laplace':'The sharp peak and exponential tails distinguish this model from a Gaussian. Its scale is the mean absolute deviation from its location, while its standard deviation is √2 times that scale.',
'LogNormal':'Multiplicative effects become additive after taking logarithms. The median is exp(mu), whereas the mean is exp(mu+sigma²/2); using mu as the mean of the original positive observations is a parameterization error.',
'MatrixNormal':'Row and column covariance are separate factors of a larger covariance on vectorized entries. Their dimensions must match the mean matrix. The formula uses column stacking, so transposing a matrix also changes the covariance interpretation.',
'Multinomial':'Each experiment returns all category counts. Those counts are dependent because their sum is fixed; increasing one count leaves fewer observations for the others.',
'NegativeBinomial':'Here the variable counts failures, not the total number of trials. For integer r, adding r converts the failure count into a trials-until-r-successes count. The real-shape extension keeps the probability model but not that literal counting story.',
'NormalGamma':'A draw couples a mean parameter to a precision parameter. Precision is inverse variance, so larger precision means a narrower conditional Gaussian; treating it as standard deviation reverses the effect.',
'Pareto':'Above a minimum size, the tail decays by a power rather than an exponential. Shape controls how severe extreme observations are and whether population moments exist.',
'Poisson':'The parameter is the expected count during the chosen observation interval. Scaling the interval scales that expected count under a constant-rate model. Events must be sufficiently independent for this interpretation.',
'Rayleigh':'The magnitude of two independent zero-mean Gaussian coordinates with equal standard deviation has this law. It often models a nonnegative amplitude rather than either signed coordinate.',
'Stable':'Stable laws are characterized most generally through a characteristic function, not an elementary density formula. The α, β, scale and location conventions must match the implementation, especially at α=1; sampling support does not imply every PDF/CDF has a closed form.',
'StudentT':'The degrees of freedom control tail heaviness. With many degrees of freedom the shape approaches a Gaussian; with few, large deviations are much more likely. The scale parameter is not generally the standard deviation.',
'Triangular':'The minimum, maximum and mode specify two straight density slopes. The mode is the peak position, not necessarily the midpoint or mean. Endpoint modes are limiting cases of the displayed interior-mode formula.',
'Weibull':'Shape one gives an exponential waiting-time model. Shapes above one describe increasing hazard, while shapes below one describe decreasing hazard; the scale controls the horizontal unit of the distribution.',
'Wishart':'For integer degrees of freedom the sample is a sum of outer products of independent Gaussian vectors. This construction explains positive semidefiniteness; sufficient rank and the stated parameter conditions are needed for a nonsingular law.',
'Zipf':'The result is a rank between one and a finite maximum. Increasing the exponent concentrates probability toward the first ranks. This finite normalization differs from the infinite zeta distribution.'
}
# Demonstrations deliberately have closed-form or symmetry-based answers; they
# are also executed in the reference-example suite instead of being prose only.
DISTRIBUTION_EXAMPLES = {
'Normal': [('Normal.CDF(0,1,1)-Normal.CDF(0,1,-1)','approximately 0.6826894921','Subtract the lower cumulative probability from the upper one to find the probability of lying within one standard deviation.')],
'Bernoulli':[('Bernoulli.PMF(0.25,1)','0.25','The mass at one is the success probability, not a density height.')],
'Beta':[('MathNet.Numerics.Distributions.Beta.PDF(2,2,0.5)','1.5','The symmetric density peaks above one. Its area, not its peak height, is the probability. The qualified class name avoids the scalar Beta function name collision in TSL.')],
'Binomial':[('Binomial.PMF(0.5,4,2)','0.375','Choose the two successful trials in six ways, each pattern having probability 1/16.')],
'Categorical':[('Categorical.PMF(new double[]{1,3},1)','0.75','The second category receives weight 3 out of total weight 4. Its index is one.')],
'Cauchy':[('Cauchy.CDF(0,1,0)','0.5','Symmetry places half the probability below the location; this does not imply that a population mean exists.')],
'ChiSquared':[('ChiSquared.CDF(2,2)','0.632120558829','Two degrees of freedom give an exponential law with rate one half.')],
'ContinuousUniform':[('ContinuousUniform.PDF(0,0.5,0.25)','2','Density two over an interval of length one half integrates to total probability one.')],
'DiscreteUniform':[('DiscreteUniform.PMF(1,6,6)','0.166666666667','Both endpoints count, so a fair die has six equally likely integer outcomes.')],
'Exponential':[('Exponential.CDF(2,0.5)','0.632120558829','At one mean waiting time, 1/rate=0.5, the accumulated probability is 1−e⁻¹.')],
'Gamma':[('Gamma.PDF(1,2,0.5)','0.735758882343','Shape one reduces gamma to the exponential density with rate two.')],
'Geometric':[('Geometric.PMF(0.5,2)','0.25','One failure followed by one success has probability (1−0.5)·0.5.')],
'Laplace':[('Laplace.CDF(0,1,0)','0.5','Half the probability lies on each side of the location.')],
'LogNormal':[('LogNormal.CDF(0,1,1)','0.5','The logarithm of one is zero, the center of the associated normal distribution; one is the lognormal median.')],
'NegativeBinomial':[('NegativeBinomial.PMF(1,0.5,0)','0.5','Zero failures before the first success means the first trial succeeds.')],
'Pareto':[('Pareto.CDF(1,2,2)','0.75','The remaining tail above two is (1/2)²=1/4, leaving cumulative probability 3/4.')],
'Poisson':[('Poisson.PMF(2,0)','0.135335283237','With expected count two, the probability of no events is e⁻².')],
'Rayleigh':[('Rayleigh.CDF(1,1)','0.393469340287','At x equal to the scale, the CDF is 1−e⁻¹ᐟ².')],
'StudentT':[('StudentT.CDF(0,1,3,0)','0.5','The center splits the probability in half even though the tails are heavier than Gaussian tails.')],
'Weibull':[('Weibull.CDF(1,2,2)','0.632120558829','Shape one is exponential with scale two; x=2 is one scale unit.')],
}

def value_tex(value):
    if value in ('NaN','Infinity','-Infinity'):return {'NaN':r'\mathrm{NaN}','Infinity':r'+\infty','-Infinity':r'-\infty'}[value]
    if value.startswith('<'):
        parts=value.strip('<>').split(';');return value_tex(parts[0].strip())+'+'+'('+value_tex(parts[1].strip())+')i'
    if not re.fullmatch(r'-?\d+(?:\.\d+)?(?:[Ee][+-]?\d+)?',value):raise ValueError('Unreviewed constant format: '+value)
    if 'e' in value.lower():
        m,e=re.split('[Ee]',value);return m+rf'\times10^{{{int(e)}}}'
    return value


def enrich(item, article, values):
    kind=item['type']; signature=item['signature']; name=signature.split('(')[0]
    if kind in distributions.FAMILIES:
        op=name.split('.')[-1]; notes=[MODEL_LESSONS[kind]]; operation_equations=[]
        if op=='CDF':
            if kind in distributions.DISCRETE:
                notes.append('For integer bounds a and b, the inclusive interval probability is F(b)−F(a−1). Subtracting F(a) instead would remove the probability mass at the lower endpoint as well.')
                operation_equations.append(Equation(r'\Pr(a\le X\le b)=F(b)-F(a-1),\quad a,b\in\mathbb Z', 'For integer bounds, inclusive probability from a to b is F(b) minus F(a−1).', 'Inclusive integer interval'))
            else:
                notes.append('For a continuous law, subtract F(a) from F(b) to obtain the interval probability between a and b. This uses cumulative areas rather than density heights; individual endpoints have zero point probability.')
                operation_equations.append(Equation(r'\Pr(a\le X\le b)=F(b)-F(a)=\int_a^b f(x)\,dx', 'Continuous interval probability is the CDF difference F(b)−F(a), or the density integrated from a to b.', 'Interval probability'))
        elif op=='PDF':
            notes.append('For a short interval of width Δx around x, probability is approximately f(x)·Δx when the density changes slowly there. The approximation improves as the interval shrinks; it is not a probability assigned to the single point x.')
        elif op=='PDFLn':
            notes.append('The returned number is ℓ(x)=ln f(x), not f(x). Recover a density with exp(ℓ(x)) before multiplying by a small interval width; multiplying the log-density itself by a width does not give a probability. A log-density can be positive when the density exceeds one.')
            notes.append('For independent observations, add their log-densities to obtain a joint log-likelihood. Keep that sum in log space when an ordinary product would underflow. Integrating log-density does not produce a cumulative probability.')
            operation_equations.append(Equation(r'\ell(x)=\ln f(x),\qquad f(x)=e^{\ell(x)}', 'Log-density ℓ(x) is ln f(x); the ordinary density is exp(ℓ(x)).', 'Log-density result'))
        elif op=='PMF':
            notes.append('Add point masses for disjoint allowed outcomes to obtain their combined probability. Unlike a continuous density, this number already refers to an individual outcome, so no bin width is needed.')
        elif op=='PMFLn':
            notes.append('The returned number is the natural logarithm of a point probability. Exponentiate it to recover the mass. A finite log-mass is at most zero; probability one has log-mass zero and probability zero has log-mass −∞.')
            notes.append('Add log-masses to combine independent observations, because their probabilities multiply. For alternative outcomes whose probabilities add, use a sum of exponentials, or its stable log-sum-exp form, instead of adding their log-masses directly.')
            operation_equations.append(Equation(r'\ell_k=\ln\Pr(X=k),\qquad\Pr(X=k)=e^{\ell_k}', 'The log-mass is ln of the point probability; exponentiating recovers that probability.', 'Log-mass result'))
        elif op.startswith('InvCDF'):
            notes.append('A quantile reverses the CDF question: instead of asking how much probability lies below a value, choose the accumulated probability and obtain its threshold. Use 0.95 for the 95th percentile, not 95.')
        elif op.startswith('Sample'):
            notes.append('A random draw is one simulated outcome, not an expected value. For reproducible experiments supply a seeded generator where the overload permits one; for lazy sample sequences, take a finite number before materializing them.')
        elif op.startswith('With'):
            notes.append('Construction creates a reusable distribution object. Read its properties or call its sampling methods afterwards; constructing the object does not itself generate an observation. Keep scale, rate, variance and precision conversions explicit.')
        elif op=='Estimate':
            notes.append('Parameter estimation answers which parameters summarize the observations under this model. It does not establish that the model fits the data; inspect residuals, tail behavior and domain assumptions separately.')
        elif op.startswith('IsValid'):
            notes.append('Use this as an early input check, then handle errors from the actual operation. Accepted parameters can include limiting cases with infinite moments or a singular density, so a true result is not a general numerical guarantee.')
        eq=Equation(MODEL_TEX[kind],article.formula,'Distribution model')
        # Stable's characteristic function and Wishart's constructive definition
        # supplement rather than replace the more general original convention.
        replace_formula=kind not in ('Stable','Wishart','NormalGamma','Triangular')
        from distribution_examples import examples_for
        examples=examples_for(kind,op,DISTRIBUTION_EXAMPLES.get(kind,()))
        return Enrichment((eq,*operation_equations),tuple(notes),examples,replace_formula)
    if kind in ('PhysicalConstants','MathematicalConstants'):
        value=values.get(name, {'i':'<0; 1>', 'Infinity':'Infinity', 'NaN':'NaN'}.get(name));eqs=[];notes=[];examples=[];replaces=False
        if value is None: raise ValueError('Missing constant value: '+name)
        if name in CONSTANT_LESSONS:
            tex,paragraphs,ex=CONSTANT_LESSONS[name]
            eqs.append(Equation(tex,article.formula,'Definition / interpretation'))
            notes.extend(paragraphs)
            examples=[WorkedExample(*e) for e in ex]
            # Keep prose formula containing SI qualifications visible as a convention.
        if kind=='PhysicalConstants':
            if name not in CONSTANT_LESSONS:
                canonical=physical.ALIASES.get(name,name)
                if canonical in BASE_EQUATIONS:
                    tex,paragraph=BASE_EQUATIONS[canonical]
                    paragraphs=(paragraph,) if paragraph else ()
                else:
                    authored=family_lesson(name)
                    if authored is None:raise ValueError('Missing physical definition equation: '+name)
                    tex,paragraphs=authored
                eqs.append(Equation(tex,article.formula,'Definition / interpretation'))
                notes.extend(paragraphs)
            notes.append('The symbol in a physical equation denotes a quantity with units; this identifier evaluates to a plain number. Supply the other quantities in the compatible units stated in this article, because the expression engine does not attach dimensional units or convert them automatically.')
            if 'Relationship' in name:
                notes.append('Use a conversion relationship in its stated direction: multiply a value in the source unit by the factor to obtain the target-unit value. Converting back requires division by that factor, not a second multiplication. Check the direction rather than inferring it from the magnitude.')
            elif 'Ratio' in name:
                notes.append('A ratio cancels matching dimensions. Multiplying the denominator quantity by this ratio recovers the numerator in the same units; swapping numerator and denominator requires the reciprocal. A magnetic-moment ratio can carry a physical sign.')
            elif name.startswith(('AtomicUnit','NaturalUnit','Planck')):
                notes.append('A natural or atomic unit is a scale chosen to simplify equations, not an instruction to change the application’s unit system. Multiply a dimensionless number of these units by the stored scale to express it in the stated conventional units.')
            else:
                notes.append('Keep the distinction between the stated physical definition and the stored approximation. Related constants may come from different historical adjustments; substituting them into an exact identity need not give exact numerical agreement. Extra displayed digits are not a measurement uncertainty.')
        elif name not in CONSTANT_LESSONS:
            notes.append('Evaluating this identifier retrieves its stored numerical approximation; it does not run the defining infinite series, limit or geometric construction. Use the definition to understand the constant and the stored-value line to judge the numerical precision actually available.')
            if value not in ('NaN','Infinity','-Infinity'):
                notes.append('Equality in a mathematical identity does not imply bit-for-bit equality after floating-point evaluation. When checking a relation involving this constant, compare an error or residual against an appropriate tolerance instead of demanding exact equality.')
        eqs.append(Equation(value_tex(value), 'Stored application value: '+value+'.','Stored numeric value'))
        return Enrichment(tuple(eqs),tuple(notes),tuple(examples),replaces)
    canonical=elementary.ALIASES.get(name,name) if kind=='ElementaryFunctions' else special.ALIASES.get(name,name) if hasattr(special,'ALIASES') else name
    if kind=='ElementaryFunctions' and '(z' in signature and canonical in COMPLEX_INVERSES:
        tex, paragraph, example=COMPLEX_INVERSES[canonical]
        return Enrichment((Equation(tex,article.formula,'Intended inverse' if canonical=='arsinh' else 'Definition'),),
            (paragraph,),(WorkedExample(*example),),canonical!='arsinh')
    if canonical in LESSONS:
        tex,paragraphs,examples=LESSONS['psi' if canonical=='polyGamma' and ',' not in signature else canonical]
        # Overloads can represent a different mathematical operation. Never give
        # incomplete gamma/beta the complete-function formula or generic log(x,b)
        # the natural-log definition.
        if canonical=='gamma' and ',' in signature:
            return Enrichment((Equation(r'\Gamma(a,x)=\int_x^\infty t^{a-1}e^{-t}\,dt',article.formula),),(
                'The lower limit x removes the beginning of the complete gamma integral, leaving an unnormalized upper tail. Divide by Γ(a) to obtain the regularized upper fraction, or use the dedicated upper-tail routine.',
                'For integer a=3, integrating twice by parts gives Γ(3,x)=exp(−x)(x²+2x+2). This area can exceed one; only division by Γ(3)=2 turns it into an upper-tail probability.'),
                (WorkedExample('gamma(3,1)','approximately 1.8393972059','The upper area is exp(−1)(1+2+2)=5/e. Dividing by Γ(3)=2 gives the separate regularized value Q(3,1).'),),True)
        if canonical=='Beta' and signature.count(',')>1:
            return Enrichment((Equation(r'B_x(a,b)=\int_0^x t^{a-1}(1-t)^{b-1}\,dt',article.formula,'Intended definition'),),(
                'Incomplete beta stops the beta integral at x rather than at one. It is an accumulated area, not yet a probability until normalized by the complete beta function.',
                'The implementation warning below is essential: the public wrapper currently forwards a different argument order. The worked example deliberately calls the correctly ordered Math.NET alternative, not the faulty wrapper.'),
                (WorkedExample('MathNet.Numerics.SpecialFunctions.BetaIncomplete(1,1,0.5)','0.5','With both shapes equal to one the integrand is one, so the area from zero to 0.5 is 0.5.'),),True)
        if canonical in ('ln','log') and ',' in signature:
            from enrichment_more import extra_enrichment
            return extra_enrichment(item,article,'log')
        label='Intended inverse' if canonical=='arsinh' else 'Underlying Airy equation' if canonical in ('AiPrime','BiPrime') else 'Definition'
        return Enrichment((Equation(tex,article.formula,label),),paragraphs,examples,canonical not in ('AiPrime','BiPrime','arsinh'))
    from enrichment_more import extra_enrichment
    return extra_enrichment(item,article,canonical)
