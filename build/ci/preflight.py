#!/usr/bin/env python3
"""Verify repo-pinned tools and record only allowlisted CI provenance, never secrets."""
from __future__ import annotations
import argparse
from datetime import datetime, timezone
import json
import os
from pathlib import Path
import platform
import re
import subprocess
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]


def capture(executable: str, *arguments: str) -> str:
    command = [executable, *arguments]
    if os.name == 'nt' and executable in {'npm', 'npx'}:
        command = ['cmd.exe', '/d', '/c', *command]
    return subprocess.check_output(command, cwd=ROOT, text=True, stderr=subprocess.STDOUT).strip()


def require_versions(expected: dict[str, str], actual: dict[str, str]) -> None:
    for name, version in expected.items():
        if actual.get(name) != version:
            raise ValueError(f'{name} differs from the repository manifest: {actual.get(name)} != {version}.')


def provider_metadata(provider: str, environment: dict[str, str]) -> dict[str, str]:
    if provider == 'local':
        return {'provider': 'local-validation', 'nativeProviderRun': 'false'}
    if provider == 'appveyor':
        if environment.get('APPVEYOR', '').lower() != 'true':
            raise ValueError('AppVeyor evidence requires an actual AppVeyor environment; do not synthesize provider flags in GitHub.')
        keys = ['APPVEYOR_BUILD_ID', 'APPVEYOR_JOB_ID', 'APPVEYOR_REPO_COMMIT', 'APPVEYOR_REPO_NAME', 'APPVEYOR_BUILD_WORKER_IMAGE']
    elif provider == 'travis':
        if environment.get('TRAVIS', '').lower() != 'true':
            raise ValueError('Travis evidence requires an actual Travis environment; do not synthesize provider flags in GitHub.')
        keys = ['TRAVIS_BUILD_ID', 'TRAVIS_JOB_ID', 'TRAVIS_COMMIT', 'TRAVIS_REPO_SLUG', 'TRAVIS_OS_NAME']
    else:
        raise ValueError('Unknown CI provider.')
    if any(not environment.get(key) for key in keys):
        raise ValueError('Native CI evidence is missing provider-supplied build metadata.')
    result = {'provider': provider, 'nativeProviderRun': 'true'}
    result.update({key: environment[key] for key in keys})
    for key in ['APPVEYOR_PULL_REQUEST_HEAD_COMMIT', 'APPVEYOR_PULL_REQUEST_NUMBER', 'TRAVIS_PULL_REQUEST_SHA', 'TRAVIS_PULL_REQUEST', 'TRAVIS_BUILD_WEB_URL']:
        if environment.get(key):
            result[key] = environment[key]
    return result


def reports() -> list[dict]:
    result = []
    namespace = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
    for path in sorted((ROOT / 'TestResults').glob('*.trx')):
        counters = ET.parse(path).find('.//t:Counters', namespace)
        if counters is None:
            raise ValueError('TRX counters are missing: ' + str(path))
        result.append({'file': str(path.relative_to(ROOT)), 'counters': counters.attrib})
    browser = ROOT / 'Computator.NET.WebClient/test-results/results.xml'
    if browser.exists():
        result.append({'file': str(browser.relative_to(ROOT)), 'counters': ET.parse(browser).getroot().attrib})
    if not result:
        raise ValueError('Completion evidence requires actual test reports, not a successful checkout.')
    return result


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--provider', choices=['local', 'appveyor', 'travis'], default='local')
    parser.add_argument('--complete', action='store_true')
    arguments = parser.parse_args()
    expected = {
        'sdk': json.loads((ROOT / 'global.json').read_text())['sdk']['version'],
        'node': (ROOT / '.nvmrc').read_text().strip(),
        'npm': json.loads((ROOT / 'Computator.NET.WebClient/package.json').read_text())['packageManager'].split('@', 1)[1],
        'cake': json.loads((ROOT / '.config/dotnet-tools.json').read_text())['tools']['cake.tool']['version']
    }
    cake_text = capture('dotnet', 'cake', '--version')
    cake_version = re.search(r'\b\d+\.\d+\.\d+(?:-[a-zA-Z0-9.]+)?\b', cake_text)
    actual = {'sdk': capture('dotnet', '--version'), 'node': capture('node', '--version').removeprefix('v'),
              'npm': capture('npm', '--version'), 'cake': cake_version.group(0) if cake_version else cake_text}
    require_versions(expected, actual)
    provider = provider_metadata(arguments.provider, dict(os.environ))
    head = capture('git', 'rev-parse', 'HEAD')
    declared = os.environ.get('APPVEYOR_REPO_COMMIT' if arguments.provider == 'appveyor' else 'TRAVIS_COMMIT', '') if arguments.provider != 'local' else ''
    if declared and declared != head:
        # Providers may check out a synthetic PR merge, not just the source head.
        subprocess.run(['git', 'merge-base', '--is-ancestor', declared, head], cwd=ROOT, check=True)
    evidence = {
        'schemaVersion': 1, 'recordedAtUtc': datetime.now(timezone.utc).isoformat(),
        'phase': 'completed-target' if arguments.complete else 'preflight', 'checkoutCommit': head,
        'providerMetadata': provider, 'tools': actual,
        'os': platform.platform(), 'architecture': platform.machine()
    }
    if arguments.complete:
        evidence['testReports'] = reports()
    destination = ROOT / 'artifacts/ci'
    destination.mkdir(parents=True, exist_ok=True)
    output = destination / (arguments.provider + ('-completed' if arguments.complete else '-preflight') + '.json')
    output.write_text(json.dumps(evidence, indent=2) + '\n')
    print(json.dumps(evidence, indent=2))


if __name__ == '__main__':
    main()
