import json,sys
from pathlib import Path
import numpy as np
import pandas as pd
from capacity import measure
def evaluate(start,end,axes):
 rows=[]
 for seed in range(start,end+1):
  for response in [-1,.6,.7,.8,.9,1]:
   raw=Path(f'maps/{seed}_{response:g}.txt').read_text()
   body={(i%64,i//64) for i,c in enumerate(raw) if c!='0'}
   patch={(i%64,i//64) for i,c in enumerate(raw) if c=='4'}
   for axis in axes:
    for sign in [-1,1]:
     delivered,spent=measure(body,patch,axis,sign,.001)
     for k in range(5):rows.append([seed,response,axis,sign,k,delivered[k],spent[k],len(body),len(patch)])
 return pd.DataFrame(rows,columns=['seed','response','axis','sign','capacity','delivered','spent','cells','transport'])
if sys.argv[1]=='select':
 train=evaluate(101,116,[0]);train.to_csv('train.csv',index=False)
 scores=train.query('response>=0').groupby(['response','capacity']).delivered.mean().unstack().to_numpy()
 winners=[]
 for repeat in range(20):
  rng=np.random.default_rng(repeat);pop=np.zeros((32,2),int)
  for generation in range(20):
   values=scores[pop[:,0],pop[:,1]]
   trials=rng.integers(0,32,(31,3))
   parents=pop[trials[np.arange(31),np.argmax(values[trials],axis=1)]].copy()
   axes=rng.integers(0,2,31);parents[np.arange(31),axes]+=rng.choice([-1,0,1],31,p=[.15,.7,.15])
   pop=np.vstack([pop[np.argmax(values)],np.clip(parents,0,4)])
  w=pop[np.argmax(scores[pop[:,0],pop[:,1]])];winners.append(tuple(map(int,w)))
 validation=evaluate(117,124,[0,1]);validation.to_csv('validation.csv',index=False)
 candidates=set(winners)
 best=max(sorted(candidates),key=lambda w:validation.query('response==@r and capacity==@k',local_dict={'r':[.6,.7,.8,.9,1][w[0]],'k':w[1]}).delivered.mean())
 choice={'response':[.6,.7,.8,.9,1][best[0]],'capacity':best[1],'winners':winners,'training_argmax':list(map(int,np.unravel_index(scores.argmax(),scores.shape)))}
 Path('frozen.json').write_text(json.dumps(choice,indent=2));print(choice)
else:
 c=json.load(open('frozen.json'));test=evaluate(201,224,[1]);test.to_csv('test.csv',index=False)
 selected=test[(test.response==c['response'])&(test.capacity==c['capacity'])].groupby('seed').delivered.mean()
 for label,r,k in [('ancestor',-1,0),('disabled',c['response'],0),('fixed_09',.9,4)]:
  ref=test[(test.response==r)&(test.capacity==k)].groupby('seed').delivered.mean();delta=selected-ref
  print(label,'selected',selected.mean(),'reference',ref.mean(),'delta',delta.mean(),'wins',sum(delta>0))
