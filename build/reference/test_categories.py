"""Subject-navigation regressions, independent of the production assignments."""
import copy
from dataclasses import replace
import json
from pathlib import Path
import unittest
from unittest.mock import patch
import xml.etree.ElementTree as ET

from common import A
import elementary
import special
from categories import subject_category
from generate import articles


class CategoryTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        root = Path(__file__).resolve().parents[2]
        cls.reference = {entry.get('Signature'): entry for entry in
                         ET.parse(root / 'Computator.NET.Core/Static/functions.xml').getroot()}

    def test_all_historical_function_memberships_are_restored(self):
        fixture = Path(__file__).with_name('fixtures') / 'legacy-subject-categories.json'
        expected = json.loads(fixture.read_text(encoding='utf-8'))['categories']
        self.assertEqual(len(expected), 318)
        self.assertEqual(len(set(expected.values())), 26)
        for signature, category in expected.items():
            with self.subTest(signature=signature):
                self.assertIn(signature, self.reference)
                self.assertEqual(self.reference[signature].findtext('Category'), category)

    def test_previously_unclassified_signatures_have_deliberate_groups(self):
        expected = {
            'DoubleFactorial(x)': 'Integer functions',
            'Eulerφ(n)': 'Integer functions',
            'isPrime(n)': 'Integer functions',
            'Permutations(n)': 'Combinatorics',
            'abs(x)': 'Absolute value',
            'abs(z)': 'Absolute value',
            'Gabor(x,mean,amplitude,position,width,phase,frequency)': 'Signal processing',
            'Gabor(z,λ,θ,ψ,σ,γ)': 'Signal processing',
            'findRoot(f,a,b)': 'Root finding',
            'OwenT(h,a)': 'Functions related to probability distributions',
            'OwenT(h,a,ah)': 'Functions related to probability distributions',
            'PolyLog(n,x)': 'Zeta and L-functions',
            'diLog(x)': 'Zeta and L-functions',
            'diLog(z)': 'Zeta and L-functions',
            'polyGamma(x)': 'Gamma and related functions',
            'ψn(x)': 'Gamma and related functions',
            'ψⁿ(x)': 'Gamma and related functions',
        }
        for signature, category in expected.items():
            with self.subTest(signature=signature):
                self.assertEqual(self.reference[signature].findtext('Category'), category)

    def test_aliases_and_overloads_have_the_same_subject_as_their_canonical_function(self):
        for kind, module in [('ElementaryFunctions', elementary), ('SpecialFunctions', special)]:
            by_name = {}
            for signature, entry in self.reference.items():
                if entry.findtext('Type') == kind:
                    by_name.setdefault(signature.split('(')[0], set()).add(entry.findtext('Category'))
            for name, categories in by_name.items():
                with self.subTest(kind=kind, name=name):
                    self.assertEqual(len(categories), 1, 'Overloads split between subject menus')
                    canonical = module.ALIASES.get(name, name)
                    self.assertEqual(categories, by_name[canonical])

    def test_every_reference_has_a_specific_subject_instead_of_a_runtime_type(self):
        for signature, entry in self.reference.items():
            with self.subTest(signature=signature):
                category = entry.findtext('Category')
                self.assertTrue(category and category.strip())
                self.assertEqual(category, category.strip())
                self.assertNotEqual(category.replace(' ', '').casefold(), entry.findtext('Type').casefold())

    def test_new_canonical_function_requires_a_reviewed_subject_even_with_valid_prose(self):
        item = {'signature': 'newFunction(x)', 'type': 'ElementaryFunctions', 'menu': 'real newFunction(real x)'}
        with patch.object(elementary, '_describe', return_value=A('New function', 'Valid reviewed explanation.')):
            with self.assertRaisesRegex(KeyError, 'No reviewed subject category'):
                elementary.describe(item, {})
        # Prefix-based mathematical families must not silently classify new APIs.
        with self.assertRaisesRegex(KeyError, 'No reviewed subject category'):
            subject_category('SpecialFunctions', 'BesselJ99')
        with self.assertRaisesRegex(KeyError, 'No reviewed subject category'):
            subject_category('PhysicalConstants', 'NewPhysicalConstant')

    def test_missing_and_generic_authored_categories_fail_generation(self):
        item = {'signature': 'sinc(x)', 'type': 'ElementaryFunctions', 'menu': 'real sinc(real x)'}
        inventory = {'items': [item], 'constants': []}
        original = elementary.describe(item, {})
        for category in ('', ' ', ' Trigonometric functions', 'ElementaryFunctions', 'Elementary functions'):
            with self.subTest(category=category):
                with patch.object(elementary, 'describe', return_value=replace(original, category=category)):
                    with self.assertRaisesRegex(ValueError, 'Missing or generic subject category'):
                        articles(inventory)

    def test_conflicting_subjects_for_one_overload_key_fail_generation(self):
        item = {'signature': 'pow(x,y)', 'type': 'ElementaryFunctions', 'menu': 'real pow(real x, real y)'}
        other = copy.deepcopy(item)
        other['menu'] = 'complex pow(complex x, complex y)'
        first = elementary.describe(item, {})
        second = replace(first, category='Root functions')
        with patch.object(elementary, 'describe', side_effect=[first, second]):
            with self.assertRaisesRegex(ValueError, 'Conflicting subject categories'):
                articles({'items': [item, other], 'constants': []})


if __name__ == '__main__':
    unittest.main()
