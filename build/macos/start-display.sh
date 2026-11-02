#!/usr/bin/env bash
set -euo pipefail

# Start a separate, authenticated XQuartz display on an ephemeral hosted Mac.
# A developer's existing XQuartz server is never reused or killed. This script
# changes only the disposable CI user's XTEST preference, not system dllmaps.
if [[ $(uname -s) != Darwin || ${GITHUB_ACTIONS:-} != true || $# -ne 1 ]]; then
    echo 'Usage (ephemeral native Mac CI only): bash build/macos/start-display.sh EVIDENCE_DIRECTORY' >&2
    exit 2
fi
output=$1
mkdir -p "$output"
output=$(cd "$output" && pwd)
defaults write org.xquartz.X11 enable_test_extensions -bool true
auth="$output/Xauthority"
(umask 077; touch "$auth")
display_number=91
while [[ -S /tmp/.X11-unix/X$display_number || -f /tmp/.X$display_number-lock ]]; do display_number=$((display_number + 1)); done
export DISPLAY=:$display_number XAUTHORITY="$auth"
cookie=$(python3 -c 'import secrets; print(secrets.token_hex(16))')
/opt/X11/bin/xauth -f "$auth" add "$DISPLAY" . "$cookie"
/opt/X11/bin/Xquartz "$DISPLAY" -nolisten tcp -noreset -auth "$auth" > "$output/xquartz-server.log" 2>&1 &
server=$!
printf '%s\n' "$server" > "$output/xquartz.pid"
ready=false
for ((attempt=0; attempt<60; attempt++)); do
    if /opt/X11/bin/xdpyinfo -display "$DISPLAY" > "$output/xdpyinfo.log" 2>&1; then ready=true; break; fi
    if ! kill -0 "$server" 2>/dev/null; then cat "$output/xquartz-server.log"; echo 'Owned XQuartz server exited before accepting authenticated clients.' >&2; exit 1; fi
    sleep 1
done
if [[ $ready != true ]]; then kill "$server" 2>/dev/null || true; echo 'XQuartz did not become ready within 60seconds.' >&2; exit 1; fi
grep -q 'XTEST' "$output/xdpyinfo.log" || { kill "$server"; echo 'XQuartz lacks the actual XTEST input extension.' >&2; exit 1; }
printf 'export DISPLAY=%q\nexport XAUTHORITY=%q\n' "$DISPLAY" "$auth" > "$output/display.env"
echo "Owned authenticated XQuartz display $DISPLAY, pid $server."
