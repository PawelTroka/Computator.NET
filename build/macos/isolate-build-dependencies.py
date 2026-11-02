"""Hide owned build candidates/data for disposable Mac CI, restoring on cleanup.

The user launcher never runs this. This proves the relocated app cannot consume
its original private GDI/shim or Homebrew font/module configurations by accident.
"""
import argparse
import json
import os
from pathlib import Path
import platform
import re

parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('operation',choices=('prepare','restore'))
parser.add_argument('--state',required=True,type=Path)
parser.add_argument('--native-root',type=Path)
parser.add_argument('--brew-prefix',type=Path)
args=parser.parse_args()
if platform.system()!='Darwin' or os.environ.get('GITHUB_ACTIONS')!='true':
    parser.error('dependency hiding is restricted to the disposable native GitHub Mac runner')
repository=Path(__file__).resolve().parents[2]
expected_native=(repository/'artifacts/macos'/('native-'+platform.machine())).resolve()
expected_brew=Path('/opt/homebrew' if platform.machine()=='arm64' else '/usr/local').resolve()
if args.operation=='restore':
    state=json.loads(args.state.read_text())
    for entry in reversed(state['paths']):
        original,hidden=Path(entry['original']),Path(entry['hidden'])
        if (not original.is_absolute() or not (original.is_relative_to(expected_native) or original.is_relative_to(expected_brew))
                or hidden.parent!=original.parent or not re.fullmatch(re.escape(original.name)+r'\.computator-acceptance-hidden-\d+',hidden.name)
                or hidden.is_symlink()):
            raise SystemExit('Refusing an unsafe dependency restoration path.')
        if hidden.exists():
            if original.exists() or original.is_symlink():
                raise SystemExit('Refusing to replace a new path while restoring build dependencies: '+str(original))
            hidden.rename(original)
    state['restored']=True
    args.state.write_text(json.dumps(state,indent=2)+'\n')
else:
    if not args.native_root or not args.brew_prefix or args.state.exists():
        parser.error('prepare requires explicit native/Brew roots and a new state file')
    native=args.native_root.resolve(strict=True)
    brew=args.brew_prefix.resolve(strict=True)
    if native!=expected_native or brew!=expected_brew:
        parser.error('unexpected owned native root or architecture-specific Homebrew prefix')
    candidates=[native/'libgdiplus',native/'xim',brew/'etc/fonts',brew/'etc/pango']
    cellar=brew/'Cellar'
    for formula,suffixes in {'fontconfig':('etc/fonts','share/fontconfig'),
                              'pango':('etc/pango','lib/pango'),
                              'glib':('lib/gio/modules','share/glib-2.0/schemas'),
                              'icu4c@78':('share/icu',),'icu4c':('share/icu',)}.items():
        for keg in (cellar/formula).glob('*'):
            candidates.extend(keg/suffix for suffix in suffixes)
    state={'schemaVersion':1,'scope':'ephemeral-native-mac-ci-only','paths':[],'restored':False}
    args.state.parent.mkdir(parents=True,exist_ok=True)
    args.state.write_text(json.dumps(state,indent=2)+'\n')
    selected=[]
    for candidate in candidates:
        if not candidate.exists():
            continue
        original=candidate.resolve(strict=True)
        if not (original.is_relative_to(native) or original.is_relative_to(brew)):
            raise SystemExit('Refusing to hide data outside the explicitly owned roots: '+str(original))
        if any(original==previous or original.is_relative_to(previous) for previous in selected):
            continue
        hidden=original.with_name(original.name+'.computator-acceptance-hidden-'+str(os.getpid()))
        if hidden.exists() or hidden.is_symlink():
            raise SystemExit('Dependency backup already exists; no replacement is permitted.')
        state['paths'].append({'original':str(original),'hidden':str(hidden)})
        # Save restoration intent before mutation, including interruption safety.
        args.state.write_text(json.dumps(state,indent=2)+'\n')
        original.rename(hidden)
        selected.append(original)
    print('Temporarily isolated build candidates/data:',len(state['paths']))
