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
log=
keep_log=0
startup_error() {
    message=$1
    status=$2
    keep_log=1
    if [ -n "$log" ]; then
        printf '%s\n' "$message" >&2
        cat -- "$log" >&2
        message=$(printf '%s\n\nDetails: %s' "$message" "$log")
    else
        printf '%s\n' "$message" >&2
    fi
    if [ "$mode" = launch ] && [ -n "${DISPLAY-}" ] && command -v xmessage >/dev/null 2>&1; then
        xmessage -center -title 'Computator.NET could not start' -buttons 'OK:0' -default OK \
            -timeout 60 "$message" >/dev/null 2>&1 || :
    fi
    exit "$status"
}
if ! command -v mono >/dev/null 2>&1; then
    startup_error 'Install the Mono runtime prerequisites listed in README.txt, then start Computator.NET again.' 2
fi
if [ -z "${DISPLAY-}" ]; then
    startup_error 'Start Computator.NET from an X11 or XWayland desktop. An X11 display is required.' 2
fi
umask 077
cache=${XDG_CACHE_HOME:-${HOME:?HOME must be set}/.cache}/Computator.NET
case "$cache" in /*) ;; *) echo 'XDG_CACHE_HOME must be an absolute directory.' >&2; exit 2;; esac
mkdir -p -- "$cache"
session=$(mktemp -d "$cache/runtime.XXXXXXXX")
log=$(mktemp "$cache/startup-errors.XXXXXXXX.log")
child=
canceled=0
cleanup() {
    rm -f -- "$session/mono.config"
    rmdir -- "$session" 2>/dev/null || :
    if [ "$keep_log" = 0 ]; then rm -f -- "$log"; fi
}
forward() { canceled=1; if [ -n "$child" ]; then kill -TERM "$child" 2>/dev/null || :; fi; }
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
mono "$preflight" prepare --package-root "$root" --config "$config" >>"$log" 2>&1 || \
    startup_error 'Computator.NET could not verify its installation. Check README.txt and the details log, then try again.' 1
mono --config "$config" "$preflight" verify --package-root "$root" --report "$report" >>"$log" 2>&1 || \
    startup_error 'Computator.NET could not load its runtime prerequisites. Check README.txt and the details log, then try again.' 1
if [ "$mode" = check ]; then
    printf 'Runtime check passed. Evidence: %s\n' "$report"
    exit 0
fi
cd -- "$root/app"
mono --config "$config" "$root/app/Computator.NET.exe" >>"$log" 2>&1 &
child=$!
status=0
wait "$child" || status=$?
# wait may have been interrupted by a forwarded signal; reap before cleanup.
if kill -0 "$child" 2>/dev/null; then wait "$child" || status=$?; fi
if [ "$status" != 0 ] && [ "$canceled" = 0 ]; then
    startup_error 'Computator.NET stopped unexpectedly. The details log can help diagnose the problem.' "$status"
fi
exit "$status"
