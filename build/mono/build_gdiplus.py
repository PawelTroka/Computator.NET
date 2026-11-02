#!/usr/bin/env python3
"""Source-pin libgdiplus+Pango for the current Ubuntu baseline in repo-owned paths."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import shutil
import subprocess
import sys
import tarfile
import urllib.request

ROOT = Path(__file__).resolve().parents[2]


def run(*args, cwd=None):
    print('+', ' '.join(map(str, args)), flush=True)
    subprocess.run(list(map(str, args)), cwd=cwd, check=True)


def capture(*args):
    return subprocess.check_output(list(map(str, args)), text=True).strip()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--baseline', required=True, choices=('ubuntu-22.04', 'ubuntu-24.04'))
    args = parser.parse_args()
    release = platform.freedesktop_os_release()
    actual = release.get('ID', '') + '-' + release.get('VERSION_ID', '')
    if platform.system() != 'Linux' or platform.machine().lower() not in ('x86_64', 'amd64') or actual != args.baseline:
        raise ValueError('Build natively on the requested Ubuntu x86_64 baseline; detected ' + actual)
    pins = json.loads((ROOT / 'build/mono/gdiplus-source.json').read_text())
    work = ROOT / 'artifacts/mono-native' / args.baseline
    work.mkdir(parents=True, exist_ok=True)
    archive = ROOT / 'artifacts/mono-native/downloads/libgdiplus-6.1.tar.gz'
    archive.parent.mkdir(parents=True, exist_ok=True)
    if not archive.exists():
        temporary = archive.with_suffix('.download')
        with urllib.request.urlopen(pins['archive'], timeout=90) as response, temporary.open('xb') as stream:
            shutil.copyfileobj(response, stream)
        temporary.rename(archive)
    if hashlib.sha512(archive.read_bytes()).hexdigest() != pins['sha512']:
        raise ValueError('libgdiplus archive differs from the reviewed source pin.')
    source = work / 'source'
    if not source.exists():
        source.mkdir()
        with tarfile.open(archive, 'r:gz') as tar:
            # Use Python's safe data filter; source archive is additionally hash pinned.
            tar.extractall(source, filter='data')
    source /= 'libgdiplus-6.1'
    prefix = work / 'prefix'
    configure = [source / 'configure', '--prefix=' + str(prefix), *pins['configure']]
    run(*configure, cwd=source)
    config = (source / 'config.h').read_text()
    if '#define USE_PANGO_RENDERING 1' not in config or '#define HAVE_X11 1' not in config:
        raise ValueError('The native build must enable both Pango rendering and X11.')
    run('make', '-j2', cwd=source)
    run('make', 'check', '-j2', cwd=source)
    run('make', 'install', cwd=source)
    destination = work / 'native'
    if destination.exists():
        raise ValueError('Native candidate already exists; preserve it and use a fresh checkout for another build.')
    destination.mkdir()
    library = prefix / 'lib/libgdiplus.so.0.0.0'
    run('readelf', '-h', library)
    needed = capture('readelf', '-d', library)
    if 'libpangocairo-1.0.so.0' not in needed or 'libX11.so.6' not in needed:
        raise ValueError('Native dependencies do not include the required Pango/X11 implementations.')
    dependencies = capture('ldd', library)
    if 'not found' in dependencies:
        raise ValueError('Unresolved libgdiplus native dependency.')
    shutil.copy2(library, destination / library.name)
    shutil.copy2(source / 'LICENSE', destination / 'LICENSE.libgdiplus')
    shutil.copy2(source / 'COPYING', destination / 'COPYING.libgdiplus')
    shutil.copy2(archive, destination / archive.name)
    shutil.copy2(ROOT / 'build/mono/build_gdiplus.py', destination / 'build_gdiplus.py')
    shutil.copy2(ROOT / 'build/mono/gdiplus-source.json', destination / 'gdiplus-source.json')
    (destination / 'SOURCE.libgdiplus.txt').write_text(
        'libgdiplus 6.1, MIT license. Complete corresponding upstream source is libgdiplus-6.1.tar.gz.\n'
        'The reviewed SHA512, source origin and configure options are in gdiplus-source.json.\n'
        'Build on the recorded Ubuntu baseline with the included build_gdiplus.py recipe.\n'
        'No patches are applied. System Cairo/Pango/X11 and image codecs are runtime prerequisites.\n')
    (destination / 'dependencies.txt').write_text(needed + '\n' + dependencies + '\n')
    metadata = {
        'schemaVersion': 1, 'version': pins['version'], 'rid': 'linux-x64',
        'baseline': args.baseline, 'entryPoint': library.name, 'pango': True, 'source': pins,
        'compiler': capture('cc', '--version').splitlines()[0],
        'dependencies': {name: capture('pkg-config', '--modversion', name)
                         for name in ('pangocairo', 'cairo', 'libpng', 'libtiff-4', 'x11')},
        'needed': needed,
        'files': {path.name: hashlib.sha256(path.read_bytes()).hexdigest()
                  for path in sorted(destination.iterdir()) if path.is_file()}}
    (destination / 'gdiplus-provenance.json').write_text(json.dumps(metadata, indent=2) + '\n')
    print('Validated libgdiplus candidate:', destination, flush=True)


if __name__ == '__main__':
    try:
        main()
    except (OSError, ValueError, subprocess.CalledProcessError) as error:
        print('libgdiplus build failed:', error, file=sys.stderr)
        sys.exit(1)
