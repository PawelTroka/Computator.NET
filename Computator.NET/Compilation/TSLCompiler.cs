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


        private static readonly string powerCatchingGroup = @"([" + SpecialSymbols.SuperscriptsWithoutSpace + @"]+)";


        private const string validVariableDeclaration = @"([α-ωΑ-Ωa-zA-Z_][α-ωΑ-Ωa-z0-9A-Z_]*)";

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

        //  public TslCompilationMode TslCompilationMode { get; set; }
        //  public List<string> Variables { get; set; }


        private readonly Regex functionRegex =
            new Regex(
                @"function[ ]+([a-zA-Z][a-zA-Z0-9_]*)\(((?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*[,])*(?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*)*[ ]*)\)[ ]*[=][ ]*([^;]+)",
                RegexOptions.Compiled);


        private readonly Regex matrixRegex = new Regex(@"matrix\s*\(\s*\{", RegexOptions.Compiled);

        private readonly Regex multiplyingRegex =
            new Regex(
                @"((?:[^α-ωΑ-Ωa-zA-Z_\d\.][^α-ωΑ-Ωa-z0-9A-Z_]*)|^)(\d+\.?\d*)((?:[α-ωΑ-Ωa-zA-Z_][α-ωΑ-Ωa-z0-9A-Z_]*))",
                RegexOptions.Compiled);

        private readonly Regex numberRaisedToAnyPowerRegex = new Regex(@"(\d+\.?\d*)" + powerCatchingGroup,
            RegexOptions.Compiled);


        private readonly Regex readOutRegex = new Regex(@"(read\s*\(\s*)&", RegexOptions.Compiled);

        private readonly Regex refRegex = new Regex(@"([\(,\s])(&)([α-ωΑ-Ωa-zA-Z_])", RegexOptions.Compiled);

        private readonly Regex varFunctionRegex =
            new Regex(
                @"var[ ]+([a-zA-Z][a-zA-Z0-9_]*)\(((?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*[,])*(?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*)*[ ]*)\)[ ]*[=][ ]*([^;]+)",
                RegexOptions.Compiled);

        private readonly Regex variableRaisedToAnyPowerRegex =
            new Regex(
                validVariableDeclaration + powerCatchingGroup + @"([^\(" + SpecialSymbols.SuperscriptsWithoutSpace +
                @"]|$)", RegexOptions.Compiled);

        public TslCompiler()
        {
            //Variables = new List<string>();
            Version = 3.5;
            // TslCompilationMode = TslCompilationMode.Simple;
        }

        public double Version { get; private set; }

        public string TransformToCSharp(string tslCode)
        {
            var listOfResults = new List<string> {tslCode};

            listOfResults.Add(matrixRegex.Replace(listOfResults.Last(), @"matrix(new [,]{"));

            listOfResults.Add(listOfResults.Last().Replace("ᵀ", ".Transpose()"));


            listOfResults.Add(readOutRegex.Replace(listOfResults.Last(), "$1 out "));

            listOfResults.Add(refRegex.Replace(listOfResults.Last(), @"$1 ref $3"));


            //TODO: this little thing
            // Func<double, double> fafa = (x) => x;
            //var ffff = new Function.Function(fafa,"fafa","fafa");

            listOfResults.Add(ReplaceLocalFunctions(listOfResults.Last()));

            listOfResults.Add(ReplacePow(listOfResults.Last()));

            listOfResults.Add(ReplaceToDoubles(listOfResults.Last()));


            //  var engineeringNotationMatches = engineeringNotationRegex.Matches(listOfResults.Last());


            listOfResults.Add(ReplaceMultiplying(listOfResults.Last()));

            listOfResults.Add(listOfResults.Last().Replace(SpecialSymbols.DotSymbol, '*'));

            return listOfResults.Last();
        }

        private string ReplaceLocalFunctions(string code)
        {
            const string nativeCompilerCompatibleLocalFunctionDeclaration = @"var $1 = TypeDeducer.Func(($2) => $3)";

            var secondPhase = functionRegex.Replace(code, nativeCompilerCompatibleLocalFunctionDeclaration);

            //function[ ]+([a-zA-Z][a-zA-Z0-9_]*)\(((?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*[,])*(?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*)*[ ]*)\)[ ]*[=][ ]*([^;]+)


            //var secondPhase = Regex.Replace(firstPhase,
            //     @"function[ ]+([a-zA-Z][a-zA-Z0-9_]*)\(((?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*[,])*(?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*)*[ ]*)\)[ ]*[=][ ]*([^;]+)",
            //     @"var $1 = TypeDeducer.Func(($2) => $3)");


            //var[ ]+([a-zA-Z][a-zA-Z0-9_]*)\(((?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*[,])*(?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*)*[ ]*)\)[ ]*[=][ ]*([^;]+)


            // var thirdPhase = Regex.Replace(secondPhase,
            //   @"var[ ]+([a-zA-Z][a-zA-Z0-9_]*)\(((?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*[,])*(?:[ ]*[a-zA-Z][a-zA-Z0-9_]*[ ]+[a-zA-Z][a-zA-Z0-9_]*[ ]*)*[ ]*)\)[ ]*[=][ ]*([^;]+)",
            // @"var $1 = TypeDeducer.Func(($2) => $3)");


            var thirdPhase = varFunctionRegex.Replace(secondPhase, nativeCompilerCompatibleLocalFunctionDeclaration);
            return thirdPhase;
        }

        private string ReplaceMultiplying(string input) //OK
        {
            var result = engineeringNotationRegex.Replace(input,
                @"{{ENGINERING#NOTATION}$1#$2{ENGINERING#NOTATION}$3{ENGINERING#NOTATION}}");

            result = multiplyingRegex.Replace(result, "$1$2*$3");
            /* delegate (Match match)
            {

                string v = match.Result("$1$2$3");//match.ToString();

                if (engineeringNotationRegex.IsMatch(v))//we don't wanna change engineering notation
                    return v;

                return match.Result(@"$1$2*$3");
            });*/

            result = changeBackEngineeringNotationRegex.Replace(result, @"$1$2$3");
            return result;
        }

        private string ReplacePow(string input) //OK
        {
            var nativeCompilerCompatiblePowerNotation = @"pow($1,$2)";
            //string result = input.ReplacePow(@"(\d*x)\^(\d+\.?\d*)");
            //return result.ReplacePow(@"\(([^\^]+)\)\^(\d+\.?\d*)");
            //var result = input;

            //case 1 - x^ANY_EXPONENT, 9.5y^ANY_EXPONENT, etc. 
            //   foreach (var c in Variables)
            //     result = Regex.Replace(result,
            //       @"(\d*" + c + @")([" + SpecialSymbols.SuperscriptsWithoutSpace + @"]+)",
            //     "pow($1,$2)");

            //case 2 - (anything in parenthesis)^ANY_EXPONENT
            // result = result.ReplacePow(@"\(([^\^]+)\)([" + GlobalConfig.Superscripts + @"]+)");

            // result =
            //   Regex.Replace(result,
            //     @"(\((?:[^()]|(?<open>\()|(?<-open>\)))+(?(open)(?!))\))([" +
            //   SpecialSymbols.SuperscriptsWithoutSpace + @"]+)", "pow($1,$2)");


            var result = expressionInParenthesesRaisedToAnyPowerRegex.Replace(input,
                nativeCompilerCompatiblePowerNotation);
            //# First '(' # Match all non-braces # Match '(', and capture into 'open' # Match ')', and delete the 'open' capture # Fails if 'open' stack isn't empty!

            //case 3 - (any ordinary number)^ANY_EXPONENT
            //     result = Regex.Replace(result,
            //      @"(\d+\.?\d*)([" + SpecialSymbols.SuperscriptsWithoutSpace + @"]+)",
            //   "pow($1,$2)");

            result = numberRaisedToAnyPowerRegex.Replace(result, nativeCompilerCompatiblePowerNotation);


            //([α-ωΑ-Ωa-zA-Z_][α-ωΑ-Ωa-z0-9A-Z_]*) // valid variable using latin and greek alphabet
            //case 4 - variable^ANY_EXPONENT
            //   foreach (var c in Variables)
            // using System.CodeDom.Compiler;
            // CodeDomProvider provider = CodeDomProvider.CreateProvider("C#");
            //    if (provider.IsValidIdentifier(YOUR_VARIABLE_NAME))
            //   {
            // Valid
            //     }
            //   result = Regex.Replace(result,
            //        @"([α-ωΑ-Ωa-zA-Z_][α-ωΑ-Ωa-z0-9A-Z_]*)([" + SpecialSymbols.SuperscriptsWithoutSpace + @"]+)",
            //    "pow($1,$2)");

            result = variableRaisedToAnyPowerRegex.Replace(result, nativeCompilerCompatiblePowerNotation + @"$3");


            //replace superscripts with normal characters:
            result = SpecialSymbols.SuperscriptsToAscii(result);

            return result;
        }

        private string ReplaceToDoubles(string input)
        {
            //return Regex.Replace(input, @"(\d+)(?:[^\.]\d+)", "$1.0");

            //1. X/5 => X/5.0
            // var ret = Regex.Replace(input, @"[\/](\d+)([^\.\d]|$)", @"/$1.0$2");

            var ret = divisionByIntegerRegex.Replace(input, @"/$1.0$2");


            //System.Windows.Forms.MessageBox.Show(ret);
            //2. X/(4+9+...+i) => X/(1.0*(4+9+...+i)) 
            //   ret = Regex.Replace(ret,
            //  @"[\/](\((?:[^()]|(?<open>\()|(?<-open>\)))+(?(open)(?!))\))", "/(1.0*($1))");
            //System.Windows.Forms.MessageBox.Show(ret);

            ret = divisionByParenthesisRegex.Replace(ret, @"/(1.0*($1))");

            return ret;
        }
    }
}