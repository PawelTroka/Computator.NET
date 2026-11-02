"""Contracts for the exact packaged-acceptance emitter used on native Macs."""
import copy
import hashlib
import json
from pathlib import Path
import platform
import subprocess
import sys
import tempfile
import unittest
from unittest import mock
import zipfile


SCRIPT = Path(__file__).with_name('validate-package.sh')
MARKER = '"$python_command" - "$resources" "$evidence" "$archive" "$repository" <<\'PY\'\n'
EMITTER = SCRIPT.read_text().split(MARKER, 1)[1].split('\nPY\n', 1)[0]


class CandidateAcceptanceContracts(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix='computator-candidate-contracts-')
        self.addCleanup(self.temporary.cleanup)
        self.base = Path(self.temporary.name)
        self.resources = self.base/'Computator.NET.app/Contents/Resources'
        (self.resources/'share').mkdir(parents=True)
        self.evidence = self.base/'evidence'
        (self.evidence/'normal-launch').mkdir(parents=True)
        (self.evidence/'normal-launch-image-contracts').mkdir()
        for name in ('relocated-native-runtime.json', 'native-runtime-data.json', 'launcher-preflight.json',
                     'preflight-contracts.log', 'normal-launch/normal-launch.json', 'normal-launch-contracts.log',
                     'normal-launch-image-contracts/image-contracts.json', 'mono-results.xml', 'candidate-contracts.log',
                     'capture-process-contracts.log', 'native-tool-process-contracts.log'):
            (self.evidence/name).write_text('retained actual-report fixture\n')
        self.archive = self.base/'Computator.NET-mono-osx-arm64.zip'
        self.reset()

    def reset(self):
        self.status = ''
        self.source = {'repository': 'https://github.com/PawelTroka/Computator.NET',
                       'commit': 'a'*40, 'tree': 'b'*40, 'trackedChanges': False}
        self.acceptance = {'passed': True, 'checkoutCommit': 'a'*40, 'checkoutDirty': False,
                           'architecture': 'arm64', 'sharedCases': 55,
                           'cases': ['case-'+str(number) for number in range(55)]}
        self.source_acceptance = copy.deepcopy(self.acceptance)
        self.manifest = {'rid': 'osx-arm64', 'source': self.source,
                         'sourceCheckoutAcceptance': self.source_acceptance}
        (self.evidence/'accepted-candidate.json').unlink(missing_ok=True)

    def save(self):
        (self.resources/'mono-package.json').write_text(json.dumps(self.manifest))
        (self.resources/'share/source-checkout-acceptance.json').write_text(json.dumps(self.source_acceptance))
        (self.evidence/'acceptance.json').write_text(json.dumps(self.acceptance))
        with zipfile.ZipFile(self.archive, 'w') as archive:
            archive.writestr('Computator.NET.app/Contents/Resources/mono-package.json',
                             (self.resources/'mono-package.json').read_bytes())
        digest = hashlib.sha256(self.archive.read_bytes()).hexdigest()
        (self.evidence/'archive.sha256').write_text(digest+'  '+str(self.archive)+'\n')

    def emit(self):
        def output(command, **unused):
            if command[0] == 'git':
                if command[-2:] == ['rev-parse', 'HEAD']: return 'a'*40+'\n'
                if command[-2:] == ['rev-parse', 'HEAD^{tree}']: return 'b'*40+'\n'
                if 'status' in command:
                    self.assertIn('--untracked-files=normal', command)
                    return self.status
            if command == ['/usr/bin/sw_vers', '-productVersion']: return '15.7\n'
            if command == ['/usr/bin/sw_vers', '-buildVersion']: return 'fixture-host\n'
            raise AssertionError('Unexpected host query: '+repr(command))
        with mock.patch.object(sys, 'argv', [str(SCRIPT), str(self.resources), str(self.evidence), str(self.archive), str(SCRIPT.resolve().parents[2])]), \
                mock.patch.object(subprocess, 'check_output', side_effect=output), \
                mock.patch.object(platform, 'machine', return_value='arm64'):
            exec(compile(EMITTER, str(SCRIPT), 'exec'), {'__name__': '__main__', '__file__': str(SCRIPT)})

    def reject(self, reason):
        with self.assertRaisesRegex(SystemExit, reason):
            self.emit()
        self.assertFalse((self.evidence/'accepted-candidate.json').exists())

    def test_clean_exact_source_and_archive_are_accepted(self):
        self.save(); self.emit()
        report = json.loads((self.evidence/'accepted-candidate.json').read_text())
        self.assertTrue(report['packagedRuntimeAccepted'])
        self.assertEqual(self.source, report['source'])
        self.assertEqual({'commit':'a'*40,'tree':'b'*40,'trackedChanges':False}, report['harnessSource'])
        self.assertEqual(hashlib.sha256((self.evidence/'mono-results.xml').read_bytes()).hexdigest(),
                         report['evidenceSha256']['mono-results.xml'])
        self.assertEqual(hashlib.sha256(self.archive.read_bytes()).hexdigest(), report['archive']['sha256'])

    def test_dirty_or_different_manifest_and_harness_sources_are_rejected(self):
        for field, value in (('trackedChanges', True), ('commit', 'c'*40), ('tree', 'd'*40)):
            with self.subTest(field=field):
                self.reset(); self.source[field] = value; self.save(); self.reject('clean.*source')
        for status in (' M tracked-source.cs\n', '?? untracked-source.cs\n'):
            with self.subTest(status=status):
                self.reset(); self.status = status; self.save(); self.reject('clean.*source')

    def test_both_source_and_packaged_acceptance_must_match_current_source(self):
        for owner in ('acceptance', 'source_acceptance'):
            for field, value in (('passed', False), ('checkoutDirty', True), ('checkoutCommit', 'c'*40),
                                 ('architecture', 'x86_64'), ('sharedCases', 54), ('cases', ['different-case'])):
                with self.subTest(owner=owner, field=field):
                    self.reset(); getattr(self, owner)[field] = value; self.save()
                    self.reject('source.*acceptance')

    def test_retained_source_acceptance_cannot_differ_from_manifest(self):
        self.save()
        (self.resources/'share/source-checkout-acceptance.json').write_text(json.dumps({'passed': False}))
        self.reject('source.*acceptance')

    def test_archive_changed_after_extraction_is_rejected(self):
        self.save()
        with self.archive.open('ab') as archive: archive.write(b'changed after extraction')
        self.reject('archive')

    def test_another_archive_manifest_cannot_claim_the_tested_payload(self):
        self.save()
        changed = copy.deepcopy(self.manifest); changed['source']['commit'] = 'c'*40
        with zipfile.ZipFile(self.archive, 'w') as archive:
            archive.writestr('Computator.NET.app/Contents/Resources/mono-package.json', json.dumps(changed))
        (self.evidence/'archive.sha256').write_text(hashlib.sha256(self.archive.read_bytes()).hexdigest()+'  '+str(self.archive)+'\n')
        self.reject('archive')


if __name__ == '__main__':
    unittest.main()
