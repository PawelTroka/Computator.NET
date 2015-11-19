using Computator.NET.DataTypes;

namespace Computator.NET.Evaluation
{
    internal class FunctionComplexEvaluator : Evaluator
    {
        public FunctionComplexEvaluator()
        {
            logger.ClassName = GetType().FullName;
        }

        public Function Evaluate(string input, string customFunctionsCode = "")
        {
            tslCode = input;
            customFunctionsTSLCode = customFunctionsCode;

            functionType = input.Contains("=") ? FunctionType.ComplexImplicit : FunctionType.Complex;

            var function = Compile();
            return new Function(function, tslCode, CSharpCode, functionType);
        }
    }
}