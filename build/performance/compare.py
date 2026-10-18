#!/usr/bin/env python3
"""Run and analyze paired Computator.NET performance observations.

Only the Python standard library is needed. The independently launched worker
process is the replication unit: inner repetitions are reduced to one median
per process before any statistical calculation. See README.md in this directory
for the workload and historical-baseline limitations.
"""

from __future__ import annotations

import argparse
import ctypes
import datetime as dt
import hashlib
import json
import math
import ntpath
import os
from pathlib import Path
import platform
import posixpath
import random
import re
import shutil
import signal
import statistics
import subprocess
import sys
import time
from typing import Any, Callable


SCHEMA = 1
VERSIONS = ("baseline", "candidate")
DEFAULT_PAIRS = 20
MINIMUM_INFERENCE_PAIRS = 15
DEFAULT_BOOTSTRAP_SAMPLES = 50_000
DEFAULT_SEED = 191
DEFAULT_THRESHOLD = 0.10
DEFAULT_CONFIDENCE = 0.95
MINIMUM_EXPECTED_TAIL_DRAWS = 20
PROCESS_WALL_METRIC = "process.wall.ms"
WORKLOAD_DETAIL_KEYS = ("expression", "script", "gridSide", "renderingIncluded", "warmupMinimumMs",
                        "compileWarmupMaximumIterations", "measurementRepetitions", "observerRuntime", "observerBitness")
VERSION_ENVIRONMENT_KEYS = frozenset({"COMPUTATOR_DATA_DIRECTORY"})

# Do not serialize the inherited environment: it can contain credentials. These
# are the only manifest overrides accepted, and they are recorded explicitly.
ENVIRONMENT_CONTROLS = {
    "LANG": "C",
    "LC_ALL": "C",
    "TZ": "UTC",
    "DOTNET_CLI_TELEMETRY_OPTOUT": "1",
    "DOTNET_NOLOGO": "1",
}
ALLOWED_ENVIRONMENT_KEYS = frozenset(ENVIRONMENT_CONTROLS) | {
    "DOTNET_TieredCompilation", "COMPlus_TieredCompilation",
    "DOTNET_TieredPGO", "COMPlus_TieredPGO",
    "DOTNET_ReadyToRun", "COMPlus_ReadyToRun",
    "DOTNET_TC_QuickJit", "COMPlus_TC_QuickJit",
    "DOTNET_TC_QuickJitForLoops", "COMPlus_TC_QuickJitForLoops",
    "DOTNET_gcServer", "COMPlus_gcServer",
    "DOTNET_GCConcurrent", "COMPlus_gcConcurrent",
}
# Runtime installation paths must remain available to a framework-dependent app.
PRESERVED_RUNTIME_ENVIRONMENT_KEYS = frozenset({
    "DOTNET_ROOT", "DOTNET_ROOT_X64", "DOTNET_ROOT_X86", "DOTNET_ROOT_ARM64",
    "DOTNET_ROOT(X86)",
})


class ValidationError(ValueError):
    """An observation, manifest or analysis input cannot support comparison."""


def _require(condition: bool, message: str) -> None:
    if not condition:
        raise ValidationError(message)


def _finite_positive(value: Any) -> bool:
    return (isinstance(value, (int, float)) and not isinstance(value, bool)
            and math.isfinite(value) and value > 0)


def _positive_int(value: Any) -> bool:
    return isinstance(value, int) and not isinstance(value, bool) and value > 0


def _nonempty_string(value: Any) -> bool:
    return isinstance(value, str) and bool(value.strip())


def _valid_schema(value: Any) -> bool:
    return isinstance(value, int) and not isinstance(value, bool) and value == SCHEMA


def _reject_json_constant(value: str) -> None:
    raise ValidationError(f"Non-finite JSON constant is forbidden: {value}")


def read_json(path: Path) -> Any:
    try:
        return json.loads(path.read_text(encoding="utf-8-sig"),
                          parse_constant=_reject_json_constant)
    except (OSError, UnicodeError, json.JSONDecodeError) as error:
        raise ValidationError(f"Cannot read JSON {path}: {error}") from error


def write_json(path: Path, value: Any) -> None:
    """Atomically checkpoint the run after every invocation."""
    path.parent.mkdir(parents=True, exist_ok=True)
    temporary = path.with_name(path.name + ".tmp")
    temporary.write_text(json.dumps(value, indent=2, ensure_ascii=False,
                                    allow_nan=False) + "\n", encoding="utf-8")
    os.replace(temporary, path)


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def _absolute_path(value: str, base: Path) -> Path:
    path = Path(value).expanduser()
    return (path if path.is_absolute() else base / path).resolve()


def _string_arguments(value: Any, location: str, allow_empty: bool = False) -> list[str]:
    _require(isinstance(value, list) and (allow_empty or bool(value)),
             f"{location} must be an argument array")
    _require(all(isinstance(item, str) and "\0" not in item for item in value),
             f"{location} arguments must be strings without NUL")
    return list(value)


def _path_identity(value: str, cwd: str) -> str:
    """Compare archived Windows paths even when analyze-only runs on Unix."""
    windows = (ntpath.isabs(cwd) and not posixpath.isabs(cwd)) or (ntpath.isabs(value) and not posixpath.isabs(value))
    path_module = ntpath if windows else posixpath
    return path_module.normcase(path_module.normpath(path_module.join(cwd, value)))


def _resolve_executable(command: list[str], cwd: Path) -> list[str]:
    result = list(command)
    executable = _absolute_path(command[0], cwd)
    if not executable.is_file():
        found = shutil.which(command[0])
        _require(found is not None, f"Executable cannot be resolved: {command[0]}")
        executable = Path(found).resolve()
    _require(executable.is_file(), f"Executable does not exist: {executable}")
    result[0] = str(executable)
    return result


def command_for(manifest: dict[str, Any], version: str, scenario: str) -> list[str]:
    settings = manifest["scenarios"][scenario]
    command = settings.get("commands", {}).get(version, manifest[version]["command"])
    return command + settings.get("args", {}).get(version, []) + ["--scenario", scenario]


