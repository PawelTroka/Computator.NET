"""Reject host drawing/numerical libraries in actual relocated process traces."""
import json
from pathlib import Path
import re
import sys

resources,evidence,mono=map(lambda item:Path(item).resolve(strict=True),sys.argv[1:])
report=json.loads((evidence/'launcher-preflight.json').read_text())
if not report.get('passed') or report.get('verifiedBootstrapFiles') != 2:
    raise SystemExit('The actual launcher did not verify the complete installed bootstrap and native payload.')
observed=set()
for filename in ('startup.log','smoke.log'):
    for line in (evidence/filename).read_text().splitlines():
        match=re.match(r'^dyld\[\d+\]: (?:<[^>]+> )?(/.*)$',line)
        if match:
            observed.add(match[1])
if len(observed)<10:
    raise SystemExit('Insufficient actual dyld trace to establish relocated native loading.')
allowed=(str(resources/'native')+'/',str(evidence/'harness')+'/',str(mono)+'/',
         '/usr/lib/','/System/Library/','/opt/X11/','/Library/Apple/')
unexpected=sorted(path for path in observed if not path.startswith(allowed))
result={'schemaVersion':1,'scope':'actual-relocated-macos-process-native-loading',
        'passed':not unexpected,'loadedImages':sorted(observed),'unexpectedImages':unexpected,
        'externalPrerequisites':['native Mono runtime','XQuartz','macOS system libraries and fonts'],
        'physicalMacAcceptance':False}
(evidence/'relocated-native-runtime.json').write_text(json.dumps(result,indent=2)+'\n')
if unexpected:
    raise SystemExit('Relocated application loaded non-packaged host libraries: '+', '.join(unexpected))
print(f'Verified {len(observed)} actual native image paths; drawing/numerical closure is relocated.')
