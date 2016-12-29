using System.CodeDom.Compiler;

namespace Computator.NET.Core.Compilation
{
    public class NativeCompilerError : CompilerError
    {
        public CompilationErrorPlace ErrorPlace { get; set; }
    }
}