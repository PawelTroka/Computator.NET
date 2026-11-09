#!/usr/bin/env bash
# Invoked inside the owned Xvfb display in the minimal end-user runtime image.
set -euo pipefail
if [[ $# != 2 ]]; then echo 'Usage: validate-desktop-menu.sh PACKAGE EVIDENCE' >&2; exit 2; fi
package=$(realpath "$1")
evidence=$(realpath "$2")
export XDG_DATA_HOME=$evidence/menu-data
export COMPUTATOR_DATA_DIRECTORY=$evidence/profile-menu
export XDG_CACHE_HOME=$evidence/menu-cache
mkdir -p "$XDG_DATA_HOME" "$COMPUTATOR_DATA_DIRECTORY" "$XDG_CACHE_HOME"
"$package/bin/install-desktop-entry"
entry=$XDG_DATA_HOME/applications/computator-net.desktop
probe=$evidence/harness/Computator.NET.Mono.DesktopMenuProbe.exe
x11_tool=$evidence/harness/normal-launch-x11
# Fresh processes share the same isolated profile. A helper AppDomain or TERM
# cannot prove that the real end-user process finalizes without an exception.
for attempt in 1 2 3; do
    /usr/bin/mono --config "$evidence/mono.config" "$probe" "$entry" \
        "$evidence/normal-close-$attempt" "$x11_tool"
done
# Simulate an unavailable runtime command without changing the actual package,
# global PATH, Mono installation or preflight executable. Its error must be
# visible from the real menu and retained in a private user log.
fake=$evidence/failed-runtime-command
mkdir "$fake"
cat > "$fake/mono" <<'EOF'
#!/bin/sh
echo 'fixture runtime unavailable' >&2
exit 42
EOF
chmod 755 "$fake/mono"
export PATH=$fake:/usr/bin:/bin XDG_CACHE_HOME=$evidence/menu-error-cache
export COMPUTATOR_DATA_DIRECTORY=$evidence/profile-menu-error
mkdir -p "$XDG_CACHE_HOME" "$COMPUTATOR_DATA_DIRECTORY"
/usr/bin/mono --config "$evidence/mono.config" "$probe" "$entry" "$evidence" "$x11_tool" --expect-error
