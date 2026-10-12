"""One-time, reviewed source migration on modernize/net10-runtime only.
Removed once the generated source edits and lockfiles are committed.
"""
from pathlib import Path
import re

root = Path(__file__).resolve().parent.parent

def edit(name, transform):
    p = root / name
    original = p.read_text(encoding='utf-8-sig')
    result = transform(original)
    if result != original:
        p.write_text(result, encoding='utf-8')

# Assembly references supplied by the old Framework must become modern packages.
for p in root.glob('Computator.NET*/Computator.NET*.csproj'):
    if 'Installer' in str(p) or 'UITests' in str(p) or p.name.endswith('.net40.csproj'):
        continue
    def project(text):
        text = text.replace('<Reference Include="System.Windows.Forms.DataVisualization" />', '<PackageReference Include="WinForms.DataVisualization" />')
        text = text.replace('<PackageReference Include="jacobslusser.ScintillaNET" />', '<PackageReference Include="Scintilla5.NET" />')
        text = re.sub(r'\s*<Reference Include="Microsoft.CSharp"\s*/>', '', text)
        text = re.sub(r'\s*<PackageReference Include="Microsoft.AspNetCore.WebUtilities"\s*/>', '', text)
        text = re.sub(r'\s*<Reference Include="Microsoft.VisualBasic.PowerPacks.Vs">.*?</Reference>', '', text, flags=re.S)
        if p.parent.name == 'Computator.NET.Core' and 'System.Formats.Nrbf' not in text:
            text = text.replace('<PackageReference Include="Microsoft.CodeAnalysis.CSharp" />', '<PackageReference Include="Microsoft.CodeAnalysis.CSharp" />\n    <PackageReference Include="System.Formats.Nrbf" />')
        return text
    edit(p.relative_to(root), project)

# Preserve the complete existing property-grid API and notifications; only storage changes.
def settings(text):
    text = text.replace('using System.Runtime.Serialization.Formatters.Binary;', '')
    text = text.replace('[Serializable]\n    public class Settings', 'public partial class Settings')
    start = text.index('        public void Save()')
    end = text.index('        public void Reset()', start)
    text = text[:start] + text[end:]
    text = text.replace('public void Reset()\n        {', 'public void Reset()\n        {\n            _canPersist = true;')
    text = text.replace('            if (!File.Exists(AppInformation.SettingsPath))\n                Reset();', '            Reset();')
    return text
edit('Computator.NET.Core/Properties/Settings.cs', settings)

# Replace windowless PowerPacks containers with direct child swatch controls.
# Preserve group-box-relative positions, click handlers and original color values.
for name in ['Computator.NET.Charting/Chart3D/UI/EditChart3DWindow.designer.cs',
             'Computator.NET.Charting/ComplexCharting/EditComplexChartWindow.Designer.cs']:
    def swatches(text):
        for container in set(re.findall(r'this\.(shapeContainer\d+) =', text)):
            pattern = r'this\.' + container + r'\.Shapes.AddRange\(new Microsoft.VisualBasic.PowerPacks.Shape\[\] \{(.*?)\}\);'
            match = re.search(pattern, text, re.S)
            if not match: raise ValueError('Missing shape list: ' + container)
            children = re.findall(r'this\.(\w+)', match.group(1))
            location = re.search(r'this\.' + container + r'\.Location = new System.Drawing.Point\((\d+), (\d+)\);', text)
            dx, dy = map(int, location.groups())
            for child in children:
                expression = r'(this\.' + child + r'\.Location = new System.Drawing.Point\()(-?\d+), (-?\d+)(\);)'
                text = re.sub(expression, lambda m: f'{m[1]}{int(m[2])+dx}, {int(m[3])+dy}{m[4]}', text)
            text = re.sub(r'this\.(\w+)\.Controls.Add\(this\.' + container + r'\);',
                lambda m: '\n            '.join(f'this.{m[1]}.Controls.Add(this.{child});' for child in children), text)
            text = re.sub(pattern, '', text, flags=re.S)
            text = re.sub(r'^.*(?:this\.' + container + r'(?:\.| =)|private Microsoft.VisualBasic.PowerPacks.ShapeContainer ' + container + r';).*\n', '', text, flags=re.M)
        text = text.replace('Microsoft.VisualBasic.PowerPacks.RectangleShape', 'Computator.NET.Charting.Controls.ColorSwatch')
        text = re.sub(r'^.*\.FillStyle = Microsoft.VisualBasic.PowerPacks.FillStyle.Solid;\n', '', text, flags=re.M)
        if 'Microsoft.VisualBasic.PowerPacks' in text: raise ValueError('Unconverted PowerPacks usage')
        return text
    edit(name, swatches)

# Modern .NET no longer implements Assembly.CodeBase, and Process.Start defaults
# to direct process creation instead of shell activation for URLs/documents.
for p in list((root/'Computator.NET.Core').rglob('*.cs')) + list((root/'Computator.NET.Desktop').rglob('*.cs')):
    def runtime(text):
        text = text.replace('Assembly.GetExecutingAssembly().CodeBase', 'Assembly.GetExecutingAssembly().Location')
        text = re.sub(r'Process.Start\((AppRelatedUrls.IssuesUrl|path)\)', r'Process.Start(new ProcessStartInfo(\1) { UseShellExecute = true })', text)
        return text
    edit(p.relative_to(root), runtime)

# Runtime-generated scripts must reference the replacement chart assembly.
edit('Computator.NET.Core/Evaluation/ScriptEvaluator.cs',
     lambda text: text.replace('"System.Windows.Forms.DataVisualization.dll"', 'typeof(System.Windows.Forms.DataVisualization.Charting.Chart).Assembly.Location'))
