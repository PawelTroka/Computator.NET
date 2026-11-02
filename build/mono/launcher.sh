#!/bin/sh
# Installed entry point. Build tools, Python and global Mono mappings are unnecessary.
set -eu
root=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd -P)
if [ "$(id -u)" = 0 ]; then
    echo 'Run Computator.NET as your normal desktop user, not root.' >&2
    exit 2
fi
case "${1-}" in
    '') mode=launch ;;
    --check) mode=check ;;
    *) echo 'Usage: computator [--check]' >&2; exit 2 ;;
esac
if [ "$#" -gt 1 ]; then echo 'Usage: computator [--check]' >&2; exit 2; fi
if ! command -v mono >/dev/null 2>&1; then
    echo 'Mono 6.8+ and its Windows.Forms/Drawing runtime assemblies are required. See README.txt.' >&2
    exit 2
fi
if [ -z "${DISPLAY-}" ]; then
    echo 'An X11 display is required. Start an X11/XWayland desktop, then run Computator.NET there.' >&2
    exit 2
fi
umask 077
cache=${XDG_CACHE_HOME:-${HOME:?HOME must be set}/.cache}/Computator.NET
mkdir -p -- "$cache"
session=$(mktemp -d "$cache/runtime.XXXXXXXX")
child=
cleanup() { rm -f -- "$session/mono.config"; rmdir -- "$session" 2>/dev/null || :; }
forward() { if [ -n "$child" ]; then kill -TERM "$child" 2>/dev/null || :; fi; }
trap cleanup EXIT
trap forward HUP INT TERM
config=$session/mono.config
report=$cache/runtime-check.json
# A host override for the Windows native package cannot select Unix libraries.
unset COMPUTATOR_GSL_DIRECTORY MONO_CFG_DIR MONO_ENV_OPTIONS MONO_PATH
LD_LIBRARY_PATH=$root/native/linux-x64/gsl:$root/native/linux-x64/gdiplus
MONO_PATH=$root/app/Facades:$root/app
export LD_LIBRARY_PATH MONO_PATH
preflight=$root/app/Computator.NET.Mono.Preflight.exe
mono "$preflight" prepare --package-root "$root" --config "$config"
mono --config "$config" "$preflight" verify --package-root "$root" --report "$report"
if [ "$mode" = check ]; then
    printf 'Runtime check passed. Evidence: %s\n' "$report"
    exit 0
fi
cd -- "$root/app"
mono --config "$config" "$root/app/Computator.NET.exe" &
child=$!
status=0
wait "$child" || status=$?
# wait may have been interrupted by a forwarded signal; reap before cleanup.
if kill -0 "$child" 2>/dev/null; then wait "$child" || status=$?; fi
exit "$status"
