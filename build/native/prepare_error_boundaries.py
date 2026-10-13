"""One-time, count-checked source refactor; not run by ordinary builds."""
from pathlib import Path
import re
root = Path(__file__).resolve().parents[2]
pattern = re.compile(r'(?P<attr>\[System\.Runtime\.InteropServices\.DllImport\(Computator\.NET\.DataTypes\.Configuration\.GslConfig\.GslDllName,\s*CallingConvention = System\.Runtime\.InteropServices\.CallingConvention\.Cdecl\)\])\s*(?://[^\n]*\n\s*)?private static extern (?P<type>\w+) (?P<name>gsl_\w+)\((?P<args>[^;]*?)\);')
for name, expected in [('SpecialFunctions.cs', 210), ('ElementaryFunctions.cs', 6)]:
    path = root / 'Computator.NET.Core/Functions' / name
    source = path.read_text(encoding='utf-8-sig')
    if '_Native(' in source: raise SystemExit('Refusing to apply the migration twice')
    embedded = source.index('public const string ToCode')
    def replace(match):
        result, function, arguments = match['type'], match['name'], match['args']
        forwarded = []
        for argument in arguments.split(','):
            words = argument.split()
            if len(words) not in (2,3) or (len(words)==3 and words[0] not in ('out','ref','in')):
                raise ValueError('Unexpected native declaration: ' + argument)
            forwarded.append((' '.join([words[0],words[-1]]) if len(words)==3 else words[-1]))
        quote = '""' if match.start() > embedded else '"'
        attribute = match['attr'].replace('CallingConvention =', 'EntryPoint = ' + quote + function + quote + ', CallingConvention =')
        return (attribute + '\n        private static extern ' + result + ' ' + function + '_Native(' + arguments + ');\n' +
            '        private static ' + result + ' ' + function + '(' + arguments + ')\n        {\n' +
            '            using (var boundary = Computator.NET.DataTypes.Configuration.GslErrorBoundary.Enter())\n            {\n' +
            '                var value = ' + function + '_Native(' + ', '.join(forwarded) + ');\n' +
            '                boundary.ThrowIfError();\n                return value;\n            }\n        }')
    modified, count = pattern.subn(replace, source)
    if count != expected: raise SystemExit(f'{name}: expected {expected} declarations, found {count}; source review required')
    path.write_text(modified, encoding='utf-8')
    print(name, count, 'native/generated-code boundaries')