def validate_manifest(data: Any, base: Path, verify_files: bool = True) -> dict[str, Any]:
    """Validate workload contracts and, for execution, pin local binary hashes.

    analyze-only deliberately does not reopen binaries: the archived manifest
    includes the hashes verified immediately before the original run.
    """
    _require(isinstance(data, dict) and _valid_schema(data.get("schema")),
             "Manifest schema must be 1")
    manifest = json.loads(json.dumps(data, allow_nan=False))
    scenarios = manifest.get("scenarios")
    _require(isinstance(scenarios, dict) and bool(scenarios),
             "scenarios must be a nonempty mapping with explicit metric contracts")
    for scenario, settings in scenarios.items():
        _require(isinstance(scenario, str) and re.fullmatch(r"[a-z][a-z0-9_-]*", scenario),
                 f"Invalid scenario identifier: {scenario!r}")
        _require(isinstance(settings, dict), f"scenarios.{scenario} must be an object")
        metrics = settings.get("metrics")
        _require(isinstance(metrics, list) and bool(metrics)
                 and all(isinstance(metric, str) and metric.endswith(".ms") for metric in metrics),
                 f"scenarios.{scenario}.metrics must list millisecond metric names")
        _require(len(metrics) == len(set(metrics)), f"Duplicate metrics in {scenario}")
        operations = settings.get("operations")
        _require(isinstance(operations, dict) and set(operations) == set(metrics),
                 f"scenarios.{scenario}.operations must specify every metric exactly once")
        _require(all(_positive_int(count) for count in operations.values()),
                 f"scenarios.{scenario}.operations must contain positive integer counts")
        _require(operations.get(PROCESS_WALL_METRIC, 1) == 1,
                 "process.wall.ms must have one operation")
        args = settings.get("args", {})
        _require(isinstance(args, dict) and set(args).issubset(VERSIONS),
                 f"scenarios.{scenario}.args only accepts baseline/candidate arrays")
        for version, arguments in args.items():
            _string_arguments(arguments, f"scenarios.{scenario}.args.{version}", True)
        commands = settings.get("commands", {})
        _require(isinstance(commands, dict) and set(commands).issubset(VERSIONS),
                 f"scenarios.{scenario}.commands only accepts baseline/candidate arrays")
        for version, command in commands.items():
            _string_arguments(command, f"scenarios.{scenario}.commands.{version}")

    environment = manifest.setdefault("environment", {})
    _require(isinstance(environment, dict)
             and set(environment).issubset(ALLOWED_ENVIRONMENT_KEYS),
             "Manifest environment contains an unsupported key; arbitrary environment values are not recorded")
    _require(all(isinstance(value, str) and "\0" not in value for value in environment.values()),
             "Manifest environment values must be strings without NUL")

    for version in VERSIONS:
        specification = manifest.get(version)
        _require(isinstance(specification, dict), f"{version} must be an object")
        command = _string_arguments(specification.get("command"), f"{version}.command")
        _require(_nonempty_string(command[0]), f"{version}.command executable is empty")
        _require(_nonempty_string(specification.get("cwd")), f"{version}.cwd is required")
        cwd = _absolute_path(specification["cwd"], base) if verify_files else specification["cwd"]
        if verify_files:
            specification["cwd"] = str(cwd)
        version_environment = specification.setdefault("environment", {})
        _require(isinstance(version_environment, dict) and set(version_environment).issubset(VERSION_ENVIRONMENT_KEYS),
                 f"{version}.environment may only override COMPUTATOR_DATA_DIRECTORY")
        for key, value in version_environment.items():
            _require(isinstance(value, str) and "\0" not in value
                     and (ntpath.isabs(value) or posixpath.isabs(value)),
                     f"{version}.environment.{key} must be an absolute directory path")
            if verify_files:
                _require(Path(value).is_absolute(), f"{version}.environment.{key} is not absolute on this host")
                version_environment[key] = str(Path(value).resolve())
        expected = specification.get("expected")
        _require(isinstance(expected, dict), f"{version}.expected is required")
        _require(_nonempty_string(expected.get("coreVersion")),
                 f"{version}.expected.coreVersion is required")
        _require(isinstance(expected.get("bitness"), int) and expected["bitness"] in (32, 64),
                 f"{version}.expected.bitness must be 32 or 64")
        for runtime_key in ("runtime", "runtimePrefix"):
            if runtime_key in expected:
                _require(_nonempty_string(expected[runtime_key]), f"{version}.expected.{runtime_key} is empty")
        _require(not ("runtime" in expected and "runtimePrefix" in expected),
                 f"{version}.expected must use runtime or runtimePrefix, not both")
        provenance = specification.get("provenance")
        _require(isinstance(provenance, dict), f"{version}.provenance is required")
        for key in ("source", "version", "buildConfiguration"):
            _require(_nonempty_string(provenance.get(key)), f"{version}.provenance.{key} is required")
        _require(provenance["buildConfiguration"].casefold() == "release",
                 f"{version} must declare a Release build")
        commit = provenance.get("commit")
        _require((isinstance(commit, str) and re.fullmatch(r"[0-9a-fA-F]{40}|[0-9a-fA-F]{64}", commit))
                 or (commit is None and _nonempty_string(provenance.get("commitUnknownReason"))),
                 f"{version}.provenance needs a full commit hash or explicit commitUnknownReason")
        artifacts = specification.get("artifacts")
        _require(isinstance(artifacts, list) and bool(artifacts),
                 f"{version}.artifacts must pin the worker and core binaries")
        artifact_paths: set[str] = set()
        roles: dict[str, list[str]] = {"worker": [], "core": [], "observer": []}
        for index, artifact in enumerate(artifacts):
            location = f"{version}.artifacts[{index}]"
            _require(isinstance(artifact, dict) and _nonempty_string(artifact.get("path")),
                     f"{location}.path is required")
            _require(isinstance(artifact.get("sha256"), str)
                     and re.fullmatch(r"[0-9a-fA-F]{64}", artifact["sha256"]),
                     f"{location}.sha256 must be a complete SHA256")
            artifact_path = _absolute_path(artifact["path"], base) if verify_files else artifact["path"]
            identity = _path_identity(str(artifact_path), str(cwd))
            _require(identity not in artifact_paths, f"Duplicate artifact {artifact_path}")
            artifact_paths.add(identity)
            if verify_files:
                artifact["path"] = str(artifact_path)
            artifact["sha256"] = artifact["sha256"].lower()
            role = artifact.get("role")
            _require(role is None or isinstance(role, str), f"{location}.role must be a string")
            if role in roles:
                roles[role].append(identity)
            if verify_files:
                _require(artifact_path.is_file(), f"Artifact does not exist: {artifact_path}")
                actual_hash = sha256_file(artifact_path)
                _require(actual_hash == artifact["sha256"], f"SHA256 mismatch: {artifact_path}")
        _require(len(roles["worker"]) == 1 and len(roles["core"]) == 1,
                 f"{version}.artifacts require exactly one role:worker and one role:core")
        if verify_files:
            _require(cwd.is_dir(), f"Working directory does not exist: {cwd}")
            command = _resolve_executable(command, cwd)
            specification["command"] = command
            actual_hash = sha256_file(Path(command[0]))
            declared_hash = specification.get("executableSha256")
            if declared_hash is not None:
                _require(isinstance(declared_hash, str) and declared_hash.lower() == actual_hash,
                         f"Executable SHA256 mismatch for {version}")
            specification["executableSha256"] = actual_hash
        command_paths = {_path_identity(argument, str(cwd)) for argument in command}
        _require(roles["worker"][0] in command_paths,
                 f"{version}: the pinned worker artifact must appear in its command")
        for scenario, settings in scenarios.items():
            override = settings.get("commands", {}).get(version)
            if override is None:
                continue
            if verify_files:
                override = _resolve_executable(override, cwd)
                settings["commands"][version] = override
                actual_hash = sha256_file(Path(override[0]))
                declared = settings.get("commandExecutableSha256", {}).get(version)
                _require(declared is None or (isinstance(declared, str) and declared.lower() == actual_hash),
                         f"{scenario}/{version}: observer executable SHA256 mismatch")
                settings.setdefault("commandExecutableSha256", {})[version] = actual_hash
            override_paths = {_path_identity(argument, str(cwd)) for argument in override}
            _require(bool(override_paths.intersection(roles["observer"] + roles["worker"])),
                     f"{scenario}/{version}: command override needs a pinned role:observer or role:worker artifact")
    _require(manifest["baseline"]["expected"]["bitness"] == manifest["candidate"]["expected"]["bitness"],
             "Baseline and candidate bitness must match")
    return manifest


