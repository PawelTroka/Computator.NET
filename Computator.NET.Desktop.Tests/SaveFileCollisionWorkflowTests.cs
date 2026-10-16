using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Bootstrapping;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Controls.CodeEditors;
using Computator.NET.Desktop.Controls.CodeEditors.AvalonEdit;
using NUnit.Framework;
using Settings = Computator.NET.Core.Properties.Settings;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class SaveFileCollisionWorkflowTests
    {
        private CoreBootstrapper _bootstrapper;
        private Form _window;
        private DialoglessEditor _editor;
        private string _directory;
        private CodeEditorType _previousEditor;

        [SetUp]
        public void SetUp()
        {
            GSLInitializer.Initialize();
            _previousEditor = Settings.Default.CodeEditor;
            _directory = Path.Combine(Path.GetTempPath(), "Computator-SaveCollision-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
        }

        [TearDown]
        public void TearDown()
        {
            _window?.Dispose();
            _bootstrapper?.Container.Dispose();
            Settings.Default.CodeEditor = _previousEditor;
            if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void SaveAsRejectsAnotherOpenFileBeforeChangingDiskOrEitherBuffer(CodeEditorType kind)
        {
            OpenEditor(kind);
            var source = OpenFile("source.tsl", "source disk content", "source unsaved");
            var target = OpenFile("target.tsl", "target disk content", "target unsaved");
            _editor.SwitchTab(source);
            _editor.NextSaveFileName = Path.Combine(_directory, ".", "TARGET.TSL");
            var tabCount = Tabs.TabPages.Count;

            NUnit.Framework.Assert.Throws<IOException>(() => _editor.SaveAs());

            AssertCurrent(source, "source unsaved");
            NUnit.Framework.Assert.That(File.ReadAllText(source), Is.EqualTo("source disk content"));
            NUnit.Framework.Assert.That(File.ReadAllText(target), Is.EqualTo("target disk content"));
            NUnit.Framework.Assert.That(Tabs.TabPages.Count, Is.EqualTo(tabCount));
            _editor.SwitchTab(target);
            AssertCurrent(target, "target unsaved");
            _editor.SwitchTab(source);
            AssertCurrent(source, "source unsaved");
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void SaveRejectsAnotherOpenAliasWithoutShowingTheFilenameDialog(CodeEditorType kind)
        {
            OpenEditor(kind);
            var source = OpenFile("source.tsl", "original disk content", "source unsaved");
            var alias = Path.Combine(_directory, ".", "source.tsl");
            _editor.NewDocument(alias);
            _editor.Text = "alias unsaved";
            _editor.SwitchTab(source);
            var tabCount = Tabs.TabPages.Count;

            NUnit.Framework.Assert.Throws<IOException>(() => _editor.Save());

            NUnit.Framework.Assert.That(_editor.SaveDialogRequests, Is.Zero);
            AssertCurrent(source, "source unsaved");
            NUnit.Framework.Assert.That(File.ReadAllText(source), Is.EqualTo("original disk content"));
            NUnit.Framework.Assert.That(Tabs.TabPages.Count, Is.EqualTo(tabCount));
            _editor.SwitchTab(alias);
            AssertCurrent(alias, "alias unsaved");
            _editor.SwitchTab(source);
            AssertCurrent(source, "source unsaved");
        }

        [TestCase(CodeEditorType.Scintilla, false)]
        [TestCase(CodeEditorType.AvalonEdit, false)]
        [TestCase(CodeEditorType.TextEditor, false)]
        [TestCase(CodeEditorType.Scintilla, true)]
        [TestCase(CodeEditorType.AvalonEdit, true)]
        [TestCase(CodeEditorType.TextEditor, true)]
        public void SaveDialogFallbackAlsoRejectsAnOpenDestination(CodeEditorType kind, bool deletedFile)
        {
            OpenEditor(kind);
            var source = _editor.CurrentFileName;
            if (deletedFile)
            {
                source = OpenFile("deleted.tsl", "old source disk content", "source unsaved");
                File.Delete(source);
            }
            else
                _editor.Text = "source unsaved";
            var target = OpenFile("target.tsl", "target disk content", "target unsaved");
            _editor.SwitchTab(source);
            _editor.NextSaveFileName = Path.Combine(_directory, ".", "TARGET.TSL");
            var tabCount = Tabs.TabPages.Count;

            // Both a first Save and a file deleted externally use the dialog path.
            NUnit.Framework.Assert.Throws<IOException>(() => _editor.Save());

            NUnit.Framework.Assert.That(_editor.SaveDialogRequests, Is.EqualTo(1));
            AssertCurrent(source, "source unsaved");
            NUnit.Framework.Assert.That(File.Exists(source), Is.False);
            NUnit.Framework.Assert.That(File.ReadAllText(target), Is.EqualTo("target disk content"));
            NUnit.Framework.Assert.That(Tabs.TabPages.Count, Is.EqualTo(tabCount));
            _editor.SwitchTab(target);
            AssertCurrent(target, "target unsaved");
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void SaveAsAllowsTheCurrentFileThroughACaseAndDotAlias(CodeEditorType kind)
        {
            OpenEditor(kind);
            var source = OpenFile("self.tsl", "old disk content", "var α = π + 2·3;");
            var alias = Path.Combine(_directory, ".", "SELF.TSL");
            _editor.NextSaveFileName = alias;
            var tabCount = Tabs.TabPages.Count;

            _editor.SaveAs();

            AssertCurrent(Path.GetFullPath(alias), "var α = π + 2·3;");
            NUnit.Framework.Assert.That(File.ReadAllText(source), Is.EqualTo("var α = π + 2·3;"));
            NUnit.Framework.Assert.That(Tabs.TabPages.Count, Is.EqualTo(tabCount));
            NUnit.Framework.Assert.That(Tabs.SelectedTab.ImageIndex, Is.EqualTo(0));

            // A normal subsequent Save must bypass filename selection entirely.
            _editor.NextSaveFileName = Path.Combine(_directory, "must-not-be-created.tsl");
            _editor.Text = "subsequent saved content";
            _editor.Save();
            NUnit.Framework.Assert.That(_editor.SaveDialogRequests, Is.EqualTo(1));
            NUnit.Framework.Assert.That(File.ReadAllText(source), Is.EqualTo("subsequent saved content"));
            NUnit.Framework.Assert.That(File.Exists(_editor.NextSaveFileName), Is.False);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void SaveAsToAnUnusedPathStillWritesAndRenamesTheCurrentDocument(CodeEditorType kind)
        {
            OpenEditor(kind);
            var source = OpenFile("source.tsl", "original disk content", "copy with π");
            var target = Path.Combine(_directory, "copy.tsl");
            _editor.NextSaveFileName = Path.Combine(_directory, ".", "copy.tsl");
            var tabCount = Tabs.TabPages.Count;

            _editor.SaveAs();

            AssertCurrent(target, "copy with π");
            NUnit.Framework.Assert.That(File.ReadAllText(source), Is.EqualTo("original disk content"));
            NUnit.Framework.Assert.That(File.ReadAllText(target), Is.EqualTo("copy with π"));
            NUnit.Framework.Assert.That(_editor.ContainsDocument(source), Is.False);
            NUnit.Framework.Assert.That(Tabs.TabPages.Count, Is.EqualTo(tabCount));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void CancelledSaveAndSaveAsPreserveBuffersAndAnInitialSaveCanStillSucceed(CodeEditorType kind)
        {
            OpenEditor(kind);
            var untitled = _editor.CurrentFileName;
            _editor.Text = "initial draft";
            _editor.NextSaveFileName = null;

            _editor.Save();

            AssertCurrent(untitled, "initial draft");
            NUnit.Framework.Assert.That(Directory.GetFiles(_directory), Is.Empty);
            var source = OpenFile("source.tsl", "source disk content", "source unsaved");

            _editor.SaveAs();

            AssertCurrent(source, "source unsaved");
            NUnit.Framework.Assert.That(File.ReadAllText(source), Is.EqualTo("source disk content"));
            NUnit.Framework.Assert.That(_editor.SaveDialogRequests, Is.EqualTo(2));
            _editor.SwitchTab(untitled);
            AssertCurrent(untitled, "initial draft");
            var target = Path.Combine(_directory, "initial.tsl");
            _editor.NextSaveFileName = target;
            _editor.Save();
            AssertCurrent(target, "initial draft");
            NUnit.Framework.Assert.That(File.ReadAllText(target), Is.EqualTo("initial draft"));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void AFailedWriteKeepsTheUntitledIdentityAndContents(CodeEditorType kind)
        {
            OpenEditor(kind);
            var untitled = _editor.CurrentFileName;
            _editor.Text = "recoverable unsaved content";
            _editor.NextSaveFileName = Path.Combine(_directory, "does-not-exist", "target.tsl");

            NUnit.Framework.Assert.Throws<DirectoryNotFoundException>(() => _editor.Save());

            AssertCurrent(untitled, "recoverable unsaved content");
            NUnit.Framework.Assert.That(Tabs.SelectedTab.ImageIndex, Is.EqualTo(1));
            NUnit.Framework.Assert.That(Directory.GetFiles(_directory), Is.Empty);
        }

        private void OpenEditor(CodeEditorType kind)
        {
            Settings.Default.CodeEditor = kind;
            _bootstrapper = new CoreBootstrapper();
            _editor = new DialoglessEditor(
                _bootstrapper.Create<ScintillaCodeEditorControl>(),
                _bootstrapper.Create<AvalonEditCodeEditor>(),
                _bootstrapper.Create<TextEditorCodeEditor>());
            _window = new Form { Width = 800, Height = 600 };
            _editor.Dock = DockStyle.Fill;
            _window.Controls.Add(_editor);
            _window.Show();
            Application.DoEvents();
        }

        private string OpenFile(string name, string diskContent, string bufferContent)
        {
            var path = Path.Combine(_directory, name);
            File.WriteAllText(path, diskContent);
            _editor.NewDocument(path);
            _editor.Text = bufferContent;
            return path;
        }

        private DocumentsTabControl Tabs => Descendants(_editor).OfType<DocumentsTabControl>().Single();

        private void AssertCurrent(string filename, string contents)
        {
            NUnit.Framework.Assert.That(_editor.CurrentFileName, Is.EqualTo(filename));
            NUnit.Framework.Assert.That(Tabs.SelectedTab.Name, Is.EqualTo(filename));
            NUnit.Framework.Assert.That(Tabs.SelectedTab.Text, Is.EqualTo(filename));
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo(contents));
            NUnit.Framework.Assert.That(_editor.ContainsDocument(filename), Is.True);
        }

        private static IEnumerable<Control> Descendants(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;
                foreach (var descendant in Descendants(child)) yield return descendant;
            }
        }

        // Only modal filename selection is replaced. The real three editors,
        // tab events, production save paths and filesystem operations all run.
        private sealed class DialoglessEditor : CodeEditorControlWrapper
        {
            internal string NextSaveFileName;
            internal int SaveDialogRequests;

            public DialoglessEditor(ScintillaCodeEditorControl scintilla, AvalonEditCodeEditor avalon,
                TextEditorCodeEditor textEditor) : base(scintilla, avalon, textEditor) { }

            protected override string SelectSaveFileName()
            {
                SaveDialogRequests++;
                return NextSaveFileName;
            }
        }
    }
}
