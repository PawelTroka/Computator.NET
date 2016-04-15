using System;
using Computator.NET;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Evaluation;
using Computator.NET.NumericalCalculations;
using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UnitTests.UITests.PresentersTests
{
    [TestClass]
    public class NumericalCalculationsPresenterTests
    {
        private Mock<ITextProvider> _customFunctionsViewMock;
        private Mock<IErrorHandler> _errorHandlerMock;
        private Mock<ITextProvider> _expressionViewMock;
        private NumericalCalculationsPresenter _numericalCalculationsPresenter;
        private Mock<INumericalCalculationsView> _numericalCalculationsViewMock;


        [TestInitialize]
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
        }

        [TestMethod]
        public void ComputeClickedForValidParametersTest_ShouldReturnCorrectValues()
        {
            EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Real));
            var a = 0.0;
            var b = 1.0;
            uint n = 10000;
            var eps = 1e-3;


            _numericalCalculationsViewMock.SetupGet(m => m.Epsilon).Returns(eps);
            _numericalCalculationsViewMock.SetupGet(m => m.A).Returns(a);
            _numericalCalculationsViewMock.SetupGet(m => m.B).Returns(b);
            _numericalCalculationsViewMock.SetupGet(m => m.N).Returns(n);
            _numericalCalculationsViewMock.SetupGet(m => m.SelectedOperation).Returns(Strings.Integral);
            _numericalCalculationsViewMock.SetupGet(m => m.SelectedMethod).Returns(Strings.trapezoidal_method);
            _numericalCalculationsViewMock.Setup(
                m =>
                    m.AddResult(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<string>()))
                .Verifiable();
            _expressionViewMock.SetupGet(m => m.Text).Returns("cos(x)");


            _numericalCalculationsPresenter = new NumericalCalculationsPresenter(_numericalCalculationsViewMock.Object,
                _errorHandlerMock.Object, _expressionViewMock.Object, _customFunctionsViewMock.Object);


            _numericalCalculationsViewMock.Raise(m => m.ComputeClicked += null, new EventArgs());


            _numericalCalculationsViewMock.Verify(
                m => m.AddResult(
                    It.Is<string>(s => s == "cos(x)"),
                    It.Is<string>(s => s == Strings.Integral),
                    It.Is<string>(s => s == Strings.trapezoidal_method),
                    It.IsAny<string>(),
                    It.Is<string>(s => s == Integral.trapezoidalMethod(Math.Cos, a, b, n).ToMathString())),
                Times.Once);
        }
    }
}