def validate_record(record: Any, scenario: str, specification: dict[str, Any],
                    settings: dict[str, Any]) -> dict[str, list[float]]:
    _require(isinstance(record, dict) and _valid_schema(record.get("schema")),
             "Worker record schema must be 1")
    _require(record.get("scenario") == scenario, "Worker scenario mismatch")
    _require(record.get("correctness") is True, "Worker correctness check did not pass")
    expected = specification["expected"]
    _require(record.get("coreVersion") == expected["coreVersion"], "Worker coreVersion mismatch")
    _require(isinstance(record.get("bitness"), int) and record["bitness"] == expected["bitness"],
             "Worker runtime bitness mismatch")
    runtime = record.get("runtime")
    _require(_nonempty_string(runtime), "Worker runtime must be recorded")
    if "runtime" in expected:
        _require(runtime == expected["runtime"], "Worker runtime mismatch")
    if "runtimePrefix" in expected:
        _require(runtime.startswith(expected["runtimePrefix"]), "Worker runtime prefix mismatch")
    metrics = record.get("metrics")
    expected_metrics = set(settings["metrics"]) - {PROCESS_WALL_METRIC}
    _require(isinstance(metrics, dict) and set(metrics) == expected_metrics,
             "Worker metric completeness mismatch (missing or unexpected metric)")
    operations = record.get("operations")
    _require(isinstance(operations, dict) and set(operations) == expected_metrics,
             "Worker operation-count completeness mismatch")
    validated: dict[str, list[float]] = {}
    for metric, observations in metrics.items():
        _require(isinstance(observations, list) and bool(observations),
                 f"{metric}: expected nonempty timing array")
        _require(all(_finite_positive(value) for value in observations),
                 f"{metric}: times must be finite and strictly positive")
        _require(_positive_int(operations[metric])
                 and operations[metric] == settings["operations"][metric],
                 f"{metric}: incompatible operation counts")
        validated[metric] = [float(value) for value in observations]
    details = record.get("details", {})
    _require(isinstance(details, dict), "Worker details must be an object")
    if "stopwatchFrequency" in details:
        _require(_finite_positive(details["stopwatchFrequency"]), "Invalid Stopwatch frequency")
    for key, value in details.items():
        if key.startswith("warmup.") and key.endswith(".budgetReached"):
            _require(isinstance(value, bool), f"{key} must be Boolean")
    return validated


def verify_artifacts(manifest: dict[str, Any]) -> list[str]:
    """Recheck identities after collection to detect files changed during a run."""
    errors = []
    for version in VERSIONS:
        specification = manifest[version]
        artifacts = specification["artifacts"] + [
            {"path": specification["command"][0], "sha256": specification["executableSha256"]}]
        for settings in manifest["scenarios"].values():
            if version in settings.get("commands", {}):
                artifacts.append({"path": settings["commands"][version][0],
                                  "sha256": settings["commandExecutableSha256"][version]})
        for artifact in artifacts:
            try:
                _require(sha256_file(Path(artifact["path"])) == artifact["sha256"],
                         f"{version}: artifact changed during run: {artifact['path']}")
            except (OSError, ValidationError) as error:
                errors.append(str(error))
    return errors


def _utc_now() -> str:
    return dt.datetime.now(dt.timezone.utc).isoformat(timespec="milliseconds")


def controlled_environment(manifest: dict[str, Any], diagnostic_tiering_off: bool) -> tuple[dict[str, str], dict[str, str]]:
    environment = dict(os.environ)
    # Remove inherited JIT/GC/profiler overrides without recording their values.
    # Each application retains its packaged runtimeconfig defaults.
    for key in list(environment):
        if (key.upper().startswith(("DOTNET_", "COMPLUS_", "CORECLR_", "COR_")) and key.upper() not in PRESERVED_RUNTIME_ENVIRONMENT_KEYS) or key.upper() in {"COMPUTATOR_DATA_DIRECTORY", "COMPUTATOR_GSL_DIRECTORY"}:
            del environment[key]
    controls = dict(ENVIRONMENT_CONTROLS)
    controls.update(manifest.get("environment", {}))
    if diagnostic_tiering_off:
        controls.update({"DOTNET_TieredCompilation": "0", "COMPlus_TieredCompilation": "0"})
    environment.update(controls)
    return environment, controls


def execution_mode(manifest: dict[str, Any], diagnostic_tiering_off: bool) -> str:
    if diagnostic_tiering_off:
        return "diagnostic-tiering-off"
    if set(manifest.get("environment", {})) - set(ENVIRONMENT_CONTROLS):
        return "manifest-runtime-overrides"
    return "packaged-runtime-defaults"


