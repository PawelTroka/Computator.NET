"""Scientific-convention regressions independent of generated equation assets.

The Windows suite executes displayed examples through their actual TSL editor.
These checks instead catch pairing a correct example with the wrong overload,
branch description, normalization or implementation caveat.
"""
import math
from pathlib import Path
import unittest
import xml.etree.ElementTree as ET

import elementary
from enrichment import COMPLEX_INVERSES, enrich
from normalization_examples import NORMALIZATION_EXAMPLES
import special


class EditorialAccuracyTests(unittest.TestCase):
    def content(self, signature, kind='SpecialFunctions'):
        menu = ('complex ' if '(z' in signature else 'real ') + signature
        item = {'signature': signature, 'type': kind, 'menu': menu}
        module = elementary if kind == 'ElementaryFunctions' else special
        article = module.describe(item, {})
        return article, enrich(item, article, {})

    def elementary_content(self, signature):
        return self.content(signature, 'ElementaryFunctions')

    def number(self, extra, code):
        example = next(example for example in extra.examples if example.code == code)
        return float(example.result.removeprefix('approximately '))

    def test_negative_real_square_root_is_documented_as_automatic_promotion(self):
        article, extra = self.elementary_content('sqrt(x)')
        self.assertIn('promotes negative', article.arguments)
        self.assertIn('automatically returns a complex', ' '.join(extra.paragraphs))
        self.assertNotIn('need the complex overload', ' '.join(extra.paragraphs))
        self.assertEqual(self.number(extra, 'Im(sqrt(-9))'), 3)

    def test_real_and_complex_roots_keep_their_distinct_branches(self):
        real, real_extra = self.elementary_content('root(x,n)')
        complex_article, complex_extra = self.elementary_content('root(z,n)')
        self.assertIn('private parity helper', real.arguments)
        self.assertIn('principal Log(z)', complex_article.arguments)
        self.assertEqual(self.number(real_extra, 'root(-8,3)'), -2)
        self.assertEqual(self.number(complex_extra, 'Re(root(-8+0i,3))'), 1)
        self.assertAlmostEqual(self.number(complex_extra, 'Im(root(-8+0i,3))'), math.sqrt(3), places=9)

    def test_nan_sign_contract_does_not_claim_a_math_sign_exception(self):
        article, _ = self.elementary_content('sgn(x)')
        self.assertIn('NaN propagates as NaN', article.arguments)
        self.assertIn('before calling Math.Sign', article.arguments)

    def test_hyperbolic_arguments_are_not_circular_angles(self):
        for signature in ['sinh(x)', 'cosh(x)', 'tanh(x)', 'ctgh(x)', 'cosech(x)']:
            article, _ = self.elementary_content(signature)
            with self.subTest(signature=signature):
                self.assertIn('dimensionless', article.arguments)
                self.assertNotIn('Angles are measured', article.arguments)

    def test_arccot_teaches_the_application_branch_and_alias(self):
        article, extra = self.elementary_content('arccot(x)')
        _, alias = self.elementary_content('arcctg(x)')
        self.assertIn('range (0,PI)', article.arguments)
        self.assertEqual(self.number(extra, 'arccot(-1)*180/PI'), 135)
        self.assertNotEqual(135, math.degrees(math.atan(-1)))
        self.assertEqual(extra, alias)

    def test_complex_inverse_examples_select_complex_inputs(self):
        for name in COMPLEX_INVERSES:
            _, extra = self.elementary_content(name + '(z)')
            with self.subTest(name=name):
                self.assertTrue(extra.examples)
                self.assertTrue(all('i' in example.code for example in extra.examples))
                self.assertNotIn(r'-1\le x\le1', extra.equations[0].tex)
                self.assertNotIn('An inverse function answers which input', ' '.join(extra.paragraphs))

    def test_pending_arsinh_fix_is_not_silently_claimed_by_the_reference(self):
        # PR #182 owns the numerical correction; remove these caveats only when
        # integrating that change into the corresponding reference base.
        for signature in ['arsinh(x)', 'arsinh(z)']:
            article, extra = self.elementary_content(signature)
            with self.subTest(signature=signature):
                self.assertIn('Log(t)+sqrt(t²+1)', article.note)
                self.assertIn('not Log(t+sqrt(t²+1))', article.note)
                self.assertEqual(extra.equations[0].label, 'Intended inverse')
                self.assertFalse(extra.replaces_formula)
                self.assertNotIn('arsinh(0) = 0', article.example)

    def test_spence_equations_use_the_reflected_argument_for_both_overloads(self):
        for argument in ['x', 'z']:
            _, spence = self.content('SpencesIntegral(' + argument + ')')
            _, dilog = self.content('diLog(' + argument + ')')
            with self.subTest(argument=argument):
                self.assertIn(r'\operatorname{Li}_2(1-z)', spence.equations[0].tex)
                self.assertNotEqual(spence.equations[0].tex, dilog.equations[0].tex)
                self.assertIn('not an alias of diLog(z)', ' '.join(spence.paragraphs))
                self.assertAlmostEqual(float(spence.examples[0].result.removeprefix('approximately ')), -math.pi**2/12, places=9)

    def test_airy_examples_evaluate_the_requested_value_or_derivative(self):
        for name in ['Ai', 'Bi', 'AiPrime', 'BiPrime']:
            _, extra = self.content(name + '(x)')
            with self.subTest(name=name):
                self.assertEqual(extra.examples[0].code, name + '(0)')
                self.assertEqual(extra.equations[0].label, 'Underlying Airy equation' if 'Prime' in name else 'Definition')

    def test_incomplete_gamma_separates_area_from_regularized_probability(self):
        _, area = self.content('gamma(a,x)')
        _, upper = self.content('gammaQ(a,x)')
        _, lower = self.content('gammaP(a,x)')
        raw = self.number(area, 'gamma(3,1)')
        q = self.number(upper, 'gammaQ(3,1)')
        p = self.number(lower, 'gammaP(3,1)')
        self.assertGreater(raw, 1)
        self.assertAlmostEqual(raw, 5/math.e, places=9)
        self.assertAlmostEqual(raw/math.gamma(3), q, places=9)
        self.assertAlmostEqual(p+q, 1, places=9)

    def test_every_exact_signature_example_is_attached_to_a_real_catalogue_entry(self):
        path = Path(__file__).resolve().parents[2] / 'Computator.NET.Core/Static/functions.xml'
        signatures = {entry.attrib['Signature'] for entry in ET.parse(path).getroot()}
        self.assertEqual(len(NORMALIZATION_EXAMPLES), 50)
        for signature, examples in NORMALIZATION_EXAMPLES.items():
            with self.subTest(signature=signature):
                self.assertIn(signature, signatures)
                _, extra = self.content(signature)
                self.assertEqual(extra.examples, examples)
                for example in examples:
                    self.assertIn(signature.split('(')[0]+'(', example.code)
                    self.assertTrue(math.isfinite(float(example.result.removeprefix('approximately '))))

    def test_laguerre_and_associated_legendre_equations_keep_overload_qualifications(self):
        _, ordinary = self.content('Laguerre(n,x)')
        _, generalized = self.content('Laguerre(n,α,x)')
        _, associated = self.content('LegendreP(l,m,x)')
        self.assertIn(r'\alpha=0', ordinary.equations[0].tex)
        self.assertNotEqual(ordinary.equations[0].tex, generalized.equations[0].tex)
        self.assertIn(r'0\le m\le l', associated.equations[0].tex)
        self.assertIn(r'P_l^{-m}', associated.equations[0].tex)
        self.assertAlmostEqual(self.number(associated, 'LegendreP(2,-1,0.5)'), math.sqrt(3)/8, places=11)

    def test_conjugate_hypergeometric_formulas_and_examples_supply_a_and_its_conjugate(self):
        for name in ['Hypergeometric2F1', 'Hypergeometric2F1renorm']:
            article, extra = self.content(name + '(a,c,x)')
            self.assertIn('conjugate(a)', article.formula)
            self.assertIn(r'\overline{a}', extra.equations[0].tex)
            self.assertIn('(i,0.5,0.25)', extra.examples[0].code)
        _, ordinary = self.content('Hypergeometric2F1(a,c,x)')
        _, renormalized = self.content('Hypergeometric2F1renorm(a,c,x)')
        self.assertAlmostEqual(float(ordinary.examples[0].result.removeprefix('approximately ')), math.cosh(math.pi/3), places=10)
        self.assertAlmostEqual(float(renormalized.examples[0].result.removeprefix('approximately ')), math.cosh(math.pi/3)/math.sqrt(math.pi), places=10)

    def test_scaled_spherical_bessel_values_include_both_scaling_and_normalization(self):
        _, regular = self.content('ModifiedSphericalBesselIn(n,x)')
        _, irregular = self.content('ModifiedSphericalBesselKn(n,x)')
        self.assertIn('e^{-|x|}', regular.equations[0].tex)
        self.assertIn('e^x', irregular.equations[0].tex)
        self.assertAlmostEqual(self.number(regular, 'ModifiedSphericalBesselIn(0,1)'), (1-math.exp(-2))/2, places=11)
        self.assertAlmostEqual(self.number(irregular, 'ModifiedSphericalBesselKn(0,1)'), math.pi/2, places=11)

    def test_elliptic_numerical_examples_match_independent_defining_integrals(self):
        def simpson(function, end, count=4096):
            step = end/count
            return step/3*(function(0)+function(end)+sum((4 if k%2 else 2)*function(k*step) for k in range(1,count)))
        for name, integrand in [('EllipticK', lambda t: 1/math.sqrt(1-.25*math.sin(t)**2)),
                                ('EllipticE', lambda t: math.sqrt(1-.25*math.sin(t)**2))]:
            _, extra = self.content(name+'(x)')
            self.assertAlmostEqual(self.number(extra, name+'(0.5)'), simpson(integrand, math.pi/2), places=10)
        _, third = self.content('EllipticΠ(φ,x,n)')
        self.assertIn('1+n', third.equations[0].tex)
        self.assertAlmostEqual(self.number(third, 'EllipticΠ(PI/4,0,1)'), simpson(lambda t: 1/(1+math.sin(t)**2), math.pi/4), places=10)

    def test_erfcx_example_retains_the_direct_wrapper_warning(self):
        article, extra = self.content('erfcx(x)')
        self.assertIn('forms erfc(z)/exp(−z²) directly', article.note)
        self.assertAlmostEqual(self.number(extra, 'erfcx(1)'), math.e*math.erfc(1), places=9)

    def test_half_order_fermi_examples_match_independent_normalized_integrals(self):
        # t=u² removes the endpoint singularity when the order is −1/2.
        # The upper tail past u=10 is negligible at the displayed precision.
        count=4096
        step=10/count
        for name, order in [('FermiDiracFmhalf', -.5), ('FermiDiracFhalf', .5), ('FermiDiracF3half', 1.5)]:
            def integrand(u):
                return 2*u**int(2*order+1)/(math.exp(u*u)+1)/math.gamma(order+1)
            integral=step/3*(integrand(0)+integrand(10)+sum((4 if k%2 else 2)*integrand(k*step) for k in range(1,count)))
            _, extra=self.content(name+'(x)')
            self.assertAlmostEqual(self.number(extra,name+'(0)'),integral,places=10)


if __name__ == '__main__':
    unittest.main()
