"""Contracts and descriptive byte counters for the current compiler comparison.

The historical workload, supervisor and inference code remain unchanged. Memory
is reported in bytes, never inserted into compare.py's millisecond metric family.
"""
import argparse
import copy
import json
import statistics
import xml.etree.ElementTree as ET
from pathlib import Path


def validate_package_pin_refactor(before, after):
    """Admit only the reviewed literal-to-property refactor, not package drift."""
    baseline, candidate = ET.fromstring(before), ET.fromstring(after)
    baseline_items = baseline.findall("./ItemGroup/PackageVersion[@Include='Microsoft.CodeAnalysis.CSharp']")
    candidate_items = candidate.findall("./ItemGroup/PackageVersion[@Include='Microsoft.CodeAnalysis.CSharp']")
    if len(baseline_items) != 1 or len(candidate_items) != 1 or baseline_items[0].get('Version') != '5.9.0':
        raise ValueError('Expected the pinned baseline Roslyn package 5.9.0 exactly once.')
    properties = candidate.findall('./PropertyGroup/ComputatorRoslynPackageVersion')
    if len(properties) != 1 or properties[0].attrib or list(properties[0]) or (properties[0].text or '').strip() != '5.9.0':
        raise ValueError('Only the unconditional ComputatorRoslynPackageVersion=5.9.0 refactor is permitted.')
    prop = properties[0]
    group = next(group for group in candidate.findall('./PropertyGroup') if prop in group)
    if group.attrib or candidate_items[0].get('Version') != '$(ComputatorRoslynPackageVersion)':
        raise ValueError('Roslyn must use the unconditional reviewed package-version property.')
    group.remove(prop)
    if not list(group):
        candidate.remove(group)
    candidate_items[0].set('Version', '5.9.0')
    def normalized(root):
        return ET.canonicalize(ET.tostring(root, encoding='unicode'), strip_text=True)
    if normalized(baseline) != normalized(candidate):
        raise ValueError('Package pins or MSBuild conditions changed beyond the approved Roslyn property refactor.')
    return {'scope': 'Only the existing Roslyn 5.9.0 literal was replaced by an unconditional same-value property.',
            'baselineRoslynVersion': '5.9.0', 'candidateRoslynVersion': '5.9.0',
            'allOtherPackageDeclarationsUnchanged': True}


def scenarios():
    common = {'engine.initialize.ms': 1, 'evaluator.create.ms': 1,
              'compile.first.ms': 1, 'first.interaction.ms': 1}
    definitions = {
        'real': {**common, 'evaluate.first.ms': 1, 'evaluate.typed.batch.ms': 65536,
                 'evaluate.public.batch.ms': 65536, 'compile.same.warm.ms': 1,
                 'compile.custom.changed.ms': 1, 'integral.simpson.batch.ms': 32},
        'complex': {**common, 'evaluate.first.ms': 1, 'evaluate.typed.batch.ms': 65536,
                    'compile.same.warm.ms': 1},
        'surface': {**common, 'evaluate.first.ms': 1, 'chart.grid.sample.ms': 4096,
                    'compile.same.warm.ms': 1},
        'script': {**common, 'script.first.ms': 1, 'script.repeated.batch.ms': 32,
                   'compile.same.warm.ms': 1},
    }
    for kind in ('real', 'complex', 'surface'):
        definitions['roslyn-' + kind] = copy.deepcopy(definitions[kind])
    for kind in ('memory-roslyn-real', 'memory-script'):
        definitions[kind] = {'edits.first.compile.ms': 1, 'edits.compile.and.check.batch.ms': 33}
    return {name: {'metrics': sorted(operations), 'operations': operations}
            for name, operations in definitions.items()}


def memory_summary(raw):
    if not raw.get('completed') or raw.get('postRunArtifactVerification', {}).get('passed') is not True:
        raise ValueError('Cannot summarize an incomplete or unauthenticated run.')
    fields = ['allocatedManagedBytes', 'managedBytesBefore', 'managedBytesBeforeCollection',
              'managedBytesAfterCollection', 'privateBytesBefore', 'privateBytesBeforeCollection',
              'privateBytesAfterCollection', 'workingSetBytesBefore', 'workingSetBytesBeforeCollection',
              'workingSetBytesAfterCollection', 'peakWorkingSetBytes']
    expected_pairs = raw['configuration']['pairs']
    result = {'scope': 'Descriptive fixed-count memory diagnostics, bytes; process medians are not allocation throughput or leak proofs.',
              'pairs': expected_pairs, 'scenarios': {}}
    for scenario in ('memory-roslyn-real', 'memory-script'):
        trials = [trial for trial in raw['trials'] if trial['scenario'] == scenario]
        if len(trials) != expected_pairs or len({trial['pair'] for trial in trials}) != expected_pairs:
            raise ValueError('Missing or duplicate memory process pair: ' + scenario)
        rows = {version: [] for version in ('baseline', 'candidate')}
        for trial in trials:
            if len(trial['invocations']) != 2 or {item['version'] for item in trial['invocations']} != set(rows):
                raise ValueError('Incomplete memory pair: ' + trial['id'])
            for item in trial['invocations']:
                record = item.get('record', {})
                if item.get('errors') or item.get('exitCode') != 0 or item.get('timedOut') or record.get('correctness') is not True:
                    raise ValueError('Invalid memory observation: ' + item['id'])
                detail = record.get('details', {})
                if detail.get('compilations') != 33 or detail.get('directCompilerDisabled') is not True:
                    raise ValueError('Changed fixed memory workload.')
                if any(not isinstance(detail.get(field), int) or isinstance(detail[field], bool) or detail[field] < 0 for field in fields):
                    raise ValueError('Missing or invalid byte counter.')
                rows[item['version']].append({field: detail[field] for field in fields})
        result['scenarios'][scenario] = {
            'processes': rows,
            'medians': {version: {field: statistics.median(row[field] for row in rows[version]) for field in fields} for version in rows},
        }
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--scenarios-output', type=Path)
    parser.add_argument('--raw', type=Path)
    parser.add_argument('--memory-output', type=Path)
    parser.add_argument('--baseline-packages', type=Path)
    parser.add_argument('--candidate-packages', type=Path)
    args = parser.parse_args()
    if args.baseline_packages and args.candidate_packages:
        print(json.dumps(validate_package_pin_refactor(args.baseline_packages.read_text(encoding='utf-8-sig'),
                                                      args.candidate_packages.read_text(encoding='utf-8-sig')), indent=2))
    elif args.scenarios_output:
        args.scenarios_output.write_text(json.dumps(scenarios(), indent=2) + '\n', encoding='utf-8')
    elif args.raw and args.memory_output:
        result = memory_summary(json.loads(args.raw.read_text(encoding='utf-8-sig')))
        args.memory_output.write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
        print(json.dumps({name: data['medians'] for name, data in result['scenarios'].items()}, indent=2))
    else:
        parser.error('Supply --scenarios-output, --raw with --memory-output, or both --baseline-packages and --candidate-packages.')


if __name__ == '__main__':
    main()
