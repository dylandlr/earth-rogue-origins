"""Finite uniform nutrient stock, shared reserve, fixed geometry except cell loss."""
import csv
import math
from pathlib import Path
import numpy as np

def assay(mask, seed, cost, horizon=180, policy="baseline"):
    live = mask.copy()
    nutrient = np.ones(mask.shape)
    reserve = waste = 0.0
    initial = nutrient.sum() + live.sum()
    first_loss = half_loss = extinction = None
    # Both members use the same coordinate priorities for simultaneous shortages.
    priority = np.random.default_rng(seed).permutation(mask.size).reshape(mask.shape)
    for tick in range(1, horizon + 1):
        empty = ~live
        edge = np.zeros_like(live)
        edge[1:] |= empty[:-1]
        edge[:-1] |= empty[1:]
        edge[:, 1:] |= empty[:, :-1]
        edge[:, :-1] |= empty[:, 1:]
        sites = edge & live
        captured = nutrient[sites] * 0.5
        nutrient[sites] -= captured
        reserve += captured.sum()
        count = int(live.sum())
        supported = min(count, math.floor((reserve + 1e-12) / cost))
        if supported < count:
            first_loss = first_loss or tick
            indices = np.flatnonzero(live)
            rank = priority.flat[indices]
            if policy == "edge_first":
                rank = rank + (~sites.flat[indices]) * mask.size
            elif policy == "interior_first":
                rank = rank + sites.flat[indices] * mask.size
            elif policy != "baseline":
                raise ValueError(policy)
            lost = indices[np.argsort(rank)[:count-supported]]
            live.flat[lost] = False
            waste += len(lost)
        paid = supported * cost
        reserve -= paid
        waste += paid
        assert reserve >= -1e-10
        assert abs(nutrient.sum()+reserve+waste+live.sum()-initial)<1e-7
        if supported <= mask.sum()/2:
            half_loss = half_loss or tick
        if not supported:
            extinction = tick
            break
    return first_loss, half_loss, extinction, live.sum(), reserve

if __name__ == '__main__':
    with open('loss_policy.csv','w',newline='') as f:
        out=csv.writer(f)
        out.writerow(['seed','form','cost','policy','first_loss','half_loss','extinction','final_cells','final_reserve'])
        for p in sorted(Path('masks').glob('*.txt')):
            seed, form = p.stem.split('_')
            mask=np.array([list(line) for line in p.read_text().splitlines()])=='#'
            for cost in (.005,.0093,.02):
                for policy in ("baseline","edge_first","interior_first"):
                    out.writerow([seed,form,cost,policy,*assay(mask,int(seed),cost,policy=policy)])
