"""Operation-specific probability examples with independently derived answers.

The examples use small finite supports, symmetry, or elementary special cases.
The numbers below are not copied from executing the function under test. Keep
parameter order aligned with Math.NET Numerics 5.0 and the runtime inventory.
"""
from math import exp, log, pi, sqrt
from enrichment import WorkedExample


def result(value):
    return format(value, '.12g')


# family: arguments preceding x/k, evaluation point, density/mass, derivation,
#         CDF point, CDF answer, cumulative derivation
EVALUATIONS = {
    'Bernoulli': ('0.25', '1', .25, 'A success has probability p=1/4; the other outcome has probability 3/4.',
                  '0', .75, 'At zero the CDF includes only the failure outcome, whose mass is 1−p=3/4.'),
    'Beta': ('2,2', '0.5', 1.5, 'Here B(2,2)=1/6, so the density is 6x(1−x). At the midpoint it is 1.5; a density may exceed one.',
             '0.5', .5, 'Equal beta shapes give a distribution symmetric around one half, with half the area on each side.'),
    'Binomial': ('0.5,4', '2', .375, 'Choose the two successes in six ways. Each four-trial pattern has probability 1/16, giving 6/16.',
                 '2', .6875, 'Add the masses at 0, 1 and 2 successes: (1+4+6)/16=11/16. The central atom prevents the CDF here from being one half.'),
    'Categorical': ('new double[]{1,3}', '1', .75, 'Index 1 is the second category. Its weight 3 is divided by the total weight 4.',
                    '0', .25, 'Through category index 0, only the first weight is included: 1/(1+3)=1/4.'),
    'Cauchy': ('0,1', '0', 1/pi, 'At the location, the squared standardized displacement vanishes and the density is 1/π.',
               '0', .5, 'Symmetry places half the probability below the location. This remains true although the population mean does not exist.'),
    'Chi': ('2', '1', exp(-.5), 'Two degrees of freedom reduce chi to a unit-scale Rayleigh law: f(1)=exp(−1/2).',
            '1', 1-exp(-.5), 'The squared radius has a chi-squared law with two degrees of freedom, giving F(1)=1−exp(−1/2).'),
    'ChiSquared': ('2', '2', exp(-1)/2, 'Two degrees of freedom give an exponential density with rate 1/2. At x=2 its value is exp(−1)/2.',
                   '2', 1-exp(-1), 'At x=2 the exponential cumulative probability is 1−exp(−x/2)=1−exp(−1).'),
    'ContinuousUniform': ('0,0.5', '0.25', 2, 'The interval has length 1/2, so a constant density of 2 is required for total area one.',
                          '0.25', .5, 'The point 0.25 lies halfway across [0,0.5], so half the total area lies below it.'),
    'ConwayMaxwellPoisson': ('1,1', '0', exp(-1), 'At ν=1 the model reduces to Poisson with mean one. Its zero-count mass is exp(−1).',
                             '0', exp(-1), 'At zero, the lower tail contains only the zero-count mass. The ν=1 normalization is exp(λ).'),
    'DiscreteUniform': ('1,6', '6', 1/6, 'Both bounds are included, giving six possible integers and mass 1/6 at each.',
                        '3', .5, 'Three of the six allowed integers, 1, 2 and 3, are at or below three.'),
    'Erlang': ('2,2', '1', 4*exp(-2), 'For shape two and rate two, f(x)=4x exp(−2x); at one this is 4 exp(−2).',
               '1', 1-3*exp(-2), 'Waiting for two events gives F(x)=1−exp(−2x)(1+2x). Substitute x=1.'),
    'Exponential': ('2', '0.5', 2*exp(-1), 'A rate of two means a mean wait of one half. The density at that wait is 2 exp(−1), not the cumulative probability.',
                    '0.5', 1-exp(-1), 'At one mean waiting time, rate·x=1, so the accumulated probability is 1−exp(−1).'),
    'FisherSnedecor': ('2,2', '1', .25, 'For d1=d2=2, the density simplifies to 1/(1+x)². At one it is 1/4.',
                      '1', .5, 'The two identically distributed independent chi-squared terms are equally likely to be larger, so the ratio has median one.'),
    'Gamma': ('1,2', '0.5', 2*exp(-1), 'Shape one is exponential. The second parameter is rate two, giving density 2 exp(−1) at x=0.5.',
              '0.5', 1-exp(-1), 'With shape one and rate two, F(x)=1−exp(−2x). The mean wait is 0.5.'),
    'Geometric': ('0.5', '2', .25, 'The second-trial success requires one failure followed by one success: (1/2)(1/2)=1/4.',
                  '2', .75, 'Success occurs within two trials unless both fail: 1−(1/2)²=3/4. This convention counts the successful trial.'),
    'Hypergeometric': ('5,2,2', '1', .6, 'From two successes and three failures, choose one of each in 2·3 ways out of C(5,2)=10 pairs.',
                       '1', .9, 'At most one success excludes only drawing both successes, which has probability 1/C(5,2)=1/10.'),
    'InverseGamma': ('1,2', '2', exp(-1)/2, 'At shape one and scale two, f(2)=2·2⁻² exp(−2/2)=exp(−1)/2.',
                     '2', exp(-1), 'For shape one the CDF is exp(−scale/x). Substituting scale=x=2 gives exp(−1).'),
    'Laplace': ('0,1', '0', .5, 'At the location the exponential factor is one; dividing by twice the scale gives 1/2.',
                '0', .5, 'The density is symmetric about its location, so either side contains half its area.'),
    'LogNormal': ('0,1', '1', 1/sqrt(2*pi), 'ln(1)=0, so the Gaussian exponent vanishes and the Jacobian factor 1/x is also one.',
                  '1', .5, 'The logarithm of one is the center of the associated normal law. One is the median, not the lognormal mean.'),
    'NegativeBinomial': ('1,0.5', '0', .5, 'With r=1, zero failures means that the first trial succeeds, with probability one half.',
                         '0', .5, 'The lower tail at zero consists solely of immediate success. The count excludes the successful trial.'),
    'Normal': ('0,1', '0', 1/sqrt(2*pi), 'At the mean the Gaussian exponent is zero. The standard-normal peak is 1/sqrt(2π).',
               '0', .5, 'Symmetry splits the standard-normal area into equal halves at zero.'),
    'Pareto': ('1,2', '2', .25, 'With minimum one and shape two, f(x)=2/x³; at two this is 1/4.',
               '2', .75, 'The upper tail beyond two is (1/2)²=1/4, leaving cumulative probability 3/4.'),
    'Poisson': ('2', '0', exp(-2), 'At zero events the factorial and power terms are both one, leaving exp(−2).',
                '0', exp(-2), 'Since counts cannot be negative, the CDF at zero equals the zero-event mass exp(−2).'),
    'Rayleigh': ('1', '1', exp(-.5), 'At one component standard deviation, f(1)=exp(−1/2). This is a density in radius, not a Gaussian component density.',
                 '1', 1-exp(-.5), 'Integrating the radial density gives F(1)=1−exp(−1/2).'),
    'Stable': ('1,0,1,0', '0', 1/pi, 'The supported α=1, β=0 branch is Cauchy. Unit scale and zero location give density 1/π at zero.',
                '0', .5, 'The symmetric Cauchy branch has median equal to its location. This example deliberately uses an implemented evaluation branch.'),
    'StudentT': ('0,1,1', '0', 1/pi, 'One degree of freedom gives the standard Cauchy law, whose central density is 1/π.',
                 '0', .5, 'The location splits the probability in half. At one degree of freedom, neither the mean nor variance exists.'),
    'Triangular': ('0,2,1', '1', 1, 'A triangle with base length two needs peak height one for area one; its mode is at x=1.',
                   '1', .5, 'The symmetric triangle places half its area to the left of its central mode.'),
    'Weibull': ('1,2', '2', exp(-1)/2, 'Shape one is exponential with scale two, hence rate one half. At x=2 the density is exp(−1)/2.',
                '2', 1-exp(-1), 'For shape one, x/scale=1 and F(x)=1−exp(−1).'),
    'Zipf': ('1,2', '1', 2/3, 'The two weights are 1 and 1/2, totaling 3/2. Rank one therefore has probability 2/3.',
             '1', 2/3, 'Only rank one is included in the CDF at one. The finite upper rank two is part of the normalization.'),
}

