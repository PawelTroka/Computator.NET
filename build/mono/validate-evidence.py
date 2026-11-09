#!/usr/bin/env python3
"""Reject missing, skipped or incomplete packaged desktop acceptance evidence."""
import sys
import xml.etree.ElementTree as ET

report = ET.parse(sys.argv[1]).getroot()
cases = report.findall('testcase')
names = {case.get('name') for case in cases}
required = {
    f'mono-{editor}-operator-{source}-{operator}'
    for editor in ('expression', 'scripting')
    for source in ('main', 'keypad-numlock-off', 'keypad-numlock-on')
    for operator in ('plus', 'minus', 'multiply', 'divide')
} | {
    f'mono-expression-native-two-plus-two-{source}'
    for source in ('main', 'keypad-numlock-off', 'keypad-numlock-on')
} | {
    f'mono-{editor}-{behavior}'
    for editor in ('expression', 'scripting')
    for behavior in ('keypad-honors-suppress-keypress', 'keypad-preserves-read-only-buffer',
                     'repeated-keypad-operators-insert-once',
                     'open-menu-keeps-keypad-out-of-background-editor')
} | {'mono-numeric-input-font-layout', 'mono-operator-input-real-x11-desktop',
     'mono-expression-native-superscript-operators',
     'mono-expression-native-selection-undo-keeps-unicode',
     'mono-scripting-native-insertion-undo-keeps-unicode',
     'mono-scripting-control-keypad-divide-keeps-shortcut-routing'}
if (report.tag != 'testsuite' or len(cases) < 55 or int(report.get('tests', '0')) != len(cases)
        or report.get('failures') != '0' or report.get('skipped') != '0'
        or any(case.find('failure') is not None or case.find('skipped') is not None for case in cases)
        or len(names) != len(cases) or not required.issubset(names)):
    raise SystemExit('Packaged Mono acceptance must include native numeric layout, all operator-input cases and Unicode editing, with zero failures/skips.')
print(f'Packaged Mono acceptance: {len(cases)} passed; zero skipped.')
