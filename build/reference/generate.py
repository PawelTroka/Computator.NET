#!/usr/bin/env python3
"""Materialize reviewed reference prose for the runtime-discovered autocomplete catalog.

Production reads functions.xml; this is an explicit editorial build/check, not a
runtime fallback, web scraper or source rewrite during application builds.
"""
from __future__ import annotations
import argparse
from collections import Counter, defaultdict
import hashlib
import html
import json
from pathlib import Path
import re
import xml.etree.ElementTree as ET
from urllib.parse import urlparse
import elementary, physical, mathematical, special, scientific, distributions
from enrichment import enrich
from common import InlineMath, Prose
from functools import lru_cache

ASSETS=Path(__file__).resolve().parents[2]/"Computator.NET.Core/Static/reference-equations"
EQUATIONS={}
COLLECT_EQUATIONS=False
@lru_cache(maxsize=1)
def equation_manifest():
    return json.loads((ASSETS/"manifest.json").read_text(encoding="utf-8"))["equations"]

def equation_html(equation):
    key=hashlib.sha256(equation.tex.encode("utf-8")).hexdigest()
    EQUATIONS[key]=equation.tex
    if COLLECT_EQUATIONS:return ""
    asset=equation_manifest()[key]
    if asset["tex"] != equation.tex:raise ValueError("Equation source mismatch: "+key)
    return ('<p class="reference-equation"><strong>'+esc(equation.label)+'</strong><br />'
        '<img class="reference-math" src="reference-equations/'+key+'.png" alt="'+esc(equation.text)+'" width="'+str(asset["width"])+'" height="'+str(asset["height"])+'" /></p>')

def inline_identity(tex):
    # A TeX expression has different size and limits in inline/display modes.
    # Keep existing display identities byte-for-byte stable.
    return hashlib.sha256(('inline\0'+tex).encode('utf-8')).hexdigest()

def prose_html(value):
    if not isinstance(value, Prose):return esc(value)
    chunks=[]
    for part in value.parts:
        if not isinstance(part, InlineMath):
            chunks.append(esc(part))
            continue
        key=inline_identity(part.tex)
        EQUATIONS[key]={'tex':part.tex,'mode':'inline'}
        if COLLECT_EQUATIONS:continue
        asset=equation_manifest()[key]
        if asset['tex']!=part.tex or asset.get('mode')!='inline':raise ValueError('Inline source mismatch: '+key)
        chunks.append('<img class="reference-math-inline" src="reference-equations/'+key+'.png" alt="'+esc(part.text)+
            '" width="'+str(asset['width'])+'" height="'+str(asset['height'])+'" data-depth="'+str(asset['depth'])+'" />')
    return ''.join(chunks)

def inline_style(image):
    # Only generated, validated integer metadata reaches these numeric styles.
    def em(value):return format(value,'.3f').rstrip('0').rstrip('.')+'em'
    return 'width:'+em(int(image.get('width'))/20)+';height:'+em(int(image.get('height'))/20)+';vertical-align:'+em(-int(image.get('data-depth'))/1000)+';'

MAPPINGS={'ElementaryFunctions':elementary,'PhysicalConstants':physical,
          'MathematicalConstants':mathematical,'SpecialFunctions':special}
SCIENCE={'MatrixFunctions','ScriptingFunctions','StatisticsFunctions','Integral','Derivative',
         'FunctionRoot','MathematicalTransformations'}
ALLOWED_TAGS={'div','p','strong','b','code','pre','br','a','ul','li','sup','sub','img'}
SECTIONS=(('formula','Definition / convention'),('arguments','Arguments / value'),
          ('note','Important'),('example','Example'))
def esc(value): return html.escape(value, quote=True)
def module_for(kind):
    if kind in MAPPINGS:return MAPPINGS[kind]
    if kind in SCIENCE:return scientific
    if kind in distributions.FAMILIES:return distributions
    raise ValueError('No editorial module for runtime category: '+kind)

