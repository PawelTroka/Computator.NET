namespace Computator.NET.Evaluation
{
    internal class FunctionComplexEvaluator : Evaluator
    {
        public FunctionComplexEvaluator()
        {
            logger.ClassName = GetType().FullName;
        }

        public DataTypes.Function Evaluate(string input, string customFunctionsCode = "")
        {
            tslCode = input;
            customFunctionsTSLCode = customFunctionsCode;

            functionType = input.Contains("=") ? DataTypes.FunctionType.ComplexImplicit : DataTypes.FunctionType.Complex;

            var function = Compile();
            return new DataTypes.Function(function, tslCode, CSharpCode, functionType);
        }
    }
}