"""Explicit probability models and operation semantics for Math.NET 5.0's exposed API."""
from common import A
# title, definition, density/mass, parameter conventions, implementation caveat
FAMILIES = {
'Normal': ('Normal (Gaussian)', 'A symmetric bell-shaped distribution about its mean. The width is the standard deviation, not the variance.',
 'f(x)=exp(−(x−μ)²/(2σ²))/(σ·sqrt(2π)); E[X]=μ; Var[X]=σ².',
 'mean=μ is real; stddev=σ>0; x is real.', 'A zero standard deviation is a degenerate point mass accepted by some library paths; ordinary density formulas assume σ>0.'),
'Bernoulli': ('Bernoulli', 'One success/failure trial: X=1 for success and X=0 for failure.',
 'P(X=1)=p; P(X=0)=1−p; E[X]=p.', '0≤p≤1; k is 0 or 1.', ''),
'Beta': ('Beta', 'A bounded continuous distribution for proportions on the unit interval.',
 'f(x)=x^(a−1)(1−x)^(b−1)/B(a,b); E[X]=a/(a+b).',
 'Shape parameters a>0 and b>0; 0<x<1. Endpoint limits may be infinite when a or b is below one.',
 'The library has limiting/degenerate cases at zero or infinite shapes. Use strictly positive finite shapes for the ordinary beta density; the short Beta class name collides with the scalar Beta special function in generated TSL. Use MathNet.Numerics.Distributions.Beta.PDF(a,b,x), or the corresponding fully qualified member, to avoid that collision.'),
'Binomial': ('Binomial', 'The number of successes among n independent Bernoulli trials with common success probability p.',
 'P(X=k)=C(n,k)·p^k·(1−p)^(n−k).', 'n is a nonnegative integer; 0≤p≤1; integer k ranges from 0 to n.', ''),
'Categorical': ('Categorical', 'A discrete choice among zero-based category indices, weighted by the supplied nonnegative masses.',
 'P(X=k)=w[k]/Σw[j], for k=0,…,length(w)−1.',
 'probabilityMass/p is a nonempty array of finite nonnegative weights with positive total; it need not sum to one.',
 'The result is a category INDEX, not the corresponding weight. The order of the array defines the category labels.'),
'Cauchy': ('Cauchy (Lorentz)', 'A symmetric heavy-tailed distribution with location equal to its median and mode.',
 'f(x)=1/[π·scale·(1+((x−location)/scale)²)].', 'location is real; scale>0; x is real.',
 'The mean and variance do not exist. A finite sample average is not an estimate of a finite theoretical Cauchy mean.'),
'Chi': ('Chi', 'The nonnegative square root of a chi-squared variable; for integer freedom it is the Euclidean length of independent standard normals.',
 'f(x)=2^(1−ν/2)·x^(ν−1)·exp(−x²/2)/Γ(ν/2), x>0.',
 'freedom=ν>0 is the number of degrees of freedom. The exposed sampling overloads take an INTEGER freedom; density/CDF overloads take real freedom.',
 'The short Chi class name collides with the scalar Chi special function in generated TSL. Use MathNet.Numerics.Distributions.Chi.PDF(freedom,x), or the corresponding fully qualified member, as in the worked examples.'),
'ChiSquared': ('Chi-squared', 'The sum of squares of independent standard normals for integer degrees of freedom, extended to positive real freedom.',
 'f(x)=x^(ν/2−1)·exp(−x/2)/(2^(ν/2)Γ(ν/2)); E[X]=ν.',
 'freedom=ν>0; x≥0. This is Gamma(shape=ν/2, rate=1/2), not a chi variable.', ''),
'ContinuousUniform': ('Continuous uniform', 'A constant density on a finite interval; equal-length subintervals have equal probabilities.',
 'f(x)=1/(upper−lower) for lower≤x≤upper, and 0 outside.',
 'Use finite lower<upper. Samples lie between the bounds; the continuous endpoint values have zero point probability.',
 'The library also accepts equal bounds as a point-mass limit. A continuous PDF is a density and can exceed one on a short interval.'),
'ConwayMaxwellPoisson': ('Conway–Maxwell–Poisson', 'A count distribution with dispersion controlled relative to a Poisson law.',
 'P(X=k)=λ^k/((k!)^ν·Z(λ,ν)); Z(λ,ν)=Σ[j≥0] λ^j/(j!)^ν.',
 'lambda=λ>0; nu=ν≥0; k is a nonnegative integer. For ν=0, a normalizable law additionally requires λ<1.',
 'ν=1 reduces to Poisson; ν>1 usually underdisperses and 0<ν<1 overdisperses relative to it. The library parameter validator is not a convergence proof for the normalizing series; avoid the improper ν=0, λ≥1 boundary.'),
'Dirichlet': ('Dirichlet', 'A joint distribution over probability vectors, often used as a prior on categorical proportions.',
 'f(x)=Γ(Σαᵢ)/ΠΓ(αᵢ) · Πxᵢ^(αᵢ−1), with Σxᵢ=1.',
 'alpha contains positive finite concentration parameters. The returned array has the same dimension, nonnegative components and sum approximately one.',
 'The ordinary density is on the simplex, not all of Euclidean space. A unit concentration vector gives a uniform simplex distribution; library-accepted degenerate parameters need separate care.'),
'DiscreteUniform': ('Discrete uniform', 'An equally likely INTEGER from an inclusive integer interval.',
 'P(X=k)=1/(upper−lower+1), k=lower,…,upper.',
 'lower and upper are integers with lower≤upper. Both endpoints belong to the support.',
 'Unlike typical Random.Next bounds, this distribution includes the upper endpoint.'),
'Erlang': ('Erlang', 'An integer-shape gamma distribution: the waiting time for shape events in a Poisson process of the given rate.',
 'f(x)=rate^shape · x^(shape−1)·exp(−rate·x)/Γ(shape); E[X]=shape/rate.',
 'shape is a positive integer; rate>0; x≥0. Scale is 1/rate.',
 'The ordinary law needs positive shape and rate; the library additionally supports limiting parameter cases. Do not confuse its rate argument with a scale.'),
'Exponential': ('Exponential', 'The nonnegative, memoryless waiting time to the next event in a constant-rate Poisson process.',
 'f(x)=rate·exp(−rate·x); F(x)=1−exp(−rate·x); E[X]=1/rate.',
 'rate>0 and x≥0; rate is inverse mean waiting time, not the mean or scale.', ''),
'FisherSnedecor': ('Fisher–Snedecor F', 'The ratio of two independent chi-squared variables, each divided by its degrees of freedom.',
 'X=(U/d1)/(V/d2), U~χ²(d1), V~χ²(d2).',
 'd1>0 and d2>0 are numerator and denominator degrees of freedom, respectively; x≥0.',
 'The mean exists only for d2>2; the variance needs d2>4. Exchanging d1 and d2 describes the reciprocal variable, not the same law.'),
'Gamma': ('Gamma distribution', 'A nonnegative waiting-time/scale model with a positive shape parameter, extending Erlang to noninteger shapes.',
 'f(x)=rate^shape · x^(shape−1)·exp(−rate·x)/Γ(shape); Var[X]=shape/rate².',
 'shape>0, rate>0, x≥0; the scale is 1/rate.',
 'This class uses SHAPE/RATE for its ordinary static functions. Library limiting cases at zero/infinite parameters are not the ordinary finite positive-parameter density.'),
'Geometric': ('Geometric — trials until success', 'The number of Bernoulli trials up to and INCLUDING the first success.',
 'P(X=k)=p(1−p)^(k−1), k=1,2,…; E[X]=1/p.',
 'Use 0<p≤1 for a proper finite waiting-time law. This implementation never samples zero.',
 'This is NOT the alternative failures-before-success convention on 0,1,… . The parameter validator accepts p=0, but that boundary has no finite first-success time.'),
'Hypergeometric': ('Hypergeometric', 'The number of successes in draws without replacement from a finite population.',
 'P(X=k)=C(success,k)·C(population−success,draws−k)/C(population,draws).',
 'population, success and draws are integers; 0≤success≤population and 0≤draws≤population. Support: max(0,draws−population+success)≤k≤min(draws,success).',
 'Sampling is without replacement, unlike the independent-trial Binomial model.'),
'InverseGamma': ('Inverse gamma', 'A positive distribution obtained by inverting a gamma variable, commonly used for variance modeling.',
 'f(x)=scale^shape · x^(−shape−1)·exp(−scale/x)/Γ(shape), x>0.',
 'shape>0 and scale>0. Here scale is the β in exp(−β/x), not a gamma rate argument to be inverted again.',
 'E[X]=scale/(shape−1) exists only for shape>1; finite variance requires shape>2.'),
'InverseWishart': ('Inverse Wishart', 'A distribution on symmetric positive-definite matrices, often used as a covariance prior.',
 'f(X) ∝ |X|^(−(ν+d+1)/2) · exp(−tr(ΨX⁻¹)/2).',
 'scale=Ψ is a real d×d symmetric positive-definite matrix; degreesOfFreedom=ν>d−1 for a proper nonsingular law.',
 'The scale is Ψ, not the covariance mean. E[X]=Ψ/(ν−d−1) only when ν>d+1. Validation checks do not replace a positive-definite factorization.'),
'Laplace': ('Laplace (double exponential)', 'A symmetric distribution with an exponential tail on each side of its location.',
 'f(x)=exp(−|x−location|/scale)/(2·scale); Var[X]=2·scale².',
 'location is real; scale>0; x is real. Scale is not the standard deviation.', ''),
'LogNormal': ('Lognormal', 'A positive variable whose natural logarithm is normally distributed.',
 'ln X~Normal(mu,sigma²); f(x)=exp(−(ln x−mu)²/(2sigma²))/(x·sigma·sqrt(2π)).',
 'mu is the mean of ln X; sigma>0 is the STANDARD DEVIATION of ln X; x>0.',
 'mu and sigma are not the mean and standard deviation of X. E[X]=exp(mu+sigma²/2); sigma=0 is a degenerate limit supported by some library paths.'),
'MatrixNormal': ('Matrix normal', 'A Gaussian distribution over matrices with separable row and column covariance.',
 'vec(X)~Normal(vec(m), k⊗v), using column-stacked vec.',
 'm is the d×p mean matrix; v is d×d row covariance; k is p×p column covariance. Covariance matrices must be symmetric positive definite.',
 'Matrix shapes are part of the contract; m is a matrix of means, not a scalar dimension. The covariance of vectorized samples is a Kronecker product, not v+k.'),
'Multinomial': ('Multinomial', 'The vector of category counts from n independent categorical trials.',
 'P(X=x)=n!·Πpᵢ^xᵢ/Πxᵢ!, with Σxᵢ=n.',
 'n is a nonnegative integer; p is a nonempty nonnegative weight array with positive total, normalized internally. The returned integer counts sum to n.',
 'One draw returns the whole count vector, not a single category index. Use Categorical for one category index.'),
'NegativeBinomial': ('Negative binomial — failures', 'The number of failures before r successes; the gamma-function extension also permits positive real r.',
 'P(X=k)=Γ(k+r)/(Γ(r)·k!) · p^r(1−p)^k, k=0,1,… .',
 'r>0, 0<p≤1 for the ordinary law; k is a nonnegative integer. p is the success probability.',
 'E[X]=r(1−p)/p. This counts failures, unlike this library’s Geometric class, which counts trials including the first success. Boundary r=0 or p=0 needs separate limiting interpretation.'),
'NormalGamma': ('Normal–gamma', 'A joint conjugate distribution for a normal mean μ and PRECISION τ (the inverse variance).',
 'τ~Gamma(precisionShape, rate=precisionInverseScale); μ|τ~Normal(meanLocation, variance=1/(meanScale·τ)).',
 'Use positive finite meanScale, precisionShape and precisionInverseScale, with real meanLocation. precShape/precInvScale are the same parameters in the validator.',
 'A sample is a MeanPrecisionPair, not a scalar observation or a mean/variance pair. Larger meanScale increases confidence in the conditional mean. The library also supports special infinite-parameter known-mean/known-precision limits.'),
'Pareto': ('Pareto', 'A heavy-tailed positive distribution with a hard lower cutoff.',
 'f(x)=shape·scale^shape/x^(shape+1), x≥scale.',
 'scale>0 is the MINIMUM value; shape>0 controls tail decay.',
 'The mean needs shape>1 and variance needs shape>2. The parameter order is scale,shape here, unlike Weibull’s shape,scale.'),
'Poisson': ('Poisson', 'A count model for independent events in a fixed exposure with a constant average rate.',
 'P(X=k)=exp(−λ)·λ^k/k!; E[X]=Var[X]=λ.',
 'lambda=λ>0 is the expected count in the exposure; k is a nonnegative integer.',
 'Specify the exposure separately in your model: λ is a count mean, not necessarily a rate per second. λ=0 has a mathematical point-mass limit but some library entry points require λ>0.'),
'Rayleigh': ('Rayleigh', 'The length of a two-dimensional vector of independent zero-mean normal components with equal standard deviation.',
 'f(x)=x·exp(−x²/(2scale²))/scale², x≥0.',
 'scale>0 is the standard deviation of EACH Gaussian component, not the mean radius.', ''),
'Stable': ('Stable', 'A heavy-tail family closed under sums up to location/scale changes; alpha controls the stability index and beta skewness.',
 'At alpha=2: Normal(location, stddev=sqrt(2)·scale). At alpha=1,beta=0: Cauchy(location,scale).',
 '0<alpha≤2, −1≤beta≤1, scale>0; location is real.',
 'Do not interchange stable-law parameterizations blindly, especially when alpha=1 and beta≠0. The implemented sampler scales/shifts a standard draw; density/CDF support is more limited than sampling.'),
'StudentT': ('Student t', 'A symmetric heavy-tailed location-scale t distribution, approaching a Gaussian as freedom grows.',
 'f(x)=Γ((ν+1)/2)/(scale·sqrt(νπ)Γ(ν/2)) · [1+((x−location)/scale)²/ν]^(−(ν+1)/2).',
 'freedom=ν>0; scale>0; location and x are real.',
 'Scale is not the standard deviation: Var[X]=scale²·ν/(ν−2) only for ν>2. The mean needs ν>1.'),
'Triangular': ('Triangular', 'A bounded piecewise-linear density rising to a mode and then falling to zero.',
 'f(x)=2(x−lower)/[(upper−lower)(mode−lower)] left of mode; 2(upper−x)/[(upper−lower)(upper−mode)] right of it.',
 'lower<upper, lower≤mode≤upper; support [lower,upper]. Endpoint modes use one-sided limiting formulas.',
 'The mode need not be the midpoint. Equal lower/upper is a degenerate limit, not the displayed ordinary density.'),
'Weibull': ('Weibull', 'A positive lifetime model whose hazard decreases, is constant, or increases according to the shape.',
 'f(x)=(shape/scale)·(x/scale)^(shape−1)·exp(−(x/scale)^shape), x≥0.',
 'shape>0, scale>0. Shape 1 gives Exponential(rate=1/scale).',
 'Use shape,scale in that order. Scale is a lifetime scale, not a rate.'),
'Wishart': ('Wishart', 'A distribution over symmetric positive-definite scatter matrices.',
 'For integer ν: X=Σ[j=1..ν] zⱼzⱼᵀ with independent zⱼ~Normal(0,scale); E[X]=ν·scale.',
 'scale is a real d×d symmetric positive-definite matrix; degreesOfFreedom=ν>d−1 gives a nonsingular law.',
 'A sample is a matrix, not an elementwise independent collection of gamma draws. The library’s parameter precheck is not a substitute for matrix positive definiteness.'),
'Zipf': ('Finite Zipf', 'A truncated rank-frequency distribution with power-law weights over ranks 1 through n.',
 'P(X=k)=k^(−s)/H(n,s); H(n,s)=Σ[j=1..n]j^(−s).',
 's>0 and integer n>0; k=1,…,n.',
 'This is the FINITE n model, not an unbounded zeta distribution; its normalization depends on n.'),
}
DISCRETE={'Bernoulli','Binomial','Categorical','ConwayMaxwellPoisson','DiscreteUniform','Geometric','Hypergeometric','NegativeBinomial','Poisson','Zipf','Multinomial'}
EXAMPLES={
 'Normal': 'Normal.CDF(0,1,0) = 0.5; Normal.InvCDF(0,1,0.5) = 0.',
 'Bernoulli': 'Bernoulli.PMF(0.25,1) = 0.25.',
 'Beta': 'MathNet.Numerics.Distributions.Beta.PDF(1,1,0.5) = 1: uniform density on [0,1].',
 'Binomial': 'Binomial.PMF(0.5,2,1) = 0.5.',
 'Cauchy': 'Cauchy.CDF(0,1,0) = 0.5.',
 'ContinuousUniform': 'ContinuousUniform.PDF(0,2,1) = 0.5.',
 'DiscreteUniform': 'DiscreteUniform.PMF(1,6,6) = 1/6.',
 'Exponential': 'Exponential.CDF(1,1) = 1−exp(−1).',
 'Gamma': 'Gamma.PDF(1,2,0.5) = 2·exp(−1).',
 'Geometric': 'Geometric.PMF(0.5,1) = 0.5; the smallest possible sample is 1.',
 'NegativeBinomial': 'NegativeBinomial.PMF(1,0.5,0) = 0.5.',
 'Poisson': 'Poisson.PMF(1,0) = exp(−1).',
 'Laplace': 'Laplace.CDF(0,1,0) = 0.5.',
 'LogNormal': 'LogNormal.CDF(0,1,1) = 0.5.',
 'Pareto': 'Pareto.CDF(1,2,2) = 0.75.',
 'Rayleigh': 'Rayleigh.CDF(1,1) = 1−exp(−0.5).',
 'StudentT': 'StudentT.CDF(0,1,3,0) = 0.5.',
 'Weibull': 'Weibull.CDF(1,2,2) = 1−exp(−1).',
}
FACTORIES={
 'WithMeanPrecision': ('Construct from mean and precision', 'precision=1/variance; stddev=1/sqrt(precision).', 'Use positive finite precision for a nondegenerate law.'),
 'WithMeanStdDev': ('Construct from mean and standard deviation', 'stddev is σ, not σ².', 'Use stddev>0 for the ordinary Gaussian density.'),
 'WithMeanVariance': ('Construct from mean and variance', 'var is a VARIANCE, not a standard deviation.', 'Use a finite positive variance for the ordinary continuous distribution.'),
 'WithShapeRate': ('Construct from shape and rate', 'rate=1/scale.', 'Use positive shape and rate; Erlang additionally requires integer shape.'),
 'WithShapeScale': ('Construct from shape and scale', 'scale=1/rate.', 'Use positive shape and scale; do not pass a rate in the scale position.'),
 'WithMuSigma': ('Construct from log-space parameters', 'ln X has mean mu and standard deviation sigma.', 'Use sigma>0 for an ordinary lognormal density.'),
}

