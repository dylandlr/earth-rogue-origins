from pathlib import Path
import json,argparse
import numpy as np
import pandas as pd

def measure(body,patch,axis,sign,demand=.005,threshold=15,steps=180,source_mass=100,upkeep=.001):
 points=sorted(body);n=len(points)
 if not n or demand<=0 or threshold<=0 or steps<=0 or min(source_mass,upkeep)<0:raise ValueError('Invalid parameters')
 idx={p:i for i,p in enumerate(points)};edges=[(i,idx[q]) for i,(x,y) in enumerate(points) for q in [(x+1,y),(x,y+1)] if q in idx]
 a=np.array([e[0] for e in edges],int);b=np.array([e[1] for e in edges],int)
 special=np.array([p in patch for p in points]);projection=np.array([p[axis]*sign for p in points]);source=projection==projection.min()
 mass=np.zeros((2,n));mass[:,source]=source_mass/source.sum();alive=np.ones((2,n),bool);damage=np.zeros((2,n))
 used=np.zeros(2);paid=np.zeros(2);dead=np.zeros(2);celltime=np.zeros(2)
 for _ in range(steps):
  celltime+=alive.sum(axis=1)
  active=np.zeros((2,n),bool);active[1]=special&alive[1]&(mass[1]>=upkeep)
  fee=upkeep*active;mass-=fee;paid+=fee.sum(axis=1)
  conductance=(.1+.1*(active[:,a]|active[:,b]))*(alive[:,a]&alive[:,b])
  flux=conductance*(mass[:,a]-mass[:,b])
  for k in range(2):mass[k]+=np.bincount(b,flux[k],n)-np.bincount(a,flux[k],n)
  assert mass.min()>=-1e-10
  consumed=np.minimum(np.maximum(mass,0),demand)*alive;mass-=consumed;used+=consumed.sum(axis=1)
  damage+=(1-consumed/demand)*alive
  died=alive&(damage>=threshold-1e-12);dead+=(mass*died).sum(axis=1);mass[died]=0;alive[died]=False
  assert np.all(mass[~alive]==0)
  assert np.allclose(mass.sum(axis=1)+dead+used+paid,source_mass,atol=1e-8,rtol=0)
 return {'alive_time':celltime/(n*steps),'final_alive':alive.mean(axis=1),'maintenance':used,'upkeep':paid,'dead_material':dead,'remaining':mass.sum(axis=1)}

def run(maps):
 rows=[]
 for seed in range(601,625):
  raw=(maps/f'{seed}_0.9.txt').read_text();assert len(raw)==3072 and set(raw)<=set('012345')
  body={(i%64,i//64) for i,c in enumerate(raw) if c!='0'};patch={(i%64,i//64) for i,c in enumerate(raw) if c=='4'}
  for demand in [.001,.005,.01]:
   for threshold in [5,15,30]:
    for axis in [0,1]:
     for sign in [-1,1]:
      result=measure(body,patch,axis,sign,demand,threshold)
      for k in range(2):rows.append(dict(seed=seed,demand=demand,threshold=threshold,axis=axis,sign=sign,enabled=k,cells=len(body),transport=len(patch),**{key:float(v[k]) for key,v in result.items()}))
 d=pd.DataFrame(rows);d.to_csv('raw.csv',index=False);reports=[];paired=[]
 for (demand,threshold),g in d.groupby(['demand','threshold']):
  report={'demand':demand,'threshold':int(threshold),'primary':bool(demand==.005 and threshold==15)}
  avg=g.groupby(['seed','enabled']).mean(numeric_only=True)
  for metric in ['alive_time','final_alive']:
   v=avg[metric].unstack();delta=(v[1]-v[0]).to_numpy();rng=np.random.default_rng(711)
   report[metric]={'disabled':float(v[0].mean()),'enabled':float(v[1].mean()),'delta':float(delta.mean()),'positive_seeds':int(sum(delta>1e-12)),'bootstrap95':np.quantile(rng.choice(delta,(10000,24)).mean(axis=1),[.025,.975]).tolist()}
   for seed,row in v.iterrows():paired.append(dict(seed=seed,demand=demand,threshold=threshold,metric=metric,disabled=row[0],enabled=row[1],delta=row[1]-row[0]))
  reports.append(report)
 pd.DataFrame(paired).to_csv('paired.csv',index=False);Path('report.json').write_text(json.dumps(reports,indent=2));print(json.dumps(reports,indent=2))
if __name__=='__main__':
 p=argparse.ArgumentParser();p.add_argument('maps',type=Path);run(p.parse_args().maps)
