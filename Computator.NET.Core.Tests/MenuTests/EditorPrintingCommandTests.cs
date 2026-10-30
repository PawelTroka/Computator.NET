using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Services;
using Computator.NET.Core.Menu.Commands;
using Computator.NET.Core.Menu.Commands.FileCommands;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
using Computator.NET.DataTypes.Events;
using Moq;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.MenuTests
{
    [TestFixture]
    public class EditorPrintingCommandTests
    {
        private Mock<ICanFileEdit> _scripts;
        private Mock<ICanFileEdit> _functions;
        private Mock<ISharedViewState> _state;
        private Mock<IApplicationManager> _application;
        private Mock<IChart2D> _real;
        private Mock<IComplexChart> _complex;
        private Mock<IChart3D> _surface;

        [SetUp]
        public void SetUp()
        {
            _scripts = new Mock<ICanFileEdit>(MockBehavior.Strict);
            _functions = new Mock<ICanFileEdit>(MockBehavior.Strict);
            _state = new Mock<ISharedViewState>(MockBehavior.Strict);
            _state.SetupProperty(state => state.CurrentView, ViewName.Scripting);
            _state.SetupProperty(state => state.CalculationsMode, CalculationsMode.Real);
            _application = new Mock<IApplicationManager>(MockBehavior.Strict);
            _real = new Mock<IChart2D>(MockBehavior.Strict);
            _complex = new Mock<IComplexChart>(MockBehavior.Strict);
            _surface = new Mock<IChart3D>(MockBehavior.Strict);
        }

        [TestCase(ViewName.Scripting, false)]
        [TestCase(ViewName.Scripting, true)]
        [TestCase(ViewName.CustomFunctions, false)]
        [TestCase(ViewName.CustomFunctions, true)]
        public void FileCommandPrintsOnlyTheCurrentEditorsBuffer(ViewName view, bool preview)
        {
            _state.Object.CurrentView = view;
            var selected = view == ViewName.Scripting ? _scripts : _functions;
            var other = view == ViewName.Scripting ? _functions : _scripts;
            Allow(selected, preview);

            Create(preview).Execute();

            Verify(selected, preview);
            other.VerifyNoOtherCalls();
            VerifyNoChartOrFallbackCalls();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ReusedCommandFollowsWorkspaceChanges(bool preview)
        {
            Allow(_scripts, preview);
            Allow(_functions, preview);
            var command = Create(preview);
            foreach (var view in new[] { ViewName.CustomFunctions, ViewName.Scripting, ViewName.CustomFunctions })
            {
                _state.Object.CurrentView = view;
                command.Execute();
                Verify(view == ViewName.Scripting ? _scripts : _functions, preview);
                (view == ViewName.Scripting ? _functions : _scripts).VerifyNoOtherCalls();
                _scripts.Invocations.Clear();
                _functions.Invocations.Clear();
            }
            VerifyNoChartOrFallbackCalls();
        }

        [TestCase(CalculationsMode.Real, false)]
        [TestCase(CalculationsMode.Real, true)]
        [TestCase(CalculationsMode.Complex, false)]
        [TestCase(CalculationsMode.Complex, true)]
        [TestCase(CalculationsMode.Fxy, false)]
        [TestCase(CalculationsMode.Fxy, true)]
        public void ExistingChartRoutingRemainsIntact(CalculationsMode mode, bool preview)
        {
            _state.Object.CurrentView = ViewName.Charting;
            _state.Object.CalculationsMode = mode;
            if (mode == CalculationsMode.Real) Allow(_real, preview);
            if (mode == CalculationsMode.Complex) Allow(_complex, preview);
            if (mode == CalculationsMode.Fxy) Allow(_surface, preview);

            Create(preview).Execute();

            if (mode == CalculationsMode.Real) Verify(_real, preview);
            if (mode == CalculationsMode.Complex) Verify(_complex, preview);
            if (mode == CalculationsMode.Fxy) Verify(_surface, preview);
            _scripts.VerifyNoOtherCalls();
            _functions.VerifyNoOtherCalls();
            _application.VerifyNoOtherCalls();
        }

        [TestCase(ViewName.Calculations, false)]
        [TestCase(ViewName.Calculations, true)]
        [TestCase(ViewName.NumericalCalculations, false)]
        [TestCase(ViewName.NumericalCalculations, true)]
        [TestCase(ViewName.SymbolicCalculations, false)]
        [TestCase(ViewName.SymbolicCalculations, true)]
        public void OtherViewsKeepTheirExistingKeyboardFallback(ViewName view, bool preview)
        {
            _state.Object.CurrentView = view;
            _application.Setup(application => application.SendStringAsKey("^P"));
            Create(preview).Execute();
            _application.Verify(application => application.SendStringAsKey("^P"), Times.Once());
            _application.VerifyNoOtherCalls();
            _scripts.VerifyNoOtherCalls();
            _functions.VerifyNoOtherCalls();
            _real.VerifyNoOtherCalls();
            _complex.VerifyNoOtherCalls();
            _surface.VerifyNoOtherCalls();
        }

        private CommandBase Create(bool preview) => preview
            ? (CommandBase)new PrintPreviewCommand(_scripts.Object, _functions.Object, _state.Object,
                _application.Object, _real.Object, _complex.Object, _surface.Object)
            : new PrintCommand(_scripts.Object, _functions.Object, _state.Object,
                _application.Object, _real.Object, _complex.Object, _surface.Object);

        private static void Allow<T>(Mock<T> target, bool preview) where T : class, IPrinting
        {
            if (preview) target.Setup(value => value.PrintPreview());
            else target.Setup(value => value.Print());
        }

        private static void Verify<T>(Mock<T> target, bool preview) where T : class, IPrinting
        {
            target.Verify(value => value.PrintPreview(), preview ? Times.Once() : Times.Never());
            target.Verify(value => value.Print(), preview ? Times.Never() : Times.Once());
            target.VerifyNoOtherCalls();
        }

        private void VerifyNoChartOrFallbackCalls()
        {
            _real.VerifyNoOtherCalls();
            _complex.VerifyNoOtherCalls();
            _surface.VerifyNoOtherCalls();
            _application.VerifyNoOtherCalls();
        }
    }
}
