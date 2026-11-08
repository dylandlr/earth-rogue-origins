# Access controls expose aggregate under-supply

The finite reservoir contained ample food, but imposed total potential uptake was below initial whole-tissue maintenance demand in95/96 seed-direction cases. The median supply/demand ratio was0.210526, minimum0.083333, maximum1.0. This is a ceiling before saturation, death or transport upkeep. Large external reserves cannot overcome that imposed rate constraint.

| Access | Starting reserve/cell | Final alive, ordinary | Final alive, transport |
|---|---:|---:|---:|
| Boundary | 0.0 | 14.360% | 15.129% |
| Boundary | 0.3 | 15.469% | 16.654% |
| Uniform | 0.0 | 1.042% | 0.906% |
| Uniform | 0.3 | 1.042% | 1.042% |

Reserves are deducted from the same1000-unit budget. Uniform access preserves initial total uptake capacity and divides it over all cells; dead-site shares are not redirected. It is an artificial control. With this largely inadequate per-cell supply, uniform access performs worse than concentrating food at a boundary. This does not establish that distributed feeding is intrinsically inferior.

Boundary feeding with reserves increases paid-transport final survival from15.13% to16.65%. Transport's paired benefit in that condition is1.186 percentage points, bootstrap95% interval[0.863,1.501]. Uniform access with reserves leaves1.042% alive under both interventions. Reserves increase alive cell-time but do not fix the sustained rate deficit.

The earlier conclusion that access and distribution constrained survival was incomplete: aggregate uptake capacity was already too low to maintain the full starting tissue. Widespread loss is therefore not a clean failure of learned morphology or transport. This diagnostic does not isolate all effects of damage and geometry, and no mechanism was evolved here.

## Validation and reproduction

Four contract tests passed, including reserve accounting and equal initial aggregate uptake. All240 baseline seed-metric values reproduce the prior reservoir experiment to1e-10. Every step in768 intervention evaluations enforces resource conservation and nonnegative resources. Data reuse seeds601-624; all intervals are exploratory and unadjusted.

Requires Python3,numpy,pandas. Keep access beside survival/maps.json and reservoir/per_seed.csv. Run `python -m unittest -v test_access.py` and `python run.py` from access. Archive includes those exact inputs plus outputs. supply_audit.json records the initial demand/capacity audit across24 seeds and four directions.

Next: prespecify total uptake as a multiple of initial tissue maintenance (for example0.5,1.0,1.5), with equal total budgets and explicit upkeep, and test fresh developmental seeds. A rate at1.0 covers baseline demand only, not transport costs. Preserve boundary/uniform controls and starting-reserve accounting. Unity integration, reproduction and evolutionary adaptation remain outstanding.
