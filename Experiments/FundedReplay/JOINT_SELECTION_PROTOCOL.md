# Joint developmental and transport selection protocol

The earlier seeds 17–24 were excluded from capacity optimization but were already used to screen the branch-0.90 variant. Their results measure transfer within a previously examined developmental set. They do not provide a fully independent end-to-end test.

## Frozen experimental design

Use archived genome 29 as the ancestor. Evolve branch 1 fateResponse and transport capacity together. Candidate fateResponse values are 0.6, 0.7, 0.8, 0.9 and 1.0. Candidate capacity levels are 0, 0.25, 0.5, 0.75 and 1.0 above ordinary transfer. Mutations move one step along one of these axes. Include ancestral response with zero upgrade as the reference.

Regrow every genome for each developmental seed. Assign capacity to every cell whose final fate is Transport, including isolated cells. This replaces the earlier investigator-selected largest persistent patch. Record full tissue geometry and fate maps for audit.

Development uses the existing 64 by 48 grid, 36 ticks, 360-cell cap and four nutrient units per site. The delivery assay uses 180 pulses and 100 nutrient units, paid local upkeep of 0.001 per fully upgraded cell per pulse, and the existing conservative transfer equation. Capacity is inactive for any cell unable to pay. The endpoint remains delivered nutrient.

Training developmental seeds are 101–116 and horizontal source directions. Validation seeds are 117–124 and all four directions. Freeze the candidate using training and validation before opening test results. Final test seeds are 201–224 and vertical directions. These seed ranges are planned, not yet executed.

Use 20 selection repeats, population 32, 20 generations, elitism and tournament size three. Report all selected candidates and comparison with exhaustive training scores because the candidate space is small.

## Controls and interpretation

Evaluate the archived ancestor, the branch-0.90 fixed-capacity reference, capacity disabled on each selected genome, and matched redistribution of transport capacity within each selected body. Match counts, connected-component sizes and initial neighbor-degree distribution where possible. Record unmatched cases explicitly.

The disabled-capacity comparison measures the contribution of the upgrade on the same body. The ancestor comparison also includes changes in geometry and cell fate. Matched redistribution examines location while controlling capacity count. These comparisons support different claims.

Report paired results per developmental seed with source directions averaged within seed. Report delivered nutrient, upkeep spent, final retained nutrient, geometry size, transport-cell count and material residuals. Use the final test set once. Any retuning after viewing it requires new seeds.

Survival, reproduction and full Unity runtime parity remain separate verification tasks. No ecological evolution claim follows from optimizing this delivery assay.

## Implementation checks completed

Five Python unittest checks passed against capacity.py using synthetic tissue. No patch produces identical results at all capacity levels. Unaffordable upgrades stay inactive. Disconnected components cannot transfer to the sink. Reflecting geometry and source direction preserves outcomes. Selection retains zero capacity when all upgrades reduce the supplied score.

The previous temporary replay workspace is absent in the current runtime. New morphology runs and joint selection remain pending regeneration of the genome-to-tissue runner and durable capture of its inputs.
