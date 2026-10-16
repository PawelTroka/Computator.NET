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
        public void ReopeningAnAliasKeepsOneBufferAndOrdinarySaveUsesItsOriginalIdentity(CodeEditorType kind)
        {
            OpenEditor(kind);
            var source = OpenFile("source.tsl", "original disk content", "source unsaved");
            var tabCount = Tabs.TabPages.Count;
            var alias = Path.Combine(_directory, ".", "source.tsl");
            _editor.NewDocument(alias);
            AssertCurrent(source, "source unsaved");
            _editor.Save();

            NUnit.Framework.Assert.That(_editor.SaveDialogRequests, Is.Zero);
            AssertCurrent(source, "source unsaved");
            NUnit.Framework.Assert.That(File.ReadAllText(source), Is.EqualTo("source unsaved"));
            NUnit.Framework.Assert.That(Tabs.TabPages.Count, Is.EqualTo(tabCount));
            _editor.SwitchTab(alias);
            AssertCurrent(source, "source unsaved");
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

        [TestCase(CodeEditorType.Scintilla, false)]
        [TestCase(CodeEditorType.AvalonEdit, false)]
        [TestCase(CodeEditorType.TextEditor, false)]
        [TestCase(CodeEditorType.Scintilla, true)]
        [TestCase(CodeEditorType.AvalonEdit, true)]
        [TestCase(CodeEditorType.TextEditor, true)]
        public void ExternalEditsWithTheSameLengthAndTimestampAreNotOverwritten(CodeEditorType kind, bool saveAs)
        {
            OpenEditor(kind);
            var source = OpenFile("source.tsl", "disk before", "recoverable draft");
            var timestamp = File.GetLastWriteTimeUtc(source);
            File.WriteAllText(source, "disk edited");
            File.SetLastWriteTimeUtc(source, timestamp);
            Settings.Default.CodeEditor = kind == CodeEditorType.Scintilla ? CodeEditorType.AvalonEdit : CodeEditorType.Scintilla;
            Settings.Default.CodeEditor = kind;
            _editor.NextSaveFileName = Path.Combine(_directory, ".", "SOURCE.TSL");

            NUnit.Framework.Assert.Throws<IOException>(() => { if (saveAs) _editor.SaveAs(); else _editor.Save(); });

            AssertCurrent(source, "recoverable draft");
            NUnit.Framework.Assert.That(File.ReadAllText(source), Is.EqualTo("disk edited"));
            // Recovery remains possible by explicitly choosing a new filename.
            var copy = Path.Combine(_directory, "recovered.tsl");
            _editor.NextSaveFileName = copy;
            _editor.SaveAs();
            AssertCurrent(copy, "recoverable draft");
            NUnit.Framework.Assert.That(File.ReadAllText(copy), Is.EqualTo("recoverable draft"));
            NUnit.Framework.Assert.That(File.ReadAllText(source), Is.EqualTo("disk edited"));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void ExternalEditsWhileTheSaveDialogIsOpenKeepBothVersions(CodeEditorType kind)
        {
            OpenEditor(kind);
            var source = OpenFile("source.tsl", "original disk", "unsaved draft");
            _editor.NextSaveFileName = source;
            _editor.DuringSaveDialog = () => File.WriteAllText(source, "external change during dialog");

            NUnit.Framework.Assert.Throws<IOException>(() => _editor.SaveAs());

            AssertCurrent(source, "unsaved draft");
            NUnit.Framework.Assert.That(File.ReadAllText(source), Is.EqualTo("external change during dialog"));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void AFileCreatedExternallyAfterOpeningAMissingPathIsNotOverwritten(CodeEditorType kind)
        {
            OpenEditor(kind);
            var source = Path.Combine(_directory, "created-later.tsl");
            _editor.NewDocument(source);
            _editor.Text = "new draft";
            File.WriteAllText(source, "externally created contents");

            NUnit.Framework.Assert.Throws<IOException>(() => _editor.Save());

            AssertCurrent(source, "new draft");
            NUnit.Framework.Assert.That(File.ReadAllText(source), Is.EqualTo("externally created contents"));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void UntitledLabelsNeverBecomeImplicitSaveDestinations(CodeEditorType kind)
        {
            OpenEditor(kind);
            var untitled = _editor.CurrentFileName;
            var oldDirectory = Environment.CurrentDirectory;
            try
            {
                Environment.CurrentDirectory = _directory;
                File.WriteAllText(untitled, "unrelated existing file");
                _editor.Text = "untitled draft";
                _editor.NextSaveFileName = null;

                _editor.Save();

                AssertCurrent(untitled, "untitled draft");
                NUnit.Framework.Assert.That(_editor.SaveDialogRequests, Is.EqualTo(1));
                NUnit.Framework.Assert.That(File.ReadAllText(untitled), Is.EqualTo("unrelated existing file"));
            }
            finally { Environment.CurrentDirectory = oldDirectory; }
        }

        [TestCase(CodeEditorType.Scintilla, false)]
        [TestCase(CodeEditorType.AvalonEdit, false)]
        [TestCase(CodeEditorType.TextEditor, false)]
        [TestCase(CodeEditorType.Scintilla, true)]
        [TestCase(CodeEditorType.AvalonEdit, true)]
        [TestCase(CodeEditorType.TextEditor, true)]
        public void AnotherEditorViewAlsoOwnsItsOpenFile(CodeEditorType kind, bool saveAs)
        {
            OpenEditor(kind);
            var source = OpenFile("source.tsl", "source disk", "source draft");
            var target = saveAs ? Path.Combine(_directory, "other-view.tsl") : source;
            if (saveAs) File.WriteAllText(target, "other disk");
            var originalTarget = File.ReadAllText(target);
            var otherBootstrapper = new CoreBootstrapper();
            using (otherBootstrapper.Container)
            using (var otherWindow = new Form())
            {
                var other = new DialoglessEditor(otherBootstrapper.Create<ScintillaCodeEditorControl>(),
                    otherBootstrapper.Create<AvalonEditCodeEditor>(), otherBootstrapper.Create<TextEditorCodeEditor>());
                otherWindow.Controls.Add(other);
                otherWindow.Show();
                other.NewDocument(target);
                other.Text = "other view unsaved draft";
                _editor.NextSaveFileName = target;

                NUnit.Framework.Assert.Throws<IOException>(() => { if (saveAs) _editor.SaveAs(); else _editor.Save(); });

                AssertCurrent(source, "source draft");
                NUnit.Framework.Assert.That(other.CurrentFileName, Is.EqualTo(target));
                NUnit.Framework.Assert.That(other.Text, Is.EqualTo("other view unsaved draft"));
                NUnit.Framework.Assert.That(File.ReadAllText(source), Is.EqualTo("source disk"));
                NUnit.Framework.Assert.That(File.ReadAllText(target), Is.EqualTo(originalTarget));
            }
            // Disposed views must neither block later saves nor retain subscriptions.
            _editor.NextSaveFileName = target;
            if (saveAs) _editor.SaveAs(); else _editor.Save();
            NUnit.Framework.Assert.That(File.ReadAllText(target), Is.EqualTo("source draft"));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void ASelectionChangeDuringTheSaveDialogCannotRedirectTheSave(CodeEditorType kind)
        {
            OpenEditor(kind);
            var source = OpenFile("source.tsl", "source disk", "source draft");
            var target = OpenFile("other.tsl", "other disk", "other draft");
            _editor.SwitchTab(source);
            _editor.NextSaveFileName = Path.Combine(_directory, "copy.tsl");
            _editor.DuringSaveDialog = () => _editor.SwitchTab(target);

            NUnit.Framework.Assert.Throws<IOException>(() => _editor.SaveAs());

            AssertCurrent(target, "other draft");
            _editor.SwitchTab(source);
            AssertCurrent(source, "source draft");
            NUnit.Framework.Assert.That(File.ReadAllText(source), Is.EqualTo("source disk"));
            NUnit.Framework.Assert.That(File.ReadAllText(target), Is.EqualTo("other disk"));
            NUnit.Framework.Assert.That(File.Exists(_editor.NextSaveFileName), Is.False);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void ALockedExistingDestinationDoesNotChangeItsBytesOrTheBuffer(CodeEditorType kind)
        {
            OpenEditor(kind);
            var source = OpenFile("source.tsl", "source disk", "source draft");
            using (new FileStream(source, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                NUnit.Framework.Assert.Throws<IOException>(() => _editor.Save());

            AssertCurrent(source, "source draft");
            NUnit.Framework.Assert.That(File.ReadAllText(source), Is.EqualTo("source disk"));
            NUnit.Framework.Assert.That(Directory.GetFiles(_directory).Select(Path.GetFileName), Is.EquivalentTo(new[] { "source.tsl" }));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void SaveAsCanExplicitlyReplaceAClosedFileWithoutChangingTheSource(CodeEditorType kind)
        {
            OpenEditor(kind);
            var source = OpenFile("source.tsl", "source disk", "source draft");
            var target = Path.Combine(_directory, "closed.tsl");
            File.WriteAllText(target, "previous destination bytes");
            _editor.NextSaveFileName = target;

            _editor.SaveAs();

            AssertCurrent(target, "source draft");
            NUnit.Framework.Assert.That(File.ReadAllText(source), Is.EqualTo("source disk"));
            NUnit.Framework.Assert.That(File.ReadAllText(target), Is.EqualTo("source draft"));
            NUnit.Framework.Assert.That(Directory.GetFiles(_directory).Length, Is.EqualTo(2));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void SaveAsToAFreshPathRecoversALockedSourceAndCommitsItsNewIdentity(CodeEditorType kind)
        {
            OpenEditor(kind);
            var source = OpenFile("source.tsl", "source disk", "recoverable draft");
            var target = Path.Combine(_directory, "recovery.tsl");
            _editor.NextSaveFileName = target;
            using (new FileStream(source, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                _editor.SaveAs();
                AssertCurrent(target, "recoverable draft");
                _editor.Text = "updated recovery";
                _editor.Save();
                AssertCurrent(target, "updated recovery");
            }
            NUnit.Framework.Assert.That(File.ReadAllText(source), Is.EqualTo("source disk"));
            NUnit.Framework.Assert.That(File.ReadAllText(target), Is.EqualTo("updated recovery"));
            NUnit.Framework.Assert.That(_editor.SaveDialogRequests, Is.EqualTo(1));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void ALockedBackgroundFileCannotInterruptASuccessfulFileRenameOrSave(CodeEditorType kind)
        {
            OpenEditor(kind);
            var source = OpenFile("source.tsl", "source disk", "source draft");
            var background = OpenFile("background.tsl", "background disk", "background draft");
            var renamed = Path.Combine(_directory, "renamed.tsl");
            _editor.SwitchTab(source);
            using (new FileStream(background, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                _editor.RenameFile(source, renamed);
                AssertCurrent(renamed, "source draft");
                _editor.Save();
            }
            NUnit.Framework.Assert.That(File.Exists(source), Is.False);
            NUnit.Framework.Assert.That(File.ReadAllText(renamed), Is.EqualTo("source draft"));
            NUnit.Framework.Assert.That(File.ReadAllText(background), Is.EqualTo("background disk"));
            _editor.SwitchTab(background);
            AssertCurrent(background, "background draft");
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
            internal Action DuringSaveDialog;

            public DialoglessEditor(ScintillaCodeEditorControl scintilla, AvalonEditCodeEditor avalon,
                TextEditorCodeEditor textEditor) : base(scintilla, avalon, textEditor) { }

            protected override string SelectSaveFileName()
            {
                SaveDialogRequests++;
                DuringSaveDialog?.Invoke();
                return NextSaveFileName;
            }
        }
    }
}
