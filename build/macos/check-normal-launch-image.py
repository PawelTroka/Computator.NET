"""Synthetic image-analysis contracts only; never reported as native GUI proof."""
import argparse
import json
from pathlib import Path
import struct
import subprocess
import zlib


def png(path, scale, paint):
    width, height = 1200 * scale, 1000 * scale
    rows = bytearray()
    for y in range(height):
        row = bytearray(b'\xff\xff\xff' * width)
        logical_y = y // scale
        for x in range(40 * scale, 920 * scale):
            logical_x = x // scale
            colored = 100 <= logical_y < 700 if paint == 'client' else paint == 'title' and 60 <= logical_y < 80
            if colored and 60 <= logical_x < 900:
                n = (logical_x // 19 + logical_y // 17) % 32
                value = (0, 0, 0) if logical_y % 15 < 3 else (n * 7, (31 - n) * 7, (n * 13) % 256)
                row[x * 3:x * 3 + 3] = bytes(value)
        rows.append(0)
        rows.extend(row)
    def chunk(name, data):
        return struct.pack('!I', len(data)) + name + data + struct.pack('!I', zlib.crc32(name + data))
    path.write_bytes(b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('!2I5B', width, height, 8, 2, 0, 0, 0))
                     + chunk(b'IDAT', zlib.compress(rows)) + chunk(b'IEND', b''))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('mono', 'config', 'probe', 'evidence'):
        parser.add_argument('--' + name, required=True, type=Path)
    args = parser.parse_args()
    output = args.evidence.resolve()
    output.mkdir(parents=True, exist_ok=False)
    geometry = [0.0] * 26
    for key, value in {0: 38, 1: 200, 4: 880, 5: 710, 7: 0, 8: 22, 9: 40, 10: 60,
                       11: 880, 12: 710, 15: 1200, 16: 1000, 18: 1200, 19: 1000, 20: 1,
                       22: 1200, 23: 1000, 24: 2, 25: 601}.items():
        geometry[key] = value
    cases = [('colored-client', True, 1, 'client', {}, (0, 0, 880, 688)),
             ('retina-client', True, 2, 'client', {}, (0, 0, 880, 688)),
             ('blank-client', False, 1, 'blank', {}, (0, 0, 880, 688)),
             ('titlebar-only', False, 1, 'title', {}, (0, 0, 880, 688)),
             ('outside-display', False, 1, 'client', {9: 800}, (0, 0, 880, 688)),
             ('frame-scale-mismatch', False, 1, 'client', {12: 800}, (0, 0, 880, 688)),
             ('outside-native-frame', False, 1, 'client', {}, (0, 0, 900, 688)),
             ('unmapped-client', False, 1, 'client', {24: 0}, (0, 0, 880, 688))]
    results = []
    for name, expected, scale, paint, changes, rectangle in cases:
        prefix = output / name
        values = geometry.copy()
        for key, value in changes.items():
            values[key] = value
        data = Path(str(prefix) + '-geometry.json')
        data.write_text(json.dumps(values), encoding='utf-8')
        image = Path(str(prefix) + '-synthetic.png')
        if paint == 'blank':
            # No stripe is placed in this image, including the titlebar.
            png(image, scale, 'blank')
        else:
            png(image, scale, paint)
        command = [str(args.mono), '--config', str(args.config), str(args.probe), str(data),
                   *map(str, rectangle), str(image), str(prefix)]
        observed = subprocess.run(command, capture_output=True, text=True, timeout=20)
        report = json.loads(Path(str(prefix) + '-paint.json').read_text(encoding='utf-8'))
        if observed.returncode != (0 if expected else 1) or report['passed'] != expected:
            raise AssertionError(f'{name}: unexpected analyzer result {observed.returncode}: {report}')
        if expected and report['pixelCrop'] != [40 * scale, 82 * scale, 880 * scale, 688 * scale]:
            raise AssertionError(f'{name}: measured client crop differs: {report["pixelCrop"]}')
        results.append({'name': name, 'passed': True, 'expectedNonblank': expected, 'exitCode': observed.returncode})
    (output / 'image-contracts.json').write_text(json.dumps({'scope': 'synthetic-contracts-not-native-GUI',
        'passed': True, 'cases': results}, indent=2) + '\n', encoding='utf-8')
    print('Normal-launch image analyzer:', len(results), 'synthetic contracts passed; no native GUI claim.')


if __name__ == '__main__':
    main()
