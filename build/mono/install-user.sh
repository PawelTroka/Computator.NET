#!/bin/sh
# Optional registration for this user. Does not copy/delete the application.
set -eu
if [ "$(id -u)" = 0 ]; then echo 'Run this as your normal desktop user.' >&2; exit 2; fi
if [ "$#" -ne 0 ]; then echo 'Usage: install-desktop-entry' >&2; exit 2; fi
root=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd -P)
# Desktop Entry Exec escaping differs from shell escaping. Quote the path and
# escape its reserved characters, including literal percent field-code markers.
escaped=$(printf '%s' "$root/bin/computator" | sed 's/\\/\\\\/g; s/"/\\"/g; s/`/\\`/g; s/\$/\\$/g; s/%/%%/g' | sed 's/\\/\\\\/g')
case "$root" in
    *'
'*) echo 'Desktop integration does not support newline characters in the installation path.' >&2; exit 2 ;;
esac
data=${XDG_DATA_HOME:-${HOME:?HOME must be set}/.local/share}
case "$data" in /*) ;; *) echo 'XDG_DATA_HOME must be an absolute directory.' >&2; exit 2;; esac
mkdir -p -- "$data/applications" "$data/icons/hicolor/256x256/apps"
icon=$data/icons/hicolor/256x256/apps/computator.png
cp -- "$root/share/computator.png" "$icon"
entry=$data/applications/computator-net.desktop
temporary=$(mktemp "$data/applications/.computator.XXXXXXXX")
trap 'rm -f -- "$temporary"' EXIT
cat > "$temporary" <<EOF
[Desktop Entry]
Type=Application
Name=Computator.NET
Comment=Numerical calculations, scripts and charts
EOF
# Keep the executable name fixed: some desktop parsers validate it before
# expanding literal %% codes. The installation path is one separately quoted
# argument; it never becomes shell source, even when it contains reserved chars.
printf '%s%s%s\n' 'Exec=/bin/sh -c "exec \\"\\$1\\"" computator "' "$escaped" '"' >> "$temporary"
cat >> "$temporary" <<EOF
Icon=computator
Terminal=true
Categories=Education;Science;Math;
StartupNotify=false
EOF
chmod 644 "$temporary"
mv -- "$temporary" "$entry"
printf 'Created %s\nKeep the extracted application at %s.\n' "$entry" "$root"
