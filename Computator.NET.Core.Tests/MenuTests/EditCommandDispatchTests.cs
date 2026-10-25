using System.Collections.Generic;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Services;
using Computator.NET.Core.Menu.Commands;
using Computator.NET.Core.Menu.Commands.EditCommands;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.Events;
using Moq;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.MenuTests
{
    [TestFixture]
    public class EditCommandDispatchTests
    {
        private Mock<ICanFileEdit> _scripting;
        private Mock<ICanFileEdit> _customFunctions;
        private Mock<ISharedViewState> _state;
        private Mock<IApplicationManager> _application;
        private List<string> _actions;

        public static IEnumerable<TestCaseData> Routes()
        {
            foreach (var action in new[] { "Undo", "Redo", "SelectAll" })
            {
                // Hidden editors must not receive actions from expression or
                // result controls, even if they report remembered focus.
                foreach (var view in new[] { ViewName.Charting, ViewName.Calculations,
                    ViewName.NumericalCalculations, ViewName.SymbolicCalculations })
                    yield return new TestCaseData(action, view, true, true, "keyboard");

                yield return new TestCaseData(action, ViewName.Scripting, true, false, "scripting");
                yield return new TestCaseData(action, ViewName.Scripting, false, true, "keyboard");
                yield return new TestCaseData(action, ViewName.CustomFunctions, true, true, "custom functions");
                yield return new TestCaseData(action, ViewName.CustomFunctions, true, false, "keyboard");
            }
        }

        [SetUp]
        public void SetUp()
        {
            _actions = new List<string>();
            _scripting = Editor("scripting");
            _customFunctions = Editor("custom functions");
            _state = new Mock<ISharedViewState>();
            _state.SetupProperty(value => value.CurrentView, ViewName.Calculations);
            _application = new Mock<IApplicationManager>(MockBehavior.Strict);
            _application.Setup(value => value.SendStringAsKey(It.IsAny<string>()))
                .Callback<string>(key => _actions.Add("keyboard:" + key));
        }

        [TestCaseSource(nameof(Routes))]
        public void OneCommandDispatchesExactlyOneActionToTheCurrentTarget(
            string action, ViewName view, bool scriptingFocused, bool functionsFocused, string target)
        {
            _state.Object.CurrentView = view;
            _scripting.SetupGet(value => value.Focused).Returns(scriptingFocused);
            _customFunctions.SetupGet(value => value.Focused).Returns(functionsFocused);

            var command = Create(action);
            command.Execute();

            var expected = target == "keyboard" ? "keyboard:" + Key(action) : target + ":" + action;
            NUnit.Framework.Assert.That(_actions, Is.EqualTo(new[] { expected }),
                "A menu action must not perform an editor operation and then send the same shortcut again.");
        }

        [TestCase("Undo")]
        [TestCase("Redo")]
        [TestCase("SelectAll")]
        public void ReusedCommandFollowsViewAndFocusChangesWithoutDuplicateDispatch(string action)
        {
            var command = Create(action);
            _state.Object.CurrentView = ViewName.Scripting;
            _scripting.SetupGet(value => value.Focused).Returns(true);
            command.Execute();

            // The output control now has focus in the same scripting view.
            _scripting.SetupGet(value => value.Focused).Returns(false);
            command.Execute();

            _state.Object.CurrentView = ViewName.CustomFunctions;
            _customFunctions.SetupGet(value => value.Focused).Returns(true);
            command.Execute();

            _state.Object.CurrentView = ViewName.Calculations;
            command.Execute();

            NUnit.Framework.Assert.That(_actions, Is.EqualTo(new[]
            {
                "scripting:" + action, "keyboard:" + Key(action),
                "custom functions:" + action, "keyboard:" + Key(action)
            }));
        }

        private Mock<ICanFileEdit> Editor(string name)
        {
            var editor = new Mock<ICanFileEdit>(MockBehavior.Strict);
            editor.SetupGet(value => value.Focused).Returns(false);
            editor.Setup(value => value.Undo()).Callback(() => _actions.Add(name + ":Undo"));
            editor.Setup(value => value.Redo()).Callback(() => _actions.Add(name + ":Redo"));
            editor.Setup(value => value.SelectAll()).Callback(() => _actions.Add(name + ":SelectAll"));
            return editor;
        }

        private CommandBase Create(string action)
        {
            switch (action)
            {
                case "Undo": return new UndoCommand(_scripting.Object, _customFunctions.Object, _state.Object, _application.Object);
                case "Redo": return new RedoCommand(_scripting.Object, _customFunctions.Object, _state.Object, _application.Object);
                default: return new SelectAllCommand(_scripting.Object, _customFunctions.Object, _state.Object, _application.Object);
            }
        }

        private static string Key(string action) => action == "Undo" ? "^Z" : action == "Redo" ? "^Y" : "^A";
    }
}
