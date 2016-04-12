using System.CodeDom.Compiler;

namespace Computator.NET.Compilation
{
    public class NativeCompilerError : CompilerError
    {
        public CompilationErrorPlace ErrorPlace { get; set; }
    }
}