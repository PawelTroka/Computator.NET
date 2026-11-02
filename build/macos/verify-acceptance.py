"""Reject missing, dropped, skipped or stale application acceptance evidence."""
import json
from pathlib import Path
import re
import subprocess
import sys
import xml.etree.ElementTree as ET

output = Path(sys.argv[1]).resolve()
architecture = sys.argv[2]
repository = Path(__file__).resolve().parents[2]
source = "\n".join(path.read_text(encoding="utf-8-sig") for path in (repository / "build/mono/SmokeTests").glob("*.cs"))
expected = re.findall(r'\bRun\("([^"+]*)"\s*,', source)
catalog = (repository / "Computator.NET.Core/NumericalCalculations/NumericalMethodsInfo.cs").read_text(encoding="utf-8-sig")
for owner, prefix, count in (("Integral", "integration", 8), ("Derivative", "derivative", 5), ("FunctionRoot", "root", 5)):
    methods = re.findall(r'\b' + owner + r'\.([A-Za-z0-9_]+)', catalog)
    if len(methods) != count:
        raise SystemExit(f"Unexpected {owner} catalog; review native acceptance discovery before release.")
    expected.extend(f"desktop-{prefix}-{name}" for name in methods)
if len(expected) < 53:
    raise SystemExit("The complete baseline 53-case shared Mono suite was not discovered.")
report = ET.parse(output / "mono-results.xml").getroot()
cases = report.findall("testcase")
observed = [case.get("name") for case in cases]
if len(expected) != len(set(expected)) or len(observed) != len(set(observed)) or set(expected) != set(observed):
    raise SystemExit(f"Incomplete shared suite: expected {len(expected)}, actual {len(observed)}; "
                     f"missing {set(expected)-set(observed)}, unexpected {set(observed)-set(expected)}")
if any(case.find("failure") is not None or case.find("skipped") is not None for case in cases):
    raise SystemExit("Every shared Mono acceptance case must pass on this native Mac; failures/skips are not release evidence.")
startup = (output / "startup.log").read_text()
if "PASS production executable startup, dependency loading, rendering and graceful close" not in startup:
    raise SystemExit("No successful actual application entry-point acceptance.")
for image in ("startup.png", "production-complex-after.png", "desktop-workflow.png",
              "real-chart.png", "complex-chart.png", "mono-3d-visible-rotated.png",
              "mono-3d-panned-export.png", "autocomplete-after.png"):
    if not (output / image).is_file():
        raise SystemExit(f"Missing current visible/export evidence: {image}")
manifest = {"schemaVersion": 1, "scope": "native-macos-mono-winforms-xquartz-application-acceptance",
            "checkoutCommit": subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=repository, text=True).strip(),
            "checkoutDirty": bool(subprocess.check_output(["git", "status", "--porcelain", "--untracked-files=no"], cwd=repository, text=True).strip()),
            "architecture": architecture, "sharedCases": len(cases), "passed": True,
            "physicalMacAcceptance": False, "signingNotarization": False,
            "cases": observed}
(output / "acceptance.json").write_text(json.dumps(manifest, indent=2) + "\n")
print(f"Verified all {len(cases)} shared application cases and actual native XQuartz startup on {architecture}.")
