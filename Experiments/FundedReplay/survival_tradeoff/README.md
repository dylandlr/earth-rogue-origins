# Why the endpoint reversal survives free transport

This diagnostic replays the previously inspected high-demand condition (demand0.01, threshold30) on seeds601-624. It does not use fresh confirmation data. All96 prior per-seed alive-time/final-survival values reproduce within1e-12.

| Intervention | Alive cell-time | Final alive | Maintenance consumed | Transport cost | Remaining food |
|---|---:|---:|---:|---:|---:|
| Ordinary | 58.22% | 44.02% | 86.02 | 0 | 13.98 |
| Paid transport | 60.90% | 41.26% | 89.93 | 1.22 | 8.85 |
| Free transport | 61.15% | 42.20% | 90.76 | 0 | 9.24 |
| Cost only | 58.10% | 43.21% | 85.61 | 1.15 | 13.24 |

Removing upkeep improves final survival relative to paid transport by0.94 percentage points. Yet free transport remains1.82 points below ordinary (paired bootstrap95% interval[-3.50,-0.38]), while increasing alive cell-time by2.93 points (interval[2.44,3.42]). Thus upkeep is not sufficient to explain the endpoint reversal under this intervention.

Transport supports greater maintenance consumption and leaves less resource at step180. This is consistent with a resource-timing tradeoff: sustaining more cell-time can spend food that would otherwise remain available later. These aggregate traces do not identify the complete causal chain; changing survival also changes network connectivity, resource distribution and later consumption. The cost-only arm is harmful, but its effect cannot be added linearly to the other arms. Free transport removes both the monetary resource charge and the solvency gate.

Each step enforces conservation and nonnegative resource, and dead cells cannot conduct or consume. Starting resource is100; all dead-material totals arezero in these runs because starvation follows resource depletion. No model parameters were tuned after results. Bootstrap intervals are exploratory, unadjusted, with seed as the unit and directions averaged within seed.

## Reproduce

Requires Python3,numpy,pandas. Keep survival_tradeoff beside survival, containing the committed maps.json and paired.csv dependencies. Run `python survival_tradeoff/run.py` from their parent directory. Outputs go beside run.py. model.py is the prior survival measure with optional trace capture and a conductance increment argument; default behavior is regression-checked against all prior seed outcomes.

The archive contains both directories and the required inputs, raw outcomes, per-seed outcomes and mean per-step traces. The repository retains the runnable model, inputs in ../survival, and the diagnostic results.

This is a fixed-food, post-development model with imposed conductance and uncalibrated irreversible starvation damage. It does not establish biological mechanisms, whole-organism fitness, reproduction, or adaptive transport placement. Next: prespecify and compare equal-total-resource feeding schedules, then evaluate survival through a longer horizon on fresh seeds to distinguish endpoint timing from robust persistence.
