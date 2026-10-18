#!/usr/bin/env python3
"""Verify preserved evidence; optionally reproduce redactions from the pinned ZIP.

This reads files only. It never installs a package or invokes an acceptance test.
Python 3.9+; standard library only. See README.md for the provenance boundary.
"""

import argparse
import collections
import hashlib
import io
import json
import re
import zipfile
from pathlib import Path


ARTIFACT_SHA256 = "1b198a1714a06c942c5a6e40fa49da7c235127857ab31ebde3859688441aa302"
ARTIFACT_BYTES = 382990
LOG_PREFIX = "installer/logs/historical/"
PRIVATE_PROPERTIES = {
    "ComputerName": "<HOSTNAME>",
    "LogonUser": "<USER>",
    "UserSID": "<USER_SID>",
    "MsiRestartManagerSessionKey": "<RESTART_MANAGER_KEY>",
    "ARPCONTACT": "<CONTACT_EMAIL>",
    "ARPHELPTELEPHONE": "<CONTACT_PHONE>",
}
TELEMETRY = (
    "PhysicalMemory|VirtualMemory|PrimaryVolumeSpaceAvailable|"
    "PrimaryVolumeSpaceRemaining|ScreenX|ScreenY|ColorBits|BorderTop|"
    "BorderSide|CaptionHeight|TextHeight|TextInternalLeading"
)


def require(condition, message):
    if not condition:
        raise ValueError(message)


def sha256(data):
    return hashlib.sha256(data).hexdigest()


def decode(data):
    encoding = "utf-16" if data[:2] in (b"\xff\xfe", b"\xfe\xff") else "utf-8-sig"
    return data.decode(encoding).replace("\r\n", "\n").replace("\r", "\n")


def json_bytes(value):
    return (json.dumps(value, indent=2, ensure_ascii=False) + "\n").encode("utf-8")


class Redactor:
    """Learn private values in memory; never serialize the value-to-alias map."""

    def __init__(self, texts, report):
        private = {report["machine"]: ("<HOSTNAME>", "private_ComputerName")}
        for text in texts:
            for name, alias in PRIVATE_PROPERTIES.items():
                # Nested uninstall logs use Property(N) as well as Property(C/S).
                values = re.findall(
                    rf"(?m)^Property\([A-Za-z]\): {re.escape(name)} = (.+)$", text
                )
                for tail in re.findall(
                    rf"PROPERTY CHANGE: (?:Adding|Modifying) {re.escape(name)} property\.([^\n]*)",
                    text,
                ):
                    values.extend(re.findall(r"(?:value is|new value:) '([^']+)'", tail))
                for value in values:
                    if value.strip():
                        private[value] = (alias, "private_" + name)
        self.private = sorted(private.items(), key=lambda item: (-len(item[0]), item[0]))

    def transform(self, text):
        counts = collections.Counter()

        def sub(name, pattern, replacement, flags=0):
            nonlocal text
            text, count = re.subn(pattern, replacement, text, flags=flags)
            if count:
                counts[name] += count

        sub("burn_ipc_secret", r"(-burn\.(?:embedded|elevated)\s+)\S+\s+\S+(?=\s+\d+)",
            r"\1<BURN_PIPE> <BURN_SECRET>", re.I)
        sub("workspace", r"[A-Z]:\\a\\Computator\.NET\\Computator\.NET(?=\\|\b)",
            lambda m: "<WORKSPACE>", re.I)
        sub("user_temp", r"[A-Z]:\\Users\\[^\\\s\"']+\\AppData\\Local\\Temp(?=\\|\b)",
            lambda m: "%TEMP%", re.I)
        sub("user_profile", r"[A-Z]:\\Users\\(?!Public(?:\\|\b)|Default(?:\\|\b))[^\\\s\"',;)]+",
            lambda m: "%USERPROFILE%", re.I)
        sub("acceptance_temp_id", r"Computator-Historical-[0-9a-f]{32}",
            "Computator-Historical-<RUN_ID>", re.I)
        sub("offline_plan_id", r"(artifacts\\clean-machine\\)[0-9a-f]{32}",
            r"\1<PLAN_ID>", re.I)
        sub("burn_temp_id", r"((?:%TEMP%|[A-Z]:\\Windows\\Temp)\\)\{[0-9a-f-]{36}\}",
            r"\1<BURN_WORKDIR>", re.I)
        for value, (alias, name) in self.private:
            sub(name, r"(?<![\w])" + re.escape(value) + r"(?![\w])", lambda m, a=alias: a, re.I)
        sub("machine_account_sid", r"S-1-5-21-(?:\d+-){2,3}\d+", "<USER_SID>", re.I)
        sub("contact_email", r"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", "<CONTACT_EMAIL>", re.I)
        sub("host_telemetry", rf"(?m)^(Property\([A-Za-z]\): (?:{TELEMETRY}) = )[^\n]*$",
            r"\1<HOST_TELEMETRY>")
        sub("host_telemetry_change",
            rf"(?m)^(.*PROPERTY CHANGE: (?:Adding|Modifying) (?:{TELEMETRY}) property\.).*$",
            r"\1 <HOST_TELEMETRY>")
        return text, dict(sorted(counts.items()))

    def json(self, value):
        if isinstance(value, str):
            return self.transform(value)[0]
        if isinstance(value, list):
            return [self.json(item) for item in value]
        if isinstance(value, dict):
            return {key: self.json(item) for key, item in value.items()}
        return value


