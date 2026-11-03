# Visual Fidelity Roadmap — Concept Reference Alignment v0.8

## Reference: our approved illustrated Carboniferous world

The reference image has thick near-black contours, warm brown terrain, soft ochre rocks, turquoise water with teal shadows, dense yellow-olive ferns, layered vertical vegetation, readable ground-contact shadows and illustrated tetrapods/arthropods. The UI uses charcoal panels, muted gold accents and restrained colored status strips.

Reference images and isolated creature source sprites originate in the earlier Unity v0.8 art archive. They must be explicitly imported into `Assets/Art/ConceptReference`—a screenshot containing the game's HUD is **not** a seamless world texture.

## This milestone

- `CarboniferousTerrainPainter` creates 40 seeded 192px terrain textures, sharing soil, water and stone colors and fern/pebble motifs.
- `CarboniferousFoliagePainter` creates eight transparent fern props, including trunk, frond silhouettes, rim light, canopy and cast shadow.
- `EcosystemSceneBuilder` assigns these higher-detail terrain tiles and fern props.
- `LiveEcosystemPreview` scatters prop sprites deterministically on forest/floodplain and uses world-position depth sorting through Unity `SortingGroup` for whole articulated organisms.
- `IllustratedCreatureSkin` now maps inherited hue and pigmentation density into consistent skin tint and body-bound marking sprites.
- `CreatureArtLibrary` and the editor art generator expose a shared `skinMarking` texture.
- `ConceptArtComparisonWindow` displays the imported approved art next to a genuine Unity morphology capture.
- Original art sprites can be brought into a local Unity project as a source archive; they must be converted into anatomical part textures before using the articulated creature skin.

## Review

1. Import the Concept Art Source Pack by copying its `Assets` directory into the repository project.
2. Generate the ecosystem scene with **Earth: Rogue Origins > World > Create Ecosystem Lab Scene**.
3. Generate a Unity morphology render with **Art > Create Morphology Review Board**.
4. Open **Art > Compare Render to Approved Concept** and review both images at game-view size.
5. Record differences in the categories: silhouette, outline, skin material, water, rocks, foliage, terrain transitions, depth and ambient lighting.

## What remains missing before the visual target is reached

- Final illustrated anatomical component sheets extracted/repainted from approved organism archetypes.
- Terrain auto-tiling, shoreline/cliff edge sprites, hard/soft boundary blending and biome transitions.
- Directional 2.5D creature views, planted feet, proportional shadows and idle/eat/swim animations.
- Non-repeating foreground trees and vegetation batches, atmosphere and soft occlusion.
- In-game UI matched to approved carbon-black and gold concept (the Ecosystem Lab uses a diagnostic HUD).
- Actual Unity screenshots showing the above. None were executed in this environment.

**Do not mark the visual fidelity milestone complete based solely on generated code.** Render real Unity frames, inspect them, then continue art refinements until the target is met.
