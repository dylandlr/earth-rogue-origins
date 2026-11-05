# Emergent Creature Generator v2.2 — Selection for Expressed Anatomy

## Actual v2.1 Unity result

The actual Unity generation, separately scaled structural view, and
spatially aligned guide/tissue image were inspected alongside 32 rows
of `population(1).csv` and four champion rows.

| Generation | Mean proxy fitness | Mean nearest-neighbor novelty | Mean guide/tissue IoU |
|---|---:|---:|---:|
| 0 | 0.6333 | 0.2852 | 0.8458 |
| 1 | 0.7868 | 0.2482 | 0.8368 |
| 2 | 0.8644 | 0.2271 | 0.8442 |
| 3 | 0.8781 | 0.1905 | 0.8080 |

The champion with id 9 in generation 1 scored 0.997692 and was
retained as ids 16 and 24 in generations 2 and 3, with identical
reported proxy fitness. Four-generation gains reflect optimization
of the current artificial objective, not functional evolution.

The evidence reveals **decreasing phenotype novelty despite the
first quality-diversity selection heuristic**. The 128x96 structural
render displays visually large branching fans while the real 48x36
tissue remains compact. The alignment image distinguishes matched
white pixels, magenta tissue outside guidance and cyan unexpressed
guidance. Broad guidance overlap alone does not demonstrate that
each inherited gene generated living appendage tissue.

## Committed v2.2 changes

1. `OpenMorphBranchExpression.Analyze` assigns each cell within
   an activation threshold to the nearest inherited organizer's
   innovation. It counts guided cells and actually occupied tissue
   cells per unique gene innovation.
2. An inherited *non-root* gene is counted as expressing if its
   organizer territory contains at least four occupied cells and
   at least 10% of its guided domain is occupied. These thresholds
   are provisional detection criteria, not functional viability.
3. The raw fitness objective no longer rewards segment count alone.
   The ten-percent structural term is earned only by **expressed
   non-root gene regions**, capped after three. The rest remains
   the previous nutrient maintenance and minimum-size proxy.
4. In each generation, retain the best raw-fitness champion and
   additionally include the most novel candidate meeting a 55%
   champion-fitness floor in the candidate parent pool, then
   fill from novelty-aware, phenotype-distinct parents.
   This is diversity preservation, not natural mating.
5. `population.csv` gains `expressed_branches` and
   `unexpressed_branches` columns. `branch_expression.csv`
   lists the genetic innovation ID, guide territory, occupied
   cells, occupancy fraction and expression flag for each gene
   in the final generation.
6. New EditMode tests check no structural credit for unexpressed
   genes and reconciliation of branch evidence with developed
   tissue. The PowerShell runner requires the expanded exports.

## Run

```powershell
git pull origin main
.\scripts\export-emergent-creatures.ps1 -UnityEditor "C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe"
```

Upload `actual_emergent_generation.png`,
`actual_expression_alignment.png`,
`population.csv` and `branch_expression.csv`.

The experiment must establish whether preserving a viable novelty
parent and removing decorative complexity credit restores actual
phenotype diversity while keeping resource survival competitive.
It might not: genetic representation and the growth rule still
bias toward compact blobs and fan-shaped radial graphs.

## Research-critical limitations

The per-innovation label identifies the organizer **with maximal
local activation**, not a demonstrated mechanically connected
appendage or organ. Repeated segments with the same gene innovation
share a label. There is no functional force, nutrition transport
during growth, locomotion, environmental sensing or endogenous
reproduction. So an `expressed_branches` value is explicitly
**expression evidence, not a functional anatomy count**.

The next major development task should link inherited orientation
and differentiated tissue to actual growth-constrained mechanics
and movement in a shared-resource environment. Another increase
in sketch complexity without physical function does not address
the central goal.
