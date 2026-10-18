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
                self.assertEqual(article['url'],'https://dlmf.nist.gov/10.47')

if __name__=='__main__':unittest.main()
