using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Properties;
using MathNet.Numerics.LinearAlgebra;

namespace Computator.NET.Evaluation
{
    internal class TypeDeducer
    {
        public static Func<TR> Func<TR>(Func<TR> f)
        {
            return f;
        }

        public static Func<T1, TR> Func<T1, TR>(Func<T1, TR> f)
        {
            return f;
        }

        public static Func<T1, T2, TR> Func<T1, T2, TR>(Func<T1, T2, TR> f)
        {
            return f;
        }

        public static Func<T1, T2, T3, TR> Func<T1, T2, T3, TR>(Func<T1, T2, T3, TR> f)
        {
            return f;
        }

        public static Func<T1, T2, T3, T4, TR> Func<T1, T2, T3, T4, TR>(Func<T1, T2, T3, T4, TR> f)
        {
            return f;
        }

        public static Func<T1, T2, T3, T4, T5, TR> Func<T1, T2, T3, T4, T5, TR>(Func<T1, T2, T3, T4, T5, TR> f)
        {
            return f;
        }

        public static Func<T1, T2, T3, T4, T5, T6, TR> Func<T1, T2, T3, T4, T5, T6, TR>(
            Func<T1, T2, T3, T4, T5, T6, TR> f)
        {
            return f;
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, TR> Func<T1, T2, T3, T4, T5, T6, T7, TR>(
            Func<T1, T2, T3, T4, T5, T6, T7, TR> f)
        {
            return f;
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, TR> Func<T1, T2, T3, T4, T5, T6, T7, T8, TR>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, TR> f)
        {
            return f;
        }
    }


    public class File
    {
        private readonly string path;
        private StreamReader sr;
        private StreamWriter sw;

        public File(string path)
        {
            this.path = path;
            reOpen();
        }

        private void reOpen()
        {
            var oStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
            var iStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            sw = new StreamWriter(oStream);
            sr = new StreamReader(iStream);
        }

        public void clear()
        {
            close();
            System.IO.File.WriteAllText(path, string.Empty);
            reOpen();
        }

        public void close()
        {
            sr.Close();
            sw.Close();
        }

        public string readln()
        {
            return sr.ReadLine();
        }

        public string readAll()
        {
            return sr.ReadToEnd();
        }

        public void write(string s)
        {
            sw.Write(s);
            sw.Flush();
        }

        public void writeln(string s)
        {
            sw.WriteLine(s);
            sw.Flush();
        }
    }

    internal static class MatrixExtensions
    {
        public static int size<T>(this Vector<T> vector) where T : struct, IEquatable<T>, IFormattable
        {
            return vector.Count;
        }

        public static int[] size<T>(this Matrix<T> matrix) where T : struct, IEquatable<T>, IFormattable
        {
            return new[] {matrix.RowCount, matrix.ColumnCount};
        }
    }


    internal static class ComplexExtension
    {
        public static string ToMathString(this Complex z)
        {
            switch (Settings.Default.NumericalOutputNotation)
            {
                case NumericalOutputNotationType.MathematicalNotation:
                    if (z.Real == 0)
                    {
                        return z.Imaginary != 0
                            ? string.Format("{0}{1}i", z.Imaginary.ToMathString(), SpecialSymbols.DotSymbol)
                            : "0";
                    }

                    if (z.Imaginary == 0)
                        return z.Real.ToMathString();
                    return z.Imaginary > 0
                        ? string.Format("{0}+{1}{2}i", z.Real.ToMathString(), z.Imaginary.ToMathString(),
                            SpecialSymbols.DotSymbol)
                        : string.Format("{0}{1}{2}i", z.Real.ToMathString(), z.Imaginary.ToMathString(),
                            SpecialSymbols.DotSymbol);
                default:
                    //case NumericalOutputNotationType.EngineeringNotation:
                    return z.ToString();
            }
        }

