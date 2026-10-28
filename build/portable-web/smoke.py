#!/usr/bin/env python3
"""Exercise a published portable local host on its actual OS and architecture.

This is headless HTTP/process acceptance, not macOS desktop or browser acceptance.
No third-party Python packages, mocked endpoints, or Windows service are used.
"""
from __future__ import annotations

import argparse
from datetime import datetime, timezone
import hashlib
import http.client
import json
import math
import os
from pathlib import Path
import platform
import shutil
import signal
import socket
import subprocess
import sys
import tempfile
import time
import urllib.error
import urllib.request


def require(condition: bool, message: str) -> None:
    if not condition:
        raise AssertionError(message)


def available_port() -> int:
    with socket.socket() as listener:
        listener.bind(('127.0.0.1', 0))
        return int(listener.getsockname()[1])


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--publish-directory', type=Path, required=True)
    parser.add_argument('--expected-architecture', choices=['X64', 'Arm64'])
    parser.add_argument('--expected-rid')
    parser.add_argument('--output', type=Path, default=Path('artifacts/portable-web-evidence/local'))
    parser.add_argument('--dotnet', default=os.environ.get('COMPUTATOR_DOTNET', 'dotnet'))
    args = parser.parse_args()
    published = args.publish_directory.resolve()
    output = args.output.resolve()
    output.mkdir(parents=True, exist_ok=True)
    pid_directory = Path(tempfile.mkdtemp(prefix='worker-pids-', dir=output))
    report_path = output / 'http-smoke.json'
    report_path.unlink(missing_ok=True)
    app = published / ('Computator.NET.PortableWeb.exe' if os.name == 'nt' else 'Computator.NET.PortableWeb')
    require(app.is_file(), f'Missing published native apphost: {app}')
    dotnet = shutil.which(args.dotnet)
    require(dotnet is not None, f'.NET executable not found: {args.dotnet}')
    environment = dict(os.environ)
    # Use the SDK/runtime explicitly selected by the developer or CI. The apphost
    # is still a native, RID-specific executable, not a cross-publish inspection.
    environment['DOTNET_ROOT'] = str(Path(dotnet).resolve().parent)
    environment['ASPNETCORE_ENVIRONMENT'] = 'Production'
    environment['DOTNET_ENVIRONMENT'] = 'Production'
    port = available_port()
    base = f'http://127.0.0.1:{port}'
    conflicting_ports: set[int] = set()
    while len(conflicting_ports) < 2:
        candidate = available_port()
        if candidate != port:
            conflicting_ports.add(candidate)
    preferred_port, endpoint_port = sorted(conflicting_ports)
    # Keep the adversarial configuration on loopback, too. A different local
    # listener is sufficient to detect a configuration override safely.
    environment['ASPNETCORE_URLS'] = f'http://127.0.0.1:{preferred_port}'
    environment['ASPNETCORE_PREFERHOSTINGURLS'] = 'true'
    environment['Kestrel__Endpoints__Injected__Url'] = f'http://127.0.0.1:{endpoint_port}'
    observations: list[dict] = []
    runtime: dict = {}
    server: subprocess.Popen | None = None
    started = datetime.now(timezone.utc).isoformat()
    repository = Path(__file__).resolve().parents[2]
    checkout_commit = subprocess.check_output(['git', '-C', str(repository), 'rev-parse', 'HEAD'], text=True).strip()
    checkout_dirty = bool(subprocess.check_output(['git', '-C', str(repository), 'status', '--porcelain'], text=True).strip())

    # Bypass proxy configuration for this exact local service only.
    client = urllib.request.build_opener(urllib.request.ProxyHandler({}))

    def request(path: str, payload=None, *, raw: bytes | None = None,
                headers: dict[str, str] | None = None, method: str | None = None):
        actual_headers = dict(headers or {})
        data = raw
        if payload is not None:
            data = json.dumps(payload, ensure_ascii=False).encode('utf-8')
        if data is not None:
            actual_headers.setdefault('Content-Type', 'application/json')
        query = urllib.request.Request(base + path, data=data, headers=actual_headers, method=method)
        try:
            response = client.open(query, timeout=20)
        except urllib.error.HTTPError as error:
            response = error
        with response:
            body = response.read()
            content_type = response.headers.get('Content-Type', '')
            parsed = json.loads(body) if body and 'json' in content_type else body.decode('utf-8', errors='replace')
            return response.status, parsed, content_type

    def status():
        code, body, _ = request('/api/portable/status')
        require(code == 200 and isinstance(body, dict), f'Invalid status response: {code} {body}')
        return body

    def wait_workers(expected: int, timeout: float = 8) -> None:
        deadline = time.monotonic() + timeout
        last = None
        while time.monotonic() < deadline:
            last = status().get('activeWorkerCount')
            if last == expected:
                return
            time.sleep(0.05)
        raise AssertionError(f'Expected {expected} active workers; last observed {last}.')

    def run(name, check) -> None:
        then = time.monotonic()
        try:
            check()
            observations.append({'name': name, 'passed': True, 'seconds': round(time.monotonic() - then, 3)})
            print(f'PASS {name}', flush=True)
        except Exception as error:
            observations.append({'name': name, 'passed': False, 'seconds': round(time.monotonic() - then, 3),
                                 'error': str(error)})
            print(f'FAIL {name}: {error}', flush=True)

    def execute(payload, expected_status: int = 200):
        code, body, _ = request('/api/v2/execute', payload)
        require(code == expected_status, f'Execution returned {code}, expected {expected_status}: {body}')
        if code == 200:
            require(isinstance(body, dict) and isinstance(body.get('result'), str), f'Invalid result contract: {body}')
            return body['result']
        require(isinstance(body, dict) and body.get('detail'), f'Missing actionable problem details: {body}')
        return body

    def number(payload, expected: float, tolerance: float = 1e-9) -> None:
        actual = float(execute(payload))
        require(math.isfinite(actual) and math.isclose(actual, expected, rel_tol=0, abs_tol=tolerance),
                f'Expected {expected}, got {actual}.')

    def dependencies() -> None:
        config = json.loads((published / 'Computator.NET.PortableWeb.runtimeconfig.json').read_text())
        options = config['runtimeOptions']
        frameworks = options.get('frameworks', [options['framework']] if 'framework' in options else [])
        require({f['name'] for f in frameworks} == {'Microsoft.NETCore.App', 'Microsoft.AspNetCore.App'},
                f'Unexpected framework requirements: {frameworks}')
        deps = json.loads((published / 'Computator.NET.PortableWeb.deps.json').read_text())
        if args.expected_rid:
            require(deps['runtimeTarget']['name'].endswith('/' + args.expected_rid),
                    f'Publication does not target {args.expected_rid}: {deps["runtimeTarget"]}')
        names = {key.split('/')[0] for key in deps['libraries']}
        forbidden = {'Computator.NET.Core', 'Computator.NET.Desktop', 'Computator.NET.Charting',
                     'Microsoft.WindowsDesktop.App', 'System.Windows.Forms', 'PresentationFramework'}
        require(not names.intersection(forbidden), f'Windows desktop dependencies found: {names.intersection(forbidden)}')
        require('Computator.NET.Engine' in names, 'Publication does not reference the actual shared Engine.')

    def advertised_scope() -> None:
        nonlocal runtime
        runtime = status()
        require(runtime.get('implementation') == 'portable-local-poc', f'Unexpected implementation: {runtime}')
        require(runtime.get('chart') is False and runtime.get('nativeGsl') is False,
                'The proof of concept must not advertise charts or the native GSL catalog.')
        require(set(runtime.get('operations', [])) == {'calculate', 'script', 'integral', 'derivative', 'function-root'},
                f'Unexpected advertised operation set: {runtime}')
        if args.expected_architecture:
            require(runtime.get('architecture') == args.expected_architecture,
                    f'Actual process architecture is {runtime.get("architecture")}, expected {args.expected_architecture}.')
        for configured_port in conflicting_ports:
            with socket.socket() as listener_probe:
                listener_probe.settimeout(1)
                require(listener_probe.connect_ex(('127.0.0.1', configured_port)) != 0,
                        f'Environment configuration opened an additional listener at {configured_port}.')

    def production_ui() -> None:
        code, body, content_type = request('/calculate')
        require(code == 200 and 'text/html' in content_type and '<script' in body,
                'Production browser entry point is not served by the portable process.')
        require(request('/api/does-not-exist')[0] == 404, 'Unknown API requests must not return the SPA HTML.')

    def complex_calculation() -> None:
        # A real complex operation whose independently known answer is 3 + 4i.
        value = execute({'operation': 'calculate', 'expression': 'z*z', 'x': 2, 'y': 1})
        parts = value.strip('<>() ').replace(';', ',').split(',')
        require(len(parts) == 2 and float(parts[0]) == 3 and float(parts[1]) == 4,
                f'Complex z*z at 2+i did not produce 3+4i: {value}')

    def custom_functions() -> None:
        definitions = 'static real Probe(real x) { return x+7; }'
        number({'operation': 'calculate', 'expression': 'Probe(3)', 'customFunctions': definitions}, 10)
        require(execute({'operation': 'script', 'expression': 'write(Probe(3)); write("zażółć λ");',
                         'customFunctions': definitions}) == '10zażółć λ', 'Custom script/UTF-8 result differs.')
        execute({'operation': 'calculate', 'expression': 'Probe(3)'}, 400)
        number({'operation': 'calculate', 'expression': '2+3'}, 5)

    def numerical_methods() -> None:
        expected = {'integral': "Simpson's method", 'derivative': 'Finite difference derivative',
                    'function-root': "Brent's method"}
        for operation, method_name in expected.items():
            code, methods, _ = request(f'/api/numerical-calculations/{operation}/list-methods')
            require(code == 200 and methods == [method_name], f'Unexpected {operation} methods: {methods}')
        number({'operation': 'integral', 'expression': '2', 'a': 0, 'b': 3, 'n': 100,
                'method': expected['integral']}, 6)
        number({'operation': 'derivative', 'expression': 'x*x', 'x': 3, 'order': 1,
                'method': expected['derivative']}, 6, 1e-6)
        number({'operation': 'function-root', 'expression': 'x*x-2', 'a': 0, 'b': 2,
                'n': 100, 'epsilon': 1e-10, 'method': expected['function-root']}, math.sqrt(2), 1e-8)
        execute({'operation': 'integral', 'expression': '2', 'a': 0, 'b': 3,
                 'method': 'Unavailable native method'}, 400)

    def local_boundary() -> None:
        payload = {'operation': 'calculate', 'expression': '2+2'}
        for headers in ({'Host': 'example.com'}, {'Origin': 'https://example.com'},
                        {'Sec-Fetch-Site': 'cross-site'}):
            require(request('/api/v2/execute', payload, headers=headers)[0] == 403,
                    f'Hostile browser request was accepted: {headers}')
        require(request('/api/v2/execute', raw=b'2+2', headers={'Content-Type': 'text/plain'})[0] == 415,
                'Execution must require application/json.')
        require(request('/api/v2/execute', raw=b'{"expression":"' + b'1' * 262145 + b'"}')[0] == 413,
                'Oversized request body was not rejected.')
        execute({'operation': 'calculate', 'expression': '1' * 65537}, 400)
        number(payload, 4)

    def unsupported_and_recovery() -> None:
        code, body, _ = request('/api/v2/chart', {'expression': 'x*x'})
        require(code == 422 and isinstance(body, dict) and body.get('detail'), f'Chart limitation not explicit: {code} {body}')
        execute({'operation': 'unknown', 'expression': '1'}, 400)
        execute({'operation': 'calculate', 'expression': 'missing(1)'}, 400)
        number({'operation': 'calculate', 'expression': '7*6'}, 42)

    def traced_worker(name: str):
        # A new private directory per run prevents an earlier report or delayed
        # file write from being mistaken for this run's executing worker.
        pid_file = pid_directory / (name + '-worker.pid')
        require(not pid_file.exists(), 'Worker PID fixture must be fresh.')
        # Only this trusted acceptance script records its own PID; the
        # application has no arbitrary-file or process-diagnostic API.
        source = ('System.IO.File.WriteAllText(' + json.dumps(str(pid_file)) +
                  ', System.Environment.ProcessId.ToString()); while (true) { }')
        return pid_file, source

    def read_worker_pid(pid_file: Path) -> int:
        require(pid_file.is_file(), f'The real TSL worker did not write its PID: {pid_file.name}')
        pid = int(pid_file.read_text())
        require(pid > 1 and server is not None and pid != server.pid,
                'TSL must run in a separate worker process.')
        return pid

    def require_reaped(pid: int) -> None:
        try:
            os.kill(pid, 0)
        except ProcessLookupError:
            return
        raise AssertionError(f'OS process {pid} still exists after worker cleanup.')

    def timeout_and_recovery() -> None:
        pid_file, source = traced_worker('timeout')
        execute({'operation': 'script', 'expression': source}, 504)
        wait_workers(0)
        require_reaped(read_worker_pid(pid_file))
        number({'operation': 'calculate', 'expression': '6*7'}, 42)

    def cancellation_and_capacity() -> None:
        connections = []
        pid_files = []
        pids = []
        started_at = time.monotonic()
        try:
            for index in range(2):
                pid_file, source = traced_worker(f'cancel-{index}')
                pid_files.append(pid_file)
                connection = http.client.HTTPConnection('127.0.0.1', port, timeout=5)
                connection.request('POST', '/api/v2/execute',
                                   json.dumps({'operation': 'script', 'expression': source}),
                                   {'Content-Type': 'application/json'})
                connections.append(connection)
            wait_workers(2)
            # Abort while the ten-second worker deadlines still have at least
            # four seconds left. The subsequent three-second check therefore
            # cannot pass merely because those deadlines expired normally.
            while not all(path.is_file() for path in pid_files) and time.monotonic() - started_at < 6:
                time.sleep(0.05)
            require(time.monotonic() - started_at < 6, 'Workers did not start in time to independently test cancellation.')
            pids = [read_worker_pid(path) for path in pid_files]
            for pid in pids:
                os.kill(pid, 0)
            code, _, _ = request('/api/v2/execute', {'operation': 'calculate', 'expression': '1'})
            require(code == 429, f'Saturated worker capacity returned {code}, expected 429.')
        finally:
            for connection in connections:
                if connection.sock is not None:
                    try:
                        connection.sock.shutdown(socket.SHUT_RDWR)
                    except OSError:
                        pass
                connection.close()
        wait_workers(0, timeout=3)
        for pid in pids:
            require_reaped(pid)
        number({'operation': 'calculate', 'expression': '7*6'}, 42)

    def graceful_shutdown() -> None:
        require(server is not None, 'No owned server process exists.')
        if os.name == 'nt':
            raise AssertionError('This POSIX SIGTERM acceptance check is intended for the documented Linux/macOS hosts.')
        pid_file, source = traced_worker('shutdown')
        connection = http.client.HTTPConnection('127.0.0.1', port, timeout=5)
        try:
            connection.request('POST', '/api/v2/execute',
                               json.dumps({'operation': 'script', 'expression': source}),
                               {'Content-Type': 'application/json'})
            wait_workers(1)
            deadline = time.monotonic() + 6
            while not pid_file.is_file() and time.monotonic() < deadline:
                time.sleep(0.05)
            worker_pid = read_worker_pid(pid_file)
            os.kill(worker_pid, 0)
            # Signal only the server. Killing the whole process group here would
            # mask a failure of the application's own child cleanup behavior.
            server.terminate()
            server.wait(timeout=8)
            require_reaped(worker_pid)
        finally:
            connection.close()

    try:
        run('portable publication dependency graph', dependencies)
        with (output / 'runtime-info.log').open('w') as info:
            subprocess.run([dotnet, '--info'], stdout=info, stderr=subprocess.STDOUT, timeout=30, check=True)
        with (output / 'server.log').open('w') as log:
            server = subprocess.Popen([str(app), '--port', str(port), '--execution-timeout-seconds', '10'],
                                      cwd=published, env=environment, stdout=log, stderr=subprocess.STDOUT,
                                      start_new_session=os.name != 'nt')
            deadline = time.monotonic() + 30
            while time.monotonic() < deadline:
                if server.poll() is not None:
                    raise RuntimeError(f'Published server exited with {server.returncode}; see {output / "server.log"}.')
                try:
                    status()
                    break
                except (OSError, urllib.error.URLError):
                    time.sleep(0.1)
            else:
                raise TimeoutError('Published server did not become ready within 30 seconds.')
            run('native apphost runtime and honest capability boundary', advertised_scope)
            run('production browser files and API routing', production_ui)
            run('real calculation changes with x', lambda: (
                number({'operation': 'calculate', 'expression': '2x+3', 'x': 7}, 17),
                number({'operation': 'calculate', 'expression': '2x+3', 'x': 9}, 21)))
            run('real complex calculation', complex_calculation)
            run('scripts, UTF-8 and request-local unsaved functions', custom_functions)
            run('portable numerical methods with known answers', numerical_methods)
            run('local HTTP origin, host and source boundaries', local_boundary)
            run('explicit unsupported operations and error recovery', unsupported_and_recovery)
            run('execution deadline reaps worker and recovers', timeout_and_recovery)
            run('capacity bound and client cancellation reap workers', cancellation_and_capacity)
            run('graceful server shutdown reaps an executing TSL worker', graceful_shutdown)
    except Exception as error:
        observations.append({'name': 'published host lifecycle', 'passed': False, 'error': str(error)})
        print(f'FAIL published host lifecycle: {error}', flush=True)
    finally:
        if server is not None:
            if server.poll() is None:
                if os.name != 'nt':
                    os.killpg(server.pid, signal.SIGTERM)
                else:
                    server.terminate()
                try:
                    server.wait(timeout=10)
                except subprocess.TimeoutExpired:
                    if os.name != 'nt':
                        os.killpg(server.pid, signal.SIGKILL)
                    else:
                        server.kill()
                    server.wait(timeout=5)
            # Also clean up owned descendants if a failed graceful-shutdown
            # check left them behind after their parent had already exited.
            if os.name != 'nt':
                try:
                    os.killpg(server.pid, signal.SIGKILL)
                except ProcessLookupError:
                    pass
        report = {'schemaVersion': 1, 'scope': 'headless-published-http-and-process-checks',
                  'guiAcceptance': False, 'startedUtc': started,
                  'completedUtc': datetime.now(timezone.utc).isoformat(),
                  'host': platform.platform(), 'machine': platform.machine(), 'runtime': runtime,
                  'checkoutCommit': checkout_commit, 'checkoutDirty': checkout_dirty,
                  'publishedDirectory': str(published), 'nativeApphost': str(app),
                  'workerPidDirectory': str(pid_directory),
                  'publishedAssemblySha256': hashlib.sha256((published / 'Computator.NET.PortableWeb.dll').read_bytes()).hexdigest(),
                  'expectedRid': args.expected_rid, 'checks': observations,
                  'passed': bool(observations) and all(item['passed'] for item in observations)}
        report_path.write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    print(f'{sum(item["passed"] for item in observations)}/{len(observations)} checks passed; {report_path}')
    return 0 if report['passed'] else 1


if __name__ == '__main__':
    sys.exit(main())