def host_metadata() -> dict[str, Any]:
    cpu = platform.processor()
    memory = None
    if sys.platform.startswith("linux"):
        try:
            for line in Path("/proc/cpuinfo").read_text(encoding="utf-8").splitlines():
                if line.lower().startswith("model name"):
                    cpu = line.partition(":")[2].strip()
                    break
            memory = os.sysconf("SC_PAGE_SIZE") * os.sysconf("SC_PHYS_PAGES")
        except (OSError, ValueError):
            pass
    elif os.name == "nt":
        cpu = os.environ.get("PROCESSOR_IDENTIFIER", cpu)
        class MemoryStatus(ctypes.Structure):
            _fields_ = [("length", ctypes.c_ulong), ("load", ctypes.c_ulong)] + [
                (name, ctypes.c_ulonglong) for name in
                ("totalPhysical", "availablePhysical", "totalPage", "availablePage", "totalVirtual", "availableVirtual", "availableExtended")]
        status = MemoryStatus()
        status.length = ctypes.sizeof(status)
        if ctypes.windll.kernel32.GlobalMemoryStatusEx(ctypes.byref(status)):
            memory = status.totalPhysical
    info: dict[str, Any] = {
        "system": platform.system(), "release": platform.release(),
        "osVersion": platform.version(), "architecture": platform.machine(),
        "processor": cpu, "logicalCpuCount": os.cpu_count(),
        "physicalMemoryBytes": memory, "pythonVersion": platform.python_version(),
        "clock": {"implementation": time.get_clock_info("perf_counter").implementation,
                  "resolutionSeconds": time.get_clock_info("perf_counter").resolution},
    }
    if hasattr(os, "sched_getaffinity"):
        info["allowedCpuIndices"] = sorted(os.sched_getaffinity(0))
    if os.name == "nt":
        try:
            power = subprocess.run(["powercfg", "/getactivescheme"], capture_output=True,
                                   text=True, timeout=10, check=False)
            if power.returncode == 0:
                info["activePowerScheme"] = power.stdout.strip()
        except (OSError, subprocess.TimeoutExpired):
            info["activePowerScheme"] = "unavailable"
    return info


class _WindowsJob:
    """Contain worker descendants so a timeout cannot poison the next pair."""

    def __init__(self) -> None:
        from ctypes import wintypes
        class BasicLimits(ctypes.Structure):
            _fields_ = [("perProcessTime", ctypes.c_longlong), ("perJobTime", ctypes.c_longlong),
                        ("flags", wintypes.DWORD), ("minimumWorkingSet", ctypes.c_size_t),
                        ("maximumWorkingSet", ctypes.c_size_t), ("activeProcesses", wintypes.DWORD),
                        ("affinity", ctypes.c_size_t), ("priority", wintypes.DWORD),
                        ("schedulingClass", wintypes.DWORD)]
        class IoCounters(ctypes.Structure):
            _fields_ = [(name, ctypes.c_ulonglong) for name in
                        ("readOps", "writeOps", "otherOps", "readBytes", "writeBytes", "otherBytes")]
        class ExtendedLimits(ctypes.Structure):
            _fields_ = [("basic", BasicLimits), ("io", IoCounters),
                        ("processMemory", ctypes.c_size_t), ("jobMemory", ctypes.c_size_t),
                        ("peakProcessMemory", ctypes.c_size_t), ("peakJobMemory", ctypes.c_size_t)]
        self.kernel = ctypes.WinDLL("kernel32", use_last_error=True)
        self.kernel.CreateJobObjectW.argtypes = [ctypes.c_void_p, wintypes.LPCWSTR]
        self.kernel.CreateJobObjectW.restype = wintypes.HANDLE
        self.kernel.SetInformationJobObject.argtypes = [wintypes.HANDLE, ctypes.c_int, ctypes.c_void_p, wintypes.DWORD]
        self.kernel.SetInformationJobObject.restype = wintypes.BOOL
        self.kernel.AssignProcessToJobObject.argtypes = [wintypes.HANDLE, wintypes.HANDLE]
        self.kernel.AssignProcessToJobObject.restype = wintypes.BOOL
        self.kernel.TerminateJobObject.argtypes = [wintypes.HANDLE, wintypes.UINT]
        self.kernel.TerminateJobObject.restype = wintypes.BOOL
        self.kernel.CloseHandle.argtypes = [wintypes.HANDLE]
        self.kernel.CloseHandle.restype = wintypes.BOOL
        self.handle = self.kernel.CreateJobObjectW(None, None)
        if not self.handle:
            raise OSError(ctypes.get_last_error(), "CreateJobObjectW failed")
        limits = ExtendedLimits()
        limits.basic.flags = 0x2000  # JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
        if not self.kernel.SetInformationJobObject(self.handle, 9, ctypes.byref(limits), ctypes.sizeof(limits)):
            error = ctypes.get_last_error()
            self.close()
            raise OSError(error, "SetInformationJobObject failed")

    def assign(self, process: subprocess.Popen[str]) -> None:
        if not self.kernel.AssignProcessToJobObject(self.handle, int(process._handle)):
            raise OSError(ctypes.get_last_error(), "AssignProcessToJobObject failed")

    def terminate(self) -> None:
        if not self.kernel.TerminateJobObject(self.handle, 1):
            raise OSError(ctypes.get_last_error(), "TerminateJobObject failed")

    def close(self) -> None:
        if self.handle:
            self.kernel.CloseHandle(self.handle)
            self.handle = None


def run_child(command: list[str], cwd: str, environment: dict[str, str], timeout: float) -> dict[str, Any]:
    invocation: dict[str, Any] = {"command": command, "startedUtc": _utc_now(),
                                  "exitCode": None, "timedOut": False,
                                  "stdout": "", "stderr": "", "record": None, "errors": []}
    process = None
    job = None
    job_assigned = False
    start = time.perf_counter()
    end = None

    def terminate() -> None:
        if process is None:
            return
        if job and job_assigned:
            job.terminate()
        elif os.name != "nt":
            try:
                os.killpg(process.pid, signal.SIGKILL)
            except ProcessLookupError:
                pass
        else:
            # Assignment failure must not leave an uncontained worker behind.
            result = subprocess.run(["taskkill", "/F", "/T", "/PID", str(process.pid)],
                                    stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, timeout=10, check=False)
            if result.returncode != 0 and process.poll() is None:
                raise OSError("Could not terminate uncontained Windows process tree")

    try:
        if os.name == "nt":
            job = _WindowsJob()
        start = time.perf_counter()
        process = subprocess.Popen(command, cwd=cwd, env=environment, stdin=subprocess.DEVNULL,
                                   stdout=subprocess.PIPE, stderr=subprocess.PIPE,
                                   text=True, encoding="utf-8-sig", errors="replace",
                                   start_new_session=(os.name != "nt"),
                                   creationflags=(subprocess.CREATE_NEW_PROCESS_GROUP if os.name == "nt" else 0))
        if job:
            job.assign(process)
            job_assigned = True
        try:
            stdout, stderr = process.communicate(timeout=timeout)
        except subprocess.TimeoutExpired:
            invocation["timedOut"] = True
            invocation["errors"].append(f"Process timeout after {timeout:g} seconds")
            terminate()
            stdout, stderr = process.communicate(timeout=10)
        end = time.perf_counter()
        invocation["stdout"] = stdout
        invocation["stderr"] = stderr
        invocation["exitCode"] = process.returncode
        if process.returncode != 0:
            invocation["errors"].append(f"Process exited with code {process.returncode}")
        if not invocation["timedOut"] and process.returncode == 0:
            try:
                invocation["record"] = json.loads(stdout, parse_constant=_reject_json_constant)
            except (ValueError, TypeError) as error:
                invocation["errors"].append(f"Worker stdout must be one JSON record: {error}")
    except (OSError, subprocess.SubprocessError) as error:
        invocation["errors"].append(f"Process launch/cleanup failed: {error}")
        if process is not None:
            try:
                terminate()
                stdout, stderr = process.communicate(timeout=10)
                invocation.update(stdout=stdout, stderr=stderr, exitCode=process.returncode)
            except (OSError, subprocess.SubprocessError) as cleanup_error:
                invocation["errors"].append(f"FATAL: process cleanup failed: {cleanup_error}")
                invocation["unsafeToContinue"] = True
    except BaseException:
        # A user interrupt should leave an incomplete checkpoint, not a detached
        # workload that silently contends with a subsequent run.
        try:
            terminate()
            if process is not None:
                process.communicate(timeout=10)
        finally:
            raise
    finally:
        invocation["processWallMs"] = ((end if end is not None else time.perf_counter()) - start) * 1000.0
        if job:
            job.close()
    return invocation


