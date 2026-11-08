import numpy as np

def measure(body,patch,axis,sign,C=1,G=1,build=0,storage_rate=0,steps=720,ratio=1.5):
 if C<1 or G<0 or min(build,storage_rate)<0:raise ValueError('Invalid storage parameters')
 points=sorted(body);n=len(points)
 if n==0:raise ValueError('Empty tissue')
 idx={p:i for i,p in enumerate(points)};edges=[(i,idx[q]) for i,(x,y) in enumerate(points) for q in [(x+1,y),(x,y+1)] if q in idx]
 a=np.array([e[0] for e in edges],int);b=np.array([e[1] for e in edges],int)
 projection=np.array([p[axis]*sign for p in points]);source=projection==projection.min();special=np.array([p in patch for p in points])
 budget=14.7*n;construction=build*(C-1)*n
 if construction+.3*n>budget:raise ValueError('Unaffordable construction')
 external=np.full(2,budget-construction-.3*n);mass=np.full((2,n),.3);alive=np.ones((2,n),bool);damage=np.zeros((2,n))
 basal=np.zeros(2);storage=np.zeros(2);transport=np.zeros(2);dead=np.zeros(2);time=np.zeros(2);acquired=np.zeros(2)
 sd=storage_rate*(C-1);demand=.01+sd
 for _ in range(steps):
  request=np.minimum(ratio*n*.01/source.sum(),np.maximum(0,min(C,G)-mass))*(alive&source)
  total=request.sum(axis=1);factor=np.minimum(1,np.divide(external,total,out=np.zeros(2),where=total>0));uptake=request*factor[:,None]
  mass+=uptake;external-=uptake.sum(axis=1);acquired+=uptake.sum(axis=1);time+=alive.sum(axis=1)
  active=np.zeros((2,n),bool);active[1]=special&alive[1]&(mass[1]>=.001)
  fee=.001*active;mass-=fee;transport+=fee.sum(axis=1)
  conductance=(.1+.1*(active[:,a]|active[:,b]))*(alive[:,a]&alive[:,b]);flux=conductance*(mass[:,a]/C-mass[:,b]/C)
  for k in range(2):
   incoming=np.bincount(b,np.maximum(flux[k],0),n)+np.bincount(a,np.maximum(-flux[k],0),n)
   scale=np.minimum(1,np.divide(np.maximum(0,C-mass[k]),incoming,out=np.ones(n),where=incoming>0))
   f=flux[k]*np.where(flux[k]>=0,scale[b],scale[a]);mass[k]+=np.bincount(b,f,n)-np.bincount(a,f,n)
  consumed=np.minimum(np.maximum(mass,0),demand)*alive;mass-=consumed
  basal+=consumed.sum(axis=1)*(.01/demand);storage+=consumed.sum(axis=1)*(sd/demand)
  damage+=(1-consumed/demand)*alive;died=alive&(damage>=30-1e-12);dead+=(mass*died).sum(axis=1);mass[died]=0;alive[died]=False
  assert mass.min()>=-1e-10 and mass.max()<=C+1e-10 and external.min()>=-1e-10
  assert np.allclose(external+mass.sum(axis=1)+basal+storage+transport+dead+construction,budget,atol=1e-8,rtol=0)
 return dict(final_alive=alive.mean(axis=1),alive_time=time/(n*steps),acquired=acquired,external=external,basal=basal,storage_cost=storage,transport_cost=transport,construction=np.full(2,construction),remaining=mass.sum(axis=1),dead_material=dead)
