#!/usr/bin/env python3
"""Build-time staging for the runtime-dependent Mono desktop distribution.

Python is a build/validation dependency; the installed launcher uses only sh/Mono.
Staging never replaces an existing payload or follows source symlinks.
"""
import argparse
import gzip
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
import re
import shutil
import stat
import struct
import subprocess
import sys
import tarfile
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
MANIFEST = 'mono-package.json'
RIDS = ('linux-x64', 'osx-x64', 'osx-arm64')
REQUIRED_APP = (
    'Computator.NET.exe', 'Computator.NET.exe.config', 'Computator.NET.Core.dll',
    'Computator.NET.Desktop.dll', 'Computator.NET.Charting.dll', 'Computator.NET.Engine.dll',
    'Computator.NET.DataTypes.dll', 'Computator.NET.Localization.dll', 'DataVisualizationX.dll',
    'Microsoft.CodeAnalysis.dll', 'Microsoft.CodeAnalysis.CSharp.dll',
    'Static/functions.xml', 'Static/TSL-Syntax.xshd',
)


def digest(path):
    checksum = hashlib.sha256()
    with path.open('rb') as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b''):
            checksum.update(block)
    return checksum.hexdigest()


def relative_path(value):
    path = PurePosixPath(value)
    if (not value or '\\' in value or path.is_absolute() or ':' in value
            or any(part in ('', '.', '..') for part in value.split('/'))):
        raise ValueError('Unsafe package-relative path: ' + value)
    return path


def confined(root, value):
    path = root.joinpath(*relative_path(value).parts)
    # Check every component: resolving first would hide an escaping symlink.
    current = root
    for part in relative_path(value).parts:
        current /= part
        if current.is_symlink():
            raise ValueError('Package paths cannot traverse symbolic links: ' + value)
    if not path.resolve().is_relative_to(root.resolve()):
        raise ValueError('Package path escaped root: ' + value)
    return path


def payload_files(root):
    for path in sorted(root.rglob('*')):
        if path.is_symlink() or (not path.is_dir() and not path.is_file()):
            raise ValueError('Unsupported package entry: ' + str(path))
        if path.is_file() and path != root / MANIFEST:
            yield path


def copy_tree(source, destination):
    if source.is_symlink() or not source.is_dir():
        raise ValueError('Expected a regular source directory: ' + str(source))
    # Reject even internal symlinks, preventing accidental copying of host files.
    list(payload_files(source))
    shutil.copytree(source, destination)


def application_contract(directory):
    for name in REQUIRED_APP:
        if not confined(directory, name).is_file():
            raise ValueError('Incomplete Mono application output: ' + name)
    if not list((directory / 'TSL Examples').rglob('*.tsl')):
        raise ValueError('The application output is missing its TSL examples.')
    if not list((directory / 'Static/reference-equations').glob('*.png')):
        raise ValueError('The application output is missing reference equation assets.')
    if not list((directory / 'Static/reference-figures').glob('*.png')):
        raise ValueError('The application output is missing reference figure assets.')
    reference = (directory / 'Static/functions.xml').read_text(encoding='utf-8-sig')
    for image in set(re.findall(r'(reference-(?:equations|figures)/[0-9a-f]{64}\.png)', reference)):
        if not confined(directory, 'Static/' + image).is_file():
            raise ValueError('The mathematical reference links to a missing image: ' + image)
    runtime = ET.parse(directory / 'Computator.NET.exe.config').getroot().find('startup/supportedRuntime')
    if runtime is None or runtime.get('sku') != '.NETFramework,Version=v4.8':
        raise ValueError('Package the actual net48 Mono output, not a Windows .NET application.')


def source_identity():
    def git(*args):
        return subprocess.check_output(['git', '-C', str(ROOT), *args], text=True).strip()
    return {'repository': 'https://github.com/PawelTroka/Computator.NET',
            'commit': git('rev-parse', 'HEAD'), 'tree': git('rev-parse', 'HEAD^{tree}'),
            'trackedChanges': bool(git('status', '--porcelain', '--untracked-files=no'))}


