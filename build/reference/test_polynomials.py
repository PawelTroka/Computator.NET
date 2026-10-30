"""Keep polynomial conventions and examples separate from reflected signatures."""
import unittest
import xml.etree.ElementTree as ET
import polynomials
from generate import articles, xml_bytes


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

    def test_unknown_method_fails_instead_of_inventing_generic_help(self):
        with self.assertRaises(KeyError): self.description('Polynomial.Unknown(x)')


if __name__ == '__main__':
    unittest.main()
