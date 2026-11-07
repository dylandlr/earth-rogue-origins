import json
from pathlib import Path
import numpy as np
import pandas as pd
from search import values
p=Path('.')
c=json.loads((p/'frozen.json').read_text())['winner'];d=pd.read_csv('test.csv');out={}
for axis,g in d.groupby('axis'):
 a=values(g,c);b=values(g,[0,0,.9,4]);delta=(a-b).to_numpy();rng=np.random.default_rng(711)
 out[str(axis)]={'selected':float(a.mean()),'prior':float(b.mean()),'delta':float(delta.mean()),'positive_seeds':int(sum(delta>0)),'interval':np.quantile(rng.choice(delta,(10000,len(delta))).mean(axis=1),[.025,.975]).tolist()}
(p/'axis_report.json').write_text(json.dumps(out,indent=2));print(json.dumps(out,indent=2))
