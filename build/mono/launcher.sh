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
interrupted=0
cleanup() {
    rm -f -- "$session/mono.config"
    rmdir -- "$session" 2>/dev/null || :
    if [ "$keep_log" = 0 ]; then rm -f -- "$log"; fi
}
forward() {
    canceled=$1
    interrupted=1
    if [ -n "$child" ]; then kill -TERM "$child" 2>/dev/null || :; fi
}
trap cleanup EXIT
trap 'forward 129' HUP
trap 'forward 130' INT
trap 'forward 143' TERM
# Shells defer signal traps while running foreground commands. Track and wait
# for every Mono stage, so cancellation also stops either runtime preflight.
run_mono() {
    if [ "$canceled" != 0 ]; then return "$canceled"; fi
    mono "$@" >>"$log" 2>&1 &
    child=$!
    wait_mono
}
wait_mono() {
    # A signal may arrive between starting the process and assigning its PID.
    if [ "$canceled" != 0 ]; then kill -TERM "$child" 2>/dev/null || :; fi
    while :; do
        interrupted=0
        status=0
        wait "$child" || status=$?
        # A trap interrupts wait before the child exits; reap it before deleting
        # the process-scoped configuration, even after repeated signals.
        if [ "$interrupted" = 0 ]; then break; fi
    done
    child=
    if [ "$canceled" != 0 ]; then return "$canceled"; fi
    return "$status"
}
config=$session/mono.config
report=$cache/runtime-check.json
# A host override for the Windows native package cannot select Unix libraries.
unset COMPUTATOR_GSL_DIRECTORY MONO_CFG_DIR MONO_ENV_OPTIONS MONO_PATH
LD_LIBRARY_PATH=$root/native/linux-x64/gsl:$root/native/linux-x64/gdiplus
MONO_PATH=$root/app/Facades:$root/app
export LD_LIBRARY_PATH MONO_PATH
preflight=$root/app/Computator.NET.Mono.Preflight.exe
run_mono "$preflight" prepare --package-root "$root" --config "$config" || {
    if [ "$canceled" != 0 ]; then exit "$canceled"; fi
    startup_error 'Computator.NET could not verify its installation. Check README.txt and the details log, then try again.' 1
}
run_mono --config "$config" "$preflight" verify --package-root "$root" --report "$report" || {
    if [ "$canceled" != 0 ]; then exit "$canceled"; fi
    startup_error 'Computator.NET could not load its runtime prerequisites. Check README.txt and the details log, then try again.' 1
}
if [ "$canceled" != 0 ]; then exit "$canceled"; fi
if [ "$mode" = check ]; then
    printf 'Runtime check passed. Evidence: %s\n' "$report"
    exit 0
fi
cd -- "$root/app"
mono --config "$config" "$root/app/Computator.NET.exe" >>"$log" 2>&1 &
child=$!
status=0
wait_mono || status=$?
if [ "$status" != 0 ] && [ "$canceled" = 0 ]; then
    startup_error 'Computator.NET stopped unexpectedly. The details log can help diagnose the problem.' "$status"
fi
exit "$status"
