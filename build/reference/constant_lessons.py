"""Authored definitions, interpretations and scalar examples for named constants.

The subject modules retain titles, sources, stored-value qualifications and units.
These lessons explain the mathematics; they never infer definitions from decimal
strings or replace the application's historical constants.
"""

CONSTANT_LESSONS = {}


def lesson(names, tex, paragraphs, examples=()):
    """Share a reviewed lesson only among explicitly named semantic aliases."""
    if isinstance(paragraphs, str):
        paragraphs = (paragraphs,)
    for name in names.split('|'):
        if name in CONSTANT_LESSONS:
            raise ValueError('Duplicate constant lesson: ' + name)
        CONSTANT_LESSONS[name] = (tex, tuple(paragraphs), tuple(examples))


lesson('PI', r'C=2\pi r,\qquad A=\pi r^2,\qquad 1\text{ turn}=2\pi\text{ radians}',
       'The circumference scales with radius, whereas the enclosed area scales with its square. The same constant converts full turns to radians; an angle written in degrees must be converted before passing it to a trigonometric function.',
       [('PI*2*2', 'approximately 12.5663706144', 'A disk of radius two has four times the area of a unit disk. The result is in square units.')])
lesson('e', r'e=\sum_{n=0}^{\infty}\frac1{n!}=\lim_{n\to\infty}\left(1+\frac1n\right)^n',
       'The finite expression compounds a unit annual growth rate in n equal steps. Making the steps shorter approaches continuous compounding, while the factorial series gives another definition of the same number.',
       [('e-pow(1.0+1.0/10,10)', 'approximately 0.1245393684', 'Ten compounding steps still give less than e. The positive difference illustrates convergence of the defining limit.')])
lesson('EulerMascheroni', r'\gamma=\lim_{n\to\infty}\left(\sum_{k=1}^n\frac1k-\ln n\right)',
       'Both the harmonic sum and logarithm grow without bound, but their difference approaches a finite limit. At a finite n, the difference still contains a truncation error; evaluating this constant does not compute that limit anew.',
       [('1.0+1.0/2+1.0/3-ln(3)-EulerMascheroni', 'approximately 0.1575053798', 'The three-term harmonic approximation is above the limiting constant. This expression displays its remaining error.')])
lesson('GoldenRatio', r'\varphi=\frac{1+\sqrt5}{2},\qquad \varphi^2=\varphi+1',
       'Removing a unit square from a rectangle of aspect ratio φ leaves a smaller rectangle with the same aspect ratio. This self-similarity produces a quadratic relation, not a new independent constant.',
       [('GoldenRatio*GoldenRatio-GoldenRatio', 'approximately 1', 'Rearranging the defining quadratic gives a residual check using the stored approximation.')])
lesson('InverseGoldenRatio', r'\frac1\varphi=\varphi-1=\frac{\sqrt5-1}{2}',
       'The reciprocal is less than one and also equals the fractional part of φ. Multiplying the two stored constants should be close to one, although independent rounded values need not give exact floating-point equality.',
       [('GoldenRatio*InverseGoldenRatio', 'approximately 1', 'The long and short sides exchange roles when an aspect ratio is inverted.')])
lesson('SilverRatio', r'\delta_{\mathrm S}=1+\sqrt2,\qquad \delta_{\mathrm S}^2=2\delta_{\mathrm S}+1',
       'The coefficient two distinguishes this quadratic from the golden-ratio equation. The legacy identifier SilverConstant refers to the plastic number, so its value does not satisfy this silver-ratio identity.',
       [('SilverRatio*SilverRatio-2*SilverRatio', 'approximately 1', 'This checks the quadratic relation for the actual SilverRatio identifier.')])
lesson('AperysConstant|ApéryConstantζ3', r'\zeta(3)=\sum_{n=1}^{\infty}\frac1{n^3}',
       'The reciprocal cubes form a positive decreasing series. After n terms, comparing the tail with integrals gives 1/[2(n+1)²] < remainder < 1/(2n²); these bounds explain how much of the constant a short partial sum misses.',
       [('AperysConstant-(1.0+1.0/8+1.0/27)', 'approximately 0.0400198661', 'After three terms the missing tail lies between 1/32 and 1/18. The constant is not the three-term partial sum.')])
lesson('ConwaysConstant|ConwayConstantλ3', r'\frac{L_{n+1}}{L_n}\longrightarrow\lambda',
       'Lₙ counts digits in the usual look-and-say sequence. It is the length ratio that approaches λ, not the ratio of the numerical values represented by those digits. ConwaysConstant stores a shorter approximation than ConwayConstantλ3.')
lesson('KhinchinsConstant|KhinchinConstantK_K0',
       r'\left(\prod_{j=1}^{n}a_j\right)^{1/n}\longrightarrow K,\qquad K=\prod_{r=1}^{\infty}\left(1+\frac1{r(r+2)}\right)^{\log_2r}',
       'The aⱼ are regular continued-fraction partial quotients. The limiting geometric mean holds for almost every real number; a periodic example with every partial quotient equal to one has mean one instead. The two identifiers retain different decimal approximations.')
lesson('ArtinConstant', r'A=\prod_{p\ \mathrm{prime}}\left(1-\frac1{p(p-1)}\right)',
       'Each prime supplies a factor smaller than one. Primitive-root density predictions also depend on the chosen base and their conjectural hypotheses; the product alone is not a probability law for every integer base.')
