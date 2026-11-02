#!/usr/bin/env bash
set -euo pipefail
configuration=${1:-Release}
case "$configuration" in Release|Debug) ;; *) echo 'Configuration must be Release or Debug.' >&2; exit 2;; esac
repository=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
cd "$repository"
baseline=$(python3 -c 'import platform; r=platform.freedesktop_os_release(); print(r.get("ID", "")+"-"+r.get("VERSION_ID", ""))')
case "$baseline" in ubuntu-22.04|ubuntu-24.04) ;; *) echo "Unsupported native package baseline: $baseline" >&2; exit 2;; esac
output=${COMPUTATOR_MONO_PACKAGE_OUTPUT:-$repository/artifacts/mono-package/$configuration/$baseline/linux-x64}
archive=${COMPUTATOR_MONO_PACKAGE_ARCHIVE:-$repository/artifacts/mono-package/$configuration/Computator.NET-$baseline-linux-x64.tar.gz}
python3 -m unittest discover -s build/mono -p test_package.py -v
python3 build/native/build_gsl.py
python3 build/mono/build_gdiplus.py --baseline "$baseline"
python3 build/mono/package.py --configuration "$configuration" --rid linux-x64 \
    --native-directory artifacts/gsl/linux-x64/native \
    --gdiplus-directory "artifacts/mono-native/$baseline/libgdiplus-6.2-pango/native" \
    --baseline "$baseline" --output "$output" --archive "$archive"
printf 'Mono Linux archive: %s\n' "$archive"
