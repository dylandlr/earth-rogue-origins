## v1.0 — Decoupling regulatory differentiation from proliferation

The **actual v0.9 Unity** `actual_regulatory_counterfactual.png` and
`regulatory_counterfactual_metrics.csv` contain four pairs. Altering
growth/tip/inhibition regulatory alleles produced:

| Pair | Original | Regulatory variant | Occupancy IoU |
|---|---:|---:|---:|
| 1 | 121 | 126 | 0.96032 |
| 2 | 174 | 187 | 0.93048 |
| 3 | 248 | 286 | 0.86713 |
| 4 | 238 | 259 | 0.91892 |

Every original occupied cell was included in its modified counterpart,
so the experimental regulatory change added tissue rather than
repatterning an existing anatomy. Neither clipping nor disconnection
occurred. The v0.9 evidence does **not** establish structurally
innovative organs or a reliable cell fate change.

The new milestone adds an **independent inherited fateResponse
allele**, separated from `growthResponse`, `tipResponse` and
`inhibitionResponse`. Structured tissue development now reads a
per-cell gene-associated fate response to local morphogens.
A deliberately elementary five-bin fate classifier maps the response
to candidate tissue labels (storage, structural, contractile,
transport, sensory). This is **not a GRN, active physiology, or proof
these cells work**. It only establishes independent genetically
controllable differentiation before a more expressive regulatory
network is introduced.

A new real Unity audit outputs:
- `actual_fate_only_counterfactual.png`
- `fate_only_metrics.csv`

Each pair is the same original genome, with the same topology, seed,
timesteps, growth alleles, budget, inhibition, and organizer geometry.
The variant changes **only fateResponse** to a strong expression
value. Any final occupancy difference would be a bug in that
isolation experiment; cell fate differences are expected and must
be verified by the actual Unity output. New EditMode tests assert
identity of the occupancy masks, cell energy and morphogen signals,
while demanding at least one fate difference.

Next true GRN priorities:
1. Regulatory network edges with variable expression thresholds,
   sign, and time-dependent activity, not fixed five-bin tissue fates.
2. A causal trace showing which regulatory interactions produced a
   differentiated cell, independent of morphology.
3. Cell fates that affect mechanics, energy transport and sensing
   rather than just colors.
4. Evolution across varied selective environments using actual
   reproductive success rather than silhouette aesthetics.

**Execution status:** The code was committed and source-reviewed;
the Unity compilation/tests and fate-only contact sheet have not
been executed in this environment.

## v0.9 — Heritable local regulatory response (committed; Unity result pending)

The actual v0.8 structured growth render and `organized_800_metrics.csv`
show all eight colonies connected, zero boundary cells, and 38 total
pruning events. Growth remains strongly size-biased across branch
variants: organized baseline sizes were 117, 174, 277, and 250 cells,
compared with 316, 436, 586, and 800 for matched added-branch genomes.
This verifies controlled spatial growth, not functional organogenesis.

### First evolvable regulatory layer

- `OpenMorphGenome.Gene` now inherits `growthResponse`,
  `tipResponse` and `inhibitionResponse` in [0,1].
  Parameter mutation can modify all three independently of adding
  anatomical branches.
- `OpenMorphRegulation` maps the local active organizer's gene, current
  chemical signals, neighbor crowding, and developmental stage
  into proliferation, distal tip stimulation, and inhibitory responses.
- The optional structured tissue growth path uses these responses;
  legacy growth remains the unchanged control. Regulatory responses
  are bounded and deterministic.
- A new real Unity contact sheet `actual_regulatory_counterfactual.png`
  shows four matched **same-topology, same-seed** pairs. Only regulatory
  alleles change, while 72 ticks and an 800-cell cap remain fixed.
- `regulatory_counterfactual_metrics.csv` records occupied cells,
  Jaccard overlap, border contact, components, and pruning per arm.
  The current perturbation changes three regulatory coefficients
  together, so a future single-allele ablation is still required.

The new regulation is an initial hand-designed response equation
with evolvable coefficients; it is **not** yet a full recurrent GRN,
biological signaling network, physiological metabolism or learned
cell fate specification.

### Verification

Run `scripts/export-open-tissue.ps1` after pulling the latest
commit. Inspect the new regulatory-only contact sheet and CSV.
Treat large size changes as confounding, not automatically anatomical
novelty. The source has been committed, but Unity compilation,
EditMode tests and the v0.9 render have not yet been verified.

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
