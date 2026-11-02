"""The qualified, pinned Math.NET Euclid surface; no replacement algorithms."""
from common import A

SOURCE = 'https://github.com/mathnet/mathnet-numerics/blob/v5.0.0/src/Numerics/Euclid.cs'


def describe(item, values):
    signature = item['signature']
    name = signature.split('(')[0].split('.')[-1]
    collection = '(integers' in signature
    big = 'BigInteger' in item['menu']
    integer = 'Arbitrary-precision BigInteger values.' if big else 'Signed integers; the pair overload uses 64-bit integer values.'

    if name == 'GreatestCommonDivisor':
        note = ('The two-value operation accepts negative numbers and zeros: gcd(a,0)=|a| and gcd(0,0)=0. '
                'The returned value is an integer, so scripts can retain values above the exact range of real numbers.')
        if collection:
            note = ('The empty collection returns 0; null throws ArgumentNullException. '
                    'Math.NET 5.0 starts its reduction at the first value and stops while its accumulated gcd is 0 or 1. '
                    'A leading zero therefore incorrectly returns 0 even when later values are nonzero. '
                    'Remove leading zeros or explicitly fold the two-value overload, starting from 0.')
        if not big:
            note += ' Extreme 64-bit inputs can throw OverflowException, including an unrepresentable positive gcd of 2^63. Use BigInteger for the full integer domain.'
        return A('Greatest common divisor of integers',
                 'Finds the nonnegative common factor that divides every supplied integer.',
                 'gcd(84,30)=6; gcd(a,b)=gcd(b,a remainder b).', integer,
                 note, 'Euclid.GreatestCommonDivisor(-84,30) = 6', SOURCE, 'Integer arithmetic')

    if name == 'LeastCommonMultiple':
        note = ('A zero operand gives 0; signs are removed from the result. '
                'For nonzero operands this is the smallest positive shared multiple, useful for combining repeating integer periods.')
        if collection:
            note += ' The empty collection returns 1 and null throws ArgumentNullException. Collection overloads reduce from the first value.'
        if not big:
            note += (' The 64-bit implementation divides before multiplying, but a large product can still silently wrap or throw. '
                     'Use BigInteger when the result or intermediate arithmetic may exceed signed 64-bit range.')
        return A('Least common multiple of integers',
                 'Finds the smallest positive common multiple of nonzero integers, with zero returned when any operand is zero.',
                 'lcm(a,b)=|a/gcd(a,b) times b| for nonzero a,b.', integer,
                 note, 'Euclid.LeastCommonMultiple(12,18) = 36', SOURCE, 'Integer arithmetic')

    if name == 'ExtendedGreatestCommonDivisor':
        note = ('Declare both coefficient variables first and pass them with out: '
                'integer u,v; integer d=Euclid.ExtendedGreatestCommonDivisor(240,46,out u,out v); '
                'then 240*u+46*v equals d=2. Use System.Numerics.BigInteger variables for the BigInteger overload. '
                'The coefficient pair is not unique; check its identity, not a particular pair of coefficients.')
        if not big:
            note += ' Extreme signed 64-bit intermediates can overflow or wrap; choose BigInteger for exact large calculations.'
        return A('Extended Euclidean algorithm',
                 'Returns a greatest common divisor and writes two Bezout coefficients to the out arguments.',
                 'a*x+b*y=gcd(a,b).', integer+' The final x,y arguments are output variables, not evaluation coordinates.',
                 note, '', SOURCE, 'Integer arithmetic')

    if name in ('Modulus', 'Remainder'):
        modulus = name == 'Modulus'
        note = ('For exact arithmetic and a nonzero divisor, modulus has the divisor sign; remainder has the dividend sign. '
                'For example, with dividend -7 and divisor 3 the modulus is 2 while the remainder is -1. '
                'Both are zero when the dividend is a multiple of the divisor. '
                'A zero integer divisor throws DivideByZeroException; a zero floating divisor produces NaN.')
        if modulus:
            note += (' Math.NET 5.0 computes ((dividend%divisor)+divisor)%divisor. '
                     'The added divisor can overflow fixed-width integer or floating arithmetic, and floating addition can round away a small remainder. '
                     'Use BigInteger for exact integer modulus at large magnitudes.')
        else:
            note += ' Fixed-width signed minimum divided by -1 can overflow. Floating inputs follow the runtime % operator, including its rounding and nonfinite behavior.'
        return A('Canonical modulus' if modulus else 'Truncating division remainder',
                 'Returns the residue using the divisor-sign convention.' if modulus else 'Returns the same remainder as the % operator.',
                 'Modulus(-7,3)=2; Modulus(7,-3)=-2.' if modulus else 'Remainder(-7,3)=-1; Remainder(7,-3)=1.',
                 'dividend and a nonzero divisor of matching supported numeric types; overloads accept int, integer, real, Single and BigInteger.',
                 note, 'Euclid.'+name+'(-7,3) = '+('2' if modulus else '-1'), SOURCE, 'Integer arithmetic')

    data = {
        'IsEven': ('Even-integer predicate', 'Tests whether an integer is divisible by two.',
                   'number=2*k for some integer k.', 'Signed 32-bit or 64-bit integer.',
                   'Zero and negative even numbers return true. The result is bool, suitable for if statements or conditional expressions.',
                   'Euclid.IsEven(-2) = true'),
        'IsOdd': ('Odd-integer predicate', 'Tests whether an integer has an odd remainder on division by two.',
                  'number=2*k+1 for some integer k.', 'Signed 32-bit or 64-bit integer.',
                  'Negative odd numbers return true; zero returns false. The result is bool.',
                  'Euclid.IsOdd(-3) = true'),
        'IsPowerOfTwo': ('Positive power-of-two predicate', 'Tests whether a positive integer contains exactly one set binary bit.',
                         'number=2^k for an integer k>=0.', 'Signed 32-bit or 64-bit integer.',
                         'One returns true. Zero and every negative value return false. The bool result is useful when selecting binary grid or transform sizes.',
                         'Euclid.IsPowerOfTwo(1024) = true'),
        'IsPerfectSquare': ('Perfect-square predicate', 'Tests whether a nonnegative integer is the square of an integer.',
                           'number=k*k for some integer k.', 'Signed 32-bit or 64-bit integer.',
                           'Zero is a perfect square; negative inputs return false. This is an integer test, not an approximate test for real values near a square.',
                           'Euclid.IsPerfectSquare(144) = true'),
        'PowerOfTwo': ('Exact integer power of two', 'Returns a representable positive integer power of two.',
                       'result=2^exponent.', 'The int overload accepts exponents 0..30; the integer (64-bit) overload accepts 0..62.',
                       'Out-of-range exponents throw ArgumentOutOfRangeException. An ordinary small integer literal selects int; use (integer)40 to select the 64-bit overload. The result stays an integer in scripts.',
                       'Euclid.PowerOfTwo((integer)40) = 1099511627776'),
        'Log2': ('Integer binary logarithm', 'Returns the position of the highest set bit of a positive 32-bit integer.',
                 'For number>0, result=floor(log2(number)).', 'A positive int (32-bit integer); this method has no 64-bit overload.',
                 'This integer operation rounds down. Math.NET 5.0 returns 0 for zero and 31 for negative inputs; those implementation values are not mathematical logarithms. Validate number>0 when that meaning is required.',
                 'Euclid.Log2(1000) = 9'),
        'CeilingToPowerOfTwo': ('Round up to a power of two', 'Finds the smallest representable power of two at least as large as a positive integer.',
                                'For number>0, result=2^ceil(log2(number)).', 'Positive int up to 2^30 or positive integer (64-bit) up to 2^62.',
                                'An exact power of two is unchanged. Larger positive inputs throw ArgumentOutOfRangeException. Math.NET 5.0 returns 0 for zero and negative inputs, so validate positivity before treating the result as a power of two.',
                                'Euclid.CeilingToPowerOfTwo(9) = 16')
    }
    title, definition, formula, arguments, note, example = data[name]
    return A(title, definition, formula, arguments, note, example, SOURCE, 'Integer arithmetic')


def teaching(name):
    if name in ('GreatestCommonDivisor', 'LeastCommonMultiple', 'ExtendedGreatestCommonDivisor'):
        return ('Keep exact integer results in integer or System.Numerics.BigInteger variables in scripts. '
                'Fixed-width integer results convert to real in the calculator; above 2^53 that conversion can round an otherwise exact integer. '
                'BigInteger results require an explicit conversion, with the whole cast expression parenthesized: '
                '((real)Euclid.GreatestCommonDivisor(System.Numerics.BigInteger.One<<80,System.Numerics.BigInteger.Zero)). '
                'Use a script instead when the exact integer value is required. '
                'The qualified Euclid methods are separate from the existing gcd, nwd, lcm and nww convenience functions.')
    if name in ('Modulus', 'Remainder'):
        return ('Use canonical modulus when wrapping a signed position into a periodic range, with positive divisor for nonnegative results. '
                'Use remainder when matching the language division convention; the two operations differ for operands with opposite signs.')
    return ('Use these integer predicates and size calculations before allocating a numerical grid or selecting an algorithm. '
            'A bool result belongs in a script condition or a conditional expression such as Euclid.IsEven(4) ? 1 : 0.')
