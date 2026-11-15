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
    fixtures: list[dict] = []
    lifecycle_timings: list[dict] = []
    short_deadline = 10
    lifecycle_deadline = 45
    worker_readiness = 20
    cancellation_cleanup = 3
    shutdown_cleanup = 8
    deadline_margin = 5
    runtime: dict = {}
    server: subprocess.Popen | None = None
    owned_servers: list[subprocess.Popen] = []
    started = datetime.now(timezone.utc).isoformat()
    repository = Path(__file__).resolve().parents[2]
    checkout_commit = subprocess.check_output(['git', '-C', str(repository), 'rev-parse', 'HEAD'], text=True).strip()
    checkout_dirty = bool(subprocess.check_output(['git', '-C', str(repository), 'status', '--porcelain'], text=True).strip())

    # Bypass proxy configuration for this exact local service only.
    client = urllib.request.build_opener(urllib.request.ProxyHandler({}))

    def request(path: str, payload=None, *, raw: bytes | None = None,
                headers: dict[str, str] | None = None, method: str | None = None, timeout: float = 20):
        actual_headers = dict(headers or {})
        data = raw
        if payload is not None:
            data = json.dumps(payload, ensure_ascii=False).encode('utf-8')
        if data is not None:
            actual_headers.setdefault('Content-Type', 'application/json')
        query = urllib.request.Request(base + path, data=data, headers=actual_headers, method=method)
        try:
            response = client.open(query, timeout=timeout)
        except urllib.error.HTTPError as error:
            response = error
        with response:
            body = response.read()
            content_type = response.headers.get('Content-Type', '')
            parsed = json.loads(body) if body and 'json' in content_type else body.decode('utf-8', errors='replace')
            return response.status, parsed, content_type

    def status(timeout: float = 20):
        code, body, _ = request('/api/portable/status', timeout=timeout)
        require(code == 200 and isinstance(body, dict), f'Invalid status response: {code} {body}')
        return body

    def wait_workers(expected: int, timeout: float = 8) -> None:
        deadline = time.monotonic() + timeout
        last = None
        while time.monotonic() < deadline:
            last = status(timeout=min(1, max(0.05, deadline - time.monotonic()))).get('activeWorkerCount')
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

    def chart(payload, expected_status: int = 200):
        code, body, _ = request('/api/v2/chart', payload)
        require(code == expected_status, f'Chart returned {code}, expected {expected_status}: {body}')
        require(isinstance(body, dict), f'Invalid chart response: {body}')
        if code != 200:
            require(body.get('detail'), f'Missing chart problem details: {body}')
            return body
        if body.get('kind') == 'surface':
            x, y, z = body.get('x'), body.get('y'), body.get('z')
            require(isinstance(x, list) and isinstance(y, list) and isinstance(z, list) and
                    9 <= len(x) <= 129 and len(x) == len(y) == len(z),
                    'Unexpected surface shape.')
            for axis in (x, y):
                require(all(isinstance(v, (int, float)) and math.isfinite(v) for v in axis) and
                        all(a < b for a, b in zip(axis, axis[1:])), 'Surface axes must be finite and increasing.')
            require(all(isinstance(row, list) and len(row) == len(x) for row in z),
                    'Surface must have row-major square samples.')
            values = [v for row in z for v in row if v is not None]
            require(all(isinstance(v, (int, float)) and math.isfinite(v) for v in values),
                    'Surface heights must be finite or null.')
            require(len(values) >= 3 and body.get('finiteSamples') == len(values) and
                    body.get('missingSamples') == len(x) * len(y) - len(values),
                    'Surface count metadata does not match actual samples.')
            return body
        require(body.get('kind') == 'curve', f'Expected numerical curve data: {body}')
        x, y = body.get('x'), body.get('y')
        require(isinstance(x, list) and isinstance(y, list) and len(x) == len(y) and 33 <= len(x) <= 8193,
                'Unexpected curve array size.')
        require(all(isinstance(value, (int, float)) and math.isfinite(value) for value in x) and
                all(left < right for left, right in zip(x, x[1:])), 'Curve coordinates must be finite and increasing.')
        require(all(value is None or isinstance(value, (int, float)) and math.isfinite(value) for value in y),
                'Undefined curve values must be null.')
        finite = sum(value is not None for value in y)
        require(body.get('finiteSamples') == finite and body.get('missingSamples') == len(x) - finite,
                'Curve sample counts differ from the actual data.')
        return body

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
        require(runtime.get('chart') is True and runtime.get('chartKinds') == ['curve', 'surface'] and runtime.get('nativeGsl') is False,
                'Portable charts must advertise curves and explicit 3D height surfaces without native GSL.')
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

    def real_curves() -> None:
        area = {'xMin': -2, 'xMax': 2, 'yMin': -1, 'yMax': 5, 'curveSamples': 65}
        curve = chart({'expression': 'x²', **area})
        require(curve['x'][0] == -2 and curve['x'][-1] == 2 and len(curve['x']) == 65,
                'Curve endpoint/count contract differs.')
        require(all(math.isclose(y, x * x, rel_tol=0, abs_tol=1e-12) for x, y in zip(curve['x'], curve['y'])),
                'Real TSL square curve does not match known values.')
        definitions = 'static real Probe(real x) { return x+7; }'
        custom = chart({'expression': 'Probe(x)', 'customFunctions': definitions, **area})
        require(all(math.isclose(y, x + 7, rel_tol=0, abs_tol=1e-12) for x, y in zip(custom['x'], custom['y'])),
                'Unsaved custom functions did not reach the actual curve worker.')
        chart({'expression': 'Probe(x)', **area}, 400)
        pole = chart({'expression': '1/x', **area})
        require(pole['y'][pole['x'].index(0)] is None, 'A singular sample must leave an explicit gap.')
        maximum = chart({'expression': 'sin(100000*x)', **area, 'curveSamples': 4097})
        require(maximum['finiteSamples'] >= 4097, 'Maximum-size curve was truncated in the worker pipe.')
        for expression, x_min, x_max, expected in (
                ('1.2345678901234567e308', 1e300, 1e308, 1.2345678901234567e308),
                ('5e-324', 1e-300, 2e-300, 5e-324)):
            extreme = chart({'expression': expression, 'xMin': x_min, 'xMax': x_max,
                             'yMin': -1, 'yMax': 1, 'curveSamples': 4097})
            require(len(extreme['x']) == 4097 and all(value == expected for value in extreme['y']),
                    'Maximum-size extreme double values were changed or truncated in the worker pipe.')
        chart({'expression': 'sqrt(-1)', **area}, 422)
        empty = chart({'expression': 'sqrt(-1)', 'allowEmpty2D': True, **area})
        require(empty['finiteSamples'] == 0 and empty['missingSamples'] == 65,
                'Viewport navigation must preserve an empty existing curve.')
        for changes in ({'curveSamples': 32}, {'curveSamples': 4098}, {'xMax': -2},
                        {'xMin': 1e16, 'xMax': 1e16 + 32}, {'yMin': -1e308, 'yMax': 1e308}):
            chart({'expression': 'x', **area, **changes}, 400)

    def height_surfaces() -> None:
        area = {'xMin': -5, 'xMax': 5, 'yMin': -5, 'yMax': 5, 'gridSize': 33}
        surface = chart({'expression': 'x²+y²', **area})
        require(len(surface['x']) == 33 and len(surface['y']) == 33 and len(surface['z']) == 33,
                'Portable f(x,y) grid has the wrong shape.')
        require(surface['z'][0][0] == 50 and surface['z'][16][16] == 0 and
                surface['z'][32][32] == 50 and surface['finiteSamples'] == 1089,
                'Portable paraboloid has incorrect numerical samples.')
        require(surface['x'][0] == -5 and surface['x'][-1] == 5 and
                surface['y'][0] == -5 and surface['y'][-1] == 5,
                'Surface axes must use the requested bounds.')
        definitions = 'static real SurfaceProbe(real x, real y) { return x-y+7; }'
        custom = chart({'expression': 'SurfaceProbe(x,y)', 'customFunctions': definitions, **area})
        require(custom['z'][0][0] == 7 and custom['z'][0][-1] == 17,
                'Unsaved custom f(x,y) definitions did not reach the portable chart worker.')
        chart({'expression': 'SurfaceProbe(x,y)', **area}, 400)
        holes = chart({'expression': 'sqrt(x)+y', **area})
        require(holes['missingSamples'] > 0 and holes['finiteSamples'] > 3 and
                holes['z'][0][0] is None and holes['z'][16][16] == 0,
                'Non-finite height fields must retain explicit null gaps.')
        chart({'expression': 'sqrt(-1)+y', **area}, 422)
        maximum = chart({'expression': 'x+y', **area, 'gridSize': 129})
        require(maximum['finiteSamples'] == 129 * 129 and maximum['z'][0][0] == -10 and
                maximum['z'][-1][-1] == 10, 'Maximum bounded surface grid was truncated.')
        for overrides in ({'gridSize': 8}, {'gridSize': 130},
                          {'xMin': 1e16, 'xMax': 1e16 + 32}):
            chart({'expression': 'x+y', **area, **overrides}, 400)

    def local_boundary() -> None:
        payload = {'operation': 'calculate', 'expression': '2+2'}
        for endpoint in ('/api/v2/execute', '/api/v2/chart'):
            for headers in ({'Host': 'example.com'}, {'Origin': 'https://example.com'},
                            {'Sec-Fetch-Site': 'cross-site'}):
                require(request(endpoint, payload, headers=headers)[0] == 403,
                        f'Hostile browser request was accepted on {endpoint}: {headers}')
            require(request(endpoint, raw=b'2+2', headers={'Content-Type': 'text/plain'})[0] == 415,
                    f'{endpoint} must require application/json.')
        # A server may terminate an oversized request while replying 413
        # (RFC9110 15.5.14). Sending the entire body first races that early
        # response with a macOS socket reset. Read the actual response to an
        # oversized Content-Length before uploading bytes; a reset is never a
        # passing substitute for 413, and accepting the body times out/fails.
        connection = http.client.HTTPConnection('127.0.0.1', port, timeout=5)
        try:
            connection.putrequest('POST', '/api/v2/execute')
            connection.putheader('Content-Type', 'application/json')
            connection.putheader('Content-Length', '262145')
            connection.putheader('Expect', '100-continue')
            connection.endheaders()
            response = connection.getresponse()
            require(response.status == 413, f'Oversized request returned {response.status}, expected 413.')
            response.read()
        finally:
            connection.close()
        execute({'operation': 'calculate', 'expression': '1' * 65537}, 400)
        number(payload, 4)

    def unsupported_and_recovery() -> None:
        for expression in ('z*z',):
            failure = chart({'expression': expression}, 400)
            require('real f(x)' in failure['detail'] and failure.get('diagnostics'),
                    'Unsupported chart input must retain the scope and compilation diagnostics.')
        for expression in ('x=1', '((x=1))', 'x²+y²=1'):
            failure = chart({'expression': expression}, 400)
            require('implicit equations are unavailable' in failure['detail'],
                    'Implicit equations must not become assignment-based horizontal curves.')
        execute({'operation': 'chart', 'expression': 'x*x'}, 400)
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

    def wait_executing_workers(pid_files: list[Path], started_at: float) -> list[int]:
        while time.monotonic() - started_at < worker_readiness:
            if all(path.is_file() and path.stat().st_size > 0 for path in pid_files):
                try:
                    pids = [read_worker_pid(path) for path in pid_files]
                except ValueError:  # File.WriteAllText may have opened but not completed its PID write yet.
                    pass
                else:
                    for pid in pids:
                        os.kill(pid, 0)
                    return pids
            time.sleep(0.05)
        raise AssertionError('Workers did not start in time to independently test lifecycle cleanup.')

    def disconnect(connections) -> None:
        for connection in connections:
            if connection.sock is not None:
                try:
                    connection.sock.shutdown(socket.SHUT_RDWR)
                except OSError:
                    pass
            connection.close()

    def timeout_and_recovery() -> None:
        pid_file, source = traced_worker('timeout')
        execute({'operation': 'script', 'expression': source}, 504)
        wait_workers(0)
        require_reaped(read_worker_pid(pid_file))
        number({'operation': 'calculate', 'expression': '6*7'}, 42)

    def curve_timeout_and_recovery() -> None:
        pid_file, source = traced_worker('curve-timeout')
        chart({'expression': 'Probe(x)', 'customFunctions': 'static real Probe(real x) { ' + source + ' }'}, 504)
        wait_workers(0)
        require_reaped(read_worker_pid(pid_file))
        chart({'expression': 'x+1', 'curveSamples': 33})

    def cancellation_and_capacity() -> None:
        connections = []
        pid_files = []
        pids = []
        started_at = time.monotonic()
        timing = {'phase': 'client-cancellation', 'workerDeadlineSeconds': lifecycle_deadline,
                  'readinessBoundSeconds': worker_readiness, 'cleanupBoundSeconds': cancellation_cleanup}
        lifecycle_timings.append(timing)
        try:
            for index in range(2):
                pid_file, source = traced_worker(f'cancel-{index}')
                pid_files.append(pid_file)
                connection = http.client.HTTPConnection('127.0.0.1', port, timeout=5)
                endpoint = '/api/v2/execute' if index == 0 else '/api/v2/chart'
                payload = {'operation': 'script', 'expression': source} if index == 0 else {
                    'expression': 'Probe(x)', 'customFunctions': 'static real Probe(real x) { ' + source + ' }'}
                connection.request('POST', endpoint, json.dumps(payload),
                                   {'Content-Type': 'application/json'})
                connections.append(connection)
            wait_workers(2)
            pids = wait_executing_workers(pid_files, started_at)
            timing['executingWorkerSeconds'] = round(time.monotonic() - started_at, 3)
            code, _, _ = request('/api/v2/execute', {'operation': 'calculate', 'expression': '1'}, timeout=3)
            require(code == 429, f'Saturated worker capacity returned {code}, expected 429.')
            require(request('/api/v2/chart', {'expression': 'x'}, timeout=3)[0] == 429,
                    'Charts must share the existing two-worker capacity bound.')
            elapsed = time.monotonic() - started_at
            require(elapsed + cancellation_cleanup + deadline_margin < lifecycle_deadline,
                    'Insufficient deadline margin to independently prove client cancellation.')
            timing['remainingDeadlineSecondsAtAbort'] = round(lifecycle_deadline - elapsed, 3)
        finally:
            disconnect(connections)
            cleanup_started = time.monotonic()
            # This must also run when readiness/capacity assertions fail; do not
            # leave occupied slots to contaminate the later shutdown fixture.
            wait_workers(0, timeout=cancellation_cleanup)
            for path in pid_files:
                if path.is_file() and path.stat().st_size > 0:
                    require_reaped(read_worker_pid(path))
            timing['cleanupSeconds'] = round(time.monotonic() - cleanup_started, 3)
        number({'operation': 'calculate', 'expression': '7*6'}, 42)

    def graceful_shutdown() -> None:
        require(server is not None, 'No owned server process exists.')
        if os.name == 'nt':
            raise AssertionError('This POSIX SIGTERM acceptance check is intended for the documented Linux/macOS hosts.')
        pid_file, source = traced_worker('shutdown')
        wait_workers(0, timeout=cancellation_cleanup)
        started_at = time.monotonic()
        timing = {'phase': 'server-shutdown', 'workerDeadlineSeconds': lifecycle_deadline,
                  'readinessBoundSeconds': worker_readiness, 'cleanupBoundSeconds': shutdown_cleanup}
        lifecycle_timings.append(timing)
        connection = http.client.HTTPConnection('127.0.0.1', port, timeout=5)
        try:
            connection.request('POST', '/api/v2/execute',
                               json.dumps({'operation': 'script', 'expression': source}),
                               {'Content-Type': 'application/json'})
            wait_workers(1)
            worker_pid = wait_executing_workers([pid_file], started_at)[0]
            elapsed = time.monotonic() - started_at
            require(elapsed + shutdown_cleanup + deadline_margin < lifecycle_deadline,
                    'Insufficient deadline margin to independently prove shutdown cleanup.')
            timing['executingWorkerSeconds'] = round(elapsed, 3)
            timing['remainingDeadlineSecondsAtStop'] = round(lifecycle_deadline - elapsed, 3)
            # Signal only the server. Killing the whole process group here would
            # mask a failure of the application's own child cleanup behavior.
            server.terminate()
            cleanup_started = time.monotonic()
            server.wait(timeout=shutdown_cleanup)
            require_reaped(worker_pid)
            timing['cleanupSeconds'] = round(time.monotonic() - cleanup_started, 3)
        finally:
            disconnect([connection])
            if server.poll() is None:
                wait_workers(0, timeout=cancellation_cleanup)
            if pid_file.is_file() and pid_file.stat().st_size > 0:
                require_reaped(read_worker_pid(pid_file))

    def start_server(log, seconds: int, name: str) -> None:
        nonlocal server
        server = subprocess.Popen([str(app), '--port', str(port), '--execution-timeout-seconds', str(seconds)],
                                  cwd=published, env=environment, stdout=log, stderr=subprocess.STDOUT,
                                  start_new_session=os.name != 'nt')
        owned_servers.append(server)
        fixture = {'name': name, 'executionTimeoutSeconds': seconds, 'serverPid': server.pid}
        fixtures.append(fixture)
        then = time.monotonic()
        while time.monotonic() - then < 30:
            if server.poll() is not None:
                raise RuntimeError(f'Published server exited with {server.returncode}; see {output / "server.log"}.')
            try:
                status(timeout=1)
                fixture['readySeconds'] = round(time.monotonic() - then, 3)
                return
            except (OSError, urllib.error.URLError):
                time.sleep(0.1)
        raise TimeoutError('Published server did not become ready within 30 seconds.')

    try:
        run('portable publication dependency graph', dependencies)
        with (output / 'runtime-info.log').open('w') as info:
            subprocess.run([dotnet, '--info'], stdout=info, stderr=subprocess.STDOUT, timeout=30, check=True)
        with (output / 'server.log').open('w') as log:
            start_server(log, short_deadline, 'functional-and-short-deadline')
            run('native apphost runtime and honest capability boundary', advertised_scope)
            run('production browser files and API routing', production_ui)
            run('real calculation changes with x', lambda: (
                number({'operation': 'calculate', 'expression': '2x+3', 'x': 7}, 17),
                number({'operation': 'calculate', 'expression': '2x+3', 'x': 9}, 21)))
            run('real complex calculation', complex_calculation)
            run('scripts, UTF-8 and request-local unsaved functions', custom_functions)
            run('portable numerical methods with known answers', numerical_methods)
            run('real curve samples, custom functions, gaps and viewport validation', real_curves)
            run('cross-platform f(x,y) surfaces, custom functions, holes and limits', height_surfaces)
            run('local HTTP origin, host and source boundaries', local_boundary)
            run('explicit unsupported operations and error recovery', unsupported_and_recovery)
            run('execution deadline reaps worker and recovers', timeout_and_recovery)
            run('curve deadline reaps worker and recovers', curve_timeout_and_recovery)
            # Keep the short default deadline proof above. A separate server
            # gives slower cold-worker startup room while keeping cancellation
            # and SIGTERM cleanup strictly earlier than the worker deadline.
            wait_workers(0)
            server.terminate()
            server.wait(timeout=shutdown_cleanup)
            start_server(log, lifecycle_deadline, 'independent-cancellation-and-shutdown')
            run('shared chart/script capacity and client cancellation reap workers', cancellation_and_capacity)
            run('graceful server shutdown reaps an executing TSL worker', graceful_shutdown)
    except Exception as error:
        observations.append({'name': 'published host lifecycle', 'passed': False, 'error': str(error)})
        print(f'FAIL published host lifecycle: {error}', flush=True)
    finally:
        for server in owned_servers:
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
                  'serverFixtures': fixtures, 'lifecycleTimings': lifecycle_timings,
                  'publishedAssemblySha256': hashlib.sha256((published / 'Computator.NET.PortableWeb.dll').read_bytes()).hexdigest(),
                  'expectedRid': args.expected_rid, 'checks': observations,
                  'passed': bool(observations) and all(item['passed'] for item in observations)}
        report_path.write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    print(f'{sum(item["passed"] for item in observations)}/{len(observations)} checks passed; {report_path}')
    return 0 if report['passed'] else 1


if __name__ == '__main__':
    sys.exit(main())
