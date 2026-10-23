#!/usr/bin/env python3
"""Capture actual historical/current generated programs, then attribute modern compile phases.

Run after performance measurements. This creates isolated diagnostic projects and
copies the authenticated payloads; it does not rebuild or modify the products or
Worker.cs. The resulting phase timings are attribution diagnostics only.
"""
from __future__ import annotations
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import shutil
import sys
from xml.sax.saxutils import escape


def sha(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open('rb') as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b''):
            digest.update(block)
    return digest.hexdigest()


def require(condition: bool, message: str) -> None:
    if not condition:
        raise ValueError(message)


def write_json(path: Path, data) -> None:
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--input', required=True, type=Path, help='Prepared/authenticated comparison input directory')
    parser.add_argument('--output', required=True, type=Path, help='Absent or empty isolated output directory')
    parser.add_argument('--dotnet', default='dotnet', help='SDK executable; repository global.json is retained')
    args = parser.parse_args()
    require(os.name == 'nt', 'This diagnostic requires the Windows comparison host.')
    input_root, output = args.input.resolve(), args.output.resolve()
    require(output != input_root and input_root not in output.parents, 'Keep diagnostic output outside authenticated input.')
    require(not output.exists() or (output.is_dir() and not any(output.iterdir())), 'Diagnostic output must be absent or empty.')
    scripts = Path(__file__).resolve().parent
    manifest = json.loads((input_root / 'manifest.json').read_text(encoding='utf-8-sig'))
    supervisor_path = scripts / 'compare.py'
    spec = importlib.util.spec_from_file_location('performance_supervisor', supervisor_path)
    supervisor = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = supervisor
    spec.loader.exec_module(supervisor)
    environment, controls = supervisor.controlled_environment({'environment': {}}, False)
    dotnet = shutil.which(args.dotnet)
    require(dotnet is not None, 'SDK executable was not found.')
    source_template = scripts / 'CompilationDiagnostics.cs.txt'
    require(source_template.is_file(), 'Diagnostic source template is missing.')

    def verify() -> None:
        for version in ('baseline', 'candidate'):
            for artifact in manifest[version]['artifacts']:
                require(sha(Path(artifact['path'])) == artifact['sha256'], 'Authenticated input changed: ' + artifact['path'])

    verify()
    output.mkdir(parents=True, exist_ok=True)
    (output / 'Directory.Build.props').write_text('<Project />\n', encoding='utf-8')
    (output / 'Directory.Build.targets').write_text('<Project />\n', encoding='utf-8')
    (output / 'NuGet.Config').write_text('<configuration><packageSources><clear/><add key="nuget.org" value="https://api.nuget.org/v3/index.json"/></packageSources></configuration>\n', encoding='utf-8')
    shutil.copyfile(scripts.parent.parent / 'global.json', output / 'global.json')
    shutil.copyfile(input_root / 'manifest.json', output / 'input-manifest.json')
    shutil.copyfile(source_template, output / 'CompilationDiagnostics.cs.txt')
    logs = output / 'logs'
    logs.mkdir()
    records = {'schema': 1, 'scope': 'Separate source/phase attribution diagnostic; no cross-version timing inference.',
               'supervisorSha256': sha(supervisor_path), 'sourceTemplateSha256': sha(source_template),
               'environmentControls': controls, 'captures': {}, 'phases': {}, 'hosts': {}}

    def invoke(name: str, command: list[str], cwd: Path, extra=None, timeout=180, worker=False):
        child_environment = dict(environment)
        child_environment.update(extra or {})
        result = supervisor.run_child(command, str(cwd), child_environment, timeout)
        result['environmentOverrides'] = {**controls, **(extra or {})}
        for stream in ('stdout', 'stderr'):
            (logs / (name + '.' + stream + '.txt')).write_text(result[stream], encoding='utf-8')
        write_json(logs / (name + '.json'), result)
        fatal_errors = [error for error in result['errors'] if not error.startswith('Worker stdout must be one JSON record:')]
        require(not fatal_errors and not result.get('unsafeToContinue') and not result['timedOut'] and result['exitCode'] == 0,
                'Diagnostic command failed; preserve logs: ' + name)
        if worker:
            record = result.get('record')
            require(isinstance(record, dict) and record.get('correctness') is True and record.get('bitness') == 64,
                    'Diagnostic correctness/identity failed: ' + name)
            return record
        return result['stdout']

    records['dotnetInfo'] = invoke('dotnet-info', [dotnet, '--info'], output)
    for version, framework in (('baseline', 'net461'), ('candidate', 'net10.0-windows')):
        payload = Path(manifest[version]['cwd'])
        version_output = output / version
        version_output.mkdir()
        project_dir = version_output / 'project'
        project_dir.mkdir()
        shutil.copyfile(source_template, project_dir / 'CompilationDiagnostics.cs')
        references = [payload / 'Computator.NET.Core.dll', payload / 'Computator.NET.DataTypes.dll']
        if version == 'candidate':
            references += [payload / 'Computator.NET.Engine.dll', payload / 'Microsoft.CodeAnalysis.dll', payload / 'Microsoft.CodeAnalysis.CSharp.dll']
        for reference in references:
            require(reference.is_file(), 'Required actual product reference missing: ' + str(reference))
        items = ''.join('<Reference Include="' + escape(reference.stem) + '"><HintPath>' + escape(str(reference)) + '</HintPath><Private>false</Private></Reference>' for reference in references)
        if version == 'baseline':
            items += '<Reference Include="System.Numerics"/><Reference Include="Microsoft.CSharp"/>'
        properties = '<DefineConstants>MODERN_COMPILER</DefineConstants>' if version == 'candidate' else ''
        project = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>' + framework + '</TargetFramework><OutputType>Exe</OutputType><AssemblyName>Compilation.Diagnostics</AssemblyName><LangVersion>7.3</LangVersion><Nullable>disable</Nullable><ImplicitUsings>disable</ImplicitUsings><UseWindowsForms>true</UseWindowsForms><UseWPF>true</UseWPF><EnableWindowsTargeting>true</EnableWindowsTargeting><PlatformTarget>x64</PlatformTarget><Prefer32Bit>false</Prefer32Bit><Optimize>true</Optimize><DebugType>portable</DebugType><EnableDefaultCompileItems>false</EnableDefaultCompileItems><RestorePackagesWithLockFile>false</RestorePackagesWithLockFile><NuGetAudit>false</NuGetAudit>' + properties + '</PropertyGroup><ItemGroup><Compile Include="CompilationDiagnostics.cs"/>' + items + '</ItemGroup></Project>\n'
        project_path = project_dir / 'Diagnostic.csproj'
        project_path.write_text(project, encoding='utf-8')
        host = version_output / 'host'
        invoke(version + '-build', [dotnet, 'build', str(project_path), '-c', 'Release', '-o', str(host), '--nologo', '-v:minimal', '-m:1', '-nr:false', '-p:UseSharedCompilation=false'], output)
        # Restore every shipped/product dependency exactly. Only the independently
        # named diagnostic host is new; the original common Worker remains intact.
        shutil.copytree(payload, host, dirs_exist_ok=True)
        if version == 'baseline':
            shutil.copyfile(payload / 'Computator.Performance.Worker.exe.config', host / 'Compilation.Diagnostics.exe.config')
        else:
            for extension in ('deps.json', 'runtimeconfig.json'):
                shutil.copyfile(payload / ('Computator.Performance.Worker.' + extension), host / ('Compilation.Diagnostics.' + extension))
        for reference in references:
            require(sha(host / reference.name) == sha(reference), 'Diagnostic payload dependency changed.')
        host_files = [file for file in host.glob('Compilation.Diagnostics.*') if file.is_file()]
        records['hosts'][version] = {'projectSha256': sha(project_path),
            'artifacts': [{'path': str(file), 'sha256': sha(file)} for file in host_files + [host / reference.name for reference in references]]}
        command = [str(host / 'Compilation.Diagnostics.exe')]
        extra = manifest[version].get('environment', {})
        records['captures'][version] = {}
        for scenario in ('real', 'script'):
            capture_dir = version_output / (scenario + '-source')
            capture_dir.mkdir()
            record = invoke(version + '-' + scenario + '-capture', command + ['capture', scenario, str(capture_dir)], host, extra, worker=True)
            expected = manifest[version]['expected']
            require(record['coreVersion'] == expected['coreVersion'] and record['coreSha256'] == sha(payload / 'Computator.NET.Core.dll'), 'Captured product identity differs.')
            if 'runtime' in expected:
                require(record['runtime'] == expected['runtime'], 'Captured runtime differs.')
            if 'runtimePrefix' in expected:
                require(record['runtime'].startswith(expected['runtimePrefix']), 'Captured runtime prefix differs.')
            require(record['fullSource']['sha256'] == sha(capture_dir / 'fullsource.cs.txt'), 'Captured source bytes differ.')
            records['captures'][version][scenario] = record
            if version == 'candidate':
                # A fresh child starts after capture; all compiler phases and its
                # first-native-compile costs are measured in this fresh process.
                phase = invoke('candidate-' + scenario + '-phases', command + ['phases', scenario, str(capture_dir / 'fullsource.cs.txt')], host, extra, timeout=240, worker=True)
                require(all(phase[key] == record[key] for key in ('coreVersion', 'coreSha256', 'runtime', 'bitness')), 'Phase process identity differs from its capture.')
                require(phase['fullSource']['sha256'] == record['fullSource']['sha256'], 'Phase source differs from the public evaluator capture.')
                require(len(phase['phases']) == 33 and all(row['correctness'] for row in phase['phases']), 'Incomplete phase trace.')
                records['phases'][scenario] = phase
            write_json(output / 'compilation-diagnostics.json', records)
        verify()
    records['candidateAndHistoricalArtifactsVerifiedBeforeAndAfter'] = True
    write_json(output / 'compilation-diagnostics.json', records)
    print('Captured exact generated sources and 33 native compile phase observations:', output)
    return 0


if __name__ == '__main__':
    try:
        raise SystemExit(main())
    except (OSError, ValueError, KeyError, TypeError) as error:
        print('Compilation diagnostic failed:', error, file=sys.stderr)
        raise SystemExit(2)