def product_version():
    props = ET.parse(ROOT / 'Version.props').getroot()
    base = props.findtext('.//ComputatorBaseVersion')
    suffix = props.findtext('.//ComputatorPrerelease')
    return base + ('-' + suffix if suffix else '')


def inventory(root):
    return [{'path': path.relative_to(root).as_posix(), 'sha256': digest(path), 'size': path.stat().st_size}
            for path in payload_files(root)]


def write_manifest(root, metadata):
    metadata.update(schemaVersion=1, files=inventory(root))
    (root / MANIFEST).write_text(json.dumps(metadata, indent=2, sort_keys=True) + '\n', encoding='utf-8')


def verify(root):
    if root.is_symlink() or not root.is_dir():
        raise ValueError('Expected a regular package directory.')
    metadata = json.loads((root / MANIFEST).read_text(encoding='utf-8'))
    if metadata.get('schemaVersion') != 1:
        raise ValueError('Unsupported Mono package manifest.')
    expected = {}
    for entry in metadata['files']:
        path = confined(root, entry['path'])
        if entry['path'] in expected or not re.fullmatch('[0-9a-f]{64}', entry['sha256']):
            raise ValueError('Invalid or duplicated inventory entry.')
        if not path.is_file() or path.stat().st_size != entry['size'] or digest(path) != entry['sha256']:
            raise ValueError('Missing or changed package file: ' + entry['path'])
        expected[entry['path']] = entry
    actual = {p.relative_to(root).as_posix() for p in payload_files(root)}
    if actual != set(expected):
        raise ValueError('Package inventory does not describe every payload file.')
    application_contract(root / 'app')
    native = metadata.get('native', {})
    if native:
        required = {'gsl', 'cblas', 'gdiplus'}
        allowed = required | {'cairo', 'x11', 'xtst', 'xinerama', 'xext', 'xcursor', 'xfixes'}
        if metadata.get('rid') not in RIDS or not required.issubset(native) or not set(native).issubset(allowed):
            raise ValueError('Incomplete native runtime selection.')
        for name in native.values():
            if name not in expected:
                raise ValueError('Selected native library is absent from the inventory: ' + name)
        if metadata['rid'].startswith('linux-'):
            for name in native.values():
                check_elf(confined(root, name), metadata['rid'])
    return metadata


def check_elf(path, rid):
    with path.open('rb') as stream:
        header = stream.read(20)
    # Linux x86_64, 64-bit little-endian ELF shared object; reject architecture substitution.
    if (rid != 'linux-x64' or len(header) != 20 or header[:6] != b'\x7fELF\x02\x01'
            or struct.unpack('<H', header[18:20])[0] != 62):
        raise ValueError('Native library is not the requested Linux x86_64 ELF: ' + str(path))


def gsl_contract(directory, rid):
    metadata = json.loads((directory / 'gsl-provenance.json').read_text(encoding='utf-8'))
    pins = json.loads((ROOT / 'build/native/gsl-source.json').read_text())
    if (metadata.get('schemaVersion') != 1 or metadata.get('version') != '2.8'
            or metadata.get('rid') != rid or metadata.get('source') != pins):
        raise ValueError('GSL provenance does not match the reviewed source pin and requested architecture.')
    for name in ('COPYING.GSL', 'SOURCE.GSL.txt', 'GSL-2.8-source.zip', metadata['entryPoint'], 'libgslcblas.so'):
        if name not in metadata['files']:
            raise ValueError('GSL provenance lacks a required binary or corresponding source: ' + name)
    for name, expected in metadata['files'].items():
        relative_path(name)
        if '/' in name or not (directory / name).is_file() or digest(directory / name) != expected:
            raise ValueError('GSL provenance hash mismatch: ' + name)
    if {p.name for p in payload_files(directory)} != set(metadata['files']) | {'gsl-provenance.json'}:
        raise ValueError('Unrecorded files in the GSL candidate.')
    return metadata


