namespace Computator.NET.Evaluation
{
    internal abstract class Evaluator
    {
        private const string LAMBDA_SCRIPT = Functions.MatrixFunctions.ToCode + Functions.ScriptingFunctions.ToCode + @"
            public static void CustomFunction(System.Windows.Forms.RichTextBox CONSOLE_OUTPUTref)
            {
            CONSOLE_OUTPUT = CONSOLE_OUTPUTref;
            ";
        private const string LAMBDA_X = @"           
            public static double CustomFunction(double x)
            {
                //Complex z = x;
                return ";
        private const string LAMBDA_Z = @"           
            public static Complex CustomFunction(Complex z)
            {
                double x=Re(z); double y=Im(z);
                return ";
        private const string LAMBDA_XY = @"           
            public static double CustomFunction(double x, double y)
            {
                return ";
        private const string LAMBDA_XYZ = @"           
            public static double CustomFunction(double x, double y, double z)
            {
                return ";

        protected const string Begin = @"
        //using System;
        //using System.Numerics;
        //using Meta.Numerics.Functions;
        //using System.Runtime.InteropServices;
        //using MathNet.Numerics;
        //using Accord.Math;
        //using Computator.NET;
        using MathNet.Numerics.Distributions;//we need it cause of added by dlls distributions

        using real = System.Double;
        using complex = System.Numerics.Complex;
        using natural = System.UInt64;
        using integer = System.Int64;
        //using Matrix = Meta.Numerics.Matrices.RectangularMatrix;
        namespace FunctionsCreatorNS
        {
            public static class FunctionsCreator 
            {

        " + Constants.MathematicalConstants.ToCode //.Replace("[N", @"/*[").Replace(@""")]", @"]*/")
                                       + Constants.PhysicalConstants.ToCode
            //.Replace("[N", @"/*[").Replace(@""")]", @"]*/")
                                       + Functions.ElementaryFunctions.ToCode
            //.Replace("[N", @"/*[").Replace(@""")]", @"]*/")
                                       + Functions.SpecialFunctions.ToCode
            //.Replace("[N", @"/*[").Replace(@""")]", @"]*/");
                                       + Functions.StatisticsFunctions.ToCode;
            //.Replace("[N", @"/*[").Replace(@""")]", @"]*/");

        protected const string End = @";
                }
            }            
    ///*{additional_objects}*///

        }";
        private readonly Compilation.TslCompiler tslCompiler;
        protected string additionalObjectsCode;
        protected string additionalUsings;
        protected string CSharpCode;
        private string CustomFunctionsCSharpCode;
        protected string customFunctionsTSLCode;
        private System.Type delegateType;
        private string functionSignature;
        protected DataTypes.FunctionType functionType;
        protected Logging.SimpleLogger logger;
        protected Compilation.NativeCompiler nativeCompiler;
        protected string tslCode;

        protected Evaluator()
        {
            nativeCompiler = new Compilation.NativeCompiler();
            tslCompiler = new Compilation.TslCompiler();
            logger = new Logging.SimpleLogger {ClassName = GetType().FullName};
            additionalObjectsCode = "";
            additionalUsings = "";
        }

        private bool IsImplicit
        {
            get
            {
                return functionType == DataTypes.FunctionType.ComplexImplicit ||
                       functionType == DataTypes.FunctionType.Real2DImplicit ||
                       functionType == DataTypes.FunctionType.Real3DImplicit;
            }
        }

        protected void SetVariablesAndSignatures()
        {
            switch (functionType)
            {
                case DataTypes.FunctionType.Real2D:
                    tslCompiler.Variables.Add("x");
                    functionSignature = LAMBDA_X;
                    delegateType = typeof (System.Func<double, double>);
                    break;
                case DataTypes.FunctionType.Complex:
                    tslCompiler.Variables.Add("z");
                    tslCompiler.Variables.Add("i");
                    functionSignature = LAMBDA_Z;
                    delegateType = typeof (System.Func<System.Numerics.Complex, System.Numerics.Complex>);
                    break;
                case DataTypes.FunctionType.Real2DImplicit:
                case DataTypes.FunctionType.Real3D:
                    tslCompiler.Variables.Add("x");
                    tslCompiler.Variables.Add("y");
                    delegateType = typeof (System.Func<double, double, double>);
                    functionSignature = LAMBDA_XY;
                    break;
                case DataTypes.FunctionType.Real3DImplicit:
                    tslCompiler.Variables.Add("x");
                    tslCompiler.Variables.Add("y");
                    tslCompiler.Variables.Add("z");
                    functionSignature = LAMBDA_XYZ;
                    delegateType = typeof (System.Func<double, double, double, double>);
                    break;
                case DataTypes.FunctionType.Scripting:
                    delegateType = typeof (System.Action<System.Windows.Forms.RichTextBox>);
                    functionSignature = LAMBDA_SCRIPT;
                    break;
            }
        }

        private void transformImplicitToExplicit()
        {
            if (IsImplicit)
            {
                CSharpCode = CSharpCode.Substring(0, CSharpCode.IndexOf("=")) + "-(" +
                             CSharpCode.Substring(CSharpCode.IndexOf("=") + 1) + ")";
            }
        }

        protected System.Delegate Compile()
        {
            SetVariablesAndSignatures();
            CustomFunctionsCSharpCode = tslCompiler.TransformToCSharp(customFunctionsTSLCode);
            CSharpCode = tslCompiler.TransformToCSharp(tslCode);
            transformImplicitToExplicit();

            var codeBuilder = new System.Text.StringBuilder();
            codeBuilder.Append(additionalUsings);
            codeBuilder.Append(Begin);
            codeBuilder.Append(CustomFunctionsCSharpCode);
            codeBuilder.Append(functionSignature);
            codeBuilder.Append(CSharpCode);
            codeBuilder.Append(End.Replace("///*{additional_objects}*///", additionalObjectsCode));
            var fullCode = codeBuilder.ToString();

            try
            {
                var assembly = nativeCompiler.Compile(fullCode);
                var cls = assembly.GetType("FunctionsCreatorNS.FunctionsCreator");
                var method = cls.GetMethod("CustomFunction",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                return System.Delegate.CreateDelegate(delegateType, method);
            }
            catch (System.Exception ex)
            {
                if (ex is Compilation.CompilationException)
                    throw;

                var message =
                    Localization.Strings
                        .ErrorInExpressionSyntaxOneOfUsedFunctionsDoesNotExistIsIncompatibleWithGivenArgumentsOrYouJustMadeAMistakeWritingExpression;
                message += System.Environment.NewLine + Localization.Strings.Details;
                message += System.Environment.NewLine + ex.Message;

                logger.Parameters["tslCode"] = tslCode;
                logger.Parameters["CSharpCode"] = CSharpCode;
                logger.Parameters["customFunctionsCode"] = CustomFunctionsCSharpCode;
                logger.MethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                logger.Log(message, Config.ErrorType.Evaluation, ex);

                throw new EvaluationException(message, ex);
            }
        }
    }
}