import json
from pathlib import Path
import numpy as np
import pandas as pd
from model import measure
p=Path(__file__).resolve().parent
maps=json.loads((p/'../survival/maps.json').read_text());rows=[];traces=[]
for seed in range(601,625):
 raw=maps[f'{seed}_0.9.txt'];body={(i%64,i//64) for i,c in enumerate(raw) if c!='0'};patch={(i%64,i//64) for i,c in enumerate(raw) if c=='4'}
 for axis in [0,1]:
  for sign in [-1,1]:
   for arm,cost,boost in [('paid',.001,.1),('free',0,.1),('cost_only',.001,0)]:
    trace=[];r=measure(body,patch,axis,sign,demand=.01,threshold=30,upkeep=cost,boost=boost,trace=trace)
    for label,k in ([(arm,1),('ordinary',0)] if arm=='paid' else [(arm,1)]):
     rows.append(dict(seed=seed,axis=axis,sign=sign,arm=label,**{key:float(v[k]) for key,v in r.items()}))
     traces.extend(dict(seed=seed,axis=axis,sign=sign,arm=label,**{key:(v if key=='step' else float(v[k])) for key,v in t.items()}) for t in trace)
d=pd.DataFrame(rows);d.to_csv(p/'raw.csv',index=False)
t=pd.DataFrame(traces);t.groupby(['arm','step']).mean(numeric_only=True).drop(columns=['seed','axis','sign']).to_csv(p/'mean_trace.csv')
v=d.groupby(['seed','arm']).mean(numeric_only=True);v.to_csv(p/'per_seed.csv');report={'means':d.groupby('arm').mean(numeric_only=True).drop(columns=['seed','axis','sign']).to_dict('index'),'comparisons':{}}
for arm,ref in [('paid','ordinary'),('free','ordinary'),('cost_only','ordinary'),('paid','free')]:
 result={}
 for metric in ['alive_time','final_alive']:
  m=v[metric].unstack();delta=(m[arm]-m[ref]).to_numpy();rng=np.random.default_rng(711)
  result[metric]={'delta':float(delta.mean()),'positive':int(sum(delta>1e-12)),'ci95':np.quantile(rng.choice(delta,(10000,24)).mean(axis=1),[.025,.975]).tolist()}
 report['comparisons'][arm+'-'+ref]=result
# Regression against persisted prior results (each seed, not just aggregate).
prior=pd.read_csv(p/'../survival/paired.csv').query('demand==0.01 and threshold==30')
for metric in ['alive_time','final_alive']:
 expected=prior.query('metric==@metric').set_index('seed');actual=v[metric].unstack()
 np.testing.assert_allclose(actual['ordinary'],expected['disabled'],rtol=0,atol=1e-12)
 np.testing.assert_allclose(actual['paid'],expected['enabled'],rtol=0,atol=1e-12)
report['prior_replay']='All 96 per-seed outcome values match to 1e-12'
(p/'report.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
