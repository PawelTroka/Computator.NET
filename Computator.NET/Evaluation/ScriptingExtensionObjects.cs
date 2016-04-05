// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseStringInterpolation


namespace Computator.NET.Evaluation
{
    //TODO: refactor this shit
    internal class TypeDeducer
    {
        public static System.Func<TR> Func<TR>(System.Func<TR> f)
        {
            return f;
        }

        public static System.Func<T1, TR> Func<T1, TR>(System.Func<T1, TR> f)
        {
            return f;
        }

        public static System.Func<T1, T2, TR> Func<T1, T2, TR>(System.Func<T1, T2, TR> f)
        {
            return f;
        }

        public static System.Func<T1, T2, T3, TR> Func<T1, T2, T3, TR>(System.Func<T1, T2, T3, TR> f)
        {
            return f;
        }

        public static System.Func<T1, T2, T3, T4, TR> Func<T1, T2, T3, T4, TR>(System.Func<T1, T2, T3, T4, TR> f)
        {
            return f;
        }

        public static System.Func<T1, T2, T3, T4, T5, TR> Func<T1, T2, T3, T4, T5, TR>(
            System.Func<T1, T2, T3, T4, T5, TR> f)
        {
            return f;
        }

        public static System.Func<T1, T2, T3, T4, T5, T6, TR> Func<T1, T2, T3, T4, T5, T6, TR>(
            System.Func<T1, T2, T3, T4, T5, T6, TR> f)
        {
            return f;
        }

        public static System.Func<T1, T2, T3, T4, T5, T6, T7, TR> Func<T1, T2, T3, T4, T5, T6, T7, TR>(
            System.Func<T1, T2, T3, T4, T5, T6, T7, TR> f)
        {
            return f;
        }

        public static System.Func<T1, T2, T3, T4, T5, T6, T7, T8, TR> Func<T1, T2, T3, T4, T5, T6, T7, T8, TR>(
            System.Func<T1, T2, T3, T4, T5, T6, T7, T8, TR> f)
        {
            return f;
        }
    }


    public class File
    {
        private readonly string path;
        private System.IO.StreamReader sr;
        private System.IO.StreamWriter sw;

        public File(string path)
        {
            this.path = path;
            reOpen();
        }

        private void reOpen()
        {
            var oStream = new System.IO.FileStream(path, System.IO.FileMode.Append, System.IO.FileAccess.Write,
                System.IO.FileShare.Read);
            var iStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read,
                System.IO.FileShare.ReadWrite);

            sw = new System.IO.StreamWriter(oStream);
            sr = new System.IO.StreamReader(iStream);
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



    internal static class ScriptingExtensions
    {
        public static int size<T>(this MathNet.Numerics.LinearAlgebra.Vector<T> vector)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return vector.Count;
        }

        public static int[] size<T>(this MathNet.Numerics.LinearAlgebra.Matrix<T> matrix)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return new[] { matrix.RowCount, matrix.ColumnCount };
        }

