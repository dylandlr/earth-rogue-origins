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
