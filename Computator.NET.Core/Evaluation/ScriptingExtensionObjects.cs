// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseStringInterpolation

namespace Computator.NET.Core.Evaluation
{
    //TODO: refactor this shit
    public class TypeDeducer
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

    public static class ScriptingExtensions
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

        private const string SuperscriptDigits = "⁰¹²³⁴⁵⁶⁷⁸⁹";

        // Parse displayed numeric values, not arbitrary TSL expressions. In
        // particular, do not interpret a comma as a thousands/decimal separator.
        public static double FromMathString(this string mathString)
        {
            System.ArgumentNullException.ThrowIfNull(mathString);
            var text = mathString.Trim();
            var dot = text.IndexOf(Computator.NET.DataTypes.Text.SpecialSymbols.DotSymbol);
            if (dot < 0)
                return ParseMathFactor(text);
            if (dot != text.LastIndexOf(Computator.NET.DataTypes.Text.SpecialSymbols.DotSymbol))
                throw new System.FormatException("A displayed number can contain at most one multiplication dot.");

            var mantissa = text.Substring(0, dot).Trim();
            var factor = text.Substring(dot + 1).Trim();
            if (mantissa.Length == 0 || factor.Length == 0)
                throw new System.FormatException("Both sides of the multiplication dot must contain a number.");

            if (IsPowerOfTen(factor))
            {
                // Parse once instead of multiplying by a separately rounded
                // power of ten: 5 * double.Parse("1E-324") would incorrectly be 0.
                return ParseInvariantNumber(mantissa + "E" + ParseSuperscriptExponent(factor.Substring(2)));
            }

            // Preserve the historical two-factor form, e.g. 2·3.
            return ParseInvariantNumber(mantissa) * ParseInvariantNumber(factor);
        }

        private static double ParseMathFactor(string text)
        {
            return IsPowerOfTen(text)
                ? ParseInvariantNumber("1E" + ParseSuperscriptExponent(text.Substring(2)))
                : ParseInvariantNumber(text);
        }

        private static bool IsPowerOfTen(string text)
        {
            return text.Length > 2 && text.StartsWith("10", System.StringComparison.Ordinal) &&
                (SuperscriptDigits.IndexOf(text[2]) >= 0 || text[2] == '⁺' || text[2] == '⁻');
        }

        private static string ParseSuperscriptExponent(string exponent)
        {
            var result = new System.Text.StringBuilder(exponent.Length);
            var index = 0;
            if (exponent[0] == '⁺' || exponent[0] == '⁻')
            {
                result.Append(exponent[0] == '⁻' ? '-' : '+');
                index++;
            }
            if (index == exponent.Length)
                throw new System.FormatException("The exponent must contain superscript digits.");
            for (; index < exponent.Length; index++)
            {
                var digit = SuperscriptDigits.IndexOf(exponent[index]);
                if (digit < 0)
                    throw new System.FormatException("The exponent must be a signed superscript integer.");
                result.Append((char)('0' + digit));
            }
            return result.ToString();
        }