def articles(inventory):
    values={entry['name']:entry['value'] for entry in inventory['constants']}
    groups=defaultdict(list)
    for item in inventory['items']:
        if item['type'] != 'Keyword': groups[item['signature']].append(item)
    if not groups:raise ValueError('Runtime inventory is empty.')
    result=[]
    for signature, variants in sorted(groups.items()):
        kinds={item['type'] for item in variants}
        if len(kinds)!=1:raise ValueError('Conflicting categories for '+signature)
        kind=next(iter(kinds)); authored=[]
        for item in sorted(variants,key=lambda value:value['menu']):
            article=module_for(kind).describe(item,values)
            category=article.category
            if (not category or category!=category.strip()
                    or category.replace(' ','').casefold()==kind.casefold()):
                raise ValueError('Missing or generic subject category for '+signature)
            if article not in [entry[1] for entry in authored]:authored.append((item['menu'],article))
        menus=sorted({item['menu'] for item in variants})
        title=authored[0][1].title
        categories={article.category for _,article in authored}
        if len(categories)!=1:raise ValueError('Conflicting subject categories for '+signature)
        category=next(iter(categories))
        url=authored[0][1].url
        # Keep overload metadata for catalogue validation. Display renderers and
        # the editorial preview remove this block: signatures live in autocomplete.
        chunks=['<div class="function-reference">',
                '<p class="reference-signature">'+'<br />'.join('<code>'+esc(menu)+'</code>' for menu in menus)+'</p>']
        for menu,article in authored:
            selected=next(item for item in variants if item['menu']==menu)
            extra=enrich(selected,article,values)
            # Do not repeat a typed signature in a second section heading.
            if len(authored)>1:chunks.append('<p class="reference-variant"><strong>'+esc(article.title)+'</strong></p>')
            chunks.append('<p class="reference-summary">'+prose_html(article.definition)+'</p>')
            for field,label in SECTIONS:
                # Introduce the function, then give its definition together with
                # the applicable arguments and implementation caveats. Longer
                # teaching notes follow this quick reference. Supplementary
                # family equations still follow those notes; they must not be
                # promoted into definitions of an individual overload.
                if field=='example':
                    for paragraph in extra.paragraphs:
                        chunks.append('<p class="reference-explanation">'+prose_html(paragraph)+'</p>')
                value=getattr(article,field)
                if field=='formula' and extra.replaces_formula:
                    for equation in extra.equations:chunks.append(equation_html(equation))
                    continue
                if field=='example' and any(value.startswith(ex.code+' ') or value.startswith(ex.code+'=') for ex in extra.examples):
                    continue
                if value:
                    value='<code>'+esc(value)+'</code>' if field=='formula' else prose_html(value)
                    chunks.append('<p class="reference-'+field+'"><strong>'+label+':</strong> '+value+'</p>')
            if not extra.replaces_formula:
                for equation in extra.equations:chunks.append(equation_html(equation))
            for example in extra.examples:
                chunks.append('<div class="reference-worked-example"><p><strong>Worked example</strong></p><pre><code>'+esc(example.code)+'</code></pre>'+
                    '<p><strong>Result:</strong> '+esc(example.result)+'</p><p>'+prose_html(example.explanation)+'</p></div>')
            if urlparse(article.url).scheme!='https':raise ValueError('Reference must be HTTPS: '+signature)
        sources=list(dict.fromkeys(article.url for _,article in authored))
        chunks.append('<p class="reference-source"><strong>Reference:</strong> '+
                      ' · '.join('<a href="'+esc(link)+'">'+esc(urlparse(link).hostname or 'Source')+'</a>' for link in sources)+'</p>')
        chunks.append('</div>')
        fragment='\n'.join(chunks)
        validate_fragment(fragment,signature)
        result.append(dict(signature=signature,title=title,type=kind,category=category,description=fragment,url=url))
    return result