def _configuration(pairs: int, timeout: float, bootstrap: int, seed: int,
                   confidence: float, threshold: float, diagnostic_tiering_off: bool) -> dict[str, Any]:
    _require(_positive_int(pairs), "pairs must be a positive integer")
    _require(_finite_positive(timeout), "timeout must be finite and positive")
    _require(_positive_int(bootstrap), "bootstrap samples must be a positive integer")
    _require(isinstance(seed, int) and not isinstance(seed, bool), "seed must be an integer")
    _require(isinstance(confidence, (int, float)) and not isinstance(confidence, bool)
             and math.isfinite(confidence) and 0 < confidence < 1, "confidence must lie between zero and one")
    _require(_finite_positive(threshold), "practical regression threshold must be finite and positive")
    return {"pairs": pairs, "timeoutSeconds": timeout, "bootstrapSamples": bootstrap,
            "seed": seed, "confidence": confidence, "practicalThreshold": threshold,
            "minimumInferencePairs": MINIMUM_INFERENCE_PAIRS,
            "mode": "diagnostic-tiering-off" if diagnostic_tiering_off else "packaged-runtime-defaults",
            "pairOrder": "counterbalanced AB/BA; scenario order rotates by pair",
            "cacheCondition": "fresh processes; operating-system file cache and native-library extraction caches are not reset",
            "replicationUnit": "one sequential baseline/candidate process pair",
            "withinProcessReduction": "median of the declared inner timing repetitions"}


def collect(manifest: dict[str, Any], output: Path, *, pairs: int = DEFAULT_PAIRS,
            timeout: float = 120, bootstrap: int = DEFAULT_BOOTSTRAP_SAMPLES,
            seed: int = DEFAULT_SEED, confidence: float = DEFAULT_CONFIDENCE,
            threshold: float = DEFAULT_THRESHOLD, diagnostic_tiering_off: bool = False,
            invoke: Callable[..., dict[str, Any]] = run_child) -> dict[str, Any]:
    configuration = _configuration(pairs, timeout, bootstrap, seed, confidence, threshold, diagnostic_tiering_off)
    configuration["mode"] = execution_mode(manifest, diagnostic_tiering_off)
    environment, controls = controlled_environment(manifest, diagnostic_tiering_off)
    raw: dict[str, Any] = {"schema": SCHEMA, "kind": "computator.performance.raw",
                           "createdUtc": _utc_now(), "completed": False,
                           "configuration": configuration, "host": host_metadata(),
                           "supervisorSha256": sha256_file(Path(__file__)),
                           "manifest": manifest, "environmentControls": controls,
                           "versionEnvironmentControls": {version: manifest[version].get("environment", {}) for version in VERSIONS},
                           "runtimeEnvironmentPolicy": "Inherited DOTNET/COMPlus/CORECLR/COR tuning and profiling settings removed; installation paths retained. Environment values outside the explicit controls are not recorded.",
                           "trials": []}
    output.mkdir(parents=True, exist_ok=True)
    logs = output / "child-logs"
    logs.mkdir(exist_ok=True)
    write_json(output / "raw.json", raw)
    scenarios = list(manifest["scenarios"])
    for pair in range(pairs):
        # Rotating scenarios spreads time/thermal drift across the workload matrix.
        rotation = pair % len(scenarios)
        for scenario in scenarios[rotation:] + scenarios[:rotation]:
            order = list(VERSIONS if pair % 2 == 0 else reversed(VERSIONS))
            trial: dict[str, Any] = {"id": f"pair-{pair:04d}-{scenario}", "pair": pair,
                                     "scenario": scenario, "order": order, "invocations": []}
            raw["trials"].append(trial)
            settings = manifest["scenarios"][scenario]
            for version in order:
                command = command_for(manifest, version, scenario)
                version_environment = manifest[version].get("environment", {})
                child_environment = dict(environment)
                child_environment.update(version_environment)
                print(f"pair {pair + 1}/{pairs} {scenario} {version}", flush=True)
                invocation = invoke(command, manifest[version]["cwd"], child_environment, timeout)
                invocation.update({"id": f"{trial['id']}-{version}", "version": version,
                                   "environmentControls": {**controls, **version_environment}})
                if not invocation.get("errors"):
                    try:
                        validate_record(invocation.get("record"), scenario, manifest[version], settings)
                    except ValidationError as error:
                        invocation.setdefault("errors", []).append(str(error))
                trial["invocations"].append(invocation)
                for channel in ("stdout", "stderr"):
                    (logs / f"{invocation['id']}.{channel}.txt").write_text(invocation.get(channel, ""), encoding="utf-8")
                write_json(output / "raw.json", raw)
                if invocation.get("unsafeToContinue"):
                    raw["abortReason"] = "Process cleanup could not be verified; stopping to avoid overlapping workloads"
                    write_json(output / "raw.json", raw)
                    return raw
    raw["completed"] = True
    raw["completedUtc"] = _utc_now()
    artifact_errors = verify_artifacts(manifest)
    raw["postRunArtifactVerification"] = {"passed": not artifact_errors, "errors": artifact_errors}
    write_json(output / "raw.json", raw)
    return raw


def percentile(sorted_values: list[float], probability: float) -> float:
    """Linear interpolation at a quantile of an already sorted sample."""
    _require(bool(sorted_values) and 0 <= probability <= 1, "Invalid percentile input")
    position = (len(sorted_values) - 1) * probability
    lower = int(math.floor(position))
    upper = int(math.ceil(position))
    weight = position - lower
    return sorted_values[lower] * (1 - weight) + sorted_values[upper] * weight


