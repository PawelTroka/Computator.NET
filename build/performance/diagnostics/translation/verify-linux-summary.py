#!/usr/bin/env python3
"""Recompute retained numeric Linux conclusions without publishing host metadata.

The optional --raw verifies the untouched full original file by SHA256 and checks
its independent process medians. Without --raw, the published numeric medians
still reproduce every timing point estimate, adjusted interval and classification.
"""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import math
import statistics
import sys


def require(condition, message):
    if not condition:
        raise ValueError(message)


def close(actual, expected):
    require(math.isclose(actual, expected, rel_tol=1e-12, abs_tol=1e-12), f'Numeric mismatch: {actual} != {expected}')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--supervisor', type=Path, required=True)
    parser.add_argument('--summary', type=Path, default=Path(__file__).with_name('linux-summary.json'))
    parser.add_argument('--raw', type=Path)
    args = parser.parse_args()
    protocol = json.loads(Path(__file__).with_name('protocol.json').read_text(encoding='utf-8'))
    require(hashlib.sha256(args.supervisor.read_bytes()).hexdigest() == protocol['supervisorSha256'], 'Use the exact recorded compare.py supervisor.')
    spec = importlib.util.spec_from_file_location('translation_recorded_supervisor', args.supervisor)
    supervisor = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = supervisor
    spec.loader.exec_module(supervisor)
    summary = json.loads(args.summary.read_text(encoding='utf-8'))
    config = summary['configuration']
    rows = summary['metrics']
    require(len(rows) == summary['familyMetricCount'] == 6, 'Declared family completeness differs.')
    require(config['pairs'] == 20, 'Original experiment had 20 independent pairs.')
    raw = None
    if args.raw:
        require(hashlib.sha256(args.raw.read_bytes()).hexdigest() == summary['rawSha256'], 'Original full raw hash differs.')
        raw = json.loads(args.raw.read_text(encoding='utf-8'))
        require(raw['completed'] is True and raw['postRunArtifactVerification'] == {'passed': True, 'errors': []}, 'Original run is incomplete or failed identity verification.')
    tail = (1 - config['confidence']) / (2 * len(rows))
    for row in rows:
        name = row['metric']
        b, c = row['baselineProcessMediansMs'], row['candidateProcessMediansMs']
        require(len(b) == len(c) == config['pairs'], 'Independent process count differs.')
        require(all(isinstance(x, (int, float)) and not isinstance(x, bool) and math.isfinite(x) and x > 0 for x in b + c), 'Invalid process latency.')
        if raw:
            actual = {'baseline': [], 'candidate': []}
            for trial in raw['trials']:
                require(trial['scenario'] == 'translation', 'Unexpected raw scenario.')
                for invocation in trial['invocations']:
                    record = invocation['record']
                    require(record['correctness'] is True and not invocation['errors'], 'Original correctness or invocation failed.')
                    actual[invocation['version']].append(statistics.median(record['metrics'][name]))
            require(actual['baseline'] == b and actual['candidate'] == c, 'Published process medians differ from original raw.')
        seed = config['seed'] + int.from_bytes(hashlib.sha256(('translation/' + name).encode()).digest()[:8], 'big')
        point, lower, upper = supervisor.paired_log_ratio_interval(b, c, samples=config['bootstrapSamples'], seed=seed, tail_probability=tail)
        close(point, row['geometricMeanPairedRatio'])
        close(lower, row['adjustedRatioInterval']['lower'])
        close(upper, row['adjustedRatioInterval']['upper'])
        close(statistics.median(b) / row['operationsPerBatch'] * 1000, row['baselineMedianMicrosecondsPerOperation'])
        close(statistics.median(c) / row['operationsPerBatch'] * 1000, row['candidateMedianMicrosecondsPerOperation'])
        require(supervisor.classify(lower, upper, config['practicalThreshold'], pairs=config['pairs'], expected_tail_draws=config['bootstrapSamples'] * tail, resolution_limited=row['timerResolutionLimited']) == row['classification'], 'Classification differs.')
    print('Verified all six published timing point estimates, adjusted intervals and classifications' + (' against the untouched original raw.' if raw else ' from the published independent process medians.'))


if __name__ == '__main__':
    try:
        main()
    except (OSError, ValueError, KeyError) as error:
        print('Verification failed:', error, file=sys.stderr)
        raise SystemExit(2)
