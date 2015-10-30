namespace Computator.NET.Evaluation
{
    internal class Function3DEvaluator : Evaluator
    {
        public Function3DEvaluator()
        {
            logger.ClassName = GetType().FullName;
        }

        public DataTypes.Function Evaluate(string input, string customFunctionsCode = "")
        {
            tslCode = input;
            customFunctionsTSLCode = customFunctionsCode;

            functionType = input.Contains("=") ? DataTypes.FunctionType.Real3DImplicit : DataTypes.FunctionType.Real3D;

            var function = Compile();
            return new DataTypes.Function(function, tslCode, CSharpCode, functionType);
        }
    }
}