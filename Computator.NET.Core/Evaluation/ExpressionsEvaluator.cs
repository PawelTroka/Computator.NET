using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Compilation.Builtins;
using Computator.NET.Core.Compilation.TslHandlers;
using Computator.NET.Core.Constants;
using Computator.NET.Core.Functions;
using Computator.NET.Core.NumericalCalculations;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Functions;
using Computator.NET.DataTypes.Utility;
using Computator.NET.Localization;
using NLog;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Computator.NET.Core.Evaluation
{
    public interface IExpressionsEvaluator
    {
        Function Evaluate(string input, string customFunctionsCode, CalculationsMode calculationsMode);
    }

    public class ExpressionsEvaluator : IExpressionsEvaluator
    {
        protected const string ScriptEntryPoint = @"
            public static void CustomFunction(System.Action<string> CONSOLE_OUTPUT_CALLBACK)
            {
                CONSOLE_OUTPUT = CONSOLE_OUTPUT_CALLBACK;
            ";
        private const string PrecompiledLambdaScript = ScriptingFunctions.ToCode + ScriptEntryPoint;
        private const string LambdaScript = MatrixFunctions.ToCode + PrecompiledLambdaScript;
        private const string LambdaX = @"           
            public static double CustomFunction(double x)
            {
                return ";
        private const string LambdaZ = @"           
            public static System.Numerics.Complex CustomFunction(System.Numerics.Complex z)
            {
                double x=Re(z); double y=Im(z);
                return ";
        private const string LambdaXy = @"           
            public static double CustomFunction(double x, double y)
            {
                return ";
        private const string LambdaXyz = @"           
            public static double CustomFunction(double x, double y, double z)
            {
                return ";

        protected const string Begin = @"
        using MathNet.Numerics.Distributions;//we need it cause of added by dlls distributions" +
                                       TslCompiler.TypesAliases + @"
        namespace FunctionsCreatorNS
        {
            public class FunctionsCreator 
            {
        " + MathematicalConstants.ToCode + PhysicalConstants.ToCode + ElementaryFunctions.ToCode +
                                       SpecialFunctions.ToCode + StatisticsFunctions.ToCode + Integral.ToCode +
                                       Derivative.ToCode + FunctionRoot.ToCode;

        private const string PrecompiledBegin = @"
        using MathNet.Numerics.Distributions;" + TslCompiler.TypesAliases + @"
        namespace FunctionsCreatorNS {
            public class FunctionsCreator : global::Computator.NET.Core.Compilation.Builtins.PrecompiledExpressionBuiltins<FunctionsCreator> {
        ";
        private const string PrecompiledScriptBegin = @"
        using MathNet.Numerics.Distributions;" + TslCompiler.TypesAliases + @"
        namespace FunctionsCreatorNS {
            public class FunctionsCreator : global::Computator.NET.Core.Compilation.Builtins.PrecompiledScriptMathBuiltins<FunctionsCreator> {
        ";
        protected const string End = @";
                }
            }            
    ///*{additional_objects}*///

        }";

        // Differential tests and specialized hosts can retain the original path.
        // No generated delegates, assemblies or constructed generic types are cached.
        protected virtual bool UsePrecompiledBuiltins => true;
        // Subclasses can replace NativeCompiler or add references even when the
        // source preamble is empty. Preserve that contract unless they opt in.
        protected virtual bool UseDirectExpressions => GetType() == typeof(ExpressionsEvaluator);
        // Select support declarations only after the complete translated program
        // has passed the compatibility policy. Specialized evaluators keep their
        // existing AdditionalObjectsCode unless they explicitly override this hook.
        protected virtual string GetAdditionalObjectsCode(bool usingPrecompiledBuiltins) => AdditionalObjectsCode;
        protected virtual string GetScriptFunctionSignature(bool usingPrecompiledBuiltins) =>
            usingPrecompiledBuiltins ? PrecompiledLambdaScript : LambdaScript;
        protected bool ScriptFormattingCompatible { get; private set; }
        private string _beginCode;
        private bool _usingPrecompiledBuiltins;
        private readonly TslCompiler _tslCompiler;
        private string _customFunctionsCSharpCode;
        private Type _delegateType;
        private string _functionSignature;
        protected string InitCodeAfterFunctionSignature = string.Empty;
        protected string AdditionalObjectsCode;
        protected string AdditionalUsings;
        protected string CustomFunctionsTslCode;
        protected FunctionType FunctionType;
        protected readonly ILogger Logger;
        protected string MainCSharpCode;
        protected string MainTslCode;
        protected NativeCompiler NativeCompiler;

        public ExpressionsEvaluator()
        {
            NativeCompiler = new NativeCompiler();
            NativeCompiler.AddDll(PathUtility.GetFullPath("Computator.NET.DataTypes.dll"));
            NativeCompiler.AddDll(typeof(PrecompiledExpressionBuiltins<>).Assembly.Location);
            _tslCompiler = new TslCompiler();
            Logger = LogManager.GetLogger(GetType().FullName);
            AdditionalObjectsCode = "";
            AdditionalUsings = "";
        }

        private string SelectBegin(string customCode, string mainCode, bool scripting)
        {
            ScriptFormattingCompatible = false;
            if (!UsePrecompiledBuiltins || !BuiltinFacadePolicy.CanUse(customCode, mainCode, scripting,
                out var scriptFormattingCompatible)) return Begin;
            ScriptFormattingCompatible = scriptFormattingCompatible;
            return scripting ? PrecompiledScriptBegin : PrecompiledBegin;
        }

        internal ExpressionAnalysis AnalyzeFunctionPowers(string expression, string customFunctionsCode, bool isImplicit)
        {
            // Routing must bind against the same built-ins and custom-name policy
            // as compilation, without emitting or executing user code.
            var coordinateType = isImplicit ? "double" : "System.Numerics.Complex";
            var custom = _tslCompiler.TransformToCSharp(customFunctionsCode, "", true);
            var source = SelectBegin(custom, expression, false) + custom +
                "public static object CustomFunction(double x, double y, " + coordinateType + " z) { return " +
                expression + "; } } }";
            var compilation = FunctionPowerResolver.Resolve(NativeCompiler.CreateCompilation(source));
            var method = compilation.SyntaxTrees.Single().GetRoot().DescendantNodes()
                .OfType<MethodDeclarationSyntax>().LastOrDefault(node => node.Identifier.ValueText == "CustomFunction");
            var result = method?.Body?.Statements.OfType<ReturnStatementSyntax>().FirstOrDefault()?.Expression;
            return ExpressionAnalysis.Parse(result?.ToFullString() ?? expression);
        }

        private bool IsImplicit =>
            FunctionType == FunctionType.ComplexImplicit || FunctionType == FunctionType.Real2DImplicit ||
            FunctionType == FunctionType.Real3DImplicit;

        public Function Evaluate(string input, string customFunctionsCode, CalculationsMode calculationsMode)
        {
            if (string.IsNullOrWhiteSpace(input)) throw new ArgumentException(Strings.Expression_should_not_be_empty_);
            if (UsePrecompiledBuiltins && UseDirectExpressions && string.IsNullOrWhiteSpace(customFunctionsCode) &&
                string.IsNullOrEmpty(AdditionalUsings) && string.IsNullOrEmpty(AdditionalObjectsCode) &&
                string.IsNullOrEmpty(InitCodeAfterFunctionSignature) &&
                DirectExpressionCompiler.TryCompile(input, calculationsMode, out var direct, out var directCode))
            {
                FunctionType = FunctionTypeFromCalculationsMode(calculationsMode, false);
                MainTslCode = input; MainCSharpCode = directCode; CustomFunctionsTslCode = "";
                return new Function(direct, FunctionType) { TslCode = input, CsCode = directCode };
            }
            var expression = ExpressionAnalysis.Parse(_tslCompiler.TransformToCSharp(input));
            if (expression.IsEmpty) throw new ArgumentException(Strings.Expression_should_not_be_empty_);
            FunctionType = FunctionTypeFromCalculationsMode(calculationsMode, expression.IsImplicit);
            MainTslCode = input;
            CustomFunctionsTslCode = !string.IsNullOrWhiteSpace(customFunctionsCode) ? customFunctionsCode : "";
            var function = Compile();
            return new Function(function, FunctionType) { TslCode = MainTslCode, CsCode = MainCSharpCode };
        }

        private FunctionType FunctionTypeFromCalculationsMode(CalculationsMode calculationsMode, bool isImplicit)
        {
            switch (calculationsMode)
            {
                case CalculationsMode.Real: return isImplicit ? FunctionType.Real2DImplicit : FunctionType.Real2D;
                case CalculationsMode.Complex: return isImplicit ? FunctionType.ComplexImplicit : FunctionType.Complex;
                case CalculationsMode.Fxy: return isImplicit ? FunctionType.Real3DImplicit : FunctionType.Real3D;
                default: throw new ArgumentOutOfRangeException(nameof(calculationsMode), calculationsMode, null);
            }
        }

        protected void SetVariablesAndSignatures()
        {
            switch (FunctionType)
            {
                case FunctionType.Real2D:
                    _functionSignature = LambdaX;
                    _delegateType = typeof(Func<double, double>);
                    break;
                case FunctionType.Complex:
                    _functionSignature = LambdaZ;
                    _delegateType = typeof(Func<Complex, Complex>);
                    break;
                case FunctionType.Real2DImplicit:
                case FunctionType.Real3D:
                    _functionSignature = LambdaXy;
                    _delegateType = typeof(Func<double, double, double>);
                    break;
                case FunctionType.Real3DImplicit:
                    _functionSignature = LambdaXyz;
                    _delegateType = typeof(Func<double, double, double, double>);
                    break;
                case FunctionType.Scripting:
                    _delegateType = typeof(Action<Action<string>>);
                    _functionSignature = GetScriptFunctionSignature(_usingPrecompiledBuiltins);
                    break;
            }
            _functionSignature += InitCodeAfterFunctionSignature;
        }

        private void TransformImplicitToExplicit()
        {
            if (IsImplicit) MainCSharpCode = ExpressionAnalysis.Parse(MainCSharpCode).ToExplicit();
        }

        protected Delegate Compile()
        {
            _customFunctionsCSharpCode = _tslCompiler.TransformToCSharp(CustomFunctionsTslCode, string.Empty, deferSymbolicFunctionPowers: true);
            MainCSharpCode = _tslCompiler.TransformToCSharp(MainTslCode, CustomFunctionsTslCode, deferSymbolicFunctionPowers: true);
            TransformImplicitToExplicit();
            _beginCode = SelectBegin(_customFunctionsCSharpCode, MainCSharpCode, FunctionType == FunctionType.Scripting);
            _usingPrecompiledBuiltins = _beginCode != Begin;
            SetVariablesAndSignatures();
            var fullCode = BuildCode();
            try
            {
                var assembly = NativeCompiler.Compile(fullCode);
                var cls = assembly.GetType("FunctionsCreatorNS.FunctionsCreator");
                var method = cls.GetMethod("CustomFunction", BindingFlags.Static | BindingFlags.Public);
                return Delegate.CreateDelegate(_delegateType, method);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"{ex.Message}{Environment.NewLine}{nameof(MainTslCode)} = '{MainTslCode}'{Environment.NewLine}{nameof(MainCSharpCode)} = '{MainCSharpCode}'{Environment.NewLine}{nameof(_customFunctionsCSharpCode)} = '{_customFunctionsCSharpCode}'");
                if (ex is CompilationException) throw;
                var message = Strings.ErrorInExpressionSyntaxOneOfUsedFunctionsDoesNotExistIsIncompatibleWithGivenArgumentsOrYouJustMadeAMistakeWritingExpression;
                message += Environment.NewLine + Strings.Details + Environment.NewLine + ex.Message;
                Logger.Error(ex, $"{message}{Environment.NewLine}{nameof(MainTslCode)} = '{MainTslCode}'{Environment.NewLine}{nameof(MainCSharpCode)} = '{MainCSharpCode}'{Environment.NewLine}{nameof(_customFunctionsCSharpCode)} = '{_customFunctionsCSharpCode}'");
                throw new EvaluationException(message, ex);
            }
        }

        private string BuildCode()
        {
            var begin = _beginCode ?? Begin;
            var codeBuilder = new StringBuilder();
            NativeCompiler.CustomFunctionsStartOffsetLine = AdditionalUsings.Count(c => c == '\n') + begin.Count(c => c == '\n');
            NativeCompiler.CustomFunctionsEndOffsetLine = NativeCompiler.CustomFunctionsStartOffsetLine + _customFunctionsCSharpCode.Count(c => c == '\n') + 1;
            NativeCompiler.MainCodeStarOffsetLine = NativeCompiler.CustomFunctionsEndOffsetLine - 1 + _functionSignature.Count(c => c == '\n');
            NativeCompiler.MainCodeEndOffsetLine = NativeCompiler.MainCodeStarOffsetLine + MainCSharpCode.Count(c => c == '\n') + 1;
            codeBuilder.Append(AdditionalUsings);
            codeBuilder.Append(begin);
            codeBuilder.Append(_customFunctionsCSharpCode);
            codeBuilder.Append(_functionSignature);
            codeBuilder.Append(MainCSharpCode);
            codeBuilder.Append(End.Replace("///*{additional_objects}*///", GetAdditionalObjectsCode(_usingPrecompiledBuiltins)));
            return codeBuilder.ToString();
        }
    }
}
