#!/usr/bin/env python3
"""Prepare/run the separate, isolated Windows per-fixture translation experiment.

Generated projects live only in --output. Source is read from two exact clean
Git commits, whose 13 compiled files are checked before building. This compiles
translator source alone; it does not replace the product/historical experiments.
"""
from __future__ import annotations
import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import re
import shutil
import statistics
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


def write_json(path: Path, value) -> None:
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def git(repo: Path, *arguments: str) -> bytes:
    return subprocess.check_output(['git', '-C', str(repo), *arguments])


def run(command: list[str], directory: Path, log: Path, environment: dict[str, str]) -> str:
    result = subprocess.run(command, cwd=directory, env=environment, text=True,
                            stdout=subprocess.PIPE, stderr=subprocess.STDOUT, check=False, timeout=900)
    log.write_text(result.stdout, encoding='utf-8')
    print(result.stdout, end='', flush=True)
    require(result.returncode == 0, f'Command failed ({result.returncode}); retained log: {log}')
    return result.stdout


def artifact(path: Path, role: str | None = None) -> dict:
    value = {'path': str(path.resolve()), 'sha256': sha(path)}
    if role:
        value['role'] = role
    return value


def check_source(repo: Path, commit: str, output: Path) -> None:
    require(git(repo, 'rev-parse', 'HEAD').decode().strip() == commit, 'Source commit differs: ' + str(repo))
    require(not git(repo, 'status', '--porcelain=v1', '--untracked-files=all', '--ignore-submodules=none').strip(), 'Source checkout must be clean: ' + str(repo))
    flags = git(repo, 'ls-files', '-v').decode().splitlines()
    require(not any(line[0].islower() or line.startswith('S ') for line in flags if line), 'Sparse/assume-unchanged source cannot establish identity.')
    require(not any(line.startswith(('120000 ', '160000 ')) for line in git(repo, 'ls-files', '--stage').decode().splitlines()), 'Source may not contain symlinks or submodules.')
    require(output != repo and output not in repo.parents, 'Output cannot contain a source checkout.')
    if repo in output.parents:
        ignored = subprocess.run(['git', '-C', str(repo), 'check-ignore', '--quiet', '--', str(output.relative_to(repo)) + '/'], check=False)
        require(ignored.returncode == 0, 'Output inside a source checkout must be ignored.')


