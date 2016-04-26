using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Logging;
using Microsoft.CSharp;

//findRoot(sin,x-1,x+1) is really interesting

namespace Computator.NET.Compilation
{
    public class NativeCompiler : CSharpCodeProvider
    {
        private readonly SimpleLogger logger;
        private readonly CompilerParameters parameters;

        public NativeCompiler()
        {
            logger = new SimpleLogger {ClassName = GetType().FullName};
            parameters = new CompilerParameters
            {
                GenerateInMemory = true,
                TempFiles = {KeepFiles = false}
            };
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Numerics.dll");
            parameters.ReferencedAssemblies.Add(GetDllPath("Meta.Numerics.dll"));
            parameters.ReferencedAssemblies.Add(GetDllPath("MathNet.Numerics.dll"));
            parameters.ReferencedAssemblies.Add(GetDllPath("Accord.Math.dll"));
            parameters.ReferencedAssemblies.Add(GetDllPath("Accord.dll"));
            parameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll"); //dynamic
        }

        public int MainCodeStarOffsetLine { get; set; }
        public int MainCodeEndOffsetLine { get; set; }

        public int CustomFunctionsStartOffsetLine { get; set; }
        public int CustomFunctionsEndOffsetLine { get; set; }

        public bool IsScripting { get; set; } = false;

        private string GetDllPath(string dllName)
        {
            var dllDirectory = GlobalConfig.FullPath(dllName);
            if (File.Exists(dllDirectory))
                return dllDirectory;

            var path = GlobalConfig.FullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), dllName);
            if (path.Contains(@"\file:\")) //hack for tests
                return path.Split(new[] {@"\file:\"}, StringSplitOptions.RemoveEmptyEntries).Last();
            return path;
        }


        private int GetLineForPlace(int line, CompilationErrorPlace place)
        {
            if (place == CompilationErrorPlace.CustomFunctions)
                return line - CustomFunctionsStartOffsetLine;
            if (place == CompilationErrorPlace.MainCode)
                return line - MainCodeStarOffsetLine;
            return line;
        }

        private bool IsLineInPlace(int line, CompilationErrorPlace place)
        {
            if (place == CompilationErrorPlace.MainCode)
                return line >= MainCodeStarOffsetLine && line <= MainCodeEndOffsetLine;
            if (place == CompilationErrorPlace.CustomFunctions)
                return line >= CustomFunctionsStartOffsetLine && line <= CustomFunctionsEndOffsetLine;
            return true;
        }

        private CompilationErrorPlace GetPlaceForLine(int line)
        {
            return line >= MainCodeStarOffsetLine && line <= MainCodeEndOffsetLine
                ? CompilationErrorPlace.MainCode
                : (line >= CustomFunctionsStartOffsetLine && line <= CustomFunctionsEndOffsetLine
                    ? CompilationErrorPlace.CustomFunctions
                    : CompilationErrorPlace.Internal);
        }

        public Assembly Compile(string input)
        {
            var results = CompileAssemblyFromSource(parameters, input);

            if (results.Errors.Count > 0)
            {
                var message =
                    new StringBuilder(
                        $"{Strings.NativeCompiler_Compile_Syntax_error}{Environment.NewLine}{Strings.Details}");
                var compilationException = CreateCompilationExceptionFromCompilerResults(results);

                if (compilationException.HasMainCodeErrors)
                {
                    message.Append(
                        $"{Environment.NewLine} {(IsScripting ? Strings.NativeCompiler_Compile_Script_errors : Strings.NativeCompiler_Compile_Expression_errors)}:");
                    message.Append(errorCollectionToString(compilationException.Errors[CompilationErrorPlace.MainCode]));
                }

                if (compilationException.HasCustomFunctionsErrors)
                {
                    message.Append($"{Environment.NewLine} {Strings.NativeCompiler_Compile_Custom_functions_errors}:");
                    message.Append(
                        errorCollectionToString(compilationException.Errors[CompilationErrorPlace.CustomFunctions]));
                }

                if (compilationException.HasInternalErrors)
                    //if there is any warning in our internal code that means we are aware of it and we dont wanna show it to user :)
                {
                    message.Append($"{Environment.NewLine} {Strings.NativeCompiler_Compile_Internal_errors}:");
                    message.Append(errorCollectionToString(compilationException.Errors[CompilationErrorPlace.Internal]));
                }

                compilationException = new CompilationException(message.ToString())
                {
                    Errors = compilationException.Errors
                };
                // compilationException.Message = message;
                // var ex = new CompilationException(message) {Errors = compilerErrors};

                logger.MethodName = MethodBase.GetCurrentMethod().Name;
                // Logger.Parameters["NativeCompilerInput"] = input;
                logger.Parameters["NativeCompilerOutput"] = "";
                foreach (var str in results.Output)
                    logger.Parameters["NativeCompilerOutput"] += str + Environment.NewLine;
                logger.Log(compilationException.Message, ErrorType.Compilation, compilationException);

                throw compilationException;
            }

            parameters.TempFiles.KeepFiles = false;
            parameters.TempFiles.Delete();
            results.TempFiles.KeepFiles = false;
            results.TempFiles.Delete();

            return results.CompiledAssembly;
        }

        private CompilationException CreateCompilationExceptionFromCompilerResults(CompilerResults results)
        {
            var compilationException = new CompilationException();

            // var compilerErrors = new CompilerErrorCollection();
            foreach (CompilerError error in results.Errors)
            {
                var placeOfError = GetPlaceForLine(error.Line);
                error.Line = GetLineForPlace(error.Line, placeOfError);
                compilationException.Errors[placeOfError].Add(error);
            }
            return compilationException;
        }

        private static string errorCollectionToString(IEnumerable<CompilerError> errorCollection)
        {
            var message = new StringBuilder();
            foreach (var error in errorCollection)
            {
                message.Append(CompilerErrorToString(error));
            }
            return message.ToString();
        }

        private static string CompilerErrorToString(CompilerError error)
        {
            var war = Strings.NativeCompiler_CompilerErrorToString_warning;
            var err = Strings.NativeCompiler_CompilerErrorToString_error;

            return
                $"{Environment.NewLine}  ({Strings.NativeCompiler_CompilerErrorToString_Line}: {error.Line} {Strings.NativeCompiler_CompilerErrorToString_Column}: {error.Column}):{(error.IsWarning ? " " + Strings.NativeCompiler_CompilerErrorToString_warning + " " : " " + Strings.NativeCompiler_CompilerErrorToString_error + " ")}{error.ErrorNumber}: {error.ErrorText}";
        }

        public void AddDll(string dllPath)
        {
            parameters.ReferencedAssemblies.Add(dllPath);
        }
    }
}