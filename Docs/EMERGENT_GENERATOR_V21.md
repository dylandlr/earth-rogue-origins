# Emergent Creature Generator v2.1 — Expressed Anatomy and Quality-Diversity

## Grounded findings from the first actual full-generation Unity export

The real Unity v2.0 run (seed 3452, 4 generations, 8 individuals each)
completed 32 evaluations and exported real `actual_emergent_generation.png`,
`actual_emergent_structures.png`, 8 heritable JSON genomes, and both CSVs.
Average proxy fitness rose from 0.633325 in generation 0 to 0.912999
in generation 3; best proxy fitness rose from 0.941923 to 0.968374.
This is improved optimization of an **artificial survival proxy**,
not evidence of real ecosystem fitness, locomotion or feeding.

The structural view shows many similar radial fans or a single bar;
tissue appears mostly small lumpy colonies. Five of the final 8
genomes contain one gene, two contain two genes, and one has three.
Root-gene repeats frequently produce multiple overlapping fan
structures. More expressed segments are not necessarily new anatomy.

**Crucial methodological problem:** the structural sheet uses
`OpenMorphRasterizer.Render`, which fits each organism independently
to a 128x96 tile. Actual tissue grows on a 48x36 grid with the
gene-organizer coordinate transform. These two images are NOT the
same spatial frame; one cannot infer tissue expression by visually
overlaying the old two sheets.

## v2.1 changes committed

- `OpenMorphPhenotypeMetrics.Analyze`: compare developed tissue with
  heritable spatial growth organizers on the **same 48x36 grid**.
  Report guide/tissue overlap, percentage of tissue in the active
  regulatory field and unused guided regions. This is a **diagnostic**,
  not a perfect anatomy-quality target.
- `OpenMorphPhenotypeMetrics.Novelty`: compute a true phenotype
  occupancy-distance score (1 − Jaccard) averaged over three nearest
  organism masks in the evaluated history. Unlike earlier descriptors,
  this does not reward adding a gene just because it exists.
- `OpenMorphEvolution`: keep raw fitness, compute a distinct selection
  score = 0.78 * raw fitness + 0.22 * phenotype novelty, preserve the
  highest-fitness individual, and seek parents with at least 0.22 raw
  occupancy distance from already selected parents. Fill from the
  remaining ranked candidates if the population truly converges.
  Distribute offspring across selected parents in a deterministic
  round-robin pattern, then mutate.
- Export `actual_expression_alignment.png`: white cells belong to both
  tissue and active regulatory guidance, magenta to developed tissue
  outside guidance and cyan to guidance without developed tissue.
  All three colors are rendered in the same physical tissue grid.
- Append `novelty`, `selection_score`, `guide_coverage` and
  `supported_tissue_cells` to `population.csv`, retaining all
  previously exported columns for comparison.
- Add EditMode regression tests for exact guide accounting,
  Jaccard novelty and deterministic ancestry.
- The dedicated PowerShell exporter rejects old CSV schemas and
  verifies the extra image. Legacy morphology experiments remain
  accessible.

## Next verified run

```powershell
git pull origin main
.\scripts\export-emergent-creatures.ps1 -UnityEditor "C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe"
```

Upload `actual_emergent_generation.png`,
`actual_expression_alignment.png`, `population.csv`, and
`champions.csv` for quantitative analysis. Structural fan
diagrams remain available but are explicitly *presentation-scaled*
and must not be presented as physical anatomy.

## Interpretation and priorities

- A change in gene count or segment count alone is insufficient.
- Compare retained unique occupied masks by generation, pairwise
  novelty and expressed structural guidance before claiming variety.
- The remaining central unsolved problem is developmental *mechanism*:
  a structural gene must produce living tissue whose arrangement
  changes actual functionality, not merely a radial capsule sketch.
- The next integrated milestone should couple ongoing growth to
  resource conservation and evaluate expressed sensors, feeding
  and contractile mechanics in a shared simulation.
- A score-based population search is not an open-ended ecosystem.
  Explicit reproduction, ecological interactions, competition,
  lineage extinction and held-out environments are still required.

**Status:** All changes are committed and structurally checked; no
v2.1 Unity compilation, new export or EditMode test execution has
been verified in the assistant environment.
