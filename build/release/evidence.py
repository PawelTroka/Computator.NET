"""Record fresh, commit-bound candidate evidence without signing or publishing anything."""
from __future__ import annotations
import argparse
from datetime import datetime
import hashlib
import json
import math
from pathlib import Path
import platform
import subprocess
import time
import xml.etree.ElementTree as ET
from versioning import check

REPORTS = {
    'TestResults/Computator.NET.Core.Tests.trx': 2118,
    'TestResults/Computator.NET.Core.IntegrationTests.trx': 947,
    'TestResults/Computator.NET.Desktop.Tests.trx': 551,
    'TestResults/Computator.NET.WebApi.IntegrationTests.trx': 130,
    'TestResults/portable.trx': 1438,
    'TestResults/reference.trx': 166,
    'Computator.NET.WebClient/test-results/results.xml': 242,
    'Computator.NET.WebClient/test-results/e2e.xml': 10,
    'Computator.NET.WebClient/test-results/custom-functions.xml': 26,
}

GSL_CHECKS = {'gamma': 2.0, 'gammaP': 1 - 2 / math.e,
              'AiPrime': -0.2588194037928068, 'BesselJ0': 1.0}


def required_gsl_directories(root: Path) -> list[str]:
    directories = ['artifacts/desktop', 'artifacts/api', 'artifacts/distribution/win-x64']
    if (root / 'build/followups/local-web.cake').is_file():
        directories.append('artifacts/local-web/api')
    return directories


def required_reports(root: Path) -> dict[str, int]:
    reports = dict(REPORTS)
    if (root / 'build/followups/local-web.cake').is_file():
        reports['Computator.NET.WebClient/test-results/local-package.xml'] = 6
    return reports


def required_artifacts(root: Path, version: dict) -> list[str]:
    framework = ET.parse(root / 'build/ModernDesktop.props').find('.//TargetFramework').text
    assembly = version['assemblyVersion']
    artifacts = [f'publish/Computator.NET.v{assembly}.{framework}.zip',
        f'publish/Computator.NET.WebApi.v{assembly}.{framework}.zip',
        f'publish/Computator.NET.WebClient.v{assembly}.net10.0.zip',
        f'artifacts/msix/Computator.NET.{version["msixVersion"]}.win-x64.msix',
        'artifacts/msix/package-validation.json',
        'artifacts/installer/output/Computator.NET.reviewed.wxs',
        'artifacts/installer/output/payload.xml',
        'artifacts/reference/inventory.json', 'artifacts/reference/coverage.json',
        'artifacts/reference/preview.html']
    if (root / 'build/followups/local-web.cake').is_file():
        artifacts.append('publish/Computator.NET.LocalWeb.win-x64.zip')
    return artifacts


def fresh_file(root: Path, relative: str, started: float) -> Path:
    path = root / relative
    if not path.resolve().is_relative_to(root.resolve()):
        raise ValueError('Candidate inventory must not follow paths outside the checkout.')
    if not path.is_file() or path.stat().st_mtime < started - 2:
        raise ValueError(f'Missing/stale release output: {relative}')
    return path

def git(root: Path, *arguments: str) -> str:
    return subprocess.check_output(['git', '-C', str(root), *arguments], text=True).strip()

