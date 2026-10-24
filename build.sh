#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")"

# Help must work on a fresh checkout, before the SDK or Cake is installed.
for argument in "$@"; do
    case "$argument" in
        --help|-h)
            printf '%s\n' "$(<build/cli-help.txt)"
            exit 0
            ;;
    esac
done

if ! command -v dotnet >/dev/null 2>&1; then
    printf '%s\n' 'The .NET SDK is not on PATH. Install the SDK selected by global.json, restart your terminal, and retry. Run ./build.sh --help for setup and target guidance.' >&2
    exit 127
fi
dotnet tool restore
exec dotnet cake build.cake "$@"
