using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Menu.Commands.FileCommands;
using Computator.NET.Core.Model;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Bootstrapping;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Controls.CodeEditors;
using Computator.NET.Desktop.Views;
using NUnit.Framework;
using Settings = Computator.NET.Core.Properties.Settings;

namespace Computator.NET.Desktop.Tests
{
    // Executes the real File > Open menu command against the two editors
    // hosted by the actual WinForms window. Only the OS file chooser is stubbed.
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class OpenDocumentMenuWorkflowTests
    {
        private sealed class SelectedFileDialog : IOpenFileDialog
        {
            public bool Accepted { get; set; } = true;
            public string FileName { get; set; }
            public string Filter { get; set; }
            public int Shows { get; private set; }
            public bool Show()
            {
                Shows++;
                return Accepted;
            }
        }

        private WinFormsBootstrapper _services;
        private MainView _window;
        private ISharedViewState _state;
        private ICodeDocumentsEditor _scripting;
        private ICodeDocumentsEditor _functions;
        private ToolStripMenuItem _openMenu;
        private SelectedFileDialog _dialog;
        private CodeEditorType _oldEditor;
        private string _oldScriptingDirectory;
        private string _oldFunctionsDirectory;
        private string _directory;

        [OneTimeSetUp]
        public void InitializeGsl() => GSLInitializer.Initialize();

        [SetUp]
        public void CreateRealDesktop()
        {
            _oldEditor = Settings.Default.CodeEditor;
            _oldScriptingDirectory = Settings.Default.ScriptingDirectory;
            _oldFunctionsDirectory = Settings.Default.CustomFunctionsDirectory;
            _directory = Path.Combine(Path.GetTempPath(), "Computator-MenuOpen-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
            Settings.Default.ScriptingDirectory = _directory;
            Settings.Default.CustomFunctionsDirectory = _directory;
            _services = new WinFormsBootstrapper();
            _dialog = new SelectedFileDialog();
            _services.RegisterInstance<IOpenFileDialog>(_dialog);
            _window = _services.Create<MainView>();
            _window.Show();

            _state = _services.Create<ISharedViewState>();
            var scriptView = _services.Create<ScriptingView>();
            var functionsView = _services.Create<CustomFunctionsView>();
            _scripting = scriptView.CodeEditorView;
            _functions = functionsView.CustomFunctionsEditor;
            NUnit.Framework.Assert.That(scriptView.FindForm(), Is.SameAs(_window));
            NUnit.Framework.Assert.That(functionsView.FindForm(), Is.SameAs(_window));

            var menu = _window.Controls.OfType<MenuStripView>().Single().Controls.OfType<MenuStrip>().Single();
            var file = menu.Items.OfType<ToolStripMenuItem>().First();
            _openMenu = file.DropDownItems.OfType<ToolStripMenuItem>()
                .Single(item => item.ShortcutKeyDisplayString == "Ctrl+O");
            NUnit.Framework.Assert.That(_dialog.Filter, Is.Not.Empty,
                "The real menu command must initialize the injected chooser.");
        }

        [TearDown]
        public void CloseRealDesktop()
        {
            _window?.Dispose();
            _services?.Container.Dispose();
            Settings.Default.CodeEditor = _oldEditor;
            Settings.Default.ScriptingDirectory = _oldScriptingDirectory;
            Settings.Default.CustomFunctionsDirectory = _oldFunctionsDirectory;
            if (_directory != null && Directory.Exists(_directory))
                Directory.Delete(_directory, true);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void FileMenuRoutesAllSixViewsAndMixedCaseExtensionsWithoutLosingTabs(CodeEditorType kind)
        {
            Settings.Default.CodeEditor = kind;
            var originalScript = WriteFile("original.tsl", "write(1);");
            var originalFunctions = WriteFile("original.tslf", "static real Original(real x) { return x; }");
            _scripting.NewDocument(originalScript);
            _functions.NewDocument(originalFunctions);
            const string scriptDraft = "unsaved script \u03b1";
            const string functionDraft = "unsaved functions \u03b2";
            _scripting.Text = scriptDraft;
            _functions.Text = functionDraft;

            foreach (ViewName view in Enum.GetValues(typeof(ViewName)))
            {
                foreach (var extension in new[] { ".tsl", ".TSL", ".tslf", ".TsLf" })
                {
                    _scripting.SwitchTab(originalScript);
                    _functions.SwitchTab(originalFunctions);
                    var functionsTarget = extension.Equals(".tslf", StringComparison.OrdinalIgnoreCase);
                    var target = functionsTarget ? _functions : _scripting;
                    var other = functionsTarget ? _scripting : _functions;
                    var originalTarget = functionsTarget ? originalFunctions : originalScript;
                    var originalOther = functionsTarget ? originalScript : originalFunctions;
                    var targetDraft = functionsTarget ? functionDraft : scriptDraft;
                    var otherDraft = functionsTarget ? scriptDraft : functionDraft;
                    var destination = functionsTarget ? ViewName.CustomFunctions : ViewName.Scripting;
                    var beforeTargetTabs = Tabs(target).TabCount;
                    var beforeOtherTabs = Tabs(other).TabCount;
                    var text = "opened " + view + " " + extension;
                    var filename = WriteFile("from-" + view + "-" + beforeTargetTabs + extension, text);

                    _state.CurrentView = view;
                    NUnit.Framework.Assert.That(_window.SelectedViewIndex, Is.EqualTo((int)view),
                        "Could not begin from " + view);
                    _dialog.FileName = filename;
                    _dialog.Accepted = true;
                    _openMenu.PerformClick();

                    NUnit.Framework.Assert.That(_state.CurrentView, Is.EqualTo(destination), view.ToString());
                    NUnit.Framework.Assert.That(_window.SelectedViewIndex, Is.EqualTo((int)destination), view.ToString());
                    NUnit.Framework.Assert.That(target.CurrentFileName, Is.EqualTo(filename), view.ToString());
                    NUnit.Framework.Assert.That(target.Text, Is.EqualTo(text), view.ToString());
                    NUnit.Framework.Assert.That(Tabs(target).TabCount, Is.EqualTo(beforeTargetTabs + 1));
                    NUnit.Framework.Assert.That(Tabs(other).TabCount, Is.EqualTo(beforeOtherTabs));
                    NUnit.Framework.Assert.That(other.CurrentFileName, Is.EqualTo(originalOther));
                    NUnit.Framework.Assert.That(other.Text, Is.EqualTo(otherDraft));
                    target.SwitchTab(originalTarget);
                    NUnit.Framework.Assert.That(target.Text, Is.EqualTo(targetDraft),
                        "Opening another file must not discard unsaved content.");
                    target.SwitchTab(filename);
                    NUnit.Framework.Assert.That(target.Text, Is.EqualTo(text),
                        "Navigation must restore the new document without reloading it.");
                    NUnit.Framework.Assert.That(File.ReadAllText(originalScript), Is.EqualTo("write(1);"));
                    NUnit.Framework.Assert.That(File.ReadAllText(originalFunctions),
                        Is.EqualTo("static real Original(real x) { return x; }"));
                }
            }
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void CancellingMenuOpenPreservesEachViewAndBothUnsavedBuffers(CodeEditorType kind)
        {
            Settings.Default.CodeEditor = kind;
            var script = WriteFile("script.tsl", "saved script");
            var functions = WriteFile("functions.tslf", "saved functions");
            _scripting.NewDocument(script);
            _functions.NewDocument(functions);
            _scripting.Text = "script draft";
            _functions.Text = "functions draft";
            var scriptsCount = Tabs(_scripting).TabCount;
            var functionsCount = Tabs(_functions).TabCount;
            _dialog.Accepted = false;

            foreach (ViewName view in Enum.GetValues(typeof(ViewName)))
            {
                _state.CurrentView = view;
                _openMenu.PerformClick();
                NUnit.Framework.Assert.That(_state.CurrentView, Is.EqualTo(view));
                NUnit.Framework.Assert.That(_window.SelectedViewIndex, Is.EqualTo((int)view));
                NUnit.Framework.Assert.That(_scripting.CurrentFileName, Is.EqualTo(script));
                NUnit.Framework.Assert.That(_functions.CurrentFileName, Is.EqualTo(functions));
                NUnit.Framework.Assert.That(_scripting.Text, Is.EqualTo("script draft"));
                NUnit.Framework.Assert.That(_functions.Text, Is.EqualTo("functions draft"));
                NUnit.Framework.Assert.That(Tabs(_scripting).TabCount, Is.EqualTo(scriptsCount));
                NUnit.Framework.Assert.That(Tabs(_functions).TabCount, Is.EqualTo(functionsCount));
            }
            NUnit.Framework.Assert.That(_dialog.Shows, Is.EqualTo(6));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void MissingAndLockedDocumentsDoNotSwitchViewsOrDestroyUnsavedTabs(CodeEditorType kind)
        {
            Settings.Default.CodeEditor = kind;
            var previous = WriteFile("previous.tsl", "disk");
            _scripting.NewDocument(previous);
            _scripting.Text = "preserve this draft";
            var originalTabCount = Tabs(_scripting).TabCount;
            var originalFunctionsTabs = Tabs(_functions).TabCount;

            foreach (ViewName view in Enum.GetValues(typeof(ViewName)))
            {
                _state.CurrentView = view;
                _dialog.FileName = Path.Combine(_directory, "missing-" + view + ".tslf");
                _dialog.Accepted = true;
                NUnit.Framework.Assert.Throws<FileNotFoundException>(() => _openMenu.PerformClick());
                NUnit.Framework.Assert.That(_state.CurrentView, Is.EqualTo(view));
                NUnit.Framework.Assert.That(_window.SelectedViewIndex, Is.EqualTo((int)view));
                NUnit.Framework.Assert.That(_scripting.CurrentFileName, Is.EqualTo(previous));
                NUnit.Framework.Assert.That(_scripting.Text, Is.EqualTo("preserve this draft"));
                NUnit.Framework.Assert.That(Tabs(_scripting).TabCount, Is.EqualTo(originalTabCount));
                NUnit.Framework.Assert.That(Tabs(_functions).TabCount, Is.EqualTo(originalFunctionsTabs));
            }

            var locked = WriteFile("locked.TsLf", "valid library text");
            _state.CurrentView = ViewName.Charting;
            using (new FileStream(locked, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                _dialog.FileName = locked;
                NUnit.Framework.Assert.Throws<IOException>(() => _openMenu.PerformClick());
                NUnit.Framework.Assert.That(_state.CurrentView, Is.EqualTo(ViewName.Charting));
                NUnit.Framework.Assert.That(_scripting.CurrentFileName, Is.EqualTo(previous));
                NUnit.Framework.Assert.That(_scripting.Text, Is.EqualTo("preserve this draft"));
                NUnit.Framework.Assert.That(_functions.ContainsDocument(locked), Is.False);
            }
        }

        [TestCase(CodeEditorType.Scintilla, ".tsl", ViewName.Scripting)]
        [TestCase(CodeEditorType.Scintilla, ".tslf", ViewName.CustomFunctions)]
        [TestCase(CodeEditorType.AvalonEdit, ".tsl", ViewName.Scripting)]
        [TestCase(CodeEditorType.AvalonEdit, ".tslf", ViewName.CustomFunctions)]
        [TestCase(CodeEditorType.TextEditor, ".tsl", ViewName.Scripting)]
        [TestCase(CodeEditorType.TextEditor, ".tslf", ViewName.CustomFunctions)]
        public void ReopeningASavedPathSelectsTheExistingUnsavedTabWithoutDuplicatingIt(
            CodeEditorType kind, string extension, ViewName destination)
        {
            Settings.Default.CodeEditor = kind;
            var target = destination == ViewName.Scripting ? _scripting : _functions;
            var filename = WriteFile("already-open" + extension, "original disk text");
            target.NewDocument(filename);
            target.Text = "unsaved contents";
            var before = Tabs(target).TabCount;

            _state.CurrentView = ViewName.Charting;
            _dialog.FileName = filename;
            _openMenu.PerformClick();

            NUnit.Framework.Assert.That(_state.CurrentView, Is.EqualTo(destination));
            NUnit.Framework.Assert.That(target.CurrentFileName, Is.EqualTo(filename));
            NUnit.Framework.Assert.That(target.Text, Is.EqualTo("unsaved contents"));
            NUnit.Framework.Assert.That(Tabs(target).TabCount, Is.EqualTo(before));
            NUnit.Framework.Assert.That(File.ReadAllText(filename), Is.EqualTo("original disk text"));
        }

        private string WriteFile(string name, string text)
        {
            var filename = Path.Combine(_directory, name);
            File.WriteAllText(filename, text);
            return filename;
        }

        private static DocumentsTabControl Tabs(ICodeDocumentsEditor editor) =>
            Descendants((Control)editor).OfType<DocumentsTabControl>().Single();

        private static IEnumerable<Control> Descendants(Control control)
        {
            foreach (Control child in control.Controls)
            {
                yield return child;
                foreach (var nested in Descendants(child))
                    yield return nested;
            }
        }
    }
}
