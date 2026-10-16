"""Reviewed inverse-function lessons, including the actual overload conventions.

Tuples contain authored TeX, interpretation paragraphs and independently derived
scalar examples. The corresponding elementary article retains its domain and
implementation warning. No numerical wrapper is changed by these lessons.
"""

REAL_INVERSES = {
    'arcsin': (r'\sin(\arcsin x)=x,\quad -1\le x\le1', (
        'The input is a sine value, such as opposite-side length divided by hypotenuse; the output is an angle in radians. For real inputs the chosen angle lies between −π/2 and π/2, including both endpoints.',
        'Sine takes the same value at several angles. Consequently arcsin(sin(θ)) returns θ only on the chosen interval. An angle of 120 degrees is folded back to 60 degrees, because both have the same sine. Multiply the returned radians by 180/PI to display degrees.'), (
        ('arcsin(0.5)*180/PI', '30', 'A side ratio of one half gives π/6 radians, or thirty degrees.'),
        ('arcsin(sin(2*PI/3))*180/PI', '60', 'The input angle is 120 degrees, but the inverse selects the angle in its principal interval with the same sine.'))),
    'arccos': (r'\cos(\arccos x)=x,\quad -1\le x\le1', (
        'For real input x, arccos finds the unique angle between zero and π whose cosine is x. A negative cosine therefore gives an obtuse angle, whereas a positive cosine gives an acute angle.',
        'For vectors a and b the ratio (a·b)/(|a||b|) is a cosine only when neither vector is zero. Rounding can move a computed ratio slightly outside [−1,1]; resolve that numerical error before asking for a real inverse.'), (
        ('arccos(-0.5)*180/PI', '120', 'A cosine of minus one half selects the obtuse angle 2π/3 radians.'),)),
    'arctan': (r'\tan(\arctan x)=x', (
        'Arctangent converts a real slope to an angle strictly between −π/2 and π/2. Its input can have any finite magnitude; steep slopes approach a right angle without reaching it.',
        'A slope y/x loses the signs of x and y as a pair: (1,1) and (−1,−1) have the same ratio. When reconstructing a vector direction in all quadrants, Phase(x+i·y) retains the quadrant information that arctan(y/x) discards.'), (
        ('arctan(1)*180/PI', '45', 'A rise of one per unit run gives slope one and an acute direction of π/4.'),
        ('Phase(-1-1i)*180/PI', '-135', 'The vector points into the third quadrant, even though dividing its coordinates gives slope one.'))),
    'arccot': (r'\operatorname{arccot}x=\frac{\pi}{2}-\arctan x', (
        'This application chooses real inverse cotangent values strictly between zero and π. Zero gives π/2, and negative inputs give obtuse angles.',
        'The choice matters when comparing tables: atan(1/x) gives a negative angle for negative x, while this function adds the branch needed to stay in (0,π). The spelling arcctg uses this same convention.'), (
        ('arccot(-1)*180/PI', '135', 'Cotangent is minus one at 135 degrees. Using arctan(1/(-1)) would instead select −45 degrees.'),)),
    'arcsec': (r'\operatorname{arcsec}x=\arccos(1/x)', (
        'Inverse secant first takes the reciprocal of its input and then applies inverse cosine. A finite real result requires |x|≥1; the returned angle lies in [0,π] with π/2 excluded.',
        'The reciprocal is inside the inverse function. Dividing one by arccos(x) would calculate a different quantity and would not undo secant.'), (
        ('arcsec(-2)*180/PI', '120', 'The reciprocal is −1/2, so the selected angle is the obtuse angle with cosine −1/2.'),)),
    'arccsc': (r'\operatorname{arccsc}x=\arcsin(1/x)', (
        'Inverse cosecant accepts real values outside the open interval (−1,1). The selected angle has the same sign as its input and lies between −π/2 and π/2, with zero excluded for finite inputs.',
        'As |x| increases, its reciprocal approaches zero and the selected angle approaches zero. A large cosecant therefore corresponds to a small angle, rather than to a large angle.'), (
        ('arccsc(-2)*180/PI', '-30', 'Taking the reciprocal gives a sine value of −1/2; the principal inverse selects −π/6.'),)),
    'arsinh': (r'\operatorname{arsinh}x=\ln(x+\sqrt{x^2+1})', (
        'The mathematical inverse of sinh is odd and maps the whole real line to itself. Near zero it behaves like x; for large positive x it grows approximately as ln(2x).',
        'The current public wrapper has the misplaced-parenthesis defect described below. The worked expression evaluates the intended formula explicitly at a moderate positive argument; it does not claim that the existing arsinh call has been repaired.'), (
        ('ln(2+sqrt(5))', 'approximately 1.4436354752', 'Solving sinh(y)=2 gives exp(y)=2+sqrt(5), so the intended inverse is ln(2+sqrt(5)).'),)),
    'arcosh': (r'\operatorname{arcosh}x=\ln(x+\sqrt{x+1}\sqrt{x-1})', (
        'Real cosh is even and has minimum value one. Its inverse therefore needs the restriction x≥1 and selects the nonnegative argument, rather than both signs.',
        'For a catenary y=a·cosh(t/a), an observed height y≥a gives the nonnegative horizontal distance a·arcosh(y/a). The negative distance describes the reflected point on the other side.'), (
        ('arcosh(2)', 'approximately 1.3169578969', 'The selected argument is ln(2+sqrt(3)); its hyperbolic cosine is two.'),)),
    'artanh': (r'\operatorname{artanh}x=\frac12\ln\frac{1+x}{1-x}', (
        'The real inverse reverses the compression performed by tanh. Its input must lie strictly between −1 and one, and the output can have any finite real magnitude.',
        'The logarithm diverges at either endpoint. Consequently small input errors near ±1 can cause large changes in the inverse; a value rounded to an endpoint cannot recover its original finite argument.'), (
        ('artanh(0.5)', 'approximately 0.5493061443', 'The logarithmic ratio is (1+1/2)/(1−1/2)=3; half of ln(3) gives the inverse.'),)),
    'arcoth': (r'\operatorname{arcoth}x=\frac12\ln\frac{x+1}{x-1}', (
        'The real inverse of coth uses |x|>1 and returns a nonzero real argument with the same sign. Positive values approaching one correspond to arguments growing without bound.',
        'The spelling arctgh refers to arcoth in this application, whereas artgh refers to artanh. Their real input domains are different, so the extra letter changes the operation.'), (
        ('arcoth(2)', 'approximately 0.5493061443', 'The ratio (2+1)/(2−1)=3 gives ln(3)/2. Its coth is two, while its tanh is one half.'),)),
    'arcsch': (r'\operatorname{arcsch}x=\ln(1/x+\sqrt{1/x^2+1})', (
        'The intended real inverse is asinh(1/x), with x≠0. Its output has the sign of x, and large |x| corresponds to a small nonzero argument.',
        'This wrapper evaluates the displayed logarithm directly; it does not call the separate arsinh routine. Reciprocals can overflow near zero and subtraction can lose accuracy for large negative reciprocal arguments.'), (
        ('arcsch(2)', 'approximately 0.4812118251', 'The reciprocal is one half. The resulting logarithm is ln((1+sqrt(5))/2).'),)),
    'arsech': (r'\operatorname{arsech}x=\ln(1/x+\sqrt{1/x+1}\sqrt{1/x-1})', (
        'The real inverse of sech uses 0<x≤1 and selects a nonnegative argument. It returns zero at x=1 and grows without bound as x approaches zero from above.',
        'Taking the reciprocal converts the problem to arcosh(1/x). A negative input has no real inverse in this convention, since sech is positive on the entire real axis.'), (
        ('arsech(0.5)', 'approximately 1.3169578969', 'A secant-hyperbolic value of one half means cosh(y)=2, giving the nonnegative inverse ln(2+sqrt(3)).'),)),
}


