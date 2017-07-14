using System.Text.RegularExpressions;

namespace Computator.NET.Core.Compilation.TslHandlers
{
    public class MatrixHandler : ITslHandler
    {
        private static readonly Regex MatrixRegex = new Regex(@"matrix\s*\(\s*\{", RegexOptions.Compiled);

        public string Replace(string code)
        {
            return MatrixRegex.Replace(code, @"matrix(new [,]{")
                .Replace("ᵀ", ".Transpose()");
        }
    }
}