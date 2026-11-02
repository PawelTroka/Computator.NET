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
"$mode_tool" verify >> "$output/display-modes.log" 2>&1 || {
    cat "$output/display-modes.log"; echo 'The selected CI display mode did not survive its setup process.' >&2; exit 1;
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
python3 -c 'import os,pathlib,sys; os.setsid(); pathlib.Path(sys.argv[1]).write_text(str(os.getpgrp())+"\n"); os.execv(sys.argv[2],sys.argv[2:])' \
    "$output/xquartz.pgid" /opt/X11/bin/Xquartz "$DISPLAY" -nolisten tcp -noreset -auth "$auth" > "$output/xquartz-server.log" 2>&1 &
for ((attempt=0; attempt<30; attempt++)); do [[ ! -s $output/xquartz.pgid ]] || break; sleep 1; done
[[ -s $output/xquartz.pgid ]] || { echo 'The owned server did not record its process group.' >&2; exit 1; }
server=$(cat "$output/xquartz.pgid")
alive() { python3 -c 'import os,sys; os.killpg(int(sys.argv[1]),0)' "$1" 2>/dev/null; }
terminate() { python3 -c 'import os,signal,sys; os.killpg(int(sys.argv[1]),signal.SIGTERM)' "$1" 2>/dev/null || true; }
ready=false
for ((attempt=0; attempt<60; attempt++)); do
    if /opt/X11/bin/xdpyinfo -display "$DISPLAY" > "$output/xdpyinfo.log" 2>&1; then ready=true; break; fi
    # The native Mac launcher can hand the server to another PID/session. Its
    # disappearance does not prove failure; await the private authenticated
    # socket for the bounded interval, then record the actual server arguments.
    sleep 1
done
if [[ $ready != true ]]; then terminate "$server"; echo 'XQuartz did not become ready within 60 seconds.' >&2; exit 1; fi
grep -q 'XTEST' "$output/xdpyinfo.log" || { terminate "$server"; echo 'XQuartz lacks the actual XTEST input extension.' >&2; exit 1; }
python3 "$repository/build/macos/display-owner.py" record "$output" --display "$DISPLAY"
# Starting Xquartz directly bypasses xinitrc, so start its ordinary window manager
# too. Testing an unmanaged server is not the normal rootless Mac desktop path.
DEBUG=1 python3 -c 'import os,pathlib,sys; os.setsid(); pathlib.Path(sys.argv[1]).write_text(str(os.getpgrp())+"\n"); os.execv(sys.argv[2],sys.argv[2:])' \
    "$output/quartz-wm.pgid" /opt/X11/bin/quartz-wm --prefs-domain "$preferences" > "$output/quartz-wm.log" 2>&1 &
for ((attempt=0; attempt<30; attempt++)); do [[ ! -s $output/quartz-wm.pgid ]] || break; sleep 1; done
[[ -s $output/quartz-wm.pgid ]] || { echo 'The owned manager did not record its process group.' >&2; exit 1; }
manager=$(cat "$output/quartz-wm.pgid")
ps -axo pid,ppid,pgid,command | grep -E '(Xquartz|quartz-wm|X11.bin)' > "$output/display-processes.log" || true
for ((attempt=0; attempt<30; attempt++)); do
    # quartz-wm does not universally publish EWMH's supporting-WM property.
    # The real root selection establishes that a WM owns this fresh server.
    /opt/X11/bin/xwininfo -root -events > "$output/window-manager.log" 2>&1 || true
    if grep -q 'SubstructureRedirect' "$output/window-manager.log"; then break; fi
    # Cocoa may transfer the process while initializing its native application.
    # Readiness is the real WM root selection, not the short-lived launch PID.
    sleep 1
done
grep -q 'SubstructureRedirect' "$output/window-manager.log" || { echo 'Normal rootless window manager did not become ready.' >&2; exit 1; }
printf 'export DISPLAY=%q\nexport XAUTHORITY=%q\n' "$DISPLAY" "$auth" > "$output/display.env"
echo "Owned authenticated rootless XQuartz display $DISPLAY, server group $server, window-manager group $manager."
