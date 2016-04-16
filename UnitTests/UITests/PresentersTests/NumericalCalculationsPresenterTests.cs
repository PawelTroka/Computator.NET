using System;
using System.Collections.Generic;
using Computator.NET;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Evaluation;
using Computator.NET.Functions;
using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Views;
using Moq;
using NUnit.Framework;

namespace UnitTests.UITests.PresentersTests
{
    [TestFixture]
    public class NumericalCalculationsPresenterTests
    {
        [Test]
        public void IntegralSelectedOperationChanged_ShouldChangeVisibility()
        {
            _numericalCalculationsViewMock.SetupAllProperties();
            _numericalCalculationsPresenter = new NumericalCalculationsPresenter(_numericalCalculationsViewMock.Object,
    _errorHandlerMock.Object, _expressionViewMock.Object, _customFunctionsViewMock.Object);

            _numericalCalculationsViewMock.SetupGet(m => m.SelectedOperation).Returns(Strings.Integral);

            _numericalCalculationsViewMock.Raise(m => m.OperationChanged += null, new EventArgs());
            
            _numericalCalculationsViewMock.VerifySet(m=>m.IntervalVisible=true);
            _numericalCalculationsViewMock.VerifySet(m => m.StepsVisible = true);

            _numericalCalculationsViewMock.VerifySet(m => m.ErrorVisible = false);
            _numericalCalculationsViewMock.VerifySet(m => m.DerrivativeVisible = false);
        }

        [Test]
        public void FunctionRootSelectedOperationChanged_ShouldChangeVisibility()
        {
            _numericalCalculationsViewMock.SetupAllProperties();
            _numericalCalculationsPresenter = new NumericalCalculationsPresenter(_numericalCalculationsViewMock.Object,
    _errorHandlerMock.Object, _expressionViewMock.Object, _customFunctionsViewMock.Object);

            _numericalCalculationsViewMock.SetupGet(m => m.SelectedOperation).Returns(Strings.Function_root);

            _numericalCalculationsViewMock.Raise(m => m.OperationChanged += null, new EventArgs());

            _numericalCalculationsViewMock.VerifySet(m => m.IntervalVisible = true);
            _numericalCalculationsViewMock.VerifySet(m => m.StepsVisible = true);

            _numericalCalculationsViewMock.VerifySet(m => m.ErrorVisible = true);
            _numericalCalculationsViewMock.VerifySet(m => m.DerrivativeVisible = false);
        }

        [Test]
        public void DerrivativeSelectedOperationChanged_ShouldChangeVisibility()
        {
            _numericalCalculationsViewMock.SetupAllProperties();
            _numericalCalculationsPresenter = new NumericalCalculationsPresenter(_numericalCalculationsViewMock.Object,
    _errorHandlerMock.Object, _expressionViewMock.Object, _customFunctionsViewMock.Object);

            _numericalCalculationsViewMock.SetupGet(m => m.SelectedOperation).Returns(Strings.Derivative);

            _numericalCalculationsViewMock.Raise(m => m.OperationChanged += null, new EventArgs());


            _numericalCalculationsViewMock.VerifySet(m => m.DerrivativeVisible = true);
            _numericalCalculationsViewMock.VerifySet(m => m.ErrorVisible = true);

            _numericalCalculationsViewMock.VerifySet(m => m.IntervalVisible = false);
            _numericalCalculationsViewMock.VerifySet(m => m.StepsVisible = false);


            
        }

        [SetUp]
        public void Init()
        {
            _errorHandlerMock = new Mock<IErrorHandler>();
            //      _errorHandlerMock.SetupAllProperties();

            _numericalCalculationsViewMock = new Mock<INumericalCalculationsView>();
            //        _numericalCalculationsViewMock.SetupAllProperties();

            _customFunctionsViewMock = new Mock<ITextProvider>();
            //          _customFunctionsViewMock.SetupAllProperties();

            _expressionViewMock = new Mock<ITextProvider>();
            //            _expressionViewMock.SetupAllProperties();



            _numericalCalculationsViewMock.Setup(
                m =>
                    m.AddResult(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<string>()))
                .Verifiable();
        }

