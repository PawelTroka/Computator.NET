"""Keep the qualified integer reference honest about the pinned library."""
import unittest
from generate import articles


class EuclidReferenceTests(unittest.TestCase):
    def article(self, signature, menu):
        return articles({'items': [{'signature': signature, 'menu': menu, 'type': 'Euclid'}],
                         'constants': []})[0]

    def test_gcd_and_lcm_are_distinct_from_the_legacy_short_names(self):
        for method in ('GreatestCommonDivisor', 'LeastCommonMultiple'):
            with self.subTest(method=method):
                result = self.article('Euclid.'+method+'(a,b)', 'integer Euclid.'+method+'(integer a, integer b)')
                self.assertEqual(result['category'], 'Integer arithmetic')
                self.assertIn('existing gcd, nwd, lcm and nww', result['description'])
                self.assertIn('above 2^53', result['description'])
                self.assertIn('BigInteger', result['description'])
                self.assertIn('BigInteger results require an explicit conversion', result['description'])
                self.assertIn('((real)Euclid.GreatestCommonDivisor', result['description'])

    def test_collection_gcd_documents_the_upstream_leading_zero_defect(self):
        result = self.article('Euclid.GreatestCommonDivisor(integers)',
                              'integer Euclid.GreatestCommonDivisor(IList`1 integers)')
        self.assertIn('leading zero therefore incorrectly returns 0', result['description'])
        self.assertIn('explicitly fold the two-value overload', result['description'])

    def test_extended_gcd_teaches_out_arguments_and_the_independent_identity(self):
        result = self.article('Euclid.ExtendedGreatestCommonDivisor(a,b,out x,out y)',
                              'integer Euclid.ExtendedGreatestCommonDivisor(integer a, integer b, out integer x, out integer y)')
        self.assertIn('out u,out v', result['description'])
        self.assertIn('240*u+46*v equals d=2', result['description'])
        self.assertIn('coefficient pair is not unique', result['description'])

    def test_lcm_uses_the_smallest_positive_multiple_for_nonzero_operands(self):
        result = self.article('Euclid.LeastCommonMultiple(a,b)',
                              'integer Euclid.LeastCommonMultiple(integer a, integer b)')
        self.assertIn('smallest positive shared multiple', result['description'])
        self.assertNotIn('first nonnegative shared multiple', result['description'])

    def test_modulus_and_remainder_explain_opposite_sign_inputs(self):
        for method in ('Modulus', 'Remainder'):
            with self.subTest(method=method):
                result = self.article('Euclid.'+method+'(dividend,divisor)',
                                      'integer Euclid.'+method+'(integer dividend, integer divisor)')
                self.assertIn('modulus is 2 while the remainder is -1', result['description'])
                self.assertIn('zero integer divisor throws DivideByZeroException', result['description'])
                if method == 'Modulus':
                    self.assertIn('can overflow fixed-width', result['description'])

    def test_power_of_two_explains_int32_vs_int64_overload_selection(self):
        result = self.article('Euclid.PowerOfTwo(exponent)', 'integer Euclid.PowerOfTwo(integer exponent)')
        self.assertIn('0..30', result['description'])
        self.assertIn('0..62', result['description'])
        self.assertIn('(integer)40', result['description'])

    def test_unreviewed_euclid_method_is_rejected(self):
        with self.assertRaises(KeyError):
            self.article('Euclid.Unknown(number)', 'integer Euclid.Unknown(integer number)')


if __name__ == '__main__':
    unittest.main()
