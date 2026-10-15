"""Check magnetic tooltip equations against independent CODATA 2010 data.

The numerical fixtures and conventions come from table XLI in
https://physics.nist.gov/cuu/Constants/codata.pdf (2010 adjustment), rather
than from the author's equation dictionaries. Production values stay intact.
"""
from fractions import Fraction
import re
import unittest

import physical


class PhysicalReferenceTests(unittest.TestCase):
    def article(self, name, value):
        return physical.describe({'signature': name}, {name: str(value)})

    def test_gfactor_equations_recover_signed_moments_with_the_correct_mass_and_spin(self):
        e, hbar = 1.602176565e-19, 1.054571726e-34
        magnetons = {
            'eℏ/(2me)': e * hbar / (2 * 9.10938291e-31),
            'eℏ/(2mμ)': e * hbar / (2 * 1.883531475e-28),
            'μN': 5.05078353e-27,
        }
        # A muon uses its own mass; a deuteron has I=1, unlike the other I=1/2 entries.
        cases = (
            ('Electron', -2.00231930436153, -9.28476430e-24),
            ('Muon', -2.0023318418, -4.49044807e-26),
            ('Proton', 5.585694713, 1.410606743e-26),
            ('Neutron', -3.82608545, -0.96623647e-26),
            ('Deuteron', 0.8574382308, 0.433073489e-26),
            ('Triton', 5.957924896, 1.504609447e-26),
            ('Helion', -4.255250613, -1.074617486e-26),
        )
        for particle, g, expected_moment in cases:
            with self.subTest(particle=particle):
                article = self.article(particle + 'GFactor', g)
                self.assertTrue(article.formula.startswith('μ = g·μref·I;'))
                reference = re.search(r'μref=([^;]+);', article.formula)[1]
                spin = float(Fraction(re.search(r'I=([0-9/]+)\.', article.formula)[1]))
                if reference == 'μN':
                    self.assertIn('μN=eℏ/(2mp)', article.formula)
                actual = g * magnetons[reference] * spin
                self.assertAlmostEqual(actual / expected_moment, 1., delta=1e-8)

    def test_gyromagnetic_formula_converts_si_moments_to_megahertz(self):
        # The MHz/T values and magnetic moments are separately tabulated quantities.
        cases = (
            ('Electron', -9.28476430e-24, 28024.95266),
            ('Neutron', -0.96623647e-26, 29.1646943),
            ('Proton', 1.410606743e-26, 42.5774806),
            ('ShieldedHelion', -1.074553044e-26, 32.43410084),
            ('ShieldedProton', 1.410570499e-26, 42.5763866),
        )
        for particle, moment, expected_mhz in cases:
            with self.subTest(particle=particle):
                article = self.article(particle + 'GyromagneticRatioOver2Pi', expected_mhz)
                # Read the conversion exponent from the displayed equation itself.
                numeric = re.fullmatch(
                    r'.*; value\[MHz/T\]=\|μ\[J/T\]\|/\(h\[J·s\]·I·10([⁰¹²³⁴⁵⁶⁷⁸⁹]+)\)\.',
                    article.formula)
                self.assertIsNotNone(numeric)
                exponent = int(numeric[1].translate(str.maketrans('⁰¹²³⁴⁵⁶⁷⁸⁹', '0123456789')))
                spin = float(Fraction(re.search(r'I=([0-9/]+)', article.note)[1]))
                actual = abs(moment) / (6.62606957e-34 * spin * 10**exponent)
                self.assertAlmostEqual(actual / expected_mhz, 1., delta=1e-8)
                self.assertIn('Unit: MHz T⁻¹.', article.arguments)

    def test_shielded_ratio_explains_both_reference_environments(self):
        article = self.article('ShieldedHelionToShieldedProtonMagneticMomentRatio', -0.7617861313)
        self.assertIn('spherical water sample at 25 °C', article.note)
        self.assertIn('spherical helium gas sample at 25 °C', article.note)


if __name__ == '__main__':
    unittest.main()