def describe(item, values=None):
    family=item['type']; title,model,formula,parameters,caveat=FAMILIES[family]
    signature=item['signature']; op=signature.split('.',1)[1].split('(',1)[0]
    args=signature.split('(',1)[1].rsplit(')',1)[0].split(',')
    notes=[caveat] if caveat else []
    category='Discrete probability distributions' if family in DISCRETE else 'Continuous probability distributions'
    if family in {'Dirichlet','Wishart','InverseWishart','MatrixNormal','NormalGamma','Multinomial'}:
        category='Multivariate probability distributions'
    url='https://numerics.mathdotnet.com/api/MathNet.Numerics.Distributions/'+family+'.htm'
    example=''
    if op in ('PDF','PDFLn','PMF','PMFLn'):
        discrete=op.startswith('PMF')
        action=('Probability mass P(X=k)' if discrete else 'Probability density f(x)')
        if op.endswith('Ln'):
            action='Natural logarithm of '+action.lower()
            notes.append('This is a natural logarithm of the mass/density, not the probability of a logarithmically transformed variable. Zero mass/density has logarithm −∞; direct log evaluation can avoid ordinary underflow.')
        else:
            notes.append('A mass is a point probability between 0 and 1.' if discrete else 'A density is not P(X=x): point probability is zero for a continuous law. Integrate the density over an interval to obtain a probability.')
        definition=action+'. '+model
        parameters+=(' Evaluate at integer k in the stated support.' if discrete else ' Evaluate at x; the support is specified above.')
        example=EXAMPLES.get(family,'')
    elif op=='CDF':
        action='Cumulative probability'
        definition='Returns F(x)=P(X≤x), the accumulated probability through x. '+model
        notes.append('This is the lower tail. An upper-tail probability is 1−F(x), though that subtraction can lose precision near one.')
        if family in DISCRETE: notes.append('For a discrete law, the mathematical CDF is a step function over integer outcomes; use integer x at jumps. Fractional-x behavior must not be used as a continuous interpolation of probability masses.')
        example=EXAMPLES.get(family,'')
    elif op.startswith('InvCDF'):
        action='Inverse CDF (quantile)'
        definition='Maps cumulative probability to the corresponding outcome threshold. '+model
        parameters+=' The argument p/probability is a cumulative probability in [0,1], not another shape parameter.'
        notes.append('The mathematical lower quantile is the smallest index whose cumulative probability reaches the requested level. Math.NET 5.0 uses binary search and is not guaranteed to return the first equal cumulative weight when zero-mass categories create ties. For example, Categorical.InvCDF(new double[]{1,0,0,1},0.5) returns 1, although the lower quantile is 0. The cumulative-array overload has the same limitation.' if family=='Categorical' else 'Interior probabilities 0<p<1 avoid infinite endpoints for unbounded distributions. Numerical inversion has finite accuracy.')
        example=EXAMPLES.get(family,'')
    elif op in FACTORIES:
        action,conversion,limits=FACTORIES[op]
        definition=action+' and returns a distribution OBJECT, not a sample. '+model
        parameters=conversion+' '+limits+' randomSource selects the generator used by that object.'
        if op=='WithMeanVariance' and family=='LogNormal':
            parameters='mean is E[X]>0 and var is Var[X]>0 in the ORIGINAL positive-variable space. sigma²=ln(1+var/mean²); mu=ln(mean)−sigma²/2. randomSource selects future sampling.'
            notes.append('These parameters are not mu and sigma of ln X; use WithMuSigma for log-space inputs.')
    elif op=='Estimate':
        action='Estimate distribution parameters'
        definition='Fits a distribution object to the supplied sample sequence. '+model
        parameters='samples is a finite, nonempty data sequence; randomSource configures subsequent draws, not the fitting data. Use enough nondegenerate observations to estimate the spread.'
        if family=='Normal': notes.append('The implementation uses the sample mean and sample standard deviation; it is not a general robust fit or a claim that the data are Gaussian.')
        elif family=='LogNormal': notes.append('Every observation must be positive. The implementation fits a normal law to natural logarithms of the data; it does not fit a Gaussian directly to the original observations.')
        elif family=='Weibull': notes.append('Use positive lifetime observations. Fitting is numerical and can fail for invalid or degenerate samples; it is not a goodness-of-fit test.')
        else: raise KeyError(signature)
    elif op.startswith('IsValid'):
        action='Parameter validity check'
        definition='Returns a Boolean indicating whether the parameter set passes the library’s validity check; it does not compute a sample or a probability. '+model
        notes.append('This precheck is not proof of numerical convergence, finite moments, finite precision, or support of every operation. Prefer the finite, nondegenerate model conditions given here, even where the library accepts a limiting case.')
    elif op in ('Sample','Samples','SampleWithCumulativeDistribution','SamplesWithCumulativeDistribution'):
        many=op.startswith('Samples'); fill='values' in args
        action='Fill sample array' if fill else 'Sample sequence' if many else 'One random draw'
        if fill:
            definition='Fills the supplied values array in place with draws and returns void. Existing array contents are replaced. '+model
        elif many:
            definition='Returns a lazy, potentially unending sample sequence. Enumerate only the number of draws you need; it is not a finite materialized array. '+model
        else:
            definition='Returns one random draw. '+model
        if 'rnd' in args:
            parameters+=' rnd is a non-null System.Random instance; its state advances as draws are made.'
        else:
            parameters+=' Uses the library’s default random source; no explicit seed is accepted by this overload.'
        notes.append('Sample values are random, not the density/CDF evaluated at the parameters. Reproducibility requires an explicit generator and a controlled sequence of calls; these are not cryptographic random sources.')
    else: raise KeyError(signature)
    if family=='Categorical' and ('CumulativeDistribution' in op or op=='IsValidCumulativeDistribution'):
        parameters='cdf/cdfUnnormalized is a nonempty nondecreasing array of finite nonnegative cumulative weights with positive final total. It need not end at one; its increments, not entries, are category masses.'
        if op.startswith('InvCDF'): parameters+=' probability lies in [0,1] and is compared to the total-normalized cumulative array.'
        if 'rnd' in args: parameters+=' rnd is the random generator.'
        if 'values' in args: parameters+=' values is the output index array, filled in place.'
    if family=='Stable' and op in ('PDF','PDFLn','CDF'):
        notes.append('IMPORTANT: Math.NET 5.0 implements this evaluation only for alpha=2 (Gaussian), alpha=1 with beta=0 (Cauchy), or alpha=0.5 with beta=1 (Lévy). Other stable parameters throw NotSupportedException even though sampling and IsValidParameterSet accept them. For the Lévy branch, use x>location for ordinary evaluation. At x=location, PDF and PDFLn return NaN instead of their mathematical limits 0 and −∞; CDF returns 0. Below location, PDF/PDFLn throw and CDF returns NaN, although the mathematical density and CDF are zero there.')
    if family=='MatrixNormal' and op=='Sample':
        notes.append('IMPORTANT: Math.NET 5.0 sampling does not implement the stated column-covariance convention. It builds v⊗k⁻¹ and unpacks the draw by columns, instead of using k⊗v. In the 1×1 case with m=0, v=1 and k=4, the actual sample variance is 1/4, not the intended 4. Do not use this sampler as if k were the advertised column covariance; the formula above describes the mathematical model, not this implementation defect.')
    return A(title+' — '+action,definition,formula,parameters,' '.join(notes),example,url,category)
