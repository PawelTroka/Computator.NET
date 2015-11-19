// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseStringInterpolation

//using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using Computator.NET.Evaluation;//this one is neeeded!!!!!!!!!!!!!!!!!!!!!!! dont remove it!!!!!

namespace Computator.NET.Functions
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public static class MatrixFunctions
    {
        #region matrix creation

        public static MathNet.Numerics.LinearAlgebra.Complex.DenseVector vector(
    params System.Numerics.Complex[] elements)
        {

            return new MathNet.Numerics.LinearAlgebra.Complex.DenseVector(elements);
        }

        public static MathNet.Numerics.LinearAlgebra.Double.DenseVector vector(params double[] elements)
        {
            return new MathNet.Numerics.LinearAlgebra.Double.DenseVector(elements);
        }

        public static MathNet.Numerics.LinearAlgebra.Vector<T> vector<T>(int n)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            if (typeof(T) == typeof(double) || ScriptingExtensions.IsNumericType(typeof(T)))
                return
                    new MathNet.Numerics.LinearAlgebra.Double.DenseVector(n) as MathNet.Numerics.LinearAlgebra.Vector<T>;
            if (typeof(T) == typeof(System.Numerics.Complex))
                return
                    new MathNet.Numerics.LinearAlgebra.Complex.DenseVector(n) as
                        MathNet.Numerics.LinearAlgebra.Vector<T>;
            throw new System.ArgumentException("Wrong type for vector creation, consider using real or complex");
        }

        /*public static MathNet.Numerics.LinearAlgebra.Matrix<dynamic> matrix(int n, int m)
{
    MathNet.Numerics.LinearAlgebra.Matrix<dynamic>.Build.Dense(n, m);
}

public static MathNet.Numerics.LinearAlgebra.Matrix<dynamic> matrix(dynamic[,] array)
{
    return MathNet.Numerics.LinearAlgebra.CreateMatrix.DenseOfArray(array);
}*/


        public static MathNet.Numerics.LinearAlgebra.Matrix<T> matrix<T>(int n, int m)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            if (typeof(T) == typeof(double) || ScriptingExtensions.IsNumericType(typeof(T)))
                return
                    new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(n, m) as
                        MathNet.Numerics.LinearAlgebra.Matrix<T>;
            if (typeof(T) == typeof(System.Numerics.Complex))
                return
                    new MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix(n, m) as
                        MathNet.Numerics.LinearAlgebra.Matrix<T>;
            throw new System.ArgumentException("Wrong type for matrix creation, consider using real or complex");
        }

        public static MathNet.Numerics.LinearAlgebra.Double.DenseMatrix matrix(string str)
        {
            if(str.Contains("{")&& str.Contains("}"))
                return MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.OfArray(Accord.Math.Matrix.Parse(str, Accord.Math.CSharpMatrixFormatProvider.InvariantCulture));
            else if (str.Contains("[") && str.Contains("]"))
                return MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.OfArray(Accord.Math.Matrix.Parse(str, Accord.Math.OctaveMatrixFormatProvider.InvariantCulture));
            else
                return MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.OfArray(Accord.Math.Matrix.Parse(str, Accord.Math.DefaultMatrixFormatProvider.InvariantCulture));
        }

        public static MathNet.Numerics.LinearAlgebra.Double.DenseMatrix matrix(double[,] array)
        {
            return MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.OfArray(array);
        }

        public static MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix matrix(System.Numerics.Complex[,] array)
        {
            return MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix.OfArray(array);
        }

        public static MathNet.Numerics.LinearAlgebra.Double.DenseMatrix matrix(int[,] array)
        {
            var darray = new double[array.GetLength(0), array.GetLength(1)];

            for (var j = 0; j < array.GetLength(0); j++)
                for (var k = 0; k < array.GetLength(1); k++)
                    darray[j, k] = array[j, k];

            return MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.OfArray(darray);
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> IdentityMatrix<T>(int n, int m)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.DenseIdentity(n, m);
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> IdentityMatrix<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.DenseIdentity(M.RowCount, M.ColumnCount);
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> ZerosMatrix<T>(int n, int m)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.Dense(n, m, MathNet.Numerics.LinearAlgebra.Matrix<T>.Zero);
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> OnesMatrix<T>(int n, int m)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.Dense(n, m, MathNet.Numerics.LinearAlgebra.Matrix<T>.One);
        }


        public static MathNet.Numerics.LinearAlgebra.Matrix<T> ZerosMatrix<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.Dense(M.RowCount, M.ColumnCount, MathNet.Numerics.LinearAlgebra.Matrix<T>.Zero);
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> OnesMatrix<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.Dense(M.RowCount, M.ColumnCount, MathNet.Numerics.LinearAlgebra.Matrix<T>.One);
        }

        #endregion

        #region matrix specific functions

        public static T Tr<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            //if (M.RowCount != M.ColumnCount)
            ////throw new DimensionMismatchException("It's imposible to calculate trace of non-square matrix!");
            return M.Trace();
        }

        public static int rank<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return M.Rank();
        }

        public static T det<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            //if (M.RowCount != M.ColumnCount)
            ////throw new DimensionMismatchException("It's imposible to calculate determinant of non-square matrix!");
            return M.Determinant();
        }

        public static bool isSymmetric<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return M.IsSymmetric();
        }

        public static bool isHermitian<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return M.IsHermitian();
        }

        public static bool isIndentity<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return MathNet.Numerics.Precision.AlmostEqualRelative(M, IdentityMatrix(M), 1e-10);
        }


        public static bool isUnitary<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return isIndentity(M * M.ConjugateTranspose());
        }

        #endregion

        #region matrix specific operations

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> minor<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M,
            int i, int j, int m, int n)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return M.SubMatrix(i, m, j, n);
        }

        public static MathNet.Numerics.LinearAlgebra.Vector<T>[] ker<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
    where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return M.Kernel();
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> pow<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M, int n)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            if (M.RowCount != M.ColumnCount)
                throw new System.ArgumentException("It's imposible to take non-square matrix to power!");
            var M2 = M.SubMatrix(0, M.RowCount, 0, M.ColumnCount);
            if (n == 0)
            {

                for (var j = 0; j < M.RowCount; j++)
                    for (var k = 0; k < M.ColumnCount; k++)
                    {
                        if (j == k)
                            M2[j, k] = MathNet.Numerics.LinearAlgebra.Matrix<T>.One;
                        else
                            M2[j, k] = MathNet.Numerics.LinearAlgebra.Matrix<T>.Zero;
                    }
                return M2;
            }
            if (n < 0)
                M2 = M2.Inverse();
            //for (int j = 1; j < Math.Abs(n); j++)
            // M = M * M;
            return M2.Power(System.Math.Abs(n));
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> inv<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            //if (M.RowCount != M.ColumnCount)
            //throw new DimensionMismatchException("It's imposible to calculate inverse matrix of non-square matrix!");
            return M.Inverse();
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> transpose<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return M.Transpose();
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> HermitianTranspose<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
    where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return M.ConjugateTranspose();
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> KroneckerProduct<T>(
            params MathNet.Numerics.LinearAlgebra.Matrix<T>[] mactrices)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            var mxRet  = MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.DenseOfMatrix(mactrices[0]);

            for (var i = 1; i < mactrices.Length; i++)
                mxRet = mxRet.KroneckerProduct(mactrices[i]);
            return mxRet;
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> KroneckerSum<T>(
    params MathNet.Numerics.LinearAlgebra.Matrix<T>[] mactrices)
    where T : struct, System.IEquatable<T>, System.IFormattable
        {
            var resultMatrix = MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.DenseOfMatrix(mactrices[0]);

            for (var i = 1; i < mactrices.Length; i++)
            {
                var idMxRet = MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.DenseIdentity(resultMatrix.RowCount, resultMatrix.ColumnCount);
                var idMxi = MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.DenseIdentity(mactrices[i].RowCount, mactrices[i].ColumnCount);
                resultMatrix = resultMatrix.KroneckerProduct(idMxi) + idMxRet.KroneckerProduct(mactrices[i]);// \mathbf{A} \oplus \mathbf{B} = \mathbf{A} \otimes \mathbf{I}_m + \mathbf{I}_n \otimes \mathbf{B} .
            }

            return resultMatrix;
        }


        //Direct sum

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> DirectSum<T>(
params MathNet.Numerics.LinearAlgebra.Matrix<T>[] matrices)
where T : struct, System.IEquatable<T>, System.IFormattable
        {

            var resultMatrix = MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.Dense(System.Linq.Enumerable.Sum(matrices, el => el.RowCount),
                System.Linq.Enumerable.Sum(matrices, el => el.ColumnCount), MathNet.Numerics.LinearAlgebra.Matrix<T>.Zero);

            var rowDisplacement = 0;

            var columnDisplacement = 0;

            foreach (MathNet.Numerics.LinearAlgebra.Matrix<T> matrix in matrices)
            {
                for (int j = 0; j < matrix.RowCount; j++)
                {
                    for (int k = 0; k < matrix.ColumnCount; k++)
                    {
                        resultMatrix[j + rowDisplacement, k + columnDisplacement] = matrix[j, k];
                    }
                }
                rowDisplacement += matrix.RowCount;
                columnDisplacement += matrix.ColumnCount;
            }

            return resultMatrix;
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> PointwiseMultiply<T>(
            params MathNet.Numerics.LinearAlgebra.Matrix<T>[] matrices)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {

            var resultMatrix = MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.DenseOfMatrix(matrices[0]);

            for (var i = 0; i < matrices.Length; i++)
                resultMatrix.PointwiseMultiply(matrices[i], resultMatrix);

            return resultMatrix;
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> PointwiseDivide<T>(
            params MathNet.Numerics.LinearAlgebra.Matrix<T>[] matrices)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {

            var resultMatrix = MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.DenseOfMatrix(matrices[0]);

            for (var i = 0; i < matrices.Length; i++)
                resultMatrix.PointwiseDivide(matrices[i], resultMatrix);

            return resultMatrix;
        }

        #endregion

        #region matrix utils

        private static byte[] ToBits(int value)
        {
            System.Collections.BitArray b = new System.Collections.BitArray(new int[] { value });

            bool[] bits = new bool[b.Count];
            b.CopyTo(bits, 0);
            byte[] bitValues = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(bits, bit => (byte)(bit ? 1 : 0)));
            return bitValues;
        }

        public static int BitwiseDotProduct(int x, int y)
        {
            var xBits = ToBits(x);
            var yBits = ToBits(y);

            var sum = 0;
            for (int i = 0; i < xBits.Length; i++)
                sum += xBits[i]*yBits[i];
            return sum;
        }

        public static System.Collections.Generic.List<dynamic> list()
        {
            return new System.Collections.Generic.List<dynamic>();
        }

        public static dynamic[] array(int n)
        {
            return new dynamic[n];
        }

        public static System.Collections.Generic.List<T> list<T>()
        {
            return new System.Collections.Generic.List<T>();
        }

        public static System.Collections.Generic.List<T> list<T>(params T[] elements)
        {
            return new System.Collections.Generic.List<T>(elements);
        }

        public static T[] array<T>(int n)
        {
            return new T[n];
        }

        public static T[] array<T>(params T[] elements)
        {
            return elements;
        }

        #endregion

        #region utils

        public const string ToCode =
            @"
        #region matrix creation

        public static MathNet.Numerics.LinearAlgebra.Complex.DenseVector vector(
    params System.Numerics.Complex[] elements)
        {
            return new MathNet.Numerics.LinearAlgebra.Complex.DenseVector(elements);
        }

        public static MathNet.Numerics.LinearAlgebra.Double.DenseVector vector(params double[] elements)
        {
            return new MathNet.Numerics.LinearAlgebra.Double.DenseVector(elements);
        }

        public static MathNet.Numerics.LinearAlgebra.Vector<T> vector<T>(int n)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            if (typeof(T) == typeof(double) || ScriptingExtensions.IsNumericType(typeof(T)))
                return
                    new MathNet.Numerics.LinearAlgebra.Double.DenseVector(n) as MathNet.Numerics.LinearAlgebra.Vector<T>;
            if (typeof(T) == typeof(System.Numerics.Complex))
                return
                    new MathNet.Numerics.LinearAlgebra.Complex.DenseVector(n) as
                        MathNet.Numerics.LinearAlgebra.Vector<T>;
            throw new System.ArgumentException(""Wrong type for vector creation, consider using real or complex"");
        }

        /*public static MathNet.Numerics.LinearAlgebra.Matrix<dynamic> matrix(int n, int m)
{
    MathNet.Numerics.LinearAlgebra.Matrix<dynamic>.Build.Dense(n, m);
}

public static MathNet.Numerics.LinearAlgebra.Matrix<dynamic> matrix(dynamic[,] array)
{
    return MathNet.Numerics.LinearAlgebra.CreateMatrix.DenseOfArray(array);
}*/


        public static MathNet.Numerics.LinearAlgebra.Matrix<T> matrix<T>(int n, int m)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            if (typeof(T) == typeof(double) || ScriptingExtensions.IsNumericType(typeof(T)))
                return
                    new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(n, m) as
                        MathNet.Numerics.LinearAlgebra.Matrix<T>;
            if (typeof(T) == typeof(System.Numerics.Complex))
                return
                    new MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix(n, m) as
                        MathNet.Numerics.LinearAlgebra.Matrix<T>;
            throw new System.ArgumentException(""Wrong type for matrix creation, consider using real or complex"");
        }

        public static MathNet.Numerics.LinearAlgebra.Double.DenseMatrix matrix(string str)
        {
            if(str.Contains(""{"")&& str.Contains(""}""))
                return MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.OfArray(Accord.Math.Matrix.Parse(str, Accord.Math.CSharpMatrixFormatProvider.InvariantCulture));
            else if (str.Contains(""["") && str.Contains(""]""))
                return MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.OfArray(Accord.Math.Matrix.Parse(str, Accord.Math.OctaveMatrixFormatProvider.InvariantCulture));
            else
                return MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.OfArray(Accord.Math.Matrix.Parse(str, Accord.Math.DefaultMatrixFormatProvider.InvariantCulture));
        }

        public static MathNet.Numerics.LinearAlgebra.Double.DenseMatrix matrix(double[,] array)
        {
            return MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.OfArray(array);
        }

        public static MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix matrix(System.Numerics.Complex[,] array)
        {
            return MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix.OfArray(array);
        }

        public static MathNet.Numerics.LinearAlgebra.Double.DenseMatrix matrix(int[,] array)
        {
            var darray = new double[array.GetLength(0), array.GetLength(1)];

            for (var j = 0; j < array.GetLength(0); j++)
                for (var k = 0; k < array.GetLength(1); k++)
                    darray[j, k] = array[j, k];

            return MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.OfArray(darray);
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> IdentityMatrix<T>(int n, int m)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.DenseIdentity(n, m);
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> IdentityMatrix<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.DenseIdentity(M.RowCount, M.ColumnCount);
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> ZerosMatrix<T>(int n, int m)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.Dense(n, m, MathNet.Numerics.LinearAlgebra.Matrix<T>.Zero);
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> OnesMatrix<T>(int n, int m)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.Dense(n, m, MathNet.Numerics.LinearAlgebra.Matrix<T>.One);
        }


        public static MathNet.Numerics.LinearAlgebra.Matrix<T> ZerosMatrix<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.Dense(M.RowCount, M.ColumnCount, MathNet.Numerics.LinearAlgebra.Matrix<T>.Zero);
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> OnesMatrix<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.Dense(M.RowCount, M.ColumnCount, MathNet.Numerics.LinearAlgebra.Matrix<T>.One);
        }

        #endregion

        #region matrix specific functions

        public static T Tr<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            //if (M.RowCount != M.ColumnCount)
            ////throw new DimensionMismatchException(""It's imposible to calculate trace of non-square matrix!"");
            return M.Trace();
        }

        public static int rank<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return M.Rank();
        }

        public static T det<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            //if (M.RowCount != M.ColumnCount)
            ////throw new DimensionMismatchException(""It's imposible to calculate determinant of non-square matrix!"");
            return M.Determinant();
        }

        public static bool isSymmetric<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return M.IsSymmetric();
        }

        public static bool isHermitian<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return M.IsHermitian();
        }

        public static bool isIndentity<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return MathNet.Numerics.Precision.AlmostEqualRelative(M, IdentityMatrix(M), 1e-10);
        }


        public static bool isUnitary<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return isIndentity(M * M.ConjugateTranspose());
        }

        #endregion

        #region matrix specific operations

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> minor<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M,
            int i, int j, int m, int n)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return M.SubMatrix(i, m, j, n);
        }

        public static MathNet.Numerics.LinearAlgebra.Vector<T>[] ker<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
    where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return M.Kernel();
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> pow<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M, int n)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            if (M.RowCount != M.ColumnCount)
                throw new System.ArgumentException(""It's imposible to take non-square matrix to power!"");
            var M2 = M.SubMatrix(0, M.RowCount, 0, M.ColumnCount);
            if (n == 0)
            {

                for (var j = 0; j < M.RowCount; j++)
                    for (var k = 0; k < M.ColumnCount; k++)
                    {
                        if (j == k)
                            M2[j, k] = MathNet.Numerics.LinearAlgebra.Matrix<T>.One;
                        else
                            M2[j, k] = MathNet.Numerics.LinearAlgebra.Matrix<T>.Zero;
                    }
                return M2;
            }
            if (n < 0)
                M2 = M2.Inverse();
            //for (int j = 1; j < Math.Abs(n); j++)
            // M = M * M;
            return M2.Power(System.Math.Abs(n));
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> inv<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            //if (M.RowCount != M.ColumnCount)
            //throw new DimensionMismatchException(""It's imposible to calculate inverse matrix of non-square matrix!"");
            return M.Inverse();
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> transpose<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return M.Transpose();
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> HermitianTranspose<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M)
    where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return M.ConjugateTranspose();
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> KroneckerProduct<T>(
            params MathNet.Numerics.LinearAlgebra.Matrix<T>[] mactrices)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            var mxRet  = MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.DenseOfMatrix(mactrices[0]);

            for (var i = 1; i < mactrices.Length; i++)
                mxRet = mxRet.KroneckerProduct(mactrices[i]);
            return mxRet;
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> KroneckerSum<T>(
    params MathNet.Numerics.LinearAlgebra.Matrix<T>[] mactrices)
    where T : struct, System.IEquatable<T>, System.IFormattable
        {
            var resultMatrix = MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.DenseOfMatrix(mactrices[0]);

            for (var i = 1; i < mactrices.Length; i++)
            {
                var idMxRet = MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.DenseIdentity(resultMatrix.RowCount, resultMatrix.ColumnCount);
                var idMxi = MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.DenseIdentity(mactrices[i].RowCount, mactrices[i].ColumnCount);
                resultMatrix = resultMatrix.KroneckerProduct(idMxi) + idMxRet.KroneckerProduct(mactrices[i]);// \mathbf{A} \oplus \mathbf{B} = \mathbf{A} \otimes \mathbf{I}_m + \mathbf{I}_n \otimes \mathbf{B} .
            }

            return resultMatrix;
        }


        //Direct sum

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> DirectSum<T>(
params MathNet.Numerics.LinearAlgebra.Matrix<T>[] matrices)
where T : struct, System.IEquatable<T>, System.IFormattable
        {

            var resultMatrix = MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.Dense(System.Linq.Enumerable.Sum(matrices, el => el.RowCount),
                System.Linq.Enumerable.Sum(matrices, el => el.ColumnCount), MathNet.Numerics.LinearAlgebra.Matrix<T>.Zero);

            var rowDisplacement = 0;

            var columnDisplacement = 0;

            foreach (MathNet.Numerics.LinearAlgebra.Matrix<T> matrix in matrices)
            {
                for (int j = 0; j < matrix.RowCount; j++)
                {
                    for (int k = 0; k < matrix.ColumnCount; k++)
                    {
                        resultMatrix[j + rowDisplacement, k + columnDisplacement] = matrix[j, k];
                    }
                }
                rowDisplacement += matrix.RowCount;
                columnDisplacement += matrix.ColumnCount;
            }

            return resultMatrix;
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> PointwiseMultiply<T>(
            params MathNet.Numerics.LinearAlgebra.Matrix<T>[] matrices)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {

            var resultMatrix = MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.DenseOfMatrix(matrices[0]);

            for (var i = 0; i < matrices.Length; i++)
                resultMatrix.PointwiseMultiply(matrices[i], resultMatrix);

            return resultMatrix;
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> PointwiseDivide<T>(
            params MathNet.Numerics.LinearAlgebra.Matrix<T>[] matrices)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {

            var resultMatrix = MathNet.Numerics.LinearAlgebra.Matrix<T>.Build.DenseOfMatrix(matrices[0]);

            for (var i = 0; i < matrices.Length; i++)
                resultMatrix.PointwiseDivide(matrices[i], resultMatrix);

            return resultMatrix;
        }

        #endregion

        #region matrix utils

        private static byte[] ToBits(int value)
        {
            System.Collections.BitArray b = new System.Collections.BitArray(new int[] { value });

            bool[] bits = new bool[b.Count];
            b.CopyTo(bits, 0);
            byte[] bitValues = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(bits, bit => (byte)(bit ? 1 : 0)));
            return bitValues;
        }

        public static int BitwiseDotProduct(int x, int y)
        {
            var xBits = ToBits(x);
            var yBits = ToBits(y);

            var sum = 0;
            for (int i = 0; i < xBits.Length; i++)
                sum += xBits[i]*yBits[i];
            return sum;
        }

        public static System.Collections.Generic.List<dynamic> list()
        {
            return new System.Collections.Generic.List<dynamic>();
        }

        public static dynamic[] array(int n)
        {
            return new dynamic[n];
        }

        public static System.Collections.Generic.List<T> list<T>()
        {
            return new System.Collections.Generic.List<T>();
        }

        public static System.Collections.Generic.List<T> list<T>(params T[] elements)
        {
            return new System.Collections.Generic.List<T>(elements);
        }

        public static T[] array<T>(int n)
        {
            return new T[n];
        }

        public static T[] array<T>(params T[] elements)
        {
            return elements;
        }

        #endregion

            ";

        #endregion
    }
}