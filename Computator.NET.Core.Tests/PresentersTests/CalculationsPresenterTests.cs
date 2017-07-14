using System;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Model;
using Computator.NET.Core.Presenters;
using Computator.NET.Core.Services.ErrorHandling;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.Functions;
using Computator.NET.Localization;
using Moq;
using NUnit.Framework;
using CalculationsMode = Computator.NET.DataTypes.CalculationsMode;

namespace Computator.NET.Core.Tests.PresentersTests
{
    [TestFixture]
    class CalculationsPresenterTests
    {
        private Mock<ICodeEditorView> _customFunctionsViewMock;
        private Mock<IErrorHandler> _errorHandlerMock;
        private Mock<ITextProvider> _expressionViewMock;
        private Mock<ICalculationsView> _numericalCalculationsViewMock;
        private Mock<IExceptionsHandler> _exceptionsHandlerMock;
        private Mock<IExpressionsEvaluator> _expressionsEvaluatorMock;

        private CalculationsPresenter _sut;
        private SharedViewState _sharedViewState;

        [SetUp]
        public void Init()
        {
            _errorHandlerMock = new Mock<IErrorHandler>();
            //      _errorHandlerMock.SetupAllProperties();

            _numericalCalculationsViewMock = new Mock<ICalculationsView>();
            //        _numericalCalculationsViewMock.SetupAllProperties();

            _customFunctionsViewMock = new Mock<ICodeEditorView>();
            //          _customFunctionsViewMock.SetupAllProperties();

            _expressionViewMock = new Mock<ITextProvider>();
            //            _expressionViewMock.SetupAllProperties();

            _numericalCalculationsViewMock.SetupAllProperties();
            _numericalCalculationsViewMock.Setup(
                m => m.AddResult(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _expressionsEvaluatorMock = new Mock<IExpressionsEvaluator>();
            _exceptionsHandlerMock = new Mock<IExceptionsHandler>();

            _sharedViewState = new SharedViewState();
            _sut = new CalculationsPresenter(_numericalCalculationsViewMock.Object,_errorHandlerMock.Object,_sharedViewState,_exceptionsHandlerMock.Object,_expressionViewMock.Object,_customFunctionsViewMock.Object,_expressionsEvaluatorMock.Object);
        }

        [TestCase(0)]
        [TestCase(222)]
        [TestCase(-13)]
        [TestCase(2.5)]
        [TestCase(-2.125)]
        [TestCase(1e22)]
        [TestCase(-3e12)]
        public void SimpleRealFunctionWithArguments_CalculateClicked_MathStringUsedInResults(double argument)
        {
            //arrange
            EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Real));
            _numericalCalculationsViewMock.Setup(m => m.X).Returns(argument);
            _expressionViewMock.Setup(m => m.Text).Returns("2x-2");
            var function = TypeDeducer.Func((double x) => 2 * x - 2);
            _expressionsEvaluatorMock.Setup<Function>(m => m.Evaluate(It.IsAny<string>(), It.IsAny<string>(),
            _sharedViewState.CalculationsMode)).Returns(new Function(function,FunctionType.Real2D));


            //act
            _numericalCalculationsViewMock.Raise(m => m.CalculateClicked += null, new EventArgs());

            //assert
            _numericalCalculationsViewMock.Verify(n => n.AddResult("2x-2", argument.ToMathString(), function(argument).ToMathString()),Times.Once);
        }
        
        [Test]
        public void EmptyExpression_CalculateClicked_MessageShown()
        {
            _expressionViewMock.Setup(m => m.Text).Returns(string.Empty);

            _numericalCalculationsViewMock.Raise(m => m.CalculateClicked += null, new EventArgs());

            _errorHandlerMock.Verify(m => m.DisplayError(Strings.Expression_should_not_be_empty_,It.IsAny<string>()),Times.Once);
        }

        [Test]
        public void EmptyExpression_CalculateClicked_ResultNotAdded()
        {
            _expressionViewMock.Setup(m => m.Text).Returns(string.Empty);

            _numericalCalculationsViewMock.Raise(m => m.CalculateClicked += null, new EventArgs());

            _numericalCalculationsViewMock.Verify(n => n.AddResult(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void FunctionThrowingException_CalculateClicked_ResultNotAdded()
        {
            //arrange
            EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Real));
            _numericalCalculationsViewMock.Setup(m => m.X).Returns(0);
            var function = TypeDeducer.Func((double x) => (x>0) ? 0.0 : throw new OverflowException());
            _expressionsEvaluatorMock.Setup<Function>(m => m.Evaluate(It.IsAny<string>(), It.IsAny<string>(),_sharedViewState.CalculationsMode)).Returns(new Function(function, FunctionType.Real2D));


            //act
            _numericalCalculationsViewMock.Raise(m => m.CalculateClicked += null, new EventArgs());

            //assert
            _numericalCalculationsViewMock.Verify(n => n.AddResult(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void FunctionThrowingException_CalculateClicked_ExceptionHandlerCalled()
        {
            //arrange
            EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Real));
            _numericalCalculationsViewMock.Setup(m => m.X).Returns(0);
            var function = TypeDeducer.Func((double x) => (x > 0) ? 0.0 : throw new OverflowException());
            _expressionsEvaluatorMock.Setup<Function>(m => m.Evaluate(It.IsAny<string>(), It.IsAny<string>(), CalculationsMode.Real)).Returns(new Function(function, FunctionType.Real2D));


            //act
            _numericalCalculationsViewMock.Raise(m => m.CalculateClicked += null, new EventArgs());

            //assert
            _exceptionsHandlerMock.Verify(m => m.HandleException(It.IsAny<CalculationException>()),Times.Once);
        }
    }
}
