import csv
from pathlib import Path
from collections import defaultdict
import numpy as np
from assay import assay
def coords(s):
    return {(i%64,i//64) for i in map(int,s.split())}
def neighbors(p):
    x,y=p
    return {(x-1,y),(x+1,y),(x,y-1),(x,y+1)}
def components(s):
    left=set(s)
    while left:
        group={left.pop()}
        stack=list(group)
        while stack:
            new=neighbors(stack.pop())&left
            left-=new
            group|=new
            stack.extend(new)
        yield group
def bitmap(s):
    mask=np.zeros((48,64),bool)
    for x,y in s: mask[y,x]=True
    return mask
def controls(patch,body,excluded):
    signature=sorted(len(neighbors(p)&body) for p in patch)
    found=set()
    for mirror in (1,-1):
        shape={(mirror*x,y) for x,y in patch}
        for _ in range(4):
            shape={(-y,x) for x,y in shape}
            anchor=min(shape)
            for x,y in body-excluded:
                moved=frozenset((x+i-anchor[0],y+j-anchor[1]) for i,j in shape)
                if moved<=body-excluded and sorted(len(neighbors(p)&body) for p in moved)==signature:
                    found.add(moved)
    return sorted(found,key=lambda s:sorted(s))
# Fixed tissue and a prepaid upkeep budget separate from the source tracer.
def delivery(body, placements, axis, sign):
    points=sorted(body)
    xy=np.array(points)
    index={p:i for i,p in enumerate(points)}
    edges=[(i,index[q]) for i,p in enumerate(points) for q in neighbors(p)&body if i<index[q]]
    a,b=np.array(edges).T
    high=np.array([[p in s for p in points] for s in placements])
    rate=np.where(high[:,a]|high[:,b],.20,.10)
    pos=sign*xy[:,axis]
    source=pos==pos.min()
    sink=pos==pos.max()
    assert not np.any(source & sink)
    reserve=np.zeros(high.shape)
    reserve[:,source]=100/source.sum()
    collected=np.zeros(len(placements))
    for _ in range(180):
        flux=rate*(reserve[:,a]-reserve[:,b])
        # All links use pre-update reserves; accumulate without scan-order bias.
        delta=np.zeros_like(reserve)
        for edge,(i,j) in enumerate(edges):
            delta[:,i]-=flux[:,edge]
            delta[:,j]+=flux[:,edge]
        reserve+=delta
        collected+=reserve[:,sink].sum(axis=1)
        reserve[:,sink]=0
        assert reserve.min()>-1e-9 and np.max(abs(reserve.sum(axis=1)+collected-100))<1e-8
    return collected

rows=defaultdict(list)
for r in csv.DictReader(open('ticks.csv')):
    rows[int(r['seed'])].append(coords(r['transport_sites']))
with open('delivery.csv','w',newline='') as f:
    w=csv.writer(f);w.writerow(['seed','axis','sign','cells','matches','ordinary','developed','matched_mean','upkeep'])
    for seed,ticks in rows.items():
        body=coords(Path(f'mask_{seed}.txt').read_text())
        patch=max(components(set.intersection(*ticks[-3:])),key=lambda s:(len(s),sorted(s)),default=set())
        if len(patch)<3:continue
        matched=controls(patch,body,ticks[-1])
        if not matched:continue
        for axis,sign in [(0,1),(0,-1),(1,1),(1,-1)]:
            values=delivery(body,[set(),patch,*matched],axis,sign)
            w.writerow([seed,axis,sign,len(patch),len(matched),values[0],values[1],values[2:].mean(),.001*180*len(patch)])
