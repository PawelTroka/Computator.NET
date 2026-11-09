#!/usr/bin/env bash
# Build-machine acceptance of the actual extracted runtime-dependent package.
set -euo pipefail
if [[ $# != 3 ]]; then echo 'Usage: validate-package.sh ARCHIVE NEW-UNPACK-DIRECTORY NEW-EVIDENCE-DIRECTORY' >&2; exit 2; fi
repository=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
archive=$(realpath "$1")
unpacked=$(realpath -m "$2")
evidence=$(realpath -m "$3")
if [[ -e "$unpacked" || -e "$evidence" || "$unpacked" == "$evidence" || "$unpacked" == "$evidence/"* || "$evidence" == "$unpacked/"* ]]; then
    echo 'Unpacked package and evidence must be distinct new directories.' >&2; exit 2
fi
configuration=${COMPUTATOR_MONO_CONFIGURATION:-Release}
cd "$repository"
for command in mono mcs cc pkg-config python3 timeout xvfb-run xauth xdotool; do command -v "$command" >/dev/null || { echo "Missing acceptance tool: $command" >&2; exit 2; }; done
python3 build/mono/package.py --extract "$archive" --output "$unpacked"
python3 build/mono/test_preflight.py --package-root "$unpacked" -v
mkdir -p "$evidence"/{profile-startup,profile-smoke,profile-launcher,config,cache,harness}
sha256sum "$archive" > "$evidence/archive.sha256"
cp "$unpacked/mono-package.json" "$evidence/mono-package.json"
smoke=$repository/build/mono/SmokeTests/bin/mono/$configuration/net48
startup=$repository/build/mono/StartupProbe/bin/mono/$configuration/net48/Computator.NET.Mono.StartupProbe.exe
cp "$smoke/Computator.NET.Mono.SmokeTests.exe" "$smoke/Computator.NET.Mono.SmokeTests.exe.config" "$evidence/harness/"
cp -R "$smoke/Localization" "$evidence/harness/Localization"
# The harness carries no production DLLs. Its packaged AppDomain uses the real
# installed application directory for assemblies and all content.
cp "$startup" "$evidence/harness/Computator.NET.Mono.StartupProbe.exe"
if [[ -f "$startup.config" ]]; then cp "$startup.config" "$evidence/harness/"; fi
# Legacy data is a checked test fixture, generated before the runtime-only phase.
mcs -nologo -nowarn:0414 -r:System.Drawing -out:"$evidence/harness/LegacyFixture.exe" \
    build/fixtures/LegacySettings.cs build/mono/LegacyFixtureWriter.cs
mono "$evidence/harness/LegacyFixture.exe" "$evidence/legacy-settings.dat"
mcs -nologo -r:System.Drawing -r:System.Runtime.Serialization \
    -out:"$evidence/harness/Computator.NET.Mono.DesktopMenuProbe.exe" \
    build/mono/DesktopMenuProbe.cs build/mono/NativeToolProcess.cs
# Shared external XRes helper; it is never installed with the application.
read -r -a x11_flags <<< "$(pkg-config --cflags --libs xres x11)"
cc -std=c11 -Wall -Wextra -Werror build/macos/normal-launch-x11.c \
    "${x11_flags[@]}" -o "$evidence/harness/normal-launch-x11"
mcs -nologo -out:"$evidence/harness/NativeToolProcessTests.exe" \
    build/mono/NativeToolProcess.cs build/mono/NativeToolProcessTests.cs
preflight=$unpacked/app/Computator.NET.Mono.Preflight.exe
mono "$preflight" prepare --package-root "$unpacked" --config "$evidence/mono.config"
unset COMPUTATOR_GSL_DIRECTORY MONO_CFG_DIR MONO_ENV_OPTIONS
export LD_LIBRARY_PATH="$unpacked/native/linux-x64/gsl:$unpacked/native/linux-x64/gdiplus"
export MONO_PATH="$unpacked/app/Facades:$unpacked/app"
export XDG_CONFIG_HOME="$evidence/config" XDG_CACHE_HOME="$evidence/cache"
# Deliberately exclude all dotnet/Node/private build-tool directories from runtime
# execution. The clean runtime-only CI container separately proves package deps.
export PATH=/usr/bin:/bin
mono_runtime=$(command -v mono)
timeout --kill-after=5s 30s "$mono_runtime" "$evidence/harness/NativeToolProcessTests.exe" \
    "$mono_runtime" "$evidence/native-tool-process" 2>&1 | tee "$evidence/native-tool-process-contracts.log"
COMPUTATOR_DATA_DIRECTORY="$evidence/profile-launcher" \
    timeout --kill-after=10s 120s xvfb-run -a -s '-screen 0 1440x1000x24' \
    "$unpacked/bin/computator" --check 2>&1 | tee "$evidence/launcher-check.log"
COMPUTATOR_DATA_DIRECTORY="$evidence/profile-startup" \
    timeout --kill-after=10s 120s xvfb-run -a -s '-screen 0 1440x1000x24' \
    mono --config "$evidence/mono.config" "$evidence/harness/Computator.NET.Mono.StartupProbe.exe" \
    "$unpacked/app/Computator.NET.exe" "$evidence" 2>&1 | tee "$evidence/startup.log"
COMPUTATOR_DATA_DIRECTORY="$evidence/profile-smoke" \
COMPUTATOR_MONO_LEGACY_FIXTURE="$evidence/legacy-settings.dat" \
COMPUTATOR_MONO_EXPECTED_APPLICATION_DIRECTORY="$unpacked/app" \
    timeout --kill-after=10s 300s xvfb-run -a -s '-screen 0 1440x1000x24' \
    mono --config "$evidence/mono.config" "$evidence/harness/Computator.NET.Mono.SmokeTests.exe" \
    --application-directory "$unpacked/app" "$evidence" 2>&1 | tee "$evidence/smoke.log"
python3 "$repository/build/mono/validate-evidence.py" "$evidence/mono-results.xml"
timeout --kill-after=10s 240s xvfb-run -a -s '-screen 0 1440x1000x24' \
    bash build/mono/validate-desktop-menu.sh "$unpacked" "$evidence" 2>&1 | tee "$evidence/desktop-menu.log"
python3 "$repository/build/mono/package.py" --verify --output "$unpacked"
printf 'Extracted package acceptance passed. Evidence: %s\n' "$evidence"
