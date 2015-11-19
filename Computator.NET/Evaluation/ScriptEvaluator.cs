using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Forms.Integration;
using System.Windows.Media.Media3D;
using Computator.NET.Config;
using Computator.NET.DataTypes;
using Settings = Computator.NET.Properties.Settings;

namespace Computator.NET.Evaluation
{
    internal class ScriptEvaluator : Evaluator
    {
        private readonly string additionalObjectsCodeCopy;
        public ScriptEvaluator()
        {
            functionType = FunctionType.Scripting;
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

            nativeCompiler.AddDll(GlobalConfig.FullPath("Computator.NET.Charting.dll"));
            nativeCompiler.AddDll(GlobalConfig.FullPath("Computator.NET.DataTypes.dll"));
            /////////////////////////
            nativeCompiler.AddDll("System.Drawing.dll");
            nativeCompiler.AddDll("System.Windows.Forms.DataVisualization.dll");
            nativeCompiler.AddDll("System.Windows.Forms.dll");
            nativeCompiler.AddDll("System.Xaml.dll");
            //nativeCompiler.AddDll("Microsoft.CSharp.dll");
            

            nativeCompiler.AddDll(typeof (AmbientLight).Assembly.Location);
            //"PresentationCore.dll");
            nativeCompiler.AddDll(typeof (XmlDataProvider).Assembly.Location);
            //"PresentationFramework.dll");
            nativeCompiler.AddDll(typeof (PresentationTraceSources).Assembly.Location);
            //"WindowsBase.dll");
            nativeCompiler.AddDll(typeof (ElementHost).Assembly.Location);
            //"WindowsFormsIntegration.dll");

            additionalObjectsCodeCopy=additionalObjectsCode = ScriptingExtensionObjects.ToCode;
            logger.ClassName = GetType().FullName;
        }

        public ScriptFunction Evaluate(string input, string customFunctionsCode = "")
        {
            tslCode = input;
            customFunctionsTSLCode = customFunctionsCode;

            additionalObjectsCode = additionalObjectsCodeCopy.Replace(
                @"Properties.Settings.Default.NumericalOutputNotation",
                "Computator.NET.DataTypes.SettingsTypes.NumericalOutputNotationType." +
                Settings.Default.NumericalOutputNotation);
                //DataTypes.SettingsTypes.NumericalOutputNotationType.MathematicalNotation

            var function = Compile();
            return new ScriptFunction(function, tslCode, CSharpCode);
        }
    }
}