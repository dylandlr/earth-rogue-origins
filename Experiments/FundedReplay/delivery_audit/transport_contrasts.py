from pathlib import Path
import json
import numpy as np
import pandas as pd
p=Path(__file__).resolve().parent;d=pd.read_csv(p/'per_seed.csv');out={}
for cap,g in d.groupby('capacity'):
 out[str(cap)]={}
 for metric in ['final_alive','alive_time']:
  v=g.pivot(index='seed',columns='enabled',values=metric);delta=(v[1]-v[0]).to_numpy();rng=np.random.default_rng(711)
  out[str(cap)][metric]={'delta':float(delta.mean()),'positive_seeds':int(sum(delta>1e-12)),'ci95':np.quantile(rng.choice(delta,(10000,24)).mean(axis=1),[.025,.975]).tolist()}
(p/'transport_contrasts.json').write_text(json.dumps(out,indent=2));print(json.dumps(out,indent=2))
