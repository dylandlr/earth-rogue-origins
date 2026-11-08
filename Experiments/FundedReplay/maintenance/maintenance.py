from pathlib import Path
import argparse,json
import numpy as np
import pandas as pd

def measure(body,patch,axis,sign,demand,steps=180,source_mass=100,upkeep=.001):
 points=sorted(body);n=len(points)
 if n==0 or demand<0 or upkeep<0 or source_mass<0:raise ValueError('Invalid tissue or rates')
 idx={p:i for i,p in enumerate(points)}
 edges=[(i,idx[q]) for i,(x,y) in enumerate(points) for q in [(x+1,y),(x,y+1)] if q in idx]
 a=np.array([e[0] for e in edges],int);b=np.array([e[1] for e in edges],int)
 special=np.array([p in patch for p in points]);projection=np.array([p[axis]*sign for p in points]);source=projection==projection.min()
 mass=np.zeros((2,n));mass[:,source]=source_mass/source.sum()
 used=np.zeros(2);paid=np.zeros(2);full=np.zeros(2)
 for _ in range(steps):
  active=np.zeros((2,n),bool);active[1]=special&(mass[1]>=upkeep)
  fee=upkeep*active;mass-=fee;paid+=fee.sum(axis=1)
  conductance=.1+.1*(active[:,a]|active[:,b]);flux=conductance*(mass[:,a]-mass[:,b])
  for k in range(2):mass[k]+=np.bincount(b,flux[k],n)-np.bincount(a,flux[k],n)
  assert mass.min()>=-1e-10
  consumed=np.minimum(np.maximum(mass,0),demand);full+=(mass>=demand-1e-12).sum(axis=1)
  mass-=consumed;used+=consumed.sum(axis=1)
  assert np.allclose(mass.sum(axis=1)+used+paid,source_mass,atol=1e-8,rtol=0)
 total=demand*n*steps
 return {'coverage':used/total if total else np.ones(2),'full_fraction':full/(n*steps),'maintenance':used,'upkeep':paid,'remaining':mass.sum(axis=1)}

def run(maps):
 rows=[]
 for seed in range(501,525):
  raw=(maps/f'{seed}_0_0_0.9.txt').read_text();assert len(raw)==3072 and set(raw)<=set('012345')
  body={(i%64,i//64) for i,c in enumerate(raw) if c!='0'};patch={(i%64,i//64) for i,c in enumerate(raw) if c=='4'}
  for demand in [.001,.005,.01]:
   for axis in [0,1]:
    for sign in [-1,1]:
     result=measure(body,patch,axis,sign,demand)
     for k in range(2):rows.append(dict(seed=seed,demand=demand,axis=axis,sign=sign,enabled=k,cells=len(body),transport=len(patch),**{key:float(v[k]) for key,v in result.items()}))
 d=pd.DataFrame(rows);d.to_csv('raw.csv',index=False);reports=[];paired=[]
 for demand,g in d.groupby('demand'):
  avg=g.groupby(['seed','enabled']).mean(numeric_only=True)
  report={'demand':demand}
  for metric in ['coverage','full_fraction','upkeep']:
   v=avg[metric].unstack();delta=(v[1]-v[0]).to_numpy();rng=np.random.default_rng(711)
   report[metric]={'disabled':float(v[0].mean()),'enabled':float(v[1].mean()),'delta':float(delta.mean()),'positive_seeds':int(sum(delta>1e-12)),'bootstrap95':np.quantile(rng.choice(delta,(10000,24)).mean(axis=1),[.025,.975]).tolist()}
   for seed,row in v.iterrows():paired.append(dict(seed=seed,demand=demand,metric=metric,disabled=row[0],enabled=row[1],delta=row[1]-row[0]))
  reports.append(report)
 pd.DataFrame(paired).to_csv('paired.csv',index=False);Path('report.json').write_text(json.dumps(reports,indent=2));print(json.dumps(reports,indent=2))
if __name__=='__main__':
 p=argparse.ArgumentParser();p.add_argument('maps',type=Path);run(p.parse_args().maps)
