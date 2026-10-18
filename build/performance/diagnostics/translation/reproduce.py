#!/usr/bin/env python3
"""Reproduce the isolated .NET 10 Linux translation guard diagnostic.

All generated projects, copied sources, binaries and observations are written to
--output. The source checkout is read-only. This is not a desktop, startup or
historical-version benchmark. See README.md and protocol.json beside this file.
"""
from __future__ import annotations
import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import shutil
import subprocess
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


def run(command: list[str], cwd: Path) -> str:
    result = subprocess.run(command, cwd=cwd, text=True, stdout=subprocess.PIPE,
                            stderr=subprocess.STDOUT, check=False)
    print(result.stdout, end='', flush=True)
    require(result.returncode == 0, f'Command failed ({result.returncode}): {command}')
    return result.stdout


def artifact(path: Path, role: str | None = None) -> dict:
    item = {'path': str(path.resolve()), 'sha256': sha(path)}
    if role:
        item['role'] = role
    return item


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--repo', type=Path, required=True, help='Clean baseline checkout; recommended commit is recorded in protocol.json')
    parser.add_argument('--dotnet', type=Path, required=True, help='Explicit dotnet executable for SDK 10.0.402 / runtime 10.0.13')
    parser.add_argument('--packages', type=Path, required=True, help='NuGet cache containing Roslyn Common and CSharp 5.9.0 net10.0 DLLs')
    parser.add_argument('--output', type=Path, required=True, help='Absent or empty isolated output directory; never a source checkout')
    parser.add_argument('--pairs', type=int, default=20, help='20 by default; fewer than 15 is exploratory only')
    parser.add_argument('--prepare-only', action='store_true')
    args = parser.parse_args()
    require(platform.system() == 'Linux' and platform.machine().lower() in ('x86_64', 'amd64'), 'This retained diagnostic requires Linux x64.')
    require(args.pairs > 0, '--pairs must be positive')
    evidence = Path(__file__).resolve().parent
    protocol = json.loads((evidence / 'protocol.json').read_text(encoding='utf-8'))
    repo, dotnet, packages, output = (path.resolve() for path in (args.repo, args.dotnet, args.packages, args.output))
    require(dotnet.is_file() and repo.is_dir(), 'Missing SDK executable or repository')
    require(not output.exists() or (output.is_dir() and not any(output.iterdir())), '--output must be absent or empty; previous observations are never overwritten.')
    require(output != repo and output not in repo.parents, '--output cannot contain the source checkout')
    if repo in output.parents:
        ignored = subprocess.run(['git', '-C', str(repo), 'check-ignore', '--quiet', '--', str(output.relative_to(repo)) + '/'], check=False)
        require(ignored.returncode == 0, 'An output inside the repository must be ignored by Git.')
    commit = subprocess.check_output(['git', '-C', str(repo), 'rev-parse', 'HEAD'], text=True).strip()
    status = subprocess.check_output(['git', '-C', str(repo), 'status', '--porcelain=v1', '--untracked-files=no'], text=True)
    require(not status.strip(), 'Use a clean baseline checkout; the source hashes are verified below.')
    for source in protocol['sourceFiles']:
        require(sha(repo / source['repositoryPath']) == source['sha256'], 'Source differs: ' + source['repositoryPath'] + '; use the recorded baseline checkout.')
    for name, expected in protocol['inputs'].items():
        path = evidence / name
        require(path.is_file() and sha(path) == expected, 'Retained diagnostic input changed: ' + name)
    supervisor = repo / 'build/performance/compare.py'
    require(sha(supervisor) == protocol['supervisorSha256'], 'Supervisor changed; use the recorded baseline checkout or retain its exact compare.py.')
    roslyn = {
        'Microsoft.CodeAnalysis.dll': packages / 'microsoft.codeanalysis.common/5.9.0/lib/net10.0/Microsoft.CodeAnalysis.dll',
        'Microsoft.CodeAnalysis.CSharp.dll': packages / 'microsoft.codeanalysis.csharp/5.9.0/lib/net10.0/Microsoft.CodeAnalysis.CSharp.dll',
    }
    for name, path in roslyn.items():
        require(path.is_file() and sha(path) == protocol['roslynSha256'][name], 'Roslyn binary differs or is missing: ' + str(path))
    output.mkdir(parents=True, exist_ok=True)
    (output / 'Directory.Build.props').write_text('<Project />\n')
    (output / 'Directory.Build.targets').write_text('<Project />\n')
    (output / 'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>\n')
    write_json(output / 'global.json', {'sdk': {'version': protocol['sdkVersion'], 'rollForward': 'disable'}})
    sdk = run([str(dotnet), '--version'], output).strip()
    require(sdk == protocol['sdkVersion'], 'SDK version mismatch')
    runtimes = run([str(dotnet), '--list-runtimes'], output)
    runtime_line = [line for line in runtimes.splitlines() if line.startswith('Microsoft.NETCore.App ' + protocol['runtimeVersion'] + ' [')]
    require(len(runtime_line) == 1, 'Required runtime is missing or ambiguous')
    runtime_dir = Path(runtime_line[0].split('[', 1)[1].rstrip(']')) / protocol['runtimeVersion']
    for name, expected in protocol['runtimeSha256'].items():
        require(sha(runtime_dir / name) == expected, 'Runtime/BCL binary differs: ' + name)
    source_dir = output / 'source'
    source_dir.mkdir()
    for source in protocol['sourceFiles']:
        shutil.copyfile(repo / source['repositoryPath'], source_dir / Path(source['repositoryPath']).name)
    original = (source_dir / 'TSLCompiler.cs').read_bytes()
    needle = b'            return Regex.Replace(result, "(@?)" + prefix + @"(\\d+)__", match =>'
    require(original.count(needle) == 1, 'Cannot identify the reviewed restoration boundary')
    refs = '\n'.join('<Reference Include="' + name.removesuffix('.dll') + '"><HintPath>' + escape(str(path)) + '</HintPath><Private>false</Private></Reference>' for name, path in roslyn.items())
    for variant in ('current', 'restore-guard'):
        directory = output / variant
        directory.mkdir()
        text = original if variant == 'current' else original.replace(needle, b'            if (literals.Count == 0) return result;\n' + needle)
        (directory / 'TSLCompiler.cs').write_bytes(text)
        project = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><LangVersion>14.0</LangVersion><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><AssemblyName>Computator.Translation.Core</AssemblyName><Version>3.0.0</Version><AssemblyVersion>3.0.0.0</AssemblyVersion><Optimize>true</Optimize><DebugType>portable</DebugType><Deterministic>true</Deterministic><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit></PropertyGroup><ItemGroup><Compile Include="TSLCompiler.cs"/><Compile Include="../source/*.cs" Exclude="../source/TSLCompiler.cs"/>' + refs + '</ItemGroup></Project>\n'
        (directory / 'Translation.Core.csproj').write_text(project)
        run([str(dotnet), 'build', str(directory / 'Translation.Core.csproj'), '-c', 'Release', '--nologo', '-v:minimal', '-m:1', '-nr:false', '-p:UseSharedCompilation=false'], output)
    for name in ('Probe.cs', 'Fixtures.json', 'Oracle.json'):
        shutil.copyfile(evidence / name, output / name)
    shutil.copyfile(supervisor, output / 'compare.py')
    probe_refs = refs.replace('<Private>false</Private>', '<Private>true</Private>')
    probe_project = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><LangVersion>14.0</LangVersion><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><AssemblyName>Computator.Translation.Probe</AssemblyName><Optimize>true</Optimize><DebugType>portable</DebugType><Deterministic>true</Deterministic><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit><UseAppHost>false</UseAppHost><Version>1.0.0</Version></PropertyGroup><ItemGroup><Compile Include="Probe.cs"/><Reference Include="Computator.Translation.Core"><HintPath>current/bin/Release/net10.0/Computator.Translation.Core.dll</HintPath><Private>true</Private></Reference>' + probe_refs + '</ItemGroup></Project>\n'
    (output / 'Probe.csproj').write_text(probe_project)
    run([str(dotnet), 'build', str(output / 'Probe.csproj'), '-c', 'Release', '--nologo', '-v:minimal', '-m:1', '-nr:false', '-p:UseSharedCompilation=false'], output)
    for variant in ('current', 'restore-guard'):
        target = output / ('payload-' + variant)
        shutil.copytree(output / 'bin/Release/net10.0', target)
        for file in (output / variant / 'bin/Release/net10.0').glob('Computator.Translation.Core.*'):
            shutil.copyfile(file, target / file.name)
    require(sha(output / 'payload-current/Computator.Translation.Probe.dll') == sha(output / 'payload-restore-guard/Computator.Translation.Probe.dll'), 'Driver bytes differ')
    write_json(output / 'preparation.json', {'scope': protocol['scope'], 'sourceCommit': commit, 'recommendedBaselineCommit': protocol['recommendedBaselineCommit'], 'sourceFiles': protocol['sourceFiles'], 'sdkVersion': sdk, 'installedRuntimes': runtimes, 'dotnet': artifact(dotnet), 'roslyn': [artifact(path) for path in roslyn.values()], 'runtime': [artifact(runtime_dir / name) for name in protocol['runtimeSha256']], 'protocolSha256': sha(evidence / 'protocol.json')})
    manifest = {'schema': 1, 'environment': {}, 'scenarios': {'translation': {'metrics': list(protocol['operations']), 'operations': protocol['operations']}}}
    extra = [output / name for name in ('Probe.cs', 'Probe.csproj', 'Fixtures.json', 'Oracle.json', 'compare.py', 'preparation.json')]
    extra += list(source_dir.glob('*.cs')) + [runtime_dir / name for name in protocol['runtimeSha256']]
    for label, variant in (('baseline', 'current'), ('candidate', 'restore-guard')):
        payload = output / ('payload-' + variant)
        provenance = {'source': 'Isolated copied translator from ' + commit + '; variant ' + variant, 'version': '3.0.0.0', 'buildConfiguration': 'Release', 'baseCommit': commit, 'scope': protocol['scope']}
        if label == 'baseline':
            provenance['commit'] = commit
        else:
            provenance['commitUnknownReason'] = 'Generated diagnostic variant; only the reviewed restore guard differs; exact source is retained.'
        files = []
        for path in sorted(payload.rglob('*')):
            if path.is_file():
                role = 'worker' if path.name == 'Computator.Translation.Probe.dll' else 'core' if path.name == 'Computator.Translation.Core.dll' else None
                files.append(artifact(path, role))
        files += [artifact(path) for path in extra + [output / variant / 'TSLCompiler.cs', output / variant / 'Translation.Core.csproj']]
        manifest[label] = {'command': [str(dotnet), str(payload / 'Computator.Translation.Probe.dll')], 'cwd': str(payload), 'expected': {'coreVersion': '3.0.0.0', 'bitness': 64, 'runtime': '.NET ' + protocol['runtimeVersion']}, 'provenance': provenance, 'artifacts': files}
    arguments = ['--fixtures', str(output / 'Fixtures.json'), '--oracle', str(output / 'Oracle.json')]
    manifest['scenarios']['translation']['args'] = {'baseline': arguments, 'candidate': arguments}
    write_json(output / 'manifest.json', manifest)
    if not args.prepare_only:
        command = [sys.executable, str(output / 'compare.py'), 'run', '--manifest', str(output / 'manifest.json'), '--output', str(output / 'results'), '--pairs', str(args.pairs), '--timeout', '90', '--bootstrap', '50000', '--seed', '191']
        return subprocess.run(command, cwd=output, check=False).returncode
    print('Prepared:', output / 'manifest.json')
    return 0


if __name__ == '__main__':
    try:
        raise SystemExit(main())
    except (OSError, ValueError, subprocess.SubprocessError) as error:
        print('Diagnostic preparation failed:', error, file=sys.stderr)
        raise SystemExit(2)
