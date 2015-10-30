// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseStringInterpolation

namespace Computator.NET.Functions
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    internal static class MatrixFunctions
    {
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

        #endregion

        #region matrix specific operations

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> minor<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M,
            int i, int j, int m, int n)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return M.SubMatrix(i, m, j, n);
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> pow<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M, int n)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            if (M.RowCount != M.ColumnCount)
                throw new System.ArgumentException("It's imposible to take non-square matrix to power!");
            if (n == 0)
            {
                var M2 = M.SubMatrix(0, M.RowCount, 0, M.ColumnCount);
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
                M = M.Inverse();
            //for (int j = 1; j < Math.Abs(n); j++)
            // M = M * M;
            return M.Power(System.Math.Abs(n));
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

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> KroneckerProduct<T>(
            params MathNet.Numerics.LinearAlgebra.Matrix<T>[] M)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            var Mx = new System.Collections.Generic.List<MathNet.Numerics.LinearAlgebra.Matrix<T>>();

            foreach (var m in M)
                Mx.Add(m);

            var MxRet = Mx[0];

            for (var i = 1; i < Mx.Count; i++)
                MxRet = MxRet.KroneckerProduct(Mx[i]);

            return MxRet;
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> PointwiseMultiply<T>(
            params MathNet.Numerics.LinearAlgebra.Matrix<T>[] M)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            var Mx = new System.Collections.Generic.List<MathNet.Numerics.LinearAlgebra.Matrix<T>>();

            foreach (var m in M)
                Mx.Add(m);

            var MxRet = Mx[0];

            for (var i = 0; i < Mx.Count; i++)
                MxRet.PointwiseMultiply(Mx[i], MxRet);

            return MxRet;
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> PointwiseDivide<T>(
            params MathNet.Numerics.LinearAlgebra.Matrix<T>[] M)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            var Mx = new System.Collections.Generic.List<MathNet.Numerics.LinearAlgebra.Matrix<T>>();

            foreach (var m in M)
                Mx.Add(m);

            var MxRet = Mx[0];

            for (var i = 0; i < Mx.Count; i++)
                MxRet.PointwiseDivide(Mx[i], MxRet);

            return MxRet;
        }

        #endregion

        #region matrix utils

#if VERSION_1_5
        public static Meta.Numerics.Matrices.RectangularMatrix matrix(double[,] array)
        {
            return new Meta.Numerics.Matrices.RectangularMatrix(array);
        }
#endif


        public static MathNet.Numerics.LinearAlgebra.Matrix<T> matrix<T>(int n, int m)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            if (typeof (T) == typeof (double) || Evaluation.ScriptingExtensions.IsNumericType(typeof (T)))
                return
                    new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(n, m) as
                        MathNet.Numerics.LinearAlgebra.Matrix<T>;
            if (typeof (T) == typeof (System.Numerics.Complex))
                return
                    new MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix(n, m) as
                        MathNet.Numerics.LinearAlgebra.Matrix<T>;
            throw new System.ArgumentException("Wrong type for matrix creation, consider using real or complex");
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
            if (typeof (T) == typeof (double) || Evaluation.ScriptingExtensions.IsNumericType(typeof (T)))
                return
                    new MathNet.Numerics.LinearAlgebra.Double.DenseVector(n) as MathNet.Numerics.LinearAlgebra.Vector<T>;
            if (typeof (T) == typeof (System.Numerics.Complex))
                return
                    new MathNet.Numerics.LinearAlgebra.Complex.DenseVector(n) as
                        MathNet.Numerics.LinearAlgebra.Vector<T>;
            throw new System.ArgumentException("Wrong type for vector creation, consider using real or complex");
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

        #endregion

        #region matrix specific operations

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> minor<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M,
            int i, int j, int m, int n)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return M.SubMatrix(i, m, j, n);
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> pow<T>(MathNet.Numerics.LinearAlgebra.Matrix<T> M, int n)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            if (M.RowCount != M.ColumnCount)
                throw new System.ArgumentException(""It's imposible to take non-square matrix to power!"");
            if (n == 0)
            {
                var M2 = M.SubMatrix(0, M.RowCount, 0, M.ColumnCount);
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
                M = M.Inverse();
            //for (int j = 1; j < Math.Abs(n); j++)
            // M = M * M;
            return M.Power(System.Math.Abs(n));
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

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> KroneckerProduct<T>(
            params MathNet.Numerics.LinearAlgebra.Matrix<T>[] M)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            var Mx = new System.Collections.Generic.List<MathNet.Numerics.LinearAlgebra.Matrix<T>>();

            foreach (var m in M)
                Mx.Add(m);

            var MxRet = Mx[0];

            for (var i = 1; i < Mx.Count; i++)
                MxRet = MxRet.KroneckerProduct(Mx[i]);

            return MxRet;
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> PointwiseMultiply<T>(
            params MathNet.Numerics.LinearAlgebra.Matrix<T>[] M)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            var Mx = new System.Collections.Generic.List<MathNet.Numerics.LinearAlgebra.Matrix<T>>();

            foreach (var m in M)
                Mx.Add(m);

            var MxRet = Mx[0];

            for (var i = 0; i < Mx.Count; i++)
                MxRet.PointwiseMultiply(Mx[i], MxRet);

            return MxRet;
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<T> PointwiseDivide<T>(
            params MathNet.Numerics.LinearAlgebra.Matrix<T>[] M)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            var Mx = new System.Collections.Generic.List<MathNet.Numerics.LinearAlgebra.Matrix<T>>();

            foreach (var m in M)
                Mx.Add(m);

            var MxRet = Mx[0];

            for (var i = 0; i < Mx.Count; i++)
                MxRet.PointwiseDivide(Mx[i], MxRet);

            return MxRet;
        }

        #endregion

        #region matrix utils

#if VERSION_1_5
        public static Meta.Numerics.Matrices.RectangularMatrix matrix(double[,] array)
        {
            return new Meta.Numerics.Matrices.RectangularMatrix(array);
        }
#endif


        public static MathNet.Numerics.LinearAlgebra.Matrix<T> matrix<T>(int n, int m)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            if (typeof (T) == typeof (double) || ScriptingExtensions.IsNumericType(typeof (T)))
                return
                    new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(n, m) as
                        MathNet.Numerics.LinearAlgebra.Matrix<T>;
            if (typeof (T) == typeof (System.Numerics.Complex))
                return
                    new MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix(n, m) as
                        MathNet.Numerics.LinearAlgebra.Matrix<T>;
            throw new System.ArgumentException(""Wrong type for matrix creation, consider using real or complex"");
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
            if (typeof (T) == typeof (double) || ScriptingExtensions.IsNumericType(typeof (T)))
                return
                    new MathNet.Numerics.LinearAlgebra.Double.DenseVector(n) as MathNet.Numerics.LinearAlgebra.Vector<T>;
            if (typeof (T) == typeof (System.Numerics.Complex))
                return
                    new MathNet.Numerics.LinearAlgebra.Complex.DenseVector(n) as
                        MathNet.Numerics.LinearAlgebra.Vector<T>;
            throw new System.ArgumentException(""Wrong type for vector creation, consider using real or complex"");
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