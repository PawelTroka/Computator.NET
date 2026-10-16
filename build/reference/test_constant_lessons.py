"""Specific definitions and conventions must accompany stored constant values."""
import unittest
from constant_lessons import CONSTANT_LESSONS
from enrichment import enrich
import mathematical
import physical


class ConstantLessonTests(unittest.TestCase):
    def extra(self, name, value='1', kind='MathematicalConstants'):
        item = {'signature': name, 'type': kind, 'menu': 'real ' + name}
        values = {name: value}
        module = mathematical if kind == 'MathematicalConstants' else physical
        return enrich(item, module.describe(item, values), values)

    def test_every_named_mathematical_value_has_a_specific_lesson(self):
        for name in mathematical.DATA:
            with self.subTest(name=name):
                value = 'NaN' if name == 'NaN' else 'Infinity' if name == 'Infinity' else '<0; 1>' if name == 'i' else '1'
                extra = self.extra(name, value)
                self.assertIn(name, CONSTANT_LESSONS)
                self.assertEqual(len(extra.equations), 2)
                self.assertEqual(extra.equations[0].label, 'Definition / interpretation')
                self.assertEqual(extra.equations[-1].label, 'Stored numeric value')
                self.assertNotEqual(extra.equations[0].tex, extra.equations[-1].tex)
                self.assertTrue(extra.paragraphs)
                self.assertNotIn('does not run the defining infinite series', ' '.join(extra.paragraphs))

    def test_no_curated_lesson_is_for_an_unexposed_identifier(self):
        for name in CONSTANT_LESSONS:
            with self.subTest(name=name):
                kind = 'MathematicalConstants' if name in mathematical.DATA else 'PhysicalConstants'
                self.extra(name, kind=kind)
        self.assertNotIn('GravitationalConstant', CONSTANT_LESSONS)

    def test_mathematical_normalizations_are_not_interchanged(self):
        gibbs = self.extra('WilbrahamGibbsConstantG')
        self.assertIn(r'\frac{W-1}{2}', gibbs.equations[0].tex)
        self.assertNotEqual(self.extra('SilverConstant').equations[0].tex, self.extra('SilverRatio').equations[0].tex)
        self.assertIn('supplementary', ' '.join(self.extra('ComplementaryTetrahedralAngle').paragraphs).lower())
        self.assertIn('even', ' '.join(self.extra('MRBconstant').paragraphs).lower())
        self.assertNotEqual(self.extra('FirstLemniscateConstantLA').equations[0].tex,
                            self.extra('SecondLemniscateConstantLB').equations[0].tex)

    def test_physical_definitions_preserve_historical_values(self):
        charge = self.extra('ElementaryCharge', '1.602176565E-19', 'PhysicalConstants')
        self.assertIn('1.602176634', charge.equations[0].tex)
        self.assertIn('1.602176565E-19', charge.equations[-1].text)
        self.assertTrue(any(example.code == '-ElementaryCharge' and
                            float(example.result.removeprefix('approximately ')) == -1.602176565E-19
                            for example in charge.examples))
        self.assertIn('plain number', ' '.join(charge.paragraphs))

    def test_cousin_brun_sum_omits_the_exceptional_pair_in_its_stored_convention(self):
        name = 'BrunConstantforPrimeCousinsB4'
        item = {'signature': name, 'type': 'MathematicalConstants', 'menu': name}
        base = mathematical.describe(item, {name: '1.1970449'})
        extra = self.extra(name, '1.1970449')
        self.assertIn('p>3', base.formula)
        self.assertIn('p>3', extra.equations[0].tex)
        self.assertIn('omitting the exceptional pair (3,7)', base.definition)
        self.assertIn('pair (3,7) is omitted', ' '.join(extra.paragraphs))
        self.assertIn('10/21', base.note)
        self.assertIn('10/21', ' '.join(extra.paragraphs))
        self.assertIn('1.1970449', extra.equations[-1].text)
        self.assertEqual(base.url, 'https://mwolf.pracownicy.uksw.edu.pl/twins_cousins.pdf')

    def test_examples_have_consistent_numeric_answers_and_interpretations(self):
        answers = {}
        for name, (_, paragraphs, examples) in CONSTANT_LESSONS.items():
            self.assertTrue(paragraphs, name)
            for code, answer, interpretation in examples:
                with self.subTest(name=name, code=code):
                    float(answer.removeprefix('approximately '))
                    self.assertTrue(code.strip())
                    self.assertTrue(interpretation.strip())
                    if code in answers:
                        self.assertEqual(answers[code], answer)
                    answers[code] = answer


if __name__ == '__main__':
    unittest.main()
