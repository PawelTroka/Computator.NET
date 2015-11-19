using Computator.NET.DataTypes;

namespace Computator.NET.Evaluation
{
    internal class Function2DEvaluator : Evaluator
    {
        public Function2DEvaluator()
        {
            logger.ClassName = GetType().FullName;
        }

        //(3.3+2.5x)¹²³+3.5x²+2¹²²⁻¹⁺ˢⁱⁿ⁽ˣ⁾⁺²

        public Function Evaluate(string input, string customFunctionsCode = "")
        {
            tslCode = input;
            customFunctionsTSLCode = customFunctionsCode;

            functionType = input.Contains("=") ? FunctionType.Real2DImplicit : FunctionType.Real2D;

            var function = Compile();
            return new Function(function, tslCode, CSharpCode, functionType);
        }
    }
}