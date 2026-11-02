#!/usr/bin/env bash
set -euo pipefail
[[ $(uname -s) == Darwin && ${GITHUB_ACTIONS:-} == true && $# -eq 1 ]] || exit 2
for name in quartz-wm xquartz; do
    path="$1/$name.pgid"
    [[ -f $path ]] || continue
    group=$(cat "$path")
    [[ $group =~ ^[1-9][0-9]*$ ]] || { echo 'Invalid owned display process group.' >&2; exit 1; }
    # Both groups were created by this task with setsid; no user's display is selected.
    python3 -c 'import os,signal,sys; os.killpg(int(sys.argv[1]),signal.SIGTERM)' "$group" 2>/dev/null || true
done
if [[ -f $1/window-manager-preferences.txt ]]; then
    preferences=$(cat "$1/window-manager-preferences.txt")
    [[ $preferences == net.computator.mono.acceptance.* ]] || exit 2
    defaults delete "$preferences" 2>/dev/null || true
fi
if [[ -f $1/display-mode-tool.path && -f $1/original-display-mode.txt ]]; then
    tool=$(cat "$1/display-mode-tool.path")
    "$tool" restore "$1/original-display-mode.txt" >> "$1/display-modes.log" 2>&1
fi
