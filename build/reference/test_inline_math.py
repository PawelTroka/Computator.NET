"""Inline prose is authored explicitly; source notation and code are never guessed."""
from unittest.mock import patch
import hashlib
import unittest
import xml.etree.ElementTree as ET
from common import A, M, P
from enrichment import Enrichment
import generate


class InlineMathTests(unittest.TestCase):
    def setUp(self):
        self.tex=r'x^\alpha\exp(-x)'
        self.key=generate.inline_identity(self.tex)
        self.asset={'tex':self.tex,'mode':'inline','width':93,'height':22,'depth':225}

    def rendered(self):
        with patch.object(generate,'equation_manifest',return_value={self.key:self.asset}):
            return generate.prose_html(P('The weight ',M(self.tex,'x^α exp(−x)'),
                ' remains part of this sentence, with explanatory words before and after the mathematical expression and its interpretation.'))

    def test_explicit_fragments_preserve_readable_text_and_literal_strings(self):
        value=P('A weight ',M(self.tex,'x^α exp(−x)'),'.')
        self.assertEqual(str('Before. '+value+' After.'),'Before. A weight x^α exp(−x). After.')
        self.assertEqual(generate.prose_html(r'Leave x^2, sin(x), $x$ and \(x\) literal.'),
                         r'Leave x^2, sin(x), $x$ and \(x\) literal.')
        self.assertEqual(generate.prose_html('<img src="remote">'),'&lt;img src=&quot;remote&quot;&gt;')

    def test_inline_identity_cannot_reuse_display_typesetting(self):
        self.assertEqual(self.key,hashlib.sha256(('inline\0'+self.tex).encode()).hexdigest())
        self.assertNotEqual(self.key,hashlib.sha256(self.tex.encode()).hexdigest())

    def test_inline_metadata_is_measured_and_has_no_source_styles(self):
        root=ET.fromstring('<p>'+self.rendered()+'</p>')
        image=root.find('img')
        self.assertEqual(image.attrib,{'class':'reference-math-inline',
            'src':'reference-equations/'+self.key+'.png','alt':'x^α exp(−x)',
            'width':'93','height':'22','data-depth':'225'})
        self.assertEqual(generate.inline_style(image),'width:4.65em;height:1.1em;vertical-align:-0.225em;')
        self.assertTrue(image.tail.startswith(' remains part'))

    def test_code_examples_and_result_text_are_not_interpreted_as_math(self):
        item={'signature':'sin(x)','menu':'real sin(real x)','type':'ElementaryFunctions'}
        article=A('Probe',P('An expression ',M(self.tex,'x^α exp(−x)'),
            ' inside a complete explanation with retained text before and after the formula.'),
            arguments='Use positive x.',example=r'Print("$x^2$ and \(x\)");',category='Trigonometric functions')
        with patch.object(generate,'equation_manifest',return_value={self.key:self.asset}), \
             patch.object(generate.elementary,'describe',return_value=article), \
             patch.object(generate,'enrich',return_value=Enrichment()):
            result=generate.articles({'items':[item],'constants':[]})[0]['description']
        root=ET.fromstring(result)
        self.assertEqual(len(root.findall("./p[@class='reference-summary']/img")),1)
        example=root.find("./p[@class='reference-example']")
        self.assertEqual(len(example.findall('img')),0)
        self.assertIn(article.example,''.join(example.itertext()))
        self.assertEqual(root.find("./p[@class='reference-signature']/code").text,item['menu'])

    def test_missing_alternatives_and_non_prose_parts_fail_during_authoring(self):
        for tex,text in [('', 'x'), ('x', ''), (' ', ' ' )]:
            with self.assertRaises(ValueError):M(tex,text)
        with self.assertRaises(TypeError):P(42)

    def test_invalid_inline_layout_metadata_is_rejected(self):
        fragment='<div><p class="reference-summary">'+self.rendered()+'</p></div>'
        generate.validate_fragment(fragment)
        for key,value in [('data-depth',''),('data-depth','NaN'),('data-depth','1;position:fixed'),
                          ('data-depth','-1001'),('data-depth','1100'),('data-depth','+1'),
                          ('data-depth','01'),('width','321'),('height','101'),
                          ('style','width:100vw'),('class','reference-math-inline extra')]:
            with self.subTest(key=key,value=value):
                root=ET.fromstring(fragment);root.find('.//img').set(key,value)
                with self.assertRaises(ValueError):generate.validate_fragment(ET.tostring(root,encoding='unicode'))

    def test_inline_and_display_asset_modes_must_match(self):
        for altered in [dict(self.asset,mode='display'),dict(self.asset,tex='x')]:
            with patch.object(generate,'equation_manifest',return_value={self.key:altered}):
                with self.assertRaises(ValueError):generate.prose_html(P(M(self.tex,'x^α exp(−x)')))

    def test_reported_laguerre_sentence_has_complete_inline_notation(self):
        item={'signature':'Laguerre(n,α,x)','menu':'real Laguerre(integer n,real α,real x)','type':'SpecialFunctions'}
        article=generate.articles({'items':[item],'constants':[]})[0]
        summary=ET.fromstring(article['description']).find("./p[@class='reference-summary']")
        images=summary.findall('img')
        self.assertEqual([image.get('alt') for image in images],['n','[0,∞)','x^α exp(−x)'])
        weight=generate.equation_manifest()[images[-1].get('src').split('/')[-1][:-4]]
        self.assertEqual(weight['tex'],r'x^\alpha\exp(-x)')
        # MathJax 4 can split inline SVGs at the unary minus. Its first segment
        # alone is only 65px wide, silently dropping '-x)'. Keep an independent
        # lower bound for this reviewed full-expression rendering.
        self.assertGreater(weight['width'],90)
        self.assertEqual(summary.text, 'A degree-')
        self.assertEqual(''.join([summary.text]+[image.get('alt')+(image.tail or '') for image in images]),
                         'A degree-n polynomial orthogonal on [0,∞) with weight x^α exp(−x).')


if __name__=='__main__':unittest.main()
