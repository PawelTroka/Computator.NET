"""Exercise the real wrapper scripts without downloading tools or building projects."""

import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import unittest


ROOT = Path(__file__).resolve().parents[2]
BASH = shutil.which('bash')
PWSH = shutil.which('pwsh')


class WrapperFixture(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix='computator wrappers ')
        self.addCleanup(self.temporary.cleanup)
        self.directory = Path(self.temporary.name)
        self.repo = self.directory / "checkout with spaces and 'apostrophe'"
        self.repo.mkdir()
        (self.repo / 'build').mkdir()
        for name in ('build.sh', 'build.ps1', 'build/cli-help.txt'):
            shutil.copyfile(ROOT / name, self.repo / name)
        self.bin = self.directory / 'bin'
        self.bin.mkdir()
        self.log = self.directory / 'calls.log'
        self.cwd_log = self.directory / 'returned-cwd.txt'
        self.environment = os.environ.copy()
        for name in ('configuration', 'build_config'):
            self.environment.pop(name, None)
        self.environment.update({
            'COMPUTATOR_WRAPPER_LOG': str(self.log),
            'COMPUTATOR_WRAPPER_RESTORE_EXIT': '0',
            'COMPUTATOR_WRAPPER_CAKE_EXIT': '0',
        })

    def assert_help(self, result):
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn('Computator.NET build and test guide', result.stdout)
        self.assertIn('Portable-Validate', result.stdout)
        self.assertIn('global.json', result.stdout)
        self.assertFalse(self.log.exists(), 'Help tried to invoke dotnet.')
        self.assertFalse((self.repo / 'artifacts').exists())
        self.assertFalse((self.repo / 'TestResults').exists())


@unittest.skipUnless(BASH and os.name != 'nt', 'Bash wrapper runs on Unix hosts.')
class BashWrapperTests(WrapperFixture):
    def setUp(self):
        super().setUp()
        # The restricted PATH has dirname but deliberately has no SDK.
        (self.bin / 'dirname').symlink_to(shutil.which('dirname'))
        self.environment['PATH'] = str(self.bin)

    def install_fake_dotnet(self):
        executable = self.bin / 'dotnet'
        executable.write_text('''#!/bin/sh
printf '%s\\0' "$#" "$PWD" "$@" >> "$COMPUTATOR_WRAPPER_LOG"
if [ "$1" = tool ]; then exit "$COMPUTATOR_WRAPPER_RESTORE_EXIT"; fi
exit "$COMPUTATOR_WRAPPER_CAKE_EXIT"
''')
        executable.chmod(0o755)

    def run_wrapper(self, *arguments):
        return subprocess.run([BASH, str(self.repo / 'build.sh'), *arguments],
                              cwd=self.directory, env=self.environment,
                              text=True, capture_output=True, timeout=30)

    def calls(self):
        fields = self.log.read_bytes().decode().split('\0')[:-1]
        calls = []
        while fields:
            count = int(fields.pop(0))
            cwd = fields.pop(0)
            arguments, fields = fields[:count], fields[count:]
            calls.append({'cwd': cwd, 'args': arguments})
        return calls

    def test_help_without_sdk(self):
        for flag in ('--help', '-h'):
            with self.subTest(flag=flag):
                self.assert_help(self.run_wrapper(flag))

    def test_help_never_restores_even_with_an_explicit_target(self):
        self.install_fake_dotnet()
        self.environment['COMPUTATOR_WRAPPER_RESTORE_EXIT'] = '37'
        self.assert_help(self.run_wrapper('--target=Build', '--help'))

    def test_exact_passthrough_and_repository_working_directory(self):
        self.install_fake_dotnet()
        arguments = ['--target=SelectedTests', '--configuration=Debug',
                     '--suite=Computator.NET.Core.Tests', '--description',
                     '--candidate-bundle=folder with spaces/a "quoted" file.exe',
                     '--label=a&b', '']
        result = self.run_wrapper(*arguments)
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(self.calls(), [
            {'cwd': str(self.repo), 'args': ['tool', 'restore']},
            {'cwd': str(self.repo), 'args': ['cake', 'build.cake', *arguments]},
        ])

    def test_failed_restore_does_not_run_cake(self):
        self.install_fake_dotnet()
        self.environment['COMPUTATOR_WRAPPER_RESTORE_EXIT'] = '37'
        result = self.run_wrapper('--target=Build')
        self.assertEqual(result.returncode, 37)
        self.assertEqual(len(self.calls()), 1)

    def test_cake_exit_code_is_preserved(self):
        self.install_fake_dotnet()
        self.environment['COMPUTATOR_WRAPPER_CAKE_EXIT'] = '43'
        self.assertEqual(self.run_wrapper('--target=Build').returncode, 43)
        self.assertEqual(len(self.calls()), 2)

    def test_missing_sdk_explains_how_to_recover(self):
        result = self.run_wrapper('--target=Build')
        self.assertEqual(result.returncode, 127)
        self.assertIn('global.json', result.stderr)
        self.assertIn('./build.sh --help', result.stderr)


