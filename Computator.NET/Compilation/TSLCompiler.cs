using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Computator.NET.DataTypes;

namespace Computator.NET.Compilation
{
    public enum TslCompilationMode
    {
        Simple,
        Scripting,
        Advanced
    }

    public class TslCompiler
    {
        public const string TypesAliases = @"
        using real = System.Double;
        using complex = System.Numerics.Complex;
        using natural = System.UInt64;
        using integer = System.Int64;";

        public static readonly string[] Keywords =
        {
            "real", "complex", "function", "var", "void", "Matrix", "string", "integer", "natural", "abstract", "as",
            "base", "break",
            "case", "catch", "checked", "continue", "default", "delegate", "do", "else", "event", "explicit", "extern",
            "false", "finally", "fixed", "for", "foreach", "goto", "if", "implicit", "in", "interface", "internal", "is",
            "lock", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private",
            "protected",
            "public", "readonly", "ref", "return", "sealed", "sizeof", "stackalloc", "switch", "this", "throw", "true",
            "try", "typeof", "unchecked", "unsafe", "using", "virtual", "while"
        };

        public static readonly string KeywordsList0 =
            "var abstract as base break case catch checked continue default delegate do else event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using virtual while";

        public static readonly string KeywordsList1 =
            "natural integer real complex function Matrix bool byte char class const decimal double enum float int long sbyte short static string struct uint ulong ushort void";

        public TslCompiler()
        {
            Variables = new List<string>();
            Version = 2.0;
            TslCompilationMode = TslCompilationMode.Simple;
        }

        public double Version { get; set; }
        public TslCompilationMode TslCompilationMode { get; set; }
        public List<string> Variables { get; set; }

        private string Normalize(string input) //TODO: make it real f(x,y) normalize
        {
            return ReplaceToDoubles(ReplacePow(ReplaceMultipling(input)));
        }

        public string TransformToCSharp(string tslCode)
        {
            var codeBuilder = new StringBuilder(tslCode);

            codeBuilder.Replace("matrix({", "matrix(new [,]{")
                .Replace("ᵀ", ".Transpose()")
                .Replace("read(&", "read(out ")
                .Replace("read( &", "read(out ")
                .Replace("(&", "( ref ");

            //TODO: this little thing
            // Func<double, double> fafa = (x) => x;
            //var ffff = new Function.Function(fafa,"fafa","fafa");

            var firstPhase = codeBuilder.ToString();

            //function[ ]+([a-zA-Z][a-zA-Z0-9_]*)\(((?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*[,])*(?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*)*[ ]*)\)[ ]*[=][ ]*([^;]+)
            var secondPhase = Regex.Replace(firstPhase,
                @"function[ ]+([a-zA-Z][a-zA-Z0-9_]*)\(((?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*[,])*(?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*)*[ ]*)\)[ ]*[=][ ]*([^;]+)",
                @"var $1 = TypeDeducer.Func(($2) => $3)");

            //var[ ]+([a-zA-Z][a-zA-Z0-9_]*)\(((?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*[,])*(?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*)*[ ]*)\)[ ]*[=][ ]*([^;]+)
            var thirdPhase = Regex.Replace(secondPhase,
                @"var[ ]+([a-zA-Z][a-zA-Z0-9_]*)\(((?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*[,])*(?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*)*[ ]*)\)[ ]*[=][ ]*([^;]+)",
                @"var $1 = TypeDeducer.Func(($2) => $3)");

            var fourthPhase = ReplacePow(thirdPhase);

            var fifthPhase = ReplaceToDoubles(fourthPhase);

            var sixthPhase = ReplaceMultipling(fifthPhase);

            var seventhPhase = sixthPhase.Replace(SpecialSymbols.DotSymbol, '*');

            return seventhPhase;
        }

        private string ReplaceMultipling(string input) //OK
        {
            //TODO: introduce good multiplying
            var result = input;
            // foreach (var c in Variables)
            //    result = Regex.Replace(result, @"(\d+\.?\d*)(" + c + @")", @"$1*$2");
            return result;
        }

        private string ReplacePow(string input) //OK
        {
            //string result = input.ReplacePow(@"(\d*x)\^(\d+\.?\d*)");
            //return result.ReplacePow(@"\(([^\^]+)\)\^(\d+\.?\d*)");
            var result = input;

            //case 1 - x^ANY_EXPONENT, 9.5y^ANY_EXPONENT, etc. 
            foreach (var c in Variables)
                result = Regex.Replace(result,
                    @"(\d*" + c + @")([" + SpecialSymbols.SuperscriptsWithoutSpace + @"]+)",
                    "pow($1,$2)");

            //case 2 - (anything in parenthesis)^ANY_EXPONENT
            // result = result.ReplacePow(@"\(([^\^]+)\)([" + GlobalConfig.Superscripts + @"]+)");
            result =
                Regex.Replace(result,
                    @"(\((?:[^()]|(?<open>\()|(?<-open>\)))+(?(open)(?!))\))([" +
                    SpecialSymbols.SuperscriptsWithoutSpace + @"]+)", "pow($1,$2)");
            //# First '(' # Match all non-braces # Match '(', and capture into 'open' # Match ')', and delete the 'open' capture # Fails if 'open' stack isn't empty!

            //case 3 - (any ordinary number)^ANY_EXPONENT
            result = Regex.Replace(result,
                @"(\d+\.?\d*)([" + SpecialSymbols.SuperscriptsWithoutSpace + @"]+)",
                "pow($1,$2)");

            //replace superscripts with normal characters:
            result = SpecialSymbols.SuperscriptsToAscii(result);

            return ReplaceMultipling(result);
        }

        private string ReplaceToDoubles(string input)
        {
            //return Regex.Replace(input, @"(\d+)(?:[^\.]\d+)", "$1.0");

            //1. X/5 => X/5.0
            var ret = Regex.Replace(input, @"[\/](\d+)([^\.\d]|$)", @"/$1.0$2");
            //System.Windows.Forms.MessageBox.Show(ret);
            //2. X/(4+9+...+i) => X/(1.0*(4+9+...+i)) 
            ret = Regex.Replace(ret,
                @"[\/](\((?:[^()]|(?<open>\()|(?<-open>\)))+(?(open)(?!))\))", "/(1.0*($1))");
            //System.Windows.Forms.MessageBox.Show(ret);
            return ret;
        }
    }
}