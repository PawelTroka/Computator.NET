"""Real X11 inventory contracts on the supplied authenticated display; no GUI parity claim."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import selectors
import shlex
import subprocess

FIXTURE = r'''
#define _POSIX_C_SOURCE 200809L
#include <X11/Xlib.h>
#include <signal.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <unistd.h>
static volatile sig_atomic_t running = 1;
static void stop(int signal_number) { (void)signal_number; running = 0; }
int main(int argc, char **argv) {
    Display *display = XOpenDisplay(NULL);
    if (!display || argc != 2) return 2;
    if (!strcmp(argv[1], "ping")) { XSync(display, False); XCloseDisplay(display); return 0; }
    signal(SIGTERM, stop);
    Window root = DefaultRootWindow(display), a = 0, b = 0, cluster = 0;
    const unsigned leaves = 1200;
    unsigned total;
    if (!strcmp(argv[1], "deep")) {
        Window parent = root;
        for (unsigned index = 0; index < 70; index++) parent = XCreateSimpleWindow(display, parent, 0, 0, 10, 10, 0, 0, 0);
        total = 70;
    } else {
        a = XCreateSimpleWindow(display, root, 0, 0, 20, 20, 0, 0, 0);
        b = XCreateSimpleWindow(display, root, 0, 0, 20, 20, 0, 0, 0);
        cluster = XCreateSimpleWindow(display, a, 0, 0, 10, 10, 0, 0, 0);
        for (unsigned index = 0; index < leaves; index++) {
            Window child = XCreateSimpleWindow(display, cluster, 0, 0, 5, 5, 0, 0, 0);
            XStoreName(display, child, "owned snapshot leaf");
        }
        total = leaves + 3;
    }
    XSync(display, False);
    printf("{\"pid\":%ld,\"a\":%lu,\"b\":%lu,\"cluster\":%lu,\"total\":%u}\n", (long)getpid(), a, b, cluster, total);
    fflush(stdout);
    unsigned tick = 0;
    while (running) {
        if (!strcmp(argv[1], "storm")) {
            XReparentWindow(display, cluster, (++tick & 1) ? b : a, 0, 0);
            XSync(display, False);
        }
        struct timespec pause = { 0, 500000 };
        nanosleep(&pause, NULL);
    }
    XCloseDisplay(display);
    return 0;
}
'''


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--tool', required=True, type=Path)
    parser.add_argument('--evidence', required=True, type=Path)
    args = parser.parse_args()
    if not os.environ.get('DISPLAY'):
        parser.error('An authenticated native X11 display is required; no skipped fallback.')
    evidence = args.evidence.resolve()
    evidence.mkdir(parents=True, exist_ok=False)
    tool = args.tool.resolve(strict=True)
    environment = os.environ.copy()
    environment.pop('DYLD_PRINT_LIBRARIES', None)
    source = evidence / 'snapshot-fixture.c'
    fixture = evidence / 'snapshot-fixture'
    source.write_text(FIXTURE)
    flags = shlex.split(subprocess.check_output(['pkg-config', '--cflags', '--libs', 'x11'], text=True, env=environment))
    subprocess.run(['cc', '-std=c99', '-O2', '-Wall', '-Wextra', '-Werror', str(source), '-o', str(fixture), *flags], env=environment, check=True, timeout=60)
    results = []

    def start(mode):
        process = subprocess.Popen([str(fixture), mode], env=environment, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        selector = selectors.DefaultSelector()
        selector.register(process.stdout, selectors.EVENT_READ)
        try:
            if not selector.select(5):
                raise TimeoutError('The owned X11 fixture did not report readiness.')
            return process, json.loads(process.stdout.readline())
        except BaseException:
            process.kill()
            process.communicate(timeout=2)
            raise
        finally:
            selector.close()

    def stop(process):
        if process.poll() is None:
            process.terminate()
        try:
            process.communicate(timeout=2)
        except subprocess.TimeoutExpired:
            process.kill()
            process.communicate(timeout=2)

    def ping():
        subprocess.run([str(fixture), 'ping'], env=environment, check=True, capture_output=True, timeout=2)

    def inventory(pid):
        result = subprocess.run([str(tool), 'inventory', str(pid)], env=environment, capture_output=True, text=True, timeout=10)
        return result, json.loads(result.stdout)

    try:
        process, owned = start('storm')
        try:
            for ordinal in range(12):
                result, snapshot = inventory(owned['pid'])
                windows = snapshot.get('windows', [])
                identifiers = [window['xid'] for window in windows]
                valid = (result.returncode == 0 and snapshot.get('complete') is True and snapshot.get('pid') == owned['pid']
                         and snapshot.get('ownership') == 'XRes-local-client-pid' and len(windows) == owned['total']
                         and len(set(identifiers)) == len(windows) and all(window['ownerPid'] == owned['pid'] for window in windows)
                         and all(window['parent'] == owned['cluster'] for window in windows if window['xid'] not in (owned['a'], owned['b'], owned['cluster']))
                         and next(window for window in windows if window['xid'] == owned['cluster'])['parent'] in (owned['a'], owned['b']))
                if not valid:
                    (evidence / 'inconsistent-inventory.json').write_text(json.dumps(snapshot, indent=2) + '\n')
                    raise AssertionError('A live reparent produced duplicate, missing, foreign or inconsistent owned XIDs.')
            results.append({'name': 'live-reparent-consistent-owned-graph', 'passed': True, 'snapshots': 12, 'ownedWindowsPerSnapshot': owned['total']})
        finally:
            stop(process)
        process, owned = start('deep')
        try:
            result, snapshot = inventory(owned['pid'])
            (evidence / 'bounded-inventory.json').write_text(json.dumps(snapshot, indent=2) + '\n')
            assert result.returncode == 5 and snapshot.get('complete') is False, 'A bounded traversal must fail closed.'
            ping()
            results.append({'name': 'bounded-failure-releases-server', 'passed': True})
        finally:
            stop(process)
        process, owned = start('stable')
        collector = None
        try:
            # Do NOT drain this pipe. Its large JSON must block only after the
            # server has been released; a separate connection must still reply.
            collector = subprocess.Popen([str(tool), 'inventory', str(owned['pid'])], env=environment, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
            selector = selectors.DefaultSelector()
            try:
                selector.register(collector.stdout, selectors.EVENT_READ)
                assert selector.select(10), 'The inventory did not begin serialization within its existing command deadline.'
            finally:
                selector.close()
            ping()
            assert collector.poll() is None, 'The backpressure fixture did not hold a full output pipe.'
            collector.kill()
            collector.communicate(timeout=2)
            ping()
            results.append({'name': 'stdout-backpressure-does-not-hold-server', 'passed': True})
        finally:
            if collector is not None and collector.poll() is None:
                collector.kill()
                collector.communicate(timeout=2)
            stop(process)
    except BaseException as error:
        results.append({'name': 'failure', 'passed': False, 'error': str(error)})
        raise
    finally:
        report = {'scope': 'real native X11 snapshot contracts; no application GUI parity claim',
                  'toolSha256': hashlib.sha256(tool.read_bytes()).hexdigest(),
                  'fixtureSourceSha256': hashlib.sha256(source.read_bytes()).hexdigest(),
                  'passed': len(results) == 3 and all(item['passed'] for item in results), 'cases': results}
        (evidence / 'xres-snapshot-contracts.json').write_text(json.dumps(report, indent=2) + '\n')
    print('Native XRes snapshot contracts: 3 passed; live reparenting, bounded release and blocked-output release. No GUI parity claim.')


if __name__ == '__main__':
    main()