@unittest.skipUnless(PWSH, 'PowerShell 7 is needed to exercise build.ps1.')
class PowerShellWrapperTests(WrapperFixture):
    def run_wrapper(self, arguments, fake=True):
        # A function intercepts precisely the two external calls. All parameter
        # binding, quoting, error propagation and location handling remain real.
        script = self.directory / 'invoke.ps1'
        mock = '''
function global:dotnet {
    $record = @{ cwd = (Get-Location).Path; args = @($args) }
    [IO.File]::AppendAllText($env:COMPUTATOR_WRAPPER_LOG,
        ($record | ConvertTo-Json -Compress) + [Environment]::NewLine)
    $global:LASTEXITCODE = if ($args[0] -eq 'tool') {
        [int]$env:COMPUTATOR_WRAPPER_RESTORE_EXIT
    } else { [int]$env:COMPUTATOR_WRAPPER_CAKE_EXIT }
}
''' if fake else '$env:PATH = $env:COMPUTATOR_WRAPPER_EMPTY_PATH\n'
        wrapper = str(self.repo / 'build.ps1').replace("'", "''")
        script.write_text("$ErrorActionPreference = 'Stop'\n" + mock +
                          f"try {{ & '{wrapper}' {arguments} }}\n" +
                          "finally { [IO.File]::WriteAllText($env:COMPUTATOR_WRAPPER_CWD, (Get-Location).Path) }\n")
        self.environment['COMPUTATOR_WRAPPER_CWD'] = str(self.cwd_log)
        self.environment['COMPUTATOR_WRAPPER_EMPTY_PATH'] = str(self.bin)
        return subprocess.run([PWSH, '-NoProfile', '-NonInteractive', '-File', str(script)],
                              cwd=self.directory, env=self.environment,
                              text=True, capture_output=True, timeout=30)

    def calls(self):
        return [json.loads(line) for line in self.log.read_text().splitlines()]

    def assert_location_restored(self):
        self.assertEqual(Path(self.cwd_log.read_text()).resolve(), self.directory.resolve())

    def test_help_without_sdk(self):
        for flag in ('-Help', '-h', '--help'):
            with self.subTest(flag=flag):
                self.assert_help(self.run_wrapper(flag, fake=False))
                self.assert_location_restored()

    def test_help_never_restores_even_with_an_explicit_target(self):
        self.environment['COMPUTATOR_WRAPPER_RESTORE_EXIT'] = '37'
        self.assert_help(self.run_wrapper('-Target Build -Help'))

    def test_exact_passthrough_and_repository_working_directory(self):
        result = self.run_wrapper(
            '-Target SelectedTests -Configuration Debug -Verbosity Diagnostic '
            '--suite=Computator.NET.Core.Tests --description '
            "'--candidate-bundle=folder with spaces/a \"quoted\" file.exe' '--label=a&b'")
        self.assertEqual(result.returncode, 0, result.stderr)
        calls = self.calls()
        self.assertEqual(calls[0]['args'], ['tool', 'restore'])
        self.assertEqual(calls[1]['args'], [
            'cake', 'build.cake', '--target=SelectedTests', '--configuration=Debug',
            '--verbosity=Diagnostic', '--suite=Computator.NET.Core.Tests', '--description',
            '--candidate-bundle=folder with spaces/a "quoted" file.exe', '--label=a&b',
        ])
        for call in calls:
            self.assertEqual(Path(call['cwd']).resolve(), self.repo.resolve())
        self.assert_location_restored()

    def test_failed_restore_does_not_run_cake(self):
        self.environment['COMPUTATOR_WRAPPER_RESTORE_EXIT'] = '37'
        result = self.run_wrapper('-Target Build')
        self.assertNotEqual(result.returncode, 0)
        self.assertIn('Cake tool restore failed (37)', result.stderr)
        self.assertEqual(len(self.calls()), 1)
        self.assert_location_restored()

    def test_cake_failure_is_reported(self):
        self.environment['COMPUTATOR_WRAPPER_CAKE_EXIT'] = '43'
        result = self.run_wrapper('-Target Build')
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("Cake target 'Build' failed (43)", result.stderr)
        self.assertEqual(len(self.calls()), 2)
        self.assert_location_restored()

    def test_missing_sdk_explains_how_to_recover(self):
        result = self.run_wrapper('-Target Build', fake=False)
        self.assertNotEqual(result.returncode, 0)
        self.assertIn('global.json', result.stderr)
        self.assertIn('./build.ps1 -Help', result.stderr)
        self.assert_location_restored()


if __name__ == '__main__':
    unittest.main()
