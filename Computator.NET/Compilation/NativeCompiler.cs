using Enumerable = System.Linq.Enumerable;

//findRoot(sin,x-1,x+1) is really interesting

namespace Computator.NET.Compilation
{
    public class NativeCompiler : Microsoft.CSharp.CSharpCodeProvider
    {
        private readonly Logging.SimpleLogger logger;
        private readonly System.CodeDom.Compiler.CompilerParameters parameters;

        public NativeCompiler()
        {
            logger = new Logging.SimpleLogger {ClassName = GetType().FullName};
            parameters = new System.CodeDom.Compiler.CompilerParameters
            {
                GenerateInMemory = true,
                TempFiles = {KeepFiles = false}
            };
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Numerics.dll");
            parameters.ReferencedAssemblies.Add(Config.GlobalConfig.FullPath("Meta.Numerics.dll"));
            parameters.ReferencedAssemblies.Add(Config.GlobalConfig.FullPath("MathNet.Numerics.dll"));
            parameters.ReferencedAssemblies.Add(Config.GlobalConfig.FullPath("Accord.Math.dll"));
            parameters.ReferencedAssemblies.Add(Config.GlobalConfig.FullPath("Accord.dll"));
        }

        public System.Reflection.Assembly Compile(string input)
        {
            System.CodeDom.Compiler.CompilerResults results = null;
            try
            {
                results = CompileAssemblyFromSource(parameters, input);
                if (results.Errors.Count > 0)
                    throw new System.Exception(Localization.Strings.BadSyntax);
            }
            catch (System.Exception ex)
            {
                var message =
                    Localization.Strings
                        .ErrorInExpressionSyntaxOneOfUsedFunctionsDoesNotExistIsIncompatibleWithGivenArgumentsOrYouJustMadeAMistakeWritingExpression;
                message += System.Environment.NewLine + Localization.Strings.Details;
                message += System.Environment.NewLine + ex.Message + System.Environment.NewLine +
                           Localization.Strings.MoreDetails;
                message += Enumerable.Aggregate(Enumerable.Cast<System.CodeDom.Compiler.CompilerError>(results.Errors),
                    message,
                    (current, err) => (!err.IsWarning) ? current + (System.Environment.NewLine + err.ErrorText) : "");

                logger.MethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                // logger.Parameters["NativeCompilerInput"] = input;
                logger.Parameters["NativeCompilerOutput"] = "";

                foreach (var str in results.Output)
                    logger.Parameters["NativeCompilerOutput"] += str + System.Environment.NewLine;

                logger.Log(message, Config.ErrorType.Compilation, ex);

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