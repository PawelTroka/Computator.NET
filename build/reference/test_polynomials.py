"""Keep polynomial conventions and examples separate from reflected signatures."""
import unittest
import xml.etree.ElementTree as ET
import polynomials
from enrichment import WorkedExample
from generate import articles, validate_fragment, xml_bytes


class PolynomialReferenceTests(unittest.TestCase):
    def description(self, signature):
        return articles({'items': [{'signature': signature, 'menu': signature, 'type': 'Polynomial'}],
                         'constants': []})[0]['description']

    def test_all_authored_entries_have_specific_lessons_and_examples(self):
        self.assertEqual(len(polynomials.ARTICLES), 14)
        self.assertEqual(set(polynomials.ARTICLES), set(polynomials.LESSONS))
        inventory = {'items': [{'signature': signature, 'menu': signature, 'type': 'Polynomial'}
                              for signature in polynomials.ARTICLES], 'constants': []}
        entries = articles(inventory)
        self.assertEqual(xml_bytes(entries), xml_bytes(articles(inventory)))
        for entry in entries:
            with self.subTest(signature=entry['signature']):
                self.assertEqual(entry['type'], 'Polynomial')
                self.assertEqual(entry['category'], 'Polynomial operations')
                self.assertEqual(entry['url'], polynomials.SOURCE)
                self.assertIn('reference-explanation', entry['description'])
                self.assertIn('reference-worked-example', entry['description'])

    def test_constructor_explains_storage_order_capacity_and_general_roots(self):
        text = self.description('new Polynomial(coefficients)')
        for clause in ('ascending exponent order', 'new Polynomial(3)', 'new Polynomial(3.0)',
                       'shares the supplied array', 'Clone()', 'Differentiate()', 'Integrate()',
                       'companion-matrix eigenvalues', 'unspecified order', 'poorly conditioned'):
            with self.subTest(clause=clause): self.assertIn(clause, text)

    def test_zero_roots_convention_is_not_a_mathematical_claim(self):
        text = self.description('Polynomial.Zero')
        self.assertIn('empty complex array', text)
        self.assertIn('mathematically every input is a root', text)
        self.assertIn('Degree = −1', text)

    def test_all_evaluation_overloads_share_one_article(self):
        signature = 'Polynomial.Evaluate(z,coefficients1,coefficients2, ...)'
        menus = ['real Polynomial.Evaluate(real z, real[] coefficients)',
                 'complex Polynomial.Evaluate(complex z, real[] coefficients)',
                 'complex Polynomial.Evaluate(complex z, complex[] coefficients)']
        entries = articles({'items': [{'signature': signature, 'menu': menu, 'type': 'Polynomial'}
                                     for menu in menus], 'constants': []})
        self.assertEqual(len(entries), 1)
        root = ET.fromstring(entries[0]['description'])
        self.assertEqual(len(root.find("./p[@class='reference-signature']").findall('code')), 3)
        self.assertIn('complex coefficients', entries[0]['description'])
        self.assertIn('ArgumentNullException', entries[0]['description'])

    def test_long_division_explains_tuple_order_aliasing_and_zero_divisor(self):
        text = self.description('Polynomial.DivideRemainder(a,b)')
        for clause in ('quotient as Item1', 'remainder as Item2', 'DivideByZeroException',
                       'remainder is a itself', 'Clone()', 'x²+1=(x−1)(x+1)+2'):
            with self.subTest(clause=clause): self.assertIn(clause, text)

    def test_pointwise_operations_are_not_conflated_with_polynomial_arithmetic(self):
        multiply = self.description('Polynomial.PointwiseMultiply(a,b)')
        divide = self.description('Polynomial.PointwiseDivide(a,b)')
        self.assertIn('smaller input degree', multiply)
        self.assertIn('differs from a(x)*b(x)', multiply)
        self.assertIn('Missing divisor coefficients are treated as zero', divide)
        self.assertIn('infinity or NaN', divide)
        self.assertIn('does not represent a(x)/b(x)', divide)

    def test_only_the_three_scripts_require_dedicated_execution_assertions(self):
        scripts = {'new Polynomial(coefficients)', 'Polynomial.Fit(x,y,order,method)',
                   'Polynomial.DivideRemainder(a,b)'}
        scalars = 0
        for signature in polynomials.ARTICLES:
            with self.subTest(signature=signature):
                root = ET.fromstring(self.description(signature))
                examples = root.findall(".//div[@class='reference-worked-example']")
                self.assertEqual(len(examples), 1)
                example = examples[0]
                if signature in scripts:
                    self.assertEqual(example.get('data-example-kind'), 'script')
                else:
                    self.assertNotIn('data-example-kind', example.attrib)
                    self.assertNotIn(';', example.find('./pre/code').text)
                    scalars += 1
        self.assertEqual(scalars, 11)

    def test_unknown_example_kinds_fail_at_authoring_and_fragment_validation(self):
        for kind in ('', 'Script', 'text', 'skip'):
            with self.subTest(kind=kind):
                with self.assertRaisesRegex(ValueError, 'Unsupported worked example kind'):
                    WorkedExample('1', '1', 'One.', kind=kind)
                root = ET.fromstring(self.description('Polynomial.Zero'))
                root.find(".//div[@class='reference-worked-example']").set('data-example-kind', kind)
                with self.assertRaisesRegex(ValueError, 'Unsupported worked example kind'):
                    validate_fragment(ET.tostring(root, encoding='unicode'))

    def test_example_kind_metadata_is_restricted_to_worked_example_containers(self):
        for selector in ('.', './p', './/pre', ".//div[@class='reference-worked-example']/p"):
            with self.subTest(selector=selector):
                root = ET.fromstring(self.description('Polynomial.Zero'))
                root.find(selector).set('data-example-kind', 'script')
                with self.assertRaisesRegex(ValueError, 'Unsupported attribute'):
                    validate_fragment(ET.tostring(root, encoding='unicode'))

    def test_quotient_and_remainder_output_has_an_explicit_separator(self):
        root = ET.fromstring(self.description('Polynomial.DivideRemainder(a,b)'))
        example = root.find(".//div[@class='reference-worked-example']")
        self.assertIn('write(parts.Item1.Evaluate(3));\nwrite(";");\nwrite(parts.Item2.Evaluate(3));',
                      example.find('./pre/code').text)
        self.assertIn('printed as 2;2', ''.join(example.itertext()))

    def test_unknown_method_fails_instead_of_inventing_generic_help(self):
        with self.assertRaises(KeyError): self.description('Polynomial.Unknown(x)')


if __name__ == '__main__':
    unittest.main()
