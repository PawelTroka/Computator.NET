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
        public int MainCodeStarOffsettLine { get; set; }
        public int MainCodeEndOffsetLine { get; set; }

        private int GetMainCodeLine(int line)
        {
            return line - MainCodeStarOffsettLine;
        }

        private bool IsMainCode(int line)
        {
            return line >= MainCodeStarOffsettLine && line <= MainCodeEndOffsetLine;
        }

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
            parameters.ReferencedAssemblies.Add(GlobalConfig.FullPath("Meta.Numerics.dll"));
            parameters.ReferencedAssemblies.Add(GlobalConfig.FullPath("MathNet.Numerics.dll"));
            parameters.ReferencedAssemblies.Add(GlobalConfig.FullPath("Accord.Math.dll"));
            parameters.ReferencedAssemblies.Add(GlobalConfig.FullPath("Accord.dll"));
            parameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");//dynamic
        }

        public Assembly Compile(string input)
        {
            CompilerResults results = null;
            try
            {
                results = CompileAssemblyFromSource(parameters, input);
                if (results.Errors.Count > 0)
                    throw new Exception(Strings.BadSyntax);
            }
            catch (Exception ex)
            {
                var message =
                    Strings
                        .ErrorInExpressionSyntaxOneOfUsedFunctionsDoesNotExistIsIncompatibleWithGivenArgumentsOrYouJustMadeAMistakeWritingExpression;
                message += Environment.NewLine + Strings.Details;
                message += Environment.NewLine + ex.Message + Environment.NewLine +
                           Strings.MoreDetails;

                var compilerErrors = new CompilerErrorCollection();

                foreach (CompilerError error in results.Errors)
                {
                    if (IsMainCode(error.Line))
                    {
                        error.Line = GetMainCodeLine(error.Line);
                        message +=
                            $"{Environment.NewLine}(Ln: {error.Line} Col: {error.Column}):{(error.IsWarning ? " warning " : " error ")}{error.ErrorNumber}: {error.ErrorText}";
                        compilerErrors.Add(error);
                        
                    }
                }
               // message += results.Errors.Cast<CompilerError>().Aggregate(message,
                    //(current, err) => (!err.IsWarning) ? current + (Environment.NewLine + err.ErrorText) : "");

                logger.MethodName = MethodBase.GetCurrentMethod().Name;
                // Logger.Parameters["NativeCompilerInput"] = input;
                logger.Parameters["NativeCompilerOutput"] = "";

                foreach (var str in results.Output)
                    logger.Parameters["NativeCompilerOutput"] += str + Environment.NewLine;

                logger.Log(message, ErrorType.Compilation, ex);

                throw new CompilationException(message, ex) {Errors = compilerErrors};
            }
            finally
            {
                parameters.TempFiles.KeepFiles = false;
                parameters.TempFiles.Delete();
                results.TempFiles.KeepFiles = false;
                results.TempFiles.Delete();
            }


            return results.CompiledAssembly;
        }

        public void AddDll(string dllPath)
        {
            parameters.ReferencedAssemblies.Add(dllPath);
        }
    }
}