"""Contract/statistics tests; these are not measurements of product performance."""

from __future__ import annotations

import contextlib
import copy
import importlib.util
import io
import json
from pathlib import Path
import statistics
import sys
import tempfile
import unittest
from unittest import mock


MODULE_SPEC = importlib.util.spec_from_file_location("performance_compare", Path(__file__).with_name("compare.py"))
compare = importlib.util.module_from_spec(MODULE_SPEC)
MODULE_SPEC.loader.exec_module(compare)


def manifest_fixture(metrics=None, scenarios=None):
    metrics = metrics or ["evaluate.batch.ms"]
    scenario_names = scenarios or ["real"]
    manifest = {
        "schema": 1,
        "scenarios": {name: {"metrics": list(metrics), "operations": {metric: 100 for metric in metrics}}
                      for name in scenario_names},
        "environment": {},
    }
    for version, core in (("baseline", "2.3.2.0"), ("candidate", "3.0.0.0")):
        manifest[version] = {
            "command": [sys.executable, "worker.py", "--test-version", version],
            "cwd": str(Path.cwd()),
            "expected": {"coreVersion": core, "bitness": 64, "runtimePrefix": "CLR "},
            "provenance": {"version": core, "source": "test fixture only", "commit": "a" * 40,
                           "buildConfiguration": "Release"},
            "artifacts": [{"path": str(Path.cwd() / "test-only-core.dll"), "sha256": "b" * 64, "role": "core"},
                          {"path": sys.executable, "sha256": "c" * 64, "role": "worker"}],
        }
    return manifest


def record_fixture(manifest, version, scenario, values=10.0):
    settings = manifest["scenarios"][scenario]
    metrics = [name for name in settings["metrics"] if name != compare.PROCESS_WALL_METRIC]
    return {
        "schema": 1, "scenario": scenario, "runtime": "CLR fixture", "bitness": 64,
        "coreVersion": manifest[version]["expected"]["coreVersion"], "correctness": True,
        "metrics": {name: list(values) if isinstance(values, list) else [values] for name in metrics},
        "operations": {name: settings["operations"][name] for name in metrics},
        "details": {"stopwatchFrequency": 10_000_000},
    }


def raw_fixture(pairs=20, baseline=10.0, candidate=12.0, metrics=None, scenarios=None, bootstrap=2000):
    manifest = manifest_fixture(metrics, scenarios)
    raw = {"schema": 1, "kind": "computator.performance.raw", "createdUtc": "2026-10-18T00:00:00Z",
           "completed": True, "host": {"system": "synthetic test fixture"}, "environmentControls": dict(compare.ENVIRONMENT_CONTROLS),
           "postRunArtifactVerification": {"passed": True, "errors": []},
           "configuration": compare._configuration(pairs, 120, bootstrap, 191, .95, .1, False),
           "manifest": manifest, "trials": []}
    for pair in range(pairs):
        order = list(compare.VERSIONS if pair % 2 == 0 else reversed(compare.VERSIONS))
        for scenario in manifest["scenarios"]:
            trial_id = f"pair-{pair:04d}-{scenario}"
            trial = {"id": trial_id, "pair": pair, "scenario": scenario, "order": order, "invocations": []}
            for version in order:
                value = baseline if version == "baseline" else candidate
                if callable(value):
                    value = value(pair)
                record = record_fixture(manifest, version, scenario, value)
                trial["invocations"].append({
                    "id": f"{trial_id}-{version}", "version": version,
                    "command": manifest[version]["command"] + ["--scenario", scenario],
                    "exitCode": 0, "timedOut": False, "errors": [],
                    "environmentControls": dict(compare.ENVIRONMENT_CONTROLS),
                    "stdout": json.dumps(record), "stderr": "", "record": record,
                    "processWallMs": 200.0 if version == "baseline" else 210.0,
                })
            raw["trials"].append(trial)
    return raw