QUANTILES = {
    'Beta': ('2,2,0.5', .5, 'Symmetry puts the median halfway between zero and one.'),
    'Categorical': ('new double[]{1,3},0.5', 1, 'The first cumulative weight is 1/4, below one half; the second reaches one, so category index 1 is selected.'),
    'Cauchy': ('0,1,0.75', 1, 'The Cauchy CDF is 1/2+arctan(x)/π. At x=1, arctan(1)=π/4 and the cumulative probability is 3/4.'),
    'ChiSquared': ('2,0.5', 2*log(2), 'For two degrees of freedom, solve 1−exp(−x/2)=1/2 to obtain x=2 ln(2).'),
    'ContinuousUniform': ('0,0.5,0.25', .125, 'A quarter of the interval length 0.5 is 0.125; the last argument is a probability, not an x-coordinate.'),
    'Exponential': ('2,0.5', log(2)/2, 'The median solves exp(−2x)=1/2, so it is ln(2)/2, smaller than the mean 1/2.'),
    'FisherSnedecor': ('2,2,0.75', 3, 'Here F(x)=x/(1+x). Solving x/(1+x)=3/4 gives x=3.'),
    'Gamma': ('1,2,0.5', log(2)/2, 'Shape one reduces to exponential with rate two. Its median is ln(2)/2.'),
    'LogNormal': ('0,1,0.5', 1, 'The median is exp(mu)=1. Exponentiating the normal median differs from exponentiating its variance-adjusted mean formula.'),
    'Normal': ('2,3,0.5', 2, 'A normal median equals its mean regardless of positive standard deviation. The final 0.5 selects the 50th percentile.'),
    'Pareto': ('1,2,0.75', 2, 'Solve 1−1/x²=3/4, using the positive branch x≥1, to obtain x=2.'),
    'Rayleigh': ('1,0.5', sqrt(2*log(2)), 'Solve 1−exp(−x²/2)=1/2. The nonnegative radius is sqrt(2 ln(2)).'),
    'StudentT': ('0,1,1,0.75', 1, 'At one degree of freedom the model is Cauchy, whose third quartile is one at unit scale.'),
    'Triangular': ('0,2,1,0.125', .5, 'On the rising side F(x)=x²/2. Probability 1/8 therefore corresponds to x=1/2.'),
}