COMPLEX_INVERSES = {
    'arcsin': (r'\sin(\arcsin z)=z',
        'System.Numerics.Complex.Asin selects one complex inverse. The real input restriction [−1,1] does not apply to a complex argument, but the cuts extending from −1 and one and the side of approach still determine the selected continuation.',
        ('Im(arcsin(i))', 'approximately 0.8813735870', 'sin(i·y)=i·sinh(y), so the inverse at i has imaginary coordinate ln(1+sqrt(2)).')),
    'arccos': (r'\cos(\arccos z)=z',
        'System.Numerics.Complex.Acos chooses one inverse rather than all angles with the same cosine. Complex results need not be real angles in [0,π]; branch cuts and their sides are part of the continuation.',
        ('Im(arccos(i))', 'approximately -0.8813735870', 'cos(π/2−i·y)=i·sinh(y), so the selected imaginary coordinate is −ln(1+sqrt(2)).')),
    'arctan': (r'\tan(\arctan z)=z,\qquad z\ne\pm i',
        'The complex inverse has singularities at z=±i. It does not reconstruct a vector’s polar angle: use Phase for that separate real-valued operation. Approaching a cut of the complex inverse from different sides can select different boundary values.',
        ('Im(arctan(0.5*i))', 'approximately 0.5493061443', 'tan(i·y)=i·tanh(y); solving tanh(y)=1/2 gives the imaginary coordinate ln(3)/2.')),
    'arccot': (r'\operatorname{arccot}z=\frac{\pi}{2}-\arctan z',
        'The application subtracts Complex.Atan(z) from π/2. This retains the real (0,π) convention when z is real and finite, but need not agree with atan(1/z) across complex branches. Singularities remain at ±i.',
        ('Im(arccot(0.5*i))', 'approximately -0.5493061443', 'Subtracting arctan(i/2) from π/2 reverses its imaginary coordinate, giving −ln(3)/2.')),
    'arcsec': (r'\operatorname{arcsec}z=\arccos(1/z)',
        'The reciprocal is formed first and passed to Complex.Acos. Zero is singular, and the reciprocal maps the inverse-cosine cuts into the corresponding cuts for this convention. It is not 1/arccos(z).',
        ('Re(arcsec(-2+0i))*180/PI', '120', 'The reciprocal is −1/2. At this point away from the cuts the selected value agrees with real inverse cosine.')),
    'arccsc': (r'\operatorname{arccsc}z=\arcsin(1/z)',
        'The reciprocal selects the argument supplied to Complex.Asin. Complex inputs are not constrained by the real condition |x|≥1, but zero remains singular and reciprocal images of branch cuts still matter.',
        ('Im(arccsc(i))', 'approximately -0.8813735870', 'The reciprocal of i is −i. The sine identity for imaginary arguments gives imaginary coordinate −ln(1+sqrt(2)).')),
    'arsinh': (r'\operatorname{arsinh}z=\operatorname{Log}\!\left(z+\sqrt{z^2+1}\right)',
        'This is the intended logarithmic construction, with compatible principal branches. The current complex wrapper instead adds Log(z) and sqrt(z²+1); the implementation warning is essential. The example explicitly evaluates the intended expression at a positive point away from cuts.',
        ('Re(ln((2+0i)+sqrt((2+0i)*(2+0i)+1)))', 'approximately 1.4436354752', 'At z=2 the principal roots and logarithm reduce to the positive real expression ln(2+sqrt(5)).')),
    'arcosh': (r'\operatorname{arcosh}z=\operatorname{Log}\!\left(z+\sqrt{z+1}\sqrt{z-1}\right)',
        'The implementation takes two separate principal square roots before the principal logarithm. Replacing their product by sqrt(z²−1) without checking branches can change the result; the real x≥1 rule is only the real slice of this construction.',
        ('Re(arcosh(2+0i))', 'approximately 1.3169578969', 'At a positive real point above one, both roots are positive and the result is ln(2+sqrt(3)).')),
    'artanh': (r'\operatorname{artanh}z=\frac12\operatorname{Log}\frac{1+z}{1-z}',
        'The principal logarithm is taken after forming the whole ratio. Splitting it into a difference of principal logarithms can alter a branch by a multiple of πi. The points ±1 are singular.',
        ('Re(artanh(0.5+0i))', 'approximately 0.5493061443', 'Inside the real interval the ratio is positive, so the complex result is the real value ln(3)/2.')),
    'arcoth': (r'\operatorname{arcoth}z=\frac12\operatorname{Log}\frac{z+1}{z-1}',
        'The principal logarithm of the ratio fixes this wrapper’s branch. The real condition |x|>1 should not be imposed on general complex inputs; points on the intervening real segment involve complex boundary values rather than a finite real inverse.',
        ('Re(arcoth(2+0i))', 'approximately 0.5493061443', 'To the right of the cut, the ratio is positive and the complex result is ln(3)/2.')),
    'arcsch': (r'\operatorname{arcsch}z=\operatorname{Log}\!\left(1/z+\sqrt{1/z^2+1}\right)',
        'The reciprocal is formed before the principal square root and logarithm. Zero is singular. The implementation evaluates this expression directly, rather than forwarding to the separate arsinh routine with its formula defect.',
        ('Re(arcsch(2+0i))', 'approximately 0.4812118251', 'At a positive real point away from cuts this is ln((1+sqrt(5))/2).')),
    'arsech': (r'\operatorname{arsech}z=\operatorname{Log}\!\left(1/z+\sqrt{1/z+1}\sqrt{1/z-1}\right)',
        'This is the principal arcosh construction applied to 1/z. The two roots keep their individual principal branches and z=0 is excluded. Real inputs between zero and one recover the nonnegative real inverse.',
        ('Re(arsech(0.5+0i))', 'approximately 1.3169578969', 'The reciprocal is two, reducing this example to principal arcosh(2).')),
}
