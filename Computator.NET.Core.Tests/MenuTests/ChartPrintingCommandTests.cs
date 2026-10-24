using Computator.NET.Core.Menu.Commands;
using Computator.NET.Core.Menu.Commands.ChartCommands;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
using Moq;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.MenuTests
{
    [TestFixture]
    public class ChartPrintingCommandTests
    {
        private Mock<IChart2D> _realChart;
        private Mock<IComplexChart> _complexChart;
        private Mock<IChart3D> _surfaceChart;
        private Mock<ISharedViewState> _state;

        [SetUp]
        public void SetUp()
        {
            _realChart = new Mock<IChart2D>(MockBehavior.Strict);
            _complexChart = new Mock<IComplexChart>(MockBehavior.Strict);
            _surfaceChart = new Mock<IChart3D>(MockBehavior.Strict);
            _state = new Mock<ISharedViewState>(MockBehavior.Strict);
            _state.SetupProperty(state => state.CalculationsMode, CalculationsMode.Real);
        }

        [TestCase(CalculationsMode.Real)]
        [TestCase(CalculationsMode.Complex)]
        [TestCase(CalculationsMode.Fxy)]
        public void PreviewOpensOnlyTheSelectedChartsPreview(CalculationsMode mode)
        {
            AllowPreviewCalls();
            _state.Object.CalculationsMode = mode;
            var command = CreatePreviewCommand();

            command.Execute();

            VerifyCalls(mode, preview: true);
        }

        [TestCase(CalculationsMode.Real)]
        [TestCase(CalculationsMode.Complex)]
        [TestCase(CalculationsMode.Fxy)]
        public void PrintStillOpensOnlyTheSelectedChartsPrintDialog(CalculationsMode mode)
        {
            AllowPrintCalls();
            _state.Object.CalculationsMode = mode;
            var command = CreatePrintCommand();

            command.Execute();

            VerifyCalls(mode, preview: false);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ExistingCommandFollowsModeChanges(bool preview)
        {
            if (preview)
                AllowPreviewCalls();
            else
                AllowPrintCalls();

            CommandBase command = preview ? CreatePreviewCommand() : CreatePrintCommand();
            var modes = new[]
            {
                CalculationsMode.Complex,
                CalculationsMode.Fxy,
                CalculationsMode.Real,
                CalculationsMode.Complex
            };

            foreach (var mode in modes)
            {
                _state.Object.CalculationsMode = mode;

                command.Execute();

                VerifyCalls(mode, preview);
                _realChart.Invocations.Clear();
                _complexChart.Invocations.Clear();
                _surfaceChart.Invocations.Clear();
            }
        }

        private PrintPreviewChartCommand CreatePreviewCommand()
        {
            return new PrintPreviewChartCommand(_realChart.Object, _complexChart.Object,
                _surfaceChart.Object, _state.Object);
        }

        private PrintChartCommand CreatePrintCommand()
        {
            return new PrintChartCommand(_realChart.Object, _complexChart.Object,
                _surfaceChart.Object, _state.Object);
        }

        private void AllowPreviewCalls()
        {
            _realChart.Setup(chart => chart.PrintPreview());
            _complexChart.Setup(chart => chart.PrintPreview());
            _surfaceChart.Setup(chart => chart.PrintPreview());
        }

        private void AllowPrintCalls()
        {
            _realChart.Setup(chart => chart.Print());
            _complexChart.Setup(chart => chart.Print());
            _surfaceChart.Setup(chart => chart.Print());
        }

        private void VerifyCalls(CalculationsMode selectedMode, bool preview)
        {
            VerifyCalls(_realChart, selectedMode == CalculationsMode.Real, preview);
            VerifyCalls(_complexChart, selectedMode == CalculationsMode.Complex, preview);
            VerifyCalls(_surfaceChart, selectedMode == CalculationsMode.Fxy, preview);
        }

        private static void VerifyCalls<TChart>(Mock<TChart> chart, bool selected, bool preview)
            where TChart : class, IChart
        {
            chart.Verify(value => value.PrintPreview(), selected && preview ? Times.Once() : Times.Never());
            chart.Verify(value => value.Print(), selected && !preview ? Times.Once() : Times.Never());
            chart.VerifyNoOtherCalls();
        }
    }
}
