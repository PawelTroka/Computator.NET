"""Record fresh, commit-bound candidate evidence without signing or publishing anything."""
from __future__ import annotations
import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import subprocess
import time
import xml.etree.ElementTree as ET
from versioning import check

REPORTS = {
    'TestResults/Computator.NET.Core.Tests.trx': 252,
    'TestResults/Computator.NET.Core.IntegrationTests.trx': 947,
    'TestResults/Computator.NET.Desktop.Tests.trx': 20,
    'TestResults/Computator.NET.WebApi.IntegrationTests.trx': 82,
    'TestResults/portable.trx': 120,
    'Computator.NET.WebClient/test-results/results.xml': 16,
    'Computator.NET.WebClient/test-results/e2e.xml': 10,
    'Computator.NET.WebClient/test-results/custom-functions.xml': 12,
}

def git(root: Path, *arguments: str) -> str:
    return subprocess.check_output(['git', '-C', str(root), *arguments], text=True).strip()

def digest(path: Path) -> str:
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()

def validate_report(path: Path, minimum: int) -> dict:
    tree = ET.parse(path)
    local = lambda element: element.tag.rsplit('}', 1)[-1]
    if local(tree.getroot()) == 'TestRun':
        cases = [node for node in tree.iter() if local(node) == 'UnitTestResult']
        failures = [node for node in cases if node.get('outcome') not in ('Passed', 'NotExecuted')]
        skipped = [node for node in cases if node.get('outcome') == 'NotExecuted']
        permitted = path.name == 'Computator.NET.WebApi.IntegrationTests.trx'
        if failures or any(not permitted or node.get('testName') != 'ReturnsHttpOkForChart3D' for node in skipped):
            raise ValueError(f'Failed/undocumented skipped test in {path.name}.')
        identifiers = [node.get('executionId') for node in cases]
        counters = next((node for node in tree.iter() if local(node) == 'Counters'), None)
        if counters is None or int(counters.get('total', '-1')) != len(cases):
            raise ValueError(f'Incomplete TRX counters in {path.name}.')
    else:
        cases = list(tree.iter('testcase'))
        if any(node.find('failure') is not None or node.find('error') is not None or node.find('skipped') is not None for node in cases):
            raise ValueError(f'Failed/skipped browser test in {path.name}.')
        skipped = []
        parents = {child: parent for parent in tree.iter() for child in parent}
        # Playwright uses testsuite.hostname for project identity. Chromium and
        # Firefox intentionally run the same names; duplicates within a project
        # remain invalid. Small synthetic unit reports need not name a browser.
        identifiers = [(parents[node].get('hostname'), parents[node].get('name'),
                        node.get('classname'), node.get('name')) for node in cases]
        if minimum >= 10:
            expected = {'chromium', 'firefox'} if path.name == 'results.xml' else {'real-chromium', 'real-firefox'}
            if {item[0] for item in identifiers} != expected:
                raise ValueError(f'Missing or unexpected browser projects in {path.name}.')
    if len(cases) < minimum or len(set(identifiers)) != len(cases):
        raise ValueError(f'Missing or duplicated test identities in {path.name}.')
    return {'cases': len(cases), 'passed': len(cases) - len(skipped), 'documentedSkips': len(skipped), 'sha256': digest(path)}

def begin(root: Path) -> None:
    if git(root, 'status', '--porcelain', '--untracked-files=no'):
        raise ValueError('Release evidence requires an unchanged tracked source tree.')
    directory = root / 'artifacts/release-evidence'
    directory.mkdir(parents=True, exist_ok=True)
    for relative in REPORTS:
        (root / relative).unlink(missing_ok=True)
    (directory / 'manifest.json').unlink(missing_ok=True)
    start = {'commit': git(root, 'rev-parse', 'HEAD'), 'startedUnix': time.time(),
             'version': check(root), 'platform': platform.platform(), 'machine': platform.machine()}
    (directory / 'started.json').write_text(json.dumps(start, indent=2), encoding='utf-8')

def finish(root: Path) -> None:
    directory = root / 'artifacts/release-evidence'
    start = json.loads((directory / 'started.json').read_text(encoding='utf-8-sig'))
    if git(root, 'rev-parse', 'HEAD') != start['commit'] or git(root, 'status', '--porcelain', '--untracked-files=no'):
        raise ValueError('The source revision changed during verification.')
    reports = {}
    for relative, minimum in REPORTS.items():
        path = root / relative
        if not path.is_file() or path.stat().st_mtime < start['startedUnix'] - 2:
            raise ValueError(f'Missing/stale release report: {relative}')
        reports[relative] = validate_report(path, minimum)
    artifacts = {}
    for folder in ('publish', 'artifacts/msix', 'artifacts/installer/output'):
        directory_path = root / folder
        if not directory_path.is_dir():
            raise ValueError(f'Missing candidate output: {folder}')
        for path in sorted(directory_path.rglob('*')):
            if path.is_file() and path.suffix.lower() in ('.zip', '.msix', '.wxs', '.xml', '.json'):
                if not path.resolve().is_relative_to(root):
                    raise ValueError('Candidate inventory must not follow paths outside the checkout.')
                artifacts[str(path.relative_to(root)).replace(os.sep, '/')] = {'sha256': digest(path), 'bytes': path.stat().st_size}
    if not any(name.endswith('.msix') for name in artifacts) or sum(name.endswith('.zip') for name in artifacts) < 3:
        raise ValueError('Required ZIP/MSIX candidate outputs are missing.')
    packages = {}
    for path in root.glob('**/obj/project.assets.json'):
        data = json.loads(path.read_text(encoding='utf-8-sig'))
        for name, item in data.get('libraries', {}).items():
            if item.get('type') == 'package':
                packages[name] = item.get('sha512')
    manifest = {**start, 'completedUnix': time.time(), 'reports': reports, 'artifacts': artifacts,
                'resolvedNuGetPackages': dict(sorted(packages.items())),
                'scope': 'Windows candidate verification on this host; not cross-OS, clean-machine, signed-install or Store certification.',
                'published': False, 'signed': False,
                'remainingAcceptance': ['clean OS without SDK', 'historical installer/profile upgrade and rollback',
                    'owner signing', 'Store activation/certification', 'intended cross-platform final-commit jobs']}
    (directory / 'manifest.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    print(f'Wrote commit-bound evidence for {len(reports)} test reports and {len(artifacts)} candidate files.')

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('operation', choices=['begin', 'finish'])
    parser.add_argument('--root', type=Path, default=Path(__file__).resolve().parents[2])
    args = parser.parse_args()
    (begin if args.operation == 'begin' else finish)(args.root.resolve())