        private const int AMax = 2;
        private const int AMin = -2;
        private const int AInc = 2;
        private const int NMax = 2;
        private const double EpsMin = 0.01;
        private const int EpsMax = 1;
        private const double EpsInc = 0.5;

        private  static void GenerateTestCases()
        {
            var interalsTestCases = new List<object>();
            var functionRootTestCases = new List<object>();
            var derrivativeTestCases = new List<object>();

            foreach (var function in functions)
                for (double a = AMin; a <= AMax; a += AInc)
                {
                    for (uint n = 1000; n <= NMax*1000; n+=1000)
                    {

                        for (var b = a; b <= AMax; b += AInc)
                        {
                            foreach (var integrationMethod in NumericalMethodsInfo.Instance.IntegrationMethods)
                                if(integrationMethod.Key != Strings.Monte_Carlo_method)
                                {
                                    
                                    interalsTestCases.Add(new object[]
                                {integrationMethod.Value, function.Value, integrationMethod.Key, function.Key, a, b,

                                (integrationMethod.Key != Strings.Romberg_s_method) ?
                                    n : n/1000 + 2 });}

                            for (var eps = EpsMin; eps < EpsMax; eps += EpsInc)
                                foreach (var functionRootMethod in NumericalMethodsInfo.Instance.FunctionRootMethods)
                                    functionRootTestCases.Add(new object[]
                                    {
                                        functionRootMethod.Value, function.Value, functionRootMethod.Key, function.Key,
                                        a,
                                        b, n, eps
                                    });
                        }
                    }
                }


            foreach (var function in functions)
                for (double a = AMin; a <= AMax; a += AInc)
                    for (uint n = 0; n <= NMax; n++)
                        for (var eps = EpsMin; eps < EpsMax; eps += EpsInc)
                            foreach (var derrivativeMethod in NumericalMethodsInfo.Instance.DerrivationMethods)
                                derrivativeTestCases.Add(new object[]
                                {
                                    derrivativeMethod.Value, function.Value, derrivativeMethod.Key, function.Key, a, n,
                                    eps
                                });


            _integralTestCases = interalsTestCases.ToArray();
            _derrivativeTestCases = derrivativeTestCases.ToArray();
            _functionRootTestCases = functionRootTestCases.ToArray();
        }

        private static object[] _integralTestCases;

        public static object[] _derrivativeTestCases;


        public static object[] _functionRootTestCases;

        private Mock<ITextProvider> _customFunctionsViewMock;
        private Mock<IErrorHandler> _errorHandlerMock;
        private Mock<ITextProvider> _expressionViewMock;
        private NumericalCalculationsPresenter _numericalCalculationsPresenter;
        private Mock<INumericalCalculationsView> _numericalCalculationsViewMock;



        private static readonly Dictionary<string, Func<double, double>> functions = new Dictionary
            <string, Func<double, double>>
        {
            {"cos(x)", Math.Cos},
            {"sin(x)", Math.Sin},
            {"tan(x)", Math.Tan},
         //   {"AiryAi(x)", x => SpecialFunctions.AiryAi(x)},
            {"x+0.001", x => x + 0.001}
        };


        [OneTimeSetUp]
        public void SetupTestCases()
        {
          
        }


        private void SetupBase(string opeartion, string method, string expression)
        {
            EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Real));
            _expressionViewMock.SetupGet(m => m.Text).Returns(expression);

            _numericalCalculationsViewMock.SetupGet(m => m.SelectedOperation).Returns(opeartion);
            _numericalCalculationsViewMock.SetupGet(m => m.SelectedMethod).Returns(method);

