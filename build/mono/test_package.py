"""Distribution contracts: reject corrupted payloads, wrong ABI and unsafe archives."""
import argparse
import ctypes
import ctypes.util
import importlib.util
import io
import json
import os
from pathlib import Path
import shutil
import struct
import subprocess
import sys
import tarfile
import tempfile
import time
import unittest

SPEC = importlib.util.spec_from_file_location('mono_package', Path(__file__).with_name('package.py'))
package = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(package)


class PackageContracts(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.base = Path(self.temporary.name)
        self.root = self.base / 'package'
        app = self.root / 'app'
        for name in package.REQUIRED_APP:
            path = app / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(b'actual payload fixture')
        (app / 'Computator.NET.exe.config').write_text(
            '<configuration><startup><supportedRuntime sku=".NETFramework,Version=v4.8"/></startup></configuration>')
        for name in ('TSL Examples/probe.tsl', 'Static/reference-equations/equation.png', 'Static/reference-figures/figure.png'):
            path = app / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(b'content')
        self.metadata = {'version': '3.0.0-preview.1', 'native': {}}
        package.write_manifest(self.root, self.metadata)

    def test_complete_inventory_roundtrips_and_archive_is_reproducible(self):
        package.verify(self.root)
        first, second = self.base / 'first.tar.gz', self.base / 'second.tar.gz'
        self.assertEqual(package.archive(self.root, first), package.archive(self.root, second))
        unpacked = self.base / 'unpacked'
        package.extract(first, unpacked)
        self.assertEqual(package.inventory(self.root), package.inventory(unpacked))

    def test_missing_app_dependency_is_rejected(self):
        (self.root / 'app/Microsoft.CodeAnalysis.CSharp.dll').unlink()
        with self.assertRaisesRegex(ValueError, 'Missing or changed'):
            package.verify(self.root)

    def test_same_size_corruption_is_rejected(self):
        path = self.root / 'app/Computator.NET.exe'
        path.write_bytes(b'x' * path.stat().st_size)
        with self.assertRaisesRegex(ValueError, 'Missing or changed'):
            package.verify(self.root)

    def test_unrecorded_file_is_rejected(self):
        (self.root / 'app/injected.dll').write_bytes(b'unrecorded')
        with self.assertRaisesRegex(ValueError, 'every payload'):
            package.verify(self.root)

    def test_duplicate_inventory_path_is_rejected(self):
        manifest = json.loads((self.root / package.MANIFEST).read_text())
        manifest['files'].append(manifest['files'][0])
        (self.root / package.MANIFEST).write_text(json.dumps(manifest))
        with self.assertRaisesRegex(ValueError, 'duplicated'):
            package.verify(self.root)

    def test_forged_manifest_cannot_bypass_runtime_content_contract(self):
        (self.root / 'app/Static/reference-figures/figure.png').unlink()
        package.write_manifest(self.root, self.metadata)
        with self.assertRaisesRegex(ValueError, 'figure assets'):
            package.verify(self.root)

    def test_windows_dotnet_output_is_rejected(self):
        (self.root / 'app/Computator.NET.exe.config').write_text('<configuration/>')
        package.write_manifest(self.root, self.metadata)
        with self.assertRaisesRegex(ValueError, 'net48'):
            package.verify(self.root)

    def test_reference_image_links_cannot_be_missing_even_with_other_assets_present(self):
        (self.root / 'app/Static/functions.xml').write_text('<img src="reference-equations/' + 'a' * 64 + '.png"/>')
        package.write_manifest(self.root, self.metadata)
        with self.assertRaisesRegex(ValueError, 'missing image'):
            package.verify(self.root)

    def test_mac_finalization_preserves_explicit_optional_native_maps(self):
        native = self.root / 'native'
        native.mkdir()
        for name in ('gsl', 'cblas', 'gdiplus', 'xim', 'x11'):
            (native / (name + '.dylib')).write_bytes(b'Mac library fixture')
        self.metadata['native'] = {'xim': 'native/xim.dylib', 'x11': 'native/x11.dylib'}
        package.write_manifest(self.root, self.metadata)
        args = argparse.Namespace(output=self.root, rid='osx-arm64', gsl='native/gsl.dylib',
                                  cblas='native/cblas.dylib', gdiplus='native/gdiplus.dylib')
        final = package.finalize(args)
        self.assertEqual(final['native']['xim'], 'native/xim.dylib')
        self.assertEqual(final['native']['x11'], 'native/x11.dylib')
        self.assertEqual(5, len(package.verify(self.root)['native']))

    def test_native_selection_must_be_inventory_bound(self):
        self.metadata.update(rid='linux-x64', native={'gsl': '../host.so', 'cblas': 'other', 'gdiplus': 'elsewhere'})
        package.write_manifest(self.root, self.metadata)
        with self.assertRaises(ValueError):
            package.verify(self.root)

    def test_linux_native_selection_rejects_wrong_architecture(self):
        library = self.base / 'arm.so'
        header = bytearray(b'\x7fELF\x02\x01' + b'\0' * 14)
        struct.pack_into('<H', header, 16, 3)  # ET_DYN
        struct.pack_into('<H', header, 18, 183)  # AArch64
        library.write_bytes(header)
        with self.assertRaisesRegex(ValueError, 'x86_64'):
            package.check_elf(library, 'linux-x64')
        struct.pack_into('<H', header, 18, 62)
        library.write_bytes(header)
        package.check_elf(library, 'linux-x64')

    def test_staging_refuses_existing_payload_instead_of_overwriting(self):
        args = argparse.Namespace(output=self.root, application_directory=self.root / 'app', configuration='Release')
        original = (self.root / 'app/Computator.NET.exe').read_bytes()
        with self.assertRaisesRegex(ValueError, 'existing data'):
            package.stage(args)
        self.assertEqual(original, (self.root / 'app/Computator.NET.exe').read_bytes())

    def test_path_validation_rejects_absolute_traversal_and_windows_paths(self):
        for name in ('../outside', '/absolute', 'nested/../outside', 'C:/host', 'nested\\file', './file', 'a//b'):
            with self.subTest(name=name), self.assertRaises(ValueError):
                package.relative_path(name)

    def malicious_tar(self, names, kind=None):
        archive = self.base / 'malicious.tar.gz'
        with tarfile.open(archive, 'w:gz') as tar:
            for name in names:
                entry = tarfile.TarInfo(name)
                entry.size = 1
                if kind:
                    entry.type, entry.linkname, entry.size = kind, '../../outside', 0
                tar.addfile(entry, io.BytesIO(b'x') if not kind else None)
        return archive

    def test_extraction_checks_all_paths_before_writing(self):
        archive = self.malicious_tar(['safe/file', '../outside'])
        output = self.base / 'unpacked'
        with self.assertRaises(ValueError):
            package.extract(archive, output)
        self.assertFalse(output.exists())
        self.assertFalse((self.base / 'outside').exists())

    def test_extraction_rejects_symlinks_hardlinks_devices_and_duplicates(self):
        for kind in (tarfile.SYMTYPE, tarfile.LNKTYPE, tarfile.CHRTYPE, tarfile.FIFOTYPE):
            with self.subTest(kind=kind), self.assertRaises(ValueError):
                package.extract(self.malicious_tar(['link'], kind), self.base / 'unpacked')
        with self.assertRaises(ValueError):
            package.extract(self.malicious_tar(['same', 'same']), self.base / 'unpacked')

    def test_extraction_never_replaces_user_directory(self):
        with self.assertRaisesRegex(ValueError, 'must not exist'):
            package.extract(self.malicious_tar(['safe']), self.root)

    def test_source_symbolic_link_is_rejected(self):
        source = self.base / 'source'
        source.mkdir()
        try:
            (source / 'host-secret').symlink_to(self.root / 'app/Computator.NET.exe')
        except OSError as error:
            self.skipTest('Host does not grant symbolic link privileges: ' + str(error))
        with self.assertRaisesRegex(ValueError, 'Unsupported'):
            package.copy_tree(source, self.base / 'copy')

    @unittest.skipUnless(sys.platform.startswith('linux'), 'Uses the actual Linux desktop-entry parser.')
    def test_user_desktop_registration_launches_paths_with_reserved_characters(self):
        install = self.base / 'Computator path $ % " ` \\'
        (install / 'bin').mkdir(parents=True)
        (install / 'share').mkdir()
        (install / 'share/computator.png').write_bytes(b'icon fixture')
        shutil.copyfile(Path(__file__).with_name('install-user.sh'), install / 'bin/install-desktop-entry')
        launcher = install / 'bin/computator'
        launcher.write_text('#!/bin/sh\nprintf "%s" "$0" > "$COMPUTATOR_INSTALL_TEST_MARKER"\n')
        launcher.chmod(0o755)
        marker = self.base / 'launched-path'
        data = self.base / 'desktop-data'
        environment = dict(os.environ, XDG_DATA_HOME=str(data), HOME=str(self.base / 'profile'),
                           COMPUTATOR_INSTALL_TEST_MARKER=str(marker))
        subprocess.run(['sh', str(install / 'bin/install-desktop-entry')], env=environment, check=True, capture_output=True)
        entry = data / 'applications/computator-net.desktop'
        # Test argument parsing without requiring a particular terminal emulator.
        entry.write_text(entry.read_text().replace('Terminal=true', 'Terminal=false'))
        gio = ctypes.CDLL(ctypes.util.find_library('gio-2.0'))
        gio.g_desktop_app_info_new_from_filename.argtypes = [ctypes.c_char_p]
        gio.g_desktop_app_info_new_from_filename.restype = ctypes.c_void_p
        gio.g_app_info_launch.argtypes = [ctypes.c_void_p, ctypes.c_void_p, ctypes.c_void_p, ctypes.c_void_p]
        gio.g_app_info_launch.restype = ctypes.c_int
        info = gio.g_desktop_app_info_new_from_filename(os.fsencode(entry))
        self.assertTrue(info, 'The desktop parser rejected the generated entry.')
        # The launched fixture only records its own path into this test directory.
        old_marker = os.environ.get('COMPUTATOR_INSTALL_TEST_MARKER')
        os.environ['COMPUTATOR_INSTALL_TEST_MARKER'] = str(marker)
        try:
            self.assertEqual(1, gio.g_app_info_launch(info, None, None, None))
            deadline = time.monotonic() + 5
            while not marker.exists() and time.monotonic() < deadline:
                time.sleep(0.01)
            self.assertEqual(str(launcher), marker.read_text())
        finally:
            if old_marker is None:
                os.environ.pop('COMPUTATOR_INSTALL_TEST_MARKER', None)
            else:
                os.environ['COMPUTATOR_INSTALL_TEST_MARKER'] = old_marker


if __name__ == '__main__':
    unittest.main(verbosity=2)
