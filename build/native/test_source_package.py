import hashlib
import io
from pathlib import Path
import subprocess
import tarfile
import tempfile
import unittest
import zipfile

from source_package import NOTICE_NAME, SOURCE_NAME, package_sources


class SourcePackageTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix='Computator-Gsl-Source-Test-')
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        self.checkout = self.root / 'vcpkg'
        self.checkout.mkdir()
        subprocess.run(['git', 'init', '-q', str(self.checkout)], check=True)
        subprocess.run(['git', '-C', str(self.checkout), 'config', 'core.autocrlf', 'true'], check=True)
        subprocess.run(['git', '-C', str(self.checkout), 'config', 'core.eol', 'crlf'], check=True)
        patch = self.checkout / 'ports/gsl/fix.patch'
        patch.parent.mkdir(parents=True)
        patch.write_text('exact patch\n', newline='\n')
        (self.checkout / 'LICENSE.txt').write_text('fixture license\n', newline='\n')
        subprocess.run(['git', '-C', str(self.checkout), 'add', '.'], check=True)
        subprocess.run(['git', '-C', str(self.checkout), '-c', 'user.name=Source Test',
                        '-c', 'user.email=source-test@example.invalid', 'commit', '-qm', 'fixture'], check=True)
        self.native = self.root / 'build/native'
        self.native.mkdir(parents=True)
        (self.native / 'build_gsl.py').write_text('# exact recipe\n', newline='\n')
        (self.native / 'gsl-source.json').write_text('{}\n', newline='\n')
        (self.native / 'probe').mkdir()
        (self.native / 'probe/CMakeLists.txt').write_text('add_executable(probe probe.c)\n', newline='\n')
        (self.native / 'probe/probe.c').write_text('#include "probe.h"\nint main(void) { return 0; }\n', newline='\n')
        (self.native / 'probe/probe.h').write_text('/* required probe header */\n', newline='\n')
        (self.native / 'ManagedProbe/obj').mkdir(parents=True)
        (self.native / 'ManagedProbe/obj/cache.json').write_text('not source')
        (self.native / 'private.pfx').write_bytes(b'not a recipe')
        self.upstream = self.root / 'gsl-2.8.tar.gz'
        self.upstream.write_bytes(b'exact upstream archive fixture')
        self.pins = {'version': '2.8', 'archive': 'https://ftp.gnu.org/gnu/gsl/gsl-2.8.tar.gz',
                     'sha512': hashlib.sha512(self.upstream.read_bytes()).hexdigest(),
                     'vcpkgCommit': subprocess.check_output(['git', '-C', str(self.checkout), 'rev-parse', 'HEAD'], text=True).strip()}

    def test_exact_source_recipe_patch_notice_and_deterministic_archive(self):
        first, second = self.root / 'one', self.root / 'two'
        for index, destination in enumerate((first, second)):
            if index:
                subprocess.run(['git', '-C', str(self.checkout), 'config', 'core.autocrlf', 'false'], check=True)
                subprocess.run(['git', '-C', str(self.checkout), 'config', 'core.eol', 'lf'], check=True)
            package_sources(self.native, self.checkout, self.upstream, self.pins, destination)
        self.assertEqual((first / SOURCE_NAME).read_bytes(), (second / SOURCE_NAME).read_bytes())
        self.assertEqual((first / NOTICE_NAME).read_bytes(), (second / NOTICE_NAME).read_bytes())
        with zipfile.ZipFile(first / SOURCE_NAME) as archive:
            self.assertEqual(archive.read('gsl-2.8.tar.gz'), self.upstream.read_bytes())
            self.assertEqual(archive.read('build/native/build_gsl.py'), b'# exact recipe\n')
            for name in ('CMakeLists.txt', 'probe.c', 'probe.h'):
                self.assertEqual(archive.read('build/native/probe/' + name), (self.native / 'probe' / name).read_bytes())
            self.assertFalse(any('obj/' in name or name.endswith('.pfx') for name in archive.namelist()))
            with tarfile.open(fileobj=io.BytesIO(archive.read('vcpkg-source.tar'))) as snapshot:
                self.assertEqual(snapshot.extractfile('ports/gsl/fix.patch').read(), b'exact patch\n')
                self.assertEqual(snapshot.extractfile('LICENSE.txt').read(), b'fixture license\n')
        self.assertIn(hashlib.sha256((first / SOURCE_NAME).read_bytes()).hexdigest(),
                      (first / NOTICE_NAME).read_text())

    def test_changed_upstream_is_rejected(self):
        self.upstream.write_bytes(b'tampered')
        with self.assertRaisesRegex(ValueError, 'SHA-512'):
            package_sources(self.native, self.checkout, self.upstream, self.pins, self.root / 'output')
        self.assertFalse((self.root / 'output').exists())

    def test_modified_port_is_rejected(self):
        (self.checkout / 'ports/gsl/fix.patch').write_text('changed patch')
        with self.assertRaisesRegex(ValueError, 'unmodified pinned'):
            package_sources(self.native, self.checkout, self.upstream, self.pins, self.root / 'output')
        self.assertFalse((self.root / 'output').exists())

    def test_wrong_pinned_commit_is_rejected(self):
        self.pins['vcpkgCommit'] = '0' * 40
        with self.assertRaisesRegex(ValueError, 'unmodified pinned'):
            package_sources(self.native, self.checkout, self.upstream, self.pins, self.root / 'output')


if __name__ == '__main__':
    unittest.main()
