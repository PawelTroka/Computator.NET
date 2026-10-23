#!/usr/bin/env python3
"""Recompute published statistics; full raw/provenance validation stays in compare.py."""
from __future__ import annotations
import argparse
import hashlib
import importlib.util
import json
import math
import statistics
from pathlib import Path


def verify(path: Path, compare) -> int:
    data = json.loads(path.read_text(encoding="utf-8"))
    if data.get("kind") != "computator.performance.published-measurements":
        raise ValueError("Expected a published measurement table")
    config, stats = data["configuration"], data["statistics"]
    rows = data["metrics"]
    if len(rows) != stats["declaredComparisonCount"]:
        raise ValueError("The declared comparison family changed")
    tail = (1 - config["confidence"]) / (2 * len(rows))
    checked = 0
    for row in rows:
        if row["classification"] == "invalid":
            continue  # Invalid metrics still count in the multiplicity adjustment.
        baseline, candidate = row["baselineProcessMediansMs"], row["candidateProcessMediansMs"]
        if len(baseline) != config["pairs"] or len(candidate) != config["pairs"]:
            raise ValueError("Incomplete process-median table")
        key = f'{row["scenario"]}/{row["metric"]}'
        seed = config["seed"] + int.from_bytes(hashlib.sha256(key.encode()).digest()[:8], "big")
        point, lower, upper = compare.paired_log_ratio_interval(
            baseline, candidate, samples=config["bootstrapSamples"], seed=seed, tail_probability=tail)
        derived = {
            "geometricMeanPairedRatio": point,
            "baselineMedianMs": statistics.median(baseline),
            "candidateMedianMs": statistics.median(candidate),
            "medianPairedDifferenceMs": statistics.median(c-b for b,c in zip(baseline,candidate)),
        }
        for name, value in derived.items():
            if not math.isclose(value, row[name], rel_tol=1e-12, abs_tol=1e-12):
                raise ValueError(f"Statistical result differs: {key}/{name}")
        for name, value in [("lower",lower),("upper",upper)]:
            if not math.isclose(value, row["ratioInterval"][name], rel_tol=1e-12, abs_tol=1e-12):
                raise ValueError(f"Adjusted interval differs: {key}/{name}")
        label = compare.classify(lower,upper,config["practicalThreshold"],pairs=config["pairs"],
            expected_tail_draws=config["bootstrapSamples"]*tail,
            resolution_limited=row["timerResolutionLimited"])
        if label != row["classification"]:
            raise ValueError(f"Classification differs: {key}")
        checked += 1
    return checked


def main() -> None:
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("tables",nargs="*",type=Path)
    parser.add_argument("--compare",type=Path,
        default=Path(__file__).resolve().parents[3]/"build/performance/compare.py")
    args=parser.parse_args()
    spec=importlib.util.spec_from_file_location("performance_compare",args.compare)
    if spec is None or spec.loader is None: raise ValueError("Cannot load compare.py")
    compare=importlib.util.module_from_spec(spec)
    spec.loader.exec_module(compare)
    paths=args.tables or sorted(Path(__file__).parent.glob("*-measurements.json"))
    if not paths: raise ValueError("No measurement tables found")
    for path in paths:
        count=verify(path,compare)
        print(f"{path.name}: {count} valid metric statistics reproduced; invalid rows retained")


if __name__ == "__main__": main()
