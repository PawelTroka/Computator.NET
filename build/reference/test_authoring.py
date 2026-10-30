"""Editorial safety checks independent of the materialized XML."""
import copy
import unittest
from generate import articles, validate_fragment, xml_bytes

class AuthoringTests(unittest.TestCase):
    def inventory(self):
        return {'items':[{'signature':'sinc(x)','menu':'real sinc(real x)','type':'ElementaryFunctions','category':'','title':''}], 'constants':[]}
    def test_unknown_function_does_not_get_generic_filler(self):
        inventory=self.inventory(); inventory['items'][0]['signature']='unknownFunction(x)'
        with self.assertRaises(KeyError): articles(inventory)
    def test_unknown_category_fails(self):
        inventory=self.inventory(); inventory['items'][0]['type']='NewDistribution'
        with self.assertRaises(ValueError): articles(inventory)
    def test_overload_key_conflicts_are_not_silently_overwritten(self):
        inventory=self.inventory(); other=copy.deepcopy(inventory['items'][0]);other['type']='SpecialFunctions';inventory['items'].append(other)
        with self.assertRaises(ValueError): articles(inventory)
    def test_active_markup_and_external_assets_are_rejected(self):
        for fragment in ('<div><script>alert(1)</script></div>', '<div><img src="https://example.org/a.png" /></div>',
            '<div onclick="alert(1)">text</div>', '<div><a href="javascript:alert(1)">link</a></div>', '<iframe />'):
            with self.subTest(fragment=fragment), self.assertRaises(ValueError):validate_fragment(fragment)
    def test_titles_do_not_drive_generation(self):
        first=self.inventory(); second=copy.deepcopy(first)
        second['items'][0].update(title='Some old help',category='Some old group')
        self.assertEqual(xml_bytes(articles(first)),xml_bytes(articles(second)))
    def test_valid_source_is_deterministic_and_has_readable_formula(self):
        first=articles(self.inventory()); second=articles(self.inventory())
        self.assertEqual(xml_bytes(first),xml_bytes(second))
        self.assertIn('sinc(0)=1',first[0]['description'])
        self.assertIn('<img class="reference-math"',first[0]['description'])
        self.assertNotIn('<strong>Syntax',first[0]['description'])
    def test_keywords_do_not_require_math_articles(self):
        inventory=self.inventory();inventory['items'].append({'signature':'while','type':'Keyword','menu':'while'})
        self.assertEqual(len(articles(inventory)),1)
    def test_inverse_error_articles_teach_both_halves_of_their_domains(self):
        for name,domain,endpoint,example in (
            ('inverseErf','−1≤x≤1','x=−1 gives −∞','inverseErf(-0.5)'),
            ('inverseErfc','0≤x≤2','x=2 gives −∞','inverseErfc(1.5)'),
        ):
            with self.subTest(function=name):
                inventory={'items':[{'signature':name+'(x)','menu':'real '+name+'(real x)','type':'SpecialFunctions'}], 'constants':[]}
                article=articles(inventory)[0]
                self.assertIn(domain,article['description'])
                self.assertIn(endpoint,article['description'])
                self.assertIn('Inputs outside the domain return NaN',article['description'])
                self.assertIn('<pre><code>'+example+'</code></pre>',article['description'])
                self.assertNotIn('accepts ONLY',article['description'])
                self.assertEqual(article['url'],'https://dlmf.nist.gov/7.17')
                if name=='inverseErfc':
                    self.assertIn('including subnormal inputs',article['description'])
    def test_riccati_hankel_articles_use_spherical_relations_and_keep_zero_caveat(self):
        for name,relation in (('RiccatiBesselξ','ξₙ(x)=x·hₙ⁽¹⁾(x)=Sₙ(x)−iCₙ(x)'),
                              ('RiccatiBesselζ','ζₙ(x)=x·hₙ⁽²⁾(x)=Sₙ(x)+iCₙ(x)')):
            with self.subTest(name=name):
                inventory={'items':[{'signature':name+'(n,x)',
                    'menu':'complex '+name+'(int n, real x)','type':'SpecialFunctions'}], 'constants':[]}
                article=articles(inventory)[0]
                self.assertIn(relation,article['description'])
                self.assertIn('spherical Bessel functions',article['description'])
                self.assertIn('Nonpositive arguments return NaN',article['description'])
                self.assertIn('finite order-zero limit at x=0 is not evaluated',article['description'])
                self.assertNotIn('CYLINDRICAL Hankel',article['description'])
                self.assertIn('Riccati–Bessel equation',article['description'])
                self.assertIn('u″+[1−n(n+1)/x²]u=0',article['description'])
                self.assertNotIn('Underlying cylindrical equation',article['description'])
                self.assertEqual(article['url'],'https://dlmf.nist.gov/10.47')
    def test_double_factorial_overloads_document_the_native_overflow_contract(self):
        inventory={'items':[
            {'signature':'DoubleFactorial(n)','menu':'real DoubleFactorial(integer n)','type':'ElementaryFunctions'},
            {'signature':'DoubleFactorial(x)','menu':'real DoubleFactorial(real x)','type':'ElementaryFunctions'}
        ],'constants':[]}
        entries=articles(inventory)
        self.assertEqual({entry['signature'] for entry in entries},{'DoubleFactorial(n)','DoubleFactorial(x)'})
        for entry in entries:
            with self.subTest(signature=entry['signature']):
                text=entry['description']
                self.assertIn('orders above 297 as overflow',text)
                self.assertIn('return-value mode yields positive infinity',text)
                self.assertIn('error-reporting mode throws',text)
                self.assertIn('rather than wrapping',text)
                self.assertIn('truncating toward zero',text)
                self.assertIn('<code>DoubleFactorial(7)</code>',text)
                self.assertIn('<strong>Result:</strong> 105',text)
                self.assertIn('<img class="reference-math"',text)

    def ode_inventory(self):
        return {'items':[
            {'signature':'DifferentialEquation.rungeKutta4(derivative,y0,start,end,samples)',
             'menu':'Double[] DifferentialEquation.rungeKutta4(function derivative, real y0, real start, real end, integer samples)',
             'type':'DifferentialEquation'},
            {'signature':'DifferentialEquation.rungeKutta4(derivative,y0,start,end,samples)',
             'menu':'Vector`1[] DifferentialEquation.rungeKutta4(function derivative, Vector`1 y0, real start, real end, integer samples)',
             'type':'DifferentialEquation'}], 'constants':[]}

    def test_ode_overloads_share_an_article_with_both_typed_signatures(self):
        inventory=self.ode_inventory()
        entries=articles(inventory)
        self.assertEqual(len(entries),1)
        article=entries[0]
        self.assertEqual(article['category'],'Ordinary differential equations')
        for item in inventory['items']:
            self.assertIn('<code>'+item['menu']+'</code>',article['description'])
        self.assertIn('/v5.0.0/src/Numerics/OdeSolvers/RungeKutta.cs',article['url'])

    def test_unknown_ode_methods_and_signatures_fail_authoring(self):
        for signature in ('DifferentialEquation.adaptive(derivative,y0,start,end,samples)',
                          'DifferentialEquation.rungeKutta4(derivative,y0,start,end,tolerance)'):
            with self.subTest(signature=signature):
                inventory=self.ode_inventory()
                inventory['items']=inventory['items'][:1]
                inventory['items'][0]['signature']=signature
                with self.assertRaises(KeyError): articles(inventory)

    def test_ode_article_explains_grid_and_zero_interval_contract(self):
        text=articles(self.ode_inventory())[0]['description']
        for contract in ('samples includes the initial and final states',
                         'default 101, accepted range 2 through 1,000,001',
                         'Reverse intervals are supported',
                         'Equal bounds return independent copies of the initial state without evaluating the derivative',
                         'finite, representable half-step'):
            self.assertIn(contract,text)

    def test_ode_article_explains_ownership_and_numerical_limits(self):
        text=articles(self.ode_inventory())[0]['description']
        for contract in ('Vector callbacks receive copies',
                         'returned states and reused derivative buffers do not alias',
                         'no adaptive error estimate, stiff-system handling or event detection',
                         'Refine the grid and check convergence',
                         'Memory grows with samples times state dimension',
                         'nonfinite arithmetic throws ArithmeticException'):
            self.assertIn(contract,text)

    def test_ode_article_teaches_a_reproducible_scalar_example(self):
        text=articles(self.ode_inventory())[0]['description']
        self.assertIn('<pre><code>DifferentialEquation.rungeKutta4((t,u) =&gt; 2*t,3.0,0.0,2.0,21)[20]</code></pre>',text)
        self.assertIn('<strong>Result:</strong> approximately 7',text)
        self.assertIn('twenty steps and twenty-one stored states',text)
        self.assertIn('The returned array stores the evolving state, not derivative values',text)

if __name__=='__main__':unittest.main()
