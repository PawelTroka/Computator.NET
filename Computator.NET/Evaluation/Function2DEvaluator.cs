using System;
using Computator.NET.Localization;
using Meta.Numerics;

namespace Computator.NET.Evaluation
{
    internal class Function2DEvaluator : Evaluator
    {
        public Function2DEvaluator()
        {
            functionType = typeof (Func<double, double>);
            lambdaFunc = @"           
            public static double CustomFunction(double x)
            {
                //Complex z = x;
                return ";
        }

        //(3.3+2.5x)¹²³+3.5x²+2¹²²⁻¹⁺ˢⁱⁿ⁽ˣ⁾⁺²

        public Func<double, double> Evaluate(string input, string CustomFunctionsCode = "")
        {
            CustomFunctionsCodeCSharp = transformTSLToCSharp(CustomFunctionsCode);

            Normalized = Normalize(input);
            Delegate function = compile();
            return x => (double) function.DynamicInvoke(x);
        }

        protected override string Normalize(string input)
        {
            return input.ReplaceMultipling('x').ReplacePow('x').ReplaceToDoubles();
        }

        public double Invoke(double x)
        {
            if (evaluatedFunction == null)
                throw new NullReferenceException(Strings.NoFunctionToInvoke);

            double result = default(double);

            try
            {
                result = (double) evaluatedFunction.DynamicInvoke(x);
            }
            catch (Exception ex2)
            {
                result = double.NaN;

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