"""Explicit polynomial, hypergeometric, elliptic, Gaussian and Bessel conventions."""
from common import A, P, M, DLMF, GSL, MATHNET, SOURCE

def polynomial(name,args,alias):
    if name.startswith('Gegenbauer'):
        n='n' if name=='Gegenbauer' else name[-1]
        return A('Gegenbauer polynomial, order '+n,P('An ultraspherical polynomial orthogonal on ', M('[-1,1]', '[−1,1]'), ' with weight ', M('(1-x^2)^{\\alpha-1/2}', '(1−x²)^(α−1/2)'), '.'),
                 'Σₙ₌₀^∞ Cₙ^(α)(x)tⁿ=(1−2xt+t²)^(−α)',
                 'Integer n≥0; α>−1/2 for the general wrapper, with the usual nondegenerate orthogonality for α≠0.',
                 'α=1/2 gives Legendre polynomials. The α=0 case has a special GSL limiting convention; it is not obtained by blindly substituting zero into this generating function.',
                 'Gegenbauer1(1,0.5) = 1' if n=='1' else '',GSL+'#gegenbauer-functions')
    if name=='Laguerre':
        associated='α' in args
        return A('Generalized Laguerre polynomial' if associated else 'Laguerre polynomial',
                 P('A degree-', M('n', 'n'), ' polynomial orthogonal on ', M('[0,\\infty)', '[0,∞)'), ' with weight ', M('x^\\alpha\\exp(-x)', 'x^α exp(−x)'), '.'),
                 'Lₙ^(α)(x)=Σₖ₌₀ⁿ (−1)ᵏ C(n+α,n−k)xᵏ/k!',
                 'Integer n≥0. '+('The general wrapper requires α>−1.' if associated else 'This overload fixes α=0.'),
                 'The argument x is an evaluation coordinate, not an index.', 'Laguerre(1,2) = -1' if not associated else '',DLMF+'18.5')
    if name=='LegendreP':
        associated=args.count(',')==2
        return A('Associated Legendre function' if associated else 'Legendre polynomial',
                 'An associated angular-momentum function built by differentiating a Legendre polynomial.' if associated else P('The degree-', M('l', 'l'), ' solution of Legendre’s equation normalized to ', M('P_l(1)=1', 'P_l(1)=1'), '.'),
                 'P_l^m(x)=(−1)^m(1−x²)^(m/2) dᵐP_l(x)/dxᵐ for m≥0.' if associated else 'P_l(x)=1/(2ˡl!) dˡ[(x²−1)ˡ]/dxˡ.',
                 'Integer l≥0, −1≤x≤1 in this wrapper.'+(' Integer |m|≤l; negative orders use the standard factorial relation.' if associated else ''),
                 'Includes the Condon–Shortley phase (−1)^m; this is not spherical-harmonic normalization.' if associated else '',
                 'LegendreP(2,0) = -0.5' if not associated else '',DLMF+'14.7')
    if name=='LegendreQ':
        return A('Legendre function of the second kind','The real second solution of Legendre’s equation, singular at the interval endpoints.',
                 'Q₀(x)=0.5·ln|(1+x)/(1−x)|', 'Integer l≥0; this wrapper permits x>−1 except x=1. It uses the real GSL convention on either side of 1.',
                 'This is not the Legendre polynomial P_l and is not complex analytic continuation across the cut.',url=GSL+'#legendre-functions-and-spherical-harmonics')
    if name=='SphericalLegendreP':
        return A('Spherical-harmonic-normalized associated Legendre','The polar factor in a normalized complex spherical harmonic, without its azimuthal exponential.',
                 'sqrt[(2l+1)(l−m)!/(4PI(l+m)!)]·P_l^m(x)', 'Integer 0≤m≤l and −1≤x≤1; set x=cosθ for spherical harmonics.',
                 'Includes the Condon–Shortley phase. It is not the unnormalized LegendreP overload.',url=GSL+'#legendre-functions-and-spherical-harmonics')
    if name=='ConicalP':
        return A('Conical Legendre function',P('A real-valued conical function whose Legendre degree has imaginary part ', M('\\lambda', 'λ'), '.'),
                 'P_(−1/2+iλ)^μ(x)', 'Real λ, x>−1. Supported μ are 0,1 and ±1/2; all other orders return NaN.',
                 'Integer μ accepts only 0 and 1. Real μ allows ±0.5 as well; the two overloads share this insertion signature.',url=GSL+'#conical-functions')
    if name in ('SphericalConicalP','CylindricalConicalP'):
        spherical=name=='SphericalConicalP'
        return A(('Spherical' if spherical else 'Cylindrical')+' regular conical function','A negative-order conical Legendre function used in separated coordinate problems.',
                 'P_(−1/2+iλ)^(−l−1/2)(x)' if spherical else 'P_(−1/2+iλ)^(−m)(x)',
                 ('Integer l≥−1' if spherical else 'Integer m≥−1')+', real λ and x>−1; this wrapper additionally rejects x=0.',
                 'The index determines the negative order; it is not the μ argument of ConicalP.',url=GSL+'#conical-functions')
    if name=='LegendreH3D':
        return A('Radial eigenfunction on three-dimensional hyperbolic space',P('A regular radial eigenfunction of the Laplacian on ', M('\\mathbb{H}^3', 'H³'), ' in the GSL normalization.'),
                 'For l=0: sin(λη)/(λsinhη), with continuous limits.',
                 'Integer l≥0, radial coordinate η≥0, real spectral parameter λ. This wrapper rejects λ=0 for l≥2.',
                 'This is a hyperbolic-space radial function, not an ordinary Legendre polynomial evaluated at η.',url=GSL+'#radial-eigenfunctions-for-hyperbolic-space')
    if name=='ChebyshevT':
        return A('Chebyshev polynomial of the first kind',P('A polynomial equioscillating on ', M('[-1,1]', '[−1,1]'), ', useful for approximation.'),
                 'Tₙ(cosθ)=cos(nθ)', 'Integer n≥0 and |x|≤1 in this wrapper, although the polynomial itself extends beyond that interval.',
                 '', 'ChebyshevT(2,0) = -1',DLMF+'18.5')
    if name in ('HermiteH','HermiteHe'):
        physicist=name=='HermiteH'
        return A(('Physicists’' if physicist else 'Probabilists’')+' Hermite polynomial','A Gaussian-weight orthogonal polynomial; the two common conventions have different scaling.',
                 'Hₙ(x)=(−1)ⁿ exp(x²)dⁿexp(−x²)/dxⁿ' if physicist else 'Heₙ(x)=(−1)ⁿ exp(x²/2)dⁿexp(−x²/2)/dxⁿ',
                 'Integer n≥0; real x.', 'Hₙ(x)=2^(n/2)Heₙ(sqrt(2)x). H₁(x)=2x whereas He₁(x)=x.',
                 'HermiteH(2,0) = -2' if physicist else 'HermiteHe(2,0) = -1',DLMF+'18.5')
    if name=='ZernikeR':
        return A('Radial Zernike polynomial','The radial factor in Zernike modes on a unit disk, used in optical-aberration analysis.',
                 'Rₙᵐ(ρ)=Σₛ₌₀^((n−m)/2) (−1)ˢ(n−s)!ρ^(n−2s)/[s!((n+m)/2−s)!((n−m)/2−s)!]',
                 'Integers n≥0, 0≤m≤n and 0≤ρ≤1. A nonzero radial polynomial requires n−m even.',
                 'No angular sin(mθ)/cos(mθ) factor or unit-RMS optical normalization is included.', 'ZernikeR(2,0,0) = -1',DLMF+'18.3')
    raise KeyError(name)