class WorkerContractTests(unittest.TestCase):
    def setUp(self):
        self.manifest = manifest_fixture()
        self.record = record_fixture(self.manifest, "baseline", "real")

    def validate(self, record):
        return compare.validate_record(record, "real", self.manifest["baseline"], self.manifest["scenarios"]["real"])

    def test_rejects_zero_negative_boolean_nan_and_infinite_timings(self):
        for invalid in [0, -1, True, False, float("nan"), float("inf"), -float("inf"), "10", None]:
            with self.subTest(value=invalid):
                record = copy.deepcopy(self.record)
                record["metrics"]["evaluate.batch.ms"] = [10.0, invalid]
                with self.assertRaises(compare.ValidationError):
                    self.validate(record)

    def test_correctness_must_be_literal_true(self):
        for value in [False, None, 1, "true", "passed"]:
            with self.subTest(value=value):
                record = copy.deepcopy(self.record)
                record["correctness"] = value
                with self.assertRaisesRegex(compare.ValidationError, "correctness"):
                    self.validate(record)

    def test_version_bitness_scenario_and_runtime_are_checked(self):
        for key, value in [("coreVersion", "2.4.0.0"), ("bitness", 32), ("bitness", 64.0),
                           ("schema", True), ("scenario", "script"), ("runtime", ".NET unexpected")]:
            with self.subTest(key=key):
                record = copy.deepcopy(self.record)
                record[key] = value
                with self.assertRaises(compare.ValidationError):
                    self.validate(record)

    def test_missing_unexpected_and_empty_metrics_rejected(self):
        for metrics in [{}, {"evaluate.batch.ms": [10], "unplanned.ms": [10]}, {"evaluate.batch.ms": []}]:
            with self.subTest(metrics=metrics):
                record = copy.deepcopy(self.record)
                record["metrics"] = metrics
                with self.assertRaises(compare.ValidationError):
                    self.validate(record)

    def test_operation_counts_must_match_exactly(self):
        for operations in [{}, {"evaluate.batch.ms": 50}, {"evaluate.batch.ms": True}, {"evaluate.batch.ms": 100, "other.ms": 1}]:
            with self.subTest(operations=operations):
                record = copy.deepcopy(self.record)
                record["operations"] = operations
                with self.assertRaises(compare.ValidationError):
                    self.validate(record)

    def test_accepts_finite_positive_observations_without_coercing_strings(self):
        self.record["metrics"]["evaluate.batch.ms"] = [0.01, 1, 2.5]
        self.assertEqual(self.validate(self.record), {"evaluate.batch.ms": [0.01, 1.0, 2.5]})

    def test_json_nan_is_not_accepted(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "bad.json"
            path.write_text('{"time": NaN}', encoding="utf-8")
            with self.assertRaises(compare.ValidationError):
                compare.read_json(path)


class StatisticsTests(unittest.TestCase):
    def test_pair_resampling_preserves_a_constant_ratio_despite_large_baseline_variation(self):
        baseline = [1.0, 10.0, 100.0, 1000.0] * 5
        candidate = [value * 2 for value in baseline]
        point, lower, upper = compare.paired_log_ratio_interval(baseline, candidate, samples=500, seed=8, tail_probability=.025)
        for value in (point, lower, upper):
            self.assertAlmostEqual(value, 2.0, places=12)

    def test_bootstrap_is_seeded_and_reproducible(self):
        baseline = [10.0] * 20
        candidate = [10 + index / 10 for index in range(20)]
        first = compare.paired_log_ratio_interval(baseline, candidate, samples=1000, seed=191, tail_probability=.025)
        second = compare.paired_log_ratio_interval(baseline, candidate, samples=1000, seed=191, tail_probability=.025)
        self.assertEqual(first, second)
        self.assertLess(first[1], first[0])
        self.assertGreater(first[2], first[0])

    def test_inner_repetitions_reduce_to_one_process_median(self):
        raw = raw_fixture(baseline=[1.0, 10.0, 1000.0], candidate=[1.0, 20.0, 1000.0])
        row = compare.analyze(raw)["metrics"][0]
        self.assertEqual(row["completePairs"], 20)
        self.assertEqual(len(row["baselineProcessMediansMs"]), 20)
        self.assertEqual(row["baselineMedianMs"], 10)
        self.assertEqual(row["candidateMedianMs"], 20)
        self.assertEqual(row["baselineMedianMsPerOperation"], .1)
        self.assertAlmostEqual(row["geometricMeanPairedRatio"], 2)
        self.assertEqual(row["classification"], "regression")

    def test_point_estimate_is_paired_geometric_mean_not_ratio_of_marginal_medians(self):
        baseline = [1., 2., 100.]
        candidate = [1., 20., 200.]
        raw = raw_fixture(pairs=15, baseline=lambda pair: baseline[pair % 3], candidate=lambda pair: candidate[pair % 3])
        row = compare.analyze(raw)["metrics"][0]
        self.assertAlmostEqual(row["geometricMeanPairedRatio"], 20 ** (1 / 3))
        self.assertNotAlmostEqual(row["geometricMeanPairedRatio"], row["candidateMedianMs"] / row["baselineMedianMs"])

    def test_minimum_independent_pair_count_is_enforced(self):
        row = compare.analyze(raw_fixture(pairs=14, candidate=20))["metrics"][0]
        self.assertEqual(row["classification"], "exploratory-insufficient-pairs")
        row = compare.analyze(raw_fixture(pairs=15, candidate=20))["metrics"][0]
        self.assertEqual(row["classification"], "regression")

    def test_bootstrap_tail_resolution_is_enforced_after_multiplicity_adjustment(self):
        raw = raw_fixture(candidate=20, metrics=[f"metric{index}.ms" for index in range(10)], bootstrap=2000)
        analysis = compare.analyze(raw)
        self.assertEqual(analysis["statistics"]["declaredComparisonCount"], 10)
        self.assertAlmostEqual(analysis["statistics"]["expectedBootstrapDrawsPerTail"], 5)
        self.assertTrue(all(row["classification"] == "exploratory-insufficient-bootstrap-resolution" for row in analysis["metrics"]))

    def test_classification_requires_full_interval_beyond_practical_threshold(self):
        kwargs = {"pairs": 20, "expected_tail_draws": 50}
        self.assertEqual(compare.classify(1.11, 1.3, .1, **kwargs), "regression")
        self.assertEqual(compare.classify(.7, .90, .1, **kwargs), "improvement")
        self.assertEqual(compare.classify(.98, 1.05, .1, **kwargs), "within-practical-band")
        self.assertEqual(compare.classify(1.05, 1.30, .1, **kwargs), "inconclusive")
        self.assertEqual(compare.classify(1.10, 1.30, .1, **kwargs), "inconclusive")
        self.assertEqual(compare.classify(.80, 1.20, .1, **kwargs), "inconclusive")

    def test_short_observations_are_resolution_limited(self):
        raw = raw_fixture(baseline=.0005, candidate=.001)
        self.assertEqual(compare.analyze(raw)["metrics"][0]["classification"], "exploratory-timer-resolution-limited")

    def test_percentile_interpolates_known_sample(self):
        self.assertEqual(compare.percentile([10, 20, 30], .25), 15)
        self.assertEqual(compare.percentile([10, 20, 30], 0), 10)
        self.assertEqual(compare.percentile([10, 20, 30], 1), 30)

    def test_bootstrap_rejects_unpaired_or_nonpositive_input(self):
        for baseline, candidate in [([1], [1, 2]), ([0], [1]), ([1], [float("nan")]), ([], [])]:
            with self.subTest(baseline=baseline, candidate=candidate):
                with self.assertRaises(compare.ValidationError):
                    compare.paired_log_ratio_interval(baseline, candidate, samples=100, seed=1, tail_probability=.025)


class RunCompletenessTests(unittest.TestCase):
    def test_one_failed_invocation_invalidates_scenario_without_dropping_pair(self):
        raw = raw_fixture(metrics=["compile.ms", "evaluate.ms"])
        raw["trials"][3]["invocations"][0]["exitCode"] = 1
        analysis = compare.analyze(raw)
        self.assertFalse(analysis["valid"])
        for row in analysis["metrics"]:
            self.assertEqual(row["completePairs"], 19)
            self.assertEqual(row["classification"], "invalid")
            self.assertNotIn("ratioInterval", row)

    def test_missing_trial_invalidates_instead_of_analyzing_survivors(self):
        raw = raw_fixture()
        raw["trials"].pop()
        row = compare.analyze(raw)["metrics"][0]
        self.assertEqual(row["classification"], "invalid")
        self.assertTrue(any("missing process pair" in error for error in row["errors"]))

    def test_missing_metric_invalidates_all_metrics_in_affected_scenario(self):
        raw = raw_fixture(metrics=["compile.ms", "evaluate.ms"])
        raw["trials"][0]["invocations"][0]["record"]["metrics"].pop("compile.ms")
        self.assertTrue(all(row["classification"] == "invalid" for row in compare.analyze(raw)["metrics"]))

    def test_false_correctness_cannot_be_hidden_by_successful_exit(self):
        raw = raw_fixture()
        raw["trials"][0]["invocations"][0]["record"]["correctness"] = False
        row = compare.analyze(raw)["metrics"][0]
        self.assertEqual(row["classification"], "invalid")
        self.assertTrue(any("correctness" in error for error in row["errors"]))

    def test_duplicate_trial_and_invocation_ids_are_rejected(self):
        raw = raw_fixture()
        raw["trials"].append(copy.deepcopy(raw["trials"][0]))
        with self.assertRaisesRegex(compare.ValidationError, "identifier"):
            compare.analyze(raw)
        raw = raw_fixture()
        raw["trials"][1]["invocations"][0]["id"] = raw["trials"][0]["invocations"][0]["id"]
        with self.assertRaisesRegex(compare.ValidationError, "identifier"):
            compare.analyze(raw)

    def test_wrong_pair_order_is_rejected(self):
        raw = raw_fixture()
        raw["trials"][1]["order"] = ["baseline", "candidate"]
        with self.assertRaisesRegex(compare.ValidationError, "AB/BA"):
            compare.analyze(raw)

    def test_command_drift_invalidates_scenario(self):
        raw = raw_fixture()
        raw["trials"][0]["invocations"][0]["command"].append("--different-workload")
        self.assertEqual(compare.analyze(raw)["metrics"][0]["classification"], "invalid")

    def test_runtime_patch_version_drift_invalidates_scenario(self):
        raw = raw_fixture()
        raw["trials"][0]["invocations"][0]["record"]["runtime"] = "CLR different patch"
        row = compare.analyze(raw)["metrics"][0]
        self.assertEqual(row["classification"], "invalid")
        self.assertTrue(any("runtime changed" in error for error in row["errors"]))

    def test_inner_repetition_count_drift_invalidates_scenario(self):
        raw = raw_fixture()
        raw["trials"][0]["invocations"][0]["record"]["metrics"]["evaluate.batch.ms"].append(10)
        row = compare.analyze(raw)["metrics"][0]
        self.assertEqual(row["classification"], "invalid")
        self.assertTrue(any("repetition counts changed" in error for error in row["errors"]))

    def test_changed_expression_or_warmup_settings_invalidate_scenario(self):
        for key, value in [("expression", "sin(x)"), ("warmupMinimumMs", 1), ("measurementRepetitions", 99)]:
            with self.subTest(key=key):
                raw = raw_fixture()
                raw["trials"][0]["invocations"][0]["record"]["details"][key] = value
                row = compare.analyze(raw)["metrics"][0]
                self.assertEqual(row["classification"], "invalid")
                self.assertTrue(any("workload or warmup settings changed" in error for error in row["errors"]))

    def test_bounded_compile_warmup_is_reported_without_claiming_steady_state(self):
        raw = raw_fixture()
        raw["trials"][0]["invocations"][0]["record"]["details"]["warmup.compile.budgetReached"] = False
        analysis = compare.analyze(raw)
        self.assertTrue(analysis["valid"])
        self.assertTrue(analysis["metrics"][0]["warmupNotes"])
        self.assertIn("do not establish steady state", compare.render_report(analysis))

    def test_post_run_binary_change_invalidates_comparison(self):
        raw = raw_fixture()
        raw["postRunArtifactVerification"] = {"passed": False, "errors": ["candidate binary changed"]}
        analysis = compare.analyze(raw)
        self.assertFalse(analysis["valid"])
        self.assertIn("candidate binary changed", analysis["errors"])

    def test_one_failed_scenario_does_not_mask_other_scenario_or_reduce_comparison_family(self):
        raw = raw_fixture(scenarios=["real", "complex"], bootstrap=5000)
        raw["trials"][0]["invocations"][0]["exitCode"] = 1
        analysis = compare.analyze(raw)
        self.assertEqual(analysis["statistics"]["declaredComparisonCount"], 2)
        rows = {row["scenario"]: row for row in analysis["metrics"]}
        self.assertEqual(rows["real"]["classification"], "invalid")
        self.assertEqual(rows["complex"]["classification"], "regression")

    def test_interrupted_run_is_explicitly_invalid_even_when_present_rows_look_complete(self):
        raw = raw_fixture()
        raw["completed"] = False
        analysis = compare.analyze(raw)
        self.assertFalse(analysis["valid"])
        self.assertIn("Run is incomplete or was interrupted", analysis["metrics"][0]["errors"])

    def test_supervisor_wall_time_is_separate_and_has_one_operation(self):
        raw = raw_fixture(metrics=["evaluate.ms", compare.PROCESS_WALL_METRIC])
        raw["manifest"]["scenarios"]["real"]["operations"][compare.PROCESS_WALL_METRIC] = 1
        analysis = compare.analyze(raw)
        row = next(row for row in analysis["metrics"] if row["metric"] == compare.PROCESS_WALL_METRIC)
        self.assertEqual(row["baselineMedianMs"], 200)
        self.assertEqual(row["candidateMedianMs"], 210)


class ManifestTests(unittest.TestCase):
    def test_nonrelease_mismatched_bitness_and_missing_commit_explanation_are_rejected(self):
        cases = [
            ("baseline", "provenance", "buildConfiguration", "Debug"),
            ("candidate", "expected", "bitness", 32),
            ("baseline", "provenance", "commit", None),
        ]
        for version, section, key, value in cases:
            with self.subTest(key=key):
                manifest = manifest_fixture()
                manifest[version][section][key] = value
                with self.assertRaises(compare.ValidationError):
                    compare.validate_manifest(manifest, Path.cwd(), verify_files=False)

    def test_arbitrary_environment_values_are_rejected_not_written_to_reports(self):
        manifest = manifest_fixture()
        manifest["environment"]["GITHUB_TOKEN"] = "test-placeholder"
        with self.assertRaisesRegex(compare.ValidationError, "unsupported key"):
            compare.validate_manifest(manifest, Path.cwd(), verify_files=False)

    def test_artifact_hashes_checked_and_executable_resolved(self):
        with tempfile.TemporaryDirectory() as directory:
            base = Path(directory)
            artifact = base / "fixture-core.dll"
            artifact.write_bytes(b"only a test fixture")
            manifest = manifest_fixture()
            for version in compare.VERSIONS:
                manifest[version]["cwd"] = directory
                manifest[version]["artifacts"] = [
                    {"path": "fixture-core.dll", "sha256": compare.sha256_file(artifact), "role": "core"},
                    {"path": sys.executable, "sha256": compare.sha256_file(Path(sys.executable)), "role": "worker"}]
            validated = compare.validate_manifest(manifest, base)
            self.assertEqual(validated["baseline"]["artifacts"][0]["path"], str(artifact.resolve()))
            self.assertEqual(validated["baseline"]["executableSha256"], compare.sha256_file(Path(sys.executable).resolve()))
            artifact.write_bytes(b"changed after manifest was made")
            with self.assertRaisesRegex(compare.ValidationError, "SHA256 mismatch"):
                compare.validate_manifest(manifest, base)

    def test_declared_executable_hash_mismatch_is_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            artifact = Path(directory) / "fixture.dll"
            artifact.write_bytes(b"fixture")
            manifest = manifest_fixture()
            for version in compare.VERSIONS:
                manifest[version]["artifacts"] = [
                    {"path": str(artifact), "sha256": compare.sha256_file(artifact), "role": "core"},
                    {"path": sys.executable, "sha256": compare.sha256_file(Path(sys.executable)), "role": "worker"}]
            manifest["candidate"]["executableSha256"] = "0" * 64
            with self.assertRaisesRegex(compare.ValidationError, "Executable SHA256 mismatch"):
                compare.validate_manifest(manifest, Path(directory))

    def test_runtime_controls_remove_inherited_profiler_and_record_only_allowed_values(self):
        inherited = {"PATH": "safe-path", "GITHUB_TOKEN": "do-not-report", "COMPlus_TieredCompilation": "0",
                     "CORECLR_ENABLE_PROFILING": "1", "DOTNET_ROOT": "runtime-path",
                     "DOTNET_ROLL_FORWARD": "Major", "DOTNET_MULTILEVEL_LOOKUP": "0",
                     "COMPUTATOR_DATA_DIRECTORY": "unintended-user-profile", "COMPUTATOR_GSL_DIRECTORY": "unintended-native-library"}
        with mock.patch.dict(compare.os.environ, inherited, clear=True):
            environment, controls = compare.controlled_environment(manifest_fixture(), False)
        self.assertNotIn("COMPlus_TieredCompilation", environment)
        self.assertNotIn("CORECLR_ENABLE_PROFILING", environment)
        self.assertNotIn("DOTNET_ROLL_FORWARD", environment)
        self.assertNotIn("DOTNET_MULTILEVEL_LOOKUP", environment)
        self.assertNotIn("COMPUTATOR_DATA_DIRECTORY", environment)
        self.assertNotIn("COMPUTATOR_GSL_DIRECTORY", environment)
        self.assertEqual(environment["DOTNET_ROOT"], "runtime-path")
        self.assertNotIn("GITHUB_TOKEN", controls)
        self.assertNotIn("do-not-report", json.dumps(controls))
        _, diagnostic = compare.controlled_environment(manifest_fixture(), True)
        self.assertEqual(diagnostic["DOTNET_TieredCompilation"], "0")
        self.assertEqual(diagnostic["COMPlus_TieredCompilation"], "0")

    def test_role_worker_must_be_in_command_and_core_role_is_required(self):
        manifest = manifest_fixture()
        manifest["baseline"]["artifacts"][1]["path"] = "/not-the-worker.exe"
        with self.assertRaisesRegex(compare.ValidationError, "pinned worker artifact"):
            compare.validate_manifest(manifest, Path.cwd(), verify_files=False)
        manifest = manifest_fixture()
        manifest["baseline"]["artifacts"][0].pop("role")
        with self.assertRaisesRegex(compare.ValidationError, "role:core"):
            compare.validate_manifest(manifest, Path.cwd(), verify_files=False)

    def test_version_profile_must_be_absolute_and_cannot_override_gsl(self):
        for controls in [{"COMPUTATOR_DATA_DIRECTORY": "relative/profile"}, {"COMPUTATOR_GSL_DIRECTORY": "/somewhere"}]:
            with self.subTest(controls=controls):
                manifest = manifest_fixture()
                manifest["candidate"]["environment"] = controls
                with self.assertRaises(compare.ValidationError):
                    compare.validate_manifest(manifest, Path.cwd(), verify_files=False)

    def test_analyze_only_preserves_archived_windows_paths_on_any_host(self):
        raw = raw_fixture()
        for version in compare.VERSIONS:
            specification = raw["manifest"][version]
            specification["cwd"] = rf"C:\performance\{version}"
            specification["command"] = [rf"C:\performance\{version}\Worker.exe"]
            specification["artifacts"] = [
                {"path": rf"C:\performance\{version}\Computator.NET.Core.dll", "sha256": "b" * 64, "role": "core"},
                {"path": rf"C:\performance\{version}\Worker.exe", "sha256": "c" * 64, "role": "worker"}]
        raw["manifest"]["candidate"]["environment"] = {"COMPUTATOR_DATA_DIRECTORY": r"C:\performance\candidate-profile"}
        for trial in raw["trials"]:
            for invocation in trial["invocations"]:
                version = invocation["version"]
                invocation["command"] = compare.command_for(raw["manifest"], version, "real")
                invocation["environmentControls"].update(raw["manifest"][version].get("environment", {}))
        validated = compare.validate_manifest(raw["manifest"], Path.cwd(), verify_files=False)
        self.assertEqual(validated["baseline"]["cwd"], r"C:\performance\baseline")
        self.assertTrue(compare.analyze(raw)["valid"])

    def test_shared_observer_command_override_must_be_pinned_and_is_checked_on_replay(self):
        raw = raw_fixture(scenarios=["startup"])
        manifest = raw["manifest"]
        observer = str(Path.cwd() / "shared-observer.exe")
        manifest["scenarios"]["startup"]["commands"] = {version: [observer] for version in compare.VERSIONS}
        with self.assertRaisesRegex(compare.ValidationError, "pinned role:observer"):
            compare.validate_manifest(manifest, Path.cwd(), verify_files=False)
        for version in compare.VERSIONS:
            manifest[version]["artifacts"].append({"path": observer, "sha256": "d" * 64, "role": "observer"})
        for trial in raw["trials"]:
            for invocation in trial["invocations"]:
                invocation["command"] = compare.command_for(manifest, invocation["version"], "startup")
        self.assertTrue(compare.analyze(raw)["valid"])
        raw["trials"][0]["invocations"][0]["command"][0] = "changed-observer.exe"
        self.assertFalse(compare.analyze(raw)["valid"])


class ExecutionTests(unittest.TestCase):
    def test_collect_runs_sequential_ab_ba_and_rotates_scenarios_preserving_all_logs(self):
        manifest = manifest_fixture(scenarios=["real", "complex"])
        profile = str(Path.cwd() / "dedicated-candidate-profile")
        manifest["candidate"]["environment"] = {"COMPUTATOR_DATA_DIRECTORY": profile}
        seen = []
        active = False

        def invoke(command, cwd, environment, timeout):
            nonlocal active
            self.assertFalse(active, "overlapping invocations")
            active = True
            version = command[command.index("--test-version") + 1]
            scenario = command[-1]
            if version == "candidate":
                self.assertEqual(environment["COMPUTATOR_DATA_DIRECTORY"], profile)
            else:
                self.assertNotIn("COMPUTATOR_DATA_DIRECTORY", environment)
            seen.append((version, scenario))
            record = record_fixture(manifest, version, scenario)
            result = {"command": command, "record": record, "stdout": json.dumps(record), "stderr": "diagnostic output\n",
                      "exitCode": 0, "timedOut": False, "errors": [], "processWallMs": 50.0}
            active = False
            return result

        with tempfile.TemporaryDirectory() as directory, mock.patch.object(compare, "host_metadata", return_value={}), \
                mock.patch.object(compare, "verify_artifacts", return_value=[]), contextlib.redirect_stdout(io.StringIO()):
            output = Path(directory)
            raw = compare.collect(manifest, output, pairs=2, invoke=invoke)
            self.assertTrue(raw["completed"])
            self.assertEqual(len(list((output / "child-logs").glob("*.stderr.txt"))), 8)
            saved = compare.read_json(output / "raw.json")
            self.assertEqual(len(saved["trials"]), 4)
            self.assertTrue(compare.analyze(saved)["valid"])
        self.assertEqual(seen, [("baseline", "real"), ("candidate", "real"), ("baseline", "complex"), ("candidate", "complex"),
                                ("candidate", "complex"), ("baseline", "complex"), ("candidate", "real"), ("baseline", "real")])

    def test_child_failure_is_retained_and_does_not_silently_retry(self):
        manifest = manifest_fixture()
        calls = []

        def invoke(command, cwd, environment, timeout):
            calls.append(command)
            return {"command": command, "record": None, "stdout": "partial stdout", "stderr": "failure details",
                    "exitCode": 1, "timedOut": False, "errors": ["failed"], "processWallMs": 50.0}

        with tempfile.TemporaryDirectory() as directory, mock.patch.object(compare, "host_metadata", return_value={}), \
                mock.patch.object(compare, "verify_artifacts", return_value=[]), contextlib.redirect_stdout(io.StringIO()):
            raw = compare.collect(manifest, Path(directory), pairs=1, invoke=invoke)
            self.assertEqual(len(calls), 2)
            self.assertFalse(compare.analyze(raw)["valid"])
            self.assertEqual(raw["trials"][0]["invocations"][0]["stderr"], "failure details")

    def test_real_child_stdout_and_stderr_are_retained_without_shell(self):
        command = [sys.executable, "-c", "import json,sys; print(json.dumps({'schema':1})); print('diagnostic',file=sys.stderr)"]
        result = compare.run_child(command, str(Path.cwd()), dict(compare.os.environ), 5)
        self.assertEqual(result["exitCode"], 0)
        self.assertEqual(result["record"], {"schema": 1})
        self.assertEqual(result["errors"], [])
        self.assertIn("diagnostic", result["stderr"])
        self.assertGreater(result["processWallMs"], 0)

    def test_real_child_timeout_is_invalid_and_bounded(self):
        command = [sys.executable, "-c", "import time; print('started',flush=True); time.sleep(30)"]
        result = compare.run_child(command, str(Path.cwd()), dict(compare.os.environ), .2)
        self.assertTrue(result["timedOut"])
        self.assertNotEqual(result["exitCode"], 0)
        self.assertTrue(result["errors"])
        self.assertIsNone(result["record"])
        self.assertLess(result["processWallMs"], 10_000)

    def test_extra_stdout_cannot_be_mistaken_for_worker_json(self):
        command = [sys.executable, "-c", "print('unexpected banner'); print('{}')"]
        result = compare.run_child(command, str(Path.cwd()), dict(compare.os.environ), 5)
        self.assertEqual(result["exitCode"], 0)
        self.assertIsNone(result["record"])
        self.assertTrue(any("one JSON record" in error for error in result["errors"]))

    def test_report_distinguishes_marginal_medians_from_paired_ratio_and_labels_baseline(self):
        report = compare.render_report(compare.analyze(raw_fixture()))
        self.assertIn("2.3.2.0", report)
        self.assertIn("their quotient need not equal", report)
        self.assertIn("Bonferroni", report)
        self.assertIn("not create a cold disk", report)


if __name__ == "__main__":
    unittest.main()