lesson('BernsteinConstantβ', r'\beta=\lim_{n\to\infty}n\inf_{\deg p\le n}\max_{-1\le x\le1}\bigl||x|-p(x)\bigr|',
       'First choose the polynomial that minimizes the worst error over the whole interval, then scale that optimal error by its degree. The corner in |x| is why this example behaves differently from approximating an analytic function.')
lesson('BlazysConstant', r'B=2+\cfrac{2}{3+\cfrac{3}{5+\cfrac{5}{7+\cdots}}}',
       'Read this as a nested fraction whose numerators and denominators use successive primes. Truncating it changes the result; storing one rounded real number does not preserve an unlimited, recoverable list of prime digits.')
lesson('BrunConstantforTwinPrimesB4', r'B_2=\sum_{\substack{p,\ p+2\ \mathrm{prime}}}\left(\frac1p+\frac1{p+2}\right)',
       'The initial pairs (3,5) and (5,7) both contain 5, and each pair contributes its own reciprocal terms. This convergent reciprocal sum differs from the twin-prime product constant and does not by itself prove infinitely many twin primes.')
lesson('BrunConstantforPrimeCousinsB4', r'B_4=\sum_{\substack{p>3\\p,\ p+4\ \mathrm{prime}}}\left(\frac1p+\frac1{p+4}\right)',
       'The exceptional pair (3,7) is omitted in the convention matching this stored estimate. The sum therefore starts with (7,11), (13,17) and (19,23). Including (3,7) would add 1/3+1/7=10/21, not a rounding correction.')
lesson('BrunConstantforPrimeQuadruplesB4', r'\sum_{\substack{p,p+2,p+6,p+8\ \mathrm{prime}}}\left(\frac1p+\frac1{p+2}+\frac1{p+6}+\frac1{p+8}\right)',
       'One term comes from a whole prime quadruplet, for example (5,7,11,13). The shared B4 suffix does not make this the cousin-prime sum: the admissible pattern and the number of reciprocals are different.')
lesson('CahenConstantC', r'C=\sum_{n=0}^{\infty}\frac{(-1)^n}{s_n-1},\qquad s_0=2,\quad s_{n+1}=1+\prod_{j=0}^{n}s_j',
       'The recurrence gives 2,3,7,43,1807,…, so the sum begins 1−1/2+1/6−1/42+1/1806. The rapidly increasing denominators explain the fast initial convergence; the alternating sign is essential.')
lesson('CatalanConstantC', r'G=\sum_{n=0}^{\infty}\frac{(-1)^n}{(2n+1)^2}',
       'Only odd denominators appear, with alternating signs. For a truncation of this decreasing alternating series, the first omitted term bounds the error; summing all reciprocal squares would instead give π²/6.',
       [('1.0-1.0/9+1.0/25-CatalanConstantC', 'approximately 0.0129232947', 'After three terms the approximation is above G, and the error is smaller than the next term 1/49.')])
lesson('ChampernowneConstantC10', r'C_{10}=0.123456789101112131415\ldots',
       'Concatenate integer strings, not their fractional values: the digits after 9 are the two digits of 10, followed by 11. The displayed construction explains the constant but cannot be continued by extracting digits from a rounded double.')
lesson('ContinuedFractionsConstant', r'\frac{\pi^2}{6\ln2\,\ln10}=\frac1{L_{\mathrm{Lochs}}}',
       'This is the reciprocal of the typical number of continued-fraction terms determined per decimal digit. Choose the direction of conversion before multiplying; a statement about typical real numbers does not describe every rational or quadratic irrational.')
lesson('DelianConstant', r'd=\sqrt[3]{2},\qquad (da)^3=2a^3',
       'Doubling a cube’s volume multiplies its edge by the cube root of two. Multiplying the edge by two would multiply the volume by eight.',
       [('DelianConstant*DelianConstant*DelianConstant', 'approximately 2', 'The three edge scale factors multiply to the requested volume scale factor.')])
lesson('EmbreeTrefethenConstantβ', r'x_{n+2}=\pm x_{n+1}\pm\beta x_n',
       'The signs are independent and equiprobable in this random recurrence. The constant marks an asymptotic growth/decay threshold under that model; a particular short sequence of signs can behave differently from the typical long-run behavior.')
lesson('ErdösBorweinConstant', r'E=\sum_{n=1}^{\infty}\frac1{2^n-1}',
       'The denominators begin 1,3,7,15,31,…, whether or not they are prime. Exponential growth of the denominators makes the tail converge much faster than the harmonic series.')
lesson('FeigenbaumReductionParameterα', r'\alpha\approx-2.502907875095893',
       'The minus sign records an orientation reversal in the spatial rescaling convention used here. Tables using the positive magnitude describe the same size factor with a different sign convention; δ is a separate scaling of parameter intervals.')
lesson('FeigenbaumBifurcationVelocityδ', r'\delta=\lim_{n\to\infty}\frac{a_n-a_{n-1}}{a_{n+1}-a_n}',
       'The aₙ locate successive period doublings in a family from the stated universality class. The numerator and denominator are parameter gaps, so this is not the ratio of the periods themselves, which double by a factor of two.')
