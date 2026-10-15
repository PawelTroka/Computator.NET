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

MAPPINGS={'ElementaryFunctions':elementary,'PhysicalConstants':physical,
          'MathematicalConstants':mathematical,'SpecialFunctions':special}
SCIENCE={'MatrixFunctions','ScriptingFunctions','StatisticsFunctions','Integral','Derivative',
         'FunctionRoot','MathematicalTransformations'}
ALLOWED_TAGS={'div','p','strong','b','code','pre','br','a','ul','li','sup','sub'}
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
            if article not in [entry[1] for entry in authored]:authored.append((item['menu'],article))
        menus=sorted({item['menu'] for item in variants})
        title=authored[0][1].title
        category=authored[0][1].category or kind
        url=authored[0][1].url
        chunks=['<div class="function-reference">',
                '<p class="reference-signature"><strong>Syntax</strong><br />'+'<br />'.join('<code>'+esc(menu)+'</code>' for menu in menus)+'</p>']
        for menu,article in authored:
            if len(authored)>1:chunks.append('<p><strong>'+esc(menu)+'</strong></p>')
            chunks.append('<p class="reference-summary">'+esc(article.definition)+'</p>')
            for field,label in SECTIONS:
                value=getattr(article,field)
                if value:
                    value='<code>'+esc(value)+'</code>' if field=='formula' else esc(value)
                    chunks.append('<p class="reference-'+field+'"><strong>'+label+':</strong> '+value+'</p>')
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
        if any(name not in ('class','href') for name in node.attrib):raise ValueError('Unsupported attribute in '+signature)
        if node.tag=='a' and urlparse(node.get('href','')).scheme!='https':raise ValueError('Unsafe link in '+signature)
    text=' '.join(root.itertext())
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
        sections.append('<section id="entry-'+str(index)+'"><h2>'+esc(item['signature'])+' — '+esc(item['title'])+'</h2>'+item['description']+'</section>')
    text='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>Computator.NET function reference</title><style>body{font:16px/1.5 system-ui,sans-serif;margin:0 auto;padding:2rem;max-width:64rem;color:#24262b;background:#fafafa}h1{font-size:2rem}h2{font-size:1.25rem}section{padding:1.2rem;margin:1.2rem 0;border:1px solid #ddd;border-radius:8px;background:white}code{font-family:Consolas,monospace;overflow-wrap:anywhere}.reference-note{border-left:3px solid #b07934;padding-left:.8rem}.reference-signature{color:#454958}.reference-source{font-size:.88rem}a{color:#305c85}p{margin:.7rem 0}</style><h1>Computator.NET function and constant reference</h1><p>Offline editorial preview. The application uses the same HTML fragments from functions.xml. No network resources are required; source links are optional reading.</p>'''+''.join(sections)+'</html>'
    path.parent.mkdir(parents=True,exist_ok=True);path.write_text(text,encoding='utf-8')

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--inventory',type=Path,required=True)
    parser.add_argument('--output',type=Path,required=True)
    parser.add_argument('--check',action='store_true')
    parser.add_argument('--preview',type=Path)
    parser.add_argument('--report',type=Path)
    args=parser.parse_args()
    inventory=json.loads(args.inventory.read_text(encoding='utf-8-sig'))
    entries=articles(inventory);data=xml_bytes(entries)
    if args.check:
        if args.output.read_bytes()!=data:raise SystemExit('Reference XML differs from the reviewed authoring source/runtime inventory. Run Reference-Generate and review the diff.')
    else:
        args.output.parent.mkdir(parents=True,exist_ok=True);args.output.write_bytes(data)
    if args.preview:write_preview(entries,args.preview)
    report={'entries':len(entries),'completionItems':len(inventory['items']),
            'excludedKeywords':sum(i['type']=='Keyword' for i in inventory['items']),
            'byType':dict(sorted(Counter(i['type'] for i in entries).items())),
            'xmlSha256':hashlib.sha256(data).hexdigest()}
    if args.report:
        args.report.parent.mkdir(parents=True,exist_ok=True);args.report.write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(report,ensure_ascii=False,indent=2))
if __name__=='__main__':main()
