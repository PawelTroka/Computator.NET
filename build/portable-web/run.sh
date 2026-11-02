#!/usr/bin/env bash
set -euo pipefail

if [ "$#" -lt 1 ]; then
  echo "Usage: bash build/portable-web/run.sh <publish-directory> [--port 5099]" >&2
  exit 2
fi
directory=$(cd "$1" && pwd)
shift
if [ ! -x "$directory/Computator.NET.PortableWeb" ] || [ ! -f "$directory/wwwroot/index.html" ]; then
  echo "A complete portable publication (including wwwroot/index.html) is required." >&2
  exit 1
fi
# Resolve a selected runtime through normal symlinks without GNU readlink -f,
# which is not supplied by macOS. The native apphost enforces the published RID's
# architecture; executing the DLL via another-architecture dotnet would not.
dotnet_command=$(command -v "${COMPUTATOR_DOTNET:-dotnet}")
while [ -L "$dotnet_command" ]; do
  runtime_directory=$(cd -P "$(dirname "$dotnet_command")" && pwd)
  target=$(readlink "$dotnet_command")
  case "$target" in /*) dotnet_command=$target ;; *) dotnet_command="$runtime_directory/$target" ;; esac
done
export DOTNET_ROOT="$(cd -P "$(dirname "$dotnet_command")" && pwd)"
# No shell background process or machine-wide service; Ctrl-C stops this host.
exec "$directory/Computator.NET.PortableWeb" "$@"
