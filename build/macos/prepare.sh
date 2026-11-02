#!/usr/bin/env bash
set -euo pipefail

# The native graph is selected only for processes using the generated launcher.
# Never edit /etc/mono, a Framework Mono installation, or a user's dllmaps.
install=false
if [[ ${1:-} == --install-dependencies ]]; then install=true; shift; fi
if [[ $# -ne 0 || $(uname -s) != Darwin ]]; then
    echo 'Usage (on macOS): bash build/macos/prepare.sh [--install-dependencies]' >&2
    exit 2
fi
repository=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)
architecture=$(uname -m)
case "$architecture" in arm64|x86_64) ;; *) echo "Unsupported native architecture: $architecture" >&2; exit 2 ;; esac
root="$repository/artifacts/macos/native-$architecture"
mkdir -p "$root/downloads" "$root/logs" "$root/bin"
root=$(cd "$root" && pwd)
exec > >(tee "$root/logs/prepare.log") 2>&1
pins="$repository/build/macos/native-dependencies.json"
pin() { python3 -c 'import json,sys; x=json.load(open(sys.argv[1])); print(x[sys.argv[2]][sys.argv[3]])' "$pins" "$1" "$2"; }
fetch() {
    local url=$1 hash=$2 destination=$3
    if [[ ! -f "$destination" ]]; then curl --fail --location --retry 3 --proto '=https' --tlsv1.2 "$url" -o "$destination"; fi
    printf '%s  %s\n' "$hash" "$destination" | shasum -a 256 -c -
}
command -v brew >/dev/null || { echo 'Homebrew for the native process architecture is required.' >&2; exit 2; }
brew_prefix=$(brew --prefix)
if [[ $architecture == arm64 && $brew_prefix != /opt/homebrew ]] || [[ $architecture == x86_64 && $brew_prefix != /usr/local ]]; then
    echo "Homebrew prefix $brew_prefix does not match native $architecture." >&2; exit 2
fi
if [[ $install == true ]]; then
    export HOMEBREW_NO_AUTO_UPDATE=1 HOMEBREW_NO_INSTALL_CLEANUP=1
    brew install mono pkgconf cairo pango giflib libexif jpeg-turbo libtiff gsl libx11 libxext libxtst libxinerama xdotool
    if [[ ! -x /opt/X11/bin/Xquartz ]] || ! pkgutil --pkg-info org.xquartz.X11 2>/dev/null | grep -q "version: $(pin xquartz version)"; then
        package="$root/downloads/XQuartz-$(pin xquartz version).pkg"
        fetch "$(pin xquartz url)" "$(pin xquartz sha256)" "$package"
        pkgutil --check-signature "$package"
        sudo installer -pkg "$package" -target /
    fi
fi
mono_prefix=$(brew --prefix mono)
mono="$mono_prefix/bin/mono"
mcs="$mono_prefix/bin/mcs"
"$mono" --version
"$mono" --version | head -n 1 | grep -F "$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1]))["monoVersion"])' "$pins")" >/dev/null || { echo 'Unexpected Mono version; update/review the native pin explicitly.' >&2; exit 1; }
[[ -x /opt/X11/bin/Xquartz ]] || { echo 'Install the pinned XQuartz package first (or use --install-dependencies).' >&2; exit 2; }
pkgutil --pkg-info org.xquartz.X11 | tee "$root/logs/xquartz-package.log"
pkgutil --pkg-info org.xquartz.X11 | grep -q "version: $(pin xquartz version)" || { echo 'XQuartz version differs from the reviewed pin.' >&2; exit 1; }
export PATH="$(brew --prefix pkgconf)/bin:$PATH"
export PKG_CONFIG_PATH="$(brew --prefix cairo)/lib/pkgconfig:$(brew --prefix pango)/lib/pkgconfig:$(brew --prefix glib)/lib/pkgconfig:$(brew --prefix libx11)/lib/pkgconfig:$(brew --prefix libxext)/lib/pkgconfig:$brew_prefix/lib/pkgconfig:$brew_prefix/share/pkgconfig"
pkg-config --exists cairo-xlib pangocairo x11
[[ $(cd "$(pkg-config --variable=prefix cairo)" && pwd -P) == "$(cd "$(brew --prefix cairo)" && pwd -P)" ]] || { echo 'Cairo selection is ambiguous.' >&2; exit 1; }
[[ $("$(brew --prefix gsl)/bin/gsl-config" --version) == "$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1]))["gslVersion"])' "$pins")" ]] || { echo 'Unexpected GSL version.' >&2; exit 1; }
source="$root/downloads/libgdiplus-$(pin libgdiplus version).tar.gz"
fetch "$(pin libgdiplus url)" "$(pin libgdiplus sha256)" "$source"
build="$root/build-libgdiplus"
prefix="$root/libgdiplus"
mkdir -p "$build" "$prefix"
if [[ ! -f "$prefix/lib/libgdiplus.dylib" ]]; then
    tar -xzf "$source" --strip-components=1 -C "$build"
    # Upstream 6.2 retains an obsolete bockbuild-only Darwin libpng12 fallback.
    # The current Cairo/Pango stack uses libpng16. Remove only that exact gate;
    # retain all PNG checks and require the normal libpng16 package below.
    python3 - "$build/configure" <<'PY'
import pathlib,sys
p=pathlib.Path(sys.argv[1]); s=p.read_text(); old='no_libpng16=yes'
if s.count(old)!=1: raise SystemExit('Unexpected upstream Darwin PNG gate; review source before building.')
p.write_text(s.replace(old,'no_libpng16=no'))
PY
    export CPPFLAGS="-I$brew_prefix/include -I$(brew --prefix jpeg-turbo)/include"
    export LDFLAGS="-L$brew_prefix/lib -L$(brew --prefix jpeg-turbo)/lib"
    (
        cd "$build"
        ./configure --prefix="$prefix" --with-x --with-pango \
            --x-includes="$(brew --prefix libx11)/include" --x-libraries="$(brew --prefix libx11)/lib" \
            --with-libjpeg="$(brew --prefix jpeg-turbo)" --with-libtiff="$(brew --prefix libtiff)" --with-libgif="$(brew --prefix giflib)"
        grep -q '^#define HAVE_X11 1' config.h
        make -j "$(sysctl -n hw.logicalcpu)"
        make install
    ) 2>&1 | tee "$root/logs/libgdiplus-build.log"
fi
nm -gU "$prefix/lib/libgdiplus.dylib" | grep '_GdipCreateFromXDrawable_linux$' > /dev/null
otool -L "$prefix/lib/libgdiplus.dylib" | tee "$root/logs/libgdiplus-linkage.log"
brew info --json=v2 --installed > "$root/logs/homebrew-installed.json"
export COMPUTATOR_MAC_NATIVE_ROOT="$root" COMPUTATOR_MAC_MONO_PREFIX="$mono_prefix" COMPUTATOR_MAC_BREW_PREFIX="$brew_prefix"
python3 "$repository/build/macos/write-native-config.py"
"$root/bin/mono" --version
echo "Prepared native $architecture graph: $root"
echo "Use $root/bin/mono, not an unrelated Mono executable, for this application."
