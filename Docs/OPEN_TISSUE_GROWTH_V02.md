## v0.7 — Multi-budget counterfactual anatomy (committed; Unity verification pending)

The first actual v0.6 `actual_equal_budget_tissue.png` was uploaded and
analyzed directly. **Every specimen occupied exactly 180 cells**,
meaning the cap took effect. The four matched pairs had:

| Pair | Common occupied cells | Only in one arm (symmetric difference) | Raw occupancy IoU |
|---|---:|---:|---:|
| 1 | 172 | 16 | 0.915 |
| 2 | 171 | 18 | 0.905 |
| 3 | 146 | 68 | 0.682 |
| 4 | 163 | 34 | 0.827 |

The evidence shows that inherited branches can alter actual anatomy
independently of overall mass, but the magnitude is inconsistent and
there is not yet a convincing functional appendage.

The v0.7 exporter keeps the original unlimited and 180-cell output
for backward compatibility and adds **three native C# Unity sheets**
for limits 180, 400 and 800 cells:
`actual_budget_180_tissue.png`, `actual_budget_400_tissue.png`,
and `actual_budget_800_tissue.png`.

`budget_sweep_metrics.csv` records, for every budget and matched pair,
true baseline/branch occupancy, common cell count, each arm's exclusive
cell count, union, raw intersection-over-union (Jaccard), symmetric
difference fraction, border touching, perimeter, compactness,
components and cavity count.

The comparisons occur on the **same spatial grid** with no scale
normalization or center registration. Common-random-number sampling
remains keyed by seed, position and developmental tick.

Interpretation gates:
- If both variants are nearly identical at 180 but separate at
  400/800, the original cap masked later structural divergence.
- If shape overlap remains high across budgets, the regulatory
  morphology itself requires improvement.
- If specimens hit the grid edge before diverging, increase world
  dimensions before claiming morphological novelty.
- If one arm fails to consume the budget, report its actual cell
  count; the cap is a maximum, not forced equal occupancy.
- Do not use high complexity alone as a proxy for viability.

**Status:** The v0.7 exporter and tests are committed. No Unity run
or Edit Mode test has yet verified this new sweep.

## v0.6 — Equal-mass morphology counterfactual (committed, Unity result pending)

The fourth real Unity tissue sheet repeats the v0.4 finding under v0.5's
cell/tick-indexed random field: branch-added genomes occupied 1,678,
1,355, 820 and 1,043 cells compared with corresponding baselines of
1,070, 375, 202 and 321 cells. Several reached the image boundary.
Mass differences dominate the outcome.

The v0.6 audit therefore exports **two real Unity contact sheets** from
the same paired genome experiment:

- `actual_tissue_growth.png` retains the unrestricted 72-step result
  and `tissue_metrics.csv` for longitudinal comparability.
- `actual_equal_budget_tissue.png` develops each organism at most 180
  occupied cells, using 72 steps and identical per-cell/tick randomness.
  `equal_budget_metrics.csv` reports final count, perimeter, boundary
  contact, connected components, cavities, compactness and bounding box.

The shared cell budget is **an experimental growth cap**, not yet a
physiologically conserved energy or nutrient budget. It prevents
uncapped area from confounding comparisons but does not measure
metabolic viability or functional novelty. New tissue is still grown
by adjacent-cell divisions.

When a tick proposes more cells than the remaining allowance, a
stable per-cell/tick randomized ranking chooses which births occur.
The ranking deliberately avoids favoring the left side of the grid
merely because raster traversal proceeds left-to-right.

Added tests check connectivity, budget adherence, deterministic
replay, equivalence of explicit/unlimited mode, and invalid caps.
Unit tests and actual Unity v0.6 exports remain **unexecuted** here.

The next review must distinguish differences in shape **at equal
occupied mass** and should flag test pairs that fail to reach 180
cells, or reach the world boundary. Only then should we introduce
conserved resource flux and true tissue death/repair.

## v0.5 — Counterfactual controls and quantitative anatomy (committed)

The third **actual Unity** sheet establishes morphological sensitivity: for
four genotype-matched pairs, adding a developmental branch increased
occupied area by about +60%, +258%, +302%, and +217%, measured directly
from the PNG. Some colonies reach the grid boundary; larger area is not
the same thing as more directed or viable structure.

