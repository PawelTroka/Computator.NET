import copy
import hashlib
import json
from pathlib import Path
import unittest
import xml.etree.ElementTree as ET
import generate
from enrichment import enrich

class RichContentTests(unittest.TestCase):
    def article(self,signature,kind='SpecialFunctions',menu=None,constants=()):
        item={'signature':signature,'type':kind,'menu':menu or signature,'title':'','category':''}
        return generate.articles({'items':[item],'constants':list(constants)})[0]['description']
    def test_complete_and_incomplete_gamma_do_not_share_a_formula(self):
        a=self.article('gamma(x)');b=self.article('gamma(a,x)')
        self.assertIn('factorial',a)
        self.assertIn('lower limit',b)
        self.assertNotEqual(ET.fromstring(a).find('.//img').get('src'),ET.fromstring(b).find('.//img').get('src'))
    def test_typeset_beta_preserves_both_bug_warning_and_correct_alternative(self):
        value=self.article('Beta(x,a,b)')
        self.assertIn('Beta(0.5,1,1) = 2',value)
        self.assertIn('MathNet.Numerics.SpecialFunctions.BetaIncomplete',value)
        self.assertIn('reference-math',value)
    def test_every_authoring_image_has_source_hash_dimensions_and_alt(self):
        for signature in ['gamma(x)','Ai(x)','SphericalBesselJ0(x)','Hypergeometric2F1(a,b,c,x)']:
            root=ET.fromstring(self.article(signature))
            for image in root.iter('img'):
                asset=generate.equation_manifest()[Path(image.get('src')).stem]
                self.assertEqual(Path(image.get('src')).stem,hashlib.sha256(asset['tex'].encode()).hexdigest())
                self.assertEqual(int(image.get('width')),asset['width'])
                self.assertTrue(image.get('alt'))
    def test_invalid_image_attributes_are_rejected(self):
        original=self.article('gamma(x)')
        for name,value in [('src','data:image/png;base64,unsafe'),('src','reference-equations/../a.png'),('width','99999'),('height','-1'),('onerror','alert(1)'),('alt','')]:
            root=ET.fromstring(original);root.find('.//img').set(name,value)
            with self.assertRaises(ValueError):generate.validate_fragment(ET.tostring(root,encoding='unicode'))
    def test_typed_overloads_are_kept_once_each(self):
        items=[{'signature':'ConicalP(μ,λ,x)','menu':menu,'type':'SpecialFunctions'} for menu in ['real ConicalP(integer μ,real λ,real x)','real ConicalP(real μ,real λ,real x)']]
        value=generate.articles({'items':items,'constants':[]})[0]['description']
        root=ET.fromstring(value)
        self.assertEqual(len(root.find("./p[@class='reference-signature']").findall('code')),2)
        self.assertNotIn('<strong>Syntax',value)
        for item in items:self.assertEqual(value.count(generate.esc(item['menu'])),1)
    def test_stored_physical_value_is_not_replaced_by_the_modern_exact_value(self):
        value=self.article('ElementaryCharge','PhysicalConstants',constants=[{'name':'ElementaryCharge','value':'1.602176565E-19'}])
        self.assertIn('Stored application value: 1.602176565E-19.',value)
        self.assertIn('1.602176634',value)
        self.assertIn('historical',value)
    def test_unknown_functions_still_fail_instead_of_receiving_generic_lessons(self):
        with self.assertRaises(KeyError):self.article('NeverImplemented(x)')
    def test_worked_example_does_not_repeat_its_old_one_line_example(self):
        value=self.article('gamma(x)')
        self.assertNotIn('<strong>Example:</strong> gamma(5)',value)
        self.assertIn('<code>gamma(5)</code>',value)
