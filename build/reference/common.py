"""Small authoring helpers; production reads materialized functions.xml only."""
from dataclasses import dataclass

GSL='https://www.gnu.org/software/gsl/doc/html/specfunc.html'
DLMF='https://dlmf.nist.gov/'
MATHNET='https://numerics.mathdotnet.com/api/'
SOURCE='https://github.com/PawelTroka/Computator.NET/blob/master/'
NIST='https://physics.nist.gov/cuu/Constants/'

@dataclass(frozen=True)
class InlineMath:
    """An explicitly authored TeX fragment with a readable text alternative."""
    tex: str
    text: str

    def __post_init__(self):
        if not self.tex.strip() or not self.text.strip():
            raise ValueError('Inline mathematics needs TeX and a text alternative.')

@dataclass(frozen=True)
class Prose:
    parts: tuple

    def __str__(self):
        return ''.join(part.text if isinstance(part, InlineMath) else part for part in self.parts)

    def __add__(self, other):
        return P(self, other)

    def __radd__(self, other):
        return P(other, self)

def P(*parts):
    """Compose prose without interpreting ordinary strings or executable code."""
    flattened=[]
    for part in parts:
        if isinstance(part, Prose):flattened.extend(part.parts)
        elif isinstance(part, (str, InlineMath)):flattened.append(part)
        else:raise TypeError('Prose parts must be strings or explicitly authored mathematics.')
    return Prose(tuple(flattened))

def M(tex, text):
    return InlineMath(tex, text)

@dataclass(frozen=True)
class Article:
    title: str
    definition: str | Prose
    formula: str=''
    arguments: str | Prose=''
    note: str | Prose=''
    example: str=''
    url: str=GSL
    category: str=''

def A(title, definition, formula='', arguments='', note='', example='', url=GSL, category=''):
    return Article(title,definition,formula,arguments,note,example,url,category)
