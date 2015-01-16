using System;
using System.Text;
using System.Windows.Forms;
using Computator.NET.Functions;
using Computator.NET.Localization;

namespace Computator.NET.Evaluation
{
    internal class ScriptEvaluator : Evaluator
    {
        public ScriptEvaluator()
        {
            functionType = typeof (Action<RichTextBox>);
            additionalUsings =
                "using System.Collections.Generic;using System.Windows.Forms.Integration;using System.Linq;using Computator.NET.Charting;using Complex = System.Numerics.Complex;using DenseVector = MathNet.Numerics.LinearAlgebra.Complex.DenseVector;using MathNet.Numerics.LinearAlgebra;using MathNet.Numerics.LinearAlgebra.Double;using Meta.Numerics;";

            lambdaFunc = MatrixFunctions.ToCode + ScriptingFunctions.ToCode + @"
            public static void CustomFunction(System.Windows.Forms.RichTextBox CONSOLE_OUTPUTref)
            {
            CONSOLE_OUTPUT = CONSOLE_OUTPUTref;
            ";

            compilerParameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Drawing.dll");
            compilerParameters.ReferencedAssemblies.Add("PresentationCore.dll");
            compilerParameters.ReferencedAssemblies.Add("PresentationFramework.dll");
            compilerParameters.ReferencedAssemblies.Add("Charting.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Xaml.dll");
            compilerParameters.ReferencedAssemblies.Add("WindowsBase.dll");
            compilerParameters.ReferencedAssemblies.Add("WindowsFormsIntegration.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Windows.Forms.DataVisualization.dll");

            additionalObjectsCode = ScriptingExtensionObjects.ToCode;
        }

        public void Evaluate(string input, string CustomFunctionsCode = "")
        {
            //common part:
            CustomFunctionsCodeCSharp = transformTSLToCSharp(CustomFunctionsCode);

            //for scripts
            Normalized = transformTSLToCSharp(input);
            //common part:
            compile();
        }

        public void Invoke(RichTextBox things)
        {
            try
            {
                evaluatedFunction.DynamicInvoke(things);
            }
            catch (Exception ex2)
            {
                var sb = new StringBuilder();
                sb.AppendLine(ex2.Message);
                if (ex2.InnerException != null)
                {
                    sb.AppendLine(ex2.InnerException.Message);
                    if (ex2.InnerException.InnerException != null)
                    {
                        sb.AppendLine(ex2.InnerException.InnerException.Message);
                        if (ex2.InnerException.InnerException != null)
                            sb.AppendLine(ex2.InnerException.InnerException.Message);
                    }
                }
                MessageBox.Show(sb.ToString(), Strings.Error);
            }
        }

        protected override string Normalize(string input)
        {
            return transformTSLToCSharp(input);
        }
    }
}