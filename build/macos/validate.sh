#!/usr/bin/env bash
set -euo pipefail
unset COMPUTATOR_GITHUB_TOKEN # Only package authoring reads immutable Git inputs.
configuration=${1:-Release}
if [[ $# -gt 2 || ${2:---no-build} != --no-build || ! $configuration =~ ^(Release|Debug)$ || $(uname -s) != Darwin ]]; then
    echo 'Usage (on the actual Mac): bash build/macos/validate.sh [Release|Debug] [--no-build]' >&2; exit 2
fi
repository=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)
cd "$repository"
architecture=$(uname -m)
native="$repository/artifacts/macos/native-$architecture"
results="$repository/artifacts/macos/acceptance-$architecture/$configuration"
mkdir -p "$results"
results=$(cd "$results" && pwd)
[[ -x $native/bin/mono ]] || { echo 'Prepare the native graph first with Mono-Mac-Prepare.' >&2; exit 2; }
export PATH="$native/bin:/opt/X11/bin:$PATH"
export COMPUTATOR_MONO="$native/bin/mono" COMPUTATOR_MCS="$native/bin/mcs"
export COMPUTATOR_EXPECTED_MAC_ARCHITECTURE="$architecture" MONO_MWF_MAC_FORCE_X11=1
export COMPUTATOR_MONO_XIM_ACCEPTANCE=enabled
if [[ -n ${COMPUTATOR_MAC_DISPLAY_ENV:-} ]]; then source "$COMPUTATOR_MAC_DISPLAY_ENV"; fi
[[ -n ${DISPLAY:-} ]] || { echo 'No authenticated XQuartz DISPLAY. Native drawing without a display is not GUI acceptance.' >&2; exit 2; }
validation_temp=$(mktemp -d "${TMPDIR:-/tmp}/computator-mac-acceptance.XXXXXXXX")
trap 'rm -rf -- "$validation_temp"' EXIT
mkdir -p "$validation_temp/startup" "$validation_temp/smoke" "$validation_temp/config" "$validation_temp/cache"
"$COMPUTATOR_MCS" -nologo -r:System.Drawing -r:System.Windows.Forms -out:"$validation_temp/MacCaptureProcessTests.exe" \
    build/macos/MacDesktopCapture.cs build/macos/MacCaptureProcessTests.cs
"$COMPUTATOR_MONO" "$validation_temp/MacCaptureProcessTests.exe" "$COMPUTATOR_MONO" "$results/capture-process-contracts" \
    2>&1 | tee "$results/capture-process-contracts.log"
"$COMPUTATOR_MCS" -nologo -out:"$validation_temp/XimContractTests.exe" build/macos/MonoXimContract.cs build/macos/MonoXimContractTests.cs
"$COMPUTATOR_MONO" "$validation_temp/XimContractTests.exe" | tee "$results/xim-metadata-contracts.log"
"$COMPUTATOR_MONO" --version | tee "$results/runtime.log"
sw_vers >> "$results/runtime.log"
git rev-parse HEAD >> "$results/runtime.log"
run="$results/run-$(date -u +%Y%m%dT%H%M%SZ)-$$"
mkdir -p "$run"
export COMPUTATOR_MAC_COMPOSITOR_EVIDENCE_DIRECTORY="$run/compositor"
calibration_status=0
# Calibration proves the actual selected drawing stack and screen pixels before
# the separate full application and shared acceptance executables are launched.
COMPUTATOR_MAC_DRAWABLE_EVIDENCE_DIRECTORY="$results/prerequisites" \
MONO_LOG_LEVEL=debug MONO_LOG_MASK=dll bash build/macos/probe-xquartz.sh --gui --output "$results/prerequisites" || calibration_status=$?
application="$repository/Computator.NET/bin/mono/$configuration/net48/Computator.NET.exe"
startup="$repository/build/mono/StartupProbe/bin/mono/$configuration/net48/Computator.NET.Mono.StartupProbe.exe"
smoke="$repository/build/mono/SmokeTests/bin/mono/$configuration/net48/Computator.NET.Mono.SmokeTests.exe"
for executable in "$application" "$startup" "$smoke"; do [[ -f $executable ]] || { echo "Missing built executable: $executable" >&2; exit 2; }; done
"$COMPUTATOR_MCS" -nologo -nowarn:0414 -r:System.Drawing -out:"$validation_temp/LegacyFixture.exe" \
    build/fixtures/LegacySettings.cs build/mono/LegacyFixtureWriter.cs
"$COMPUTATOR_MONO" "$validation_temp/LegacyFixture.exe" "$validation_temp/legacy-settings.dat"
# Each invocation owns a fresh directory. Do not permit old screenshots/reports
# to survive as evidence of a failed current invocation.
startup_status=0
COMPUTATOR_DATA_DIRECTORY="$validation_temp/startup" XDG_CONFIG_HOME="$validation_temp/config" XDG_CACHE_HOME="$validation_temp/cache" \
python3 build/macos/run-bounded.py --seconds 150 "$COMPUTATOR_MONO" --debug "$startup" "$application" "$run" \
    2>&1 | tee "$run/startup.log" || startup_status=$?
smoke_status=0
COMPUTATOR_DATA_DIRECTORY="$validation_temp/smoke" COMPUTATOR_MONO_LEGACY_FIXTURE="$validation_temp/legacy-settings.dat" \
XDG_CONFIG_HOME="$validation_temp/config" XDG_CACHE_HOME="$validation_temp/cache" \
python3 build/macos/run-bounded.py --seconds 420 "$COMPUTATOR_MONO" --debug "$smoke" "$run" \
    2>&1 | tee "$run/smoke.log" || smoke_status=$?
report_status=0
python3 build/macos/verify-acceptance.py "$run" "$architecture" || report_status=$?
native_status=0
python3 build/macos/run-bounded.py --seconds 240 make -C "$native/build-libgdiplus" check \
    2>&1 | tee "$run/native-conformance.log" || native_status=$?
native_report_status=0
python3 - "$native/build-libgdiplus/tests" "$run/native-conformance.json" "$native_status" <<'PY' || native_report_status=$?
import json,pathlib,sys
tests=pathlib.Path(sys.argv[1]); results=[]
for report in sorted(tests.glob('*.trs')):
    lines=report.read_text().splitlines()
    value=next((line.split(':',2)[-1].strip() for line in lines if line.startswith(':test-result:')),None)
    results.append({'name':report.stem,'result':value})
passed=int(sys.argv[3])==0 and len(results)==36 and all(test['result']=='PASS' for test in results)
pathlib.Path(sys.argv[2]).write_text(json.dumps({'schemaVersion':1,'scope':'unchanged-upstream-libgdiplus-6.2-conformance','exitCode':int(sys.argv[3]),'passed':passed,'tests':results},indent=2)+'\n')
if not passed: raise SystemExit('Native libgdiplus conformance did not pass all 36 unchanged upstream tests.')
print('Native libgdiplus conformance: 36 passed, no failures or skips.')
PY
status=0
for component in "$calibration_status" "$startup_status" "$smoke_status" "$report_status" "$native_status" "$native_report_status"; do
    [[ $component == 0 ]] || status=1
done
python3 - "$run" "$calibration_status" "$startup_status" "$smoke_status" "$report_status" "$native_status" "$native_report_status" <<'PY'
import json,pathlib,sys
root=pathlib.Path(sys.argv[1])
stages=dict(zip(('calibration','startup','sharedSuite','applicationReport','nativeTests','nativeReport'),map(int,sys.argv[2:])))
passed=all(value==0 for value in stages.values())
result={'schemaVersion':1,'scope':'native-macos-source-validation-process-exit-codes','passed':passed,'stages':stages}
(root/'stage-exit-codes.json').write_text(json.dumps(result,indent=2)+'\n')
application=root/'acceptance.json'
if application.is_file():
    report=json.loads(application.read_text())
    report['applicationAssertionsPassed']=report.get('passed') is True
    report['stageExitCodes']=stages
    report['passed']=report['applicationAssertionsPassed'] and passed
    application.write_text(json.dumps(report,indent=2)+'\n')
print('Native source validation exit codes:',json.dumps(result,sort_keys=True),flush=True)
PY
exit "$status"