def stage(args):
    destination = args.output.absolute()
    application = (args.application_directory or ROOT / f'Computator.NET/bin/mono/{args.configuration}/net48').resolve()
    if destination.is_symlink() or (destination.exists() and any(destination.iterdir())):
        raise ValueError('Output must be absent or an empty regular directory; existing data is never replaced.')
    if destination.resolve().is_relative_to(application) or application.is_relative_to(destination.resolve()):
        raise ValueError('Package output and application input must not overlap.')
    application_contract(application)
    destination.mkdir(parents=True, exist_ok=True)
    copy_tree(application, destination / 'app')
    preflight = ROOT / f'build/mono/Preflight/bin/mono/{args.configuration}/net48'
    if preflight.is_dir():
        for name in ('Computator.NET.Mono.Preflight.exe', 'Computator.NET.Mono.Preflight.exe.config'):
            if (preflight / name).is_file():
                shutil.copy2(preflight / name, destination / 'app' / name)
    (destination / 'share').mkdir()
    shutil.copy2(ROOT / 'LICENSE', destination / 'share/COPYING.Computator.NET')
    shutil.copy2(ROOT / 'Graphics/computator.net-icon.png', destination / 'share/computator.png')
    identity = source_identity()
    source = destination / 'share/Computator.NET-source.tar.gz'
    with source.open('wb') as stream:
        subprocess.run(['git', '-C', str(ROOT), 'archive', '--format=tar.gz', '--prefix=Computator.NET/',
                        identity['commit']], stdout=stream, check=True)
    metadata = {'version': product_version(), 'source': identity, 'native': {},
                'application': {'files': len(list(payload_files(destination / 'app'))),
                                'equationAssets': len(list((destination / 'app/Static/reference-equations').glob('*.png'))),
                                'figureAssets': len(list((destination / 'app/Static/reference-figures').glob('*.png'))),
                                'functionsXmlSha256': digest(destination / 'app/Static/functions.xml')}}
    write_manifest(destination, metadata)
    return metadata


def finalize(args):
    root = args.output.resolve()
    metadata = json.loads((root / MANIFEST).read_text(encoding='utf-8'))
    native = dict(metadata.get('native', {}))
    native.update(gsl=args.gsl, cblas=args.cblas, gdiplus=args.gdiplus)
    metadata.update(rid=args.rid, native=native)
    for name in metadata['native'].values():
        if not confined(root, name).is_file():
            raise ValueError('Selected native library is missing: ' + name)
    write_manifest(root, metadata)
    verify(root)
    return metadata


def archive(root, target):
    verify(root)
    if target.exists() or target.resolve().is_relative_to(root.resolve()):
        raise ValueError('Archive must be a new file outside the package root.')
    target.parent.mkdir(parents=True, exist_ok=True)
    # Fixed metadata and gzip header make rebuilding one staged payload reproducible.
    with target.open('xb') as raw, gzip.GzipFile(filename='', mode='wb', fileobj=raw, mtime=0) as zipped:
        with tarfile.open(fileobj=zipped, mode='w|', format=tarfile.PAX_FORMAT) as tar:
            for path in sorted(root.rglob('*')):
                if not path.is_file():
                    continue
                info = tar.gettarinfo(str(path), path.relative_to(root).as_posix())
                info.uid = info.gid = info.mtime = 0
                info.uname = info.gname = ''
                info.mode = 0o755 if path.stat().st_mode & stat.S_IXUSR else 0o644
                with path.open('rb') as stream:
                    tar.addfile(info, stream)
    return digest(target)


def extract(target, output):
    if output.is_symlink() or output.exists():
        raise ValueError('Extraction output must not exist.')
    with tarfile.open(target, 'r:gz') as tar:
        members = tar.getmembers()
        names = set()
        for member in members:
            relative_path(member.name)
            if not member.isfile() or member.name in names:
                raise ValueError('Only unique regular files are allowed in the distribution archive.')
            names.add(member.name)
        # Inventory validation follows before any executable from the payload is used.
        output.mkdir(parents=True)
        tar.extractall(output, members=members, filter='data')
    verify(output)


