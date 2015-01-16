using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Meta.Numerics;
using Complex = System.Numerics.Complex;
using DenseVector = MathNet.Numerics.LinearAlgebra.Complex.DenseVector;

namespace Computator.NET.Functions
{
    internal static class MatrixFunctions
    {
        #region matrix specific functions

        public static T Tr<T>(Matrix<T> M) where T : struct, IEquatable<T>, IFormattable
        {
            if (M.RowCount != M.ColumnCount)
                throw new DimensionMismatchException("It's imposible to calculate trace of non-square matrix!");
            return M.Trace();
        }

        public static int rank<T>(Matrix<T> M) where T : struct, IEquatable<T>, IFormattable
        {
            return M.Rank();
        }

        public static T det<T>(Matrix<T> M) where T : struct, IEquatable<T>, IFormattable
        {
            if (M.RowCount != M.ColumnCount)
                throw new DimensionMismatchException("It's imposible to calculate determinant of non-square matrix!");
            return M.Determinant();
        }

        #endregion

        #region matrix specific operations

        public static Matrix<T> minor<T>(Matrix<T> M, int i, int j, int m, int n)
            where T : struct, IEquatable<T>, IFormattable
        {
            return M.SubMatrix(i, m, j, n);
        }

        public static Matrix<T> pow<T>(Matrix<T> M, int n) where T : struct, IEquatable<T>, IFormattable
        {
            if (M.RowCount != M.ColumnCount)
                throw new DimensionMismatchException("It's imposible to take non-square matrix to power!");
            if (n == 0)
            {
                Matrix<T> M2 = M.SubMatrix(0, M.RowCount, 0, M.ColumnCount);
                for (int j = 0; j < M.RowCount; j++)
                    for (int k = 0; k < M.ColumnCount; k++)
                    {
                        if (j == k)
                            M2[j, k] = Matrix<T>.One;
                        else
                            M2[j, k] = Matrix<T>.Zero;
                    }
                return M2;
            }
            if (n < 0)
                M = M.Inverse();
            //for (int j = 1; j < Math.Abs(n); j++)
            // M = M * M;
            return M.Power(Math.Abs(n));
        }

        public static Matrix<T> inv<T>(Matrix<T> M) where T : struct, IEquatable<T>, IFormattable
        {
            if (M.RowCount != M.ColumnCount)
                throw new DimensionMismatchException("It's imposible to calculate inverse matrix of non-square matrix!");
            return M.Inverse();
        }

        public static Matrix<T> transpose<T>(Matrix<T> M) where T : struct, IEquatable<T>, IFormattable
        {
            return M.Transpose();
        }

        public static Matrix<T> KroneckerProduct<T>(params Matrix<T>[] M) where T : struct, IEquatable<T>, IFormattable
        {
            var Mx = new List<Matrix<T>>();

            foreach (var m in M)
                Mx.Add(m);

            Matrix<T> MxRet = Mx[0];

            for (int i = 0; i < Mx.Count; i++)
                MxRet = MxRet.KroneckerProduct(Mx[i]);

            return MxRet;
        }

        public static Matrix<T> PointwiseMultiply<T>(params Matrix<T>[] M) where T : struct, IEquatable<T>, IFormattable
        {
            var Mx = new List<Matrix<T>>();

            foreach (var m in M)
                Mx.Add(m);

            Matrix<T> MxRet = Mx[0];

            for (int i = 0; i < Mx.Count; i++)
                MxRet.PointwiseMultiply(Mx[i], MxRet);

            return MxRet;
        }

