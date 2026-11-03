"""Actual ordinary .app launcher acceptance, separate from AppDomain/shared tests.

Prerequisites are external test-only executables. The application gets only the
runtime PATH; a second XRes tool process proves ownership using local peer PID.
No title-only selection, rootless XGetImage, injection, fake click or forced close
can satisfy this gate. Finder/autostart/TCC remain physical-machine acceptance.
"""
import argparse
import ctypes
import hashlib
import json
import os
from pathlib import Path
import platform
import signal
import subprocess
import time


def run(command, *, seconds=10):
    environment = os.environ.copy()
    environment.pop('DYLD_PRINT_LIBRARIES', None)
    result = subprocess.run(list(map(str, command)), env=environment, stdout=subprocess.PIPE,
                            stderr=subprocess.PIPE, text=True, timeout=seconds)
    if result.returncode:
        raise RuntimeError(f'External acceptance command failed ({result.returncode}): {command!r}\n{result.stderr}')
    return result.stdout


def processes():
    rows = []
    for line in run(['/bin/ps', '-ww', '-axo', 'pid=,ppid=,pgid=,uid=,command=']).splitlines():
        fields = line.strip().split(None, 4)
        if len(fields) == 5:
            rows.append(dict(zip(('pid', 'ppid', 'pgid', 'uid'), map(int, fields[:4])), command=fields[4]))
    return rows


def owned_processes(rows, session, session_for=None):
    if session_for is None:
        session_for = os.getsid
    result = []
    for row in rows:
        if row['uid'] != os.getuid():
            continue
        try:
            if session_for(row['pid']) == session:
                result.append(row)
        except ProcessLookupError:
            pass
    return result


def native_executable(pid):
    native = ctypes.CDLL('/usr/lib/libproc.dylib')
    native.proc_pidpath.argtypes = [ctypes.c_int, ctypes.c_void_p, ctypes.c_uint32]
    native.proc_pidpath.restype = ctypes.c_int
    path = ctypes.create_string_buffer(4096)
    if native.proc_pidpath(pid, path, len(path)) <= 0:
        raise RuntimeError('Cannot resolve the actual owned process executable.')
    return Path(os.fsdecode(path.value)).resolve(strict=True)


def application_process(rows, session, mono, executable=native_executable):
    candidates = [row for row in owned_processes(rows, session)
                  if row['pid'] != session and row['command'].endswith(' Computator.NET.exe')]
    if len(candidates) > 1:
        raise RuntimeError('Ambiguous actual application processes inside the owned launch session.')
    if not candidates:
        return None
    row = candidates[0]
    if not os.path.samefile(executable(row['pid']), mono):
        raise RuntimeError('The application process is not the selected native Mono executable.')
    return row


def select_windows(snapshot, pid):
    if snapshot.get('pid') != pid or snapshot.get('ownership') != 'XRes-local-client-pid' or not snapshot.get('complete'):
        raise RuntimeError('Incomplete or mismatched native PID ownership inventory.')
    windows = snapshot['windows']
    if any(window['ownerPid'] != pid for window in windows):
        raise RuntimeError('Native inventory included an unowned window.')
    if len({window['xid'] for window in windows}) != len(windows):
        raise RuntimeError('Native inventory included duplicate window identifiers.')
    main = [window for window in windows if window['mapState'] == 2 and window['canClose']
            and window['width'] >= 600 and window['height'] >= 400 and 'Computator.NET' in window['name']]
    if len(main) > 1:
        raise RuntimeError('Ambiguous mapped main windows owned by the actual Mono PID.')
    if not main:
        return None
    whole = main[0]
    clients = [window for window in windows if window['parent'] == whole['xid'] and window['mapState'] == 2
               and window['width'] >= whole['width'] * .8 and window['height'] >= whole['height'] * .8]
    if len(clients) != 1:
        raise RuntimeError('Expected one directly contained owned main client, excluding the WM frame/titlebar.')
    return whole, clients[0]


def cwd_for(pid):
    values = [line[1:] for line in run(['/usr/sbin/lsof', '-a', '-p', str(pid), '-d', 'cwd', '-Fn']).splitlines()
              if line.startswith('n')]
    if len(values) != 1:
        raise RuntimeError('The actual application CWD is unavailable or ambiguous.')
    return Path(values[0]).resolve(strict=True)


def visible_client_rectangle(whole, client):
    # Mono can allocate a client drawable larger than its parent. Only the
    # actual portion clipped INSIDE the owned whole window is visible UI.
    x, y = max(0, -client['x']), max(0, -client['y'])
    right = min(client['width'], whole['width'] - client['x'])
    bottom = min(client['height'], whole['height'] - client['y'])
    if right - x < 500 or bottom - y < 300:
        raise RuntimeError('The mapped owned main window has no sufficiently large visible client interior.')
    return x, y, right - x, bottom - y


