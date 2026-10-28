#!/usr/bin/env bash
set -euo pipefail

# Opt-in independent host. This does not publish the Windows WebApi or package.
root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
rid=${1:-}
case "$rid" in
  osx-x64|osx-arm64|linux-x64|linux-arm64) ;;
  *) echo "Usage: bash build/portable-web/build.sh <osx-x64|osx-arm64|linux-x64|linux-arm64> [output-directory]" >&2; exit 2 ;;
esac
if [ "$#" -gt 2 ]; then echo "Too many arguments." >&2; exit 2; fi
output=${2:-"$root/artifacts/portable-web/$rid"}
case "$output" in /*) ;; *) output="$root/$output" ;; esac
dotnet_command=${COMPUTATOR_DOTNET:-dotnet}
npm_command=${COMPUTATOR_NPM:-npm}
npm_command=$(command -v "$npm_command")
export PATH="$(dirname "$npm_command"):$PATH"

cd "$root/Computator.NET.WebClient"
expected_npm=$(node -p "require('./package.json').packageManager.split('@')[1]")
if [ "$("$npm_command" --version)" != "$expected_npm" ]; then
  echo "Use the repository-pinned npm $expected_npm (or select it with COMPUTATOR_NPM)." >&2
  exit 1
fi
"$npm_command" ci --no-audit --no-fund
# Keep this reduced-capability bundle separate from the normal Windows bundle.
VITE_API_BASE_URL=/api VITE_PORTABLE_LOCAL=true \
  "$npm_command" run build -- --outDir dist-portable
cd "$root"
"$dotnet_command" publish Computator.NET.PortableWeb/Computator.NET.PortableWeb.csproj \
  --configuration Release --runtime "$rid" --self-contained false --output "$output"
echo "Published portable local preview: $output"
echo "Run: bash build/portable-web/run.sh '$output'"
