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

if __name__=='__main__':unittest.main()
