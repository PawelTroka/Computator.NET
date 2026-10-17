"""Additional reviewed mathematical families and practical numerical guidance.

Formulae are authored per operation/family, not inferred from arbitrary prose.
A family equation supplements the original convention unless it is equivalent.
"""
import re
from enrichment import Enrichment, Equation, WorkedExample

EXTRA={}
def add(names,tex,explanation,example=None):
    for name in names.split('|'):
        EXTRA[name]=(tex,explanation,() if example is None else (WorkedExample(*example),))

add('log10',r'\log_{10}x=\frac{\ln x}{\ln10}', 'A common logarithm counts powers of ten: increasing a positive input by a factor of ten increases its logarithm by one. For complex arguments, use the stated logarithm branch rather than real product identities.',('log10(1000)','3','One thousand is ten cubed.'))
add('log2',r'\log_2 x=\frac{\ln x}{\ln2}', 'A base-two logarithm counts doublings. It is useful for binary scales and information quantities, but an information formula still needs the correct probability model.',('log2(8)','3','Three doublings take one to eight.'))
add('log',r'\log_b x=\frac{\ln x}{\ln b}', 'Read the argument order in the signature: the value and the base are different inputs. A real logarithm requires a positive value and a positive base other than one; a complex argument introduces branch choices.')
add('pow',r'x^y=e^{y\ln x}\quad(x>0)', 'A positive real base has an unambiguous real power. A negative base with a noninteger exponent generally requires a complex branch; integer exponents remain repeated multiplication. Use the overload’s domain rather than assuming every real pair produces a finite result.',('pow(2,5)','32','An integer exponent means five factors of two.'))
add('root',r'w^n=z', 'An n-th root solves a power equation; a complex nonzero number has n distinct roots for positive integer n. This helper selects the branch described below, not all roots at once. Use the original convention to decide what negative real inputs mean.')
add('Re',r'\operatorname{Re}(a+ib)=a','The real part is a signed coordinate, not the magnitude. Taking it discards the imaginary component, so it is suitable only when that projection is what the calculation requires.')
add('Im',r'\operatorname{Im}(a+ib)=b','The imaginary part is a real number multiplying i. It does not include another factor of i and it can be negative.')
add('conj',r'\overline{a+ib}=a-ib','Complex conjugation reflects a point across the real axis. Multiplying a complex number by its conjugate gives the square of its magnitude, a real nonnegative quantity.')
add('Magnitude',r'|z|=\sqrt{(\operatorname{Re}z)^2+(\operatorname{Im}z)^2}','Magnitude measures distance from the complex origin and discards direction. It is not the sum of the real and imaginary coordinates.')
add('Phase',r'z=|z|e^{i\arg z}','Phase measures the direction of a nonzero complex number in radians. A principal phase has a cut where it jumps by a full turn; unwrap a sequence explicitly when a continuous accumulated phase is needed.')
add('sgn',r'\operatorname{sgn}x=\begin{cases}-1,&x<0,\\0,&x=0,\\1,&x>0,\end{cases}','Sign records direction without retaining magnitude. The zero case is important in piecewise definitions; do not replace it with division by abs(x) at x=0.',('sgn(-4)','-1','The negative input selects the negative branch regardless of its magnitude.'))
add('KroneckerDelta',r'\delta_{ij}=\begin{cases}1,&i=j,\\0,&i\ne j,\end{cases}','The Kronecker delta selects matching discrete indices, for example diagonal entries of an identity matrix. It is unrelated to an infinitely narrow continuous Dirac impulse.',('KroneckerDelta(2,2)','1','The indices match exactly.'))
add('DiracDelta',r'\int_{-\infty}^{\infty}\delta(x-a)f(x)\,dx=f(a)','This equation describes the mathematical distribution, NOT a quadrature guarantee for the application’s point helper. Sampling an infinite marker at a grid point does not reproduce its unit-area action. For numerical impulses choose and normalize a finite-width model.')
add('gcd',r'\gcd(a,b)=\gcd(b,a\bmod b)','Euclid’s reduction replaces a divisibility problem by a smaller remainder problem. Use the positive-integer domain specified by this wrapper; its zero and negative-input behavior is not the general number-theory convention.',('gcd(12,18)','6','Six is the largest positive integer dividing both values.'))
add('lcm',r'\operatorname{lcm}(a,b)=\frac{ab}{\gcd(a,b)}\quad(a,b>0)','A least common multiple gives the first positive coincidence of two integer periods. The product formula is mathematical; intermediate integer arithmetic can overflow before division in this implementation.',('lcm(4,6)','12','A period-four and a period-six event first coincide again at twelve.'))
add('Eulerφ',r'\varphi(n)=n\prod_{p\mid n}\left(1-\frac1p\right)','Only distinct prime divisors appear in the product. The totient counts coprime residue classes, not the number of primes below n.',('Eulerφ(9)','6','The allowed integers are 1, 2, 4, 5, 7 and 8.'))
add('isPrime|prime',r'p>1,\quad d\mid p\Longrightarrow d\in\{1,p\}\quad(d>0)','Primality asks whether there are nontrivial positive divisors. The two public helpers have different output contracts: isPrime is a predicate, while prime is a numeric flag and does not return the n-th prime.')
add('DoubleFactorial',r'n!!=n(n-2)(n-4)\cdots','Take every second integer rather than multiplying all integers up to n. Odd and even orders therefore form separate products; this is not factorial(n) applied twice.',('DoubleFactorial(7)','105','Multiply 7, 5, 3 and 1.'))
add('TaylorCoeff',r'c_n(x)=\frac{x^n}{n!},\qquad e^x=\sum_{n=0}^{\infty}c_n(x)','This returns one exponential-series term. Obtaining an approximation to exp(x) requires summing terms and controlling the discarded tail; one coefficient is not the whole Taylor approximation.',('TaylorCoeff(3,2)','approximately 1.3333333333','The third term at x=2 is 8/6.'))
add('Permutations',r'P(n)=n!','Permutations count orderings of all distinct objects. Equal-looking or repeated objects require a different counting model.',('Permutations(4)','24','There are 4 choices, then 3, then 2, then 1.'))
add('Variations',r'V(n,k)=\frac{n!}{(n-k)!}','Ordered selections distinguish which object goes in each position. They differ from combinations by the k! possible orders of each chosen set.',('Variations(5,2)','20','There are five choices for first place and four for second.'))
add('VariationsWithRepetition',r'V_{\mathrm{rep}}(n,k)=n^k','Repetition leaves all n choices available at every one of the k positions. The order still matters.',('VariationsWithRepetition(3,2)','9','Three independent choices for each of two positions give nine sequences.'))
add('CombinationsWithRepetition',r'C_{\mathrm{rep}}(n,k)=\binom{n+k-1}{k}','This counts unordered selections with repeated kinds allowed. It is useful for distributing identical objects among distinct categories, rather than ordering distinct objects.',('CombinationsWithRepetition(3,2)','6','From A, B and C choose AA, AB, AC, BB, BC or CC.'))

