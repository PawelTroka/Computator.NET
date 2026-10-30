"""Contract checks for additive compiler diagnostics; no timing assertions."""
import unittest
from compiler_comparison_manifest import memory_summary, scenarios


class CompilerComparisonContracts(unittest.TestCase):
    def test_forced_roslyn_workloads_use_identical_original_operation_counts(self):
        definitions = scenarios()
        for name in ('real', 'complex', 'surface'):
            self.assertEqual(definitions[name], definitions['roslyn-' + name])
        self.assertEqual(definitions['real']['operations']['compile.custom.changed.ms'], 1)
        self.assertEqual(definitions['memory-script']['operations']['edits.compile.and.check.batch.ms'], 33)
        self.assertTrue(all(metric.endswith('.ms') for definition in definitions.values() for metric in definition['metrics']))

    @staticmethod
    def raw():
        fields = ['allocatedManagedBytes', 'managedBytesBefore', 'managedBytesBeforeCollection',
                  'managedBytesAfterCollection', 'privateBytesBefore', 'privateBytesBeforeCollection',
                  'privateBytesAfterCollection', 'workingSetBytesBefore', 'workingSetBytesBeforeCollection',
                  'workingSetBytesAfterCollection', 'peakWorkingSetBytes']
        result = {'completed': True, 'postRunArtifactVerification': {'passed': True}, 'configuration': {'pairs': 2}, 'trials': []}
        for scenario in ('memory-roslyn-real', 'memory-script'):
            for pair in range(2):
                invocations = []
                for version in ('baseline', 'candidate'):
                    details = {field: (100 + 20 * pair) if version == 'baseline' else (80 + 10 * pair) for field in fields}
                    details.update(compilations=33, directCompilerDisabled=True)
                    invocations.append({'id': version, 'version': version, 'exitCode': 0, 'timedOut': False, 'errors': [],
                                        'record': {'correctness': True, 'details': details}})
                result['trials'].append({'id': scenario + str(pair), 'scenario': scenario, 'pair': pair, 'invocations': invocations})
        return result

    def test_memory_uses_process_byte_counters_without_unit_conversion(self):
        summary = memory_summary(self.raw())
        value = summary['scenarios']['memory-script']
        self.assertEqual(value['medians']['baseline']['privateBytesAfterCollection'], 110)
        self.assertEqual(value['medians']['candidate']['allocatedManagedBytes'], 85)
        self.assertEqual(len(value['processes']['baseline']), 2)

    def test_failed_incomplete_and_changed_workloads_are_not_silently_dropped(self):
        for mutation in ('failed', 'missing', 'duplicate', 'count', 'bytes', 'artifacts'):
            raw = self.raw()
            item = raw['trials'][0]['invocations'][0]
            if mutation == 'failed': item['exitCode'] = 1
            elif mutation == 'missing': raw['trials'].pop()
            elif mutation == 'duplicate': raw['trials'][1]['pair'] = raw['trials'][0]['pair']
            elif mutation == 'count': item['record']['details']['compilations'] = 32
            elif mutation == 'bytes': item['record']['details']['privateBytesAfterCollection'] = True
            elif mutation == 'artifacts': raw['postRunArtifactVerification']['passed'] = False
            with self.subTest(mutation=mutation), self.assertRaises(ValueError):
                memory_summary(raw)


if __name__ == '__main__':
    unittest.main()
