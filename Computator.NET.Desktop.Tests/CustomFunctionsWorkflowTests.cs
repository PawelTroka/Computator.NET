using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Menu.Commands.FileCommands;
using Computator.NET.Core.Model;
using Computator.NET.Core.Natives;
using Computator.NET.Core.Services.ErrorHandling;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.DataTypes;
using Computator.NET.Desktop.Bootstrapping;
using Computator.NET.Desktop.Views;
using Moq;
using NUnit.Framework;
using Settings = Computator.NET.Core.Properties.Settings;

namespace Computator.NET.Desktop.Tests
{
    // Exercise the real composition root and the editor actually placed in the
    // window. Evaluator-only tests cannot detect a disconnected hidden editor.
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class CustomFunctionsWorkflowTests
    {
        private WinFormsBootstrapper _bootstrapper;
        private MainView _window;
        private CustomFunctionsView _custom;
        private ScriptingView _scripting;
        private ISharedViewState _state;
        private readonly List<string> _errors = new List<string>();
        private string _directory;
        private string _oldScriptDirectory;
        private string _oldCustomDirectory;
        private CodeEditorType _oldEditor;

        [OneTimeSetUp]
        public void OpenRealDesktop()
        {
            GSLInitializer.Initialize();
            _directory = Path.Combine(Path.GetTempPath(), "Computator-CustomFunctions-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
            _oldScriptDirectory = Settings.Default.ScriptingDirectory;
            _oldCustomDirectory = Settings.Default.CustomFunctionsDirectory;
            _oldEditor = Settings.Default.CodeEditor;
            Settings.Default.ScriptingDirectory = _directory;
            Settings.Default.CustomFunctionsDirectory = _directory;
            _bootstrapper = new WinFormsBootstrapper();
            var errors = new Mock<IErrorHandler>();
            errors.Setup(handler => handler.DisplayError(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((message, title) => _errors.Add(message));
            _bootstrapper.RegisterInstance<IErrorHandler>(errors.Object);
            _window = _bootstrapper.Create<MainView>();
            _ = _window.Handle;
            _custom = _bootstrapper.Create<CustomFunctionsView>();
            _scripting = _bootstrapper.Create<ScriptingView>();
            _state = _bootstrapper.Create<ISharedViewState>();
            NUnit.Framework.Assert.That(_custom.FindForm(), Is.SameAs(_window));
            NUnit.Framework.Assert.That(_scripting.FindForm(), Is.SameAs(_window));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void DisplayedCustomFunctionsFeedCalculationScriptAndFileCommands(CodeEditorType editor)
        {
            Settings.Default.CodeEditor = editor;
            _errors.Clear();
            var path = Path.Combine(_directory, editor + ".tslf");
            File.WriteAllText(path, "static real CustomProbe(real x) { return x + 42; }");
            _custom.CustomFunctionsEditor.NewDocument(path);
            _custom.CustomFunctionsEditor.SwitchTab(path);
            NUnit.Framework.Assert.That(_custom.CustomFunctionsEditor.Text, Does.Contain("CustomProbe"));

            var expression = _bootstrapper.Create<ITextProvider>();
            expression.Text = "CustomProbe(0)";
            _state.DefaultActions[ViewName.Calculations](this, EventArgs.Empty);
            AssertNoErrors();
            NUnit.Framework.Assert.That(Find(_window, "calculationValueTextBox").Text, Is.EqualTo("42"));

            // Unsaved edits, not an old disk snapshot or a second hidden editor.
            _custom.CustomFunctionsEditor.Text = "static real CustomProbe(real x) { return x + 73; }";
            _state.DefaultActions[ViewName.Calculations](this, EventArgs.Empty);
            AssertNoErrors();
            NUnit.Framework.Assert.That(Find(_window, "calculationValueTextBox").Text, Is.EqualTo("73"));
            _scripting.CodeEditorView.Text = "write(CustomProbe(0));";
            _state.DefaultActions[ViewName.Scripting](this, EventArgs.Empty);
            AssertNoErrors();
            NUnit.Framework.Assert.That(Find(_window, "consoleOutputTextBox").Text, Does.EndWith("73"));

            // Resolve the command embedded in the real menu graph, not a freshly
            // constructed command with manually supplied test dependencies.
            _state.CurrentView = ViewName.CustomFunctions;
            var save = _bootstrapper.Create<SaveCommand>();
            var customField = typeof(SaveCommand).GetField("customFunctionsCodeEditor", BindingFlags.NonPublic | BindingFlags.Instance);
            NUnit.Framework.Assert.That(customField.GetValue(save), Is.SameAs(_custom.CustomFunctionsEditor));
            save.Execute();
            NUnit.Framework.Assert.That(File.ReadAllText(path), Is.EqualTo(_custom.CustomFunctionsEditor.Text));

            _custom.CustomFunctionsEditor.RemoveTab(path);
            _custom.CustomFunctionsEditor.NewDocument(path);
            _custom.CustomFunctionsEditor.SwitchTab(path);
            _state.DefaultActions[ViewName.Calculations](this, EventArgs.Empty);
            AssertNoErrors();
            NUnit.Framework.Assert.That(Find(_window, "calculationValueTextBox").Text, Is.EqualTo("73"));
        }

        [Test]
        public void ErrorHighlighterUsesTheDisplayedCustomFunctionsEditor()
        {
            var handler = _bootstrapper.Create<IExceptionsHandler>();
            var field = typeof(ExceptionsHandler).GetField("_customFunctionsEditor", BindingFlags.NonPublic | BindingFlags.Instance);
            NUnit.Framework.Assert.That(field.GetValue(handler), Is.SameAs(_custom.CustomFunctionsEditor));
            NUnit.Framework.Assert.That(_custom.CustomFunctionsEditor, Is.Not.SameAs(_scripting.CodeEditorView));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void VisibleCustomDefinitionsDetermineWhetherASuperscriptIsAnInput(CodeEditorType editor)
        {
            Settings.Default.CodeEditor = editor;
            _errors.Clear();
            _state.CurrentView = ViewName.CustomFunctions;
            _custom.CustomFunctionsEditor.Text = "static real cosᶻ(real value) { return value + 7; }";
            var expression = _bootstrapper.Create<ITextProvider>();
            expression.Text = "cosᶻ(0)";
            _state.CurrentView = ViewName.Calculations;
            NUnit.Framework.Assert.That(_state.CalculationsMode, Is.EqualTo(CalculationsMode.Real));
            _state.DefaultActions[ViewName.Calculations](this, EventArgs.Empty);
            AssertNoErrors();
            NUnit.Framework.Assert.That(Find(_window, "calculationValueTextBox").Text, Is.EqualTo("7"));

            // Removing the exact name exposes the built-in cosine raised to z.
            // Returning to calculation must reroute the unchanged expression.
            _state.CurrentView = ViewName.CustomFunctions;
            _custom.CustomFunctionsEditor.Text = "";
            _state.CurrentView = ViewName.Calculations;
            NUnit.Framework.Assert.That(_state.CalculationsMode, Is.EqualTo(CalculationsMode.Complex));
            _state.DefaultActions[ViewName.Calculations](this, EventArgs.Empty);
            AssertNoErrors();
            NUnit.Framework.Assert.That(Find(_window, "calculationValueTextBox").Text, Is.EqualTo("1"));
        }

        private void AssertNoErrors() => NUnit.Framework.Assert.That(_errors, Is.Empty, string.Join(Environment.NewLine, _errors));
        private static Control Find(Control root, string name) => root.Controls.Find(name, true).Single();

        [OneTimeTearDown]
        public void CloseRealDesktop()
        {
            Settings.Default.CodeEditor = _oldEditor;
            Settings.Default.ScriptingDirectory = _oldScriptDirectory;
            Settings.Default.CustomFunctionsDirectory = _oldCustomDirectory;
            _window?.Dispose();
            _bootstrapper?.Container.Dispose();
            if (_directory != null && Directory.Exists(_directory)) Directory.Delete(_directory, true);
        }
    }
}
