using Computator.NET.Core.Properties;
using Computator.NET.Core.Compilation.Builtins;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Configuration;
using Computator.NET.DataTypes.Functions;
using Computator.NET.DataTypes.Utility;

namespace Computator.NET.Core.Evaluation
{
    public interface IScriptEvaluator
    {
        ScriptFunction Evaluate(string input, string customFunctionsCode);
    }

    public class ScriptEvaluator : ExpressionsEvaluator, IScriptEvaluator
    {
        private readonly string _additionalObjectsCodeCopy;
        private string _numericalOutputNotation;

        protected override string GetAdditionalObjectsCode(bool usingPrecompiledBuiltins)
        {
            if (usingPrecompiledBuiltins && GetType() == typeof(ScriptEvaluator) &&
                FunctionType == FunctionType.Scripting)
                return (NumericalExtensions.ToCode + PrecompiledScriptObjects.ToCode).Replace(
                    @"Properties.Settings.Default.NumericalOutputNotation", _numericalOutputNotation);
            return base.GetAdditionalObjectsCode(usingPrecompiledBuiltins);
        }

        public ScriptEvaluator()
        {
            FunctionType = FunctionType.Scripting;
            AdditionalUsings = @"
            //using System.Collections.Generic;
            //using System.Windows.Forms.Integration;
            //using System.Linq;
            //using Computator.NET.Charting;
            //using Complex = System.Numerics.Complex;
            //using DenseVector = MathNet.Numerics.LinearAlgebra.Complex.DenseVector;
            //using MathNet.Numerics.LinearAlgebra;
            //using MathNet.Numerics.LinearAlgebra.Double;
            //using Meta.Numerics; 
            //using System.IO;
            //using System.Windows.Forms;
            //using System.Windows.Media;
            //using System.Windows.Media.Media3D;
            //using Computator.NET.Charting.Chart3D;
            //using Computator.NET.Charting.ComplexCharting;
            //using Computator.NET.Charting.RealCharting;
            //using Computator.NET.DataTypes;
            //using Computator.NET.DataTypes.SettingsTypes;
            //using Meta.Numerics.Matrices;
            ";

            NativeCompiler.AddDll(PathUtility.GetFullPath("Computator.NET.Charting.dll"));
            /////////////////////////
            NativeCompiler.AddDll("System.Drawing.dll");

            NativeCompiler.AddDll(RuntimeInformation.IsUnix
                ? PathUtility.GetFullPath("DataVisualizationX.dll")
                : typeof(System.Windows.Forms.DataVisualization.Charting.Chart).Assembly.Location);

            NativeCompiler.AddDll("System.Windows.Forms.dll");
            //NativeCompiler.AddDll("Microsoft.CSharp.dll");

#if !__MonoCS__
            NativeCompiler.AddDll("System.Xaml.dll");
            NativeCompiler.AddDll(typeof(System.Windows.Media.Media3D.AmbientLight).Assembly.Location);//"PresentationCore.dll");
            NativeCompiler.AddDll(typeof(System.Windows.Data.XmlDataProvider).Assembly.Location);//"PresentationFramework.dll");
            NativeCompiler.AddDll(typeof(System.Windows.Forms.Integration.ElementHost).Assembly.Location);//"WindowsFormsIntegration.dll");
            NativeCompiler.AddDll(typeof(System.Windows.Media.Matrix).Assembly.Location);//"WindowsBase.dll");
#endif

            NativeCompiler.IsScripting = true;

            _additionalObjectsCodeCopy = AdditionalObjectsCode = NumericalExtensions.ToCode + ScriptingExtensionObjects.ToCode;
        }

        public ScriptFunction Evaluate(string input, string customFunctionsCode)
        {
            MainTslCode = input;
            CustomFunctionsTslCode =
                !string.IsNullOrWhiteSpace(customFunctionsCode)
                    ? customFunctionsCode
                    : "";

            _numericalOutputNotation = "Computator.NET.DataTypes.SettingsTypes.NumericalOutputNotationType." +
                Settings.Default.NumericalOutputNotation;
            AdditionalObjectsCode = _additionalObjectsCodeCopy.Replace(
                @"Properties.Settings.Default.NumericalOutputNotation", _numericalOutputNotation);

            InitCodeAfterFunctionSignature =
                $@"setWorkingDirectory(@""{Settings.Default.WorkingDirectory}"");";
            

            var function = Compile();
            return new ScriptFunction(function) {TslCode = MainTslCode, CsCode = MainCSharpCode};
        }
    }
}
