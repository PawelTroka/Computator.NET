import tempfile
from pathlib import Path
import unittest
from versioning import identity, check, expected_block
from evidence import validate_report

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
        for base, revision in [('256.0.0','0'), ('3.0.65535','0'), ('03.0.0','0'), ('3.0.0','-1')]:
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

if __name__ == '__main__': unittest.main()
