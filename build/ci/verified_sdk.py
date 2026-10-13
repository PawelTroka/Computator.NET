#!/usr/bin/env python3
"""Install the repo-pinned SDK from Microsoft's hash-bearing release metadata.
No downloaded shell/PowerShell installer is executed. The destination is dedicated
CI tool storage, never the system installation. Shared by AppVeyor and Travis.
"""
from __future__ import annotations
import argparse
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
import platform
import shutil
import subprocess
import tarfile
import tempfile
from urllib.parse import urlparse
from urllib.request import urlopen
import zipfile

ROOT = Path(__file__).resolve().parents[2]
ALLOWED_HOSTS = {'builds.dotnet.microsoft.com', 'dotnetcli.blob.core.windows.net', 'dotnetcli.azureedge.net'}


def checked_url(url: str) -> str:
    parsed = urlparse(url)
    if parsed.scheme != 'https' or parsed.hostname not in ALLOWED_HOSTS or parsed.username or parsed.password:
        raise ValueError('The SDK download must use an approved Microsoft HTTPS host.')
    return url


def select_file(metadata: dict, version: str, rid: str) -> dict:
    candidates: dict[str, dict] = {}
    suffix = '.zip' if rid.startswith('win-') else '.tar.gz'
    for release in metadata.get('releases', []):
        for sdk in [release.get('sdk', {}), *release.get('sdks', [])]:
            if sdk.get('version') == version:
                for item in sdk.get('files', []):
                    if item.get('rid') == rid and item.get('url', '').endswith(suffix):
                        checked_url(item['url'])
                        digest = item.get('hash', '')
                        if len(digest) != 128 or any(char not in '0123456789abcdefABCDEF' for char in digest):
                            raise ValueError('SDK metadata lacks a SHA-512 digest.')
                        candidates[item['url']] = item
    if len(candidates) != 1:
        raise ValueError(f'Expected exactly one SDK {version} archive for {rid}; got {len(candidates)}.')
    return next(iter(candidates.values()))


def safe_name(name: str) -> None:
    path = PurePosixPath(name.replace('\\', '/'))
    if path.is_absolute() or '..' in path.parts or ':' in name:
        raise ValueError('Unsafe SDK archive member: ' + name)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--destination', type=Path, required=True)
    options = parser.parse_args()
    version = json.loads((ROOT / 'global.json').read_text())['sdk']['version']
    if not version or any(char not in '0123456789.' for char in version):
        raise ValueError('This bootstrap only accepts an explicit stable SDK version.')
    system = {'Windows': 'win', 'Linux': 'linux', 'Darwin': 'osx'}.get(platform.system())
    architecture = {'AMD64': 'x64', 'x86_64': 'x64', 'arm64': 'arm64', 'aarch64': 'arm64'}.get(platform.machine())
    if not system or not architecture:
        raise ValueError('Unsupported SDK bootstrap OS/architecture.')
    rid = f'{system}-{architecture}'
    destination = options.destination.absolute()
    if destination.is_symlink() or destination == Path(destination.anchor):
        raise ValueError('Use a dedicated, non-redirected SDK directory.')
    metadata_url = f'https://builds.dotnet.microsoft.com/dotnet/release-metadata/{version.split(".")[0]}.0/releases.json'
    with urlopen(checked_url(metadata_url), timeout=60) as response:
        checked_url(response.geturl())
        metadata = json.load(response)
    item = select_file(metadata, version, rid)
    inventory = {'schemaVersion': 1, 'version': version, 'rid': rid, 'url': item['url'], 'sha512': item['hash'].lower()}
    marker = destination / 'computator-sdk.json'
    if destination.exists():
        if not marker.is_file() or json.loads(marker.read_text()) != inventory:
            raise ValueError('Refusing to overwrite an existing or unrecognized SDK directory. Choose a fresh CI tool directory.')
    else:
        destination.parent.mkdir(parents=True, exist_ok=True)
        with tempfile.TemporaryDirectory(prefix='.computator-sdk-', dir=destination.parent) as temporary:
            work = Path(temporary)
            archive = work / ('sdk.zip' if system == 'win' else 'sdk.tar.gz')
            digest = hashlib.sha512()
            size = 0
            with urlopen(checked_url(item['url']), timeout=120) as response, archive.open('wb') as output:
                checked_url(response.geturl())
                while block := response.read(1024 * 1024):
                    size += len(block)
                    if size > 1024 * 1024 * 1024:
                        raise ValueError('SDK archive exceeds the bootstrap limit.')
                    digest.update(block)
                    output.write(block)
            if digest.hexdigest().lower() != inventory['sha512']:
                raise ValueError('Downloaded SDK failed SHA-512 verification.')
            unpacked = work / 'sdk'
            unpacked.mkdir()
            if system == 'win':
                with zipfile.ZipFile(archive) as source:
                    for member in source.infolist():
                        safe_name(member.filename)
                        if (member.external_attr >> 16) & 0o170000 == 0o120000:
                            raise ValueError('Unexpected SDK archive symlink.')
                    source.extractall(unpacked)
            else:
                with tarfile.open(archive, 'r:gz') as source:
                    for member in source.getmembers():
                        safe_name(member.name)
                        if not member.isfile() and not member.isdir():
                            raise ValueError('Unexpected SDK archive link or special file.')
                    source.extractall(unpacked, filter='data')
            (unpacked / 'computator-sdk.json').write_text(json.dumps(inventory, indent=2) + '\n')
            unpacked.rename(destination)
    executable = destination / ('dotnet.exe' if system == 'win' else 'dotnet')
    environment = dict(os.environ, DOTNET_ROOT=str(destination), PATH=str(destination) + os.pathsep + os.environ.get('PATH', ''))
    actual = subprocess.check_output([str(executable), '--version'], cwd=ROOT, env=environment, text=True).strip()
    if actual != version:
        raise ValueError(f'Installed SDK does not match global.json: {actual} != {version}.')
    print(json.dumps(inventory, indent=2))


if __name__ == '__main__':
    main()
