# Earth: Rogue Origins — v2.3 Causal Branch Expression

## Source: actual Unity v2.2 output

The user submitted real Unity final-generation tissue, structural and
alignment images and `population(2).csv`,
`branch_expression.csv`, `champions(2).csv`.

| Generation | Mean raw fitness | Mean novelty | Mean guide overlap |
|---|---:|---:|---:|
| 0 | 0.5854 | 0.2852 | 0.8458 |
| 1 | 0.7285 | 0.2482 | 0.8368 |
| 2 | 0.8220 | 0.2229 | 0.8456 |
| 3 | 0.8307 | 0.1983 | 0.8155 |

The final champion, id 31, scored 0.966667, versus 0.931026
in generations 1 and 2. Phenotype novelty still declined.
The final generation contains several related, compact colonies.

The prior `branch_expression.csv` marks every non-root gene as
expressed. That is a **weak, correlational** test based on occupied
pixels inside each gene's organizer territory: it does not demonstrate
the extra gene actually caused those pixels to grow.

## v2.3 intervention — same-seed gene knockouts

For each non-root gene of an evaluated organism:

1. Copy the genome without changing its random seed.
2. Remove that gene and all genetically dependent descendants.
3. Run the SAME structured recurrent developmental simulation,
   grid size, number of growth steps and tissue cap.
4. Compare original and ablated 2D occupancy masks at the exact
   same coordinates (intersection-over-union, changed cell counts).
5. Mark the branch morphologically active when its ablation changes
   at least 3.5% of the occupied union.

This is a causal intervention on **the gene subtree**. A result
does not prove that a particular appendage is functional, or
separate a gene's direct action from its effects on descendants.

The ten-percent structural component in the existing artificial
fitness now credits these **counterfactually active** branches
instead of merely overlapping growth guidance.

The exporter additionally writes
`causal_branch_ablation.csv`, with the original/ablated cell counts,
changed cells, IoU, relative effect size, and active flag for each
final-generation non-root gene. `population.csv` gains
`causal_branches` and `strongest_branch_effect`.

## Test and next run

```powershell
git pull origin main
.\scripts\export-emergent-creatures.ps1 -UnityEditor "C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe"
```

Inspect the new causal-branch CSV alongside actual Unity tissue,
alignment and population results. Tests verify parent/child
deletion, seed preservation, correctly computed occupancy effects,
and zero branch credit for an unbranched founder.

## Next major bottleneck

Do not add visual appendages by rendering unrealized genome graphs.
The next stage must connect tissue fates and spatial orientation to
measurable functions in the same coordinate frame: resource-costly
division, contractile forces, sensing, movement, resource capture,
and competition. Use actual environment outcomes to rank designs.

A gene knockout that alters size but no spatially independent
feature remains an anatomical-extent effect, **not organogenesis**.

**Status:** v2.3 source and tests committed; Unity compile, EditMode
and real rerender not yet verified.
