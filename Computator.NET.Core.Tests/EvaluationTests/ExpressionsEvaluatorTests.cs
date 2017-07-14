using System;
using System.Collections.Generic;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Tests.EvaluationTests
{
    [TestFixture]
    public class ExpressionsEvaluatorTests
    {
        private ExpressionsEvaluator _expressionsEvaluator;

        private static readonly Dictionary<string,Func<double,double>> Functions = new Dictionary<string, Func<double, double>>()
        {
            {"cos(x)", Math.Cos},
            {"1/x+x-pow(1/x,x)/PI", (x) => 1/x+x-Math.Pow(1/x,x)/Math.PI},
            {"x+0.001", x => x + 0.001},
            {"Debye(1,x)", x => SpecialFunctions.Debye(1,x) }
        };

        [OneTimeSetUp]
        public void Init()
        {
            _expressionsEvaluator = new ExpressionsEvaluator();
            GSLInitializer.Initialize();
        }

        [TestCase("cos(x)", 321e1 - 123.21321)]
        [TestCase("1/x+x-pow(1/x,x)/PI", 0.46127846127861583)]
        [TestCase("x+0.001", 2e9 -981.1321412)]
        [TestCase("Debye(1,x)", 11)]
        public void ExpressionShouldEvaluateTheSameAsFunction(string tsl, double x)
        {
            //arrange
            var evaluatedFunction = _expressionsEvaluator.Evaluate(tsl, string.Empty, CalculationsMode.Real);

            //act
            var valueOfEvaluatedFunctionAtPointX = evaluatedFunction.Evaluate(x);
            var valueOfRealFunctionAtPointX = Functions[tsl].Invoke(x);

            //assert
            Assert.AreEqual(valueOfRealFunctionAtPointX, valueOfEvaluatedFunctionAtPointX);

        }
    }
}