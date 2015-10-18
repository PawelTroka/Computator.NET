using System;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Computator.NET.Compilation;
using Computator.NET.Config;
using Computator.NET.Constants;
using Computator.NET.DataTypes;
using Computator.NET.Functions;
using Computator.NET.Localization;
using Computator.NET.Logging;

namespace Computator.NET.Evaluation
{
    internal abstract class Evaluator
    {
        private const string LAMBDA_SCRIPT = MatrixFunctions.ToCode + ScriptingFunctions.ToCode + @"
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
        using System;
        using System.Numerics;
        using Meta.Numerics.Functions;
        using System.Runtime.InteropServices;
        using MathNet.Numerics;
        using MathNet.Numerics.Distributions;//we need it cause of added by dlls distributions
        using Accord.Math;
        using real = System.Double;
        using complex = System.Numerics.Complex;
        using natural = System.UInt64;
        using integer = System.Int64;
        //using Matrix = Meta.Numerics.Matrices.RectangularMatrix;
        namespace FunctionsCreatorNS
        {
            public static class FunctionsCreator 
            {

        " + MathematicalConstants.ToCode //.Replace("[N", @"/*[").Replace(@""")]", @"]*/")
                                       + PhysicalConstants.ToCode //.Replace("[N", @"/*[").Replace(@""")]", @"]*/")
                                       + ElementaryFunctions.ToCode //.Replace("[N", @"/*[").Replace(@""")]", @"]*/")
                                       + SpecialFunctions.ToCode //.Replace("[N", @"/*[").Replace(@""")]", @"]*/");
                                       + StatisticsFunctions.ToCode; //.Replace("[N", @"/*[").Replace(@""")]", @"]*/");

        protected const string End = @";
                }
            }            
    ///*{additional_objects}*///

        }";
        private readonly TslCompiler tslCompiler;
        protected string additionalObjectsCode;
        protected string additionalUsings;
        protected string CSharpCode;
        private string CustomFunctionsCSharpCode;
        protected string customFunctionsTSLCode;
        private Type delegateType;
        private string functionSignature;
        protected FunctionType functionType;
        protected SimpleLogger logger;
        protected NativeCompiler nativeCompiler;
        protected string tslCode;

        protected Evaluator()
        {
            nativeCompiler = new NativeCompiler();
            tslCompiler = new TslCompiler();
            logger = new SimpleLogger {ClassName = GetType().FullName};
            additionalObjectsCode = "";
            additionalUsings = "";
        }

        private bool IsImplicit
        {
            get
            {
                return functionType == FunctionType.ComplexImplicit || functionType == FunctionType.Real2DImplicit ||
                       functionType == FunctionType.Real3DImplicit;
            }
        }

        protected void SetVariablesAndSignatures()
        {
            switch (functionType)
            {
                case FunctionType.Real2D:
                    tslCompiler.Variables.Add("x");
                    functionSignature = LAMBDA_X;
                    delegateType = typeof (Func<double, double>);
                    break;
                case FunctionType.Complex:
                    tslCompiler.Variables.Add("z");
                    tslCompiler.Variables.Add("i");
                    functionSignature = LAMBDA_Z;
                    delegateType = typeof (Func<Complex, Complex>);
                    break;
                case FunctionType.Real2DImplicit:
                case FunctionType.Real3D:
                    tslCompiler.Variables.Add("x");
                    tslCompiler.Variables.Add("y");
                    delegateType = typeof (Func<double, double, double>);
                    functionSignature = LAMBDA_XY;
                    break;
                case FunctionType.Real3DImplicit:
                    tslCompiler.Variables.Add("x");
                    tslCompiler.Variables.Add("y");
                    tslCompiler.Variables.Add("z");
                    functionSignature = LAMBDA_XYZ;
                    delegateType = typeof (Func<double, double, double, double>);
                    break;
                case FunctionType.Scripting:
                    delegateType = typeof (Action<RichTextBox>);
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

        protected Delegate Compile()
        {
            SetVariablesAndSignatures();
            CustomFunctionsCSharpCode = tslCompiler.TransformToCSharp(customFunctionsTSLCode);
            CSharpCode = tslCompiler.TransformToCSharp(tslCode);
            transformImplicitToExplicit();

            var codeBuilder = new StringBuilder();
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
                var method = cls.GetMethod("CustomFunction", BindingFlags.Static | BindingFlags.Public);
                return Delegate.CreateDelegate(delegateType, method);
            }
            catch (Exception ex)
            {
                if (ex is CompilationException)
                    throw;

                var message =
                    Strings
                        .ErrorInExpressionSyntaxOneOfUsedFunctionsDoesNotExistIsIncompatibleWithGivenArgumentsOrYouJustMadeAMistakeWritingExpression;
                message += Environment.NewLine + Strings.Details;
                message += Environment.NewLine + ex.Message;

                logger.Parameters["tslCode"] = tslCode;
                logger.Parameters["CSharpCode"] = CSharpCode;
                logger.Parameters["customFunctionsCode"] = CustomFunctionsCSharpCode;
                logger.MethodName = MethodBase.GetCurrentMethod().Name;
                logger.Log(message, ErrorType.Evaluation, ex);

                throw new EvaluationException(message, ex);
            }
        }
    }
}