def digest(path: Path) -> str:
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def packaged_gsl_evidence(root: Path, started: float) -> dict:
    """Bind local probe observations to current files, not an authenticity attestation."""
    root = root.resolve()
    directories = required_gsl_directories(root)
    def same_path(value: object, expected: Path) -> bool:
        return isinstance(value, str) and Path(value).is_absolute() and Path(value).resolve() == expected.resolve()

    latest = {}
    for path in (root / 'TestResults/packaged-gsl').glob('*/result.json'):
        if path.stat().st_mtime < started - 2:
            continue
        relative = path.relative_to(root).as_posix()
        fresh_file(root, relative, started)
        data = json.loads(path.read_text(encoding='utf-8-sig'))
        if not isinstance(data, dict):
            raise ValueError(f'Invalid packaged GSL report: {relative}')
        # Compare absolute, normalized paths produced by the probe. In particular,
        # another checkout or another payload cannot supply this output's report.
        published = data.get('publishedDirectory')
        for directory in directories:
            if same_path(published, root / directory):
                previous = latest.get(directory)
                if previous is None or path.stat().st_mtime_ns > previous[0].stat().st_mtime_ns:
                    latest[directory] = (path, data)

    def payload_file(relative: str) -> Path:
        path = root / relative
        if not path.is_file() or not path.resolve().is_relative_to(root):
            raise ValueError(f'Missing/outside-checkout packaged GSL file: {relative}')
        return path

    def matching_hash(value: object, path: Path) -> bool:
        return isinstance(value, str) and value.lower() == digest(path)

    evidence = {}
    for directory in directories:
        if directory not in latest:
            raise ValueError(f'Missing/stale packaged GSL report for {directory}')
        path, data = latest[directory]
        report = path.relative_to(root).as_posix()
        if type(data.get('schemaVersion')) is not int or data['schemaVersion'] != 1 or data.get('passed') is not True or data.get('failure') is not None:
            raise ValueError(f'Failed/invalid packaged GSL report: {report}')
        try:
            completed = datetime.fromisoformat(data['completedUtc'])
            if completed.tzinfo is None or not started <= completed.timestamp() <= time.time() + 2:
                raise ValueError()
        except (KeyError, TypeError, ValueError, OverflowError) as error:
            raise ValueError(f'Stale/invalid packaged GSL completion time: {report}') from error
        native = directory + '/native/gsl/gsl.dll'
        library = payload_file(native)
        if any(not same_path(data.get(key), library) for key in ('expectedLibrary', 'selectedLibrary')):
            raise ValueError(f'Wrong packaged GSL library path: {report}')
        if data.get('nativeVersion') != '2.8' or not matching_hash(data.get('nativeHash'), library):
            raise ValueError(f'Wrong packaged GSL version/hash: {report}')
        checks = data.get('checks')
        for name, expected in GSL_CHECKS.items():
            actual = checks.get(name) if isinstance(checks, dict) else None
            try:
                valid = type(actual) in (int, float) and math.isfinite(actual) and math.isclose(actual, expected, rel_tol=0, abs_tol=1e-12)
            except OverflowError:
                valid = False
            if not valid:
                raise ValueError(f'Missing/incorrect packaged GSL numerical check {name}: {report}')
        provenance = directory + '/native/gsl/gsl-provenance.json'
        provenance_file = payload_file(provenance)
        inventory = json.loads(provenance_file.read_text(encoding='utf-8-sig'))
        files = inventory.get('files') if isinstance(inventory, dict) else None
        if (not isinstance(inventory, dict) or type(inventory.get('schemaVersion')) is not int or inventory['schemaVersion'] != 1 or
                inventory.get('version') != '2.8' or inventory.get('rid') != 'win-x64' or inventory.get('entryPoint') != 'gsl.dll' or
                not isinstance(files, dict) or not {'gsl.dll', 'gslcblas.dll', 'COPYING.GSL',
                                                   'GSL-2.8-source.zip', 'SOURCE.GSL.txt'} <= files.keys()):
            raise ValueError(f'Invalid packaged GSL provenance: {provenance}')
        # The inventory may grow with additional source and build-recipe files.
        # Verify every declared file without imposing a fixed payload count.
        for name, expected_hash in files.items():
            if not isinstance(name, str) or name in ('', '.', '..') or '/' in name or '\\' in name:
                raise ValueError(f'Invalid packaged GSL provenance filename: {provenance}')
            payload = payload_file(directory + '/native/gsl/' + name)
            if not matching_hash(expected_hash, payload):
                raise ValueError(f'Packaged GSL provenance hash mismatch: {provenance}: {name}')
        evidence[directory] = {'report': report, 'reportSha256': digest(path),
            'nativeLibrary': native, 'nativeVersion': '2.8', 'nativeSha256': digest(library),
            'provenance': provenance, 'provenanceSha256': digest(provenance_file),
            'checks': {name: checks[name] for name in GSL_CHECKS}}
    return evidence


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
        identifiers = [node.get('testId') for node in cases]
        definitions = {node.get('id') for node in tree.iter() if local(node) == 'UnitTest'}
        summary = next((node for node in tree.iter() if local(node) == 'ResultSummary'), None)
        counters = next((node for node in tree.iter() if local(node) == 'Counters'), None)
        if (summary is None or summary.get('outcome') not in ('Passed', 'Completed') or
                counters is None or int(counters.get('total', '-1')) != len(cases) or
                int(counters.get('passed', '-1')) != len(cases) - len(skipped) or
                any(not identity or identity not in definitions for identity in identifiers)):
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
        if minimum >= 10 or path.name == 'local-package.xml':
            expected = {'chromium', 'firefox'} if path.name in ('results.xml', 'local-package.xml') else {'real-chromium', 'real-firefox'}
            if {item[0] for item in identifiers} != expected:
                raise ValueError(f'Missing or unexpected browser projects in {path.name}.')
    if len(cases) < minimum or len(set(identifiers)) != len(cases):
        raise ValueError(f'Missing or duplicated test identities in {path.name}.')
    return {'cases': len(cases), 'passed': len(cases) - len(skipped), 'documentedSkips': len(skipped), 'sha256': digest(path)}