def validate_fragment(fragment,signature='fragment'):
    root=ET.fromstring(fragment)
    for node in root.iter():
        if node.tag not in ALLOWED_TAGS:raise ValueError('Unsupported markup in '+signature)
        allowed=('class','src','alt','width','height','data-depth') if node.tag=='img' else ('class','href')
        if any(name not in allowed for name in node.attrib):raise ValueError('Unsupported attribute in '+signature)
        if node.tag=='img':
            src=node.get('src','')
            if not re.fullmatch(r'reference-equations/[a-f0-9]{64}\.png',src):raise ValueError('Nonlocal equation image: '+signature)
            inline=node.get('class')=='reference-math-inline'
            if node.get('class') not in ('reference-math','reference-math-inline') or not node.get('alt','').strip():raise ValueError('Missing equation alternative: '+signature)
            for dimension,maximum in [('width',320 if inline else 2400),('height',100 if inline else 800)]:
                if not re.fullmatch(r'[1-9][0-9]{0,3}',node.get(dimension,'')) or int(node.get(dimension))>maximum:raise ValueError('Invalid image dimensions: '+signature)
            if inline:
                depth=node.get('data-depth','')
                if not re.fullmatch(r'0|-?[1-9][0-9]{0,3}',depth) or not -1000<=int(depth)<int(node.get('height'))*50:
                    raise ValueError('Invalid inline baseline: '+signature)
            elif 'data-depth' in node.attrib:raise ValueError('Display equations do not have inline metadata: '+signature)
        if node.tag=='a' and urlparse(node.get('href','')).scheme!='https':raise ValueError('Unsafe link in '+signature)
    text=' '.join(root.itertext())+' '+' '.join(image.get('alt','') for image in root.iter('img'))
    if len(text.split())<20:raise ValueError('Insufficient descriptive content: '+signature)
    if re.search(r'not done yet|_title_|TODO|TBD|lorem ipsum',text,re.I):raise ValueError('Placeholder: '+signature)
    if not root.findall("./p[@class='reference-summary']"):raise ValueError('Missing summary: '+signature)

def xml_bytes(entries):
    parts=['<?xml version="1.0" encoding="utf-8"?>',
           '<!-- Generated from reviewed build/reference/*.py and the actual runtime catalog. -->',
           '<!-- Regenerate explicitly through Cake Reference-Generate; normal tests only verify. -->',
           '<FunctionsDetails>']
    for item in entries:
        parts.append('  <FunctionInfo Signature="'+esc(item['signature'])+'">')
        for field in ('title','type','category'):
            parts.append('    <'+field.capitalize()+'>'+esc(item[field])+'</'+field.capitalize()+'>')
        parts.append('    <Description><![CDATA['+item['description'].replace(']]>',']]]]><![CDATA[>')+']]></Description>')
        parts.append('    <Url>'+esc(item['url'])+'</Url>')
        parts.append('  </FunctionInfo>')
    parts.append('</FunctionsDetails>')
    return ('\n'.join(parts)+'\n').encode('utf-8')

