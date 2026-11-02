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
for command in mono python3 timeout xvfb-run xauth; do command -v "$command" >/dev/null || { echo "Missing acceptance tool: $command" >&2; exit 2; }; done
python3 build/mono/package.py --extract "$archive" --output "$unpacked"
mkdir -p "$evidence"/{profile-startup,profile-smoke,profile-launcher,config,cache,harness}
sha256sum "$archive" > "$evidence/archive.sha256"
cp "$unpacked/mono-package.json" "$evidence/mono-package.json"
smoke=$repository/build/mono/SmokeTests/bin/mono/$configuration/net48
startup=$repository/build/mono/StartupProbe/bin/mono/$configuration/net48/Computator.NET.Mono.StartupProbe.exe
cp "$smoke/Computator.NET.Mono.SmokeTests.exe" "$smoke/Computator.NET.Mono.SmokeTests.exe.config" "$evidence/harness/"
cp -R "$smoke/Localization" "$evidence/harness/Localization"
# The harness carries no production DLLs. Shared content is read from the actual
# installed package; this preserves the test executable's expected base paths.
ln -s "$unpacked/app/Static" "$evidence/harness/Static"
ln -s "$unpacked/app/TSL Examples" "$evidence/harness/TSL Examples"
cp "$startup" "$evidence/harness/Computator.NET.Mono.StartupProbe.exe"
if [[ -f "$startup.config" ]]; then cp "$startup.config" "$evidence/harness/"; fi
# Legacy data is a checked test fixture, generated before the runtime-only phase.
mcs -nologo -nowarn:0414 -r:System.Drawing -out:"$evidence/harness/LegacyFixture.exe" \
    build/fixtures/LegacySettings.cs build/mono/LegacyFixtureWriter.cs
mono "$evidence/harness/LegacyFixture.exe" "$evidence/legacy-settings.dat"
preflight=$unpacked/app/Computator.NET.Mono.Preflight.exe
mono "$preflight" prepare --package-root "$unpacked" --config "$evidence/mono.config"
unset COMPUTATOR_GSL_DIRECTORY MONO_CFG_DIR MONO_ENV_OPTIONS
export LD_LIBRARY_PATH="$unpacked/native/linux-x64/gsl:$unpacked/native/linux-x64/gdiplus"
export MONO_PATH="$unpacked/app"
export XDG_CONFIG_HOME="$evidence/config" XDG_CACHE_HOME="$evidence/cache"
# Deliberately exclude all dotnet/Node/private build-tool directories from runtime
# execution. The clean runtime-only CI container separately proves package deps.
export PATH=/usr/bin:/bin
COMPUTATOR_DATA_DIRECTORY="$evidence/profile-launcher" \
    xvfb-run -a -s '-screen 0 1440x1000x24' timeout --kill-after=10s 120s \
    "$unpacked/bin/computator" --check 2>&1 | tee "$evidence/launcher-check.log"
COMPUTATOR_DATA_DIRECTORY="$evidence/profile-startup" \
    xvfb-run -a -s '-screen 0 1440x1000x24' timeout --kill-after=10s 120s \
    mono --config "$evidence/mono.config" "$evidence/harness/Computator.NET.Mono.StartupProbe.exe" \
    "$unpacked/app/Computator.NET.exe" "$evidence" 2>&1 | tee "$evidence/startup.log"
COMPUTATOR_DATA_DIRECTORY="$evidence/profile-smoke" \
COMPUTATOR_MONO_LEGACY_FIXTURE="$evidence/legacy-settings.dat" \
COMPUTATOR_MONO_EXPECTED_APPLICATION_DIRECTORY="$unpacked/app" \
    xvfb-run -a -s '-screen 0 1440x1000x24' timeout --kill-after=10s 300s \
    mono --config "$evidence/mono.config" "$evidence/harness/Computator.NET.Mono.SmokeTests.exe" \
    "$evidence" 2>&1 | tee "$evidence/smoke.log"
python3 "$repository/build/mono/package.py" --verify --output "$unpacked"
printf 'Extracted package acceptance passed. Evidence: %s\n' "$evidence"