The previous stochastic growth implementation consumed a sequential RNG
stream only when a cell was eligible for growth. Changing the genotype
therefore changed subsequent random draws across the grid, contaminating
the counterfactual. In v0.5 each birth uses a stable hash keyed by
`(seed, tick, x, y)`: both treatments encounter the same stochastic
field even when eligibility differs.

`OpenMorphTopologyMetrics.Analyze` measures actual tissue area,
perimeter, compactness (`4πA/P²`), fill fraction within the bounding
box, connected components, empty holes, and boundary-touching cells.
These are observation metrics, **not** a fitness function; a highly
compact body is not automatically more biologically useful.

The Unity audit exports these as CSV columns alongside both treatments.
Known-shape regression tests use a hollow ring, a filled square, and
separate edge-touching cells. The source is committed, but the revised
Unity test suite and contact sheet have not yet been executed.

**Next gate:** establish whether structural changes survive equal-growth
budget comparisons and held-out seeds, without relying on boundary
clipping or sheer tissue mass. Only then proceed to nutrient flux,
cell-death, repair, and physical sensor/actuator function.

## v0.4 — Counterfactual developmental growth (committed; Unity result pending)

The second **actual Unity** tissue sheet still produced eight compact
rounded colonies (sizes 249–366 occupied pixels), even with 72 growth
ticks. This is evidence that longer runtime and the branch guidance
field alone were insufficient.

### Changes

- Reworked birth rules to permit low-signal cell fronts to advance
  where inherited branch guidance is strong, while retaining local
  adjacency and energy requirements.
- Added provisional tissue energy recovery influenced by local
  developmental signal and nearby occupied tissue. This is a modeling
  intervention, **not a conserved nutrient-transport system**.
- A new audit exports four matched baseline/added-branch pairs,
  each pair having the same seed and previous mutation history.
- Reports genotype, expressed segments, cell-fate counts,
  bounding-box width/height and which counterfactual arm produced
  the result. Added a regression test that demands a branch
  mutation change actual occupancy, not just cell coloring.

### Evaluation criteria

Compare columns 1/2, 3/4, 5/6 and 7/8 in the new
`actual_tissue_growth.png`. A useful v0.4 result is a
clear branch-induced change in shape (preferably an elongation or
new protrusion), rather than size or palette alone. Evidence must
come from the user's real Unity export and metrics.

The model still has neither mature cellular metabolism nor
self-sustaining organs; failing morphology remains informative.

## v0.3 — Branch-coupled growth response (committed, pending Unity verification)

The first actual Unity v0.2 contact sheet shows **eight compact tissue
colonies rather than divergent complex anatomical structures**. Direct
pixel analysis of that user-provided 256x96 PNG measured 210, 210, 319,
211, 356, 418, 343 and 306 occupied cells (among eight 64x48 tiles).
The bounding boxes range from roughly 18x17 to 26x25 pixels.

### Mechanism change

- `OpenMorphGrowthGuide.Build` now converts the inherited variable
  segment graph into local regulatory concentration fields. A
  branching mutation changes the guidance field in previously
  inaccessible regions of the tissue plane.
- `OpenMorphTissue.Grow` blends that graph-derived concentration with
  the earlier chemical/spatial gene rule. New cells are **still
  generated only next to existing cells**, with positive energy,
  sufficient chemical concentration, and a seeded growth decision.
  The graph does not stamp colored capsules onto the tissue image.
- The tissue audit grows for 72 ticks (previously 40) and adds
  expressed segment count to the CSV, for quantitative comparison
  of branching and tissue occupancy.
- Regression tests cover deterministic guidance, distinct signal
  geography when a gene gains branches, and existing birth/fate
  invariants.

### Scientific limitations

These are deliberately **directed growth cues** from a geometric
genotype, not yet emergent regulatory gene networks. More elongated
or divergent shapes do not imply physiology, viability, or the
evolutionary discovery of a useful new body plan. The 2D cell field
still has no differentiated mechanical forces, fluid transport,
sustained nutrient inflow, programmed cell death, or co-evolved
movement. The eight audit genomes are seeded mutation trajectories,
not populations selected by survival.

