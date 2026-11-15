using System;
using System.Drawing;
using System.Reflection;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace Computator.NET.Desktop.Controls.CodeEditors
{
    // SimpleCodeEditor 2.0.0 exposes a Document setter, but does not rebind
    // the native selection manager or the text area's document listeners.
    // Keep this compatibility boundary together until the vendor provides a
    // document-switch API. Recreating the public TextAreaControl is insufficient:
    // TextEditorControl retains private primary/secondary controls for Undo and
    // redraw, while completion and other consumers retain the existing TextArea.
    internal sealed class TextEditorDocumentBinding : IDisposable
    {
        private static readonly FieldInfo SelectionDocument =
            typeof(SelectionManager).GetField("document", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(typeof(SelectionManager).FullName, "document");

        private readonly TextEditorControl _editor;
        private readonly TextAreaControl _area;
        private readonly EventHandler _commitUpdate;
        private readonly EventHandler _resetTextArea;
        private readonly EventHandler _validateCaret;
        private readonly EventHandler _foldingsChanged;
        private readonly EventHandler _areaUpdate;
        private readonly DocumentEventHandler _selectionChanged;
        private readonly DocumentEventHandler _adjustScrollBars;

        internal TextEditorDocumentBinding(TextEditorControl editor)
        {
            _editor = editor;
            _area = editor.ActiveTextAreaControl;
            _commitUpdate = Bind<EventHandler>(typeof(TextEditorControl), editor, "CommitUpdateRequested");
            _resetTextArea = Bind<EventHandler>(typeof(TextArea), _area.TextArea, "TextContentChanged");
            _validateCaret = Bind<EventHandler>(typeof(TextAreaControl), _area, "DocumentTextContentChanged");
            _foldingsChanged = Bind<EventHandler>(typeof(TextArea), _area.TextArea, "DocumentFoldingsChanged");
            _areaUpdate = Bind<EventHandler>(typeof(TextAreaControl), _area, "DocumentUpdateCommitted");
            _selectionChanged = Bind<DocumentEventHandler>(typeof(SelectionManager), _area.SelectionManager, "DocumentChanged");
            _adjustScrollBars = Bind<DocumentEventHandler>(typeof(TextAreaControl), _area, "AdjustScrollBarsOnDocumentChange");
        }

        internal void SwitchDocument(IDocument next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            var previous = _editor.Document;
            if (ReferenceEquals(previous, next)) return;

            // Clear while offsets still refer to the outgoing document. Do not
            // replace live controls, selections or undo stacks with snapshots.
            _area.SelectionManager.ClearSelection();
            Detach(previous);
            previous.UndoStack.TextEditorControl = null;

            _editor.Document = next;
            SelectionDocument.SetValue(_area.SelectionManager, next);

            // Match construction order. Native disposal now detaches these
            // same delegates from the active document, not an obsolete buffer.
            next.DocumentChanged += _selectionChanged;
            next.TextContentChanged += _resetTextArea;
            next.FoldingManager.FoldingsChanged += _foldingsChanged;
            next.TextContentChanged += _validateCaret;
            next.DocumentChanged += _adjustScrollBars;
            next.UpdateCommited += _areaUpdate;
            next.UpdateCommited += _commitUpdate;

            _area.TextArea.VirtualTop = Point.Empty;
            _adjustScrollBars(next, new DocumentEventArgs(next));
            _resetTextArea(next, EventArgs.Empty);
            _area.TextArea.UpdateMatchingBracket();
        }

        private void Detach(IDocument document)
        {
            document.UpdateCommited -= _commitUpdate;
            document.TextContentChanged -= _resetTextArea;
            document.TextContentChanged -= _validateCaret;
            document.FoldingManager.FoldingsChanged -= _foldingsChanged;
            document.UpdateCommited -= _areaUpdate;
            document.DocumentChanged -= _selectionChanged;
            document.DocumentChanged -= _adjustScrollBars;
        }

        public void Dispose()
        {
            Detach(_editor.Document);
        }

        private static T Bind<T>(Type declaringType, object target, string name) where T : Delegate
        {
            var method = declaringType.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new MissingMethodException(declaringType.FullName, name);
            return (T)Delegate.CreateDelegate(typeof(T), target, method);
        }
    }
}
