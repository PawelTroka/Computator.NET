"""Real POSIX process-group regressions; no Mono or graphical display required."""
import os
from pathlib import Path
import signal
import subprocess
import sys
import tempfile
import unittest


RUNNER = Path(__file__).with_name('run-bounded.py')


@unittest.skipUnless(os.name == 'posix', 'Requires POSIX process groups.')
class BoundedProcessContracts(unittest.TestCase):
    def run_command(self, code, *, seconds=10, arguments=(), child_environment=()):
        command = [sys.executable, str(RUNNER), '--seconds', str(seconds)]
        for entry in child_environment:
            command.extend(('--child-env', entry))
        command.extend((sys.executable, '-c', code, *map(str, arguments)))
        return subprocess.run(command, capture_output=True, text=True, timeout=seconds + 12)

    def test_preserves_success_failure_and_child_environment(self):
        for status in (0, 7):
            with self.subTest(status=status):
                result = self.run_command(
                    'import os, sys; print(os.environ["COMPUTATOR_BOUNDED_TEST"]); sys.exit(int(sys.argv[1]))',
                    arguments=(status,), child_environment=('COMPUTATOR_BOUNDED_TEST=value with spaces',))
                self.assertEqual(status, result.returncode, result.stderr)
                self.assertEqual('value with spaces\n', result.stdout)

    def test_timeout_preserves_status_124_for_a_stubborn_leader(self):
        result = self.run_command(
            'import signal, time; signal.signal(signal.SIGTERM, signal.SIG_IGN); time.sleep(60)', seconds=1)
        self.assertEqual(124, result.returncode, result.stderr)
        self.assertIn('exceeded 1 seconds', result.stderr)

    def run_descendant_fixture(self, timeout):
        with tempfile.TemporaryDirectory(prefix='computator-owned-group-') as temporary:
            root = Path(temporary)
            descendant = root / 'descendant.py'
            descendant.write_text(
                'import pathlib, signal, sys, time\n'
                'signal.signal(signal.SIGTERM, signal.SIG_IGN)\n'
                'print("descendant-ready", flush=True)\n'
                'pathlib.Path(sys.argv[1]).write_text("ready")\n'
                'time.sleep(60)\n', encoding='utf-8')
            leader = root / 'leader.py'
            leader.write_text(
                'import os, pathlib, subprocess, sys, time\n'
                'root = pathlib.Path(sys.argv[1])\n'
                '(root / "group").write_text(str(os.getpgrp()))\n'
                'subprocess.Popen([sys.executable, str(root / "descendant.py"), str(root / "ready")])\n'
                'while not (root / "ready").exists(): time.sleep(.01)\n'
                'if sys.argv[2] == "timeout": time.sleep(60)\n', encoding='utf-8')
            command = [sys.executable, str(RUNNER), '--seconds', '2' if timeout else '10',
                       sys.executable, str(leader), str(root), 'timeout' if timeout else 'success']
            # The descendant keeps these real output pipes open until it exits.
            # Reaping just the leader cannot make communicate() finish.
            process = subprocess.Popen(command, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
            completed = False
            try:
                try:
                    stdout, stderr = process.communicate(timeout=12)
                except subprocess.TimeoutExpired:
                    self.fail('An owned descendant survived cleanup and kept the output pipes open.')
                completed = True
                self.assertEqual(124 if timeout else 0, process.returncode, stderr)
                self.assertEqual('descendant-ready\n', stdout)
                if timeout:
                    self.assertIn('exceeded 2 seconds', stderr)
            finally:
                # Failure-only fixture cleanup, restricted to the group recorded
                # by this fixture. It cannot turn a failed assertion into a pass.
                if not completed:
                    if (root / 'group').exists():
                        try:
                            os.killpg(int((root / 'group').read_text()), signal.SIGKILL)
                        except ProcessLookupError:
                            pass
                    if process.poll() is None:
                        process.kill()
                    process.communicate(timeout=5)

    def test_success_terminates_stubborn_descendant_without_touching_other_session(self):
        peer = subprocess.Popen([sys.executable, '-c', 'import time; time.sleep(60)'], start_new_session=True)
        try:
            self.run_descendant_fixture(timeout=False)
            self.assertIsNone(peer.poll(), 'Cleanup must not terminate an unrelated process session.')
        finally:
            peer.terminate()
            peer.wait(timeout=5)

    def test_timeout_terminates_descendant_after_leader_accepts_sigterm(self):
        self.run_descendant_fixture(timeout=True)


if __name__ == '__main__':
    unittest.main(verbosity=2)
