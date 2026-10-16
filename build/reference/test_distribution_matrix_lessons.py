"""Editorial regressions for operation meaning and public-call examples.

The C# reference suite executes every published scalar example through TSL.
These authoring tests cover selection and explanatory mistakes that execution
alone cannot detect, including confusing a density with its logarithm.
"""
import math
from pathlib import Path
import re
import unittest
import xml.etree.ElementTree as ET

import distributions
import scientific
from distribution_examples import examples_for
from enrichment import enrich


class DistributionMatrixLessonsTests(unittest.TestCase):
    def lesson(self, signature, kind):
        item = {'signature': signature, 'menu': signature, 'type': kind}
        module = scientific if kind == 'MatrixFunctions' else distributions
        return enrich(item, module.describe(item, {}), {})

    def catalogue(self):
        path = Path(__file__).resolve().parents[2] / 'Computator.NET.Core/Static/functions.xml'
        for entry in ET.parse(path).getroot():
            yield entry.attrib['Signature'], entry.findtext('Type')

    def test_log_density_explains_exponentiation_and_does_not_use_density_shortcut(self):
        lesson = self.lesson('ContinuousUniform.PDFLn(lower,upper,x)', 'ContinuousUniform')
        text = ' '.join(lesson.paragraphs)
        self.assertIn('exp(ℓ(x))', text)
        self.assertIn('log-density can be positive', text)
        self.assertNotIn('probability is approximately f(x)·Δx', text)
        self.assertTrue(any(equation.label == 'Log-density result' for equation in lesson.equations))
        self.assertIn('.PDFLn(', lesson.examples[0].code)
        self.assertAlmostEqual(float(lesson.examples[0].result), math.log(2), places=11)

    def test_log_mass_separates_independent_events_from_alternative_outcomes(self):
        lesson = self.lesson('Bernoulli.PMFLn(p,k)', 'Bernoulli')
        text = ' '.join(lesson.paragraphs)
        self.assertIn('log-sum-exp', text)
        self.assertIn('probabilities multiply', text)
        self.assertIn('probabilities add', text)
        self.assertNotIn('this number already refers to an individual outcome', text)
        self.assertAlmostEqual(float(lesson.examples[0].result), math.log(.25), places=11)

    def test_discrete_cdf_retains_probability_at_inclusive_lower_bound(self):
        discrete = self.lesson('Binomial.CDF(p,n,x)', 'Binomial')
        continuous = self.lesson('Normal.CDF(mean,stddev,x)', 'Normal')
        self.assertIn('F(a−1)', ' '.join(discrete.paragraphs))
        self.assertNotIn('F(a−1)', ' '.join(continuous.paragraphs))
        self.assertEqual(discrete.examples[0].code, 'Binomial.CDF(0.5,4,2)')
        self.assertEqual(float(discrete.examples[0].result), 11/16)

    def test_all_catalogued_deterministic_distribution_operations_explain_their_own_call(self):
        checked = 0
        for signature, kind in self.catalogue():
            if kind not in distributions.FAMILIES:
                continue
            operation = signature.split('.', 1)[1].split('(', 1)[0]
            if operation.startswith('Sample'):
                continue
            with self.subTest(signature=signature):
                lesson = self.lesson(signature, kind)
                self.assertTrue(lesson.examples)
                self.assertTrue(any('.'+operation+'(' in example.code for example in lesson.examples))
            checked += 1
        self.assertGreater(checked, 100)

    def test_colliding_function_names_use_the_unambiguous_distribution_class(self):
        for family, operations in [('Beta', ('PDF', 'PDFLn', 'CDF', 'InvCDF', 'IsValidParameterSet')),
                                   ('Chi', ('PDF', 'PDFLn', 'CDF', 'IsValidParameterSet'))]:
            for operation in operations:
                with self.subTest(family=family, operation=operation):
                    example = examples_for(family, operation)[0]
                    self.assertIn('MathNet.Numerics.Distributions.'+family+'.'+operation+'(', example.code)

    def test_factory_examples_keep_rate_scale_variance_and_precision_distinct(self):
        self.assertEqual(float(examples_for('Gamma', 'WithShapeRate')[0].result), .5)
        self.assertEqual(float(examples_for('Gamma', 'WithShapeScale')[0].result), 8)
        self.assertEqual(float(examples_for('Normal', 'WithMeanVariance')[0].result), 3)
        self.assertEqual(float(examples_for('Normal', 'WithMeanPrecision')[0].result), .5)
        self.assertAlmostEqual(float(examples_for('LogNormal', 'WithMeanVariance')[0].result), math.sqrt(math.log(2)), places=11)

    def test_lu_prose_and_equation_agree_on_the_pivot_convention(self):
        item = {'signature': 'LU<T>(M)', 'menu': 'LU<T>(M)', 'type': 'MatrixFunctions'}
        article = scientific.describe(item, {})
        self.assertIn('P·A = L·U', article.formula)
        self.assertIn('P⁻¹', article.formula)
        self.assertNotIn('A = P·L·U', article.formula)
        lesson = enrich(item, article, {})
        self.assertEqual(lesson.equations[0].tex, r'PA=LU')
        self.assertEqual(float(lesson.examples[0].result), -2)

    def test_all_matrix_signatures_have_relevant_scalar_examples(self):
        checked = 0
        for signature, kind in self.catalogue():
            if kind != 'MatrixFunctions':
                continue
            name = re.sub(r'<.*>', '', signature.split('(', 1)[0])
            with self.subTest(signature=signature):
                lesson = self.lesson(signature, kind)
                self.assertTrue(lesson.examples)
                self.assertTrue(any(name+'(' in example.code or name+'<double>(' in example.code for example in lesson.examples))
                self.assertTrue(all(math.isfinite(float(example.result.replace('approximately ', ''))) for example in lesson.examples))
            checked += 1
        self.assertEqual(checked, 42)

    def test_matrix_constructors_demonstrate_size_and_element_overloads(self):
        allocated = self.lesson('vector<T>(n)', 'MatrixFunctions').examples[0]
        elements = self.lesson('vector(elements1,elements2, ...)', 'MatrixFunctions').examples[0]
        self.assertIn('vector<double>(3)', allocated.code)
        self.assertIn('single value three', allocated.explanation)
        self.assertIn('vector(3.0,4.0)', elements.code)
        self.assertEqual(float(elements.result), 5)

    def test_matrix_examples_do_not_silently_repair_documented_wrapper_defects(self):
        multiply = self.lesson('PointwiseMultiply<T>(matrices1,matrices2, ...)', 'MatrixFunctions').examples[0]
        divide = self.lesson('PointwiseDivide<T>(matrices1,matrices2, ...)', 'MatrixFunctions').examples[0]
        self.assertEqual(float(multiply.result), 12)
        self.assertIn('intended', multiply.explanation)
        self.assertEqual(float(divide.result), .25)
        self.assertIn('repeats', divide.explanation)

    def test_reused_probability_examples_have_one_consistent_published_answer(self):
        answers = {}
        for signature, kind in self.catalogue():
            if kind not in distributions.FAMILIES and kind != 'MatrixFunctions':
                continue
            for example in self.lesson(signature, kind).examples:
                if example.code in answers:
                    self.assertEqual(example.result, answers[example.code], example.code)
                answers[example.code] = example.result
        self.assertGreater(len(answers), 150)


if __name__ == '__main__':
    unittest.main()