def stability_report(results: Path, protocol: dict) -> None:
    raw_path = results / 'raw.json'
    if not raw_path.exists():
        return
    raw = json.loads(raw_path.read_text(encoding='utf-8-sig'))
    rows = []
    for trial in raw.get('trials', []):
        fixture = protocol['scenarios'][trial['scenario']]['fixture']
        metric = protocol['scenarios'][trial['scenario']]['metric']
        for invocation in trial.get('invocations', []):
            record = invocation.get('record') or {}
            detail = record.get('details') or {}
            times = record.get('metrics', {}).get(metric, [])
            late_early = statistics.median(times[-3:]) / statistics.median(times[:3]) if len(times) >= 6 else None
            rows.append({'pair': trial['pair'], 'scenario': trial['scenario'], 'version': invocation['version'],
                         'correctness': record.get('correctness'),
                         'warmupBudgetReached': detail.get('warmup.' + fixture + '.budgetReached'),
                         'warmupStabilityObserved': detail.get('warmup.' + fixture + '.stabilityObserved'),
                         'warmupLast20LateEarlyRatio': detail.get('warmup.' + fixture + '.last20LateEarlyRatio'),
                         'warmupLast20RelativeMad': detail.get('warmup.' + fixture + '.last20RelativeMad'),
                         'timedLast3First3MedianRatio': late_early})
    write_json(results / 'stability.json', {'scope': 'Prespecified descriptive stability diagnostics. No observations are removed and inferential intervals are not recomputed from selected rows.', 'rawSha256': sha(raw_path), 'criteria': protocol['stabilityDiagnostic'], 'rows': rows})
    warnings = sum(row['warmupBudgetReached'] is not True or row['warmupStabilityObserved'] is not True for row in rows)
    text = ('# Independent-fixture warmup diagnostics\n\n'
            f'{len(rows)} retained process records; {warnings} did not satisfy both the ten-second budget and the prespecified last-20-batch stability checks.\n\n'
            'These are descriptive checks, not a selection rule. Every observation remains in the paired analysis. '
            'A warning means the experiment does not establish settled steady state for that process. '
            'See stability.json and each original warmup trace for the exact values.\n')
    (results / 'stability.md').write_text(text, encoding='utf-8')


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--baseline-repo', type=Path, required=True)
    parser.add_argument('--candidate-repo', type=Path, required=True)
    parser.add_argument('--candidate-commit', required=True)
    parser.add_argument('--dotnet', type=Path, required=True)
    parser.add_argument('--packages', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--pairs', type=int, default=20)
    parser.add_argument('--prepare-only', action='store_true')
    args = parser.parse_args()
    require(platform.system() == 'Windows' and platform.machine().lower() in ('amd64', 'x86_64'), 'This separate protocol requires Windows x64.')
    require(re.fullmatch('[0-9a-fA-F]{40}', args.candidate_commit) is not None, 'Candidate must be an explicit full commit SHA.')
    require(args.pairs > 0, '--pairs must be positive')
    evidence = Path(__file__).resolve().parent
    baseline, candidate, dotnet, packages, output = (value.resolve() for value in (args.baseline_repo, args.candidate_repo, args.dotnet, args.packages, args.output))
    commit = args.candidate_commit.lower()
    evidence_relative = 'build/performance/diagnostics/translation/'
    require(evidence == candidate / evidence_relative, 'Run the preparer from the explicitly pinned candidate checkout.')
    evidence_names = ('FixtureProbe.cs', 'Fixtures.json', 'Oracle.json', 'protocol.json', 'windows-protocol.json', 'reproduce-windows-fixtures.py')
    evidence_blobs = {name: git(candidate, 'show', commit + ':' + evidence_relative + name) for name in evidence_names}
    # Git blobs preserve the recorded bytes even when Windows checkout settings
    # convert line endings. A clean checkout is still required below.
    protocol = json.loads(evidence_blobs['windows-protocol.json'])
    original = json.loads(evidence_blobs['protocol.json'])
    require(baseline != candidate, 'Supply independent source checkouts.')
    require(dotnet.is_file(), 'Missing explicit dotnet executable.')
    require(not output.exists() or (output.is_dir() and not any(output.iterdir())), 'Output must be absent or empty; previous observations are never overwritten.')
    check_source(baseline, protocol['baselineCommit'], output)
    check_source(candidate, commit, output)
    require(hashlib.sha256(evidence_blobs['FixtureProbe.cs']).hexdigest() == protocol['probeSha256'], 'Independent fixture probe source changed.')
    for name in ('Fixtures.json', 'Oracle.json'):
        require(hashlib.sha256(evidence_blobs[name]).hexdigest() == original['inputs'][name], 'Correctness input changed: ' + name)
    snapshots: dict[str, dict[str, bytes]] = {'baseline': {}, 'candidate': {}}
    needle = b'            return Regex.Replace(result, "(@?)" + prefix + @"(\\d+)__", match =>'
    for item in original['sourceFiles']:
        name = item['repositoryPath']
        before = git(baseline, 'show', protocol['baselineCommit'] + ':' + name)
        after = git(candidate, 'show', commit + ':' + name)
        require(hashlib.sha256(before).hexdigest() == item['sha256'], 'Baseline translator source differs: ' + name)
        if name.endswith('/TSLCompiler.cs'):
            require(before.count(needle) == 1, 'Reviewed restoration boundary is ambiguous.')
            require(after == before.replace(needle, protocol['guardInsertionUtf8'].encode() + needle), 'Candidate must contain exactly the reviewed guard and its two explanatory comments.')
            require(hashlib.sha256(after).hexdigest() == protocol['candidateTranslatorSha256'], 'Candidate guard identity differs.')
        else:
            require(after == before, 'Unrelated compiled source changed: ' + name)
        snapshots['baseline'][name] = before
        snapshots['candidate'][name] = after
    supervisor_before = git(baseline, 'show', protocol['baselineCommit'] + ':build/performance/compare.py')
    supervisor_after = git(candidate, 'show', commit + ':build/performance/compare.py')
    require(supervisor_after == supervisor_before and hashlib.sha256(supervisor_before).hexdigest() == original['supervisorSha256'], 'Supervisor differs from reviewed shared protocol.')
    output.mkdir(parents=True, exist_ok=True)
    logs = output / 'preparation-logs'
    logs.mkdir()
    (output / 'Directory.Build.props').write_text('<Project />\n', encoding='utf-8')
    (output / 'Directory.Build.targets').write_text('<Project />\n', encoding='utf-8')
    (output / 'NuGet.Config').write_text('<configuration><packageSources><clear/><add key="nuget.org" value="https://api.nuget.org/v3/index.json"/></packageSources></configuration>\n', encoding='utf-8')
    write_json(output / 'global.json', {'sdk': {'version': protocol['sdkVersion'], 'rollForward': 'disable'}})
    environment = os.environ.copy()
    environment.update({'NUGET_PACKAGES': str(packages), 'MSBUILDDISABLENODEREUSE': '1', 'DOTNET_CLI_TELEMETRY_OPTOUT': '1', 'DOTNET_NOLOGO': '1'})
    sdk = run([str(dotnet), '--version'], output, logs / 'sdk.txt', environment).strip()
    require(sdk == protocol['sdkVersion'], 'The explicit SDK must be 10.0.402.')
    installed = run([str(dotnet), '--list-runtimes'], output, logs / 'runtimes.txt', environment)
    runtime_lines = [line for line in installed.splitlines() if line.startswith('Microsoft.NETCore.App ' + protocol['runtimeVersion'] + ' [')]
    require(len(runtime_lines) == 1, 'Required runtime is missing or ambiguous.')
    runtime_dir = Path(runtime_lines[0].split('[', 1)[1].rstrip(']')) / protocol['runtimeVersion']
    runtime_files = [runtime_dir / name for name in ('System.Private.CoreLib.dll', 'System.Text.RegularExpressions.dll', 'coreclr.dll', 'clrjit.dll', 'hostpolicy.dll')]
    require(all(path.is_file() for path in runtime_files), 'Expected Windows CLR/JIT/BCL files are missing.')
    dependency_directory = output / 'dependency-restore'
    dependency_directory.mkdir()
    dependency = dependency_directory / 'Dependency.csproj'
    dependency.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit></PropertyGroup><ItemGroup><PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="5.9.0"/></ItemGroup></Project>\n', encoding='utf-8')
    run([str(dotnet), 'restore', str(dependency), '--nologo', '-v:minimal'], output, logs / 'dependency-restore.txt', environment)
    roslyn = {'Microsoft.CodeAnalysis.dll': packages / 'microsoft.codeanalysis.common/5.9.0/lib/net10.0/Microsoft.CodeAnalysis.dll', 'Microsoft.CodeAnalysis.CSharp.dll': packages / 'microsoft.codeanalysis.csharp/5.9.0/lib/net10.0/Microsoft.CodeAnalysis.CSharp.dll'}
    for name, path in roslyn.items():
        require(path.is_file() and sha(path) == original['roslynSha256'][name], 'Roslyn dependency differs: ' + name)
    references = ''.join('<Reference Include="' + name.removesuffix('.dll') + '"><HintPath>' + escape(str(path)) + '</HintPath><Private>false</Private></Reference>' for name, path in roslyn.items())
    options = '<TargetFramework>net10.0</TargetFramework><LangVersion>14.0</LangVersion><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><Optimize>true</Optimize><DebugType>portable</DebugType><Deterministic>true</Deterministic><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit>'
    builds = []
    try:
        for label in ('baseline', 'candidate'):
            directory = output / label
            directory.mkdir()
            for name, data in snapshots[label].items():
                (directory / Path(name).name).write_bytes(data)
            project = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>' + options + '<AssemblyName>Computator.Translation.Core</AssemblyName><Version>3.0.0</Version><AssemblyVersion>3.0.0.0</AssemblyVersion></PropertyGroup><ItemGroup><Compile Include="*.cs"/>' + references + '</ItemGroup></Project>\n'
            path = directory / 'Translation.Core.csproj'
            path.write_text(project, encoding='utf-8')
            command = [str(dotnet), 'build', str(path), '-c', 'Release', '--nologo', '-v:minimal', '-m:1', '-nr:false', '-p:UseSharedCompilation=false']
            builds.append(command)
            run(command, output, logs / (label + '-build.txt'), environment)
        for name in ('FixtureProbe.cs', 'Fixtures.json', 'Oracle.json', 'protocol.json', 'windows-protocol.json'):
            (output / name).write_bytes(evidence_blobs[name])
        (output / 'compare.py').write_bytes(supervisor_before)
        project = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>' + options + '<AssemblyName>Computator.Translation.FixtureProbe</AssemblyName><OutputType>Exe</OutputType><UseAppHost>false</UseAppHost><Version>1.0.0</Version><RuntimeFrameworkVersion>' + protocol['runtimeVersion'] + '</RuntimeFrameworkVersion><RollForward>Disable</RollForward></PropertyGroup><ItemGroup><Compile Include="FixtureProbe.cs"/><Reference Include="Computator.Translation.Core"><HintPath>baseline/bin/Release/net10.0/Computator.Translation.Core.dll</HintPath><Private>true</Private></Reference>' + references.replace('<Private>false</Private>', '<Private>true</Private>') + '</ItemGroup></Project>\n'
        (output / 'FixtureProbe.csproj').write_text(project, encoding='utf-8')
        command = [str(dotnet), 'build', str(output / 'FixtureProbe.csproj'), '-c', 'Release', '--nologo', '-v:minimal', '-m:1', '-nr:false', '-p:UseSharedCompilation=false']
        builds.append(command)
        run(command, output, logs / 'probe-build.txt', environment)
    finally:
        run([str(dotnet), 'build-server', 'shutdown'], output, logs / 'build-server-shutdown.txt', environment)
    write_json(output / 'build-commands.json', builds)
    for label in ('baseline', 'candidate'):
        payload = output / ('payload-' + label)
        shutil.copytree(output / 'bin/Release/net10.0', payload)
        for path in (output / label / 'bin/Release/net10.0').glob('Computator.Translation.Core.*'):
            shutil.copyfile(path, payload / path.name)
    require(sha(output / 'payload-baseline/Computator.Translation.FixtureProbe.dll') == sha(output / 'payload-candidate/Computator.Translation.FixtureProbe.dll'), 'Common driver identity differs.')
    check_source(baseline, protocol['baselineCommit'], output)
    check_source(candidate, commit, output)
    for label, repo, source_commit in (('baseline', baseline, protocol['baselineCommit']), ('candidate', candidate, commit)):
        for name, data in snapshots[label].items():
            require(git(repo, 'show', source_commit + ':' + name) == data, 'Source changed during preparation.')
    preparation = {'scope': protocol['scope'], 'sourceCommits': {'baseline': protocol['baselineCommit'], 'candidate': commit}, 'sdkVersion': sdk, 'installedRuntimes': installed, 'dotnet': artifact(dotnet), 'runtime': [artifact(path) for path in runtime_files], 'roslyn': [artifact(path) for path in roslyn.values()], 'preparer': artifact(Path(__file__)), 'preparerGitBlobSha256': hashlib.sha256(evidence_blobs['reproduce-windows-fixtures.py']).hexdigest(), 'sourceBlobSha256': {label: {name: hashlib.sha256(data).hexdigest() for name, data in files.items()} for label, files in snapshots.items()}}
    write_json(output / 'preparation.json', preparation)
    common_files = [output / name for name in ('FixtureProbe.cs', 'FixtureProbe.csproj', 'Fixtures.json', 'Oracle.json', 'protocol.json', 'windows-protocol.json', 'compare.py', 'preparation.json', 'build-commands.json')] + runtime_files
    manifest = {'schema': 1, 'environment': {}, 'diagnostic': 'isolated-windows-independent-translation-fixtures', 'scenarios': {}}
    for scenario, settings in protocol['scenarios'].items():
        arguments = ['--fixtures', str(output / 'Fixtures.json'), '--oracle', str(output / 'Oracle.json')]
        manifest['scenarios'][scenario] = {'metrics': [settings['metric']], 'operations': {settings['metric']: settings['operations']}, 'args': {'baseline': arguments, 'candidate': arguments}}
    for label, source_commit in (('baseline', protocol['baselineCommit']), ('candidate', commit)):
        payload = output / ('payload-' + label)
        files = []
        for path in sorted(payload.rglob('*')):
            if path.is_file():
                role = 'worker' if path.name == 'Computator.Translation.FixtureProbe.dll' else 'core' if path.name == 'Computator.Translation.Core.dll' else None
                files.append(artifact(path, role))
        files += [artifact(path) for path in common_files + sorted((output / label).glob('*.cs')) + [output / label / 'Translation.Core.csproj']]
        manifest[label] = {'command': [str(dotnet), str(payload / 'Computator.Translation.FixtureProbe.dll')], 'cwd': str(payload), 'artifacts': files, 'expected': {'runtime': '.NET ' + protocol['runtimeVersion'], 'bitness': 64, 'coreVersion': '3.0.0.0'}, 'provenance': {'source': '13 exact Git source blobs from ' + source_commit + '; isolated translator, one fixture per process', 'commit': source_commit, 'version': '3.0.0.0', 'buildConfiguration': 'Release', 'scope': protocol['scope']}}
    write_json(output / 'manifest.json', manifest)
    print('Prepared:', output / 'manifest.json')
    if args.prepare_only:
        return 0
    command = [sys.executable, str(output / 'compare.py'), 'run', '--manifest', str(output / 'manifest.json'), '--output', str(output / 'results'), '--pairs', str(args.pairs), '--timeout', '90', '--bootstrap', '50000', '--seed', '191']
    result = subprocess.run(command, cwd=output, check=False)
    stability_report(output / 'results', protocol)
    return result.returncode


if __name__ == '__main__':
    try:
        raise SystemExit(main())
    except (OSError, ValueError, subprocess.SubprocessError) as error:
        print('Independent Windows diagnostic failed:', error, file=sys.stderr)
        raise SystemExit(2)
