using System;
using Computator.NET.Localization;
using Meta.Numerics;

namespace Computator.NET.Evaluation
{
    internal class Function3DEvaluator : Evaluator
    {
        public Function3DEvaluator()
        {
            functionType = typeof (Func<double, double, double>);
            lambdaFunc = @"           
            public static double CustomFunction(double x, double y)
            {
                return ";
        }

        public Func<double, double, double> Evaluate(string input, string CustomFunctionsCode = "")
        {
            CustomFunctionsCodeCSharp = transformTSLToCSharp(CustomFunctionsCode);

            Normalized = Normalize(input);
            Delegate function = compile();
            return (x, y) => (double) function.DynamicInvoke(x, y);
        }

        protected override string Normalize(string input) //TODO: make it real f(x,y) normalize
        {
            return input.ReplaceMultipling('x', 'y').ReplacePow('x', 'y').ReplaceToDoubles();
        }

        public double Invoke(double x, double y)
        {
            if (evaluatedFunction == null)
                throw new NullReferenceException(Strings.NoFunctionToInvoke);

            double result = default(double);

            try
            {
                result = (double) evaluatedFunction.DynamicInvoke(x, y);
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