# Uptake saturation is a major assay constraint

All initial cells are four-neighbor reachable from the feeding boundary in96/96 seed-direction cases. Initial disconnected components therefore do not explain this benchmark's poor survival. This says nothing about disconnection after cells die.

Raising the uptake-storage limit from1 to10, without changing total food budget or potential uptake rate, gives:

| Uptake limit | Final alive, ordinary | Final alive, paid transport |
|---|---:|---:|
| 1 | 13.637% | 14.867% |
| 10 | 44.697% | 48.310% |

The higher limit improves final survival on24/24 seeds for both interventions. With transport, the increase is33.442 percentage points, paired bootstrap95% interval[32.306,34.582]. Acquired food increases from118.09 to591.92 units on average. Ordinary transport likewise acquires substantially more food. Equal budget and rate ceiling do not imply equal realized uptake.

At limit10, paid transport adds3.613 percentage points of final survival over ordinary conductance (interval[3.138,4.095]), improving24/24 seeds. At limit1 its advantage remains1.231 points. The stronger storage intervention therefore does not eliminate the transport effect.

These results identify the imposed uptake saturation rule as a major constraint. Increasing the limit allows larger local food accumulation and potentially steeper diffusion gradients; these effects are coupled in this model and are not separately identified here. The intervention was imposed by the experimenter: this is not evolved storage, a newly evolved organ, or biological validation. Seeds701-724 were already inspected; intervals are exploratory and unadjusted. Limit10 is not promoted as an optimized setting.

## Verification and reproduction

Four diagnostic tests passed: disconnected island, diagonal separation, unchanged rate ceiling and larger uptake allowance. Cap1 reproduces all240 prior seed-metric outcomes to1e-10. Every simulation step retains the existing resource conservation, nonnegativity and dead-cell restrictions.384 intervention evaluations ran720 steps each.

Requires Python3,numpy,pandas. Keep delivery_audit beside normalized containing model.py,maps.json,per_seed.csv. From delivery_audit:

```
python -m unittest -v test_audit.py
python run.py
python transport_contrasts.py
```

Archive includes exact dependencies and raw outputs. Repository retains source, per-seed summaries, connectivity audit and reports. Fixed Mono-derived developmental tissue, no Unity integration or reproduction.

Next: separate physical storage capacity from the uptake gate and diffusion concentration law. If storage becomes heritable, assign its material or maintenance costs before selection. Then test a frozen, costed model on fresh seeds. Raising a free numerical limit alone cannot establish adaptive morphogenesis.