            _numericalCalculationsPresenter = new NumericalCalculationsPresenter(_numericalCalculationsViewMock.Object,
    _errorHandlerMock.Object, _expressionViewMock.Object, _customFunctionsViewMock.Object);
        }

        private void SetupForIntegral(string method, string expression, double a, double b, uint n)
        {
            SetupBase(Strings.Integral, method, expression);

            _numericalCalculationsViewMock.SetupGet(m => m.A).Returns(a);
            _numericalCalculationsViewMock.SetupGet(m => m.B).Returns(b);
            _numericalCalculationsViewMock.SetupGet(m => m.N).Returns(n);
        }

        private void SetupForDerrivative(string method, string expression, double eps, double x, uint order)
        {
            SetupBase(Strings.Derivative, method, expression);

            _numericalCalculationsViewMock.SetupGet(m => m.Epsilon).Returns(eps);
            _numericalCalculationsViewMock.SetupGet(m => m.X).Returns(x);
            _numericalCalculationsViewMock.SetupGet(m => m.Order).Returns(order);
        }


        private void SetupForFunctionRoot(string method, string expression, double a, double b, uint n, double eps)
        {
            SetupBase(Strings.Function_root, method, expression);

            _numericalCalculationsViewMock.SetupGet(m => m.A).Returns(a);
            _numericalCalculationsViewMock.SetupGet(m => m.B).Returns(b);
            _numericalCalculationsViewMock.SetupGet(m => m.N).Returns(n);
            _numericalCalculationsViewMock.SetupGet(m => m.Epsilon).Returns(eps);
        }


        [TestCaseSource(nameof(DerrivativeTestCases))]
        public void DerrivativeForValidParametersTest_ShouldReturnCorrectValues(
            Func<Func<double, double>, double, uint, double, double> func,
            Func<double, double> function,
            string method, string expression,  double x, uint order, double eps)
        {
            SetupForDerrivative(method, expression, eps, x, order);

            _numericalCalculationsViewMock.Raise(m => m.ComputeClicked += null, new EventArgs());

            _numericalCalculationsViewMock.Verify(
                m => m.AddResult(
                    It.Is<string>(s => s == expression),
                    It.Is<string>(s => s == Strings.Derivative),
                    It.Is<string>(s => s == method),
                    It.IsAny<string>(),
                    It.Is<string>(s => s == func(function, x, order, eps).ToMathString())),
                Times.Once);
        }

        [TestCaseSource(nameof(FunctionRootTestCases))]
        public void FunctionRootForValidParametersTest_ShouldReturnCorrectValues(
            Func<Func<double, double>, double, double, double, uint, double> func,
            Func<double, double> function,
            string method, string expression,
            double a, double b, uint n, double eps)
        {
            SetupForFunctionRoot(method, expression, a, b, n, eps);

            _numericalCalculationsViewMock.Raise(m => m.ComputeClicked += null, new EventArgs());

            _numericalCalculationsViewMock.Verify(
                m => m.AddResult(
                    It.Is<string>(s => s == expression),
                    It.Is<string>(s => s == Strings.Function_root),
                    It.Is<string>(s => s == method),
                    It.IsAny<string>(),
                    It.Is<string>(s => s == func(function, a, b, eps, n).ToMathString())),
                Times.Once);
        }

        [TestCaseSource(nameof(IntegralTestCases))]
        public void IntegralForValidParametersTest_ShouldReturnCorrectValues(
            Func<Func<double, double>, double, double, double, double> func,
            Func<double, double> function,
            string method, string expression,
            double a, double b, uint n)
        {
            SetupForIntegral(method, expression, a, b, n);

            _numericalCalculationsViewMock.Raise(m => m.ComputeClicked += null, new EventArgs());

            _numericalCalculationsViewMock.Verify(
                m => m.AddResult(
                    It.Is<string>(s => s == expression),
                    It.Is<string>(s => s == Strings.Integral),
                    It.Is<string>(s => s == method),
                    It.IsAny<string>(),
                    It.Is<string>(s => s == func(function, a, b, n).ToMathString())),
                Times.Once);
        }

        public static object[] IntegralTestCases
        {
            get { if (_integralTestCases == null) GenerateTestCases(); return _integralTestCases; }
            set { _integralTestCases = value; }
        }

        public static object[] DerrivativeTestCases
        {
            get { if (_derrivativeTestCases == null) GenerateTestCases(); return _derrivativeTestCases; }
            set { _derrivativeTestCases = value; }
        }

        public static object[] FunctionRootTestCases
        {
            get { if(_functionRootTestCases==null) GenerateTestCases(); return _functionRootTestCases; }
            set { _functionRootTestCases = value; }
        }
    }
}