add('gammaP|leftRegularizedGamma',r'P(a,x)=\frac{1}{\Gamma(a)}\int_0^x t^{a-1}e^{-t}\,dt','The lower regularized gamma function is a fraction of the complete gamma integral. For positive shape and nonnegative x it lies between zero and one. Together with the upper fraction Q it sums to one.',('gammaP(1,1)','approximately 0.6321205588','At shape one the integral is 1−exp(−1).'))
add('gammaQ|rightRegularizedGamma',r'Q(a,x)=\frac{1}{\Gamma(a)}\int_x^\infty t^{a-1}e^{-t}\,dt','This is the upper fraction, not the lower fraction. A dedicated upper-tail evaluation avoids subtracting a CDF already rounded to one when the remaining probability is very small.',('gammaQ(1,1)','approximately 0.3678794412','At shape one the upper tail equals exp(−1).'))
add('BetaNorm',r'I_x(a,b)=\frac{B_x(a,b)}{B(a,b)}','Normalization turns the incomplete beta area into a cumulative probability for positive shapes. The first argument x is the integration limit; a and b are the shape parameters. For a=b=1 the density is uniform and the result is x.',('BetaNorm(0.5,1,1)','0.5','With a uniform density on [0,1], half of the total probability lies below 0.5.'))
add('logBeta',r'\ln B(a,b)=\ln\Gamma(a)+\ln\Gamma(b)-\ln\Gamma(a+b)','A log-normalization is useful when products of small probabilities underflow. Keep the value in log space as long as possible instead of exponentiating immediately.')
add('Pochhammer',r'(a)_x=\frac{\Gamma(a+x)}{\Gamma(a)}','For a nonnegative integer x this is a rising product starting at a. It differs from a falling factorial, which steps down instead. Analytic continuation needs the parameter restrictions and pole conventions below.')
add('PochhammerRelative',r'\frac{(a)_x-1}{x}','The relative form is useful near x=0, where directly subtracting one from a quantity near one loses digits. Its limiting value is digamma(a) when that limit exists.')
add('HurwitzZeta',r'\zeta(s,q)=\sum_{n=0}^{\infty}\frac1{(n+q)^s}','The shift q moves the starting point of the reciprocal-power sum. q=1 recovers Riemann zeta in the convergent half-plane; it is not a second exponent.',('HurwitzZeta(2,1)','approximately 1.6449340668','At shift one the sum is 1+1/4+1/9+⋯=π²/6.'))
add('DirichletEta',r'\eta(s)=\sum_{n=1}^{\infty}\frac{(-1)^{n-1}}{n^s}=(1-2^{1-s})\zeta(s)','Alternating signs improve convergence compared with the positive zeta series. At s=1 the product representation has a removable cancellation; the limiting value is ln(2).',('DirichletEta(1)','approximately 0.6931471806','This is the alternating harmonic sum.'))
add('LambertW0|LambertWm1',r'W(x)e^{W(x)}=x','On the negative real interval where two solutions exist, the branch number chooses which one is returned. Branch zero stays above −1; branch −1 stays below −1. The wrapper’s endpoint restrictions must still be respected.')
add('diLog|SpencesIntegral',r'\operatorname{Li}_2(z)=\sum_{n=1}^{\infty}\frac{z^n}{n^2}\quad(|z|<1)','The convergent series defines the dilogarithm near zero. Elsewhere the implementation uses continuation with its stated branch. Be careful with the name Spence: some references use Li₂(1−z), whereas this application’s alias uses the convention stated below.')
add('PolyLog',r'\operatorname{Li}_s(z)=\sum_{n=1}^{\infty}\frac{z^n}{n^s}\quad(|z|<1)','The order changes the reciprocal-power weighting of the series. Order one connects to −ln(1−z), and order two is the dilogarithm. Do not sum the defining series outside its convergence region as an implementation recipe.')
add('Si',r'\operatorname{Si}(x)=\int_0^x\frac{\sin t}{t}\,dt','Sine integral accumulates both positive and negative lobes of sinc. The area approaches a limit through oscillation; the result is not simply x times the current sinc value.',('Si(0)','0','An integral over a zero-length interval is zero.'))
add('Ci',r'\operatorname{Ci}(x)=-\int_x^\infty\frac{\cos t}{t}\,dt','Cosine integral uses a tail normalization and has a logarithmic singularity near zero. Unlike sine integral, there is no finite value obtained by filling a removable hole at zero.')
add('Ei',r'\operatorname{Ei}(x)=\operatorname{PV}\int_{-\infty}^x\frac{e^t}{t}\,dt','The principal-value prescription balances the singular contributions on opposite sides of zero. It is not an ordinary absolutely convergent integral across the pole and should not be reproduced by naive quadrature.')
add('En',r'E_n(x)=\int_1^\infty\frac{e^{-xt}}{t^n}\,dt','The order n changes the algebraic weight, while x controls exponential decay. The lower bound is one, unlike the integration limits of Ei.')
add('Shi',r'\operatorname{Shi}(x)=\int_0^x\frac{\sinh t}{t}\,dt','The integrand is finite at zero by continuity. Unlike sine integral, hyperbolic sine integral grows rather than oscillating on the positive real axis.')
add('Chi',r'\operatorname{Chi}(x)=\gamma+\ln|x|+\int_0^x\frac{\cosh t-1}{t}\,dt','Subtracting one removes the singularity inside the remaining integral, but the separate logarithm still diverges at zero. Read γ here as the Euler–Mascheroni constant, not the gamma function.')
add('Ti|Tai',r'\operatorname{Ti}_2(x)=\int_0^x\frac{\arctan t}{t}\,dt','The name means an integral of the inverse tangent, not the inverse of a tangent integral. The ratio tends to one at the origin and can be integrated continuously through it.')
add('Clausen',r'\operatorname{Cl}_2(x)=\sum_{n=1}^{\infty}\frac{\sin(nx)}{n^2}','This periodic sine series weights higher harmonics by the inverse square of their order. The argument is an angle in radians, and changing x by 2π leaves the value unchanged.')
add('Dawson',r'D(x)=e^{-x^2}\int_0^x e^{t^2}\,dt','The exponential prefactor balances the rapidly growing integral. Compute Dawson directly rather than multiplying two extreme intermediate quantities. Near zero it resembles x; in the tails it decays approximately as 1/(2x).')
add('Ein',r'\operatorname{Ein}(z)=\int_0^z\frac{1-e^{-t}}{t}\,dt','The apparent division by zero is removable: the numerator behaves like t near the origin. This entire function removes the logarithmic complication found in related exponential integrals.')
add('FresnelC',r'C(x)=\int_0^x\cos\!\left(\frac{\pi t^2}{2}\right)\,dt','The oscillations become faster with t because phase is quadratic. This normalized diffraction convention includes π/2; omitting it changes the input scale and limiting normalization.')
add('FresnelS',r'S(x)=\int_0^x\sin\!\left(\frac{\pi t^2}{2}\right)\,dt','This is the sine component of the normalized Fresnel integral. Pair it with C(x) to describe the two coordinates of the Cornu spiral; neither component alone represents the complex amplitude.')
add('Fresnel',r'F(x)=C(x)+iS(x)','The real and imaginary parts contain the Fresnel cosine and sine integrals. Keeping both parts is important for interference, where phases combine before taking a magnitude.')
add('erfcx',r'\operatorname{erfcx}(z)=e^{z^2}\operatorname{erfc}(z)','The scaled complementary error function removes the leading exponential decay on the positive real axis. It is useful for small-tail calculations where separately computing exp(z²) and erfc(z) would overflow and underflow.')
add('erfi',r'\operatorname{erfi}(z)=-i\operatorname{erf}(iz)','The imaginary error function is real for real inputs despite its name. The definition rotates the argument of erf in the complex plane, producing very different growth on the real axis.')
add('faddeeva',r'w(z)=e^{-z^2}\operatorname{erfc}(-iz)','This complex line-shape function combines Gaussian and dispersive behavior. Its real and imaginary parts play different roles; replacing it by a real error function would discard part of the response.')
add('inverseErf',r'\operatorname{erf}(\operatorname{erf}^{-1}x)=x','The inverse recovers a threshold from an error-function value. Endpoints correspond to infinite limits, and values close to an endpoint are sensitive to small input errors.')
add('inverseErfc',r'\operatorname{erfc}(\operatorname{erfc}^{-1}x)=x','Complementary inversion is useful for a small tail probability supplied directly. It avoids first subtracting that small value from one and losing its significant digits.')
add('logErfc',r'\log\operatorname{erfc}(x)','This returns the logarithm of a complementary tail. Log space can represent probabilities whose ordinary floating-point values would underflow to zero.')
add('erfZ',r'\phi(x)=\frac{e^{-x^2/2}}{\sqrt{2\pi}}','This is the standard normal density, not erf itself. Its integral over an interval gives probability; evaluating it at one point gives a density height.')
add('erfQ',r'Q(x)=\frac12\operatorname{erfc}\!\left(\frac{x}{\sqrt2}\right)','The standard normal upper tail decreases as the threshold increases. It is the probability above x, not the CDF below x.',('erfQ(0)','0.5','Symmetry divides standard normal probability equally at zero.'))
add('hazard',r'h(x)=\frac{\phi(x)}{Q(x)}','The normal hazard divides density by remaining upper-tail probability. It is a conditional rate per unit of the variable, not a cumulative probability.')
add('OwenT',r'T(h,a)=\frac1{2\pi}\int_0^a\frac{e^{-h^2(1+t^2)/2}}{1+t^2}\,dt','Owen’s T describes a Gaussian probability over a wedge-like region. Its second argument controls the geometry, not a normal standard deviation. Use the stated overload contract for any supplied product parameter.')
add('Debye',r'D_n(x)=\frac{n}{x^n}\int_0^x\frac{t^n}{e^t-1}\,dt','The normalization removes the leading small-x power so the mathematical limit at zero is one. Debye functions appear when summing thermal mode contributions; the order is part of the model, not a derivative count.')
add('TransportJ',r'J_n(x)=\int_0^x\frac{t^n e^t}{(e^t-1)^2}\,dt','This thermal transport integral weights modes differently from the Debye integral. The extra factor in the denominator changes the low-temperature and low-argument behavior; the functions are not interchangeable.')
add('SynchrotronF',r'F(x)=x\int_x^\infty K_{5/3}(t)\,dt','The first synchrotron function accumulates a modified-Bessel tail before multiplying by the frequency ratio. It is a spectral shape; physical units and prefactors belong to the radiation model.')
add('SynchrotronG',r'G(x)=xK_{2/3}(x)','This second spectral factor is a pointwise Bessel expression rather than the tail integral defining F. Use the correct combination for the polarization convention of the physical model.')
add('HydrogenicR',r'\int_0^\infty|R_{nl}(r)|^2r^2\,dr=1','The radial wavefunction must be combined with a spherical harmonic for the complete orbital. The factor r² in the radial volume element matters: integrating |R|² without it tests a different normalization.',('HydrogenicR(1,0,1,0)','2','At the origin the hydrogen 1s radial factor is two in the atomic-unit convention.'))
add('SphericalHarmonic',r'\int_0^{2\pi}\int_0^\pi|Y_l^m(\theta,\phi)|^2\sin\theta\,d\theta\,d\phi=1','The angular normalization includes sin(θ), the area element on the unit sphere. θ is measured from the polar axis; confusing it with latitude changes the function. Keep complex phase until probabilities or intensities are formed.')

