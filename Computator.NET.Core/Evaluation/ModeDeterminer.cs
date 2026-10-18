using System;
using Computator.NET.Core.Compilation;
using Computator.NET.DataTypes;

namespace Computator.NET.Core.Evaluation
{
    public interface IModeDeterminer
    {
        CalculationsMode DetermineMode(string input);
        CalculationsMode DetermineMode(string input, string customFunctionsCode) => DetermineMode(input);
    }

    public class ModeDeterminer : IModeDeterminer
    {
        private readonly ITslCompiler _tslCompiler;

        public ModeDeterminer(ITslCompiler tslCompiler)
        {
            _tslCompiler = tslCompiler ?? throw new ArgumentNullException(nameof(tslCompiler));
        }

        public CalculationsMode DetermineMode(string input) => DetermineMode(input, string.Empty);

        public CalculationsMode DetermineMode(string input, string customFunctionsCode)
        {
            if (string.IsNullOrWhiteSpace(input))
                return CalculationsMode.Error;

            var cSharp = _tslCompiler.TransformToCSharp(input, customFunctionsCode ?? "", true);
            var expression = ExpressionAnalysis.Parse(cSharp);
            if (expression.IsEmpty)
                return CalculationsMode.Error;

            var inputs = expression.HasFunctionPowers
                ? new ExpressionsEvaluator().AnalyzeFunctionPowers(cSharp, customFunctionsCode ?? "", expression.IsImplicit)
                : expression;
            var isZ = inputs.ContainsVariable("z");
            var isI = inputs.ContainsVariable("i");
            var isY = inputs.ContainsVariable("y");

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