        public static Matrix<T> PointwiseDivide<T>(params Matrix<T>[] M) where T : struct, IEquatable<T>, IFormattable
        {
            var Mx = new List<Matrix<T>>();

            foreach (var m in M)
                Mx.Add(m);

            Matrix<T> MxRet = Mx[0];

            for (int i = 0; i < Mx.Count; i++)
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


        public static Matrix<T> matrix<T>(int n, int m) where T : struct, IEquatable<T>, IFormattable
        {
            if (typeof (T) == typeof (double) || typeof (T).IsNumericType())
                return new DenseMatrix(n, m) as Matrix<T>;
            if (typeof (T) == typeof (Complex))
                return new MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix(n, m) as Matrix<T>;
            throw new ArgumentException("Wrong type for matrix creation, consider using real or complex");
        }

        public static DenseMatrix matrix(double[,] array)
        {
            return DenseMatrix.OfArray(array);
        }

        public static MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix matrix(Complex[,] array)
        {
            return MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix.OfArray(array);
        }

        public static DenseMatrix matrix(int[,] array)
        {
            var darray = new double[array.GetLength(0), array.GetLength(1)];

            for (int j = 0; j < array.GetLength(0); j++)
                for (int k = 0; k < array.GetLength(1); k++)
                    darray[j, k] = array[j, k];

            return DenseMatrix.OfArray(darray);
        }

        public static DenseVector vector(params Complex[] elements)
        {
            return new DenseVector(elements);
        }

        public static MathNet.Numerics.LinearAlgebra.Double.DenseVector vector(params double[] elements)
        {
            return new MathNet.Numerics.LinearAlgebra.Double.DenseVector(elements);
        }

        public static Vector<T> vector<T>(int n) where T : struct, IEquatable<T>, IFormattable
        {
            if (typeof (T) == typeof (double) || typeof (T).IsNumericType())
                return new MathNet.Numerics.LinearAlgebra.Double.DenseVector(n) as Vector<T>;
            if (typeof (T) == typeof (Complex))
                return new DenseVector(n) as Vector<T>;
            throw new ArgumentException("Wrong type for vector creation, consider using real or complex");
        }


        public static List<T> list<T>()
        {
            return new List<T>();
        }

        public static List<T> list<T>(params T[] elements)
        {
            return new List<T>(elements);
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

        public const string ToCode =
            @"
        #region matrix specific functions

        public static T Tr<T>(Matrix<T> M) where T : struct, IEquatable<T>, IFormattable
        {
            if (M.RowCount != M.ColumnCount)
                throw new DimensionMismatchException(""It's imposible to calculate trace of non-square matrix!"");
            return M.Trace();
        }

        public static int rank<T>(Matrix<T> M) where T : struct, IEquatable<T>, IFormattable
        {
            return M.Rank();
        }

        public static T det<T>(Matrix<T> M) where T : struct, IEquatable<T>, IFormattable
        {
            if (M.RowCount != M.ColumnCount)
                throw new DimensionMismatchException(""It's imposible to calculate determinant of non-square matrix!"");
            return M.Determinant();
        }

        #endregion

        #region matrix specific operations

        public static Matrix<T> minor<T>(Matrix<T> M, int i, int j, int m, int n)
            where T : struct, IEquatable<T>, IFormattable
        {
            return M.SubMatrix(i, m, j, n);
        }

        public static Matrix<T> pow<T>(Matrix<T> M, int n) where T : struct, IEquatable<T>, IFormattable
        {
            if (M.RowCount != M.ColumnCount)
                throw new DimensionMismatchException(""It's imposible to take non-square matrix to power!"");
            if (n == 0)
            {
                Matrix<T> M2 = M.SubMatrix(0, M.RowCount, 0, M.ColumnCount);
                for (int j = 0; j < M.RowCount; j++)
                    for (int k = 0; k < M.ColumnCount; k++)
                    {
                        if (j == k)
                            M2[j, k] = Matrix<T>.One;
                        else
                            M2[j, k] = Matrix<T>.Zero;
                    }
                return M2;
            }
            if (n < 0)
                M = M.Inverse();
            //for (int j = 1; j < Math.Abs(n); j++)
            // M = M * M;
            return M.Power(Math.Abs(n));
        }

        public static Matrix<T> inv<T>(Matrix<T> M) where T : struct, IEquatable<T>, IFormattable
        {
            if (M.RowCount != M.ColumnCount)
                throw new DimensionMismatchException(""It's imposible to calculate inverse matrix of non-square matrix!"");
            return M.Inverse();
        }

        public static Matrix<T> transpose<T>(Matrix<T> M) where T : struct, IEquatable<T>, IFormattable
        {
            return M.Transpose();
        }

        public static Matrix<T> KroneckerProduct<T>(params Matrix<T>[] M) where T : struct, IEquatable<T>, IFormattable
        {
            var Mx = new List<Matrix<T>>();

            foreach (var m in M)
                Mx.Add(m);

            Matrix<T> MxRet = Mx[0];

            for (int i = 0; i < Mx.Count; i++)
                MxRet = MxRet.KroneckerProduct(Mx[i]);

            return MxRet;
        }

        public static Matrix<T> PointwiseMultiply<T>(params Matrix<T>[] M) where T : struct, IEquatable<T>, IFormattable
        {
            var Mx = new List<Matrix<T>>();

            foreach (var m in M)
                Mx.Add(m);

            Matrix<T> MxRet = Mx[0];

            for (int i = 0; i < Mx.Count; i++)
                MxRet.PointwiseMultiply(Mx[i], MxRet);

            return MxRet;
        }

        public static Matrix<T> PointwiseDivide<T>(params Matrix<T>[] M) where T : struct, IEquatable<T>, IFormattable
        {
            var Mx = new List<Matrix<T>>();

            foreach (var m in M)
                Mx.Add(m);

            Matrix<T> MxRet = Mx[0];

            for (int i = 0; i < Mx.Count; i++)
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


        public static Matrix<T> matrix<T>(int n, int m) where T : struct, IEquatable<T>, IFormattable
        {
            if (typeof (T) == typeof (double) || typeof (T).IsNumericType())
                return new DenseMatrix(n, m) as Matrix<T>;
            if (typeof (T) == typeof (Complex))
                return new MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix(n, m) as Matrix<T>;
            throw new ArgumentException(""Wrong type for matrix creation, consider using real or complex"");
        }

        public static DenseMatrix matrix(double[,] array)
        {
            return DenseMatrix.OfArray(array);
        }

        public static MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix matrix(Complex[,] array)
        {
            return MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix.OfArray(array);
        }

        public static DenseMatrix matrix(int[,] array)
        {
            var darray = new double[array.GetLength(0), array.GetLength(1)];

            for (int j = 0; j < array.GetLength(0); j++)
                for (int k = 0; k < array.GetLength(1); k++)
                    darray[j, k] = array[j, k];

            return DenseMatrix.OfArray(darray);
        }

        public static DenseVector vector(params Complex[] elements)
        {
            return new DenseVector(elements);
        }

        public static MathNet.Numerics.LinearAlgebra.Double.DenseVector vector(params double[] elements)
        {
            return new MathNet.Numerics.LinearAlgebra.Double.DenseVector(elements);
        }

        public static Vector<T> vector<T>(int n) where T : struct, IEquatable<T>, IFormattable
        {
            if (typeof (T) == typeof (double) || typeof (T).IsNumericType())
                return new MathNet.Numerics.LinearAlgebra.Double.DenseVector(n) as Vector<T>;
            if (typeof (T) == typeof (Complex))
                return new DenseVector(n) as Vector<T>;
            throw new ArgumentException(""Wrong type for vector creation, consider using real or complex"");
        }


        public static List<T> list<T>()
        {
            return new List<T>();
        }

        public static List<T> list<T>(params T[] elements)
        {
            return new List<T>(elements);
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
    }

    internal static class ArrayExtension
    {
        public static int size(this Array array)
        {
            return array.Length;
        }

        public static void Add<T>(this T[] array, T element)
        {
            var narray = new T[array.Length + 1];
            narray[array.Length] = element;
            array = narray;
        }
    }

    internal static class ListExtension
    {
        public static int size<T>(this List<T> list)
        {
            return list.Count;
        }
    }
}