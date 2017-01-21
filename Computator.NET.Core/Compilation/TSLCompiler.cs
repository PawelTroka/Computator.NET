using System.Collections.Generic;
using System.Linq;
using Computator.NET.DataTypes;

namespace Computator.NET.Core.Compilation
{
    public interface ITslCompiler
    {
        string TransformToCSharp(string tslCode);
    }

    public class TslCompiler : ITslCompiler
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


        private static readonly ITslHandler[] TslHandlers = {
            new MatrixHandler(),
            new RefOutHandler(),
            //new AbsHandler(), 
            new FunctionsHandler(),
            new PowHandler(),
            new DivisionHandler(),
            new MultiplyingHandler(),
            new SpecialSymbolsHandler(), 
        };
        
        
        public string TransformToCSharp(string tslCode)
        {
            return TslHandlers.Aggregate(tslCode, (current, tslHandler) => tslHandler.Replace(current));
        }
    }
}