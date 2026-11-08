from pathlib import Path
import json
import numpy as np
import pandas as pd
from model import measure
p=Path(__file__).resolve().parent
CONDITIONS={'baseline':dict(), 'free_pool':dict(reserve_capacity=9), 'costed_pool':dict(reserve_capacity=9,build=.1,storage_rate=.0001,exchange_cost=.01)}
def run():
 maps=json.loads((p/'../normalized/maps.json').read_text());rows=[]
 for seed in range(701,725):
  raw=maps[f'{seed}_0.9.txt'];body={(i%64,i//64) for i,c in enumerate(raw) if c!='0'};patch={(i%64,i//64) for i,c in enumerate(raw) if c=='4'}
  for condition,kw in CONDITIONS.items():
   for axis in [0,1]:
    for sign in [-1,1]:
     r=measure(body,patch,axis,sign,**kw)
     for k in range(2):rows.append(dict(seed=seed,condition=condition,axis=axis,sign=sign,enabled=k,**{key:float(v[k]) for key,v in r.items()}))
 d=pd.DataFrame(rows);d.to_csv(p/'raw.csv',index=False);v=d.groupby(['seed','condition','enabled']).mean(numeric_only=True);v.to_csv(p/'per_seed.csv');out={'means':[],'comparisons':{}}
 for (c,k),g in d.groupby(['condition','enabled']):out['means'].append(dict(condition=c,enabled=int(k),**g[['final_alive','alive_time','acquired','construction','storage_cost']].mean().to_dict()))
 for k in [0,1]:
  for c in ['free_pool','costed_pool']:
   t=v.xs(k,level='enabled')['final_alive'].unstack();delta=(t[c]-t.baseline).to_numpy();rng=np.random.default_rng(711)
   out['comparisons'][f'{c}-{k}']={'delta':float(delta.mean()),'positive_seeds':int(sum(delta>1e-12)),'ci95':np.quantile(rng.choice(delta,(10000,24)).mean(axis=1),[.025,.975]).tolist()}
 (p/'report.json').write_text(json.dumps(out,indent=2));print('Completed costed storage diagnostic')
if __name__=='__main__':run()
