#!/usr/bin/env bash
set -euo pipefail

configuration=${1:-Release}
if [[ $# -gt 1 || ! "$configuration" =~ ^(Release|Debug)$ ]]; then
    printf '%s\n' 'Usage: bash build/mono/build.sh [Release|Debug]' >&2
    exit 2
fi

repository=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)
dotnet_command=${COMPUTATOR_DOTNET:-dotnet}
results="$repository/artifacts/mono/$configuration"
mkdir -p -- "$results"
cd -- "$repository"

# The root project also builds the real executable and a startup probe with no
# application references. The opt-in property propagates through this graph.
"$dotnet_command" build build/mono/SmokeTests/Computator.NET.Mono.SmokeTests.csproj \
    -c "$configuration" -p:ComputatorMono=true -p:UseSharedCompilation=false -m:1 \
    -bl:"$results/build.binlog" --nologo 2>&1 | tee "$results/build.log"
