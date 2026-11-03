"""Portable ownership/selection contracts; these do not certify a native Mac GUI."""
import copy
import importlib.util
import os
from pathlib import Path
import tempfile
import unittest
from unittest import mock

spec = importlib.util.spec_from_file_location('normal_launch', Path(__file__).with_name('normal-launch.py'))
launch = importlib.util.module_from_spec(spec)
spec.loader.exec_module(launch)


class NormalLaunchContracts(unittest.TestCase):
    def setUp(self):
        uid = mock.patch.object(os, 'getuid', return_value=4711, create=True)
        uid.start()
        self.addCleanup(uid.stop)
        self.snapshot = {'pid': 42, 'ownership': 'XRes-local-client-pid', 'complete': True, 'windows': [
            {'xid': 600, 'parent': 200, 'ownerPid': 42, 'mapState': 2, 'width': 880, 'height': 710,
             'canClose': True, 'name': 'Computator.NET'},
            {'xid': 601, 'parent': 600, 'ownerPid': 42, 'mapState': 2, 'x': 0, 'y': 22, 'width': 880, 'height': 688,
             'canClose': False, 'name': ''}]}

    def test_owned_client_is_direct_child_not_frame(self):
        whole, client = launch.select_windows(self.snapshot, 42)
        self.assertEqual((whole['xid'], client['xid']), (600, 601))

    def test_title_and_property_claim_do_not_replace_xres_pid(self):
        for mutation in ({'pid': 43}, {'ownership': '_NET_WM_PID'}, {'complete': False}):
            with self.subTest(mutation=mutation), self.assertRaises(RuntimeError):
                launch.select_windows(dict(self.snapshot, **mutation), 42)

    def test_foreign_or_duplicate_window_is_rejected(self):
        for key, value, message in (('ownerPid', 99, 'unowned window'), ('xid', 600, 'duplicate window identifiers')):
            snapshot = copy.deepcopy(self.snapshot)
            snapshot['windows'][1][key] = value
            with self.subTest(key=key), self.assertRaisesRegex(RuntimeError, message):
                launch.select_windows(snapshot, 42)

    def test_unmapped_splash_cannot_pass_main_window(self):
        self.snapshot['windows'][0]['mapState'] = 0
        self.assertIsNone(launch.select_windows(self.snapshot, 42))
        self.snapshot['windows'][0].update(mapState=2, width=400, height=240)
        self.assertIsNone(launch.select_windows(self.snapshot, 42))

    def test_ambiguous_main_or_client_is_rejected(self):
        for index in (0, 1):
            snapshot = copy.deepcopy(self.snapshot)
            extra = dict(snapshot['windows'][index], xid=900)
            snapshot['windows'].append(extra)
            with self.subTest(index=index), self.assertRaises(RuntimeError):
                launch.select_windows(snapshot, 42)

    def test_whole_window_without_its_real_client_is_rejected(self):
        self.snapshot['windows'][1]['parent'] = 200
        with self.assertRaises(RuntimeError):
            launch.select_windows(self.snapshot, 42)

    def test_client_drawable_is_clipped_to_owned_parent_not_titlebar_or_background(self):
        whole, client = launch.select_windows(self.snapshot, 42)
        client.update(width=888, height=718)
        self.assertEqual((0, 0, 880, 688), launch.visible_client_rectangle(whole, client))
        client.update(x=-4, y=-8)
        self.assertEqual((4, 8, 880, 710), launch.visible_client_rectangle(whole, client))
        client['y'] = 600
        with self.assertRaises(RuntimeError):
            launch.visible_client_rectangle(whole, client)

    def test_process_session_not_just_same_user_or_group(self):
        rows = [dict(pid=pid, ppid=1, pgid=group, uid=os.getuid(), command='fixture')
                for pid, group in ((10, 10), (11, 11), (12, 10))]
        sessions = {10: 10, 11: 10, 12: 99}
        self.assertEqual([10, 11], [row['pid'] for row in launch.owned_processes(rows, 10, sessions.__getitem__)])

    def test_vanished_process_is_not_retained_or_selected(self):
        def vanished(_):
            raise ProcessLookupError()
        row = dict(pid=10, ppid=1, pgid=10, uid=os.getuid(), command='fixture')
        self.assertEqual([], launch.owned_processes([row], 10, vanished))

    def test_application_requires_selected_native_executable_identity(self):
        with tempfile.TemporaryDirectory() as temporary:
            mono, foreign = Path(temporary) / 'mono', Path(temporary) / 'foreign'
            mono.write_text('selected', encoding='utf-8')
            foreign.write_text('foreign', encoding='utf-8')
            row = dict(pid=42, ppid=10, pgid=10, uid=os.getuid(), command='mono --config owned.config Computator.NET.exe')
            with mock.patch.object(launch, 'owned_processes', return_value=[row]):
                self.assertEqual(row, launch.application_process([], 10, mono, lambda _: mono))
                with self.assertRaises(RuntimeError):
                    launch.application_process([], 10, mono, lambda _: foreign)
                row['command'] += ' --preflight'
                self.assertIsNone(launch.application_process([], 10, mono, lambda _: mono))

    def test_two_application_processes_are_ambiguous(self):
        rows = [dict(pid=pid, ppid=10, pgid=10, uid=os.getuid(), command='mono Computator.NET.exe') for pid in (42, 43)]
        with mock.patch.object(launch, 'owned_processes', return_value=rows), self.assertRaises(RuntimeError):
            launch.application_process([], 10, Path('unused'))

    def test_output_must_stay_outside_entire_bundle(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            bundle = root / 'Computator.NET.app'
            bundle.mkdir()
            self.assertFalse(launch.outside(bundle / 'Contents/evidence', bundle))
            self.assertFalse(launch.outside(root / 'COMPUTATOR.NET.APP/evidence', bundle))
            self.assertTrue(launch.outside(root / 'external evidence', bundle))
            alias = root / 'alias'
            try:
                alias.symlink_to(bundle, target_is_directory=True)
            except OSError:
                return  # symlink privilege is platform-specific; lexical cases above always run
            self.assertFalse(launch.outside(alias / 'evidence', bundle))


if __name__ == '__main__':
    unittest.main()
