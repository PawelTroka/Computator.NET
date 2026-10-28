#!/usr/bin/env bash
set -euo pipefail

# Diagnose the existing Mono/XQuartz route without installing software, changing
# Mono's machine-wide dllmap, or changing the desktop / 3D implementations.
#
# Run in an XQuartz terminal (or a shell with its existing DISPLAY):
#   bash build/macos/probe-xquartz.sh
#   bash build/macos/probe-xquartz.sh --gui
# Native loading without a display can be checked separately:
#   bash build/macos/probe-xquartz.sh --headless
#
# COMPUTATOR_MONO and COMPUTATOR_MCS can select executable paths. For a custom
# X11-enabled libgdiplus build, COMPUTATOR_MAC_LIBRARY_PATH sets DYLD_LIBRARY_PATH
# for the probe process only. Include all required native library directories,
# e.g. /path/to/x11-gdiplus/lib:/opt/X11/lib:/path/to/gsl/lib. The log reports the
# libraries the actual Mono process loaded; installed package names alone do not
# establish compatibility. Stock Homebrew mono-libgdiplus uses --without-x11.

mode=display
output=
while [[ $# -gt 0 ]]; do
    case "$1" in
        --headless|--gui)
            if [[ "$mode" != display ]]; then
                printf '%s\n' 'Choose either --headless or --gui.' >&2
                exit 2
            fi
            mode=${1#--}
            shift
            ;;
        --output)
            if [[ $# -lt 2 || -z "$2" ]]; then
                printf '%s\n' '--output requires a directory.' >&2
                exit 2
            fi
            output=$2
            shift 2
            ;;
        --help|-h)
            printf '%s\n' 'Usage: bash build/macos/probe-xquartz.sh [--headless|--gui] [--output DIRECTORY]'
            printf '%s\n' 'Default: native prerequisites and a connection to the existing X11 DISPLAY.'
            printf '%s\n' '--headless: native prerequisites only; --gui: also show and capture a small test window.'
            printf '%s\n' 'COMPUTATOR_MONO / COMPUTATOR_MCS select executable paths.'
            printf '%s\n' 'COMPUTATOR_MAC_LIBRARY_PATH selects DYLD_LIBRARY_PATH for the probe process only.'
            printf '%s\n' 'Default evidence directory: artifacts/macos/xquartz (override with --output).'
            printf '%s\n' 'Every mode is a standalone diagnostic, not Computator.NET GUI acceptance.'
            exit 0
            ;;
        *)
            printf 'Unknown argument: %s\n' "$1" >&2
            exit 2
            ;;
    esac
done

if [[ "$(uname -s)" != Darwin ]]; then
    printf '%s\n' 'This diagnostic requires macOS; Linux results cannot validate the Mac native stack.' >&2
    exit 2
fi

repository=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)
mono_command=${COMPUTATOR_MONO:-mono}
mcs_command=${COMPUTATOR_MCS:-mcs}
for dependency in "$mono_command" "$mcs_command"; do
    if ! command -v "$dependency" >/dev/null 2>&1; then
        printf 'Missing executable: %s. Install/select Framework Mono for the process architecture first.\n' "$dependency" >&2
        exit 2
    fi
done

output=${output:-"$repository/artifacts/macos/xquartz"}
mkdir -p -- "$output"
output=$(cd -- "$output" && pwd)
probe_temp=$(mktemp -d "${TMPDIR:-/tmp}/computator-xquartz.XXXXXXXX")
trap 'rm -rf -- "$probe_temp"' EXIT
# A failed invocation must not leave an earlier successful capture beside its log.
rm -f -- "$output/xquartz-probe.png"

status=0
(
    printf 'Mode: %s\n' "$mode"
    sw_vers
    printf 'Shell architecture: %s\n' "$(uname -m)"
    printf 'Mono executable: %s\n' "$(command -v "$mono_command")"
    printf 'Compiler executable: %s\n' "$(command -v "$mcs_command")"
    "$mono_command" --version
    "$mcs_command" -nologo -r:System.Drawing -r:System.Windows.Forms \
        -out:"$probe_temp/XQuartzProbe.exe" "$repository/build/macos/XQuartzProbe.cs"
    export MONO_MWF_MAC_FORCE_X11=1
    if [[ -n "${COMPUTATOR_MAC_LIBRARY_PATH:-}" ]]; then
        export DYLD_LIBRARY_PATH="$COMPUTATOR_MAC_LIBRARY_PATH"
    fi
    "$mono_command" "$probe_temp/XQuartzProbe.exe" "$mode" "$output/xquartz-probe.png"
) 2>&1 | tee "$output/probe.log" || status=$?

exit "$status"
