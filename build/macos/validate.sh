#!/usr/bin/env bash
set -euo pipefail
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
"$COMPUTATOR_MONO" --version | tee "$results/runtime.log"
sw_vers >> "$results/runtime.log"
git rev-parse HEAD >> "$results/runtime.log"
run="$results/run-$(date -u +%Y%m%dT%H%M%SZ)-$$"
mkdir -p "$run"
status=0
# Calibration proves the actual selected drawing stack and screen pixels before
# the separate full application and shared acceptance executables are launched.
MONO_LOG_LEVEL=debug MONO_LOG_MASK=dll bash build/macos/probe-xquartz.sh --gui --output "$results/prerequisites" || status=1
application="$repository/Computator.NET/bin/mono/$configuration/net48/Computator.NET.exe"
startup="$repository/build/mono/StartupProbe/bin/mono/$configuration/net48/Computator.NET.Mono.StartupProbe.exe"
smoke="$repository/build/mono/SmokeTests/bin/mono/$configuration/net48/Computator.NET.Mono.SmokeTests.exe"
for executable in "$application" "$startup" "$smoke"; do [[ -f $executable ]] || { echo "Missing built executable: $executable" >&2; exit 2; }; done
"$COMPUTATOR_MCS" -nologo -nowarn:0414 -r:System.Drawing -out:"$validation_temp/LegacyFixture.exe" \
    build/fixtures/LegacySettings.cs build/mono/LegacyFixtureWriter.cs
"$COMPUTATOR_MONO" "$validation_temp/LegacyFixture.exe" "$validation_temp/legacy-settings.dat"
# Each invocation owns a fresh directory. Do not permit old screenshots/reports
# to survive as evidence of a failed current invocation.
COMPUTATOR_DATA_DIRECTORY="$validation_temp/startup" XDG_CONFIG_HOME="$validation_temp/config" XDG_CACHE_HOME="$validation_temp/cache" \
python3 build/macos/run-bounded.py --seconds 150 "$COMPUTATOR_MONO" --debug "$startup" "$application" "$run" \
    2>&1 | tee "$run/startup.log" || status=1
COMPUTATOR_DATA_DIRECTORY="$validation_temp/smoke" COMPUTATOR_MONO_LEGACY_FIXTURE="$validation_temp/legacy-settings.dat" \
XDG_CONFIG_HOME="$validation_temp/config" XDG_CACHE_HOME="$validation_temp/cache" \
python3 build/macos/run-bounded.py --seconds 420 "$COMPUTATOR_MONO" --debug "$smoke" "$run" \
    2>&1 | tee "$run/smoke.log" || status=1
python3 build/macos/verify-acceptance.py "$run" "$architecture" || status=1
native_status=0
python3 build/macos/run-bounded.py --seconds 240 make -C "$native/build-libgdiplus" check \
    2>&1 | tee "$run/native-conformance.log" || native_status=$?
python3 - "$native/build-libgdiplus/tests" "$run/native-conformance.json" "$native_status" <<'PY'
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
[[ $native_status == 0 ]] || status=1
exit "$status"
