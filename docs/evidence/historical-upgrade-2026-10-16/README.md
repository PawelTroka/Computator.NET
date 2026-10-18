# Preserved historical installer acceptance evidence

This directory preserves the existing Windows acceptance evidence for [PR #174](https://github.com/PawelTroka/Computator.NET/pull/174). The accepted execution is [run 38906708104](https://github.com/PawelTroka/Computator.NET/actions/runs/38906708104), using harness commit [`d852eaa7a683a4bf084b75af281ff9aad762f09c`](https://github.com/PawelTroka/Computator.NET/tree/d852eaa7a683a4bf084b75af281ff9aad762f09c). Preservation and analysis took place on **2026-10-17**.

The [acceptance documentation](../../HISTORICAL-UPGRADE-ACCEPTANCE.md) remains the lifecycle overview. The [existing structured report](../historical-upgrade-2026-10-16.json) retains every result field, with private machine identifiers and user paths replaced by aliases. The [rollback investigation](ROLLBACK-DIAGNOSTIC.md) explains the diagnostic and the precise scope supported by the logs.

## Original artifact verification

| Field | Verified value |
| --- | --- |
| Repository | `PawelTroka/Computator.NET` |
| Artifact | `historical-upgrade-evidence`, ID `12048572484` |
| Workflow run | `38906708104`, job `116991972175` (`lifecycle`, successful) |
| Downloaded archive size | **382,990 bytes** |
| Original SHA-256 | `1b198a1714a06c942c5a6e40fa49da7c235127857ab31ebde3859688441aa302` |
| GitHub expiry | **2026-10-30 13:20:52 UTC**; `expired: false` when retrieved |
| Verification | Downloaded size and SHA-256 matched GitHub metadata; every ZIP member passed CRC verification. |
| File inventory | **29 original files**, each inventoried by its original size and SHA-256. |

The original digest verifies the downloaded archive against GitHub's recorded digest. It is not a publisher signature. [manifest.json](manifest.json) retains the artifact metadata, raw member hashes, retention decisions, sanitized member hashes and redaction counts. It contains no signed download URL or private-value substitution table.

The durable evidence is committed Git content; its availability does not depend on the expiring Actions artifact. The raw archive is not republished because it includes machine metadata and a Burn interprocess secret. Sanitized files have their own hashes and are not represented as original bytes.

## Retained evidence

### Complete installer traces

[installer-logs.zip](installer-logs.zip) contains all **nine complete installer logs**, totaling **20,867 lines**, decoded to UTF-8 with LF endings and sanitized. No log lines were removed, reordered, or inserted. References in the diagnostic document use these same line numbers.

| Phase | Members inside `installer-logs.zip` |
| --- | --- |
| Authentic historical EXE installation | `historical-bundle-install.log`; `historical-bundle-install_000_Computator.NET.msi.log` |
| Deliberately failed MSI upgrade and rollback | `failed-upgrade.log` |
| Unmodified candidate EXE upgrade | `candidate-bundle-upgrade.log`; `candidate-bundle-upgrade_000_Computator.NET.msi.log`; `candidate-bundle-upgrade_001_{252e03f2-85db-4a38-a29c-14e31af5aae1}.log` (historical Burn removal) |
| Candidate MSI repair | `candidate-repair.log` |
| Candidate EXE uninstall | `candidate-bundle-uninstall.log`; `candidate-bundle-uninstall_000_Computator.NET.msi.log` |

These logs retain the complete operational context, including the intentional failure, internal diagnostic notes, restoration operations, and final engine results. The diagnostic at `failed-upgrade.log:2611` remains visible.

### Results and provenance

| File | Purpose |
| --- | --- |
| [Existing acceptance report](../historical-upgrade-2026-10-16.json) | Actual execution status, completed stages, package/harness hashes, identities, payload counts, six final user-data hashes and cleanup outcome. Only private identifiers and paths were changed during preservation. |
| [candidate-provenance.json](candidate-provenance.json) | Original candidate artifact/run, source commit and tree, tested harness commit, and candidate archive hash. |
| [prepared-plan.json](prepared-plan.json) | Actual offline plan and ten input-to-hash mappings. **Preparation only: the offline Sandbox was not launched.** |
| [settings-fixture-results.log](settings-fixture-results.log) | One sanitized copy of the identical settings-verification output from three original stdout members. All three source-member identities/hashes remain in the manifest. |
| [workflow-result-excerpts.json](workflow-result-excerpts.json) | Selected output from the existing successful job: explicit synthetic-contract qualifications, real COM checks in both PowerShell versions, lifecycle success, and final plan-only preparation. This supplementary file was retrieved from job logs, not from the artifact ZIP. |
| [source-references.json](source-references.json) | All 35 archived harness-source files mapped to immutable Git blobs with original/Git SHA-256 values and exact line-ending comparisons. Also records the original report comparison. |
| [manifest.json](manifest.json) | Complete original-member inventory, selection rationale, original and retained hashes, artifact metadata, and preservation limits. |
| [SHA256SUMS](SHA256SUMS) and [verify.py](verify.py) | Checksums and read-only verification, including optional reproduction of redactions from the pinned original archive. |

The supplementary job excerpts have source line numbers and a SHA-256 of the connector-decoded UTF-8/LF job log. That hash is explicitly **not** a checksum of an original-byte GitHub download endpoint. The full job setup/environment and repeated copies of artifact log tails are omitted.

## Evidence already in Git and omitted duplicates

The original artifact's `acceptance.json` is **semantically identical** to the report already committed at [`d9fb572f96a267c1033c71bf936efa18f298e33e`](https://github.com/PawelTroka/Computator.NET/tree/d9fb572f96a267c1033c71bf936efa18f298e33e). Differences in the original bytes are BOM, line endings and JSON formatting. There were no missing or changed result fields. This preservation updates that existing report with aliases instead of creating another report copy. The previous report's immutable commit/hash remains recorded for provenance; this change does not rewrite repository history.

All **35** files in `historical-review/harness-source.zip` match the tested Git commit after CRLF-to-LF conversion, with no other changes. Every original individual source file is exactly reconstructable by converting its pinned Git bytes from LF to CRLF, including any BOM. The ZIP container's metadata is not needed or claimed to be reconstructable. The source archive is therefore represented by pinned Git references and file hashes.

The remaining omissions are **13 zero-byte stdout/stderr files**. The three nonempty stdout files have identical bytes and are retained once, with all three original mappings. No installer packages, caches, extracted application payloads, private user-data contents or generated temporary trees were added. Those installer packages and the candidate `payload.xml` were not present in this source evidence archive.

## Privacy and reproducibility

The transformation code is visible in `verify.py` and can be replayed against the pinned source archive. It learns private property values in memory across all logs, including nested `Property(N)` records, so the same values are also removed from registry-operation records. It does not serialize original private values.

| Data | Treatment |
| --- | --- |
| Runner hostname, account and machine-specific user SID | Replaced by role aliases; the well-known SYSTEM SID remains for execution-context interpretation. |
| User profile, workspace and temporary directories | Replaced by `%USERPROFILE%`, `%TEMP%`, `<WORKSPACE>` and temporary-workdir aliases. The x86/x64 installation paths and Unicode fixture filenames remain. |
| Restart Manager session values | Redacted wherever they occur. |
| Burn embedded pipe and interprocess secret | Both arguments redacted; command mode and process correlation retained. |
| Email and telephone contact metadata | Redacted in property dumps and registry/product operations. |
| Memory, available-disk and display telemetry | Values redacted; property names retained. |
| Installer actions and evidence | Timestamps, PID/thread correlation, action order, return codes, product/upgrade/package identities, hashes and diagnostic text retained. |

Both automated scans and an independent review checked the outgoing files and all nine log members for the original private values and credential patterns. The original source files were compared with the sanitized traces to confirm unchanged line counts, action sequences, product identities, and hash literals.

### Verify the committed evidence

From this directory, using Python 3.9 or later:

```sh
python verify.py
```

This checks the file checksum list, ZIP CRCs and exact membership, all nine retained log hashes, line counts, and the failure/rollback diagnostic anchors. The scoped `.gitattributes` keeps evidence text as LF on Windows so checksum verification also works there. Checksums provide a consistency check; the Git commit remains the identity of the reviewed evidence set.

If the original downloaded ZIP is still available, also verify its size, hash, CRCs, all 29 original member hashes, reproducible redactions, existing report and the 35 source references:

```sh
python verify.py --source-zip /path/to/historical-upgrade-evidence-12048572484.zip
```

The script reads evidence files only. It does not extract executable packages, invoke a workflow, run an installer or alter a machine.

## Interpretation limits

- The rollback diagnostic is classified as **nonfatal in this observed intentional-failure run, with no demonstrated application recovery defect**. Its exact internal return-code namespace remains unproven; it is neither suppressed nor given a blanket harmless-message exemption. See the [full investigation](ROLLBACK-DIAGNOSTIC.md).
- The log independently shows **49 historical file removals and 49 historical file-restoration copy operations**. The accepted harness additionally compared all 49 historical file hashes. That per-file hash baseline existed only in memory and was not exported, so this archive cannot supply the 49 original hash values.
- The later successful candidate installation checked **569 payload hashes** against `payload.xml`. This artifact identifies that inventory by hash but does not contain `payload.xml` or its 569 individual values.
- Rollback checked **five pre-existing user-data files**. The successful candidate later created `settings.json`; **six final user-data hashes** were checked through repair and uninstall.
- The deliberate Type 19 error occurs after the `InstallFiles` sequence entry but **before execution of the parent candidate installation script**. Genuine historical removal and recovery occurred; rollback after a partially written candidate payload was not exercised.
- The successful run used an SDK-equipped hosted Windows runner. The offline no-SDK plan was prepared, not executed. Other failure modes and the existing release gates remain as described in the acceptance documentation.

This follow-up preserves and analyzes existing evidence. It does not change application code, acceptance harnesses, installer authoring, validated package bytes, or the approval state of a release.