lesson('FransénRobinsonConstant', r'F=\int_0^{\infty}\frac{dx}{\Gamma(x)}',
       'The reciprocal gamma function tends to zero at the left endpoint, despite gamma itself having a pole there. Numerical integration should use that continuous endpoint value and account for the tail rather than sampling gamma(0) and inverting an error.')
lesson('GaussConstantG', r'G=\frac1{\operatorname{AGM}(1,\sqrt2)}',
       'Arithmetic–geometric iteration replaces two positive numbers by their arithmetic and geometric means. Both approach the same limit; taking its reciprocal gives this constant, not Newton’s gravitational constant G.')
lesson('GaussKuzminWirsingConstantλ1', r'\lambda=\left|\lambda_2(\mathcal L_{\mathrm G})\right|\approx0.3036630029',
       'The Gauss continued-fraction transfer operator has a leading eigenvalue one for its stationary density. The magnitude of its next eigenvalue describes a decaying mode toward that density; it is a convergence rate, not a mean partial quotient or a probability of one particular digit.')
lesson('GelfondConstant', r'G=e^\pi',
       'The natural logarithm of this constant is π. It differs from the Gelfond–Schneider constant, whose base is two and whose exponent is √2.',
       [('ln(GelfondConstant)/PI', 'approximately 1', 'Taking a logarithm recovers the exponent and provides a check on which named exponential is stored.')])
lesson('GelfondSchneiderConstant', r'2^{\sqrt2}=\exp(\sqrt2\ln2)',
       'An irrational exponent can be evaluated through a real exponential and logarithm. This is not e raised to π, despite the similar historical name.',
       [('log2(GelfondSchneiderConstant)', 'approximately 1.4142135624', 'The logarithm identifies the irrational exponent used in the definition.')])
lesson('GerverMovingSofaConstant', r'A(w)=A(1)w^2',
       'The reference corridor has width one. A geometrically similar corridor of width w requires lengths to scale by w and areas by w²; the constant describes the specified construction, not a procedure for finding a path.')
lesson('HammersleyUpperBoundOnGerverConstant', r'A_{\mathrm H}=\frac\pi2+\frac2\pi',
       'A feasible sofa construction establishes an achievable area and therefore a lower bound on the optimum. The legacy identifier’s word “Upper” must not be read as a mathematical upper-bound certificate.')
lesson('GibbsConstantG', r'\operatorname{Si}(\pi)=\int_0^\pi\frac{\sin t}{t}\,dt',
       'At zero use the removable limit sin(t)/t→1. This integral is an ingredient in the Gibbs overshoot calculation; to get the usual fractional excess above a jump, divide by π and subtract one half.',
       [('GibbsConstantG/PI-0.5', 'approximately 0.0894898722', 'The overshoot fraction is about 8.949 percent of the jump, not 1.8519 times the jump.')])
lesson('WilbrahamGibbsConstantG', r'W=\frac{2\operatorname{Si}(\pi)}\pi,\qquad\text{overshoot fraction}=\frac{W-1}{2}',
       'The factor of two and the subtraction of one matter when comparing conventions. Increasing a Fourier cutoff narrows the region near a jump, but the limiting relative overshoot is still nonzero.',
       [('100*(WilbrahamGibbsConstantG-1)/2', 'approximately 8.9489872236', 'Convert the stored normalization to the familiar overshoot percentage of a jump.')])
lesson('GlaisherKinkelinConstantA', r'A=\exp\left(\frac1{12}-\zeta^{\prime}(-1)\right)',
       'The prime denotes differentiation with respect to the zeta function’s argument. A value of ζ at −1 is not enough: this definition uses its slope there, and the final exponential makes A positive.')
lesson('GolombDickmanConstantλ', r'\frac{\operatorname E[L_n]}n\longrightarrow\lambda',
       'Lₙ is the length of the longest cycle in a uniformly random permutation of n objects. The expectation is taken over permutations; one particular permutation may have a longest cycle anywhere from one to n.')
lesson('GompertzConstantG', r'G=\int_0^\infty\frac{e^{-t}}{1+t}\,dt=eE_1(1)',
       'The integral is positive and smaller than the integral of exp(−t) alone, which is one. This gives an immediate scale check and distinguishes the exponential integral E₁ from the exponential function itself.')
lesson('GrossmannConstant', r'x_0=1,\quad x_1=c,\quad x_{n+2}=\frac{x_n}{1+x_{n+1}}',
       'The named value selects the special initial condition for this recurrence. An error in that starting value can change a sensitive long iteration; repeating the recurrence does not recover digits absent from the stored approximation.')
lesson('KhinchinLévyConstantγ', r'\lim_{n\to\infty}q_n^{1/n}=\exp\left(\frac{\pi^2}{12\ln2}\right)',
       'The qₙ are denominators of regular continued-fraction convergents. This is the typical multiplicative growth factor; taking its logarithm gives the logarithmic Lévy constant.',
       [('ln(KhinchinLévyConstantγ)', 'approximately 1.1865691104', 'The logarithm converts a multiplicative growth factor into a growth rate.')])
lesson('KnuthRandomgeneratorsConstant', r'K=\frac12\left(1-\frac1{\sqrt3}\right)',
       'The expression is a fixed algebraic number. Using it in a calculation supplies neither random samples nor a seed, and the word “random” in the name is not a security property.')