def write_preview(entries,path):
    sections=[]
    for index,item in enumerate(entries):
        # Match the desktop/web article presentation without mutating the source
        # metadata used by catalogue coverage checks. Never strip example code.
        fragment=ET.fromstring(item['description'])
        for parent in fragment.iter():
            for child in list(parent):
                if 'reference-signature' in child.get('class','').split():
                    parent.remove(child)
        for parent in list(fragment.iter()):
            for child_index,image in enumerate(list(parent)):
                if image.tag!='img' or image.get('class')!='reference-math-inline':continue
                image.set('style',inline_style(image))
                image.set('aria-hidden','true')
                alternative=image.get('alt');image.set('alt','')
                wrapper=ET.Element('span',{'class':'reference-inline'})
                wrapper.tail=image.tail;image.tail=None
                parent.remove(image);parent.insert(child_index,wrapper)
                wrapper.append(image)
                ET.SubElement(wrapper,'span',{'class':'reference-math-text'}).text=alternative
        description=ET.tostring(fragment,encoding='unicode')
        sections.append('<section id="entry-'+str(index)+'" data-signature="'+esc(item['signature'])+'"><h2>'+esc(item['title'])+'</h2>'+description+'</section>')
    text='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>Computator.NET function reference</title><style>body{font:16px/1.5 system-ui,sans-serif;margin:0 auto;padding:2rem;max-width:64rem;color:#24262b;background:#fafafa}h1{font-size:2rem}h2{font-size:1.25rem}section{padding:1.2rem;margin:1.2rem 0;border:1px solid #ddd;border-radius:8px;background:white}code{font-family:Consolas,monospace;overflow-wrap:anywhere}.reference-note{border-left:3px solid #b07934;padding-left:.8rem}.reference-source{font-size:.88rem}a{color:#305c85}p{margin:.7rem 0}</style><h1>Computator.NET function and constant reference</h1><p>Offline editorial preview. The application uses the same HTML fragments from functions.xml. No network resources are required; source links are optional reading.</p>'''+''.join(sections)+'</html>'
    text=text.replace('</style>', '.reference-equation{overflow-x:auto;padding:12px;background:white}.reference-math{display:block;max-width:none}.reference-math-inline{display:inline-block;max-width:none}.reference-math-text{position:absolute;width:1px;height:1px;padding:0;margin:-1px;overflow:hidden;clip:rect(0,0,0,0);white-space:nowrap;border:0}.reference-worked-example{border-left:3px solid #718595;padding:0 16px}pre{white-space:pre-wrap;overflow-wrap:anywhere}</style>')
    path.parent.mkdir(parents=True,exist_ok=True);path.write_text(text,encoding='utf-8')
    import shutil
    shutil.copytree(ASSETS,path.parent/'reference-equations',dirs_exist_ok=True)

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--inventory',type=Path,required=True)
    parser.add_argument('--equations-only',type=Path)
    parser.add_argument('--output',type=Path)
    parser.add_argument('--check',action='store_true')
    parser.add_argument('--preview',type=Path)
    parser.add_argument('--report',type=Path)
    args=parser.parse_args()
    inventory=json.loads(args.inventory.read_text(encoding='utf-8-sig'))
    global COLLECT_EQUATIONS
    COLLECT_EQUATIONS=args.equations_only is not None
    entries=articles(inventory)
    if args.equations_only:
        args.equations_only.parent.mkdir(parents=True,exist_ok=True)
        args.equations_only.write_text(json.dumps(EQUATIONS,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
        return
    if args.output is None:parser.error('--output is required unless collecting equations')
    data=xml_bytes(entries)
    if args.check:
        if args.output.read_bytes()!=data:raise SystemExit('Reference XML differs from the reviewed authoring source/runtime inventory. Run Reference-Generate and review the diff.')
    else:
        args.output.parent.mkdir(parents=True,exist_ok=True);args.output.write_bytes(data)
    if args.preview:write_preview(entries,args.preview)
    report={'entries':len(entries),'completionItems':len(inventory['items']),
            'excludedKeywords':sum(i['type']=='Keyword' for i in inventory['items']),
            'byType':dict(sorted(Counter(i['type'] for i in entries).items())),
            'xmlSha256':hashlib.sha256(data).hexdigest(),
            'entriesWithTypesetMath':sum('reference-math' in e['description'] for e in entries),
            'entriesWithInlineMath':sum('reference-math-inline' in e['description'] for e in entries),
            'entriesWithTeachingNotes':sum('reference-explanation' in e['description'] for e in entries),
            'entriesWithWorkedExamples':sum('reference-worked-example' in e['description'] for e in entries),
            'uniqueTypesetEquations':len(EQUATIONS)}
    if args.report:
        args.report.parent.mkdir(parents=True,exist_ok=True);args.report.write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(report,ensure_ascii=False,indent=2))
if __name__=='__main__':main()