        private static double ParseInvariantNumber(string text)
        {
            if (text == "∞" || text == "+∞") return double.PositiveInfinity;
            if (text == "-∞") return double.NegativeInfinity;
            return double.Parse(text, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string ToMathString(this System.Numerics.Complex z)
        {
            //if (double.IsNaN(z.Real) && double.IsNaN(z.Imaginary))
            //return "NaN";

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
                            ? string.Format("{0}{1}i", z.Imaginary.ToMathString(), Computator.NET.DataTypes.Text.SpecialSymbols.DotSymbol)
                            : "0";
                    }

                    if (z.Imaginary == 0)
                        return z.Real.ToMathString();
                    return (z.Imaginary > 0 || double.IsNaN(z.Imaginary))
                        ? string.Format("{0}+{1}{2}i", z.Real.ToMathString(), z.Imaginary.ToMathString(),
                            Computator.NET.DataTypes.Text.SpecialSymbols.DotSymbol)
                        : string.Format("{0}{1}{2}i", z.Real.ToMathString(), z.Imaginary.ToMathString(),
                            Computator.NET.DataTypes.Text.SpecialSymbols.DotSymbol);
                default:
                    //case NumericalOutputNotationType.EngineeringNotation:
                    return z.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public static string ToMathString(this double x)
        {
            if (x == double.PositiveInfinity)
                return Computator.NET.DataTypes.Text.SpecialSymbols.Infinity;
            if (x == double.NegativeInfinity)
                return string.Format("-{0}", Computator.NET.DataTypes.Text.SpecialSymbols.Infinity);
            return x.ToString(System.Globalization.CultureInfo.InvariantCulture).ToMathString();
        }

        // Format decimal directly: converting to double introduces binary rounding
        // artifacts that modern .NET's round-trip formatting correctly exposes.
        public static string ToMathString(this decimal x)
        {
            return DecimalDisplay.Format(x).ToMathString();
        }

        public static string ToMathString(this string str)
        {
            switch (Properties.Settings.Default.NumericalOutputNotation)
            {
                case Computator.NET.DataTypes.SettingsTypes.NumericalOutputNotationType.MathematicalNotation:
                    if (!str.Contains("E") && !str.Contains("e"))
                        return str;
                    var chunks = str.Split('E', 'e');
                    var ret = string.Format("{0}{1}10{2}", chunks[0], Computator.NET.DataTypes.Text.SpecialSymbols.DotSymbol,
                        Computator.NET.DataTypes.Text.SpecialSymbols.AsciiToSuperscript(double.Parse(chunks[1]).ToString(System.Globalization.CultureInfo.InvariantCulture)));//we parse first then apply ToString() again to get rid of notations from engineering notation like eg +019 (when original walue was 1E+19) simple hack :)
                    return ret;
                case Computator.NET.DataTypes.SettingsTypes.NumericalOutputNotationType.EngineeringNotation:
                    return str;
                default:
                    return str;
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
        public const string ToCode = Computator.NET.Core.Functions.ReadForm.ToCode +
                                     @"
     public class TypeDeducer
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

    public static class ScriptingExtensions
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

        public static double FromMathString(this string mathString)
        {
            // Keep generated scripts on the same parser as numeric input controls.
            return Computator.NET.Core.Evaluation.ScriptingExtensions.FromMathString(mathString);
        }

        public static string ToMathString(this System.Numerics.Complex z)
        {
            //if (double.IsNaN(z.Real) && double.IsNaN(z.Imaginary))
            //return ""NaN"";

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
                            ? string.Format(""{0}{1}i"", z.Imaginary.ToMathString(), Computator.NET.DataTypes.Text.SpecialSymbols.DotSymbol)
                            : ""0"";
                    }

                    if (z.Imaginary == 0)
                        return z.Real.ToMathString();
                    return (z.Imaginary > 0 || double.IsNaN(z.Imaginary))
                        ? string.Format(""{0}+{1}{2}i"", z.Real.ToMathString(), z.Imaginary.ToMathString(),
                            Computator.NET.DataTypes.Text.SpecialSymbols.DotSymbol)
                        : string.Format(""{0}{1}{2}i"", z.Real.ToMathString(), z.Imaginary.ToMathString(),
                            Computator.NET.DataTypes.Text.SpecialSymbols.DotSymbol);
                default:
                    //case NumericalOutputNotationType.EngineeringNotation:
                    return z.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public static string ToMathString(this double x)
        {
            if (x == double.PositiveInfinity)
                return Computator.NET.DataTypes.Text.SpecialSymbols.Infinity;
            if (x == double.NegativeInfinity)
                return string.Format(""-{0}"", Computator.NET.DataTypes.Text.SpecialSymbols.Infinity);
            return x.ToString(System.Globalization.CultureInfo.InvariantCulture).ToMathString();
        }

        // Share decimal formatting with the host, then apply the script's notation.
        public static string ToMathString(this decimal x)
        {
            return Computator.NET.Core.Evaluation.DecimalDisplay.Format(x).ToMathString();
        }

        public static string ToMathString(this string str)
        {
            switch (Properties.Settings.Default.NumericalOutputNotation)
            {
                case Computator.NET.DataTypes.SettingsTypes.NumericalOutputNotationType.MathematicalNotation:
                    if (!str.Contains(""E"") && !str.Contains(""e""))
                        return str;
                    var chunks = str.Split('E', 'e');
                    var ret = string.Format(""{0}{1}10{2}"", chunks[0], Computator.NET.DataTypes.Text.SpecialSymbols.DotSymbol,
                        Computator.NET.DataTypes.Text.SpecialSymbols.AsciiToSuperscript(double.Parse(chunks[1]).ToString(System.Globalization.CultureInfo.InvariantCulture)));//we parse first then apply ToString() again to get rid of notations from engineering notation like eg +019 (when original walue was 1E+19) simple hack :)
                    return ret;
                case Computator.NET.DataTypes.SettingsTypes.NumericalOutputNotationType.EngineeringNotation:
                    return str;
                default:
                    return str;
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