def hypergeometric(name,args):
    renorm='renorm' in name;conj=args=='a,c,x'
    if '2F1' in name:
        return A(('Renormalized ' if renorm else '')+('conjugate-parameter ' if conj else '')+'Gauss hypergeometric function',
                 'The Gauss hypergeometric solution with numerator parameters '+('a and its complex conjugate.' if conj else P(M('a,b', 'a,b'), ' and denominator parameter ', M('c', 'c'), '.')),
                 '₂F₁(a,b;c;x)=Σₖ₌₀^∞ (a)ₖ(b)ₖ xᵏ/[(c)ₖ k!]'+(' ; returned value is ₂F₁/Γ(c).' if renorm else '.'),
                 ('The complex overload supplies b=conjugate(a), not an arbitrary second complex parameter. ' if conj else '')+
                 'Real x in the GSL real-domain implementation; the series converges for |x|<1, with supported boundary/continuation cases. The ordinary form has denominator poles at c=0,−1,−2,… unless a limiting cancellation applies.',
                 'Renormalization removes the explicit Γ(c) factor, and is not division by Γ(a)Γ(b).' if renorm else 'Do not infer complex branch values for x>1 from this real-valued interface.',url=GSL+'#hypergeometric-functions')
    if name=='Hypergeometric0F1':
        return A('Confluent hypergeometric limit function','A hypergeometric function closely related to Bessel functions.',
                 '₀F₁(;c;x)=Σₖ₌₀^∞ xᵏ/[(c)ₖ k!]', 'Real c,x; c must not be zero or a negative integer for the ordinary definition.',
                 'The empty numerator parameter list is intentional. The Pochhammer symbol denotes a rising factorial.', 'Hypergeometric0F1(1,0) = 1',DLMF+'16.2')
    if name=='Hypergeometric1F1':
        return A('Kummer confluent hypergeometric function',P('The regular confluent hypergeometric solution ', M('M(a,b,x)', 'M(a,b,x)'), '.'),
                 '₁F₁(a;b;x)=Σₖ₌₀^∞ (a)ₖ xᵏ/[(b)ₖ k!]',
                 'The integer m,n overload uses m as a and n as b. Real x; ordinary denominator parameters b=0,−1,… are singular except for valid terminating/limiting cases.',
                 'Unlike the similarly named Tricomi U, this is the solution normalized to 1 at x=0.', 'Hypergeometric1F1(1.0,1.0,1) ≈ e',DLMF+'13.2')
    if name=='HypergeometricU':
        return A('Tricomi confluent hypergeometric function','The second standard confluent hypergeometric solution, selected by its large-positive-x behavior.',
                 'x·U″+(b−x)U′−aU=0; U(a,b,x) ~ x^(−a).',
                 'Use real x>0 for the real integral branch; integer m,n overloads mean a=m,b=n. The library supplies continuations in its supported parameter domain.',
                 'This is U, not Kummer M=₁F₁; the two solutions have different normalizations.',url=DLMF+'13.2')
    if name=='Hypergeometric2F0':
        return A('Asymptotic hypergeometric function ₂F₀','A generally divergent hypergeometric series interpreted through a confluent-hypergeometric continuation.',
                 '₂F₀(a,b;;x) ~ Σₖ₌₀^∞ (a)ₖ(b)ₖ xᵏ/k!',
                 'For ordinary nonterminating parameters the series is asymptotic, not a convergent power series at nonzero x. GSL provides the real x<0 branch via U.',
                 'The series terminates when a or b is a nonpositive integer. Do not label this the Gauss ₂F₁ function.',url=GSL+'#hypergeometric-functions')
    raise KeyError(name)

