# Maintenance feasibility diagnostic

Use the retained fixed candidate (attachment offsets0,0; branch fateResponse0.9) on existing developmental seeds501-524. These seeds have been inspected: this is an exploratory mechanistic diagnostic, not a fresh confirmatory holdout or model selection.

For each tissue compare ordinary conductance0.1 with transport conductance0.2 on edges touching an active transport cell. Same source mass100,180 steps, four cardinal source directions. No absorbing sink: all cells demand maintenance after each diffusion step. Demand rates0.001,0.005,0.01 per cell per step are reported separately, with no post hoc choice of winner. Transport upkeep0.001 per active transport cell per step, paid before diffusion. Each cell consumes min(available,demand); unmet demand is recorded. No death, regrowth or reproduction is introduced.

Primary metric: fraction of total maintenance demand supplied. Secondary: fraction of cell-steps fully supplied and total transport upkeep. Average directions within seed; paired enabled-minus-disabled means and bootstrap95% intervals (10000 resamples,RNG711). Material accounting: remaining+maintenance consumed+transport upkeep=100 every step. Geometry and differentiated fate remain fixed across each intervention.

This tests whether the delivery-assay benefit extends to distributed demand. Even positive results do not establish organism survival or adaptive placement. The executable model remains a Python post-development assay using Mono-generated fate maps.