def paired_log_ratio_interval(baseline: list[float], candidate: list[float], *,
                              samples: int, seed: int, tail_probability: float) -> tuple[float, float, float]:
    _require(bool(baseline) and len(baseline) == len(candidate), "Paired observations must be complete")
    _require(all(_finite_positive(value) for value in baseline + candidate), "Paired observations must be finite and positive")
    _require(_positive_int(samples), "Bootstrap sample count must be positive")
    _require(0 < tail_probability < 0.5, "Invalid bootstrap tail probability")
    logs = [math.log(c) - math.log(b) for b, c in zip(baseline, candidate)]
    point = math.exp(statistics.fmean(logs))
    if len(set(logs)) == 1:
        return point, point, point
    generator = random.Random(seed)
    count = len(logs)
    means = sorted(statistics.fmean(logs[generator.randrange(count)] for _ in range(count))
                   for _ in range(samples))
    return (point, math.exp(percentile(means, tail_probability)),
            math.exp(percentile(means, 1 - tail_probability)))


def classify(lower: float, upper: float, threshold: float, *, pairs: int,
             expected_tail_draws: float, resolution_limited: bool = False) -> str:
    if pairs < MINIMUM_INFERENCE_PAIRS:
        return "exploratory-insufficient-pairs"
    if expected_tail_draws < MINIMUM_EXPECTED_TAIL_DRAWS:
        return "exploratory-insufficient-bootstrap-resolution"
    if resolution_limited:
        return "exploratory-timer-resolution-limited"
    if lower > 1 + threshold:
        return "regression"
    if upper < 1 / (1 + threshold):
        return "improvement"
    if lower >= 1 / (1 + threshold) and upper <= 1 + threshold:
        return "within-practical-band"
    return "inconclusive"


