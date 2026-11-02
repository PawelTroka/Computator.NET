"""Native dictionary-data selection; independent from application GUI claims."""
import ctypes
import hashlib
import json
import os
from pathlib import Path
import platform
import sys

if platform.system()!='Darwin':
    raise SystemExit('Run native Mac runtime-data acceptance on the actual Mac.')
resources,evidence=map(lambda item:Path(item).resolve(strict=True),sys.argv[1:])
manifest=json.loads((resources/'mono-package.json').read_text())
observed=[]
for item in manifest['runtimeData']['thaiDictionaries']:
    dictionary=resources/item['path']
    if hashlib.sha256(dictionary.read_bytes()).hexdigest()!=item['sha256']:
        raise SystemExit('Packaged dictionary differs from its pinned installed input.')
    if Path(os.environ.get('LIBTHAI_DICTDIR','')).resolve()!=dictionary.parent:
        raise SystemExit('Default native word breaking must select the packaged dictionary directory.')
    library=resources/'native'/manifest['rid']/item['library']
    native=ctypes.CDLL(str(library))
    native.th_brk_new.argtypes=[ctypes.c_char_p];native.th_brk_new.restype=ctypes.c_void_p
    native.th_brk_find_breaks.argtypes=[ctypes.c_void_p,ctypes.c_char_p,ctypes.POINTER(ctypes.c_int),ctypes.c_size_t]
    native.th_brk_find_breaks.restype=ctypes.c_int
    native.th_brk_delete.argtypes=[ctypes.c_void_p];native.th_brk_delete.restype=None
    # Upstream tests/test_thbrk.c TestSamples contains this dictionary-based
    # sample and these exact TIS-620 boundary positions.
    sample='ten% of all สิบ%ของทั้งหมด'.encode('tis-620')
    expected=[5,8,12,16,19]
    results=[]
    for path in (None,os.fsencode(dictionary)):
        breaker=native.th_brk_new(path)
        if not breaker: raise SystemExit('Native dictionary loading failed.')
        try:
            positions=(ctypes.c_int*100)()
            count=native.th_brk_find_breaks(breaker,sample,positions,100)
            actual=list(positions[:count])
            if actual!=expected:
                raise SystemExit('Native Thai word boundaries differ from the independent upstream sample: '+repr(actual))
            results.append(actual)
        finally:
            native.th_brk_delete(breaker)
    observed.append({'library':str(library),'dictionary':str(dictionary),'sha256':item['sha256'],
                     'defaultAndExplicitBoundaries':results,'upstreamExpected':expected})
report={'schemaVersion':1,'scope':'native-macos-packaged-non-dylib-runtime-data','passed':True,
        'fontconfig':os.environ.get('FONTCONFIG_FILE'),'gioModules':os.environ.get('GIO_MODULE_DIR'),
        'thaiDictionaries':observed,'applicationGuiAcceptance':False}
(evidence/'native-runtime-data.json').write_text(json.dumps(report,indent=2)+'\n')
print('Native runtime dictionary data:',len(observed),'providers passed upstream known answers.')
