## v1.3 — Growth opportunity attribution (committed; Unity audit pending)

The actual v1.2 `grn_growth_outcomes.csv` and
`grn_growth_trace.csv` show the calibrated recurrent model no longer
collapses globally. Final static/recurrent occupied cells by pair were
121/122, 174/173, 248/190, and 238/235. All recurrent runs still
had successful divisions through ticks 70–72. Every proposed birth
was accepted, and cell accounting reconciled; the 800-cell cap did not
cause these differences.

Pair 3 is an outlier: at ticks 10/20/30 its static run occupied
15/52/123 cells against recurrent 11/26/52. At tick 72 it finished
248 versus 190 cells (−23.4%). A blanket GRN stimulation would be
unjustified because the other three pairs are close to static.

### Diagnostics added

The `OpenMorphTissue` development ledger now records for each tick:

- number of locally eligible sites (after the common organizer gate),
- mean structured/static division probability at those sites,
- mean probability after recurrent GRN modulation,
- number of eligible cells whose probability decreased or increased,
- proposed and accepted divisions, pruning, and occupied cells.

`OpenMorphGRNTraceAudit` includes these columns in
`grn_growth_trace.csv` and aggregate opportunity/probability
columns in `grn_growth_outcomes.csv`. These are read-only
measurements; the simulation's birth decisions are unchanged.

The analytical decision rule is:
- If pair 3 has similar eligible-site counts but materially lower
  recurrent final chance, GRN modulation directly suppresses
  proposed divisions.
- If pair 3 develops far fewer eligible sites while the within-arm
  probability remains comparable, earlier morphology changed the
  future opportunity frontier.
- Both may occur and require additional matched-state
  counterfactual analysis. Dynamic growth paths cannot by
  themselves prove the causal contribution of a single regulatory
  edge.

Tests check probability bounds, opportunity accounting and that
static mode leaves base and final probability identical.

### Validation

Run `scripts/export-open-tissue.ps1` again in Unity 6 and
upload `grn_growth_outcomes.csv` and
`grn_growth_trace.csv`. Unity execution and the added tests have
not yet been verified. Do not infer functional physiology or
open-ended evolution from occupancy counts alone.

## v1.2 — Calibrated recurrent regulation and growth accounting

The user uploaded two **actual Unity v1.1 C# renders**:
`actual_grn_counterfactual.png` and
`actual_grn_edge_ablation.png`. Qualitative inspection shows
that the recurrent arms are much smaller than their nonrecurrent
controls and several specimens barely develop. We do not yet have
the matching v1.1 numeric CSVs in this conversation, so specific
cell counts, expression averages or causal edge-ablation effects
are **not established**. The polished fish/newt/arthropod comparison
illustrations generated in the previous conversation are concept art,
**not** output from this Unity system.

### Identified mechanism

The v1.1 birth equation first computed a low organizer-limited
division chance, then multiplied that probability by
`(.25+1.10*GRN.growth)` and subtracted
`.20*GRN.brake`. Because static inhibition had already been
applied, this compounded inhibition and could suppress otherwise
viable division fronts.

### v1.2 intervention

- `OpenMorphGRN.ModulateGrowth` expresses recurrent regulation as a
  bounded change to the existing static growth probability:
  `clamp01(baseChance + .18*(growth-.5) - .10*(brake-.5))`.
  Neutral expression leaves base probability invariant. Strong
  positive/negative expression can still change growth without
  applying the previous multiplicative bottleneck.
- The deterministic cell/tick-indexed stochastic field and fixed
  tissue budget remain unchanged, enabling paired comparisons.
- `OpenMorphTissue.Field` now records per-tick occupancy, proposed
  births, accepted births and pruned cells. These must satisfy
  `occupied[t+1] = occupied[t] + acceptedBirths[t] - pruned[t]`.
