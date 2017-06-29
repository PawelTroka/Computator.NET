using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Computator.NET.DataTypes;
using Computator.NET.Localization;
using Microsoft.CSharp;
using NLog;

//findRoot(sin,x-1,x+1) is really interesting

namespace Computator.NET.Core.Compilation
{
    public class NativeCompiler : CSharpCodeProvider
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly CompilerParameters _parameters;

        public NativeCompiler()
        {
            _parameters = new CompilerParameters
            {
                GenerateInMemory = true,
                TempFiles = {KeepFiles = false}
            };
            _parameters.ReferencedAssemblies.Add("System.dll");
            _parameters.ReferencedAssemblies.Add("System.Core.dll");
            _parameters.ReferencedAssemblies.Add("System.Numerics.dll");
            _parameters.ReferencedAssemblies.Add(GetDllPath("Meta.Numerics.dll"));
            _parameters.ReferencedAssemblies.Add(GetDllPath("MathNet.Numerics.dll"));
            _parameters.ReferencedAssemblies.Add(GetDllPath("Accord.Math.dll"));
            _parameters.ReferencedAssemblies.Add(GetDllPath("Accord.dll"));
            _parameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll"); //dynamic
        }

        public int MainCodeStarOffsetLine { get; set; }
        public int MainCodeEndOffsetLine { get; set; }

        public int CustomFunctionsStartOffsetLine { get; set; }
        public int CustomFunctionsEndOffsetLine { get; set; }

        public bool IsScripting { get; set; } = false;

        private string GetDllPath(string dllName)
        {
            var dllDirectory = PathUtility.GetFullPath(dllName);
            if (File.Exists(dllDirectory))
                return dllDirectory;

            var path = PathUtility.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), dllName);
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
            if (RuntimeInformation.IsUnix)
            {
                if (!Directory.Exists(AppInformation.TempDirectory))
                {
                    Directory.CreateDirectory(AppInformation.TempDirectory);
                }
                //fix for #39 - otherwise in Mono CSharpCodeProvider will always return the same assembly
                _parameters.OutputAssembly = Path.Combine(AppInformation.TempDirectory, Guid.NewGuid().ToString());
            }
            var results = CompileAssemblyFromSource(_parameters, input);

            if (results.Errors.HasErrors)
            {
                var message =
                    new StringBuilder(
                        $"{Strings.Syntax_error}{Environment.NewLine}{Strings.Details}");
                var compilationException = CreateCompilationExceptionFromCompilerResults(results);

                if (compilationException.HasMainCodeErrors)
                {
                    message.Append(
                        $"{Environment.NewLine} {(IsScripting ? Strings.Script_errors : Strings.Expression_errors)}:");
                    message.Append(ErrorCollectionToString(compilationException.Errors[CompilationErrorPlace.MainCode]));
                }

                if (compilationException.HasCustomFunctionsErrors)
                {
                    message.Append($"{Environment.NewLine} {Strings.Custom_functions_errors}:");
                    message.Append(
                        ErrorCollectionToString(compilationException.Errors[CompilationErrorPlace.CustomFunctions]));
                }

                if (compilationException.HasInternalErrors)
                    //if there is any warning in our internal code that means we are aware of it and we dont wanna show it to user :)
                {
                    message.Append($"{Environment.NewLine} {Strings.Internal_errors}:");
                    message.Append(ErrorCollectionToString(compilationException.Errors[CompilationErrorPlace.Internal]));
                }

                compilationException = new CompilationException(message.ToString())
                {
                    Errors = compilationException.Errors
                };
                // compilationException.Message = message;
                // var ex = new CompilationException(message) {Errors = compilerErrors};
                
                var nativeCompilerOutput = "";
                foreach (var str in results.Output)
                    nativeCompilerOutput += str + Environment.NewLine;

                Logger.Error(compilationException,
                    $"{compilationException.Message}{Environment.NewLine}{nameof(nativeCompilerOutput)} = \'{nativeCompilerOutput}\'");

                throw compilationException;
            }

            _parameters.TempFiles.KeepFiles = false;
            _parameters.TempFiles.Delete();
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

        private static string ErrorCollectionToString(IEnumerable<CompilerError> errorCollection)
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
            var war = Strings.NativeCompiler_warning;
            var err = Strings.NativeCompiler_error;

            return
                $"{Environment.NewLine}  ({Strings.NativeCompiler_Line}: {error.Line} {Strings.NativeCompiler_Column}: {error.Column}):{(error.IsWarning ? " " + Strings.NativeCompiler_warning + " " : " " + Strings.NativeCompiler_error + " ")}{error.ErrorNumber}: {error.ErrorText}";
        }

        public void AddDll(string dllPath)
        {
            _parameters.ReferencedAssemblies.Add(dllPath);
        }
    }
}