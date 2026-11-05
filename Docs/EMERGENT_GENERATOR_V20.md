# Emergent Creature Generator — Integrated v2.0 Experimental Pipeline

## Why this milestone matters

Earlier Earth: Rogue Origins work spent many cycles on individual
64x48 tissue sheets, GRN ablations, and fixed body templates. Those
experiments established deterministic, branch-sensitive growth,
candidate tissue differentiation, recurrent expression and
nutrient maintenance, but never actually ran a population through
inheritance and environmental selection.

The first complete generator connects the existing modules now.

## One end-to-end loop

`OpenMorphEvolution.Evolve(seed:3452,generations:4,populationSize:8)`

For each generation:
1. Create eight variable-topology, heritable genomes. Initial
   genomes are independent random walks from the untyped founder,
   not hand-authored species templates.
2. Express each genome's arbitrary branching graph, local
   organizer fields and recurrent GRN to grow connected 2D tissue
   for 36 steps on a 48x36 field (cap 360 cells).
3. Expose each frozen developed body to scarcity and moderate
   nutrient conditions for 50 maintenance steps.
4. Score survival fractions, minimum biomass and structural
   complexity. Fitness is an **explicit experimental proxy**,
   not true reproductive success. Tiny seed-only bodies are
   penalized to prevent a trivial no-growth solution.
5. Retain the best individual and mutate selected parents from
   the top half to form the next generation. Record generation,
   ancestor, genotype and phenotype details.
6. Retain every evaluated phenotype, not just the highest scorer.

This loop is deterministic by seed and deliberately small enough
to run in a Unity Editor batch export.

## Actual output

```powershell
git pull origin main
.\scripts\export-emergent-creatures.ps1 -UnityEditor "C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe"
```

Exported files in a new `ValidationResults/Emergent/run-...` directory:

- `actual_emergent_generation.png`: all eight final-generation
  tissue morphologies, native C# simulation cells doubled by
  nearest neighbor for inspection.
- `actual_emergent_structures.png`: the same eight genomes
  decoded into genuine variable-topology segment geometry
  by the C# graph renderer. This is a separate diagnostic
  view, **not** a physical skeleton extracted from tissue.
- `population.csv`: all 32 evaluated individuals, generations,
  parent IDs, genome complexity, tissue occupancy, survival
  fractions, proxy fitness and diversity descriptor.
- `champions.csv`: champion per generation.
- Eight individual `genome_<id>.json` records containing
  heritable gene instructions for final-generation replay.
- `provenance.txt` explicitly describes limits.

The Unity export runner validates all expected files, ensures
a new output folder is created, checks 32 population rows and
eight final genome records, and prints exact current-run paths.

## Honest scope

This is an **integrated evolutionary optimization experiment**,
not a finished emergent-creature game. Selection is score-based,
not living organisms reproducing in a shared ecosystem. The
resource trial runs on frozen bodies, not concurrently growing
tissue. No nervous system, physical locomotion, feeding organ,
predator-prey interaction, articulated animation or natural
reproductive fitness has yet been integrated.

Appearance can still be amorphous and ugly; the generator does
not draw from the predesigned Carboniferous eight-archetype
sprites, and no aesthetic image generation should be presented
as its actual result.

## Immediate evaluation gates

1. Unity compile, tests and true full-generator export pass.
2. `population.csv` has 4x8 unique sequential IDs, valid
   parent generations and no invalid scores.
3. `champions.csv` can be compared for improvement *without*
   asserting it must increase every generation.
4. All 8 final genomes can be decoded and replayed deterministically.
5. Generated structures differ in topology/genome, not only color
   or size; track effective population diversity explicitly.
6. Resource conservation test residuals remain small.

## Fast next iteration

Once the first end-to-end output is verified:
- Optimize runtime by caching genotype development and resource
  outcomes, and profile actual Editor runtime.
- Add competition for the **same** local resource field,
  mating/reproduction with inheritable lineages, and a
  selection-free population control.
- Couple nutrient cost to **development itself**, so new cells
  cannot arise from provisional free-energy heuristics.
- Couple morphology to sensors, contractile actuation and
  simulated movement; evaluate survival/feeding/locomotion from
  real actions and physics rather than complexity proxies.
- Make a living Unity scene with zoomable evolving organisms,
  biome variation, event logs and save/load lineage histories.

A general-purpose evolving developmental engine is a long-term
research objective, not proof that every biological body plan
is computationally representable.
