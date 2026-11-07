# Distributed maintenance feasibility

Transport capacity improved mean maintenance coverage at all three predeclared demand levels, after paying local transport upkeep from the same finite resource pool.

| Demand per cell-step | Disabled coverage | Enabled coverage | Paired change (percentage points) | Positive seeds |
|---|---:|---:|---:|---:|
| 0.001 | 87.7173% | 89.0818% | +1.3645 | 24/24 |
| 0.005 | 71.8151% | 74.6085% | +2.7934 | 24/24 |
| 0.010 | 50.2761% | 50.9171% | +0.6410 | 18/24 |

Paired bootstrap95% intervals for coverage changes, in percentage points: [1.1799,1.5455], [2.4770,3.1091], [0.3851,0.9034], respectively. These are exploratory, unadjusted intervals, not confirmatory significance claims. Average transport expenditures were2.1667,1.7725,1.3474 units out of the initial100. Source direction was averaged within each developmental seed.

The assay reuses previously inspected seeds501-524 and the retained fixed candidate. It compares identical tissue and fate maps with transport enabled/disabled. All cells consume maintenance; none is an absorbing delivery sink. Positive effects therefore extend to distributed demand in this assay. Cells with unmet demand continue to conduct and may recover: there is no death mechanism, so this does not establish survival, reproduction, organ function or selected placement advantage. Conductance and upkeep are imposed mechanics, not learned within the assay.

Five contract tests passed: no-transport equivalence, exact single-cell accounting, disconnected demand, zero-resource inactivity, mirror invariance. All576 condition evaluations completed with material conservation and nonnegative resource assertions on every step.

## Reproduce

Requires Python3, numpy,pandas. With the included24 maps:

```
python -m unittest -v test_maintenance.py
python maintenance.py maps
```

The maps originate from the balanced_selection Runner and pinned simulation source8524cb46a22b7fb5e7ee5e6a69af71fac3351d9a with the Mono Unity math shim. If regenerating, run balanced_selection's Runner for501-524 and use the0_0_0.9 maps. Unity parity remains unverified. The archive includes raw results, paired results and maps; source code, protocol and reports are committed in this directory.

Next step: introduce a separately specified energy-reserve/death model, verify conservation including dead-cell resources, and test whether capacity improves viable cell-time on fresh seeds. Do not select death thresholds after inspecting those results.
