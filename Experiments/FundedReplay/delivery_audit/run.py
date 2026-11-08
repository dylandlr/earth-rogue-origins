from pathlib import Path
import sys,json
from collections import deque
import numpy as np
import pandas as pd
p=Path(__file__).resolve().parent
sys.path.insert(0,str(p/'../normalized'))
from model import measure

def reachable(body,sources):
 found=set(sources);todo=deque(sources)
 while todo:
  x,y=todo.popleft()
  for q in [(x+1,y),(x-1,y),(x,y+1),(x,y-1)]:
   if q in body and q not in found:found.add(q);todo.append(q)
 return found

def run():
 maps=json.loads((p/'../normalized/maps.json').read_text());rows=[];graphs=[]
 for seed in range(701,725):
  raw=maps[f'{seed}_0.9.txt'];body={(i%64,i//64) for i,c in enumerate(raw) if c!='0'};patch={(i%64,i//64) for i,c in enumerate(raw) if c=='4'};n=len(body)
  for axis in [0,1]:
   for sign in [-1,1]:
    edge=min(q[axis]*sign for q in body);sources={q for q in body if q[axis]*sign==edge}
    graphs.append(dict(seed=seed,axis=axis,sign=sign,cells=n,sources=len(sources),reachable=len(reachable(body,sources))))
    for cap in [1,10]:
     r=measure(body,patch,axis,sign,demand=.01,threshold=30,steps=720,source_mass=14.7*n,rate=1.5*n*.01/len(sources),reserve=.3,capacity=cap)
     for k in range(2):rows.append(dict(seed=seed,axis=axis,sign=sign,capacity=cap,enabled=k,**{key:float(v[k]) for key,v in r.items()}))
 d=pd.DataFrame(rows);d.to_csv(p/'raw.csv',index=False);g=pd.DataFrame(graphs);g.to_csv(p/'connectivity.csv',index=False)
 v=d.groupby(['seed','capacity','enabled']).mean(numeric_only=True);v.to_csv(p/'per_seed.csv')
 prior=pd.read_csv(p/'../normalized/per_seed.csv').query('uniform==False and ratio==1.5').set_index(['seed','enabled']).sort_index()
 base=v.xs(1,level='capacity').sort_index()
 for key in ['final_alive','alive_time','acquired','external','upkeep']:np.testing.assert_allclose(base[key],prior[key],atol=1e-10,rtol=0)
 report={'all_initial_cells_reachable':bool((g.reachable==g.cells).all()),'fully_reachable_cases':int(sum(g.reachable==g.cells)),'cases':len(g),'mean_initial_reachable_fraction':float((g.reachable/g.cells).mean()),'means':[],'comparisons':{}}
 for (cap,k),t in d.groupby(['capacity','enabled']):report['means'].append(dict(capacity=int(cap),enabled=int(k),**t[['final_alive','alive_time','acquired','upkeep']].mean().to_dict()))
 for k in [0,1]:
  metrics={}
  for key in ['final_alive','alive_time','acquired']:
   z=v.xs(k,level='enabled')[key].unstack();delta=(z[10]-z[1]).to_numpy();rng=np.random.default_rng(711)
   metrics[key]={'delta':float(delta.mean()),'positive_seeds':int(sum(delta>1e-12)),'ci95':np.quantile(rng.choice(delta,(10000,24)).mean(axis=1),[.025,.975]).tolist()}
  report['comparisons'][str(k)]=metrics
 (p/'report.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
if __name__=='__main__':run()
