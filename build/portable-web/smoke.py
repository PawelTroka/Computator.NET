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

    def symbolic(payload, expected_status: int = 200):
        code, body, content_type = request('/api/v2/symbolic', payload)
        require(code == expected_status, f'Symbolic request returned {code}, expected {expected_status}: {body}')
        require(isinstance(body, dict) and 'json' in content_type, f'Invalid symbolic response: {body}')
        if code == 200:
            require(isinstance(body.get('result'), str) and isinstance(body.get('latex'), str) and
                    body.get('status') in {'exact', 'approximate', 'unevaluated', 'undefined'} and
                    isinstance(body.get('warnings'), list), f'Invalid symbolic result contract: {body}')
        return body

    def oversized_request_is_rejected(path: str) -> None:
        # Read the actual 413 before uploading bytes. Sending the whole body
        # first races an early rejection with a macOS socket reset; a reset is
        # never a passing substitute for the required HTTP response.
        connection = http.client.HTTPConnection('127.0.0.1', port, timeout=5)
        try:
            connection.putrequest('POST', path)
            connection.putheader('Content-Type', 'application/json')
            connection.putheader('Content-Length', '262145')
            connection.putheader('Expect', '100-continue')
            connection.endheaders()
            response = connection.getresponse()
            require(response.status == 413, f'Oversized {path} request returned {response.status}, expected 413.')
            response.read()
        finally:
            connection.close()

    def symbolic_algebra() -> None:
        require(symbolic({'expression': '1/3 + 1/6'})['result'].replace(' ', '') == '1/2', 'Exact rational sum differs.')
        for operation, expression, expected in [('simplify', '(x+1)^2-x^2', '7'),
                                                ('expand', '(x+1)^3', '64'), ('factor', 'x^2-1', '8')]:
            transformed = symbolic({'operation': operation, 'expression': expression})
            value = symbolic({'operation': 'substitute', 'expression': transformed['result'], 'substitutions': {'x': '3'}})
            require(transformed['status'] == 'exact' and value['result'] == expected, f'{operation} identity differs: {value}')
        unicode = symbolic({'operation': 'substitute', 'expression': 'α² + β', 'substitutions': {'α': '2', 'β': '3'}})
        require(unicode['result'] == '7', f'Unicode expression was changed in transport: {unicode}')
        complex_roots = symbolic({'operation': 'solve', 'expression': 'x^2+1=0', 'domain': 'complex'})
        real_roots = symbolic({'operation': 'solve', 'expression': 'x^2+1=0', 'domain': 'real'})
        require('i' in complex_roots['result'] and 'i' not in real_roots['result'], 'Real/complex solution domains were lost.')

    def symbolic_calculus() -> None:
        derivative = symbolic({'operation': 'differentiate', 'expression': 'x^4', 'order': 2})
        value = symbolic({'operation': 'substitute', 'expression': derivative['result'], 'substitutions': {'x': '2'}})
        require(value['result'] == '48', f'Second derivative value differs: {value}')
        integral = symbolic({'operation': 'integrate', 'expression': 'x^2'})
        require(integral['status'] == 'exact' and 'C' in integral['result'], 'Antiderivative lost its arbitrary constant.')
        require(symbolic({'operation': 'definite-integrate', 'expression': 'x^2', 'lowerBound': '0', 'upperBound': '3'})['result'] == '9',
                'Exact definite integral differs.')
        require(symbolic({'operation': 'limit', 'expression': 'sin(x)/x'})['result'] == '1', 'Removable-singularity limit differs.')
        taylor = symbolic({'operation': 'taylor', 'expression': 'sin(x)', 'order': 3})
        value = symbolic({'operation': 'substitute', 'expression': taylor['result'], 'substitutions': {'x': '1'}})
        require(value['result'].replace(' ', '') == '5/6', f'Taylor degree differs: {value}')
        unresolved = symbolic({'operation': 'integrate', 'expression': 'sin(x^x)'})
        require(unresolved['status'] == 'unevaluated' and unresolved['warnings'], 'Unresolved integral was reported as solved.')

    def symbolic_tsl_notation() -> None:
        for expression, expected in [('2·x⁽ⁿ⁺¹⁾', '16'), ('2·x⁻²', '1/2'), ('sin²(x)+cos²(x)', '1'),
                                     ('sin³(PI/6)²', '1/64'), ('log(8,2)', '3')]:
            result = symbolic({'operation': 'substitute', 'expression': expression, 'substitutions': {'x': '2', 'n': '2'}})
            require(result['status'] == 'exact' and result['result'].replace(' ', '') == expected,
                    f'TSL expression changed meaning: {expression}: {result}')
        derivative = symbolic({'operation': 'differentiate', 'expression': '2·x³'})
        require('·' in derivative['result'] and '²' in derivative['result'] and
                '*' not in derivative['result'] and '^' not in derivative['result'], f'Result is not native TSL: {derivative}')
        value = symbolic({'operation': 'substitute', 'expression': derivative['result'], 'substitutions': {'x': '2'}})
        require(value['result'] == '24', f'TSL result cannot be reused: {value}')
        number({'operation': 'calculate', 'expression': derivative['result'], 'x': 2}, 24)
        power = symbolic({'expression': '2*x^(n+1)'})
        require('·' in power['result'] and 'ⁿ' in power['result'] and '^' not in power['result'],
                f'Compound exponent was not rendered as TSL: {power}')
        require(symbolic({'operation': 'substitute', 'expression': power['result'], 'substitutions': {'x': '2', 'n': '2'}})['result'] == '16',
                'Formatted compound exponent changed meaning when reused.')
        logarithm = symbolic({'expression': 'log(x,2)'})
        require(symbolic({'operation': 'substitute', 'expression': logarithm['result'], 'substitutions': {'x': '8'}})['result'] == '3',
                'Two-argument log must preserve TSL argument/base order in reused results.')
        number({'operation': 'calculate', 'expression': logarithm['result'], 'x': 8}, 3)

    def symbolic_matrices_and_precision() -> None:
        matrix = '[[1,2],[3,4]]'
        require(symbolic({'operation': 'determinant', 'expression': matrix})['result'] == '-2', 'Matrix determinant differs.')
        require(symbolic({'operation': 'trace', 'expression': matrix})['result'] == '5', 'Matrix trace differs.')
        for operation in ('inverse', 'transpose'):
            result = symbolic({'operation': operation, 'expression': matrix})
            require(result['status'] == 'exact' and result['latex'], f'Matrix {operation} did not produce an exact result.')
        solution = symbolic({'operation': 'solve-system', 'equations': ['x+y=3', 'x-y=1'], 'variables': ['x', 'y']})
        require(solution['status'] == 'exact' and '2' in solution['result'] and '1' in solution['result'], f'System solution differs: {solution}')
        evaluated = symbolic({'operation': 'evaluate', 'expression': 'sqrt(x)', 'substitutions': {'x': '2'}, 'precision': 40})
        require(evaluated['status'] == 'approximate' and evaluated['result'].startswith('1.414213562373095048801688724209698'),
                f'Arbitrary precision or substitutions were lost: {evaluated}')

    def symbolic_boundaries_and_recovery() -> None:
        require(request('/api/v2/symbolic', {'expression': '1'}, headers={'Origin': 'https://example.invalid'})[0] == 403,
                'Symbolic route bypassed origin protection.')
        require(request('/api/v2/symbolic', raw=b'{}', headers={'Content-Type': 'text/plain'})[0] == 415,
                'Symbolic route accepted a non-JSON body.')
        for payload in ({'expression': 'x' * 8193}, {'expression': 'x', 'operation': 'unknown'}, {'expression': 'x+'},
                        {'expression': 'x', 'operation': 'differentiate', 'order': 999}, {'expression': 'System.IO.File.Delete("x")'}):
            symbolic(payload, 400)
        oversized_request_is_rejected('/api/v2/symbolic')
        require(request('/api/v2/execute', {'operation': 'symbolic', 'symbolic': {'expression': '1'}})[0] == 400,
                'The ordinary execution route must retain its operation contract.')
        require(symbolic({'expression': '6*7'})['result'] == '42', 'Symbolic errors prevented a later valid request.')
        wait_workers(0)

    def symbolic_raw_json_contract() -> None:
        fixtures = json.loads((repository / 'tests/fixtures/symbolic-invalid-json.json').read_text(encoding='utf-8'))
        for fixture in fixtures:
            code, body, content_type = request('/api/v2/symbolic', raw=fixture['json'].encode('utf-8'),
                                               headers={'Content-Type': 'application/json'})
            require(code == 400 and isinstance(body, dict) and body.get('status') == 400 and body.get('detail') and
                    'application/problem+json' in content_type,
                    f"Raw symbolic JSON case {fixture['name']} was not rejected with an actionable 400: {code} {body}")
        require(symbolic({'Expression': 'log(8,2)'})['result'] == '3', 'Symbolic field names lost case-insensitive compatibility.')
        require(symbolic({'operation': 'substitute', 'expression': 'x-X',
                          'substitutions': {'x': '3', 'X': '2'}})['result'] == '1',
                'Case-sensitive mathematical variable names were incorrectly collapsed.')
        wait_workers(0)

    def symbolic_review_regressions() -> None:
        for replacements in ({'x': 'y', 'ComputatorSubstitution_1': '2'}, {'ComputatorSubstitution_1': '2', 'x': 'y'}):
            result = symbolic({'operation': 'substitute', 'expression': 'x', 'substitutions': replacements})
            require(result['result'] == 'y' and result['status'] == 'exact', 'An internal substitution symbol captured user input.')
        system = symbolic({'operation': 'solve-system', 'equations': ['sqrt(x)^2=-1', 'y=0'],
                           'variables': ['x', 'y'], 'domain': 'real'})
        require(system['status'] == 'exact' and system['result'].replace(' ', '') in ('{}', '∅'),
                f'Real system accepted a candidate outside the original radical domain: {system}')
        cotangent = symbolic({'operation': 'evaluate', 'expression': 'arccot(-1)'})
        require(cotangent['status'] == 'approximate' and math.isclose(float(cotangent['result']), 3 * math.pi / 4, abs_tol=1e-14),
                f'Inverse cotangent differs from the numerical TSL principal value: {cotangent}')

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
        require(runtime.get('symbolic') is True and set(runtime.get('symbolicOperations', [])) ==
                {'simplify', 'expand', 'factor', 'differentiate', 'integrate', 'definite-integrate', 'limit', 'solve',
                 'solve-system', 'substitute', 'evaluate', 'taylor', 'determinant', 'inverse', 'transpose', 'trace'},
                f'Symbolic capability discovery is incomplete: {runtime}')
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
        oversized_request_is_rejected('/api/v2/execute')
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
                connection.request('POST', '/api/v2/execute',
                                   json.dumps({'operation': 'script', 'expression': source}),
                                   {'Content-Type': 'application/json'})
                connections.append(connection)
            wait_workers(2)
            pids = wait_executing_workers(pid_files, started_at)
            timing['executingWorkerSeconds'] = round(time.monotonic() - started_at, 3)
            code, _, _ = request('/api/v2/execute', {'operation': 'calculate', 'expression': '1'}, timeout=3)
            require(code == 429, f'Saturated worker capacity returned {code}, expected 429.')
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
            run('symbolic exact algebra, substitution, Unicode and solution domains', symbolic_algebra)
            run('symbolic calculus, Taylor degree and unresolved integrals', symbolic_calculus)
            run('symbolic native TSL notation and reusable output', symbolic_tsl_notation)
            run('symbolic matrices, equation systems and arbitrary precision', symbolic_matrices_and_precision)
            run('symbolic request boundaries, worker cleanup and recovery', symbolic_boundaries_and_recovery)
            run('symbolic raw JSON type and duplicate-member contract', symbolic_raw_json_contract)
            run('symbolic substitution capture and original system domains', symbolic_review_regressions)
            run('local HTTP origin, host and source boundaries', local_boundary)
            run('explicit unsupported operations and error recovery', unsupported_and_recovery)
            run('execution deadline reaps worker and recovers', timeout_and_recovery)
            # Keep the short default deadline proof above. A separate server
            # gives slower cold-worker startup room while keeping cancellation
            # and SIGTERM cleanup strictly earlier than the worker deadline.
            wait_workers(0)
            server.terminate()
            server.wait(timeout=shutdown_cleanup)
            start_server(log, lifecycle_deadline, 'independent-cancellation-and-shutdown')
            run('capacity bound and client cancellation reap workers', cancellation_and_capacity)
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
