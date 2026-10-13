"""Read one product identity; validate or update its checked-in compatibility attributes."""
from __future__ import annotations
import argparse
import json
from pathlib import Path
import re
import xml.etree.ElementTree as ET

START = '// BEGIN GENERATED PRODUCT VERSION'
END = '// END GENERATED PRODUCT VERSION'

def identity(root: Path) -> dict[str, str]:
    document = ET.parse(root / 'Version.props')
    def required(name: str) -> str:
        values = document.findall('.//' + name)
        if len(values) != 1:
            raise ValueError(f'Expected exactly one {name}.')
        return (values[0].text or '').strip()
    base, revision, label = (required(name) for name in ('ComputatorBaseVersion', 'ComputatorRevision', 'ComputatorPrerelease'))
    if not re.fullmatch(r'(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)', base):
        raise ValueError('The base version must have three canonical numeric components.')
    if not re.fullmatch(r'0|[1-9][0-9]*', revision):
        raise ValueError('Revision must be a canonical non-negative integer.')
    major, minor, patch = map(int, base.split('.'))
    if major > 255 or minor > 255 or patch > 65534 or int(revision) > 65534:
        raise ValueError('Version exceeds the shared MSI/assembly/MSIX numeric limits.')
    if major == 0 or int(revision) != 0:
        raise ValueError('The shared Store identity requires a positive major version and a zero fourth component.')
    if label and not re.fullmatch(r'[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*', label):
        raise ValueError('Invalid prerelease label.')
    full = base + (('-' + label) if label else '')
    return {'productVersion': full, 'assemblyVersion': base + '.' + revision,
            'fileVersion': base + '.' + revision, 'msiVersion': base, 'msixVersion': base + '.' + revision,
            'candidate': bool(label)}

def expected_block(values: dict) -> str:
    return '\n'.join((START,
        f'[assembly: AssemblyVersion("{values["assemblyVersion"]}")]',
        f'[assembly: AssemblyFileVersion("{values["fileVersion"]}")]',
        f'[assembly: AssemblyInformationalVersion("{values["productVersion"]}")]', END))

def check(root: Path, write: bool = False) -> dict:
    values = identity(root)
    path = root / 'SharedAssemblyInfo.cs'
    text = path.read_text(encoding='utf-8-sig')
    pattern = re.compile(re.escape(START) + r'.*?' + re.escape(END), re.DOTALL)
    if len(pattern.findall(text)) != 1:
        raise ValueError('Expected exactly one generated version block.')
    replacement = expected_block(values)
    if pattern.search(text).group() != replacement:
        if not write:
            raise ValueError('SharedAssemblyInfo.cs differs from Version.props. Run python build/release/versioning.py --write and review the diff.')
        path.write_text(pattern.sub(lambda _: replacement, text), encoding='utf-8')
    return values

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--root', type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument('--write', action='store_true', help='Explicitly regenerate checked-in compatibility attributes; normal builds never write source.')
    args = parser.parse_args()
    print(json.dumps(check(args.root.resolve(), args.write), indent=2))
