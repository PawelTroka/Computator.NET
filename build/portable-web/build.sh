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

cd "$root"
"$dotnet_command" tool restore
"$dotnet_command" cake build.cake --target=PortableWeb-Publish \
  --portable-web-rid="$rid" --portable-web-output="$output"
echo "Published portable local preview: $output"
echo "Run: bash build/portable-web/run.sh '$output'"
