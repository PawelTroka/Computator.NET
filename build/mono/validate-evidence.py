#!/usr/bin/env python3
"""Reject missing, skipped or incomplete packaged desktop acceptance evidence."""
import sys
import xml.etree.ElementTree as ET

report = ET.parse(sys.argv[1]).getroot()
cases = report.findall('testcase')
if (report.tag != 'testsuite' or len(cases) < 56 or int(report.get('tests', '0')) != len(cases)
        or report.get('failures') != '0' or report.get('skipped') != '0'
        or any(case.find('failure') is not None or case.find('skipped') is not None for case in cases)
        or len({case.get('name') for case in cases}) != len(cases)
        or not {'mono-numeric-input-font-layout', 'mono-symbolic-calculations-and-native-input'} <= {case.get('name') for case in cases}):
    raise SystemExit('Packaged Mono acceptance must execute at least 56 unique cases, including native numeric font layout and symbolic calculations, with zero failures/skips.')
print(f'Packaged Mono acceptance: {len(cases)} passed; zero skipped.')
