import csv
from pathlib import Path
import numpy as np
from capacity import neighbors, measure
def matches(body,patch):
    rng=np.random.default_rng(991)
    groups={degree:sorted(p for p in body if len(neighbors(p)&body)==degree) for degree in range(5)}
    counts={degree:sum(len(neighbors(p)&body)==degree for p in patch) for degree in range(5)}
    found=set()
    for _ in range(2000):
        selected=frozenset(p for degree in range(5) for p in [groups[degree][int(i)] for i in rng.choice(len(groups[degree]),counts[degree],replace=False)])
        if selected!=patch:found.add(selected)
        if len(found)==20:break
    return sorted(found,key=lambda s:sorted(s))
with open('degree_controls.csv','w',newline='') as f:
    writer=csv.writer(f)
    writer.writerow(['seed','transport_cells','controls','developed','control_mean','developed_spent','control_spent'])
    for seed in range(201,225):
        raw=Path(f'maps/{seed}_0.9.txt').read_text()
        body={(i%64,i//64) for i,c in enumerate(raw) if c!='0'}
        patch={(i%64,i//64) for i,c in enumerate(raw) if c=='4'}
        controls=matches(body,patch)
        def score(p):
            values=[measure(body,p,1,sign,.001) for sign in (-1,1)]
            return np.mean([v[0][4] for v in values]),np.mean([v[1][4] for v in values])
        a=score(patch)
        b=np.mean([score(p) for p in controls],axis=0) if controls else [None,None]
        writer.writerow([seed,len(patch),len(controls),a[0],b[0],a[1],b[1]])
