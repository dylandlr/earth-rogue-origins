# Emergent Allopatric Divergence — v2.2

## What is implemented

The ecosystem now has a **conservative, observational** mechanism for
identifying geographically isolated populations that have accumulated
persistent inherited differences. On a qualifying split, it assigns a new
species ID to the existing right-hand cohort, creates a new Latin-style
scientific binomial, and keeps a traceable parent species ID.

It does **not** invent a new individual, modify genomes to match a name, or
change the underlying food, locomotion and genetic inheritance systems.

## How a split is considered

1. Examine living organisms of the same parent species, ordered by x-position.
2. Identify the largest horizontal gap that leaves at least
   `minimumPerPopulation=4` organisms on each side.
3. Require an actual gap of at least 3 world units and centroid separation
   of at least 8 world units.
4. Require at least four members **per side** from generation 2 or beyond.
5. Compare the groups' average six diploid physiology loci and fourteen
   developmental regulatory values: 20 independent summary dimensions.
6. Require average absolute between-group difference of at least 0.18.
7. Observe this pattern on five consecutive checks, spaced 50 simulation
   steps apart. A failed window resets the evidence counter. Large changes
   in group centroid also reset continuity.

The default thresholds are explicit experimental parameters, **not calibrated
biological constants**. They are deliberately stringent to avoid relabeling
every temporarily distant or differently colored animal.

## New lineage record

The new `ScientificTaxon` includes:
- `speciesId`, `genus`, `epithet` and diagnostic traits
- `parentSpeciesId` (root species are -1)
- `foundedTick`
- `foundingGeneticDistance` and `foundingGeographicGap`

The old species record remains unchanged. Only animals that belonged to the
promoted cohort receive the new species ID. Their IDs, family relationships,
existing genes, diets, positions and health remain unchanged. The ecosystem
event stream records the new binomial, ancestral binomial, and supporting
divergence measurements. The field guide shows a compact ancestry marker.

## Verification

`Assets/Tests/Editor/SpeciationTests.cs` adds controlled synthetic-population
tests that exercise:
- stable repeated divergence observations
- no split from geography alone, or genes alone
- minimum sample and generation requirements
- resetting evidence after a lost isolation condition
- world-level promotion with preserved individual pedigree and immutable
  parent scientific name

The tests are **committed but not executed in Unity**. Run the project's
`scripts/validate-unity.ps1` with the installed Unity 6 Editor and capture
Edit Mode results. The repository source was checked for balanced braces and
the expected integrations; this is not a substitute for compilation.

## Scientific cautions and next experiments

This is an **operational allopatric divergence proxy**, not confirmed
reproductive isolation or formally published speciation. The current mating
rule only chooses partners sharing a `speciesId`, so reproduction is
administratively restricted after a split. That rule must eventually be
replaced with a modeled compatibility/fitness function and explicit gene-flow
experiments; the current rule cannot be cited as independent evidence of
reproductive isolation.

Other pending gaps:
- no calibrated population genetics model, demographic drift model, hybrid
  viability experiment, or quantitative genetic validation
- spatial split uses the x-axis; true habitat barriers and 2D connected
  components are future work
- full genealogical phylogenetic tree, extinction and taxonomic revisions
  are not yet supported
- genus is currently inferred from founding morphology, not cladistically
  reconstructed from all ancestors
- 20-gene summary traits are not independent molecular markers, so the
  distance metric must not be interpreted as actual genome sequence distance
- distinct offspring naming should never be triggered by a single mutation
  or palette change

**Next priority:** enable a 2D spatial connectivity model with terrain
resistance, add gene-flow and reproductive compatibility trials, and perform
seeded baseline/ablation sweeps against the no-speciation control.
