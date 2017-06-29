using System;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Model;
using Computator.NET.Core.Presenters;
using Computator.NET.Core.Services.ErrorHandling;
using Computator.NET.Localization;
using Moq;
using NUnit.Framework;

namespace Computator.NET.Tests.PresentersTests
{
    [TestFixture]
    public class NumericalCalculationsPresenterTests
    {

        private Mock<ICodeEditorView> _customFunctionsViewMock;
        private Mock<IErrorHandler> _errorHandlerMock;
        private Mock<ITextProvider> _expressionViewMock;
        private Mock<INumericalCalculationsView> _numericalCalculationsViewMock;
        private Mock<IExceptionsHandler> _exceptionsHandlerMock;
        private Mock<IExpressionsEvaluator> _expressionsEvaluatorMock;

        private NumericalCalculationsPresenter _sut;


        [SetUp]
        public void Init()
        {
            _errorHandlerMock = new Mock<IErrorHandler>();
            //      _errorHandlerMock.SetupAllProperties();

            _numericalCalculationsViewMock = new Mock<INumericalCalculationsView>();
            //        _numericalCalculationsViewMock.SetupAllProperties();

            _customFunctionsViewMock = new Mock<ICodeEditorView>();
            //          _customFunctionsViewMock.SetupAllProperties();

            _expressionViewMock = new Mock<ITextProvider>();
            //            _expressionViewMock.SetupAllProperties();


            _numericalCalculationsViewMock.SetupAllProperties();

            _numericalCalculationsViewMock.Setup(
                m =>
                    m.AddResult(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<string>()))
                .Verifiable();


            _expressionsEvaluatorMock = new Mock<IExpressionsEvaluator>();
            _exceptionsHandlerMock = new Mock<IExceptionsHandler>();

            _sut = new NumericalCalculationsPresenter(_numericalCalculationsViewMock.Object,_errorHandlerMock.Object,new SharedViewState(), _exceptionsHandlerMock.Object, _expressionViewMock.Object, _customFunctionsViewMock.Object, _expressionsEvaluatorMock.Object);
        }

        [Test]
        public void DerrivativeSelectedOperationChanged_ShouldChangeVisibility()
        {
            //arrange
            _numericalCalculationsViewMock.SetupGet(m => m.SelectedOperation).Returns(Strings.Derivative);

            //act
            _numericalCalculationsViewMock.Raise(m => m.OperationChanged += null, new EventArgs());

            //assert
            _numericalCalculationsViewMock.VerifySet(m => m.DerrivativeVisible = true);
            _numericalCalculationsViewMock.VerifySet(m => m.ErrorVisible = true);
            _numericalCalculationsViewMock.VerifySet(m => m.IntervalVisible = false);
            _numericalCalculationsViewMock.VerifySet(m => m.StepsVisible = false);
        }

        [Test]
        public void FunctionRootSelectedOperationChanged_ShouldChangeVisibility()
        {
            //arrange
            _numericalCalculationsViewMock.SetupGet(m => m.SelectedOperation).Returns(Strings.Function_root);

            //act
            _numericalCalculationsViewMock.Raise(m => m.OperationChanged += null, new EventArgs());

            //assert
            _numericalCalculationsViewMock.VerifySet(m => m.IntervalVisible = true);
            _numericalCalculationsViewMock.VerifySet(m => m.StepsVisible = true);
            _numericalCalculationsViewMock.VerifySet(m => m.ErrorVisible = true);
            _numericalCalculationsViewMock.VerifySet(m => m.DerrivativeVisible = false);
        }

        [Test]
        public void IntegralSelectedOperationChanged_ShouldChangeVisibility()
        {
            //arrange
            _numericalCalculationsViewMock.SetupGet(m => m.SelectedOperation).Returns(Strings.Integral);

            //act
            _numericalCalculationsViewMock.Raise(m => m.OperationChanged += null, new EventArgs());


            //assert
            _numericalCalculationsViewMock.VerifySet(m => m.IntervalVisible = true);
            _numericalCalculationsViewMock.VerifySet(m => m.StepsVisible = true);
            _numericalCalculationsViewMock.VerifySet(m => m.ErrorVisible = false);
            _numericalCalculationsViewMock.VerifySet(m => m.DerrivativeVisible = false);
        }
    }
}