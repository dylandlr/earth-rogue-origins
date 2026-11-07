import json,sys,itertools
from pathlib import Path
import numpy as np
import pandas as pd
from capacity import measure

def evaluate(seeds,axes,candidates=None):
 rows=[]
 morphs=list(itertools.product(range(-1,2),range(-1,2),[.8,.9,1.])) if candidates is None else sorted(set(tuple(c[:3]) for c in candidates))
 for seed in seeds:
  for x,y,r in morphs:
   raw=Path(f'maps/{seed}_{x}_{y}_{r:g}.txt').read_text()
   assert len(raw)==3072 and set(raw)<=set('012345')
   body={(i%64,i//64) for i,c in enumerate(raw) if c!='0'};patch={(i%64,i//64) for i,c in enumerate(raw) if c=='4'}
   for axis,sign in itertools.product(axes,[-1,1]):
    delivery,spent=measure(body,patch,axis,sign,.001)
    for k in range(5):rows.append([seed,x,y,r,k,axis,sign,delivery[k],spent[k],len(body),len(patch)])
 return pd.DataFrame(rows,columns=['seed','x','y','response','capacity','axis','sign','delivery','spent','cells','transport'])

def values(df,c):
 x,y,r,k=c
 return df.query('x==@x and y==@y and response==@r and capacity==@k').groupby('seed').delivery.mean()

if __name__=='__main__':
 if sys.argv[1]=='select':
  train=evaluate(range(301,309),[0]);train.to_csv('train.csv',index=False)
  scores=train.groupby(['x','y','response','capacity']).delivery.mean().sort_values(ascending=False,kind='stable')
  finalists=[list(c) for c in scores.index[:3]]
  val=evaluate(range(309,313),[0,1],finalists);val.to_csv('validation.csv',index=False)
  winner=max(finalists,key=lambda c:values(val,c).mean())
  frozen={'winner':winner,'finalists':finalists,'training_means':[float(scores.loc[tuple(c)]) for c in finalists],'validation_means':[float(values(val,c).mean()) for c in finalists]}
  Path('frozen.json').write_text(json.dumps(frozen,indent=2));print(frozen)
 else:
  c=json.loads(Path('frozen.json').read_text())['winner'];baseline=[0,0,.9,4];disabled=c[:3]+[0]
  data=evaluate(range(401,425),[1],[c,baseline,disabled]);data.to_csv('test.csv',index=False)
  result=pd.DataFrame({'selected':values(data,c),'prior':values(data,baseline),'disabled':values(data,disabled)})
  report={'winner':c,'means':result.mean().to_dict(),'comparisons':{}}
  for label in ['prior','disabled']:
   delta=(result.selected-result[label]).to_numpy();rng=np.random.default_rng(711)
   ci=np.quantile(rng.choice(delta,(10000,len(delta))).mean(axis=1),[.025,.975])
   report['comparisons'][label]={'mean_delta':float(delta.mean()),'positive_seeds':int(sum(delta>0)),'bootstrap_95_percent':ci.tolist()}
  result.to_csv('paired_test.csv');Path('report.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
