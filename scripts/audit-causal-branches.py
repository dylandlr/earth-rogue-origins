#!/usr/bin/env python3
"""Audit Earth: Rogue Origins Unity v2.3 causal branch exports (stdlib only).

This script checks accounting identities; it does NOT infer organs or fitness
causality from occupancy masks alone.
"""
import argparse
import csv
import json
import math
from collections import defaultdict
from pathlib import Path


def read_csv(path, required):
    with path.open(newline="", encoding="utf-8-sig") as stream:
        reader = csv.DictReader(stream)
        missing = set(required) - set(reader.fieldnames or [])
        if missing:
            raise ValueError(f"{path}: missing columns: {', '.join(sorted(missing))}")
        return list(reader)


def integer(row, field):
    return int(row[field])


def finite(row, field):
    result = float(row[field])
    if not math.isfinite(result):
        raise ValueError(f"nonfinite {field} in {row}")
    return result


def audit(folder):
    population = read_csv(folder / "population.csv", (
        "generation", "id", "occupied", "fitness", "novelty",
        "causal_branches", "strongest_branch_effect"))
    ablations = read_csv(folder / "causal_branch_ablation.csv", (
        "generation", "id", "innovation", "removed_genes",
        "original_cells", "ablated_cells", "changed_cells",
        "occupancy_iou", "changed_fraction", "morphologically_active"))
    errors = []
    warnings = []
    grouped = defaultdict(list)
    individuals = {}
    for row in population:
        key = (integer(row, "generation"), integer(row, "id"))
        if key in individuals:
            errors.append(f"duplicate population entry {key}")
        individuals[key] = row
    audited = []
    for row in ablations:
        key = (integer(row, "generation"), integer(row, "id"))
        grouped[key].append(row)
        if key not in individuals:
            errors.append(f"ablation without population entry {key}")
        original = integer(row, "original_cells")
        ablated = integer(row, "ablated_cells")
        changed = integer(row, "changed_cells")
        removed = integer(row, "removed_genes")
        iou = finite(row, "occupancy_iou")
        effect = finite(row, "changed_fraction")
        if min(original, ablated, changed) < 0 or removed < 1:
            errors.append(f"invalid cell or removed-gene count {key}")
        # A xor B cannot be smaller than the absolute difference in cell count.
        if changed < abs(original - ablated) or changed > original + ablated:
            errors.append(f"impossible occupancy symmetric difference {key}")
        union = (original + ablated + changed) / 2
        expected = changed / union if union else 0.0
        expected_iou = 1 - expected if union else 1.0
        if abs(effect - expected) > 0.0001:
            errors.append(f"changed_fraction inconsistent with cell counts {key}")
        if abs(iou - expected_iou) > 0.0001:
            errors.append(f"occupancy_iou inconsistent with cell counts {key}")
        active = row["morphologically_active"].lower() == "true"
        if row["morphologically_active"].lower() not in ("true", "false"):
            errors.append(f"invalid active flag {key}")
        if active != (effect >= 0.035):
            errors.append(f"active flag disagrees with 3.5% threshold {key}")
        # This is only a mass-balanced rearrangement signal, NOT organ formation.
        mass_change = abs(original - ablated)
        redistribution_cells = max(0, changed - mass_change)
        audited.append({
            "generation": key[0], "id": key[1],
            "innovation": integer(row, "innovation"),
            "removed_genes": removed,
            "original_cells": original, "ablated_cells": ablated,
            "changed_cells": changed, "net_mass_change": original - ablated,
            "redistribution_cells": redistribution_cells,
            "redistribution_share": round(redistribution_cells / changed, 6) if changed else 0,
            "changed_fraction": effect, "active": active,
            "interpretation": ("no_detectable_effect" if changed == 0 else
                "pure_extent_compatible" if redistribution_cells == 0 else
                "mixed_gain_loss_requires_spatial_review")
        })

    final = max((integer(p, "generation") for p in population), default=None)
    for key, p in individuals.items():
        if key[0] != final:
            continue  # branch detail exported only for the final generation
        branches = grouped.get(key, [])
        active_count = sum(b["morphologically_active"].lower() == "true" for b in branches)
        strongest = max((finite(b, "changed_fraction") for b in branches), default=0.0)
        if active_count != integer(p, "causal_branches"):
            errors.append(f"causal_branches mismatch for {key}")
        if abs(strongest - finite(p, "strongest_branch_effect")) > 0.0001:
            errors.append(f"strongest_branch_effect mismatch for {key}")
        if branches and any(integer(b, "original_cells") != integer(p, "occupied") for b in branches):
            errors.append(f"original tissue size mismatch for {key}")
        if not branches and integer(p, "causal_branches") != 0:
            errors.append(f"no branch rows for apparently active organism {key}")

    by_generation = []
    for generation in sorted({integer(p, "generation") for p in population}):
        rows = [p for p in population if integer(p, "generation") == generation]
        by_generation.append({
            "generation": generation, "population": len(rows),
            "mean_fitness": round(sum(finite(p, "fitness") for p in rows) / len(rows), 6),
            "mean_novelty": round(sum(finite(p, "novelty") for p in rows) / len(rows), 6),
            "mean_occupied": round(sum(integer(p, "occupied") for p in rows) / len(rows), 3)
        })
    if len(by_generation) > 1 and by_generation[-1]["mean_novelty"] < by_generation[0]["mean_novelty"]:
        warnings.append("mean novelty declined from initial to final generation")
    if audited and not any(a["redistribution_cells"] for a in audited):
        warnings.append("all observed knockout changes are compatible with net extent changes")
    if not audited:
        warnings.append("no causal ablation rows: structural effects cannot be verified")
    report = {
        "schema": "earth-rogue-origins.causal-audit.v1",
        "source": str(folder),
        "final_generation": final,
        "branch_interventions": len(audited),
        "active_interventions": sum(a["active"] for a in audited),
        "mixed_gain_loss_interventions": sum(a["redistribution_cells"] > 0 for a in audited),
        "generation_summary": by_generation,
        "warnings": warnings,
        "errors": errors,
        "limitations": [
            "Changed cells and IoU are occupancy effects, not evidence of organ function.",
            "Mixed gain/loss can reflect global deformation, not a discrete new appendage.",
            "Gene ablations also remove descendants and do not isolate direct gene action.",
            "Fitness is an engineered proxy, not verified ecological survival."
        ]
    }
    return report, audited


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("export_dir", type=Path, help="directory containing Unity CSV exports")
    parser.add_argument("--output-dir", type=Path, help="defaults to export_dir/audit")
    args = parser.parse_args()
    report, audited = audit(args.export_dir)
    output = args.output_dir or args.export_dir / "audit"
    output.mkdir(parents=True, exist_ok=True)
    (output / "causal_audit.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    if audited:
        with (output / "branch_effect_types.csv").open("w", newline="", encoding="utf-8") as stream:
            writer = csv.DictWriter(stream, fieldnames=list(audited[0]))
            writer.writeheader()
            writer.writerows(audited)
    print(json.dumps({k: v for k, v in report.items() if k not in ("generation_summary", "limitations")}, indent=2))
    return 1 if report["errors"] else 0


if __name__ == "__main__":
    raise SystemExit(main())
