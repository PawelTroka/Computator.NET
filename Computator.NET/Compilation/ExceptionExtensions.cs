using System;
using Computator.NET.DataTypes;
using Computator.NET.Evaluation;

namespace Computator.NET.Compilation
{
    public static class ExceptionExtensions
    {
        public static bool IsInternal(this Exception ex)
        {
            return ex is CompilationException || ex is CalculationException ||
                   ex is EvaluationException;
        }
    }
}