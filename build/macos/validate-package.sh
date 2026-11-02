#!/usr/bin/env bash
# Native, relocated acceptance of the actual Mono .app. No offscreen GUI substitute.
set -euo pipefail
configuration=${1:-Release}
[[ $(uname -s) == Darwin && $# -eq 2 && $configuration =~ ^(Release|Debug)$ ]] || {
    echo 'Usage (on the actual Mac): bash build/macos/validate-package.sh Release|Debug ARCHIVE.zip' >&2; exit 2;
}
repository=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)
cd "$repository"
python_command=$(command -v python3)
architecture=$(uname -m)
rid=osx-x64; [[ $architecture != arm64 ]] || rid=osx-arm64
native="$repository/artifacts/macos/native-$architecture"
mono_prefix=$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1]))["monoPrefix"])' "$native/native-manifest.json")
mono="$mono_prefix/bin/mono"
mcs="$mono_prefix/bin/mcs"
archive=$(python3 -c 'import pathlib,sys;print(pathlib.Path(sys.argv[1]).resolve(strict=True))' "$2")
evidence="$repository/artifacts/macos/package/$configuration/acceptance-$rid-$(date -u +%Y%m%dT%H%M%SZ)-$$"
mkdir -p "$evidence"/{harness,profile-startup,profile-smoke,profile-launcher,config,cache}
evidence=$(cd "$evidence" && pwd)
unpack="$evidence/relocated folder with spaces"
python3 build/macos/package.py --extract "$archive" --output "$unpack"
bundle="$unpack/Computator.NET.app"
resources="$bundle/Contents/Resources"
[[ -f $resources/mono-package.json ]] || { echo 'Expected complete Computator.NET.app in the extracted archive.' >&2; exit 2; }
shasum -a 256 "$archive" > "$evidence/archive.sha256"
cp "$resources/mono-package.json" "$evidence/mono-package.json"
python3 build/macos/test_package.py -v 2>&1 | tee "$evidence/archive-contracts.log"
python3 build/macos/test_preflight.py --package-root "$resources" --mono "$mono" -v 2>&1 | tee "$evidence/preflight-contracts.log"
smoke="$repository/build/mono/SmokeTests/bin/mono/$configuration/net48"
startup="$repository/build/mono/StartupProbe/bin/mono/$configuration/net48/Computator.NET.Mono.StartupProbe.exe"
cp "$smoke/Computator.NET.Mono.SmokeTests.exe" "$smoke/Computator.NET.Mono.SmokeTests.exe.config" "$evidence/harness/"
cp -R "$smoke/Localization" "$evidence/harness/Localization"
cp "$startup" "$evidence/harness/Computator.NET.Mono.StartupProbe.exe"
if [[ -f $startup.config ]]; then cp "$startup.config" "$evidence/harness/"; fi
"$mcs" -nologo -nowarn:0414 -r:System.Drawing -out:"$evidence/harness/LegacyFixture.exe" \
    build/fixtures/LegacySettings.cs build/mono/LegacyFixtureWriter.cs
"$mono" "$evidence/harness/LegacyFixture.exe" "$evidence/legacy-settings.dat"
unset COMPUTATOR_GSL_DIRECTORY MONO_CFG_DIR MONO_ENV_OPTIONS MONO_WINFORMS_XIM_STYLE
export MONO_MWF_MAC_FORCE_X11=1 COMPUTATOR_MONO_XIM_ACCEPTANCE=enabled COMPUTATOR_NONINTERACTIVE=1
export FONTCONFIG_FILE="$resources/native/fonts.conf" FONTCONFIG_PATH="$resources/native"
export GIO_MODULE_DIR="$resources/native/gio-modules" GIO_EXTRA_MODULES= XLOCALEDIR=/opt/X11/share/X11/locale
export LIBTHAI_DICTDIR="$resources/native/libthai"
export MONO_PATH="$resources/app:$resources/app/Facades"
export XDG_CONFIG_HOME="$evidence/config" XDG_CACHE_HOME="$evidence/cache"
export COMPUTATOR_MAC_COMPOSITOR_EVIDENCE_DIRECTORY="$evidence/compositor"
if [[ -n ${COMPUTATOR_MAC_DISPLAY_ENV:-} ]]; then source "$COMPUTATOR_MAC_DISPLAY_ENV"; fi
[[ -n ${DISPLAY:-} ]] || { echo 'An authenticated rootless XQuartz display is required.' >&2; exit 2; }
"$mono" "$resources/app/Computator.NET.Mono.Preflight.exe" prepare --package-root "$resources" --config "$evidence/mono.config"
# The capture helper is acceptance-only, outside the installation. Rewrite its
# X11 import to the SAME relocated library selected by the production preflight.
cp "$native/display/libcomputator-mac-display.dylib" "$evidence/harness/libcomputator-mac-display.dylib"
python3 - "$resources" "$evidence" <<'PY'
import hashlib,json,pathlib,shlex,shutil,subprocess,sys,xml.sax.saxutils
resources,evidence=map(pathlib.Path,sys.argv[1:])
manifest=json.loads((resources/'mono-package.json').read_text())
helper=evidence/'harness/libcomputator-mac-display.dylib'
lines=subprocess.check_output(['otool','-L',str(helper)],text=True).splitlines()[1:]
matches=[line.strip().split(' (compatibility version',1)[0] for line in lines if '/libX11.' in line]
if len(matches)!=1: raise SystemExit('Expected exactly one reviewed test-helper X11 dependency.')
subprocess.run(['install_name_tool','-change',matches[0],str(resources/manifest['native']['x11']),str(helper)],check=True)
subprocess.run(['codesign','--force','--sign','-','--timestamp=none',str(helper)],check=True)
subprocess.run(['codesign','--verify','--strict',str(helper)],check=True)
config=evidence/'mono.config'; value=config.read_text()
if value.count('</configuration>')!=1: raise SystemExit('Unexpected process configuration.')
entry='<dllmap dll="computator_mac_display" target='+xml.sax.saxutils.quoteattr(str(helper))+' os="osx" />'
config.write_text(value.replace('</configuration>',entry+'</configuration>'))
(evidence/'capture-helper.json').write_text(json.dumps({'scope':'test-only-outside-bundle','x11':str(resources/manifest['native']['x11']),
    'helper':str(helper),'dependencies':subprocess.check_output(['otool','-L',str(helper)],text=True)},indent=2)+'\n')
