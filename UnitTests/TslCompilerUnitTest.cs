using Computator.NET.Compilation;
using Computator.NET.Data;
using Computator.NET.DataTypes;
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
        public void PassingLambdaAsArgumentTest1()
        {
            IsTheSameAfterCompilation(@"Derivative.derivative(ax => MathieuSE(1,1,ax),x,6)"); 
        }

        [TestMethod]
        public void PassingLambdaAsArgumentTest2()
        {
            IsTheSameAfterCompilation(@"Derivative.derivative((x) => MathieuSE(1,1,x),x,6)");
        }

        [TestMethod]
        public void PassingLambdaAsArgumentTest3()
        {
            IsTheSameAfterCompilation(@"Derivative.derivative((real x) => MathieuSE(1,1,x),x,6)");
        }

        private bool IsTheSameAfterCompilation(string code)
        {
            if (code == tslCompiler.TransformToCSharp(code))
                return true;
            else
                throw new AssertFailedException();
        }

        [TestMethod]
        public void CommasInSuperscriptTest()
        {
            var func = $@"2ᴹᵃᵗʸᵃˢ⁽ˣ{SpecialSymbols.CommaSuperscript}ʸ⁾";

            Assert.AreEqual(@"pow(2,Matyas(x,y))", tslCompiler.TransformToCSharp(func));
        }

        [TestMethod]
        public void Test1()
        {
           // tslCompiler.Variables.Add("x");

            Assert.AreEqual("pow(x,2)+cos(pow(x,2*x+1.1+cos(x)))+2/3.0",
                tslCompiler.TransformToCSharp($"x²+cos(x²˙ˣ⁺¹{SpecialSymbols.DecimalSeparatorSuperscript}¹⁺ᶜᵒˢ⁽ˣ⁾)+2/3"), "Fail!!!");
        }

        [TestMethod]
        public void Test6()
        {
        //    tslCompiler.Variables.Add("x");

            Assert.AreEqual("pow(2,103213.323232)", tslCompiler.TransformToCSharp($"2¹⁰³²¹³{SpecialSymbols.DecimalSeparatorSuperscript}³²³²³²"), "Fail!!!");
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
//            tslCompiler.Variables.AddRange(new[] {"x", "y", "z"});

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
        //    tslCompiler.Variables.Add("x");

            Assert.AreEqual("(pow(10,2)*x)/(1.0*((10-6*pow(x,2)+pow((25-pow(x,2)),2)+10*(25-pow(x,2)))))",
                tslCompiler.TransformToCSharp("(10²·x)/(10-6·x²+(25-x²)²+10·(25-x²))"), "Fail!!!");
        }

        [TestMethod]
        public void Test7()
        {
       //     tslCompiler.Variables.Add("i");

            Assert.AreEqual("pow((cos(1.0)),i)",
                tslCompiler.TransformToCSharp("(cos(1.0))ⁱ"), "Fail!!!");
        }

        [TestMethod]
        public void IntegerNumberRaisedToThePowerImaginaryUnitTest()
        {
     //       tslCompiler.Variables.Add("i");

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
            Assert.AreEqual("2*cos(csaksah+chssss)/pow(chiEnergy,hahaha)-pow(complicated_variableVar,thisiscomplicatedReally)+2*cos(csaksah+chssss)/pow(chiEnergy,hahaha)",
    tslCompiler.TransformToCSharp("2·cos(csaksah+chssss)/chiEnergyʰᵃʰᵃʰᵃ-complicated_variableVarᵗʰⁱˢⁱˢᶜᵒᵐᵖˡⁱᶜᵃᵗᵉᵈᴿᵉᵃˡˡʸ+2·cos(csaksah+chssss)/chiEnergyʰᵃʰᵃʰᵃ"), "Fail!!!");
        }

        [TestMethod]
        public void LongCustomVariableRaisedToComplicatedExpressionTest()
        {
            Assert.AreEqual("pow(_kul9uXulu_var,cos(x)*sin(haxxxx/2.0))",
    tslCompiler.TransformToCSharp($"_kul9uXulu_varᶜᵒˢ⁽ˣ⁾˙ˢⁱⁿ⁽ʰᵃˣˣˣˣ˸²{SpecialSymbols.DecimalSeparatorSuperscript}⁰⁾"), "Fail!!!");
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



        [TestMethod]
        public void MultiplyingSimpleFunctionByIntegerTest()
        {
            Assert.AreEqual("2*cos(x)",
    tslCompiler.TransformToCSharp("2cos(x)"), "Fail!!!");
        }

        [TestMethod]
        public void MultiplyingSimpleFunctionByFloatTest()
        {
            Assert.AreEqual("21212.321312*_simplErFunc(x,y, z, aaaA_a)",
    tslCompiler.TransformToCSharp("21212.321312_simplErFunc(x,y, z, aaaA_a)"), "Fail!!!");
        }

        [TestMethod]
        public void MultiplyingComplicatedExpressionTest()
        {
            Assert.AreEqual("  32132.321*Xaaa__sa+a/b-232121.321*c-(223.21321*simpleFunction(x))",
    tslCompiler.TransformToCSharp("  32132.321Xaaa__sa+a/b-232121.321c-(223.21321simpleFunction(x))"), "Fail!!!");
        }


        [TestMethod]
        public void MultiplyingEngineeringNotationShouldBeLeftUnchanged1Test()
        {
            Assert.AreEqual("1e11",
    tslCompiler.TransformToCSharp("1e11"), "Fail!!!");
        }

        [TestMethod]
        public void MultiplyingEngineeringNotationShouldBeLeftUnchanged2Test()
        {
            Assert.AreEqual("2.1121e121",
    tslCompiler.TransformToCSharp("2.1121e121"), "Fail!!!");
        }

        [TestMethod]
        public void MultiplyingEngineeringNotationShouldBeLeftUnchanged3Test()
        {
            Assert.AreEqual("2E11",
    tslCompiler.TransformToCSharp("2E11"), "Fail!!!");
        }

        [TestMethod]
        public void MultiplyingEngineeringNotationShouldBeLeftUnchanged4Test()
        {
            Assert.AreEqual("2.1121E121",
    tslCompiler.TransformToCSharp("2.1121E121"), "Fail!!!");
        }



        [TestMethod]
        public void MultiplyingEngineeringNotationShouldBeLeftUnchanged5Test()
        {
            Assert.AreEqual("3.3E+21",
    tslCompiler.TransformToCSharp("3.3E+21"), "Fail!!!");
        }



        [TestMethod]
        public void MultiplyingEngineeringNotationShouldBeLeftUnchanged6Test()
        {
            Assert.AreEqual("2.2E-11",
    tslCompiler.TransformToCSharp("2.2E-11"), "Fail!!!");
        }


        [TestMethod]
        public void MultiplyingEngineeringNotationShouldBeLeftUnchanged7Test()
        {
            Assert.AreEqual("3.3e+21",
    tslCompiler.TransformToCSharp("3.3e+21"), "Fail!!!");
        }


        [TestMethod]
        public void MultiplyingEngineeringNotationShouldBeLeftUnchanged8Test()
        {
            Assert.AreEqual("2.2e-11",
    tslCompiler.TransformToCSharp("2.2e-11"), "Fail!!!");
        }
        [TestMethod]
        public void MultiplyingEngineeringNotationShouldBeLeftUnchanged9Test()
        {
            Assert.AreEqual("3.3E+2",
    tslCompiler.TransformToCSharp("3.3E+2"), "Fail!!!");
        }



        [TestMethod]
        public void MultiplyingEngineeringNotationShouldBeLeftUnchanged10Test()
        {
            Assert.AreEqual("2.2E-1",
    tslCompiler.TransformToCSharp("2.2E-1"), "Fail!!!");
        }



        [TestMethod]
        public void MultiplyingPseudoEngineeringNotationShouldBChangedTest1()
        {
            Assert.AreEqual("2*Ex10aa",
    tslCompiler.TransformToCSharp("2Ex10aa"), "Fail!!!");
        }

        [TestMethod]
        public void MultiplyingPseudoEngineeringNotationShouldBChangedTest2()
        {
            Assert.AreEqual("2*Ex10",
    tslCompiler.TransformToCSharp("2Ex10"), "Fail!!!");
        }

        [TestMethod]
        public void MultiplyingPseudoEngineeringNotationShouldBChangedTest3()
        {
            Assert.AreEqual("1*e11a",
    tslCompiler.TransformToCSharp("1e11a"), "Fail!!!");
        }

        [TestMethod]
        public void MultiplyingPseudoEngineeringNotationShouldBChangedTest4()
        {
            Assert.AreEqual("3123.3321*e",
    tslCompiler.TransformToCSharp("3123.3321e"), "Fail!!!");
        }

        [TestMethod]
        public void MultiplyingPseudoEngineeringNotationShouldBChangedTest5()
        {
            Assert.AreEqual("2*E",
    tslCompiler.TransformToCSharp("2E"), "Fail!!!");
        }

        [TestMethod]
        public void MultiplyingPseudoEngineeringNotationShouldBChangedTest6()
        {
            Assert.AreEqual("(1*e)",
    tslCompiler.TransformToCSharp("(1e)"), "Fail!!!");
        }
        [TestMethod]
        public void MultiplyingPseudoEngineeringNotationShouldBChangedTest7()
        {
            Assert.AreEqual("1*e+1*E",
    tslCompiler.TransformToCSharp("1e+1E"), "Fail!!!");
        }
        [TestMethod]
        public void MultiplyingPseudoEngineeringNotationShouldBChangedTest8()
        {
            Assert.AreEqual("2*e-10*e",
    tslCompiler.TransformToCSharp("2e-10e"), "Fail!!!");
        }
        [TestMethod]
        public void MultiplyingPseudoEngineeringNotationShouldBChangedTest9()
        {
            Assert.AreEqual("1*e+1*e+1*e+1*e+1*e+1*e",
    tslCompiler.TransformToCSharp("1e+1e+1e+1e+1e+1e"), "Fail!!!");
        }


        [TestMethod]
        public void MultiplyingPseudoEngineeringNotationShouldBChangedTest10()
        {
            Assert.AreEqual("1*e+1.1",
    tslCompiler.TransformToCSharp("1e+1.1"), "Fail!!!");
        }
        [TestMethod]
        public void MultiplyingPseudoEngineeringNotationShouldBChangedTest11()
        {
            Assert.AreEqual("2.22221*e-100.1321",
    tslCompiler.TransformToCSharp("2.22221e-100.1321"), "Fail!!!");
        }
        [TestMethod]

        public void MultiplyingPseudoEngineeringNotationShouldBChangedTest12()
        {
            Assert.AreEqual("9*E",
    tslCompiler.TransformToCSharp("9E"), "Fail!!!");
        }

        [TestMethod]
        public void MultiplyingPseudoEngineeringNotationShouldBChangedTest13()
        {
            Assert.AreEqual("9.92121*e",
    tslCompiler.TransformToCSharp("9.92121e"), "Fail!!!");
        }



        [TestMethod]
        public void MultiplyingPseudoEngineeringNotationShouldBChangedTest14()
        {
            Assert.AreEqual("(1*e)+2*aa",
    tslCompiler.TransformToCSharp("(1e)+2aa"), "Fail!!!");
        }


        [TestMethod]
        public void MultiplyingPseudoEngineeringNotationShouldBChangedTest145()
        {
            Assert.AreEqual("(1*e) 2*aa",
    tslCompiler.TransformToCSharp("(1e) 2aa"), "Fail!!!");
        }

        [TestMethod]
        public void MultiplyingPseudoEngineeringNotationShouldBChangedTest15()
        {
            Assert.AreEqual(@"(2*e)
2*q",
    tslCompiler.TransformToCSharp(@"(2e)
2q"), "Fail!!!");
        }


        [TestMethod]
        public void MultiplyingPseudoEngineeringNotationShouldBChangedTest16()
        {
            Assert.AreEqual(@"2*E
(1*e)",
    tslCompiler.TransformToCSharp(@"2E
(1e)"), "Fail!!!");
        }

        //2e-10e
        //1e+1E
        //1e+1e+1e+1e+1e+1e
    }
}