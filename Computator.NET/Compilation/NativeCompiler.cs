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
                message += Environment.NewLine + ex.Message + Environment.NewLine + Strings.MoreDetails;
                message += results.Errors.Cast<CompilerError>()
                    .Aggregate(message,
                        (current, err) => (!err.IsWarning) ? current + (Environment.NewLine + err.ErrorText) : "");

                logger.MethodName = MethodBase.GetCurrentMethod().Name;
                // logger.Parameters["NativeCompilerInput"] = input;
                logger.Parameters["NativeCompilerOutput"] = "";

                foreach (var str in results.Output)
                    logger.Parameters["NativeCompilerOutput"] += str + Environment.NewLine;

                logger.Log(message, ErrorType.Compilation, ex);

                throw new CompilationException(message, ex);
            }
            finally
            {
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