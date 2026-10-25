using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Bootstrapping;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Bootstrapping;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Controls.CodeEditors;
using Computator.NET.Desktop.Controls.CodeEditors.AvalonEdit;
using Computator.NET.Desktop.Views;
using NUnit.Framework;
using Settings = Computator.NET.Core.Properties.Settings;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class DocumentCloseWorkflowTests
    {
        private CoreBootstrapper _services;
        private Form _window;
        private PromptedEditor _editor;
        private ScintillaCodeEditorControl _scintilla;
        private AvalonEditCodeEditor _avalon;
        private TextEditorCodeEditor _textEditor;
        private string _directory;
        private CodeEditorType _oldEditor;
        private CodeEditorType _kind;
        private DocumentsTabControl Tabs => Descendants(_editor).OfType<DocumentsTabControl>().Single();

        [SetUp]
        public void SetUp()
        {
            GSLInitializer.Initialize();
            _oldEditor = Settings.Default.CodeEditor;
            _directory = Path.Combine(Path.GetTempPath(), "Computator-DocumentClose-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
        }

        [TearDown]
        public void TearDown()
        {
            _window?.Dispose();
            _services?.Container.Dispose();
            Settings.Default.CodeEditor = _oldEditor;
            Directory.Delete(_directory, true);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void EditingUndoRedoAndSaveUpdateTheActualTabIndicator(CodeEditorType kind)
        {
            OpenEditor(kind);
            var path = OpenFile("saved.tsl", "write(1);");
            NUnit.Framework.Assert.That(Tabs.SelectedTab.ImageIndex, Is.Zero);
            InsertText("\nwrite(2);");
            NUnit.Framework.Assert.That(Tabs.SelectedTab.ImageIndex, Is.EqualTo(1));
            _editor.Undo();
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("write(1);"));
            NUnit.Framework.Assert.That(Tabs.SelectedTab.ImageIndex, Is.Zero);
            _editor.Redo();
            NUnit.Framework.Assert.That(Tabs.SelectedTab.ImageIndex, Is.EqualTo(1));
            _editor.Save();
            NUnit.Framework.Assert.That(Tabs.SelectedTab.ImageIndex, Is.Zero);
            NUnit.Framework.Assert.That(File.ReadAllText(path), Is.EqualTo(_editor.Text));
            _editor.CloseDocument(path);
            NUnit.Framework.Assert.That(_editor.PromptedFiles, Is.Empty);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void CancelKeepsAnEditedSavedDocumentAndItsDiskVersion(CodeEditorType kind)
        {
            OpenEditor(kind);
            var path = OpenFile("saved.tsl", "original");
            _editor.Text = "unsaved draft";
            _editor.Decisions.Enqueue(DialogResult.Cancel);
            _editor.CloseDocument(path);
            NUnit.Framework.Assert.That(_editor.ContainsDocument(path), Is.True);
            NUnit.Framework.Assert.That(_editor.CurrentFileName, Is.EqualTo(path));
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("unsaved draft"));
            NUnit.Framework.Assert.That(Tabs.SelectedTab.ImageIndex, Is.EqualTo(1));
            NUnit.Framework.Assert.That(File.ReadAllText(path), Is.EqualTo("original"));
            NUnit.Framework.Assert.That(_editor.PromptedFiles, Is.EqualTo(new[] { path }));
        }

        [TestCase(CodeEditorType.Scintilla, DialogResult.No)]
        [TestCase(CodeEditorType.AvalonEdit, DialogResult.No)]
        [TestCase(CodeEditorType.TextEditor, DialogResult.No)]
        [TestCase(CodeEditorType.Scintilla, DialogResult.Yes)]
        [TestCase(CodeEditorType.AvalonEdit, DialogResult.Yes)]
        [TestCase(CodeEditorType.TextEditor, DialogResult.Yes)]
        public void ExplicitSaveOrDiscardClosesOnlyTheRequestedBuffer(CodeEditorType kind, DialogResult decision)
        {
            OpenEditor(kind);
            var path = OpenFile("saved.tsl", "original");
            _editor.Text = "chosen draft";
            var other = OpenFile("other.tsl", "other disk");
            _editor.Text = "other draft";
            _editor.Decisions.Enqueue(decision);
            _editor.CloseDocument(path);
            NUnit.Framework.Assert.That(_editor.ContainsDocument(path), Is.False);
            NUnit.Framework.Assert.That(_editor.CurrentFileName, Is.EqualTo(other));
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("other draft"));
            NUnit.Framework.Assert.That(Tabs.SelectedTab.ImageIndex, Is.EqualTo(1));
            NUnit.Framework.Assert.That(File.ReadAllText(path), Is.EqualTo(decision == DialogResult.Yes ? "chosen draft" : "original"));
            NUnit.Framework.Assert.That(File.ReadAllText(other), Is.EqualTo("other disk"));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void CancelledSaveAsKeepsTheUntitledDraft(CodeEditorType kind)
        {
            OpenEditor(kind);
            var name = _editor.CurrentFileName;
            _editor.Text = "untitled draft";
            _editor.Decisions.Enqueue(DialogResult.Yes);
            _editor.SavePath = null;
            _editor.CloseDocument(name);
            NUnit.Framework.Assert.That(_editor.ContainsDocument(name), Is.True);
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("untitled draft"));
            NUnit.Framework.Assert.That(Tabs.SelectedTab.Tag, Is.Null);
            NUnit.Framework.Assert.That(Tabs.SelectedTab.ImageIndex, Is.EqualTo(1));
            NUnit.Framework.Assert.That(_editor.SaveRequests, Is.EqualTo(1));
            NUnit.Framework.Assert.That(Directory.GetFiles(_directory), Is.Empty);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void SavingUntitledWhileClosingWritesItBeforeRemovingItsRenamedTab(CodeEditorType kind)
        {
            OpenEditor(kind);
            var name = _editor.CurrentFileName;
            _editor.Text = "saved from close";
            _editor.SavePath = Path.Combine(_directory, "new.tsl");
            _editor.Decisions.Enqueue(DialogResult.Yes);
            _editor.CloseDocument(name);
            NUnit.Framework.Assert.That(_editor.ContainsDocument(name), Is.False);
            NUnit.Framework.Assert.That(_editor.ContainsDocument(_editor.SavePath), Is.False);
            NUnit.Framework.Assert.That(File.ReadAllText(_editor.SavePath), Is.EqualTo("saved from close"));
            NUnit.Framework.Assert.That(_editor.ConfirmCloseDocuments(), Is.True, "The replacement blank tab is clean.");
            NUnit.Framework.Assert.That(_editor.PromptedFiles.Count, Is.EqualTo(1));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void FailedSavePreservesBothTheDraftAndAnExternalFileChange(CodeEditorType kind)
        {
            OpenEditor(kind);
            var path = OpenFile("saved.tsl", "original");
            _editor.Text = "local draft";
            File.WriteAllText(path, "external edit");
            _editor.Decisions.Enqueue(DialogResult.Yes);
            _editor.CloseDocument(path);
            NUnit.Framework.Assert.That(_editor.ContainsDocument(path), Is.True);
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("local draft"));
            NUnit.Framework.Assert.That(Tabs.SelectedTab.ImageIndex, Is.EqualTo(1));
            NUnit.Framework.Assert.That(File.ReadAllText(path), Is.EqualTo("external edit"));
            NUnit.Framework.Assert.That(_editor.SaveErrors, Has.Count.EqualTo(1));
            NUnit.Framework.Assert.That(_editor.SaveErrors[0], Does.Contain("changed outside this editor"));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void SaveAsCollisionDuringCloseKeepsBothDocuments(CodeEditorType kind)
        {
            OpenEditor(kind);
            var untitled = _editor.CurrentFileName;
            _editor.Text = "new draft";
            var target = OpenFile("target.tsl", "target disk");
            _editor.Text = "target draft";
            _editor.SavePath = target;
            _editor.Decisions.Enqueue(DialogResult.Yes);
            _editor.CloseDocument(untitled);
            NUnit.Framework.Assert.That(_editor.ContainsDocument(untitled), Is.True);
            NUnit.Framework.Assert.That(_editor.ContainsDocument(target), Is.True);
            NUnit.Framework.Assert.That(_editor.CurrentFileName, Is.EqualTo(target));
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("target draft"));
            NUnit.Framework.Assert.That(File.ReadAllText(target), Is.EqualTo("target disk"));
            _editor.SwitchTab(untitled);
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("new draft"));
            NUnit.Framework.Assert.That(_editor.SaveErrors, Has.Count.EqualTo(1));
        }

        [TestCase(CodeEditorType.Scintilla, "closeOtherTabsToolStripMenuItem", 0)]
        [TestCase(CodeEditorType.AvalonEdit, "closeOtherTabsToolStripMenuItem", 0)]
        [TestCase(CodeEditorType.TextEditor, "closeOtherTabsToolStripMenuItem", 0)]
        [TestCase(CodeEditorType.Scintilla, "closeTabsToTheLeftToolStripMenuItem", 2)]
        [TestCase(CodeEditorType.AvalonEdit, "closeTabsToTheLeftToolStripMenuItem", 2)]
        [TestCase(CodeEditorType.TextEditor, "closeTabsToTheLeftToolStripMenuItem", 2)]
        [TestCase(CodeEditorType.Scintilla, "closeTabsToTheRightToolStripMenuItem", 0)]
        [TestCase(CodeEditorType.AvalonEdit, "closeTabsToTheRightToolStripMenuItem", 0)]
        [TestCase(CodeEditorType.TextEditor, "closeTabsToTheRightToolStripMenuItem", 0)]
        public void CancellingBulkCloseDoesNotDiscardAnyPreviouslyApprovedBuffer(CodeEditorType kind, string menu, int selected)
        {
            OpenEditor(kind);
            var paths = Enumerable.Range(0, 3).Select(index =>
            {
                var path = OpenFile("file" + index + ".tsl", "disk" + index);
                _editor.Text = "draft" + index;
                return path;
            }).ToArray();
            _editor.SwitchTab(paths[selected]);
            _editor.Decisions.Enqueue(DialogResult.No);
            _editor.Decisions.Enqueue(DialogResult.Cancel);
            var count = Tabs.TabCount;
            ((ToolStripMenuItem)Tabs.ContextMenuStrip.Items[menu]).PerformClick();
            NUnit.Framework.Assert.That(Tabs.TabCount, Is.EqualTo(count));
            NUnit.Framework.Assert.That(_editor.CurrentFileName, Is.EqualTo(paths[selected]));
            for (var i = 0; i < paths.Length; i++)
            {
                _editor.SwitchTab(paths[i]);
                NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("draft" + i));
                NUnit.Framework.Assert.That(File.ReadAllText(paths[i]), Is.EqualTo("disk" + i));
                NUnit.Framework.Assert.That(Tabs.SelectedTab.ImageIndex, Is.EqualTo(1));
            }
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void EditorTransfersKeepDirtyStateAndTheLoadedBaseline(CodeEditorType kind)
        {
            OpenEditor(kind);
            var path = OpenFile("saved.tsl", "original");
            _editor.Text = "draft";
            foreach (var next in new[] { CodeEditorType.Scintilla, CodeEditorType.AvalonEdit, CodeEditorType.TextEditor })
            {
                Settings.Default.CodeEditor = next;
                NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("draft"));
                NUnit.Framework.Assert.That(Tabs.SelectedTab.ImageIndex, Is.EqualTo(1));
            }
            _editor.Text = "original";
            NUnit.Framework.Assert.That(Tabs.SelectedTab.ImageIndex, Is.Zero);
            _editor.CloseDocument(path);
            NUnit.Framework.Assert.That(_editor.PromptedFiles, Is.Empty);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void ClosingAllDocumentsKeepsEveryTabUntilEveryDecisionSucceeds(CodeEditorType kind)
        {
            OpenEditor(kind);
            var first = OpenFile("first.tsl", "first disk");
            _editor.Text = "first draft";
            var second = OpenFile("second.tsl", "second disk");
            _editor.Text = "second draft";
            var count = Tabs.TabCount;
            _editor.Decisions.Enqueue(DialogResult.Yes);
            _editor.Decisions.Enqueue(DialogResult.Cancel);
            NUnit.Framework.Assert.That(Tabs.TryCloseTabs(Tabs.TabPages.Cast<TabPage>()), Is.False);
            NUnit.Framework.Assert.That(Tabs.TabCount, Is.EqualTo(count));
            NUnit.Framework.Assert.That(File.ReadAllText(first), Is.EqualTo("first draft"));
            NUnit.Framework.Assert.That(File.ReadAllText(second), Is.EqualTo("second disk"));
            _editor.SwitchTab(first);
            NUnit.Framework.Assert.That(Tabs.SelectedTab.ImageIndex, Is.Zero);
            _editor.SwitchTab(second);
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("second draft"));
            NUnit.Framework.Assert.That(Tabs.SelectedTab.ImageIndex, Is.EqualTo(1));
            _editor.Decisions.Enqueue(DialogResult.No);
            NUnit.Framework.Assert.That(Tabs.TryCloseTabs(Tabs.TabPages.Cast<TabPage>()), Is.True);
            NUnit.Framework.Assert.That(_editor.ContainsDocument(first), Is.False);
            NUnit.Framework.Assert.That(_editor.ContainsDocument(second), Is.False);
            NUnit.Framework.Assert.That(Tabs.TabCount, Is.EqualTo(2), "One clean replacement tab and the new-tab button.");
            NUnit.Framework.Assert.That(_editor.Text, Is.Empty);
        }

        [TestCase(CloseReason.UserClosing)]
        [TestCase(CloseReason.ApplicationExitCall)]
        public void MainWindowClosingChecksBothEditorsAndRetainsEarlierDiscardedDraftOnCancel(CloseReason reason)
        {
            OpenEditor(CodeEditorType.TextEditor);
            _editor.Text = "script draft";
            var customServices = new CoreBootstrapper();
            using var customContainer = customServices.Container;
            using var customEditor = new PromptedEditor(customServices.Create<ScintillaCodeEditorControl>(),
                customServices.Create<AvalonEditCodeEditor>(), customServices.Create<TextEditorCodeEditor>());
            customEditor.Text = "custom function draft";
            var mainServices = new WinFormsBootstrapper();
            using var mainContainer = mainServices.Container;
            _window.Controls.Remove(_editor);
            _window.Dispose();
            var main = new MainView(mainServices.Create<MenuStripView>(), mainServices.Create<ToolBarView>(),
                mainServices.Create<CalculationsView>(), mainServices.Create<NumericalCalculationsView>(),
                new ScriptingView(_editor, new SolutionExplorerView()),
                new CustomFunctionsView(customEditor, new SolutionExplorerView()),
                mainServices.Create<ChartingView>(), mainServices.Create<ExpressionView>());
            _window = main;
            _ = main.Handle;
            _editor.Decisions.Enqueue(DialogResult.No);
            customEditor.Decisions.Enqueue(DialogResult.Cancel);
            var args = new FormClosingEventArgs(reason, false);
            typeof(MainView).GetMethod("OnFormClosing", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(main, new object[] { args });
            NUnit.Framework.Assert.That(args.Cancel, Is.True);
            NUnit.Framework.Assert.That(main.IsDisposed, Is.False);
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("script draft"));
            NUnit.Framework.Assert.That(customEditor.Text, Is.EqualTo("custom function draft"));
            NUnit.Framework.Assert.That(_editor.PromptedFiles, Has.Count.EqualTo(1));
            NUnit.Framework.Assert.That(customEditor.PromptedFiles, Has.Count.EqualTo(1));
            _editor.Decisions.Enqueue(DialogResult.No);
            customEditor.Decisions.Enqueue(DialogResult.No);
            args = new FormClosingEventArgs(reason, false);
            typeof(MainView).GetMethod("OnFormClosing", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(main, new object[] { args });
            NUnit.Framework.Assert.That(args.Cancel, Is.False);
            main.Dispose();
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void ExitApprovalRejectsATabAddedDuringItsLastPrompt(CodeEditorType kind)
        {
            OpenEditor(kind);
            var original = _editor.CurrentFileName;
            _editor.Text = "approved draft";
            string added = null;
            _editor.Decisions.Enqueue(DialogResult.No);
            _editor.BeforePrompt = () =>
            {
                _editor.NewDocument();
                added = _editor.CurrentFileName;
                _editor.Text = "new unapproved draft";
                _editor.SwitchTab(original);
            };
            NUnit.Framework.Assert.That(_editor.ConfirmCloseDocuments(), Is.False);
            NUnit.Framework.Assert.That(_editor.ContainsDocument(original), Is.True);
            NUnit.Framework.Assert.That(_editor.ContainsDocument(added), Is.True);
            _editor.SwitchTab(added);
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("new unapproved draft"));
        }

        [TestCase("edit")]
        [TestCase("add")]
        [TestCase("rename")]
        public void MainWindowRechecksScriptDocumentsAfterTheCustomFunctionsPrompt(string change)
        {
            OpenEditor(CodeEditorType.TextEditor);
            var original = _editor.CurrentFileName;
            _editor.Text = "approved script draft";
            var customServices = new CoreBootstrapper();
            using var customContainer = customServices.Container;
            using var customEditor = new PromptedEditor(customServices.Create<ScintillaCodeEditorControl>(),
                customServices.Create<AvalonEditCodeEditor>(), customServices.Create<TextEditorCodeEditor>());
            customEditor.Text = "custom function draft";
            var mainServices = new WinFormsBootstrapper();
            using var mainContainer = mainServices.Container;
            _window.Controls.Remove(_editor);
            _window.Dispose();
            var main = new MainView(mainServices.Create<MenuStripView>(), mainServices.Create<ToolBarView>(),
                mainServices.Create<CalculationsView>(), mainServices.Create<NumericalCalculationsView>(),
                new ScriptingView(_editor, new SolutionExplorerView()),
                new CustomFunctionsView(customEditor, new SolutionExplorerView()),
                mainServices.Create<ChartingView>(), mainServices.Create<ExpressionView>());
            _window = main;
            _ = main.Handle;
            _editor.Decisions.Enqueue(DialogResult.No);
            customEditor.Decisions.Enqueue(DialogResult.No);
            customEditor.BeforePrompt = () =>
            {
                if (change == "add") _editor.NewDocument();
                if (change == "rename") _editor.RenameDocument(original, Path.Combine(_directory, "renamed.tsl"));
                else _editor.Text = "late script draft";
            };
            var args = new FormClosingEventArgs(CloseReason.ApplicationExitCall, false);
            typeof(MainView).GetMethod("OnFormClosing", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(main, new object[] { args });
            NUnit.Framework.Assert.That(args.Cancel, Is.True);
            NUnit.Framework.Assert.That(main.IsDisposed, Is.False);
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo(change == "rename" ? "approved script draft" : "late script draft"));
            NUnit.Framework.Assert.That(customEditor.Text, Is.EqualTo("custom function draft"));
            if (change == "add") NUnit.Framework.Assert.That(_editor.ContainsDocument(original), Is.True);
            main.Dispose();
        }

        private void OpenEditor(CodeEditorType kind)
        {
            Settings.Default.CodeEditor = _kind = kind;
            _services = new CoreBootstrapper();
            _scintilla = _services.Create<ScintillaCodeEditorControl>();
            _avalon = _services.Create<AvalonEditCodeEditor>();
            _textEditor = _services.Create<TextEditorCodeEditor>();
            _editor = new PromptedEditor(_scintilla, _avalon, _textEditor);
            _window = new Form { Width = 800, Height = 600 };
            _window.Controls.Add(_editor);
            _window.Show();
            Application.DoEvents();
        }

        private string OpenFile(string name, string contents)
        {
            var path = Path.Combine(_directory, name);
            File.WriteAllText(path, contents);
            _editor.NewDocument(path);
            return path;
        }

        private void InsertText(string text)
        {
            if (_kind == CodeEditorType.Scintilla)
            {
                _scintilla.BeginUndoAction();
                try { _scintilla.AppendText(text); }
                finally { _scintilla.EndUndoAction(); }
            }
            else if (_kind == CodeEditorType.AvalonEdit) _avalon.Document.Insert(_avalon.Document.TextLength, text);
            else _textEditor.Document.Insert(_textEditor.Document.TextLength, text);
            Application.DoEvents();
        }

        private static IEnumerable<Control> Descendants(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;
                foreach (var descendant in Descendants(child)) yield return descendant;
            }
        }

        private sealed class PromptedEditor : CodeEditorControlWrapper
        {
            internal readonly Queue<DialogResult> Decisions = new Queue<DialogResult>();
            internal readonly List<string> PromptedFiles = new List<string>();
            internal readonly List<string> SaveErrors = new List<string>();
            internal string SavePath;
            internal int SaveRequests;
            internal Action BeforePrompt;

            public PromptedEditor(ScintillaCodeEditorControl scintilla, AvalonEditCodeEditor avalon,
                TextEditorCodeEditor textEditor) : base(scintilla, avalon, textEditor) { }

            protected override DialogResult PromptToSaveChanges(string filename)
            {
                PromptedFiles.Add(filename);
                BeforePrompt?.Invoke();
                NUnit.Framework.Assert.That(Decisions, Is.Not.Empty, "Unexpected unsaved-document prompt for " + filename);
                return Decisions.Dequeue();
            }

            protected override string SelectSaveFileName() { SaveRequests++; return SavePath; }
            protected override void ShowCloseSaveError(string message) { SaveErrors.Add(message); }
        }
    }
}
