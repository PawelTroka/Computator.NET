"""A worked example must run in the editor named by its generated article."""
from dataclasses import replace
from pathlib import Path
import unittest
from unittest.mock import patch
import xml.etree.ElementTree as ET

import generate
import scientific
from enrichment import Enrichment, WorkedExample, enrich


class WorkedExampleModeTests(unittest.TestCase):
    def article(self, signature, kind):
        item = {'signature': signature, 'type': kind, 'menu': signature}
        with patch.object(generate, 'COLLECT_EQUATIONS', True):
            return ET.fromstring(generate.articles({'items': [item], 'constants': []})[0]['description'])

    def test_matrix_article_contains_the_editor_and_complete_runnable_statement(self):
        root = self.article('det<T>(M)', 'MatrixFunctions')
        example = root.find(".//div[@class='reference-worked-example']")
        self.assertEqual(example.findtext("./p[@class='reference-example-context']"), 'Scripting editor')
        self.assertEqual(example.findtext('./pre/code'), 'writeln(det(matrix({{3,1},{1,2}})));')
        self.assertIn('Result: 5', ''.join(example.itertext()))
        generate.validate_fragment(ET.tostring(root, encoding='unicode'))

    def test_expression_examples_remain_copyable_without_script_output_calls(self):
        example = self.article('Normal.PDF(mean,stddev,x)', 'Normal').find(".//div[@class='reference-worked-example']")
        self.assertIsNone(example.find("./p[@class='reference-example-context']"))
        self.assertEqual(example.findtext('./pre/code'), 'Normal.PDF(0,1,0)')

    def test_matrix_distribution_predicates_use_scripting_but_scalar_predicates_do_not(self):
        for signature, kind in [
            ('Wishart.IsValidParameterSet(degreesOfFreedom,scale)', 'Wishart'),
            ('InverseWishart.IsValidParameterSet(degreesOfFreedom,scale)', 'InverseWishart'),
            ('MatrixNormal.IsValidParameterSet(mean,rowCovariance,columnCovariance)', 'MatrixNormal'),
        ]:
            with self.subTest(kind=kind):
                example = self.article(signature, kind).find(".//div[@class='reference-worked-example']")
                self.assertEqual(example.findtext("./p[@class='reference-example-context']"), 'Scripting editor')
                self.assertTrue(example.findtext('./pre/code').startswith('writeln((' + kind + '.IsValidParameterSet('))
        scalar = self.article('Normal.IsValidParameterSet(mean,stddev)', 'Normal')
        self.assertIsNone(scalar.find(".//p[@class='reference-example-context']"))

    def test_all_matrix_overloads_preserve_the_required_editor_through_enrichment(self):
        catalogue = Path(__file__).resolve().parents[2] / 'Computator.NET.Core/Static/functions.xml'
        checked = 0
        for entry in ET.parse(catalogue).getroot():
            if entry.findtext('Type') != 'MatrixFunctions':
                continue
            signature = entry.attrib['Signature']
            item = {'signature': signature, 'type': 'MatrixFunctions', 'menu': signature}
            lesson = enrich(item, scientific.describe(item, {}), {})
            with self.subTest(signature=signature):
                self.assertTrue(lesson.examples)
                self.assertTrue(all(example.editor == 'scripting' for example in lesson.examples))
            checked += 1
        self.assertEqual(checked, 42)

    def test_generation_rejects_unknown_editor_values(self):
        example = WorkedExample('Normal.PDF(0,1,0)', '0.398942280401', 'The standard-normal density at its center.')
        for editor in ('console', 'Scripting', '', None):
            with self.subTest(editor=editor), patch.object(generate, 'enrich', return_value=Enrichment(examples=(replace(example, editor=editor),))):
                with self.assertRaisesRegex(ValueError, 'Unsupported worked-example editor'):
                    self.article('Normal.PDF(mean,stddev,x)', 'Normal')

    def test_script_generic_type_is_escaped_as_inert_content(self):
        root = self.article('matrix<T>(m,n)', 'MatrixFunctions')
        example = root.find(".//div[@class='reference-worked-example']")
        self.assertEqual(example.findtext('./pre/code'), 'writeln(matrix<double>(2,3).ColumnCount);')
        self.assertEqual(list(example.find('./pre/code')), [])
        generate.validate_fragment(ET.tostring(root, encoding='unicode'))


if __name__ == '__main__':
    unittest.main()
