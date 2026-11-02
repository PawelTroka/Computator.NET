#!/usr/bin/env bash
# Build external acceptance tools before hiding build dependencies. Neither tool
# is copied into the .app or referenced by its Mono config/native dependency graph.
set -euo pipefail
[[ $(uname -s) == Darwin && $# -eq 2 ]] || {
    echo 'Usage (on the actual Mac): bash build/macos/build-normal-launch-probe.sh OUTPUT-DIRECTORY ABSOLUTE-MCS' >&2; exit 2;
}
repository=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)
mkdir -p "$1"
output=$(cd "$1" && pwd)
[[ $2 == /* && -x $2 ]] || { echo 'Select the external native Mono compiler by absolute path.' >&2; exit 2; }
xres_prefix=$(brew --prefix libxres)
x11_prefix=$(brew --prefix libx11)
export PKG_CONFIG_PATH="$xres_prefix/lib/pkgconfig:$x11_prefix/lib/pkgconfig${PKG_CONFIG_PATH:+:$PKG_CONFIG_PATH}"
read -r -a cflags <<< "$(pkg-config --cflags xres x11)"
read -r -a libraries <<< "$(pkg-config --libs xres x11)"
clang -std=c11 -Wall -Wextra -Werror -arch "$(uname -m)" "${cflags[@]}" \
    "$repository/build/macos/normal-launch-x11.c" "${libraries[@]}" -o "$output/normal-launch-x11"
"$2" -nologo -r:System.Drawing -r:System.Web.Extensions \
    -out:"$output/NormalLaunchImageProbe.exe" "$repository/build/macos/NormalLaunchImageProbe.cs"
otool -L "$output/normal-launch-x11" > "$output/normal-launch-x11-dependencies.log"
shasum -a 256 "$output/normal-launch-x11" "$output/NormalLaunchImageProbe.exe" > "$output/normal-launch-tools.sha256"
echo "External ordinary-launch ownership/capture tools: $output"
