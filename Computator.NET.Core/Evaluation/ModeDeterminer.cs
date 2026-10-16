using System;
using Computator.NET.Core.Compilation;
using Computator.NET.DataTypes;

namespace Computator.NET.Core.Evaluation
{
    public interface IModeDeterminer
    {
        CalculationsMode DetermineMode(string input);
    }

    public class ModeDeterminer : IModeDeterminer
    {
        private readonly ITslCompiler _tslCompiler;

        public ModeDeterminer(ITslCompiler tslCompiler)
        {
            _tslCompiler = tslCompiler ?? throw new ArgumentNullException(nameof(tslCompiler));
        }

        public CalculationsMode DetermineMode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return CalculationsMode.Error;

            var expression = ExpressionAnalysis.Parse(_tslCompiler.TransformToCSharp(input));
            if (expression.IsEmpty)
                return CalculationsMode.Error;

            var isZ = expression.ContainsVariable("z");
            var isI = expression.ContainsVariable("i");
            var isY = expression.ContainsVariable("y");

            if (!expression.IsImplicit)
            {
                if (isZ || isI)
                    return CalculationsMode.Complex;
                return isY ? CalculationsMode.Fxy : CalculationsMode.Real;
            }

            if (isI)
                return CalculationsMode.Error; // Implicit complex functions are not supported.

            return isZ && isY ? CalculationsMode.Fxy : CalculationsMode.Real;
        }
    }
}
