using System;
using System.Collections.Generic;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Model;
using Computator.NET.Core.Presenters;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.Localization;
using Moq;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.PresentersTests
{
    [TestFixture, NonParallelizable]
    public class ChartAreaValuesPresenterTests
    {
        [TestCase(CalculationsMode.Real, "x", "y")]
        [TestCase(CalculationsMode.Fxy, "x", "y")]
        [TestCase(CalculationsMode.Complex, "Re(z)", "Im(z)")]
        public void InitialLabelsUseTheAlreadySelectedMode(CalculationsMode mode, string xAxis, string yAxis)
        {
            var view = new Mock<IChartAreaValuesView>();
            view.SetupAllProperties();
            var state = new Mock<ISharedViewState>();
            state.SetupGet(s => s.CalculationsMode).Returns(mode);

            using var presenter = new ChartAreaValuesPresenter(view.Object, state.Object);

            NUnit.Framework.Assert.That(view.Object.XAxisLabel, Is.EqualTo(xAxis));
            NUnit.Framework.Assert.That(view.Object.YAxisLabel, Is.EqualTo(yAxis));
            view.VerifySet(v => v.AddChartLabel = mode == CalculationsMode.Complex
                ? Strings.DrawChart : Strings.AddToChart, Times.Once());
        }

        [Test]
        public void ModeChangesPreserveBoundsAndRefreshExistingValidationMessages()
        {
            var view = new Mock<IChartAreaValuesView>();
            view.SetupAllProperties();
            var errors = new Dictionary<string, string>();
            view.Setup(v => v.SetError(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((property, error) => errors[property] = error);
            var state = new Mock<ISharedViewState>();
            state.SetupGet(s => s.CalculationsMode).Returns(CalculationsMode.Real);
            using var presenter = new ChartAreaValuesPresenter(view.Object, state.Object);

            // Invert both ranges, then switch mode without editing a field.
            view.Object.XMin = 7;
            view.Object.XMax = 2;
            view.Object.YMin = 9;
            view.Object.YMax = -4;
            view.Raise(v => v.XMinChanged += null, EventArgs.Empty);
            foreach (var mode in new[] { CalculationsMode.Complex, CalculationsMode.Fxy,
                CalculationsMode.Real, CalculationsMode.Complex })
            {
                // Both the forced mode buttons and expression detection publish
                // this event; no expression or bound change is required here.
                EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(mode));
                var xAxis = mode == CalculationsMode.Complex ? "Re(z)" : "x";
                var yAxis = mode == CalculationsMode.Complex ? "Im(z)" : "y";
                NUnit.Framework.Assert.That(view.Object.XAxisLabel, Is.EqualTo(xAxis));
                NUnit.Framework.Assert.That(view.Object.YAxisLabel, Is.EqualTo(yAxis));
                NUnit.Framework.Assert.That(new[] { view.Object.XMin, view.Object.XMax,
                    view.Object.YMin, view.Object.YMax }, Is.EqualTo(new[] { 7d, 2d, 9d, -4d }));
                NUnit.Framework.Assert.That(errors[nameof(view.Object.XMin)],
                    Is.EqualTo($"{xAxis} min {Strings.NeedsToBeLessThan} {xAxis} max"));
                NUnit.Framework.Assert.That(errors[nameof(view.Object.XMax)],
                    Is.EqualTo($"{xAxis} max {Strings.NeedsToBeGreaterThan} {xAxis} min"));
                NUnit.Framework.Assert.That(errors[nameof(view.Object.YMin)],
                    Is.EqualTo($"{yAxis} min {Strings.NeedsToBeLessThan} {yAxis} max"));
                NUnit.Framework.Assert.That(errors[nameof(view.Object.YMax)],
                    Is.EqualTo($"{yAxis} max {Strings.NeedsToBeGreaterThan} {yAxis} min"));
            }

            view.Object.XMin = -7;
            view.Object.YMin = -9;
            view.Raise(v => v.YMinChanged += null, EventArgs.Empty);
            NUnit.Framework.Assert.That(errors.Values, Is.All.Null);
        }
    }
}
