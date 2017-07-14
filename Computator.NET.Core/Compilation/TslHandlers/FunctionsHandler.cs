//#define _COMPILE_TO_FUNCTION_CLASS_INSTEAD_OF_FUNC_DELEGATE

using System.Text.RegularExpressions;

namespace Computator.NET.Core.Compilation.TslHandlers
{
    public class FunctionsHandler : ITslHandler
    {
        private static readonly Regex FunctionRegex =
            new Regex(
                $@"(?:var|function)\s+({Groups.Identifier})\(((?:\s*{Groups.Identifier}\s+{Groups.Identifier}\s*,)*(?:\s*{Groups.Identifier}\s+{
                    Groups.Identifier}\s*)*\s*)\)\s*[=]\s*([^;]+)",
                RegexOptions.Compiled);
        public string Replace(string code)
        {
            //TODO: this little thing
            // Func<double, double> fafa = (x) => x;
            //var ffff = new Function.Function(fafa,"fafa","fafa");

#if _COMPILE_TO_FUNCTION_CLASS_INSTEAD_OF_FUNC_DELEGATE
            var secondPhase = functionRegex.Replace(code, @"var $1 = new Computator.NET.DataTypes.Function(TypeDeducer.Func(($2) => $3),""$3"",null,false)");
#else
            var secondPhase = FunctionRegex.Replace(code, @"var $1 = TypeDeducer.Func(($2) => $3)");
#endif
            return secondPhase;
        }
    }
}