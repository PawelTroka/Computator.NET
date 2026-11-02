#!/usr/bin/env bash
set -euo pipefail
[[ $(uname -s) == Darwin && $(uname -m) == arm64 && $# -eq 1 ]] || { echo 'Build the Mono XIM ABI shim only on native Apple Silicon.' >&2; exit 2; }
repository=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)
output=$1
mkdir -p "$output"
x11=$(brew --prefix libx11)
read -r -a x11_cflags <<< "$(pkg-config --cflags x11)"
clang -c -arch arm64 -std=c11 -Wall -Wextra -Werror -fvisibility=hidden \
    "${x11_cflags[@]}" "$repository/build/macos/mono-xim.c" -o "$output/mono-xim.o"
clang -c -arch arm64 "$repository/build/macos/mono-xim-arm64.S" -o "$output/mono-xim-arm64.o"
clang -dynamiclib -arch arm64 -L"$x11/lib" -lX11 -Wl,-headerpad_max_install_names \
    -Wl,-install_name,"$output/libcomputator-mono-xim.dylib" \
    "$output/mono-xim.o" "$output/mono-xim-arm64.o" \
    -o "$output/libcomputator-mono-xim.dylib"
lipo "$output/libcomputator-mono-xim.dylib" -verify_arch arm64
codesign --force --sign - --timestamp=none "$output/libcomputator-mono-xim.dylib"
