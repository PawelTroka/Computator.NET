#!/usr/bin/env bash
set -euo pipefail
[[ $(uname -s) == Darwin && $# -eq 1 ]] || { echo 'Build this XQuartz diagnostic on the actual Mac.' >&2; exit 2; }
repository=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)
output=$1
mkdir -p "$output"
x11=$(brew --prefix libx11)
clang -dynamiclib -arch "$(uname -m)" -std=c11 -Wall -Wextra -Werror -fvisibility=hidden \
    -I"$x11/include" -L"$x11/lib" -lX11 \
    "$repository/build/macos/xquartz-capture.c" -o "$output/libcomputator-mac-display.dylib"
codesign --force --sign - --timestamp=none "$output/libcomputator-mac-display.dylib"
