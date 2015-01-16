using System;
using System.Collections.Generic;
using System.Numerics;
using Meta.Numerics.Matrices;

namespace Computator.NET.Evaluation
{
    internal static class ComplexExtension
    {
        public static string ToMathString(this Complex z)
        {
            if (z.Real == 0)
            {
                if (z.Imaginary != 0)
                    return z.Imaginary.ToString() + "i";
                else
                    return "0";
            }

            if (z.Imaginary == 0)
                return z.Real.ToString();
            else
            {
                if (z.Imaginary > 0)
                    return z.Real.ToString() + "+" + z.Imaginary.ToString() + "i";
                else
                    return z.Real.ToString() + z.Imaginary.ToString() + "i";
            }
        }
    }

    internal static class MatrixExtension
    {
        public static string ToString2(this RectangularMatrix rm)
        {
            string s = "";

            for (int j = 0; j < rm.RowCount; j++)
            {
                for (int i = 0; i < rm.ColumnCount; i++)
                    s += rm[j, i].ToString() + "  ";
                s += '\n';
            }
            return s;
        }
    }

    internal static class ObjectExtension
    {
        public static bool IsNumericType(this object o)
        {
            if (o == null)
                return false;
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
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

    internal static class ScriptingExtensionObjects
    {
        public const string ToCode = ReadForm.ToCode +
                                     @"
            static class ComplexExtension
            {
                public static string ToMathString(this Complex z)
                {
                    if(z.Real==0)
                    {
                        if (z.Imaginary != 0)
                            return z.Imaginary.ToString() + ""i"";
                        else
                            return ""0"";
                    }

                    if (z.Imaginary == 0)
                        return z.Real.ToString();
                    else
                    {
                        if (z.Imaginary > 0)
                            return z.Real.ToString() + ""+"" + z.Imaginary.ToString()+""i"";
                        else
                            return z.Real.ToString() + z.Imaginary.ToString()+""i"";
                    }

                }
            }        	

            static class MatrixExtension
            {
                public static string ToString2(this Meta.Numerics.Matrices.RectangularMatrix rm)
                {
                    string s = """";

                    for (int j = 0; j < rm.RowCount; j++)
                    {
                        for (int i = 0; i < rm.ColumnCount; i++)
                            s += rm[j, i].ToString() + ""  "";
                        s += '\n';
                    }
                    return s;
                }
            }

            static class ObjectExtension
            {
                public static bool IsNumericType(this object o)
                {
                    if (o == null)
                        return false;
                    switch (Type.GetTypeCode(o.GetType()))
                    {
                        case TypeCode.Byte:
                        case TypeCode.SByte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Decimal:
                        case TypeCode.Double:
                        case TypeCode.Single:
                            return true;
                        default:
                            return false;
                    }
                }
            }

            static class ArrayExtension
            {
                public static int size(this Array array)
                {
                    return array.Length;
                }

                public static void Add<T>(this T[] array,T element)
                {
                    var narray = new T[array.Length + 1];
                    narray[array.Length] = element;
                    array = narray;
                }
            }

            static class ListExtension
            {
                public static int size<T>(this List<T> list)
                {
                    return list.Count;
                }
            }
        ";
    }
}