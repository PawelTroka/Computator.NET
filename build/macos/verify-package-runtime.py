"""Reject host drawing/numerical libraries in actual relocated process traces."""
import json
from pathlib import Path
import re
import sys

resources,evidence,mono=map(lambda item:Path(item).resolve(strict=True),sys.argv[1:])
if (mono/'bin/mono').resolve(strict=True).parent.parent!=mono:
    raise SystemExit('The external runtime allowance must name the exact Mono installation, not a broad tool prefix.')
report=json.loads((evidence/'launcher-preflight.json').read_text())
if not report.get('passed') or report.get('verifiedBootstrapFiles') != 2:
    raise SystemExit('The actual launcher did not verify the complete installed bootstrap and native payload.')
manifest=json.loads((resources/'mono-package.json').read_text())
normal=json.loads((evidence/'normal-launch/normal-launch.json').read_text())
if (normal.get('passed') is not True or normal.get('launcherExitCode')!=0
        or normal.get('source')!=manifest['source']):
    raise SystemExit('The fresh ordinary launcher did not accept and normally close this exact package.')
observed=set();counts={}
for filename in ('startup.log','smoke.log','normal-launch/launcher.stderr.log'):
    current=set()
    for line in (evidence/filename).read_text().splitlines():
        match=re.match(r'^dyld\[(\d+)\]: (?:<[^>]+> )?(/.*)$',line)
        if match:
            observed.add(match[2])
            if filename!='normal-launch/launcher.stderr.log' or int(match[1])==normal['applicationProcess']['pid']:
                current.add(match[2])
    counts[filename]=len(current)
    if len(current)<10:
        raise SystemExit('Insufficient actual application dyld trace to establish relocated native loading: '+filename)
allowed=(str(resources/'native')+'/',str(evidence/'harness')+'/',str(mono)+'/',
         '/usr/lib/','/System/Library/','/Library/Apple/')
resolved={path:str(Path(path).resolve()) for path in observed}
unexpected=sorted(path for path in observed if not resolved[path].startswith(allowed))
result={'schemaVersion':1,'scope':'actual-relocated-macos-process-native-loading',
        'passed':not unexpected,'loadedImages':sorted(observed),'canonicalImagePaths':resolved,'unexpectedImages':unexpected,
        'applicationTraceCounts':counts,'normalLaunchApplicationPid':normal['applicationProcess']['pid'],
        'externalPrerequisites':['exact native Mono runtime installation','macOS system libraries and fonts'],
        'xQuartzScope':'external server/locale data in separate processes; client dylibs must be packaged',
        'physicalMacAcceptance':False}
(evidence/'relocated-native-runtime.json').write_text(json.dumps(result,indent=2)+'\n')
if unexpected:
    raise SystemExit('Relocated application loaded non-packaged host libraries: '+', '.join(unexpected))
print(f'Verified {len(observed)} actual native image paths; drawing/numerical closure is relocated.')
