#!/usr/bin/env bash
# Runs in the clean runtime-only container; no Python, mcs, .NET SDK or Node.
set -euo pipefail
if [[ $# != 2 ]]; then echo 'Usage: accept-package-runtime.sh PACKAGE-DIRECTORY PREPARED-EVIDENCE-DIRECTORY' >&2; exit 2; fi
unpacked=$(realpath "$1")
evidence=$(realpath "$2")
if [[ $(id -u) == 0 ]]; then echo 'Run package acceptance as a normal user.' >&2; exit 2; fi
for tool in dotnet node npm mcs csc python3; do
    if command -v "$tool" >/dev/null 2>&1; then echo "Unexpected build tool in runtime image: $tool" >&2; exit 1; fi
done
mkdir -p "$evidence"/{profile-startup,profile-smoke,profile-launcher,config,cache}
export HOME=$evidence/home XDG_CONFIG_HOME=$evidence/config XDG_CACHE_HOME=$evidence/cache
mkdir -p "$HOME"
unset COMPUTATOR_GSL_DIRECTORY MONO_CFG_DIR MONO_ENV_OPTIONS
export LD_LIBRARY_PATH="$unpacked/native/linux-x64/gsl:$unpacked/native/linux-x64/gdiplus"
export MONO_PATH="$unpacked/app/Facades:$unpacked/app"
export PATH=/usr/bin:/bin
dpkg-query -W > "$evidence/runtime-packages.txt"
mono --version > "$evidence/mono-version.txt"
preflight=$unpacked/app/Computator.NET.Mono.Preflight.exe
mono "$preflight" prepare --package-root "$unpacked" --config "$evidence/mono.config"
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
