"""Run the shipped managed preflight against an isolated copy of a real package.

These exercise inventory/configuration rejection through the Mono process. The
separate package acceptance executes native imports and the actual desktop.
"""
import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import unittest
import xml.etree.ElementTree as ET


class RuntimePreflightContracts(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.temporary = tempfile.TemporaryDirectory(prefix='computator-preflight-')
        cls.base = Path(cls.temporary.name)
        if (PACKAGE.name == 'Resources' and PACKAGE.parent.name == 'Contents'
                and PACKAGE.parent.parent.suffix == '.app'):
            # The Mac preflight also verifies fixed bootstrap files outside
            # Resources. Preserve the real bundle shape in the isolated copy.
            bundle = cls.base / 'application with spaces.app'
            shutil.copytree(PACKAGE.parent.parent, bundle, symlinks=True)
            cls.package = bundle / 'Contents/Resources'
        else:
            cls.package = cls.base / 'package with spaces'
            shutil.copytree(PACKAGE, cls.package, symlinks=True)
        cls.executable = cls.package / 'app/Computator.NET.Mono.Preflight.exe'
        cls.manifest_path = cls.package / 'mono-package.json'
        cls.original_manifest = cls.manifest_path.read_bytes()

    @classmethod
    def tearDownClass(cls):
        cls.temporary.cleanup()

    def setUp(self):
        self.output = self.base / self.id().rsplit('.', 1)[-1] / 'mono.config'
        self.output.parent.mkdir()
        self.addCleanup(self.manifest_path.write_bytes, self.original_manifest)

    def process_environment(self):
        environment = os.environ.copy()
        for key in ('MONO_ENV_OPTIONS', 'MONO_CFG_DIR'):
            environment.pop(key, None)
        manifest = json.loads(self.original_manifest)
        if manifest.get('rid', '').startswith('linux-'):
            # The fixture is a COPY of the input package. Its GSL dependency
            # search must not retain the caller's original private CBLAS path.
            environment['LD_LIBRARY_PATH'] = ':'.join(dict.fromkeys(
                str((self.package / path).parent) for path in manifest['native'].values()))
        return environment

    def prepare(self, output=None):
        return subprocess.run([MONO, str(self.executable), 'prepare', '--package-root',
                               str(self.package), '--config', str(output or self.output)],
                              env=self.process_environment(), text=True, capture_output=True, timeout=60)

    def reject(self, result, message):
        self.assertNotEqual(0, result.returncode, result.stdout + result.stderr)
        self.assertIn(message, result.stderr)
        self.assertFalse(self.output.exists(), 'Rejected preparation must not create a configuration.')

    def alter_manifest(self, update):
        value = json.loads(self.original_manifest)
        update(value)
        self.manifest_path.write_text(json.dumps(value))

    def test_process_config_selects_each_packaged_library_and_can_be_refreshed(self):
        result = self.prepare()
        self.assertEqual(0, result.returncode, result.stderr)
        content = self.output.read_text()
        self.assertTrue(content.startswith('<!-- Computator.NET generated process configuration -->'))
        self.assertNotIn('<?xml', content)
        maps = ET.fromstring(content).findall('dllmap')
        native = json.loads(self.original_manifest)['native']
        for name in ('gsl', 'gdiplus'):
            self.assertTrue(any(node.get('dll') == name and
                                node.get('target') == str(self.package / native[name]) for node in maps))
        self.assertEqual(0, self.prepare().returncode, 'The launcher must refresh its own generated config.')

    def test_changed_payload_is_rejected_before_config_is_written(self):
        target = self.package / 'app/Computator.NET.exe.config'
        original = target.read_bytes()
        self.addCleanup(target.write_bytes, original)
        target.write_bytes(bytes([original[0] ^ 1]) + original[1:])
        self.reject(self.prepare(), 'differs from its inventory')

    def test_unrecorded_file_is_rejected(self):
        target = self.package / 'app/unrecorded.dll'
        target.write_bytes(b'unrecorded')
        self.addCleanup(target.unlink)
        self.reject(self.prepare(), 'Unrecorded package file')

    def test_duplicate_manifest_entry_is_rejected(self):
        self.alter_manifest(lambda value: value['files'].append(value['files'][0]))
        self.reject(self.prepare(), 'duplicate package inventory')

    def test_native_mapping_cannot_escape_inventory(self):
        self.alter_manifest(lambda value: value['native'].update(gsl='../outside.so'))
        self.reject(self.prepare(), 'absent from the inventory')

    def test_wrong_architecture_is_rejected(self):
        self.alter_manifest(lambda value: value.update(rid='unsupported-cpu'))
        self.reject(self.prepare(), 'does not match native Mono process')

    def test_user_config_is_preserved(self):
        self.output.write_text('user-owned content')
        result = self.prepare()
        self.assertNotEqual(0, result.returncode)
        self.assertIn('not a generated package configuration', result.stderr)
        self.assertEqual('user-owned content', self.output.read_text())

    def test_direct_package_output_is_rejected(self):
        target = self.package / 'must-not-be-created.config'
        self.reject(self.prepare(target), 'outside the read-only package')
        self.assertFalse(target.exists())

    def test_cache_symlink_cannot_redirect_output_into_the_package(self):
        alias = self.output.parent / 'redirected-cache'
        alias.symlink_to(self.package, target_is_directory=True)
        target = alias / 'must-not-be-created.config'
        # Cleanup also makes the red regression safe to run against an old executable.
        self.addCleanup(lambda: target.unlink() if target.exists() else None)
        self.reject(self.prepare(target), 'outside the read-only package')
        self.assertFalse(target.exists())

    def test_failed_verification_replaces_an_actual_prior_success_report(self):
        self.assertEqual(0, self.prepare().returncode)
        report = self.output.parent / 'runtime-check.json'
        command = [MONO, '--config', str(self.output), str(self.executable), 'verify', '--package-root',
                   str(self.package), '--report', str(report)]
        first = subprocess.run(command, env=self.process_environment(), text=True, capture_output=True, timeout=60)
        self.assertEqual(0, first.returncode, first.stdout + first.stderr)
        prior = json.loads(report.read_text())
        self.assertTrue(prior['passed'])
        self.alter_manifest(lambda value: value['files'].append(value['files'][0]))
        second = subprocess.run(command, env=self.process_environment(), text=True, capture_output=True, timeout=60)
        self.assertNotEqual(0, second.returncode)
        current = json.loads(report.read_text())
        self.assertFalse(current['passed'], 'A stale success report must never survive a failed current verification.')
        self.assertNotEqual(prior['startedUtc'], current['startedUtc'])
        self.assertIn('duplicate package inventory', current['failure'])


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--package-root', required=True, type=Path)
    parser.add_argument('--mono', default='mono')
    arguments, remaining = parser.parse_known_args()
    PACKAGE = arguments.package_root.resolve(strict=True)
    MONO = arguments.mono
    unittest.main(argv=[sys.argv[0], *remaining])
