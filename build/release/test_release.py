import tempfile
from datetime import datetime, timezone
from pathlib import Path
import unittest
from unittest.mock import patch
from versioning import identity, check, expected_block
from evidence import (begin, finish, validate_report, fresh_file, required_artifacts, required_reports,
                      digest, required_gsl_directories, packaged_gsl_evidence)
import json
import os
import time

class ReleaseTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.root = Path(self.temporary.name)
    def tearDown(self): self.temporary.cleanup()
    def version(self, base='3.0.0', revision='0', label='preview.1'):
        (self.root / 'Version.props').write_text(f'<Project><ComputatorBaseVersion>{base}</ComputatorBaseVersion><ComputatorRevision>{revision}</ComputatorRevision><ComputatorPrerelease>{label}</ComputatorPrerelease></Project>')
    def test_identity_maps_all_channels(self):
        self.version(); value = identity(self.root)
        self.assertEqual(value['productVersion'], '3.0.0-preview.1')
        self.assertEqual(value['msiVersion'], '3.0.0')
        self.assertEqual(value['msixVersion'], value['assemblyVersion'])
    def test_out_of_range_and_noncanonical_versions_fail(self):
        for base, revision in [('256.0.0','0'), ('3.0.65535','0'), ('03.0.0','0'), ('3.0.0','-1'), ('0.1.0','0'), ('3.0.0','1')]:
            with self.subTest(base=base, revision=revision):
                self.version(base, revision)
                with self.assertRaises(ValueError): identity(self.root)
    def test_drift_fails_without_source_write(self):
        self.version(); original = expected_block({**identity(self.root), 'fileVersion':'1.0.0.0'})
        file = self.root / 'SharedAssemblyInfo.cs'; file.write_text(original)
        with self.assertRaises(ValueError): check(self.root)
        self.assertEqual(file.read_text(), original)
        check(self.root, write=True); check(self.root)
    def test_browser_failure_skip_and_missing_cases_rejected(self):
        file = self.root / 'results.xml'
        for child in ('<failure/>', '<error/>', '<skipped/>'):
            file.write_text(f'<testsuite><testcase name="a" classname="b">{child}</testcase></testsuite>')
            with self.assertRaises(ValueError): validate_report(file, 1)
        file.write_text('<testsuite/>')
        with self.assertRaises(ValueError): validate_report(file, 1)
    def test_duplicate_browser_identity_rejected(self):
        file = self.root / 'results.xml'; file.write_text('<testsuite><testcase name="a" classname="b"/><testcase name="a" classname="b"/></testsuite>')
        with self.assertRaises(ValueError): validate_report(file, 2)
    def test_successful_report_records_hash(self):
        file = self.root / 'results.xml'; file.write_text('<testsuite><testcase name="a" classname="b"/></testsuite>')
        value = validate_report(file, 1)
        self.assertEqual(value['passed'], 1); self.assertEqual(len(value['sha256']), 64)
    def browser_report(self, projects, duplicate=False):
        suites = []
        for project in projects:
            cases = ''.join(f'<testcase name="case-{i}" classname="workflows"/>' for i in range(5))
            if duplicate: cases += '<testcase name="case-0" classname="workflows"/>'
            suites.append(f'<testsuite name="workflows" hostname="{project}">{cases}</testsuite>')
        return '<testsuites>' + ''.join(suites) + '</testsuites>'
    def test_same_names_in_chromium_and_firefox_are_distinct(self):
        file = self.root / 'results.xml'
        file.write_text(self.browser_report(['chromium', 'firefox']))
        self.assertEqual(validate_report(file, 10)['passed'], 10)
        file = self.root / 'e2e.xml'
        file.write_text(self.browser_report(['real-chromium', 'real-firefox']))
        self.assertEqual(validate_report(file, 10)['passed'], 10)
    def test_duplicate_same_project_and_missing_browser_are_rejected(self):
        file = self.root / 'results.xml'
        for projects, duplicate in [(['chromium', 'firefox'], True), (['chromium', 'chromium'], False),
                                    (['real-chromium', 'real-firefox'], False)]:
            with self.subTest(projects=projects, duplicate=duplicate):
                file.write_text(self.browser_report(projects, duplicate))
                with self.assertRaises(ValueError): validate_report(file, 10)

    def test_duplicate_test_with_new_execution_id_does_not_inflate_trx_count(self):
        file = self.root / 'core.trx'
        results = ''.join(f'<UnitTestResult executionId="{i}" testId="same-test" outcome="Passed"/>' for i in range(2))
        file.write_text(f'<TestRun><Results>{results}</Results><TestDefinitions><UnitTest id="same-test"/></TestDefinitions>'
            '<ResultSummary outcome="Completed"><Counters total="2" passed="2"/></ResultSummary></TestRun>')
        with self.assertRaises(ValueError): validate_report(file, 2)

    def test_aborted_trx_run_with_passing_cases_is_rejected(self):
        file = self.root / 'core.trx'
        file.write_text('<TestRun><Results><UnitTestResult testId="case" outcome="Passed"/></Results>'
            '<TestDefinitions><UnitTest id="case"/></TestDefinitions>'
            '<ResultSummary outcome="Aborted"><Counters total="1" passed="1"/></ResultSummary></TestRun>')
        with self.assertRaises(ValueError): validate_report(file, 1)

    def test_old_candidate_cannot_supply_missing_current_artifact(self):
        file = self.root / 'candidate.zip'; file.write_bytes(b'old payload')
        os.utime(file, (1, 1))
        with self.assertRaises(ValueError): fresh_file(self.root, file.name, time.time())
        with self.assertRaises(ValueError): fresh_file(self.root, 'missing.zip', time.time())

    def test_inventory_requires_exact_version_and_local_package(self):
        self.version()
        (self.root / 'build/followups').mkdir(parents=True)
        (self.root / 'build/ModernDesktop.props').write_text('<Project><TargetFramework>net10.0-windows</TargetFramework></Project>')
        (self.root / 'build/followups/local-web.cake').touch()
        artifacts = required_artifacts(self.root, identity(self.root))
        self.assertIn('publish/Computator.NET.v3.0.0.0.net10.0-windows.zip', artifacts)
        self.assertIn('publish/Computator.NET.LocalWeb.win-x64.zip', artifacts)

    def test_local_package_requires_both_browser_projects(self):
        file = self.root / 'local-package.xml'
        file.write_text(self.browser_report(['chromium', 'firefox']))
        validate_report(file, 2)
        file.write_text(self.browser_report(['chromium']))
        with self.assertRaises(ValueError): validate_report(file, 2)

    def trx_report(self, count):
        results = ''.join(f'<UnitTestResult testId="case-{i}" outcome="Passed"/>' for i in range(count))
        definitions = ''.join(f'<UnitTest id="case-{i}"/>' for i in range(count))
        return (f'<TestRun><Results>{results}</Results><TestDefinitions>{definitions}</TestDefinitions>'
                f'<ResultSummary outcome="Completed"><Counters total="{count}" passed="{count}"/></ResultSummary></TestRun>')

    def gsl_candidate(self):
        started = time.time()
        reports = {}
        for index, directory in enumerate(required_gsl_directories(self.root)):
            native = self.root / directory / 'native/gsl'
            native.mkdir(parents=True, exist_ok=True)
            files = ('gsl.dll', 'gslcblas.dll', 'COPYING.GSL', 'GSL-2.8-source.zip', 'SOURCE.GSL.txt')
            for name in files:
                (native / name).write_bytes((directory + ': fixture ' + name).encode())
            provenance = {'schemaVersion': 1, 'version': '2.8', 'rid': 'win-x64', 'entryPoint': 'gsl.dll',
                          'files': {name: digest(native / name) for name in files}}
            (native / 'gsl-provenance.json').write_text(json.dumps(provenance))
            report = self.root / f'TestResults/packaged-gsl/probe-{index}/result.json'
            report.parent.mkdir(parents=True, exist_ok=True)
            report.write_text(json.dumps({'schemaVersion': 1, 'completedUtc': datetime.now(timezone.utc).isoformat(),
                'passed': True, 'publishedDirectory': str(self.root / directory),
                'expectedLibrary': str(native / 'gsl.dll'), 'selectedLibrary': str(native / 'gsl.dll'),
                'nativeVersion': '2.8', 'nativeHash': digest(native / 'gsl.dll').upper(),
                'checks': {'gamma': 2, 'gammaP': 0.26424111765711533, 'AiPrime': -0.2588194037928068, 'BesselJ0': 1},
                'failure': None}))
            reports[directory] = report
        return started, reports

    def test_gsl_requires_each_publish_directory_and_optional_local_api(self):
        self.assertEqual(required_gsl_directories(self.root), ['artifacts/desktop', 'artifacts/api', 'artifacts/distribution/win-x64'])
        (self.root / 'build/followups').mkdir(parents=True)
        (self.root / 'build/followups/local-web.cake').touch()
        started, reports = self.gsl_candidate()
        self.assertEqual(set(packaged_gsl_evidence(self.root, started)), set(reports))
        self.assertIn('artifacts/local-web/api', reports)
        for directory, path in reports.items():
            with self.subTest(directory=directory):
                original = path.read_bytes(); path.unlink()
                with self.assertRaisesRegex(ValueError, directory): packaged_gsl_evidence(self.root, started)
                path.write_bytes(original)

    def test_gsl_rejects_stale_report_even_with_current_claimed_time(self):
        started, reports = self.gsl_candidate()
        path = reports['artifacts/desktop']
        os.utime(path, (1, 1))
        with self.assertRaisesRegex(ValueError, 'Missing/stale'): packaged_gsl_evidence(self.root, started)

    def test_gsl_rejects_failed_misdirected_or_stale_claims(self):
        started, reports = self.gsl_candidate()
        path = reports['artifacts/api']; original = json.loads(path.read_text())
        for field, value in [('schemaVersion', 2), ('schemaVersion', True), ('passed', False), ('passed', 1),
                             ('failure', 'probe failed'), ('nativeVersion', '1.16'), ('nativeHash', '0' * 64),
                             ('expectedLibrary', str(self.root / 'old/gsl.dll')),
                             ('selectedLibrary', str(self.root / 'old/gsl.dll')),
                             ('publishedDirectory', str(self.root / 'another-checkout/artifacts/api')),
                             ('completedUtc', '2000-01-01T00:00:00+00:00'),
                             ('completedUtc', datetime.fromtimestamp(started - 0.1, timezone.utc).isoformat()),
                             ('completedUtc', datetime.fromtimestamp(time.time() + 60, timezone.utc).isoformat()),
                             ('completedUtc', datetime.now().isoformat()), ('completedUtc', None)]:
            with self.subTest(field=field, value=value):
                path.write_text(json.dumps({**original, field: value}))
                with self.assertRaises(ValueError): packaged_gsl_evidence(self.root, started)

    def test_gsl_checks_must_be_present_finite_numeric_and_correct(self):
        started, reports = self.gsl_candidate()
        path = reports['artifacts/api']; original = json.loads(path.read_text())
        for name in original['checks']:
            for value in (None, float('nan'), float('inf'), True, '2', 99):
                with self.subTest(name=name, value=value):
                    checks = {**original['checks'], name: value}
                    if value is None: del checks[name]
                    path.write_text(json.dumps({**original, 'checks': checks}))
                    with self.assertRaisesRegex(ValueError, name): packaged_gsl_evidence(self.root, started)

    def test_gsl_current_dll_must_match_successful_probe(self):
        started, _ = self.gsl_candidate()
        (self.root / 'artifacts/api/native/gsl/gsl.dll').write_bytes(b'replaced after the probe')
        with self.assertRaisesRegex(ValueError, 'version/hash'): packaged_gsl_evidence(self.root, started)

    def test_gsl_latest_failed_probe_cannot_fall_back_to_earlier_success(self):
        started, reports = self.gsl_candidate()
        old = reports['artifacts/api']; data = json.loads(old.read_text())
        latest = old.parent.with_name('later-probe') / 'result.json'
        latest.parent.mkdir(); latest.write_text(json.dumps({**data, 'passed': False, 'failure': 'failed'}))
        os.utime(old, (started, started)); os.utime(latest, (started + 1, started + 1))
        with self.assertRaisesRegex(ValueError, 'Failed/invalid'): packaged_gsl_evidence(self.root, started)
        old.write_text(json.dumps({**data, 'passed': False})); os.utime(old, (started, started))
        latest.write_text(json.dumps(data)); os.utime(latest, (started + 1, started + 1))
        value = packaged_gsl_evidence(self.root, started)['artifacts/api']
        self.assertEqual(value['report'], latest.relative_to(self.root).as_posix())

    def test_gsl_provenance_must_exist_and_match_current_native_payload(self):
        started, _ = self.gsl_candidate()
        path = self.root / 'artifacts/api/native/gsl/gsl-provenance.json'; original = path.read_text()
        path.unlink()
        with self.assertRaisesRegex(ValueError, 'gsl-provenance'): packaged_gsl_evidence(self.root, started)
        path.write_text(original)
        (path.parent / 'gslcblas.dll').write_bytes(b'changed BLAS')
        with self.assertRaisesRegex(ValueError, 'provenance hash mismatch'): packaged_gsl_evidence(self.root, started)

    def test_gsl_provenance_allows_additional_source_files_and_cached_timestamps(self):
        started, _ = self.gsl_candidate()
        path = self.root / 'artifacts/api/native/gsl/gsl-provenance.json'; data = json.loads(path.read_text())
        for name in ('extra-build-recipe.txt', 'extra-license.txt'):
            (path.parent / name).write_bytes(b'fixture source distribution')
            data['files'][name] = digest(path.parent / name)
        path.write_text(json.dumps(data))
        for file in path.parent.iterdir(): os.utime(file, (1, 1))
        value = packaged_gsl_evidence(self.root, started)['artifacts/api']
        self.assertEqual(value['provenanceSha256'], digest(path))
        (path.parent / 'GSL-2.8-source.zip').write_bytes(b'changed source archive')
        with self.assertRaisesRegex(ValueError, 'provenance hash mismatch'): packaged_gsl_evidence(self.root, started)

    def test_gsl_source_archive_and_notice_are_required_files_and_provenance_entries(self):
        started, _ = self.gsl_candidate()
        path = self.root / 'artifacts/api/native/gsl/gsl-provenance.json'; original = json.loads(path.read_text())
        for name in ('GSL-2.8-source.zip', 'SOURCE.GSL.txt'):
            with self.subTest(name=name):
                file = path.parent / name; content = file.read_bytes(); file.unlink()
                with self.assertRaisesRegex(ValueError, name): packaged_gsl_evidence(self.root, started)
                file.write_bytes(content)
                data = {**original, 'files': dict(original['files'])}; del data['files'][name]
                path.write_text(json.dumps(data))
                with self.assertRaisesRegex(ValueError, 'provenance'): packaged_gsl_evidence(self.root, started)
                path.write_text(json.dumps(original))

    def test_gsl_provenance_rejects_missing_entries_and_path_traversal(self):
        started, _ = self.gsl_candidate()
        path = self.root / 'artifacts/api/native/gsl/gsl-provenance.json'; original = json.loads(path.read_text())
        for name in ('../gsl.dll', '..\\gsl.dll', '.', ''):
            path.write_text(json.dumps({**original, 'files': {**original['files'], name: '0' * 64}}))
            with self.assertRaisesRegex(ValueError, 'filename'): packaged_gsl_evidence(self.root, started)
        path.write_text(json.dumps({**original, 'files': {'gsl.dll': original['files']['gsl.dll']}}))
        with self.assertRaisesRegex(ValueError, 'provenance'): packaged_gsl_evidence(self.root, started)

    def reference_candidate(self):
        # Exercise the actual begin/finish orchestration. Only Git is substituted;
        # every required report and file is real and confined to this fixture.
        fake_git = patch('evidence.git', side_effect=lambda root, *args: 'candidate-commit' if args[0] == 'rev-parse' else '')
        fake_git.start()
        self.addCleanup(fake_git.stop)
        self.version()
        (self.root / 'SharedAssemblyInfo.cs').write_text(expected_block(identity(self.root)))
        (self.root / 'build/followups').mkdir(parents=True)
        (self.root / 'build/followups/local-web.cake').touch()
        (self.root / 'build/ModernDesktop.props').write_text('<Project><TargetFramework>net10.0-windows</TargetFramework></Project>')
        (self.root / 'Computator.NET.slnx').write_text('<Solution/>')
        reference = self.root / 'TestResults/reference.trx'
        reference.parent.mkdir(parents=True)
        reference.write_text('previous run must not survive begin')
        begin(self.root)
        self.assertFalse(reference.exists())
        for relative, count in required_reports(self.root).items():
            path = self.root / relative
            path.parent.mkdir(parents=True, exist_ok=True)
            if path.suffix == '.trx':
                path.write_text(self.trx_report(count))
            else:
                projects = ('chromium', 'firefox') if path.name in ('results.xml', 'local-package.xml') else ('real-chromium', 'real-firefox')
                cases = ''.join(f'<testcase classname="workflow" name="case-{i}"/>' for i in range(count // 2))
                path.write_text('<testsuites>' + ''.join(f'<testsuite hostname="{project}" name="workflow">{cases}</testsuite>' for project in projects) + '</testsuites>')
        for relative in required_artifacts(self.root, identity(self.root)):
            path = self.root / relative
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(b'current candidate fixture')
        self.gsl_candidate()
        return reference

    def test_release_cannot_omit_or_reuse_reference_validation(self):
        reference = self.reference_candidate()
        self.assertEqual(required_reports(self.root)['TestResults/reference.trx'], 166)
        for state in ('missing', 'stale', 'incomplete'):
            with self.subTest(state=state):
                if state == 'missing':
                    reference.unlink()
                else:
                    reference.write_text(self.trx_report(152 if state == 'incomplete' else 153))
                    timestamp = 1 if state == 'stale' else time.time()
                    os.utime(reference, (timestamp, timestamp))
                with self.assertRaisesRegex(ValueError, 'reference.trx'):
                    finish(self.root)
                self.assertFalse((self.root / 'artifacts/release-evidence/manifest.json').exists())

    def test_release_manifest_binds_reference_test_and_rendered_artifacts(self):
        self.reference_candidate()
        finish(self.root)
        manifest = json.loads((self.root / 'artifacts/release-evidence/manifest.json').read_text())
        self.assertEqual(manifest['reports']['TestResults/reference.trx']['passed'], 166)
        for filename in ('inventory.json', 'coverage.json', 'preview.html'):
            relative = 'artifacts/reference/' + filename
            self.assertEqual(manifest['artifacts'][relative]['sha256'], digest(self.root / relative))

    def test_release_manifest_binds_each_gsl_report_and_current_provenance(self):
        self.reference_candidate()
        finish(self.root)
        manifest = json.loads((self.root / 'artifacts/release-evidence/manifest.json').read_text())
        self.assertEqual(set(manifest['packagedGsl']), set(required_gsl_directories(self.root)))
        for observation in manifest['packagedGsl'].values():
            for field in ('report', 'provenance'):
                relative = observation[field]
                self.assertEqual(observation[field + 'Sha256'], digest(self.root / relative))
                self.assertEqual(manifest['artifacts'][relative]['sha256'], digest(self.root / relative))
            self.assertEqual(observation['nativeSha256'], digest(self.root / observation['nativeLibrary']))
        self.assertIn('self-reported', manifest['packagedGslScope'])

    def test_release_rejects_missing_gsl_evidence_and_removes_old_acceptance(self):
        self.reference_candidate()
        finish(self.root)
        report = next((self.root / 'TestResults/packaged-gsl').glob('*/result.json'))
        report.unlink()
        with self.assertRaisesRegex(ValueError, 'Missing/stale packaged GSL'): finish(self.root)
        self.assertFalse((self.root / 'artifacts/release-evidence/manifest.json').exists())

if __name__ == '__main__': unittest.main()
