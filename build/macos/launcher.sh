#!/bin/bash
set -euo pipefail
resources=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../Resources" && pwd)
fail() {
    printf 'Computator.NET: %s\n' "$1" >&2
    if [[ ${COMPUTATOR_NONINTERACTIVE:-} != 1 ]]; then
        /usr/bin/osascript - "$1" <<'APPLESCRIPT' || true
on run argv
    display dialog (item 1 of argv) with title "Computator.NET could not start" buttons {"OK"} default button "OK"
end run
APPLESCRIPT
    fi
    exit 1
}
[[ $(uname -s) == Darwin ]] || fail 'This application requires macOS.'
architecture=$(uname -m)
case "$architecture" in
    arm64) default_mono=/opt/homebrew/bin/mono ;;
    x86_64) default_mono=/usr/local/bin/mono ;;
    *) fail "Unsupported Mac architecture: $architecture" ;;
esac
mono=${COMPUTATOR_MONO:-$default_mono}
[[ -x $mono ]] || fail 'Install native Mono6.14.1 for this Mac architecture. See README.txt in Contents/Resources.'
# The managed preflight enforces the minimum Mono version and the actual loaded
# WinForms import/struct metadata before applying the architecture-specific shim.
[[ -x /opt/X11/bin/Xquartz ]] || fail 'Install XQuartz2.8.6, then log out and log back in before first use.'
/usr/sbin/pkgutil --pkg-info org.xquartz.X11 2>/dev/null | grep 'version: 2.8.6' > /dev/null || fail 'Use the tested XQuartz2.8.6 package for this release candidate.'
temporary=$(mktemp -d "${TMPDIR:-/tmp}/computator-mac-launch.XXXXXXXX")
trap 'rm -rf -- "$temporary"' EXIT
unset MONO_ENV_OPTIONS MONO_CFG_DIR MONO_PATH COMPUTATOR_GSL_DIRECTORY MONO_WINFORMS_XIM_STYLE
unset MONO_GAC_PREFIX DYLD_LIBRARY_PATH DYLD_FALLBACK_LIBRARY_PATH DYLD_FRAMEWORK_PATH DYLD_FALLBACK_FRAMEWORK_PATH DYLD_INSERT_LIBRARIES
export MONO_MWF_MAC_FORCE_X11=1
export MONO_PATH="$resources/app:$resources/app/Facades"
export FONTCONFIG_FILE="$resources/native/fonts.conf"
export FONTCONFIG_PATH="$resources/native"
export GIO_MODULE_DIR="$resources/native/gio-modules" GIO_EXTRA_MODULES=
export LIBTHAI_DICTDIR="$resources/native/libthai"
export XLOCALEDIR=/opt/X11/share/X11/locale
preflight="$resources/app/Computator.NET.Mono.Preflight.exe"
"$mono" "$preflight" prepare --package-root "$resources" --config "$temporary/mono.config" >&2 || fail 'The package integrity or native dependency preflight failed.'
"$mono" --config "$temporary/mono.config" "$preflight" verify --package-root "$resources" --report "$temporary/preflight.json" >&2 || fail 'The runtime architecture or native ABI preflight failed.'
if [[ ${1:-} == --preflight ]]; then cat "$temporary/preflight.json"; exit 0; fi
if [[ -z ${DISPLAY:-} ]]; then
    /usr/bin/open -a XQuartz || fail 'Could not open XQuartz.'
    for ((attempt=0; attempt<30; attempt++)); do
        export DISPLAY=$(/bin/launchctl getenv DISPLAY)
        if [[ -n $DISPLAY ]] && /opt/X11/bin/xdpyinfo >/dev/null 2>&1; then break; fi
        sleep 1
    done
fi
[[ -n ${DISPLAY:-} ]] && /opt/X11/bin/xdpyinfo >/dev/null 2>&1 || fail 'No authenticated XQuartz display is available. Log out and log back in after installing XQuartz, then retry.'
cd "$resources/app"
"$mono" --config "$temporary/mono.config" Computator.NET.exe "$@"