        public static string ToMathString(this double x)
        {
            switch (Settings.Default.NumericalOutputNotation)
            {
                case NumericalOutputNotationType.MathematicalNotation:
                    var str = x.ToString();
                    if (!str.Contains("E") && !str.Contains("e"))
                        return str;
                    var chunks = str.Split('E', 'e');
                    var ret = string.Format("{0}{1}10{2}", chunks[0], SpecialSymbols.DotSymbol,
                        SpecialSymbols.AsciiToSuperscript(chunks[1]));
                    return ret;
                default:
                    //case NumericalOutputNotationType.EngineeringNotation:
                    return x.ToString();
            }
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
    class TypeDeducer
    {
        public static Func<TR> Func<TR>(Func<TR> f) { return f; }
        public static Func<T1, TR> Func<T1, TR>(Func<T1, TR> f) { return f; }
        public static Func<T1, T2, TR> Func<T1, T2, TR>(Func<T1, T2, TR> f) { return f; }
        public static Func<T1, T2, T3, TR> Func<T1, T2, T3, TR>(Func<T1, T2, T3, TR> f) { return f; }
        public static Func<T1, T2, T3, T4, TR> Func<T1, T2, T3, T4, TR>(Func<T1, T2, T3, T4, TR> f) { return f; }
        public static Func<T1, T2, T3, T4, T5, TR> Func<T1, T2, T3, T4, T5, TR>(Func<T1, T2, T3, T4, T5, TR> f) { return f; }
        public static Func<T1, T2, T3, T4, T5, T6, TR> Func<T1, T2, T3, T4, T5, T6, TR>(Func<T1, T2, T3, T4, T5, T6, TR> f) { return f; }

        public static Func<T1, T2, T3, T4, T5, T6, T7, TR> Func<T1, T2, T3, T4, T5, T6, T7, TR>(Func<T1, T2, T3, T4, T5, T6, T7, TR> f) { return f; }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, TR> Func<T1, T2, T3, T4, T5, T6, T7, T8, TR>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TR> f) { return f; }

        //usage:
        //var zip = TypeDeducer.Func( (int a, int b) => a*a+b*b );
        //
    }


    public class File
    {

        private StreamReader sr;

        private StreamWriter sw;

        private string path;

        public File(string path)
        {
            this.path = path;
            reOpen();
        }

        private void reOpen()
        {
            var oStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
            var iStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            sw = new System.IO.StreamWriter(oStream);
            sr = new System.IO.StreamReader(iStream);
        }


        public void clear()
        {
            close();
            System.IO.File.WriteAllText(path, String.Empty);
            reOpen();
        }

        public void close()
        {
            sr.Close();
            sw.Close();
        }

        public string readln()
        {
            return sr.ReadLine();
        }

        public string readAll()
        {
            return sr.ReadToEnd();
        }

        public void write(string s)
        {
            sw.Write(s);
            sw.Flush();
        }

        public void writeln(string s)
        {
            sw.WriteLine(s);
            sw.Flush();
        }
    }

    internal static class MatrixExtensions
    {
        public static int size<T>(this MathNet.Numerics.LinearAlgebra.Vector<T> vector) where T : struct, IEquatable<T>, IFormattable
        {
            return vector.Count;
        }


        public static int[] size<T>(this MathNet.Numerics.LinearAlgebra.Matrix<T> matrix) where T : struct, IEquatable<T>, IFormattable
        {
            return new int[]{matrix.RowCount,matrix.ColumnCount};
        }
    }


    internal static class ComplexExtension
    {
        public static string ToMathString(this Complex z)
        {
            switch (Properties.Settings.Default.NumericalOutputNotation)
            {
                case NumericalOutputNotationType.MathematicalNotation:
                    if (z.Real == 0)
                    {
                        return z.Imaginary != 0 ? string.Format(""{0}{1}i"", z.Imaginary.ToMathString(), SpecialSymbols.DotSymbol) : ""0"";
                    }

                    if (z.Imaginary == 0)
                        return z.Real.ToMathString();
                    return z.Imaginary > 0
                        ? string.Format(""{0}+{1}{2}i"", z.Real.ToMathString(), z.Imaginary.ToMathString(), SpecialSymbols.DotSymbol)
                        : string.Format(""{0}{1}{2}i"", z.Real.ToMathString(), z.Imaginary.ToMathString(), SpecialSymbols.DotSymbol);
                default:
                //case NumericalOutputNotationType.EngineeringNotation:
                    return z.ToString();
            }
        }

        public static string ToMathString(this double x)
        {
            switch (Properties.Settings.Default.NumericalOutputNotation)
            {
                case NumericalOutputNotationType.MathematicalNotation:
                    var str = x.ToString();
                    if (!str.Contains(""E"") && !str.Contains(""e""))
                        return str;
                    else
                    {
                        var chunks = str.Split('E', 'e');
                        string ret = string.Format(""{0}{1}10{2}"", chunks[0], SpecialSymbols.DotSymbol, SpecialSymbols.AsciiToSuperscript(chunks[1]));
                        return ret;
                    }
                default:
                    //case NumericalOutputNotationType.EngineeringNotation:
                    return x.ToString();
            }
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

        ";
    }
}