#!/usr/bin/env bash
set -euo pipefail

configuration=Release
skip_build=false
for argument in "$@"; do
    case "$argument" in
        Release|Debug) configuration=$argument ;;
        --no-build) skip_build=true ;;
        *) printf '%s\n' 'Usage: bash build/mono/validate.sh [Release|Debug] [--no-build]' >&2; exit 2 ;;
    esac
done

repository=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)
results="$repository/artifacts/mono/$configuration"
mkdir -p -- "$results"
cd -- "$repository"

if [[ "$(uname -s)" != Linux ]]; then
    printf '%s\n' 'This acceptance runner currently validates Linux + X11. macOS runtime support is not verified.' >&2
    exit 2
fi
for dependency in mono mcs xvfb-run timeout; do
    if ! command -v "$dependency" >/dev/null; then
        printf 'Missing validation dependency: %s. See docs/MONO-DESKTOP.md.\n' "$dependency" >&2
        exit 2
    fi
done

mono --version | tee "$results/runtime.log"
if [[ "$skip_build" != true ]]; then
    bash build/mono/build.sh "$configuration"
fi

application="$repository/Computator.NET/bin/mono/$configuration/net48/Computator.NET.exe"
startup_probe="$repository/build/mono/StartupProbe/bin/mono/$configuration/net48/Computator.NET.Mono.StartupProbe.exe"
smoke_tests="$repository/build/mono/SmokeTests/bin/mono/$configuration/net48/Computator.NET.Mono.SmokeTests.exe"
for executable in "$application" "$startup_probe" "$smoke_tests"; do
    if [[ ! -f "$executable" ]]; then
        printf 'Missing built executable: %s\n' "$executable" >&2
        exit 2
    fi
done

# Exercise fresh, separate profiles without touching the user's saved documents
# or preferences. The production profile override already exists for migration tests.
validation_temp=$(mktemp -d "${TMPDIR:-/tmp}/computator-mono-acceptance.XXXXXXXX")
trap 'rm -rf -- "$validation_temp"' EXIT
mkdir -p "$validation_temp/startup" "$validation_temp/smoke" "$validation_temp/config" "$validation_temp/cache"
# Regenerate only trusted test data with Mono's Framework serializer. The
# production import path uses the bounded data-only NrbfDecoder, never Deserialize.
mcs -nologo -nowarn:0414 -r:System.Drawing -out:"$validation_temp/LegacyFixture.exe" \
    build/fixtures/LegacySettings.cs build/mono/LegacyFixtureWriter.cs
mono "$validation_temp/LegacyFixture.exe" "$validation_temp/legacy-settings.dat"
# A failed or timed-out run must not expose an old passing report/screenshot.
rm -f -- "$results/mono-results.xml" "$results/startup.png" "$results/desktop-workflow.png" \
    "$results/real-chart.png" "$results/complex-chart.png" "$results/unsupported-3d.png" "$results/function-reference.png" \
    "$results/calculator-inputs.png" "$results/numerical-inputs.png" \
    "$results/find-replace.png" "$results/find-replace-dialog.png"
status=0
COMPUTATOR_DATA_DIRECTORY="$validation_temp/startup" \
XDG_CONFIG_HOME="$validation_temp/config" XDG_CACHE_HOME="$validation_temp/cache" \
xvfb-run -a -s '-screen 0 1440x1000x24' timeout --kill-after=10s 120s \
    mono --debug "$startup_probe" "$application" "$results" \
    2>&1 | tee "$results/startup.log" || status=1

COMPUTATOR_DATA_DIRECTORY="$validation_temp/smoke" \
COMPUTATOR_MONO_LEGACY_FIXTURE="$validation_temp/legacy-settings.dat" \
XDG_CONFIG_HOME="$validation_temp/config" XDG_CACHE_HOME="$validation_temp/cache" \
xvfb-run -a -s '-screen 0 1440x1000x24' timeout --kill-after=10s 300s \
    mono --debug "$smoke_tests" "$results" \
    2>&1 | tee "$results/smoke.log" || status=1

exit "$status"
