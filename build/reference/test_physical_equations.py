"""Protect unit directions, normalizations and complete named-base coverage."""
import unittest
import physical
from constant_lessons import CONSTANT_LESSONS
from enrichment import enrich
from physical_equations import BASE_EQUATIONS
from physical_family_lessons import ATOMIC_TEX, NATURAL_TEX, PARTICLE_SYMBOLS, family_lesson


class PhysicalEquationTests(unittest.TestCase):
    def extra(self, name):
        item = {'signature': name, 'type': 'PhysicalConstants', 'menu': 'real ' + name}
        return enrich(item, physical.describe(item, {name: '1'}), {name: '1'})

    def test_all_named_bases_and_aliases_have_a_defining_equation(self):
        normalized_lessons = {physical.ALIASES.get(name, name) for name in CONSTANT_LESSONS}
        self.assertEqual(set(BASE_EQUATIONS), set(physical.BASE) - normalized_lessons)
        for name in set(physical.BASE) | set(physical.ALIASES):
            with self.subTest(name=name):
                extra = self.extra(name)
                self.assertEqual(len(extra.equations), 2)
                self.assertEqual(extra.equations[0].label, 'Definition / interpretation')
                self.assertEqual(extra.equations[1].label, 'Stored numeric value')

    def test_atomic_natural_and_particle_grammars_match_the_authored_catalogue(self):
        self.assertEqual(set(ATOMIC_TEX), set(physical.ATOMIC))
        self.assertEqual(set(NATURAL_TEX), set(physical.NATURAL))
        self.assertEqual(set(PARTICLE_SYMBOLS), set(physical.PARTICLES))
        for prefix, keys in [('AtomicUnitOf', physical.ATOMIC), ('NaturalUnitOf', physical.NATURAL)]:
            for key in keys:
                self.assertEqual(len(self.extra(prefix + key).equations), 2)
        with self.assertRaises(KeyError):
            family_lesson('UnknownPhysicalConstant')
        with self.assertRaises(KeyError):
            family_lesson('AlphaParticleChargeToMassQuotient')

    def test_energy_conversion_divides_source_energy_by_target_energy(self):
        forward, _ = family_lesson('HertzElectronVoltRelationship')
        reverse, _ = family_lesson('ElectronVoltHertzRelationship')
        self.assertIn(r'\frac{h\,(1\,\mathrm{Hz})}{e\,(1\,\mathrm V)}', forward)
        self.assertIn(r'\frac{e\,(1\,\mathrm V)}{h\,(1\,\mathrm{Hz})}', reverse)
        self.assertIn(r'q_{\mathrm{eV}}=q_{\mathrm{Hz}}', forward)

    def test_magnetic_units_spins_and_signs_remain_distinct(self):
        self.assertIn(r'2\mu_{\mathrm B}', family_lesson('AtomicUnitOfMagneticDipoleMoment')[0])
        self.assertIn(r'4\pi\varepsilon_0', family_lesson('AtomicUnitOfPermittivity')[0])
        self.assertIn(r'I=1', family_lesson('DeuteronGFactor')[0])
        self.assertIn(r'I=\frac12', family_lesson('ProtonGFactor')[0])
        self.assertIn(r'\frac{-e}{m_e}', family_lesson('ElectronChargeToMassQuotient')[0])
        self.assertIn(r'\mu_p^{\prime}', family_lesson('ShieldedProtonMagneticMoment')[0])
        mhz = family_lesson('ElectronGyromagneticRatioOver2Pi')[0]
        self.assertIn(r'|\mu_e|', mhz)
        self.assertIn(r'10^6', mhz)
        self.assertIn(r'\mathrm{MHz}', mhz)

    def test_historical_xray_line_definition_uses_the_assigned_multiplier(self):
        item = {'signature': 'MoXUnit', 'type': 'PhysicalConstants', 'menu': 'real MoXUnit'}
        base = physical.describe(item, {'MoXUnit': '1.00209952E-13'})
        self.assertIn('707.831', base.formula)
        self.assertNotIn('707.400', base.formula)
        self.assertIn('707.831', BASE_EQUATIONS['MoXUnit'][0])
        self.assertIn('707.831', BASE_EQUATIONS['MoXUnit'][1])
        self.assertIn('1.00209952E-13', base.arguments)
        self.assertEqual(base.url, 'https://physics.nist.gov/cuu/pdf/JPCRD2010CODATA.pdf#page=41')


if __name__ == '__main__':
    unittest.main()
