"""Keep historical binaries and prepare a same-binary Roslyn-only control."""
import argparse
import copy
import json
from pathlib import Path


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--input', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--experiment', choices=['historical', 'modern'], required=True)
    args = parser.parse_args()
    manifest = json.loads(args.input.read_text(encoding='utf-8-sig'))
    if args.experiment == 'modern':
        # Exactly the same product/driver/runtime bytes on this machine. Only the
        # explicit, recorded opt-out switch differs. No second source rebuild.
        manifest['baseline'] = copy.deepcopy(manifest['candidate'])
        manifest['baseline']['command'].append('--roslyn-only')
        profile = args.output.parent.resolve() / 'roslyn-control-profile'
        profile.mkdir(exist_ok=False)
        manifest['baseline']['environment']['COMPUTATOR_DATA_DIRECTORY'] = str(profile)
        manifest['baseline']['provenance']['version'] += ' (direct numeric compiler disabled)'
        manifest['baseline']['provenance']['directCompilerDisabled'] = True
        # A child desktop does not inherit the observer's AppContext switch. Do
        # not present two identical startup treatments as a compiler comparison.
        manifest['scenarios'].pop('startup')
    manifest['candidate']['provenance']['directCompilerDisabled'] = False
    manifest['experiment'] = args.experiment + '-direct-numeric-r2r'
    for scenario in ('auto-real', 'auto-complex', 'auto-surface'):
        metrics = ['engine.initialize.ms', 'automatic.mode.ms', 'automatic.first.interaction.ms']
        manifest['scenarios'][scenario] = {'metrics': metrics, 'operations': {metric: 1 for metric in metrics}}
    args.output.write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8')


if __name__ == '__main__':
    main()
