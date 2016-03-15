using Computator.NET.Compilation;
using Computator.NET.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class AutocompletionDataTest
    {
        [TestMethod]
        public void TestScripting()
        {
            var content = AutocompletionData.GetAutocompleteItemsForScripting();
        }

        [TestMethod]
        public void TestExpressions()
        {
            var content = AutocompletionData.GetAutocompleteItemsForExpressions();
        }
    }


    [TestClass]
    public class TslCompilerUnitTest
    {
        private TslCompiler tslCompiler;

        [TestInitialize]
        public void Init()
        {
            tslCompiler = new TslCompiler();
        }

        [TestMethod]
        public void Test1()
        {
            tslCompiler.Variables.Add("x");

            Assert.AreEqual("pow(x,2)+cos(pow(x,2*x+1.1+cos(x)))+2/3.0",
                tslCompiler.TransformToCSharp("x²+cos(x²˙ˣ⁺¹ॱ¹⁺ᶜᵒˢ⁽ˣ⁾)+2/3"), "Fail!!!");
        }

        [TestMethod]
        public void Test6()
        {
            tslCompiler.Variables.Add("x");

            Assert.AreEqual("pow(2,103213.323232)", tslCompiler.TransformToCSharp("2¹⁰³²¹³ॱ³²³²³²"), "Fail!!!");
        }

        [TestMethod]
        public void Test2()
        {
            Assert.AreEqual("var f = TypeDeducer.Func((real x, real y, complex z) => 100/(1.0*((2+2))))",
                tslCompiler.TransformToCSharp("var f(real x, real y, complex z)=100/(2+2)"), "Fail!!!");
        }

        [TestMethod]
        public void Test3()
        {
            tslCompiler.Variables.AddRange(new[] {"x", "y", "z"});

            Assert.AreEqual("z*x*y+pow(y,x*z*y+11*x+cos(x/y))",
                tslCompiler.TransformToCSharp("z·x·y+yˣ˙ᶻ˙ʸ⁺¹¹˙ˣ⁺ᶜᵒˢ⁽ˣ˸ʸ⁾"), "Fail!!!");
        }

        [TestMethod]
        public void Test4()
        {
            Assert.AreEqual("var sumOfValues = TypeDeducer.Func((string str, integer k) => k+k+k+str.Length)",
                tslCompiler.TransformToCSharp("function sumOfValues(string str, integer k)=k+k+k+str.Length"), "Fail!!!");
        }

        [TestMethod]
        public void Test5()
        {
            tslCompiler.Variables.Add("x");

            Assert.AreEqual("(pow(10,2)*x)/(1.0*((10-6*pow(x,2)+pow((25-pow(x,2)),2)+10*(25-pow(x,2)))))",
                tslCompiler.TransformToCSharp("(10²·x)/(10-6·x²+(25-x²)²+10·(25-x²))"), "Fail!!!");
        }

        [TestMethod]
        public void Test7()
        {
            tslCompiler.Variables.Add("i");

            Assert.AreEqual("pow((cos(1.0)),i)",
                tslCompiler.TransformToCSharp("(cos(1.0))ⁱ"), "Fail!!!");
        }

        [TestMethod]
        public void IntegerNumberRaisedToThePowerImaginaryUnitTest()
        {
            tslCompiler.Variables.Add("i");

            Assert.AreEqual("pow(2,i)",
                tslCompiler.TransformToCSharp("2ⁱ"), "Fail!!!");
        }

        [TestMethod]
        public void FloatNumberRaisedToThePowerPiMultipliedByImaginaryUnitPlusOneTest()
        {
            Assert.AreEqual("pow(2.6,PI*i)+1",
    tslCompiler.TransformToCSharp("2.6ᴾᴵ˙ⁱ+1"), "Fail!!!");

        }


        [TestMethod]
        public void EulerNumberInParenthesisRaisedToThePowerOfPiAndImaginaryUnitPlusOneTest()
        {
            Assert.AreEqual("pow((e),PI*i)+1",
    tslCompiler.TransformToCSharp("(e)ᴾᴵ˙ⁱ+1"), "Fail!!!");
        }





        //testing raising to power variables and constants:
        [TestMethod]
        public void LongCustomVariableRaisedToLongCustomVariablePlusComplicatedExpressionsTest()
        {
            Assert.AreEqual("2*cos(csaksah+chssss)/pow(chiEnergy,hahaha)-pow(complicated_variableVar,thisiscomplicatedreally)+2*cos(csaksah+chssss)/pow(chiEnergy,hahaha)",
    tslCompiler.TransformToCSharp("2·cos(csaksah+chssss)/chiEnergyʰᵃʰᵃʰᵃ-complicated_variableVarᵗʰⁱˢⁱˢᶜᵒᵐᵖˡⁱᶜᵃᵗᵉᵈᴿᵉᵃˡˡʸ+2·cos(csaksah+chssss)/chiEnergyʰᵃʰᵃʰᵃ"), "Fail!!!");
        }

        [TestMethod]
        public void LongCustomVariableRaisedToComplicatedExpressionTest()
        {
            Assert.AreEqual("pow(_kul9uXulu_var,cos(x)*sin(haxxxx/2.0))",
    tslCompiler.TransformToCSharp("_kul9uXulu_varᶜᵒˢ⁽ˣ⁾˙ˢⁱⁿ⁽ʰᵃˣˣˣˣ˸²ॱ⁰⁾"), "Fail!!!");
        }



        [TestMethod]
        public void LongCustomVariableRaisedToConstantPowerPlusOneTest()
        {
            Assert.AreEqual("pow(functionOutput,PI*i)+1",
    tslCompiler.TransformToCSharp("functionOutputᴾᴵ˙ⁱ+1"), "Fail!!!");
        }

        [TestMethod]
        public void LongCustomVariableRaisedToConstantPowerTest()
        {
            Assert.AreEqual("pow(functionOutput,PI*i)",
    tslCompiler.TransformToCSharp("functionOutputᴾᴵ˙ⁱ"), "Fail!!!");
        }


        [TestMethod]
        public void CustomVariableRaisedToConstantPowerPlusOneTest()
        {
            Assert.AreEqual("pow(u,PI*i)+1",
    tslCompiler.TransformToCSharp("uᴾᴵ˙ⁱ+1"), "Fail!!!");
        }

        [TestMethod]
        public void CustomVariableRaisedToConstantPowerTest()
        {
            Assert.AreEqual("pow(u,PI*i)",
    tslCompiler.TransformToCSharp("uᴾᴵ˙ⁱ"), "Fail!!!");
        }

        [TestMethod]
        public void ConstantRaisedToConstantPowerPlusOneTest()
        {
            Assert.AreEqual("pow(e,PI*i)+1",
    tslCompiler.TransformToCSharp("eᴾᴵ˙ⁱ+1"), "Fail!!!");
        }

        [TestMethod]
        public void ConstantRaisedToConstantPowerTest()
        {
            Assert.AreEqual("pow(e,PI*i)",
    tslCompiler.TransformToCSharp("eᴾᴵ˙ⁱ"), "Fail!!!");
        }
    }
}