using System;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes;

namespace Computator.NET.Core.Compilation
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