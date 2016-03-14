using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Computator.NET.Config;
using Computator.NET.Localization;
using Computator.NET.Logging;
using Microsoft.CSharp;

//findRoot(sin,x-1,x+1) is really interesting

namespace Computator.NET.Compilation
{
    public class NativeCompiler : CSharpCodeProvider
    {
        private readonly SimpleLogger logger;
        private readonly CompilerParameters parameters;
        public int MainCodeStarOffsetLine { get; set; }
        public int MainCodeEndOffsetLine { get; set; }

        public int CustomFunctionsStartOffsetLine { get; set; }
        public int CustomFunctionsEndOffsetLine { get; set; }




        private int GetLineForPlace(int line, CompilationErrorPlace place)
        {
            if (place == CompilationErrorPlace.CustomFunctions)
                return line - CustomFunctionsStartOffsetLine;
            else if (place == CompilationErrorPlace.MainCode)
                return line - MainCodeStarOffsetLine;
            else
                return line;
        }

        private bool IsLineInPlace(int line, CompilationErrorPlace place)
        {
            if (place == CompilationErrorPlace.MainCode)
                return line >= MainCodeStarOffsetLine && line <= MainCodeEndOffsetLine;
            else if (place == CompilationErrorPlace.CustomFunctions)
                return line >= CustomFunctionsStartOffsetLine && line <= CustomFunctionsEndOffsetLine;
            else
                return true;
        }

        private CompilationErrorPlace GetPlaceForLine(int line)
        {
            return (line >= MainCodeStarOffsetLine && line <= MainCodeEndOffsetLine)
                ? CompilationErrorPlace.MainCode
                : (line >= CustomFunctionsStartOffsetLine && line <= CustomFunctionsEndOffsetLine
                    ? CompilationErrorPlace.CustomFunctions
                    : CompilationErrorPlace.Internal);
        }

        public NativeCompiler()
        {
            logger = new SimpleLogger { ClassName = GetType().FullName };
            parameters = new CompilerParameters
            {
                GenerateInMemory = true,
                TempFiles = { KeepFiles = false }
            };
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Numerics.dll");
            parameters.ReferencedAssemblies.Add(GlobalConfig.FullPath("Meta.Numerics.dll"));
            parameters.ReferencedAssemblies.Add(GlobalConfig.FullPath("MathNet.Numerics.dll"));
            parameters.ReferencedAssemblies.Add(GlobalConfig.FullPath("Accord.Math.dll"));
            parameters.ReferencedAssemblies.Add(GlobalConfig.FullPath("Accord.dll"));
            parameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");//dynamic
        }

        public Assembly Compile(string input)
        {
            var results = CompileAssemblyFromSource(parameters, input);

            if (results.Errors.Count > 0)
            {

                var message = $"Syntax error{Environment.NewLine}{Strings.Details}";
                var compilationException = new CompilationException();

                // var compilerErrors = new CompilerErrorCollection();
                foreach (CompilerError error in results.Errors)
                {
                    var placeOfError = GetPlaceForLine(error.Line);

                    error.Line = GetLineForPlace(error.Line, placeOfError);

                    compilationException.Errors[placeOfError].Add(error);
                }

                if (compilationException.HasMainCodeErrors)
                {
                    message += Environment.NewLine + " Code errors:";
                    foreach (var error in compilationException.Errors[CompilationErrorPlace.MainCode])
                    {
                        message +=
                            $"{Environment.NewLine}(Ln: {error.Line} Col: {error.Column}):{(error.IsWarning ? " warning " : " error ")}{error.ErrorNumber}: {error.ErrorText}";
                    }
                }


                if (compilationException.HasCustomFunctionsErrors)
                {
                    message += Environment.NewLine + " Custom functions errors:";
                    foreach (var error in compilationException.Errors[CompilationErrorPlace.CustomFunctions])
                    {
                        message +=
                            $"{Environment.NewLine}(Ln: {error.Line} Col: {error.Column}):{(error.IsWarning ? " warning " : " error ")}{error.ErrorNumber}: {error.ErrorText}";
                    }
                }

                if (compilationException.HasInternalErrors)//if there is any warning in our internal code that means we are aware of it and we dont wanna show it to user :)
                {
                    message += Environment.NewLine + " Internal errors:";
                    foreach (var error in compilationException.Errors[CompilationErrorPlace.Internal])
                    {
                        message +=
                            $"{Environment.NewLine}(Ln: {error.Line} Col: {error.Column}):{(error.IsWarning ? " warning " : " error ")}{error.ErrorNumber}: {error.ErrorText}";
                    }
                }

                compilationException = new CompilationException(message) { Errors = compilationException.Errors };
                // compilationException.Message = message;
                // var ex = new CompilationException(message) {Errors = compilerErrors};

                logger.MethodName = MethodBase.GetCurrentMethod().Name;
                // Logger.Parameters["NativeCompilerInput"] = input;
                logger.Parameters["NativeCompilerOutput"] = "";
                foreach (var str in results.Output)
                    logger.Parameters["NativeCompilerOutput"] += str + Environment.NewLine;
                logger.Log(message, ErrorType.Compilation, compilationException);

                throw compilationException;
            }

            parameters.TempFiles.KeepFiles = false;
            parameters.TempFiles.Delete();
            results.TempFiles.KeepFiles = false;
            results.TempFiles.Delete();

            return results.CompiledAssembly;
        }

        public void AddDll(string dllPath)
        {
            parameters.ReferencedAssemblies.Add(dllPath);
        }
    }
}