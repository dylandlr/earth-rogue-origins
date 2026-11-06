"""Finite uniform nutrient stock, shared reserve, fixed geometry except cell loss."""
import csv
import math
from pathlib import Path
import numpy as np

def assay(mask, seed, cost, horizon=180, policy="edge_first", block=False):
    live = mask.copy()
    nutrient = np.ones(mask.shape)
    reserve = waste = 0.0
    initial = nutrient.sum() + live.sum()
    first_loss = half_loss = extinction = None
    # Both members use the same coordinate priorities for simultaneous shortages.
    priority = np.random.default_rng(seed).permutation(mask.size).reshape(mask.shape)
    new_capture = total_capture = 0.0
    for tick in range(1, horizon + 1):
        empty = ~live
        edge = np.zeros_like(live)
        edge[1:] |= empty[:-1]
        edge[:-1] |= empty[1:]
        edge[:, 1:] |= empty[:, :-1]
        edge[:, :-1] |= empty[:, 1:]
        sites = edge & live
        if tick == 1:
            original_edge = sites.copy()
        eligible = sites & original_edge if block else sites
        captured = nutrient[eligible] * 0.5
        new_capture += captured[~original_edge[eligible]].sum()
        total_capture += captured.sum()
        nutrient[eligible] -= captured
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
    return first_loss, half_loss, extinction, live.sum(), reserve, new_capture, total_capture

if __name__ == '__main__':
    with open('new_exposure.csv','w',newline='') as f:
        out=csv.writer(f)
        out.writerow(['seed','form','cost','block','first_loss','half_loss','extinction','final_cells','final_reserve','new_capture','total_capture'])
        for p in sorted(Path('masks').glob('*.txt')):
            seed, form = p.stem.split('_')
            mask=np.array([list(line) for line in p.read_text().splitlines()])=='#'
            for cost in (.0093,):
                for block in (False,True):
                    out.writerow([seed,form,cost,block,*assay(mask,int(seed),cost,block=block)])
