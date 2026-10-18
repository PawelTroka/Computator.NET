using System;
using System.Collections.Generic;
using Computator.NET.Core.Abstract.Views;
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
        [Test]
        public void ModeChangesRenameExistingRangeErrorsWithoutChangingUserValues()
        {
            var view = new Mock<IChartAreaValuesView>();
            view.SetupAllProperties();
            var errors = new Dictionary<string, string>();
            view.Setup(value => value.SetError(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((property, message) => errors[property] = message);
            using var presenter = new ChartAreaValuesPresenter(view.Object);

            view.Object.XMin = 7;
            view.Object.XMax = 7;
            view.Object.YMin = 4;
            view.Object.YMax = -2;
            view.Raise(value => value.XMinChanged += null, EventArgs.Empty);
            view.Raise(value => value.YMaxChanged += null, EventArgs.Empty);

            Publish(CalculationsMode.Complex);
            NUnit.Framework.Assert.That(view.Object.XAxisName, Is.EqualTo("Re(z)"));
            NUnit.Framework.Assert.That(view.Object.YAxisName, Is.EqualTo("Im(z)"));
            NUnit.Framework.Assert.That(errors[nameof(IChartAreaValuesView.XMax)],
                Is.EqualTo($"Re(z) max {Strings.NeedsToBeGreaterThan} Re(z) min"));
            NUnit.Framework.Assert.That(errors[nameof(IChartAreaValuesView.YMin)],
                Is.EqualTo($"Im(z) min {Strings.NeedsToBeLessThan} Im(z) max"));
            view.VerifySet(value => value.AddChartLabel = Strings.DrawChart, Times.Once);

            Publish(CalculationsMode.Error);
            NUnit.Framework.Assert.That(view.Object.XAxisName, Is.EqualTo("Re(z)"),
                "An incomplete expression should retain its last meaningful coordinate names.");

            foreach (var mode in new[] { CalculationsMode.Fxy, CalculationsMode.Complex, CalculationsMode.Real })
                Publish(mode);

            NUnit.Framework.Assert.That(view.Object.XAxisName, Is.EqualTo("x"));
            NUnit.Framework.Assert.That(view.Object.YAxisName, Is.EqualTo("y"));
            NUnit.Framework.Assert.That(errors[nameof(IChartAreaValuesView.XMin)],
                Is.EqualTo($"x min {Strings.NeedsToBeLessThan} x max"));
            NUnit.Framework.Assert.That(errors[nameof(IChartAreaValuesView.YMax)],
                Is.EqualTo($"y max {Strings.NeedsToBeGreaterThan} y min"));
            NUnit.Framework.Assert.That(new[] { view.Object.XMin, view.Object.XMax, view.Object.YMin, view.Object.YMax },
                Is.EqualTo(new double[] { 7, 7, 4, -2 }));

            view.Object.XMin = -7;
            view.Object.YMin = -4;
            view.Raise(value => value.XMinChanged += null, EventArgs.Empty);
            view.Raise(value => value.YMinChanged += null, EventArgs.Empty);
            NUnit.Framework.Assert.That(errors.Values, Is.All.Null,
                "Correcting the ranges must clear the renamed errors as before.");
        }

        private static void Publish(CalculationsMode mode)
            => EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(mode));
    }
}
