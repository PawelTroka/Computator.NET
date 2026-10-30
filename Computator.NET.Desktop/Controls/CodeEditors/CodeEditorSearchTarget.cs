using System;
using System.Collections.Generic;
using System.Linq;
using Computator.NET.Core.Abstract.Controls;
#if !__MonoCS__
using Computator.NET.Desktop.Controls.CodeEditors.AvalonEdit;
#endif

namespace Computator.NET.Desktop.Controls.CodeEditors
{
    // Keep selection and undo operations in the editor's native document model.
    internal sealed class CodeEditorSearchTarget
    {
        private readonly ICodeEditorControl _editor;
        internal CodeEditorSearchTarget(ICodeEditorControl editor) { _editor = editor; }
        internal string Text => _editor.Text;
        internal bool HasAdvancedSearch
        {
            get
            {
#if !__MonoCS__
                return _editor is AvalonEditCodeEditor;
#else
                return false;
#endif
            }
        }

        internal void ShowAdvancedSearch()
        {
#if !__MonoCS__
            if (_editor is AvalonEditCodeEditor avalon) avalon.ShowAdvancedSearch();
#endif
        }

        internal bool IsReadOnly
        {
            get
            {
#if !__MonoCS__
                if (_editor is AvalonEditCodeEditor avalon) return avalon.IsReadOnly;
                if (_editor is ScintillaCodeEditorControl scintilla) return scintilla.ReadOnly;
#endif
                return ((TextEditorCodeEditor)_editor).Document.ReadOnly;
            }
        }

        internal int SelectionStart
        {
            get
            {
#if !__MonoCS__
                if (_editor is AvalonEditCodeEditor avalon) return avalon.SelectionStart;
                if (_editor is ScintillaCodeEditorControl scintilla) return scintilla.SelectionStart;
#endif
                var legacy = (TextEditorCodeEditor)_editor;
                var selection = legacy.ActiveTextAreaControl.SelectionManager.SelectionCollection.FirstOrDefault();
                return selection == null ? legacy.ActiveTextAreaControl.Caret.Offset :
                    legacy.Document.PositionToOffset(selection.StartPosition);
            }
        }

        internal int SelectionLength
        {
            get
            {
#if !__MonoCS__
                if (_editor is AvalonEditCodeEditor avalon) return avalon.SelectionLength;
                if (_editor is ScintillaCodeEditorControl scintilla) return scintilla.SelectionEnd - scintilla.SelectionStart;
#endif
                var legacy = (TextEditorCodeEditor)_editor;
                var selection = legacy.ActiveTextAreaControl.SelectionManager.SelectionCollection.FirstOrDefault();
                return selection == null ? 0 : legacy.Document.PositionToOffset(selection.EndPosition) -
                    legacy.Document.PositionToOffset(selection.StartPosition);
            }
        }

        internal void Select(int start, int length)
        {
#if !__MonoCS__
            if (_editor is AvalonEditCodeEditor avalon)
            {
                avalon.Select(start, length);
                var position = avalon.Document.GetLocation(start);
                avalon.ScrollTo(position.Line, position.Column);
                return;
            }
            if (_editor is ScintillaCodeEditorControl scintilla)
            {
                scintilla.SetSelection(start + length, start);
                scintilla.ScrollCaret();
                return;
            }
#endif
            var legacy = (TextEditorCodeEditor)_editor;
            var area = legacy.ActiveTextAreaControl;
            area.SelectionManager.ClearSelection();
            area.Caret.Position = legacy.Document.OffsetToPosition(start + length);
            // The legacy manager retains its construction-time document. Its
            // two-position overload would bind the selection to that old buffer.
            area.SelectionManager.SetSelection(new ICSharpCode.TextEditor.Document.DefaultSelection(
                legacy.Document, legacy.Document.OffsetToPosition(start), legacy.Document.OffsetToPosition(start + length)));
            area.ScrollToCaret();
            legacy.Refresh();
        }

        internal bool Replace(IReadOnlyList<int> offsets, int length, string replacement)
        {
            if (IsReadOnly) return false;
            if (offsets.Count == 0) return true;
            replacement = replacement ?? string.Empty;
#if !__MonoCS__
            if (_editor is AvalonEditCodeEditor avalon)
            {
                using (avalon.Document.RunUpdate())
                    for (var i = offsets.Count - 1; i >= 0; i--)
                        avalon.Document.Replace(offsets[i], length, replacement);
                return true;
            }
            if (_editor is ScintillaCodeEditorControl scintilla)
            {
                scintilla.BeginUndoAction();
                try
                {
                    for (var i = offsets.Count - 1; i >= 0; i--)
                    {
                        scintilla.TargetStart = offsets[i];
                        scintilla.TargetEnd = offsets[i] + length;
                        scintilla.ReplaceTarget(replacement);
                    }
                }
                finally { scintilla.EndUndoAction(); }
                return true;
            }
#endif
            var legacy = (TextEditorCodeEditor)_editor;
            legacy.Document.UndoStack.StartUndoGroup();
            try
            {
                for (var i = offsets.Count - 1; i >= 0; i--)
                    legacy.Document.Replace(offsets[i], length, replacement);
            }
            finally { legacy.Document.UndoStack.EndUndoGroup(); }
            legacy.Refresh();
            return true;
        }
    }
}
