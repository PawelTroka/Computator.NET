import copy
from html.parser import HTMLParser
from tempfile import TemporaryDirectory
from unittest.mock import patch
import hashlib
import json
import re
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
    def test_typeset_beta_describes_the_repaired_public_wrappers(self):
        for signature in ['Beta(x,a,b)','β(x,a,b)','BetaNorm(x,a,b)']:
            with self.subTest(signature=signature):
                value=self.article(signature)
                self.assertIn('The public order is x,a,b',value)
                self.assertIn('reference-math',value)
                self.assertIn('reference-worked-example',value)
                self.assertNotIn('legacy parameter-order caveat',value)
                self.assertNotIn('Beta(0.5,1,1) = 2',value)
                self.assertNotIn('until the numerical wrapper is repaired',value)
                self.assertNotIn('implementation warning below',value)
                self.assertNotIn('forwards a different argument order',value)
    def test_nine_j_reference_describes_the_corrected_doubled_spin_contract(self):
        signature='Coupling9j(ja,jb,jc,jd,je,jf,jg,jh,ji)'
        menu='real Coupling9j('+','.join('integer '+name for name in ['ja','jb','jc','jd','je','jf','jg','jh','ji'])+')'
        value=self.article(signature,menu=menu)
        self.assertIn('TWICE the physical spin',value)
        self.assertIn('ji selects the lower-right spin independently of jf',value)
        self.assertIn('Coupling9j(2,2,2,2,2,2,2,2,0) = 1/18',value)
        self.assertNotIn('forwards jf',value)
        self.assertNotIn('not a general 9-j calculation',value)
    def test_every_authoring_image_has_source_hash_dimensions_and_alt(self):
        for signature in ['gamma(x)','Ai(x)','SphericalBesselJ0(x)','Hypergeometric2F1(a,b,c,x)']:
            root=ET.fromstring(self.article(signature))
            for image in root.iter('img'):
                asset=generate.equation_manifest()[Path(image.get('src')).stem]
                identity=('inline\0' if asset.get('mode')=='inline' else '')+asset['tex']
                self.assertEqual(Path(image.get('src')).stem,hashlib.sha256(identity.encode()).hexdigest())
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

    def test_all_benchmarks_have_explanatory_diagrams_with_their_convention(self):
        for name in ['Ackley','Rastrigin','Bohachevsky1','Himmelblau','Matyas','Rosenbrock','dropWave','sixHumpCamel']:
            with self.subTest(name=name):
                root=ET.fromstring(self.article(name+'(x,y)'))
                arguments=root.find("./p[@class='reference-arguments']")
                figure=root.find("./div[@class='reference-figure']")
                self.assertTrue(figure.find('img').get('alt'))
                self.assertEqual(figure.find('img').get('class'),'reference-plot')
                self.assertTrue(figure.find('img').get('src').startswith('reference-figures/'))
                self.assertIn('Reading the landscape',''.join(figure.itertext()))
                if name=='Bohachevsky1':
                    self.assertIn('omits +0.7',arguments.text or ''.join(arguments.itertext()))
                    self.assertIn('minimum is -0.7',''.join(figure.itertext()))

    def test_figure_allowlist_rejects_wrong_class_directory_and_attributes(self):
        original=self.article('Ackley(x,y)')
        for name,value in [('src','https://example.invalid/a.png'),('src','reference-figures/../a.png'),
                           ('class','reference-math'),('width','2401'),('height','801'),
                           ('data-depth','0'),('style','width:100vw'),('alt','')]:
            with self.subTest(name=name,value=value):
                root=ET.fromstring(original)
                root.find(".//img[@class='reference-plot']").set(name,value)
                with self.assertRaises(ValueError):generate.validate_fragment(ET.tostring(root,encoding='unicode'))


class VisibleText(HTMLParser):
    def __init__(self):
        super().__init__()
        self.parts = []

    def handle_data(self, data):
        self.parts.append(data)


