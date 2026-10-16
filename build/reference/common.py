"""Small authoring helpers; production reads materialized functions.xml only."""
from dataclasses import dataclass

GSL='https://www.gnu.org/software/gsl/doc/html/specfunc.html'
DLMF='https://dlmf.nist.gov/'
MATHNET='https://numerics.mathdotnet.com/api/'
SOURCE='https://github.com/PawelTroka/Computator.NET/blob/master/'
NIST='https://physics.nist.gov/cuu/Constants/'
@dataclass(frozen=True)
class Article:
    title: str
    definition: str
    formula: str=''
    arguments: str=''
    note: str=''
    example: str=''
    url: str=GSL
    category: str=''

def A(title, definition, formula='', arguments='', note='', example='', url=GSL, category=''):
    return Article(title,definition,formula,arguments,note,example,url,category)