FACTORY_EXAMPLES = {
    ('Normal', 'WithMeanPrecision'): ('2,4,null', 'StdDev', .5, 'Precision four means variance 1/4 and standard deviation 1/2. Constructing the object does not sample it.'),
    ('Normal', 'WithMeanStdDev'): ('2,3,null', 'Variance', 9, 'A supplied standard deviation of three means variance nine; the factory returns an object whose property can be inspected.'),
    ('Normal', 'WithMeanVariance'): ('2,9,null', 'StdDev', 3, 'A supplied variance of nine corresponds to standard deviation three, not nine.'),
    ('Gamma', 'WithShapeRate'): ('2,4,null', 'Mean', .5, 'The mean is shape/rate=2/4. Passing four as a scale would produce a different model.'),
    ('Gamma', 'WithShapeScale'): ('2,4,null', 'Mean', 8, 'The mean is shape·scale=2·4. The factory performs the conversion from scale to rate.'),
    ('Erlang', 'WithShapeRate'): ('2,4,null', 'Mean', .5, 'Two exponential waits of mean 1/4 have total mean 1/2.'),
    ('Erlang', 'WithShapeScale'): ('2,4,null', 'Mean', 8, 'Two exponential waits of mean four have total mean eight; scale is the mean of each wait.'),
    ('LogNormal', 'WithMeanVariance'): ('2,4,null', 'Sigma', sqrt(log(2)), 'For original-space mean two and variance four, sigma²=ln(1+4/2²)=ln(2). Sigma describes the logarithms.'),
    ('LogNormal', 'WithMuSigma'): ('0,1,null', 'Mean', exp(.5), 'The original-space mean is exp(mu+sigma²/2)=exp(1/2), although the log-space mean mu is zero.'),
}

