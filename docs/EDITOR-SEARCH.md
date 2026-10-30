# Desktop find and replace

In **Scripting** or **Custom functions**, choose **Edit → Find…** (`Ctrl+F`) or
**Edit → Replace…** (`Ctrl+H`). The dialog searches the active document, including
unsaved changes. Its **Current document** label follows the selected tab. Switching
to another workspace hides the dialog; opening it again keeps that workspace's
query and options.

| Action | Behavior |
| --- | --- |
| Find next / `F3` | Select the next occurrence, wrapping to the beginning after the last match |
| Find previous / `Shift+F3` | Select the previous occurrence, wrapping to the end before the first match |
| Match case | Require the same letter case; matching is independent of the UI language |
| Whole word | Exclude matches adjoining Unicode letters, digits, underscores or combining marks |
| Replace | Replace the selected match, then find the next; if the selection is not a match, find the next first |
| Replace all | Replace every nonoverlapping match in the current document |
| Close / `Esc` | Close the dialog |

Searches and replacements are **literal**. For example, searching for `a.*`
finds those three characters; replacing with `$1` inserts `$1`. Opening the dialog
with a single-line selection uses that text as the query. An empty replacement
deletes matches. Read-only documents remain searchable and their replacement
buttons are disabled.

Each replacement action uses the editor's undo history. One **Undo** reverses an
entire **Replace all** operation. Replacements update the document's unsaved marker;
use **Save** when you want to write the changes to disk. Other open documents are
unaffected.

The shared dialog works with Scintilla, AvalonEdit and TextEditor on Windows and
the reduced Mono desktop's TextEditor. When AvalonEdit is selected, **Advanced
search…** opens its existing native search panel, including regular expressions.
`F3` and `Shift+F3` navigate that panel's results while it is open. Open **Find** or
**Replace** again to return to the shared literal search dialog.

## Regression checks

`TextSearchTests` in the portable Engine test project checks literal matching,
wrapping and Unicode boundaries. `FindReplaceWorkflowTests` in the Windows desktop
test project exercises the shown application, all three native editors, menu and
keyboard routing, replacement/undo, read-only state, document changes and disposal.
It attaches screenshots of the actual dialogs to its test results.

The existing [Mono acceptance runner](MONO-DESKTOP.md) executes the same desktop
menu/dialog workflow against the real legacy editor and saves `find-replace.png`
and `find-replace-dialog.png` alongside its report. A successful cross-build alone
does not establish native Windows UI behavior.
