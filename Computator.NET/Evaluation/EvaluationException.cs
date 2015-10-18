using System;

namespace Computator.NET.Evaluation
{
    internal class EvaluationException : Exception
    {
        public EvaluationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}