def analyze(raw: Any, *, bootstrap: int | None = None, seed: int | None = None) -> dict[str, Any]:
    _require(isinstance(raw, dict) and _valid_schema(raw.get("schema"))
             and raw.get("kind") == "computator.performance.raw", "Input must be a raw schema-1 benchmark run")
    manifest = validate_manifest(raw.get("manifest"), Path.cwd(), verify_files=False)
    saved = raw.get("configuration")
    _require(isinstance(saved, dict), "Run configuration is missing")
    configuration = _configuration(saved.get("pairs"), saved.get("timeoutSeconds"),
                                   bootstrap if bootstrap is not None else saved.get("bootstrapSamples"),
                                   seed if seed is not None else saved.get("seed"),
                                   saved.get("confidence"), saved.get("practicalThreshold"),
                                   saved.get("mode") == "diagnostic-tiering-off")
    configuration["mode"] = execution_mode(manifest, saved.get("mode") == "diagnostic-tiering-off")
    _require(saved.get("mode") == configuration["mode"], "Execution mode does not match recorded runtime overrides")
    controls = dict(ENVIRONMENT_CONTROLS)
    controls.update(manifest.get("environment", {}))
    if saved["mode"] == "diagnostic-tiering-off":
        controls.update({"DOTNET_TieredCompilation": "0", "COMPlus_TieredCompilation": "0"})
    _require(raw.get("environmentControls") == controls, "Recorded environment controls differ from the manifest/mode")
    trials = raw.get("trials")
    _require(isinstance(trials, list), "Run trials are missing")
    expected_pairs = configuration["pairs"]
    global_errors: list[str] = []
    if raw.get("completed") is not True:
        global_errors.append("Run is incomplete or was interrupted")
    verification = raw.get("postRunArtifactVerification")
    if not isinstance(verification, dict) or verification.get("passed") is not True or verification.get("errors") != []:
        global_errors.append("Post-run binary identity verification is missing or failed")
        if isinstance(verification, dict) and isinstance(verification.get("errors"), list):
            global_errors.extend(str(error) for error in verification["errors"])
    by_scenario: dict[str, dict[int, dict[str, Any]]] = {scenario: {} for scenario in manifest["scenarios"]}
    seen_trial_ids: set[str] = set()
    seen_invocation_ids: set[str] = set()
    for trial in trials:
        _require(isinstance(trial, dict), "Trial must be an object")
        scenario = trial.get("scenario")
        _require(scenario in by_scenario, f"Unexpected scenario in raw trials: {scenario}")
        pair = trial.get("pair")
        _require(isinstance(pair, int) and not isinstance(pair, bool) and 0 <= pair < expected_pairs,
                 "Trial pair index is invalid")
        trial_id = f"pair-{pair:04d}-{scenario}"
        _require(trial.get("id") == trial_id and trial_id not in seen_trial_ids,
                 "Trial identifier is duplicate or does not match its pair/scenario")
        seen_trial_ids.add(trial_id)
        _require(pair not in by_scenario[scenario], "Duplicate scenario pair")
        expected_order = list(VERSIONS if pair % 2 == 0 else reversed(VERSIONS))
        _require(trial.get("order") == expected_order, "Trial AB/BA order does not match configuration")
        invocations = trial.get("invocations")
        _require(isinstance(invocations, list), "Trial invocations must be an array")
        _require(len(invocations) <= 2, "Trial contains excess invocations")
        for index, invocation in enumerate(invocations):
            _require(isinstance(invocation, dict), "Invocation must be an object")
            version = invocation.get("version")
            _require(version == expected_order[index], "Invocation order/version mismatch")
            invocation_id = f"{trial_id}-{version}"
            _require(invocation.get("id") == invocation_id and invocation_id not in seen_invocation_ids,
                     "Invocation identifier is duplicate or does not match trial/version")
            seen_invocation_ids.add(invocation_id)
        by_scenario[scenario][pair] = trial

    metric_count = sum(len(settings["metrics"]) for settings in manifest["scenarios"].values())
    tail = (1 - configuration["confidence"]) / (2 * metric_count)
    analysis: dict[str, Any] = {
        "schema": SCHEMA, "kind": "computator.performance.analysis", "createdUtc": _utc_now(),
        "sourceCreatedUtc": raw.get("createdUtc"), "configuration": configuration,
        "host": raw.get("host"), "environmentControls": raw.get("environmentControls"),
        "versionEnvironmentControls": {version: manifest[version].get("environment", {}) for version in VERSIONS},
        "provenance": {version: manifest[version]["provenance"] for version in VERSIONS},
        "statistics": {"method": "paired percentile bootstrap of the mean log candidate/baseline latency ratio",
                       "pointEstimate": "geometric mean of paired per-process median ratios",
                       "multiplicity": "Bonferroni across every declared scenario/metric, including invalid metrics",
                       "familyConfidence": configuration["confidence"], "declaredComparisonCount": metric_count,
                       "marginalConfidence": 1 - 2 * tail,
                       "expectedBootstrapDrawsPerTail": configuration["bootstrapSamples"] * tail,
                       "qualification": "Bootstrap confidence intervals are approximate; process independence and a stable host remain assumptions."},
        "errors": global_errors, "metrics": [],
    }
    for scenario, settings in manifest["scenarios"].items():
        errors = list(global_errors)
        values: dict[str, dict[str, list[float]]] = {
            metric: {version: [] for version in VERSIONS} for metric in settings["metrics"]}
        timing_floors = {metric: [] for metric in settings["metrics"]}
        repetition_signatures = set()
        workload_signatures = set()
        warmup_notes = set()
        records_complete = 0
        runtime_signatures: dict[str, set[tuple[str, Any]]] = {version: set() for version in VERSIONS}
        for pair in range(expected_pairs):
            trial = by_scenario[scenario].get(pair)
            if trial is None or len(trial["invocations"]) != 2:
                errors.append(f"{scenario} pair {pair}: missing process pair")
                continue
            validated_pair: dict[str, tuple[dict[str, list[float]], dict[str, Any]]] = {}
            for invocation in trial["invocations"]:
                version = invocation["version"]
                try:
                    _require(type(invocation.get("exitCode")) is int and invocation["exitCode"] == 0
                             and invocation.get("timedOut") is False,
                             "Process failed or timed out")
                    _require(invocation.get("errors") == [], "Invocation has recorded validation/launch errors")
                    _require(not invocation.get("unsafeToContinue"), "Process cleanup was unverified")
                    expected_command = command_for(manifest, version, scenario)
                    _require(invocation.get("command") == expected_command, "Invocation command differs from manifest")
                    _require(invocation.get("environmentControls") == {**controls, **manifest[version].get("environment", {})},
                             "Invocation environment controls differ from manifest")
                    measurements = validate_record(invocation.get("record"), scenario, manifest[version], settings)
                    if PROCESS_WALL_METRIC in values:
                        _require(_finite_positive(invocation.get("processWallMs")), "Invalid supervisor process wall time")
                        measurements[PROCESS_WALL_METRIC] = [float(invocation["processWallMs"])]
                    record = invocation["record"]
                    runtime_signatures[version].add((record["runtime"], record["bitness"]))
                    repetition_signatures.add(tuple(sorted((metric, len(observations)) for metric, observations in measurements.items())))
                    details = record.get("details", {})
                    workload_signatures.add(json.dumps({key: details[key] for key in WORKLOAD_DETAIL_KEYS if key in details}, sort_keys=True))
                    for key, value in details.items():
                        if key.startswith("warmup.") and key.endswith(".budgetReached") and value is False:
                            warmup_notes.add(f"{version}: {key} was false in at least one process; the bounded warmup did not meet its wall-time target, so these timings do not establish steady state")
                    validated_pair[version] = (measurements, record)
                except ValidationError as error:
                    errors.append(f"{scenario} pair {pair} {version}: {error}")
            if len(validated_pair) != 2:
                continue
            records_complete += 1
            for version, (measurements, record) in validated_pair.items():
                for metric, observations in measurements.items():
                    values[metric][version].append(statistics.median(observations))
                    frequency = record.get("details", {}).get("stopwatchFrequency")
                    if metric != PROCESS_WALL_METRIC and frequency:
                        timing_floors[metric].append(min(observations) <= 10_000.0 / frequency)
        for version, signatures in runtime_signatures.items():
            if len(signatures) > 1:
                errors.append(f"{scenario}: {version} runtime changed between process pairs")
        if len(repetition_signatures) > 1:
            errors.append(f"{scenario}: inner timing repetition counts changed between versions or process pairs")
        if len(workload_signatures) > 1:
            errors.append(f"{scenario}: workload or warmup settings changed between versions or process pairs")
        for metric in settings["metrics"]:
            row: dict[str, Any] = {"scenario": scenario, "metric": metric,
                                   "operationsPerObservation": settings["operations"][metric],
                                   "expectedPairs": expected_pairs, "completePairs": records_complete,
                                   "warmupNotes": sorted(warmup_notes)}
            if errors:
                row.update({"classification": "invalid", "errors": errors,
                            "baselineProcessMediansMs": values[metric]["baseline"],
                            "candidateProcessMediansMs": values[metric]["candidate"]})
                analysis["metrics"].append(row)
                continue
            baseline = values[metric]["baseline"]
            candidate = values[metric]["candidate"]
            stable_seed = configuration["seed"] + int.from_bytes(hashlib.sha256(f"{scenario}/{metric}".encode()).digest()[:8], "big")
            point, lower, upper = paired_log_ratio_interval(baseline, candidate, samples=configuration["bootstrapSamples"],
                                                            seed=stable_seed, tail_probability=tail)
            b_median, c_median = statistics.median(baseline), statistics.median(candidate)
            resolution_limited = any(timing_floors[metric])
            row.update({
                "classification": classify(lower, upper, configuration["practicalThreshold"],
                                           pairs=expected_pairs,
                                           expected_tail_draws=configuration["bootstrapSamples"] * tail,
                                           resolution_limited=resolution_limited),
                "baselineMedianMs": b_median, "candidateMedianMs": c_median,
                "baselineMedianMsPerOperation": b_median / settings["operations"][metric],
                "candidateMedianMsPerOperation": c_median / settings["operations"][metric],
                "medianPairedDifferenceMs": statistics.median(c - b for b, c in zip(baseline, candidate)),
                "geometricMeanPairedRatio": point, "relativeChangePercent": (point - 1) * 100,
                "ratioInterval": {"lower": lower, "upper": upper},
                "baselineProcessMediansMs": baseline, "candidateProcessMediansMs": candidate,
                "timerResolutionLimited": resolution_limited,
            })
            analysis["metrics"].append(row)
    analysis["valid"] = all(row["classification"] != "invalid" for row in analysis["metrics"])
    analysis["regressionCount"] = sum(row["classification"] == "regression" for row in analysis["metrics"])
    return analysis


