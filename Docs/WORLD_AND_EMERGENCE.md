# World, Evolution, Procedural Art and Emergence — Technical Design v0.1

## Core rule

**The simulation is authoritative. The art and UI are projections of simulation state.**
Art never changes fitness, resource stock, genetic inheritance or survival. Organisms are not forced to become a historically known species.

## Implemented substrate (first integration)

- `EcosystemWorld`: discrete 2D cells and organism agents, seeded random stream, fixed 0.2-s ticks.
- Cells track terrain, elevation, moisture, temperature, plant biomass, detritus, soil nutrients and water depth.
- Plant growth follows capacity-limited growth modulated by moisture and nutrient availability. Detritus returns nutrients, which diffuse to adjacent cells.
- Agents hold location, energy, health, age, diet, species label, six diploid traits, IDs and parent IDs.
- Behavior policies seek locally available resources, move, graze/scavenge/predate, reproduce, age and die.
- Offspring recombine parent alleles and may mutate. Fitness emerges from energy acquisition and successful reproduction, not a fixed stat score.
- `LiveEcosystemPreview` shows independent organism sprites and tile objects, with a live world HUD.
- `ProceduralArtGenerator` produces 24 anatomy/color morphs and 40 habitat tile variants as importable PNGs.
- `EcosystemSceneBuilder` creates `Assets/Scenes/EcosystemLab.unity` with generated sprites assigned.
- Edit Mode tests cover deterministic creation and steps, nonnegative resources, and basic population accounting.

## Unity workflow

1. Open repository in Unity 6 (6000.0.23f1).
2. Run **Earth: Rogue Origins > World > Create Ecosystem Lab Scene**. It generates PNGs and a scene.
3. Press Play in the generated scene.
4. Press Space to pause, 1/2/3 for speed, WASD to pan, wheel to zoom; P/O adds/removes 2 degrees of temperature offset; C raises CO2; R resets.
5. Run tests from Window > General > Test Runner > EditMode.

This scene is a *simulation lab*, not the final polished campaign or a drop-in replacement for the original illustrated prototype.

## Emergent systems roadmap

### Body-plan evolution
Current six quantitative loci are insufficient for real morphological innovation. Extend the genome to a gene-regulatory graph or constrained body-plan grammar:

```
Genome -> development -> articulated skeleton -> locomotor phenotype
       -> ecological capability -> survival/reproduction -> inheritance
```

Genotype should determine appendage count/placement, propulsion, mouthparts, sensory range, armor, body size and metabolic cost. Constrain development to valid anatomies. Genetic changes produce mesh/sprite layer changes **and** physics/energetic consequences. Avoid traits that are cosmetic-only in the simulation.

### Behavior
Begin with inspectable utility AI, explicit sensors and finite-energy actions. Then compare:
1. Scripted heuristic baseline
2. Evolvable behavior weights
3. Small recurrent neural controller with mutation and natural selection
4. Optional lifelong local learning

Do not use LLMs as a substitute for evolution. Capture observations/actions/energy costs per decision for causal attribution.

### Ecological interactions
Current model has primitive food cycling and predation. Expand to explicit trophic levels, species-specific diets, carcass nutrient transfers, parasites, plant defenses, local carrying capacity, niche specialization, cooperative behaviors and predator avoidance. Allow organisms to alter the habitat (burrows, trails, nests, dams) only by consuming energy/materials and affecting future agents. These are **niche construction** mechanics.

### Speciation
A species label is currently assigned at spawn and **does not imply emergent speciation**. Introduce an inherited mate-compatibility signature and test reproductive isolation alongside population divergence. A clade is an inferred cluster, not an arbitrary new species assigned by an upgrade button.

### Geological and global world model
Use three spatial/temporal resolutions:
- **Active region**: individual agents, 5-10 ecosystem ticks/s
- **Background biomes**: population cohorts with aggregated food/energy/trait distributions
- **Planet**: slow climate, tectonic, ocean and atmosphere fields and rare catastrophes

Transitions between resolutions must conserve population, biomass and nutrients to avoid exploiting LOD. Geological history should be scenario data, not a four-era level sequence.

### Art pipeline
Approved art reference: outlined 2.5D cartoon, darker swamp palette, layered plants, thick border silhouettes, readable creatures at game zoom. Procedural sprite prototypes use deterministic palette+geometry generation; final assets must have:
- Separate parts (torso, head, tail, limbs, armor, fins, eyes and decals)
- Pivots, overlap/depth ordering, masks, eight-direction facings
- Animator data for idle/walk/swim/eat/attack/hurt/death
- Trait-to-appearance binding with stable seeds
- A manually art-directed palette library and biome-specific tilesets

The art generator is a **blockout tool**, not a claim of concept-art fidelity. It intentionally generates plain 2D PNGs that designers can retouch.

## Scientific/engineering quality

- Reproducibility: persist seed, timestep, configuration, gene schema version and environment parameters.
- No reward hacking: all claims must come from organism lifetimes and offspring counts.
- Compare environment treatments across paired seeds and >=10 replicates.
- Track extinction rates, age distribution, food-web energy flow, allele trajectories, offspring survival, resource spatial autocorrelation and speciation/compatibility metrics.
- Test mass/energy balance with explicit bookkeeping. The v0.1 prototype is not yet physically calibrated and some resource conversions are heuristic.
- Performance: avoid one SpriteRenderer/GameObject per soil cell at world scale; move to chunk meshes/batched Tilemaps, a spatial index for neighbors, and time-sliced background simulation.

## Known limitations

- The current preview uses a single-threaded CPU simulation with O(agents * nearby-tiles + agents squared in predator detection) worst-case; its 600 organism cap is a safety bound, not a validated benchmark.
- Population growth may be unstable, and cells do not yet model conservation-complete water, carbon or atmospheric oxygen.
- Mature organisms are not assigned sex; reproductive compatibility is only diet/species/co-location.
- Individual creature movement is local greedy pursuit rather than true pathfinding and lacks terrain obstacles.
- Terrain is deterministic but does not yet simulate moving continents.
- Full Unity compile, edit mode tests, real-time playtest and visual fidelity must still be verified in an installed Unity Editor.
