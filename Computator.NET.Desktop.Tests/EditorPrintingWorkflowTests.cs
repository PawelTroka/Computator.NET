using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Bootstrapping;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Controls.CodeEditors;
using Computator.NET.Desktop.Controls.CodeEditors.AvalonEdit;
using Computator.NET.Desktop.Controls.CodeEditors.Printing;
using NUnit.Framework;
using Settings = Computator.NET.Core.Properties.Settings;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class EditorPrintingWorkflowTests
    {
        private CoreBootstrapper _services;
        private Form _window;
        private CapturingEditor _editor;
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
            _window = null;
            _services = null;
            _oldEditor = Settings.Default.CodeEditor;
            _directory = Path.Combine(Path.GetTempPath(), "Computator-Printing-" + Guid.NewGuid().ToString("N"));
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

        [TestCase(CodeEditorType.Scintilla, false)]
        [TestCase(CodeEditorType.Scintilla, true)]
        [TestCase(CodeEditorType.AvalonEdit, false)]
        [TestCase(CodeEditorType.AvalonEdit, true)]
        [TestCase(CodeEditorType.TextEditor, false)]
        [TestCase(CodeEditorType.TextEditor, true)]
        public void PrintAndPreviewCaptureTheSelectedUnsavedBufferWithoutChangingIt(CodeEditorType kind, bool preview)
        {
            OpenEditor(kind);
            var first = OpenFile("źródło.tsl", "saved first");
            var second = OpenFile("helpers.tslf", "saved second");
            _editor.Text = "other unsaved buffer";
            _editor.SwitchTab(first);
            const string draft = "write(\"α * β 😀\");\r\n\twrite(2³);";
            _editor.Text = draft;
            _editor.SelectAll();
            var selectedText = SelectedText();
            var selected = Tabs.SelectedTab;
            var dirty = selected.ImageIndex;

            if (preview) _editor.PrintPreview(); else _editor.Print();

            NUnit.Framework.Assert.That(_editor.Requests, Is.EqualTo(1));
            NUnit.Framework.Assert.That(_editor.Preview, Is.EqualTo(preview));
            NUnit.Framework.Assert.That(_editor.Snapshot, Is.EqualTo(draft));
            NUnit.Framework.Assert.That(_editor.DocumentName, Is.EqualTo(Path.GetFileName(first)));
            NUnit.Framework.Assert.That(Tabs.SelectedTab, Is.SameAs(selected));
            NUnit.Framework.Assert.That(selected.ImageIndex, Is.EqualTo(dirty));
            NUnit.Framework.Assert.That(_editor.CurrentFileName, Is.EqualTo(first));
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo(draft));
            NUnit.Framework.Assert.That(SelectedText(), Is.EqualTo(selectedText));
            NUnit.Framework.Assert.That(File.ReadAllText(first), Is.EqualTo("saved first"));
            NUnit.Framework.Assert.That(File.ReadAllText(second), Is.EqualTo("saved second"));
            _editor.SwitchTab(second);
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("other unsaved buffer"));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void EmptyDocumentsDoNotOpenADialogOrSubmitABlankJob(CodeEditorType kind)
        {
            OpenEditor(kind);
            _editor.Text = string.Empty;
            _editor.Print();
            _editor.PrintPreview();
            NUnit.Framework.Assert.That(_editor.Requests, Is.Zero);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void UntitledSnapshotDoesNotFollowEditsOrTabChangesDuringTheDialog(CodeEditorType kind)
        {
            OpenEditor(kind);
            var original = _editor.CurrentFileName;
            _editor.Text = "unsaved original";
            _editor.DuringDialog = document =>
            {
                _editor.Text = "changed after snapshot";
                _editor.NewDocument();
                _editor.Text = "different document";
                NUnit.Framework.Assert.That(document.SnapshotText, Is.EqualTo("unsaved original"));
            };

            _editor.PrintPreview();

            NUnit.Framework.Assert.That(_editor.Snapshot, Is.EqualTo("unsaved original"));
            NUnit.Framework.Assert.That(_editor.DocumentName, Is.EqualTo(Path.GetFileName(original)));
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("different document"));
            _editor.SwitchTab(original);
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("changed after snapshot"));
            NUnit.Framework.Assert.That(Directory.GetFiles(_directory), Is.Empty);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void PrintingDoesNotRereadLockedOrDeletedBackingFiles(CodeEditorType kind)
        {
            OpenEditor(kind);
            var path = OpenFile("locked.tsl", "saved");
            _editor.Text = "draft held only in the editor";
            using (new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                _editor.Print();
            File.Delete(path);
            _editor.PrintPreview();
            NUnit.Framework.Assert.That(_editor.Requests, Is.EqualTo(2));
            NUnit.Framework.Assert.That(_editor.Snapshot, Is.EqualTo("draft held only in the editor"));
            NUnit.Framework.Assert.That(File.Exists(path), Is.False);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void PrinterErrorsLeaveTheBufferSelectionAndDirtyStateIntact(CodeEditorType kind)
        {
            OpenEditor(kind);
            _editor.Text = "unsaved after printer failure";
            _editor.SelectAll();
            var selectedText = SelectedText();
            var tab = Tabs.SelectedTab;
            var dirty = tab.ImageIndex;
            _editor.DuringDialog = _ => throw new CodePrintLayoutException("No printable area.");

            _editor.Print();

            NUnit.Framework.Assert.That(_editor.Error, Is.EqualTo("No printable area."));
            NUnit.Framework.Assert.That(Tabs.SelectedTab, Is.SameAs(tab));
            NUnit.Framework.Assert.That(tab.ImageIndex, Is.EqualTo(dirty));
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("unsaved after printer failure"));
            NUnit.Framework.Assert.That(SelectedText(), Is.EqualTo(selectedText));
            NUnit.Framework.Assert.That(Directory.GetFiles(_directory), Is.Empty);
        }

        [Test]
        public void CleanupFailureAfterAPageErrorDoesNotShowASecondErrorDialog()
        {
            OpenEditor(CodeEditorType.TextEditor);
            _editor.Text = "preserved after both failures";
            _editor.DuringDialog = document =>
            {
                document.ReportError(new CodePrintLayoutException("Page does not fit."));
                throw new System.ComponentModel.Win32Exception("Controller cleanup failed.");
            };

            _editor.Print();

            NUnit.Framework.Assert.That(_editor.Errors, Is.EqualTo(new[] { "Page does not fit." }));
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("preserved after both failures"));
        }

        [Test]
        public void UnexpectedProgrammingErrorsAreNotConvertedIntoPrinterWarnings()
        {
            OpenEditor(CodeEditorType.TextEditor);
            _editor.Text = "preserved draft";
            var failure = new InvalidOperationException("Unexpected document state.");
            _editor.DuringDialog = _ => throw failure;

            var actual = NUnit.Framework.Assert.Throws<InvalidOperationException>(() => _editor.Print());

            NUnit.Framework.Assert.That(actual, Is.SameAs(failure));
            NUnit.Framework.Assert.That(CodePrintDocument.IsPrintError(failure), Is.False);
            NUnit.Framework.Assert.That(_editor.Errors, Is.Empty);
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo("preserved draft"));
        }

        [Test]
        public void ProductionRendererWrapsUnicodeIntoMultiplePagesAndProducesAnImageAttachment()
        {
            var text = string.Join("\n", Enumerable.Range(1, 60).Select(index =>
                "write(\"α 😀 a\u0301 ² " + index + "\");\t// " + new string('x', 60)));
            using var font = new Font(FontFamily.GenericMonospace, 10);
            using var document = new CodePrintDocument("unicode.tsl", text, font);
            using var bitmap = new Bitmap(320, 260);
            using var graphics = Graphics.FromImage(bitmap);
            var bounds = new RectangleF(10, 10, 300, 240);
            var page = 0;
            bool more;
            do
            {
                graphics.Clear(Color.White);
                more = document.RenderPage(graphics, bounds);
                page++;
                NUnit.Framework.Assert.That(HasInk(bitmap, new Rectangle(10, 32, 300, 190)), Is.True,
                    "Every page must contain document text, not only its header/footer.");
                NUnit.Framework.Assert.That(page, Is.LessThan(100), "Pagination must terminate.");
                if (page == 1)
                {
                    var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "editor-print-first-page.png");
                    bitmap.Save(path, ImageFormat.Png);
                    TestContext.AddTestAttachment(path, "Production editor print renderer, first page");
                }
            } while (more);
            NUnit.Framework.Assert.That(page, Is.GreaterThan(1));
        }

        [Test]
        public void StartingANewPrintPassResetsTextAndPageNumber()
        {
            using var font = new Font(FontFamily.GenericMonospace, 10);
            using var document = new CodePrintDocument("repeat.tsl", new string('W', 500), font);
            using var bitmap = new Bitmap(300, 180);
            using var graphics = Graphics.FromImage(bitmap);
            var bounds = new RectangleF(10, 10, 280, 160);
            graphics.Clear(Color.White);
            NUnit.Framework.Assert.That(document.RenderPage(graphics, bounds), Is.True);
            var first = Pixels(bitmap);
            graphics.Clear(Color.White);
            document.RenderPage(graphics, bounds);

            document.ResetPagination();
            graphics.Clear(Color.White);
            NUnit.Framework.Assert.That(document.RenderPage(graphics, bounds), Is.True);

            NUnit.Framework.Assert.That(Pixels(bitmap), Is.EqualTo(first),
                "Printing after preview must restart at the first row and page number.");
        }

        [Test]
        public void UnusablePageBoundsFailBeforeDrawing()
        {
            using var font = new Font(FontFamily.GenericMonospace, 10);
            using var document = new CodePrintDocument("small.tsl", "text", font);
            using var bitmap = new Bitmap(100, 100);
            using var graphics = Graphics.FromImage(bitmap);
            NUnit.Framework.Assert.Throws<CodePrintLayoutException>(() =>
                document.RenderPage(graphics, new RectangleF(0, 0, 100, 1)));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void InvalidPrinterFailuresAreCancelledAtTheCallbackBoundary(bool pageCallback)
        {
            using var font = new Font(FontFamily.GenericMonospace, 10);
            using var document = new CodePrintDocument("missing-printer.tsl", "unchanged text", font);
            document.PrinterSettings.PrinterName = "Computator missing printer " + Guid.NewGuid().ToString("N");
            string error = null;
            document.PrintFailed += message => error = message;
            if (pageCallback)
            {
                using var bitmap = new Bitmap(100, 100);
                using var graphics = Graphics.FromImage(bitmap);
                var args = new PrintPageEventArgs(graphics, new Rectangle(0, 0, 100, 100),
                    new Rectangle(0, 0, 100, 100), new PageSettings(document.PrinterSettings));
                typeof(CodePrintDocument).GetMethod("OnPrintPage", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(document, new object[] { args });
                NUnit.Framework.Assert.That(args.Cancel, Is.True);
                NUnit.Framework.Assert.That(args.HasMorePages, Is.False);
            }
            else
            {
                var args = new PrintEventArgs();
                typeof(CodePrintDocument).GetMethod("OnBeginPrint", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(document, new object[] { args });
                NUnit.Framework.Assert.That(args.Cancel, Is.True);
            }
            NUnit.Framework.Assert.That(error, Is.Not.Null.And.Not.Empty);
            NUnit.Framework.Assert.That(document.SnapshotText, Is.EqualTo("unchanged text"));
        }

        private void OpenEditor(CodeEditorType kind)
        {
            Settings.Default.CodeEditor = _kind = kind;
            _services = new CoreBootstrapper();
            _scintilla = _services.Create<ScintillaCodeEditorControl>();
            _avalon = _services.Create<AvalonEditCodeEditor>();
            _textEditor = _services.Create<TextEditorCodeEditor>();
            _editor = new CapturingEditor(_scintilla, _avalon, _textEditor);
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

        private string SelectedText()
        {
            if (_kind == CodeEditorType.Scintilla) return _scintilla.SelectedText;
            if (_kind == CodeEditorType.AvalonEdit) return _avalon.SelectedText;
            return _textEditor.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText;
        }

        private static IEnumerable<Control> Descendants(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;
                foreach (var descendant in Descendants(child)) yield return descendant;
            }
        }

        private static bool HasInk(Bitmap image, Rectangle bounds)
        {
            for (var y = bounds.Top; y < bounds.Bottom; y++)
                for (var x = bounds.Left; x < bounds.Right; x++)
                    if (image.GetPixel(x, y).ToArgb() != Color.White.ToArgb()) return true;
            return false;
        }

        private static int[] Pixels(Bitmap image)
        {
            var pixels = new int[image.Width * image.Height];
            for (var y = 0; y < image.Height; y++)
                for (var x = 0; x < image.Width; x++)
                    pixels[y * image.Width + x] = image.GetPixel(x, y).ToArgb();
            return pixels;
        }

        private sealed class CapturingEditor : CodeEditorControlWrapper
        {
            internal int Requests;
            internal bool Preview;
            internal string Snapshot;
            internal string DocumentName;
            internal string Error;
            internal readonly List<string> Errors = new List<string>();
            internal Action<CodePrintDocument> DuringDialog;

            internal CapturingEditor(ScintillaCodeEditorControl scintilla, AvalonEditCodeEditor avalon,
                TextEditorCodeEditor textEditor) : base(scintilla, avalon, textEditor) { }

            protected override void ShowPrintDialog(PrintDocument document, bool preview)
            {
                var code = (CodePrintDocument)document;
                Requests++;
                Preview = preview;
                Snapshot = code.SnapshotText;
                DocumentName = code.DocumentName;
                NUnit.Framework.Assert.That(code.OriginAtMargins, Is.True);
                DuringDialog?.Invoke(code);
            }

            protected override void ShowPrintError(string message)
            {
                Error = message;
                Errors.Add(message);
            }
        }
    }
}
