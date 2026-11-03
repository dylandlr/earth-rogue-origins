## Visual fidelity v2.0 — authored head masks
- [x] Replace rectangular contour fill with mask-based 1px silhouette outlines
- [x] Author thirteen-station asymmetrical profiles for four head types
- [x] Keep inherited size deformation, shading and independent face layers
- [x] Add contour ordering and boxed-edge regression tests
- [ ] Execute Unity Editor test suite and C# contact sheet
- [ ] Tune cheek, mouth, eye, neck and body transitions to new head profiles against approved art
- [ ] Accept at 32 / 64 / 96 pixel gameplay sizes before wider procedural expansion

## Creature Face Fidelity v1.9
- [x] Inherit eye placement from authored head morphology
- [x] Add composable cheek, mouth, nostril and eye highlight details
- [x] Add face landmark bounding and compositing regression tests
- [x] Produce standalone 5-species v1.8 to v1.9 face study PNGs
- [ ] Run Unity and verify actual C# sprite output visually
- [ ] Curate asymmetric face silhouettes and improve shoulder/head joins

## Visual Fidelity v1.8 (silhouette-first)
- [x] Add procedural clustered dorsal spots and individually shaped toes to C# rasterizer
- [x] Replace pointed tetrapod tail wedge with softer tapered end
- [x] Generate five original transparent v1.8 sprites, walk strip and v1.5 comparisons in Python
- [x] Add bounds/determinism regression checks for new primitives
- [ ] Validate these visual changes in an actual Unity rendering run
- [ ] Keep refining facial character, low body ratio and organic skin clusters until the recreated sprite target is reached

## Pixel art fidelity v1.1 (art-directed procedural phase)
- [x] Lock six color ramps for ink, shadow, midtone, highlight and pattern
- [x] Define eight intentional organism archetypes and marking grammars
- [x] Integrate palette-specific contour inks, skin-bound spots, bands and stripes
- [x] Export ordered, context-sensitive composable layers and validate image parity
- [x] Expose archetype/palette/marking selection in Unity Composer
- [x] Add tests for distinct archetype renders, palette order and pigment variation
- [ ] Run Unity compile, Edit Mode tests and export actual rendered contact sheets
- [x] Replace primitive head/trunk ellipses with authored 7-point contour profiles
- [x] Add composable neck/shoulder/hip concealment layers and corrected leg anchors
- [x] Add deterministic four-frame walk/swim pixel poses and per-frame layered exports
- [x] Add connected-silhouette and animation-compositing regression tests
- [x] Add tail-root concealment, improved toe silhouettes and separate proportional ground shadows
- [x] Preserve limb root anchors across walk/swim poses
- [ ] Replace remaining procedural joint caps with curated hand-painted anatomical masks
- [ ] Perform side-by-side quality acceptance against the recreated sprite reference

## Milestone 1B: Original composable pixel-art asset system
- [x] Versioned recipes, body-plan grammars and ordered part attachment descriptors
- [x] Original 128×96 transparent pixel compositor with deterministic colors, outlines and markings
- [x] Starter exporter for 12 creatures across tetrapod, fish and arthropod silhouettes
- [x] Independent per-part PNGs, editable recipe JSON, layer metadata, contact sheet
- [x] Visual composer/editor window with individual export
- [x] Bridge inherited tetrapod genes to gameplay pixel sprites
- [x] Add deterministic pixel, layer parity and reproduction constraint tests
- [ ] Compile Unity and execute the complete Edit Mode test suite
- [ ] Replace primitive procedural brushes with authored pixel materials and anatomy templates
- [ ] Add multiple silhouette modules and validated part variants per body plan
- [ ] Add multi-direction poses, animation frames and runtime atlasing
- [ ] Model fish and arthropod physiology before enabling them as ecosystem agents


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
- [x] Add actual Unity-camera morphology review generator with phenotype CSV and blank-output checks
- [ ] Execute visual review and compare screenshots to approved concept artwork
- [ ] Add hand-painted 2.5D surface materials and illustrated anatomical layers
- [x] Add initial four-frame walk/swim morphology posing (not production IK)
- [x] Add first articulated multi-part rig with head, tail, limbs and dorsal plates
- [x] Add reusable illustrated part library and anatomical skin binding (prototype artwork)
- [x] Add Carboniferous terrain palette, ferns, positional depth and genetic markings
- [x] Add approved concept/reference comparison window
- [ ] Import isolated approved organism illustrations into a curated anatomical component atlas
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
