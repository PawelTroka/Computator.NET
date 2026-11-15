"""Distribution contracts: reject corrupted payloads, wrong ABI and unsafe archives."""
import argparse
import copy
import ctypes
import ctypes.util
import importlib.util
import io
import json
import os
from pathlib import Path
import shutil
import signal
import struct
import subprocess
import sys
import tarfile
import tempfile
import time
import unittest
from unittest import mock

SPEC = importlib.util.spec_from_file_location('mono_package', Path(__file__).with_name('package.py'))
package = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(package)


@unittest.skipUnless(os.name == 'posix', 'Runs the Unix launcher and sends process signals.')
class LauncherLifetimeContracts(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix='computator-launcher-')
        self.addCleanup(self.temporary.cleanup)
        self.base = Path(self.temporary.name)
        self.root = self.base / 'package with spaces'
        (self.root / 'bin').mkdir(parents=True)
        (self.root / 'app').mkdir()
        self.launcher = self.root / 'bin/computator'
        shutil.copyfile(Path(__file__).with_name('launcher.sh'), self.launcher)
        tools = self.base / 'tools'
        tools.mkdir()
        # Replace prerequisites at process boundaries; the real launcher still
        # creates its configuration, starts/waits for each child, and cleans up.
        (tools / 'id').write_text('#!/bin/sh\nprintf "1000\\n"\n')
        (tools / 'xmessage').write_text('#!/bin/sh\nprintf "%s\\n" "$*" > "$COMPUTATOR_TEST_DIALOG"\n')
        (tools / 'mono').write_text('''#!/usr/bin/env python3
import json, os, signal, sys, time
from pathlib import Path
arguments = sys.argv[1:]
stage = 'prepare' if 'prepare' in arguments else 'verify' if 'verify' in arguments else 'desktop'
configuration = Path(arguments[arguments.index('--config') + 1])
if stage == 'prepare':
    configuration.write_text('<configuration/>')
def stop(signum, frame):
    signal.signal(signal.SIGTERM, signal.SIG_IGN)
    Path(os.environ['COMPUTATOR_TEST_STOPPING']).touch()
    time.sleep(0.1)
    Path(os.environ['COMPUTATOR_TEST_STOPPED']).write_text(json.dumps({'configurationExists': configuration.exists()}))
    sys.exit(0)
signal.signal(signal.SIGTERM, stop)
with open(os.environ['COMPUTATOR_TEST_TRACE'], 'a') as trace:
    trace.write(json.dumps({'stage': stage, 'pid': os.getpid()}) + '\\n')
if stage == os.environ.get('COMPUTATOR_TEST_BLOCK_STAGE'):
    while True: time.sleep(1)
print('fixture ' + stage + ' output', flush=True)
if stage == os.environ.get('COMPUTATOR_TEST_FAIL_STAGE'):
    sys.exit(int(os.environ['COMPUTATOR_TEST_EXIT_CODE']))
if stage == 'verify':
    Path(arguments[arguments.index('--report') + 1]).write_text('{}')
''')
        for tool in tools.iterdir():
            tool.chmod(0o755)
        self.cache = self.base / 'cache'
        self.trace = self.base / 'trace.jsonl'
        self.dialog = self.base / 'dialog'
        self.stopping = self.base / 'stopping'
        self.stopped = self.base / 'stopped.json'
        self.environment = dict(os.environ, PATH=str(tools) + os.pathsep + os.environ['PATH'],
                                DISPLAY=':launcher-fixture', XDG_CACHE_HOME=str(self.cache),
                                COMPUTATOR_TEST_TRACE=str(self.trace), COMPUTATOR_TEST_DIALOG=str(self.dialog),
                                COMPUTATOR_TEST_STOPPING=str(self.stopping), COMPUTATOR_TEST_STOPPED=str(self.stopped))

    def launch(self, arguments=(), **environment):
        process = subprocess.Popen(['sh', str(self.launcher), *arguments],
                                   env=dict(self.environment, **environment),
                                   stdout=subprocess.PIPE, stderr=subprocess.PIPE,
                                   text=True, start_new_session=True)

        def cleanup():
            # Reap even an unpatched launcher that leaves a blocked preflight.
            try:
                os.killpg(process.pid, signal.SIGKILL)
            except ProcessLookupError:
                pass
            process.communicate(timeout=5)
        self.addCleanup(cleanup)
        return process

    def stages(self):
        return [json.loads(line) for line in self.trace.read_text().splitlines()] if self.trace.exists() else []

    def wait_for_stage(self, stage):
        deadline = time.monotonic() + 5
        while time.monotonic() < deadline:
            records = self.stages()
            if records and records[-1]['stage'] == stage:
                return records
            time.sleep(0.01)
        self.fail('Launcher did not reach ' + stage)

    def assert_cleaned(self):
        self.assertEqual([], list(self.cache.glob('Computator.NET/runtime.*')))
        self.assertEqual([], list(self.cache.glob('Computator.NET/startup-errors.*.log')))
        self.assertFalse(self.dialog.exists(), 'Cancellation is not a startup failure dialog.')
        for record in self.stages():
            with self.assertRaises(ProcessLookupError, msg='Mono child was not reaped: ' + repr(record)):
                os.kill(record['pid'], 0)

    def cancel_at(self, stage, signum=signal.SIGTERM, arguments=(), repeat=False):
        process = self.launch(arguments, COMPUTATOR_TEST_BLOCK_STAGE=stage)
        before = self.wait_for_stage(stage)
        process.send_signal(signum)
        if repeat:
            deadline = time.monotonic() + 2
            while not self.stopping.exists() and time.monotonic() < deadline:
                time.sleep(0.001)
            self.assertTrue(self.stopping.exists(), 'The first signal was not forwarded to Mono.')
            process.send_signal(signum)
        try:
            stdout, stderr = process.communicate(timeout=2)
        except subprocess.TimeoutExpired:
            self.fail('Cancellation did not terminate and reap the active ' + stage + ' child.')
        self.assertEqual(128 + signum, process.returncode, stderr)
        self.assertEqual(before, self.stages(), 'Cancellation must not start another Mono stage.')
        self.assertNotIn('Runtime check passed', stdout)
        self.assertTrue(json.loads(self.stopped.read_text())['configurationExists'],
                        'The runtime configuration was removed before the child finished stopping.')
        self.assert_cleaned()

    def test_term_during_prepare_stops_startup_and_reaps_child(self):
        self.cancel_at('prepare')

    def test_term_during_verify_stops_startup_and_reaps_child(self):
        self.cancel_at('verify')

    def test_interrupt_during_prepare_stops_startup(self):
        self.cancel_at('prepare', signal.SIGINT)

    def test_hangup_during_verify_stops_startup(self):
        self.cancel_at('verify', signal.SIGHUP)

    def test_check_cancellation_never_reports_success(self):
        self.cancel_at('verify', arguments=('--check',))

    def test_desktop_cancellation_reaps_before_cleanup(self):
        self.cancel_at('desktop')

    def test_repeated_signals_still_wait_for_graceful_preflight_exit(self):
        self.cancel_at('verify', repeat=True)

    def test_successful_check_does_not_start_desktop(self):
        process = self.launch(('--check',))
        stdout, stderr = process.communicate(timeout=5)
        self.assertEqual(0, process.returncode, stderr)
        self.assertIn('Runtime check passed', stdout)
        self.assertEqual(['prepare', 'verify'], [record['stage'] for record in self.stages()])
        self.assert_cleaned()

    def test_successful_desktop_keeps_zero_exit(self):
        process = self.launch()
        _, stderr = process.communicate(timeout=5)
        self.assertEqual(0, process.returncode, stderr)
        self.assertEqual(['prepare', 'verify', 'desktop'], [record['stage'] for record in self.stages()])
        self.assert_cleaned()

    def assert_failure(self, stage, expected_stages, expected_status):
        process = self.launch(COMPUTATOR_TEST_FAIL_STAGE=stage, COMPUTATOR_TEST_EXIT_CODE='17')
        _, stderr = process.communicate(timeout=5)
        self.assertEqual(expected_status, process.returncode, stderr)
        self.assertEqual(expected_stages, [record['stage'] for record in self.stages()])
        logs = list(self.cache.glob('Computator.NET/startup-errors.*.log'))
        self.assertEqual(1, len(logs), 'A real failure must retain its diagnostic log.')
        self.assertIn('fixture ' + stage + ' output', logs[0].read_text())
        self.assertTrue(self.dialog.exists())
        self.assertEqual([], list(self.cache.glob('Computator.NET/runtime.*')))

    def test_prepare_failure_keeps_diagnostics_and_does_not_continue(self):
        self.assert_failure('prepare', ['prepare'], 1)

    def test_verify_failure_keeps_diagnostics_and_does_not_start_desktop(self):
        self.assert_failure('verify', ['prepare', 'verify'], 1)

    def test_desktop_failure_preserves_exit_status_and_diagnostics(self):
        self.assert_failure('desktop', ['prepare', 'verify', 'desktop'], 17)


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
        identity = package.product_identity()
        self.metadata = {'version': identity['productVersion'], 'productIdentity': identity, 'native': {}}
        package.write_manifest(self.root, self.metadata)

    def test_complete_inventory_roundtrips_and_archive_is_reproducible(self):
        frozen = self.base / 'frozen-source'
        frozen.mkdir()
        (frozen / 'Version.props').write_text('<Project><ComputatorBaseVersion>3.0.9803</ComputatorBaseVersion>'
            '<ComputatorRevision>21702</ComputatorRevision><ComputatorPrerelease>preview.1</ComputatorPrerelease></Project>')
        identity = package.product_identity(frozen)
        self.assertEqual(identity['productVersion'], '3.0.9803.21702-preview.1')
        self.assertEqual(identity['assemblyVersion'], '3.0.9803.21702')
        self.assertEqual(identity['msixVersion'], '3.0.9803.0')
        self.metadata.update(version=identity['productVersion'], productIdentity=identity)
        package.write_manifest(self.root, self.metadata)
        package.verify(self.root)
        first, second = self.base / 'first.tar.gz', self.base / 'second.tar.gz'
        self.assertEqual(package.archive(self.root, first), package.archive(self.root, second))
        unpacked = self.base / 'unpacked'
        package.extract(first, unpacked)
        self.assertEqual(package.inventory(self.root), package.inventory(unpacked))
        self.assertEqual(package.verify(unpacked)['productIdentity'], identity)

    def test_manifest_cannot_forge_frozen_identity_or_its_package_projections(self):
        for field, value in (('productVersion', '3.0.0-preview.1'),
                             ('msixVersion', '3.0.0.1'),
                             ('macBundleVersion', '3.0.0.0'),
                             ('versionTimestamp', '1999-12-31T23:59:58+00:00')):
            with self.subTest(field=field):
                metadata = copy.deepcopy(self.metadata)
                metadata['productIdentity'][field] = value
                package.write_manifest(self.root, metadata)
                with self.assertRaisesRegex(ValueError, 'frozen product identity'):
                    package.verify(self.root)
        metadata = copy.deepcopy(self.metadata)
        del metadata['productIdentity']
        package.write_manifest(self.root, metadata)
        with self.assertRaisesRegex(ValueError, 'complete frozen product identity'):
            package.verify(self.root)

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
        install = self.base / 'Computator path = $ % " ` \\'
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
        self.assertIn('Terminal=false', entry.read_text(), 'Desktop menu launch must not require a terminal emulator.')
        # The launched fixture only records its own path into this test directory.
        with mock.patch.dict(os.environ, HOME=environment['HOME'], XDG_DATA_HOME=str(data),
                             XDG_CACHE_HOME=str(self.base / 'cache'),
                             DBUS_SESSION_BUS_ADDRESS='unix:path=' + str(self.base / 'no-session-bus'),
                             COMPUTATOR_INSTALL_TEST_MARKER=str(marker)):
            gio = ctypes.CDLL(ctypes.util.find_library('gio-2.0'))
            gio.g_desktop_app_info_new_from_filename.argtypes = [ctypes.c_char_p]
            gio.g_desktop_app_info_new_from_filename.restype = ctypes.c_void_p
            gio.g_app_info_launch.argtypes = [ctypes.c_void_p, ctypes.c_void_p, ctypes.c_void_p, ctypes.c_void_p]
            gio.g_app_info_launch.restype = ctypes.c_int
            gio.g_object_unref.argtypes = [ctypes.c_void_p]
            info = gio.g_desktop_app_info_new_from_filename(os.fsencode(entry))
            self.assertTrue(info, 'The desktop parser rejected the generated entry: ' + entry.read_text())
            try:
                self.assertEqual(1, gio.g_app_info_launch(info, None, None, None))
                deadline = time.monotonic() + 5
                while not marker.exists() and time.monotonic() < deadline:
                    time.sleep(0.01)
                self.assertEqual(str(launcher), marker.read_text())
            finally:
                gio.g_object_unref(info)


if __name__ == '__main__':
    unittest.main(verbosity=2)
