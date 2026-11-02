"""Bound native GUI processes and reap only the process group we launch."""
import argparse
import os
import signal
import subprocess
import sys

parser = argparse.ArgumentParser()
parser.add_argument("--seconds", type=int, required=True)
parser.add_argument("command", nargs=argparse.REMAINDER)
args = parser.parse_args()
if not args.command or not 1 <= args.seconds <= 900:
    parser.error("a command and a timeout between 1 and 900 seconds are required")
process = subprocess.Popen(args.command, start_new_session=True)
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
    try:
        process.wait(timeout=5)
    except subprocess.TimeoutExpired:
        os.killpg(process.pid, signal.SIGKILL)
        process.wait()
sys.exit(result)
