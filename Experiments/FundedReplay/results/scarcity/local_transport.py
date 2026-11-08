"""Run from the directory containing masks/. One synchronous exchange per pulse."""
from pathlib import Path
import csv
import numpy as np

def exchange(reserve, live, rate):
    delta = np.zeros_like(reserve)
    for axis in (0, 1):
        left, right = [slice(None)]*2, [slice(None)]*2
        left[axis], right[axis] = slice(None,-1), slice(1,None)
        a,b = tuple(left), tuple(right)
        flux = rate*(reserve[a]-reserve[b])*(live[a]&live[b])
        delta[a] -= flux
        delta[b] += flux
    return reserve+delta

def assay(mask, rate, endowment):
    live=mask.copy()
    reserve=endowment*live
    nutrient=np.ones(mask.shape)
    initial=nutrient.sum()+live.sum()+reserve.sum()
    waste=0.0
    cell_pulses=0
    first_loss=None
    for pulse in range(1,181):
        edge=np.zeros_like(live)
        edge[1:] |= ~live[:-1]
        edge[:-1] |= ~live[1:]
        edge[:,1:] |= ~live[:,:-1]
        edge[:,:-1] |= ~live[:,1:]
        capture=.5*nutrient*(edge&live)
        nutrient-=capture
        reserve=exchange(reserve+capture,live,rate)
        assert reserve.min()>=-1e-12
        lost=live & (reserve<.0093)
        if lost.any() and first_loss is None:
            first_loss=pulse
        waste+=lost.sum()+reserve[lost].sum()
        reserve[lost]=0
        live[lost]=False
        reserve[live]-=.0093
        waste+=.0093*live.sum()
        cell_pulses+=int(live.sum())
        assert abs(nutrient.sum()+reserve.sum()+live.sum()+waste-initial)<1e-7
        if not live.any():
            break
    return first_loss,cell_pulses,int(live.sum()),pulse if not live.any() else None

if __name__=='__main__':
    with open('local_transport.csv','w',newline='') as file:
        writer=csv.writer(file)
        writer.writerow(['seed','form','rate','initial_reserve_per_cell','first_loss','cell_pulses','final_cells','extinction'])
        for path in sorted(Path('masks').glob('*.txt')):
            seed,form=path.stem.split('_')
            mask=np.array([list(s) for s in path.read_text().splitlines()])=='#'
            for reserve in (0.0,.093):
                for rate in (0,.1,.24):
                    writer.writerow([seed,form,rate,reserve,*assay(mask,rate,reserve)])
