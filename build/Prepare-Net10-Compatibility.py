"""One-time designer metadata migration. No analyzer suppression or test skipping.
Only properties identified by the compiler's WFO1000 diagnostic are edited.
The generated source diff is committed for review; remove this preparer afterward.
"""
from pathlib import Path
import re
import subprocess

root = Path(__file__).resolve().parent.parent
props = root / 'build/ModernDesktop.props'
text = props.read_text(encoding='utf-8-sig')
if 'WindowsAssemblyInfo.cs' not in text:
    text = text.replace('<Compile Include="$(MSBuildThisFileDirectory)..\\SharedAssemblyInfo.cs"',
        '<Compile Include="$(MSBuildThisFileDirectory)WindowsAssemblyInfo.cs" Link="Properties\\WindowsAssemblyInfo.cs" />\n    <Compile Include="$(MSBuildThisFileDirectory)..\\SharedAssemblyInfo.cs"')
    props.write_text(text, encoding='utf-8')

# Designer-owned appearance/behavior values retain their legacy Visible semantics.
visible_files = {
    'Computator.NET.Charting/ComplexCharting/ComplexChart.cs',
    'Computator.NET.Charting/RealCharting/Chart2D.cs',
    'Computator.NET.Desktop/Controls/ScientificNumericUpDown.cs',
    'Computator.NET.Desktop/Controls/ToolStripRadioButtonMenuItem.cs',
    'Computator.NET.Charting/ToolStripRadioButtonMenuItem.cs',
    'Computator.NET.Desktop/Controls/Field.cs',
    'Computator.NET.Desktop/Controls/IconEx.cs',
}
# Dependency-injected views, compiler offsets, document paths and live editor data
# are runtime state, never input to generated designer source/resource files.
hidden_files = {
    'Computator.NET.Core/Compilation/NativeCompiler.cs',
    'Computator.NET.Desktop/Controls/DirectoryTree.cs',
    'Computator.NET.Desktop/Controls/ExpressionTextBox.cs',
    'Computator.NET.Desktop/Controls/DocumentsTabControl.cs',
    'Computator.NET.Desktop/Controls/AutocompleteMenu/WebBrowserTooltip.cs',
}
hidden_names = {'TargetControlWrapper', 'TargetWrapper', 'VisibleItems', 'SelectedItemIndex',
                'HighlightedItemIndex', 'ListView', 'Fragment', 'Host', 'CurrentFileName',
                'Documents', 'DirectoryTree', 'CodeEditorWrapper', 'Label'}
pattern = re.compile(r"^(.*?\.cs)\((\d+),\d+\): error WFO1000: Property '([^']+)'", re.M)

for attempt in range(6):
    build = subprocess.run(['dotnet', 'build', 'Computator.NET.sln', '-c', 'Release', '--no-restore', '-v:minimal'],
        cwd=root, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True, encoding='utf-8', errors='replace')
    (root / f'net10-designer-build-{attempt}.log').write_text(build.stdout, encoding='utf-8')
    found = {}
    for filename, line, name in pattern.findall(build.stdout):
        path = Path(filename).resolve()
        relative = path.relative_to(root).as_posix()
        if relative in visible_files:
            mode = 'Visible'
        elif relative in hidden_files or relative.startswith('Computator.NET.Desktop/Views/') or relative.startswith('Computator.NET.Desktop/Controls/CodeEditors/'):
            mode = 'Hidden'
        elif relative.startswith('Computator.NET.Desktop/Controls/AutocompleteMenu/'):
            mode = 'Visible'
        else:
            raise RuntimeError(f'Review serialization intent manually: {relative}: {name}')
        if name in hidden_names:
            mode = 'Hidden'
        found.setdefault(path, {})[int(line)] = (name, mode)
    if not found:
        print(f'No remaining WFO1000 diagnostics. Build exit code: {build.returncode}')
        break
    for path, entries in found.items():
        lines = path.read_text(encoding='utf-8-sig').splitlines(keepends=True)
        for line, (name, mode) in sorted(entries.items(), reverse=True):
            declaration = lines[line - 1]
            if not re.search(r'\b' + re.escape(name) + r'\b', declaration):
                raise RuntimeError(f'Source no longer matches diagnostic: {path}:{line}')
            indent = re.match(r'\s*', declaration)[0]
            attribute = f'{indent}[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.{mode})]\n'
            lines.insert(line - 1, attribute)
            print(f'{path.relative_to(root)}:{name} -> {mode}')
        path.write_text(''.join(lines), encoding='utf-8')
else:
    raise RuntimeError('Designer metadata migration did not converge; review build diagnostics.')
