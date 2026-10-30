"""Curated special-function reference; no numerical source is changed."""
from common import A, P, M, DLMF, GSL, MATHNET, SOURCE
from dataclasses import replace
from categories import subject_category
from special_families import polynomial, hypergeometric, elliptic, probability, bessel
ALIASES={'Γ':'gamma','logΓ':'logGamma','ψ':'psi','digamma':'psi','ψn':'polyGamma','ψⁿ':'polyGamma',
         'β':'Beta','logβ':'logBeta','diLogarithm':'diLog','Riemannζ':'RiemannZeta','η':'DirichletEta',
         'ζ':'HurwitzZeta','AiryAi':'Ai','AiryBi':'Bi','Rnl':'HydrogenicR','RiccatiBesselψ':'RiccatiBesselS',
         'RiccatiBesselχ':'RiccatiBesselC'}
TESTS={
 'Ackley':('Ackley optimization landscape','A multimodal benchmark with a narrow basin at the origin and many shallow surrounding minima.',
           '−20·exp(−0.2·sqrt(Σxⱼ²/d)) − exp(Σcos(2PI·xⱼ)/d) + 20 + e',
           'Supply d≥1 real coordinates. The mathematical global minimum is 0 at the all-zero vector.'),
 'Rastrigin':('Rastrigin optimization landscape','A quadratic bowl modulated by a cosine in every coordinate; a test of escaping local minima.',
              '10d + Σ[xⱼ² − 10cos(2PI·xⱼ)]', 'Supply d≥1 real coordinates; the global minimum is 0 at the origin.'),
 'Bohachevsky1':('First Bohachevsky benchmark','A two-dimensional bowl with periodic local structure.',
                 'x² + 2y² − 0.3cos(3PI·x) − 0.4cos(4PI·y) + 0.7', 'Real x,y. The displayed standard form has minimum 0 at (0,0), but the pinned implementation omits +0.7 and has value -0.7 at that same minimizer. Add 0.7 explicitly when comparing with the standard benchmark.'),
 'dropWave':('Drop-wave benchmark','A radially symmetric oscillatory optimization surface.',
             '−[1+cos(12sqrt(x²+y²))]/[2+(x²+y²)/2]', 'Real x,y; global minimum −1 at (0,0).'),
 'Himmelblau':('Himmelblau benchmark','A two-dimensional polynomial with four equal global minima.',
               '(x²+y−11)² + (x+y²−7)²', 'Real x,y; (3,2) is one global minimizer, with value 0.'),
 'Matyas':('Matyas benchmark','A shallow quadratic valley used to test numerical minimization.',
           '0.26(x²+y²)−0.48xy', 'Real x,y; minimum 0 at (0,0).'),
 'sixHumpCamel':('Six-hump camel benchmark','A two-dimensional polynomial with multiple local minima and two symmetric global minima.',
                 '(4−2.1x²+x⁴/3)x²+xy+(−4+4y²)y²', 'Real x,y; global minima are about −1.031628 at (±0.089842,∓0.712656).'),
 'Rosenbrock':('Rosenbrock valley','A benchmark whose curved narrow valley makes numerical minimization difficult.',
                'Σⱼ₌₁ᵈ⁻¹ [100(xⱼ₊₁−xⱼ²)²+(1−xⱼ)²]', 'Two or more real coordinates. The two-argument overload uses d=2. The global minimum is 0 when all coordinates are 1.')
}
INTEGRALS={
 'Si':('Sine integral','Accumulated sinc oscillation from the origin.', 'Si(x)=∫₀ˣ sin(t)/t dt', 'Real x; the removable value of the integrand at zero is 1. Si(0)=0; Si is odd.'),
 'Ci':('Cosine integral',P('A logarithmically singular antiderivative of ', M('\\cos(x)/x', 'cos(x)/x'), ', with the conventional tail normalization.'),
        'Ci(x)=−∫ₓ^∞ cos(t)/t dt = γ+ln(x)+∫₀ˣ [cos(t)−1]/t dt', 'Use real x>0. This wrapper rejects x<0; the limiting value as x→0+ is −∞.'),
 'Ei':('Exponential integral','The real principal-value exponential integral, useful in growth, decay and transport problems.',
        'Ei(x)=PV ∫₋∞ˣ exp(t)/t dt', 'Real x≠0. PV specifies the Cauchy principal value across t=0; Ei has a logarithmic singularity there.'),
 'En':('Generalized exponential integral','A decaying integral indexed by a nonnegative integer order.',
        'Eₙ(x)=∫₁^∞ exp(−xt)/tⁿ dt', 'Use integer n≥0 and x>0. At x=0 the integral is finite only for n>1, giving 1/(n−1).'),
 'Shi':('Hyperbolic sine integral','The hyperbolic analogue of the sine integral.',
         'Shi(x)=∫₀ˣ sinh(t)/t dt', 'Real x; the integrand has removable value 1 at zero. Shi is odd and grows rapidly for large |x|.'),
 'Chi':('Hyperbolic cosine integral','A logarithmically singular integral associated with cosh.',
         'Chi(x)=γ+ln|x|+∫₀ˣ [cosh(t)−1]/t dt', 'Real x≠0; this wrapper explicitly returns NaN at zero.'),
 'Ti':('Inverse tangent integral',P('Integrates arctangent divided by its argument; it is sometimes written ', M('\\operatorname{Ti}_2', 'Ti₂'), '.'),
        'Ti₂(x)=∫₀ˣ atan(t)/t dt', 'Real x; the integrand tends to 1 at zero. This is an integral of arctangent, not the inverse of an integral.'),
 'Tai':('Arctangent integral',P('The GSL-backed inverse tangent integral, using the same mathematical ', M('\\operatorname{Ti}_2', 'Ti₂'), ' convention as Ti.'),
         'Tai(x)=∫₀ˣ atan(t)/t dt', 'Real x; a different numerical implementation from Ti, not a different normalization.'),
 'Clausen':('Clausen integral of order two',P('The imaginary part of the dilogarithm on the unit circle; an odd, ', M('2\\pi', '2PI'), '-periodic function.'),
            'Cl₂(x)=Σₙ₌₁^∞ sin(nx)/n² = −∫₀ˣ ln|2sin(t/2)| dt', 'Real angle x in radians; logarithmic integrand singularities are integrable.'),
 'Dawson':('Dawson integral',P('An exponentially scaled integral of ', M('\\exp(t^2)', 'exp(t²)'), ', often encountered in Gaussian line-shape calculations.'),
           'D(x)=exp(−x²)∫₀ˣ exp(t²) dt', 'Real x. It is odd, D(0)=0, and D(x) is asymptotic to 1/(2x) for large |x|.'),
 'Ein':('Entire complementary exponential integral',P('An entire complex function with the logarithmic singularity removed from ', M('E_1', 'E₁'), '.'),
        'Ein(z)=∫₀ᶻ [1−exp(−t)]/t dt = Σₙ₌₁^∞ (−1)ⁿ⁺¹zⁿ/(n·n!)', 'Complex z; Ein(0)=0 and the integrand has a removable singularity. This is not Ei(z).'),
 'FresnelC':('Fresnel cosine integral','The real component of the normalized Fresnel integral, used in diffraction.',
             'C(x)=∫₀ˣ cos(PI·t²/2) dt', 'Real x. The PI/2 factor is part of this convention; C tends to 1/2 as x→+∞.'),
 'FresnelS':('Fresnel sine integral','The imaginary component of the normalized Fresnel integral, used in diffraction.',
             'S(x)=∫₀ˣ sin(PI·t²/2) dt', 'Real x. The PI/2 factor is part of this convention; S tends to 1/2 as x→+∞.'),
 'Fresnel':('Complex Fresnel integral','Combines the Fresnel cosine and sine integrals into one complex result.',
            'F(x)=C(x)+iS(x)=∫₀ˣ exp(i·PI·t²/2) dt', 'The argument x is real; the result is complex. Both component integrals are zero at the origin.')
}

