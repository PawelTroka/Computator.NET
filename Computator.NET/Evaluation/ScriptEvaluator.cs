namespace Computator.NET.Evaluation
{
    internal class ScriptEvaluator : Evaluator
    {
        public ScriptEvaluator()
        {
            functionType = DataTypes.FunctionType.Scripting;
            additionalUsings = @"
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

            nativeCompiler.AddDll(Config.GlobalConfig.FullPath("Computator.NET.Charting.dll"));
            nativeCompiler.AddDll(Config.GlobalConfig.FullPath("Computator.NET.DataTypes.dll"));
                /////////////////////////
            nativeCompiler.AddDll("System.Drawing.dll");
            nativeCompiler.AddDll("System.Windows.Forms.DataVisualization.dll");
            nativeCompiler.AddDll("System.Windows.Forms.dll");
            nativeCompiler.AddDll("System.Xaml.dll");

            nativeCompiler.AddDll(typeof (System.Windows.Media.Media3D.AmbientLight).Assembly.Location);
                //"PresentationCore.dll");
            nativeCompiler.AddDll(typeof (System.Windows.Data.XmlDataProvider).Assembly.Location);
                //"PresentationFramework.dll");
            nativeCompiler.AddDll(typeof (System.Diagnostics.PresentationTraceSources).Assembly.Location);
                //"WindowsBase.dll");
            nativeCompiler.AddDll(typeof (System.Windows.Forms.Integration.ElementHost).Assembly.Location);
                //"WindowsFormsIntegration.dll");

            additionalObjectsCode = ScriptingExtensionObjects.ToCode;
            logger.ClassName = GetType().FullName;
        }

        public DataTypes.ScriptFunction Evaluate(string input, string customFunctionsCode = "")
        {
            tslCode = input;
            customFunctionsTSLCode = customFunctionsCode;

            additionalObjectsCode = additionalObjectsCode.Replace(
                @"Properties.Settings.Default.NumericalOutputNotation",
                "Computator.NET.DataTypes.SettingsTypes.NumericalOutputNotationType." + Properties.Settings.Default.NumericalOutputNotation);//DataTypes.SettingsTypes.NumericalOutputNotationType.MathematicalNotation

            var function = Compile();
            return new DataTypes.ScriptFunction(function, tslCode, CSharpCode);
        }
    }
}