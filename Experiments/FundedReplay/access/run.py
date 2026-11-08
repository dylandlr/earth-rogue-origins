from pathlib import Path
import json
import numpy as np
import pandas as pd
from model import measure
p=Path(__file__).resolve().parent

def run():
 maps=json.loads((p/'../survival/maps.json').read_text());rows=[]
 for seed in range(601,625):
  raw=maps[f'{seed}_0.9.txt'];body={(i%64,i//64) for i,c in enumerate(raw) if c!='0'};patch={(i%64,i//64) for i,c in enumerate(raw) if c=='4'}
  for uniform in [False,True]:
   for reserve in [0,.3]:
    for axis in [0,1]:
     for sign in [-1,1]:
      r=measure(body,patch,axis,sign,demand=.01,threshold=30,steps=720,source_mass=1000,rate=.1,uniform=uniform,reserve=reserve)
      for k in range(2):rows.append(dict(seed=seed,uniform=uniform,reserve=reserve,axis=axis,sign=sign,enabled=k,**{key:float(v[k]) for key,v in r.items()}))
 d=pd.DataFrame(rows);d.to_csv(p/'raw.csv',index=False);v=d.groupby(['uniform','reserve','seed','enabled']).mean(numeric_only=True);v.to_csv(p/'per_seed.csv');reports=[]
 for (uniform,reserve),g in d.groupby(['uniform','reserve']):
  avg=g.groupby(['seed','enabled']).mean(numeric_only=True);report={'uniform':bool(uniform),'reserve':reserve}
  for metric in ['final_alive','alive_time','acquired','external','upkeep']:
   m=avg[metric].unstack();delta=(m[1]-m[0]).to_numpy();rng=np.random.default_rng(711)
   report[metric]={'disabled':float(m[0].mean()),'enabled':float(m[1].mean()),'delta':float(delta.mean()),'ci95':np.quantile(rng.choice(delta,(10000,24)).mean(axis=1),[.025,.975]).tolist()}
  reports.append(report)
 old=pd.read_csv(p/'../reservoir/per_seed.csv').query('budget==1000 and rate==0.1').set_index(['seed','enabled']).sort_index()
 new=v.loc[(False,0)].sort_index()
 for metric in ['final_alive','alive_time','acquired','external','upkeep']:
  np.testing.assert_allclose(new[metric],old[metric],atol=1e-10,rtol=0)
 (p/'report.json').write_text(json.dumps(reports,indent=2));print('All conditions complete; 240 baseline seed-metric values reproduced')
if __name__=='__main__':run()
