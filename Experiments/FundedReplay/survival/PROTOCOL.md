# Starvation and death pilot

Freeze before new maps or assay outcomes. Use retained genome29 candidate: original attachments, branch fateResponse0.9, capacity disabled versus enabled on identical tissue. Fresh developmental seeds601-624; no candidate tuning. Same36-tick Mono development and funded-growth parameters as earlier studies.

After development, put100 resource units at one extremal boundary, equally among source cells.180 synchronous steps; four directions averaged within seed. Upkeep0.001 per solvent, living transport cell before diffusion. Living edges conduct at0.1, or0.2 if either endpoint activates transport. Dead cells conduct nothing. Living cells consume up to their maintenance demand after diffusion.

Cumulative starvation damage increases by (unmet demand / demand) each step. No healing or reset; this is a dimensionless deficit counter, not an energy reserve. A cell dies when damage reaches threshold; remaining resource moves into a sealed dead-material ledger and cannot be reused. No births. Deaths occur after the current maintenance step. Alive cell-time counts cells alive at step start, divided by initial cells*180.

Primary condition: demand0.005, death threshold15 full-demand-equivalent missed steps. Sensitivity: full grid demands{0.001,0.005,0.01} x thresholds{5,15,30}, reported separately with no cherry-picking. Primary paired metric enabled-minus-disabled normalized alive cell-time. Secondary final surviving fraction. Bootstrap10000 seeds,RNG711; directions are not independent replicates. Survival benefit established within this stipulated assay only if primary95% interval lower bound>0. All sensitivity intervals descriptive/unadjusted.

Check nonnegative resources and initial100 = living resource + dead resource + maintenance consumed + transport spent each step. This is a post-development Python model with explicit uncalibrated death rules; no claim of biological validity, Unity runtime parity, evolved placement, reproduction or whole-organism fitness. Threshold sensitivity cannot eliminate structural model dependence.