# Invalid examples test what the predicate actually rejects, rather than claiming
# that every accepted boundary defines a proper, nonsingular probability law.
INVALID_PARAMETERS = {
    'Bernoulli': ('-0.1', 'A negative number cannot be a success probability.'),
    'Beta': ('-1,2', 'A negative beta shape is inadmissible.'),
    'Binomial': ('0.5,-1', 'The number of trials cannot be negative.'),
    'Cauchy': ('0,-1', 'A scale cannot be negative even though the location may be.'),
    'Chi': ('-1', 'Degrees of freedom cannot be negative.'),
    'ChiSquared': ('-1', 'Degrees of freedom cannot be negative.'),
    'ContinuousUniform': ('2,1', 'The lower bound exceeds the upper bound.'),
    'ConwayMaxwellPoisson': ('-1,1', 'A negative λ would make the count weights inadmissible.'),
    'Dirichlet': ('new double[]{-1,2}', 'Each concentration must satisfy the library condition; this vector contains a negative one.'),
    'DiscreteUniform': ('2,1', 'An inclusive integer interval still requires its bounds in increasing order.'),
    'Erlang': ('-1,1', 'A negative number of waiting stages is invalid.'),
    'Exponential': ('-1', 'A waiting-time rate cannot be negative.'),
    'FisherSnedecor': ('-1,2', 'Both numerator and denominator degrees of freedom must be admissible.'),
    'Gamma': ('-1,1', 'A negative gamma shape is invalid.'),
    'Geometric': ('1.1', 'A success probability cannot exceed one.'),
    'Hypergeometric': ('5,6,2', 'Six successes cannot be present in a population of five.'),
    'InverseGamma': ('-1,1', 'The inverse-gamma shape cannot be negative.'),
    'InverseWishart': ('-1,matrix({{1}})', 'A positive scale matrix does not make negative degrees of freedom admissible.'),
    'Laplace': ('0,-1', 'Laplace scale is positive; its signed location is a separate parameter.'),
    'LogNormal': ('0,-1', 'The standard deviation of logarithms cannot be negative.'),
    'MatrixNormal': ('matrix({{0},{0}}),matrix({{1}}),matrix({{1}})', 'A mean with two rows requires a two-by-two row covariance; the supplied one-by-one matrix has the wrong shape.'),
    'Multinomial': ('new double[]{-1,2},3', 'A category weight cannot be negative even when the weights have a positive total.'),
    'NegativeBinomial': ('-1,0.5', 'A negative target-success shape is invalid.'),
    'Normal': ('0,-1', 'The mean may be negative; a standard deviation may not.'),
    'NormalGamma': ('0,-1,1,1', 'The conditional-mean precision multiplier cannot be negative.'),
    'Pareto': ('-1,2', 'The minimum scale must be positive, not a negative cutoff.'),
    'Poisson': ('-1', 'The expected count cannot be negative.'),
    'Rayleigh': ('-1', 'The underlying Gaussian component standard deviation cannot be negative.'),
    'Stable': ('3,0,1,0', 'The stability index α cannot exceed two; sampling support does not extend this range.'),
    'StudentT': ('0,-1,3', 'The location-scale t model requires a positive scale.'),
    'Triangular': ('0,2,3', 'The mode three lies outside the interval [0,2].'),
    'Weibull': ('0,1', 'Shape zero is not an ordinary Weibull lifetime law.'),
    'Wishart': ('-1,matrix({{1}})', 'Negative degrees of freedom are invalid even with a positive one-by-one scale matrix.'),
    'Zipf': ('1,0', 'A finite rank distribution must contain at least one allowed rank.'),
}