### Next acceptance gate

Run `scripts/export-open-tissue.ps1` again and compare both raw
Unity PNGs and their `tissue_metrics.csv`. Check that the changes
produce shape differences, not merely larger disks. Preserve an
unchanged founder and genotype-matched branch-addition pair in a
future standardized counterfactual export. Inspect if growth follows
branches *without* producing clipped or disconnected islands.

# Open-ended Morphogenesis v0.2 — Local Tissue Development

## Direction

The old eight-animal art grammar is retained as a compatibility baseline.
The experimental open-morphology architecture now includes two separate
phenotype decoders:

1. **Graph morphology (v0.1):** inherited variable-size growth graph
   builds and connects structural capsules. This supports new topologies
   but cannot model emergent tissue shapes.
2. **Cellular tissue field (v0.2):** genotype-dependent local rules,
   chemical diffusion, neighbor-only growth, and differentiated cell
   fates. The resulting tissue is rasterized independently of any
   predefined head/fin/limb naming scheme.

Neither system is yet wired into authoritative organism survival,
movement, reproduction, or ecosystem selection. This is developmental
infrastructure, not a claim of artificial life or unrestricted evolution.

## Implemented code

`OpenMorphTissue.Grow(genome,width,height,steps)` initializes one
structural cell at the origin and iterates a deterministic double-buffered
grid. Every tick:
- The two signaling fields diffuse to four adjacent cells and decay.
- Occupied cells modify local signal production according to the genome.
- Unoccupied cells may become tissue if neighboring occupied cells have
  usable energy and local inherited activation is sufficient.
- Cells differentiate into structural, contractile, sensory, transport,
  or storage fates using their local signal concentrations and inherited
  response. These are descriptive **candidate cell fates** only; no
  force generation, sensation or circulatory transport has been implemented.
- Existing tissue pays maintenance costs. New cells inherit a bounded
  energy reserve from local neighbors.

The 2D field is bounded to at most 256x256 cells and 256 steps per call.
All pseudo-random growth is seeded and replayable.

The gene response is localized in the tissue plane, using inherited
orientation, attachment coordinates, dimensions, pigment, rigidity, bend,
and repeat parameters. The response keeps absolute spatial influence,
avoiding a normalized single-gene signal that would be identical
everywhere.

## Evidence and verification

`OpenMorphTissueAudit.Run` exports a native 4x2 sheet of eight actual
C# cellular development runs, each using a deterministic sequence of
topology mutations, plus CSV fate counts:

```powershell
git pull origin main
.\scripts\export-open-tissue.ps1 -UnityEditor "C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe"
```

Output appears under a new
`ValidationResults/OpenTissue/run-*/actual_tissue_growth.png`,
with `tissue_metrics.csv` and `provenance.txt`.

`OpenMorphTissueTests` checks deterministic replay, exact zero-step
seed behavior, neighbor-connected births, bounded/finite signaling,
proper fate counts, and rejected invalid settings.

**Execution status:** all changes are committed but the current agent
has not run Unity or the Edit Mode tests. A successful Unity export is
required before claiming visual improvements or verified tissue growth.

## Important limitations and next milestones

- New tissue currently grows on a 2D raster, not a continuous 3D
  mechanical mesh. A useful next step is occupancy-derived topology
  analysis: components, cavities, branching, and local anatomy metrics.
- There is no explicit nutrient source, metabolic flux network, tissue
  death, repair, or conservation-law-compliant growth.
- Different cell fates are colors and labels, not functional systems.
- Genome expression is spatially fixed; a proper regulatory network
  should respond dynamically to nearby cellular messages and time.
- Gene graph topology has not yet been linked to local tissue lineage
  networks; that is the next fundamental coupling.
- Viability and selection must eventually be evaluated through
  physics, resource consumption, sensors/actuators and survival, not
  merely shape complexity or image resemblance.

After verifying the v0.2 export, implement a phenotype ledger that
records every growth/differentiation event, lineage connectivity,
tissue energy, topology changes, and development-state hashes.
Then connect it to co-evolving controllers and the live ecosystem.