def linux_package(args):
    if args.rid != 'linux-x64' or not all((args.native_directory, args.gdiplus_directory, args.baseline)):
        raise ValueError('Linux packaging requires --rid linux-x64, --native-directory, --gdiplus-directory and --baseline.')
    gsl = gsl_contract(args.native_directory, args.rid)
    gdiplus = json.loads((args.gdiplus_directory / 'gdiplus-provenance.json').read_text())
    gdiplus_pins = json.loads((ROOT / 'build/mono/gdiplus-source.json').read_text())
    if (gdiplus.get('schemaVersion') != 1 or gdiplus.get('version') != gdiplus_pins['version']
            or gdiplus.get('rid') != args.rid or not gdiplus.get('pango') or gdiplus.get('baseline') != args.baseline
            or gdiplus.get('source') != gdiplus_pins):
        raise ValueError('libgdiplus must be a matching source-pinned Pango build for the selected distro baseline.')
    for name, expected in gdiplus['files'].items():
        path = confined(args.gdiplus_directory, name)
        if not path.is_file() or digest(path) != expected:
            raise ValueError('libgdiplus provenance hash mismatch: ' + name)
    if {p.relative_to(args.gdiplus_directory).as_posix() for p in payload_files(args.gdiplus_directory)} != set(gdiplus['files']) | {'gdiplus-provenance.json'}:
        raise ValueError('Unrecorded files in the libgdiplus candidate.')
    metadata = stage(args)
    root = args.output
    if not (root / 'app/Computator.NET.Mono.Preflight.exe').is_file():
        raise ValueError('Build the Mono runtime preflight before producing a Linux package.')
    native = root / 'native' / args.rid
    copy_tree(args.native_directory, native / 'gsl')
    copy_tree(args.gdiplus_directory, native / 'gdiplus')
    (root / 'bin').mkdir()
    for source, target in [('launcher.sh', 'bin/computator'), ('install-user.sh', 'bin/install-desktop-entry')]:
        shutil.copyfile(ROOT / 'build/mono' / source, root / target)
        (root / target).chmod(0o755)
    shutil.copyfile(ROOT / 'build/mono/LINUX-PACKAGE.txt', root / 'README.txt')
    metadata.update(rid=args.rid, baseline=args.baseline,
                    native={'gsl': f'native/{args.rid}/gsl/' + gsl['entryPoint'],
                            'cblas': f'native/{args.rid}/gsl/libgslcblas.so',
                            'gdiplus': f'native/{args.rid}/gdiplus/' + gdiplus['entryPoint']},
                    runtime={'monoMinimum': '6.8', 'framework': 'net48', 'display': 'X11 or XWayland',
                             'dependencies': gdiplus['dependencies']})
    write_manifest(root, metadata)
    verify(root)
    return metadata


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    actions = parser.add_mutually_exclusive_group()
    actions.add_argument('--stage-only', action='store_true')
    actions.add_argument('--finalize', action='store_true')
    actions.add_argument('--verify', action='store_true')
    actions.add_argument('--extract', type=Path)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--configuration', choices=('Release', 'Debug'), default='Release')
    parser.add_argument('--application-directory', type=Path)
    parser.add_argument('--rid', choices=RIDS)
    parser.add_argument('--native-directory', type=Path)
    parser.add_argument('--gdiplus-directory', type=Path)
    parser.add_argument('--baseline')
    parser.add_argument('--gsl')
    parser.add_argument('--cblas')
    parser.add_argument('--gdiplus')
    parser.add_argument('--archive', type=Path)
    args = parser.parse_args()
    if args.extract:
        extract(args.extract, args.output)
    elif args.verify:
        verify(args.output)
    elif args.finalize:
        if not args.rid or not all((args.gsl, args.cblas, args.gdiplus)):
            parser.error('--finalize requires --rid --gsl --cblas --gdiplus')
        finalize(args)
    elif args.stage_only:
        stage(args)
    else:
        linux_package(args)
    if args.archive:
        print('Archive SHA256:', archive(args.output, args.archive))
    print('Verified Mono package:', args.output)


if __name__ == '__main__':
    try:
        main()
    except (OSError, ValueError, KeyError, ET.ParseError, subprocess.CalledProcessError) as error:
        print('Mono packaging failed:', error, file=sys.stderr)
        sys.exit(1)
