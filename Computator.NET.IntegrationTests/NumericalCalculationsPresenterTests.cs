using System;
using System.Collections.Generic;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.Localization;
using Moq;
using NUnit.Framework;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Services;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Bootstrapping;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Computator.NET.Core.Presenters;
using Computator.NET.Core.Services.ErrorHandling;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.IntegrationTests
{
    public class IntegrationTestsBootstrapper : CoreBootstrapper
    {
        public IntegrationTestsBootstrapper()
        {

        }
    }

    [TestFixture]
    public partial class NumericalCalculationsPresenterTests
    {
        private Mock<IMessagingService> _messegingServiceMock = new Mock<IMessagingService>();
        [OneTimeSetUp]
        public void Setup()
        {
            _messegingServiceMock.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            GSLInitializer.Initialize(null);
        }

        [SetUp]
        public void Init()
        {
            _bootstrapper = new CoreBootstrapper();

            _errorHandlerMock = new Mock<IErrorHandler>();
            _bootstrapper.RegisterInstance(_errorHandlerMock.Object);

            _numericalCalculationsViewMock = new Mock<INumericalCalculationsView>();

            _bootstrapper.RegisterInstance(_numericalCalculationsViewMock.Object);


            _customFunctionsViewMock = new Mock<ICodeEditorView>();
            _bootstrapper.RegisterInstance(_customFunctionsViewMock.Object);
            _bootstrapper.RegisterInstance<ISupportsExceptionHighliting>(_customFunctionsViewMock.Object);


            _bootstrapper.RegisterInstance(new Mock<IChart2D>().Object);
            _bootstrapper.RegisterInstance(new Mock<IComplexChart>().Object);
            _bootstrapper.RegisterInstance(new Mock<IChart3D>().Object);


            _expressionViewMock = new Mock<ITextProvider>();
            _bootstrapper.RegisterInstance(_expressionViewMock.Object);


            _numericalCalculationsViewMock.Setup(
                m =>
                    m.AddResult(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<string>()))
                .Verifiable();
        }


        private Mock<ICodeEditorView> _customFunctionsViewMock;
        private Mock<IErrorHandler> _errorHandlerMock;
        private Mock<ITextProvider> _expressionViewMock;

        private NumericalCalculationsPresenter _numericalCalculationsPresenter;
        private Mock<INumericalCalculationsView> _numericalCalculationsViewMock;


        private static readonly Dictionary<string, Func<double, double>> functions = new Dictionary
            <string, Func<double, double>>
        {
            {"sin(x)", Math.Sin},
            {"tan(x)", Math.Tan},
            {"Gegenbauer1(1,x)", x => SpecialFunctions.Gegenbauer1(1,x)},
            {"x+0.001", x => x + 0.001}
        };

        private CoreBootstrapper _bootstrapper;

        private void SetupBase(string opeartion, string method, string expression)
        {
            _expressionViewMock.SetupGet(m => m.Text).Returns(expression);

            _numericalCalculationsViewMock.SetupGet(m => m.SelectedOperation).Returns(opeartion);
            _numericalCalculationsViewMock.SetupGet(m => m.SelectedMethod).Returns(method);

            _numericalCalculationsPresenter = _bootstrapper.Create<NumericalCalculationsPresenter>();

            EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Real));
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
            string method, string expression, double x, uint order, double eps)
        {
            SetupForDerrivative(method, expression, eps, x, order);

            _numericalCalculationsViewMock.Raise(m => m.ComputeClicked += null, new EventArgs());

            _numericalCalculationsViewMock.Verify(
                m =>
                    m.AddResult(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<string>()), Times.Once);

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
    }
}