"""Small, worked matrix calculations using the public TSL spelling.

An observed entry, norm or dimension keeps the answer scalar and independently
checkable in the reference-example execution suite. The wrappers are available
in the scripting editor, so generation presents complete writeln statements.
These examples preserve the current contract, including legacy defects.
"""
from enrichment import WorkedExample


def scripting_example(code, result, explanation):
    return WorkedExample(code, result, explanation, editor='scripting')


OPERATIONS = {
    'det': ('det(matrix({{3,1},{1,2}}))', '5',
            'For this two-by-two matrix, ad−bc=3·2−1·1=5. The nonzero determinant shows that these two column directions are independent.'),
    'Tr': ('Tr(matrix({{1,2},{3,4}}))', '5',
           'Trace adds only the diagonal entries, 1+4. The sum of all four entries would be ten and is a different operation.'),
    'inv': ('inv(matrix({{3,1},{1,2}}))[0,0]', '0.4',
            'The inverse is (1/5) times the matrix with rows [2,−1] and [−1,3]. Zero-based entry [0,0] is therefore 2/5.'),
    'transpose': ('transpose(matrix({{1,2,3},{4,5,6}}))[2,0]', '3',
                  'The original two-by-three matrix becomes three-by-two. Entry [0,2], which is three, moves to [2,0].'),
    'HermitianTranspose': ('Im(HermitianTranspose(matrix({{1+2*i}}))[0,0])', '-2',
                           'The one-by-one matrix needs no visible index exchange, so this isolates conjugation: 1+2i becomes 1−2i. Its imaginary coordinate is −2.'),
    'rank': ('rank(matrix({{1,2},{2,4}}))', '1',
             'The second column is twice the first, so there is one independent column direction. This exact small example avoids ambiguity about a near-singular tolerance.'),
    'ker': ('ker(matrix({{1,2},{2,4}})).Length', '1',
            'Both equations reduce to x+2y=0, whose solutions are multiples of (−2,1). One basis vector spans the kernel; the library may return a differently normalized vector.'),
    'solve': ('solve(matrix({{3,1,9},{1,2,8}}))[1]', '3',
              'The last column supplies the right-hand side: 3x+y=9 and x+2y=8. Substitution gives x=2 and y=3; index one selects y.'),
    'pow': ('pow(matrix({{1,1},{0,1}}),3)[0,1]', '3',
            'Write the matrix as I+N, where N²=0. Then (I+N)³=I+3N and entry [0,1] is three. An elementwise cube would leave that entry equal to one.'),
    'LU': ('LU(matrix({{0,1},{2,3}})).Determinant', '-2',
           'The zero leading entry requires a row pivot. The determinant remains 0·3−1·2=−2; ignoring the sign of the permutation would give the wrong answer.'),
    'QR': ('abs(QR(matrix({{3,0},{4,2}})).R[0,0])', '5',
           'The first column has Euclidean length sqrt(3²+4²)=5, which is the magnitude of the first diagonal coefficient in R. QR sign conventions can change its sign without changing A=QR.'),
    'GramSchmidt': ('abs(GramSchmidt(matrix({{3,0},{4,2}})).R[0,0])', '5',
                    'Normalizing the first column (3,4) extracts length five. Later columns have their projection on that direction removed before normalization.'),
    'Cholesky': ('Cholesky(matrix({{4,2},{2,3}})).Factor[1,1]', 'approximately 1.414213562373',
                 'The lower factor starts with L00=sqrt(4)=2 and L10=2/2=1. The remaining diagonal is sqrt(3−1²)=sqrt(2); multiplying L by its transpose reconstructs A.'),
    'Svd': ('Svd(matrix({{3,0},{0,2}})).S[0]', '3',
            'This diagonal map stretches the two coordinate axes by three and two. Singular values are returned in decreasing order, so the largest stretch is three.'),
    'Evd': ('Tr(Evd(matrix({{2,0},{0,3}})).D)', '5',
            'The coordinate axes are eigenvectors with eigenvalues two and three. Summing the diagonal eigenvalue matrix gives five without depending on the order of the returned eigenpairs.'),
    'isSymmetric': ('(isSymmetric(matrix({{1,2},{2,3}})) ? 1 : 0)', '1',
                    'The off-diagonal entries match after transposition. The predicate is true; the conditional expresses that result as the number one.'),
    'isHermitian': ('(isHermitian(matrix({{1,i},{-i,2}})) ? 1 : 0)', '1',
                    'The off-diagonal entries i and −i are conjugates and the diagonal is real. Conjugate transposition reproduces the matrix; ordinary transposition does not. The conditional displays true as one.'),
    'isIndentity': ('(isIndentity(matrix({{1,0,0},{0,1,0}})) ? 1 : 0)', '1',
                    'The wrapper accepts this rectangular diagonal-one pattern. The numeric one means the predicate returned true; it does not make the two-by-three matrix a square identity.'),
    'isUnitary': ('(isUnitary(matrix({{1,0,0},{0,1,0}})) ? 1 : 0)', '1',
                  'The two rows have unit length and zero mutual inner product, so A·Aᴴ is a two-by-two identity. The wrapper returns true even though A is rectangular; Aᴴ·A is not a three-by-three identity.'),
    'KroneckerProduct': ('KroneckerProduct(matrix({{1,2}}),matrix({{3,4},{5,6}}))[1,3]', '12',
                         'The one-by-two matrix creates side-by-side blocks B and 2B. The result has two rows and four columns; its last entry is 2·6=12.'),
    'KroneckerSum': ('KroneckerSum(matrix({{1,0},{0,2}}),matrix({{3,0},{0,4}}))[3,3]', '6',
                     'For diagonal factors, the new diagonal contains all pairwise sums: 1+3, 1+4, 2+3 and 2+4. The last is six.'),
    'DirectSum': ('DirectSum(matrix({{1,2}}),matrix({{3},{4}}))[2,2]', '4',
                  'A one-by-two block and a two-by-one block give a three-by-three result. The second block begins at row one, column two, placing its final value four at [2,2].'),
    'PointwiseMultiply': ('PointwiseMultiply(matrix({{2}}),matrix({{3}}))[0,0]', '12',
                          'The current wrapper starts with two and processes its first operand again: 2·2·3=12. The intended two-operand elementwise product would be six. This example exposes the documented defect rather than concealing it.'),
    'PointwiseDivide': ('PointwiseDivide(matrix({{2}}),matrix({{4}}))[0,0]', '0.25',
                        'The current wrapper evaluates 2/2/4=1/4 because it repeats the first operand. The intended quotient 2/4 would be one half. A zero first entry would introduce 0/0 at the first step.'),
    'minor': ('minor(matrix({{1,2,3},{4,5,6}}),0,1,2,2)[1,0]', '5',
              'Start at row zero, column one and take two rows and two columns. The extracted block has rows [2,3] and [5,6], so its entry [1,0] is five. No determinant is computed.'),
    'BitwiseDotProduct': ('BitwiseDotProduct(6,3)', '1',
                          'Six has binary digits 110 and three has 011. Their shared set bits form 010, which contains one set bit. Counting differs from multiplying the integers.'),
}


