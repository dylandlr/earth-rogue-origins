import csv
from pathlib import Path
import numpy as np
from capacity import neighbors, measure
def matches(body,patch):
    signature=sorted(len(neighbors(p)&body) for p in patch)
    result=set()
    for reflection in (-1,1):
        shape={(reflection*x,y) for x,y in patch}
        for _ in range(4):
            shape={(-y,x) for x,y in shape}
            anchor=min(shape)
            for x,y in body:
                candidate=frozenset((x+i-anchor[0],y+j-anchor[1]) for i,j in shape)
                if candidate==patch or not candidate<=body:
                    continue
                if sorted(len(neighbors(p)&body) for p in candidate)==signature:
                    result.add(candidate)
    return sorted(result,key=lambda s:sorted(s))
with open('placement.csv','w',newline='') as f:
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
