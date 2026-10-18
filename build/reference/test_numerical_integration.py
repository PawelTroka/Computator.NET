"""Keep the independent numerical fixes and enriched reference in agreement."""
import hashlib
from pathlib import Path
import unittest
import xml.etree.ElementTree as ET

import generate


DERIVATIVE_METHODS = (
    'finiteDifferenceFormula',
    'stableFiniteDifferenceFormula',
    'twoPointfiniteDifferenceFormula',
    'centeredFivePointMethod',
)
ROOT_SIGNATURE = 'FunctionRoot.secantNewtonRaphsonMethod(fx,a,b,errorMax,N)'
TRANSFORM_SIGNATURE = 'Transform(functionPoints,transformate)'


def integration_inventory():
    items = [
        {
            'signature': 'Derivative.' + name + '(fx,x,order,eps)',
            'menu': 'real Derivative.' + name + '(function fx, real x, natural order, real eps)',
            'type': 'Derivative',
        }
        for name in DERIVATIVE_METHODS
    ]
    items.append({
        'signature': ROOT_SIGNATURE,
        'menu': 'real FunctionRoot.secantNewtonRaphsonMethod(function fx, real a, real b, real errorMax, natural N)',
        'type': 'FunctionRoot',
    })
    for element_type in ('Complex', 'Double'):
        items.append({
            'signature': TRANSFORM_SIGNATURE,
            'menu': element_type + '[] Transform(' + element_type + '[] functionPoints, string transformate)',
            'type': 'MathematicalTransformations',
        })
    return {'items': items, 'constants': []}


class NumericalIntegrationTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.inventory = integration_inventory()
        cls.entries = {entry['signature']: entry for entry in generate.articles(cls.inventory)}

    def fragment(self, signature):
        return ET.fromstring(self.entries[signature]['description'])

    def text(self, root, section):
        return ' '.join(' '.join(node.itertext()) for node in
                        root.findall("./p[@class='reference-" + section + "']"))

    def equation_sources(self, root):
        sources = []
        for image in root.findall("./p[@class='reference-equation']/img"):
            key = Path(image.get('src')).stem
            equation = generate.equation_manifest()[key]['tex']
            self.assertEqual(key, hashlib.sha256(equation.encode('utf-8')).hexdigest())
            self.assertTrue(image.get('alt'))
            sources.append(equation)
        return sources

    def test_four_derivatives_keep_recursive_step_contract_and_teaching(self):
        for name in DERIVATIVE_METHODS:
            with self.subTest(method=name):
                root = self.fragment('Derivative.' + name + '(fx,x,order,eps)')
                note = self.text(root, 'note')
                self.assertIn('retain the chosen eps at every level', note)
                self.assertIn('relative step recalculated at each evaluation point', note)
                self.assertIn('not a guaranteed result error', note)
                self.assertNotIn('revert to the default eps', note)
                self.assertIn('order zero returns fx(x)', self.text(root, 'arguments'))
                if name == 'stableFiniteDifferenceFormula':
                    self.assertIn('sqrt(eps)', self.text(root, 'arguments'))
                    self.assertIn('actual representable difference', self.text(root, 'arguments'))
                else:
                    self.assertIn('h=x·eps', self.text(root, 'arguments'))
                teaching = self.text(root, 'explanation')
                self.assertIn('floating-point subtraction and evaluation noise', teaching)
                self.assertIn('Higher derivative orders amplify noise and cancellation', teaching)
                self.assertEqual(self.equation_sources(root), [r'f^{(n)}(x)=\frac{d^n f}{dx^n}'])
                self.assertIn('for order=1', self.text(root, 'formula'))

    def test_bounded_secant_keeps_correct_guesses_and_root_cautions(self):
        root = self.fragment(ROOT_SIGNATURE)
        note = self.text(root, 'note')
        for contract in (
            'Use a<b',
            'Two distinct interior starting guesses',
            'midpoint and one-third point',
            '[a,b] is supplied as the interval',
            'too narrow for two interior floating-point values return NaN',
            'does not guarantee secant convergence',
            'no exact derivative is used',
        ):
            self.assertIn(contract, note)
        self.assertNotIn('(a+b)/3', note)
        teaching = self.text(root, 'explanation')
        self.assertIn('continuous function guarantees at least one root, but not uniqueness', teaching)
        self.assertIn('Verify both the reported location and its residual', teaching)
        self.assertEqual(self.equation_sources(root), [r'f(x_*)=0'])

    def test_transform_preserves_two_column_contract_and_typesets_hartley_once(self):
        root = self.fragment(TRANSFORM_SIGNATURE)
        note = self.text(root, 'note')
        for contract in (
            'N-by-2 real array with columns Re and Im',
            'transform BOTH axes with orthonormal scaling',
            'return N complex coefficients without modifying the input',
            'DCT and IDCT are inverses; DHT is its own inverse',
            'complex transforms and real DHT return empty arrays for empty input',
            'Real DHT updates its input in place',
            'complex FFT/IFFT uses Math.NET defaults and mutates the input array',
            'Complex FHT/IFHT starts from only the real component',
        ):
            self.assertIn(contract, note)
        self.assertNotIn('can throw for nonempty input', note)
        teaching = self.text(root, 'explanation')
        self.assertIn('This operation acts on sampled chart data', teaching)
        self.assertIn('Keep an unmodified copy when comparing transformations', teaching)
        sources = self.equation_sources(root)
        self.assertEqual(len(sources), 1)
        for term in (r'\frac{1}{\sqrt N}', r'\sum_{n=0}^{N-1}', r'\cos', r'+\sin',
                     r'\frac{2\pi kn}{N}', r'H(H(x))&=x', r'(N>0)'):
            self.assertIn(term, sources[0])
        self.assertIn('Real orthonormal Hartley transform', self.text(root, 'equation'))
        self.assertIn('H(H(x))=x', root.find("./p[@class='reference-equation']/img").get('alt'))
        self.assertEqual(root.findall("./p[@class='reference-formula']"), [],
                         'The equivalent typeset equation replaces the raw DHT formula.')

    def test_six_article_keys_categories_and_seven_typed_overloads_survive(self):
        categories = {
            'Derivative': 'Numerical differentiation',
            'FunctionRoot': 'Root finding',
            'MathematicalTransformations': 'Discrete transforms',
        }
        self.assertEqual(len(self.entries), 6)
        self.assertEqual(set(self.entries), {item['signature'] for item in self.inventory['items']})
        menu_count = 0
        for signature, entry in self.entries.items():
            with self.subTest(signature=signature):
                expected = [item for item in self.inventory['items'] if item['signature'] == signature]
                self.assertEqual(entry['type'], expected[0]['type'])
                self.assertEqual(entry['category'], categories[entry['type']])
                root = self.fragment(signature)
                blocks = root.findall("./p[@class='reference-signature']")
                self.assertEqual(len(blocks), 1)
                menus = [node.text for node in blocks[0].findall('code')]
                self.assertEqual(menus, sorted(item['menu'] for item in expected))
                self.assertNotIn('<strong>Syntax', entry['description'])
                for menu in menus:
                    self.assertEqual(entry['description'].count(generate.esc(menu)), 1)
                menu_count += len(menus)
        self.assertEqual(menu_count, 7)

if __name__ == '__main__':
    unittest.main()