def elliptic(name,args):
    if name.startswith('Carlson'):
        kind=name[-1]
        formula={'C':'R_C(x,y)=1/2 ∫₀^∞ dt/[(t+y)sqrt(t+x)]',
                 'D':'R_D(x,y,z)=3/2 ∫₀^∞ dt/[(t+z)sqrt((t+x)(t+y)(t+z))]',
                 'F':'R_F(x,y,z)=1/2 ∫₀^∞ dt/sqrt((t+x)(t+y)(t+z))',
                 'J':'R_J(x,y,z,p)=3/2 ∫₀^∞ dt/[(t+p)sqrt((t+x)(t+y)(t+z))]'}[kind]
        return A('Carlson symmetric elliptic integral R_'+kind,'A symmetric-form elliptic integral useful for expressing many Legendre-form elliptic integrals.',formula,
                 'This wrapper requires every supplied argument to be strictly positive, even where broader limiting definitions exist.',
                 'R_D is symmetric in x,y; R_F and R_J are symmetric in x,y,z; R_D(x,y,z)=R_J(x,y,z,z).',url=DLMF+'19.16')
    if name.startswith('JacobiElliptic'):
        kind=name[-2:].lower()
        return A('Jacobi elliptic '+kind,P('An elliptic analogue of a trigonometric function, parameterized by ', M('m', 'm'), '.'),
                 {'sn':'sn(u|m)=sin(am(u|m))','cn':'cn(u|m)=cos(am(u|m))','dn':'dn²(u|m)=1−m·sn²(u|m)'}[kind],
                 'Real u and −1≤m≤1 in this wrapper. Here m is the PARAMETER k², unlike the modulus argument of EllipticK/E/F.',
                 'Identities: sn²+cn²=1 and dn²+m·sn²=1. At m=0, sn=sin, cn=cos and dn=1.',url=DLMF+'22.2')
    complete=',' not in args or args=='x,n'
    kind=name[len('Elliptic'):]
    end='PI/2' if complete else 'φ'
    integrand={'K':'1/sqrt(1−k²sin²t)','F':'1/sqrt(1−k²sin²t)','E':'sqrt(1−k²sin²t)',
               'Π':'1/[(1+n·sin²t)sqrt(1−k²sin²t)]','D':'sin²t/sqrt(1−k²sin²t)'}[kind]
    domain='The argument named x is the MODULUS k, not m=k². '
    if complete:domain+='Use |k|<1'+('; EllipticE also admits k=1' if kind=='E' else '')+'. '
    else:domain+='This wrapper limits −PI/2≤φ≤PI/2 and −1≤k≤1; avoid singular endpoint combinations. '
    if kind=='Π':domain+=('This complete form requires n>−1.' if complete else 'The characteristic n is an integer in this incomplete overload; n≥−1, with denominator singularities excluded.')
    note='This Π uses 1+n·sin²t. References using 1−n·sin²t need the opposite sign of n.' if kind=='Π' else 'Complete means integration to PI/2, not evaluation at φ=k.'
    return A(('Complete' if complete else 'Incomplete')+' elliptic integral '+kind,'The '+{'K':'first','F':'first','E':'second','Π':'third','D':'D-form'}[kind]+' Legendre elliptic integral in the application’s modulus convention.',
             '∫₀^'+end+' '+integrand+' dt',domain,note,
             'EllipticK(0) ≈ PI/2' if kind=='K' else 'EllipticD(0) ≈ PI/4' if kind=='D' and complete else '',GSL+'#elliptic-integrals')