lesson('KolakoskiConstantγ', r'K=(0.110010110\ldots)_2',
       'Omit the initial 1 of the Kolakoski sequence and map subsequent 1s to binary zeros and 2s to binary ones. Binary place values halve at each step; this encoding is not the frequency of a symbol in the sequence.')
lesson('LandauRamanujanConstant', r'\#\{n\le x:n=a^2+b^2,\ a,b\in\mathbb Z\}\sim K\frac{x}{\sqrt{\ln x}}',
       'The count includes each represented integer once. For example, multiple representations or exchanging a and b do not create additional counted integers; the asymptotic expression is not an exact finite counting formula.')
lesson('LaplaceLimitConstantλ', r'\frac{\lambda e^{\sqrt{1+\lambda^2}}}{1+\sqrt{1+\lambda^2}}=1,\qquad\lambda>0',
       'A convergence threshold belongs to a particular series representation of Kepler’s equation. Above it the equation can still have a solution even though that power series is unsuitable for finding it.')
lesson('LemniscateConstantL', r'L=2\int_0^1\frac{dt}{\sqrt{1-t^4}}=\frac{\Gamma(1/4)^2}{2\sqrt{2\pi}}',
       'The factor of two distinguishes this normalization from FirstLemniscateConstantLA. The endpoint at one is singular but integrable; treating the integrand’s infinite endpoint value as a finite sample is not a quadrature rule.',
       [('LemniscateConstantL/FirstLemniscateConstantLA', 'approximately 2', 'These two names use different normalizations of the same basic integral.')])
lesson('FirstLemniscateConstantLA', r'L_A=\int_0^1\frac{dt}{\sqrt{1-t^4}}=\frac L2',
       'This is the unmultiplied integral. It is about 1.311 rather than 2.622; checking the normalization before copying a table value prevents a factor-of-two error.')
lesson('SecondLemniscateConstantLB', r'L_B=\int_0^1\frac{t^2\,dt}{\sqrt{1-t^4}},\qquad L_A L_B=\frac\pi4',
       'The additional t² changes the weighting of the same interval, so this is not a second copy of the first integral. Their product identity provides an independent consistency check.',
       [('4*FirstLemniscateConstantLA*SecondLemniscateConstantLB/PI', 'approximately 1', 'The product of the two normalizations is π/4.')])
lesson('LévyConstantγ', r'\lim_{n\to\infty}\frac{\ln q_n}{n}=\frac{\pi^2}{12\ln2}',
       'Dividing log denominator by the number of continued-fraction steps gives a typical asymptotic rate. Exponentiating it gives the multiplicative convention used by KhinchinLévyConstantγ.')
lesson('LiouvilleConstant', r'L=\sum_{n=1}^{\infty}10^{-n!}',
       'The nonzero decimal positions begin 1,2,6,24,120,… . The gaps grow extremely quickly; a floating-point value can represent the beginning of the number but not the remote isolated digits used in its mathematical construction.')
lesson('LochConstant', r'L_{\mathrm{Lochs}}=\frac{6\ln2\,\ln10}{\pi^2}',
       'This convention measures typical continued-fraction terms per known decimal digit. ContinuedFractionsConstant uses the reciprocal conversion, so the two should multiply to about one.',
       [('LochConstant*ContinuedFractionsConstant', 'approximately 1', 'Reciprocal conventions describe opposite directions of the same typical conversion.')])
lesson('MadelungConstantM3', r'M=\sum_{(i,j,k)\ne(0,0,0)}\frac{(-1)^{i+j+k}}{\sqrt{i^2+j^2+k^2}}\quad\text{(electrostatic summation)}',
       'The alternating lattice charges produce both positive and negative contributions. The sum is conditionally convergent, so a physical electrostatic or Ewald prescription is part of its definition; arbitrary rearrangements need not preserve it.')
lesson('MeisselMertensConstantB1', r'B_1=\lim_{x\to\infty}\left(\sum_{p\le x,\ p\ \mathrm{prime}}\frac1p-\ln\ln x\right)',
       'Only primes are included, so the growth to subtract is ln(ln x), not ln x. Applying the harmonic-sum Euler–Mascheroni formula to this prime-only sum would remove the wrong main term.')
lesson('MillsConstantA', r'\left\lfloor A^{3^n}\right\rfloor\quad(n=1,2,\ldots)',
       'The powers grow so quickly that tiny errors in A are greatly amplified before taking the floor. The stored decimal is useful for illustrating the definition, but is not a dependable arbitrary-size prime generator.')
lesson('MKBconstant', r'\left|\lim_{N\to\infty}\int_1^{2N}e^{i\pi x}x^{1/x}\,dx\right|',
       'The endpoints follow the even integers. That specified subsequence controls cancellation in the oscillatory integral; replacing it by an arbitrary upper limit changes the convergence question.')
lesson('MRBconstant', r'\lim_{N\to\infty}\sum_{n=1}^{2N}(-1)^n n^{1/n}',
       'Individual terms do not tend to zero in magnitude, so the ordinary alternating-series test does not apply. Pairing adjacent terms through an even cutoff is essential to this definition.')
lesson('OmegaConstantW1', r'\Omega e^\Omega=1,\qquad\Omega=W_0(1)',
       'The exponential and the prefactor both increase for positive inputs, so the positive solution is unique. The fixed-point form Ω=exp(−Ω) is useful for recognizing the same constant in apparently different equations.',
       [('OmegaConstantW1*exp(OmegaConstantW1)', 'approximately 1', 'Substitution satisfies the defining transcendental equation.')])