def load_original(path):
    data = path.read_bytes()
    require(len(data) == ARTIFACT_BYTES, "Original artifact byte length mismatch")
    require(sha256(data) == ARTIFACT_SHA256, "Original artifact SHA-256 mismatch")
    archive = zipfile.ZipFile(io.BytesIO(data))
    require(archive.testzip() is None, "Original artifact CRC failure")
    names = archive.namelist()
    require(len(names) == len(set(names)), "Duplicate original ZIP member")
    files = {name: archive.read(name) for name in names if not name.endswith("/")}
    texts = [decode(data) for name, data in files.items() if name.endswith((".log", ".json"))]
    report = json.loads(decode(files[LOG_PREFIX + "acceptance.json"]))
    return files, Redactor(texts, report), report


def verify(root, original_path=None):
    # Keep referenced parent documents inside docs/, not an arbitrary filesystem path.
    boundary = root.parent.parent.resolve()
    checksum_count = 0
    for row in (root / "SHA256SUMS").read_text(encoding="utf-8").splitlines():
        expected, name = row.split("  ", 1)
        path = (root / name).resolve()
        require(path.is_relative_to(boundary), "Checksum path leaves docs/")
        require(sha256(path.read_bytes()) == expected, f"Checksum mismatch: {name}")
        checksum_count += 1
    manifest = json.loads((root / "manifest.json").read_text(encoding="utf-8"))
    sources = manifest["members"]
    with zipfile.ZipFile(root / "installer-logs.zip") as archive:
        require(archive.testzip() is None, "Preserved log ZIP CRC failure")
        log_entries = [entry for entry in sources if entry.get("archiveMember")]
        expected_members = {entry["archiveMember"] for entry in log_entries}
        require(len(archive.namelist()) == len(expected_members), "Unexpected or duplicate preserved log")
        require(set(archive.namelist()) == expected_members, "Preserved log membership mismatch")
        for entry in log_entries:
            data = archive.read(entry["archiveMember"])
            require(sha256(data) == entry["retainedSha256"], "Preserved log SHA-256 mismatch")
            require(len(data) == entry["retainedBytes"], "Preserved log length mismatch")
            require(len(data.decode("utf-8").splitlines()) == entry["lines"], "Preserved log line-count mismatch")
        failed = archive.read("failed-upgrade.log").decode("utf-8").splitlines()
        require("ComputatorAcceptanceFailure. Return value 3." in failed[2028], "Failure marker changed")
        require("Executing op: End(" in failed[2609], "Rollback End marker changed")
        require("Error in rollback skipped.\tReturn: 5" in failed[2610], "Diagnostic changed")
        if original_path:
            files, redactor, report = load_original(original_path)
            require(set(files) == {entry["path"] for entry in sources}, "Original member inventory mismatch")
            for entry in sources:
                raw = files[entry["path"]]
                require(sha256(raw) == entry["sha256"], "Original member SHA-256 mismatch")
                require(len(raw) == entry["bytes"], "Original member length mismatch")
                if entry.get("archiveMember"):
                    original_text = decode(raw)
                    clean, counts = redactor.transform(original_text)
                    require(clean.encode("utf-8") == archive.read(entry["archiveMember"]), "Log redaction reproduction mismatch")
                    require(counts == entry["redactions"], "Log redaction counts changed")
                    require(len(original_text.splitlines()) == entry["lines"], "Original log line numbers changed")
                elif entry.get("transform") == "normalized-json":
                    data = json_bytes(json.loads(decode(raw)))
                    require(data == (root / entry["retainedPath"]).read_bytes(), "JSON reproduction mismatch")
                elif entry.get("transform") == "deduplicated-stdout":
                    data = redactor.transform(decode(raw))[0].encode("utf-8")
                    require(data == (root / entry["retainedPath"]).read_bytes(), "Stdout reproduction mismatch")
            clean_report = json_bytes(redactor.json(report))
            require(clean_report == (root.parent / "historical-upgrade-2026-10-16.json").read_bytes(),
                    "Acceptance report redaction reproduction mismatch")
            references = json.loads((root / "source-references.json").read_text(encoding="utf-8"))
            with zipfile.ZipFile(io.BytesIO(files["historical-review/harness-source.zip"])) as source_zip:
                members = {name for name in source_zip.namelist() if not name.endswith("/")}
                require(members == {entry["path"] for entry in references["files"]}, "Source inventory mismatch")
                for entry in references["files"]:
                    raw = source_zip.read(entry["path"])
                    git = raw.replace(b"\r\n", b"\n")
                    require(sha256(raw) == entry["archiveSha256"], "Archived source hash mismatch")
                    require(sha256(git) == entry["gitSha256"], "Git source hash mismatch")
                    header = f"blob {len(git)}\0".encode("ascii")
                    require(hashlib.sha1(header + git).hexdigest() == entry["gitBlob"], "Git blob identity mismatch")
                    require(git.replace(b"\n", b"\r\n") == raw, "Source line-ending reconstruction mismatch")
    print(f"Verified {checksum_count} file checksums and {len(log_entries)} complete installer logs.")
    if original_path:
        print("Verified original artifact SHA-256/CRCs, all 29 members, redactions, report and all 35 source references.")
    print("Evidence verification only; no installer or acceptance run was executed.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source-zip", type=Path, help="Optional original artifact ZIP, never extracted or executed")
    args = parser.parse_args()
    verify(Path(__file__).resolve().parent, args.source_zip)
