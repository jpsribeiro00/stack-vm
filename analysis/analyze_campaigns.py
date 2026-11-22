
"""Aggregate independent BenchmarkDotNet campaigns."""

import argparse
from pathlib import Path

import pandas as pd

parser = argparse.ArgumentParser()
parser.add_argument("files", nargs="+", type=Path)
parser.add_argument(
    "--out",
    type=Path,
    default=Path("results/charts/campaigns"),
)
args = parser.parse_args()
args.out.mkdir(parents=True, exist_ok=True)


def to_ns(value):
    text = str(value).replace("μ", "µ").replace(",", "").strip()
    amount, unit = text.split()[:2]

    multipliers = {
        "ns": 1,
        "us": 1e3,
        "µs": 1e3,
        "ms": 1e6,
        "s": 1e9,
    }

    return float(amount) * multipliers[unit]


frames = []

for index, path in enumerate(args.files, 1):
    frame = pd.read_csv(path)

    required = {"Method", "N", "Mean"}
    missing = required - set(frame.columns)

    if missing:
        raise SystemExit(f"{path}: missing columns {missing}")

    if "Workload" not in frame.columns:
        frame["Workload"] = "Recursion"

    if "InitialCapacity" not in frame.columns:
        frame["InitialCapacity"] = 32

    frame["mean_ns"] = frame["Mean"].map(to_ns)
    frame["Campaign"] = index

    frames.append(frame)

df = pd.concat(frames, ignore_index=True)

keys = ["Campaign", "Workload", "N", "InitialCapacity"]

wide = df.pivot_table(
    index=keys,
    columns="Method",
    values="mean_ns",
    aggfunc="first",
)

wide = wide.dropna(
    subset=["SingleStack", "DualStack"]
).reset_index()

wide["ratio_single_over_dual"] = (
    wide["SingleStack"] / wide["DualStack"]
)

wide.to_csv(
    args.out / "paired_campaign_ratios.csv",
    index=False,
)

summary = wide.groupby(
    ["Workload", "N", "InitialCapacity"]
)["ratio_single_over_dual"].agg(
    ["count", "mean", "std", "median"]
).reset_index()

summary.to_csv(
    args.out / "campaign_summary.csv",
    index=False,
)

print(summary.to_string(index=False))