lesson('OtterConstantα', r'\alpha=\lim_{n\to\infty}t_n^{1/n}',
       'Unlabeled tree counts have an exponential main factor αⁿ together with polynomial and coefficient factors. Taking an n-th root isolates the exponential rate, but does not give an exact count at finite n.')
lesson('OtterAsymptoticConstantβuForUnrootedTrees', r't_n\sim\beta_u\alpha^n n^{-5/2}',
       'No vertex is distinguished as a root. The leading coefficient belongs with the unrooted exponent −5/2; substituting the rooted coefficient or exponent mixes two different counting problems.')
lesson('OtterAsymptoticConstantβrForRootedTrees', r'r_n\sim\beta_r\alpha^n n^{-3/2}',
       'A root is part of the tree structure being counted. Rooting changes the polynomial factor and leading coefficient even though the exponential rate α is shared with unrooted trees.')
lesson('PlasticNumberρ|SilverConstant', r'\rho^3=\rho+1,\qquad\rho>1',
       'A limiting ratio for a recurrence with a term equal to the sum of the terms two and three places earlier obeys ρ³=ρ+1. The legacy SilverConstant stores this cubic root, not the usual silver ratio 1+√2.',
       [('PlasticNumberρ*PlasticNumberρ*PlasticNumberρ-PlasticNumberρ', 'approximately 1', 'The stored plastic number satisfies the cubic relation.')])
lesson('PogsonRatio', r'\frac{F_1}{F_2}=10^{0.4(m_2-m_1)},\qquad P=10^{0.4}',
       'A smaller astronomical magnitude denotes greater flux. One magnitude corresponds to a factor P; five magnitudes compound that factor five times, giving a flux ratio of one hundred.',
       [('pow(PogsonRatio,5)', 'approximately 100', 'Five equal magnitude steps multiply flux ratios rather than adding them.')])
lesson('PolyaRandomwalkConstantp3', r'p_{\mathrm{return}}=\Pr(\exists n\ge1:S_n=0)',
       'Each step of this three-dimensional walk selects one of six nearest neighbors equally. The probability is for any eventual return after departure; it is not the probability of being at the origin after one particular number of steps.')
lesson('PrévostConstant', r'\sum_{n=1}^{\infty}\frac1{F_n},\qquad F_1=F_2=1',
       'Both initial Fibonacci ones contribute, so the series begins 1+1+1/2+1/3+1/5. Splitting the positive indices into even and odd subsets gives the two reciprocal-Fibonacci constants below.')
lesson('ReciprocalEvenFibonacciConstant', r'\sum_{n=1}^{\infty}\frac1{F_{2n}}',
       'Even refers to indices 2,4,6,…, whose Fibonacci values are 1,3,8,… . It does not mean selecting only Fibonacci numbers with even numerical values.')
lesson('ReciprocalOddFibonacciConstant', r'\sum_{n=1}^{\infty}\frac1{F_{2n-1}}',
       'Odd indices 1,3,5,… give values 1,2,5,… . Combining this disjoint index subset with the even-index subset recovers the full reciprocal sum.',
       [('ReciprocalEvenFibonacciConstant+ReciprocalOddFibonacciConstant', 'approximately 3.3598856662', 'The two disjoint index subsets reconstruct Prévost’s reciprocal-Fibonacci sum.')])
lesson('RényiParkingConstantm', r'\frac{\operatorname E[\text{covered length}]}{\text{available length}}\longrightarrow m',
       'Sequential random placement leaves gaps too short for another unit interval. Those gaps explain why the limiting covered fraction is below one even though an optimally arranged one-dimensional packing could fill the line.')
lesson('SalemNumberσ1', r'x^{10}+x^9-x^7-x^6-x^5-x^4-x^3+x+1=0,\qquad x>1',
       'This picks the real root greater than one of Lehmer’s reciprocal polynomial. Being a prominent small example does not establish that no smaller Salem number exists.')
lesson('SierpinskiConstantK', r'K=\pi\left[2\gamma+2\ln2+3\ln\pi-4\ln\Gamma(1/4)\right]',
       'The logarithm applies to gamma at one quarter, not to one quarter itself. This combination belongs to a weighted sum-of-two-squares asymptotic; it is unrelated to the fractal dimension of the Sierpiński triangle.')
lesson('SoldnerConstantμ', r'\operatorname{li}(\mu)=0,\qquad\operatorname{li}(x)=\operatorname{PV}\int_0^x\frac{dt}{\ln t}',
       'The singularity at t=1 requires a principal-value convention. An integral beginning at 2 has a different additive constant and a different zero, so the lower-limit convention is part of the meaning of this constant.')
lesson('SomosQuadraticRecurrenceConstant', r'\sigma=\prod_{n=1}^{\infty}n^{1/2^n}=\exp\left(\sum_{n=1}^{\infty}\frac{\ln n}{2^n}\right)',
       'Taking logarithms turns this weighted infinite product into an additive series. The exponents shrink geometrically; omitting the weights would produce a divergent product of all positive integers.')
