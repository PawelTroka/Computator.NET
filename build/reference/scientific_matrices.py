"""Existing matrix API: definitions, ownership and explicit wrapper caveats."""
from common import A, SOURCE, MATHNET

def matrix(name,args,sig):
    url=SOURCE+'Computator.NET.Core/Functions/MatrixFunctions.cs'
    generic='For generic numeric matrix creation use T=real (double) or T=complex. The wrapper’s attempted cast for other numeric T can return null; the generic-looking name is not a guarantee of all scalar types.'
    if name in ('matrix','vector'):
        if name=='matrix' and args=='array':
            return A('Dense matrix from a rectangular array','Copies a rectangular two-dimensional array into a Math.NET real or complex dense matrix.',
                     'M[row,column] corresponds to array[row,column]; indices start at zero.',
                     'Integer arrays become real matrices; real and complex arrays retain that numerical domain. The dynamic array overload chooses complex if it finds a complex element.',
                     'Dynamic elements must be convertible to the chosen scalar type. A matrix is not a jagged array; rows must have consistent lengths.',url=url,category='Matrix construction')
        if 'elements' in args:
            return A('Dense vector from elements','Constructs a real or complex Math.NET column vector from the listed elements.',
                     'vector(a,b,c) has length 3.', 'Pass scalar values of a compatible numerical type; real and complex overloads share the insertion signature.',
                     'The returned vector is mutable; do not confuse a length argument with the single-element params overload.',url=url,category='Matrix construction')
        return A('Zero-filled '+name,'Allocates a dense '+name+' of the requested size.',
                 'matrix<T>(n,m): n rows, m columns; vector<T>(n): n elements.',
                 'Nonnegative dimensions within available memory; library-specific zero-size constraints apply.',generic,url=url,category='Matrix construction')
    if name in ('IdentityMatrix','OnesMatrix','ZerosMatrix'):
        word={'IdentityMatrix':'diagonal ones','OnesMatrix':'all ones','ZerosMatrix':'all zeros'}[name]
        return A({'IdentityMatrix':'Identity-pattern matrix','OnesMatrix':'All-ones matrix','ZerosMatrix':'Zero matrix'}[name],
                 'Creates a new dense matrix with '+word+'.',
                 'M[i,j]=1 when i=j, and 0 otherwise.' if name=='IdentityMatrix' else 'Every entry equals '+('1.' if name=='OnesMatrix' else '0.'),
                 'n,m are row/column counts; the M overload copies the SHAPE of M, not its values.',
                 'A rectangular identity-pattern matrix is not a square multiplicative identity.' if name=='IdentityMatrix' else 'Allocation produces a new matrix and does not overwrite the source M.',url=url,category='Matrix construction')
    if name in ('array','list'):
        params='elements' in args;typed='<T>' in sig
        return A(('Typed ' if typed else 'Dynamic ')+('fixed-size array' if name=='array' else 'resizable list'),
                 ('Collects the supplied elements in order.' if params else 'Creates an empty list.' if name=='list' else 'Allocates n entries initialized to the scalar/reference default.'),
                 '', 'Use zero-based indexing. '+('The type parameter specifies the element type.' if typed else 'Elements are dynamic objects; numerical matrix operations require a separate matrix conversion.'),
                 ('The params-array overload returns the supplied array; it is not necessarily an independent copy.' if params and name=='array' else 'Length n must be nonnegative.' if name=='array' else 'A list can grow; its Count is not a matrix row count.'),url=url,category='Collections')
    basics={
      'Tr':('Matrix trace','Sums the diagonal entries of a square matrix.','tr(M)=ΣᵢMᵢᵢ','Square matrix; result has scalar type T.',''),
      'det':('Matrix determinant','The signed/complex volume factor of a square linear map.','det(AB)=det(A)det(B)','Square matrix; a zero determinant indicates singularity.','Floating-point near-singularity should be assessed with conditioning or rank, not exact comparison alone.'),
      'rank':('Numerical matrix rank','Estimates the number of linearly independent columns/rows.','rank(M)≤min(rows,columns)','A real or complex matrix; numerical rank depends on a singular-value threshold.','Not an exact symbolic rank for nearly dependent data.'),
      'ker':('Kernel or null-space basis','Returns basis vectors annihilated by the matrix.','M·v≈0 for every returned basis vector v.','Real or complex matrix; returns an array of vectors, possibly empty.','The basis is not unique and numerical rank thresholds affect its dimension.'),
      'inv':('Matrix inverse','Returns the inverse of a nonsingular square matrix.','M·inv(M)≈I','Square nonsingular matrix.','For a linear system, solving directly is generally preferable to explicitly forming the inverse; ill-conditioning amplifies error.'),
      'transpose':('Matrix transpose','Exchanges row and column indices without conjugating complex values.','(Mᵀ)ᵢⱼ=Mⱼᵢ','Any compatible rectangular matrix.','Use HermitianTranspose for complex conjugation as well.'),
      'HermitianTranspose':('Conjugate transpose','Transposes the matrix and conjugates each complex entry.','(Mᴴ)ᵢⱼ=conjugate(Mⱼᵢ)','Real input reduces to an ordinary transpose.',''),
      'minor':('Rectangular submatrix — legacy minor name','Extracts a contiguous block, not a determinant or cofactor.','Rows i,…,i+m−1; columns j,…,j+n−1.','i,j are zero-based starting indices; m,n are row/column counts within M.','Despite the name, this does not remove a row/column and does not compute a signed minor.'),
      'pow':('Integer matrix power','Raises a square matrix to an integer power using matrix multiplication.','M⁰=I; M⁻ⁿ=(M⁻¹)ⁿ for n>0.','Square M; negative n requires invertibility. Exclude the most-negative 32-bit integer because Math.Abs(n) overflows.','This is not an elementwise power.'),
      'solve':('Solve an augmented linear system','Treats the last column of M as the right-hand side and the preceding columns as the coefficient matrix.','For M=[A|b], returns x solving A·x≈b.','At least two columns; dimensions/rank must suit the underlying Math.NET solver.','Do not pass just A and expect an implicit right-hand side. Rectangular or ill-conditioned systems follow Math.NET solver behavior.'),
      'isSymmetric':('Symmetry predicate','Tests equality with the ordinary transpose.','M=Mᵀ','A matrix to compare with its transpose.','For complex matrices this is symmetry, not Hermitian symmetry.'),
      'isHermitian':('Hermitian-symmetry predicate','Tests equality with the conjugate transpose.','M=Mᴴ','A matrix to compare with its conjugate transpose.','Hermitian matrices have real diagonal entries; floating-point equality follows Math.NET’s predicate.'),
      'isIndentity':('Approximate identity-pattern predicate','Compares the matrix with an identity-pattern matrix of the same shape.','M≈IdentityMatrix(M)','Uses relative comparison tolerance 1e−10.','The public identifier is spelled isIndentity. Rectangular diagonal-one matrices can also pass; this does not enforce squareness.'),
      'isUnitary':('Approximate row-unitarity predicate','Checks the product with the conjugate transpose against identity.','M·Mᴴ≈I, using the isIndentity tolerance 1e−10.','A square matrix satisfying this condition is unitary (orthogonal when real).','The wrapper does not require a square matrix, so rectangular matrices with orthonormal rows can also pass.'),
      'BitwiseDotProduct':('Bit-vector dot product','Counts bit positions where both signed 32-bit integer representations contain a one.','popcount(x AND y)','Uses all 32 bits, including the sign bit of negative integers.','This returns an integer count, NOT the parity-only dot product over the field with two elements.')
    }
    if name in basics:
        t,d,f,a,n=basics[name];return A(t,d,f,a,n,url=url,category='Linear algebra')
    if name in ('LU','QR','Cholesky','Evd','Svd','GramSchmidt'):
        d,f,a={
          'LU':('Pivoted lower/upper triangular factorization.','A = P·L·U, subject to the factorization object’s permutation convention.','Square matrix for the usual LU solve/determinant operations.'),
          'QR':('Orthogonal/unitary–triangular factorization.','A=Q·R','Rectangular real or complex matrix; Q has orthogonal/unitary columns as provided by the selected Math.NET factorization.'),
          'Cholesky':('Triangular factorization of a positive-definite matrix.','A=L·Lᴴ','Square symmetric/Hermitian positive-definite matrix; nonsymmetric or nonpositive-definite input is not admissible.'),
          'Evd':('Eigenvalue decomposition.','A·V≈V·D','Square matrix. Eigenvalues may be complex even for a real nonsymmetric matrix.'),
          'Svd':('Singular value decomposition.','A=U·Σ·Vᴴ','Any supported rectangular matrix; singular values are real and nonnegative.'),
          'GramSchmidt':('QR factorization through Gram–Schmidt orthogonalization.','A=Q·R','Use linearly independent columns for a well-conditioned factorization; near dependence can lose orthogonality.')
        }[name]
        return A(name+' factorization object',d,f,a,'Returns a factorization object exposing factors and solve/diagnostic operations; it is not just one factor matrix.',url=MATHNET+'MathNet.Numerics.LinearAlgebra.Factorization/index.htm',category='Matrix factorizations')
    if name in ('KroneckerProduct','KroneckerSum','DirectSum','PointwiseMultiply','PointwiseDivide'):
        t,d,f,n={
          'KroneckerProduct':('Kronecker tensor product','Forms a block matrix by replacing each entry of the left operand with that entry times the right operand.','(A⊗B) block (i,j)=AᵢⱼB.','At least one compatible matrix; folds left to right. Matrix dimensions multiply, so memory grows quickly.'),
          'KroneckerSum':('Kronecker sum','Combines independent linear operators on a tensor-product space.','A⊕B=A⊗I+I⊗B.','Use square matrices for the standard definition; folds left to right. This is not the block-diagonal direct sum.'),
          'DirectSum':('Block-diagonal direct sum','Places matrices along a larger matrix’s diagonal with zeros elsewhere.','diag(A,B,…)','Input blocks can be rectangular. Row and column counts add; a new dense matrix is allocated.'),
          'PointwiseMultiply':('Elementwise multiplication — first-operand caveat','Intended to multiply equally shaped matrices entry by entry.','Intended: resultᵢⱼ=AᵢⱼBᵢⱼ…','The current loop starts with a copy of A and multiplies from index 0, so A is included TWICE: A²⊙B⊙…. A single argument is squared. This is not ordinary matrix multiplication.'),
          'PointwiseDivide':('Elementwise division — first-operand caveat','Intended to divide equally shaped matrices entry by entry.','Intended: resultᵢⱼ=Aᵢⱼ/Bᵢⱼ/…','The current loop starts with a copy of A and divides from index 0, giving (A/A)⊘B⊘…. Zeros in A may already cause invalid values. This is not matrix inverse multiplication.')
        }[name]
        return A(t,d,f,'Nonempty inputs for indexed left-fold implementations; elementwise operations require equal dimensions.',n,url=url,category='Matrix operations')
    raise KeyError('Matrix '+sig)
