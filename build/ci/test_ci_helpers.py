import unittest
from verified_sdk import checked_url, safe_name, select_file
from preflight import provider_metadata, require_versions


class BootstrapAndEvidenceTests(unittest.TestCase):
    def test_exact_versions_required(self):
        require_versions({'sdk': '10.0.401'}, {'sdk': '10.0.401'})
        with self.assertRaises(ValueError):
            require_versions({'sdk': '10.0.401'}, {'sdk': '10.0.400'})

    def test_github_mirror_cannot_claim_provider_execution(self):
        for provider in ('appveyor', 'travis'):
            with self.assertRaises(ValueError):
                provider_metadata(provider, {'GITHUB_ACTIONS': 'true'})
        self.assertEqual(provider_metadata('local', {})['nativeProviderRun'], 'false')

    def test_provider_flag_without_build_identity_is_insufficient(self):
        with self.assertRaises(ValueError):
            provider_metadata('appveyor', {'APPVEYOR': 'True'})
        with self.assertRaises(ValueError):
            provider_metadata('travis', {'TRAVIS': 'true'})

    def test_evidence_never_dumps_secret_environment_values(self):
        values = {'APPVEYOR': 'True', 'APPVEYOR_BUILD_ID': '1', 'APPVEYOR_JOB_ID': '2',
                  'APPVEYOR_REPO_COMMIT': 'a' * 40, 'APPVEYOR_REPO_NAME': 'owner/repo',
                  'APPVEYOR_BUILD_WORKER_IMAGE': 'Windows', 'SECRET': 'do-not-print', 'APPVEYOR_API_TOKEN': 'private'}
        result = provider_metadata('appveyor', values)
        self.assertNotIn('SECRET', result)
        self.assertNotIn('APPVEYOR_API_TOKEN', result)

    def test_archive_traversal_and_unapproved_sources_rejected(self):
        for name in ('../evil', '/absolute', 'C:\\outside', 'dir/../../evil'):
            with self.assertRaises(ValueError):
                safe_name(name)
        safe_name('sdk/10.0.401/dotnet.dll')
        for url in ('http://builds.dotnet.microsoft.com/file', 'https://example.com/sdk', 'https://user@builds.dotnet.microsoft.com/file'):
            with self.assertRaises(ValueError):
                checked_url(url)

    def test_metadata_selects_exact_version_and_rid(self):
        item = {'rid': 'linux-x64', 'url': 'https://builds.dotnet.microsoft.com/sdk.tar.gz', 'hash': 'a' * 128}
        data = {'releases': [{'sdk': {'version': '10.0.401', 'files': [item]}}]}
        self.assertEqual(select_file(data, '10.0.401', 'linux-x64'), item)
        for version, rid in [('10.0.400', 'linux-x64'), ('10.0.401', 'linux-arm64')]:
            with self.assertRaises(ValueError):
                select_file(data, version, rid)
        item['hash'] = 'not-a-hash'
        with self.assertRaises(ValueError):
            select_file(data, '10.0.401', 'linux-x64')


if __name__ == '__main__':
    unittest.main()
