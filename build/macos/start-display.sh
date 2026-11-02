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
repository=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)
mode_tool="$repository/artifacts/macos/native-$(uname -m)/display/select-display-mode"
[[ -x $mode_tool ]] || { echo 'Prepared native display mode diagnostic is missing.' >&2; exit 2; }
printf '%s\n' "$mode_tool" > "$output/display-mode-tool.path"
# These disposable hosted displays are sometimes 1024px wide. Select a real
# supported larger mode before XQuartz starts, never crop an oversized window.
"$mode_tool" prepare "$output/original-display-mode.txt" > "$output/display-modes.log" 2>&1 || {
    cat "$output/display-modes.log"; echo 'No usable native CI display mode; ordinary GUI acceptance remains blocked.' >&2; exit 1;
}
defaults write org.xquartz.X11 enable_test_extensions -bool true
preferences="net.computator.mono.acceptance.$$"
defaults write "$preferences" wm_auto_quit -bool false
printf '%s\n' "$preferences" > "$output/window-manager-preferences.txt"
auth="$output/Xauthority"
(umask 077; touch "$auth")
display_number=91
while [[ -S /tmp/.X11-unix/X$display_number || -f /tmp/.X$display_number-lock ]]; do display_number=$((display_number + 1)); done
export DISPLAY=:$display_number XAUTHORITY="$auth"
cookie=$(python3 -c 'import secrets; print(secrets.token_hex(16))')
/opt/X11/bin/xauth -f "$auth" add "$DISPLAY" . "$cookie"
python3 -c 'import os,sys; os.setsid(); os.execv(sys.argv[1],sys.argv[1:])' \
    /opt/X11/bin/Xquartz "$DISPLAY" -nolisten tcp -noreset -auth "$auth" > "$output/xquartz-server.log" 2>&1 &
server=$!
printf '%s\n' "$server" > "$output/xquartz.pgid"
ready=false
for ((attempt=0; attempt<60; attempt++)); do
    if /opt/X11/bin/xdpyinfo -display "$DISPLAY" > "$output/xdpyinfo.log" 2>&1; then ready=true; break; fi
    if ! kill -0 -- "-$server" 2>/dev/null; then cat "$output/xquartz-server.log"; echo 'Owned XQuartz server exited before accepting authenticated clients.' >&2; exit 1; fi
    sleep 1
done
if [[ $ready != true ]]; then kill -- "-$server" 2>/dev/null || true; echo 'XQuartz did not become ready within 60 seconds.' >&2; exit 1; fi
grep -q 'XTEST' "$output/xdpyinfo.log" || { kill -- "-$server"; echo 'XQuartz lacks the actual XTEST input extension.' >&2; exit 1; }
# Starting Xquartz directly bypasses xinitrc, so start its ordinary window manager
# too. Testing an unmanaged server is not the normal rootless Mac desktop path.
DEBUG=1 python3 -c 'import os,sys; os.setsid(); os.execv(sys.argv[1],sys.argv[1:])' \
    /opt/X11/bin/quartz-wm --prefs-domain "$preferences" > "$output/quartz-wm.log" 2>&1 &
manager=$!
printf '%s\n' "$manager" > "$output/quartz-wm.pgid"
for ((attempt=0; attempt<30; attempt++)); do
    # quartz-wm does not universally publish EWMH's supporting-WM property.
    # The real root selection establishes that a WM owns this fresh server.
    /opt/X11/bin/xwininfo -root -events > "$output/window-manager.log" 2>&1 || true
    if grep -q 'SubstructureRedirect' "$output/window-manager.log"; then break; fi
    if ! kill -0 -- "-$manager" 2>/dev/null; then cat "$output/quartz-wm.log"; echo 'Owned quartz-wm exited.' >&2; exit 1; fi
    sleep 1
done
grep -q 'SubstructureRedirect' "$output/window-manager.log" || { echo 'Normal rootless window manager did not become ready.' >&2; exit 1; }
printf 'export DISPLAY=%q\nexport XAUTHORITY=%q\n' "$DISPLAY" "$auth" > "$output/display.env"
echo "Owned authenticated rootless XQuartz display $DISPLAY, server group $server, window-manager group $manager."