def probability(name,args,complex_arg):
    data={
     'erf':('Error function','The normalized integral of a Gaussian, continued to an entire complex function.', 'erf(z)=2/sqrt(PI) ∫₀ᶻ exp(−t²)dt', 'Real or complex input; erf(0)=0. For real x, erf(x) lies in [−1,1].'),
     'erfc':('Complementary error function','The upper Gaussian integral complementary to erf.', 'erfc(z)=1−erf(z)', 'Real or complex input. For real x, erfc(x) decreases from 2 to 0.'),
     'erfcx':('Exponentially scaled complementary error','Removes the leading Gaussian exponential from erfc mathematically.', 'erfcx(z)=exp(z²)erfc(z)', 'Real or complex input; erfcx(0)=1.'),
     'erfi':('Imaginary error function','An entire counterpart of erf defined by rotating its argument.', 'erfi(z)=−i·erf(i·z)', 'Complex input; for real z its mathematical value is real, despite the name.'),
     'faddeeva':('Faddeeva complex probability function','A complex Gaussian-response function used in Voigt line profiles and wave propagation.', 'w(z)=exp(−z²)erfc(−i·z)', 'Complex z; w is entire. This is not simply erf(z) or a real probability.'),
     'inverseErf':('Inverse error function',P('Solves ', M('\\operatorname{erf}(y)=x', 'erf(y)=x'), ' on the real principal inverse.'), 'erf(inverseErf(x))=x', 'Real domain −1≤x≤1. Negative inputs give negative results; x=−1 gives −∞, x=0 gives 0 and x=1 gives +∞. Inputs outside the domain return NaN.'),
     'inverseErfc':('Inverse complementary error',P('Solves ', M('\\operatorname{erfc}(y)=x', 'erfc(y)=x'), ' on the real principal inverse.'), 'erfc(inverseErfc(x))=x', 'Real domain 0≤x≤2. Inputs above 1 give negative results; x=0 gives +∞, x=1 gives 0 and x=2 gives −∞. Inputs outside the domain return NaN.'),
     'logErfc':('Logarithm of the complementary error function','Computes the Gaussian-tail logarithm directly rather than taking a logarithm after tail underflow.', 'ln(erfc(x))', 'Real x; useful for likelihoods involving small Gaussian tails.'),
     'erfZ':('Standard normal density','The probability density of a zero-mean, unit-variance Gaussian.', 'φ(x)=exp(−x²/2)/sqrt(2PI)', 'Real x; a density, not a tail probability. Area over an interval gives a probability.'),
     'erfQ':('Standard normal upper-tail probability',P('The probability that a standard normal variable exceeds ', M('x', 'x'), '.'), 'Q(x)=1/2·erfc(x/sqrt(2))', 'Real x; values lie in [0,1]. erfQ(0)=1/2.'),
     'hazard':('Standard normal hazard function','The Gaussian density divided by the remaining upper-tail probability.', 'h(x)=φ(x)/Q(x)', 'Real x. This is the normal hazard or inverse Mills ratio, not a probability bounded by 1.'),
     'OwenT':('Owen’s T function','A bivariate-normal probability integral useful for skew-normal and correlated-tail calculations.', 'T(h,a)=1/(2PI) ∫₀ᵃ exp[−h²(1+t²)/2]/(1+t²) dt', 'Real h,a; even in h and odd in a. The three-argument overload requires ah=a·h as a precomputed product.'),
     'Logistic':('Logistic sigmoid','Maps a real log-odds value to a probability.', 'σ(x)=1/(1+exp(−x))', 'Real x; mathematical values lie in (0,1), with floating-point saturation at extremes. Logistic(0)=0.5.'),
     'Logit':('Log-odds transform','The inverse of the logistic sigmoid on probabilities strictly between zero and one.', 'logit(p)=ln[p/(1−p)]', '0≤p≤1 in this wrapper; endpoints give −∞ and +∞. Outside the interval returns NaN.')
    }
    title,definition,formula,domain=data[name]
    note=''
    if name=='erfcx':note='The current wrapper forms erfc(z)/exp(−z²) directly. It can still overflow, underflow or suffer cancellation; do not assume the name guarantees a stabilized scaled algorithm.'
    if name=='erfc' and complex_arg:note='The complex wrapper subtracts erf(z) from 1, so very small complementary tails may lose relative accuracy.'
    if name=='inverseErfc':note='Positive binary64 tails, including subnormal inputs, have finite inverses. Complementary inversion is evaluated directly: forming inverseErf(1−x) first would round very small x away.'
    return A(title,definition,formula,domain,note,url=DLMF+('7.17' if name in ('inverseErf','inverseErfc') else '7.2') if name not in ('OwenT','Logistic','Logit') else 'https://accord-framework.net/docs/html/T_Accord_Math_OwensT.htm' if name=='OwenT' else MATHNET+'MathNet.Numerics/SpecialFunctions.htm')

