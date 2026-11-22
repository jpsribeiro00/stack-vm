
"""Analyze deterministic stack storage metrics."""

import argparse
from pathlib import Path

import matplotlib.pyplot as plt
import pandas as pd

parser = argparse.ArgumentParser()
parser.add_argument("csv", type=Path)
parser.add_argument(
    "--out",
    type=Path,
    default=Path("results/charts/storage"),
)
args = parser.parse_args()
args.out.mkdir(parents=True, exist_ok=True)

df = pd.read_csv(args.csv)

required = {
    "Workload", "N", "Architecture", "InitialCapacity",
    "Instructions", "MaxAllocatedSlots", "StackResizes",
}

missing = required - set(df.columns)
if missing:
    raise SystemExit(f"Missing columns: {missing}")

for (workload, n, capacity), group in df.groupby(
    ["Workload", "N", "InitialCapacity"]
):
    if (
        group["Instructions"].nunique() != 1
        or set(group["Architecture"]) != {"SingleStack", "DualStack"}
    ):
        raise SystemExit(
            f"Invalid equivalence: {workload}, {n}, {capacity}"
        )

recursion = df[df["Workload"] == "Recursion"]

metrics = [
    ("MaxAllocatedSlots", "Maximum allocated integer slots"),
    ("StackResizes", "Buffer expansions"),
]

for metric, label in metrics:
    fig, ax = plt.subplots(figsize=(8, 5))

    for (architecture, capacity), group in recursion.groupby(
        ["Architecture", "InitialCapacity"]
    ):
        group = group.sort_values("N")
        ax.plot(
            group["N"],
            group[metric],
            marker="o",
            label=f"{architecture} / initial={capacity}",
        )

    ax.set_xlabel("Recursion depth N")
    ax.set_ylabel(label)
    ax.set_title(f"Recursion: {label}")
    ax.grid(alpha=0.25)
    ax.legend(fontsize=7)

    fig.tight_layout()
    fig.savefig(
        args.out / f"recursion_{metric.lower()}.png",
        dpi=180,
    )
    plt.close(fig)

summary = df.pivot_table(
    index=["Workload", "N", "InitialCapacity"],
    columns="Architecture",
    values="MaxAllocatedSlots",
).reset_index()

summary["dual_to_single_allocated_slots"] = (
    summary["DualStack"] / summary["SingleStack"]
)

summary.to_csv(args.out / "capacity_comparison.csv", index=False)

print(f"Results saved to {args.out}")
