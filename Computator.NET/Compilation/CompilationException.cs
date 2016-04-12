using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Computator.NET.Compilation
{
    internal class CompilationException : Exception
    {
        public CompilationException(string message, Exception innerException)
            : base(message, innerException)
        {
            Errors = new Dictionary<CompilationErrorPlace, List<CompilerError>>
            {
                {CompilationErrorPlace.CustomFunctions, new List<CompilerError>()},
                {CompilationErrorPlace.Internal, new List<CompilerError>()},
                {CompilationErrorPlace.MainCode, new List<CompilerError>()}
            };
        }

        public CompilationException(string message)
            : base(message)
        {
            Errors = new Dictionary<CompilationErrorPlace, List<CompilerError>>
            {
                {CompilationErrorPlace.CustomFunctions, new List<CompilerError>()},
                {CompilationErrorPlace.Internal, new List<CompilerError>()},
                {CompilationErrorPlace.MainCode, new List<CompilerError>()}
            };
        }

        public CompilationException()
        {
            Errors = new Dictionary<CompilationErrorPlace, List<CompilerError>>
            {
                {CompilationErrorPlace.CustomFunctions, new List<CompilerError>()},
                {CompilationErrorPlace.Internal, new List<CompilerError>()},
                {CompilationErrorPlace.MainCode, new List<CompilerError>()}
            };
        }

        public Dictionary<CompilationErrorPlace, List<CompilerError>> Errors { get; set; }


        public bool HasInternalErrors
            =>
                Errors.ContainsKey(CompilationErrorPlace.Internal) &&
                Errors[CompilationErrorPlace.Internal].Any(er => !er.IsWarning);

        public bool HasMainCodeErrors
            =>
                Errors.ContainsKey(CompilationErrorPlace.MainCode) &&
                Errors[CompilationErrorPlace.MainCode].Any(er => !er.IsWarning);

        public bool HasCustomFunctionsErrors
            =>
                Errors.ContainsKey(CompilationErrorPlace.CustomFunctions) &&
                Errors[CompilationErrorPlace.CustomFunctions].Any(er => !er.IsWarning);
    }
}