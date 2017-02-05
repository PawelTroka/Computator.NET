using System;
using System.Collections.Generic;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.Localization;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Bootstrapping;
using Computator.NET.Core.Evaluation;
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
        [SetUp]
        public void Init()
        {
            _container = (new CoreBootstrapper()).Container;


            _errorHandlerMock = new Mock<IErrorHandler>();
            //      _errorHandlerMock.SetupAllProperties();
            _container.RegisterInstance(_errorHandlerMock.Object, new ContainerControlledLifetimeManager());

            _numericalCalculationsViewMock = new Mock<INumericalCalculationsView>();
            //        _numericalCalculationsViewMock.SetupAllProperties();

            _container.RegisterInstance(_numericalCalculationsViewMock.Object, new ContainerControlledLifetimeManager());


            _customFunctionsViewMock = new Mock<ICodeEditorView>();
            //          _customFunctionsViewMock.SetupAllProperties();
            _container.RegisterInstance(_customFunctionsViewMock.Object, new ContainerControlledLifetimeManager());
            _container.RegisterInstance<ISupportsExceptionHighliting>(_customFunctionsViewMock.Object, new ContainerControlledLifetimeManager());


            _container.RegisterInstance(new Mock<IChart2D>().Object);
            _container.RegisterInstance(new Mock<IComplexChart>().Object);
            _container.RegisterInstance(new Mock<IChart3D>().Object);


            _expressionViewMock = new Mock<ITextProvider>();
            //            _expressionViewMock.SetupAllProperties();
            _container.RegisterInstance(_expressionViewMock.Object, new ContainerControlledLifetimeManager());


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
            {"cos(x)", Math.Cos},
            {"sin(x)", Math.Sin},
            {"tan(x)", Math.Tan},
            //   {"AiryAi(x)", x => SpecialFunctions.AiryAi(x)},
            {"x+0.001", x => x + 0.001}
        };

        private IUnityContainer _container;


        [OneTimeSetUp]
        public void SetupTestCases()
        {
        }


        private void SetupBase(string opeartion, string method, string expression)
        {
            _expressionViewMock.SetupGet(m => m.Text).Returns(expression);

            _numericalCalculationsViewMock.SetupGet(m => m.SelectedOperation).Returns(opeartion);
            _numericalCalculationsViewMock.SetupGet(m => m.SelectedMethod).Returns(method);

            // _numericalCalculationsPresenter = new NumericalCalculationsPresenter(_numericalCalculationsViewMock.Object,
            //    _errorHandlerMock.Object);
            _numericalCalculationsPresenter = _container.Resolve<NumericalCalculationsPresenter>();

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