def outside(path, bundle):
    # Canonical existing parents catch symlinks, and casefold closes APFS aliases.
    target, root = str(path.resolve()).casefold(), str(bundle.resolve()).casefold()
    return target != root and not target.startswith(root + os.sep)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('bundle', 'mono', 'config', 'capture-helper', 'x11-tool', 'image-probe', 'evidence'):
        parser.add_argument('--' + name, required=True, type=Path)
    args = parser.parse_args()
    if platform.system() != 'Darwin' or not os.environ.get('DISPLAY'):
        parser.error('Run on the actual Mac with its authenticated rootless XQuartz DISPLAY.')
    bundle = args.bundle.resolve(strict=True)
    resources = bundle / 'Contents/Resources'
    app = resources / 'app'
    launcher = bundle / 'Contents/MacOS/Computator.NET'
    manifest = json.loads((resources / 'mono-package.json').read_text(encoding='utf-8'))
    mono = args.mono.resolve(strict=True)
    tools = {name: getattr(args, name.replace('-', '_')).resolve(strict=True)
             for name in ('config', 'capture-helper', 'x11-tool', 'image-probe')}
    evidence = args.evidence.resolve()
    if not outside(evidence, bundle) or any(not outside(value, bundle) for value in tools.values()):
        parser.error('All acceptance tools/configs/evidence must stay outside the complete .app.')
    evidence.mkdir(parents=True, exist_ok=False)
    for directory in ('profile', 'config', 'cache', 'compositor'):
        (evidence / directory).mkdir()
    report = {'schemaVersion': 1, 'scope': 'ordinary-shell-launch-native-macos', 'passed': False,
              'source': manifest['source'], 'bundle': str(bundle), 'mono': str(mono),
              'finderLaunchAccepted': False, 'physicalMacAccepted': False,
              'launcherSha256': hashlib.sha256(launcher.read_bytes()).hexdigest(),
              'toolsSha256': {name: hashlib.sha256(value.read_bytes()).hexdigest() for name, value in tools.items()},
              'host': {'system': platform.system(), 'release': platform.release(), 'machine': platform.machine()},
              'ownedProcessSnapshots': []}
    environment = os.environ.copy()
    for name in ('MONO_ENV_OPTIONS', 'MONO_CFG_DIR', 'MONO_PATH', 'DYLD_INSERT_LIBRARIES', 'DYLD_LIBRARY_PATH',
                 'DYLD_FALLBACK_LIBRARY_PATH', 'DYLD_PRINT_LIBRARIES', 'COMPUTATOR_GSL_DIRECTORY'):
        environment.pop(name, None)
    environment.update(PATH='/usr/bin:/bin:/usr/sbin:/sbin:/opt/X11/bin', COMPUTATOR_MONO=str(mono),
                       COMPUTATOR_DATA_DIRECTORY=str(evidence / 'profile'), COMPUTATOR_NONINTERACTIVE='1',
                       XDG_CONFIG_HOME=str(evidence / 'config'), XDG_CACHE_HOME=str(evidence / 'cache'),
                       COMPUTATOR_MONO_TRACE_LIBRARIES='1')
    report['runtimePath'] = environment['PATH']
    child = None
    try:
        with (evidence / 'launcher.stdout.log').open('w', encoding='utf-8') as stdout, \
             (evidence / 'launcher.stderr.log').open('w', encoding='utf-8') as stderr:
            child = subprocess.Popen([str(launcher)], cwd=evidence, env=environment,
                                     stdout=stdout, stderr=stderr, start_new_session=True)
            report['launcherPid'] = child.pid
            if os.getpgid(child.pid) != child.pid or os.getsid(child.pid) != child.pid:
                raise RuntimeError('The ordinary launcher did not start in the fresh owned process session.')
            deadline = time.monotonic() + 90
            selected = None
            previous = None
            ready = False
            stable_since = time.monotonic()
            while time.monotonic() < deadline:
                if child.poll() is not None:
                    raise RuntimeError(f'Ordinary launcher exited before visible GUI acceptance: {child.returncode}.')
                row = application_process(processes(), child.pid, mono)
                if row:
                    snapshot = json.loads(run([tools['x11-tool'], 'inventory', row['pid']]))
                    (evidence / 'owned-x11-latest.json').write_text(json.dumps(snapshot, indent=2) + '\n', encoding='utf-8')
                    selected = select_windows(snapshot, row['pid'])
                    if selected != previous:
                        previous = selected
                        stable_since = time.monotonic()
                    if selected and time.monotonic() - stable_since >= .4:
                        ready = True
                        break
                time.sleep(.2)
            if not ready:
                raise TimeoutError('The actual owned Mono process did not map its complete main client within 90 seconds.')
            report['applicationProcess'] = row
            report['ownedProcessSnapshots'].append(owned_processes(processes(), child.pid))
            cwd = cwd_for(row['pid'])
            report['applicationCwd'] = str(cwd)
            if not os.path.samefile(cwd, app):
                raise RuntimeError('The real ordinary-launch application CWD is not relocated Resources/app.')
            whole, client = selected
            report['wholeWindow'] = whole; report['clientWindow'] = client
            visible = visible_client_rectangle(whole, client)
            report['visibleClientX11Rectangle'] = visible
            helper = ctypes.CDLL(str(tools['capture-helper']))
            geometry_call = helper.ComputatorMacDesktopGeometry
            geometry_call.argtypes = [ctypes.c_ulong, ctypes.POINTER(ctypes.c_double), ctypes.c_size_t, ctypes.c_char_p]
            geometry_call.restype = ctypes.c_int
            painted = False
            paint_deadline = time.monotonic() + 15
            attempt = 0
            while time.monotonic() < paint_deadline:
                attempt += 1
                prefix = evidence / 'compositor' / f'normal-launch-{attempt:02}'
                data = (ctypes.c_double * 26)()
                status = geometry_call(client['xid'], data, len(data), os.fsencode(str(prefix) + '-native.json'))
                if status:
                    raise RuntimeError(f'Actual foreign client-to-Quartz geometry failed: {status}.')
                geometry = list(data)
                if geometry[25] != client['xid']:
                    raise RuntimeError('Compositor geometry returned a different native client XID.')
                geometry_file = Path(str(prefix) + '-geometry.json')
                geometry_file.write_text(json.dumps(geometry) + '\n', encoding='utf-8')
                screen = Path(str(prefix) + '-display.png')
                run(['/usr/sbin/screencapture', '-x', '-D', '1', screen])
                command = [str(mono), '--config', str(tools['config']), str(tools['image-probe']), str(geometry_file),
                           *map(str, visible), str(screen), str(prefix)]
                analysis = subprocess.run(command, env={key: value for key, value in environment.items()
                                                       if key != 'DYLD_PRINT_LIBRARIES'},
                                          stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True, timeout=15)
                (Path(str(prefix) + '-analysis.log')).write_text(analysis.stdout + analysis.stderr, encoding='utf-8')
                if analysis.returncode not in (0, 1):
                    raise RuntimeError('The external actual-pixel analysis did not run normally.')
                if analysis.returncode == 0:
                    painted = True
                    report['paintReport'] = str(prefix) + '-paint.json'
                    break
                time.sleep(.2)
            if not painted:
                raise RuntimeError('The actual normal-launch main client stayed blank; retained real compositor pixels.')
            # Re-prove the same live PID, tree and geometry immediately before
            # exactly one native normal-close request. No kill can pass this gate.
            live = application_process(processes(), child.pid, mono)
            if not live or live['pid'] != row['pid']:
                raise RuntimeError('The accepted application process disappeared or changed before normal close.')
            final_snapshot = json.loads(run([tools['x11-tool'], 'inventory', row['pid']]))
            if select_windows(final_snapshot, row['pid']) != selected:
                raise RuntimeError('The actual owned main/client window changed during visible acceptance.')
            (evidence / 'owned-x11-before-close.json').write_text(json.dumps(final_snapshot, indent=2) + '\n', encoding='utf-8')
            report['normalClose'] = json.loads(run([tools['x11-tool'], 'close', row['pid'], whole['xid']]))
            report['launcherExitCode'] = child.wait(timeout=30)
            if child.returncode != 0:
                raise RuntimeError(f'Ordinary launcher normal close returned {child.returncode}, expected zero.')
            time.sleep(.5)
            survivors = owned_processes(processes(), child.pid)
            report['ownedSurvivorsBeforeCleanup'] = survivors
            if survivors:
                raise RuntimeError('Owned ordinary-launch processes survived normal close.')
            report['passed'] = True
    except Exception as error:
        report['error'] = str(error)
    finally:
        # Failure cleanup stays explicitly outside the acceptance oracle and is
        # limited to the fresh session/PGID we created. Never touch XQuartz/WM.
        if child is not None:
            try:
                leftovers = owned_processes(processes(), child.pid)
                report['ownedProcessSnapshots'].append(leftovers)
                if leftovers:
                    report['passed'] = False
                    report['failureCleanupRequired'] = True
                    for group in {row['pgid'] for row in leftovers}:
                        try: os.killpg(group, signal.SIGTERM)
                        except ProcessLookupError: pass
                    deadline = time.monotonic() + 5
                    while owned_processes(processes(), child.pid) and time.monotonic() < deadline:
                        time.sleep(.1)
                    for group in {row['pgid'] for row in owned_processes(processes(), child.pid)}:
                        try: os.killpg(group, signal.SIGKILL)
                        except ProcessLookupError: pass
                    if child.poll() is None: child.wait(timeout=5)
            except Exception as error:
                report['passed'] = False
                report['cleanupError'] = str(error)
        retained = list((evidence / 'compositor').glob('*')) + [evidence / name for name in
            ('owned-x11-latest.json', 'owned-x11-before-close.json', 'launcher.stdout.log', 'launcher.stderr.log')]
        report['evidenceSha256'] = {str(path.relative_to(evidence)): hashlib.sha256(path.read_bytes()).hexdigest()
                                    for path in retained if path.is_file()}
        (evidence / 'normal-launch.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    if not report['passed']:
        raise SystemExit('Actual ordinary .app launch failed: ' + report.get('error', 'unknown failure'))
    print('Actual ordinary .app launch: owned native Mono/client pixels, Resources/app CWD, WM_DELETE_WINDOW and exit0 passed.')


if __name__ == '__main__':
    main()
