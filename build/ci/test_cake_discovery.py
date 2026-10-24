"""Run after dotnet tool restore; exercise the real pinned Cake task graph."""

from pathlib import Path
import shutil
import subprocess
import tempfile
import unittest


ROOT = Path(__file__).resolve().parents[2]


class CakeDiscoveryTests(unittest.TestCase):
    def test_dry_run_does_not_prepare_build_output_or_windows_fixtures(self):
        # Copy only the graph and the manifests needed to load it. An accidental
        # setup/build cannot hide behind an existing build or fixture generator.
        sources = [ROOT / relative for relative in (
            'build.cake', 'Version.props', 'global.json',
            'build/ModernDesktop.props', '.config/dotnet-tools.json')]
        # Loaded helpers can live directly in build/ or in nested module folders.
        sources.extend((ROOT / 'build').rglob('*.cake'))
        for flag in ('--dryrun', '--noop', '--whatif'):
            with self.subTest(flag=flag), tempfile.TemporaryDirectory(prefix='computator cake discovery ') as temp:
                root = Path(temp)
                for source in sources:
                    destination = root / source.relative_to(ROOT)
                    destination.parent.mkdir(parents=True, exist_ok=True)
                    shutil.copyfile(source, destination)
                result = subprocess.run(
                    ['dotnet', 'cake', 'build.cake', '--target=Build', flag],
                    cwd=root, text=True, capture_output=True, timeout=120)
                self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
                self.assertIn('Restore', result.stdout)
                self.assertIn('Build', result.stdout)
                self.assertIn('No tasks were actually executed.', result.stdout)
                self.assertFalse((root / 'artifacts').exists(), 'Dry run executed build setup.')
                self.assertFalse((root / 'TestResults').exists(), 'Dry run prepared test results.')


if __name__ == '__main__':
    unittest.main()
