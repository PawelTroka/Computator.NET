"""Portable launcher contracts at its native-process boundary."""
import json
import os
from pathlib import Path
import shlex
import shutil
import subprocess
import sys
import tempfile
import unittest


LAUNCHER = Path(__file__).with_name('run.sh')


class LauncherContracts(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory()
        self.addCleanup(temporary.cleanup)
        self.base = Path(temporary.name)
        self.runtime = self.base / 'selected runtime'
        self.runtime.mkdir()
        self.dotnet = self.runtime / 'dotnet'
        # The launcher must retain the native apphost, not reinterpret its DLL
        # through a possibly different-architecture dotnet executable.
        self.dotnet.write_text('#!/bin/sh\nexit 99\n')
        self.dotnet.chmod(0o755)
        self.published = self.base / 'published application'
        (self.published / 'wwwroot').mkdir(parents=True)
        (self.published / 'wwwroot/index.html').write_text('<!doctype html>')
        apphost = self.published / 'Computator.NET.PortableWeb'
        probe = (
            'import json, os, sys\n'
            'print(json.dumps({"arguments": sys.argv[1:], "roots": '
            '{key: value for key, value in os.environ.items() if key.startswith("DOTNET_ROOT")}}))\n'
            'sys.exit(int(os.environ.get("LAUNCHER_PROBE_EXIT", "0")))\n')
        apphost.write_text(f'#!/bin/sh\nexec {shlex.quote(sys.executable)} -c {shlex.quote(probe)} "$@"\n')
        apphost.chmod(0o755)
        self.environment = {name: value for name, value in os.environ.items()
                            if not name.startswith('DOTNET_ROOT') and name != 'COMPUTATOR_DOTNET'}
        self.environment['COMPUTATOR_DOTNET'] = str(self.dotnet)

    def launch(self, environment=None, arguments=()):
        return subprocess.run([shutil.which('bash'), str(LAUNCHER), str(self.published), *arguments],
                              env=environment or self.environment, text=True, capture_output=True, timeout=10)

    def assert_selected_runtime(self, result):
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(json.loads(result.stdout)['roots'], {'DOTNET_ROOT': str(self.runtime.resolve())})

    def test_explicit_runtime_overrides_inherited_architecture_roots(self):
        for names in (('DOTNET_ROOT_X64',), ('DOTNET_ROOT_ARM64',),
                      ('DOTNET_ROOT_X64', 'DOTNET_ROOT_ARM64', 'DOTNET_ROOT_X86', 'DOTNET_ROOT_ARM')):
            with self.subTest(roots=names):
                environment = dict(self.environment, DOTNET_ROOT='/stale/generic')
                environment.update({name: '/stale/architecture' for name in names})
                self.assert_selected_runtime(self.launch(environment))

    def test_path_selected_runtime_overrides_inherited_architecture_roots(self):
        executable_directory = self.base / 'runtime on PATH'
        executable_directory.mkdir()
        (executable_directory / 'dotnet').symlink_to(self.dotnet)
        environment = dict(self.environment, DOTNET_ROOT_X64='/stale/x64', DOTNET_ROOT_ARM64='/stale/arm64')
        environment.pop('COMPUTATOR_DOTNET')
        environment['PATH'] = str(executable_directory) + os.pathsep + environment.get('PATH', '')
        self.assert_selected_runtime(self.launch(environment))

    def test_runtime_selection_resolves_relative_and_absolute_symlinks(self):
        executable_directory = self.base / 'links'
        executable_directory.mkdir()
        (self.base / 'runtime link').symlink_to(self.dotnet)
        selected = executable_directory / 'selected'
        selected.symlink_to('../runtime link')
        self.assert_selected_runtime(self.launch(dict(self.environment, COMPUTATOR_DOTNET=str(selected))))

    def test_native_apphost_receives_exact_arguments_and_controls_exit_status(self):
        arguments = ['--port', '5100', 'space and "quotes"', 'zażółć λ', 'line\nbreak']
        result = self.launch(dict(self.environment, LAUNCHER_PROBE_EXIT='23'), arguments)
        self.assertEqual(result.returncode, 23, result.stderr)
        self.assertEqual(json.loads(result.stdout)['arguments'], arguments)

    def test_missing_frontend_rejects_publication_before_launch(self):
        (self.published / 'wwwroot/index.html').unlink()
        result = self.launch()
        self.assertNotEqual(result.returncode, 0)
        self.assertIn('complete portable publication', result.stderr)
        self.assertEqual(result.stdout, '')

    def test_missing_selected_runtime_does_not_launch_apphost(self):
        result = self.launch(dict(self.environment, COMPUTATOR_DOTNET=str(self.base / 'missing-dotnet')))
        self.assertNotEqual(result.returncode, 0)
        self.assertEqual(result.stdout, '')


if __name__ == '__main__':
    unittest.main()