tool=pathlib.Path(shutil.which('xdotool') or '').resolve(strict=True)
if not tool.is_file(): raise SystemExit('The external native keyboard acceptance tool is missing.')
bindir=evidence/'harness/bin';bindir.mkdir()
wrapper=bindir/'xdotool'
wrapper.write_text('#!/bin/sh\nunset DYLD_PRINT_LIBRARIES\nexec '+shlex.quote(str(tool))+' "$@"\n')
wrapper.chmod(0o755)
(evidence/'keyboard-tool.json').write_text(json.dumps({'scope':'test-only-outside-application-process',
    'path':str(tool),'sha256':hashlib.sha256(tool.read_bytes()).hexdigest(),
    'wrapper':str(wrapper),'inheritedDyldTracingCleared':True},indent=2)+'\n')
PY
# Never modify a developer's Homebrew installation. On disposable hosted CI,
# hide the original candidates and host font/module data and restore even on red.
if [[ ${GITHUB_ACTIONS:-} == true ]]; then
    state="$evidence/isolated-build-dependencies.json"
    trap '"$python_command" "$repository/build/macos/isolate-build-dependencies.py" restore --state "$state"' EXIT
    python3 build/macos/isolate-build-dependencies.py prepare --state "$state" --native-root "$native" --brew-prefix "$(brew --prefix)"
fi
# Runtime child processes cannot discover the build SDK, Node or mcs via PATH.
# The external test orchestrator uses its already selected absolute Python path.
export PATH="$evidence/harness/bin:/usr/bin:/bin:/usr/sbin:/sbin:/opt/X11/bin"
# Both the ordinary launcher and the shared AppDomains consume the immutable
# relocated payload; generated configs/reports/profile data stay outside it.
chmod -R a-w "$bundle"
"$python_command" build/macos/verify-runtime-data.py "$resources" "$evidence"
COMPUTATOR_MONO="$mono" COMPUTATOR_DATA_DIRECTORY="$evidence/profile-launcher" \
    "$bundle/Contents/MacOS/Computator.NET" --preflight > "$evidence/launcher-preflight.json" 2> "$evidence/launcher-preflight.log"
COMPUTATOR_DATA_DIRECTORY="$evidence/profile-startup" \
"$python_command" build/macos/run-bounded.py --seconds 180 --child-env DYLD_PRINT_LIBRARIES=1 "$mono" --config "$evidence/mono.config" \
    "$evidence/harness/Computator.NET.Mono.StartupProbe.exe" "$resources/app/Computator.NET.exe" "$evidence" \
    2>&1 | tee "$evidence/startup.log"
COMPUTATOR_DATA_DIRECTORY="$evidence/profile-smoke" COMPUTATOR_MONO_LEGACY_FIXTURE="$evidence/legacy-settings.dat" \
COMPUTATOR_MONO_EXPECTED_APPLICATION_DIRECTORY="$resources/app" \
"$python_command" build/macos/run-bounded.py --seconds 480 --child-env DYLD_PRINT_LIBRARIES=1 "$mono" --config "$evidence/mono.config" \
    "$evidence/harness/Computator.NET.Mono.SmokeTests.exe" --application-directory "$resources/app" "$evidence" \
    2>&1 | tee "$evidence/smoke.log"
"$python_command" build/macos/verify-acceptance.py "$evidence" "$architecture"
"$python_command" build/macos/package.py --verify --output "$bundle"
"$python_command" build/macos/verify-package-runtime.py "$resources" "$evidence" "$mono_prefix"
"$python_command" - "$resources" "$evidence" "$archive" <<'PY'
import hashlib,json,pathlib,sys
resources,evidence,archive=map(pathlib.Path,sys.argv[1:])
manifest=json.loads((resources/'mono-package.json').read_text())
acceptance=json.loads((evidence/'acceptance.json').read_text())
digest=hashlib.sha256()
with archive.open('rb') as stream:
    for block in iter(lambda:stream.read(1024*1024),b''): digest.update(block)
reports={name:hashlib.sha256((evidence/name).read_bytes()).hexdigest() for name in
         ('acceptance.json','relocated-native-runtime.json','native-runtime-data.json','launcher-preflight.json','preflight-contracts.log')}
report={'schemaVersion':1,'scope':'accepted-for-physical-mac-testing','rid':manifest['rid'],'source':manifest['source'],
        'archive':{'name':archive.name,'sha256':digest.hexdigest(),'size':archive.stat().st_size},
        'packagedRuntimeAccepted':True,'sharedCases':acceptance['sharedCases'],'evidenceSha256':reports,
        'physicalMacAccepted':False,'ownerSigned':False,'notarized':False}
(evidence/'accepted-candidate.json').write_text(json.dumps(report,indent=2)+'\n')
print('Accepted native archive SHA256:',digest.hexdigest())
PY
echo "Relocated native Mono package acceptance passed: $evidence"
