using System;

namespace Computator.NET.Core.Evaluation
{
    public class EvaluationException : Exception
    {
        public EvaluationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}