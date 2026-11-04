# v0.8 Structured Morphogenesis — Organizers, Tips, Inhibition and Pruning

## Evidence motivating the change

The user's verified real Unity v0.7 multi-budget sweep is archived as
`actual_budget_180_tissue.png`, `actual_budget_400_tissue.png`,
`actual_budget_800_tissue.png`, and `budget_sweep_metrics.csv`.

Mean matched-pair raw-occupancy Jaccard scores across the four pairs:

| Cap | Mean Jaccard | Mean symmetric difference |
|---|---:|---:|
| 180 cells | 0.832 | 0.168 |
| 400 cells | 0.638 | 0.362 |
| 800 cells | 0.438 | 0.562 |

These are **cell caps, not equal final body mass** at 400 or 800.
At 800, baseline populations in pairs 1, 2 and 4 stopped at
202, 321 and 375 cells respectively, while their branch variants
reached 800. Therefore falling overlap is partly mass-related,
and cannot be described as strong equal-mass anatomical innovation.
The legitimate equal-mass comparison is at cap 180, where all
eight variants reached 180 occupied cells. Pairs 1 and 2 are most
similar there.

The existing tissue system can produce different numbers of cells
and irregular shapes, but does not reliably generate functional
anatomical appendages or stable tissue regions.

## Implementation

- `OpenMorphOrganizers`: derives continuous axial, tip/proximal-distal,
  growth activation, inhibitory fields and per-cell regulatory innovation
  identifiers from the decoded heritable growth graph. It does **not**
  categorize structures as legs, heads, fins or tails.
- `OpenMorphTissue.Grow(..., structured:true)`: optional alternative
  growth mode. Local organizers boost distal front activity and suppress
  diffuse lateral expansion. The previous `structured:false` mode
  remains unchanged as a longitudinal control.
- Conservative terminal-cell pruning can remove no more than one
  qualifying leaf cell per tick; old and proposed neighborhood checks
  protect connected components and newborn tissue.
- Cell state now records which regulatory innovation is active,
  independently of clonal lineage. This is **not** a predefined organ tag.
- `OpenMorphBudgetSweep` additionally emits:
  `actual_organized_800_tissue.png` and
  `organized_800_metrics.csv`. Both use the same eight genomes,
  seed, 72 developmental steps, 64x48 grid and 800-cell maximum as
  the earlier `actual_budget_800_tissue.png` control.
- Regression tests check organizer determinism and bounds, tip
  activation, structured-growth reproducibility, connectivity and
  bounded pruning.

## Next Unity command

```powershell
git pull origin main
.\scripts\export-open-tissue.ps1 -UnityEditor "C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe"
```

Inspect `actual_budget_800_tissue.png` next to
`actual_organized_800_tissue.png`. Compare row-by-row against
`organized_800_metrics.csv` and the previous budget sweep.

## Acceptance and falsification

- Existing regression suites must compile and pass in Unity.
- Every developed organism must remain connected (one component).
- Border contact must not worsen materially.
- Structured animals must not collapse into the seed cell or all
  fail to grow due to excessive inhibition.
- Prefer localized appendages and directional growth to large
  amorphous blobs; evaluate on actual unretouched Unity pixels.
- **No anatomical novelty claim based solely on IoU**: compare cell
  counts, clipping, perimeter, compactness and visible organization.
- Several newly differentiated organizers should map to distinct
  surviving tissue regions; the exporter reports active organizer
  count, but does not yet certify differentiated function.
- This remains a 2D experimental tissue automaton, not conserved
  metabolism, functional organs, true embryogenesis or evidence of
  unbounded evolution.

## Next milestones

1. If organized growth works, move from fixed organizer parameters
   to an inherited gene-regulatory network that responds to local
   signals, tissue fate, age and mechanical loading.
2. Introduce nutrient sources, transport costs, explicit tissue energy
   conservation, cell death and tissue repair with quantitative ledgers.
3. Connect geometric body to genuine actuation, sensors, neural
   control, environmental interactions and reproductive fitness.
4. Evolve organisms in several held-out environments; report novelty
   and viability separately, retaining frozen lineages as baselines.
