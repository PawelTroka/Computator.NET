using System;
using System.CodeDom.Compiler;
using System.Drawing;
using System.Linq;
using System.Threading;
using Computator.NET.Core.Model;
using Computator.NET.Desktop.Controls.CodeEditors;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class TextEditorErrorHighlightingTests
    {
        private const string Source = "write(1);\r\nmissing(2);\r\nwrite(3);";
        private TextEditorCodeEditor _editor;

        [SetUp]
        public void OpenLegacyEditor()
        {
            _editor = new TextEditorCodeEditor(new SharedViewState());
            _editor.NewDocument("first.tsl", Source);
            NUnit.Framework.Assert.That(_editor.Handle, Is.Not.EqualTo(IntPtr.Zero));
        }

        [TearDown]
        public void DisposeEditor()
        {
            _editor.Dispose();
        }

        [Test]
        public void DiagnosticsHighlightTheReportedSourceLineWithoutEditingTheDocument()
        {
            _editor.ActiveTextAreaControl.Caret.Position = new TextLocation(4, 1);
            var caret = _editor.ActiveTextAreaControl.Caret.Position;
            var couldUndo = _editor.EnableUndo;

            _editor.HighlightErrors(new[] { Error(2, "CS0103", "The name 'missing' does not exist.") });

            var marker = Markers(_editor.Document).Single();
            NUnit.Framework.Assert.Multiple(() =>
            {
                NUnit.Framework.Assert.That(marker.Offset, Is.EqualTo(Source.IndexOf("missing", StringComparison.Ordinal)));
                NUnit.Framework.Assert.That(marker.Length, Is.EqualTo("missing(2);".Length));
                NUnit.Framework.Assert.That(marker.TextMarkerType, Is.EqualTo(TextMarkerType.SolidBlock));
                NUnit.Framework.Assert.That(marker.ToolTip, Is.EqualTo("CS0103: The name 'missing' does not exist."));
                NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo(Source));
                NUnit.Framework.Assert.That(_editor.ActiveTextAreaControl.Caret.Position, Is.EqualTo(caret));
                NUnit.Framework.Assert.That(_editor.EnableUndo, Is.EqualTo(couldUndo));
            });
        }

        [Test]
        public void ANewCompilationReplacesOldDiagnosticsAndCombinesMessagesOnTheSameLine()
        {
            _editor.HighlightErrors(new[] { Error(1) });
            NUnit.Framework.Assert.That(Markers(_editor.Document), Has.Length.EqualTo(1));

            _editor.HighlightErrors(new[]
            {
                Error(2, "CS0103", "Unknown name."),
                Error(2, "CS1002", "Missing semicolon."),
                Error(2, "CS0103", "Unknown name.")
            });

            var marker = Markers(_editor.Document).Single();
            NUnit.Framework.Assert.That(marker.Offset, Is.EqualTo(Source.IndexOf("missing", StringComparison.Ordinal)));
            NUnit.Framework.Assert.That(marker.ToolTip,
                Is.EqualTo("CS0103: Unknown name." + Environment.NewLine + "CS1002: Missing semicolon."));
        }

        [Test]
        public void ClearingAndReplacingDiagnosticsPreserveUnrelatedMarkers()
        {
            var searchResult = new TextMarker(0, 5, TextMarkerType.SolidBlock, Color.LightGreen);
            _editor.Document.MarkerStrategy.AddMarker(searchResult);
            _editor.HighlightErrors(new[] { Error(2) });
            NUnit.Framework.Assert.That(Markers(_editor.Document), Has.Length.EqualTo(2));

            _editor.ClearHighlightedErrors();
            _editor.ClearHighlightedErrors();

            NUnit.Framework.Assert.That(Markers(_editor.Document), Is.EqualTo(new[] { searchResult }));
            _editor.HighlightErrors(new[] { Error(1) });
            _editor.HighlightErrors(new[] { Error(3) });
            NUnit.Framework.Assert.That(Markers(_editor.Document), Does.Contain(searchResult));
            NUnit.Framework.Assert.That(Markers(_editor.Document), Has.Length.EqualTo(2));
            _editor.HighlightErrors(Array.Empty<CompilerError>());
            NUnit.Framework.Assert.That(Markers(_editor.Document), Is.EqualTo(new[] { searchResult }));
        }

        [TestCase("insert")]
        [TestCase("remove")]
        [TestCase("replace")]
        [TestCase("replace-all")]
        [TestCase("undo")]
        [TestCase("redo")]
        public void EditingTheCompiledBufferClearsStaleDiagnostics(string action)
        {
            if (action == "undo" || action == "redo")
                _editor.Document.Insert(_editor.Document.TextLength, "\r\nwrite(4);");
            if (action == "redo")
                _editor.Undo();
            _editor.HighlightErrors(new[] { Error(2) });
            NUnit.Framework.Assert.That(Markers(_editor.Document), Has.Length.EqualTo(1));

            switch (action)
            {
                case "insert": _editor.Document.Insert(0, "// prefix\r\n"); break;
                case "remove": _editor.Document.Remove(0, 1); break;
                case "replace": _editor.Document.Replace(0, 5, "print"); break;
                case "replace-all": _editor.Text = "write(42);"; break;
                case "undo": _editor.Undo(); break;
                case "redo": _editor.Redo(); break;
            }

            NUnit.Framework.Assert.That(Markers(_editor.Document), Is.Empty);
        }

        [Test]
        public void DiagnosticsStayWithTheirDocumentAcrossSwitchesAndRenames()
        {
            _editor.HighlightErrors(new[] { Error(2) });
            var first = _editor.Document;
            var firstMarker = Markers(first).Single();
            _editor.NewDocument("second.tsl", "anotherMissing();");
            NUnit.Framework.Assert.That(Markers(_editor.Document), Is.Empty);
            _editor.HighlightErrors(new[] { Error(1) });
            var second = _editor.Document;
            var secondMarker = Markers(second).Single();

            _editor.RenameDocument("first.tsl", "renamed.tsl");
            _editor.SwitchDocument("renamed.tsl");
            NUnit.Framework.Assert.That(Markers(_editor.Document), Is.EqualTo(new[] { firstMarker }));
            _editor.ClearHighlightedErrors();
            NUnit.Framework.Assert.That(Markers(first), Is.Empty);
            NUnit.Framework.Assert.That(Markers(second), Is.EqualTo(new[] { secondMarker }));
            _editor.SwitchDocument("second.tsl");
            NUnit.Framework.Assert.That(Markers(_editor.Document), Is.EqualTo(new[] { secondMarker }));
        }

        [Test]
        public void EditingABackgroundDocumentDoesNotClearTheActiveDocumentsDiagnostics()
        {
            _editor.HighlightErrors(new[] { Error(2) });
            var first = _editor.Document;
            NUnit.Framework.Assert.That(Markers(first), Has.Length.EqualTo(1));
            _editor.NewDocument("second.tsl", "anotherMissing();");
            _editor.HighlightErrors(new[] { Error(1) });
            var secondMarker = Markers(_editor.Document).Single();

            first.Insert(0, "// changed\r\n");

            NUnit.Framework.Assert.That(Markers(first), Is.Empty);
            NUnit.Framework.Assert.That(Markers(_editor.Document), Is.EqualTo(new[] { secondMarker }));
        }

        [Test]
        public void ClosingADocumentRemovesItsDiagnosticsWithoutClearingSurvivingDocuments()
        {
            _editor.HighlightErrors(new[] { Error(2) });
            var first = _editor.Document;
            NUnit.Framework.Assert.That(Markers(first), Has.Length.EqualTo(1));
            _editor.NewDocument("second.tsl", "anotherMissing();");
            _editor.HighlightErrors(new[] { Error(1) });
            var secondMarker = Markers(_editor.Document).Single();

            _editor.CloseDocument("first.tsl");

            NUnit.Framework.Assert.That(Markers(first), Is.Empty);
            NUnit.Framework.Assert.That(Markers(_editor.Document), Is.EqualTo(new[] { secondMarker }));
            _editor.NewDocument("first.tsl", Source);
            NUnit.Framework.Assert.That(Markers(_editor.Document), Is.Empty);
        }

        [Test]
        public void DisposingTheEditorRemovesDiagnosticsFromAllItsDocuments()
        {
            _editor.HighlightErrors(new[] { Error(2) });
            var first = _editor.Document;
            NUnit.Framework.Assert.That(Markers(first), Has.Length.EqualTo(1));
            _editor.NewDocument("second.tsl", "anotherMissing();");
            _editor.HighlightErrors(new[] { Error(1) });
            var second = _editor.Document;
            NUnit.Framework.Assert.That(Markers(second), Has.Length.EqualTo(1));

            _editor.Dispose();

            NUnit.Framework.Assert.That(Markers(first), Is.Empty);
            NUnit.Framework.Assert.That(Markers(second), Is.Empty);
        }

        [Test]
        public void DiagnosticsWithoutAVisibleSourceLineDoNotBreakErrorReporting()
        {
            _editor.HighlightErrors(new[] { Error(-1), Error(0), Error(99), null });
            NUnit.Framework.Assert.That(Markers(_editor.Document), Is.Empty);
            _editor.HighlightErrors(null);
            _editor.NewDocument("empty.tsl", "");
            _editor.HighlightErrors(new[] { Error(1), Error(2) });
            NUnit.Framework.Assert.That(Markers(_editor.Document), Is.Empty);
            _editor.Text = "write(1);\r\n\r\n";
            _editor.HighlightErrors(new[] { Error(2), Error(3) });
            NUnit.Framework.Assert.That(Markers(_editor.Document), Is.Empty);
        }

        private static CompilerError Error(int line, string number = "CS0103", string message = "Unknown name.") =>
            new CompilerError("source.tsl", line, 1, number, message);

        private static TextMarker[] Markers(IDocument document) => document.MarkerStrategy.TextMarker.ToArray();
    }
}
