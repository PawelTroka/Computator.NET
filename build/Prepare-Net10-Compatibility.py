"""One-time source patch on the experimental branch, removed after review."""
from pathlib import Path
import re
root = Path(__file__).resolve().parent.parent
for p in root.glob('Computator.NET*/**/*.cs'):
    if any(part in ('obj', 'bin') for part in p.parts): continue
    old = p.read_text(encoding='utf-8-sig')
    text = old
    if p.name == 'ScintillaCodeEditorControl.cs':
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
        text = re.sub(r'(=\s*)(?!await\b)(\w+\.Content\.ReadAs(?:String|Stream|ByteArray)Async\([^;]*?\))\s*;', r'\1await \2;', text)
    text = re.sub(r'(?<![\w.])RuntimeInformation\.IsRunningOnMono\b', '(System.Type.GetType("Mono.Runtime") != null)', text)
    text = re.sub(r'(?<![\w.])RuntimeInformation\.Is64BitProcess\b', 'System.Environment.Is64BitProcess', text)
    text = text.replace('Assembly.GetExecutingAssembly().GetName().CodeBase', 'new Uri(Assembly.GetExecutingAssembly().Location).AbsoluteUri')
    text = re.sub(r'^\s*\[(?:System\.Runtime\.ExceptionServices\.)?HandleProcessCorruptedStateExceptions(?:Attribute)?(?:\(\))?\]\s*$', '', text, flags=re.M)
    if text != old:
        p.write_text(text, encoding='utf-8')
        print('Changed:', p.relative_to(root))
# NU1510 confirms these are supplied by the net10 Windows shared frameworks.
# Remove redundant references; use the runtime's serviced assemblies instead.
for p in list(root.glob('Computator.NET*/*.csproj')) + [root/'build/ModernDesktop.props', root/'Directory.Packages.props']:
    if 'Installer' in str(p) or 'UITests' in str(p) or '.net40.' in str(p): continue
    old = p.read_text(encoding='utf-8-sig')
    text = re.sub(r'\s*<Package(?:Reference|Version) Include="(?:System.Formats.Nrbf|System.CodeDom|System.Resources.Extensions)"[^>]*/>', '', old)
    if old != text:
        p.write_text(text, encoding='utf-8')
        print('Framework-supplied package removed:', p.relative_to(root))
