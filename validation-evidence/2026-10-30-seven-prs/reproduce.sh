#!/usr/bin/env bash
set -euo pipefail

if [[ $# != 3 || ! "$1" =~ ^(dotnet|mono)$ ]]; then
    printf '%s\n' 'Usage: bash reproduce.sh dotnet|mono /path/to/Computator.NET /new/output/directory' >&2
    exit 2
fi
mode=$1
repository=$(git -C "$2" rev-parse --show-toplevel)
output=$(python3 -c 'import pathlib,sys; print(pathlib.Path(sys.argv[1]).resolve())' "$3")
bundle=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
dotnet_command=${COMPUTATOR_DOTNET:-dotnet}

if [[ -e "$output" ]]; then
    printf 'Output directory already exists: %s\n' "$output" >&2
    exit 2
fi
if [[ "$mode" == dotnet ]]; then
    commit=189d28c0ec7acbf77d380f14f95ef450e7bfbd84
    expected_tree=58b013e7e3f81ec62858a481860b73de0a88a7ff
else
    commit=a854ee8d9cdd9f598b6c9fe4c00471faf3c5f290
    expected_tree=40cc4713ef0bdb6b49a5cc32fab35b49e17997ce
fi
git -C "$repository" cat-file -e "$commit^{commit}"
actual_tree=$(git -C "$repository" rev-parse "$commit^{tree}")
if [[ "$actual_tree" != "$expected_tree" ]]; then
    printf 'Unexpected input tree: %s\n' "$actual_tree" >&2
    exit 2
fi
python3 - "$bundle" <<'PY'
import hashlib,json,sys
from pathlib import Path
bundle=Path(sys.argv[1])
for name,expected in json.loads((bundle/'manifest.json').read_text())['fixtureSha256'].items():
    if hashlib.sha256((bundle/name).read_bytes()).hexdigest()!=expected:
        raise SystemExit('Fixture hash mismatch: '+name)
PY

mkdir -p -- "$output"
worktree="$output/worktree"
git -C "$repository" worktree add --detach "$worktree" "$commit"
cleanup() { git -C "$repository" worktree remove --force "$worktree" >/dev/null 2>&1 || true; }
trap cleanup EXIT
cd -- "$worktree"
sdk=$("$dotnet_command" --version)
if [[ "$sdk" != 10.0.403 ]]; then
    printf 'This reproduction requires SDK 10.0.403; selected %s. Set COMPUTATOR_DOTNET to the intended dotnet executable.\n' "$sdk" >&2
    exit 2
fi
python3 - "$mode" "$commit" "$actual_tree" "$sdk" "$output" <<'PY'
import json,sys
from pathlib import Path
mode,commit,tree,sdk,output=sys.argv[1:]
Path(output,'reproduction-input.json').write_text(json.dumps(dict(mode=mode,commit=commit,tree=tree,sdk=sdk),indent=2)+'\n')
PY

if [[ "$mode" == dotnet ]]; then
    cp -- "$bundle/SevenPrCrossFeatureTests.cs" Computator.NET.Engine.Tests/
    "$dotnet_command" test Computator.NET.Engine.Tests/Computator.NET.Engine.Tests.csproj \
        -c Release -m:1 -nr:false -p:UseSharedCompilation=false \
        --filter FullyQualifiedName~SevenPrCrossFeatureTests \
        --logger 'trx;LogFileName=cross-feature.trx' --results-directory "$output/results" \
        2>&1 | tee "$output/dotnet.log"
else
    COMPUTATOR_DOTNET="$dotnet_command" bash build/mono/build.sh Release 2>&1 | tee "$output/build.log"
    runtime="$worktree/build/mono/SmokeTests/bin/mono/Release/net48"
    python3 - "$runtime" "$output" <<'PY'
import hashlib,json,sys
from pathlib import Path
runtime,output=map(Path,sys.argv[1:])
(output/'binary-sha256.json').write_text(json.dumps({p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(runtime.glob('*.dll'))},indent=2)+'\n')
PY
    cd -- "$runtime"
    mcs -nologo -langversion:7.2 -r:System.Numerics -r:System.Xml.Linq \
        -r:Computator.NET.Core.dll -r:Computator.NET.Engine.dll -r:Computator.NET.DataTypes.dll \
        -out:MonoNumericalProbe.exe "$bundle/MonoNumericalProbe.cs"
    cp -- Computator.NET.Mono.SmokeTests.exe.config MonoNumericalProbe.exe.config
    mkdir -p -- "$output/profile" "$output/config" "$output/cache"
    mono --version > "$output/runtime.txt"
    COMPUTATOR_DATA_DIRECTORY="$output/profile" XDG_CONFIG_HOME="$output/config" XDG_CACHE_HOME="$output/cache" \
        xvfb-run -a timeout --kill-after=10s 120s mono --debug MonoNumericalProbe.exe "$output/mono-numerics-results.xml" \
        2>&1 | tee "$output/probe.log"
fi

python3 - "$mode" "$output" <<'PY'
from pathlib import Path
import json,sys,xml.etree.ElementTree as ET
mode,folder=sys.argv[1:]; folder=Path(folder)
if mode=='dotnet':
    ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
    counters=ET.parse(folder/'results/cross-feature.trx').find('.//t:Counters',ns).attrib
    passed,failed,skipped=map(lambda name:int(counters[name]),['passed','failed','notExecuted'])
    expected=30
else:
    counters=ET.parse(folder/'mono-numerics-results.xml').getroot().attrib
    passed,failed,skipped=map(lambda name:int(counters[name]),['passed','failed','skipped'])
    expected=116
summary=dict(passed=passed,failed=failed,skipped=skipped,expected=expected)
(folder/'summary.json').write_text(json.dumps(summary,indent=2)+'\n')
if (passed,failed,skipped)!=(expected,0,0):
    raise SystemExit('Unexpected test totals: '+json.dumps(summary))
print(json.dumps(summary))
PY