def examples_for(signature, base):
    if base in OPERATIONS:
        return (scripting_example(*OPERATIONS[base]),)
    if base in ('IdentityMatrix', 'OnesMatrix', 'ZerosMatrix'):
        shaped = signature.endswith('(M)')
        call = base+'(matrix({{4,5,6},{7,8,9}}))' if shaped else base+'<double>(2,3)'
        answer = '0' if base == 'ZerosMatrix' else '1'
        text = {'IdentityMatrix': 'The two-by-three result has ones at [0,0] and [1,1] and zeros elsewhere. The dimensions specify a rectangular identity pattern, not a two-sided square identity.',
                'OnesMatrix': 'Every entry is one, including the selected entry [1,1]. This is an all-ones matrix, whose off-diagonal entries differ from an identity pattern.',
                'ZerosMatrix': 'Every entry is zero, including [1,1], while the allocated shape remains two-by-three.'}[base]
        if shaped:
            text += ' The argument contributes only its shape; its original entry eight is not copied.'
        return (scripting_example(call+'[1,1]', answer, text),)
    if base == 'matrix':
        if '<' in signature:
            return (scripting_example('matrix<double>(2,3).ColumnCount', '3',
                    'The arguments give two rows and three columns. The generic type selects real storage; all six entries initially contain zero.'),)
        return (scripting_example('matrix({{1,2,3},{4,5,6}})[1,2]', '6',
                'The outer rows contain three columns each. Zero-based row one and column two select the last value, six; a ragged set of rows is not a rectangular matrix.'),)
    if base == 'vector':
        if '<' in signature:
            return (scripting_example('vector<double>(3).Count', '3',
                    'The explicit generic size overload allocates three zero entries. In contrast, vector(3.0) constructs a vector containing the single value three.'),)
        return (scripting_example('vector(3.0,4.0).L2Norm()', '5',
                'These are two elements, not row and column dimensions. The Euclidean vector length is sqrt(3²+4²)=5.'),)
    if base == 'array':
        if 'elements' in signature:
            return (scripting_example('array<double>(2,4,6)[1]', '4',
                    'Three supplied elements form an array in that order. Index one selects the second value, four.'),)
        call = 'array<double>(3)' if '<' in signature else 'array(3)'
        return (scripting_example(call+'.Length', '3',
                'The integer argument selects the size overload and allocates three slots; it is not an element value. For a single real element, array<double>(3.0) explicitly selects the params overload.'),)
    if base == 'list':
        if 'elements' in signature:
            return (scripting_example('list<double>(2,4,6)[1]', '4',
                    'The initial list contains three values in order, and index one retrieves four. Additional elements can later be appended without changing this indexing convention.'),)
        call = 'list<double>()' if '<' in signature else 'list()'
        return (scripting_example(call+'.Count', '0',
                'Constructing a list with no elements gives count zero. It can grow later; this constructor does not allocate a vector of numeric zeros.'),)
    raise KeyError('No reviewed matrix example for '+signature)