def describe(item, values):
    raw = item['signature'].split('(')[0]
    category = subject_category('SpecialFunctions', ALIASES.get(raw, raw))
    return replace(_describe(item, values), category=category)


def _describe(item, values):
    sig=item['signature']; raw=sig.split('(')[0]; name=ALIASES.get(raw,raw)
    args=sig[sig.find('(')+1:-1]; complex_arg='complex z' in item['menu']
    alias=('Alias of '+name+' with the same convention. ') if raw!=name else ''
    if name in TESTS:
        title,definition,formula,domain=TESTS[name]
        return A(title,definition,formula,domain,'This evaluates a benchmark surface; it does not run an optimizer.',url=('https://github.com/mathnet/mathnet-numerics/blob/v5.0.0/src/Numerics/SpecialFunctions/TestFunctions.cs' if name=='Bohachevsky1' else MATHNET+'MathNet.Numerics/TestFunctions.htm'),category='Optimization benchmarks')
    if name=='findRoot':
        return A('Bracketed scalar root search',P('Attempts to find a zero of the supplied real function within ', M('[a,b]', '[a,b]'), '.'),
                 'Find x such that f(x)=0.', 'Supply a callable f and finite endpoints with a<b that bracket a continuous root.',
                 'This convenience wrapper fixes accuracy at 0.01 and maximum iterations at 10,000, catches solver exceptions and returns NaN on failure. Use FunctionRoot methods when you need explicit controls.',
                 'findRoot(x => x*x-2,0,2) ≈ 1.414 (within the requested accuracy)',MATHNET+'MathNet.Numerics/FindRoots.htm')
    if name=='Gabor':
        if args.startswith('z,'):
            return A('Two-dimensional complex Gabor kernel','A rotated anisotropic Gaussian envelope multiplied by a complex sinusoid.',
                     'exp[−(x′²+γ²y′²)/(2σ²)]·exp[i(2PI·x′/λ+ψ)]',
                     'x′=xcosθ+ysinθ, y′=−xsinθ+ycosθ; θ,ψ are radians, λ is wavelength, σ>0 the envelope width and γ the aspect ratio.',
                     'This wrapper takes x=Re(z), y=Im(z) but casts both to integers before calling Accord. Noninteger coordinates are truncated, not sampled continuously.',url='https://accord-framework.net/docs/html/T_Accord_Math_Gabor.htm')
        return A('One-dimensional Gabor function','A localized sinusoidal signal with a Gaussian envelope and an additive mean.',
                 'Gaussian-windowed cosine with the supplied amplitude, position, width, phase and frequency.',
                 'x is the evaluation coordinate; mean is the offset, position the envelope center, width its positive scale, amplitude its height. Frequency and phase follow Accord’s Function1D convention.',
                 'This returns a real scalar. It is distinct from the wavelength/rotation-based complex two-dimensional overload.',url='https://accord-framework.net/docs/html/M_Accord_Math_Gabor_Function1D.htm')
    if name in ('gamma','logGamma','psi','polyGamma','gammaP','gammaQ','leftRegularizedGamma','rightRegularizedGamma'):
        if name=='gamma' and ',' in args:
            return A('Upper incomplete gamma',P('The tail of the gamma integral, without division by ', M('\\Gamma(a)', 'Γ(a)'), '.'),
                     'Γ(a,x)=∫ₓ^∞ t^(a−1)exp(−t) dt', 'Real a>0 and x≥0; other inputs are rejected by this wrapper.',
                     alias+'Do not confuse this with the lower incomplete gamma or the normalized tails gammaP/gammaQ.', 'gamma(1,2) ≈ exp(-2)',DLMF+'8.2')
        if name in ('gammaP','gammaQ','leftRegularizedGamma','rightRegularizedGamma'):
            lower=name in ('gammaP','leftRegularizedGamma')
            return A(('Lower' if lower else 'Upper')+' regularized gamma',
                     ('The accumulated lower' if lower else 'The remaining upper')+' gamma-integral fraction, also a gamma-distribution tail.',
                     ('P(a,x)=∫₀ˣ t^(a−1)e^(−t)dt/Γ(a)' if lower else 'Q(a,x)=∫ₓ^∞ t^(a−1)e^(−t)dt/Γ(a)'),
                     'Use a>0 and x≥0; results then lie in [0,1]. P(a,x)+Q(a,x)=1.',
                     ('gammaQ delegates to Math.NET and also admits its a=0 limiting convention; the other wrappers require a>0. ' if name=='gammaQ' else '')+
                     'The second argument is the integration limit, not a scale parameter.',
                     ('P(1,0)=0' if lower else 'Q(1,0)=1')+' in this convention.',DLMF+'8.2')
        if name=='gamma':
            return A('Gamma function'+(' — complex argument' if complex_arg else ''),'The meromorphic extension of the factorial relation to noninteger arguments.',
                     'Γ(z)=∫₀^∞ t^(z−1)e^(−t) dt for Re(z)>0; Γ(z+1)=zΓ(z).',
                     'Simple poles at 0,−1,−2,…; finite negative nonintegers are defined by continuation. Large magnitudes may overflow.',
                     alias+'Γ(n)=(n−1)!, not n!. The complex gamma function is single-valued away from its poles.', 'gamma(5) = 24',DLMF+'5.2')
        if name=='logGamma':
            return A('Logarithm of gamma'+(' — complex argument' if complex_arg else ''),P('Evaluates ', M('\\log\\Gamma', 'log Γ'), ' without first forming a possibly overflowing gamma value.'),
                     'exp(LogΓ(z))=Γ(z) away from poles, with the library’s continuous branch convention.',
                     ('This wrapper rejects Re(z)<0 even though complex log-gamma has a larger mathematical continuation. Poles and branch conventions remain relevant.' if complex_arg else 'Real x>0 only in this wrapper; x≤0 returns NaN.'),
                     alias+'A complex log-gamma value need not equal the principal logarithm of a separately rounded complex gamma value.', 'logGamma(1) = 0',DLMF+'5.2')
        if name=='psi' or (name=='polyGamma' and args=='x'):
            return A('Digamma — logarithmic derivative of gamma',P('The derivative of ', M('\\log\\Gamma', 'log Γ'), ', widely used in likelihood equations and harmonic-number identities.'),
                     'ψ(z)=Γ′(z)/Γ(z); ψ(n)=Hₙ₋₁−γ for positive integers n.',
                     'Poles at 0,−1,−2,…; use non-pole real or complex arguments.',
                     alias+('The one-argument polyGamma/ψn/ψⁿ names mean order zero, i.e. digamma, not trigamma.' if name=='polyGamma' else ''), 'psi(1) ≈ -0.5772156649',DLMF+'5.2')
        return A('Polygamma of integer order',P('The ', M('n', 'n'), '-th derivative of the digamma function.'),
                 'ψ⁽ⁿ⁾(x)=dⁿ⁺¹ lnΓ(x)/dxⁿ⁺¹; ψ⁽⁰⁾=ψ.',
                 'Integer n≥0; avoid gamma poles in x. n=1 is trigamma, n=2 tetragamma.',alias,
                 'polyGamma(1,1) ≈ PI*PI/6',DLMF+'5.15')
    if name in ('Beta','BetaNorm','logBeta','Pochhammer','PochhammerRelative'):
        if name=='Beta' and args=='a,b':
            return A('Beta function','The beta integral; the normalization connecting two gamma factors.',
                     'B(a,b)=∫₀¹ t^(a−1)(1−t)^(b−1)dt = Γ(a)Γ(b)/Γ(a+b)',
                     'Real a>0 and b>0 in this wrapper; symmetric in a and b.',alias,'Beta(1,1) = 1',DLMF+'5.12')
        if name=='Beta':
            return A('Lower incomplete beta',P('The lower beta integral accumulated up to ', M('x', 'x'), ', without division by the complete beta function.'),
                     'Bₓ(a,b)=∫₀ˣ t^(a−1)(1−t)^(b−1)dt.',
                     'Real 0≤x≤1 and a,b>0; out-of-domain values return NaN. The first argument is the integration limit and the other two are shape parameters.',
                     alias+'The public order is x,a,b; Meta.Numerics receives Beta(a,b,x). This is an unnormalized area: Beta(0,a,b)=0 and Beta(1,a,b)=Beta(a,b). Use BetaNorm for the corresponding cumulative probability.',
                     'Beta(0.5,1,1) = 0.5',DLMF+'8.17')
        if name=='BetaNorm':
            return A('Regularized incomplete beta',
                     P('The lower beta integral divided by ', M('B(a,b)', 'B(a,b)'), ', giving a cumulative probability for positive shape parameters.'),
                     'Iₓ(a,b)=Bₓ(a,b)/B(a,b).',
                     'Real 0≤x≤1 and a,b>0; out-of-domain values return NaN. The value is in [0,1], with endpoints zero and one.',
                     'The public order is x,a,b; GSL receives gsl_sf_beta_inc(a,b,x). Unlike Beta(x,a,b), this function includes normalization by the complete beta integral.',
                     'BetaNorm(0.5,1,1) = 0.5',GSL+'#incomplete-beta-function')
        if name=='logBeta':
            return A('Natural logarithm of beta','A numerically useful logarithmic form of the positive beta function.',
                     'ln B(a,b)=lnΓ(a)+lnΓ(b)−lnΓ(a+b)', 'Real a>0,b>0 in this wrapper.',alias,'logBeta(1,1) = 0',DLMF+'5.12')
        relative=name=='PochhammerRelative'
        return A('Relative Pochhammer increment' if relative else 'Pochhammer rising factorial',
                 'A cancellation-aware relative increment of the gamma ratio.' if relative else 'A gamma ratio extending a product of consecutively increasing factors.',
                 '[(a)ₓ−1]/x, with limiting value ψ(a) at x=0.' if relative else '(a)ₓ=Γ(a+x)/Γ(a); (a)ₙ=a(a+1)…(a+n−1).',
                 'The mathematical gamma ratio requires appropriate continuation at poles. Use positive a and positive x for a straightforward real value.',
                 'This wrapper rejects nonpositive integer x (including zero) and nonpositive integer a+x. Thus it blocks some mathematical limits, including the relative x=0 limit; it is not a falling factorial.',
                 '' if relative else 'Pochhammer(3,2) = 12',DLMF+'5.2')
    if name in ('diLog','SpencesIntegral','PolyLog','RiemannZeta','DirichletEta','HurwitzZeta'):
        if name in ('diLog','SpencesIntegral','PolyLog'):
            spence=name=='SpencesIntegral'; general=name=='PolyLog'
            return A('Spence integral' if spence else ('Polylogarithm of integer order' if general else 'Dilogarithm'),
                     'The dilogarithm evaluated at one minus the argument.' if spence else 'A weighted power series continued beyond its disk of convergence.',
                     'Spence(t)=Li₂(1−t).' if spence else ('Liₙ(x)=Σₖ₌₁^∞ xᵏ/kⁿ for |x|<1.' if general else 'Li₂(z)=Σₖ₌₁^∞ zᵏ/k² for |z|<1.'),
                     ('Complex argument uses the library continuation; the Li₂ cut is [1,+∞), hence Spence has a cut along the nonpositive real axis.' if spence and complex_arg else
                      'Complex argument; Li₂ has its conventional branch cut from 1 to +∞.' if complex_arg else
                      'Real x≥0 in this Spence wrapper.' if spence else
                      'Integer n≥0 and real x≤1 in this wrapper; endpoints can diverge for low n.' if general else 'Real x≤1 in this wrapper; use the complex overload beyond that interval.'),
                     alias+('This convention is Li₂(1−x), not Li₂(x).' if spence else ''),
                     'SpencesIntegral(1) = 0' if spence else 'PolyLog(2,1) ≈ PI*PI/6' if general else 'diLog(1) ≈ PI*PI/6',DLMF+'25.12')
        if name=='RiemannZeta':
            return A('Riemann zeta function','Analytic continuation of the sum of reciprocal powers of positive integers.',
                     'ζ(s)=Σₙ₌₁^∞ n^(−s) for Re(s)>1.', 'A simple pole occurs at s=1; trivial zeros occur at negative even integers. Complex input uses the meromorphic continuation.',alias,'RiemannZeta(2) ≈ PI*PI/6',DLMF+'25.2')
        if name=='DirichletEta':
            return A('Dirichlet eta function',P('The alternating zeta series, with its removable value supplied at ', M('s=1', 's=1'), '.'),
                     'η(s)=Σₙ₌₁^∞ (−1)ⁿ⁻¹/nˢ = (1−2^(1−s))ζ(s)', 'Real s; the series converges for s>0, with continuation outside that range.',alias,'DirichletEta(1) ≈ ln(2)',DLMF+'25.2')
        return A('Hurwitz zeta function','Shifts the reciprocal-power series away from the positive integers.',
                 'ζ(s,q)=Σₙ₌₀^∞ (n+q)^(−s)', 'The argument named x is the exponent s. This real GSL interface requires s>1 and q>0; it is not the full analytic continuation.',alias,'HurwitzZeta(2,1) ≈ PI*PI/6',DLMF+'25.11')
    if name in INTEGRALS:
        title,definition,formula,domain=INTEGRALS[name]
        note='The wrapper uses real continuations for a few negative-x orders; the displayed integral definition is the positive-x branch, not a convergence claim for negative x.' if name=='En' else ''
        return A(title,definition,formula,domain,note,url=DLMF+('7.2' if name in ('Dawson','Fresnel','FresnelC','FresnelS') else '6.2'))
    if name=='Debye':
        return A('Normalized Debye integral','A thermal integral used in lattice heat-capacity and related statistical-physics models.',
                 'Dₙ(x)=n/xⁿ ∫₀ˣ tⁿ/(exp(t)−1) dt; Dₙ(0)=1 for n>0.',
                 'Use integer n=1,…,6 and x≥0 for the supported normalized functions.',
                 'Orders outside 1–6, including n=0, return NaN. Higher orders are not implemented; the unnormalized infinite-limit integral Γ(n+1)ζ(n+1) is not a substitute for Dₙ(x).',
                 'Debye(1,0) = 1',GSL+'#debye-functions')
    if name=='TransportJ':
        return A('Transport integral','A Bose-weighted integral used in transport and heat-capacity models.',
                 'Jₙ(x)=∫₀ˣ tⁿ exp(t)/(exp(t)−1)² dt', 'Supported integer orders n=2,3,4,5 and real x≥0; other orders return NaN.',
                 'This is the unnormalized integral, unlike the separately normalized Debye family.','TransportJ(2,0) = 0',GSL+'#transport-functions')
    if name in ('SynchrotronF','SynchrotronG'):
        return A('First synchrotron function' if name.endswith('F') else 'Second synchrotron function',
                 'A modified-Bessel spectral factor used to describe synchrotron radiation.',
                 'F(x)=x∫ₓ^∞ K₅⁄₃(t) dt' if name.endswith('F') else 'G(x)=xK₂⁄₃(x)',
                 'Real x≥0; in radiation applications x is a dimensionless frequency ratio. Negative inputs return NaN.',url=GSL+'#synchrotron-functions')
    if name.startswith('FermiDirac'):
        if args=='x,b':
            return A('Incomplete Fermi–Dirac integral of order zero','The Fermi occupation integral above a lower cutoff.',
                     'F₀(x,b)=∫ᵦ^∞ dt/[exp(t−x)+1] = ln(1+exp(x−b))',
                     'x is a real dimensionless chemical-potential parameter; b≥0 is the lower integration limit.',
                     'This is a tail integral, not the integral from zero to b.','FermiDiracF0(0,0) ≈ ln(2)',GSL+'#fermi-dirac-function')
        order={'FermiDiracFmhalf':'−1/2','FermiDiracFhalf':'1/2','FermiDiracF3half':'3/2','FermiDiracF0':'0','FermiDiracFj':'j'}[name]
        return A('Normalized Fermi–Dirac integral, order '+order,'The Fermi occupation integral with the gamma normalization included.',
                 'Fⱼ(x)=1/Γ(j+1) ∫₀^∞ tʲ/[exp(t−x)+1] dt for j>−1.',
                 'x is real and dimensionless. '+('j is an integer; the GSL routine extends nonpositive integer orders by recurrence, with F₋₁(x)=1/(1+exp(−x)).' if order=='j' else 'This entry fixes j='+order+'.'),
                 'Some references omit 1/Γ(j+1); their values then differ by that factor.',
                 'FermiDiracF0(0) ≈ ln(2)' if order=='0' else '',GSL+'#fermi-dirac-function')
    if name in ('LambertW0','LambertWm1'):
        lower=name.endswith('m1')
        return A('Lambert W, real branch '+('−1' if lower else '0'), P('Inverts the map ', M('w\\mapsto w\\exp(w)', 'w↦w·exp(w)'), ', selecting one branch.'),
                 'W(x)·exp(W(x))=x.',
                 ('The negative real branch has x∈[−1/e,0) and W≤−1.' if lower else 'The principal real branch has x≥−1/e and W≥−1.'),
                 'This wrapper rejects x=−1/e as well as smaller inputs, although the mathematical endpoint is W=−1. '+
                 ('For positive x, GSL’s W₋₁ routine returns its W₀ continuation; that is not a real negative-branch value.' if lower else ''),
                 '' if lower else 'LambertW0(0) = 0',DLMF+'4.13')
    if name in ('CoulombF','CoulombG','CoulombFprime','CoulombGprime','CoulombC','CoulombW','HydrogenicR'):
        if name=='HydrogenicR':
            return A('Normalized hydrogenic radial wavefunction','The radial part of a bound-state wavefunction in a point-nucleus Coulomb potential.',
                     'Rₙₗ(r)=2Z^(3/2)/n² · sqrt[(n−l−1)!/(n+l)!] · (2Zr/n)ˡ exp(−Zr/n) Lₙ₋ₗ₋₁^(2l+1)(2Zr/n)',
                     'Integers n≥1, 0≤l<n, nuclear charge Z>0, radius r≥0 in atomic units. The full orbital is Rₙₗ(r)Yₗᵐ(θ,φ).',
                     alias+'Radial normalization is ∫₀^∞ |R|²r²dr=1, not ∫|R|²dr=1.', 'HydrogenicR(1,0,1,0) = 2',GSL+'#normalized-hydrogenic-bound-states')
        if name=='CoulombC':
            return A('Regular Coulomb-wave normalization','The small-radius normalization factor of the regular Coulomb solution.',
                     'C_L(η)=2ᴸ exp(−PI·η/2)|Γ(L+1+iη)|/Γ(2L+2); F_L(η,ρ) ~ C_L(η)ρ^(L+1).',
                     'Integer L≥0; real Sommerfeld parameter η.',url=DLMF+'33.2')
        if name=='CoulombW':
            return A('Application-defined Coulomb linear combination','Combines the regular and irregular Coulomb waves using the application’s fixed order-1 and order-2 normalization coefficients.',
                     'CoulombW(L,η,ρ)=C₁(η)F_L(η,ρ)+C₂(η)G_L(η,ρ)',
                     'Integer L≥0, real η and ρ>0 for the nonsingular irregular term.',
                     'Despite the W name this is not the Wronskian, a Whittaker W function, or a boundary-condition-independent “complete” physical solution.',url=SOURCE+'Computator.NET.Core/Functions/SpecialFunctions.cs')
        prime='prime' in name; regular=name.startswith('CoulombF')
        actual='F_L' if regular else 'G_L'
        note='Derivatives are with respect to dimensionless ρ, not energy or physical radius.' if prime else ''
        if prime:note+=' Returned GSL overflow scaling exponents are not applied; avoid interpreting a scaled-overflow result as the full derivative.'
        return A(('Derivative of ' if prime else '')+('regular' if regular else 'irregular')+' Coulomb wave',
                 'A real solution of the radial Coulomb scattering equation'+(' differentiated in its radial argument.' if prime else '.'),
                 'u″+[1−2η/ρ−L(L+1)/ρ²]u=0; '+actual+('′(η,ρ)' if prime else '(η,ρ)'),
                 'Integer L≥0, real η; use ρ>0. F is regular at the origin; G is the independent irregular solution.',note,url=DLMF+'33.2')
    if name.startswith('Coupling') or name in ('ClebschGordan','SphericalHarmonic'):
        if name=='SphericalHarmonic':
            return A('Complex spherical harmonic','A normalized angular-momentum eigenfunction on the unit sphere.',
                     'Yₗᵐ(θ,φ)=sqrt[(2l+1)(l−m)!/(4PI(l+m)!)] Pₗᵐ(cosθ)exp(i·mφ) for m≥0.',
                     'Integers l≥0, −l≤m≤l. θ is polar angle and φ azimuth, both radians. Negative m follows Yₗ⁻ᵐ=(−1)ᵐ conjugate(Yₗᵐ).',
                     'The associated Legendre convention includes the Condon–Shortley phase; ∫|Y|²dΩ=1.', 'SphericalHarmonic(0,0,0,0) ≈ 1/sqrt(4*PI)',DLMF+'14.30')
        if name=='ClebschGordan':
            return A('Clebsch–Gordan coefficient',P('The amplitude for coupling ', M('|j_1,m_1\\rangle', '|j₁,m₁⟩'), ' and ', M('|j_2,m_2\\rangle', '|j₂,m₂⟩'), ' into ', M('|j,m\\rangle', '|j,m⟩'), '.'),
                     '⟨j₁m₁,j₂m₂|jm⟩=(−1)^(j₁−j₂+m)sqrt(2j+1) · (j₁ j₂ j; m₁ m₂ −m)',
                     'Use actual integer or half-integer spins, not doubled values; j≥0, |m|≤j, matching half-integer parity. Selection rules require m=m₁+m₂ and triangle conditions.',url=DLMF+'34.1')
        doubled='integer' in item['menu']
        kind={'Coupling3j':'Wigner 3-j symbol','Coupling6j':'Wigner 6-j symbol','Coupling9j':'Wigner 9-j symbol','CouplingRacahW':'Racah W coefficient'}[name]
        note='The integer and real 3-j/6-j overloads use DIFFERENT argument units; explicitly use real literals when supplying actual spins.' if name in ('Coupling3j','Coupling6j') else ''
        if name=='Coupling9j':note='The final argument ji selects the lower-right spin independently of jf. A forbidden row or column coupling has value zero.'
        return A(kind,'An angular-momentum recoupling coefficient with phase and selection rules fixed by the conventional Wigner/Racah definition.',
                 'Arguments specify the successive rows of the 3×3 Wigner array.' if name=='Coupling9j' else '',
                 ('Every integer argument is TWICE the physical spin/projection: e.g. 1 represents 1/2. ' if doubled else 'Arguments are the actual nonnegative integer or half-integer spin values. ')+
                 ('Magnetic projections must obey |m|≤j, compatible parity and m₁+m₂+m₃=0.' if name=='Coupling3j' else 'Triangle and parity selection rules apply; forbidden couplings vanish or are rejected by the library.'),note,
                 example='Coupling9j(2,2,2,2,2,2,2,2,0) = 1/18 ≈ 0.0555555555555556' if name=='Coupling9j' else '',
                 url=DLMF+'34.2' if name=='Coupling3j' else DLMF+'34.4' if name=='Coupling6j' else DLMF+'34.6' if name=='Coupling9j' else GSL+'#coupling-coefficients')
    if name in ('Gegenbauer','Gegenbauer1','Gegenbauer2','Gegenbauer3','Laguerre','LegendreP','LegendreQ','SphericalLegendreP','ConicalP','SphericalConicalP','CylindricalConicalP','LegendreH3D','ChebyshevT','HermiteH','HermiteHe','ZernikeR'):
        return polynomial(name,args,alias)
    if name.startswith('Hypergeometric'):return hypergeometric(name,args)
    if name.startswith('Carlson') or name.startswith('Elliptic') or name.startswith('JacobiElliptic'):return elliptic(name,args)
    if name in ('erf','erfc','erfcx','erfi','faddeeva','inverseErf','inverseErfc','logErfc','erfZ','erfQ','hazard','OwenT','Logistic','Logit'):
        return probability(name,args,complex_arg)
    if 'Bessel' in name or name.startswith(('Hankel','SphericalHankel','Struve')):return bessel(name,args,alias)
    if name in ('Ai','Bi','AiPrime','BiPrime','AiZeros','BiZeros','AiZerosPrime','BiZerosPrime'):
        first=name.startswith('Ai'); base='Ai' if first else 'Bi'; prime='Prime' in name; zeros='Zeros' in name
        if zeros:
            return A(base+('′' if prime else '')+' negative zero by index',
                     P('Returns the ',M('s','s'),'-th negative real zero, ordered outward from the origin, of ',
                       M(r'\operatorname{'+base+'}'+(r'^{\prime}' if prime else ''),base+('′' if prime else '')),'.'),
                     base+('′' if prime else '')+'(a_s)=0; a₁>a₂>… and all a_s<0.',
                     'The argument x is an INDEX, not a coordinate. Supply a positive integer starting at 1.',
                     'The wrapper rounds x to the nearest integer with midpoint ties away from zero; x<0.5 is rejected. Avoid noninteger or out-of-range indices.',
                     'AiZeros(1) ≈ -2.3381074105' if name=='AiZeros' else '',DLMF+'9.9')
        return A(base+(' derivative' if prime else ' Airy function'),
                 ('Derivative of the '+('decaying' if first else 'growing')+' Airy solution.' if prime else
                  (P('The Airy solution that decays for large positive ', M('x', 'x'), '.') if first else P('The independent Airy solution that grows for large positive ', M('x', 'x'), '.'))),
                 'y″−xy=0; Ai and Bi oscillate for negative x.',
                 'Real x; derivative is with respect to x. These are unscaled values and may overflow/underflow for large positive x.',alias,
                 'Ai(0) ≈ 0.3550280539' if name=='Ai' else 'Bi(0) ≈ 0.6149266274' if name=='Bi' else '',DLMF+'9.2')
    if name.startswith('Mathieu'):
        characteristic=name in ('MathieuAn','MathieuBn');radial=name in ('MathieuMc','MathieuMs');even=name in ('MathieuAn','MathieuCE','MathieuMc')
        return A(('Even' if even else 'Odd')+(' Mathieu characteristic value' if characteristic else ' radial Mathieu function' if radial else ' angular Mathieu function'),
                 (P('The eigenvalue ', M('a_n(q)', 'aₙ(q)'), ' or ', M('b_n(q)', 'bₙ(q)'), ' for which the angular Mathieu equation has the corresponding periodic solution.') if characteristic else
                  'A radial solution of the modified Mathieu equation used in elliptic-coordinate boundary problems.' if radial else
                  'A periodic angular solution of Mathieu’s equation with the GSL normalization.'),
                 'f″−[a−2q cosh(2x)]f=0' if radial else 'y″+[a−2q cos(2x)]y=0',
                 'Integer order n≥0 for ce/a, n≥1 for se/b; real q. '+
                 ('This wrapper permits kinds j=1 or 2 and q>0 only; x is radial coordinate.' if radial else '' if characteristic else 'x is an angle in radians; q is the dimensionless Mathieu parameter.'),
                 'Argument order is n,q,x, not n,x,q.' if not characteristic and not radial else '',url=GSL+'#mathieu-functions')
    raise KeyError('Undocumented special function '+sig)