        public static string ToMathString(this System.Numerics.Complex z)
        {
            switch (Properties.Settings.Default.NumericalOutputNotation)
            {
                case Computator.NET.DataTypes.SettingsTypes.NumericalOutputNotationType.MathematicalNotation:
                    if (z.Real == 0)
                    {
                        if (z.Imaginary == 1.0)
                            return "i";

                        if (z.Imaginary == -1.0)
                            return "-i";

                        return z.Imaginary != 0
                            ? string.Format("{0}{1}i", z.Imaginary.ToMathString(), Computator.NET.DataTypes.SpecialSymbols.DotSymbol)
                            : "0";
                    }

                    if (z.Imaginary == 0)
                        return z.Real.ToMathString();
                    return (z.Imaginary > 0 || double.IsNaN(z.Imaginary))
                        ? string.Format("{0}+{1}{2}i", z.Real.ToMathString(), z.Imaginary.ToMathString(),
                            Computator.NET.DataTypes.SpecialSymbols.DotSymbol)
                        : string.Format("{0}{1}{2}i", z.Real.ToMathString(), z.Imaginary.ToMathString(),
                            Computator.NET.DataTypes.SpecialSymbols.DotSymbol);
                default:
                    //case NumericalOutputNotationType.EngineeringNotation:
                    return z.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public static string ToMathString(this double x)
        {
            switch (Properties.Settings.Default.NumericalOutputNotation)
            {
                case Computator.NET.DataTypes.SettingsTypes.NumericalOutputNotationType.MathematicalNotation:
                    var str = x.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    if (!str.Contains("E") && !str.Contains("e"))
                    {
                        if (x == double.PositiveInfinity)
                            return Computator.NET.DataTypes.SpecialSymbols.Infinity;
                        else if(x == double.NegativeInfinity)
                            return string.Format("-{0}", Computator.NET.DataTypes.SpecialSymbols.Infinity);
                        return str;
                    }
                    var chunks = str.Split('E', 'e');
                    var ret = string.Format("{0}{1}10{2}", chunks[0], Computator.NET.DataTypes.SpecialSymbols.DotSymbol,
                        Computator.NET.DataTypes.SpecialSymbols.AsciiToSuperscript(double.Parse(chunks[1]).ToString(System.Globalization.CultureInfo.InvariantCulture)));//we parse first then apply ToString() again to get rid of notations from engineering notation like eg +019 (when original walue was 1E+19) simple hack :)
                    return ret;
                default:
                    //case NumericalOutputNotationType.EngineeringNotation:
                    return x.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }



        public static bool IsNumericType(this object o)
        {
            if (o == null)
                return false;
            switch (System.Type.GetTypeCode(o.GetType()))
            {
                case System.TypeCode.Byte:
                case System.TypeCode.SByte:
                case System.TypeCode.UInt16:
                case System.TypeCode.UInt32:
                case System.TypeCode.UInt64:
                case System.TypeCode.Int16:
                case System.TypeCode.Int32:
                case System.TypeCode.Int64:
                case System.TypeCode.Decimal:
                case System.TypeCode.Double:
                case System.TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static int size<T>(this System.Collections.Generic.List<T> list)
        {
            return list.Count;
        }

        public static int size(this System.Array array)
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

    internal static class ScriptingExtensionObjects
    {
        public const string ToCode = ReadForm.ToCode +
                                     @"
 internal class TypeDeducer
    {
        public static System.Func<TR> Func<TR>(System.Func<TR> f)
        {
            return f;
        }

        public static System.Func<T1, TR> Func<T1, TR>(System.Func<T1, TR> f)
        {
            return f;
        }

        public static System.Func<T1, T2, TR> Func<T1, T2, TR>(System.Func<T1, T2, TR> f)
        {
            return f;
        }

        public static System.Func<T1, T2, T3, TR> Func<T1, T2, T3, TR>(System.Func<T1, T2, T3, TR> f)
        {
            return f;
        }

        public static System.Func<T1, T2, T3, T4, TR> Func<T1, T2, T3, T4, TR>(System.Func<T1, T2, T3, T4, TR> f)
        {
            return f;
        }

        public static System.Func<T1, T2, T3, T4, T5, TR> Func<T1, T2, T3, T4, T5, TR>(
            System.Func<T1, T2, T3, T4, T5, TR> f)
        {
            return f;
        }

        public static System.Func<T1, T2, T3, T4, T5, T6, TR> Func<T1, T2, T3, T4, T5, T6, TR>(
            System.Func<T1, T2, T3, T4, T5, T6, TR> f)
        {
            return f;
        }

        public static System.Func<T1, T2, T3, T4, T5, T6, T7, TR> Func<T1, T2, T3, T4, T5, T6, T7, TR>(
            System.Func<T1, T2, T3, T4, T5, T6, T7, TR> f)
        {
            return f;
        }

        public static System.Func<T1, T2, T3, T4, T5, T6, T7, T8, TR> Func<T1, T2, T3, T4, T5, T6, T7, T8, TR>(
            System.Func<T1, T2, T3, T4, T5, T6, T7, T8, TR> f)
        {
            return f;
        }
    }


    public class File
    {
        private readonly string path;
        private System.IO.StreamReader sr;
        private System.IO.StreamWriter sw;

        public File(string path)
        {
            this.path = path;
            reOpen();
        }

        private void reOpen()
        {
            var oStream = new System.IO.FileStream(path, System.IO.FileMode.Append, System.IO.FileAccess.Write,
                System.IO.FileShare.Read);
            var iStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read,
                System.IO.FileShare.ReadWrite);

            sw = new System.IO.StreamWriter(oStream);
            sr = new System.IO.StreamReader(iStream);
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



    internal static class ScriptingExtensions
    {
        public static int size<T>(this MathNet.Numerics.LinearAlgebra.Vector<T> vector)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return vector.Count;
        }

        public static int[] size<T>(this MathNet.Numerics.LinearAlgebra.Matrix<T> matrix)
            where T : struct, System.IEquatable<T>, System.IFormattable
        {
            return new[] { matrix.RowCount, matrix.ColumnCount };
        }

            public static string ToMathString(this System.Numerics.Complex z)
        {
            switch (Properties.Settings.Default.NumericalOutputNotation)
            {
                case Computator.NET.DataTypes.SettingsTypes.NumericalOutputNotationType.MathematicalNotation:
                    if (z.Real == 0)
                    {
                        if (z.Imaginary == 1.0)
                            return ""i"";

                        if (z.Imaginary == -1.0)
                            return ""-i"";

                        return z.Imaginary != 0
                            ? string.Format(""{0}{1}i"", z.Imaginary.ToMathString(), Computator.NET.DataTypes.SpecialSymbols.DotSymbol)
                            : ""0"";
                    }

                    if (z.Imaginary == 0)
                        return z.Real.ToMathString();
                    return z.Imaginary > 0
                        ? string.Format(""{0}+{1}{2}i"", z.Real.ToMathString(), z.Imaginary.ToMathString(),
                            Computator.NET.DataTypes.SpecialSymbols.DotSymbol)
                        : string.Format(""{0}{1}{2}i"", z.Real.ToMathString(), z.Imaginary.ToMathString(),
                            Computator.NET.DataTypes.SpecialSymbols.DotSymbol);
                default:
                    //case NumericalOutputNotationType.EngineeringNotation:
                    return z.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public static string ToMathString(this double x)
        {
            switch (Properties.Settings.Default.NumericalOutputNotation)
            {
                case Computator.NET.DataTypes.SettingsTypes.NumericalOutputNotationType.MathematicalNotation:
                    var str = x.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    if (!str.Contains(""E"") && !str.Contains(""e""))
                    {
                        if (x == double.PositiveInfinity)
                            return Computator.NET.DataTypes.SpecialSymbols.Infinity;
                        else if(x == double.NegativeInfinity)
                            return string.Format(""-{0}"", Computator.NET.DataTypes.SpecialSymbols.Infinity);
                        return str;
                    }
                    var chunks = str.Split('E', 'e');
                    var ret = string.Format(""{0}{1}10{2}"", chunks[0], Computator.NET.DataTypes.SpecialSymbols.DotSymbol,
                        Computator.NET.DataTypes.SpecialSymbols.AsciiToSuperscript(double.Parse(chunks[1]).ToString(System.Globalization.CultureInfo.InvariantCulture)));//we parse first then apply ToString() again to get rid of notations from engineering notation like eg +019 (when original walue was 1E+19) simple hack :)
                    return ret;
                default:
                    //case NumericalOutputNotationType.EngineeringNotation:
                    return x.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }



        public static bool IsNumericType(this object o)
        {
            if (o == null)
                return false;
            switch (System.Type.GetTypeCode(o.GetType()))
            {
                case System.TypeCode.Byte:
                case System.TypeCode.SByte:
                case System.TypeCode.UInt16:
                case System.TypeCode.UInt32:
                case System.TypeCode.UInt64:
                case System.TypeCode.Int16:
                case System.TypeCode.Int32:
                case System.TypeCode.Int64:
                case System.TypeCode.Decimal:
                case System.TypeCode.Double:
                case System.TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static int size<T>(this System.Collections.Generic.List<T> list)
        {
            return list.Count;
        }

        public static int size(this System.Array array)
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

        ";
    }
}