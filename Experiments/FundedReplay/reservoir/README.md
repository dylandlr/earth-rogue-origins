# Finite reservoir: conditional transport benefit

Exploratory test on reused seeds601-624, fixed morphology and four source directions,720 steps. Each transport intervention has its own equal finite reservoir; organisms do not compete. All six prespecified conditions are retained.

| External budget | Uptake rate/site/step | Final alive, ordinary | Final alive, transport | Delta, percentage points | Seeds improved |
|---|---:|---:|---:|---:|---:|
| 100 | 0.02 | 4.944% | 4.773% | -0.171 | 0/24 |
| 100 | 0.1 | 5.571% | 5.059% | -0.513 | 8/24 |
| 100 | 0.5 | 5.756% | 4.703% | -1.053 | 7/24 |
| 1000 | 0.02 | 5.303% | 5.123% | -0.180 | 0/24 |
| 1000 | 0.1 | 14.360% | 15.129% | +0.768 | 22/24 |
| 1000 | 0.5 | 15.004% | 16.376% | +1.372 | 22/24 |

At budget1000/rate0.5, transport increases final survival1.372 percentage points (descriptive paired bootstrap95% interval[1.005,1.745]). Uptake rises from116.73 to128.88 units out of1000. At budget1000/rate0.1, final survival increases0.768 points ([0.566,0.979]). Both improve22/24 seeds. These are conditional exploratory effects; there is no overall promotion or fresh confirmation.

At budget100 and both faster rates, transport acquires more food but leaves fewer final survivors. At rate0.02 it hurts under both budgets. Transport is not uniformly beneficial. Faster redistribution can support acquisition under this saturation rule, but effects remain coupled to upkeep, damage and changing connectivity. The experiment does not separately identify those mechanisms.

Even the strongest condition retains only16.38% of initial cells, with871.12 resource units still external on average. Increasing available food alone does not prevent widespread death; fixed feeding-site geometry, uptake saturation and irreversible early starvation constrain access and distribution. This identifies an assay limitation to investigate, not evidence of an evolved organ.

## Validation and reproduction

Six contract tests passed: uptake rate, reservoir exhaustion, saturation, dead-cell uptake cessation, zero uptake and no-patch equivalence. All1152 evaluations checked conservation of external+internal+dead+maintenance+upkeep resource, nonnegative resource, and dead-cell restrictions every step. No parameter tuning followed outcomes.

Python3 with numpy,pandas. Keep reservoir beside survival containing maps.json. From reservoir run:

```
python -m unittest -v test_reservoir.py
python run.py
```

Archive includes raw outcomes, per-seed summaries, reports, code and exact maps. The repository preserves source, summaries and maps in ../survival. C# development remains the previous Mono-derived snapshot; this Python extension has not been integrated into Unity. Reproduction, evolutionary selection, competition and persistence beyond720 remain untested.

Next: freeze a spatial food-access model and test fresh developmental seeds, including a uniform-access control and explicit local reserves. Keep starting reserves within the same budget and distinguish early starvation from long-run acquisition. Do not reuse601-624 as a holdout.
