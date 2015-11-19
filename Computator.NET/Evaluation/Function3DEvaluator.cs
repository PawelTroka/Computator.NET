using Computator.NET.DataTypes;

namespace Computator.NET.Evaluation
{
    internal class Function3DEvaluator : Evaluator
    {
        public Function3DEvaluator()
        {
            logger.ClassName = GetType().FullName;
        }

        public Function Evaluate(string input, string customFunctionsCode = "")
        {
            tslCode = input;
            customFunctionsTSLCode = customFunctionsCode;

            functionType = input.Contains("=") ? FunctionType.Real3DImplicit : FunctionType.Real3D;

            var function = Compile();
            return new Function(function, tslCode, CSharpCode, functionType);
        }
    }
}