def begin(root: Path) -> None:
    if git(root, 'status', '--porcelain', '--untracked-files=normal'):
        raise ValueError('Release evidence requires a clean source tree, including no untracked source files.')
    directory = root / 'artifacts/release-evidence'
    directory.mkdir(parents=True, exist_ok=True)
    for relative in required_reports(root):
        (root / relative).unlink(missing_ok=True)
    (directory / 'manifest.json').unlink(missing_ok=True)
    start = {'commit': git(root, 'rev-parse', 'HEAD'), 'startedUnix': time.time(),
             'version': check(root), 'platform': platform.platform(), 'machine': platform.machine()}
    (directory / 'started.json').write_text(json.dumps(start, indent=2), encoding='utf-8')

def finish(root: Path) -> None:
    directory = root / 'artifacts/release-evidence'
    (directory / 'manifest.json').unlink(missing_ok=True)
    start = json.loads((directory / 'started.json').read_text(encoding='utf-8-sig'))
    if git(root, 'rev-parse', 'HEAD') != start['commit'] or git(root, 'status', '--porcelain', '--untracked-files=normal'):
        raise ValueError('The source revision changed during verification.')
    reports = {}
    for relative, minimum in required_reports(root).items():
        path = fresh_file(root, relative, start['startedUnix'])
        reports[relative] = validate_report(path, minimum)
    artifacts = {}
    for relative in required_artifacts(root, start['version']):
        path = fresh_file(root, relative, start['startedUnix'])
        artifacts[relative] = {'sha256': digest(path), 'bytes': path.stat().st_size}
    gsl = packaged_gsl_evidence(root, start['startedUnix'])
    for observation in gsl.values():
        for key in ('report', 'provenance'):
            relative = observation[key]
            path = root / relative
            artifacts[relative] = {'sha256': digest(path), 'bytes': path.stat().st_size}
    packages = {}
    # Inventory only the supported project graph restored by this run. Old obj
    # trees from historical projects must not contaminate candidate provenance.
    solution = ET.parse(root / 'Computator.NET.slnx')
    for project in solution.iter('Project'):
        relative = str(Path(project.get('Path')).parent / 'obj/project.assets.json').replace('\\', '/')
        # NuGet legitimately keeps an unchanged assets file on incremental
        # restore. Freshness is required for test reports and produced packages,
        # while this inventory records the graph consumed by those builds.
        path = root / relative
        data = json.loads(path.read_text(encoding='utf-8-sig'))
        for name, item in data.get('libraries', {}).items():
            if item.get('type') == 'package':
                packages[name] = item.get('sha512')
    manifest = {**start, 'completedUnix': time.time(), 'reports': reports, 'artifacts': artifacts,
                'resolvedNuGetPackages': dict(sorted(packages.items())), 'packagedGsl': gsl,
                'packagedGslScope': 'Fresh local probe observations bound to current payload/provenance hashes; self-reported results, not independent authenticity or cryptographic trust.',
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
