#!/usr/bin/env bash
set -euo pipefail
[[ $(uname -s) == Darwin && ${GITHUB_ACTIONS:-} == true && $# -eq 1 ]] || exit 2
for name in quartz-wm xquartz; do
    path="$1/$name.pgid"
    [[ -f $path ]] || continue
    group=$(cat "$path")
    [[ $group =~ ^[1-9][0-9]*$ ]] || { echo 'Invalid owned display process group.' >&2; exit 1; }
    # Both groups were created by this task with setsid; no user's display is selected.
    kill -TERM -- "-$group" 2>/dev/null || true
done
