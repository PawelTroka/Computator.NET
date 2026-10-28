#!/usr/bin/env python3
"""Prepare the reviewed PR #294 + browser 2D combination for disposable CI.

This deliberately targets one immutable WebGL implementation and one reviewed
Chart.tsx bridge. It changes the checkout and index only: no commits, branch
updates, merges or pushes. Source drift requires an explicitly reviewed bridge.
"""

from __future__ import annotations

import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys


BASE = "fe1aba49b5f9538355ffaaa3d1683f69a23d2d9c"
WEBGL_HEAD = "9fc1b7949fab5dab4cd3ce04cfef2bf944511b01"
CHART = "Computator.NET.WebClient/ClientApp/components/Chart.tsx"
# The bridge is reviewed against this 2D source, not arbitrary future edits.
CHART_SOURCE = "489bc60d6ab928a4e3f703ccec3385c28165ca6b"
BRIDGE = "build/compatibility/webgl-chart-bridge.patch"
SWITCHING_TEST = "build/compatibility/chart-renderer-switching.spec.ts"
SWITCHING_TARGET = "Computator.NET.WebClient/e2e/chart-renderer-switching.spec.ts"
ROOT = Path(__file__).resolve().parents[2]


def git(*arguments: str, input_bytes: bytes | None = None) -> bytes:
    result = subprocess.run(
        ["git", *arguments], cwd=ROOT, input=input_bytes,
        stdout=subprocess.PIPE, stderr=subprocess.PIPE, check=False,
    )
    if result.returncode:
        details = (result.stdout + result.stderr).decode("utf-8", errors="replace")
        raise RuntimeError(f"git {' '.join(arguments)} failed:\n{details}")
    return result.stdout


def text(*arguments: str) -> str:
    return git(*arguments).decode("utf-8").strip()


def require_commit(sha: str) -> None:
    found = subprocess.run(
        ["git", "cat-file", "-e", sha + "^{commit}"], cwd=ROOT,
        stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, check=False,
    )
    if found.returncode:
        # Fetch objects by immutable SHA without updating local or remote refs.
        git("fetch", "--filter=blob:none", "--no-tags", "--depth=1",
            "--no-write-fetch-head", "origin", sha)
    if text("rev-parse", sha + "^{commit}") != sha:
        raise RuntimeError(f"Unexpected object identity for {sha}.")


def main() -> None:
    if text("status", "--porcelain=v1", "--untracked-files=normal"):
        raise RuntimeError("Compatibility preparation requires a clean disposable checkout and index.")
    original = text("rev-parse", "HEAD")
    if text("rev-parse", ":" + CHART) != CHART_SOURCE:
        raise RuntimeError("Chart.tsx has drifted from the reviewed 2D bridge. Review and update the bridge and its source fingerprint.")
    bridge = ROOT / BRIDGE
    if not bridge.is_file():
        raise RuntimeError(f"The reviewed bridge is missing: {BRIDGE}")
    switching_source, switching_target = ROOT / SWITCHING_TEST, ROOT / SWITCHING_TARGET
    if not switching_source.is_file() or switching_target.exists():
        raise RuntimeError("The dedicated renderer-switching test source is missing or its destination already exists.")

    require_commit(BASE)
    require_commit(WEBGL_HEAD)
    patch = git("diff", "--binary", "--full-index", BASE, WEBGL_HEAD)
    if not patch:
        raise RuntimeError("The immutable WebGL source diff is empty.")
    git("apply", "--3way", "--index", "--exclude=" + CHART,
        "--exclude=Computator.NET.slnx", input_bytes=patch)
    git("apply", "--index", input_bytes=bridge.read_bytes())
    # The switching scenario is meaningful only in this combined candidate;
    # standalone 2D CI must not add an expected missing-3D skip.
    switching_target.write_bytes(switching_source.read_bytes())
    git("add", "--", SWITCHING_TARGET)

    solution = ROOT / "Computator.NET.slnx"
    content = solution.read_text(encoding="utf-8")
    anchor = '    <File Path="docs/followups/WEB-2D-INTERACTIONS.md" />'
    addition = '    <File Path="docs/followups/WEB-3D-SURFACES.md" />'
    if content.count(anchor) != 1 or "docs/followups/WEB-3D-SURFACES.md" in content:
        raise RuntimeError("Unexpected solution document inventory; refusing an ambiguous or duplicate insertion.")
    solution.write_text(content.replace(anchor, anchor + "\n" + addition), encoding="utf-8", newline="\n")
    git("add", "--", "Computator.NET.slnx")
    git("diff", "--cached", "--check")
    if text("ls-files", "--unmerged"):
        raise RuntimeError("The prepared checkout contains unresolved source conflicts.")
    if text("rev-parse", "HEAD") != original:
        raise RuntimeError("The preparation unexpectedly changed HEAD.")

    identifiers = {
        "original_2d_head": original,
        "webgl_294_head": WEBGL_HEAD,
        "webgl_294_base": BASE,
        "prepared_tree": text("write-tree"),
        "chart_bridge_source_blob": CHART_SOURCE,
        "chart_bridge_sha256": hashlib.sha256(bridge.read_bytes()).hexdigest(),
        "staged_files": text("diff", "--cached", "--name-only").splitlines(),
    }
    output = ROOT / "artifacts/compatibility/webgl-294.json"
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(identifiers, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(identifiers, indent=2), flush=True)
    summary = os.environ.get("GITHUB_STEP_SUMMARY")
    if summary:
        with open(summary, "a", encoding="utf-8") as handle:
            handle.write("## Temporary Web chart compatibility candidate\n\n")
            handle.write(f"- 2D source: `{original}`\n- PR #294 source: `{WEBGL_HEAD}`\n")
            handle.write(f"- Prepared source tree: `{identifiers['prepared_tree']}`\n\n")
            handle.write("Preparation changes only this disposable checkout and index. Runtime results are reported by the following Windows browser/API step.\n")


if __name__ == "__main__":
    try:
        main()
    except (OSError, RuntimeError) as error:
        print(f"Compatibility preparation failed: {error}", file=sys.stderr)
        sys.exit(1)