class PreviewTests(unittest.TestCase):
    def test_preview_keeps_distinct_article_anchors_when_wrapping_inline_math(self):
        with TemporaryDirectory() as directory:
            assets = Path(directory) / 'assets'
            assets.mkdir()
            image = ('<img class="reference-math-inline" src="reference-equations/' + 'a' * 64 +
                     '.png" alt="x" width="12" height="10" data-depth="25" />')
            entries = [
                {'signature': 'Plain(x)', 'title': 'Plain article',
                 'description': '<div><p>Meaning.</p><pre><code>Plain(1)</code></pre></div>'},
                {'signature': 'Inline(x)', 'title': 'Inline article',
                 'description': '<div><p>Before ' + image + ' after.</p></div>'},
                {'signature': 'Multiple(x)', 'title': 'Multiple expressions',
                 'description': '<div><p>First ' + image + ', second ' + image + '.</p></div>'},
            ]
            original = copy.deepcopy(entries)
            output = Path(directory) / 'preview.html'
            with patch.object(generate, 'ASSETS', assets):
                generate.write_preview(entries, output)
            anchors = re.findall(r'<section id="([^"]+)" data-signature="([^"]+)"',
                                 output.read_text(encoding='utf-8'))
            self.assertEqual(anchors, [('entry-' + str(index), entry['signature'])
                                       for index, entry in enumerate(entries)])
            self.assertEqual(entries, original)

    def test_preview_removes_all_signature_blocks_but_keeps_content_and_metadata(self):
        for signature_block in [
            '<p class="reference-signature"><code>real gamma(real x)</code><br /><code>complex gamma(complex z)</code></p>',
            '<div class="legacy reference-signature extra"><strong>Syntax</strong><code>real gamma(real x)</code></div>',
        ]:
            with self.subTest(signature_block=signature_block), TemporaryDirectory() as directory:
                assets = Path(directory) / 'assets'
                assets.mkdir()
                entries = [{'signature': 'gamma(x)', 'title': 'Gamma function', 'description':
                    '<div class="function-reference">' + signature_block +
                    '<p class="reference-summary">Extends factorials.</p>' +
                    '<p class="reference-arguments">Positive real x.</p>' +
                    '<pre><code>gamma(5)</code></pre><p>Result: 24</p></div>'}]
                original = copy.deepcopy(entries)
                output = Path(directory) / 'preview.html'
                with patch.object(generate, 'ASSETS', assets):
                    generate.write_preview(entries, output)
                document = output.read_text(encoding='utf-8')
                text = VisibleText()
                text.feed(document)
                visible = ''.join(text.parts)
                for unwanted in ('reference-signature', 'real gamma(real x)', 'complex gamma(complex z)'):
                    self.assertNotIn(unwanted, document)
                self.assertNotIn('gamma(x)', visible)
                self.assertIn('<h2>Gamma function</h2>', document)
                self.assertIn('data-signature="gamma(x)"', document)
                self.assertIn('Extends factorials.', visible)
                self.assertIn('Positive real x.', visible)
                self.assertIn('<code>gamma(5)</code>', document)
                self.assertIn('Result: 24', visible)
                self.assertEqual(entries, original)

    def test_preview_without_signature_metadata_is_still_rendered(self):
        with TemporaryDirectory() as directory:
            assets = Path(directory) / 'assets'
            assets.mkdir()
            output = Path(directory) / 'preview.html'
            entries = [{'signature': 'Probe(x)', 'title': 'Probe <value>',
                        'description': '<div><p>Meaning.</p><pre><code>Probe(1)</code></pre></div>'}]
            with patch.object(generate, 'ASSETS', assets):
                generate.write_preview(entries, output)
            document = output.read_text(encoding='utf-8')
            self.assertIn('<h2>Probe &lt;value&gt;</h2>', document)
            self.assertIn('<code>Probe(1)</code>', document)
            self.assertIn('Meaning.', document)


if __name__ == '__main__':
    unittest.main()