MATRICES={
'det':(r'\det\begin{pmatrix}a&b\\c&d\end{pmatrix}=ad-bc','Determinant measures signed volume scaling for real matrices. A zero determinant signals singularity, but a tiny computed determinant alone is not a reliable conditioning estimate.'),
'inv':(r'A^{-1}A=AA^{-1}=I','An inverse exists only for a nonsingular square matrix. For a linear system, a direct solve or factorization usually avoids forming the inverse and is preferable numerically.'),
'Tr':(r'\operatorname{tr}A=\sum_i A_{ii}','Trace sums diagonal entries. For a square matrix it also equals the sum of eigenvalues with multiplicity; it is not the sum of every entry.'),
'transpose':(r'(A^{\mathsf T})_{ij}=A_{ji}','Transposition swaps rows and columns without conjugating entries. With complex data, this differs from the Hermitian transpose.'),
'HermitianTranspose':(r'(A^{\mathsf H})_{ij}=\overline{A_{ji}}','Conjugate transposition swaps indices and reverses the sign of imaginary parts. It is the operation used in complex inner products and unitary factorizations.'),
'rank':(r'\operatorname{rank}A=\dim\operatorname{col}A','Rank counts independent column directions. In floating point, nearly dependent directions require a numerical threshold; a computed rank can change when a poorly conditioned matrix is perturbed.'),
'ker':(r'\ker A=\{x:Ax=0\}','The kernel contains vectors mapped to zero. A returned basis spans the whole null space, but individual basis vectors are not unique. Verify the residual of A times each basis vector.'),
'solve':(r'AX=B','Solving recovers unknowns from a linear relation. Check the original augmented-matrix convention below and verify the residual AX−B afterwards; a small residual alone does not guarantee a well-conditioned answer.'),
'pow':(r'A^n=\underbrace{AA\cdots A}_{n\ \mathrm{factors}}','A matrix power uses matrix multiplication, not elementwise exponentiation. For square matrices the zero power is identity; read the implementation’s restrictions before requesting a negative or noninteger exponent.'),
'IdentityMatrix':(r'I_{ij}=\delta_{ij}','The overload taking a matrix copies its shape, not its contents. Only a square identity of the matching size is a two-sided multiplicative identity.'),
'OnesMatrix':(r'M_{ij}=1','An all-ones matrix is not an identity matrix. Multiplication by it forms sums; an identity matrix instead preserves the operand.'),
'ZerosMatrix':(r'M_{ij}=0','The zero matrix is an additive identity of a specified shape. Its dimensions still matter even though every stored value is zero.'),
'Cholesky':(r'A=LL^{\mathsf H}','Positive definiteness is a mathematical prerequisite, not a numerical preference. Use a suitable alternative factorization for indefinite matrices rather than interpreting a failed Cholesky factor as a partial solution.'),
'QR':(r'A=QR,\qquad Q^{\mathsf H}Q=I','QR separates orthogonal directions from upper-triangular coefficients. It is useful for least squares because orthogonal transformations preserve the Euclidean norm.'),
'GramSchmidt':(r'A=QR,\qquad Q^{\mathsf H}Q=I','Gram–Schmidt builds orthogonal directions successively. Nearly parallel columns can lose orthogonality through cancellation, so inspect conditioning and residuals in difficult least-squares problems.'),
'LU':(r'PA=LU','Pivoting permutes rows before triangular elimination. The returned factorization object defines how its permutation is represented; do not multiply factors without checking that convention.'),
'Evd':(r'AV=VD','Each column of V is associated with an eigenvalue in D, subject to the factorization object’s real/complex block convention. A real nonsymmetric matrix can have complex eigenvalues.'),
'Svd':(r'A=U\Sigma V^{\mathsf H}','Singular values measure stretching along orthogonal directions and are nonnegative. Unlike eigenvalue decomposition, SVD is defined for rectangular matrices and is useful for rank and conditioning analysis.'),
'isSymmetric':(r'A=A^{\mathsf T}','Symmetry compares transposed coordinates, without complex conjugation. For complex matrices, a symmetric matrix need not be Hermitian.'),
'isHermitian':(r'A=A^{\mathsf H}','Hermitian symmetry includes conjugation and forces diagonal entries to be real. Use the predicate’s actual floating-point comparison contract rather than assuming an arbitrary tolerance.'),
'isIndentity':(r'A\approx I','The public spelling is isIndentity. Its approximate pattern check can accept a rectangular diagonal-one matrix, so a true result does not prove the matrix is a square multiplicative identity.'),
'isUnitary':(r'AA^{\mathsf H}\approx I','For square matrices, unitarity preserves complex inner products. This wrapper checks row orthonormality and does not itself enforce a square shape; rectangular matrices require a more precise interpretation.'),
'KroneckerProduct':(r'(A\otimes B)_{ij\ \mathrm{block}}=A_{ij}B','Each entry of A scales a whole copy of B. Dimensions multiply rather than add, so tensor-product constructions can become large even when the factors are modest.'),
'KroneckerSum':(r'A\oplus B=A\otimes I+I\otimes B','A Kronecker sum combines operators acting on independent factors of a tensor-product space. It is not the block-diagonal direct sum despite the overlapping mathematical notation.'),
'DirectSum':(r'A\mathbin{\oplus}_{\mathrm{direct}}B=\begin{pmatrix}A&0\\0&B\end{pmatrix}','A direct sum places independent blocks along the diagonal. Row counts and column counts add; no cross-block coupling is introduced.'),
'PointwiseMultiply':(r'(A\odot B)_{ij}=A_{ij}B_{ij}','This equation is the intended elementwise operation, not the current wrapper’s first-operand behavior. The warning below records that A is included twice; do not confuse the buggy fold with ordinary matrix multiplication.'),
'PointwiseDivide':(r'(A\oslash B)_{ij}=A_{ij}/B_{ij}','Elementwise division is not a matrix solve. The original warning below also records the current repeated first operand, which can introduce invalid zero-over-zero entries before any later operands are processed.'),
'minor':(r'B_{uv}=A_{i+u,j+v}','This helper extracts the stated submatrix range. Read its index and size contract; it is not automatically the determinant minor or signed cofactor used in a Laplace expansion.'),
'BitwiseDotProduct':(r'\sum_{k=0}^{31}\operatorname{bit}_k(x)\operatorname{bit}_k(y)','The result counts shared set bits over all 32 bit positions. It is not a parity-reduced inner product over a two-element field; negative integers also contribute their sign bit.')
}