lesson('ShallWilsonOrTwinPrimesConstantC2', r'C_2=\prod_{p>2,\ p\ \mathrm{prime}}\frac{p(p-2)}{(p-1)^2}',
       'The prime two is excluded, because its factor would vanish. The Hardy–Littlewood twin-prime conjecture uses coefficient 2C₂; this product differs from Brun’s sum of reciprocals of actual twin primes.')
lesson('TheodorusConstant', r'T=\sqrt3',
       'The space diagonal of a cube is obtained by applying Pythagoras twice: its square is the sum of three squared edges. A face diagonal would instead use √2.',
       [('TheodorusConstant*TheodorusConstant', 'approximately 3', 'A unit cube contributes one squared unit from each of three perpendicular directions.')])
lesson('ViswanathConstant', r'x_0=x_1=1,\quad x_{n+2}=x_{n+1}\pm x_n,\quad |x_n|^{1/n}\longrightarrow v\quad\text{almost surely}',
       'At each step choose the sign independently with equal probabilities. The constant describes typical long-run exponential growth, not monotonic growth of every sample path; individual terms may shrink or vanish before later terms grow.')
lesson('MagicAngle', r'3\cos^2\theta-1=0,\qquad\theta=\arccos\frac1{\sqrt3}',
       'The angle makes the second Legendre polynomial vanish. This is why a contribution proportional to 3cos²θ−1 cancels in the stated orientation; it does not make every angular dependence vanish.',
       [('MagicAngle*180/PI', 'approximately 54.7356103172', 'The stored value is in radians. Multiplying by 180/PI expresses the same angle in degrees.')])
lesson('ComplementaryMagicAngle', r'\theta_{\mathrm c}=\frac\pi2-\theta_{\mathrm{magic}}',
       'These two angles add to a right angle. Complementary means π/2 in radians, so adding the stored values should give π/2 rather than π.',
       [('MagicAngle+ComplementaryMagicAngle', 'approximately 1.5707963268', 'The sum is a right angle in radians, not ninety radians.')])
lesson('TetrahedralAngle', r'\cos\theta=-\frac13,\qquad\theta=\arccos\left(-\frac13\right)',
       'The dot product of two normalized directions from a regular tetrahedron’s center to different vertices is −1/3. A negative cosine places the angle above a right angle.',
       [('cos(TetrahedralAngle)', 'approximately -0.3333333333', 'The stored angle reproduces the normalized dot product between tetrahedral directions.')])
lesson('ComplementaryTetrahedralAngle', r'\theta_{\mathrm s}=\pi-\theta_{\mathrm{tetra}},\qquad\cos\theta_{\mathrm s}=\frac13',
       'Despite the identifier’s word “Complementary”, this is the supplementary angle. The two tetrahedral angles add to a straight angle, and their cosines have opposite signs.',
       [('(TetrahedralAngle+ComplementaryTetrahedralAngle)*180/PI', 'approximately 180', 'This confirms the supplement convention encoded by the legacy name.')])
lesson('GravitoidConstant', r'k=\left(\frac{64}{27}\right)^{1/4}',
       'This dimensionless number is a width-to-depth ratio in the specified homogeneous gravitating shape problem. It sets geometry; Newton’s gravitational constant supplies a separate dimensional scale.')
lesson('MinimumAreaOfAConstantWidthFigure', r'A(w)=\frac{\pi-\sqrt3}{2}\,w^2',
       'The minimum is attained by a Reuleaux triangle among convex plane figures of the specified constant width. Rescaling width multiplies area by the square of the scale factor; width is not a circle radius.',
       [('4*MinimumAreaOfAConstantWidthFigure', 'approximately 2.819083692', 'A constant-width figure of width two has four times the minimum unit-width area.')])
lesson('MoserWormConstant', r'A(L)=A(1)L^2',
       'The stored candidate area refers to curves of length one. Scaling every curve and its cover by L multiplies area by L²; the legacy estimate is not an exact optimality certificate or a statement about a current best bound.')
lesson('SquareDrillConstant', r'f=2\sqrt3+\frac\pi6-3',
       'The idealized Reuleaux construction reaches most, but not all, of the enclosing square. Subtracting its area fraction from one isolates the uncovered corners.',
       [('100*(1-SquareDrillConstant)', 'approximately 1.2299609264', 'About 1.23 percent of the square remains uncovered in this idealized geometric construction.')])
lesson('UniversalParabolicConstant', r'P=\sqrt2+\ln(1+\sqrt2)',
       'The arc bounded by a parabola’s latus rectum grows in direct proportion to the semilatus rectum. Their ratio is independent of scale; the logarithmic term comes from integrating the arc-length element.')
lesson('PowerTowerOfI', r'z=i^z=\exp\left(\frac{i\pi z}{2}\right)',
       'This fixed-point equation uses the principal logarithm Log(i)=iπ/2. Choosing a different logarithm branch changes the iteration; the identifier returns one stored attracting fixed point, not an unlimited tower evaluation.')
lesson('i', r'i^2=-1,\qquad \operatorname{Re}i=0,\quad\operatorname{Im}i=1',
       'Real and imaginary parts are separate coordinates of one complex number. Multiplying by i rotates a complex value by a quarter turn; doing so twice reverses its direction.',
       [('Re(i*i)', '-1', 'Two quarter turns send the real unit to minus one. Re extracts the real scalar coordinate of the complex product.')])
