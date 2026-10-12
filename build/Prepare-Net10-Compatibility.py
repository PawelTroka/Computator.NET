"""One-time source patch; ordinary build/test workflows never modify source.
The generated diff is committed only to the experimental branch for review.
"""
from pathlib import Path
import re
root = Path(__file__).resolve().parent.parent
for p in root.glob('Computator.NET*/**/*.cs'):
    if any(part in ('obj', 'bin') for part in p.parts):
        continue
    old = p.read_text(encoding='utf-8-sig')
    text = old
    if p.name == 'ScintillaCodeEditorControl.cs':
        # Scintilla 5 uses independently versioned Scintilla/Lexilla libraries
        # supplied by its NuGet package, not the old embedded SciLexer binary.
        match = re.search(r'\bstatic\s+ScintillaCodeEditorControl\s*\(\s*\)\s*\{', text)
        if match:
            pos, depth = match.end(), 1
            while depth and pos < len(text):
                depth += (text[pos] == '{') - (text[pos] == '}')
                pos += 1
            if depth: raise ValueError('Unbalanced static constructor')
            text = text[:match.start()] + text[pos:]
        text = re.sub(r'\bLexer\s*=\s*Lexer\.Cpp\s*;', 'LexerName = "cpp";', text)
        text = re.sub(r'\bLexerLanguage\s*=\s*"cs"\s*;', 'LexerName = "cpp";', text)
        if 'SetModulePath(' in text: raise ValueError('Unconverted Scintilla module loader')
    if 'WebApi.IntegrationTests' in str(p):
        # These APIs now return Tasks; await the content before inspecting it.
        text = re.sub(r'(=\s*)(?!await\b)(\w+\.Content\.ReadAs(?:String|Stream|ByteArray)Async\([^;]*?\))\s*;', r'\1await \2;', text)
    text = re.sub(r'(?<![\w.])RuntimeInformation\.IsRunningOnMono\b', '(System.Type.GetType("Mono.Runtime") != null)', text)
    text = re.sub(r'(?<![\w.])RuntimeInformation\.Is64BitProcess\b', 'System.Environment.Is64BitProcess', text)
    text = text.replace('Assembly.GetExecutingAssembly().GetName().CodeBase', 'new Uri(Assembly.GetExecutingAssembly().Location).AbsoluteUri')
    text = re.sub(r'^\s*\[(?:System\.Runtime\.ExceptionServices\.)?HandleProcessCorruptedStateExceptions(?:Attribute)?(?:\(\))?\]\s*$', '', text, flags=re.M)
    if text != old:
        p.write_text(text, encoding='utf-8')
        print('Changed:', p.relative_to(root))
p = root / 'Directory.Packages.props'
text = p.read_text(encoding='utf-8-sig')
p.write_text(text.replace('Include="System.Formats.Nrbf" Version="10.0.12"', 'Include="System.Formats.Nrbf" Version="10.0.13"'), encoding='utf-8')
