using System;
using Computator.NET.Localization;
using Meta.Numerics;
using Complex = System.Numerics.Complex;

namespace Computator.NET.Evaluation
{
    internal class FunctionComplexEvaluator : Evaluator
    {
        public FunctionComplexEvaluator()
        {
            functionType = typeof (Func<Complex, Complex>);
            lambdaFunc = @"           
            public static Complex CustomFunction(Complex z)
            {
                double x=Re(z); double y=Im(z);
                return ";
        }

        public Func<Complex, Complex> Evaluate(string input, string CustomFunctionsCode = "")
        {
            CustomFunctionsCodeCSharp = transformTSLToCSharp(CustomFunctionsCode);

            Normalized = Normalize(input);
            Delegate function = compile();
            return z => (Complex) function.DynamicInvoke(z);
        }

        protected override string Normalize(string input)
        {
            return input.ReplaceMultipling('i', 'z').ReplacePow('i', 'z').ReplaceToDoubles();
        }

        public Complex Invoke(Complex z)
        {
            if (evaluatedFunction == null)
                throw new NullReferenceException(Strings.NoFunctionToInvoke);

            Complex result = default(Complex);

            try
            {
                result = (Complex) evaluatedFunction.DynamicInvoke(z);
            }
            catch (Exception ex2)
            {
                result = new Complex(double.NaN, double.NaN);

                if (ex2 is NonconvergenceException)
                    throw ex2;
                if (ex2 is DimensionMismatchException)
                    throw ex2;
                if (ex2 is ArgumentException || ex2 is ArgumentOutOfRangeException)
                    throw ex2;
                throw new NonconvergenceException(
                    Strings.ForChosenValuesOneOrMoreOfTheFunctionsInYourExpressionCannotConvergence + ex2.Message + "\n" +
                    ex2.Source);
            }

            return result;
        }
    }
}