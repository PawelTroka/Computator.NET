"""Keep the two zero-filled factory articles consistent with their scalar contract."""
import unittest
from generate import articles


class MatrixFactoryReferenceTests(unittest.TestCase):
    def test_factories_explain_supported_types_and_dimension_errors(self):
        inventory = {'items': [
            {'signature': 'matrix<T>(n,m)', 'menu': 'Matrix matrix<T>(integer n, integer m)',
             'type': 'MatrixFunctions'},
            {'signature': 'vector<T>(n)', 'menu': 'Vector`1 vector<T>(integer n)',
             'type': 'MatrixFunctions'},
        ], 'constants': []}
        entries = articles(inventory)
        self.assertEqual(len(entries), 2)
        for entry in entries:
            with self.subTest(signature=entry['signature']):
                self.assertEqual(entry['category'], 'Matrix construction')
                text = entry['description']
                for scalar in ('real (double)', 'float', 'complex (System.Numerics.Complex)',
                               'MathNet.Numerics.Complex32'):
                    self.assertIn(scalar, text)
                self.assertIn('integer, natural and decimal, throw ArgumentException', text)
                self.assertIn('zero dimensions create empty', text)
                self.assertIn('Negative dimensions throw ArgumentOutOfRangeException', text)
                self.assertNotIn('return null', text)
                self.assertIn('&lt;float&gt;', text)

    def test_element_overloads_still_describe_values_not_lengths(self):
        inventory = {'items': [
            {'signature': 'vector(elements1,elements2, ...)',
             'menu': 'Vector vector(real elements1, real elements2, ...)', 'type': 'MatrixFunctions'},
        ], 'constants': []}
        text = articles(inventory)[0]['description']
        self.assertIn('single-element params overload', text)
        self.assertNotIn('Complex32', text)


if __name__ == '__main__':
    unittest.main()
