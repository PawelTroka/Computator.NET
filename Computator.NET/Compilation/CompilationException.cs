namespace Computator.NET.Compilation
{
    internal class CompilationException : System.Exception
    {
        public CompilationException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public static class ExceptionExtensions
    {
        public static bool IsInternal(this System.Exception ex)
        {
            return (ex is CompilationException || ex is DataTypes.CalculationException ||
                    ex is Evaluation.EvaluationException);
        }
    }
}