def bessel(name,args,alias):
    if name in ('StruveL0','StruveL1'):
        order=name[-1]
        return A('Modified Struve function L_'+order,'The modified Struve solution of an inhomogeneous Bessel equation, distinct from modified Bessel I.',
                 'L_ν(x)=Σₖ₌₀^∞ (x/2)^(2k+ν+1)/[Γ(k+3/2)Γ(k+ν+3/2)]',
                 'Real x; this entry fixes ν='+order+'. Large positive x gives rapid exponential growth.',url=DLMF+'11.2')
    if name=='logBesselKν':
        return A('Logarithm of modified Bessel K',P('Computes ', M('\\ln K_\\nu(x)', 'ln K_ν(x)'), ' without requiring a representable unscaled ', M('K', 'K'), ' value.'),
                 'ln[K_ν(x)]', 'Real ν≥0 and x>0 in this wrapper; otherwise NaN.',
                 'K is positive for this domain. This logarithm is distinct from a scaled K function.',url=GSL+'#irregular-modified-cylindrical-bessel-functions')
    if name=='BesselJνZeros':
        return A('Positive zero of Bessel J by index',P('Returns the ', M('s', 's'), '-th positive zero of ', M('J_\\nu', 'J_ν'), '.'),
                 'J_ν(j_(ν,s))=0; 0<j_(ν,1)<j_(ν,2)<…', 'Real order ν≥0; supply integer index s≥1.',
                 's is rounded to the nearest unsigned integer, with midpoint ties away from zero; s<0.5 is rejected. It is not a starting guess for root finding.',
                 'BesselJνZeros(0,1) ≈ 2.4048255577',DLMF+'10.21')
    if name in ('RiccatiBesselξ','RiccatiBesselζ'):
        first=name.endswith('ξ')
        return A('Riccati–Hankel function of the '+('first' if first else 'second')+' kind',
                 'A spherical Hankel wave multiplied by its radial argument, with the same normalization as the real Riccati–Bessel functions S and C.',
                 'ξₙ(x)=x·hₙ⁽¹⁾(x)=Sₙ(x)−iCₙ(x)' if first else 'ζₙ(x)=x·hₙ⁽²⁾(x)=Sₙ(x)+iCₙ(x)',
                 'Integer n and real x>0. Nonpositive arguments return NaN; the finite order-zero limit at x=0 is not evaluated by this wrapper.',
                 'Here Sₙ=x·jₙ and Cₙ=−x·yₙ use spherical Bessel functions. The first and second kinds are complex conjugates on the positive real axis; the outgoing/incoming interpretation also depends on the time convention. For 0<x≤1e−8, the implementation uses scaled radial values to avoid overflowing an intermediate spherical function when the final components remain representable.',
                 name+'(0,0.5) ≈ 0.4794255386'+(' − ' if first else ' + ')+'0.8775825619i',DLMF+'10.47')
    if name in ('RiccatiBesselS','RiccatiBesselC'):
        actual={'RiccatiBesselS':'x·jₙ(x)','RiccatiBesselC':'−x·yₙ(x)'}[name]
        note=alias+' jₙ and yₙ are spherical Bessel functions. C/χ uses a minus sign; some references use the opposite sign.'
        return A('Riccati-labelled Bessel combination','The application’s radial Bessel combination, with its sign and cylindrical/spherical convention made explicit.',actual,
                 'Integer n; use x>0 for irregular/Hankel forms. Singular limits at zero should not be inferred from multiplication by x.',note,url=SOURCE+'Computator.NET.Core/Functions/SpecialFunctions.cs')
    if name.startswith(('Hankel','SphericalHankel')):
        spherical=name.startswith('Spherical');first=name.endswith('1')
        return A(('Spherical Hankel' if spherical else 'Cylindrical Hankel')+' function of the '+('first' if first else 'second')+' kind',
                 'A complex combination of the independent regular and irregular Bessel solutions, useful for wave boundary conditions.',
                 ('hₙ' if spherical else 'H_α')+('⁽¹⁾' if first else '⁽²⁾')+'(x)='+('jₙ' if spherical else 'J_α')+'(x)'+('+' if first else '−')+'i·'+('yₙ' if spherical else 'Y_α')+'(x)',
                 ('Integer order n' if spherical else 'Real order α')+' and positive real x for the nonsingular real-axis interface.',
                 'Whether this represents an outgoing or incoming wave also depends on the chosen time convention.',url=DLMF+('10.47' if spherical else '10.2'))
    spherical='Spherical' in name; modified=name.startswith('Modified')
    family='I' if 'BesselI' in name else 'K' if 'BesselK' in name else 'J' if 'BesselJ' in name else 'Y'
    order=name[-1] if name[-1] in '0123' else 'ν' if 'ν' in name else 'n'
    if name=='BesselJnPrime':
        return A('Derivative of integer-order Bessel J','Derivative with respect to the argument, evaluated by the adjacent-order recurrence.',
                 'J′ₙ(x)=[Jₙ₋₁(x)−Jₙ₊₁(x)]/2', 'Integer n and real x supported by the underlying cylindrical Bessel functions.',
                 'This differentiates x, not the order n.',url=DLMF+'10.6')
    if modified and spherical:
        return A('Exponentially scaled modified spherical Bessel '+family.lower(),
                 'A scaled modified spherical Bessel value; the exponential scale is part of the returned result.',
                 'exp(−|x|)·iₙ(x), with iₙ(x)=sqrt(PI/(2x)) Iₙ₊₁⁄₂(x).' if family=='I' else
                 'exp(x)·kₙ(x), with kₙ(x)=sqrt(PI/(2x)) Kₙ₊₁⁄₂(x).',
                 'Integer n≥0; '+('real x, with limiting behavior at zero supplied by GSL.' if family=='I' else 'strictly positive real x.'),
                 'The public name omits “scaled”. Do not compare directly with unscaled tabulated iₙ/kₙ. GSL’s k₀(x)=PI·exp(−x)/(2x), not exp(−x)/x.',url=GSL+'#modified-spherical-bessel-functions')
    definition={
      'J':'The solution regular at the origin for nonnegative integer order.',
      'Y':'The independent irregular real-axis Bessel solution, singular at the origin for nonnegative integer order.',
      'I':'The modified-Bessel solution that grows exponentially on the positive real axis.',
      'K':'The modified-Bessel solution that decays exponentially on the positive real axis.'}[family]
    formula=('jₙ(x)=sqrt(PI/(2x))Jₙ₊₁⁄₂(x)' if family=='J' else 'yₙ(x)=sqrt(PI/(2x))Yₙ₊₁⁄₂(x)') if spherical else \
        ('x²y″+xy′−(x²+ν²)y=0' if modified else 'x²y″+xy′+(x²−ν²)y=0')
    domain=('Order fixed at '+order if order in '0123' else 'Integer order n' if order=='n' else 'Real order ν')+'. '
    domain+=('Use x>0 for this real irregular/modified-K interface; zero is singular.' if family in ('Y','K') else
             'Real x≥0 in the real-order wrapper; negative arguments can require complex continuation.' if order=='ν' else
             'Real x; standard regular limits apply at zero for nonnegative integer order.')
    return A(('Modified ' if modified else '')+('spherical ' if spherical else 'cylindrical ')+family+' Bessel function, order '+order,
             definition,formula,domain,'This is an unscaled value.' if modified else '',
             'BesselJ0(0) = 1' if name=='BesselJ0' else 'SphericalBesselJ0(0) = 1' if name=='SphericalBesselJ0' else '',DLMF+('10.47' if spherical else '10.25' if modified else '10.2'))
