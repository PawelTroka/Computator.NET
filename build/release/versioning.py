"""Read one product identity; validate or update its checked-in compatibility attributes."""
from __future__ import annotations
import argparse
from datetime import date, datetime
import json
from pathlib import Path
import re
import urllib.request
import xml.etree.ElementTree as ET

START = '// BEGIN GENERATED PRODUCT VERSION'
END = '// END GENERATED PRODUCT VERSION'
STAMP = 'ComputatorVersionTimestamp'
RELEASES = 'https://api.github.com/repos/PawelTroka/Computator.NET/releases'


def timestamp(value: str | datetime | None = None) -> datetime:
    """One operator-local instant; downstream builds never consult their clocks."""
    if value is None:
        value = datetime.now().astimezone()
    elif isinstance(value, str):
        value = datetime.fromisoformat(value.replace('Z', '+00:00'))
    if not isinstance(value, datetime) or value.utcoffset() is None:
        raise ValueError('A release timestamp must include its UTC offset.')
    return value.replace(microsecond=0)


def wildcard_components(value: datetime) -> tuple[int, int]:
    # Match Roslyn VersionHelper: local calendar date and wall-clock TimeOfDay,
    # not elapsed UTC seconds across a daylight-saving transition.
    build = (value.date() - date(2000, 1, 1)).days
    revision = (value.hour * 3600 + value.minute * 60 + value.second) // 2
    if not 0 <= build <= 65534:
        raise ValueError('The wildcard build date is outside the assembly version range.')
    return build, revision


def projected_identity(base: str, revision: str, label: str, frozen: str = '') -> dict:
    if not re.fullmatch(r'(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)', base):
        raise ValueError('The base version must have three canonical numeric components.')
    if not re.fullmatch(r'0|[1-9][0-9]*', revision):
        raise ValueError('Revision must be a canonical non-negative integer.')
    major, minor, build = map(int, base.split('.'))
    if major > 255 or minor > 255 or build > 65534 or int(revision) > 65534:
        raise ValueError('Version exceeds the shared MSI/assembly/MSIX numeric limits.')
    if major == 0:
        raise ValueError('The shared Store identity requires a positive major version.')
    if label and not re.fullmatch(r'[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*', label):
        raise ValueError('Invalid prerelease label.')
    numeric = base + '.' + revision
    values = {'productVersion': numeric + (('-' + label) if label else ''),
              'assemblyVersion': numeric, 'fileVersion': numeric,
              'msiVersion': base, 'msixVersion': base + '.0',
              'macBundleVersion': f'{major}.{build}.{revision}',
              'macBundleShortVersion': base, 'candidate': bool(label)}
    if frozen:
        instant = timestamp(frozen)
        if wildcard_components(instant) != (build, int(revision)):
            raise ValueError('Frozen release timestamp differs from the numeric build/revision.')
        values['versionTimestamp'] = instant.isoformat(timespec='seconds')
    return values

def identity(root: Path) -> dict:
    document = ET.parse(root / 'Version.props')
    def required(name: str) -> str:
        values = document.findall('.//' + name)
        if len(values) != 1:
            raise ValueError(f'Expected exactly one {name}.')
        return (values[0].text or '').strip()
    base, revision, label = (required(name) for name in ('ComputatorBaseVersion', 'ComputatorRevision', 'ComputatorPrerelease'))
    frozen = document.findall('.//' + STAMP)
    if len(frozen) > 1:
        raise ValueError('Expected at most one frozen release timestamp.')
    return projected_identity(base, revision, label, (frozen[0].text or '').strip() if frozen else '')


def published_release_guard(values: dict, releases: list) -> bool:
    """A second public build on one local day cannot upgrade MSI or Store."""
    if not isinstance(releases, list):
        raise ValueError('Published-release input must be a JSON list of GitHub releases.')
    candidate = tuple(map(int, values['msiVersion'].split('.')))
    exact_retry = False
    for release in releases:
        if not isinstance(release, dict):
            raise ValueError('Invalid published-release record.')
        # Tags, CI runs and draft releases are not distributed versions.
        if release.get('draft') is not False or not release.get('published_at'):
            continue
        tag = release.get('tag_name', '')
        if not isinstance(tag, str):
            raise ValueError('Invalid published release tag.')
        product = tag.removeprefix('v')
        match = re.fullmatch(r'((?:0|[1-9][0-9]*)\.(?:0|[1-9][0-9]*)\.(?:0|[1-9][0-9]*))(?:\.(?:0|[1-9][0-9]*))?(?:-[0-9A-Za-z.-]+)?', product)
        if not match:
            continue
        published = tuple(map(int, match[1].split('.')))
        if published > candidate:
            raise ValueError('The release clock would downgrade an already published package version.')
        if published == candidate and product != values['productVersion']:
            raise ValueError('A different release is already published for this local day: MSI/MSIX versions would collide. Reuse the exact frozen release for a retry or prepare on a later local date.')
        if published == candidate:
            exact_retry = True
    return exact_retry