lesson('NaN', r'\operatorname{NaN}\ne\operatorname{NaN}\quad\text{(floating-point equality)}',
       'NaN records an invalid or unrepresentable result rather than a real number. Its exceptional equality rule is why a NaN predicate is needed; comparing a value with the NaN constant is not a reliable validity check.')
lesson('Infinity', r'(+\infty)-(+\infty)\longmapsto\operatorname{NaN}\quad\text{(floating-point arithmetic)}',
       'Infinity is a floating-point special value, not a finite number with unusually many digits. Subtracting two infinite results loses the information needed for cancellation and produces NaN; a limit of a difference must be analyzed before those terms overflow.')


# Physical equations retain the original unit statements and metrology warnings.
# Their examples use the application's historical values, not silently updated SI.
lesson('SpeedOfLightInVacuum|c', r'd=ct,\qquad c=299\,792\,458\ \mathrm{m\,s^{-1}}',
       'Use seconds for travel time to obtain meters. The vacuum propagation speed is different from a material’s phase or group velocity; an optical medium needs its own propagation model.',
       [('SpeedOfLightInVacuum*1e-9', '0.299792458', 'Light travels about 30 centimeters in one nanosecond in vacuum.')])
lesson('ElementaryCharge', r'q_{\mathrm{electron}}=-e,\qquad e_{\mathrm{SI}}=1.602176634\times10^{-19}\ \mathrm C',
       'The stored constant is a positive charge magnitude. An electron’s charge is negative; dropping that sign reverses the predicted electric force. The current exact SI definition and the retained historical decimal are explicitly different.',
       [('-ElementaryCharge', '-1.602176565E-19', 'An electron carries negative charge. This uses the application’s historical magnitude, not the newer exact SI number above.')])
lesson('PlanckConstant|h', r'E=h\nu,\qquad\nu=\frac{\omega}{2\pi}',
       'Ordinary frequency ν counts cycles per second. Angular frequency ω counts radians per second, so using ω with h without division by 2π overestimates the photon energy by that factor.',
       [('PlanckConstant*5e14', 'approximately 3.313034785E-19', 'For frequency 5×10¹⁴ Hz, multiply by the stored Planck constant to obtain joules. This retains the historical application value.')])
lesson('PlanckConstantOver2Pi', r'\hbar=\frac h{2\pi},\qquad E=\hbar\omega',
       'Reduced Planck constant accompanies angular frequency and angular momentum. The factor of 2π converts cycles to radians; h and ℏ are related conventions, not interchangeable numeric values.',
       [('PlanckConstantOver2Pi*1e15', '1.054571726E-19', 'At angular frequency 10¹⁵ radians per second, ℏω gives this energy in joules from the stored value.')])
lesson('BoltzmannConstant', r'E_{\mathrm{scale}}=k_{\mathrm B}T',
       'Temperature is absolute temperature in kelvin. kBT is an energy scale; the average kinetic energy of a classical monatomic particle is instead 3kBT/2, and a particular particle need not have either exact value.',
       [('BoltzmannConstant*300', 'approximately 4.1419464E-21', 'At 300 kelvin this is the characteristic thermal energy in joules, not the energy of every particle.')])
lesson('BoltzmannConstantInEvperK', r'E_{\mathrm{scale}}[\mathrm{eV}]=k_{\mathrm B}[\mathrm{eV/K}]\,T[\mathrm K]',
       'This representation already returns electronvolts when multiplied by kelvin. Dividing by elementary charge again would perform the joule-to-electronvolt conversion twice.')
lesson('AvogadroConstant', r'N=nN_{\mathrm A}',
       'The amount n is in moles, while N counts the specified entities. State whether those entities are atoms, molecules, ions or formula units before converting; one mole of molecules need not contain one mole of atoms.',
       [('2*AvogadroConstant', '1.204428258E24', 'Two moles contain this many specified entities using the historical stored Avogadro value.')])
lesson('G|NewtonianConstantOfGravitation', r'F=G\frac{m_1m_2}{r^2}',
       'This gives the magnitude of the Newtonian attraction for point masses or spherically symmetric bodies outside their surfaces. Use center-to-center separation r; the force is attractive and acts along the line of centers. The short G and longer identifier retain slightly different historical decimals.',
       [('NewtonianConstantOfGravitation*2*3/(0.5*0.5)', '1.6017216E-9', 'Masses of 2 kg and 3 kg separated by 0.5 m attract with this force in newtons under the point-mass model.')])
lesson('StandardAccelerationOfGravity', r'W=mg_n,\qquad g_n=9.80665\ \mathrm{m\,s^{-2}}',
       'Standard gravity is a defined reference acceleration used in unit conventions. Weight mg is a force in newtons; mass in kilograms does not become a different mass when local gravity changes.',
       [('2*StandardAccelerationOfGravity', '19.6133', 'A 2 kg mass has this reference weight in newtons. Actual local gravity can differ.')])
lesson('g', r'v(t)=v_0+gt\quad\text{for constant acceleration}',
       'The short identifier stores 9.80625, which differs from the standard reference acceleration 9.80665. Specify the acceleration and sign direction for the model rather than silently treating every g spelling as the same constant.',
       [('2*g', '19.6125', 'Starting from rest, two seconds of constant acceleration at this stored g changes velocity by 19.6125 m/s in the chosen positive direction.')])
