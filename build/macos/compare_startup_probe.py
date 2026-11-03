"""Native ARM counterfactual for forced GUI/compiler AppDomain unloading.

This diagnostic follows ordinary application/package acceptance. It preserves
that accepted app and compares only two external StartupProbe entry points.
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
import signal
import struct
import subprocess
import sys
import time
import xml.etree.ElementTree as ET
import zipfile

CONTROL = '0608a6565aaaa536e341ad21a7cdd5179bc574d7'
MARKER = 'PASS production executable startup, dependency loading, rendering and graceful close'
COMMON = ('global.json', 'Version.props', 'SharedAssemblyInfo.cs', 'Directory.Build.props',
          'Directory.Build.targets', 'Directory.Packages.props', 'build/ProjectTree.targets',
          'build/ModernDesktop.props', 'build/MonoDesktop.props',
          'build/mono/StartupProbe/Computator.NET.Mono.StartupProbe.csproj',
          'build/mono/NativeDesktopCapture.cs', 'build/mono/NativeX11Input.cs',
          'build/macos/MacDesktopCapture.cs')


def digest(path):
    value = hashlib.sha256()
    with Path(path).open('rb') as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b''):
            value.update(block)
    return value.hexdigest()


def write(path, value):
    with path.open('x', encoding='utf-8') as stream:
        json.dump(value, stream, indent=2)
        stream.write('\n')


def load(path):
    return json.loads(path.read_text(encoding='utf-8'))


def git(repository, *arguments):
    return subprocess.check_output(['git', '-C', str(repository), *arguments], timeout=30)


def normalized(value):
    return value.decode('utf-8-sig').replace('\r\n', '\n')


def assertion_offsets(text):
    """Classify producer-merged raw output, never concatenated separate pipes."""
    marker = text.find(MARKER)
    assertion = re.search(r'Assertion at metadata\.c:3434[^\r\n]*gclass_in_image', text)
    failed = assertion.start() if assertion else -1
    unload = text.find('unload_thread_main', assertion.end()) if assertion else -1
    return {'normalMarkerOffset': marker, 'assertionOffset': failed, 'unloadStackOffset': unload,
            'orderedPostCloseAssertion': marker >= 0 and failed > marker and unload > failed}


def process(command, environment, cwd, output, seconds, deadline, merged=False):
    """Keep raw streams before verdict; reap only the process group we own."""
    output.mkdir()
    stdout = output / ('combined.raw.log' if merged else 'stdout.log')
    stderr = output / 'stderr.log'
    timed_out = overflow = False
    def group_exists(pid):
        try:
            os.killpg(pid, 0)
            return True
        except ProcessLookupError:
            return False
    first = stdout.open('xb')
    second = None if merged else stderr.open('xb')
    try:
        child = subprocess.Popen(command, cwd=cwd, env=environment, stdout=first,
                                 stderr=subprocess.STDOUT if merged else second, start_new_session=True)
        limit = min(deadline, time.monotonic() + seconds)
        while child.poll() is None:
            timed_out = time.monotonic() >= limit
            overflow = stdout.stat().st_size > 8 * 1024**2 or (not merged and stderr.stat().st_size > 8 * 1024**2)
            if timed_out or overflow:
                break
            time.sleep(0.05)
        if timed_out or overflow:
            try:
                os.killpg(child.pid, signal.SIGTERM)
            except ProcessLookupError:
                pass
            try:
                child.wait(timeout=5)
            except subprocess.TimeoutExpired:
                os.killpg(child.pid, signal.SIGKILL)
        result = child.wait(timeout=5)
        survivors = group_exists(child.pid)
        if survivors:
            try:
                os.killpg(child.pid, signal.SIGTERM)
            except ProcessLookupError:
                pass
            cleanup_deadline = time.monotonic() + 3
            while group_exists(child.pid) and time.monotonic() < cleanup_deadline:
                time.sleep(0.05)
            if group_exists(child.pid):
                try:
                    os.killpg(child.pid, signal.SIGKILL)
                except ProcessLookupError:
                    pass
                cleanup_deadline = time.monotonic() + 3
                while group_exists(child.pid) and time.monotonic() < cleanup_deadline:
                    time.sleep(0.05)
        remaining = group_exists(child.pid)
    finally:
        first.close()
        if second is not None: second.close()
    overflow = overflow or stdout.stat().st_size > 8 * 1024**2 or (not merged and stderr.stat().st_size > 8 * 1024**2)
    record = {'argv': list(map(str, command)), 'pid': child.pid, 'exitCode': result,
              'timedOut': timed_out, 'outputLimitExceeded': overflow,
              'ownedProcessGroupSurvivedExit': survivors,
              'ownedProcessGroupRemainingAfterCleanup': remaining,
              'streamRouting': 'producer FD2 redirected to FD1; one kernel-ordered raw stream' if merged else 'independent stdout and stderr',
              'stdout': {'file': stdout.name, 'bytes': stdout.stat().st_size, 'sha256': digest(stdout)},
              'stderr': {'redirectedToStdout': True} if merged else {'file': stderr.name, 'bytes': stderr.stat().st_size, 'sha256': digest(stderr)}}
    write(output / 'process.json', record)
    return record, stdout.read_text(encoding='utf-8', errors='replace') + ('' if merged else '\n' + stderr.read_text(encoding='utf-8', errors='replace'))


def picture(path):
    with path.open('rb') as stream:
        header = stream.read(24)
    if len(header) != 24 or header[:8] != b'\x89PNG\r\n\x1a\n' or header[12:16] != b'IHDR':
        raise ValueError('Missing actual native PNG: ' + str(path))
    width, height = struct.unpack('>II', header[16:24])
    if min(width, height) < 32:
        raise ValueError('Native PNG dimensions are too small')
    return {'width': width, 'height': height, 'sha256': digest(path)}


def compile_probe(repository, output, program, environment, deadline):
    output.mkdir()
    original = repository / 'build/mono/StartupProbe/Computator.NET.Mono.StartupProbe.csproj'
    project = ET.parse(original)
    for element in project.iter():
        key = 'Project' if element.tag == 'Import' else 'Include' if element.tag == 'Compile' else None
        if key and key in element.attrib:
            relative = element.attrib[key]
            if '$' in relative or '*' in relative:
                raise ValueError('Unexpected indirect probe build input')
            element.set(key, str((original.parent / relative.replace('\\', '/')).resolve(strict=True)))
    project_path = output / original.name
    project.write(project_path, encoding='utf-8', xml_declaration=True)
    (output / 'Program.cs').write_bytes(program)
    command = [shutil.which('dotnet'), 'build', str(project_path), '-c', 'Release',
               '-p:ComputatorMono=true', '-p:UseSharedCompilation=false', '--disable-build-servers', '-m:1', '-nr:false',
               '-p:DirectoryBuildPropsPath=' + str(repository / 'Directory.Build.props'),
               '-p:DirectoryBuildTargetsPath=' + str(repository / 'Directory.Build.targets'),
               '-p:DirectoryPackagesPropsPath=' + str(repository / 'Directory.Packages.props'), '--nologo']
    if not command[0]:
        raise ValueError('The pinned SDK must be available for the external probe build')
    result, _ = process(command, environment, repository, output / 'build', 180, deadline)
    if result['exitCode'] or result['timedOut'] or result['outputLimitExceeded'] or result['ownedProcessGroupSurvivedExit']:
        raise ValueError('External probe build failed; raw build streams retained')
    executable = output / 'bin/mono/Release/net48/Computator.NET.Mono.StartupProbe.exe'
    if not executable.is_file():
        raise ValueError('Expected the real net48 startup-probe output')
    return executable, {'programSha256': digest(output / 'Program.cs'),
                        'projectSha256': digest(project_path), 'executableSha256': digest(executable),
                        'buildProcess': result}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--repository', type=Path, required=True)
    parser.add_argument('--evidence', type=Path, required=True)
    parser.add_argument('--pairs', type=int, default=8)
    args = parser.parse_args()
    if platform.system() != 'Darwin' or platform.machine() != 'arm64' or not 8 <= args.pairs <= 16:
        parser.error('This bounded diagnostic requires native Apple Silicon and 8–16 pairs.')
    import resource
    repository = args.repository.resolve(strict=True)
    output = (repository / args.evidence).resolve() if not args.evidence.is_absolute() else args.evidence.resolve()
    if output.exists() or not output.is_relative_to(repository / 'artifacts/macos'):
        raise ValueError('Use a new external evidence directory under artifacts/macos')
    if git(repository, 'status', '--porcelain', '--untracked-files=normal').strip():
        raise ValueError('The accepted source checkout must remain clean')
    if shutil.disk_usage(repository).free < 1024**3:
        raise ValueError('At least 1 GiB is required for bounded comparison evidence/build outputs')
    output.mkdir(parents=True)
    summary = {'schemaVersion': 1, 'scope': 'diagnostic forced-AppDomain-unload counterfactual; ordinary application acceptance is separate',
               'passed': False, 'pairsRequired': args.pairs, 'controlCommit': CONTROL, 'trials': []}
    try:
        deadline = time.monotonic() + 1200
        marker_paths = list((repository / 'artifacts/macos/package/Release').glob('acceptance-osx-arm64-*/accepted-candidate.json'))
        if len(marker_paths) != 1:
            raise ValueError('Exactly one ordinarily accepted ARM package is required first')
        accepted_path = marker_paths[0]
        accepted, accepted_root = load(accepted_path), accepted_path.parent
        commit = git(repository, 'rev-parse', 'HEAD').decode().strip()
        tree = git(repository, 'rev-parse', 'HEAD^{tree}').decode().strip()
        if (accepted.get('packagedRuntimeAccepted') is not True or accepted.get('sharedCases', 0) < 55
                or accepted['source']['commit'] != commit or accepted['source']['tree'] != tree
                or accepted['source']['trackedChanges'] is not False):
            raise ValueError('Ordinary accepted package does not match this exact clean source')
        if len(accepted['evidenceSha256']) != 14:
            raise ValueError('Ordinary acceptance must retain all 14 evidence hashes')
        for name, expected in accepted['evidenceSha256'].items():
            path = (accepted_root / name).resolve(strict=True)
            if not path.is_relative_to(accepted_root) or digest(path) != expected:
                raise ValueError('Ordinary accepted evidence changed')
        normal = load(accepted_root / 'normal-launch/normal-launch.json')
        pid = normal['applicationProcess']['pid']
        if (normal.get('passed') is not True or normal['launcherExitCode'] != 0
                or normal['ownedSurvivorsBeforeCleanup'] or normal.get('failureCleanupRequired')
                or normal['source'] != accepted['source']
                or normal['clientWindow']['ownerPid'] != pid or normal['wholeWindow']['ownerPid'] != pid
                or normal['clientWindow']['parent'] != normal['wholeWindow']['xid']
                or normal['normalClose'] != {'pid': pid, 'xid': normal['wholeWindow']['xid'], 'protocol': 'WM_DELETE_WINDOW', 'sent': True, 'xerror': 0}):
            raise ValueError('Ordinary launcher must already have passed strict PID-owned normal close')
        bundle = accepted_root / 'relocated folder with spaces/Computator.NET.app'
        if Path(normal['bundle']).resolve(strict=True) != bundle.resolve(strict=True):
            raise ValueError('Ordinary launcher report belongs to a different relocated bundle')
        resources = bundle / 'Contents/Resources'
        manifest = load(resources / 'mono-package.json')
        archives = list((repository / 'artifacts/macos/package/Release').rglob(accepted['archive']['name']))
        if (len(archives) != 1 or archives[0].stat().st_size != accepted['archive']['size']
                or digest(archives[0]) != accepted['archive']['sha256']):
            raise ValueError('The ordinary accepted archive changed or is ambiguous')
        with zipfile.ZipFile(archives[0]) as archive:
            member = 'Computator.NET.app/Contents/Resources/mono-package.json'
            if sum(item.filename == member for item in archive.infolist()) != 1:
                raise ValueError('The ordinary accepted archive manifest is ambiguous')
            archived_manifest = archive.read(member)
        if (archived_manifest != (resources / 'mono-package.json').read_bytes()
                or archived_manifest != (accepted_root / 'mono-package.json').read_bytes()
                or manifest['source'] != accepted['source']):
            raise ValueError('The actual compared payload differs from the ordinary accepted archive')
        identity = manifest['productIdentity']
        version = subprocess.check_output([sys.executable, str(repository / 'build/release/versioning.py')], cwd=repository, text=True, timeout=30)
        if identity != json.loads(version) or manifest['version'] != identity['productVersion'] or not identity.get('versionTimestamp'):
            raise ValueError('Comparison must use the accepted frozen numeric identity')
        subprocess.run([sys.executable, str(repository / 'build/macos/package.py'), '--verify', '--output', str(bundle)], check=True, timeout=180)
        config = accepted_root / 'mono.config'
        native = repository / 'artifacts/macos/native-arm64'
        mono_prefix = Path(load(native / 'native-manifest.json')['monoPrefix'])
        mono = (mono_prefix / 'bin/mono').resolve(strict=True)
        mono_version = subprocess.check_output([str(mono), '--version'], text=True, timeout=30)
        if 'version 6.14.1' not in mono_version:
            raise ValueError('This diagnostic targets the observed Mono 6.14.1 cleanup assertion')
        try:
            git(repository, 'cat-file', '-e', CONTROL + ':build/mono/StartupProbe/Program.cs')
        except subprocess.CalledProcessError:
            subprocess.run(['git', '-C', str(repository), 'fetch', '--no-tags', '--depth=1', 'origin', CONTROL], check=True,
                           timeout=max(1, min(120, deadline-time.monotonic())))
        common = {}
        for name in COMMON:
            current = (repository / name).read_bytes()
            if current != git(repository, 'show', CONTROL + ':' + name):
                raise ValueError('A common build input changed since the actual control: ' + name)
            common[name] = hashlib.sha256(current).hexdigest()
        program_path = 'build/mono/StartupProbe/Program.cs'
        old, fixed = git(repository, 'show', CONTROL + ':' + program_path), (repository / program_path).read_bytes()
        declaration = 'public sealed class StartupRunner : MarshalByRefObject'
        old_tail, fixed_tail = normalized(old).partition(declaration)[2], normalized(fixed).partition(declaration)[2]
        if not old_tail or old_tail != fixed_tail:
            raise ValueError('The complete real production StartupRunner must be unchanged')
        inherited = ('HOME', 'USER', 'LOGNAME', 'TMPDIR', 'PATH', 'DOTNET_ROOT', 'DOTNET_CLI_HOME', 'NUGET_PACKAGES', 'LANG', 'LC_ALL', 'LC_CTYPE')
        build_env = {key: os.environ[key] for key in inherited if key in os.environ}
        build_env.update(DOTNET_CLI_TELEMETRY_OPTOUT='1', DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1', DOTNET_NOLOGO='1', MSBUILDDISABLENODEREUSE='1')
        probes, builds = {}, {}
        for label, source in (('old', old), ('fixed', fixed)):
            probes[label], builds[label] = compile_probe(repository, output / label, source, build_env, deadline)
        if builds['old']['projectSha256'] != builds['fixed']['projectSha256']:
            raise ValueError('The two isolated project copies differ')
        snapshot_paths = [archives[0], resources / 'mono-package.json', resources / 'app/Computator.NET.exe', resources / 'app/Computator.NET.Core.dll',
                          resources / 'app/Computator.NET.exe.config', config, accepted_root / 'harness/libcomputator-mac-display.dylib', mono,
                          *(mono_prefix / 'lib/mono/4.5' / name for name in ('mscorlib.dll', 'System.dll', 'System.Core.dll', 'System.Drawing.dll', 'System.Windows.Forms.dll'))]
        for key in ('COMPUTATOR_MAC_DISPLAY_ENV', 'XAUTHORITY'):
            if os.environ.get(key): snapshot_paths.append(Path(os.environ[key]))
        before = {str(path): digest(path) for path in snapshot_paths}
        summary.update(source=accepted['source'], productIdentity=identity, ordinaryAcceptedReportSha256=digest(accepted_path),
                       ordinaryNormalLaunchSha256=digest(accepted_root / 'normal-launch/normal-launch.json'),
                       commonBuildInputs=common, startupRunnerSha256=hashlib.sha256(old_tail.encode()).hexdigest(),
                       controlProgramGitBlob=git(repository, 'rev-parse', CONTROL + ':' + program_path).decode().strip(),
                       mono={'path': str(mono), 'version': mono_version, 'sha256': digest(mono)}, builds=builds,
                       runtimeInputsBefore=before, display=os.environ.get('DISPLAY'))
        resource.setrlimit(resource.RLIMIT_CORE, (0, 0))
        old_failures = 0
        summary['pairOrder'] = []
        for pair in range(args.pairs):
            order = ('old', 'fixed') if pair % 2 == 0 else ('fixed', 'old')
            summary['pairOrder'].append({'pair': pair + 1, 'order': list(order)})
            for label in order:
                trial = output / ('pair-%02d-%s' % (pair + 1, label)); trial.mkdir()
                for name in ('profile', 'config', 'cache', 'compositor'): (trial / name).mkdir()
                env = {key: os.environ[key] for key in ('HOME', 'USER', 'LOGNAME', 'TMPDIR', 'DISPLAY', 'XAUTHORITY', 'LANG', 'LC_ALL', 'LC_CTYPE') if key in os.environ}
                env.update(PATH=str(accepted_root / 'harness/bin') + ':/usr/bin:/bin:/usr/sbin:/sbin:/opt/X11/bin',
                           MONO_MWF_MAC_FORCE_X11='1', COMPUTATOR_MONO_XIM_ACCEPTANCE='enabled', COMPUTATOR_NONINTERACTIVE='1',
                           FONTCONFIG_FILE=str(resources / 'native/fonts.conf'), FONTCONFIG_PATH=str(resources / 'native'),
                           GIO_MODULE_DIR=str(resources / 'native/gio-modules'), GIO_EXTRA_MODULES='', XLOCALEDIR='/opt/X11/share/X11/locale',
                           LIBTHAI_DICTDIR=str(resources / 'native/libthai'), MONO_PATH=str(resources / 'app') + ':' + str(resources / 'app/Facades'),
                           XDG_CONFIG_HOME=str(trial / 'config'), XDG_CACHE_HOME=str(trial / 'cache'), COMPUTATOR_DATA_DIRECTORY=str(trial / 'profile'),
                           COMPUTATOR_MAC_COMPOSITOR_EVIDENCE_DIRECTORY=str(trial / 'compositor'), DYLD_PRINT_LIBRARIES='1')
                if not env.get('DISPLAY'): raise ValueError('The authenticated accepted XQuartz display is required')
                result, text = process([str(mono), '--config', str(config), str(probes[label]), str(resources / 'app/Computator.NET.exe'), str(trial)], env, resources / 'app', trial / 'process', 90, deadline, merged=True)
                offsets = assertion_offsets(text)
                known = offsets['orderedPostCloseAssertion'] and 'sigabrt_signal_handler' in text
                native_problem = any(token in text for token in ('Native Crash Reporting', 'Assertion at metadata.c:', 'gclass_in_image', 'sigabrt_signal_handler'))
                colors = re.findall(r'Production complex chart: visible=True[^\n]*colored-pixels=(\d+)', text)
                result.update(pair=pair + 1, variant=label, knownUnloadAssertion=known, nativeCrashOrAssertionObserved=native_problem, assertionOrdering=offsets,
                              environment=env, normalMarkerCount=text.count(MARKER), actualDrawAcknowledgements=text.count('Native X11 click acknowledged once:'))
                summary['trials'].append(result)
                result['images'] = {name: picture(trial / name) for name in ('startup.png', 'production-complex-after.png')}
                write(trial / 'trial.json', result)
                if (result['timedOut'] or result['outputLimitExceeded'] or result['ownedProcessGroupSurvivedExit']
                        or result['normalMarkerCount'] != 1 or result['actualDrawAcknowledgements'] != 1 or not colors or int(colors[-1]) <= 200):
                    raise ValueError('Trial did not preserve strict real GUI/input/render/normal-close acceptance')
                if label == 'fixed' and (result['exitCode'] != 0 or native_problem):
                    raise ValueError('The fixed probe did not exit normally')
                if label == 'old':
                    if result['exitCode'] == 0 and native_problem:
                        raise ValueError('Old control reported a native crash with an inconsistent zero exit')
                    if result['exitCode'] != 0:
                        if result['exitCode'] != -signal.SIGABRT or not known:
                            raise ValueError('The old control failed for a different reason/signature/ordering')
                        old_failures += 1
        after = {str(path): digest(path) for path in snapshot_paths}
        if before != after or common != {name: digest(repository / name) for name in COMMON}:
            raise ValueError('App/config/Mono/display/common build inputs changed during comparison')
        subprocess.run([sys.executable, str(repository / 'build/macos/package.py'), '--verify', '--output', str(bundle)], check=True, timeout=180)
        if git(repository, 'status', '--porcelain', '--untracked-files=normal').strip():
            raise ValueError('The source checkout changed during comparison')
        if old_failures == 0: raise ValueError('The old native cleanup assertion did not reproduce; no causal claim is made')
        summary.update(passed=True, reproducedOldAssertions=old_failures, fixedNormalExits=args.pairs,
                       runtimeInputsAfter=after, coreDumpLimitBytes=0)
    except Exception as error:
        summary['failure'] = str(error)
        raise
    finally:
        write(output / 'startup-unload-counterfactual.json', summary)
    print(json.dumps({'passed': True, 'oldAssertions': summary['reproducedOldAssertions'], 'fixedNormalExits': args.pairs, 'evidence': str(output)}))


if __name__ == '__main__':
    main()
