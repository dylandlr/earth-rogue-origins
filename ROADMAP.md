# Roadmap — Earth: Rogue Origins

## Milestone 0: Reproducible Unity baseline
- [x] Establish writable GitHub repository
- [x] Add Unity version/configuration and build helper
- [ ] Import complete Unity v0.8 sources, scene and art
- [ ] Record clean-editor compile result
- [ ] Record startup screenshot and input smoke-test result
- [ ] Add automated Unity test runner

## Milestone 1A: Anatomy-aware procedural assets (in progress)
- [x] Add 14-control developmental genome and inheritance model
- [x] Convert regulatory controls into anchored body geometry and morphology-derived physical costs
- [x] Add curved dorsal armor, connected fins and shape-driven limbs to runtime sprite baker
- [x] Connect phenotypes to live organism movement, defense and upkeep
- [x] Bake morphology contact sheet through a Unity Editor command
- [x] Commit deterministic anatomy tests
- [ ] Execute Unity compilation and Edit Mode tests
- [ ] Review generated contact sheet in Unity against approved concept artwork
- [ ] Add hand-painted 2.5D surface materials and illustrated anatomical layers
- [x] Add initial four-frame walk/swim morphology posing (not production IK)
- [x] Add first articulated multi-part rig with head, tail, limbs and dorsal plates
- [x] Add reusable illustrated part library and anatomical skin binding (prototype artwork)
- [ ] Replace starter illustrations with approved hand-painted component atlases
- [ ] Add IK foot planting and direction-dependent poses
- [x] Add reference-counted phenotype sprite cache
- [ ] Profile GPU/CPU memory and replace per-morph raster sheets with deformable parts/atlases

## Milestone 1: Concept-matched playable habitat
- [ ] Create layered illustrated terrain tileset
- [ ] Separate animated creatures from scenery
- [ ] Add pathfinding, collision and terrain cost
- [ ] Recreate dark/gold top bar, sidebars and command controls
- [ ] Support click-to-select, pan, zoom, follow and speed controls

## Milestone 2: Evolution Engine
- [ ] Diploid genome with reproducible seeded random numbers
- [ ] Allele inheritance, recombination, mutation and phenotype mapping
- [ ] Energy budget, nutrition, survival, mating and mortality
- [ ] Selection experiments with confidence intervals and saved seeds
- [ ] Lineage and population genetics visualizations

## Milestone 3: Ecosystems and divergence
- [ ] Plants, prey, predators, decomposers and competition
- [ ] Ecological niches, resource flows and food webs
- [ ] Geography, dispersal, migration and isolation
- [ ] Population divergence and explicit reproductive isolation model
- [ ] Species extinction and adaptive radiation

## Milestone 4: Deep-time roguelike
- [ ] Continuous geologic time and climate systems
- [ ] Regional and planetary events
- [ ] Emergent environmental bottlenecks
- [ ] Multiple viable evolutionary futures
- [ ] Persistent historical chronicle and replay

## Quality gates
Every system must have a deterministic test where practical. No simulation screenshots, charts, or fitness claims should be presented as verified until the executable or experiment has actually been run.
