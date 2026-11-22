using System;
using System.ComponentModel;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Model;
using Computator.NET.Core.Presenters;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Moq;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.PresentersTests
{
    [TestFixture]
    public class SymbolicExpressionRoutingTests
    {
        [TestCase("theta² + alpha")]
        [TestCase("2·x + x⁽ʸ⁺¹⁾")]
        [TestCase("xᶜᵒˢ⁽ʸ⁾")]
        [TestCase("[[1,2],[3,4]]")]
        [TestCase("x²=2")]
        [TestCase("sin(")]
        public void SymbolicTypingAndEnterBypassNumericTslClassification(string input)
        {
            var expression = new Mock<IExpressionTextBox>();
            expression.SetupGet(value => value.Text).Returns(input);
            var view = new Mock<IExpressionView>();
            view.SetupGet(value => value.ExpressionTextBox).Returns(expression.Object);
            var state = new Mock<ISharedViewState>();
            state.SetupGet(value => value.CurrentView).Returns(ViewName.SymbolicCalculations);
            var calls = 0;
            state.SetupGet(value => value.CurrentAction).Returns((Action<object, EventArgs>)((sender, args) => calls++));
            var mode = new Mock<IModeDeterminer>(MockBehavior.Strict);
            var definitions = new Mock<IScriptProvider>(MockBehavior.Strict);
            _ = new ExpressionViewPresenter(view.Object, state.Object, mode.Object, definitions.Object);
            expression.Raise(value => value.TextChanged += null, EventArgs.Empty);
            expression.Raise(value => value.KeyPress += null, new KeyPressEventArgs('\r'));
            NUnit.Framework.Assert.That(calls, Is.EqualTo(1));
            mode.VerifyNoOtherCalls();
            definitions.VerifyNoOtherCalls();
        }

        [Test]
        public void ReturningToNumericCalculationsRestoresItsExistingClassification()
        {
            var expression = new Mock<IExpressionTextBox>();
            expression.SetupGet(value => value.Text).Returns("x²");
            var view = new Mock<IExpressionView>();
            view.SetupGet(value => value.ExpressionTextBox).Returns(expression.Object);
            var state = new Mock<ISharedViewState>();
            state.SetupAllProperties();
            state.Object.CurrentView = ViewName.SymbolicCalculations;
            var mode = new Mock<IModeDeterminer>();
            mode.Setup(value => value.DetermineMode("x²", "")).Returns(CalculationsMode.Real);
            var definitions = new Mock<IScriptProvider>();
            definitions.SetupGet(value => value.Text).Returns("");
            _ = new ExpressionViewPresenter(view.Object, state.Object, mode.Object, definitions.Object);
            state.Object.CurrentView = ViewName.Calculations;
            state.Raise(value => value.PropertyChanged += null, new PropertyChangedEventArgs(nameof(ISharedViewState.CurrentView)));
            mode.Verify(value => value.DetermineMode("x²", ""), Times.Once);
            view.VerifySet(value => value.Visible = true, Times.Once);
        }
    }
}
