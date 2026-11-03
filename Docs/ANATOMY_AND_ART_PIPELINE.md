# Anatomy & Visual Fidelity — v0.2

## Approved target
Top-down 2.5D illustrated Carboniferous world, hand-painted cartoon animals, strong dark contour, warm olive/ochre/terracotta body colors, turquoise shallow water, dense layered vegetation, and consistent overhead-left highlights. Aim for a readable colony-sim silhouette at 64-96 screen pixels per organism.

The first procedural sprite generator merely attached armor/fin triangles to a fixed image. **That is now a legacy blockout tool.**

## Implemented now

- `DevelopmentalGenome`: 14 inherited, bounded regulatory controls.
- `Development.Express`: maps the regulatory genome and six-locus physiology to anatomical geometry and physical costs.
- `AnatomicalPhenotype`: trunk/head/tail/limb proportions, surface-anchor positions, 17-point dorsal curve, body mass, terrestrial/swimming speeds, defense, and upkeep.
- `AnatomySpriteBaker`: generates anatomy-aware raster images using layered outlines, body shading, tail fins, limb attachment, dorsal armor and deterministic pigmentation.
- `AnatomyAtlasBuilder`: exports 24 comparative morphs plus contact sheet from four coordinated adaptation axes.
- `LiveEcosystemPreview`: spawns unique anatomy sprites for inherited organisms.
- `EcosystemWorld`: offspring inherit developmental controls; phenotype affects movement, energy upkeep and defense.

## Unity commands

- `Earth: Rogue Origins > World > Create Ecosystem Lab Scene`
- `Earth: Rogue Origins > Art > Generate Anatomy Atlas and Contact Sheet`
- `Window > General > Test Runner > EditMode`

Expected generated path: `Assets/Art/Generated/Anatomy/`.

## The v0.2 visual contract

1. A part must attach to an anatomical anchor—not arbitrary pixel coordinates.
2. Armor roots must sit on the dorsal curve and move when trunk geometry changes.
3. A larger tail fin must affect swimming propulsion and total upkeep.
4. Limb length changes should affect locomotion, silhouette and basic metabolism.
5. Color/spot variation should be deterministic for organism ID and seed.
6. A new organism should be generated from genome + stable seed, with no random Unity global state.
7. The shape must be valid at every supported gene value in [0,1].

## Limits of current implementation

- CPU-baked rasterized sprites approximate the silhouette but **do not yet match the polished concept art**. They need curated illustration overlays, texture masks, higher-quality curves, environment-aware lighting and species-specific animation.
- This is an early tetrapod body plan only. A generic "any animal" generator would produce anatomically invalid creatures; add validated alternate body-plan grammars, not arbitrary appendages.
- The 14 regulatory parameters are continuous blended inherited values, not a biological gene regulatory network.
- Body appearance is cached per organism, but growth, injury and age-based geometry are not yet rendered.
- There is currently no skeletal animation or IK.
- Sprite generation at birth is CPU-heavy. Build a hash-quantized phenotype atlas and pool in later production.
- Unity Edit Mode tests and visual playtests are committed but have not run in this environment.

## Next milestones

**M1. Illustration system**
Use painted component atlases for heads, limbs, tails and surfaces. Each component has:
`anchor, tangent, mask, depth, material, 2D bones, compatible body plans`.
Layer those onto the procedural geometry; generate normal/light masks so shading looks consistent.

**M2. Skeletal locomotion**
Animated gait phase, planted feet with procedural IK, swim tail undulation. Use the same physical body and controller values as the agent's locomotion; speed must not be simulated independently of phenotype.

**M3. Phenotype cache and streaming**
One immutable phenotype descriptor per organism. Bake/canonicalize sprites by stable content hash; reuse textures for equivalent morphologies; pool when organisms die.

**M4. Visual QA**
Automated contact sheets: idle/walk/swim side-by-side, silhouette-only previews, 64px gameplay views, dorsal anchor debug overlays and attachment collision checks.

**M5. Multiple validated body-plan grammars**
Fish, arthropod, amphibian, reptile, early mammal, each with compatible inheritance and biome capabilities. Divergence must be traceable to inherited variation and ecological filtering—not a scripted epoch change.
