#!/usr/bin/env python3
"""Build a pinned GSL candidate and test it; never modifies machine-wide packages or PATH."""
from __future__ import annotations
import hashlib
import json
import os
from pathlib import Path
import platform
import shutil
import subprocess
import sys
from source_package import package_sources

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'build/native'


def run(*args: object, cwd: Path = ROOT, env: dict[str, str] | None = None) -> None:
    command = [str(arg) for arg in args]
    print('+', ' '.join(command), flush=True)
    subprocess.run(command, cwd=cwd, env=env, check=True)


def capture(*args: object, cwd: Path = ROOT, env: dict[str, str] | None = None) -> str:
    return subprocess.check_output([str(arg) for arg in args], cwd=cwd, env=env, text=True).strip()


def main() -> None:
    run(sys.executable, '-m', 'unittest', 'discover', '-s', SOURCE, '-p', 'test_source_package.py')
    pins = json.loads((SOURCE / 'gsl-source.json').read_text())
    machine = platform.machine().lower()
    arch = {'amd64': 'x64', 'x86_64': 'x64', 'arm64': 'arm64', 'aarch64': 'arm64'}.get(machine)
    os_name = {'Windows': 'win', 'Linux': 'linux', 'Darwin': 'osx'}.get(platform.system())
    rid = f'{os_name}-{arch}'
    triplet = 'computator-' + rid
    if not (SOURCE / 'triplets' / (triplet + '.cmake')).exists():
        raise RuntimeError(f'No validated build recipe for {rid}; refusing architecture substitution.')
    work = ROOT / 'artifacts/native-build' / rid
    work.mkdir(parents=True, exist_ok=True)
    # The Unix bootstrap checks VCPKG_DOWNLOADS before creating it; Windows had
    # silently created it. Make the repository-owned directory explicit on all OSs.
    (work / 'downloads').mkdir(parents=True, exist_ok=True)
    checkout = ROOT / 'artifacts/native-tools/vcpkg'
    if not (checkout / '.git').exists():
        checkout.mkdir(parents=True, exist_ok=True)
        run('git', 'init', checkout)
        run('git', 'remote', 'add', 'origin', 'https://github.com/microsoft/vcpkg.git', cwd=checkout)
        run('git', 'fetch', '--depth=1', 'origin', pins['vcpkgCommit'], cwd=checkout)
        run('git', 'checkout', '--detach', 'FETCH_HEAD', cwd=checkout)
    if capture('git', 'rev-parse', 'HEAD', cwd=checkout) != pins['vcpkgCommit']:
        raise RuntimeError('The cached vcpkg checkout does not match the reviewed commit.')
    if capture('git', 'status', '--porcelain', '--untracked-files=no', cwd=checkout):
        raise RuntimeError('Refusing a modified vcpkg source checkout.')
    port = json.loads((checkout / 'ports/gsl/vcpkg.json').read_text())
    if port['version'] != pins['version']:
        raise RuntimeError('The reviewed GSL port version does not match the source inventory.')
    if pins['sha512'] not in (checkout / 'ports/gsl/portfile.cmake').read_text():
        raise RuntimeError('The GSL source hash differs from the reviewed recipe.')
    environment = dict(os.environ, VCPKG_DISABLE_METRICS='1', VCPKG_DOWNLOADS=str(work / 'downloads'))
    if os_name == 'win':
        run('cmd.exe', '/d', '/c', str(checkout / 'bootstrap-vcpkg.bat'), '-disableMetrics', env=environment)
        executable = checkout / 'vcpkg.exe'
    else:
        run('bash', checkout / 'bootstrap-vcpkg.sh', '-disableMetrics', env=environment)
        executable = checkout / 'vcpkg'
    installed = work / 'installed'
    run(executable, 'install', '--triplet=' + triplet, '--overlay-triplets=' + str(SOURCE / 'triplets'),
        '--x-manifest-root=' + str(SOURCE / 'gsl'), '--x-install-root=' + str(installed), '--binarysource=clear', env=environment)
    prefix = installed / triplet
    probe_build = work / 'probe'
    # Use the source-pinned vcpkg tool recipe, not an older CI-image CMake that
    # cannot discover the image's compiler (notably Visual Studio 2026).
    tool_environment = dict(environment, VCPKG_FORCE_DOWNLOADED_BINARIES='1')
    cmake = Path(capture(executable, 'fetch', 'cmake', '--x-stderr-status', env=tool_environment))
    ctest = cmake.with_name('ctest.exe' if os_name == 'win' else 'ctest')
    if not cmake.is_file() or not ctest.is_file():
        raise RuntimeError('The pinned CMake tool package is incomplete.')
    run(cmake, '-S', SOURCE / 'probe', '-B', probe_build,
        '-DCMAKE_BUILD_TYPE=Release', '-DCMAKE_PREFIX_PATH=' + str(prefix))
    run(cmake, '--build', probe_build, '--config', 'Release', '--parallel', '2')
    probe_environment = dict(environment)
    if os_name == 'win':
        probe_environment['PATH'] = str(prefix / 'bin') + os.pathsep + environment['PATH']
    elif os_name == 'linux':
        probe_environment['LD_LIBRARY_PATH'] = str(prefix / 'lib')
    else:
        probe_environment['DYLD_LIBRARY_PATH'] = str(prefix / 'lib')
    run(ctest, '--test-dir', probe_build, '-C', 'Release', '--output-on-failure', env=probe_environment)
    destination = ROOT / 'artifacts/gsl' / rid / 'native'
    if destination.exists():
        shutil.rmtree(destination)
    destination.mkdir(parents=True)
    candidates = list((prefix / 'bin').glob('*.dll')) if os_name == 'win' else [
        path for path in (prefix / 'lib').iterdir() if '.so' in path.name or path.name.endswith('.dylib')]
    if not candidates:
        raise RuntimeError('The recipe did not produce dynamic libraries.')
    for path in candidates:
        shutil.copy2(path, destination / path.name, follow_symlinks=True)
    gsl_name = 'gsl.dll' if os_name == 'win' else ('libgsl.dylib' if os_name == 'osx' else 'libgsl.so')
    if not (destination / gsl_name).exists():
        raise RuntimeError(f'Expected GSL runtime entry is missing: {gsl_name}')
    shutil.copy2(prefix / 'share/gsl/copyright', destination / 'COPYING.GSL')
    package_sources(SOURCE, checkout, work / 'downloads' / f'gsl-{pins["version"]}.tar.gz', pins, destination)
    manifest = {
        'schemaVersion': 1, 'version': pins['version'], 'rid': rid, 'entryPoint': gsl_name,
        'source': pins, 'cmake': capture(cmake, '--version').splitlines()[0],
        'toolchain': (probe_build / 'CMakeCache.txt').read_text(errors='replace'),
        'files': {path.name: hashlib.sha256(path.read_bytes()).hexdigest() for path in destination.iterdir() if path.is_file()}
    }
    (destination / 'gsl-provenance.json').write_text(json.dumps(manifest, indent=2) + '\n')
    run('dotnet', 'run', '--project', SOURCE / 'ManagedProbe/ManagedProbe.csproj', '--configuration', 'Release', '--', destination)
    print('Validated GSL candidate:', destination, flush=True)


if __name__ == '__main__':
    try:
        main()
    except (OSError, ValueError, RuntimeError, subprocess.CalledProcessError) as error:
        print(f'GSL build failed: {error}', file=sys.stderr)
        sys.exit(1)
