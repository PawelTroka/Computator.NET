# Desktop document identity and save safety

This review covers the dependent PRs #187 (document identity) and #189 (save
protection). The complete protection requires both PRs. It does not add dirty
document close prompts or session recovery (issues #60 and #59).

## Invariants

- A selected document tab, its backend document, and its save destination identify
  the same document. Public switch/close operations go through the tab lifecycle.
- File-backed document paths become absolute when opened. Relative paths cannot
  change meaning after a script changes the process working directory. Untitled
  labels are not filesystem paths.
- Reopening ordinary case/dot aliases selects the existing buffer. A failed file
  open leaves the prior selection, every existing buffer, and disk contents intact.
- A rename checks the destination before either a tree file move or dictionary
  mutation. It cannot replace another open document, including a document whose
  backing file was deleted outside the application.
- Changing editor implementations transfers the current buffers without rereading
  disk files. The source registry remains available until the target is populated.
  Literal asterisks in source are preserved during transfers and saves.
- Save destination ownership includes both the scripting and custom-functions
  editor views. Disposed views release ownership and settings subscriptions.
- A file snapshot is captured from the same bytes used to load the buffer. Save
  compares the observed file identity and bytes with that snapshot; size and
  timestamp alone are insufficient. Reopening, renaming or switching editors does
  not silently adopt an externally changed version as the baseline.
- Save As remains a way to preserve a draft under a new name after a conflict.
  An explicit selection of a closed existing file may replace that file, subject
  to the native overwrite prompt and a final snapshot check. Selecting the
  current file still uses its original loaded/saved baseline.
- Writes are staged and flushed in the destination directory, then committed.
  New destinations use a no-overwrite move. Existing destinations use replacement
  with a temporary backup; there is no in-place truncating fallback. A failed
  replacement attempts a no-overwrite restoration. If restoration is obstructed,
  the exception identifies the retained original backup. A successful replacement
  also retains and reports its backup if that backup contains an unexpected
  external version or cannot be safely verified and removed.
- Cancelling a filename dialog makes no write or identity change. A changed
  document selection during the dialog aborts the save before writing.

## Path and concurrency boundaries

Ordinary Windows case/dot aliases use the same identity. Device/extended paths,
alternate streams, reserved device names, and trailing-dot/space components are
rejected before mutation. Windows file IDs also identify existing physical aliases
such as short names, hard links and symbolic links for collision checks.

Replacing a reparse-point or multi-hardlink destination could sever its link
relationship. Such saves are refused with a request to choose a new filename.
Filesystems that cannot provide a reliable Windows identity are refused rather
than silently weakening ownership checks. UNC/remote filesystem behavior remains
subject to the manual checks below.

Identity comparisons use metadata-only handles, so an exclusively locked source
or unrelated background file does not prevent saving a draft under a fresh name.
Actual baseline reads and writes still honor file locks. Ambiguous case-only
spellings in a case-sensitive directory are refused instead of combining two
different files into one tab.

The writer holds a handle that excludes in-place writers during final comparison
and replacement. Windows replacement does not provide an atomic compare-and-swap
against another process renaming/replacing the path. This implementation does not
promise protection against every concurrent namespace mutation, power failure,
or filesystem/hardware fault. Before removing a successful replacement's backup,
the writer compares it with the expected version. An unexpected or unverifiable
backup is retained and its path is shown after committing the saved identity.
A cleanup failure must not turn a completed save into a reported failure.

Folder-tree renames do not yet relocate the identities of all open descendants.
An affected document retains its buffer and original baseline; saving to a new
filename is the recovery path. Explicitly closing a dirty document still follows
the application's existing behavior; survivor-preservation tests are not a claim
that unsaved-close prompting has been implemented.

## Automated validation

The desktop fixtures instantiate the real WinForms/WPF/Scintilla editors on an
STA thread. They exercise tab events, context menu commands, the production
Save/Save As methods, and real filesystem reads/writes. The modal filename chooser
is substituted; this is not native-dialog UI automation.

The file-safety fixture exercises the same production writer with deterministic
fault seams for partial staging, destination changes, and partial replacement.
Windows-specific file-identity cases must be assessed in the Windows result, not
in a Linux-only run.

Run the existing acceptance target on Windows with the repository-pinned SDK:

```powershell
./build.ps1 -Target CI '--suite=Computator.NET.Desktop.Tests'
```

The existing `Cake Windows validation` workflow runs this target on PR updates.
Review its exact checkout SHA, TRX counts, failures/skips, and published executable
startup/main-window/shutdown smoke evidence. The latter is a separate smoke test,
not proof of interactive Save workflows. Current run links and exact results are
recorded in the PR descriptions so they can be updated without changing tested
source commits.

## Manual Windows acceptance

Use disposable copies of documents and record the application build, editor type,
and original/resulting bytes. Repeat the core sequences for Scintilla, AvalonEdit,
and TextEditor; include both scripting and custom-functions views.

| Scenario | Required observation |
| --- | --- |
| Edit A/B/C, select A, close background B, Save | A receives A's draft; C's buffer/file and B's file remain unchanged. |
| Close selected tab; close left/right/other tabs | Every surviving tab retains its own text and destination. |
| Rename an open file, including case-only rename | Tab/key/path move together and the old name can be reused safely. |
| Delete B externally, then try to rename A to B in the tree | Refused before moving A; B's unsaved buffer survives. |
| Open a locked/unreadable file | Error leaves the previously selected buffer and its Save destination unchanged. |
| Change editor implementation with several dirty/locked files | All buffers and selection survive; files are not reread or saved. |
| Native Save As: Cancel, Esc, close button | No disk write, filename change or lost buffer. |
| Native Save As: overwrite prompt No/Cancel | No destination write or document identity change. |
| Native Save As: overwrite a closed file and confirm | Only the chosen destination changes; a subsequent Save uses its new identity. |
| Native chooser filters/extensions, Unicode/spaces, case/dot aliases | Final accepted name is correct; existing buffers are not duplicated or overwritten. |
| Destination is open in either editor view | Refused even if native overwrite was confirmed. |
| Current/open source changes before or during native dialog | The external bytes and editor draft survive; Save As to a fresh path succeeds. |
| Closed Save As target changes after the FileOk snapshot | The later external version is not silently discarded; the draft survives or the saved-file notice identifies the retained backup. |
| Locked/read-only destination or full/unavailable volume | Error retains buffers and prior bytes (or reports the retained backup if recovery is obstructed). |
| Link/short-name alias, UNC share, case-sensitive directory | Alias collision is refused or resolved to the same buffer; no other file is changed. |

## Merge sequence

1. Review #187 and its final-head Windows results.
2. Review #189 against that exact #187 head and its combined Windows results.
3. With the owner's approval, merge #187 first. Keep its branch until #189 has
   been retargeted to `master` and its resulting diff checked.
4. If #187 was squash/rebase merged, update #189's base/history as necessary;
   confirm that its diff contains only the save follow-up and that the integrated
   tree retains both fixes. Re-run CI on the resulting merge candidate.
5. Merge #189 only after review/approval and the integrated checks. Do not release
   the intermediate #187-only state as the complete save-safety fix.

Neither merging nor publishing a release is part of this validation task.

## API references

- [Windows path formats and normalization](https://learn.microsoft.com/dotnet/standard/io/file-path-formats)
- [Windows file identity](https://learn.microsoft.com/windows/win32/api/fileapi/nf-fileapi-getfileinformationbyhandle)
- [ReplaceFile behavior and partial-failure cases](https://learn.microsoft.com/windows/win32/api/winbase/nf-winbase-replacefilew)
