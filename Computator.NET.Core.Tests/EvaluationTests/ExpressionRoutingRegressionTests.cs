using System;
using System.Numerics;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.EvaluationTests
{
    [TestFixture]
    public class ExpressionRoutingRegressionTests
    {
        private readonly ModeDeterminer _modeDeterminer = new ModeDeterminer(new TslCompiler());

        [OneTimeSetUp]
        public void InitializeNativeFunctions() => GSLInitializer.Initialize();

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \r\n\t")]
        [TestCase("/* y = i */")]
        [TestCase("// z = y\r\n")]
        public void EmptyExpressionsHaveErrorMode(string input)
        {
            Assert.That(_modeDeterminer.DetermineMode(input), Is.EqualTo(CalculationsMode.Error));
        }

        [TestCase("x>0?y:0", CalculationsMode.Fxy)]
        [TestCase("x==0?y:1", CalculationsMode.Fxy)]
        [TestCase("x<=0?y:1", CalculationsMode.Fxy)]
        [TestCase("x!=0?y:1", CalculationsMode.Fxy)]
        [TestCase("y>0?1:0", CalculationsMode.Fxy)]
        [TestCase("z.Real", CalculationsMode.Complex)]
        [TestCase("new[]{z}[0]", CalculationsMode.Complex)]
        [TestCase("new[]{i}[0]", CalculationsMode.Complex)]
        [TestCase("Sample(\" i = z \" ,y)", CalculationsMode.Fxy)]
        [TestCase("Sample(@\" i = z \" ,x)", CalculationsMode.Real)]
        [TestCase("Sample('i',x)", CalculationsMode.Real)]
        [TestCase("Sample(\"\"\" i = z \"\"\",x)", CalculationsMode.Real)]
        [TestCase("Sample($\" i = {y} \" ,x)", CalculationsMode.Fxy)]
        [TestCase("holder.y + holder.i + holder.z", CalculationsMode.Real)]
        [TestCase("Sample(y:1)", CalculationsMode.Real)]
        [TestCase("x /* = y + i */ + 1", CalculationsMode.Real)]
        [TestCase("x + 1 // = z + i", CalculationsMode.Real)]
        [TestCase("((x²+y²=25))", CalculationsMode.Real)]
        [TestCase("x²+y²+z²=25", CalculationsMode.Fxy)]
        [TestCase("x+i=2", CalculationsMode.Error)]
        public void RoutingUsesExecutableSyntax(string input, CalculationsMode expected)
        {
            Assert.That(_modeDeterminer.DetermineMode(input), Is.EqualTo(expected));
        }

        [TestCase("x==0?y:1", 0, 7, 7)]
        [TestCase("x!=0?y:1", 0, 7, 1)]
        [TestCase("x<=0?y:1", -2, 7, 7)]
        [TestCase("x>=0?y:1", -2, 7, 1)]
        [TestCase("x>0?y:1", 2, 7, 7)]
        [TestCase("((x²+y²=25))", 3, 4, 0)]
        [TestCase("(x>0?y:1)=2", 3, 7, 5)]
        public void RoutedExpressionsCompileAndEvaluate(string input, double x, double y, double expected)
        {
            var function = new ExpressionsEvaluator().Evaluate(input, "", _modeDeterminer.DetermineMode(input));
            Assert.That(function.Evaluate(x, y), Is.EqualTo(expected).Within(1e-12));
        }

        [TestCase("System.Convert.ToDouble(\"1=1\".Length)+x", 5)]
        [TestCase("System.Convert.ToDouble(\" i \".Length)+x", 5)]
        [TestCase("x /* = y + i */ + 1", 3)]
        [TestCase("x+1 // = z + i", 3)]
        public void LiteralAndCommentContentsDoNotChangeEvaluation(string input, double expected)
        {
            var mode = _modeDeterminer.DetermineMode(input);
            Assert.That(mode, Is.EqualTo(CalculationsMode.Real));
            var function = new ExpressionsEvaluator().Evaluate(input, "", mode);
            Assert.That(function.Evaluate(2), Is.EqualTo(expected));
        }

        [Test]
        public void MemberAccessOnComplexVariableSelectsComplexEvaluator()
        {
            var function = new ExpressionsEvaluator().Evaluate("z.Real", "", _modeDeterminer.DetermineMode("z.Real"));
            Assert.That(function.Evaluate(new Complex(3, 4)), Is.EqualTo(new Complex(3, 0)));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        [TestCase("/* empty */")]
        public void EvaluationRejectsEmptyInputWithArgumentError(string input)
        {
            Assert.Throws<ArgumentException>(() => new ExpressionsEvaluator().Evaluate(input, "", CalculationsMode.Real));
        }

        [Test]
        public void ChainedEquationsAreRejectedInsteadOfAssigningAParameter()
        {
            Assert.Throws<ArgumentException>(() => new ExpressionsEvaluator().Evaluate("x=y=1", "", CalculationsMode.Real));
        }
    }
}
