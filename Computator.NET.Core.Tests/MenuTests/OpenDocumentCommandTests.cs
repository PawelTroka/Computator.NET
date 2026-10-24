using System;
using System.Collections.Generic;
using System.IO;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Menu.Commands.FileCommands;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.Configuration;
using Computator.NET.DataTypes.Events;
using Moq;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.MenuTests
{
    [TestFixture]
    public class OpenDocumentCommandTests
    {
        private static IEnumerable<TestCaseData> AcceptedDocuments()
        {
            foreach (ViewName view in Enum.GetValues(typeof(ViewName)))
            {
                yield return new TestCaseData(view, "scripts/źródło with spaces.tsl", ViewName.Scripting);
                yield return new TestCaseData(view, "scripts/example.v2.TSL", ViewName.Scripting);
                yield return new TestCaseData(view, "functions/źródło with spaces.tslf", ViewName.CustomFunctions);
                yield return new TestCaseData(view, "functions/example.v2.TsLf", ViewName.CustomFunctions);
            }
        }

        private static IEnumerable<TestCaseData> FailedDocuments()
        {
            foreach (ViewName view in Enum.GetValues(typeof(ViewName)))
            {
                yield return new TestCaseData(view, "unreadable.tsl", ViewName.Scripting);
                yield return new TestCaseData(view, "unreadable.tslf", ViewName.CustomFunctions);
            }
        }

        [TestCaseSource(nameof(AcceptedDocuments))]
        public void AcceptedFileOpensInItsOwnEditorBeforeActivatingThatView(
            ViewName previousView, string filename, ViewName expectedView)
        {
            var state = State(previousView);
            var scripting = new Mock<ICanFileEdit>(MockBehavior.Strict);
            var customFunctions = new Mock<ICanFileEdit>(MockBehavior.Strict);
            var target = expectedView == ViewName.Scripting ? scripting : customFunctions;
            var other = expectedView == ViewName.Scripting ? customFunctions : scripting;
            target.Setup(editor => editor.NewDocument(filename)).Callback(() =>
                NUnit.Framework.Assert.That(state.Object.CurrentView, Is.EqualTo(previousView),
                    "Changing tools before the file is opened would hide the previous work if opening fails."));
            var dialog = Dialog(filename, accepted: true);
            var command = new OpenCommand(scripting.Object, customFunctions.Object, state.Object, dialog.Object);

            command.Execute();

            NUnit.Framework.Assert.That(state.Object.CurrentView, Is.EqualTo(expectedView));
            target.Verify(editor => editor.NewDocument(filename), Times.Once());
            target.VerifyNoOtherCalls();
            other.VerifyNoOtherCalls();
            dialog.Verify(value => value.Show(), Times.Once());
            dialog.VerifyGet(value => value.FileName, Times.Once());
            NUnit.Framework.Assert.That(dialog.Object.Filter, Is.EqualTo(HandledFilesInformation.TslFilesFIlter));
        }

        [TestCase(ViewName.Charting)]
        [TestCase(ViewName.Calculations)]
        [TestCase(ViewName.NumericalCalculations)]
        [TestCase(ViewName.SymbolicCalculations)]
        [TestCase(ViewName.Scripting)]
        [TestCase(ViewName.CustomFunctions)]
        public void CancellingDoesNotReadAFileOrChangeTheCurrentView(ViewName previousView)
        {
            var state = State(previousView);
            var scripting = new Mock<ICanFileEdit>(MockBehavior.Strict);
            var customFunctions = new Mock<ICanFileEdit>(MockBehavior.Strict);
            var dialog = Dialog(null, accepted: false);
            var command = new OpenCommand(scripting.Object, customFunctions.Object, state.Object, dialog.Object);

            command.Execute();

            NUnit.Framework.Assert.That(state.Object.CurrentView, Is.EqualTo(previousView));
            scripting.VerifyNoOtherCalls();
            customFunctions.VerifyNoOtherCalls();
            dialog.VerifyGet(value => value.FileName, Times.Never());
        }

        [TestCaseSource(nameof(FailedDocuments))]
        public void FailedOpenLeavesThePreviousViewActive(
            ViewName previousView, string filename, ViewName expectedView)
        {
            var state = State(previousView);
            var scripting = new Mock<ICanFileEdit>(MockBehavior.Strict);
            var customFunctions = new Mock<ICanFileEdit>(MockBehavior.Strict);
            var target = expectedView == ViewName.Scripting ? scripting : customFunctions;
            var other = expectedView == ViewName.Scripting ? customFunctions : scripting;
            var failure = new IOException("The selected document could not be read.");
            target.Setup(editor => editor.NewDocument(filename)).Throws(failure);
            var dialog = Dialog(filename, accepted: true);
            var command = new OpenCommand(scripting.Object, customFunctions.Object, state.Object, dialog.Object);

            var actual = NUnit.Framework.Assert.Throws<IOException>(() => command.Execute());
            NUnit.Framework.Assert.That(actual, Is.SameAs(failure));

            NUnit.Framework.Assert.That(state.Object.CurrentView, Is.EqualTo(previousView));
            target.Verify(editor => editor.NewDocument(filename), Times.Once());
            target.VerifyNoOtherCalls();
            other.VerifyNoOtherCalls();
        }

        [Test]
        public void ReusedCommandRoutesEachNewSelectionIndependentlyOfThePreviousTool()
        {
            var state = State(ViewName.Charting);
            var scripting = new Mock<ICanFileEdit>(MockBehavior.Strict);
            var customFunctions = new Mock<ICanFileEdit>(MockBehavior.Strict);
            scripting.Setup(editor => editor.NewDocument("plot.tsl"));
            customFunctions.Setup(editor => editor.NewDocument("helpers.tslf"));
            var dialog = Dialog(null, accepted: true);
            dialog.SetupSequence(value => value.FileName)
                .Returns("helpers.tslf").Returns("plot.tsl").Returns("helpers.tslf");
            var command = new OpenCommand(scripting.Object, customFunctions.Object, state.Object, dialog.Object);

            command.Execute();
            NUnit.Framework.Assert.That(state.Object.CurrentView, Is.EqualTo(ViewName.CustomFunctions));
            command.Execute();
            NUnit.Framework.Assert.That(state.Object.CurrentView, Is.EqualTo(ViewName.Scripting));
            command.Execute();
            NUnit.Framework.Assert.That(state.Object.CurrentView, Is.EqualTo(ViewName.CustomFunctions));

            scripting.Verify(editor => editor.NewDocument("plot.tsl"), Times.Once());
            customFunctions.Verify(editor => editor.NewDocument("helpers.tslf"), Times.Exactly(2));
            scripting.VerifyNoOtherCalls();
            customFunctions.VerifyNoOtherCalls();
        }

        [TestCase("helpers.tslf.bak")]
        [TestCase("script without extension")]
        public void OtherExplicitlyChosenFilesUseTheScriptEditor(string filename)
        {
            var state = State(ViewName.CustomFunctions);
            var scripting = new Mock<ICanFileEdit>(MockBehavior.Strict);
            var customFunctions = new Mock<ICanFileEdit>(MockBehavior.Strict);
            scripting.Setup(editor => editor.NewDocument(filename));
            var dialog = Dialog(filename, accepted: true);
            var command = new OpenCommand(scripting.Object, customFunctions.Object, state.Object, dialog.Object);

            command.Execute();

            NUnit.Framework.Assert.That(state.Object.CurrentView, Is.EqualTo(ViewName.Scripting));
            scripting.Verify(editor => editor.NewDocument(filename), Times.Once());
            scripting.VerifyNoOtherCalls();
            customFunctions.VerifyNoOtherCalls();
        }

        private static Mock<ISharedViewState> State(ViewName view)
        {
            var state = new Mock<ISharedViewState>(MockBehavior.Strict);
            state.SetupProperty(value => value.CurrentView, view);
            return state;
        }

        private static Mock<IOpenFileDialog> Dialog(string filename, bool accepted)
        {
            var dialog = new Mock<IOpenFileDialog>(MockBehavior.Strict);
            dialog.SetupProperty(value => value.Filter);
            dialog.Setup(value => value.Show()).Returns(accepted);
            if (accepted) dialog.SetupGet(value => value.FileName).Returns(filename);
            return dialog;
        }
    }
}
