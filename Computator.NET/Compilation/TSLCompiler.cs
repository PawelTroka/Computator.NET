using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Computator.NET.DataTypes;

namespace Computator.NET.Compilation
{
    /* public enum TslCompilationMode
    {
        Simple,
        Scripting,
        Advanced
    }*/

    public class TslCompiler
    {
        public const string TypesAliases = @"
        using real = System.Double;
        using complex = System.Numerics.Complex;
        using natural = System.UInt64;
        using integer = System.Int64;";


      //  private static readonly string powerCatchingGroup = @"([" + SpecialSymbols.SuperscriptsWithoutSpace + @"]+)";


        private static readonly string powerCatchingGroup =
            $@"(\s*[{SpecialSymbols.SuperscriptsWithoutSpace}][{SpecialSymbols.Superscripts}]*)((?:(?:[^{SpecialSymbols.Superscripts}])|$))";


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



      //  private string legalNames = 

        private readonly Regex changeBackEngineeringNotationRegex =
            new Regex(
                @"{{ENGINERING#NOTATION}(\d+\.?\d*)#([Ee][+-]?\d+){ENGINERING#NOTATION}([^\dα-ωΑ-Ωa-zA-Z_.]*){ENGINERING#NOTATION}}",
                RegexOptions.Compiled);


        private readonly Regex divisionByIntegerRegex = new Regex(@"[\/]\s*(\d+)([^\.\d]|$)", RegexOptions.Compiled);

        private readonly Regex divisionByParenthesisRegex =
            new Regex(@"[\/]\s*(\((?:[^()]|(?<open>\()|(?<-open>\)))+(?(open)(?!))\))", RegexOptions.Compiled);


        private readonly Regex engineeringNotationRegex =
            new Regex(@"(\d+\.?\d*)([Ee][+-]?\d+)([^\dα-ωΑ-Ωa-zA-Z_.]|$)", RegexOptions.Compiled);

        private readonly Regex expressionInParenthesesRaisedToAnyPowerRegex =
            new Regex(@"(\((?:[^()]|(?<open>\()|(?<-open>\)))+(?(open)(?!))\))" + powerCatchingGroup,
                RegexOptions.Compiled);


        private readonly Regex absRegex =
            new Regex(@"\|((?:[^|]|(?<open>\|)|(?<-open>\|))+(?(open)(?!)))\|",
                RegexOptions.Compiled);



        private const string legalFirstIdentifier = @"α-ωΑ-Ωa-zA-Z_";

        private const string identifier = @"[α-ωΑ-Ωa-zA-Z_][α-ωΑ-Ωa-z0-9A-Z_]*";


        private readonly Regex functionRegex =
            new Regex(
                $@"(?:var|function)\s+({identifier})\(((?:\s*{identifier}\s+{identifier}\s*,)*(?:\s*{identifier}\s+{identifier}\s*)*\s*)\)\s*[=]\s*([^;]+)",
                RegexOptions.Compiled);


        private readonly Regex matrixRegex = new Regex(@"matrix\s*\(\s*\{", RegexOptions.Compiled);

        private readonly Regex multiplyingRegex =
            new Regex(
                $@"((?:[^α-ωΑ-Ωa-zA-Z_\d\.][^α-ωΑ-Ωa-z0-9A-Z_]*)|^)(\d+\.?\d*)((?:{identifier}))",
                RegexOptions.Compiled);

        private readonly Regex numberRaisedToAnyPowerRegex = new Regex($@"(\d+\.?\d*){powerCatchingGroup}",
            RegexOptions.Compiled);


        private readonly Regex readOutRegex = new Regex(@"(read\s*\(\s*)&", RegexOptions.Compiled);

        private readonly Regex refRegex = new Regex(@"([\(,\s])(&)([α-ωΑ-Ωa-zA-Z_])", RegexOptions.Compiled);


        private readonly Regex variableRaisedToAnyPowerRegex =
            new Regex(
                $@"({identifier}){powerCatchingGroup}", RegexOptions.Compiled);

        public TslCompiler()
        {
            Version = 3.7m;
        }

        public decimal Version { get; private set; }

        public string TransformToCSharp(string tslCode)
        {
            var listOfResults = new List<string> {tslCode};

            listOfResults.Add(matrixRegex.Replace(listOfResults.Last(), @"matrix(new [,]{"));

            listOfResults.Add(listOfResults.Last().Replace("ᵀ", ".Transpose()"));


            listOfResults.Add(readOutRegex.Replace(listOfResults.Last(), "$1 out "));

            listOfResults.Add(refRegex.Replace(listOfResults.Last(), @"$1 ref $3"));



           // listOfResults.Add(ReplaceAbs(listOfResults.Last()));

            //TODO: this little thing
            // Func<double, double> fafa = (x) => x;
            //var ffff = new Function.Function(fafa,"fafa","fafa");

            listOfResults.Add(ReplaceLocalFunctions(listOfResults.Last()));

            listOfResults.Add(ReplacePow(listOfResults.Last()));

            listOfResults.Add(ReplaceToDoubles(listOfResults.Last()));


            listOfResults.Add(ReplaceMultiplying(listOfResults.Last()));

            listOfResults.Add(listOfResults.Last().Replace(SpecialSymbols.DotSymbol, '*'));

            listOfResults.Add(listOfResults.Last().Replace(SpecialSymbols.Infinity, "double.PositiveInfinity"));

            return listOfResults.Last();
        }

        private string ReplaceAbs(string code)
        {
            var result = code;
            
            while (result.Count(c => c == '|') > 0 && result.Count(c => c == '|')%2 == 0)
                result = absRegex.Replace(result, @"abs($1)");
            return result;
        }

        private string ReplaceLocalFunctions(string code)
        {
            var secondPhase = functionRegex.Replace(code, @"var $1 = TypeDeducer.Func(($2) => $3)");
            return secondPhase;
        }

        private string ReplaceMultiplying(string input) //OK
        {
            var result = engineeringNotationRegex.Replace(input,
                @"{{ENGINERING#NOTATION}$1#$2{ENGINERING#NOTATION}$3{ENGINERING#NOTATION}}");

            result = multiplyingRegex.Replace(result, "$1$2*$3");

            result = changeBackEngineeringNotationRegex.Replace(result, @"$1$2$3");
            return result;
        }

        private string ReplacePow(string input) //OK
        {
            var nativeCompilerCompatiblePowerNotation = @"pow($1,$2)$3";

            var result = input;

            while(expressionInParenthesesRaisedToAnyPowerRegex.IsMatch(result))
                result = expressionInParenthesesRaisedToAnyPowerRegex.Replace(result,//http://stackoverflow.com/questions/7898310/using-regex-to-balance-match-parenthesis
                nativeCompilerCompatiblePowerNotation);

            result = numberRaisedToAnyPowerRegex.Replace(result, nativeCompilerCompatiblePowerNotation);

            result = variableRaisedToAnyPowerRegex.Replace(result, nativeCompilerCompatiblePowerNotation);

            result = SpecialSymbols.SuperscriptsToAscii(result);

            return result;
        }

        private string ReplaceToDoubles(string input)
        {
            var ret = divisionByIntegerRegex.Replace(input, @"/$1.0$2");

            ret = divisionByParenthesisRegex.Replace(ret, @"/(1.0*($1))");

            return ret;
        }
    }
}