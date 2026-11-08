# Finite reservoir uptake diagnostic

Exploratory development on inspected seeds601-624, not new confirmation. Retain prior genome29 tissue and compare ordinary/paid transport independently under equal external budgets. No competition between organisms, birth or selection.

Initial internal food0. A finite external reservoir starts at100 or1000 units. Each original extremal source cell can acquire up to rate{0.02,0.1,0.5} per step, provided it is alive and its internal food is below1. Request=min(rate,max(0,1-current food)). If reservoir cannot cover aggregate requests, allocate proportionally. Deduct every uptake from the reservoir. Dead source cells cannot acquire; no source relocation. This is uptake saturation, not a hard cap on food arriving through diffusion.

Then apply prior upkeep0.001, transport conductance0.2 vs ordinary0.1, maintenance0.01, cumulative starvation threshold30. Run720 steps. Report all six budget/rate conditions; no best-condition selection. Primary descriptive endpoint: final living fraction. Secondary alive cell-time, uptake, external remainder and upkeep. Average four directions within each seed; bootstrap10000,RNG711, descriptive unadjusted95% intervals.

Assert reservoir+internal+dead material+consumed maintenance+upkeep equals initial external budget every step. Check no negative resources or dead-cell consumption. This model tests coupling between transport and acquisition under imposed uptake rules. It cannot establish persistent survival beyond720, adaptive transport placement, reproduction, Unity parity or biological validity.