lesson('ElectronMass', r'E_0=m_{\mathrm e}c^2',
       'Rest mass sets the electron’s rest-energy scale. It is not its kinetic energy; at low speeds the kinetic energy is approximately mv²/2, whereas relativistic calculations need the Lorentz factor.')
lesson('BohrRadius', r'a_0=\frac{\hbar}{m_{\mathrm e}c\alpha}',
       'For the ideal nonrelativistic hydrogenic Coulomb problem, the characteristic orbital length scales roughly as a₀/Z. An orbital is a probability distribution, not a tiny planet constrained to a circular track.',
       [('BohrRadius/2', '2.6458860546E-11', 'The a₀/Z length scale for nuclear charge Z=2 is half a₀, neglecting reduced-mass and relativistic corrections.')])
lesson('FineStructureConstant', r'\alpha=\frac{e^2}{4\pi\varepsilon_0\hbar c}',
       'This dimensionless combination describes electromagnetic coupling in the stated low-energy convention. It is not a charge in coulombs; powers of α often organize perturbative corrections, whose applicability still depends on the physical problem.')
lesson('AtomicMassConstant|UnifiedAtomicMassUnit', r'm_{\mathrm u}=\frac{m({}^{12}\mathrm C)}{12}',
       'The reference is a free carbon-12 atom in the specified state, including its electrons. A nucleus has a different mass; simply adding free-particle masses also omits binding-energy effects.')
lesson('ElectronVolt', r'\Delta U=q\,\Delta V,\qquad1\ \mathrm{eV}=e\,(1\ \mathrm V)',
       'An electronvolt is a positive energy unit, not a voltage. Electric potential energy changes by qΔV; the corresponding kinetic-energy gain is −qΔV when no other work is done. An electron moving to a potential one volt higher gains one eV. Multiply an energy in eV by the stored joule-per-eV factor to obtain joules.',
       [('2*ElectronVolt', '3.20435313E-19', 'Two electronvolts converted to joules with the application’s stored historical factor.')])
lesson('MolarGasConstant', r'pV=nRT,\qquad R=N_{\mathrm A}k_{\mathrm B}',
       'Use pressure in pascals, volume in cubic meters, amount in moles and absolute temperature in kelvin. The ideal-gas relation is a model; strong interactions and condensation can make it inaccurate.',
       [('MolarGasConstant*300/0.024', '103930.77625', 'One mole at 300 K in 0.024 m³ has this ideal-gas pressure in pascals. Liters would need conversion to cubic meters first.')])
lesson('FaradayConstant', r'Q=nF,\qquad F=N_{\mathrm A}e',
       'Here n counts moles of elementary charges. Moving one mole of doubly charged ions transfers two moles of elementary charge, so chemical stoichiometry determines the multiplier.',
       [('0.5*FaradayConstant', '48242.66825', 'Half a mole of elementary positive charges carries this charge in coulombs using the stored factor.')])
lesson('StefanBoltzmannConstant', r'M=\sigma T^4,\qquad q_{\mathrm{net}}=\varepsilon\sigma(T^4-T_{\mathrm{env}}^4)',
       'M is the radiant power per unit surface area of an ideal blackbody. Net exchange with a large isothermal surrounding surface includes the surrounding temperature; the gray-body expression also assumes a suitable emissivity and radiative geometry.',
       [('StefanBoltzmannConstant*300*300*300*300', '459.300213', 'A 300 K blackbody emits about 459.3 W/m² in total. This is emitted power, not net heat loss to a 300 K environment.')])
lesson('WienWavelengthDisplacementLawConstant', r'\lambda_{\max}T=b_\lambda',
       'This locates the peak of spectral radiance per unit wavelength. Changing to radiance per unit frequency changes the density and therefore the peak; c/λmax is not generally the frequency-domain peak.',
       [('WienWavelengthDisplacementLawConstant/3000', 'approximately 9.6592403333E-7', 'At 3000 K the wavelength-domain peak is about 966 nm using the stored displacement constant.')])
lesson('WienFrequencyDisplacementLawConstant', r'\nu_{\max}=b_\nu T',
       'The peak is defined for a spectrum measured per unit frequency. The frequency and wavelength spectra contain a Jacobian factor when converted, which explains why their maxima are not related by simply substituting ν=c/λ.')
lesson('ConductanceQuantum', r'G_0=\frac{2e^2}{h},\qquad I=G_0V',
       'This is the conductance scale for a perfectly transmitting spin-degenerate channel in the ideal model. The factor of two counts spin degeneracy; imperfect transmission or a different degeneracy changes the measured conductance.',
       [('ConductanceQuantum*0.01', '7.7480917346E-7', 'At 10 mV an ideal one-channel conductance G₀ carries about 0.775 microampere. The number is in amperes.')])
lesson('StandardAtmosphere|StandardStatePressure', r'1\ \mathrm{atm}=101325\ \mathrm{Pa},\qquad1\ \mathrm{bar}=100000\ \mathrm{Pa}',
       'One atmosphere and one bar are distinct defined pressure references. A gas volume quoted at a standard condition must specify both pressure and temperature; the word “standard” alone does not choose between these values.',
       [('StandardAtmosphere/StandardStatePressure', '1.01325', 'One atmosphere is 1.01325 bar, so these two reference pressures are not interchangeable.')])
