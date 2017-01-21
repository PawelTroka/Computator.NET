using Computator.NET.DataTypes;

namespace Computator.NET.Core.Compilation
{
    public class SpecialSymbolsHandler : ITslHandler
    {
        public string Replace(string code)
        {
            return code.Replace(SpecialSymbols.DotSymbol, '*')
                .Replace(SpecialSymbols.Infinity, "double.PositiveInfinity");
        }
    }
}