def load_published_releases(path: Path | None = None) -> list:
    if path is not None:
        return json.loads(path.read_text(encoding='utf-8-sig'))
    releases = []
    for page in range(1, 101):
        request = urllib.request.Request(f'{RELEASES}?per_page=100&page={page}',
            headers={'Accept': 'application/vnd.github+json', 'User-Agent': 'Computator.NET-release-prepare'})
        with urllib.request.urlopen(request, timeout=30) as response:
            content = response.read(4 * 1024 * 1024 + 1)
        if len(content) > 4 * 1024 * 1024:
            raise ValueError('Published-release response exceeds its bounded size.')
        batch = json.loads(content)
        if not isinstance(batch, list):
            raise ValueError('GitHub did not return a release list.')
        releases.extend(batch)
        if len(batch) < 100:
            return releases
    raise ValueError('Published-release pagination exceeded its bounded limit.')


def prepare(root: Path, at: str | datetime | None = None, published_releases: list | None = None) -> dict:
    old = check(root)  # Never repair unrelated source/version drift while stamping.
    instant = timestamp(at)
    build, revision = wildcard_components(instant)
    major, minor, _ = old['msiVersion'].split('.')
    document = ET.parse(root / 'Version.props')
    label = (document.findtext('.//ComputatorPrerelease') or '').strip()
    values = projected_identity(f'{major}.{minor}.{build}', str(revision), label,
                                instant.isoformat(timespec='seconds'))
    if old.get('versionTimestamp') and instant < timestamp(old['versionTimestamp']):
        raise ValueError('The release clock would move the frozen timestamp backwards.')
    if tuple(map(int, values['assemblyVersion'].split('.'))) < tuple(map(int, old['assemblyVersion'].split('.'))):
        raise ValueError('The release clock would move the frozen assembly version backwards.')
    exact_retry = published_release_guard(values, load_published_releases() if published_releases is None else published_releases)
    if exact_retry:
        if (old['productVersion'] != values['productVersion']
                or old.get('versionTimestamp') != values['versionTimestamp']):
            raise ValueError('A published identity may only be retried with its original frozen source identity and timestamp.')
        return old  # Preserve both source files byte-for-byte, including formatting.
    text = (root / 'Version.props').read_text(encoding='utf-8-sig')
    for name, value in [('ComputatorBaseVersion', values['msiVersion']),
                        ('ComputatorRevision', str(revision)), (STAMP, values['versionTimestamp'])]:
        pattern = re.compile(r'(<'+name+r'>)[^<]*(</'+name+r'>)')
        if len(pattern.findall(text)) != 1:
            raise ValueError('Release preparation requires exactly one literal ' + name + '.')
        text = pattern.sub(lambda match: match[1] + value + match[2], text)
    shared = (root / 'SharedAssemblyInfo.cs').read_text(encoding='utf-8-sig')
    shared = re.sub(re.escape(START) + r'.*?' + re.escape(END),
                    lambda _: expected_block(values), shared, flags=re.DOTALL)
    # All validation and network reads finish before either tracked file changes.
    (root / 'Version.props').write_text(text, encoding='utf-8')
    (root / 'SharedAssemblyInfo.cs').write_text(shared, encoding='utf-8')
    return check(root)

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
    action = parser.add_mutually_exclusive_group()
    action.add_argument('--write', action='store_true', help='Explicitly regenerate checked-in compatibility attributes; normal builds never write source.')
    action.add_argument('--prepare', action='store_true', help='Freeze classic major.minor.* values once; review and commit before CI.')
    parser.add_argument('--timestamp', help='Canonical operator-local ISO 8601 timestamp with UTC offset; defaults to one local now().')
    parser.add_argument('--published-releases', type=Path, help='Controller-fetched GitHub release JSON; otherwise preparation fetches public releases.')
    args = parser.parse_args()
    if not args.prepare and (args.timestamp is not None or args.published_releases is not None):
        parser.error('--timestamp and --published-releases require --prepare.')
    values = (prepare(args.root.resolve(), args.timestamp,
              load_published_releases(args.published_releases) if args.published_releases else None)
              if args.prepare else check(args.root.resolve(), args.write))
    print(json.dumps(values, indent=2))
