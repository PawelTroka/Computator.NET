# Real-profile and activation acceptance

Roadmap item 6, after the three developer/UI repairs and five earlier roadmap items. The public regression suite uses synthetic profiles only:

```powershell
./build.ps1 -Target Profile-Validate
```

`ProfileTrial.Prepare(backupDirectory, newTrialDirectory)` is a data-only API for preparing a private manual trial. It reads JSON or the known NRBF settings through the existing safe reader, checks bounded input sizes, rejects directory links/junctions and overlapping destinations, and copies TSL/TSLF documents byte-for-byte. Original settings are retained as `.original`; the only active settings are new JSON with directory preferences rebased into the trial. External working/document roots are refused: collect them into an explicitly prepared backup first. No document is executed by preparation and no logs, unrelated files or private signing material are collected.

Close other writers while copying; this is a local backup helper, not a defense against an attacker racing directory entries. On a failure it leaves an incomplete trial for inspection instead of recursively deleting paths that may have changed. It never changes the source. The returned relative-name/hash inventory is private and must not be uploaded automatically.

Manual acceptance still requires trusted representative historical documents, all three editors, unsaved edits, file save/reopen/restart, old settings conversion and rollback, Unicode paths, uppercase extensions, DPI/multiple monitors and chart appearance. Set `COMPUTATOR_DATA_DIRECTORY` to the successfully prepared trial before starting the application. A data-directory override alone does not rebase old absolute directory preferences; that is why preparation validates and rewrites them.

Shell document detection now compares extensions case-insensitively, preserves spaces/Unicode and never logs document paths. It does not execute files merely to detect their type. Windows Start-menu/Store activation requires its own installed-package acceptance; these tests are not a claim that a real private corpus or signed package has been tested.
