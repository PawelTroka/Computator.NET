//#define DO_NOT_USE_ABS_YET
using System;
using Computator.NET.Core.Compilation;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Tests.CompilersTests
{
	[TestFixture]
	public class TslCompilerTests
	{
		[SetUp]
		public void Init()
		{
			_tslCompiler = new TslCompiler();
		}

		private TslCompiler _tslCompiler;


		[Test]
		public void SimpleFunction_ShouldNotChange()
		{
			IsTheSameAfterCompilation(@"Hypergeometric0F1(y,x)");
		}

		[Test]
		public void WhileExpressionInParenthesis_ShouldNotChange()
		{
			IsTheSameAfterCompilation(@"while(sample!=""igo"")
	(akka+=""beta"");");
		}

		[Test]
		public void WhileExpression_ShouldNotChange()
		{
			IsTheSameAfterCompilation(@"while(sample!=""igo"")
	akka+=""beta"";");
		}

		[Test]
		public void EmptyWhileExpression_ShouldNotChange()
		{
			IsTheSameAfterCompilation(@"while(b!=42);");
		}

		[Test]
		public void IfExpressionInParenthesis_ShouldNotChange()
		{
			IsTheSameAfterCompilation(@"if(a==b)
	(a+=b);");
		}

		[Test]
		public void IfExpression_ShouldNotChange()
		{
			IsTheSameAfterCompilation(@"if(a==b)
	a+=b;");
		}

		[Test]
		public void EmptyIfExpressionWithString_ShouldNotChange()
		{
			IsTheSameAfterCompilation(@"if(phrase!=""empty"")");
		}

		[Test]
		public void EmptyIfExpression_ShouldNotChange()
		{
			IsTheSameAfterCompilation(@"if(values==123.12)");
		}

		[Test]
		public void ForExpressionInParenthesis_ShouldNotChange()
		{
			IsTheSameAfterCompilation(@"for(var j=0;j<10;j++)
	(r+=PI+e+function(j,r));");
		}

		[Test]
		public void ForExpression_ShouldNotChange()
		{
			IsTheSameAfterCompilation(@"for(var j=0;j<10;j++)
	r+=PI+e+function(j,r);");
		}

		[Test]
		public void EmptyForExpression_ShouldNotChange()
		{
			IsTheSameAfterCompilation(@"for(var j=0;j<10;j++);");
		}

		[Test]
		public void FunctionInParenthesisFollowedWByTwoInParenthesis_ShouldBeMultiplying()
		{
			Assert.AreEqual("(cos(x))*(2)", _tslCompiler.TransformToCSharp("(cos(x))(2)"));
		}

		[Test]
		public void FunctionInParenthesisFollowedWByTwo_ShouldBeMultiplying()
		{
			Assert.AreEqual("(cos(x))*2", _tslCompiler.TransformToCSharp("(cos(x))2"));
		}

		[Test]
		public void FunctionInParenthesisFollowedWByXInParenthesis_ShouldBeMultiplying()
		{
			Assert.AreEqual("(cos(x))*(x)", _tslCompiler.TransformToCSharp("(cos(x))(x)"));
		}

		[Test]
		public void TwoFollowedByExpressionInParenthesis_ShouldBeMultiplying()
		{
			Assert.AreEqual("2*(cos(x)+x)", _tslCompiler.TransformToCSharp("2(cos(x)+x)"));
		}

		[Test]
		public void TwoFollowedByFunctionFollowedByX_ShouldBeMultiplying()
		{
			Assert.AreEqual("2*cos(x)*x", _tslCompiler.TransformToCSharp("2cos(x)x"));
		}

		[Test]
		public void FunctionInParenthesisFollowedWhitespaceAndX_ShouldBeMultiplying()
		{
			Assert.AreEqual("(cos(x))*x", _tslCompiler.TransformToCSharp("(cos(x))  x"));
		}

		[Test]
		public void TwoForThePowerOfTwoInParenthesisFollowedByByX_ShouldBeMultiplying()
		{
			Assert.AreEqual("(pow(2,2))*x", _tslCompiler.TransformToCSharp("(2²)x"));
		}

		[Test]
		public void TwoForThePowerOfTwoFollowedByByX_ShouldBeMultiplying()
		{
			Assert.AreEqual("pow(2,2)*x",_tslCompiler.TransformToCSharp("2²x"));   
		}

		private bool IsTheSameAfterCompilation(string code)
		{
			var afterTransform = _tslCompiler.TransformToCSharp(code);

			if (code == afterTransform)
				return true;
			throw new AssertionException($@"Expected:{Environment.NewLine}{code}{Environment.NewLine}Actual:{Environment.NewLine}{afterTransform}");
		}

		[Test]
		public void CommasInSuperscriptTest()
		{
			var func = $@"2ᴹᵃᵗʸᵃˢ⁽ˣ{SpecialSymbols.CommaSuperscript}ʸ⁾";

			Assert.AreEqual(@"pow(2,Matyas(x,y))", _tslCompiler.TransformToCSharp(func));
		}

		[Test]
		public void ConstantRaisedToConstantPowerPlusOneTest()
		{
			Assert.AreEqual("pow(e,PI*i)+1",
				_tslCompiler.TransformToCSharp("eᴾᴵ˙ⁱ+1"));
		}

		[Test]
		public void ConstantRaisedToConstantPowerTest()
		{
			Assert.AreEqual("pow(e,PI*i)",
				_tslCompiler.TransformToCSharp("eᴾᴵ˙ⁱ"));
		}


		[Test]
		public void CustomVariableRaisedToConstantPowerPlusOneTest()
		{
			Assert.AreEqual("pow(u,PI*i)+1",
				_tslCompiler.TransformToCSharp("uᴾᴵ˙ⁱ+1"));
		}

		[Test]
		public void CustomVariableRaisedToConstantPowerTest()
		{
			Assert.AreEqual("pow(u,PI*i)",
				_tslCompiler.TransformToCSharp("uᴾᴵ˙ⁱ"));
		}


		[Test]
		public void EulerNumberInParenthesisRaisedToThePowerOfPiAndImaginaryUnitPlusOneTest()
		{
			Assert.AreEqual("pow((e),PI*i)+1",
				_tslCompiler.TransformToCSharp("(e)ᴾᴵ˙ⁱ+1"));
		}

		[Test]
		public void FloatNumberRaisedToThePowerPiMultipliedByImaginaryUnitPlusOneTest()
		{
			Assert.AreEqual("pow(2.6,PI*i)+1",
				_tslCompiler.TransformToCSharp("2.6ᴾᴵ˙ⁱ+1"));
		}

		[Test]
		public void FunctionWithSuperscriptInItsNameShouldntBeInterpretedAsExponentShortTest()
		{
			IsTheSameAfterCompilation(@"ψⁿ(x);");
		}

		[Test]
		public void FunctionWithSuperscriptInItsNameShouldntBeInterpretedAsExponentTest()
		{
			IsTheSameAfterCompilation(@"r+=e+ψⁿ(j,r);");
		}

		[Test]
		public void IntegerNumberRaisedToThePowerImaginaryUnitTest()
		{
			//       tslCompiler.Variables.Add("i");

			Assert.AreEqual("pow(2,i)",
				_tslCompiler.TransformToCSharp("2ⁱ"));
		}


		[Test]
		public void LocalFunctionUsingExponentWithParenthesisTest()
		{
			var func = $@"var f(real x, real y) = (x / y)²;";

			Assert.AreEqual(@"var f = TypeDeducer.Func((real x, real y) => pow((x / y),2));",
				_tslCompiler.TransformToCSharp(func));
		}

		[Test]
		public void LocalFunctionUsingParenthesisTest()
		{
			var func = $@"var f(real x, real y) = (x)+(y)+(2);";

			Assert.AreEqual(@"var f = TypeDeducer.Func((real x, real y) => (x)+(y)+(2));",
				_tslCompiler.TransformToCSharp(func));
		}

		[Test]
		public void LongCustomVariableRaisedToComplicatedExpressionTest()
		{
			Assert.AreEqual("pow(_kul9uXulu_var,cos(x)*sin(haxxxx/2.0))",
				_tslCompiler.TransformToCSharp(
					$"_kul9uXulu_varᶜᵒˢ⁽ˣ⁾˙ˢⁱⁿ⁽ʰᵃˣˣˣˣ˸²{SpecialSymbols.DecimalSeparatorSuperscript}⁰⁾"));
		}


		[Test]
		public void LongCustomVariableRaisedToConstantPowerPlusOneTest()
		{
			Assert.AreEqual("pow(functionOutput,PI*i)+1",
				_tslCompiler.TransformToCSharp("functionOutputᴾᴵ˙ⁱ+1"));
		}

		[Test]
		public void LongCustomVariableRaisedToConstantPowerTest()
		{
			Assert.AreEqual("pow(functionOutput,PI*i)",
				_tslCompiler.TransformToCSharp("functionOutputᴾᴵ˙ⁱ"));
		}


		//testing raising to power variables and constants:
		[Test]
		public void LongCustomVariableRaisedToLongCustomVariablePlusComplicatedExpressionsTest()
		{
			Assert.AreEqual(
				"2*cos(csaksah+chssss)/pow(chiEnergy,hahaha)-pow(complicated_variableVar,thisiscomplicatedReally)+2*cos(csaksah+chssss)/pow(chiEnergy,hahaha)",
				_tslCompiler.TransformToCSharp(
					"2·cos(csaksah+chssss)/chiEnergyʰᵃʰᵃʰᵃ-complicated_variableVarᵗʰⁱˢⁱˢᶜᵒᵐᵖˡⁱᶜᵃᵗᵉᵈᴿᵉᵃˡˡʸ+2·cos(csaksah+chssss)/chiEnergyʰᵃʰᵃʰᵃ"),
				"Fail!!!");
		}

		[Test]
		public void MultiplyingComplicatedExpressionTest()
		{
			Assert.AreEqual("  32132.321*Xaaa__sa+a/b-232121.321*c-(223.21321*simpleFunction(x))",
				_tslCompiler.TransformToCSharp("  32132.321Xaaa__sa+a/b-232121.321c-(223.21321simpleFunction(x))"),
				"Fail!!!");
		}


		[Test]
		public void MultiplyingEngineeringNotationShouldBeLeftUnchanged10Test()
		{
			Assert.AreEqual("2.2E-1",
				_tslCompiler.TransformToCSharp("2.2E-1"));
		}


		[Test]
		public void MultiplyingEngineeringNotationShouldBeLeftUnchanged1Test()
		{
			Assert.AreEqual("1e11",
				_tslCompiler.TransformToCSharp("1e11"));
		}

		[Test]
		public void MultiplyingEngineeringNotationShouldBeLeftUnchanged2Test()
		{
			Assert.AreEqual("2.1121e121",
				_tslCompiler.TransformToCSharp("2.1121e121"));
		}

		[Test]
		public void MultiplyingEngineeringNotationShouldBeLeftUnchanged3Test()
		{
			Assert.AreEqual("2E11",
				_tslCompiler.TransformToCSharp("2E11"));
		}

		[Test]
		public void MultiplyingEngineeringNotationShouldBeLeftUnchanged4Test()
		{
			Assert.AreEqual("2.1121E121",
				_tslCompiler.TransformToCSharp("2.1121E121"));
		}


		[Test]
		public void MultiplyingEngineeringNotationShouldBeLeftUnchanged5Test()
		{
			Assert.AreEqual("3.3E+21",
				_tslCompiler.TransformToCSharp("3.3E+21"));
		}


		[Test]
		public void MultiplyingEngineeringNotationShouldBeLeftUnchanged6Test()
		{
			Assert.AreEqual("2.2E-11",
				_tslCompiler.TransformToCSharp("2.2E-11"));
		}


		[Test]
		public void MultiplyingEngineeringNotationShouldBeLeftUnchanged7Test()
		{
			Assert.AreEqual("3.3e+21",
				_tslCompiler.TransformToCSharp("3.3e+21"));
		}


		[Test]
		public void MultiplyingEngineeringNotationShouldBeLeftUnchanged8Test()
		{
			Assert.AreEqual("2.2e-11",
				_tslCompiler.TransformToCSharp("2.2e-11"));
		}

		[Test]
		public void MultiplyingEngineeringNotationShouldBeLeftUnchanged9Test()
		{
			Assert.AreEqual("3.3E+2",
				_tslCompiler.TransformToCSharp("3.3E+2"));
		}


		[Test]
		public void MultiplyingPseudoEngineeringNotationShouldBChangedTest1()
		{
			Assert.AreEqual("2*Ex10aa",
				_tslCompiler.TransformToCSharp("2Ex10aa"));
		}


		[Test]
		public void MultiplyingPseudoEngineeringNotationShouldBChangedTest10()
		{
			Assert.AreEqual("1*e+1.1",
				_tslCompiler.TransformToCSharp("1e+1.1"));
		}

		[Test]
		public void MultiplyingPseudoEngineeringNotationShouldBChangedTest11()
		{
			Assert.AreEqual("2.22221*e-100.1321",
				_tslCompiler.TransformToCSharp("2.22221e-100.1321"));
		}

		[Test]
		public void MultiplyingPseudoEngineeringNotationShouldBChangedTest12()
		{
			Assert.AreEqual("9*E",
				_tslCompiler.TransformToCSharp("9E"));
		}

		[Test]
		public void MultiplyingPseudoEngineeringNotationShouldBChangedTest13()
		{
			Assert.AreEqual("9.92121*e",
				_tslCompiler.TransformToCSharp("9.92121e"));
		}


		[Test]
		public void MultiplyingPseudoEngineeringNotationShouldBChangedTest14()
		{
			Assert.AreEqual("(1*e)+2*aa",
				_tslCompiler.TransformToCSharp("(1e)+2aa"));
		}


		[Test]
		public void MultiplyingPseudoEngineeringNotationShouldBChangedTest145()
		{
			Assert.AreEqual("(1*e)*2*aa",
				_tslCompiler.TransformToCSharp("(1e) 2aa"));
		}

		[Test]
		public void MultiplyingPseudoEngineeringNotationShouldBChangedTest15()
		{
			Assert.AreEqual(@"(2*e)*2*q",
				_tslCompiler.TransformToCSharp(@"(2e)
2q"));
		}


		[Test]
		public void MultiplyingPseudoEngineeringNotationShouldBChangedTest16()
		{
			Assert.AreEqual(@"2*E
(1*e)",
				_tslCompiler.TransformToCSharp(@"2E
(1e)"));
		}

		[Test]
		public void MultiplyingPseudoEngineeringNotationShouldBChangedTest2()
		{
			Assert.AreEqual("2*Ex10",
				_tslCompiler.TransformToCSharp("2Ex10"));
		}

		[Test]
		public void MultiplyingPseudoEngineeringNotationShouldBChangedTest3()
		{
			Assert.AreEqual("1*e11a",
				_tslCompiler.TransformToCSharp("1e11a"));
		}

		[Test]
		public void MultiplyingPseudoEngineeringNotationShouldBChangedTest4()
		{
			Assert.AreEqual("3123.3321*e",
				_tslCompiler.TransformToCSharp("3123.3321e"));
		}

		[Test]
		public void MultiplyingPseudoEngineeringNotationShouldBChangedTest5()
		{
			Assert.AreEqual("2*E",
				_tslCompiler.TransformToCSharp("2E"));
		}

		[Test]
		public void MultiplyingPseudoEngineeringNotationShouldBChangedTest6()
		{
			Assert.AreEqual("(1*e)",
				_tslCompiler.TransformToCSharp("(1e)"));
		}

		[Test]
		public void MultiplyingPseudoEngineeringNotationShouldBChangedTest7()
		{
			Assert.AreEqual("1*e+1*E",
				_tslCompiler.TransformToCSharp("1e+1E"));
		}

		[Test]
		public void MultiplyingPseudoEngineeringNotationShouldBChangedTest8()
		{
			Assert.AreEqual("2*e-10*e",
				_tslCompiler.TransformToCSharp("2e-10e"));
		}

		[Test]
		public void MultiplyingPseudoEngineeringNotationShouldBChangedTest9()
		{
			Assert.AreEqual("1*e+1*e+1*e+1*e+1*e+1*e",
				_tslCompiler.TransformToCSharp("1e+1e+1e+1e+1e+1e"));
		}

		[Test]
		public void MultiplyingSimpleFunctionByFloatTest()
		{
			Assert.AreEqual("21212.321312*_simplErFunc(x,y, z, aaaA_a)",
				_tslCompiler.TransformToCSharp("21212.321312_simplErFunc(x,y, z, aaaA_a)"));
		}


		[Test]
		public void MultiplyingSimpleFunctionByIntegerTest()
		{
			Assert.AreEqual("2*cos(x)",
				_tslCompiler.TransformToCSharp("2cos(x)"));
		}

		[Test]
		public void ParenthesisWithinParenthesisAllToExponentTest()
		{
			Assert.AreEqual(@"pow((pow((x+y),1/2.0)+pow((z-x-y),z)),2)",
				_tslCompiler.TransformToCSharp(@"((x+y)¹˸²+(z-x-y)ᶻ)²"));
		}

		[Test]
		public void PassingLambdaAsArgumentTest1()
		{
			IsTheSameAfterCompilation(@"Derivative.derivative(ax => MathieuSE(1,1,ax),x,6)");
		}

		[Test]
		public void PassingLambdaAsArgumentTest2()
		{
			IsTheSameAfterCompilation(@"Derivative.derivative((x) => MathieuSE(1,1,x),x,6)");
		}

		[Test]
		public void PassingLambdaAsArgumentTest3()
		{
			IsTheSameAfterCompilation(@"Derivative.derivative((real x) => MathieuSE(1,1,x),x,6)");
		}

		[Test]
		public void SpacesInExponentAndLocalFunctionTest()
		{
			var def = @"   var    f(   real   x    ,   real    y  )    =   x  ²   ˸  ʸ ˙  ²  ;  ";

			Assert.AreEqual(
				@"   var f = TypeDeducer.Func((   real   x    ,   real    y  ) => pow(x,  2   /  y *  2  ));  ",
				_tslCompiler.TransformToCSharp(def));
		}

#if !DO_NOT_USE_ABS_YET
		[Test]
		public void AbsWithCosAndSin()
		{
			Assert.AreEqual(@"y*abs(cos(z))+x*abs(sin(z))", _tslCompiler.TransformToCSharp(@"y|cos(z)|+x|sin(z)|"));
		}
		[Test]
		public void AbsInSuperscriptTest()
		{
			Assert.AreEqual(@"(pow(z,1000*abs(z)))/(1.0*((pow(z,1000*abs(z)))))", _tslCompiler.TransformToCSharp(@"(z¹⁰⁰⁰ꞋᶻꞋ)/(z¹⁰⁰⁰ꞋᶻꞋ)"));
		}

		[Test]
		public void AbsWithoutParenthesisInSuperscriptTest()
		{
			Assert.AreEqual(@"pow(z,1000*abs(z))/pow(z,1000*abs(z))", _tslCompiler.TransformToCSharp(@"z¹⁰⁰⁰ꞋᶻꞋ/z¹⁰⁰⁰ꞋᶻꞋ"));
		}

		[Test]
		public void AbsInSuperscriptWithMultiplyingByConstantTest()
		{
			Assert.AreEqual(@"pow(z,z*abs(z)+2)/pow(z,PI*abs(z+2))", _tslCompiler.TransformToCSharp(@"zᶻꞋᶻꞋ⁺²/zᴾᴵꞋᶻ⁺²Ꞌ"));
		}

		[Test]
		public void AbsTest()
		{
			Assert.AreEqual(@"abs(cos(x))=abs(y)", _tslCompiler.TransformToCSharp(@"|cos(x)|=|y|"));
		}

		[Test]
		public void AbsTest1()
		{
			Assert.AreEqual(@"abs(x)*abs(y)", _tslCompiler.TransformToCSharp(@"|x||y|"));
		}
		[Test]
		public void AbsTest2()
		{
			Assert.AreEqual(@"abs(x)*abs(y)", _tslCompiler.TransformToCSharp($@"|x|{Environment.NewLine}|y|"));
		}

		[Test]
		public void AbsTest3()
		{
			Assert.AreEqual(@"abs(1-2*abs( 2+x)+1)", _tslCompiler.TransformToCSharp(@"|1-2| 2+x|+1|"));
		}

		[Test]
		public void AbsTest4()
		{
			Assert.AreEqual(@"abs(x)*abs(x)", _tslCompiler.TransformToCSharp($@"|x|{Environment.NewLine}{Environment.NewLine}|x|"));
		}

		[Test]
		public void AbsTest5()
		{
			Assert.AreEqual(@"abs(cos(x))   =abs(y)", _tslCompiler.TransformToCSharp(@"|cos(x)|   =|y|"));
		}

		[Test]
		public void AbsTest6()
		{
			Assert.AreEqual(@"abs(     -    2  )", _tslCompiler.TransformToCSharp(@"|     -    2  |"));
		}

		[Test]
		public void AbsTest7()
		{
			Assert.AreEqual(@"abs(-2)", _tslCompiler.TransformToCSharp(@"|-2|"));
		}

		[Test]
		public void AbsTest8()
		{
			Assert.AreEqual(@"abs(1-abs(2+x)+1)", _tslCompiler.TransformToCSharp(@"|1-|2+x|+1|"));
		}

		[Test]
		public void AbsTest81()
		{
			Assert.AreEqual(@"x*abs(1-abs(2+x)+1)", _tslCompiler.TransformToCSharp(@"x|1-|2+x|+1|"));
		}

		[Test]
		public void AbsTest9()
		{
			Assert.AreEqual(@"abs(1-2*abs(2+x)+1)", _tslCompiler.TransformToCSharp(@"|1-2|2+x|+1|"));
		}

		[Test]
		public void AbsTest10()
		{
			Assert.AreEqual(@"abs(abs(1-2*abs(2+x)+1))", _tslCompiler.TransformToCSharp(@"||1-2|2+x|+1||"));
		}
		[Test]
		public void AbsTest11()
		{
			Assert.AreEqual(@"abs(-abs(1-2*abs(2+x)+1))", _tslCompiler.TransformToCSharp(@"|-|1-2|2+x|+1||"));
		}

		[Test]
		public void AbsTest12()
		{
			Assert.AreEqual(@"abs(1-abs(2+x))", _tslCompiler.TransformToCSharp(@"|1-|2+x||"));
		}

		[Test]
		public void AbsTest13()
		{
			Assert.AreEqual(@"abs(x)", _tslCompiler.TransformToCSharp(@"|x|"));
		}

		[Test]
		public void AbsTest14()
		{
			Assert.AreEqual(@"2*abs(x)", _tslCompiler.TransformToCSharp(@"2|x|"));
		}

		[Test]
		public void AbsTest15()
		{
			Assert.AreEqual(@"2*abs(   x      )", _tslCompiler.TransformToCSharp(@"2|   x      |"));
		}

		[Test]
		public void AbsTest16()
		{
			Assert.AreEqual(@"abs(2*abs(x)+abs(-2+x))", _tslCompiler.TransformToCSharp(@"|2|x|+|-2+x||"));
		}

		[Test]
		public void AbsTest17()
		{
			Assert.AreEqual(@"2*abs(x)+abs(-2+x)", _tslCompiler.TransformToCSharp(@"2|x|+|-2+x|"));
		}

		[Test]
		public void AbsTest18()
		{
			IsTheSameAfterCompilation(@"if(true || false)");
		}
		[Test]
		public void AbsTest19()
		{
			IsTheSameAfterCompilation(@"a|b");
		}

		[Test]
		public void AbsSuperscriptTest1()
		{
			Assert.AreEqual(@"pow(2,abs(cos(x))-2*x)", _tslCompiler.TransformToCSharp($@"2{SpecialSymbols.ModulusSuperscript}ᶜᵒˢ⁽ˣ⁾{SpecialSymbols.ModulusSuperscript}⁻²ˣ"));
		}
#endif


		[Test]
		public void ComplicatedExpressionWithMultiplePowersAndDecimalSepratorInSuperscript_ShouldUsePowFunctionAndDecimalSeparator()
		{
			// tslCompiler.Variables.Add("x");

			Assert.AreEqual("pow(x,2)+cos(pow(x,2*x+1.1+cos(x)))+2/3.0",
				_tslCompiler.TransformToCSharp($"x²+cos(x²˙ˣ⁺¹{SpecialSymbols.DecimalSeparatorSuperscript}¹⁺ᶜᵒˢ⁽ˣ⁾)+2/3"),
				"Fail!!!");
		}

		[Test]
		public void InlineFunctionWithTypes_ShouldBeTypeDeducerFuncWithTypes()
		{
			Assert.AreEqual("var f = TypeDeducer.Func((real x, real y, complex z) => 100/(1.0*((2+2))))",
				_tslCompiler.TransformToCSharp("var f(real x, real y, complex z)=100/(2+2)"));
		}

		[Test]
		public void ExpressionWithPowerWithPlusMultiplyingAndDivison_ShouldUsePowFunctionWithCorrectlySwitchedOperators()
		{
//            tslCompiler.Variables.AddRange(new[] {"x", "y", "z"});

			Assert.AreEqual("z*x*y+pow(y,x*z*y+11*x+cos(x/y))",
				_tslCompiler.TransformToCSharp("z·x·y+yˣ˙ᶻ˙ʸ⁺¹¹˙ˣ⁺ᶜᵒˢ⁽ˣ˸ʸ⁾"));
		}

		[Test]
		public void InlineFunction_ShouldTransformToTypeDeducerFunc()
		{
			Assert.AreEqual("var sumOfValues = TypeDeducer.Func((string str, integer k) => k+k+k+str.Length)",
				_tslCompiler.TransformToCSharp("function sumOfValues(string str, integer k)=k+k+k+str.Length"));
		}

		[Test]
		public void ComplicatedExpressionWithPowersAndParenthesis_ShouldBeWithPowFunctionAndCorrectParenthesis()
		{
			//    tslCompiler.Variables.Add("x");

			Assert.AreEqual("(pow(10,2)*x)/(1.0*((10-6*pow(x,2)+pow((25-pow(x,2)),2)+10*(25-pow(x,2)))))",
				_tslCompiler.TransformToCSharp("(10²·x)/(10-6·x²+(25-x²)²+10·(25-x²))"));
		}

		[Test]
		public void TwoToPowerWithSuperscriptDecimalSeparator_ShouldBeFunctionPowWithOrdinaryDecimalSeparator()
		{
			//    tslCompiler.Variables.Add("x");

			Assert.AreEqual("pow(2,103213.323232)",
				_tslCompiler.TransformToCSharp($"2¹⁰³²¹³{SpecialSymbols.DecimalSeparatorSuperscript}³²³²³²"));
		}

		[Test]
		public void FunctionToThePowerOfImgainaryUnit_ShouldBeFunctionPow()
		{
			//     tslCompiler.Variables.Add("i");

			Assert.AreEqual("pow((cos(1.0)),i)",
				_tslCompiler.TransformToCSharp("(cos(1.0))ⁱ"));
		}

		//1e+1e+1e+1e+1e+1e
		//1e+1E

		//2e-10e
	}
}