def render_report(analysis: dict[str, Any]) -> str:
    def escape(value: Any) -> str:
        return str(value).replace("|", "\\|").replace("\n", " ")
    configuration = analysis["configuration"]
    statistics_info = analysis["statistics"]
    lines = ["# Computator.NET paired performance comparison", "",
             f"Validity: **{'complete' if analysis['valid'] else 'INVALID — failures or missing observations'}**. "
             f"Execution mode: `{configuration['mode']}`.", "",
             "## Baselines and conditions", ""]
    for version in VERSIONS:
        provenance = analysis["provenance"][version]
        commit = provenance.get("commit") or f"unknown ({provenance['commitUnknownReason']})"
        lines.append(f"- **{version.capitalize()}:** {escape(provenance['version'])}; {escape(provenance['buildConfiguration'])}; "
                     f"commit `{escape(commit)}`; source: {escape(provenance['source'])}.")
    lines += ["", f"- Requested process pairs per scenario: **{configuration['pairs']}**; inference requires at least {MINIMUM_INFERENCE_PAIRS}.",
              f"- {configuration['cacheCondition']}.",
              "- Trials run sequentially, counterbalancing baseline/candidate launch order and rotating scenario order.",
              "- Each process contributes the median of its inner repetitions; inner repetitions are not independent samples.",
              "- Milliseconds below describe the complete timed observation/batch. Operations per observation are fixed by the manifest.",
              "", "## Measurements", "",
              "| Scenario / metric | Ops | Baseline median ms | Candidate median ms | Paired Δ ms | Paired ratio | Adjusted interval | Classification |",
              "|---|---:|---:|---:|---:|---:|---|---|"]
    for row in analysis["metrics"]:
        name = escape(f"{row['scenario']} / {row['metric']}")
        if row["classification"] == "invalid":
            lines.append(f"| {name} | {row['operationsPerObservation']} | — | — | — | — | — | invalid ({row['completePairs']}/{row['expectedPairs']} complete pairs) |")
            continue
        interval = row["ratioInterval"]
        lines.append(f"| {name} | {row['operationsPerObservation']} | {row['baselineMedianMs']:.6g} | {row['candidateMedianMs']:.6g} | "
                     f"{row['medianPairedDifferenceMs']:+.6g} | {row['geometricMeanPairedRatio']:.4f}× | "
                     f"{interval['lower']:.4f}–{interval['upper']:.4f}× | {row['classification']} |")
    threshold = configuration["practicalThreshold"]
    lines += ["", "## Interpretation", "",
              f"Ratios are candidate/baseline; values above 1 mean higher latency. The point estimate is the geometric mean of paired process-median ratios. "
              f"The displayed baseline and candidate milliseconds are medians across processes; their quotient need not equal the paired ratio.", "",
              f"Intervals use {configuration['bootstrapSamples']:,} seeded paired-bootstrap draws, with Bonferroni adjustment across "
              f"{statistics_info['declaredComparisonCount']} declared comparisons for an approximate {100 * statistics_info['familyConfidence']:.1f}% family confidence level. "
              f"Each interval has nominal {100 * statistics_info['marginalConfidence']:.5f}% marginal confidence. Bootstrap intervals remain approximate, especially with few process pairs.", "",
              f"A regression requires the entire interval above {1 + threshold:.3f}×; an improvement requires the entire interval below {1 / (1 + threshold):.3f}×. "
              "The practical band is reciprocal, so the threshold is symmetric when baseline and candidate are exchanged. "
              "`within-practical-band` means the complete interval is inside those boundaries; `inconclusive` does not prove equivalence. "
              f"Fewer than {MINIMUM_INFERENCE_PAIRS} pairs, fewer than {MINIMUM_EXPECTED_TAIL_DRAWS} expected bootstrap draws per adjusted tail, "
              "or observations within ten timer ticks suppress inferential labels.", "",
              "Failures are retained and invalidate all comparisons for the affected scenario; successful pairs are never silently substituted for a complete run. "
              "Runtime, bitness, operation counts, metric completeness and correctness must all match their declared contracts. "
              "A declared source/version is provenance supplied by the person preparing the manifest; SHA256 verification establishes binary identity, not source authenticity.", "",
              "Fresh processes reset process-local JIT/compiler caches. They do not create a cold disk or reset the operating system file cache. "
              "Thermal state, host contention and power management can still affect a run. Diagnostic tiering-off results require separate interpretation from the application defaults.", ""]
    invalid_errors = sorted({error for row in analysis["metrics"] for error in row.get("errors", [])})
    warmup_notes = sorted({f"{row['scenario']}: {note}" for row in analysis["metrics"] for note in row.get("warmupNotes", [])})
    if warmup_notes:
        lines += ["## Warmup observations", ""] + [f"- {escape(note)}" for note in warmup_notes] + [""]
    if invalid_errors:
        lines += ["## Invalid observations", ""] + [f"- {escape(error)}" for error in invalid_errors] + [""]
    lines += ["Raw worker output, stderr, individual timings, binary hashes, host metadata, controls and all failed invocations are retained in `raw.json` and `child-logs/`. "
              "`analysis.json` also includes process medians and milliseconds per operation.", ""]
    return "\n".join(lines)


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    subparsers = parser.add_subparsers(dest="command", required=True)
    runner = subparsers.add_parser("run", help="Collect sequential paired fresh-process observations")
    runner.add_argument("--manifest", type=Path, required=True)
    runner.add_argument("--output", type=Path, required=True, help="New output directory (must not already contain raw.json)")
    runner.add_argument("--pairs", type=int, default=DEFAULT_PAIRS)
    runner.add_argument("--timeout", type=float, default=120)
    runner.add_argument("--bootstrap", type=int, default=DEFAULT_BOOTSTRAP_SAMPLES)
    runner.add_argument("--seed", type=int, default=DEFAULT_SEED)
    runner.add_argument("--confidence", type=float, default=DEFAULT_CONFIDENCE)
    runner.add_argument("--threshold", type=float, default=DEFAULT_THRESHOLD)
    runner.add_argument("--diagnostic-tiering-off", action="store_true")
    analyzer = subparsers.add_parser("analyze", help="Reproduce analysis using archived raw records only")
    analyzer.add_argument("--input", type=Path, required=True)
    analyzer.add_argument("--output", type=Path, required=True)
    analyzer.add_argument("--bootstrap", type=int)
    analyzer.add_argument("--seed", type=int)
    arguments = parser.parse_args(argv)
    try:
        if arguments.command == "run":
            _require(not (arguments.output / "raw.json").exists(), "Output already contains raw.json; select a fresh directory to preserve the previous run")
            manifest_path = arguments.manifest.resolve()
            manifest = validate_manifest(read_json(manifest_path), manifest_path.parent)
            raw = collect(manifest, arguments.output, pairs=arguments.pairs, timeout=arguments.timeout,
                          bootstrap=arguments.bootstrap, seed=arguments.seed, confidence=arguments.confidence,
                          threshold=arguments.threshold, diagnostic_tiering_off=arguments.diagnostic_tiering_off)
        else:
            raw = read_json(arguments.input)
        analysis = analyze(raw, bootstrap=arguments.bootstrap, seed=arguments.seed)
        arguments.output.mkdir(parents=True, exist_ok=True)
        write_json(arguments.output / "analysis.json", analysis)
        (arguments.output / "report.md").write_text(render_report(analysis), encoding="utf-8")
        print(f"Report: {arguments.output / 'report.md'}")
        print(f"Valid: {analysis['valid']}; practical regressions: {analysis['regressionCount']}")
        return 0 if analysis["valid"] else 2
    except (ValidationError, OSError, OverflowError) as error:
        print(f"Performance comparison invalid: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