def extra_enrichment(item,article,name):
    sig=item['signature'];kind=item['type'];base=re.sub(r'<.*>', '',name).split('.')[-1]
    if kind=='MatrixFunctions':
        if base in MATRICES:
            tex,text=MATRICES[base]
            return Enrichment((Equation(tex,article.formula,'Mathematical interpretation'),),(text,),(),False)
        text={'matrix':'Rows and columns have different roles: row i and column j select one entry, using zero-based indices. Create a small matrix first and verify its shape before applying an operation to a larger data set.',
              'vector':'A vector is a one-dimensional collection with linear-algebra operations. A single scalar can be interpreted as a size or as one element depending on the overload; use the displayed typed signature to make the intended construction explicit.',
              'array':'An array is a mutable storage container, not automatically a mathematical matrix or vector. Indexing starts at zero, and allocation fixes the number of slots rather than filling them with a sequence of indices.',
              'list':'A list can grow as elements are appended. Unlike a matrix, it has no built-in row/column algebra; convert data deliberately when a numerical routine expects a matrix or array.'}[base]
        return Enrichment((),(text,),(),False)
    if kind=='Integral':
        methods={'simpsonMethod':'Simpson’s rule combines samples with alternating weights four and two. An even subinterval count is required. Smoothness is important: a discontinuity or unresolved narrow peak can defeat a seemingly dense grid.',
          'trapezoidalMethod':'The trapezoidal rule connects samples by straight segments. Refining the grid reduces discretization error for smooth functions, but a small change after one refinement is not a proof that a missed feature was resolved.',
          'rectangleMethod':'Rectangles approximate area from one representative height per subinterval. The choice of sampling point and the number of intervals affect bias; inspect the original method convention below.',
          'monteCarloMethod':'Monte Carlo estimates an average of randomly sampled heights and multiplies by interval width. Sampling error is random and decreases slowly with sample count; reproducibility and uncertainty need separate handling.',
          'rombergMethod':'Romberg extrapolation combines successively refined trapezoidal estimates to cancel leading error terms. The smoothness assumptions behind that extrapolation are important near corners and singularities.',
          'doubleExponentialTransformation':'A change of variables concentrates samples near endpoints. This can help with integrable endpoint singularities, but it does not make divergent integrals finite or remove the need to check convergence.',
          'infiniteAdaptiveGaussKronrodMethod':'Adaptive quadrature subdivides where paired rules suggest larger error. Infinite-interval transformations still depend on the tail behavior; inspect the supported interval convention and error controls.',
          'nonAdaptiveGaussKronrodMethod':'Gauss–Kronrod pairs reuse quadrature nodes to estimate error. A nonadaptive method does not automatically keep subdividing a difficult interval, so narrow or rapidly oscillating features need care.',
          'integrate':'The method name selects a numerical approximation, not a symbolic antiderivative. Choose a rule suited to smoothness, endpoints and oscillation, then check stability under a tighter work budget.'}
        return Enrichment((Equation(r'I=\int_a^b f(x)\,dx', 'Signed area from a to b under f(x).','Quantity being approximated'),),(methods[base],
            'Integration returns signed area: values below the axis contribute negatively. Splitting at known singularities or discontinuities is often more informative than only increasing the number of samples.'),(),False)
    if kind=='Derivative':
        return Enrichment((Equation(r'f^{(n)}(x)=\frac{d^n f}{dx^n}', 'Derivative of order n with respect to x.','Quantity being approximated'),),(
            'Numerical differentiation estimates local change from nearby function values. A smaller step reduces truncation error only until floating-point subtraction and evaluation noise begin to dominate; the smallest possible step is not generally best.',
            'Higher derivative orders amplify noise and cancellation. Check a known smooth example and compare several step sizes before interpreting fine structure in a derivative of sampled or noisy data.'),(),False)
    if kind=='FunctionRoot' or name=='findRoot':
        return Enrichment((Equation(r'f(x_*)=0','A root x* makes the function equal to zero.','Target equation'),),(
            'A sign-changing bracket for a continuous function guarantees at least one root, but not uniqueness. A pole can also change sign, so continuity across the bracket is essential.',
            'Verify both the reported location and its residual f(x*). A small residual can correspond to a large location error near a flat root; a narrow interval alone can also be misleading near a discontinuity.'),(),False)
    if kind=='StatisticsFunctions':
        if name=='rnd':return Enrichment((),('This generator is mutable state. A seed supports repeatable sequences only when the same calls are made in the same order; sharing it among independent experiments couples their results.',),(),False)
        tex=r'U\sim\operatorname{Uniform}(0,1),\qquad X=a+(b-a)U'
        return Enrichment((Equation(tex,'A uniform draw U scales and shifts into the requested range.','Sampling model'),),(
            'Read the typed overload: integer limits use an inclusive integer interval, while real arguments scale a continuous draw. An interval endpoint in an example is not a promise that random sampling will hit it in a finite run.',
            'A complex overload has its own geometric convention below. Independent real and imaginary draws fill a rectangle, whereas one shared interpolation parameter stays on a line segment; these are different distributions.'),(),False)
    if kind=='MathematicalTransformations':return Enrichment((Equation(
        r'\begin{aligned}H_k&=\frac{1}{\sqrt N}\sum_{n=0}^{N-1}x_n\left[\cos\!\left(\frac{2\pi kn}{N}\right)+\sin\!\left(\frac{2\pi kn}{N}\right)\right],\\H(H(x))&=x\qquad(N>0).\end{aligned}',
        article.formula,'Real orthonormal Hartley transform'),),(
        'This operation acts on sampled chart data. A point transformation is not necessarily equivalent to transforming the underlying continuous function, especially after interpolation or numerical differentiation.',
        'Keep an unmodified copy when comparing transformations. The stored sample spacing and chosen chart interval are part of the numerical result.'),(),True)
    if kind=='ScriptingFunctions':
        if base.startswith('File') or 'file' in sig or 'path' in sig or 'writer' in sig:
            text='File operations affect external state, unlike a pure mathematical expression. Use a deliberate working directory and a disposable example file when learning. Check the original append/overwrite and ownership rules below; opening a writer is not a numerical calculation.'
        elif base.startswith('read'):text='Interactive input pauses execution until data is supplied. Keep the expected type and conversion rules explicit. A script that works with a desktop input dialog may not have the same interaction in a headless or worker-backed environment.'
        elif base.startswith('plot'):text='A plotted curve or surface is a sampled representation. Insufficient resolution can hide oscillations and discontinuities, and joining samples across a pole can suggest a false continuous curve. Adjust bounds and sampling quality to the feature being investigated.'
        elif base in ('write','writeln','show'):text='Output is a presentation step, not a change to the mathematical value. Keep intermediate values in variables when later operations need their full precision; reparsing a rounded display can change the calculation.'
        else:text='A conversion changes the representation or type of a value. It does not add physical units or improve the original numerical precision; handle out-of-range and unsupported conversions explicitly.'
        return Enrichment((),(text,),(),False)
    if name in EXTRA:
        tex,text,examples=EXTRA[name]
        return Enrichment((Equation(tex,article.formula or text,'Mathematical interpretation'),),(text,),examples,False)
    if kind!='SpecialFunctions':return Enrichment()
    if 'Bessel' in name or name.startswith(('Hankel','SphericalHankel','Struve')):
        if 'Zeros' in name:tex=r'J_\nu(j_{\nu,s})=0';text='The argument s selects a numbered positive zero, not an evaluation coordinate. Zeros determine eigenfrequencies when a boundary condition forces a cylindrical mode to vanish.'
        elif 'Prime' in name:tex=r'J_n^{\prime}(x)=\frac{J_{n-1}(x)-J_{n+1}(x)}{2}';text='A derivative changes the radial boundary condition: a zero derivative can represent a different physical mode than a zero function value. The prime is differentiation, not a shifted order.'
        elif 'Modified' in name or name.startswith('logBessel'):
            tex=r'x^2y^{\prime\prime}+xy^{\prime}-(x^2+\nu^2)y=0'
            text='Modified cylindrical Bessel solutions grow or decay rather than oscillate on the positive real axis. Spherical variants introduce a radial scaling and half-integer order; scaled or logarithmic forms remove dominant growth, so the exact normalization below is essential.'
        elif name.startswith('Struve'):
            tex=r'x^2y^{\prime\prime}+xy^{\prime}-(x^2+\nu^2)y=\frac{4(x/2)^{\nu+1}}{\sqrt\pi\,\Gamma(\nu+1/2)}'
            text='The modified Struve function solves an inhomogeneous equation. The forcing term distinguishes it from modified Bessel I and K; a similar name or large-x scale is not an equality of functions.'
        else:
            tex=r'x^2y^{\prime\prime}+xy^{\prime}+(x^2-\nu^2)y=0'
            text='Bessel functions describe radial modes in cylindrical geometry. Spherical and Riccati variants rescale related half-integer solutions; Hankel functions combine independent solutions into complex waves. Use the specific function and normalization below, rather than treating all family names as aliases.'
        return Enrichment((Equation(tex,tex,'Underlying cylindrical equation' if not ('Zeros' in name or 'Prime' in name) else 'Defining relation'),),(text,),(),False)
    if name.startswith(('AiZeros','BiZeros')):
        f=('\\operatorname{Ai}' if name.startswith('Ai') else '\\operatorname{Bi}')+('^{\\prime}' if 'Prime' in name else '')
        return Enrichment((Equation(f+r'(a_s)=0,\qquad a_1>a_2>\cdots',article.formula),),('The input selects a zero by index; it is not the x coordinate at which Ai or Bi is evaluated. Distinguish zeros of the function from zeros of its derivative, since they impose different boundary conditions.',),(),True)
    if name.startswith('FermiDirac'):
        tex=r'F_j(x)=\frac1{\Gamma(j+1)}\int_0^\infty\frac{t^j}{e^{t-x}+1}\,dt\quad(j>-1)'
        if sig.count(',') and name=='FermiDiracF0':tex=r'F_0(x,b)=\ln(1+e^{x-b})'
        return Enrichment((Equation(tex,article.formula,'Normalized integral'),),('The gamma normalization is part of this convention. Tables that omit it differ by Γ(j+1). The exponential denominator represents occupation, while the power of t accounts for the mode weighting.',),(),False)
    if name.startswith('Mathieu'):
        return Enrichment((Equation(r'y^{\prime\prime}+(a-2q\cos2x)y=0', 'Angular Mathieu equation.','Underlying angular equation'),),('Periodic angular Mathieu solutions occur only for selected characteristic values. The An/Bn routines find those values, whereas CE/SE evaluate the corresponding angular functions. Modified radial functions use a transformed equation; the names are not interchangeable.',),(),False)
    if name.startswith('Coulomb'):
        return Enrichment((Equation(r'u^{\prime\prime}+\left(1-\frac{2\eta}{\rho}-\frac{L(L+1)}{\rho^2}\right)u=0','Radial Coulomb equation.','Underlying wave equation'),),('The radial coordinate ρ and Coulomb parameter η play different roles. Regular and irregular solutions encode different behavior at the origin; derivative and normalization helpers act on those solutions rather than introducing a new potential.',),(),False)
    if name.startswith('Coupling') or name=='ClebschGordan':
        return Enrichment((Equation(r'|j_1-j_2|\le j\le j_1+j_2','Triangle rule for coupling two angular momenta.','Selection-rule example'),),('Recoupling coefficients are amplitudes with phase conventions, not probabilities by themselves. Integer wrappers can encode twice the physical spin; using 1 where 1/2 was intended changes the state space before any arithmetic takes place.',),(),False)
    if name.startswith('Hypergeometric'):
        forms={'Hypergeometric0F1':r'{}_0F_1(;c;x)=\sum_{k=0}^{\infty}\frac{x^k}{(c)_k k!}',
        'Hypergeometric1F1':r'{}_1F_1(a;b;x)=\sum_{k=0}^{\infty}\frac{(a)_k x^k}{(b)_k k!}',
        'Hypergeometric2F1':r'{}_2F_1(a,b;c;x)=\sum_{k=0}^{\infty}\frac{(a)_k(b)_k x^k}{(c)_k k!}',
        'Hypergeometric2F1renorm':r'\frac{{}_2F_1(a,b;c;x)}{\Gamma(c)}',
        'HypergeometricU':r'xy^{\prime\prime}+(b-x)y^{\prime}-ay=0,\quad U(a,b,x)\sim x^{-a}',
        'Hypergeometric2F0':r'{}_2F_0(a,b;;x)\sim\sum_{k=0}^{\infty}\frac{(a)_k(b)_k x^k}{k!}'}
        return Enrichment((Equation(forms[name],article.formula,'Defining representation'),),('The parameters control the differential equation and its normalization, while x is the evaluation coordinate. A terminating series, a convergent series and an asymptotic expansion require different interpretations. The representation below is not permission to sum an infinite series outside its domain.',),(),False)
    if name.startswith('Gegenbauer'):
        return Enrichment((Equation(r'\sum_{n=0}^{\infty}C_n^{(\alpha)}(x)t^n=(1-2xt+t^2)^{-\alpha}',article.formula),),('The coefficient of tⁿ in this generating function is the degree-n polynomial. α selects an orthogonal-polynomial family; setting α=1/2 recovers Legendre polynomials, subject to the domain conventions below.',),(),True)
    polynomials={
    'Laguerre':r'L_n^{(\alpha)}(x)=\sum_{k=0}^{n}(-1)^k\binom{n+\alpha}{n-k}\frac{x^k}{k!}',
    'LegendreP':r'P_l(x)=\frac1{2^l l!}\frac{d^l}{dx^l}(x^2-1)^l',
    'LegendreQ':r'Q_0(x)=\frac12\ln\left|\frac{1+x}{1-x}\right|',
    'SphericalLegendreP':r'\sqrt{\frac{(2l+1)(l-m)!}{4\pi(l+m)!}}P_l^m(x)',
    'ChebyshevT':r'T_n(\cos\theta)=\cos(n\theta)',
    'HermiteH':r'H_n(x)=(-1)^n e^{x^2}\frac{d^n}{dx^n}e^{-x^2}',
    'HermiteHe':r'\operatorname{He}_n(x)=(-1)^n e^{x^2/2}\frac{d^n}{dx^n}e^{-x^2/2}',
    'ZernikeR':r'R_n^m(\rho)=\sum_{s=0}^{(n-m)/2}\frac{(-1)^s(n-s)!\rho^{n-2s}}{s!((n+m)/2-s)!((n-m)/2-s)!}',
    'ConicalP':r'P_{-1/2+i\lambda}^{\mu}(x)',
    'SphericalConicalP':r'P_{-1/2+i\lambda}^{-l-1/2}(x)',
    'CylindricalConicalP':r'P_{-1/2+i\lambda}^{-m}(x)',
    'LegendreH3D':r'H_0(\lambda,\eta)=\frac{\sin(\lambda\eta)}{\lambda\sinh\eta}'}
    if name in polynomials:
        tex=polynomials[name]
        if name=='LegendreP' and sig.count(',')==2:tex=r'P_l^m(x)=(-1)^m(1-x^2)^{m/2}\frac{d^mP_l(x)}{dx^m}'
        return Enrichment((Equation(tex,article.formula,'Representation / normalization'),),('Degree, order and evaluation coordinate are separate inputs. Changing a degree chooses another member of the family, not another point on the same curve. The normalization and weighting convention determine how coefficients from different references can be compared.',),(),False)
    if name.startswith('JacobiElliptic'):
        return Enrichment((Equation(r'\operatorname{sn}^2(u|m)+\operatorname{cn}^2(u|m)=1,\qquad\operatorname{dn}^2(u|m)+m\operatorname{sn}^2(u|m)=1','Jacobi elliptic identities; m is the parameter.','Family identities'),),('These generalize trigonometric functions: at parameter m=0, sn and cn reduce to sin and cos and dn becomes one. Here m is the parameter, while Legendre-form elliptic-integral routines below use the modulus k; m=k².',),(),False)
    if name.startswith('Elliptic'):
        symbol=name[len('Elliptic'):];upper=r'\pi/2' if sig.count(',')==0 or (symbol=='Π' and sig.count(',')==1) else r'\phi'
        kernels={'K':r'\frac1{\sqrt{1-k^2\sin^2t}}','F':r'\frac1{\sqrt{1-k^2\sin^2t}}','E':r'\sqrt{1-k^2\sin^2t}','D':r'\frac{\sin^2t}{\sqrt{1-k^2\sin^2t}}','Π':r'\frac1{(1+n\sin^2t)\sqrt{1-k^2\sin^2t}}'}
        return Enrichment((Equation(r'\int_0^{'+upper+'}'+kernels[symbol]+r'\,dt',article.formula,'Integral convention'),),('The complete integral ends at π/2; the incomplete integral ends at the supplied angle. The application uses modulus k, not parameter m=k². For the third kind, also check the sign of the characteristic in the denominator: conventions differ between libraries.',),(),False)
    if name.startswith('Carlson'):
        forms={'C':r'R_C(x,y)=\frac12\int_0^\infty\frac{dt}{(t+y)\sqrt{t+x}}','F':r'R_F(x,y,z)=\frac12\int_0^\infty\frac{dt}{\sqrt{(t+x)(t+y)(t+z)}}','D':r'R_D(x,y,z)=\frac32\int_0^\infty\frac{dt}{(t+z)\sqrt{(t+x)(t+y)(t+z)}}','J':r'R_J(x,y,z,p)=\frac32\int_0^\infty\frac{dt}{(t+p)\sqrt{(t+x)(t+y)(t+z)}}'}
        return Enrichment((Equation(forms[name[-1]],article.formula),),('Symmetric elliptic forms avoid selecting an amplitude/modulus representation first. Their arguments are not simply the k and φ of Legendre-form integrals, so translate the formula rather than reusing arguments by position.',),(),True)
    return Enrichment()

