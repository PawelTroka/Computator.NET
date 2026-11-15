using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Controls.CodeEditors;
#if !__MonoCS__
using Computator.NET.Desktop.Controls.CodeEditors.AvalonEdit;
#endif
using NUnit.Framework;
using Settings = Computator.NET.Core.Properties.Settings;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class DocumentCloneWorkflowTests
    {
        private Form _window;
        private CloneEditor _editor;
        private DocumentsTabControl _tabs;
        private string _directory;
        private CodeEditorType _oldEditor;

        private static IEnumerable<CodeEditorType> EditorKinds =>
#if __MonoCS__
            new[] { CodeEditorType.TextEditor };
#else
            new[] { CodeEditorType.Scintilla, CodeEditorType.AvalonEdit, CodeEditorType.TextEditor };
#endif

        [SetUp]
        public void SetUp()
        {
            _oldEditor = Settings.Default.CodeEditor;
            _directory = Path.Combine(Path.GetTempPath(), "Computator-Clone-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
        }

        [TearDown]
        public void TearDown()
        {
            _window?.Dispose();
            Settings.Default.CodeEditor = _oldEditor;
            if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
        }

        [TestCaseSource(nameof(EditorKinds))]
        public void CloneSavedDraftKeepsBuffersAndSaveDestinationsIndependent(CodeEditorType kind)
        {
            OpenEditor(kind);
            var sourcePath = Path.Combine(_directory, "source.tsl");
            File.WriteAllText(sourcePath, "write(1);");
            _editor.NewDocument(sourcePath);
            _editor.Text = "write(2); // unsaved Żółć 😀";
            var source = _tabs.SelectedTab;
            var sourceText = _editor.Text;
            var count = _tabs.TabCount;

            CloneItem.PerformClick();

            var clone = _tabs.SelectedTab;
            NUnit.Framework.Assert.That(_tabs.TabCount, Is.EqualTo(count + 1));
            NUnit.Framework.Assert.That(clone, Is.Not.SameAs(source));
            NUnit.Framework.Assert.That(clone.Tag, Is.Null, "The clone must not inherit the source file's save identity.");
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo(sourceText));
            NUnit.Framework.Assert.That(clone.ImageIndex, Is.EqualTo(1));
            NUnit.Framework.Assert.That(File.ReadAllText(sourcePath), Is.EqualTo("write(1);"));

            _editor.Text = "write(3); // clone only";
            _editor.CloseDocument(clone.Text);
            NUnit.Framework.Assert.That(_editor.PromptedFiles, Is.EqualTo(new[] { clone.Text }));
            NUnit.Framework.Assert.That(_editor.ContainsDocument(clone.Text), Is.True, "Cancel must preserve the unsaved clone.");
            _editor.SavePath = Path.Combine(_directory, "clone.tsl");
            _editor.Save();
            NUnit.Framework.Assert.That(File.ReadAllText(_editor.SavePath), Is.EqualTo("write(3); // clone only"));
            NUnit.Framework.Assert.That(clone.Tag, Is.EqualTo(_editor.SavePath));
            NUnit.Framework.Assert.That(clone.ImageIndex, Is.Zero);

            _editor.SwitchTab(sourcePath);
            NUnit.Framework.Assert.That(_tabs.SelectedTab, Is.SameAs(source));
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo(sourceText));
            NUnit.Framework.Assert.That(source.ImageIndex, Is.EqualTo(1));
            NUnit.Framework.Assert.That(File.ReadAllText(sourcePath), Is.EqualTo("write(1);"));
            _editor.Save();
            NUnit.Framework.Assert.That(File.ReadAllText(sourcePath), Is.EqualTo(sourceText));
            NUnit.Framework.Assert.That(File.ReadAllText(_editor.SavePath), Is.EqualTo("write(3); // clone only"));
        }

        [TestCaseSource(nameof(EditorKinds))]
        public void RepeatedUntitledClonesHaveIndependentNamesAndDirtyState(CodeEditorType kind)
        {
            OpenEditor(kind);
            foreach (var text in new[] { string.Empty, "first line\nsecond line", "α·β + 1 // 😀" })
            {
                _editor.NewDocument();
                var sourceName = _editor.CurrentFileName;
                _editor.Text = text;
                CloneItem.PerformClick();
                var firstName = _editor.CurrentFileName;
                NUnit.Framework.Assert.That(firstName, Is.Not.EqualTo(sourceName));
                NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo(text));
                NUnit.Framework.Assert.That(_tabs.SelectedTab.Tag, Is.Null);
                NUnit.Framework.Assert.That(_tabs.SelectedTab.ImageIndex, Is.EqualTo(text.Length == 0 ? 0 : 1));

                CloneItem.PerformClick();
                var secondName = _editor.CurrentFileName;
                NUnit.Framework.Assert.That(secondName, Is.Not.EqualTo(sourceName).And.Not.EqualTo(firstName));
                _editor.Text = "second clone";
                _editor.SwitchTab(firstName);
                NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo(text));
                _editor.SwitchTab(sourceName);
                NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo(text));
            }
            NUnit.Framework.Assert.That(Directory.GetFiles(_directory), Is.Empty);
        }

        [TestCaseSource(nameof(EditorKinds))]
        public void RightClickClonesTheClickedBackgroundDocument(CodeEditorType kind)
        {
            OpenEditor(kind);
            _editor.Text = "clicked document";
            var source = _tabs.SelectedTab;
            _editor.NewDocument();
            var otherName = _editor.CurrentFileName;
            _editor.Text = "previously selected document";
            var header = _tabs.GetTabRect(_tabs.TabPages.IndexOf(source));
            var point = new Point(header.Left + header.Width / 2, header.Top + header.Height / 2);
            typeof(Control).GetMethod("OnMouseDown", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_tabs, new object[] { new MouseEventArgs(MouseButtons.Right, 1, point.X, point.Y, 0) });

            _tabs.ContextMenuStrip.Show(_tabs, point);
            CloneItem.PerformClick();
            _tabs.ContextMenuStrip.Close();

            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("clicked document"));
            NUnit.Framework.Assert.That(_tabs.SelectedTab, Is.Not.SameAs(source));
            NUnit.Framework.Assert.That(_tabs.SelectedTab.Tag, Is.Null);
            _editor.SwitchTab(otherName);
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("previously selected document"));
        }

        [TestCaseSource(nameof(EditorKinds))]
        public void OpenMenuKeepsItsChosenDocumentWhenSelectionChanges(CodeEditorType kind)
        {
            OpenEditor(kind);
            _editor.Text = "chosen at menu opening";
            var sourceName = _editor.CurrentFileName;
            _editor.NewDocument();
            var otherName = _editor.CurrentFileName;
            _editor.Text = "later selection";
            _editor.SwitchTab(sourceName);
            _tabs.ContextMenuStrip.Show(_tabs, new Point(5, 5));
            _editor.SwitchTab(otherName);

            // Native activation can close the dropdown before the item's Click.
            _tabs.ContextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
            CloneItem.PerformClick();

            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("chosen at menu opening"));
            NUnit.Framework.Assert.That(_editor.CurrentFileName, Is.Not.EqualTo(sourceName).And.Not.EqualTo(otherName));
        }

        [TestCaseSource(nameof(EditorKinds))]
        public void DismissingAMenuDoesNotRetainItsCloneTarget(CodeEditorType kind)
        {
            OpenEditor(kind);
            _editor.Text = "dismissed target";
            _tabs.ContextMenuStrip.Show(_tabs, new Point(5, 5));
            _tabs.ContextMenuStrip.Close();
            _editor.NewDocument();
            var selectedName = _editor.CurrentFileName;
            _editor.Text = "keyboard target";
            typeof(Control).GetMethod("OnKeyDown", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_tabs, new object[] { new KeyEventArgs(Keys.Apps) });

            _tabs.ContextMenuStrip.Show(_tabs, new Point(5, 5));
            CloneItem.PerformClick();
            _tabs.ContextMenuStrip.Close();

            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("keyboard target"));
            NUnit.Framework.Assert.That(_editor.CurrentFileName, Is.Not.EqualTo(selectedName));
        }

        [TestCaseSource(nameof(EditorKinds))]
        public void RemovedMenuTargetCannotCloneAnotherDocument(CodeEditorType kind)
        {
            OpenEditor(kind);
            var sourceName = _editor.CurrentFileName;
            _editor.NewDocument();
            var otherName = _editor.CurrentFileName;
            _editor.Text = "keep this document";
            _editor.SwitchTab(sourceName);
            _tabs.ContextMenuStrip.Show(_tabs, new Point(5, 5));
            _editor.CloseDocument(sourceName);
            var count = _tabs.TabCount;

            _tabs.ContextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
            CloneItem.PerformClick();

            NUnit.Framework.Assert.That(_tabs.TabCount, Is.EqualTo(count));
            NUnit.Framework.Assert.That(_editor.CurrentFileName, Is.EqualTo(otherName));
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("keep this document"));
        }

        [Test]
        public void CloneWithoutADocumentIsDisabled()
        {
            _window = new Form();
            _tabs = new DocumentsTabControl();
            _tabs.TabPages.Clear();
            _window.Controls.Add(_tabs);
            _window.Show();
            _tabs.ContextMenuStrip.Show(_tabs, new Point(5, 5));

            NUnit.Framework.Assert.That(CloneItem.Enabled, Is.False);
            CloneItem.PerformClick();
            _tabs.ContextMenuStrip.Close();
            NUnit.Framework.Assert.That(_tabs.TabCount, Is.Zero);
        }

        private ToolStripMenuItem CloneItem =>
            (ToolStripMenuItem)_tabs.ContextMenuStrip.Items["cloneTabToolStripMenuItem"];

        private void OpenEditor(CodeEditorType kind)
        {
            Settings.Default.CodeEditor = kind;
            var state = new SharedViewState();
            var textEditor = new TextEditorCodeEditor(state);
#if __MonoCS__
            _editor = new CloneEditor(textEditor);
#else
            var completion = new EmptyCompletion();
            _editor = new CloneEditor(new ScintillaCodeEditorControl(state, completion),
                new AvalonEditCodeEditor(state, completion), textEditor);
#endif
            _window = new Form { Width = 1000, Height = 600 };
            _window.Controls.Add(_editor);
            _window.Show();
            Application.DoEvents();
            _tabs = Descendants(_editor).OfType<DocumentsTabControl>().Single();
        }

        private static IEnumerable<Control> Descendants(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;
                foreach (var descendant in Descendants(child)) yield return descendant;
            }
        }

        private sealed class EmptyCompletion : IAutocompleteProvider
        {
            public IEnumerable<AutocompleteItem> ExpressionAutocompleteItems => new AutocompleteItem[0];
            public IEnumerable<AutocompleteItem> ScriptingAutocompleteItems => new AutocompleteItem[0];
        }

        private sealed class CloneEditor : CodeEditorControlWrapper
        {
            public string SavePath;
            public readonly List<string> PromptedFiles = new List<string>();
#if __MonoCS__
            public CloneEditor(TextEditorCodeEditor textEditor) : base(textEditor) { }
#else
            public CloneEditor(ScintillaCodeEditorControl scintilla, AvalonEditCodeEditor avalon,
                TextEditorCodeEditor textEditor) : base(scintilla, avalon, textEditor) { }
#endif
            protected override string SelectSaveFileName() => SavePath;

            protected override DialogResult PromptToSaveChanges(string filename)
            {
                PromptedFiles.Add(filename);
                return DialogResult.Cancel;
            }
        }
    }
}