- `OpenMorphGRNTraceAudit` adds `grn_growth_trace.csv` and
  `grn_growth_outcomes.csv` to the real Unity export, comparing
  the same four baseline genomes in static structured mode versus
  calibrated recurrent mode over 72 ticks with an 800-cell cap.
- EditMode tests assert neutral GRN modulation, bounded changes and
  per-tick birth/pruning accounting, in addition to existing
  connectivity and deterministic replay tests.

### Verification and next step

```powershell
git pull origin main
.\scripts\export-open-tissue.ps1 -UnityEditor "C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe"
```

Upload `actual_grn_counterfactual.png`,
`actual_grn_edge_ablation.png`,
`grn_growth_outcomes.csv` and `grn_edge_ablation_metrics.csv`.
Compare morphology with the v1.1 Unity renders, looking for colony
recovery without becoming unbounded blobs. A useful result has
connected organisms, no major new clipping, sufficient growth, and
recurrent expression that alters tissue patterns or behaviors.
High cell count by itself is not proof of anatomical novelty.

No v1.2 Unity compilation, test execution or rendered image has
yet been verified. Conserved metabolism and functional tissue
physics remain future work.

## v1.1 — Recurrent heritable gene regulation (committed; real Unity export pending)

The user's v1.0 actual Unity fate-only experiment passed its strict
geometry-isolation gate for all four pairs:

| Pair | Occupied in each arm | Occupancy IoU | Cells with changed fate |
|---|---:|---:|---:|
| 1 | 121 | 1.000 | 121 |
| 2 | 174 | 1.000 | 174 |
| 3 | 248 | 1.000 | 248 |
| 4 | 238 | 1.000 | 238 |

All arms remained connected, and each pair had the same pruning
count. This verifies independent genetic control over **candidate fate
labels**, not mechanical, metabolic or sensory functionality.

### New implementation

- `OpenMorphGenome.Gene` now contains six signed, independently
  mutable and heritable gene-to-gene regulatory couplings among
  *growth*, *fate*, and *brake* expression channels.
- `OpenMorphGRN.Step` is a per-cell **synchronous recurrent update**.
  It reads the preceding tick's expression state alongside chemical
  signals, local organizer-tip activity, and neighboring-cell
  crowding. It returns bounded expression values in [0,1].
- `OpenMorphTissue.Grow(..., structured:true,
  recurrentRegulation:true)` is opt-in, retaining all earlier
  fixed-regulation experiments for reproducible comparisons. Newborn
  cells inherit the selected parent's expression state as input to
  their own first GRN step.
- `OpenMorphGRNAudit` exports
  `actual_grn_counterfactual.png` and
  `grn_counterfactual_metrics.csv`, comparing static structured
  regulation with recurrent regulation on identical genes, seed, 72
  ticks and the same 800-cell cap.
- `OpenMorphGRNEdgeAudit` exports
  `actual_grn_edge_ablation.png` and
  `grn_edge_ablation_metrics.csv`. Both arms use the recurrent
  model. The control disables only the six signed interactions,
  leaving organizer geometry, baseline expression alleles, seed,
  development time and cell cap unchanged.
- Tests check signed allele bounds/copy inheritance, sensitivity to
  previous expression states, deterministic tissue replay,
  boundedness and connectivity.

### Interpretation

A v1.1 success requires actual changes in per-cell regulatory
expression and/or fate patterns that survive controlled comparison.
An increased cell count alone is not evidence of organogenesis.
The edge-ablation study asks whether gene-to-gene interactions
cause differences beyond simply adding recurrence.

This is an intentionally small, hand-designed 3-channel GRN
with evolvable interaction weights. It is **not** a biological
transcription model, an open-ended evolving ecosystem, or a working
metabolism. Functional tissue mechanics and conserved resource
flows remain unimplemented.

### Unity acceptance

Run the existing `scripts/export-open-tissue.ps1` command and inspect
both new images and CSV files. Unity compilation and the EditMode suite
have not yet run in the current session. Treat all predictions about
the appearance of recurrent morphogenesis as unverified until that run.

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
