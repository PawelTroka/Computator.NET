using Computator.NET.DataTypes.Text;

namespace Computator.NET.Core.Compilation.TslHandlers
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