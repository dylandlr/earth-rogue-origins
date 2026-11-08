import csv
from pathlib import Path
from collections import defaultdict
import numpy as np
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

def measure(body, patch, axis, sign, price):
    points=sorted(body)
    idx={p:i for i,p in enumerate(points)}
    edges=[(i,idx[q]) for i,p in enumerate(points) for q in neighbors(p)&body if i<idx[q]]
    a,b=np.array(edges).T
    # Five heritable levels spanning ordinary to double conductance.
    levels=np.arange(5)[:,None]/4
    special=np.array([p in patch for p in points])
    projection=np.array([p[axis]*sign for p in points])
    source=projection==min(projection)
    sink=projection==max(projection)
    mass=np.zeros((5,len(points)))
    mass[:,source]=100/source.sum()
    delivered=np.zeros(5)
    spent=np.zeros(5)
    for pulse in range(180):
        fee=price*levels
        active=special & (mass>=fee)
        paid=fee*active
        mass-=paid
        spent+=paid.sum(axis=1)
        conductance=.1+.1*levels*(active[:,a]|active[:,b])
        flux=conductance*(mass[:,a]-mass[:,b])
        for k in range(5):
            mass[k]+=np.bincount(b,flux[k],len(points))-np.bincount(a,flux[k],len(points))
        delivered+=mass[:,sink].sum(axis=1)
        mass[:,sink]=0
        assert mass.min()>=-1e-10 and np.allclose(mass.sum(axis=1)+delivered+spent,100,rtol=0,atol=1e-8)
    return delivered,spent

def evolve(scores, repeat):
    rng=np.random.default_rng(repeat)
    pop=np.zeros(32,dtype=int)
    for _ in range(20):
        contestants=rng.choice(pop,(31,3))
        parents=contestants[np.arange(31),np.argmax(scores[contestants],axis=1)]
        mutation=rng.choice([-1,0,1],31,p=[.15,.70,.15])
        pop=np.r_[pop[np.argmax(scores[pop])],np.clip(parents+mutation,0,4)]
    return pop[np.argmax(scores[pop])]

if __name__=='__main__':
    import pandas as pd
    ticks=defaultdict(list)
    for row in csv.DictReader(open('ticks.csv')):
        ticks[int(row['seed'])].append(coords(row['transport_sites']))
    rows=[]
    for seed,history in ticks.items():
        body=coords(Path(f'mask_{seed}.txt').read_text())
        patch=max(components(set.intersection(*history[-3:])),key=lambda s:(len(s),sorted(s)),default=set())
        if len(patch)<3: patch=set()
        for price in (.0005,.001,.005,.02):
            for axis in (0,1):
                for sign in (-1,1):
                    output,paid=measure(body,patch,axis,sign,price)
                    for k in range(5):
                        rows.append([seed,price,axis,sign,k,output[k],paid[k]])
    data=pd.DataFrame(rows,columns=['seed','price','axis','sign','allele','delivered','spent'])
    data.to_csv('capacity.csv',index=False)
    for price,d in data.groupby('price'):
        train=d.query('seed<=16 and axis==0').groupby('allele').delivered.mean().to_numpy()
        chosen=[evolve(train,i) for i in range(20)]
        test=d.query('seed>16 and axis==1').pivot(index=['seed','sign'],columns='allele',values='delivered')
        print(price, np.unique(chosen,return_counts=True), 'train',train-train[0], 'test means',(test.mean()-test[0].mean()).to_list(),'paired wins',(test.sub(test[0],axis=0).groupby(level=0).mean()>0).sum().to_list())
