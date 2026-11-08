from pathlib import Path
import json
import numpy as np
import pandas as pd
from model import measure

def schedules():
 a=np.zeros(720);a[0]=100
 b=np.zeros(720);b[np.arange(0,180,30)]=100/6
 c=np.zeros(720);c[:180]=100/180
 return dict(upfront=a,pulses=b,gradual=c)

def run():
 p=Path(__file__).resolve().parent;maps=json.loads((p/'../survival/maps.json').read_text());rows=[]
 for seed in range(601,625):
  raw=maps[f'{seed}_0.9.txt'];body={(i%64,i//64) for i,c in enumerate(raw) if c!='0'};patch={(i%64,i//64) for i,c in enumerate(raw) if c=='4'}
  for name,schedule in schedules().items():
   for axis in [0,1]:
    for sign in [-1,1]:
     trace=[];measure(body,patch,axis,sign,demand=.01,threshold=30,steps=720,schedule=schedule,trace=trace)
     for horizon in [180,360,720]:
      t=trace[horizon-1]
      for k in range(2):rows.append(dict(seed=seed,schedule=name,axis=axis,sign=sign,horizon=horizon,enabled=k,**{key:float(v[k]) for key,v in t.items() if key!='step'}))
 d=pd.DataFrame(rows);d.to_csv(p/'raw.csv',index=False);v=d.groupby(['schedule','horizon','seed','enabled']).mean(numeric_only=True);v.to_csv(p/'per_seed.csv')
 reports=[]
 for (schedule,horizon),g in d.groupby(['schedule','horizon']):
  avg=g.groupby(['seed','enabled']).mean(numeric_only=True);report={'schedule':schedule,'horizon':int(horizon)}
  for metric in ['alive_time','alive','inaccessible','remaining']:
   m=avg[metric].unstack();delta=(m[1]-m[0]).to_numpy();rng=np.random.default_rng(711)
   report[metric]={'disabled':float(m[0].mean()),'enabled':float(m[1].mean()),'delta':float(delta.mean()),'ci95':np.quantile(rng.choice(delta,(10000,24)).mean(axis=1),[.025,.975]).tolist()}
  reports.append(report)
 prior=pd.read_csv(p/'../survival/paired.csv').query('demand==0.01 and threshold==30')
 old=v.loc[('upfront',180)]
 for metric,original in [('alive_time','alive_time'),('alive','final_alive')]:
  expected=prior.query('metric==@original').set_index('seed');actual=old[metric].unstack()
  np.testing.assert_allclose(actual[0],expected.disabled,atol=1e-12,rtol=0);np.testing.assert_allclose(actual[1],expected.enabled,atol=1e-12,rtol=0)
 (p/'report.json').write_text(json.dumps(reports,indent=2));print('Prior 96 per-seed outcomes reproduced; all schedule runs complete')
if __name__=='__main__':run()
