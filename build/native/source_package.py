"""Package the exact native sources and recipes beside distributed GSL binaries."""
from __future__ import annotations
import hashlib
import io
from pathlib import Path
import shutil
import subprocess
import tempfile
import zipfile

SOURCE_NAME = 'GSL-2.8-source.zip'
NOTICE_NAME = 'SOURCE.GSL.txt'


def add_stream(archive: zipfile.ZipFile, name: str, stream) -> None:
    info = zipfile.ZipInfo(name, date_time=(1980, 1, 1, 0, 0, 0))
    info.create_system = 3
    info.external_attr = 0o100644 << 16
    info.compress_type = zipfile.ZIP_DEFLATED
    with archive.open(info, 'w') as target:
        shutil.copyfileobj(stream, target)


def package_sources(native: Path, checkout: Path, upstream: Path, pins: dict, destination: Path) -> None:
    with upstream.open('rb') as stream:
        upstream_hash = hashlib.file_digest(stream, 'sha512').hexdigest()
    if upstream_hash != pins['sha512']:
        raise ValueError('The corresponding-source archive does not match the reviewed GNU SHA-512.')
    commit = subprocess.check_output(['git', '-C', str(checkout), 'rev-parse', 'HEAD'], text=True).strip()
    if commit != pins['vcpkgCommit'] or subprocess.check_output(
            ['git', '-C', str(checkout), 'status', '--porcelain', '--untracked-files=no'], text=True).strip():
        raise ValueError('Corresponding-source packaging requires the unmodified pinned vcpkg checkout.')
    recipes = sorted(path for path in native.rglob('*') if path.is_file() and
                     not {'bin', 'obj', '__pycache__'}.intersection(path.relative_to(native).parts) and
                     path.suffix in {'.py', '.json', '.cmake', '.txt', '.cs', '.csproj',
                                     '.c', '.h', '.cc', '.cpp', '.cxx', '.hpp'})
    if not (native / 'build_gsl.py') in recipes or not (native / 'gsl-source.json') in recipes:
        raise ValueError('Native build recipes are incomplete.')
    readme = f'''GNU GSL {pins['version']} corresponding native source

This archive accompanies the GSL and CBLAS runtime libraries. GSL is licensed
under GPL-3.0-or-later; see gsl-{pins['version']}.tar.gz/COPYING and COPYING.GSL
beside this archive. The vcpkg snapshot carries its own LICENSE.txt notices.

Contents:
- gsl-{pins['version']}.tar.gz: exact upstream archive, SHA-512 {pins['sha512']}
- vcpkg-source.tar: complete tracked vcpkg source at {pins['vcpkgCommit']},
  including ports/gsl/0001-configure.patch, 0002-add-fp-control.patch, the
  replacement CMakeLists.txt, portfile and all vcpkg build scripts.
- build/native: Computator.NET's native build script, pins, triplets and probes.

Rebuild (Windows x64, Linux x64 or macOS x64/ARM64):
1. Extract this archive into an empty working directory. Install Git, Python 3,
   a supported
   C/C++ toolchain (MSVC on Windows) and .NET SDK 10.0.402 or a compatible patch.
2. The automated driver verifies Git HEAD and fetches the pinned vcpkg commit
   into artifacts/native-tools/vcpkg. Keep that directory absent before the first
   run, or use a clean checkout of that exact commit there. vcpkg-source.tar
   separately preserves its complete tracked source, including the port, patches
   and scripts, independently of https://github.com/microsoft/vcpkg .
3. Put gsl-{pins['version']}.tar.gz under artifacts/native-build/<rid>/downloads,
   where <rid> is win-x64, linux-x64, osx-x64 or osx-arm64.
4. Run python build/native/build_gsl.py from the archive root. The pinned port
   applies both patches and the replacement CMake project; triplets control
   static CRT/dynamic GSL linkage. C and managed ABI probes must pass. The build
   downloads the checksum-verified CMake tool selected by the pinned vcpkg
   recipe; it may also download other unmodified general-purpose build tools.

The build records compiler configuration and per-file hashes in
gsl-provenance.json. No compiler binaries, credentials or build caches are in
this archive. This covers the native GSL build; Computator.NET host source is
maintained separately at https://github.com/PawelTroka/Computator.NET .
'''
    destination.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryFile() as snapshot:
        subprocess.run(['git', '-C', str(checkout), 'archive', '--format=tar', commit], stdout=snapshot, check=True)
        snapshot.seek(0)
        with zipfile.ZipFile(destination / SOURCE_NAME, 'w', compression=zipfile.ZIP_DEFLATED) as archive:
            add_stream(archive, 'README.txt', io.BytesIO(readme.encode('utf-8')))
            with upstream.open('rb') as stream:
                add_stream(archive, f'gsl-{pins["version"]}.tar.gz', stream)
            add_stream(archive, 'vcpkg-source.tar', snapshot)
            for path in recipes:
                if path.is_symlink():
                    raise ValueError('Native source recipes must not be symbolic links.')
                with path.open('rb') as stream:
                    add_stream(archive, 'build/native/' + path.relative_to(native).as_posix(), stream)
    notice = f'''GNU GSL {pins['version']} and CBLAS are provided under GPL-3.0-or-later.
The accompanying COPYING.GSL contains the license. {SOURCE_NAME} includes the
exact upstream source, applied patches and pinned build scripts, plus rebuild
instructions. Source archive SHA-256:
{hashlib.sha256((destination / SOURCE_NAME).read_bytes()).hexdigest()}
Upstream: {pins['archive']}
vcpkg source commit: {pins['vcpkgCommit']}
The native provenance inventory binds these source and notice files to this
binary payload. Keep them together when redistributing these libraries.
'''
    (destination / NOTICE_NAME).write_text(notice, encoding='utf-8', newline='\n')
