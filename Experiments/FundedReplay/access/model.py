from pathlib import Path
import json,argparse
import numpy as np
import pandas as pd

def measure(body,patch,axis,sign,demand=.005,threshold=15,steps=180,source_mass=100,upkeep=.001,boost=.1,trace=None,rate=.1,capacity=1,uniform=False,reserve=0):
 points=sorted(body);n=len(points)
 if not n or demand<=0 or threshold<=0 or steps<=0 or min(source_mass,upkeep,rate,capacity)<0:raise ValueError('Invalid parameters')
 idx={p:i for i,p in enumerate(points)};edges=[(i,idx[q]) for i,(x,y) in enumerate(points) for q in [(x+1,y),(x,y+1)] if q in idx]
 a=np.array([e[0] for e in edges],int);b=np.array([e[1] for e in edges],int)
 special=np.array([p in patch for p in points]);projection=np.array([p[axis]*sign for p in points]);source=projection==projection.min()
 boundary_count=source.sum()
 if uniform:source=np.ones(n,bool)
 effective_rate=rate*boundary_count/source.sum()
 if reserve<0 or reserve*n>source_mass:raise ValueError('Reserve exceeds budget')
 mass=np.full((2,n),float(reserve));alive=np.ones((2,n),bool);damage=np.zeros((2,n))
 used=np.zeros(2);paid=np.zeros(2);dead=np.zeros(2);celltime=np.zeros(2)
 reservoir=np.full(2,float(source_mass-reserve*n));acquired=np.zeros(2)
 for step in range(steps):
  request=np.minimum(effective_rate,np.maximum(0,capacity-mass))*(alive&source)
  total=request.sum(axis=1)
  factor=np.minimum(1,np.divide(reservoir,total,out=np.zeros(2),where=total>0))
  uptake=request*factor[:,None];amount=uptake.sum(axis=1)
  mass+=uptake;reservoir-=amount;acquired+=amount
  assert reservoir.min()>=-1e-10
  celltime+=alive.sum(axis=1)
  active=np.zeros((2,n),bool);active[1]=special&alive[1]&(mass[1]>=upkeep)
  fee=upkeep*active;mass-=fee;paid+=fee.sum(axis=1)
  conductance=(.1+boost*(active[:,a]|active[:,b]))*(alive[:,a]&alive[:,b])
  flux=conductance*(mass[:,a]-mass[:,b])
  for k in range(2):mass[k]+=np.bincount(b,flux[k],n)-np.bincount(a,flux[k],n)
  assert mass.min()>=-1e-10
  consumed=np.minimum(np.maximum(mass,0),demand)*alive;mass-=consumed;used+=consumed.sum(axis=1)
  damage+=(1-consumed/demand)*alive
  died=alive&(damage>=threshold-1e-12);dead+=(mass*died).sum(axis=1);mass[died]=0;alive[died]=False
  assert np.all(mass[~alive]==0)
  assert np.allclose(mass.sum(axis=1)+dead+used+paid+reservoir,source_mass,atol=1e-8,rtol=0)
  if trace is not None:
   trace.append(dict(step=step+1,alive=alive.mean(axis=1).copy(),maintenance=used.copy(),upkeep=paid.copy(),remaining=mass.sum(axis=1).copy(),dead_material=dead.copy(),external=reservoir.copy(),acquired=acquired.copy(),alive_time=celltime/(n*(step+1))))
 return {'alive_time':celltime/(n*steps),'final_alive':alive.mean(axis=1),'maintenance':used,'upkeep':paid,'dead_material':dead,'remaining':mass.sum(axis=1),'external':reservoir,'acquired':acquired}
