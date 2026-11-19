"""Plot BenchmarkDotNet CSV exports; requires pandas and matplotlib.

Usage: python analysis/plot_results.py path/to/StackVm.Benchmarks.VirtualMachineBenchmarks-report.csv
"""
import argparse
from pathlib import Path
import pandas as pd
import matplotlib.pyplot as plt
import re

parser = argparse.ArgumentParser()
parser.add_argument("csv", type=Path)
parser.add_argument("--out", type=Path, default=Path("results/charts"))
args = parser.parse_args()
args.out.mkdir(parents=True, exist_ok=True)
df = pd.read_csv(args.csv)
required = {"Method", "Workload", "N", "Mean"}
if not required.issubset(df.columns):
    raise SystemExit(f"Missing columns: {required - set(df.columns)}. Found: {list(df.columns)}")

def to_ns(value):
    if value is None:
        return float("nan")

    text = str(value).strip()

    # Normalize different microsecond symbols.
    text = text.replace("μ", "µ")

    match = re.match(
        r"^([\d.,]+)\s*(ns|µs|us|ms|s)$",
        text
    )

    if not match:
        raise ValueError(f"Unsupported time format: {value}")

    number, unit = match.groups()

    # BenchmarkDotNet uses commas as thousands separators.
    number = number.replace(",", "")

    multipliers = {
        "ns": 1,
        "µs": 1e3,
        "us": 1e3,
        "ms": 1e6,
        "s": 1e9,
    }

    return float(number) * multipliers[unit]

df["mean_ns"] = df["Mean"].map(to_ns)
df["N"] = pd.to_numeric(df["N"])
for workload, part in df.groupby("Workload"):
    fig, ax = plt.subplots(figsize=(7, 4.5))
    for method, group in part.groupby("Method"):
        group = group.sort_values("N")
        ax.plot(group["N"], group["mean_ns"] / 1e6, marker="o", label=method)
    ax.set_xlabel("Workload size (N)")
    ax.set_ylabel("Mean execution time (ms)")
    ax.set_title(f"VM execution time — {workload}")
    ax.legend()
    ax.grid(alpha=0.25)
    fig.tight_layout()
    fig.savefig(args.out / f"{workload.lower()}_time.png", dpi=180)
    plt.close(fig)

wide = df.pivot_table(index=["Workload", "N"], columns="Method", values="mean_ns").reset_index()
wide["speedup_single_over_dual"] = wide["SingleStack"] / wide["DualStack"]
wide.to_csv(args.out / "speedup.csv", index=False)
print(f"Saved charts and speedup.csv in {args.out}")