def examples_for(family, operation, fallback=()):
    """Keep a calculation about the selected operation in its own article."""
    # TSL also exposes scalar Beta(...) and Chi(...) methods in the generated
    # class. Qualify the distribution type to avoid C# member-name lookup.
    api = 'MathNet.Numerics.Distributions.'+family if family in ('Beta', 'Chi') else family
    if operation in ('PDF', 'PDFLn', 'PMF', 'PMFLn') and family in EVALUATIONS:
        args, point, probability, why, _, _, _ = EVALUATIONS[family]
        answer = probability
        if operation.endswith('Ln'):
            answer = log(probability)
            why += (' The requested result is ln of that density. Exponentiate it to recover the density before any interval-area approximation.'
                    if operation == 'PDFLn' else ' The requested result is ln of that mass. Exponentiating recovers the point probability; the logarithm itself is not a probability.')
        return (WorkedExample(api+'.'+operation+'('+args+','+point+')', result(answer), why),)
    if operation == 'CDF' and family in EVALUATIONS:
        args, _, _, _, point, answer, why = EVALUATIONS[family]
        example = WorkedExample(api+'.CDF('+args+','+point+')', result(answer), why)
        if family == 'Normal':
            return (example, WorkedExample('Normal.CDF(0,1,1)-Normal.CDF(0,1,-1)', 'approximately 0.6826894921',
                    'Subtract the lower cumulative probability from the upper one. This is the probability of lying within one standard deviation; the CDF at one alone includes the entire lower tail.'))
        return (example,)
    if operation == 'InvCDF' and family in QUANTILES:
        args, answer, why = QUANTILES[family]
        return (WorkedExample(api+'.InvCDF('+args+')', result(answer), why),)
    if family == 'Categorical' and operation == 'InvCDFWithCumulativeDistribution':
        return (WorkedExample('Categorical.InvCDFWithCumulativeDistribution(new double[]{1,4},0.5)', '1',
                'Cumulative weights [1,4] encode masses [1,3]. Normalize by the final total four: the first threshold is 1/4, so the median category has index one.'),)
    if (family, operation) in FACTORY_EXAMPLES:
        args, prop, answer, why = FACTORY_EXAMPLES[(family, operation)]
        return (WorkedExample(api+'.'+operation+'('+args+').'+prop, result(answer), why),)
    if operation == 'Estimate':
        if family == 'Normal':
            return (WorkedExample('Normal.Estimate(new double[]{1,2,3},null).StdDev', '1',
                    'The sample mean is two. Squared deviations total two; Math.NET uses the sample variance 2/(3−1)=1, so the fitted standard deviation is one.'),)
        if family == 'LogNormal':
            return (WorkedExample('LogNormal.Estimate(new double[]{1,exp(1),exp(2)},null).Sigma', '1',
                    'The natural logarithms are 0, 1 and 2. Their sample standard deviation is one; Sigma measures spread in log space, not the spread of the original observations.'),)
        if family == 'Weibull':
            return (WorkedExample('System.Math.Round(Weibull.Estimate(new double[]{1,2,3},null).Scale,3)', '2.259',
                    'The positive lifetimes 1, 2 and 3 give a fitted scale about 2.259 after numerical likelihood iteration. It differs from the arithmetic mean two. Three-place rounding makes the illustrative precision explicit.'),)
    if operation == 'IsValidParameterSet' and family in INVALID_PARAMETERS:
        args, why = INVALID_PARAMETERS[family]
        # These three examples construct matrices through scripting-only TSL
        # helpers. Other probability examples remain ordinary expressions.
        editor = 'scripting' if family in ('InverseWishart', 'MatrixNormal', 'Wishart') else 'expression'
        return (WorkedExample('('+api+'.IsValidParameterSet('+args+') ? 1 : 0)', '0',
                why+' The conditional displays false as numeric zero. This example demonstrates rejection; acceptance of other inputs still needs the qualifications above.', editor=editor),)
    if family == 'Categorical' and operation in ('IsValidCumulativeDistribution', 'IsValidProbabilityMass'):
        args, why = ('new double[]{1,0}', 'Cumulative weights may be unnormalized but cannot decrease.') if operation == 'IsValidCumulativeDistribution' else ('new double[]{-1,3}', 'Individual probability weights cannot be negative even when their total is positive.')
        return (WorkedExample('(Categorical.'+operation+'('+args+') ? 1 : 0)', '0', why+' The conditional converts the false predicate result to numeric zero.'),)
    return tuple(WorkedExample(*example) for example in fallback)