# Optimization landscapes: evaluation is not an optimization algorithm.
add('Ackley',r'-20e^{-0.2\sqrt{\frac1d\sum_j x_j^2}}-e^{\frac1d\sum_j\cos(2\pi x_j)}+20+e','The central basin is surrounded by periodic local structure. Use it to test whether an optimizer can reach the narrow global basin from several starting points, not just whether it decreases the objective once.')
add('Rastrigin',r'10d+\sum_{j=1}^d\left[x_j^2-10\cos(2\pi x_j)\right]','Cosine modulation creates many local minima inside a larger quadratic bowl. A local descent method can stop away from the global minimum even though the function is smooth.')
add('Bohachevsky1',r'x^2+2y^2-0.3\cos(3\pi x)-0.4\cos(4\pi y)+0.7','The differently scaled coordinates and oscillations test how an optimizer handles directional curvature. The displayed standard form has its minimum zero at the origin. The pinned Math.NET implementation omits the final +0.7 and returns -0.7 there instead. This vertical shift changes objective values, not the minimizing coordinates; evaluating the function does not search for them.',('Bohachevsky1(0,0)+0.7','approximately 0','The current implementation returns -0.7 at the origin. Adding the omitted constant recovers the standard benchmark value zero; this is an explicit normalization correction, not the raw function result.'))
add('Himmelblau',r'(x^2+y-11)^2+(x+y^2-7)^2','There are multiple equally good global minimizers. Different starting points can legitimately converge to different answers, so compare objective values and residual conditions rather than demanding one particular coordinate pair.',('Himmelblau(3,2)','0','Both squared terms vanish: 9+2−11=0 and 3+4−7=0.'))
add('Matyas',r'0.26(x^2+y^2)-0.48xy','The mixed term rotates and flattens the quadratic valley. It gives a simple example in which coordinate directions are not the principal curvature directions.',('Matyas(0,0)','0','All quadratic contributions vanish at the origin.'))
add('Rosenbrock',r'\sum_{j=1}^{d-1}\left[100(x_{j+1}-x_j^2)^2+(1-x_j)^2\right]','The minimum lies in a narrow curved valley. Finding the valley is easier than progressing along it; a small gradient component in one coordinate does not establish convergence in all coordinates.',('Rosenbrock(1,1)','0','Both terms of the two-dimensional expression vanish at one.'))
add('dropWave',r'-\frac{1+\cos(12\sqrt{x^2+y^2})}{2+(x^2+y^2)/2}','The objective oscillates with distance from the origin. Its negative sign means the global minimum is the deepest negative value, not the zero approached in the far field.',('dropWave(0,0)','-1','At zero radius the numerator is two and the denominator is two, with a minus sign.'))
add('sixHumpCamel',r'\left(4-2.1x^2+\frac{x^4}{3}\right)x^2+xy+(-4+4y^2)y^2','Several stationary points coexist, including two symmetry-related global minima. A solver returning a stationary point has not necessarily found the best minimum; compare the objective and local curvature.')
add('Gabor',r'g(x)=A e^{-\frac{(x-x_0)^2}{2\sigma^2}}\cos(\omega x+\phi)','A Gabor model localizes an oscillation inside a Gaussian envelope. Width controls spatial localization, while carrier frequency controls oscillation. This displayed one-dimensional model is an interpretation, not a replacement for the exact phase, rotation, offset and scale conventions of the overload below.')
