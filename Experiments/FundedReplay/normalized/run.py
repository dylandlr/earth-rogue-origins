from pathlib import Path
import json
import numpy as np
import pandas as pd
from model import measure
p=Path(__file__).resolve().parent

def parameters(body,axis,sign,ratio):
 n=len(body);v=[x[axis]*sign for x in body];boundary=v.count(min(v));d=.01
 return dict(demand=d,threshold=30,steps=720,source_mass=2*n*d*720+.3*n,rate=ratio*n*d/boundary,reserve=.3)

def run():
 rows=[]
 for seed in range(701,725):
  raw=(p/'maps'/f'{seed}_0.9.txt').read_text();assert len(raw)==3072 and set(raw)<=set('012345')
  body={(i%64,i//64) for i,c in enumerate(raw) if c!='0'};patch={(i%64,i//64) for i,c in enumerate(raw) if c=='4'}
  for uniform in [False,True]:
   for ratio in [.5,1,1.5]:
    for axis in [0,1]:
     for sign in [-1,1]:
      kwargs=parameters(body,axis,sign,ratio);r=measure(body,patch,axis,sign,uniform=uniform,**kwargs)
      for k in range(2):rows.append(dict(seed=seed,uniform=uniform,ratio=ratio,axis=axis,sign=sign,enabled=k,cells=len(body),budget=kwargs['source_mass'],**{key:float(v[k]) for key,v in r.items()}))
 d=pd.DataFrame(rows);d.to_csv(p/'raw.csv',index=False);v=d.groupby(['uniform','ratio','seed','enabled']).mean(numeric_only=True);v.to_csv(p/'per_seed.csv');reports=[]
 for (uniform,ratio),g in d.groupby(['uniform','ratio']):
  avg=g.groupby(['seed','enabled']).mean(numeric_only=True);report={'uniform':bool(uniform),'ratio':ratio,'primary':bool(not uniform and ratio==1.5)}
  for metric in ['final_alive','alive_time','acquired','external','upkeep']:
   m=avg[metric].unstack();delta=(m[1]-m[0]).to_numpy();rng=np.random.default_rng(711)
   report[metric]={'disabled':float(m[0].mean()),'enabled':float(m[1].mean()),'delta':float(delta.mean()),'positive_seeds':int(sum(delta>1e-12)),'ci95':np.quantile(rng.choice(delta,(10000,24)).mean(axis=1),[.025,.975]).tolist()}
  reports.append(report)
 (p/'report.json').write_text(json.dumps(reports,indent=2));print('Completed fresh-seed normalized supply pilot')
if __name__=='__main__':run()
