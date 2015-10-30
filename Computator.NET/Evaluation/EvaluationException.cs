namespace Computator.NET.Evaluation
{
    internal class EvaluationException : System.Exception
    {
        public EvaluationException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}