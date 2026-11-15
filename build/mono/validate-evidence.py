#!/usr/bin/env python3
"""Reject missing, skipped or incomplete packaged desktop acceptance evidence."""
import sys
import xml.etree.ElementTree as ET

report = ET.parse(sys.argv[1]).getroot()
cases = report.findall('testcase')
if (report.tag != 'testsuite' or len(cases) < 58 or int(report.get('tests', '0')) != len(cases)
        or report.get('failures') != '0' or report.get('skipped') != '0'
        or any(case.find('failure') is not None or case.find('skipped') is not None for case in cases)
        or len({case.get('name') for case in cases}) != len(cases)
        or not {'mono-numeric-input-font-layout', 'mono-math-font-rendering',
                'mono-math-font-settings-roundtrip', 'mono-editor-math-fonts-and-document-switches'}
        .issubset({case.get('name') for case in cases})):
    raise SystemExit('Packaged Mono acceptance must execute at least 58 unique cases, including native font rendering, reload and editor documents, with zero failures/skips.')
print(f'Packaged Mono acceptance: {len(cases)} passed; zero skipped.')
