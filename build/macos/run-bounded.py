"""Bound native GUI commands and terminate only the process group we launch."""
import argparse
import os
import signal
import subprocess
import sys
import time

parser = argparse.ArgumentParser()
parser.add_argument("--seconds", type=int, required=True)
parser.add_argument("--child-env", action="append", default=[])
parser.add_argument("command", nargs=argparse.REMAINDER)
args = parser.parse_args()
if not args.command or not 1 <= args.seconds <= 900:
    parser.error("a command and a timeout between 1 and 900 seconds are required")
environment = os.environ.copy()
for entry in args.child_env:
    key, separator, value = entry.partition("=")
    if not separator or not key or not key.replace("_", "a").isalnum() or key[0].isdigit():
        parser.error("--child-env requires a valid NAME=value assignment")
    environment[key] = value
process = subprocess.Popen(args.command, start_new_session=True, env=environment)
try:
    result = process.wait(timeout=args.seconds)
except subprocess.TimeoutExpired:
    print(f"FAIL: owned GUI process exceeded {args.seconds} seconds.", file=sys.stderr)
    result = 124
finally:
    # Descendants must not survive an apparently successful probe either.
    try:
        os.killpg(process.pid, signal.SIGTERM)
    except ProcessLookupError:
        pass
    else:
        # The leader can exit before a descendant which ignores SIGTERM. Waiting
        # only for that leader then leaves the descendant (and its output pipes)
        # alive. Apply the grace period to the owned group instead.
        deadline = time.monotonic() + 5
        while True:
            process.poll()  # Reap an exited leader before probing the group.
            try:
                os.killpg(process.pid, 0)
            except ProcessLookupError:
                break  # Never signal this numeric group again after it vanishes.
            remaining = deadline - time.monotonic()
            if remaining <= 0:
                try:
                    os.killpg(process.pid, signal.SIGKILL)
                except ProcessLookupError:
                    pass  # The last member can exit between the probe and kill.
                break
            time.sleep(min(.05, remaining))
    process.wait(timeout=5)
sys.exit(result)
