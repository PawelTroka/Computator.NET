using System;
using System.CodeDom.Compiler;
using Computator.NET.DataTypes;
using Computator.NET.Evaluation;

namespace Computator.NET.Compilation
{
    public enum CompilationErrorPlace
    {
        MainCode,
        CustomFunctions,
        Internal,
    }

    public class NativeCompilerError : CompilerError
    {
     public CompilationErrorPlace ErrorPlace { get; set; }
    }

    internal class CompilationException : Exception
    {
        public CompilerErrorCollection Errors { get; set; }

        public CompilationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public static class ExceptionExtensions
    {
        public static bool IsInternal(this Exception ex)
        {
            return (ex is CompilationException || ex is CalculationException ||
                    ex is EvaluationException);
        }
    }
}